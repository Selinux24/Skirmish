using System;

namespace Engine.Collada
{
    [Serializable]
    public class ContentColladaException : Exception
    {
        public ContentColladaException() { }
        public ContentColladaException(string message) : base(message) { }
        public ContentColladaException(string message, Exception inner) : base(message, inner) { }
        protected ContentColladaException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
