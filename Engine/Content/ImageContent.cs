using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine.Content
{
    /// <summary>
    /// Image content
    /// </summary>
    public class ImageContent : IEquatable<ImageContent>
    {
        /// <summary>
        /// Image data in stream
        /// </summary>
        public MemoryStream Stream
        {
            get
            {
                return Streams.FirstOrDefault();
            }
            set
            {
                if (value == null)
                {
                    Streams = new MemoryStream[] { };
                }
                else
                {
                    Streams = new[] { value };
                }
            }
        }
        /// <summary>
        /// Image array streams
        /// </summary>
        public IEnumerable<MemoryStream> Streams { get; set; } = new MemoryStream[] { };
        /// <summary>
        /// Image path
        /// </summary>
        public string Path
        {
            get
            {
                return Paths.FirstOrDefault();
            }
            set
            {
                if (value == null)
                {
                    Paths = new string[] { };
                }
                else
                {
                    Paths = new[] { value };
                }
            }
        }
        /// <summary>
        /// Image array paths
        /// </summary>
        public IEnumerable<string> Paths { get; set; } = new string[] { };
        /// <summary>
        /// Gets whether the image content is an image array
        /// </summary>
        public bool IsArray
        {
            get
            {
                return (Paths.Count() > 1) || (Streams.Count() > 1);
            }
        }
        /// <summary>
        /// Gets or sets whether the image content is cubic
        /// </summary>
        public bool IsCubic { get; set; }
        /// <summary>
        /// Gets the image count into the image content
        /// </summary>
        public int Count
        {
            get
            {
                return Paths.Count() + Streams.Count();
            }
        }
        /// <summary>
        /// Crop rectangle
        /// </summary>
        public Rectangle CropRectangle { get; set; } = Rectangle.Empty;
        /// <summary>
        /// Texture faces
        /// </summary>
        public Rectangle[] Faces { get; set; } = new Rectangle[] { };

        /// <summary>
        /// Creates a unique texture image
        /// </summary>
        /// <param name="texture">Path to texture</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns content</returns>
        public static ImageContent Texture(string texture, Rectangle? rectangle = null)
        {
            string directory = System.IO.Path.GetDirectoryName(texture);
            string fileName = System.IO.Path.GetFileName(texture);

            return Texture(directory, fileName, rectangle);
        }
        /// <summary>
        /// Creates a unique texture image
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="texture">Path to texture</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns content</returns>
        public static ImageContent Texture(string contentFolder, string texture, Rectangle? rectangle = null)
        {
            var p = ContentManager.FindPaths(contentFolder, texture);

            return new ImageContent()
            {
                Paths = p,
                CropRectangle = rectangle ?? Rectangle.Empty,
            };
        }
        /// <summary>
        /// Creates a unique texture image
        /// </summary>
        /// <param name="texture">Texture stream</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns content</returns>
        public static ImageContent Texture(MemoryStream texture, Rectangle? rectangle = null)
        {
            return new ImageContent()
            {
                Stream = texture,
                CropRectangle = rectangle ?? Rectangle.Empty,
            };
        }
        /// <summary>
        /// Creates a texture array image
        /// </summary>
        /// <param name="textures">Paths to textures</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns content</returns>
        public static ImageContent Array(IEnumerable<string> textures, Rectangle? rectangle = null)
        {
            return Array(string.Empty, textures, rectangle);
        }
        /// <summary>
        /// Creates a texture array image
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="textures">Paths to textures</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns content</returns>
        public static ImageContent Array(string contentFolder, IEnumerable<string> textures, Rectangle? rectangle = null)
        {
            var p = ContentManager.FindPaths(contentFolder, textures);

            return new ImageContent()
            {
                Paths = p,
                CropRectangle = rectangle ?? Rectangle.Empty,
            };
        }
        /// <summary>
        /// Creates a texture array image
        /// </summary>
        /// <param name="textures">Texture streams</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns content</returns>
        public static ImageContent Array(IEnumerable<MemoryStream> textures, Rectangle? rectangle = null)
        {
            return new ImageContent()
            {
                Streams = textures,
                CropRectangle = rectangle ?? Rectangle.Empty,
            };
        }
        /// <summary>
        /// Creates a cubic texture image
        /// </summary>
        /// <param name="texture">Path to texture</param>
        /// <param name="faces">Texture faces</param>
        /// <returns>Returns content</returns>
        public static ImageContent Cubic(string texture, Rectangle[] faces = null)
        {
            string directory = System.IO.Path.GetDirectoryName(texture);
            string fileName = System.IO.Path.GetFileName(texture);

            return Cubic(directory, fileName, faces);
        }
        /// <summary>
        /// Creates a cubic texture image
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="texture">Path to texture</param>
        /// <param name="faces">Texture faces</param>
        /// <returns>Returns content</returns>
        public static ImageContent Cubic(string contentFolder, string texture, Rectangle[] faces = null)
        {
            var p = ContentManager.FindPaths(contentFolder, texture);

            return new ImageContent()
            {
                Paths = p,
                IsCubic = true,
                Faces = faces,
            };
        }
        /// <summary>
        /// Creates a cubic texture image
        /// </summary>
        /// <param name="texture">Texture stream</param>
        /// <param name="faces">Texture faces</param>
        /// <returns>Returns content</returns>
        public static ImageContent Cubic(MemoryStream texture, Rectangle[] faces = null)
        {
            return new ImageContent()
            {
                Stream = texture,
                IsCubic = true,
                Faces = faces,
            };
        }

        /// <inheritdoc/>
        public static bool operator !=(ImageContent left, ImageContent right)
        {
            return !(left == right);
        }
        /// <inheritdoc/>
        public static bool operator ==(ImageContent left, ImageContent right)
        {
            return
                Helper.ListIsEqual(left.Streams, right.Streams) &&
                Helper.ListIsEqual(left.Paths, right.Paths) &&
                left.IsCubic == right.IsCubic &&
                left.CropRectangle == right.CropRectangle &&
                Helper.ListIsEqual(left.Faces, right.Faces);
        }
        /// <inheritdoc/>
        public bool Equals(ImageContent other)
        {
            return this == other;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is ImageContent content)
            {
                return this == content;
            }

            return false;
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Path))
            {
                return $"Path: {Path};";
            }
            if (Paths.Any())
            {
                return $"Path array: {Paths.Count()} elements;";
            }
            else if (Stream != null)
            {
                return $"Stream: {Stream.Length} bytes;";
            }
            else if (Streams.Any())
            {
                return $"Stream array: {Streams.Count()} elements;";
            }
            else
            {
                return "Empty;";
            }
        }
    }
}
