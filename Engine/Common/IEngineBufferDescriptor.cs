﻿
namespace Engine.Common
{
    /// <summary>
    /// Engine buffer descriptor interface
    /// </summary>
    public interface IEngineBufferDescriptor
    {
        /// <summary>
        /// Dynamic buffer
        /// </summary>
        bool Dynamic { get; }
        /// <summary>
        /// Vertex buffer index in the buffer manager list
        /// </summary>
        int BufferIndex { get; }
        /// <summary>
        /// Gets whether the current buffer is dirty
        /// </summary>
        /// <remarks>A buffer is dirty when needs reallocation or if it's not allocated at all</remarks>
        bool Dirty { get; }
        /// <summary>
        /// Allocated size into graphics device
        /// </summary>
        int AllocatedSize { get; }
        /// <summary>
        /// Gets the size of the data to allocate
        /// </summary>
        int ToAllocateSize { get; }
        /// <summary>
        /// Gets whether the internal buffer needs reallocation
        /// </summary>
        bool ReallocationNeeded { get; }
        /// <summary>
        /// Gets whether the internal buffer is currently allocated in the graphic device
        /// </summary>
        bool Allocated { get; }
        /// <summary>
        /// Number of allocations
        /// </summary>
        int Allocations { get; }

        /// <summary>
        /// Updates the allocated buffer size
        /// </summary>
        void Allocate();
        /// <summary>
        /// Copies the descriptor
        /// </summary>
        IEngineBufferDescriptor Copy();
    }
}
