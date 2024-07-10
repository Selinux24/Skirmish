using Engine.Common;

namespace Engine
{
    using SharpDX.DXGI;

    /// <summary>
    /// Drawing methods of buffer manager
    /// </summary>
    public static class GameDrawingHelper
    {
        /// <summary>
        /// Sets vertex buffers to device context
        /// </summary>
        /// <param name="dc">Device context</param>
        public static bool SetVertexBuffers(this Game game, IEngineDeviceContext dc)
        {
            if (dc == null)
            {
                return false;
            }

            var bufferManager = game.BufferManager;

            if (!bufferManager.Initilialized)
            {
                Logger.WriteWarning(game, "Attempt to set vertex buffers to Input Assembler with no initialized manager");
                return false;
            }

            if (bufferManager.HasVertexBufferDescriptorsDirty())
            {
                Logger.WriteWarning(game, "Attempt to set vertex buffers to Input Assembler with dirty descriptors");
                return false;
            }

            dc.IASetVertexBuffers(0, bufferManager.GetVertexBufferBindings());

            return true;
        }
        /// <summary>
        /// Sets input layout to device context
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="vertexShader">Vertex shader</param>
        /// <param name="descriptor">Buffer descriptor</param>
        /// <param name="topology">Topology</param>
        /// <param name="instanced">Use instancig data</param>
        public static bool SetInputAssembler(this Game game, IEngineDeviceContext dc, IEngineShader vertexShader, BufferDescriptor descriptor, Topology topology, bool instanced)
        {
            if (dc == null)
            {
                return false;
            }

            if (descriptor == null)
            {
                return true;
            }

            if (!descriptor.Ready)
            {
                return false;
            }

            var bufferManager = game.BufferManager;

            if (!bufferManager.Initilialized)
            {
                Logger.WriteWarning(game, "Attempt to set technique to Input Assembler with no initialized manager");
                return false;
            }

            var vertexBufferDescriptor = bufferManager.GetVertexBufferDescriptor(descriptor.BufferDescriptionIndex);
            if (vertexBufferDescriptor.Dirty)
            {
                Logger.WriteWarning(game, $"Attempt to set technique in buffer description {descriptor.BufferDescriptionIndex} to Input Assembler with no allocated buffer");
                return false;
            }

            if (!bufferManager.GetOrCreateInputLayout(descriptor.Id, vertexShader, vertexBufferDescriptor, instanced, out var layout))
            {
                return false;
            }

            dc.IAInputLayout = layout;
            dc.IAPrimitiveTopology = topology;
            return true;
        }
        /// <summary>
        /// Sets index buffers to device context
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="descriptor">Buffer descriptor</param>
        public static bool SetIndexBuffer(this Game game, IEngineDeviceContext dc, BufferDescriptor descriptor)
        {
            if (dc == null)
            {
                return false;
            }

            if (descriptor == null)
            {
                dc.IASetIndexBuffer(null, Format.R32_UInt, 0);
                return true;
            }

            if (!descriptor.Ready)
            {
                return false;
            }

            var bufferManager = game.BufferManager;

            if (!bufferManager.Initilialized)
            {
                Logger.WriteWarning(game, "Attempt to set index buffers to Input Assembler with no initialized manager");
                return false;
            }

            var indexBufferDescriptor = bufferManager.GetIndexBufferDescriptor(descriptor.BufferDescriptionIndex);
            if (indexBufferDescriptor.Dirty)
            {
                Logger.WriteWarning(game, $"Attempt to set index buffer in buffer description {descriptor.BufferDescriptionIndex} to Input Assembler with no allocated buffer");
                return false;
            }

            dc.IASetIndexBuffer(bufferManager.GetIndexBuffer(descriptor.BufferDescriptionIndex), Format.R32_UInt, 0);

            return true;
        }

