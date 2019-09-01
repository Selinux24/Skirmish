using System;

namespace Engine
{
    /// <summary>
    /// Game status collected event
    /// </summary>
    public class GameStatusCollectedEventArgs : EventArgs
    {
        /// <summary>
        /// Time trace status
        /// </summary>
        public GameStatus Trace { get; set; } = new GameStatus();
    }
}
