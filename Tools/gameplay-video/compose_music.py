"""Create a quiet, original 40-second music bed for the preview (no samples)."""

import math
import struct
import wave
from pathlib import Path

RATE = 22050
DURATION = 40
BEAT = 60 / 96
NOTES = (261.63, 329.63, 392.00, 329.63, 293.66, 349.23, 440.00, 349.23,
         329.63, 392.00, 493.88, 392.00, 293.66, 349.23, 392.00, 349.23)
CHORDS = ((130.81, 164.81, 196.00), (146.83, 174.61, 220.00),
          (164.81, 196.00, 246.94), (146.83, 174.61, 196.00))

target = Path(__file__).parent / 'public' / 'music.wav'
with wave.open(str(target), 'wb') as audio:
    audio.setnchannels(1)
    audio.setsampwidth(2)
    audio.setframerate(RATE)
    block = bytearray()
    for sample in range(RATE * DURATION):
        t = sample / RATE
        note_index = int(t / BEAT)
        phase = t % BEAT
        melody = math.sin(2 * math.pi * NOTES[note_index % len(NOTES)] * phase)
        melody *= math.exp(-5 * phase) * min(1, phase * 50) * 0.10
        chord = CHORDS[(note_index // 4) % len(CHORDS)]
        pad = sum(math.sin(2 * math.pi * f * t) for f in chord) * 0.018
        fade = min(1, t / 1.5, (DURATION - t) / 2)
        value = max(-1, min(1, (melody + pad) * fade))
        block.extend(struct.pack('<h', int(value * 32767)))
        if len(block) >= 65536:
            audio.writeframesraw(block)
            block.clear()
    audio.writeframes(block)
print(target)
