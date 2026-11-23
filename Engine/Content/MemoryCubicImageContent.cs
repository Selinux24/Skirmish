using Engine.Common;
using Engine.Mathematics;
using System;
using System.IO;

namespace Engine.Content
{
    /// <summary>
    /// Cubic image content from memory
    /// </summary>
    public sealed class MemoryCubicImageContent : IImageContent, IEquatable<MemoryCubicImageContent>
    {
        const string CubicTextureFacesStr = "A list of 6 faces must be specified, or not faces at all.";

        /// <summary>
        /// Image data in stream
        /// </summary>
        private readonly MemoryStream stream;
        /// <summary>
        /// Texture faces
        /// </summary>
        private readonly RectI[] faces;
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">Texture stream</param>
        /// <param name="faces">Texture faces</param>
        public MemoryCubicImageContent(MemoryStream stream, RectI[] faces = null)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (faces != null && faces.Length != 0 && faces.Length != 6)
            {
                throw new ArgumentException(CubicTextureFacesStr, nameof(faces));
            }

            this.stream = stream;
            this.faces = faces ?? [];
        }

        /// <inheritdoc/>
        public EngineShaderResourceView CreateResource(Game game, string name, bool mipAutogen = true, bool dynamic = false)
        {
            Name = name;

            return game.Graphics.LoadTextureCubic($"{name}_{GetResourceKey()}", stream, faces, mipAutogen, dynamic);
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
                Helper.CompareArrays(left.faces, right.faces);
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
