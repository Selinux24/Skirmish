using SharpDX;
using SharpDX.DXGI;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.IO;

namespace Engine.Helpers
{
    using Engine.Helpers.DDS;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Texture loader
    /// </summary>
    static class HelperTextures
    {
        private static bool GetInfo(string filename, out DDSHeader header, out int offset, out byte[] buffer)
        {
            buffer = File.ReadAllBytes(filename);

            return GetInfo(buffer, out header, out offset);
        }
        private static bool GetInfo(MemoryStream stream, out DDSHeader header, out int offset, out byte[] buffer)
        {
            buffer = stream.GetBuffer();
            return GetInfo(buffer, out header, out offset);
        }
        private static bool GetInfo(byte[] data, out DDSHeader header, out int offset)
        {
            // Validate DDS file in memory
            header = new DDSHeader();
            offset = 0;

            if (data.Length < (sizeof(uint) + DDSHeader.StructSize))
            {
                return false;
            }

            //first is magic number
            int dwMagicNumber = BitConverter.ToInt32(data, 0);
            if (dwMagicNumber != DDSHeader.DDS_MAGIC)
            {
                return false;
            }

            header = data.ToStructure<DDSHeader>(4, DDSHeader.StructSize);

            // Verify header to validate DDS file
            if (header.Size != DDSHeader.StructSize ||
                header.PixelFormat.Size != DDSPixelFormat.StructSize)
            {
                return false;
            }

            // Check for DX10 extension
            bool bDXT10Header = false;
            if (header.IsDX10)
            {
                // Must be long enough for both headers and magic value
                if (data.Length < (DDSHeader.StructSize + 4 + DDSHeaderDX10.StructSize))
                {
                    return false;
                }

                bDXT10Header = true;
            }

            offset = 4 + DDSHeader.StructSize + (bDXT10Header ? DDSHeaderDX10.StructSize : 0);

            return true;
        }
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
        private static Resource CreateResource(Device device, string filename, bool shaderResource)
        {
            DDSHeader header;
            int offset;
            byte[] buffer;
            if (GetInfo(filename, out header, out offset, out buffer))
            {
                bool isCube;
                return CreateResourceFromDDS(device, header, null, buffer, offset, 0, shaderResource, out isCube);
            }
            else
            {
                using (var bitmap = ReadBitmap(filename))
                {
                    return CreateResourceFromBitmapSource(device, bitmap, shaderResource);
                }
            }
        }
        private static Resource CreateResource(Device device, byte[] buffer, bool shaderResource)
        {
            DDSHeader header;
            int offset;
            if (GetInfo(buffer, out header, out offset))
            {
                bool isCube;
                return CreateResourceFromDDS(device, header, null, buffer, offset, 0, shaderResource, out isCube);
            }
            else
            {
                using (var mem = new MemoryStream(buffer))
                using (var bitmap = ReadBitmap(mem))
                {
                    return CreateResourceFromBitmapSource(device, bitmap, shaderResource);
                }
            }
        }
        private static Resource CreateResource(Device device, MemoryStream stream, bool shaderResource)
        {
            DDSHeader header;
            int offset;
            byte[] buffer;
            if (GetInfo(stream, out header, out offset, out buffer))
            {
                bool isCube;
                return CreateResourceFromDDS(device, header, null, buffer, offset, 0, shaderResource, out isCube);
            }
            else
            {
                using (var bitmap = ReadBitmap(stream))
                {
                    return CreateResourceFromBitmapSource(device, bitmap, shaderResource);
                }
            }
        }
        private static Resource CreateResourceFromBitmapSource(Device device, BitmapSource bitmap, bool shaderResource)
        {
            // Allocate DataStream to receive the WIC image pixels
            int stride = bitmap.Size.Width * 4;

            using (var buffer = new DataStream(bitmap.Size.Height * stride, true, true))
            {
                // Copy the content of the WIC to the buffer
                bitmap.CopyPixels(stride, buffer);

                var dr = new DataRectangle(buffer.DataPointer, stride);

                Texture2DDescription description;

                if (shaderResource)
                {
                    description = new Texture2DDescription()
                    {
                        Width = bitmap.Size.Width,
                        Height = bitmap.Size.Height,
                        ArraySize = 1,
                        BindFlags = BindFlags.ShaderResource,
                        Usage = ResourceUsage.Immutable,
                        CpuAccessFlags = CpuAccessFlags.None,
                        Format = Format.R8G8B8A8_UNorm,
                        MipLevels = 1,
                        OptionFlags = ResourceOptionFlags.None,
                        SampleDescription = new SampleDescription(1, 0),
                    };
                }
                else
                {
                    description = new Texture2DDescription()
                    {
                        Width = bitmap.Size.Width,
                        Height = bitmap.Size.Height,
                        ArraySize = 1,
                        BindFlags = BindFlags.None,
                        Usage = ResourceUsage.Staging,
                        CpuAccessFlags = CpuAccessFlags.Write | CpuAccessFlags.Read,
                        Format = Format.R8G8B8A8_UNorm,
                        MipLevels = 1,
                        OptionFlags = ResourceOptionFlags.None,
                        SampleDescription = new SampleDescription(1, 0),
                    };
                }

                return new Texture2D(device, description, dr);
            }
        }
        private static Resource CreateResourceFromDDS(Device device, DDSHeader header, DDSHeaderDX10? header10, byte[] bitData, int offset, int maxsize, bool shaderResource, out bool isCubeMap)
        {
            isCubeMap = false;

            bool validFile = false;

            int width = header.Width;
            int height = header.Height;
            int depth = header.Depth;
            Format format;
            ResourceDimension resDim;
            int arraySize;

            if (header.IsDX10)
            {
                validFile = header10.Value.Validate(
                    header.Flags,
                    ref width, ref height, ref depth,
                    out format, out resDim, out arraySize, out isCubeMap);
            }
            else
            {
                validFile = header.Validate(
                    ref width, ref height, ref depth,
                    out format, out resDim, out arraySize, out isCubeMap);
            }

            if (validFile)
            {
                int mipCount = header.MipMapCount;
                if (0 == mipCount)
                {
                    mipCount = 1;
                }

                int numBytes = 0;
                int numRowBytes = 0;
                int numRows = 0;
                DDSPixelFormat.GetSurfaceInfo(
                    width, height, format, 
                    out numBytes, out numRowBytes, out numRows);

                var bytes = new byte[numBytes];
                Array.Copy(bitData, offset, bytes, 0, numBytes);

                using (var buffer = DataStream.Create(bytes, true, true))
                {
                    var dr = new DataRectangle(buffer.DataPointer, numRowBytes);

                    Texture2DDescription description;

                    if (shaderResource)
                    {
                        description = new Texture2DDescription()
                        {
                            Width = width,
                            Height = height,
                            ArraySize = 1,
                            BindFlags = BindFlags.ShaderResource,
                            Usage = ResourceUsage.Immutable,
                            CpuAccessFlags = CpuAccessFlags.None,
                            Format = format,
                            MipLevels = 1,
                            OptionFlags = ResourceOptionFlags.None,
                            SampleDescription = new SampleDescription(1, 0),
                        };
                    }
                    else
                    {
                        description = new Texture2DDescription()
                        {
                            Width = width,
                            Height = height,
                            ArraySize = 1,
                            BindFlags = BindFlags.None,
                            Usage = ResourceUsage.Staging,
                            CpuAccessFlags = CpuAccessFlags.Write | CpuAccessFlags.Read,
                            Format = format,
                            MipLevels = 1,
                            OptionFlags = ResourceOptionFlags.None,
                            SampleDescription = new SampleDescription(1, 0),
                        };
                    }

                    return new Texture2D(device, description, dr);
                }
            }
            else
            {
                throw new EngineException("Bad DDS File");
            }
        }

