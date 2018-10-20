using System.IO;

namespace Engine.Content
{
    /// <summary>
    /// Image content
    /// </summary>
    public class ImageContent
    {
        /// <summary>
        /// Path list
        /// </summary>
        private string[] paths = null;

        /// <summary>
        /// Image data in stream
        /// </summary>
        public MemoryStream Stream
        {
            get
            {
                return this.Streams != null && this.Streams.Length == 1 ? this.Streams[0] : null;
            }
            set
            {
                this.Streams = new[] { value };
            }
        }
        /// <summary>
        /// Image array streams
        /// </summary>
        public MemoryStream[] Streams { get; set; } = null;
        /// <summary>
        /// Image path
        /// </summary>
        public string Path
        {
            get
            {
                return this.paths != null && this.paths.Length == 1 ? this.paths[0] : null;
            }
            set
            {
                this.paths = new[] { value };
            }
        }
        /// <summary>
        /// Image array paths
        /// </summary>
        public string[] Paths
        {
            get
            {
                return this.paths != null && this.paths.Length > 1 ? this.paths : null;
            }
            set
            {
                this.paths = value;
            }
        }
        /// <summary>
        /// Gets whether the image content is an image array
        /// </summary>
        public bool IsArray
        {
            get
            {
                return
                    (this.paths != null && this.paths.Length > 1) ||
                    (this.Streams != null && this.Streams.Length > 1);
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
                if (this.paths != null || this.Streams != null)
                {
                    return this.paths != null ? this.paths.Length : this.Streams.Length;
                }
                else
                {
                    return 0;
                }
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
        public static ImageContent Array(string contentFolder, string[] textures)
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
        public static ImageContent Array(MemoryStream[] textures)
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
            if (!string.IsNullOrEmpty(this.Path))
            {
                return string.Format("Path: {0}; ", this.Path);
            }
            if (this.Paths != null && this.Paths.Length > 0)
            {
                return string.Format("Path array: {0}; ", this.Paths.Length);
            }
            else if (this.Stream != null)
            {
                return string.Format("Stream: {0} bytes; ", this.Stream.Length);
            }
            else if (this.Streams != null && this.Streams.Length > 0)
            {
                return string.Format("Stream array: {0}; ", this.Streams.Length);
            }
            else
            {
                return "Empty;";
            }
        }
    }
}
