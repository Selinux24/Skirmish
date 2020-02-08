using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Vertex buffer description
    /// </summary>
    class VertexBufferDescription
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
        /// Instancing buffer descriptoy
        /// </summary>
        public BufferDescriptor InstancingDescriptor { get; set; } = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public VertexBufferDescription(VertexTypes type, bool dynamic)
        {
            this.Type = type;
            this.Dynamic = dynamic;
        }

        /// <summary>
        /// Gets the buffer format stride
        /// </summary>
        /// <returns>Returns the buffer format stride in bytes</returns>
        public int GetStride()
        {
            return this.data.FirstOrDefault()?.GetStride() ?? 0;
        }
        /// <summary>
        /// Adds the input element to the internal input list, of the specified slot
        /// </summary>
        /// <param name="slot">Buffer descriptor slot</param>
        public void AddInputs(int slot)
        {
            //Get the input element list from the vertex data
            var inputs = this.data.First().GetInput(slot);

            //Adds the input list
            this.input.AddRange(inputs);

            //Updates the allocated size
            this.AllocatedSize = this.data.Count;
        }
        /// <summary>
        /// Clears the internal input list
        /// </summary>
        public void ClearInputs()
        {
            this.input.Clear();
            this.AllocatedSize = 0;
        }
        /// <summary>
        /// Adds the specified instancing input elements to the internal list
        /// </summary>
        /// <param name="instancingSlot">Instancing buffer slot</param>
        public void AddInstancingInputs(int instancingSlot)
        {
            var instancingInputs = VertexInstancingData.Input(instancingSlot);
            this.input.AddRange(instancingInputs);
        }
        /// <summary>
        /// Crears the instancing inputs from the input elements
        /// </summary>
        public void ClearInstancingInputs()
        {
            this.input.RemoveAll(i => i.Classification == InputClassification.PerInstanceData);
        }

        /// <summary>
        /// Adds a buffer descritor to the internal descriptos list
        /// </summary>
        /// <param name="descriptor">Descriptor</param>
        /// <param name="id">Id</param>
        /// <param name="bufferDescriptionIndex">Buffer description index</param>
        /// <param name="vertices">Vertex list</param>
        public void AddDescriptor(BufferDescriptor descriptor, string id, int bufferDescriptionIndex, IEnumerable<IVertexData> vertices)
        {
            Monitor.Enter(this.data);
            //Store current data index as descriptor offset
            int offset = this.data.Count;
            //Add items to data list
            this.data.AddRange(vertices);
            Monitor.Exit(this.data);

            //Add the new descriptor to main descriptor list
            descriptor.Id = id;
            descriptor.BufferDescriptionIndex = bufferDescriptionIndex;
            descriptor.BufferOffset = offset;
            descriptor.Count = vertices.Count();

            Monitor.Enter(this.vertexDescriptors);
            this.vertexDescriptors.Add(descriptor);
            Monitor.Exit(this.vertexDescriptors);
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
                Monitor.Enter(this.data);
                this.data.RemoveRange(descriptor.BufferOffset, descriptor.Count);
                Monitor.Exit(this.data);
            }

            Monitor.Enter(this.vertexDescriptors);
            //Remove descriptor
            this.vertexDescriptors.Remove(descriptor);

            if (this.vertexDescriptors.Any())
            {
                //Reallocate descriptor offsets
                this.vertexDescriptors[0].BufferOffset = 0;
                for (int i = 1; i < this.vertexDescriptors.Count; i++)
                {
                    var prev = this.vertexDescriptors[i - 1];

                    this.vertexDescriptors[i].BufferOffset = prev.BufferOffset + prev.Count;
                }
            }
            Monitor.Exit(this.vertexDescriptors);
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns a description of the instance</returns>
        public override string ToString()
        {
            return $"[{Type}][{Dynamic}] Instances: {this.InstancingDescriptor?.Count ?? 0} AllocatedSize: {AllocatedSize} ToAllocateSize: {ToAllocateSize} Dirty: {Dirty}";
        }
    }
}
