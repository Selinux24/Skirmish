using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Engine.Common
{
    /// <summary>
    /// Index buffer description
    /// </summary>
    public class BufferManagerIndices : IEngineBufferDescriptor
    {
        /// <summary>
        /// Data list
        /// </summary>
        private readonly List<uint> data = new();
        /// <summary>
        /// Descriptor list
        /// </summary>
        private readonly List<BufferDescriptor> descriptors = new();

        /// <inheritdoc/>
        public bool Dynamic { get; private set; }
        /// <inheritdoc/>
        public int BufferIndex { get; set; } = -1;
        /// <inheritdoc/>
        public int AllocatedSize { get; set; } = 0;
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
        public bool Allocated { get; set; } = false;
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
        public IEnumerable<uint> Data { get { return data.ToArray(); } }

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
            int offset;

            Monitor.Enter(data);
            try
            {
                //Store current data index as descriptor offset
                offset = data.Count;
                //Add items to data list
                data.AddRange(indices);
            }
            finally
            {
                Monitor.Exit(data);
            }

            //Create and add the new descriptor to main descriptor list
            descriptor.Id = id;
            descriptor.BufferDescriptionIndex = bufferDescriptionIndex;
            descriptor.BufferOffset = offset;
            descriptor.Count = indices.Count();

            Monitor.Enter(descriptors);
            try
            {
                descriptors.Add(descriptor);
            }
            finally
            {
                Monitor.Exit(descriptors);
            }
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
                try
                {
                    //If descriptor has items, remove from buffer descriptors
                    data.RemoveRange(descriptor.BufferOffset, descriptor.Count);
                }
                finally
                {
                    Monitor.Exit(data);
                }
            }

            Monitor.Enter(descriptors);
            try
            {
                //Remove from descriptors list
                descriptors.RemoveAt(index);

                if (!descriptors.Any())
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
            finally
            {
                Monitor.Exit(descriptors);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[{Dynamic}] AllocatedSize: {AllocatedSize} ToAllocateSize: {ToAllocateSize} Dirty: {Dirty}";
        }
    }
}
