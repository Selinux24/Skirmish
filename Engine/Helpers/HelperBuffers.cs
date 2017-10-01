using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.Helpers
{
    using Engine.Common;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Helper methods for graphic resources
    /// </summary>
    public static class HelperBuffers
    {
        /// <summary>
        /// Creates an inmutable index buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="graphics">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public static Buffer CreateIndexBufferImmutable<T>(this Graphics graphics, string name, T[] data)
            where T : struct
        {
            return CreateIndexBuffer<T>(graphics, name, data, false);
        }
        /// <summary>
        /// Creates a writable index buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="graphics">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public static Buffer CreateIndexBufferWrite<T>(this Graphics graphics, string name, T[] data)
            where T : struct
        {
            return CreateIndexBuffer<T>(graphics, name, data, true);
        }
        /// <summary>
        /// Creates an index buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="graphics">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        public static Buffer CreateIndexBuffer<T>(this Graphics graphics, string name, T[] data, bool dynamic)
            where T : struct
        {
            return CreateBuffer<T>(
                graphics,
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
        /// <param name="graphics">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public static Buffer CreateVertexBufferImmutable<T>(this Graphics graphics, string name, T[] data)
            where T : struct
        {
            return CreateVertexBuffer<T>(graphics, name, data, false);
        }
        /// <summary>
        /// Creates a writable vertex buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="graphics">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public static Buffer CreateVertexBufferWrite<T>(this Graphics graphics, string name, T[] data)
            where T : struct
        {
            return CreateVertexBuffer<T>(graphics, name, data, true);
        }
        /// <summary>
        /// Creates a vertex buffer from IVertexData
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        /// <returns>Returns new buffer</returns>
        public static Buffer CreateVertexBuffer(this Graphics graphics, string name, IVertexData[] vertices, bool dynamic)
        {
            Buffer buffer = null;

            if (vertices != null && vertices.Length > 0)
            {
                if (vertices[0].VertexType == VertexTypes.Billboard)
                {
                    buffer = CreateVertexBuffer<VertexBillboard>(graphics, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.Particle)
                {
                    buffer = CreateVertexBuffer<VertexCPUParticle>(graphics, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.GPUParticle)
                {
                    buffer = CreateVertexBuffer<VertexGPUParticle>(graphics, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.Position)
                {
                    buffer = CreateVertexBuffer<VertexPosition>(graphics, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionColor)
                {
                    buffer = CreateVertexBuffer<VertexPositionColor>(graphics, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalColor)
                {
                    buffer = CreateVertexBuffer<VertexPositionNormalColor>(graphics, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionTexture)
                {
                    buffer = CreateVertexBuffer<VertexPositionTexture>(graphics, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalTexture)
                {
                    buffer = CreateVertexBuffer<VertexPositionNormalTexture>(graphics, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureTangent)
                {
                    buffer = CreateVertexBuffer<VertexPositionNormalTextureTangent>(graphics, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.Terrain)
                {
                    buffer = CreateVertexBuffer<VertexTerrain>(graphics, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPosition>(graphics, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionColorSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPositionColor>(graphics, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalColorSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPositionNormalColor>(graphics, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionTextureSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPositionTexture>(graphics, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPositionNormalTexture>(graphics, name, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureTangentSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPositionNormalTextureTangent>(graphics, name, vertices, dynamic);
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
        /// <param name="graphics">Graphics device</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns new buffer</returns>
        public static Buffer CreateVertexBuffer<T>(this Graphics graphics, string name, IVertexData[] vertices, bool dynamic) where T : struct, IVertexData
        {
            T[] data = Array.ConvertAll((IVertexData[])vertices, v => (T)v);

            if (dynamic)
            {
                return graphics.CreateVertexBufferWrite(name, data);
            }
            else
            {
                return graphics.CreateVertexBufferImmutable(name, data);
            }
        }
        /// <summary>
        /// Creates a vertex buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="graphics">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public static Buffer CreateVertexBuffer<T>(this Graphics graphics, string name, T[] data, bool dynamic)
            where T : struct
        {
            return CreateBuffer<T>(
                graphics,
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
        public static Buffer CreateBuffer<T>(this Graphics graphics, string name, int length, ResourceUsage usage, BindFlags binding, CpuAccessFlags access)
            where T : struct
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T)) * length;

            Counters.RegBuffer(typeof(T), name, usage, binding, sizeInBytes, length);

            var description = new BufferDescription()
            {
                Usage = usage,
                SizeInBytes = sizeInBytes,
                BindFlags = binding,
                CpuAccessFlags = access,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            return new Buffer(graphics.Device, description);
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
        public static Buffer CreateBuffer<T>(this Graphics graphics, string name, T[] data, ResourceUsage usage, BindFlags binding, CpuAccessFlags access)
            where T : struct
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T)) * data.Length;

            Counters.RegBuffer(typeof(T), name, usage, binding, sizeInBytes, data.Length);

            using (var dstr = new DataStream(sizeInBytes, true, true))
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

                return new Buffer(graphics.Device, dstr, description);
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
    }
}
