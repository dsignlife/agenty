#!/usr/bin/env python3
import os
import sys
import json
import hashlib
import argparse
import pathlib
import time
from typing import Dict, List, Tuple, Any

DEFAULT_COLLECTION = "agenty-code"
DEFAULT_QDRANT_URL = "http://localhost:6333"
STATE_FILE = ".rag_indexer_state.json"

EXCLUDED_DIRS = {
    ".git",
    "bin",
    "obj",
    "node_modules",
    ".venv",
    "venv",
    "__pycache__",
    "dist",
    "build",
}

EXCLUDED_FILES = {
    ".env",
    STATE_FILE,
}

BINARY_EXTENSIONS = {
    ".png", ".jpg", ".jpeg", ".gif", ".ico", ".pdf", ".zip", ".tar", ".gz",
    ".exe", ".dll", ".so", ".dylib", ".bin", ".db", ".sqlite", ".pyc"
}

def is_excluded(path: pathlib.Path, root: pathlib.Path) -> bool:
    rel = path.relative_to(root)
    parts = rel.parts

    # Check if any parent or name matches excluded dirs
    for part in parts:
        if part in EXCLUDED_DIRS or part.startswith(".git"):
            return True

    if path.name in EXCLUDED_FILES or path.name.endswith(".pyc") or path.name.endswith(".sqlite"):
        return True

    if path.suffix.lower() in BINARY_EXTENSIONS:
        return True

    return False

def scan_files(root: pathlib.Path) -> Dict[str, str]:
    """Scans repository and returns {relative_path_str: sha256_hex}."""
    file_hashes = {}
    if not root.exists():
        return file_hashes

    for p in root.rglob("*"):
        if p.is_file() and not is_excluded(p, root):
            try:
                h = compute_sha256(p)
                rel = str(p.relative_to(root))
                file_hashes[rel] = h
            except Exception as e:
                print(f"Error reading {p}: {e}", file=sys.stderr)
    return file_hashes

def compute_sha256(path: pathlib.Path) -> str:
    sha256 = hashlib.sha256()
    with open(path, "rb") as f:
        while True:
            data = f.read(65536)
            if not data:
                break
            sha256.update(data)
    return sha256.hexdigest()

def chunk_text(text: str, chunk_size: int = 1000, overlap: int = 200) -> List[str]:
    if not text:
        return []
    chunks = []
    start = 0
    length = len(text)
    if length <= chunk_size:
        return [text]
    while start < length:
        end = start + chunk_size
        chunks.append(text[start:end])
        if end >= length:
            break
        start = end - overlap
    return chunks

def get_file_type(path: str) -> str:
    ext = pathlib.Path(path).suffix.lower()
    mapping = {
        ".py": "python",
        ".cs": "csharp",
        ".js": "javascript",
        ".ts": "typescript",
        ".md": "markdown",
        ".json": "json",
        ".yml": "yaml",
        ".yaml": "yaml",
        ".sh": "shell",
        ".txt": "text",
    }
    return mapping.get(ext, ext.lstrip(".") or "unknown")

def deterministic_point_id(rel_path: str, chunk_idx: int) -> str:
    # Generate a stable UUID or deterministic integer/string ID if supported,
    # or let Qdrant handle string/UUID IDs. qdrant-client accepts UUIDs or uint64.
    # We can create a deterministic UUID5 using NAMESPACE_URL or NAMESPACE_DNS.
    import uuid
    ns = uuid.NAMESPACE_URL
    name = f"{rel_path}#chunk{chunk_idx}"
    return str(uuid.uuid5(ns, name))

def load_state(root: pathlib.Path) -> Dict[str, str]:
    state_path = root / STATE_FILE
    if state_path.exists():
        try:
            with open(state_path, "r", encoding="utf-8") as f:
                data = json.load(f)
                if isinstance(data, dict):
                    return data
        except Exception:
            pass
    return {}

def save_state(root: pathlib.Path, state: Dict[str, str]):
    state_path = root / STATE_FILE
    with open(state_path, "w", encoding="utf-8") as f:
        json.dump(state, f, indent=2)

def get_qdrant_client(url: str):
    from qdrant_client import QdrantClient
    # If URL is local or http, instantiate client
    if url.startswith("http://") or url.startswith("https://"):
        # parse host and port if needed or pass url
        return QdrantClient(url=url)
    return QdrantClient(url=url)

def ensure_collection(client, collection_name: str):
    from qdrant_client.http import models
    collections = client.get_collections().collections
    exists = any(c.name == collection_name for c in collections)
    if not exists:
        # FastEmbed default embedding size is usually 384 (e.g. BAAI/bge-small-en-v1.5) or similar.
        # With fastembed integration in qdrant-client, add_cached or text embedding handles vectors automatically or we specify vector params.
        # Wait, qdrant-client[fastembed] client.add() automatically creates collection or manages vectors if configured,
        # but let's check standard qdrant-client fastembed usage or create collection with fastembed vector config if needed,
        # or use client.add(collection_name, documents=..., metadata=...) which handles collection creation automatically in qdrant-client!
        pass

