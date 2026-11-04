"""Generates the offline lesson voice-over clips.

Reads docs/tutorial/lesson-voice-script.tsv (written by Unity menu 17: one row per authored lesson page and language)
and writes one MP3 per unique sentence set to Assets/Audio/LessonVoice/<lang>/<hash>.mp3. The hash is the fingerprint
LessonVoice.Hash() computes at runtime, so the app only plays a clip while the page text still matches it.

Voices come from Microsoft's neural text-to-speech service via the edge-tts package, so generating needs internet; the
finished MP3 files are ordinary assets and play fully offline in the app.

Usage (from the project root):
    python Tools/generate_lesson_voice.py            # generate missing or changed recordings
    python Tools/generate_lesson_voice.py --force    # regenerate everything
"""
import argparse
import asyncio
import csv
import hashlib
import json
import os
import sys

import edge_tts

SCRIPT = "docs/tutorial/lesson-voice-script.tsv"
# Import bulk wording with Unity menu 16, then export with menu 17 before generating.
# Runtime scene components and this export must contain identical text for recording fingerprints to match.
MANIFEST = "docs/tutorial/lesson-voice-manifest.json"
OUT = "Assets/Audio/LessonVoice"
VOICES = {"ms": "ms-MY-YasminNeural", "en": "en-GB-SoniaNeural"}
RATE = "-8%"          # a little slower for young listeners
CONCURRENCY = 4
RETRIES = 4


def load_clips():
    clips = {}
    pages = set()
    with open(SCRIPT, encoding="utf-8", newline="") as f:
        for row in csv.reader(f, delimiter="\t"):
            if len(row) < 6 or row[0].startswith("#") or row[0] == "scene":
                continue
            lang, key, text = row[2], row[4], row[5].strip()
            if lang not in VOICES or not text:
                raise ValueError("Unsupported language or empty narration: " + str(row[:5]))
            # C# hashes UTF-16 code units, including surrogate pairs for non-BMP characters.
            fingerprint = 2166136261
            units = text.encode("utf-16-le")
            for i in range(0, len(units), 2):
                fingerprint = ((fingerprint ^ int.from_bytes(units[i:i + 2], "little")) * 16777619) & 0xffffffff
            if key != f"{fingerprint:08x}":
                raise ValueError("Stale script hash: " + str(row[:5]))
            page = (row[1], lang, row[3])
            if page in pages:
                raise ValueError("Duplicate page/language: " + str(page))
            pages.add(page)
            if (lang, key) in clips and clips[(lang, key)] != text:
                raise ValueError("Fingerprint collision: " + lang + "/" + key)
            clips[(lang, key)] = text
    if not clips:
        raise ValueError("No narration rows; refusing to prune existing recordings.")
    return clips


async def render(sem, lang, key, text, force):
    path = os.path.join(OUT, lang, key + ".mp3")
    if os.path.exists(path) and os.path.getsize(path) > 0 and not force:
        return path, "kept"
    async with sem:
        for attempt in range(1, RETRIES + 1):
            try:
                communicate = edge_tts.Communicate(text, VOICES[lang], rate=RATE)
                tmp = path + ".part"
                await communicate.save(tmp)
                if os.path.getsize(tmp) == 0:
                    raise RuntimeError("empty audio")
                os.replace(tmp, path)
                return path, "made"
            except Exception as error:  # network hiccups are common; retry with a pause
                if attempt == RETRIES:
                    return path, "FAILED: %s" % error
                await asyncio.sleep(2 * attempt)


async def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()
    clips = load_clips()
    manifest = {}
    if os.path.exists(MANIFEST):
        with open(MANIFEST, encoding="utf-8") as f:
            manifest = json.load(f)
    signatures = {
        (lang, key): hashlib.sha256(json.dumps([text, VOICES[lang], RATE], ensure_ascii=False).encode("utf-8")).hexdigest()
        for (lang, key), text in clips.items()
    }
    for lang in VOICES:
        os.makedirs(os.path.join(OUT, lang), exist_ok=True)
    sem = asyncio.Semaphore(CONCURRENCY)
    results = await asyncio.gather(*(render(sem, l, k, t, args.force or manifest.get(l + "/" + k) != signatures[(l, k)])
                                     for (l, k), t in sorted(clips.items())))
    failed = [(p, r) for p, r in results if r.startswith("FAILED")]
    # Remove clips no page refers to any more (for example after lesson text changed).
    wanted = {os.path.join(OUT, l, k + ".mp3") for (l, k) in clips}
    pruned = 0
    for lang in VOICES if not failed else []:
        folder = os.path.join(OUT, lang)
        for name in os.listdir(folder):
            full = os.path.join(folder, name)
            if name.endswith((".mp3", ".mp3.meta")) and full[: -5 if name.endswith(".meta") else None] not in wanted:
                os.remove(full)
                pruned += 1
    made = sum(1 for _, r in results if r == "made")
    kept = sum(1 for _, r in results if r == "kept")
    if not failed:
        temporary = MANIFEST + ".part"
        with open(temporary, "w", encoding="utf-8") as f:
            json.dump({l + "/" + k: signature for (l, k), signature in sorted(signatures.items())}, f, indent=2)
            f.write("\n")
        os.replace(temporary, MANIFEST)
    print("clips: %d unique | generated %d | already present %d | removed unused %d | failed %d" % (len(results), made, kept, pruned, len(failed)))
    for p, r in failed:
        print("  ", p, r)
    sys.exit(1 if failed else 0)


if __name__ == "__main__":
    asyncio.run(main())
