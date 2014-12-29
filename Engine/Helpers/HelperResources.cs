using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using BindFlags = SharpDX.Direct3D11.BindFlags;
using Buffer = SharpDX.Direct3D11.Buffer;
using BufferDescription = SharpDX.Direct3D11.BufferDescription;
using CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using FilterFlags = SharpDX.Direct3D11.FilterFlags;
using ImageLoadInformation = SharpDX.Direct3D11.ImageLoadInformation;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using MapMode = SharpDX.Direct3D11.MapMode;
using Resource = SharpDX.Direct3D11.Resource;
using ResourceOptionFlags = SharpDX.Direct3D11.ResourceOptionFlags;
using ResourceUsage = SharpDX.Direct3D11.ResourceUsage;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using ShaderResourceViewDescription = SharpDX.Direct3D11.ShaderResourceViewDescription;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using Texture2DDescription = SharpDX.Direct3D11.Texture2DDescription;

namespace Engine.Helpers
{
    public static class HelperResources
    {
        public static Buffer CreateVertexBufferImmutable<T>(this Device device, T[] data)
           where T : struct
        {
            return CreateBuffer<T>(
                device,
                data,
                ResourceUsage.Immutable,
                BindFlags.VertexBuffer,
                CpuAccessFlags.None);
        }
        public static Buffer CreateIndexBufferImmutable<T>(this Device device, T[] data)
            where T : struct
        {
            return CreateBuffer<T>(
                device,
                data,
                ResourceUsage.Immutable,
                BindFlags.IndexBuffer,
                CpuAccessFlags.None);
        }
        public static Buffer CreateVertexBufferWrite<T>(this Device device, T[] data)
            where T : struct
        {
            return CreateBuffer<T>(
                device,
                data,
                ResourceUsage.Dynamic,
                BindFlags.VertexBuffer,
                CpuAccessFlags.Write);
        }
        public static Buffer CreateIndexBufferWrite<T>(this Device device, T[] data)
            where T : struct
        {
            return CreateBuffer<T>(
                device,
                data,
                ResourceUsage.Dynamic,
                BindFlags.IndexBuffer,
                CpuAccessFlags.Write);
        }
        public static Buffer CreateConstantBuffer<T>(this Device device)
            where T : struct
        {
            int size = ((Marshal.SizeOf(typeof(T)) + 15) / 16) * 16;

            BufferDescription description = new BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = size,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            return new Buffer(device, description);
        }
        public static Buffer CreateBuffer<T>(this Device device, T[] data, ResourceUsage usage, BindFlags binding, CpuAccessFlags access)
            where T : struct
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T)) * data.Length;

            using (DataStream d = new DataStream(sizeInBytes, true, true))
            {
                d.WriteRange(data);
                d.Position = 0;

                return new Buffer(
                    device,
                    d,
                    new BufferDescription()
                    {
                        Usage = usage,
                        SizeInBytes = sizeInBytes,
                        BindFlags = binding,
                        CpuAccessFlags = access,
                        OptionFlags = ResourceOptionFlags.None,
                        StructureByteStride = 0,
                    });
            }
        }

        public static void WriteBuffer<T>(this DeviceContext deviceContext, Buffer buffer, T[] data)
            where T : struct
        {
            if (data != null && data.Length > 0)
            {
                DataStream stream;
                deviceContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None, out stream);
                using (stream)
                {
                    stream.Position = 0;
                    stream.WriteRange(data);
                }
                deviceContext.UnmapSubresource(buffer, 0);
            }
        }
        public static void WriteConstantBuffer<T>(this DeviceContext deviceContext, Buffer constantBuffer, T value, long offset)
            where T : struct
        {
            DataStream stream;
            deviceContext.MapSubresource(constantBuffer, MapMode.WriteDiscard, MapFlags.None, out stream);
            using (stream)
            {
                stream.Position = offset;
                stream.Write(value);
            }
            deviceContext.UnmapSubresource(constantBuffer, 0);
        }

        public static ShaderResourceView LoadTexture(this Device device, byte[] buffer)
        {
            return ShaderResourceView.FromMemory(device, buffer, ImageLoadInformation.Default);
        }
        public static ShaderResourceView LoadTexture(this Device device, string filename)
        {
            return ShaderResourceView.FromFile(device, filename);
        }
        public static ShaderResourceView LoadTextureArray(this Device device, string[] filenames)
        {
            List<Texture2D> textureList = new List<Texture2D>();

            for (int i = 0; i < filenames.Length; i++)
            {
                textureList.Add(Texture2D.FromFile<Texture2D>(
                    device,
                    filenames[i],
                    new ImageLoadInformation()
                    {
                        FirstMipLevel = 0,
                        Usage = ResourceUsage.Staging,
                        BindFlags = BindFlags.None,
                        CpuAccessFlags = CpuAccessFlags.Write | CpuAccessFlags.Read,
                        OptionFlags = ResourceOptionFlags.None,
                        Format = Format.R8G8B8A8_UNorm,
                        Filter = FilterFlags.None,
                        MipFilter = FilterFlags.Linear,
                    }));
            }

            Texture2DDescription textureDescription = textureList[0].Description;

            using (Texture2D textureArray = new Texture2D(
                device,
                new Texture2DDescription()
                {
                    Width = textureDescription.Width,
                    Height = textureDescription.Height,
                    MipLevels = textureDescription.MipLevels,
                    ArraySize = filenames.Length,
                    Format = textureDescription.Format,
                    SampleDescription = new SampleDescription()
                    {
                        Count = 1,
                        Quality = 0,
                    },
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
                        DataBox mappedTex2D = device.ImmediateContext.MapSubresource(
                            textureList[i],
                            mipLevel,
                            MapMode.Read,
                            MapFlags.None);

                        int subIndex = Resource.CalculateSubResourceIndex(
                            mipLevel,
                            i,
                            textureDescription.MipLevels);

                        device.ImmediateContext.UpdateSubresource(
                            textureArray,
                            subIndex,
                            null,
                            mappedTex2D.DataPointer,
                            mappedTex2D.RowPitch,
                            mappedTex2D.SlicePitch);

                        device.ImmediateContext.UnmapSubresource(
                            textureList[i],
                            mipLevel);
                    }

                    textureList[i].Dispose();
                }

                ShaderResourceView result = new ShaderResourceView(
                    device,
                    textureArray,
                    new ShaderResourceViewDescription()
                    {
                        Format = textureDescription.Format,
                        Dimension = ShaderResourceViewDimension.Texture2DArray,
                        Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource()
                        {
                            MostDetailedMip = 0,
                            MipLevels = textureDescription.MipLevels,
                            FirstArraySlice = 0,
                            ArraySize = filenames.Length,
                        },
                    });

                return result;
            }
        }
        public static ShaderResourceView LoadTextureCube(this Device device, string filename, int faceSize)
        {
            Format format = Format.R8G8B8A8_UNorm;

            using (Texture2D cubeTex = new Texture2D(
                device,
                new Texture2DDescription()
                {
                    Width = faceSize,
                    Height = faceSize,
                    MipLevels = 0,
                    ArraySize = 6,
                    SampleDescription = new SampleDescription()
                    {
                        Count = 1,
                        Quality = 0,
                    },
                    Format = format,
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.GenerateMipMaps | ResourceOptionFlags.TextureCube,
                }))
            {
                return new ShaderResourceView(
                    device,
                    cubeTex,
                    new ShaderResourceViewDescription()
                    {
                        Format = format,
                        Dimension = ShaderResourceViewDimension.TextureCube,
                        TextureCube = new ShaderResourceViewDescription.TextureCubeResource()
                        {
                            MostDetailedMip = 0,
                            MipLevels = -1,
                        },
                    });
            }
        }
    }
}
