import React from 'react';
import {AbsoluteFill, Audio, Composition, interpolate, OffthreadVideo, Sequence, staticFile, useCurrentFrame} from 'remotion';

const fps = 30;

// Edit the sequence here. Each clip is a real recording of the game.
const shots = [
  {file: 'menu.mp4', seconds: 5},
  {file: 'activities.mp4', seconds: 5},
  {file: 'rocket.mp4', seconds: 9},
  {file: 'quiz.mp4', seconds: 9},
  {file: 'lesson.mp4', seconds: 8},
] as const;

function GameShot({file}: {file: string}) {
  return <AbsoluteFill style={{backgroundColor: '#d9f1f5'}}>
    <OffthreadVideo src={staticFile(file)} muted style={{width: '100%', height: '100%', objectFit: 'contain'}} />
  </AbsoluteFill>;
}

function EndCard() {
  const frame = useCurrentFrame();
  const opacity = interpolate(frame, [0, 15], [0, 1], {extrapolateRight: 'clamp'});
  return <AbsoluteFill style={{background: '#d9f1f5', color: '#1c3742', display: 'flex', justifyContent: 'center', alignItems: 'center', fontFamily: 'Fredoka, sans-serif', opacity}}>
    <div style={{fontSize: 100, fontWeight: 600}}>FUN MATH</div>
    <div style={{fontSize: 42, marginTop: 20}}>Jom belajar matematik bersama!</div>
  </AbsoluteFill>;
}

function Film({music = true}: {music?: boolean}) {
  let start = 0;
  return <AbsoluteFill>
    <style>{`@font-face {font-family: Fredoka; src: url('${staticFile('Fredoka.ttf')}');}`}</style>
    {music ? <Audio src={staticFile('music.wav')} volume={2.5} /> : null}
    {shots.map((shot) => {
      const frames = shot.seconds * fps;
      const at = start;
      start += frames;
      return <Sequence key={shot.file} from={at} durationInFrames={frames}><GameShot file={shot.file} /></Sequence>;
    })}
    <Sequence from={start} durationInFrames={4 * fps}><EndCard /></Sequence>
  </AbsoluteFill>;
}

const totalFrames = (shots.reduce((sum, shot) => sum + shot.seconds, 0) + 4) * fps;

export const VideoRoot = () => <>
  <Composition id="MathFunPreview" component={Film} width={1920} height={1080} fps={fps} durationInFrames={totalFrames} />
  <Composition id="MathFunFinal" component={Film} width={1920} height={1080} fps={fps} durationInFrames={totalFrames} defaultProps={{music: true}} />
  <Composition id="MathFunFinalNoMusic" component={Film} width={1920} height={1080} fps={fps} durationInFrames={totalFrames} defaultProps={{music: false}} />
</>;
