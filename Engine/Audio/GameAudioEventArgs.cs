using System;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio event arguments class
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="effect">Effect</param>
    public class GameAudioEventArgs(IGameAudioEffect effect) : EventArgs
    {
        /// <summary>
        /// Effect
        /// </summary>
        public IGameAudioEffect Effect { get; private set; } = effect;
    }
}