def sync_repository(root: pathlib.Path, qdrant_url: str, collection_name: str, force: bool = False):
    print(f"Scanning repository at {root}...")
    current_files = scan_files(root)
    old_state = load_state(root)

    added = []
    modified = []
    deleted = []

    for path, h in current_files.items():
        if path not in old_state:
            added.append(path)
        elif old_state[path] != h:
            modified.append(path)

    for path in old_state:
        if path not in current_files:
            deleted.append(path)

    if not added and not modified and not deleted and not force:
        print("No changes detected.")
        return

    print(f"Changes: {len(added)} added, {len(modified)} modified, {len(deleted)} deleted.")

    from qdrant_client import QdrantClient
    from qdrant_client.http import models
    from fastembed import TextEmbedding

    client = QdrantClient(url=qdrant_url)
    embedding_model = TextEmbedding()

    # Ensure collection exists
    if not client.collection_exists(collection_name):
        client.create_collection(
            collection_name=collection_name,
            vectors_config=models.VectorParams(
                size=TextEmbedding.get_embedding_size(embedding_model.model_name),
                distance=models.Distance.COSINE
            )
        )

    repo_name = root.resolve().name

    # Handle deletions / updates (remove old points for modified & deleted files)
    files_to_remove = modified + deleted
    if files_to_remove:
        print(f"Removing old entries for {len(files_to_remove)} files from Qdrant...")
        for path in files_to_remove:
            try:
                client.delete(
                    collection_name=collection_name,
                    points_selector=models.FilterSelector(
                        filter=models.Filter(
                            must=[
                                models.FieldCondition(
                                    key="path",
                                    match=models.MatchValue(value=path)
                                )
                            ]
                        )
                    )
                )
            except Exception as e:
                print(f"Warning: failed to delete points for {path}: {e}")

    # Handle additions & modifications (index files)
    files_to_index = added + modified
    if force and not files_to_index:
        files_to_index = list(current_files.keys())

    if files_to_index:
        print(f"Indexing {len(files_to_index)} files...")
        for path in files_to_index:
            full_path = root / path
            if not full_path.exists():
                continue
            try:
                content = full_path.read_text(encoding="utf-8", errors="ignore")
            except Exception as e:
                print(f"Skipping {path}: cannot read ({e})")
                continue

            chunks = chunk_text(content)
            if not chunks:
                continue

            documents = []
            metadatas = []
            ids = []

            file_type = get_file_type(path)
            file_hash = current_files[path]

            for idx, chunk in enumerate(chunks):
                documents.append(chunk)
                metadatas.append({
                    "path": path,
                    "chunk_index": idx,
                    "content_hash": file_hash,
                    "file_type": file_type,
                    "repository": repo_name,
                })
                ids.append(deterministic_point_id(path, idx))

            try:
                vectors = list(embedding_model.embed(documents))
                points = [
                    models.PointStruct(
                        id=point_id,
                        vector=vector.tolist() if hasattr(vector, "tolist") else list(vector),
                        payload=metadata
                    )
                    for point_id, vector, metadata in zip(ids, vectors, metadatas)
                ]
                client.upsert(
                    collection_name=collection_name,
                    points=points,
                )
            except Exception as e:
                print(f"Error indexing {path}: {e}")

    save_state(root, current_files)
    print("Sync completed successfully.")

def watch_repository(root: pathlib.Path, qdrant_url: str, collection_name: str, interval: float = 2.0):
    print(f"Starting watch mode on {root} (interval: {interval}s)... Press Ctrl+C to stop.")
    # Initial sync or load state
    if not (root / STATE_FILE).exists():
        print("Initial sync...")
        sync_repository(root, qdrant_url, collection_name)
    else:
        print("Loading existing state...")

    try:
        while True:
            time.sleep(interval)
            current_files = scan_files(root)
            old_state = load_state(root)

            if current_files != old_state:
                print("\nChange detected! Syncing...")
                sync_repository(root, qdrant_url, collection_name)
    except KeyboardInterrupt:
        print("\nWatch mode stopped.")

def main():
    parser = argparse.ArgumentParser(description="RAG Indexer for Qdrant using FastEmbed")
    parser.add_argument("command", choices=["sync", "watch"], help="Command to execute")
    parser.add_argument("--url", default=os.getenv("QDRANT_URL", DEFAULT_QDRANT_URL), help="Qdrant URL")
    parser.add_argument("--collection", default=os.getenv("QDRANT_COLLECTION", DEFAULT_COLLECTION), help="Collection name")
    parser.add_argument("--interval", type=float, default=2.0, help="Polling interval in seconds for watch mode")
    parser.add_argument("--force", action="store_true", help="Force re-indexing all files")
    parser.add_argument("--root", default=".", help="Root directory to index")

    args = parser.parse_args()
    root_path = pathlib.Path(args.root).resolve()

    if args.command == "sync":
        sync_repository(root_path, args.url, args.collection, force=args.force)
    elif args.command == "watch":
        watch_repository(root_path, args.url, args.collection, interval=args.interval)

if __name__ == "__main__":
    main()
