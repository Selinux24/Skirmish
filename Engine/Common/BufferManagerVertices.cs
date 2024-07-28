using Engine.BuiltIn.Primitives;
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
    public class BufferManagerVertices(VertexTypes type, bool dynamic) : IEngineBufferDescriptor
    {
        /// <summary>
        /// Data list
        /// </summary>
        private readonly List<IVertexData> data = [];
        /// <summary>
        /// Input element list
        /// </summary>
        private readonly List<EngineInputElement> input = [];
        /// <summary>
        /// Vertex descriptor list
        /// </summary>
        private readonly List<BufferDescriptor> vertexDescriptors = [];

        /// <summary>
        /// Vertex type
        /// </summary>
        public VertexTypes Type { get; private set; } = type;
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
        /// Vertex data
        /// </summary>
        public IEnumerable<IVertexData> Data { get { return [.. data]; } }
        /// <summary>
        /// Instancing buffer descriptor
        /// </summary>
        public BufferDescriptor InstancingDescriptor { get; set; } = null;
        /// <summary>
        /// Input elements
        /// </summary>
        public IEnumerable<EngineInputElement> Input { get { return [.. input]; } }
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
            return data.FirstOrDefault()?.GetStride() ?? 0;
        }

        /// <summary>
        /// Adds the input element to the internal input list, of the specified slot
        /// </summary>
        /// <param name="slot">Buffer descriptor slot</param>
        public void AddInputs(int slot)
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
            var inputs = data[0].GetInput(slot);

            //Adds the input list
            input.AddRange(inputs);
        }
        /// <summary>
        /// Clears the internal input list
        /// </summary>
        public void ClearInputs()
        {
            input.Clear();
            AllocatedSize = 0;
        }
        /// <summary>
        /// Sets the specified instancing input elements to the internal list
        /// </summary>
        /// <param name="instancingSlot">Instancing buffer slot</param>
        public void SetInstancingInputs(int instancingSlot)
        {
            input.RemoveAll(i => i.Classification == EngineInputClassification.PerInstanceData);

            var instancingInputs = VertexInstancingData.Input(instancingSlot);

            input.AddRange(instancingInputs);
        }

        /// <summary>
        /// Adds a buffer descritor to the internal descriptors list
        /// </summary>
        /// <param name="descriptor">Descriptor</param>
        /// <param name="id">Id</param>
        /// <param name="bufferDescriptionIndex">Buffer description index</param>
        /// <param name="vertices">Vertex list</param>
        public void AddDescriptor(BufferDescriptor descriptor, string id, int bufferDescriptionIndex, IEnumerable<IVertexData> vertices)
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
        /// <summary>
        /// Removes a buffer descriptor from the internal list
        /// </summary>
        /// <param name="descriptor">Buffer descriptor to remove</param>
        public void RemoveDescriptor(BufferDescriptor descriptor)
        {
            if (descriptor.Count > 0)
            {
                //If descriptor has items, remove from buffer descriptors
                data.RemoveRange(descriptor.BufferOffset, descriptor.Count);
            }

            //Remove descriptor
            vertexDescriptors.Remove(descriptor);

            if (vertexDescriptors.Count == 0)
            {
                return;
            }

            //Reallocate descriptor offsets
            vertexDescriptors[0].BufferOffset = 0;
            for (int i = 1; i < vertexDescriptors.Count; i++)
            {
                var prev = vertexDescriptors[i - 1];

                vertexDescriptors[i].BufferOffset = prev.BufferOffset + prev.Count;
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
            var d = new BufferManagerVertices(Type, Dynamic)
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

            return $"[{Type}]{strDynamic} AllocatedSize: {AllocatedSize} ToAllocateSize: {ToAllocateSize} Dirty: {Dirty}{strInstancesDesc}";
        }
    }
}
