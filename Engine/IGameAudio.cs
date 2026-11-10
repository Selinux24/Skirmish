using Engine.Audio;
using System;

namespace Engine
{
    /// <summary>
    /// Game audio interface
    /// </summary>
    public interface IGameAudio : IDisposable
    {
        /// <summary>
        /// Gets whether the audio device is currently running or not
        /// </summary>
        bool Running { get; }
        /// <summary>
        /// Input sample rate
        /// </summary>
        uint InputSampleRate { get; }
        /// <summary>
        /// Output channels
        /// </summary>
        uint InputChannelCount { get; }
        /// <summary>
        /// Use redirect to LFE
        /// </summary>
        bool UseRedirectToLFE { get; }

        /// <summary>
        /// Gets or sets the master volume value
        /// </summary>
        /// <remarks>From 0 to 1</remarks>
        float MasterVolume { get; set; }
        /// <summary>
        /// Gets whether the master voice uses a limiter or not
        /// </summary>
        bool UseMasteringLimiter { get; }
        /// <summary>
        /// Mastering release limit
        /// </summary>
        uint ReleaseLimit { get; }
        /// <summary>
        /// Mastering loudness limit
        /// </summary>
        uint LoudnessLimit { get; }

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
        /// Enables the mastering limiter
        /// </summary>
        /// <param name="release">Speed at which the limiter stops affecting audio once it drops below the limiter's threshold</param>
        /// <param name="loudness">Threshold of the limiter</param>
        void EnableMasteringLimiter(uint release, uint loudness);
        /// <summary>
        /// Disables the mastering limiter
        /// </summary>
        void DisableMasteringLimiter();

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
