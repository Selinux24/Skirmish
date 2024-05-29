using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Index buffer description
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class BufferManagerIndices(bool dynamic) : IEngineBufferDescriptor
    {
        /// <summary>
        /// Data list
        /// </summary>
        private readonly List<uint> data = [];
        /// <summary>
        /// Descriptor list
        /// </summary>
        private readonly List<BufferDescriptor> descriptors = [];

        /// <inheritdoc/>
        public bool Dynamic { get; private set; } = dynamic;
        /// <inheritdoc/>
        public int BufferIndex { get; set; } = -1;
        /// <inheritdoc/>
        public int AllocatedSize { get; private set; } = 0;
        /// <inheritdoc/>
        public int ToAllocateSize
        {
            get
            {
                return data?.Count ?? 0;
            }
        }
        /// <inheritdoc/>
        public bool ReallocationNeeded { get; set; } = false;
        /// <inheritdoc/>
        public bool Allocated { get; private set; } = false;
        /// <inheritdoc/>
        public int Allocations { get; private set; } = 0;
        /// <inheritdoc/>
        public bool Dirty
        {
            get
            {
                return !Allocated || ReallocationNeeded;
            }
        }
        /// <summary>
        /// Index data
        /// </summary>
        public IEnumerable<uint> Data { get { return [.. data]; } }

        /// <summary>
        /// Adds a buffer descritor to the internal descriptors list
        /// </summary>
        /// <param name="descriptor">Descriptor</param>
        /// <param name="id">Id</param>
        /// <param name="bufferDescriptionIndex">Buffer description index</param>
        /// <param name="indices">Index list</param>
        /// <returns>Returns the new registerd descriptor</returns>
        public void AddDescriptor(BufferDescriptor descriptor, string id, int bufferDescriptionIndex, IEnumerable<uint> indices)
        {
            int offset;

            //Store current data index as descriptor offset
            offset = data.Count;

            //Add items to data list
            data.AddRange(indices);

            //Create and add the new descriptor to main descriptor list
            descriptor.Id = id;
            descriptor.BufferDescriptionIndex = bufferDescriptionIndex;
            descriptor.BufferOffset = offset;
            descriptor.Count = indices.Count();

            descriptors.Add(descriptor);
        }
        /// <summary>
        /// Removes a buffer descriptor from the internal list
        /// </summary>
        /// <param name="descriptor">Buffer descriptor to remove</param>
        public void RemoveDescriptor(BufferDescriptor descriptor)
        {
            //Find descriptor
            var index = descriptors.IndexOf(descriptor);
            if (index < 0)
            {
                return;
            }

            if (descriptor.Count > 0)
            {
                //If descriptor has items, remove from buffer descriptors
                data.RemoveRange(descriptor.BufferOffset, descriptor.Count);
            }

            //Remove from descriptors list
            descriptors.RemoveAt(index);

            if (descriptors.Count == 0)
            {
                return;
            }

            //Reallocate descriptor offsets
            descriptors[0].BufferOffset = 0;
            for (int i = 1; i < descriptors.Count; i++)
            {
                var prev = descriptors[i - 1];

                descriptors[i].BufferOffset = prev.BufferOffset + prev.Count;
            }
        }

        /// <inheritdoc/>
        public void Allocate()
        {
            AllocatedSize = Data.Count();
            Allocated = true;
            Allocations++;
            ReallocationNeeded = false;
        }
        /// <inheritdoc/>
        public IEngineBufferDescriptor Copy()
        {
            var d = new BufferManagerIndices(Dynamic)
            {
                BufferIndex = BufferIndex,
                AllocatedSize = 0,
                ReallocationNeeded = true,
                Allocated = false,
                Allocations = Allocations,
            };

            d.data.AddRange(data);
            d.descriptors.AddRange(descriptors);

            return d;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string strDynamic = Dynamic ? "[Dynamic]" : "";

            return $"[{typeof(uint)}]{strDynamic} AllocatedSize: {AllocatedSize} ToAllocateSize: {ToAllocateSize} Dirty: {Dirty}";
        }
    }
}
