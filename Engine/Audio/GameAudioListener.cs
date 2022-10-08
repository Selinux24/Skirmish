﻿
namespace Engine.Audio
{
    /// <summary>
    /// Game audio listener
    /// </summary>
    class GameAudioListener : GameAudioAgent, IGameAudioListener
    {
        /// <summary>
        /// Cone
        /// </summary>
        public GameAudioConeDescription? Cone { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GameAudioListener() : base()
        {

        }
    }
}
