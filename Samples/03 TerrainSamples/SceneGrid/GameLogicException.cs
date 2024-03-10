using System;

namespace TerrainSamples.SceneGrid
{
    public class GameLogicException : Exception
    {
        public GameLogicException() { }
        public GameLogicException(string message) : base(message) { }
        public GameLogicException(string message, Exception inner) : base(message, inner) { }
    }
}
