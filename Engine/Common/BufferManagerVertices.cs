using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Vertex buffer description
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class BufferManagerVertices<T>(bool dynamic) : IEngineVertexBufferDescriptor
        where T : struct, IVertexData
    {
        /// <summary>
        /// Data list
        /// </summary>
        private readonly List<T> data = [];
        /// <summary>
        /// Data stride
        /// </summary>
        private readonly int stride = default(T).GetStride();
        /// <summary>
        /// Input element list
        /// </summary>
        private readonly List<EngineInputElement> input = [];
        /// <summary>
        /// Vertex descriptor list
        /// </summary>
        private readonly List<BufferDescriptor> vertexDescriptors = [];

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
        public int BufferBindingIndex { get; set; } = -1;
        /// <inheritdoc/>
        public BufferDescriptor InstancingDescriptor { get; set; } = null;

        /// <inheritdoc/>
        public int GetStride()
        {
            return stride;
        }
        /// <inheritdoc/>
        public int SizeInBytes()
        {
            return data.Count * stride;
        }
        /// <inheritdoc/>
        public EngineInputElement[] GetInput(bool instanced)
        {
            AddInputs();

            var inputElements = instanced ?
                input :
                input.FindAll(i => i.Classification != EngineInputClassification.PerInstanceData);

            return [.. inputElements];
        }

        /// <inheritdoc/>
        public bool OfType<TData>() where TData : struct, IVertexData
        {
            var thisType = typeof(T);
            var otherType = typeof(TData);

            return otherType == thisType;
        }

        /// <inheritdoc/>
        public void AddInputs()
        {
            if (data.Count == 0)
            {
                return;
            }

            if (input.Count > 0)
            {
                return;
            }

            //Get the input element list from the vertex data
            var inputs = default(T).GetInput(BufferIndex);

            //Adds the input list
            input.AddRange(inputs);
        }
        /// <inheritdoc/>
        public void ClearInputs()
        {
            input.Clear();
            AllocatedSize = 0;
        }
        /// <inheritdoc/>
        public void SetInstancingInputs(EngineInputElement[] instancingInputs)
        {
            input.RemoveAll(i => i.Classification == EngineInputClassification.PerInstanceData);

            input.AddRange(instancingInputs);
        }

        /// <summary>
        /// Adds a buffer descritor to the internal descriptors list
        /// </summary>
        /// <param name="descriptor">Descriptor</param>
        /// <param name="id">Id</param>
        /// <param name="bufferDescriptionIndex">Buffer description index</param>
        /// <param name="vertices">Vertex list</param>
        public void AddDescriptor(BufferDescriptor descriptor, string id, int bufferDescriptionIndex, IEnumerable<T> vertices)
        {
            int offset;

            //Store current data index as descriptor offset
            offset = data.Count;
            //Add items to data list
            data.AddRange(vertices);

            //Add the new descriptor to main descriptor list
            descriptor.Id = id;
            descriptor.BufferDescriptionIndex = bufferDescriptionIndex;
            descriptor.BufferOffset = offset;
            descriptor.Count = vertices.Count();

            vertexDescriptors.Add(descriptor);
        }
        /// <inheritdoc/>
        public void RemoveDescriptor(BufferDescriptor descriptor)
        {
            if (descriptor.Count > 0)
            {
                //If descriptor has items, remove from buffer descriptors
                data.RemoveRange(descriptor.BufferOffset, descriptor.Count);
            }

            //Remove descriptor
            vertexDescriptors.Remove(descriptor);

            if (vertexDescriptors.Count != 0)
            {
                //Reallocate descriptor offsets
                vertexDescriptors[0].BufferOffset = 0;
                for (int i = 1; i < vertexDescriptors.Count; i++)
                {
                    var prev = vertexDescriptors[i - 1];

                    vertexDescriptors[i].BufferOffset = prev.BufferOffset + prev.Count;
                }
            }

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
            return graphics.CreateVertexBuffer(name, data, Dynamic);
        }
        /// <inheritdoc/>
        public IEngineVertexBufferDescriptor Copy()
        {
            var d = new BufferManagerVertices<T>(Dynamic)
            {
                BufferIndex = BufferIndex,
                AllocatedSize = 0,
                ReallocationNeeded = true,
                Allocated = false,
                Allocations = Allocations,
                BufferBindingIndex = BufferBindingIndex,
                InstancingDescriptor = InstancingDescriptor,
            };

            d.data.AddRange(data);
            d.input.AddRange(input);
            d.vertexDescriptors.AddRange(vertexDescriptors);

            return d;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string strDynamic = Dynamic ? "[Dynamic]" : "";
            string strInstancesDesc = (InstancingDescriptor?.Count ?? 0) > 0 ? $" Instances: {InstancingDescriptor.Id}|{InstancingDescriptor.Count}" : "";

            return $"[{strDynamic} AllocatedSize: {AllocatedSize} ToAllocateSize: {SizeInBytes()} Dirty: {Dirty}{strInstancesDesc}";
        }
    }
}
