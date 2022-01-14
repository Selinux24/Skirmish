using SharpDX;
using System;
using System.IO;
using System.Linq;

namespace Engine.Content
{
    using Engine.Common;

    /// <summary>
    /// Cubic image content from memory
    /// </summary>
    public sealed class MemoryCubicImageContent : IImageContent, IEquatable<MemoryCubicImageContent>
    {
        /// <summary>
        /// Image data in stream
        /// </summary>
        private readonly MemoryStream stream;
        /// <summary>
        /// Texture faces
        /// </summary>
        private readonly Rectangle[] faces;
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
        /// <param name="stream">Texture stream</param>
        /// <param name="faces">Texture faces</param>
        public MemoryCubicImageContent(MemoryStream stream, Rectangle[] faces = null)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream), "A stream must be specified.");
            this.faces = faces ?? throw new ArgumentNullException(nameof(faces), "A cube face list must be specified.");

            if (faces.Count() != 6)
            {
                throw new ArgumentOutOfRangeException(nameof(faces), $"A list o 6 faces must be specified.");
            }
        }

        /// <inheritdoc/>
        public EngineShaderResourceView CreateResource(Game game, bool mipAutogen = true, bool dynamic = false)
        {
            return game.Graphics.LoadTextureCubic(GetResourceKey(), stream, faces, mipAutogen, dynamic);
        }
        /// <inheritdoc/>
        public string GetResourceKey()
        {
            return stream.GetMd5Sum();
        }

        /// <inheritdoc/>
        public static bool operator !=(MemoryCubicImageContent left, MemoryCubicImageContent right)
        {
            return !(left == right);
        }
        /// <inheritdoc/>
        public static bool operator ==(MemoryCubicImageContent left, MemoryCubicImageContent right)
        {
            return
                left.stream == right.stream &&
                Helper.ListIsEqual(left.faces, right.faces);
        }
        /// <inheritdoc/>
        public bool Equals(MemoryCubicImageContent other)
        {
            return this == other;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is MemoryCubicImageContent content)
            {
                return this == content;
            }

            return false;
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return stream.GetHashCode() ^ faces.GetHashCode();
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Stream cubic: {stream.Length} bytes;";
        }
    }
}
