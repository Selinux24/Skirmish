using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Engine.Common
{
    /// <summary>
    /// Vertex buffer description
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    public class BufferManagerInstances<T> : IEngineBufferDescriptor
        where T : struct, IInstacingData
    {
        /// <summary>
        /// Instancing descriptor list
        /// </summary>
        private readonly List<BufferDescriptor> instancingDescriptors = new();

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
                return Instances;
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
        /// Instances
        /// </summary>
        public int Instances { get; set; } = 0;
        /// <summary>
        /// Vertex buffer binding index in the manager list
        /// </summary>
        public int BufferBindingIndex { get; set; } = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        public BufferManagerInstances(bool dynamic)
        {
            Dynamic = dynamic;
        }

        /// <summary>
        /// Gets the buffer format stride
        /// </summary>
        /// <returns>Returns the buffer format stride in bytes</returns>
        public static int GetStride()
        {
            return default(T).GetStride();
        }

        /// <summary>
        /// Adds a buffer descritor to the internal descriptors list
        /// </summary>
        /// <param name="descriptor">Descriptor</param>
        /// <param name="id">Id</param>
        /// <param name="bufferDescriptionIndex">Buffer description</param>
        /// <param name="instances">Number of instances</param>
        public void AddDescriptor(BufferDescriptor descriptor, string id, int bufferDescriptionIndex, int instances)
        {
            //Store current data index as descriptor offset
            int offset = Instances;

            //Increment the instance count
            Instances += instances;

            //Add the new descriptor to main descriptor list
            descriptor.Id = id;
            descriptor.BufferDescriptionIndex = bufferDescriptionIndex;
            descriptor.BufferOffset = offset;
            descriptor.Count = instances;

            Monitor.Enter(instancingDescriptors);
            try
            {
                instancingDescriptors.Add(descriptor);
            }
            finally
            {
                Monitor.Exit(instancingDescriptors);
            }
        }
        /// <summary>
        /// Removes a buffer descriptor from the internal list
        /// </summary>
        /// <param name="descriptor">Buffer descriptor to remove</param>
        /// <param name="instances">Number of instances</param>
        public void RemoveDescriptor(BufferDescriptor descriptor, int instances)
        {
            Monitor.Enter(instancingDescriptors);
            try
            {
                //Remove descriptor
                instancingDescriptors.Remove(descriptor);

                if (instancingDescriptors.Any())
                {
                    //Reallocate descriptor offsets
                    instancingDescriptors[0].BufferOffset = 0;
                    for (int i = 1; i < instancingDescriptors.Count; i++)
                    {
                        var prev = instancingDescriptors[i - 1];

                        instancingDescriptors[i].BufferOffset = prev.BufferOffset + prev.Count;
                    }
                }
            }
            finally
            {
                Monitor.Exit(instancingDescriptors);
            }

            Instances -= instances;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[{typeof(T)}][{Dynamic}] Instances: {Instances} AllocatedSize: {AllocatedSize} ToAllocateSize: {ToAllocateSize} Dirty: {Dirty}";
        }
    }
}
