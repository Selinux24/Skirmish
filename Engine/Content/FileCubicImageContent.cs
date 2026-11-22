using Engine.Common;
using Engine.Mathematics;
using System;

namespace Engine.Content
{
    /// <summary>
    /// Cubic image content from file
    /// </summary>
    public sealed class FileCubicImageContent : IImageContent, IEquatable<FileCubicImageContent>
    {
        /// <summary>
        /// Image path
        /// </summary>
        private readonly string path;
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
        /// <param name="path">Path to image</param>
        /// <param name="faces">Cube faces</param>
        public FileCubicImageContent(string path, RectI[] faces)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            ArgumentNullException.ThrowIfNull(faces);
            if (faces.Length != 6)
            {
                throw new ArgumentException($"A list o 6 faces must be specified.", nameof(faces));
            }

            var p = ContentManager.FindPaths(path);
            if (p.Length == 0)
            {
                throw new ArgumentException("The specified path has no results.", nameof(path));
            }

            this.path = p[0];
            this.faces = faces;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="contentFileName">Path to image</param>
        /// <param name="faces">Cube faces</param>
        public FileCubicImageContent(string contentFolder, string contentFileName, RectI[] faces = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(contentFileName);
            ArgumentNullException.ThrowIfNull(faces);
            if (faces.Length != 6)
            {
                throw new ArgumentException($"A list o 6 faces must be specified.", nameof(faces));
            }

            var p = ContentManager.FindPaths(contentFolder, contentFileName);
            if (p.Length == 0)
            {
                throw new ArgumentException("The specified content file name has no results.", nameof(contentFileName));
            }

            path = p[0];
            this.faces = faces;
        }

        /// <inheritdoc/>
        public EngineShaderResourceView CreateResource(Game game, string name, bool mipAutogen = true, bool dynamic = false)
        {
            Name = name;

            return game.Graphics.LoadTextureCubic($"{name}_{GetResourceKey()}", path, faces, mipAutogen, dynamic);
        }
        /// <inheritdoc/>
        public string GetResourceKey()
        {
            return path.GetMd5Sum();
        }

        /// <inheritdoc/>
        public static bool operator !=(FileCubicImageContent left, FileCubicImageContent right)
        {
            return !(left == right);
        }
        /// <inheritdoc/>
        public static bool operator ==(FileCubicImageContent left, FileCubicImageContent right)
        {
            return
                left.path == right.path &&
                Helper.CompareArrays(left.faces, right.faces);
        }
        /// <inheritdoc/>
        public bool Equals(FileCubicImageContent other)
        {
            return this == other;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is FileCubicImageContent content)
            {
                return this == content;
            }

            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(path, faces);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Cubic Path: {path};";
        }
    }
}
