using SharpDX;
using System;
using System.IO;

namespace Engine.Content
{
    using Engine.Common;

    /// <summary>
    /// Image content from memory
    /// </summary>
    public sealed class MemoryImageContent : IImageContent, IEquatable<MemoryImageContent>
    {
        /// <summary>
        /// Image data in stream
        /// </summary>
        private readonly MemoryStream stream;
        /// <summary>
        /// Crop rectangle
        /// </summary>
        private readonly Rectangle cropRectangle;
        /// <inheritdoc/>
        public int Count
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">Memory stream</param>
        /// <param name="cropRectangle">Crop rectangle</param>
        public MemoryImageContent(MemoryStream stream, Rectangle? cropRectangle = null)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream), "A stream must be specified.");
            this.cropRectangle = cropRectangle ?? Rectangle.Empty;
        }

        /// <inheritdoc/>
        public EngineShaderResourceView CreateResource(Game game, bool mipAutogen = true, bool dynamic = false)
        {
            if (cropRectangle == Rectangle.Empty)
            {
                return game.Graphics.LoadTexture(GetResourceKey(), stream, mipAutogen, dynamic);
            }
            else
            {
                return game.Graphics.LoadTexture(GetResourceKey(), stream, cropRectangle, mipAutogen, dynamic);
            }
        }
        /// <inheritdoc/>
        public string GetResourceKey()
        {
            return stream.GetMd5Sum();
        }

        /// <inheritdoc/>
        public static bool operator !=(MemoryImageContent left, MemoryImageContent right)
        {
            return !(left == right);
        }
        /// <inheritdoc/>
        public static bool operator ==(MemoryImageContent left, MemoryImageContent right)
        {
            return
                left.stream == right.stream &&
                left.cropRectangle == right.cropRectangle;
        }
        /// <inheritdoc/>
        public bool Equals(MemoryImageContent other)
        {
            return this == other;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is MemoryImageContent content)
            {
                return this == content;
            }

            return false;
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return stream.GetHashCode() ^ cropRectangle.GetHashCode();
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Stream: {stream.Length} bytes;";
        }
    }
}
