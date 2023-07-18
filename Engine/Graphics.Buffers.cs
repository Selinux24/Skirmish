using SharpDX;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine
{
    using Engine.Common;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Graphic buffers management
    /// </summary>
    public sealed partial class Graphics
    {
        /// <summary>
        /// Creates a vertex buffer
        /// </summary>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Vertex data collection</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        internal Buffer CreateVertexBuffer(string name, IEnumerable<IVertexData> data, bool dynamic)
        {
            var vertexType = data.First().VertexType;

            return vertexType switch
            {
                VertexTypes.Billboard => CreateVertexBuffer(name, data.OfType<VertexBillboard>(), dynamic),
                VertexTypes.Decal => CreateVertexBuffer(name, data.OfType<VertexDecal>(), dynamic),
                VertexTypes.CPUParticle => CreateVertexBuffer(name, data.OfType<VertexCpuParticle>(), dynamic),
                VertexTypes.GPUParticle => CreateVertexBuffer(name, data.OfType<VertexGpuParticle>(), dynamic),
                VertexTypes.Font => CreateVertexBuffer(name, data.OfType<VertexFont>(), dynamic),
                VertexTypes.Terrain => CreateVertexBuffer(name, data.OfType<VertexTerrain>(), dynamic),
                VertexTypes.Position => CreateVertexBuffer(name, data.OfType<VertexPosition>(), dynamic),
                VertexTypes.PositionColor => CreateVertexBuffer(name, data.OfType<VertexPositionColor>(), dynamic),
                VertexTypes.PositionTexture => CreateVertexBuffer(name, data.OfType<VertexPositionTexture>(), dynamic),
                VertexTypes.PositionNormalColor => CreateVertexBuffer(name, data.OfType<VertexPositionNormalColor>(), dynamic),
                VertexTypes.PositionNormalTexture => CreateVertexBuffer(name, data.OfType<VertexPositionNormalTexture>(), dynamic),
                VertexTypes.PositionNormalTextureTangent => CreateVertexBuffer(name, data.OfType<VertexPositionNormalTextureTangent>(), dynamic),
                VertexTypes.PositionSkinned => CreateVertexBuffer(name, data.OfType<VertexSkinnedPosition>(), dynamic),
                VertexTypes.PositionColorSkinned => CreateVertexBuffer(name, data.OfType<VertexSkinnedPositionColor>(), dynamic),
                VertexTypes.PositionTextureSkinned => CreateVertexBuffer(name, data.OfType<VertexSkinnedPositionTexture>(), dynamic),
                VertexTypes.PositionNormalColorSkinned => CreateVertexBuffer(name, data.OfType<VertexSkinnedPositionNormalColor>(), dynamic),
                VertexTypes.PositionNormalTextureSkinned => CreateVertexBuffer(name, data.OfType<VertexSkinnedPositionNormalTexture>(), dynamic),
                VertexTypes.PositionNormalTextureTangentSkinned => CreateVertexBuffer(name, data.OfType<VertexSkinnedPositionNormalTextureTangent>(), dynamic),
                _ => throw new EngineException($"Unknown vertex type: {vertexType}"),
            };
        }
        /// <summary>
        /// Creates a vertex buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="sizeInBytes">Buffer size in bytes</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        internal Buffer CreateVertexBuffer(string name, int sizeInBytes, bool dynamic)
        {
            return CreateBuffer(
                name,
                sizeInBytes,
                dynamic ? ResourceUsage.Dynamic : ResourceUsage.Immutable,
                BindFlags.VertexBuffer,
                dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None);
        }
        /// <summary>
        /// Creates a vertex buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        internal Buffer CreateVertexBuffer<T>(string name, IEnumerable<T> data, bool dynamic)
            where T : struct
        {
            return CreateBuffer(
                name,
                data,
                dynamic ? ResourceUsage.Dynamic : ResourceUsage.Immutable,
                BindFlags.VertexBuffer,
                dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None);
        }

        /// <summary>
        /// Creates an index buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        internal Buffer CreateIndexBuffer<T>(string name, IEnumerable<T> data, bool dynamic)
            where T : struct
        {
            return CreateBuffer(
                name,
                data,
                dynamic ? ResourceUsage.Dynamic : ResourceUsage.Immutable,
                BindFlags.IndexBuffer,
                dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None);
        }

        /// <summary>
        /// Creates a stream-out buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        internal Buffer CreateStreamOutBuffer<T>(string name, IEnumerable<T> data)
            where T : struct
        {
            return CreateBuffer(
                name,
                data,
                ResourceUsage.Default,
                BindFlags.VertexBuffer | BindFlags.StreamOutput,
                CpuAccessFlags.None);
        }
        /// <summary>
        /// Creates a stream-out buffer
        /// </summary>
        /// <param name="name">Buffer name</param>
        /// <param name="sizeInBytes">Buffer size in bytes</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        internal Buffer CreateStreamOutBuffer(string name, int sizeInBytes)
        {
            return CreateBuffer(
                name,
                sizeInBytes,
                ResourceUsage.Default,
                BindFlags.VertexBuffer | BindFlags.StreamOutput,
                CpuAccessFlags.None);
        }
        /// <summary>
        /// Creates a stream-out buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="length">Buffer length</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        internal Buffer CreateStreamOutBuffer<T>(string name, int length)
            where T : struct
        {
            return CreateBuffer<T>(
                name,
                length,
                ResourceUsage.Default,
                BindFlags.VertexBuffer | BindFlags.StreamOutput,
                CpuAccessFlags.None);
        }

        /// <summary>
        /// Creates a constant buffer for the specified data type
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <returns>Returns created buffer</returns>
        internal Buffer CreateConstantBuffer<T>(string name)
            where T : struct, IBufferData
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T));
            sizeInBytes = sizeInBytes / 16 * 16;

            ResourceUsage usage = ResourceUsage.Default;
            BindFlags binding = BindFlags.ConstantBuffer;

            Counters.RegBuffer(typeof(T), name, (int)usage, (int)binding, sizeInBytes, 1);

            var description = new BufferDescription()
            {
                Usage = usage,
                SizeInBytes = sizeInBytes,
                BindFlags = binding,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            return new Buffer(device, description)
            {
                DebugName = name,
            };
        }

        /// <summary>
        /// Creates a buffer for the specified data type
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="sizeInBytes">Buffer size in bytes</param>
        /// <param name="usage">Resource usage</param>
        /// <param name="binding">Binding</param>
        /// <param name="access">Cpu access</param>
        /// <returns>Returns created buffer</returns>
        internal Buffer CreateBuffer(string name, int sizeInBytes, ResourceUsage usage, BindFlags binding, CpuAccessFlags access)
        {
            Counters.RegBuffer(typeof(object), name, (int)usage, (int)binding, sizeInBytes, sizeInBytes);

            var description = new BufferDescription()
            {
                Usage = usage,
                SizeInBytes = sizeInBytes,
                BindFlags = binding,
                CpuAccessFlags = access,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            return new Buffer(device, description)
            {
                DebugName = name,
            };
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
        internal Buffer CreateBuffer<T>(string name, int length, ResourceUsage usage, BindFlags binding, CpuAccessFlags access)
            where T : struct
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T)) * length;

            Counters.RegBuffer(typeof(T), name, (int)usage, (int)binding, sizeInBytes, length);

            var description = new BufferDescription()
            {
                Usage = usage,
                SizeInBytes = sizeInBytes,
                BindFlags = binding,
                CpuAccessFlags = access,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            return new Buffer(device, description)
            {
                DebugName = name,
            };
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
        internal Buffer CreateBuffer<T>(string name, IEnumerable<T> data, ResourceUsage usage, BindFlags binding, CpuAccessFlags access)
            where T : struct
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T)) * data.Count();

            Counters.RegBuffer(typeof(T), name, (int)usage, (int)binding, sizeInBytes, data.Count());

            using var dstr = new DataStream(sizeInBytes, true, true);
            dstr.WriteRange(data.ToArray());
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

            return new Buffer(device, dstr, description)
            {
                DebugName = name,
            };
        }

        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="data">Complete data</param>
        internal bool WriteDiscardBuffer<T>(Buffer buffer, T data)
            where T : struct
        {
            return WriteDiscardBuffer(buffer, 0, new[] { data });
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="data">Complete data</param>
        internal bool WriteDiscardBuffer<T>(Buffer buffer, IEnumerable<T> data)
            where T : struct
        {
            return WriteDiscardBuffer(buffer, 0, data);
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Buffer element offset to write</param>
        /// <param name="data">Complete data</param>
        internal bool WriteDiscardBuffer<T>(Buffer buffer, long offset, IEnumerable<T> data)
            where T : struct
        {
            if (buffer == null)
            {
                return false;
            }

            if (data?.Any() != true)
            {
                return true;
            }

            ImmediateContext.GetDeviceContext().MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None, out DataStream stream);
            using (stream)
            {
                stream.Position = Marshal.SizeOf(default(T)) * offset;
                stream.WriteRange(data.ToArray());
            }
            ImmediateContext.GetDeviceContext().UnmapSubresource(buffer, 0);

            Counters.BufferWrites++;

            return true;
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="data">Complete data</param>
        internal bool WriteNoOverwriteBuffer<T>(Buffer buffer, IEnumerable<T> data)
            where T : struct
        {
            return WriteNoOverwriteBuffer(buffer, 0, data);
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Buffer element offset to write</param>
        /// <param name="data">Complete data</param>
        internal bool WriteNoOverwriteBuffer<T>(Buffer buffer, long offset, IEnumerable<T> data)
            where T : struct
        {
            if (buffer == null)
            {
                return false;
            }

            if (data?.Any() != true)
            {
                return true;
            }

            ImmediateContext.GetDeviceContext().MapSubresource(buffer, MapMode.WriteNoOverwrite, MapFlags.None, out DataStream stream);
            using (stream)
            {
                stream.Position = Marshal.SizeOf(default(T)) * offset;
                stream.WriteRange(data.ToArray());
            }
            ImmediateContext.GetDeviceContext().UnmapSubresource(buffer, 0);

            Counters.BufferWrites++;

            return true;
        }

        /// <summary>
        /// Updates a constant buffer value in the device
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        /// <param name="dataStream">Data stream</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="value">Value</param>
        internal bool UpdateConstantBuffer<T>(DataStream dataStream, Buffer buffer, T value) where T : struct, IBufferData
        {
            Marshal.StructureToPtr(value, dataStream.DataPointer, false);

            var dataBox = new DataBox(dataStream.DataPointer, 0, 0);
            device.ImmediateContext.UpdateSubresource(dataBox, buffer, 0);

            return true;
        }

        /// <summary>
        /// Reads an array of values from the specified buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphics context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns readed data</returns>
        internal IEnumerable<T> ReadBuffer<T>(Buffer buffer, int length)
            where T : struct
        {
            return ReadBuffer<T>(buffer, 0, length);
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
        internal IEnumerable<T> ReadBuffer<T>(Buffer buffer, long offset, int length)
            where T : struct
        {
            Counters.BufferReads++;

            T[] data = new T[length];

            ImmediateContext.GetDeviceContext().MapSubresource(buffer, MapMode.Read, MapFlags.None, out DataStream stream);
            using (stream)
            {
                stream.Position = Marshal.SizeOf(default(T)) * offset;

                for (int i = 0; i < length; i++)
                {
                    data[i] = stream.Read<T>();
                }
            }
            ImmediateContext.GetDeviceContext().UnmapSubresource(buffer, 0);

            return data;
        }
    }
}
