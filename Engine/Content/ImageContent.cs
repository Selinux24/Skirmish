using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine.Content
{
    /// <summary>
    /// Image content
    /// </summary>
    public class ImageContent
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
        /// Creates a unique texture image
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="texture">Path to texture</param>
        /// <returns>Returns content</returns>
        public static ImageContent Texture(string contentFolder, string texture)
        {
            var p = ContentManager.FindPaths(contentFolder, texture);

            return new ImageContent()
            {
                Paths = p,
            };
        }
        /// <summary>
        /// Creates a unique texture image
        /// </summary>
        /// <param name="texture">Texture stream</param>
        /// <returns>Returns content</returns>
        public static ImageContent Texture(MemoryStream texture)
        {
            return new ImageContent()
            {
                Stream = texture,
            };
        }
        /// <summary>
        /// Creates a texture array image
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="textures">Paths to textures</param>
        /// <returns>Returns content</returns>
        public static ImageContent Array(string contentFolder, IEnumerable<string> textures)
        {
            var p = ContentManager.FindPaths(contentFolder, textures);

            return new ImageContent()
            {
                Paths = p,
            };
        }
        /// <summary>
        /// Creates a texture array image
        /// </summary>
        /// <param name="textures">Texture streams</param>
        /// <returns>Returns content</returns>
        public static ImageContent Array(IEnumerable<MemoryStream> textures)
        {
            return new ImageContent()
            {
                Streams = textures,
            };
        }
        /// <summary>
        /// Creates a cubic texture image
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="texture">Path to texture</param>
        /// <returns>Returns content</returns>
        public static ImageContent Cubic(string contentFolder, string texture)
        {
            var p = ContentManager.FindPaths(contentFolder, texture);

            return new ImageContent()
            {
                Paths = p,
                IsCubic = true,
            };
        }
        /// <summary>
        /// Creates a cubic texture image
        /// </summary>
        /// <param name="texture">Texture stream</param>
        /// <returns>Returns content</returns>
        public static ImageContent Cubic(MemoryStream texture)
        {
            return new ImageContent()
            {
                Stream = texture,
                IsCubic = true,
            };
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
