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
        # Empty text
        self.assertEqual(rag_indexer.chunk_text(""), [])

        # Short text
        text = "Hello world"
        self.assertEqual(rag_indexer.chunk_text(text, chunk_size=100), ["Hello world"])

        # Long text with overlap
        long_text = "A" * 2500
        chunks = rag_indexer.chunk_text(long_text, chunk_size=1000, overlap=200)
        self.assertTrue(len(chunks) > 1)
        self.assertEqual(chunks[0], "A" * 1000)
        self.assertEqual(chunks[1], "A" * 1000) # exact overlap mechanics check

    def test_get_file_type(self):
        self.assertEqual(rag_indexer.get_file_type("foo.py"), "python")
        self.assertEqual(rag_indexer.get_file_type("bar.cs"), "csharp")
        self.assertEqual(rag_indexer.get_file_type("README.md"), "markdown")
        self.assertEqual(rag_indexer.get_file_type("unknown.xyz"), "xyz")

    def test_is_excluded(self):
        # Create dummy structure
        git_dir = self.root / ".git"
        git_dir.mkdir()
        git_file = git_dir / "config"
        git_file.write_text("config")

        node_modules = self.root / "node_modules"
        node_modules.mkdir()
        nm_file = node_modules / "index.js"
        nm_file.write_text("console.log()")

        env_file = self.root / ".env"
        env_file.write_text("SECRET=123")

        normal_file = self.root / "main.py"
        normal_file.write_text("print('hello')")

        bin_file = self.root / "image.png"
        bin_file.write_bytes(b"\x89PNG\r\n\x1a\n")

        self.assertTrue(rag_indexer.is_excluded(git_file, self.root))
        self.assertTrue(rag_indexer.is_excluded(nm_file, self.root))
        self.assertTrue(rag_indexer.is_excluded(env_file, self.root))
        self.assertTrue(rag_indexer.is_excluded(bin_file, self.root))
        self.assertFalse(rag_indexer.is_excluded(normal_file, self.root))

    def test_compute_sha256_and_scan(self):
        f1 = self.root / "file1.py"
        f1.write_text("content 1")
        f2 = self.root / "file2.md"
        f2.write_text("content 2")

        h1 = rag_indexer.compute_sha256(f1)
        self.assertEqual(len(h1), 64)

        scanned = rag_indexer.scan_files(self.root)
        self.assertIn("file1.py", scanned)
        self.assertIn("file2.md", scanned)
        self.assertEqual(scanned["file1.py"], h1)

if __name__ == "__main__":
    unittest.main()
