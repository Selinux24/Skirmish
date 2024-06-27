using System;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio loader
    /// </summary>
    public class GameAudioLoader : IGameAudioLoader
    {
        /// <inheritdoc/>
        public Func<IGameAudio> GetDelegate(int sampleRate)
        {
            return () => { return new GameAudio(sampleRate); };
        }
    }
}
