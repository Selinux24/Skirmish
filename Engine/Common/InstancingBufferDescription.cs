using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Engine.Common
{
    /// <summary>
    /// Vertex buffer description
    /// </summary>
    class InstancingBufferDescription
    {
        /// <summary>
        /// Instancing descriptor list
        /// </summary>
        private readonly List<BufferDescriptor> instancingDescriptors = new List<BufferDescriptor>();

        /// <summary>
        /// Dynamic buffer
        /// </summary>
        public readonly bool Dynamic;
        /// <summary>
        /// Instances
        /// </summary>
        public int Instances { get; set; } = 0;
        /// <summary>
        /// Vertex buffer index in the buffer manager list
        /// </summary>
        public int BufferIndex { get; set; } = -1;
        /// <summary>
        /// Vertex buffer binding index in the manager list
        /// </summary>
        public int BufferBindingIndex { get; set; } = -1;
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
                return this.Instances;
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
        public InstancingBufferDescription(bool dynamic)
        {
            this.Dynamic = dynamic;
        }

        /// <summary>
        /// Gets the buffer format stride
        /// </summary>
        /// <returns>Returns the buffer format stride in bytes</returns>
        public int GetStride()
        {
            return default(VertexInstancingData).GetStride();
        }

        /// <summary>
        /// Adds a buffer descritor to the internal descriptos list
        /// </summary>
        /// <param name="descriptor">Descriptor</param>
        /// <param name="id">Id</param>
        /// <param name="bufferDescriptionIndex">Buffer description</param>
        /// <param name="instances">Number of instances</param>
        public void AddDescriptor(BufferDescriptor descriptor, string id, int bufferDescriptionIndex, int instances)
        {
            //Store current data index as descriptor offset
            int offset = this.Instances;

            //Increment the instance count
            this.Instances += instances;

            //Add the new descriptor to main descriptor list
            descriptor.Id = id;
            descriptor.BufferDescriptionIndex = bufferDescriptionIndex;
            descriptor.BufferOffset = offset;
            descriptor.Count = instances;

            Monitor.Enter(this.instancingDescriptors);
            this.instancingDescriptors.Add(descriptor);
            Monitor.Exit(this.instancingDescriptors);
        }
        /// <summary>
        /// Removes a buffer descriptor from the internal list
        /// </summary>
        /// <param name="descriptor">Buffer descriptor to remove</param>
        /// <param name="instances">Number of instances</param>
        public void RemoveDescriptor(BufferDescriptor descriptor, int instances)
        {
            Monitor.Enter(this.instancingDescriptors);
            //Remove descriptor
            this.instancingDescriptors.Remove(descriptor);

            if (this.instancingDescriptors.Any())
            {
                //Reallocate descriptor offsets
                this.instancingDescriptors[0].BufferOffset = 0;
                for (int i = 1; i < this.instancingDescriptors.Count; i++)
                {
                    var prev = this.instancingDescriptors[i - 1];

                    this.instancingDescriptors[i].BufferOffset = prev.BufferOffset + prev.Count;
                }
            }
            Monitor.Exit(this.instancingDescriptors);

            this.Instances -= instances;
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns a description of the instance</returns>
        public override string ToString()
        {
            return $"[{typeof(VertexInstancingData)}][{Dynamic}] Instances: {Instances} AllocatedSize: {AllocatedSize} ToAllocateSize: {ToAllocateSize} Dirty: {Dirty}";
        }
    }
}
