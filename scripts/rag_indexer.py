#!/usr/bin/env python3
import os
import sys
import json
import hashlib
import argparse
import pathlib
import time
import subprocess
from typing import Dict, List, Tuple, Any

DEFAULT_COLLECTION = "agenty-code"
DEFAULT_QDRANT_URL = "http://localhost:6333"
STATE_FILE = ".rag_indexer_state.json"
EMBEDDING_MODEL_NAME = "sentence-transformers/all-MiniLM-L6-v2"
VECTOR_NAME = "fast-all-minilm-l6-v2"

SUPPORTED_EXTENSIONS = {
    ".cs",
    ".csproj",
    ".sln",
    ".props",
    ".targets",
    ".md",
    ".json",
    ".yml",
    ".yaml",
    ".xml",
    ".py",
}

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

    for part in parts:
        if part in EXCLUDED_DIRS or part.startswith(".git"):
            return True

    name = path.name
    if name in EXCLUDED_FILES or name.endswith(".pyc") or name.endswith(".sqlite"):
        return True

    if name == ".env" or name.endswith(".env") or name.startswith(".env."):
        return True

    ext = path.suffix.lower()
    if ext not in SUPPORTED_EXTENSIONS:
        return True

    if ext in BINARY_EXTENSIONS:
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
        ".csproj": "csproj",
        ".sln": "solution",
        ".props": "props",
        ".targets": "targets",
        ".xml": "xml",
    }
    return mapping.get(ext, ext.lstrip(".") or "unknown")

def deterministic_point_id(rel_path: str, chunk_idx: int) -> str:
    import uuid
    ns = uuid.NAMESPACE_URL
    name = f"{rel_path}#chunk{chunk_idx}"
    return str(uuid.uuid5(ns, name))

def get_repository_name(root: pathlib.Path, explicit_repo: str = None) -> str:
    if explicit_repo:
        return explicit_repo
    env_repo = os.getenv("QDRANT_REPO")
    if env_repo:
        return env_repo

    try:
        result = subprocess.run(
            ["git", "-C", str(root), "config", "--get", "remote.origin.url"],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            timeout=2
        )
        if result.returncode == 0 and result.stdout.strip():
            url = result.stdout.strip()
            if url.endswith(".git"):
                url = url[:-4]
            name = url.split("/")[-1].split(":")[-1]
            if name:
                return name
    except Exception:
        pass

    return root.resolve().name

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

def sync_repository(root: pathlib.Path, qdrant_url: str, collection_name: str, force: bool = False, repo_override: str = None) -> bool:
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

    repo_name = get_repository_name(root, explicit_repo=repo_override)

    if not added and not modified and not deleted and not force:
        print("No changes detected.")
        return True

    print(f"Changes: {len(added)} added, {len(modified)} modified, {len(deleted)} deleted.")

    from qdrant_client import QdrantClient
    from qdrant_client.http import models
    from fastembed import TextEmbedding

    client = QdrantClient(url=qdrant_url)
    embedding_model = TextEmbedding(model_name=EMBEDDING_MODEL_NAME)

    # Ensure collection exists with named vector
    if client.collection_exists(collection_name):
        try:
            info = client.get_collection(collection_name)
            vectors_config = info.config.params.vectors
            has_named = isinstance(vectors_config, dict) and VECTOR_NAME in vectors_config
            if not has_named:
                print(f"Collection {collection_name} exists without named vector {VECTOR_NAME}. Recreating...")
                client.delete_collection(collection_name)
                client.create_collection(
                    collection_name=collection_name,
                    vectors_config={
                        VECTOR_NAME: models.VectorParams(
                            size=384,
                            distance=models.Distance.COSINE
                        )
                    }
                )
        except Exception:
            pass
    else:
        client.create_collection(
            collection_name=collection_name,
            vectors_config={
                VECTOR_NAME: models.VectorParams(
                    size=384,
                    distance=models.Distance.COSINE
                )
            }
        )

    failed_files = set()

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
                                    key="repository",
                                    match=models.MatchValue(value=repo_name)
                                ),
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

    successfully_indexed = []

    if files_to_index:
        print(f"Indexing {len(files_to_index)} files for repository '{repo_name}'...")
        for path in files_to_index:
            full_path = root / path
            if not full_path.exists():
                failed_files.add(path)
                continue
            try:
                content = full_path.read_text(encoding="utf-8", errors="ignore")
            except Exception as e:
                print(f"Skipping {path}: cannot read ({e})")
                failed_files.add(path)
                continue

            chunks = chunk_text(content)
            if not chunks:
                successfully_indexed.append(path)
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
                        vector={VECTOR_NAME: vector.tolist() if hasattr(vector, "tolist") else list(vector)},
                        payload={
                            "document": chunk,
                            "metadata": metadata
                        }
                    )
                    for point_id, vector, chunk, metadata in zip(ids, vectors, documents, metadatas)
                ]
                client.upsert(
                    collection_name=collection_name,
                    points=points,
                )
                successfully_indexed.append(path)
            except Exception as e:
                print(f"Error indexing {path}: {e}")
                failed_files.add(path)

    new_state = dict(old_state)
    for path in deleted:
        if path in new_state:
            del new_state[path]
    for path in successfully_indexed:
        new_state[path] = current_files[path]

    save_state(root, new_state)

    if failed_files:
        print(f"Sync completed with errors. Failed files: {list(failed_files)}", file=sys.stderr)
        return False

    print("Sync completed successfully.")
    return True

def watch_repository(root: pathlib.Path, qdrant_url: str, collection_name: str, interval: float = 2.0, repo_override: str = None):
    print(f"Starting watch mode on {root} (interval: {interval}s)... Press Ctrl+C to stop.")
    if not (root / STATE_FILE).exists():
        print("Initial sync...")
        sync_repository(root, qdrant_url, collection_name, repo_override=repo_override)
    else:
        print("Loading existing state...")

    try:
        while True:
            time.sleep(interval)
            current_files = scan_files(root)
            old_state = load_state(root)

            if current_files != old_state:
                print("\nChange detected! Syncing...")
                sync_repository(root, qdrant_url, collection_name, repo_override=repo_override)
    except KeyboardInterrupt:
        print("\nWatch mode stopped.")

def main():
    parser = argparse.ArgumentParser(description="RAG Indexer for Qdrant using FastEmbed with MCP compatibility")
    parser.add_argument("command", choices=["sync", "watch"], help="Command to execute")
    parser.add_argument("--url", default=os.getenv("QDRANT_URL", DEFAULT_QDRANT_URL), help="Qdrant URL")
    parser.add_argument("--collection", default=os.getenv("QDRANT_COLLECTION", DEFAULT_COLLECTION), help="Collection name")
    parser.add_argument("--interval", type=float, default=2.0, help="Polling interval in seconds for watch mode")
    parser.add_argument("--force", action="store_true", help="Force re-indexing all files")
    parser.add_argument("--root", default=".", help="Root directory to index")
    parser.add_argument("--repo", default=None, help="Explicit repository name override")

    args = parser.parse_args()
    root_path = pathlib.Path(args.root).resolve()

    success = True
    if args.command == "sync":
        success = sync_repository(root_path, args.url, args.collection, force=args.force, repo_override=args.repo)
        if not success:
            sys.exit(1)
    elif args.command == "watch":
        watch_repository(root_path, args.url, args.collection, interval=args.interval, repo_override=args.repo)

if __name__ == "__main__":
    main()
