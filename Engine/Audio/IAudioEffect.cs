﻿using System;

namespace Engine.Audio
{
    /// <summary>
    /// Audio effect interface
    /// </summary>
    public interface IAudioEffect : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this instance is looped.
        /// </summary>
        bool IsLooped { get; set; }
        /// <summary>
        /// Gets or sets the pan value of the sound effect.
        /// </summary>
        /// <remarks>The value is clamped to (-1f, 1f) range.</remarks>
        /// <exception cref="ObjectDisposedException">Is thrown if the current instance was already disposed.</exception>
        float Pan { get; set; }
        /// <summary>
        /// Gets or sets the pitch value of the sound effect.
        /// </summary>
        /// <remarks>The value is clamped to (-1f, 1f) range.</remarks>
        /// <exception cref="ObjectDisposedException">Is thrown if the current instance was already disposed.</exception>
        float Pitch { get; set; }
        /// <summary>
        /// Gets or sets the volume of the current sound effect instance.
        /// </summary>
        /// <remarks>The value is clamped to (0f, 1f) range.</remarks>
        /// <exception cref="ObjectDisposedException">Is thrown if the current instance was already disposed.</exception>
        float Volume { get; set; }

        /// <summary>
        /// Gets or sets whether the master voice uses 3D audio or not
        /// </summary>
        bool UseAudio3D { get; set; }
        /// <summary>
        /// Emitter
        /// </summary>
        IGameAudioEmitter Emitter { get; set; }
        /// <summary>
        /// Listener
        /// </summary>
        IGameAudioListener Listener { get; set; }

        /// <summary>
        /// Gets the effect total duration
        /// </summary>
        TimeSpan Duration { get; }
        /// <summary>
        /// Gets the state of the current sound effect instance.
        /// </summary>
        AudioState State { get; }
        /// <summary>
        /// The instance is due to dispose
        /// </summary>
        bool DueToDispose { get; }
        /// <summary>
        /// Gets or sets the position in seconds.
        /// </summary>
        /// <value>The position.</value>
        TimeSpan Position { get; set; }

        /// <summary>
        /// Event fired when the audio starts
        /// </summary>
        event GameAudioHandler AudioStart;
        /// <summary>
        /// Event fired when the audio ends
        /// </summary>
        event GameAudioHandler AudioEnd;
        /// <summary>
        /// Event fired when a loop ends
        /// </summary>
        event GameAudioHandler LoopEnd;
        /// <summary>
        /// Event fired when the play progressed
        /// </summary>
        event GameAudioProgressHandler PlayProgress;

        /// <summary>
        /// Plays the current instance. If it is already playing - the call is ignored.
        /// </summary>
        void Play();
        /// <summary>
        /// Plays the current instance. If it is already playing - the call is ignored.
        /// </summary>
        /// <param name="start">Start position</param>
        void Play(TimeSpan start);
        /// <summary>
        /// Stops the playback of the current instance indicating whether the stop should occur immediately of at the end of the sound.
        /// </summary>
        /// <param name="immediate">A value indicating whether the playback should be stopped immediately or at the end of the sound.</param>
        void Stop(bool immediate = true);
        /// <summary>
        /// Pauses the playback of the current instance.
        /// </summary>
        void Pause();
        /// <summary>
        /// Resumes playback of the current instance.
        /// </summary>
        void Resume();
        /// <summary>
        /// Resets the current instance.
        /// </summary>
        void Reset();

        /// <summary>
        /// Applies the 3D effect to the current sound effect instance.
        /// </summary>
        /// <param name="gameTime">Game time</param>
        void Update(GameTime gameTime);

        /// <summary>
        /// Gets the reverb effect
        /// </summary>
        ReverbPresets? GetReverb();
        /// <summary>
        /// Set reverb to voice
        /// </summary>
        /// <param name="reverb">Reverb index</param>
        bool SetReverb(ReverbPresets? reverb);

        /// <summary>
        /// Gets the output matrix configuration
        /// </summary>
        /// <returns>Returns an array of floats from 0 to 1.</returns>
        float[] GetOutputMatrix();
    }
}
