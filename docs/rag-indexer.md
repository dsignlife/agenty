# RAG Indexer

The RAG Indexer (`scripts/rag_indexer.py`) indexes repository source code into Qdrant using local FastEmbed embeddings with MCP compatibility.

## Collections & Distinction

- **`agenty-knowledge`**: Curated/manual project knowledge, architecture decisions, and operational notes managed via the `qdrant` MCP server.
- **`agenty-code`**: Automatically generated repository source index managed by the indexer script (`rag_indexer.py`) and queried via the `qdrant-code` MCP server.

## Embedding Model & Named Vectors

- **Model**: `sentence-transformers/all-MiniLM-L6-v2` via FastEmbed.
- **Named Vector**: `fast-all-minilm-l6-v2` (compatible with the official `mcp-server-qdrant`).
- **Payload Structure**: Each chunk is stored with an MCP-compatible payload:
  ```json
  {
    "document": "<actual source chunk>",
    "metadata": {
      "path": "...",
      "chunk_index": 0,
      "content_hash": "...",
      "file_type": "...",
      "repository": "..."
    }
  }
  ```

## Supported Files & Allowlist

Only supported source and documentation extensions are indexed:
`.cs`, `.csproj`, `.sln`, `.props`, `.targets`, `.md`, `.json`, `.yml`, `.yaml`, `.xml`, `.py`.

Exclusions:
- `.git`, `bin`, `obj`, `node_modules`, `.venv`, `venv`, `__pycache__`, `dist`, `build`
- `.env`, `.env.*`, `*.env`
- Binary and database files (`.png`, `.jpg`, `.pdf`, `.zip`, `.db`, `.sqlite`, etc.)

## Running Commands via `uv`

### 1. Sync Mode
Performs an incremental scan of the repository, computes SHA-256 hashes, tracks state in `.rag_indexer_state.json` (advanced only after successful upsert), and upserts embeddings into Qdrant.

```bash
QDRANT_URL=http://qdrant:6333 uv run --with "qdrant-client[fastembed]" scripts/rag_indexer.py sync
```

Options:
- `--url`: Qdrant server URL (default: `http://localhost:6333` or `QDRANT_URL` environment variable).
- `--collection`: Qdrant collection name (default: `agenty-code` or `QDRANT_COLLECTION` environment variable).
- `--force`: Force re-indexing of all files.
- `--root`: Root directory to index (default: `.`).

### 2. Watch Mode
Runs a polling loop (default interval: 2 seconds) that continuously monitors for file changes and automatically syncs incremental updates.

```bash
QDRANT_URL=http://qdrant:6333 uv run --with "qdrant-client[fastembed]" scripts/rag_indexer.py watch
```

## Qdrant MCP Retrieval (`qdrant-code`)

Configured in `.openclaude.json` / project settings:
- Server Name: `qdrant-code`
- Collection: `agenty-code`
- URL: `http://qdrant:6333`
- Embedding Provider: `fastembed`
- Embedding Model: `sentence-transformers/all-MiniLM-L6-v2`
