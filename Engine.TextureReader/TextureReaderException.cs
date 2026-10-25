using System;

namespace Engine.TextureReader
{
    public class TextureReaderException : Exception
    {
        public TextureReaderException() { }
        public TextureReaderException(string message) : base(message) { }
        public TextureReaderException(string message, Exception inner) : base(message, inner) { }
    }
}
