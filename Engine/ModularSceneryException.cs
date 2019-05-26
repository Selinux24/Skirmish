using System;

namespace Engine
{
    /// <summary>
    /// Modular scenery exception
    /// </summary>
    [Serializable]
    public class ModularSceneryException : EngineException
    {
        public ModularSceneryException() { }
        public ModularSceneryException(string message) : base(message) { }
        public ModularSceneryException(string message, Exception inner) : base(message, inner) { }
        protected ModularSceneryException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
