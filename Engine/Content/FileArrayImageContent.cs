﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine.Content
{
    using Engine.Common;

    /// <summary>
    /// Image content array from file
    /// </summary>
    public sealed class FileArrayImageContent : IImageContent, IEquatable<FileArrayImageContent>
    {
        /// <summary>
        /// Image array paths
        /// </summary>
        private readonly IEnumerable<string> paths;
        /// <summary>
        /// Crop rectangle
        /// </summary>
        private readonly Rectangle cropRectangle;
        /// <inheritdoc/>
        public int Count
        {
            get
            {
                return paths.Count();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="paths">Paths to image</param>
        /// <param name="cropRectangle">Crop rectangle</param>
        public FileArrayImageContent(string path, Rectangle? cropRectangle = null)
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

            paths = p;
            this.cropRectangle = cropRectangle ?? Rectangle.Empty;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="contentFileName">Path to image</param>
        /// <param name="cropRectangle">Crop rectangle</param>
        public FileArrayImageContent(string contentFolder, string contentFileName, Rectangle? cropRectangle = null)
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

            paths = p;
            this.cropRectangle = cropRectangle ?? Rectangle.Empty;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="paths">Paths to images</param>
        /// <param name="cropRectangle">Crop rectangle</param>
        public FileArrayImageContent(IEnumerable<string> paths, Rectangle? cropRectangle = null)
        {
            this.paths = paths ?? throw new ArgumentNullException(nameof(paths), "A path array must be specified.");
            this.cropRectangle = cropRectangle ?? Rectangle.Empty;

            if (!paths.Any(s => s != null))
            {
                throw new ArgumentOutOfRangeException(nameof(paths), $"At least, one path must be specified in the array.");
            }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="contentFileNames">Paths to images</param>
        /// <param name="cropRectangle">Crop rectangle</param>
        public FileArrayImageContent(string contentFolder, IEnumerable<string> contentFileNames, Rectangle? cropRectangle = null)
        {
            if (contentFileNames == null)
            {
                throw new ArgumentNullException(nameof(contentFileNames), "A content file name list must be specified.");
            }

            if (!contentFileNames.Any(s => s != null))
            {
                throw new ArgumentOutOfRangeException(nameof(contentFileNames), $"At least, one content file name must be specified in the array.");
            }

            var p = ContentManager.FindPaths(contentFolder, contentFileNames);
            if (!p.Any())
            {
                throw new ArgumentException("The specified content file name list has no results.", nameof(contentFileNames));
            }

            paths = p;
            this.cropRectangle = cropRectangle ?? Rectangle.Empty;
        }

        /// <inheritdoc/>
        public EngineShaderResourceView CreateResource(Game game, bool mipAutogen = true, bool dynamic = false)
        {
            return game.Graphics.LoadTextureArray(GetResourceKey(), paths, cropRectangle, mipAutogen, dynamic);
        }
        /// <inheritdoc/>
        public string GetResourceKey()
        {
            return paths.GetMd5Sum();
        }

        /// <inheritdoc/>
        public static bool operator !=(FileArrayImageContent left, FileArrayImageContent right)
        {
            return !(left == right);
        }
        /// <inheritdoc/>
        public static bool operator ==(FileArrayImageContent left, FileArrayImageContent right)
        {
            return
                Helper.ListIsEqual(left.paths, right.paths) &&
                left.cropRectangle == right.cropRectangle;
        }
        /// <inheritdoc/>
        public bool Equals(FileArrayImageContent other)
        {
            return this == other;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is FileArrayImageContent content)
            {
                return this == content;
            }

            return false;
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return paths.GetHashCode() ^ cropRectangle.GetHashCode();
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Path array: {paths.Count()} elements;";
        }
    }
}