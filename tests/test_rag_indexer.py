import unittest
import pathlib
import tempfile
import os
import sys

# Add scripts directory to path so we can import rag_indexer
sys.path.insert(0, str(pathlib.Path(__file__).parent.parent / "scripts"))
import rag_indexer

class TestRagIndexer(unittest.TestCase):

    def setUp(self):
        self.test_dir = tempfile.TemporaryDirectory()
        self.root = pathlib.Path(self.test_dir.name)

    def tearDown(self):
        self.test_dir.cleanup()

    def test_chunk_text(self):
        self.assertEqual(rag_indexer.chunk_text(""), [])

        text = "Hello world"
        self.assertEqual(rag_indexer.chunk_text(text, chunk_size=100), ["Hello world"])

        long_text = "A" * 2500
        chunks = rag_indexer.chunk_text(long_text, chunk_size=1000, overlap=200)
        self.assertTrue(len(chunks) > 1)
        self.assertEqual(chunks[0], "A" * 1000)
        self.assertEqual(chunks[1], "A" * 1000)

    def test_get_file_type(self):
        self.assertEqual(rag_indexer.get_file_type("foo.py"), "python")
        self.assertEqual(rag_indexer.get_file_type("bar.cs"), "csharp")
        self.assertEqual(rag_indexer.get_file_type("README.md"), "markdown")
        self.assertEqual(rag_indexer.get_file_type("config.json"), "json")
        self.assertEqual(rag_indexer.get_file_type("proj.csproj"), "csproj")

    def test_is_excluded_and_allowlist(self):
        py_file = self.root / "main.py"
        py_file.write_text("print('hello')")
        md_file = self.root / "README.md"
        md_file.write_text("# Title")
        cs_file = self.root / "Program.cs"
        cs_file.write_text("class Program {}")

        env_file = self.root / ".env"
        env_file.write_text("SECRET=123")
        env_local = self.root / ".env.local"
        env_local.write_text("FOO=BAR")
        custom_env = self.root / "staging.env"
        custom_env.write_text("API=123")
        txt_file = self.root / "notes.txt"
        txt_file.write_text("notes")

        git_dir = self.root / ".git"
        git_dir.mkdir()
        git_file = git_dir / "config"
        git_file.write_text("config")

        self.assertFalse(rag_indexer.is_excluded(py_file, self.root))
        self.assertFalse(rag_indexer.is_excluded(md_file, self.root))
        self.assertFalse(rag_indexer.is_excluded(cs_file, self.root))

        self.assertTrue(rag_indexer.is_excluded(env_file, self.root))
        self.assertTrue(rag_indexer.is_excluded(env_local, self.root))
        self.assertTrue(rag_indexer.is_excluded(custom_env, self.root))
        self.assertTrue(rag_indexer.is_excluded(txt_file, self.root))
        self.assertTrue(rag_indexer.is_excluded(git_file, self.root))

    def test_deterministic_point_id(self):
        id1 = rag_indexer.deterministic_point_id("main.py", 0)
        id2 = rag_indexer.deterministic_point_id("main.py", 0)
        id3 = rag_indexer.deterministic_point_id("main.py", 1)
        self.assertEqual(id1, id2)
        self.assertNotEqual(id1, id3)
        self.assertEqual(len(id1), 36)

    def test_compute_sha256_and_scan(self):
        f1 = self.root / "file1.py"
        f1.write_text("print('test')")
        f2 = self.root / "file2.md"
        f2.write_text("# Doc")
        f3 = self.root / "ignored.txt"
        f3.write_text("ignore")

        h1 = rag_indexer.compute_sha256(f1)
        self.assertEqual(len(h1), 64)

        scanned = rag_indexer.scan_files(self.root)
        self.assertIn("file1.py", scanned)
        self.assertIn("file2.md", scanned)
        self.assertNotIn("ignored.txt", scanned)
        self.assertEqual(scanned["file1.py"], h1)

    def test_state_loading_and_saving(self):
        state = {"file1.py": "abc123hash"}
        rag_indexer.save_state(self.root, state)
        loaded = rag_indexer.load_state(self.root)
        self.assertEqual(loaded, state)

    def test_get_repository_name(self):
        # Explicit override
        repo = rag_indexer.get_repository_name(self.root, explicit_repo="custom-repo")
        self.assertEqual(repo, "custom-repo")

        # Env override
        os.environ["QDRANT_REPO"] = "env-repo"
        try:
            repo = rag_indexer.get_repository_name(self.root)
            self.assertEqual(repo, "env-repo")
        finally:
            del os.environ["QDRANT_REPO"]

        # Fallback to root directory name if no git remote
        repo = rag_indexer.get_repository_name(self.root)
        self.assertEqual(repo, self.root.resolve().name)

if __name__ == "__main__":
    unittest.main()
