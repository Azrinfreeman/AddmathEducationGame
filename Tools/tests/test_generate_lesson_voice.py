"""Generator safety checks. Run: python -m unittest discover -s Tools/tests -v"""
import asyncio
import contextlib
import importlib.util
import io
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

spec = importlib.util.spec_from_file_location("lesson_voice", Path(__file__).parents[1] / "generate_lesson_voice.py")
voice = importlib.util.module_from_spec(spec)
spec.loader.exec_module(voice)


class GenerationSafetyTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        root = Path(self.temporary.name)
        self.script = root / "script.tsv"
        self.manifest = root / "manifest.json"
        self.output = root / "audio"
        for name, value in [("SCRIPT", self.script), ("MANIFEST", self.manifest), ("OUT", self.output)]:
            patcher = patch.object(voice, name, str(value))
            patcher.start()
            self.addCleanup(patcher.stop)
        self.write_script("Lesson\tAssets/scene.unity\ten\t1\t4f9f2cab\thello\n")

    def write_script(self, body):
        self.script.write_text("scene\tscenePath\tlang\tpage\thash\ttext\n" + body, encoding="utf-8")

    def test_rejects_stale_hash_before_generation(self):
        self.write_script("Lesson\tAssets/scene.unity\ten\t1\t4f9f2cab\tedited text\n")
        with self.assertRaisesRegex(ValueError, "Stale script hash"):
            voice.load_clips()

    def test_rejects_empty_export_instead_of_pruning_all_audio(self):
        self.write_script("")
        with self.assertRaisesRegex(ValueError, "refusing to prune"):
            voice.load_clips()

    def test_rejects_duplicate_page_language(self):
        row = "Lesson\tAssets/scene.unity\ten\t1\t4f9f2cab\thello\n"
        self.write_script(row + row)
        with self.assertRaisesRegex(ValueError, "Duplicate page/language"):
            voice.load_clips()

    def run_generator(self, fail=False):
        forced = []

        async def render(sem, lang, key, text, force):
            forced.append(force)
            path = self.output / lang / (key + ".mp3")
            if not fail:
                path.write_bytes(b"test recording")
            return str(path), "FAILED: simulated service failure" if fail else "made"

        with patch.object(voice, "render", render), patch("sys.argv", ["generator"]), contextlib.redirect_stdout(io.StringIO()):
            with self.assertRaises(SystemExit) as result:
                asyncio.run(voice.main())
        self.assertEqual(result.exception.code, 1 if fail else 0)
        return forced

    def test_voice_or_rate_changes_regenerate_existing_clip(self):
        self.assertEqual(self.run_generator(), [True])
        self.assertEqual(self.run_generator(), [False])
        with patch.object(voice, "RATE", "-12%"):
            self.assertEqual(self.run_generator(), [True])

    def test_failure_preserves_unused_recordings_and_manifest(self):
        folder = self.output / "en"
        folder.mkdir(parents=True)
        old = folder / "old.mp3"
        old.write_bytes(b"previous recording")
        self.manifest.write_text(json.dumps({"en/old": "old signature"}), encoding="utf-8")
        before = self.manifest.read_bytes()
        self.run_generator(fail=True)
        self.assertTrue(old.exists())
        self.assertEqual(self.manifest.read_bytes(), before)


if __name__ == "__main__":
    unittest.main()

