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
        public EngineBuffer CreateVertexBuffer(string name, IEnumerable<IVertexData> data, bool dynamic)
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
        public EngineBuffer CreateVertexBuffer(string name, int sizeInBytes, bool dynamic)
        {
            return CreateBuffer(
                name,
                sizeInBytes,
                dynamic ? EngineResourceUsage.Dynamic : EngineResourceUsage.Immutable,
                EngineBindFlags.VertexBuffer,
                dynamic ? EngineCpuAccessFlags.Write : EngineCpuAccessFlags.None);
        }
        /// <summary>
        /// Creates a vertex buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public EngineBuffer CreateVertexBuffer<T>(string name, IEnumerable<T> data, bool dynamic)
            where T : struct
        {
            return CreateBuffer(
                name,
                data,
                dynamic ? EngineResourceUsage.Dynamic : EngineResourceUsage.Immutable,
                EngineBindFlags.VertexBuffer,
                dynamic ? EngineCpuAccessFlags.Write : EngineCpuAccessFlags.None);
        }

        /// <summary>
        /// Creates an index buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        public EngineBuffer CreateIndexBuffer<T>(string name, IEnumerable<T> data, bool dynamic)
            where T : struct
        {
            return CreateBuffer(
                name,
                data,
                dynamic ? EngineResourceUsage.Dynamic : EngineResourceUsage.Immutable,
                EngineBindFlags.IndexBuffer,
                dynamic ? EngineCpuAccessFlags.Write : EngineCpuAccessFlags.None);
        }

        /// <summary>
        /// Creates a stream-out buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public EngineBuffer CreateStreamOutBuffer<T>(string name, IEnumerable<T> data)
            where T : struct
        {
            return CreateBuffer(
                name,
                data,
                EngineResourceUsage.Default,
                EngineBindFlags.VertexBuffer | EngineBindFlags.StreamOutput,
                EngineCpuAccessFlags.None);
        }
        /// <summary>
        /// Creates a stream-out buffer
        /// </summary>
        /// <param name="name">Buffer name</param>
        /// <param name="sizeInBytes">Buffer size in bytes</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public EngineBuffer CreateStreamOutBuffer(string name, int sizeInBytes)
        {
            return CreateBuffer(
                name,
                sizeInBytes,
                EngineResourceUsage.Default,
                EngineBindFlags.VertexBuffer | EngineBindFlags.StreamOutput,
                EngineCpuAccessFlags.None);
        }
        /// <summary>
        /// Creates a stream-out buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="length">Buffer length</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public EngineBuffer CreateStreamOutBuffer<T>(string name, int length)
            where T : struct
        {
            return CreateBuffer<T>(
                name,
                length,
                EngineResourceUsage.Default,
                EngineBindFlags.VertexBuffer | EngineBindFlags.StreamOutput,
                EngineCpuAccessFlags.None);
        }

        /// <summary>
        /// Creates a constant buffer for the specified data type
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <returns>Returns created buffer</returns>
        public EngineBuffer CreateConstantBuffer<T>(string name)
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

            var buffer = new Buffer(device, description)
            {
                DebugName = name,
            };

            return new EngineBuffer(buffer);
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
        public EngineBuffer CreateBuffer(string name, int sizeInBytes, EngineResourceUsage usage, EngineBindFlags binding, EngineCpuAccessFlags access)
        {
            Counters.RegBuffer(typeof(object), name, (int)usage, (int)binding, sizeInBytes, sizeInBytes);

            var description = new BufferDescription()
            {
                Usage = (ResourceUsage)usage,
                SizeInBytes = sizeInBytes,
                BindFlags = (BindFlags)binding,
                CpuAccessFlags = (CpuAccessFlags)access,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            var buffer = new Buffer(device, description)
            {
                DebugName = name,
            };

            return new EngineBuffer(buffer);
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
        public EngineBuffer CreateBuffer<T>(string name, int length, EngineResourceUsage usage, EngineBindFlags binding, EngineCpuAccessFlags access)
            where T : struct
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T)) * length;

            Counters.RegBuffer(typeof(T), name, (int)usage, (int)binding, sizeInBytes, length);

            var description = new BufferDescription()
            {
                Usage = (ResourceUsage)usage,
                SizeInBytes = sizeInBytes,
                BindFlags = (BindFlags)binding,
                CpuAccessFlags = (CpuAccessFlags)access,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            var buffer = new Buffer(device, description)
            {
                DebugName = name,
            };

            return new EngineBuffer(buffer);
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
        public EngineBuffer CreateBuffer<T>(string name, IEnumerable<T> data, EngineResourceUsage usage, EngineBindFlags binding, EngineCpuAccessFlags access)
            where T : struct
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T)) * data.Count();

            Counters.RegBuffer(typeof(T), name, (int)usage, (int)binding, sizeInBytes, data.Count());

            using var dstr = new DataStream(sizeInBytes, true, true);
            dstr.WriteRange(data.ToArray());
            dstr.Position = 0;

            var description = new BufferDescription()
            {
                Usage = (ResourceUsage)usage,
                SizeInBytes = sizeInBytes,
                BindFlags = (BindFlags)binding,
                CpuAccessFlags = (CpuAccessFlags)access,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            var buffer = new Buffer(device, dstr, description)
            {
                DebugName = name,
            };

            return new EngineBuffer(buffer);
        }
    }
}
