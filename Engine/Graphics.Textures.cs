using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Engine
{
    using Engine.Common;
    using Engine.TextureReader;

    /// <summary>
    /// Graphic textures management
    /// </summary>
    public sealed partial class Graphics
    {
        /// <summary>
        /// Checks mipmap auto-generation in the device
        /// </summary>
        /// <param name="format">Format to test</param>
        /// <returns>Returns true if the device support mipmap auto-generation for the specified format</returns>
        private bool DeviceSupportsMipMapGeneration(Format format)
        {
            var fmtSupport = device.CheckFormatSupport(format);

            return fmtSupport.HasFlag(FormatSupport.MipAutogen);
        }

        /// <summary>
        /// Creates a resource view from a texture description
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Texture description</param>
        /// <param name="tryMipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the new shader resource view</returns>
        private ID3D11ShaderResourceView1 CreateResourceView(string name, TextureData description, bool tryMipAutogen, bool dynamic)
        {
            bool mipAutogen = false;
            if (tryMipAutogen && description.MipMaps == 1)
            {
                mipAutogen = DeviceSupportsMipMapGeneration(description.Format);
            }

            using var texture = CreateTexture2D(name, description, mipAutogen, dynamic);
            var desc = new ShaderResourceViewDescription1()
            {
                Format = texture.Description.Format,
                ViewDimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new Texture2DShaderResourceView1()
                {
                    MipLevels = mipAutogen ? uint.MaxValue : texture.Description1.MipLevels,
                },
            };
            var result = device.CreateShaderResourceView1(texture, desc);
            result.DebugName = name;

            if (mipAutogen)
            {
                ID3D11DeviceContext3 ic = immediateContext;
                ic.GenerateMips(result);
            }

            return result;
        }
        /// <summary>
        /// Creates a resource view from a texture description list
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="descriptions">Texture description list</param>
        /// <param name="tryMipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the new shader resource view</returns>
        private ID3D11ShaderResourceView1 CreateResourceView(string name, IEnumerable<TextureData> descriptions, bool tryMipAutogen, bool dynamic)
        {
            var description = descriptions.First();

            bool mipAutogen = false;
            if (tryMipAutogen && description.MipMaps == 1)
            {
                mipAutogen = DeviceSupportsMipMapGeneration(description.Format);
            }

            using var textureArray = CreateTexture2D(name, descriptions, mipAutogen, dynamic);
            var desc = new ShaderResourceViewDescription1()
            {
                Format = textureArray.Description1.Format,
                ViewDimension = ShaderResourceViewDimension.Texture2DArray,
                Texture2DArray = new Texture2DArrayShaderResourceView1()
                {
                    ArraySize = textureArray.Description.ArraySize,
                    MipLevels = mipAutogen ? uint.MaxValue : textureArray.Description1.MipLevels,
                },
            };
            var result = device.CreateShaderResourceView1(textureArray, desc);
            result.DebugName = name;

            if (mipAutogen)
            {
                ID3D11DeviceContext3 ic = immediateContext;
                ic.GenerateMips(result);
            }

            return result;
        }
        /// <summary>
        /// Creates a resource view from a texture description
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Texture description</param>
        /// <param name="tryMipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the new shader resource view</returns>
        private ID3D11ShaderResourceView1 CreateCubicResourceView(string name, TextureData description, bool tryMipAutogen, bool dynamic)
        {
            bool mipAutogen = false;
            if (tryMipAutogen && description.MipMaps == 1)
            {
                mipAutogen = DeviceSupportsMipMapGeneration(description.Format);
            }

            using var texture = CreateTexture2DCube(name, description, mipAutogen, dynamic);
            var desc = new ShaderResourceViewDescription1()
            {
                Format = texture.Description.Format,
                ViewDimension = ShaderResourceViewDimension.TextureCube,
                TextureCube = new TextureCubeShaderResourceView()
                {
                    MipLevels = mipAutogen ? uint.MaxValue : texture.Description1.MipLevels,
                }
            };
            var result = device.CreateShaderResourceView1(texture, desc);
            result.DebugName = name;

            if (mipAutogen)
            {
                ID3D11DeviceContext3 ic = immediateContext;
                ic.GenerateMips(result);
            }

            return result;
        }
        /// <summary>
        /// Creates a resource view from a texture description
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="descriptions">Texture descriptions</param>
        /// <param name="tryMipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the new shader resource view</returns>
        private ID3D11ShaderResourceView1 CreateCubicResourceView(string name, IEnumerable<TextureData> descriptions, bool tryMipAutogen, bool dynamic)
        {
            var description = descriptions.First();

            bool mipAutogen = false;
            if (tryMipAutogen && description.MipMaps == 1)
            {
                mipAutogen = DeviceSupportsMipMapGeneration(description.Format);
            }

            using var textureArray = CreateTexture2DCube(name, descriptions, mipAutogen, dynamic);
            var desc = new ShaderResourceViewDescription1()
            {
                Format = textureArray.Description1.Format,
                ViewDimension = ShaderResourceViewDimension.TextureCubeArray,
                TextureCubeArray = new TextureCubeArrayShaderResourceView()
                {
                    NumCubes = textureArray.Description1.ArraySize / 6,
                    MipLevels = mipAutogen ? uint.MaxValue : textureArray.Description1.MipLevels,
                }
            };
            var result = device.CreateShaderResourceView1(textureArray, desc);
            result.DebugName = name;

            if (mipAutogen)
            {
                ID3D11DeviceContext3 ic = immediateContext;
                ic.GenerateMips(result);
            }

            return result;
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
        private ID3D11ShaderResourceView1 Create1DTextureResourceView<T>(string name, uint size, IEnumerable<T> values, bool dynamic) where T : unmanaged
        {
            using var texture = CreateTexture1D(name, size, values, dynamic);
            var srv = device.CreateShaderResourceView1(texture);
            srv.DebugName = name;
            return srv;
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
        private ID3D11ShaderResourceView1 Create2DTextureResourceView<T>(string name, uint size, IEnumerable<T> values, bool dynamic) where T : unmanaged
        {
            return Create2DTextureResourceView(name, size, size, values, dynamic);
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
        private ID3D11ShaderResourceView1 Create2DTextureResourceView<T>(string name, uint width, uint height, IEnumerable<T> values, bool dynamic) where T : unmanaged
        {
            using var texture = CreateTexture2D(name, width, height, values, dynamic);
            var srv = device.CreateShaderResourceView1(texture);
            srv.DebugName = name;
            return srv;
        }

        /// <summary>
        /// Creates a one dimension texture with the specified initial data
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Name</param>
        /// <param name="size">Texture size</param>
        /// <param name="values">Initial values</param>
        /// <param name="dynamic">Dynamic</param>
        private ID3D11Texture1D CreateTexture1D<T>(string name, uint size, IEnumerable<T> values, bool dynamic) where T : unmanaged
        {
            FrameCounters.Textures++;

            var desc = new Texture1DDescription()
            {
                Format = Format.R32G32B32A32_Float,
                Width = size,
                ArraySize = 1,
                MipLevels = 1,
                Usage = dynamic ? ResourceUsage.Dynamic : ResourceUsage.Immutable,
                BindFlags = BindFlags.ShaderResource,
                CPUAccessFlags = dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None,
            };

            var texture = CreateWithSubresource(
                values,
                desc.Width, 1, desc.Format,
                data => device.CreateTexture1D(desc, data));

            texture.DebugName = name;

            return texture;
        }
        /// <summary>
        /// Creates a two dimension texture with the specified initial data
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Name</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="values">Initial values</param>
        /// <param name="dynamic">Dynamic</param>
        private ID3D11Texture2D1 CreateTexture2D<T>(string name, uint width, uint height, IEnumerable<T> values, bool dynamic) where T : unmanaged
        {
            FrameCounters.Textures++;

            var desc = new Texture2DDescription1()
            {
                Format = Format.R32G32B32A32_Float,
                Width = width,
                Height = height,
                ArraySize = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = dynamic ? ResourceUsage.Dynamic : ResourceUsage.Immutable,
                BindFlags = BindFlags.ShaderResource,
                CPUAccessFlags = dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None,
            };

            var texture = CreateWithSubresource(
                values,
                desc.Width, desc.Height, desc.Format,
                data => device.CreateTexture2D1(desc, [data]));

            texture.DebugName = name;

            return texture;
        }
        /// <summary>
        /// Creates an empty Texture2D
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Texture data</param>
        /// <param name="generateMips">Generate mips for the texture</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the Texture2D</returns>
        private ID3D11Texture2D1 CreateTexture2D(string name, TextureData description, bool generateMips, bool dynamic)
        {
            FrameCounters.Textures++;

            var width = description.Width;
            var height = description.Height;
            var format = description.Format;
            var mipMaps = description.MipMaps;
            uint arraySize = 1;

            var desc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = arraySize,
                BindFlags = generateMips ? BindFlags.ShaderResource | BindFlags.RenderTarget : BindFlags.ShaderResource,
                Usage = dynamic ? ResourceUsage.Dynamic : ResourceUsage.Default,
                CPUAccessFlags = dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
                Format = format,
                MipLevels = generateMips ? 0 : mipMaps,
                MiscFlags = generateMips ? ResourceOptionFlags.GenerateMips : ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };

            if (generateMips)
            {
                var texture = device.CreateTexture2D1(desc);
                texture.DebugName = name;

                var index = texture.CalculateSubResourceIndex(0, 0, out _);

                ID3D11DeviceContext3 ic = immediateContext;
                ic.UpdateSubresource(description.GetData(0, 0), texture, index);

                return texture;
            }
            else
            {
                var texture = device.CreateTexture2D1(desc, description.GetData());
                texture.DebugName = name;

                return texture;
            }
        }
        /// <summary>
        /// Creates an empty Texture2D
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="descriptions">Texture data list</param>
        /// <param name="generateMips">Generate mips for the texture</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the Texture2D</returns>
        private ID3D11Texture2D1 CreateTexture2D(string name, IEnumerable<TextureData> descriptions, bool generateMips, bool dynamic)
        {
            FrameCounters.Textures++;

            var description = descriptions.First();

            var width = description.Width;
            var height = description.Height;
            var format = description.Format;
            var mipMaps = description.MipMaps;
            uint arraySize = (uint)descriptions.Count();

            var desc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = arraySize,
                BindFlags = generateMips ? BindFlags.ShaderResource | BindFlags.RenderTarget : BindFlags.ShaderResource,
                Usage = dynamic ? ResourceUsage.Dynamic : ResourceUsage.Default,
                CPUAccessFlags = dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
                Format = format,
                MipLevels = generateMips ? 0 : mipMaps,
                MiscFlags = generateMips ? ResourceOptionFlags.GenerateMips : ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };

            if (generateMips)
            {
                var textureArray = device.CreateTexture2D1(desc);

                ID3D11DeviceContext3 ic = immediateContext;

                uint i = 0;
                foreach (var currentDesc in descriptions)
                {
                    var index = textureArray.CalculateSubResourceIndex(0, i++, out _);

                    ic.UpdateSubresource(currentDesc.GetData(0, 0), textureArray, index);
                }

                return textureArray;
            }
            else
            {
                var data = descriptions.SelectMany(d => d.GetData()).ToArray();

                var texture = device.CreateTexture2D1(desc, data);
                texture.DebugName = name;

                return texture;
            }
        }
        /// <summary>
        /// Creates a Texture2DCube
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Texture data</param>
        /// <param name="generateMips">Generate mips for the texture</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the Texture2DCube</returns>
        private ID3D11Texture2D1 CreateTexture2DCube(string name, TextureData description, bool generateMips, bool dynamic)
        {
            FrameCounters.Textures++;

            var width = description.Width;
            var height = description.Height;
            var format = description.Format;
            var mipMaps = description.MipMaps;

            var desc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = 6,
                BindFlags = generateMips ? BindFlags.ShaderResource | BindFlags.RenderTarget : BindFlags.ShaderResource,
                Usage = dynamic ? ResourceUsage.Dynamic : ResourceUsage.Default,
                CPUAccessFlags = dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
                Format = format,
                MipLevels = generateMips ? 0 : mipMaps,
                MiscFlags = generateMips ? ResourceOptionFlags.TextureCube | ResourceOptionFlags.GenerateMips : ResourceOptionFlags.TextureCube,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };

            if (generateMips)
            {
                var texture = device.CreateTexture2D1(desc);
                texture.DebugName = name;

                var index = texture.CalculateSubResourceIndex(0, 0, out _);

                ID3D11DeviceContext3 ic = immediateContext;
                ic.UpdateSubresource(description.GetData(0, 0), texture, index);

                return texture;
            }
            else
            {
                var texture = device.CreateTexture2D1(desc, description.GetData());
                texture.DebugName = name;

                return texture;
            }
        }
        /// <summary>
        /// Creates a Texture2DCube
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="descriptions">Texture data list</param>
        /// <param name="generateMips">Generate mips for the texture</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns the Texture2DCube</returns>
        private ID3D11Texture2D1 CreateTexture2DCube(string name, IEnumerable<TextureData> descriptions, bool generateMips, bool dynamic)
        {
            FrameCounters.Textures++;

            var description = descriptions.First();

            var width = description.Width;
            var height = description.Height;
            var format = description.Format;
            var mipMaps = description.MipMaps;
            uint arraySize = (uint)descriptions.Count();

            var desc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = arraySize * 6,
                BindFlags = generateMips ? BindFlags.ShaderResource | BindFlags.RenderTarget : BindFlags.ShaderResource,
                Usage = dynamic ? ResourceUsage.Dynamic : ResourceUsage.Default,
                CPUAccessFlags = dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
                Format = format,
                MipLevels = generateMips ? 0 : mipMaps,
                MiscFlags = generateMips ? ResourceOptionFlags.TextureCube | ResourceOptionFlags.GenerateMips : ResourceOptionFlags.TextureCube,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };

            if (generateMips)
            {
                var textureArray = device.CreateTexture2D1(desc);
                textureArray.DebugName = name;

                ID3D11DeviceContext3 ic = immediateContext;

                uint i = 0;
                foreach (var currentDesc in descriptions)
                {
                    var index = textureArray.CalculateSubResourceIndex(0, i++, out _);

                    ic.UpdateSubresource(currentDesc.GetData(0, 0), textureArray, index);
                }

                return textureArray;
            }
            else
            {
                var data = descriptions.SelectMany(d => d.GetData()).ToArray();

                var texture = device.CreateTexture2D1(desc, data);
                texture.DebugName = name;

                return texture;
            }
        }
        /// <summary>
        /// Creates the data structure for text initialization
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        /// <param name="values">Data values</param>
        /// <param name="width">Texture width</param>
        /// <param name="height">Texture height</param>
        /// <param name="format">Format</param>
        /// <returns>Returns the data structure</returns>
        private static unsafe TResource CreateWithSubresource<T, TResource>(IEnumerable<T> values, uint width, uint height, Format format, Func<SubresourceData, TResource> createResourceFunc)
            where T : unmanaged
        {
            T[] array = new T[width * height];
            Array.Copy(values.ToArray(), array, values.Count());

            // 2. Cálculos de Pitch
            uint bitsPerPixel = format.GetBitsPerPixel();
            uint bytesPerPixel = (bitsPerPixel + 7) / 8;
            uint rowPitch = width * bytesPerPixel;
            uint slicePitch = rowPitch * height;

            // 3. Fijación automática mediante el compilador
            fixed (T* ptr = array)
            {
                return createResourceFunc(new((IntPtr)ptr, rowPitch, slicePitch));
            }
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
            using var resource = TextureData.ReadTexture(filename);

            return new EngineShaderResourceView(name, CreateResourceView(name, resource, mipAutogen, dynamic));
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
            using var resource = TextureData.ReadTexture(stream);

            return new EngineShaderResourceView(name, CreateResourceView(name, resource, mipAutogen, dynamic));
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
        public EngineShaderResourceView LoadTexture(string name, string filename, RectI rectangle, bool mipAutogen, bool dynamic)
        {
            using var resource = TextureData.ReadTexture(filename, rectangle);

            return new EngineShaderResourceView(name, CreateResourceView(name, resource, mipAutogen, dynamic));
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
        public EngineShaderResourceView LoadTexture(string name, MemoryStream stream, RectI rectangle, bool mipAutogen, bool dynamic)
        {
            using var resource = TextureData.ReadTexture(stream, rectangle);

            return new EngineShaderResourceView(name, CreateResourceView(name, resource, mipAutogen, dynamic));
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
        public EngineShaderResourceView LoadTextureArray(string name, string filename, IEnumerable<RectI> rectangles, bool mipAutogen, bool dynamic)
        {
            var textureList = TextureData.ReadTextureArray(filename, rectangles);

            var srv = new EngineShaderResourceView(name, CreateResourceView(name, textureList, mipAutogen, dynamic));

            textureList.ToList().ForEach(t => t.Dispose());

            return srv;
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
        public EngineShaderResourceView LoadTextureArray(string name, MemoryStream stream, IEnumerable<RectI> rectangles, bool mipAutogen, bool dynamic)
        {
            var textureList = TextureData.ReadTextureArray(stream, rectangles);

            var srv = new EngineShaderResourceView(name, CreateResourceView(name, textureList, mipAutogen, dynamic));

            textureList.ToList().ForEach(t => t.Dispose());

            return srv;
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
            var textureList = TextureData.ReadTextureArray(filenames);

            var srv = new EngineShaderResourceView(name, CreateResourceView(name, textureList, mipAutogen, dynamic));

            textureList.ToList().ForEach(t => t.Dispose());

            return srv;
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
            var textureList = TextureData.ReadTextureArray(streams);

            var srv = new EngineShaderResourceView(name, CreateResourceView(name, textureList, mipAutogen, dynamic));

            textureList.ToList().ForEach(t => t.Dispose());

            return srv;
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
        public EngineShaderResourceView LoadTextureArray(string name, IEnumerable<string> filenames, RectI rectangle, bool mipAutogen, bool dynamic)
        {
            var textureList = TextureData.ReadTextureArray(filenames, rectangle);

            var srv = new EngineShaderResourceView(name, CreateResourceView(name, textureList, mipAutogen, dynamic));

            textureList.ToList().ForEach(t => t.Dispose());

            return srv;
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
        public EngineShaderResourceView LoadTextureArray(string name, IEnumerable<MemoryStream> streams, RectI rectangle, bool mipAutogen, bool dynamic)
        {
            var textureList = TextureData.ReadTextureArray(streams, rectangle);

            var srv = new EngineShaderResourceView(name, CreateResourceView(name, textureList, mipAutogen, dynamic));

            textureList.ToList().ForEach(t => t.Dispose());

            return srv;
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
        public EngineShaderResourceView LoadTextureCubic(string name, string filename, IEnumerable<RectI> faces, bool mipAutogen, bool dynamic)
        {
            if (faces?.Count() == 6)
            {
                var resources = TextureData.ReadTextureCubic(filename, faces);

                var srv = new EngineShaderResourceView(name, CreateCubicResourceView(name, resources, mipAutogen, dynamic));

                resources.ToList().ForEach(t => t.Dispose());

                return srv;
            }
            else
            {
                using var resource = TextureData.ReadTexture(filename, RectI.Empty);

                return new EngineShaderResourceView(name, CreateCubicResourceView(name, resource, mipAutogen, dynamic));
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
        public EngineShaderResourceView LoadTextureCubic(string name, MemoryStream stream, IEnumerable<RectI> faces, bool mipAutogen, bool dynamic)
        {
            if (faces?.Count() == 6)
            {
                var resources = TextureData.ReadTextureCubic(stream, faces);

                var srv = new EngineShaderResourceView(name, CreateCubicResourceView(name, resources, mipAutogen, dynamic));

                resources.ToList().ForEach(t => t.Dispose());

                return srv;
            }
            else
            {
                using var resource = TextureData.ReadTexture(stream, RectI.Empty);

                return new EngineShaderResourceView(name, CreateCubicResourceView(name, resource, mipAutogen, dynamic));
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
        public EngineShaderResourceView CreateRandomTexture(string name, uint size, float min, float max, int seed = 0, bool dynamic = true)
        {
            var rnd = Helper.NewGenerator(seed);

            var randomValues = new List<Vector4>();
            for (int i = 0; i < size; i++)
            {
                randomValues.Add(rnd.NextVector4(new Vector4(min), new Vector4(max)));
            }

            return new EngineShaderResourceView(name, Create1DTextureResourceView(name, size, randomValues, dynamic));
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
        public EngineShaderResourceView CreateValueArrayTexture<T>(string name, uint size, IEnumerable<T> values, bool dynamic) where T : unmanaged
        {
            return new EngineShaderResourceView(name, Create2DTextureResourceView(name, size, values, dynamic));
        }
    }
}
