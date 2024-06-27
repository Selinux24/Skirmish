using System;

namespace Engine.Collada
{
    public class ContentColladaException : Exception
    {
        public ContentColladaException() { }
        public ContentColladaException(string message) : base(message) { }
        public ContentColladaException(string message, Exception inner) : base(message, inner) { }
    }
}
