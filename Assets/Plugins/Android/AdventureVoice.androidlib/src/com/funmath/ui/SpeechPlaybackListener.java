package com.funmath.ui;

import android.speech.tts.UtteranceProgressListener;

/** Completion callbacks for existing Unity-owned TTS engines. No engine or audio player is created. */
public final class SpeechPlaybackListener extends UtteranceProgressListener {
    public interface Callback { void onFinished(String utteranceId); }
    private final Callback callback;
    public SpeechPlaybackListener(Callback callback) { this.callback = callback; }
    @Override public void onStart(String id) { }
    @Override public void onDone(String id) { callback.onFinished(id); }
    @Override public void onError(String id) { callback.onFinished(id); }
    @Override public void onError(String id, int code) { callback.onFinished(id); }
    @Override public void onStop(String id, boolean interrupted) { callback.onFinished(id); }
}
