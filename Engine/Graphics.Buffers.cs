using Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine
{
    using Vortice.Direct3D11;

    /// <summary>
    /// Graphic buffers management
    /// </summary>
    public sealed partial class Graphics
    {
        /// <summary>
        /// Creates a vertex buffer
        /// </summary>
        /// <param name="name">Buffer name</param>
        /// <param name="sizeInBytes">Buffer size in bytes</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public EngineBuffer CreateVertexBuffer(string name, uint sizeInBytes, bool dynamic)
        {
            return CreateBuffer(
                name,
                sizeInBytes,
                dynamic ? EngineResourceUsage.Dynamic : EngineResourceUsage.Immutable,
                EngineBinds.VertexBuffer,
                dynamic ? EngineCpuAccess.Write : EngineCpuAccess.None);
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
            where T : unmanaged
        {
            return CreateBuffer(
                name,
                data,
                dynamic ? EngineResourceUsage.Dynamic : EngineResourceUsage.Immutable,
                EngineBinds.VertexBuffer,
                dynamic ? EngineCpuAccess.Write : EngineCpuAccess.None);
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
            where T : unmanaged
        {
            return CreateBuffer(
                name,
                data,
                dynamic ? EngineResourceUsage.Dynamic : EngineResourceUsage.Immutable,
                EngineBinds.IndexBuffer,
                dynamic ? EngineCpuAccess.Write : EngineCpuAccess.None);
        }

        /// <summary>
        /// Creates a stream-out buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public EngineBuffer CreateStreamOutBuffer<T>(string name, IEnumerable<T> data)
            where T : unmanaged
        {
            return CreateBuffer(
                name,
                data,
                EngineResourceUsage.Default,
                EngineBinds.VertexBuffer | EngineBinds.StreamOutput,
                EngineCpuAccess.None);
        }
        /// <summary>
        /// Creates a stream-out buffer
        /// </summary>
        /// <param name="name">Buffer name</param>
        /// <param name="sizeInBytes">Buffer size in bytes</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public EngineBuffer CreateStreamOutBuffer(string name, uint sizeInBytes)
        {
            return CreateBuffer(
                name,
                sizeInBytes,
                EngineResourceUsage.Default,
                EngineBinds.VertexBuffer | EngineBinds.StreamOutput,
                EngineCpuAccess.None);
        }
        /// <summary>
        /// Creates a stream-out buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="length">Buffer length</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public EngineBuffer CreateStreamOutBuffer<T>(string name, uint length)
            where T : unmanaged
        {
            return CreateBuffer<T>(
                name,
                length,
                EngineResourceUsage.Default,
                EngineBinds.VertexBuffer | EngineBinds.StreamOutput,
                EngineCpuAccess.None);
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
            uint sizeInBytes = (uint)Marshal.SizeOf<T>();
            sizeInBytes = sizeInBytes / 16 * 16;

            ResourceUsage usage = ResourceUsage.Default;
            BindFlags binding = BindFlags.ConstantBuffer;

            FrameCounters.RegBuffer<T>(name, (int)usage, (int)binding, sizeInBytes, 1);

            var description = new BufferDescription()
            {
                Usage = usage,
                ByteWidth = sizeInBytes,
                BindFlags = binding,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            var res = device.CreateBuffer(description, null, out var buffer);
            if (res.Failure)
            {
                throw new EngineException(res.ToString());
            }

            return new EngineBuffer(name, buffer);
        }

        /// <summary>
        /// Creates a buffer for the specified data type
        /// </summary>
        /// <param name="name">Buffer name</param>
        /// <param name="sizeInBytes">Buffer size in bytes</param>
        /// <param name="usage">Resource usage</param>
        /// <param name="binding">Binding</param>
        /// <param name="access">Cpu access</param>
        /// <returns>Returns created buffer</returns>
        public EngineBuffer CreateBuffer(string name, uint sizeInBytes, EngineResourceUsage usage, EngineBinds binding, EngineCpuAccess access)
        {
            if (sizeInBytes == 0)
            {
                return null;
            }

            FrameCounters.RegBuffer(name, (int)usage, (int)binding, sizeInBytes, sizeInBytes);

            var description = new BufferDescription()
            {
                Usage = (ResourceUsage)usage,
                ByteWidth = sizeInBytes,
                BindFlags = (BindFlags)binding,
                CPUAccessFlags = (CpuAccessFlags)access,
                MiscFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            var res = device.CreateBuffer(description, null, out var buffer);
            if (res.Failure)
            {
                throw new EngineException(res.ToString());
            }

            return new EngineBuffer(name, buffer);
        }
        /// <summary>
        /// Creates a buffer for the specified data type
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="length">Buffer length</param>
        /// <param name="usage">Resource usage</param>
        /// <param name="binding">Binding</param>
        /// <param name="access">Cpu access</param>
        /// <returns>Returns created buffer</returns>
        public EngineBuffer CreateBuffer<T>(string name, uint length, EngineResourceUsage usage, EngineBinds binding, EngineCpuAccess access)
            where T : unmanaged
        {
            uint sizeInBytes = (uint)Marshal.SizeOf<T>() * length;
            if (sizeInBytes == 0)
            {
                return null;
            }

            FrameCounters.RegBuffer<T>(name, (int)usage, (int)binding, sizeInBytes, length);

            var description = new BufferDescription()
            {
                Usage = (ResourceUsage)usage,
                ByteWidth = sizeInBytes,
                BindFlags = (BindFlags)binding,
                CPUAccessFlags = (CpuAccessFlags)access,
                MiscFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            var res = device.CreateBuffer(description, null, out var buffer);
            if (res.Failure)
            {
                throw new EngineException(res.ToString());
            }

            return new EngineBuffer(name, buffer);
        }
        /// <summary>
        /// Creates a buffer for the specified data type
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data</param>
        /// <param name="usage">Resource usage</param>
        /// <param name="binding">Binding</param>
        /// <param name="access">Cpu access</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        public EngineBuffer CreateBuffer<T>(string name, IEnumerable<T> values, EngineResourceUsage usage, EngineBinds binding, EngineCpuAccess access)
            where T : unmanaged
        {
            var buffer = CreateWithSubresource(
                name, values,
                (ResourceUsage)usage, (BindFlags)binding, (CpuAccessFlags)access,
                (desc, data) =>
                {
                    var res = device.CreateBuffer(desc, data, out var b);
                    if (res.Failure)
                    {
                        throw new EngineException(res.ToString());
                    }
                    return b;
                });

            buffer.DebugName = name;

            return new EngineBuffer(name, buffer);
        }
        /// <summary>
        /// Creates a populated resource using a callback function
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <typeparam name="TResource">Resource type</typeparam>
        /// <param name="name">Resource name</param>
        /// <param name="values">Data values</param>
        /// <param name="usage">Resource usage</param>
        /// <param name="binding">Data binding</param>
        /// <param name="access">CPU access flags</param>
        /// <param name="createResourceFunc">Callback</param>
        /// <returns>Returns the created resource</returns>
        private static unsafe TResource CreateWithSubresource<T, TResource>(
            string name, IEnumerable<T> values, ResourceUsage usage, BindFlags binding, CpuAccessFlags access,
            Func<BufferDescription, SubresourceData, TResource> createResourceFunc)
            where T : unmanaged
        {
            uint length = (uint)values.Count();

            T[] array = new T[length];
            Array.Copy(values.ToArray(), array, length);

            uint bytesPerPixel = (uint)Marshal.SizeOf<T>() * length;
            uint rowPitch = length * bytesPerPixel;
            uint slicePitch = rowPitch;

            var desc = new BufferDescription()
            {
                Usage = usage,
                ByteWidth = bytesPerPixel,
                BindFlags = binding,
                CPUAccessFlags = access,
                MiscFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            FrameCounters.RegBuffer<T>(name, (int)usage, (int)binding, bytesPerPixel, length);

            fixed (T* ptr = array)
            {
                return createResourceFunc(desc, new((IntPtr)ptr, rowPitch, slicePitch));
            }
        }
    }
}
