using System;

namespace TerrainSamples.SceneGrid
{
    [Serializable]
    public class GameLogicException : Exception
    {
        public GameLogicException() { }
        public GameLogicException(string message) : base(message) { }
        public GameLogicException(string message, Exception inner) : base(message, inner) { }
        protected GameLogicException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
