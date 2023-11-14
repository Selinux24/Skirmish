using System;

namespace Engine
{
    /// <summary>
    /// Game audio loader interface
    /// </summary>
    public interface IGameAudioLoader
    {
        /// <summary>
        /// Gets the audio loader delegate
        /// </summary>
        /// <param name="sampleRate">Sample rate</param>
        Func<IGameAudio> GetDelegate(int sampleRate);
    }
}
