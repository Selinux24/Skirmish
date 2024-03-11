using SharpDX;
using System;
using System.IO;
using System.Linq;

namespace Engine.Content
{
    using Engine.Common;

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
        private readonly Rectangle[] faces;
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
        public FileCubicImageContent(string path, Rectangle[] faces = null)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path), "A path must be specified.");
            }

            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            var p = ContentManager.FindPaths(directory, fileName);
            if (!p.Any())
            {
                throw new ArgumentException("The specified path has no results.", nameof(path));
            }

            this.path = p.First();
            this.faces = faces ?? throw new ArgumentNullException(nameof(faces), "A cube face list must be specified.");

            if (faces.Length != 6)
            {
                throw new ArgumentOutOfRangeException(nameof(faces), $"A list o 6 faces must be specified.");
            }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="contentFileName">Path to image</param>
        /// <param name="faces">Cube faces</param>
        public FileCubicImageContent(string contentFolder, string contentFileName, Rectangle[] faces = null)
        {
            if (contentFileName == null)
            {
                throw new ArgumentNullException(nameof(contentFileName), "A content file name must be specified.");
            }

            var p = ContentManager.FindPaths(contentFolder, contentFileName);
            if (!p.Any())
            {
                throw new ArgumentException("The specified content file name has no results.", nameof(contentFileName));
            }

            path = p.First();
            this.faces = faces ?? throw new ArgumentNullException(nameof(faces), "A cube face list must be specified.");

            if (faces.Length != 6)
            {
                throw new ArgumentOutOfRangeException(nameof(faces), $"A list o 6 faces must be specified.");
            }
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
                Helper.CompareEnumerables(left.faces, right.faces);
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
            return path.GetHashCode() ^ faces.GetHashCode();
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Cubic Path: {path};";
        }
    }
}
