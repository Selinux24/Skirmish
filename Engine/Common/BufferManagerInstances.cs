using System.Collections.Generic;

namespace Engine.Common
{
    /// <summary>
    /// Vertex buffer description
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class BufferManagerInstances<T>(bool dynamic) : IEngineBufferDescriptor
        where T : struct, IInstacingData
    {
        /// <summary>
        /// Instancing descriptor list
        /// </summary>
        private readonly List<BufferDescriptor> instancingDescriptors = [];

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
                return Instances;
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
        /// Instances
        /// </summary>
        public int Instances { get; set; } = 0;
        /// <summary>
        /// Vertex buffer binding index in the manager list
        /// </summary>
        public int BufferBindingIndex { get; set; } = -1;

        /// <summary>
        /// Gets the buffer format stride
        /// </summary>
        /// <returns>Returns the buffer format stride in bytes</returns>
        public int GetStride()
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

            instancingDescriptors.Add(descriptor);
        }
        /// <summary>
        /// Removes a buffer descriptor from the internal list
        /// </summary>
        /// <param name="descriptor">Buffer descriptor to remove</param>
        /// <param name="instances">Number of instances</param>
        public void RemoveDescriptor(BufferDescriptor descriptor, int instances)
        {
            //Remove descriptor
            instancingDescriptors.Remove(descriptor);

            if (instancingDescriptors.Count != 0)
            {
                //Reallocate descriptor offsets
                instancingDescriptors[0].BufferOffset = 0;
                for (int i = 1; i < instancingDescriptors.Count; i++)
                {
                    var prev = instancingDescriptors[i - 1];

                    instancingDescriptors[i].BufferOffset = prev.BufferOffset + prev.Count;
                }
            }

            Instances -= instances;
        }

        /// <inheritdoc/>
        public void Allocate()
        {
            AllocatedSize = Instances;
            Allocated = true;
            Allocations++;
            ReallocationNeeded = false;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string strDynamic = Dynamic ? "[Dynamic]" : "";
            string strInstances = Instances > 0 ? $" Instances: {Instances}" : "";

            return $"[{typeof(T)}]{strDynamic} AllocatedSize: {AllocatedSize} ToAllocateSize: {ToAllocateSize} Dirty: {Dirty}{strInstances}";
        }
    }
}
