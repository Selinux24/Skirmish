using System;
using System.Runtime.Serialization;

namespace Engine
{
    [Serializable]
    public class EngineException : Exception
    {
        public EngineException() { }
        public EngineException(string message) : base(message) { }
        public EngineException(string message, Exception inner) : base(message, inner) { }
        protected EngineException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
