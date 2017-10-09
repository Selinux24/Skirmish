using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.IO;

namespace Engine.Helpers
{
    using Engine.Common;
    using Engine.Helpers.DDS;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Texture loader
    /// </summary>
    static class HelperTextures
    {
        private static BitmapSource ReadBitmap(string filename)
        {
            using (var factory = new ImagingFactory2())
            {
                var bitmapDecoder = new BitmapDecoder(factory, filename, DecodeOptions.CacheOnLoad);

                var formatConverter = new FormatConverter(factory);

                formatConverter.Initialize(
                    bitmapDecoder.GetFrame(0),
                    PixelFormat.Format32bppPRGBA,
                    BitmapDitherType.None,
                    null,
                    0.0,
                    BitmapPaletteType.Custom);

                return formatConverter;
            }
        }
        private static BitmapSource ReadBitmap(Stream stream)
        {
            using (var factory = new ImagingFactory2())
            {
                var bitmapDecoder = new BitmapDecoder(factory, stream, DecodeOptions.CacheOnLoad);

                var formatConverter = new FormatConverter(factory);

                formatConverter.Initialize(
                    bitmapDecoder.GetFrame(0),
                    PixelFormat.Format32bppPRGBA,
                    BitmapDitherType.None,
                    null,
                    0.0,
                    BitmapPaletteType.Custom);

                return formatConverter;
            }
        }
        private static TextureDescription ReadTexture(byte[] buffer)
        {
            DDSHeader header;
            int offset;
            if (DDSHeader.GetInfo(buffer, out header, out offset))
            {
                return new TextureDescription(header, null, buffer, offset, 0);
            }
            else
            {
                using (var stream = new MemoryStream(buffer))
                using (var bitmap = ReadBitmap(stream))
                {
                    return new TextureDescription(bitmap);
                }
            }
        }
        private static TextureDescription ReadTexture(string filename)
        {
            DDSHeader header;
            int offset;
            byte[] buffer;
            if (DDSHeader.GetInfo(filename, out header, out offset, out buffer))
            {
                return new TextureDescription(header, null, buffer, offset, 0);
            }
            else
            {
                using (var bitmap = ReadBitmap(filename))
                {
                    return new TextureDescription(bitmap);
                }
            }
        }
        private static TextureDescription ReadTexture(MemoryStream stream)
        {
            DDSHeader header;
            int offset;
            byte[] buffer;
            if (DDSHeader.GetInfo(stream, out header, out offset, out buffer))
            {
                return new TextureDescription(header, null, buffer, offset, 0);
            }
            else
            {
                using (var bitmap = ReadBitmap(stream))
                {
                    return new TextureDescription(bitmap);
                }
            }
        }
        private static TextureDescription[] ReadTexture(string[] filenames)
        {
            TextureDescription[] textureList = new TextureDescription[filenames.Length];

            for (int i = 0; i < filenames.Length; i++)
            {
                textureList[i] = ReadTexture(filenames[i]);
            }

            return textureList;
        }
        private static TextureDescription[] ReadTexture(MemoryStream[] streams)
        {
            TextureDescription[] textureList = new TextureDescription[streams.Length];

            for (int i = 0; i < streams.Length; i++)
            {
                textureList[i] = ReadTexture(streams[i]);
            }

            return textureList;
        }
        private static EngineShaderResourceView CreateResource(Graphics graphics, TextureDescription tDesc)
        {
            var fmtSupport = graphics.Device.CheckFormatSupport(tDesc.Format);
            var autogen = fmtSupport.HasFlag(FormatSupport.MipAutogen);

            using (var texture = CreateTexture2D(graphics, tDesc.Width, tDesc.Height, tDesc.Format, 1, autogen))
            {
                EngineShaderResourceView result = null;

                if (autogen)
                {
                    var description = new ShaderResourceViewDescription()
                    {
                        Format = texture.Description.Format,
                        Dimension = ShaderResourceViewDimension.Texture2D,
                        Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                        {
                            MipLevels = (autogen) ? -1 : 1,
                        }
                    };
                    result = new EngineShaderResourceView(graphics.Device, texture, description);
                }
                else
                {
                    result = new EngineShaderResourceView(graphics.Device, texture);
                }

                graphics.DeviceContext.UpdateSubresource(tDesc.GetDataBox(), texture, 0);

                if (autogen)
                {
                    graphics.DeviceContext.GenerateMips(result.SRV);
                }

                return result;
            }
        }
        private static EngineShaderResourceView CreateResource(Graphics graphics, TextureDescription[] tDescList)
        {
            var textureDescription = tDescList[0];

            var fmtSupport = graphics.Device.CheckFormatSupport(textureDescription.Format);
            var autogen = fmtSupport.HasFlag(FormatSupport.MipAutogen);

            using (var textureArray = CreateTexture2D(graphics, textureDescription.Width, textureDescription.Height, textureDescription.Format, tDescList.Length, autogen))
            {
                EngineShaderResourceView result = null;

                if (autogen)
                {
                    var desc = new ShaderResourceViewDescription()
                    {
                        Format = textureDescription.Format,
                        Dimension = ShaderResourceViewDimension.Texture2DArray,
                        Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource()
                        {
                            ArraySize = tDescList.Length,
                            MipLevels = (autogen) ? -1 : 1,
                        }
                    };

                    result = new EngineShaderResourceView(graphics.Device, textureArray, desc);
                }
                else
                {
                    result = new EngineShaderResourceView(graphics.Device, textureArray);
                }

                for (int i = 0; i < tDescList.Length; i++)
                {
                    int mipSize;
                    var index = textureArray.CalculateSubResourceIndex(0, i, out mipSize);

                    graphics.DeviceContext.UpdateSubresource(tDescList[i].GetDataBox(), textureArray, index);
                }

                if (autogen)
                {
                    graphics.DeviceContext.GenerateMips(result.SRV);
                }

                return result;
            }
        }
        private static Texture2D CreateTexture2D(Graphics graphics, int width, int height, Format format, int arraySize, bool generateMips)
        {
            var description = new Texture2DDescription()
            {
                Width = width,
                Height = height,
                ArraySize = arraySize,
                BindFlags = (generateMips) ? BindFlags.ShaderResource | BindFlags.RenderTarget : BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = format,
                MipLevels = (generateMips) ? 0 : 1,
                OptionFlags = (generateMips) ? ResourceOptionFlags.GenerateMipMaps : ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
            };

            return new Texture2D(graphics.Device, description);
        }

