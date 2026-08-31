# RAG Indexer

The RAG Indexer (`scripts/rag_indexer.py`) indexes repository source code into Qdrant using local FastEmbed embeddings.

## Running Commands via `uv`

### 1. Sync Mode
Performs a one-time scan of the repository, detects changes (added, modified, deleted files) via SHA-256 hashes, updates local index state, and upserts embeddings into Qdrant.

```bash
uv run --with "qdrant-client[fastembed]" scripts/rag_indexer.py sync
```

Options:
- `--url`: Qdrant server URL (default: `http://localhost:6333` or `QDRANT_URL` environment variable).
- `--collection`: Qdrant collection name (default: `agenty-code` or `QDRANT_COLLECTION` environment variable).
- `--force`: Force re-indexing of all files.
- `--root`: Root directory to index (default: `.`).

### 2. Watch Mode
Runs a polling loop (default interval: 2 seconds) that continuously checks for file changes and automatically syncs incremental updates.

```bash
uv run --with "qdrant-client[fastembed]" scripts/rag_indexer.py watch
```

Options:
- `--interval`: Polling interval in seconds (default: `2.0`).

## Configuration

- **Qdrant URL**: Configurable via `--url` or environment variable `QDRANT_URL`.
- **Collection Name**: Configurable via `--collection` or environment variable `QDRANT_COLLECTION` (defaults to `agenty-code`).

## How Incremental Updates Work

1. **State Tracking**: A local JSON state file (`.rag_indexer_state.json`) maps relative file paths to their last known SHA-256 content hashes.
2. **Scan & Hash**: During each sync/poll cycle, supported files across the repository are scanned and their SHA-256 hashes are computed.
3. **Diff Calculation**:
   - **Added files**: New paths not present in the previous state are chunked and indexed.
   - **Modified files**: Paths whose hash differs from the previous state have their old Qdrant points deleted and are re-chunked and re-indexed.
   - **Deleted files**: Paths present in the previous state but missing from the current scan have their Qdrant points deleted.
4. **State Persistence**: The state file is updated with the current file hashes.