        /// <summary>
        /// Writes instancing data
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="descriptor">Buffer descriptor</param>
        /// <param name="data">Instancig data</param>
        /// <param name="discard">Discards buffer content, no overwrite otherwise</param>
        public static bool WriteInstancingData<T>(this Game game, IEngineDeviceContext dc, BufferDescriptor descriptor, T[] data, bool discard = true)
            where T : struct
        {
            var bufferManager = game.BufferManager;

            var descriptors = bufferManager.GetInstancingBufferDescriptors();
            var vertexBuffers = bufferManager.GetVertexBuffers();

            return game.WriteDiscardBuffer(dc, descriptor, data, descriptors, vertexBuffers, discard);
        }
        /// <summary>
        /// Writes vertex data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="dc">Device context</param>
        /// <param name="descriptor">Buffer descriptor</param>
        /// <param name="data">Data to write</param>
        /// <param name="discard">Discards buffer content, no overwrite otherwise</param>
        public static bool WriteVertexBuffer<T>(this Game game, IEngineDeviceContext dc, BufferDescriptor descriptor, T[] data, bool discard = true)
            where T : struct
        {
            var bufferManager = game.BufferManager;

            var descriptors = bufferManager.GetVertexBufferDescriptors();
            var vertexBuffers = bufferManager.GetVertexBuffers();

            return game.WriteDiscardBuffer(dc, descriptor, data, descriptors, vertexBuffers, discard);
        }
        /// <summary>
        /// Writes index data into buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="descriptor">Buffer descriptor</param>
        /// <param name="data">Data to write</param>
        /// <param name="discard">Discards buffer content, no overwrite otherwise</param>
        public static bool WriteIndexBuffer(this Game game, IEngineDeviceContext dc, BufferDescriptor descriptor, uint[] data, bool discard = true)
        {
            var bufferManager = game.BufferManager;

            var descriptors = bufferManager.GetIndexBufferDescriptors();
            var indexBuffers = bufferManager.GetIndexBuffers();

            return game.WriteDiscardBuffer(dc, descriptor, data, descriptors, indexBuffers, discard);
        }

        /// <summary>
        /// Writes data in buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="dc">Device context</param>
        /// <param name="descriptor">Buffer descriptor</param>
        /// <param name="data">Data to write</param>
        /// <param name="bufferDescriptors">Buffer descriptors</param>
        /// <param name="buffers">Buffer list</param>
        /// <param name="discard">Discards buffer content, no overwrite otherwise</param>
        private static bool WriteDiscardBuffer<T>(this Game game, IEngineDeviceContext dc, BufferDescriptor descriptor, T[] data, IEngineBufferDescriptor[] bufferDescriptors, EngineBuffer[] buffers, bool discard)
            where T : struct
        {
            if (!game.ValidateWriteBuffer(dc, descriptor, bufferDescriptors, buffers, out var buffer))
            {
                return false;
            }

            if (discard)
            {
                return dc.WriteDiscardBuffer(buffer, descriptor.BufferOffset, data);
            }

            if (!dc.IsImmediateContext)
            {
                Logger.WriteWarning(game, $"Invalid WriteNoOverwriteBuffer action into deferred dc: {dc}. WriteDiscardBuffer action performed instead.");
            }

            return dc.WriteNoOverwriteBuffer(buffer, descriptor.BufferOffset, data);
        }
        /// <summary>
        /// Validates write data parameters
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="descriptor">Buffer descriptor</param>
        /// <param name="bufferDescriptors">Buffer descriptors</param>
        /// <param name="buffers">Buffer list</param>
        /// <param name="buffer">Returns the buffer to update</param>
        private static bool ValidateWriteBuffer(this Game game, IEngineDeviceContext dc, BufferDescriptor descriptor, IEngineBufferDescriptor[] bufferDescriptors, EngineBuffer[] buffers, out EngineBuffer buffer)
        {
            buffer = null;

            if (dc == null)
            {
                return false;
            }

            if (descriptor?.Ready != true)
            {
                return false;
            }

            var bufferManager = game.BufferManager;

            if (!bufferManager.Initilialized)
            {
                Logger.WriteWarning(game, $"Attempt to write data in buffer description {descriptor.BufferDescriptionIndex} with no initialized manager");
                return false;
            }

            var bufferDescriptor = bufferDescriptors[descriptor.BufferDescriptionIndex];
            if (bufferDescriptor.Dirty)
            {
                Logger.WriteWarning(game, $"Attempt to write data in buffer description {descriptor.BufferDescriptionIndex} with no allocated buffer");
                return false;
            }

            buffer = buffers[bufferDescriptor.BufferIndex];

            return true;
        }
    }
}
