using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Engine.Common
{
    /// <summary>
    /// Index buffer description
    /// </summary>
    class BufferManagerIndices
    {
        /// <summary>
        /// Data list
        /// </summary>
        private readonly List<uint> data = new List<uint>();
        /// <summary>
        /// Descriptor list
        /// </summary>
        public readonly List<BufferDescriptor> descriptors = new List<BufferDescriptor>();

        /// <summary>
        /// Dynamic
        /// </summary>
        public readonly bool Dynamic;
        /// <summary>
        /// Index data
        /// </summary>
        public IEnumerable<uint> Data { get { return data.ToArray(); } }
        /// <summary>
        /// Index buffer index in the buffer manager list
        /// </summary>
        public int BufferIndex { get; set; } = -1;
        /// <summary>
        /// Allocated size into graphics device
        /// </summary>
        public int AllocatedSize { get; set; } = 0;
        /// <summary>
        /// Gets the size of the data to allocate
        /// </summary>
        public int ToAllocateSize
        {
            get
            {
                return data?.Count ?? 0;
            }
        }
        /// <summary>
        /// Gets wether the internal buffer needs reallocation
        /// </summary>
        public bool ReallocationNeeded { get; set; } = false;
        /// <summary>
        /// Gets wether the internal buffer is currently allocated in the graphic device
        /// </summary>
        public bool Allocated { get; set; } = false;
        /// <summary>
        /// Gets wether the current buffer is dirty
        /// </summary>
        /// <remarks>A buffer is dirty when needs reallocation or if it's not allocated at all</remarks>
        public bool Dirty
        {
            get
            {
                return !Allocated || ReallocationNeeded;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public BufferManagerIndices(bool dynamic)
        {
            Dynamic = dynamic;
        }

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
            Monitor.Enter(data);
            //Store current data index as descriptor offset
            int offset = data.Count;
            //Add items to data list
            data.AddRange(indices);
            Monitor.Exit(data);

            //Create and add the new descriptor to main descriptor list
            descriptor.Id = id;
            descriptor.BufferDescriptionIndex = bufferDescriptionIndex;
            descriptor.BufferOffset = offset;
            descriptor.Count = indices.Count();

            Monitor.Enter(descriptors);
            descriptors.Add(descriptor);
            Monitor.Exit(descriptors);
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
                Monitor.Enter(data);
                //If descriptor has items, remove from buffer descriptors
                data.RemoveRange(descriptor.BufferOffset, descriptor.Count);
                Monitor.Exit(data);
            }

            Monitor.Enter(descriptors);
            //Remove from descriptors list
            descriptors.RemoveAt(index);

            if (descriptors.Any())
            {
                //Reallocate descriptor offsets
                descriptors[0].BufferOffset = 0;
                for (int i = 1; i < descriptors.Count; i++)
                {
                    var prev = descriptors[i - 1];

                    descriptors[i].BufferOffset = prev.BufferOffset + prev.Count;
                }
            }
            Monitor.Exit(descriptors);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[{Dynamic}] AllocatedSize: {AllocatedSize} ToAllocateSize: {ToAllocateSize} Dirty: {Dirty}";
        }
    }
}
