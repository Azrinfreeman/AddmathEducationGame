# MathFun gameplay video

This Remotion project assembles real Unity footage. The source recordings are in
`Captures/video/source/`. The shorter cropped clips under `public/` are generated
from those recordings and are not included in Git.
With FFmpeg on your PATH, run `py -3 prepare_clips.py` to regenerate missing clips;
existing files are preserved. Copy `Assets/UI/Fonts/Fredoka.ttf` to `public/`
if it is missing.

Edit the shot order and durations in `src/video.tsx`. Run `npm install`
and then `npm run studio` to preview, or `npm run preview` to render the current
half-resolution draft to `out/preview.mp4`. Run `py -3 compose_music.py` first to
generate the original, sample-free music bed. The recording has no game audio.
Review game asset rights and the preview before rendering or publishing a finished video.

The game footage was captured from Unity's Game view at a 1920×1080 canvas; the
recorded window was cropped and resized to 1280×720 for this draft. The crop and
source ranges are defined in `prepare_clips.py`.
