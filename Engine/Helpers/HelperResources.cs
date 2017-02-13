using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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
using Texture1D = SharpDX.Direct3D11.Texture1D;
using Texture1DDescription = SharpDX.Direct3D11.Texture1DDescription;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using Texture2DDescription = SharpDX.Direct3D11.Texture2DDescription;

namespace Engine.Helpers
{
    using Engine.Common;

    /// <summary>
    /// Helper methods for graphic resources
    /// </summary>
    public static class HelperResources
    {
        /// <summary>
        /// Creates an inmutable index buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="device">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public static Buffer CreateIndexBufferImmutable<T>(this Device device, string name, T[] data)
            where T : struct
        {
            return CreateIndexBuffer<T>(device, name, data, false);
        }
        /// <summary>
        /// Creates a writable index buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="device">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public static Buffer CreateIndexBufferWrite<T>(this Device device, string name, T[] data)
            where T : struct
        {
            return CreateIndexBuffer<T>(device, name, data, true);
        }
        /// <summary>
        /// Creates an index buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="device">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        public static Buffer CreateIndexBuffer<T>(this Device device, string name, T[] data, bool dynamic)
            where T : struct
        {
            return CreateBuffer<T>(
                device,
                name,
                data,
                dynamic ? ResourceUsage.Dynamic : ResourceUsage.Immutable,
                BindFlags.IndexBuffer,
                dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None);
        }

