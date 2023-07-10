using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine
{
    using Engine.Common;
    using Engine.Helpers;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Graphic textures management
    /// </summary>
    public sealed partial class Graphics
    {
        /// <summary>
        /// Creates a resource view from a texture description
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Texture description</param>
        /// <param name="tryMipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the new shader resource view</returns>
        private ShaderResourceView1 CreateResource(string name, TextureData description, bool tryMipAutogen, bool dynamic)
        {
            bool mipAutogen = false;

            if (tryMipAutogen && description.MipMaps == 1)
            {
                var fmtSupport = device.CheckFormatSupport(description.Format);
                mipAutogen = fmtSupport.HasFlag(FormatSupport.MipAutogen);
            }

            if (mipAutogen)
            {
                var texture = CreateTexture2D(description.Width, description.Height, description.Format, 1, mipAutogen, dynamic);
                texture.DebugName = name;
                using (texture)
                {
                    var desc = new ShaderResourceViewDescription1()
                    {
                        Format = texture.Description.Format,
                        Dimension = ShaderResourceViewDimension.Texture2D,
                        Texture2D = new ShaderResourceViewDescription1.Texture2DResource1()
                        {
                            MipLevels = -1,
                        },
                    };
                    var result = new ShaderResourceView1(device, texture, desc)
                    {
                        DebugName = name,
                    };

                    immediateContext.UpdateSubresource(description.GetDataBox(0, 0), texture, 0);

                    immediateContext.GenerateMips(result);

                    return result;
                }
            }
            else
            {
                var width = description.Width;
                var height = description.Height;
                var format = description.Format;
                var mipMaps = description.MipMaps;
                var arraySize = description.ArraySize;
                var data = description.GetDataBoxes();

                var texture = CreateTexture2D(width, height, format, mipMaps, arraySize, data, dynamic);
                texture.DebugName = name;
                using (texture)
                {
                    var desc = new ShaderResourceViewDescription1()
                    {
                        Format = format,
                        Dimension = ShaderResourceViewDimension.Texture2D,
                        Texture2D = new ShaderResourceViewDescription1.Texture2DResource1()
                        {
                            MipLevels = mipMaps,
                        },
                    };
                    return new ShaderResourceView1(device, texture, desc)
                    {
                        DebugName = name,
                    };
                }
            }
        }
        /// <summary>
        /// Creates a resource view from a texture description list
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="descriptions">Texture description list</param>
        /// <param name="tryMipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the new shader resource view</returns>
        private ShaderResourceView1 CreateResource(string name, IEnumerable<TextureData> descriptions, bool tryMipAutogen, bool dynamic)
        {
            var description = descriptions.First();
            int count = descriptions.Count();

            bool mipAutogen = false;

            if (tryMipAutogen && description.MipMaps == 1)
            {
                var fmtSupport = device.CheckFormatSupport(description.Format);
                mipAutogen = fmtSupport.HasFlag(FormatSupport.MipAutogen);
            }

            if (mipAutogen)
            {
                var textureArray = CreateTexture2D(description.Width, description.Height, description.Format, count, mipAutogen, dynamic);
                textureArray.DebugName = name;
                using (textureArray)
                {
                    var desc = new ShaderResourceViewDescription1()
                    {
                        Format = description.Format,
                        Dimension = ShaderResourceViewDimension.Texture2DArray,
                        Texture2DArray = new ShaderResourceViewDescription1.Texture2DArrayResource1()
                        {
                            ArraySize = count,
                            MipLevels = -1,
                        },
                    };
                    var result = new ShaderResourceView1(device, textureArray, desc)
                    {
                        DebugName = name,
                    };

                    int i = 0;
                    foreach (var currentDesc in descriptions)
                    {
                        var index = textureArray.CalculateSubResourceIndex(0, i++, out int mipSize);

                        immediateContext.UpdateSubresource(currentDesc.GetDataBox(0, 0), textureArray, index);
                    }

                    immediateContext.GenerateMips(result);

                    return result;
                }
            }
            else
            {
                var width = description.Width;
                var height = description.Height;
                var format = description.Format;
                var mipMaps = description.MipMaps;
                var arraySize = count;
                var data = new List<DataBox>();

                foreach (var currentDesc in descriptions)
                {
                    data.AddRange(currentDesc.GetDataBoxes());
                }

                var textureArray = CreateTexture2D(width, height, format, mipMaps, arraySize, data.ToArray(), dynamic);
                textureArray.DebugName = name;
                using (textureArray)
                {
                    var desc = new ShaderResourceViewDescription1()
                    {
                        Format = format,
                        Dimension = ShaderResourceViewDimension.Texture2DArray,
                        Texture2DArray = new ShaderResourceViewDescription1.Texture2DArrayResource1()
                        {
                            ArraySize = arraySize,
                            MipLevels = mipMaps,
                        },
                    };
                    return new ShaderResourceView1(device, textureArray, desc)
                    {
                        DebugName = name,
                    };
                }
            }
        }
        /// <summary>
        /// Creates a resource view from a texture description
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Texture description</param>
        /// <param name="tryMipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the new shader resource view</returns>
        private ShaderResourceView1 CreateResourceCubic(string name, TextureData description, bool tryMipAutogen, bool dynamic)
        {
            bool mipAutogen = false;

            if (tryMipAutogen && description.MipMaps == 1)
            {
                var fmtSupport = device.CheckFormatSupport(description.Format);
                mipAutogen = fmtSupport.HasFlag(FormatSupport.MipAutogen);
            }

            if (mipAutogen)
            {
                var texture = CreateTexture2DCube(description.Width, description.Height, description.Format, 1, mipAutogen, dynamic);
                texture.DebugName = name;
                using (texture)
                {
                    var desc = new ShaderResourceViewDescription1()
                    {
                        Format = texture.Description.Format,
                        Dimension = ShaderResourceViewDimension.TextureCube,
                        TextureCube = new ShaderResourceViewDescription.TextureCubeResource()
                        {
                            MipLevels = -1,
                        }
                    };
                    var result = new ShaderResourceView1(device, texture, desc)
                    {
                        DebugName = name,
                    };

                    immediateContext.UpdateSubresource(description.GetDataBox(0, 0), texture, 0);

                    immediateContext.GenerateMips(result);

                    return result;
                }
            }
            else
            {
                var width = description.Width;
                var height = description.Height;
                var format = description.Format;
                var mipMaps = description.MipMaps;
                var data = description.GetDataBoxes();

                var texture = CreateTexture2DCube(width, height, format, mipMaps, 1, data, dynamic);
                texture.DebugName = name;
                using (texture)
                {
                    var desc = new ShaderResourceViewDescription1()
                    {
                        Format = format,
                        Dimension = ShaderResourceViewDimension.TextureCube,
                        TextureCube = new ShaderResourceViewDescription.TextureCubeResource()
                        {
                            MipLevels = mipMaps,
                        }
                    };
                    return new ShaderResourceView1(device, texture, desc)
                    {
                        DebugName = name,
                    };
                }
            }
        }
        /// <summary>
        /// Creates a resource view from a texture description
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="descriptions">Texture descriptions</param>
        /// <param name="tryMipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the new shader resource view</returns>
        private ShaderResourceView1 CreateResourceCubic(string name, IEnumerable<TextureData> descriptions, bool tryMipAutogen, bool dynamic)
        {
            var description = descriptions.First();
            int count = descriptions.Count();

            bool mipAutogen = false;

            if (tryMipAutogen && description.MipMaps == 1)
            {
                var fmtSupport = device.CheckFormatSupport(description.Format);
                mipAutogen = fmtSupport.HasFlag(FormatSupport.MipAutogen);
            }

            if (mipAutogen)
            {
                var textureArray = CreateTexture2DCube(description.Width, description.Height, description.Format, count, mipAutogen, dynamic);
                textureArray.DebugName = name;
                using (textureArray)
                {
                    var desc = new ShaderResourceViewDescription1()
                    {
                        Format = description.Format,
                        Dimension = ShaderResourceViewDimension.TextureCubeArray,
                        TextureCubeArray = new ShaderResourceViewDescription.TextureCubeArrayResource()
                        {
                            CubeCount = count,
                            MipLevels = -1,
                        }
                    };
                    var result = new ShaderResourceView1(device, textureArray, desc)
                    {
                        DebugName = name,
                    };

                    int i = 0;
                    foreach (var currentDesc in descriptions)
                    {
                        var index = textureArray.CalculateSubResourceIndex(0, i++, out int mipSize);

                        immediateContext.UpdateSubresource(currentDesc.GetDataBox(0, 0), textureArray, index);
                    }

                    immediateContext.GenerateMips(result);

                    return result;
                }
            }
            else
            {
                var width = description.Width;
                var height = description.Height;
                var format = description.Format;
                var mipMaps = description.MipMaps;
                var arraySize = count;
                var data = new List<DataBox>();

                foreach (var currentDesc in descriptions)
                {
                    data.AddRange(currentDesc.GetDataBoxes());
                }

                var textureArray = CreateTexture2DCube(width, height, format, mipMaps, arraySize, data.ToArray(), dynamic);
                textureArray.DebugName = name;
                using (textureArray)
                {
                    var desc = new ShaderResourceViewDescription1()
                    {
                        Format = format,
                        Dimension = ShaderResourceViewDimension.TextureCube,
                        TextureCubeArray = new ShaderResourceViewDescription.TextureCubeArrayResource()
                        {
                            CubeCount = arraySize,
                            MipLevels = mipMaps,
                        },
                    };
                    return new ShaderResourceView1(device, textureArray, desc)
                    {
                        DebugName = name,
                    };
                }
            }
        }
        /// <summary>
        /// Creates a texture filled with specified values
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Name</param>
        /// <param name="size">Texture size</param>
        /// <param name="values">Texture values</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns created texture</returns>
        private ShaderResourceView1 CreateTexture1D<T>(string name, int size, IEnumerable<T> values, bool dynamic) where T : struct
        {
            try
            {
                Counters.Textures++;

                using (var str = DataStream.Create(values.ToArray(), false, false))
                {
                    using (var randTex = new Texture1D(
                        device,
                        new Texture1DDescription()
                        {
                            Format = Format.R32G32B32A32_Float,
                            Width = size,
                            ArraySize = 1,
                            MipLevels = 1,
                            Usage = dynamic ? ResourceUsage.Dynamic : ResourceUsage.Immutable,
                            BindFlags = BindFlags.ShaderResource,
                            CpuAccessFlags = dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.None,
                        },
                        str))
                    {
                        randTex.DebugName = name;

                        return new ShaderResourceView1(device, randTex)
                        {
                            DebugName = name,
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateTexture1D from value array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a texture filled with specified values
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Name</param>
        /// <param name="size">Texture size</param>
        /// <param name="values">Texture values</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns created texture</returns>
        private ShaderResourceView1 CreateTexture2D<T>(string name, int size, IEnumerable<T> values, bool dynamic) where T : struct
        {
            return CreateTexture2D(name, size, size, values, dynamic);
        }
        /// <summary>
        /// Creates a texture filled with specified values
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Name</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="values">Texture values</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns created texture</returns>
        private ShaderResourceView1 CreateTexture2D<T>(string name, int width, int height, IEnumerable<T> values, bool dynamic) where T : struct
        {
            try
            {
                Counters.Textures++;

                T[] tmp = new T[width * height];
                Array.Copy(values.ToArray(), tmp, values.Count());

                using (var str = DataStream.Create(tmp, false, false))
                {
                    var dBox = new DataBox(str.DataPointer, width * FormatHelper.SizeOfInBytes(Format.R32G32B32A32_Float), 0);

                    using (var texture = new Texture2D1(
                        device,
                        new Texture2DDescription1()
                        {
                            Format = Format.R32G32B32A32_Float,
                            Width = width,
                            Height = height,
                            ArraySize = 1,
                            MipLevels = 1,
                            SampleDescription = new SampleDescription(1, 0),
                            Usage = dynamic ? ResourceUsage.Dynamic : ResourceUsage.Immutable,
                            BindFlags = BindFlags.ShaderResource,
                            CpuAccessFlags = dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.None,
                        },
                        new[] { dBox }))
                    {
                        texture.DebugName = name;

                        return new ShaderResourceView1(device, texture)
                        {
                            DebugName = name,
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateTexture2D from value array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates an empty Texture2D
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="format">Format</param>
        /// <param name="arraySize">Size</param>
        /// <param name="generateMips">Generate mips for the texture</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the Texture2D</returns>
        private Texture2D1 CreateTexture2D(int width, int height, Format format, int arraySize, bool generateMips, bool dynamic)
        {
            var description = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = arraySize,
                BindFlags = (generateMips) ? BindFlags.ShaderResource | BindFlags.RenderTarget : BindFlags.ShaderResource,
                Usage = dynamic ? ResourceUsage.Dynamic : ResourceUsage.Default,
                CpuAccessFlags = dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
                Format = format,
                MipLevels = (generateMips) ? 0 : 1,
                OptionFlags = (generateMips) ? ResourceOptionFlags.GenerateMipMaps : ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };

            return new Texture2D1(device, description);
        }
        /// <summary>
        /// Creates a Texture2D
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="format">Format</param>
        /// <param name="mipMaps">Mipmap count</param>
        /// <param name="arraySize">Array size</param>
        /// <param name="data">Initial data</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the Texture2D</returns>
        private Texture2D1 CreateTexture2D(int width, int height, Format format, int mipMaps, int arraySize, DataBox[] data, bool dynamic)
        {
            var description = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = arraySize,
                BindFlags = BindFlags.ShaderResource,
                Usage = dynamic ? ResourceUsage.Dynamic : ResourceUsage.Default,
                CpuAccessFlags = dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
                Format = format,
                MipLevels = mipMaps,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };

            return new Texture2D1(device, description, data);
        }
        /// <summary>
        /// Creates a Texture2DCube
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="format">Format</param>
        /// <param name="arraySize">Array size</param>
        /// <param name="generateMips">Generate mips for the texture</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the Texture2DCube</returns>
        private Texture2D1 CreateTexture2DCube(int width, int height, Format format, int arraySize, bool generateMips, bool dynamic)
        {
            var description = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = arraySize * 6,
                BindFlags = (generateMips) ? BindFlags.ShaderResource | BindFlags.RenderTarget : BindFlags.ShaderResource,
                Usage = dynamic ? ResourceUsage.Dynamic : ResourceUsage.Default,
                CpuAccessFlags = dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
                Format = format,
                MipLevels = (generateMips) ? 0 : 1,
                OptionFlags = (generateMips) ? ResourceOptionFlags.TextureCube | ResourceOptionFlags.GenerateMipMaps : ResourceOptionFlags.TextureCube,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };

            return new Texture2D1(device, description);
        }
        /// <summary>
        /// Creates a Texture2DCube
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="format">Format</param>
        /// <param name="mipMaps">Mipmap count</param>
        /// <param name="arraySize">Array size</param>
        /// <param name="data">Initial data</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the Texture2DCube</returns>
        private Texture2D1 CreateTexture2DCube(int width, int height, Format format, int mipMaps, int arraySize, DataBox[] data, bool dynamic)
        {
            var description = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = arraySize * 6,
                BindFlags = BindFlags.ShaderResource,
                Usage = dynamic ? ResourceUsage.Dynamic : ResourceUsage.Default,
                CpuAccessFlags = dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
                Format = format,
                MipLevels = mipMaps,
                OptionFlags = ResourceOptionFlags.TextureCube,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };

            return new Texture2D1(device, description, data);
        }

        /// <summary>
        /// Loads a texture from file in the graphics device
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the resource view</returns>
        public EngineShaderResourceView LoadTexture(string name, string filename, bool mipAutogen, bool dynamic)
        {
            try
            {
                Counters.Textures++;

                using (var resource = TextureData.ReadTexture(filename))
                {
                    return new EngineShaderResourceView(name, CreateResource(name, resource, mipAutogen, dynamic));
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from file Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture from file in the graphics device
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="stream">Stream</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the resource view</returns>
        public EngineShaderResourceView LoadTexture(string name, MemoryStream stream, bool mipAutogen, bool dynamic)
        {
            try
            {
                Counters.Textures++;

                using (var resource = TextureData.ReadTexture(stream))
                {
                    return new EngineShaderResourceView(name, CreateResource(name, resource, mipAutogen, dynamic));
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from stream Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture from file in the graphics device
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the resource view</returns>
        public EngineShaderResourceView LoadTexture(string name, string filename, Rectangle rectangle, bool mipAutogen, bool dynamic)
        {
            try
            {
                Counters.Textures++;

                using (var resource = TextureData.ReadTexture(filename, rectangle))
                {
                    return new EngineShaderResourceView(name, CreateResource(name, resource, mipAutogen, dynamic));
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from file Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture from file in the graphics device
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="stream">Stream</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the resource view</returns>
        public EngineShaderResourceView LoadTexture(string name, MemoryStream stream, Rectangle rectangle, bool mipAutogen, bool dynamic)
        {
            try
            {
                Counters.Textures++;

                using (var resource = TextureData.ReadTexture(stream, rectangle))
                {
                    return new EngineShaderResourceView(name, CreateResource(name, resource, mipAutogen, dynamic));
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from stream Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture array from a file in the graphics device
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path file</param>
        /// <param name="rectangles">Crop rectangle list</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the resource view</returns>
        public EngineShaderResourceView LoadTextureArray(string name, string filename, IEnumerable<Rectangle> rectangles, bool mipAutogen, bool dynamic)
        {
            try
            {
                var textureList = TextureData.ReadTextureArray(filename, rectangles);

                return new EngineShaderResourceView(name, CreateResource(name, textureList, mipAutogen, dynamic));
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from file array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture array from a file in the graphics device
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="stream">Stream</param>
        /// <param name="rectangles">Crop rectangle list</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the resource view</returns>
        public EngineShaderResourceView LoadTextureArray(string name, MemoryStream stream, IEnumerable<Rectangle> rectangles, bool mipAutogen, bool dynamic)
        {
            try
            {
                var textureList = TextureData.ReadTextureArray(stream, rectangles);

                return new EngineShaderResourceView(name, CreateResource(name, textureList, mipAutogen, dynamic));
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from stream array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture array from a file collection in the graphics device
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filenames">Path file collection</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the resource view</returns>
        public EngineShaderResourceView LoadTextureArray(string name, IEnumerable<string> filenames, bool mipAutogen, bool dynamic)
        {
            try
            {
                var textureList = TextureData.ReadTextureArray(filenames);

                return new EngineShaderResourceView(name, CreateResource(name, textureList, mipAutogen, dynamic));
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from file array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture array from a file collection in the graphics device
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="streams">Stream collection</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the resource view</returns>
        public EngineShaderResourceView LoadTextureArray(string name, IEnumerable<MemoryStream> streams, bool mipAutogen, bool dynamic)
        {
            try
            {
                var textureList = TextureData.ReadTextureArray(streams);

                return new EngineShaderResourceView(name, CreateResource(name, textureList, mipAutogen, dynamic));
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from stream array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture array from a file collection in the graphics device
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filenames">Path file collection</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the resource view</returns>
        public EngineShaderResourceView LoadTextureArray(string name, IEnumerable<string> filenames, Rectangle rectangle, bool mipAutogen, bool dynamic)
        {
            try
            {
                var textureList = TextureData.ReadTextureArray(filenames, rectangle);

                return new EngineShaderResourceView(name, CreateResource(name, textureList, mipAutogen, dynamic));
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from file array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture array from a file collection in the graphics device
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="streams">Stream collection</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the resource view</returns>
        public EngineShaderResourceView LoadTextureArray(string name, IEnumerable<MemoryStream> streams, Rectangle rectangle, bool mipAutogen, bool dynamic)
        {
            try
            {
                var textureList = TextureData.ReadTextureArray(streams, rectangle);

                return new EngineShaderResourceView(name, CreateResource(name, textureList, mipAutogen, dynamic));
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from stream array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture from file in the graphics device
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="faces">Cube faces</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the resource view</returns>
        public EngineShaderResourceView LoadTextureCubic(string name, string filename, IEnumerable<Rectangle> faces, bool mipAutogen, bool dynamic)
        {
            try
            {
                Counters.Textures++;

                if (faces?.Count() == 6)
                {
                    var resources = TextureData.ReadTextureCubic(filename, faces);

                    return new EngineShaderResourceView(name, CreateResourceCubic(name, resources, mipAutogen, dynamic));
                }
                else
                {
                    var resource = TextureData.ReadTexture(filename, Rectangle.Empty);

                    return new EngineShaderResourceView(name, CreateResourceCubic(name, resource, mipAutogen, dynamic));
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from file Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture from file in the graphics device
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="stream">Stream</param>
        /// <param name="faces">Cube faces</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the resource view</returns>
        public EngineShaderResourceView LoadTextureCubic(string name, MemoryStream stream, IEnumerable<Rectangle> faces, bool mipAutogen, bool dynamic)
        {
            try
            {
                Counters.Textures++;

                if (faces?.Count() == 6)
                {
                    var resources = TextureData.ReadTextureCubic(stream, faces);

                    return new EngineShaderResourceView(name, CreateResourceCubic(name, resources, mipAutogen, dynamic));
                }
                else
                {
                    var resource = TextureData.ReadTexture(stream, Rectangle.Empty);

                    return new EngineShaderResourceView(name, CreateResourceCubic(name, resource, mipAutogen, dynamic));
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from stream Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a random 1D texture
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="size">Texture size</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="seed">Random seed</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns created texture</returns>
        public EngineShaderResourceView CreateRandomTexture(string name, int size, float min, float max, int seed = 0, bool dynamic = true)
        {
            try
            {
                Counters.Textures++;

                Random rnd = new Random(seed);

                var randomValues = new List<Vector4>();
                for (int i = 0; i < size; i++)
                {
                    randomValues.Add(rnd.NextVector4(new Vector4(min), new Vector4(max)));
                }

                return new EngineShaderResourceView(name, CreateTexture1D(name, size, randomValues, dynamic));
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateRandomTexture Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a value array texture
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="name">Name</param>
        /// <param name="size">Size</param>
        /// <param name="values">Values</param>
        /// <param name="dynamic">Dynamic resource</param>
        /// <returns>Returns created texture</returns>
        public EngineShaderResourceView CreateValueArrayTexture<T>(string name, int size, IEnumerable<T> values, bool dynamic) where T : struct
        {
            try
            {
                Counters.Textures++;

                return new EngineShaderResourceView(name, CreateTexture2D(name, size, values, dynamic));
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateValueArrayTexture Error. See inner exception for details", ex);
            }
        }

        /// <summary>
        /// Updates a texture
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="texture">Texture to update</param>
        /// <param name="data">Data to write</param>
        public void UpdateTexture1D<T>(EngineShaderResourceView texture, IEnumerable<T> data) where T : struct
        {
            if (data?.Any() == true)
            {
                using (var resource = texture.GetResource().Resource.QueryInterface<Texture1D>())
                {
                    immediateContext.MapSubresource(resource, 0, MapMode.WriteDiscard, MapFlags.None, out DataStream stream);
                    using (stream)
                    {
                        stream.Position = 0;
                        stream.WriteRange(data.ToArray());
                    }
                    immediateContext.UnmapSubresource(resource, 0);
                }

                Counters.BufferWrites++;
            }
        }
        /// <summary>
        /// Updates a texture
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="texture">Texture to update</param>
        /// <param name="data">Data to write</param>
        public void UpdateTexture2D<T>(EngineShaderResourceView texture, IEnumerable<T> data) where T : struct
        {
            if (data?.Any() == true)
            {
                using (var resource = texture.GetResource().Resource.QueryInterface<Texture2D1>())
                {
                    immediateContext.MapSubresource(resource, 0, MapMode.WriteDiscard, MapFlags.None, out DataStream stream);
                    using (stream)
                    {
                        stream.Position = 0;
                        stream.WriteRange(data.ToArray());
                    }
                    immediateContext.UnmapSubresource(resource, 0);
                }

                Counters.BufferWrites++;
            }
        }
    }
}
