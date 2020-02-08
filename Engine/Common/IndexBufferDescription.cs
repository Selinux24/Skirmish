using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Engine.Common
{
    /// <summary>
    /// Index buffer description
    /// </summary>
    class IndexBufferDescription
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
                return this.data?.Count ?? 0;
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
        public IndexBufferDescription(bool dynamic)
        {
            this.Dynamic = dynamic;
        }

        /// <summary>
        /// Adds a buffer descritor to the internal descriptos list
        /// </summary>
        /// <param name="descriptor">Descriptor</param>
        /// <param name="id">Id</param>
        /// <param name="slot">Buffer slot</param>
        /// <param name="indices">Index list</param>
        /// <returns>Returns the new registerd descriptor</returns>
        public void AddDescriptor(BufferDescriptor descriptor, string id, int slot, IEnumerable<uint> indices)
        {
            Monitor.Enter(this.data);
            //Store current data index as descriptor offset
            int offset = this.data.Count;
            //Add items to data list
            this.data.AddRange(indices);
            Monitor.Exit(this.data);

            //Create and add the new descriptor to main descriptor list
            descriptor.Id = id;
            descriptor.Slot = slot;
            descriptor.Offset = offset;
            descriptor.Count = indices.Count();

            Monitor.Enter(this.descriptors);
            this.descriptors.Add(descriptor);
            Monitor.Exit(this.descriptors);
        }
        /// <summary>
        /// Removes a buffer descriptor from the internal list
        /// </summary>
        /// <param name="descriptor">Buffer descriptor to remove</param>
        public void RemoveDescriptor(BufferDescriptor descriptor)
        {
            //Find descriptor
            var index = this.descriptors.IndexOf(descriptor);
            if (index < 0)
            {
                return;
            }

            if (descriptor.Count > 0)
            {
                Monitor.Enter(this.data);
                //If descriptor has items, remove from buffer descriptors
                this.data.RemoveRange(descriptor.Offset, descriptor.Count);
                Monitor.Exit(this.data);
            }

            Monitor.Enter(this.descriptors);
            //Remove from descriptors list
            this.descriptors.RemoveAt(index);

            if (this.descriptors.Any())
            {
                //Reallocate descriptor offsets
                this.descriptors[0].Offset = 0;
                for (int i = 1; i < this.descriptors.Count; i++)
                {
                    var prev = this.descriptors[i - 1];

                    this.descriptors[i].Offset = prev.Offset + prev.Count;
                }
            }
            Monitor.Exit(this.descriptors);
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns a description of the instance</returns>
        public override string ToString()
        {
            return $"[{Dynamic}] AllocatedSize: {AllocatedSize} ToAllocateSize: {ToAllocateSize} Dirty: {Dirty}";
        }
    }
}