        /// <summary>
        /// Loads a texture from memory in the graphics device
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="buffer">Data buffer</param>
        /// <returns>Returns the resource view</returns>
        public static ShaderResourceView LoadTexture(this Graphics graphics, byte[] buffer)
        {
            try
            {
                Counters.Textures++;

                var texture = CreateResource(graphics.Device, buffer, true);

                return new ShaderResourceView(graphics.Device, texture);
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
        public static ShaderResourceView LoadTexture(this Graphics graphics, string filename)
        {
            try
            {
                Counters.Textures++;

                var texture = CreateResource(graphics.Device, filename, true);

                return new ShaderResourceView(graphics.Device, texture);
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
        public static ShaderResourceView LoadTexture(this Graphics graphics, MemoryStream stream)
        {
            try
            {
                Counters.Textures++;

                var texture = CreateResource(graphics.Device, stream, true);

                return new ShaderResourceView(graphics.Device, texture);
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
        public static ShaderResourceView LoadTextureArray(this Graphics graphics, string[] filenames)
        {
            try
            {
                Counters.Textures++;

                List<Texture2D> textureList = new List<Texture2D>();

                for (int i = 0; i < filenames.Length; i++)
                {
                    var texture = (Texture2D)CreateResource(graphics.Device, File.ReadAllBytes(filenames[i]), false);

                    textureList.Add(texture);
                }

                var textureDescription = textureList[0].Description;

                using (var textureArray = new Texture2D(
                    graphics.Device,
                    new Texture2DDescription()
                    {
                        Width = textureDescription.Width,
                        Height = textureDescription.Height,
                        MipLevels = textureDescription.MipLevels,
                        ArraySize = filenames.Length,
                        Format = textureDescription.Format,
                        SampleDescription = new SampleDescription(1, 0),
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.ShaderResource,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None,
                    }))
                {

                    for (int i = 0; i < textureList.Count; i++)
                    {
                        for (int mipLevel = 0; mipLevel < textureDescription.MipLevels; mipLevel++)
                        {
                            var mappedTex2D = graphics.Device.ImmediateContext.MapSubresource(
                                textureList[i],
                                mipLevel,
                                MapMode.Read,
                                MapFlags.None);

                            int subIndex = Resource.CalculateSubResourceIndex(
                                mipLevel,
                                i,
                                textureDescription.MipLevels);

                            graphics.Device.ImmediateContext.UpdateSubresource(
                                textureArray,
                                subIndex,
                                null,
                                mappedTex2D.DataPointer,
                                mappedTex2D.RowPitch,
                                mappedTex2D.SlicePitch);

                            graphics.Device.ImmediateContext.UnmapSubresource(
                                textureList[i],
                                mipLevel);
                        }

                        textureList[i].Dispose();
                    }

                    return new ShaderResourceView(graphics.Device, textureArray);
                }
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
        public static ShaderResourceView LoadTextureArray(this Graphics graphics, MemoryStream[] streams)
        {
            try
            {
                Counters.Textures++;

                List<Texture2D> textureList = new List<Texture2D>();

                for (int i = 0; i < streams.Length; i++)
                {
                    var texture = (Texture2D)CreateResource(graphics.Device, streams[i], false);

                    textureList.Add(texture);
                }

                var textureDescription = textureList[0].Description;

                using (var textureArray = new Texture2D(
                    graphics.Device,
                    new Texture2DDescription()
                    {
                        Width = textureDescription.Width,
                        Height = textureDescription.Height,
                        MipLevels = textureDescription.MipLevels,
                        ArraySize = streams.Length,
                        Format = textureDescription.Format,
                        SampleDescription = new SampleDescription(1, 0),
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.ShaderResource,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None,
                    }))
                {
                    for (int i = 0; i < textureList.Count; i++)
                    {
                        for (int mipLevel = 0; mipLevel < textureDescription.MipLevels; mipLevel++)
                        {
                            var mappedTex2D = graphics.Device.ImmediateContext.MapSubresource(
                                textureList[i],
                                mipLevel,
                                MapMode.Read,
                                MapFlags.None);

                            int subIndex = Resource.CalculateSubResourceIndex(
                                mipLevel,
                                i,
                                textureDescription.MipLevels);

                            graphics.Device.ImmediateContext.UpdateSubresource(
                                textureArray,
                                subIndex,
                                null,
                                mappedTex2D.DataPointer,
                                mappedTex2D.RowPitch,
                                mappedTex2D.SlicePitch);

                            graphics.Device.ImmediateContext.UnmapSubresource(
                                textureList[i],
                                mipLevel);
                        }

                        textureList[i].Dispose();
                    }

                    return new ShaderResourceView(graphics.Device, textureArray);
                }
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
        public static ShaderResourceView LoadTextureCube(this Graphics graphics, string filename, Format format, int faceSize)
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
                    return new ShaderResourceView(graphics.Device, cubeTex);
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
        public static ShaderResourceView LoadTextureCube(this Graphics graphics, MemoryStream stream, Format format, int faceSize)
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
                    return new ShaderResourceView(graphics.Device, cubeTex);
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
        public static ShaderResourceView CreateTexture1D(this Graphics graphics, int size, Vector4[] values)
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
                        return new ShaderResourceView(graphics.Device, randTex);
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
        public static ShaderResourceView CreateTexture2D(this Graphics graphics, int size, Vector4[] values)
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
                        return new ShaderResourceView(graphics.Device, texture);
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
        public static ShaderResourceView CreateRandomTexture(this Graphics graphics, int size, float min, float max, int seed = 0)
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
    }
}