        /// <summary>
        /// Loads a texture from memory in the graphics device
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="buffer">Data buffer</param>
        /// <returns>Returns the resource view</returns>
        public static EngineShaderResourceView LoadTexture(this Graphics graphics, byte[] buffer)
        {
            try
            {
                Counters.Textures++;

                using (var resorce = ReadTexture(buffer))
                {
                    return CreateResource(graphics, resorce);
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from byte array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture from file in the graphics device
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="filename">Path to file</param>
        /// <returns>Returns the resource view</returns>
        public static EngineShaderResourceView LoadTexture(this Graphics graphics, string filename)
        {
            try
            {
                Counters.Textures++;

                using (var resorce = ReadTexture(filename))
                {
                    return CreateResource(graphics, resorce);
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
        /// <param name="graphics">Graphics device</param>
        /// <param name="stream">Stream</param>
        /// <returns>Returns the resource view</returns>
        public static EngineShaderResourceView LoadTexture(this Graphics graphics, MemoryStream stream)
        {
            try
            {
                Counters.Textures++;

                using (var resorce = ReadTexture(stream))
                {
                    return CreateResource(graphics, resorce);
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from stream Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture array from a file collection in the graphics device
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="filenames">Path file collection</param>
        /// <returns>Returns the resource view</returns>
        public static EngineShaderResourceView LoadTextureArray(this Graphics graphics, string[] filenames)
        {
            try
            {
                Counters.Textures++;

                var textureList = ReadTexture(filenames);

                var resource = CreateResource(graphics, textureList);

                Helper.Dispose(textureList);

                return resource;
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from file array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture array from a file collection in the graphics device
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="streams">Stream collection</param>
        /// <returns>Returns the resource view</returns>
        public static EngineShaderResourceView LoadTextureArray(this Graphics graphics, MemoryStream[] streams)
        {
            try
            {
                Counters.Textures++;

                var textureList = ReadTexture(streams);

                var resource = CreateResource(graphics, textureList);

                Helper.Dispose(textureList);

                return resource;
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from stream array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a cube texture from file in the graphics device
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="filename">Path to file</param>
        /// <param name="format">Format</param>
        /// <param name="faceSize">Face size</param>
        /// <returns>Returns the resource view</returns>
        public static EngineShaderResourceView LoadTextureCube(this Graphics graphics, string filename, Format format, int faceSize)
        {
            try
            {
                Counters.Textures++;

                using (var cubeTex = new Texture2D(
                    graphics.Device,
                    new Texture2DDescription()
                    {
                        Width = faceSize,
                        Height = faceSize,
                        MipLevels = 0,
                        ArraySize = 6,
                        SampleDescription = new SampleDescription(1, 0),
                        Format = format,
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.GenerateMipMaps | ResourceOptionFlags.TextureCube,
                    }))
                {
                    return new EngineShaderResourceView(graphics.Device, cubeTex);
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTextureCube from filename Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a cube texture from file in the graphics device
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="stream">Stream</param>
        /// <param name="format">Format</param>
        /// <param name="faceSize">Face size</param>
        /// <returns>Returns the resource view</returns>
        public static EngineShaderResourceView LoadTextureCube(this Graphics graphics, MemoryStream stream, Format format, int faceSize)
        {
            try
            {
                Counters.Textures++;

                using (var cubeTex = new Texture2D(
                    graphics.Device,
                    new Texture2DDescription()
                    {
                        Width = faceSize,
                        Height = faceSize,
                        MipLevels = 0,
                        ArraySize = 6,
                        SampleDescription = new SampleDescription(1, 0),
                        Format = format,
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.GenerateMipMaps | ResourceOptionFlags.TextureCube,
                    }))
                {
                    return new EngineShaderResourceView(graphics.Device, cubeTex);
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTextureCube from stream Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a texture filled with specified values
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="size">Texture size</param>
        /// <param name="values">Color values</param>
        /// <returns>Returns created texture</returns>
        public static EngineShaderResourceView CreateTexture1D(this Graphics graphics, int size, Vector4[] values)
        {
            try
            {
                Counters.Textures++;

                using (var str = DataStream.Create(values, false, false))
                {
                    using (var randTex = new Texture1D(
                        graphics.Device,
                        new Texture1DDescription()
                        {
                            Format = Format.R32G32B32A32_Float,
                            Width = size,
                            ArraySize = 1,
                            MipLevels = 1,
                            Usage = ResourceUsage.Immutable,
                            BindFlags = BindFlags.ShaderResource,
                            CpuAccessFlags = CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.None,
                        },
                        str))
                    {
                        return new EngineShaderResourceView(graphics.Device, randTex);
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
        /// <param name="graphics">Graphics device</param>
        /// <param name="size">Texture size</param>
        /// <param name="values">Color values</param>
        /// <returns>Returns created texture</returns>
        public static EngineShaderResourceView CreateTexture2D(this Graphics graphics, int size, Vector4[] values)
        {
            try
            {
                Counters.Textures++;

                var tmp = new Vector4[size * size];
                Array.Copy(values, tmp, values.Length);

                using (var str = DataStream.Create(tmp, false, false))
                {
                    var dBox = new DataBox(str.DataPointer, size * (int)FormatHelper.SizeOfInBytes(Format.R32G32B32A32_Float), 0);

                    using (var texture = new Texture2D(
                        graphics.Device,
                        new Texture2DDescription()
                        {
                            Format = Format.R32G32B32A32_Float,
                            Width = size,
                            Height = size,
                            ArraySize = 1,
                            MipLevels = 1,
                            SampleDescription = new SampleDescription(1, 0),
                            Usage = ResourceUsage.Immutable,
                            BindFlags = BindFlags.ShaderResource,
                            CpuAccessFlags = CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.None,
                        },
                        new[] { dBox }))
                    {
                        return new EngineShaderResourceView(graphics.Device, texture);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateTexture2D from value array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a random 1D texture
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="size">Texture size</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="seed">Random seed</param>
        /// <returns>Returns created texture</returns>
        public static EngineShaderResourceView CreateRandomTexture(this Graphics graphics, int size, float min, float max, int seed = 0)
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

                return CreateTexture1D(graphics, size, randomValues.ToArray());
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateRandomTexture Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a texture for render target use
        /// </summary>
        /// <param name="graphics">Device</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Returns new texture</returns>
        public static Texture2D CreateRenderTargetTexture(this Graphics graphics, Format format, int width, int height)
        {
            try
            {
                Counters.Textures++;

                return new Texture2D(
                    graphics.Device,
                    new Texture2DDescription()
                    {
                        Width = width,
                        Height = height,
                        MipLevels = 1,
                        ArraySize = 1,
                        Format = format,
                        SampleDescription = graphics.CurrentSampleDescription,
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None
                    });
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateRenderTargetTexture Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a set of texture and depth stencil view for shadow mapping
        /// </summary>
        /// <param name="graphics">Device</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="dsv">Resulting Depth Stencil View</param>
        /// <param name="srv">Resulting Shader Resource View</param>
        public static void CreateShadowMapTextures(this Graphics graphics, int width, int height, out EngineDepthStencilView dsv, out EngineShaderResourceView srv)
        {
            var depthMap = new Texture2D(
                graphics.Device,
                new Texture2DDescription
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.R24G8_Typeless,
                    SampleDescription = graphics.CurrentSampleDescription,
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                });

            using (depthMap)
            {
                var dsDimension = graphics.MultiSampled ?
                    DepthStencilViewDimension.Texture2DMultisampled :
                    DepthStencilViewDimension.Texture2D;

                var dsDescription = new DepthStencilViewDescription
                {
                    Flags = DepthStencilViewFlags.None,
                    Format = Format.D24_UNorm_S8_UInt,
                    Dimension = dsDimension,
                    Texture2D = new DepthStencilViewDescription.Texture2DResource()
                    {
                        MipSlice = 0,
                    },
                    Texture2DMS = new DepthStencilViewDescription.Texture2DMultisampledResource()
                    {

                    },
                };

                var rvDimension = graphics.MultiSampled ?
                    ShaderResourceViewDimension.Texture2DMultisampled :
                    ShaderResourceViewDimension.Texture2D;

                var rvDescription = new ShaderResourceViewDescription
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    Dimension = rvDimension,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0
                    },
                    Texture2DMS = new ShaderResourceViewDescription.Texture2DMultisampledResource()
                    {

                    },
                };

                dsv = new EngineDepthStencilView(graphics.Device, depthMap, dsDescription);
                srv = new EngineShaderResourceView(graphics.Device, depthMap, rvDescription);
            }
        }
    }
}
