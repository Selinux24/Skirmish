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
                return this.Streams.FirstOrDefault();
            }
            set
            {
                if (value == null)
                {
                    this.Streams = new MemoryStream[] { };
                }
                else
                {
                    this.Streams = new[] { value };
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
                return this.Paths.FirstOrDefault();
            }
            set
            {
                if (value == null)
                {
                    this.Paths = new string[] { };
                }
                else
                {
                    this.Paths = new[] { value };
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
                return (this.Paths.Count() > 1) || (this.Streams.Count() > 1);
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
                return this.Paths.Count() + this.Streams.Count();
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

        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation of instance</returns>
        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(this.Path))
            {
                return string.Format("Path: {0}; ", this.Path);
            }
            if (this.Paths.Any())
            {
                return string.Format("Path array: {0}; ", this.Paths.Count());
            }
            else if (this.Stream != null)
            {
                return string.Format("Stream: {0} bytes; ", this.Stream.Length);
            }
            else if (this.Streams.Any())
            {
                return string.Format("Stream array: {0}; ", this.Streams.Count());
            }
            else
            {
                return "Empty;";
            }
        }
    }
}
