using System;

namespace Engine
{
    using Engine.Audio;

    /// <summary>
    /// Game audio interface
    /// </summary>
    public interface IGameAudio : IDisposable
    {
        /// <summary>
        /// Input sample rate
        /// </summary>
        public int InputSampleRate { get; }
        /// <summary>
        /// Output channels
        /// </summary>
        public int InputChannelCount { get; }
        /// <summary>
        /// Use redirect to LFE
        /// </summary>
        public bool UseRedirectToLFE { get; }

        /// <summary>
        /// Gets or sets the master volume value
        /// </summary>
        /// <remarks>From 0 to 1</remarks>
        float MasterVolume { get; set; }
        /// <summary>
        /// Gets or sets whether the master voice uses a limiter or not
        /// </summary>
        bool UseMasteringLimiter { get; set; }

        /// <summary>
        /// Creates a new effect
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="effectParameters">Effect parameters</param>
        IGameAudioEffect CreateEffect(string fileName, GameAudioEffectParameters effectParameters);

        /// <summary>
        /// Starts the audio device
        /// </summary>
        void Start();
        /// <summary>
        /// Stops the audio device
        /// </summary>
        void Stop();

        /// <summary>
        /// Sets the mastering limiter parameters
        /// </summary>
        /// <param name="release">Speed at which the limiter stops affecting audio once it drops below the limiter's threshold</param>
        /// <param name="loudness">Threshold of the limiter</param>
        void SetMasteringLimit(int release, int loudness);
    }
}
