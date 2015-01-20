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
        public MemoryStream Stream { get; set; }
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
                return this.paths != null && this.paths.Length > 1;
            }
        }
        /// <summary>
        /// Gets or sets whether the image content is cubic
        /// </summary>
        public bool IsCubic { get; set; }
        /// <summary>
        /// Cubic face size
        /// </summary>
        public int CubicFaceSize { get; set; }

        public static ImageContent Texture(string texture)
        {
            return new ImageContent()
            {
                Path = texture,
            };
        }
        public static ImageContent Array(string[] textures)
        {
            return new ImageContent()
            {
                Paths = textures,
            };
        }
        public static ImageContent Cubic(string texture, int faceSize)
        {
            return new ImageContent()
            {
                Path = texture,
                IsCubic = true,
                CubicFaceSize = faceSize,
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
            else if (this.Stream != null)
            {
                return string.Format("Stream: {0} bytes; ", this.Stream.Length);
            }
            else
            {
                return "Empty;";
            }
        }
    }
}