        /// <summary>
        /// Creates an inmutable vertex buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="device">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public static Buffer CreateVertexBufferImmutable<T>(this Device device, string name, T[] data)
            where T : struct
        {
            return CreateVertexBuffer<T>(device, name, data, false);
        }
        /// <summary>
        /// Creates a writable vertex buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="device">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public static Buffer CreateVertexBufferWrite<T>(this Device device, string name, T[] data)
            where T : struct
        {
            return CreateVertexBuffer<T>(device, name, data, true);
        }
        /// <summary>
        /// Creates a vertex buffer from IVertexData
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        /// <returns>Returns new buffer</returns>
        public static Buffer CreateVertexBuffer(this Device device, string name, IVertexData[] vertices, bool dynamic)
        {
            Buffer buffer = null;

            if (vertices != null && vertices.Length > 0)
            {
                if (vertices[0].VertexType == VertexTypes.Billboard)
                {
                    buffer = CreateVertexBuffer<VertexBillboard>(device, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.Particle)
                {
                    buffer = CreateVertexBuffer<VertexCPUParticle>(device, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.GPUParticle)
                {
                    buffer = CreateVertexBuffer<VertexGPUParticle>(device, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.Position)
                {
                    buffer = CreateVertexBuffer<VertexPosition>(device, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionColor)
                {
                    buffer = CreateVertexBuffer<VertexPositionColor>(device, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalColor)
                {
                    buffer = CreateVertexBuffer<VertexPositionNormalColor>(device, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionTexture)
                {
                    buffer = CreateVertexBuffer<VertexPositionTexture>(device, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalTexture)
                {
                    buffer = CreateVertexBuffer<VertexPositionNormalTexture>(device, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureTangent)
                {
                    buffer = CreateVertexBuffer<VertexPositionNormalTextureTangent>(device, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.Terrain)
                {
                    buffer = CreateVertexBuffer<VertexTerrain>(device, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPosition>(device, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionColorSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPositionColor>(device, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalColorSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPositionNormalColor>(device, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionTextureSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPositionTexture>(device, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPositionNormalTexture>(device, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureTangentSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPositionNormalTextureTangent>(device, name, vertices, dynamic);
                }
                else
                {
                    throw new Exception(string.Format("Unknown vertex type: {0}", vertices[0].VertexType));
                }
            }

            return buffer;
        }
        /// <summary>
        /// Creates a vertex buffer from IVertexData
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="device">Device</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns new buffer</returns>
        public static Buffer CreateVertexBuffer<T>(this Device device, string name, IVertexData[] vertices, bool dynamic) where T : struct, IVertexData
        {
            T[] data = Array.ConvertAll((IVertexData[])vertices, v => (T)v);

            if (dynamic)
            {
                return device.CreateVertexBufferWrite(name, data);
            }
            else
            {
                return device.CreateVertexBufferImmutable(name, data);
            }
        }
        /// <summary>
        /// Creates a vertex buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="device">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public static Buffer CreateVertexBuffer<T>(this Device device, string name, T[] data, bool dynamic)
            where T : struct
        {
            return CreateBuffer<T>(
                device,
                name,
                data,
                dynamic ? ResourceUsage.Dynamic : ResourceUsage.Immutable,
                BindFlags.VertexBuffer,
                dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None);
        }

        /// <summary>
        /// Creates a buffer for the specified data type
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="device">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="length">Buffer length</param>
        /// <param name="usage">Resource usage</param>
        /// <param name="binding">Binding</param>
        /// <param name="access">Cpu access</param>
        /// <returns>Returns created buffer</returns>
        public static Buffer CreateBuffer<T>(this Device device, string name, int length, ResourceUsage usage, BindFlags binding, CpuAccessFlags access)
            where T : struct
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T)) * length;

            Counters.RegBuffer(typeof(T), name, usage, binding, sizeInBytes, length);

            BufferDescription description = new BufferDescription()
            {
                Usage = usage,
                SizeInBytes = sizeInBytes,
                BindFlags = binding,
                CpuAccessFlags = access,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            return new Buffer(device, description);
        }
        /// <summary>
        /// Creates a buffer for the specified data type
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="device">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data</param>
        /// <param name="usage">Resource usage</param>
        /// <param name="binding">Binding</param>
        /// <param name="access">Cpu access</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public static Buffer CreateBuffer<T>(this Device device, string name, T[] data, ResourceUsage usage, BindFlags binding, CpuAccessFlags access)
            where T : struct
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T)) * data.Length;

            Counters.RegBuffer(typeof(T), name, usage, binding, sizeInBytes, data.Length);

            using (DataStream dstr = new DataStream(sizeInBytes, true, true))
            {
                dstr.WriteRange(data);
                dstr.Position = 0;

                var description = new BufferDescription()
                {
                    Usage = usage,
                    SizeInBytes = sizeInBytes,
                    BindFlags = binding,
                    CpuAccessFlags = access,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0,
                };

                return new Buffer(device, dstr, description);
            }
        }

        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="data">Complete data</param>
        public static void WriteDiscardBuffer<T>(this DeviceContext deviceContext, Buffer buffer, params T[] data)
            where T : struct
        {
            WriteDiscardBuffer<T>(deviceContext, buffer, 0, data);
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="data">Complete data</param>
        public static void WriteNoOverwriteBuffer<T>(this DeviceContext deviceContext, Buffer buffer, params T[] data)
            where T : struct
        {
            WriteNoOverwriteBuffer<T>(deviceContext, buffer, 0, data);
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Buffer element offset to write</param>
        /// <param name="data">Complete data</param>
        public static void WriteDiscardBuffer<T>(this DeviceContext deviceContext, Buffer buffer, long offset, params T[] data)
            where T : struct
        {
            Counters.BufferWrites++;

            if (data != null && data.Length > 0)
            {
                DataStream stream;
                deviceContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None, out stream);
                using (stream)
                {
                    stream.Position = Marshal.SizeOf(default(T)) * offset;
                    stream.WriteRange(data);
                }
                deviceContext.UnmapSubresource(buffer, 0);
            }
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Buffer element offset to write</param>
        /// <param name="data">Complete data</param>
        public static void WriteNoOverwriteBuffer<T>(this DeviceContext deviceContext, Buffer buffer, long offset, params T[] data)
            where T : struct
        {
            Counters.BufferWrites++;

            if (data != null && data.Length > 0)
            {
                DataStream stream;
                deviceContext.MapSubresource(buffer, MapMode.WriteNoOverwrite, MapFlags.None, out stream);
                using (stream)
                {
                    stream.Position = Marshal.SizeOf(default(T)) * offset;
                    stream.WriteRange(data);
                }
                deviceContext.UnmapSubresource(buffer, 0);
            }
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <param name="deviceContext">Graphics context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="vertices">Vertices</param>
        public static void WriteDiscardBuffer(this DeviceContext deviceContext, Buffer buffer, long offset, IVertexData[] vertices)
        {
            if (vertices[0].VertexType == VertexTypes.Billboard)
            {
                deviceContext.WriteDiscardBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexBillboard)v));
            }
            else if (vertices[0].VertexType == VertexTypes.Position)
            {
                deviceContext.WriteDiscardBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexPosition)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionColor)
            {
                deviceContext.WriteDiscardBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexPositionColor)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalColor)
            {
                deviceContext.WriteDiscardBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexPositionNormalColor)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionTexture)
            {
                deviceContext.WriteDiscardBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexPositionTexture)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalTexture)
            {
                deviceContext.WriteDiscardBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexPositionNormalTexture)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureTangent)
            {
                deviceContext.WriteDiscardBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexPositionNormalTextureTangent)v));
            }
            else if (vertices[0].VertexType == VertexTypes.Terrain)
            {
                deviceContext.WriteDiscardBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexTerrain)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionSkinned)
            {
                deviceContext.WriteDiscardBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexSkinnedPosition)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionColorSkinned)
            {
                deviceContext.WriteDiscardBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexSkinnedPositionColor)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalColorSkinned)
            {
                deviceContext.WriteDiscardBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexSkinnedPositionNormalColor)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionTextureSkinned)
            {
                deviceContext.WriteDiscardBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexSkinnedPositionTexture)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureSkinned)
            {
                deviceContext.WriteDiscardBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexSkinnedPositionNormalTexture)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureTangentSkinned)
            {
                deviceContext.WriteDiscardBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexSkinnedPositionNormalTextureTangent)v));
            }
            else
            {
                throw new Exception(string.Format("Unknown vertex type: {0}", vertices[0].VertexType));
            }
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <param name="deviceContext">Graphics context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="vertices">Vertices</param>
        public static void WriteNoOverwriteBuffer(this DeviceContext deviceContext, Buffer buffer, long offset, IVertexData[] vertices)
        {
            if (vertices[0].VertexType == VertexTypes.Billboard)
            {
                deviceContext.WriteNoOverwriteBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexBillboard)v));
            }
            else if (vertices[0].VertexType == VertexTypes.Position)
            {
                deviceContext.WriteNoOverwriteBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexPosition)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionColor)
            {
                deviceContext.WriteNoOverwriteBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexPositionColor)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalColor)
            {
                deviceContext.WriteNoOverwriteBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexPositionNormalColor)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionTexture)
            {
                deviceContext.WriteNoOverwriteBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexPositionTexture)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalTexture)
            {
                deviceContext.WriteNoOverwriteBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexPositionNormalTexture)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureTangent)
            {
                deviceContext.WriteNoOverwriteBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexPositionNormalTextureTangent)v));
            }
            else if (vertices[0].VertexType == VertexTypes.Terrain)
            {
                deviceContext.WriteNoOverwriteBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexTerrain)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionSkinned)
            {
                deviceContext.WriteNoOverwriteBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexSkinnedPosition)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionColorSkinned)
            {
                deviceContext.WriteNoOverwriteBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexSkinnedPositionColor)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalColorSkinned)
            {
                deviceContext.WriteNoOverwriteBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexSkinnedPositionNormalColor)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionTextureSkinned)
            {
                deviceContext.WriteNoOverwriteBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexSkinnedPositionTexture)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureSkinned)
            {
                deviceContext.WriteNoOverwriteBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexSkinnedPositionNormalTexture)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureTangentSkinned)
            {
                deviceContext.WriteNoOverwriteBuffer(buffer, offset, Array.ConvertAll(vertices, v => (VertexSkinnedPositionNormalTextureTangent)v));
            }
            else
            {
                throw new Exception(string.Format("Unknown vertex type: {0}", vertices[0].VertexType));
            }
        }

        /// <summary>
        /// Reads an array of values from the specified buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphics context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns readed data</returns>
        public static T[] ReadBuffer<T>(this DeviceContext deviceContext, Buffer buffer, int length)
            where T : struct
        {
            return ReadBuffer<T>(deviceContext, buffer, 0, length);
        }
        /// <summary>
        /// Reads an array of values from the specified buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphics context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset to read</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns readed data</returns>
        public static T[] ReadBuffer<T>(this DeviceContext deviceContext, Buffer buffer, long offset, int length)
            where T : struct
        {
            Counters.BufferReads++;

            T[] data = new T[length];

            DataStream stream;
            deviceContext.MapSubresource(buffer, MapMode.Read, MapFlags.None, out stream);
            using (stream)
            {
                stream.Position = Marshal.SizeOf(default(T)) * offset;

                for (int i = 0; i < length; i++)
                {
                    data[i] = stream.Read<T>();
                }
            }
            deviceContext.UnmapSubresource(buffer, 0);

            return data;
        }

        /// <summary>
        /// Loads a texture from memory in the graphics device
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="buffer">Data buffer</param>
        /// <returns>Returns the resource view</returns>
        public static ShaderResourceView LoadTexture(this Device device, byte[] buffer)
        {
            Counters.Textures++;

            return ShaderResourceView.FromMemory(device, buffer, ImageLoadInformation.Default);
        }
        /// <summary>
        /// Loads a texture from file in the graphics device
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="filename">Path to file</param>
        /// <returns>Returns the resource view</returns>
        public static ShaderResourceView LoadTexture(this Device device, string filename)
        {
            Counters.Textures++;

            return ShaderResourceView.FromFile(device, filename, ImageLoadInformation.Default);
        }
        /// <summary>
        /// Loads a texture from file in the graphics device
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="file">Stream</param>
        /// <returns>Returns the resource view</returns>
        public static ShaderResourceView LoadTexture(this Device device, MemoryStream file)
        {
            Counters.Textures++;

            return ShaderResourceView.FromStream(device, file, (int)file.Length, ImageLoadInformation.Default);
        }
        /// <summary>
        /// Loads a texture array from a file collection in the graphics device
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="filenames">Path file collection</param>
        /// <returns>Returns the resource view</returns>
        public static ShaderResourceView LoadTextureArray(this Device device, string[] filenames)
        {
            Counters.Textures++;

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
        /// <summary>
        /// Loads a texture array from a file collection in the graphics device
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="files">Stream collection</param>
        /// <returns>Returns the resource view</returns>
        public static ShaderResourceView LoadTextureArray(this Device device, MemoryStream[] files)
        {
            List<Texture2D> textureList = new List<Texture2D>();

            for (int i = 0; i < files.Length; i++)
            {
                textureList.Add(Texture2D.FromStream<Texture2D>(
                    device,
                    files[i],
                    (int)files[i].Length,
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
                    ArraySize = files.Length,
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
                            ArraySize = files.Length,
                        },
                    });

                return result;
            }
        }
        /// <summary>
        /// Loads a cube texture from file in the graphics device
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="filename">Path to file</param>
        /// <param name="format">Format</param>
        /// <param name="faceSize">Face size</param>
        /// <returns>Returns the resource view</returns>
        public static ShaderResourceView LoadTextureCube(this Device device, string filename, Format format, int faceSize)
        {
            Counters.Textures++;

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
        /// <summary>
        /// Loads a cube texture from file in the graphics device
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="file">Stream</param>
        /// <param name="format">Format</param>
        /// <param name="faceSize">Face size</param>
        /// <returns>Returns the resource view</returns>
        public static ShaderResourceView LoadTextureCube(this Device device, MemoryStream file, Format format, int faceSize)
        {
            Counters.Textures++;

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
        /// <summary>
        /// Creates a texture filled with specified values
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="size">Texture size</param>
        /// <param name="values">Color values</param>
        /// <returns>Returns created texture</returns>
        public static ShaderResourceView CreateTexture1D(this Device device, int size, Vector4[] values)
        {
            Counters.Textures++;

            using (DataStream str = DataStream.Create(values, false, false))
            {
                using (Resource randTex = new Texture1D(
                    device,
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
                    ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription();
                    srvDesc.Format = Format.R32G32B32A32_Float;
                    srvDesc.Dimension = ShaderResourceViewDimension.Texture1D;
                    srvDesc.Texture1D.MipLevels = 1;
                    srvDesc.Texture1D.MostDetailedMip = 0;

                    return new ShaderResourceView(device, randTex, srvDesc);
                }
            }
        }
        /// <summary>
        /// Creates a texture filled with specified values
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="size">Texture size</param>
        /// <param name="values">Color values</param>
        /// <returns>Returns created texture</returns>
        public static ShaderResourceView CreateTexture2D(this Device device, int size, Vector4[] values)
        {
            Counters.Textures++;

            Vector4[] tmp = new Vector4[size * size];
            Array.Copy(values, tmp, values.Length);

            using (DataStream str = DataStream.Create(tmp, false, false))
            {
                DataBox dBox = new DataBox(str.DataPointer, size * (int)FormatHelper.SizeOfInBytes(Format.R32G32B32A32_Float), 0);

                using (Texture2D texture = new Texture2D(
                    device,
                    new Texture2DDescription()
                    {
                        Format = Format.R32G32B32A32_Float,
                        Width = size,
                        Height = size,
                        ArraySize = 1,
                        MipLevels = 1,
                        SampleDescription = new SampleDescription()
                        {
                            Count = 1,
                            Quality = 0,
                        },
                        Usage = ResourceUsage.Immutable,
                        BindFlags = BindFlags.ShaderResource,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None,
                    },
                    new[] { dBox }))
                {
                    ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription();
                    srvDesc.Format = Format.R32G32B32A32_Float;
                    srvDesc.Dimension = ShaderResourceViewDimension.Texture2D;
                    srvDesc.Texture1D.MipLevels = 1;
                    srvDesc.Texture1D.MostDetailedMip = 0;

                    return new ShaderResourceView(device, texture, srvDesc);
                }
            }
        }
        /// <summary>
        /// Creates a random 1D texture
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="size">Texture size</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="seed">Random seed</param>
        /// <returns>Returns created texture</returns>
        public static ShaderResourceView CreateRandomTexture(this Device device, int size, float min, float max, int seed = 0)
        {
            Counters.Textures++;

            Random rnd = new Random(seed);

            var randomValues = new List<Vector4>();
            for (int i = 0; i < size; i++)
            {
                randomValues.Add(rnd.NextVector4(new Vector4(min), new Vector4(max)));
            }

            return CreateTexture1D(device, size, randomValues.ToArray());
        }
        /// <summary>
        /// Creates a texture for render target use
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Returns new texture</returns>
        public static Texture2D CreateRenderTargetTexture(this Device device, Format format, int width, int height)
        {
            Counters.Textures++;

            return new Texture2D(
                device,
                new Texture2DDescription()
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = format,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                });
        }
    }
}
