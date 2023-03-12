using System;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio event arguments class
    /// </summary>
    public class GameAudioEventArgs : EventArgs
    {
        /// <summary>
        /// Effect
        /// </summary>
        public IAudioEffect Effect { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="effect">Effect</param>
        public GameAudioEventArgs(IAudioEffect effect)
        {
            Effect = effect;
        }
    }
}
