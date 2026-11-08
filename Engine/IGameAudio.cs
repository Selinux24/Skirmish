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
        public uint InputSampleRate { get; }
        /// <summary>
        /// Output channels
        /// </summary>
        public uint InputChannelCount { get; }
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
        /// Gets the operation set Id
        /// </summary>
        /// <remarks>Returns the commit now Id if not operation set is active</remarks>
        uint OperationSet { get; }

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
        void SetMasteringLimit(uint release, uint loudness);

        /// <summary>
        /// Starts a new operation Set
        /// </summary>
        /// <returns>Returns the operation set Id</returns>
        uint StartOperationSet();
        /// <summary>
        /// Commits the specified operation set
        /// </summary>
        /// <param name="operationSet">Operation set</param>
        void CommitOperation(uint operationSet);
    }
}
