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
    public class BufferManagerInstances<T>(bool dynamic) : IEngineInstancingBufferDescriptor
        where T : struct, IInstacingData
    {
        /// <summary>
        /// Instancing descriptor list
        /// </summary>
        private readonly List<BufferDescriptor> instancingDescriptors = [];
        /// <summary>
        /// Stride
        /// </summary>
        private readonly int stride = default(T).GetStride();

        /// <inheritdoc/>
        public bool Dynamic { get; private set; } = dynamic;
        /// <inheritdoc/>
        public int BufferIndex { get; set; } = -1;
        /// <inheritdoc/>
        public int AllocatedSize { get; private set; } = 0;
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
        /// <inheritdoc/>
        public int Instances { get; set; } = 0;
        /// <inheritdoc/>
        public int BufferBindingIndex { get; set; } = -1;

        /// <inheritdoc/>
        public int GetStride()
        {
            return stride;
        }
        /// <inheritdoc/>
        public int SizeInBytes()
        {
            return Instances * stride;
        }
        /// <inheritdoc/>
        public EngineInputElement[] GetInput()
        {
            return default(T).GetInput(BufferIndex);
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
        public void RemoveDescriptor(BufferDescriptor descriptor)
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

            int instances = descriptor.Count;
            Instances -= instances;

            ReallocationNeeded = true;
        }

        /// <inheritdoc/>
        public void Allocate()
        {
            AllocatedSize = SizeInBytes();
            Allocated = true;
            Allocations++;
            ReallocationNeeded = false;
        }
        /// <inheritdoc/>
        public EngineBuffer CreateBuffer(Graphics graphics, string name)
        {
            int sizeInBytes = SizeInBytes();

            return graphics.CreateVertexBuffer(name, sizeInBytes, Dynamic);
        }
        /// <inheritdoc/>
        public IEngineInstancingBufferDescriptor Copy()
        {
            var d = new BufferManagerInstances<T>(Dynamic)
            {
                BufferIndex = BufferIndex,
                AllocatedSize = 0,
                ReallocationNeeded = true,
                Allocated = false,
                Allocations = Allocations,
                Instances = Instances,
                BufferBindingIndex = BufferBindingIndex
            };

            d.instancingDescriptors.AddRange(instancingDescriptors);

            return d;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string strDynamic = Dynamic ? "[Dynamic]" : "";
            string strInstances = Instances > 0 ? $" Instances: {Instances}" : "";

            return $"[{typeof(T)}]{strDynamic} AllocatedSize: {AllocatedSize} ToAllocateSize: {SizeInBytes()} Dirty: {Dirty}{strInstances}";
        }
    }
}
