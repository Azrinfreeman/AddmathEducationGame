"""Make cropped edit clips from the original recordings; preserve existing clips."""

from pathlib import Path
import subprocess

project = Path(__file__).resolve().parents[2]
video = Path(__file__).resolve().parent
source = project / 'Captures' / 'video' / 'source'
public = video / 'public'
public.mkdir(exist_ok=True)

# Source recording, start in seconds, duration in seconds, output name.
clips = (
    ('demo-take-01.mp4', 5, 5, 'menu.mp4'),
    ('demo-take-01.mp4', 14, 5, 'activities.mp4'),
    ('demo-take-01.mp4', 48, 9, 'rocket.mp4'),
    ('demo-take-01.mp4', 90, 9, 'quiz.mp4'),
    ('demo-take-02.mp4', 16, 8, 'lesson.mp4'),
)
for filename, start, duration, name in clips:
    destination = public / name
    if destination.exists():
        print(f'Keeping existing {destination}')
        continue
    subprocess.run([
        'ffmpeg', '-hide_banner', '-loglevel', 'error', '-ss', str(start),
        '-i', str(source / filename), '-t', str(duration), '-vf',
        'crop=1920:1080:50:474,scale=1280:720', '-r', '30', '-c:v',
        'libx264', '-preset', 'veryfast', '-crf', '20', '-an', str(destination),
    ], check=True)
    print(f'Created {destination}')
