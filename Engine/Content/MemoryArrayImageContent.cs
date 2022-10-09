using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine.Content
{
    using Engine.Common;

    /// <summary>
    /// Image content array from memory
    /// </summary>
    public sealed class MemoryArrayImageContent : IImageContent, IEquatable<MemoryArrayImageContent>
    {
        /// <summary>
        /// Image array streams
        /// </summary>
        private readonly IEnumerable<MemoryStream> streams;
        /// <summary>
        /// Crop rectangle
        /// </summary>
        private readonly Rectangle cropRectangle;
        /// <inheritdoc/>
        public string Name { get; private set; }
        /// <inheritdoc/>
        public int Count
        {
            get
            {
                return streams.Count();
            }
        }

        /// <summary>
        /// Cosntructor
        /// </summary>
        /// <param name="streams">Texture streams</param>
        /// <param name="cropRectangle">Crop rectangle</param>
        public MemoryArrayImageContent(IEnumerable<MemoryStream> streams, Rectangle? cropRectangle = null)
        {
            this.streams = streams ?? throw new ArgumentNullException(nameof(streams), "A stream array must be specified.");
            this.cropRectangle = cropRectangle ?? Rectangle.Empty;

            if (!streams.Any(s => s != null))
            {
                throw new ArgumentOutOfRangeException(nameof(streams), $"At least, one stream must be specified in the array.");
            }
        }

        /// <inheritdoc/>
        public EngineShaderResourceView CreateResource(Game game, string name, bool mipAutogen = true, bool dynamic = false)
        {
            Name = name;

            if (cropRectangle == Rectangle.Empty)
            {
                return game.Graphics.LoadTextureArray($"{name}_{GetResourceKey()}", streams, mipAutogen, dynamic);
            }
            else
            {
                return game.Graphics.LoadTextureArray($"{name}_{GetResourceKey()}", streams, cropRectangle, mipAutogen, dynamic);
            }
        }
        /// <inheritdoc/>
        public string GetResourceKey()
        {
            return streams.GetMd5Sum();
        }

        /// <inheritdoc/>
        public static bool operator !=(MemoryArrayImageContent left, MemoryArrayImageContent right)
        {
            return !(left == right);
        }
        /// <inheritdoc/>
        public static bool operator ==(MemoryArrayImageContent left, MemoryArrayImageContent right)
        {
            return
                Helper.CompareEnumerables(left.streams, right.streams) &&
                left.cropRectangle == right.cropRectangle;
        }
        /// <inheritdoc/>
        public bool Equals(MemoryArrayImageContent other)
        {
            return this == other;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is MemoryArrayImageContent content)
            {
                return this == content;
            }

            return false;
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return streams.GetHashCode() ^ cropRectangle.GetHashCode();
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Stream array: {streams.Count()} elements;";
        }
    }
}
