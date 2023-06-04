using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Vertex buffer description
    /// </summary>
    public class BufferManagerVertices
    {
        /// <summary>
        /// Data list
        /// </summary>
        private readonly List<IVertexData> data = new List<IVertexData>();
        /// <summary>
        /// Input element list
        /// </summary>
        private readonly List<InputElement> input = new List<InputElement>();
        /// <summary>
        /// Vertex descriptor list
        /// </summary>
        private readonly List<BufferDescriptor> vertexDescriptors = new List<BufferDescriptor>();

        /// <summary>
        /// Vertex type
        /// </summary>
        public readonly VertexTypes Type;
        /// <summary>
        /// Dynamic buffer
        /// </summary>
        public readonly bool Dynamic;
        /// <summary>
        /// Vertex data
        /// </summary>
        public IEnumerable<IVertexData> Data { get { return data.ToArray(); } }
        /// <summary>
        /// Input elements
        /// </summary>
        public IEnumerable<InputElement> Input { get { return input.ToArray(); } }
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
        /// Instancing buffer descriptoy
        /// </summary>
        public BufferDescriptor InstancingDescriptor { get; set; } = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public BufferManagerVertices(VertexTypes type, bool dynamic)
        {
            Type = type;
            Dynamic = dynamic;
        }

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
            //Get the input element list from the vertex data
            var inputs = data[0].GetInput(slot);

            //Adds the input list
            input.AddRange(inputs);

            //Updates the allocated size
            AllocatedSize = data.Count;
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
        /// Adds the specified instancing input elements to the internal list
        /// </summary>
        /// <param name="instancingSlot">Instancing buffer slot</param>
        public void AddInstancingInputs(int instancingSlot)
        {
            var instancingInputs = VertexInstancingData.Input(instancingSlot);
            input.AddRange(instancingInputs);
        }
        /// <summary>
        /// Crears the instancing inputs from the input elements
        /// </summary>
        public void ClearInstancingInputs()
        {
            input.RemoveAll(i => i.Classification == InputClassification.PerInstanceData);
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

            Monitor.Enter(data);
            try
            {
                //Store current data index as descriptor offset
                offset = data.Count;
                //Add items to data list
                data.AddRange(vertices);
            }
            finally
            {
                Monitor.Exit(data);
            }

            //Add the new descriptor to main descriptor list
            descriptor.Id = id;
            descriptor.BufferDescriptionIndex = bufferDescriptionIndex;
            descriptor.BufferOffset = offset;
            descriptor.Count = vertices.Count();

            Monitor.Enter(vertexDescriptors);
            try
            {
                vertexDescriptors.Add(descriptor);
            }
            finally
            {
                Monitor.Exit(vertexDescriptors);
            }
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
                Monitor.Enter(data);
                try
                {
                    data.RemoveRange(descriptor.BufferOffset, descriptor.Count);
                }
                finally
                {
                    Monitor.Exit(data);
                }
            }

            Monitor.Enter(vertexDescriptors);
            try
            {
                //Remove descriptor
                vertexDescriptors.Remove(descriptor);

                if (!vertexDescriptors.Any())
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
            finally
            {
                Monitor.Exit(vertexDescriptors);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[{Type}][{Dynamic}] Instances: {InstancingDescriptor?.Count ?? 0} AllocatedSize: {AllocatedSize} ToAllocateSize: {ToAllocateSize} Dirty: {Dirty}";
        }
    }
}
