using System.IO;
using Device = SharpDX.Direct3D11.Device;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Content
{
    using Engine.Helpers;

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
        /// Stream list
        /// </summary>
        private MemoryStream[] streams = null;

        /// <summary>
        /// Image data in stream
        /// </summary>
        public MemoryStream Stream
        {
            get
            {
                return this.streams != null && this.streams.Length == 1 ? this.streams[0] : null;
            }
            set
            {
                this.streams = new[] { value };
            }
        }
        /// <summary>
        /// Image array streams
        /// </summary>
        public MemoryStream[] Streams
        {
            get
            {
                return this.streams != null && this.streams.Length > 1 ? this.streams : null;
            }
            set
            {
                this.streams = value;
            }
        }
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
                    (this.streams != null && this.streams.Length > 1);
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

        /// <summary>
        /// Creates a unique texture image
        /// </summary>
        /// <param name="texture">Path to texture</param>
        /// <returns>Returns content</returns>
        public static ImageContent Texture(string texture)
        {
            return new ImageContent()
            {
                Path = texture,
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
        /// <param name="textures">Paths to textures</param>
        /// <returns>Returns content</returns>
        public static ImageContent Array(string[] textures)
        {
            return new ImageContent()
            {
                Paths = textures,
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
        /// <param name="texture">Path to texture</param>
        /// <param name="faceSize">Face size</param>
        /// <returns>Returns content</returns>
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
        /// Creates a cubic texture image
        /// </summary>
        /// <param name="texture">Texture stream</param>
        /// <param name="faceSize">Face size</param>
        /// <returns>Returns content</returns>
        public static ImageContent Cubic(MemoryStream texture, int faceSize)
        {
            return new ImageContent()
            {
                Stream = texture,
                IsCubic = true,
                CubicFaceSize = faceSize,
            };
        }

        /// <summary>
        /// Generate the resource view
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <returns>Returns the created resource view</returns>
        public ShaderResourceView CreateResource(Device device)
        {
            ShaderResourceView view = null;

            if (this.Stream != null)
            {
                byte[] buffer = this.Stream.GetBuffer();

                view = device.LoadTexture(buffer);
            }
            else
            {
                if (this.IsArray)
                {
                    if (this.Paths != null && this.Paths.Length > 0)
                    {
                        view = device.LoadTextureArray(this.Paths);
                    }
                    else if (this.Streams != null && this.Streams.Length > 0)
                    {
                        view = device.LoadTextureArray(this.Streams);
                    }
                }
                else if (this.IsCubic)
                {
                    int faceSize = this.CubicFaceSize;

                    if (this.Path != null)
                    {
                        view = device.LoadTextureCube(this.Path, faceSize);
                    }
                    else if (this.Stream != null)
                    {
                        view = device.LoadTextureCube(this.Stream, faceSize);
                    }
                }
                else
                {
                    if (this.Path != null)
                    {
                        view = device.LoadTexture(this.Path);
                    }
                    else if (this.Stream != null)
                    {
                        view = device.LoadTexture(this.Stream);
                    }
                }
            }

            return view;
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
