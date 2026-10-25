using System;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio loader
    /// </summary>
    public class GameAudioLoader : IGameAudioLoader
    {
        /// <inheritdoc/>
        public Func<IGameAudio> GetDelegate(uint sampleRate)
        {
            return () => { return new GameAudio(sampleRate); };
        }
    }
}
