using SharpDX;
using System;
using System.IO;

namespace Engine.Content
{
    using Engine.Common;

    /// <summary>
    /// Image content from memory
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="stream">Memory stream</param>
    /// <param name="cropRectangle">Crop rectangle</param>
    public sealed class MemoryImageContent(MemoryStream stream, Rectangle? cropRectangle = null) : IImageContent, IEquatable<MemoryImageContent>
    {
        /// <summary>
        /// Image data in stream
        /// </summary>
        private readonly MemoryStream stream = stream ?? throw new ArgumentNullException(nameof(stream), "A stream must be specified.");
        /// <summary>
        /// Crop rectangle
        /// </summary>
        private readonly Rectangle cropRectangle = cropRectangle ?? Rectangle.Empty;
        /// <inheritdoc/>
        public string Name { get; private set; }
        /// <inheritdoc/>
        public int Count
        {
            get
            {
                return 1;
            }
        }

        /// <inheritdoc/>
        public EngineShaderResourceView CreateResource(Game game, string name, bool mipAutogen = true, bool dynamic = false)
        {
            Name = name;

            if (cropRectangle == Rectangle.Empty)
            {
                return game.Graphics.LoadTexture($"{name}_{GetResourceKey()}", stream, mipAutogen, dynamic);
            }
            else
            {
                return game.Graphics.LoadTexture($"{name}_{GetResourceKey()}", stream, cropRectangle, mipAutogen, dynamic);
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
