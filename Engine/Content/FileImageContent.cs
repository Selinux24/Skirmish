using SharpDX;
using System;
using System.IO;
using System.Linq;

namespace Engine.Content
{
    using Engine.Common;

    /// <summary>
    /// Image content from file
    /// </summary>
    public sealed class FileImageContent : IImageContent, IEquatable<FileImageContent>
    {
        /// <summary>
        /// Image path
        /// </summary>
        private readonly string path;
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
                return 1;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path to image</param>
        /// <param name="cropRectangle">Crop rectangle</param>
        public FileImageContent(string path, Rectangle? cropRectangle = null)
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
            this.cropRectangle = cropRectangle ?? Rectangle.Empty;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="contentFileName">Path to image</param>
        /// <param name="cropRectangle">Crop rectangle</param>
        public FileImageContent(string contentFolder, string contentFileName, Rectangle? cropRectangle = null)
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
            this.cropRectangle = cropRectangle ?? Rectangle.Empty;
        }

        /// <inheritdoc/>
        public EngineShaderResourceView CreateResource(Game game, string name, bool mipAutogen = true, bool dynamic = false)
        {
            Name = name;

            if (cropRectangle == Rectangle.Empty)
            {
                return game.Graphics.LoadTexture($"{name}_{GetResourceKey()}", path, mipAutogen, dynamic);
            }
            else
            {
                return game.Graphics.LoadTexture($"{name}_{GetResourceKey()}", path, cropRectangle, mipAutogen, dynamic);
            }
        }
        /// <inheritdoc/>
        public string GetResourceKey()
        {
            return path.GetMd5Sum();
        }

        /// <inheritdoc/>
        public static bool operator !=(FileImageContent left, FileImageContent right)
        {
            return !(left == right);
        }
        /// <inheritdoc/>
        public static bool operator ==(FileImageContent left, FileImageContent right)
        {
            return
                left.path == right.path &&
                left.cropRectangle == right.cropRectangle;
        }
        /// <inheritdoc/>
        public bool Equals(FileImageContent other)
        {
            return this == other;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is FileImageContent content)
            {
                return this == content;
            }

            return false;
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return path.GetHashCode() ^ cropRectangle.GetHashCode();
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Path: {path};";
        }
    }
}
