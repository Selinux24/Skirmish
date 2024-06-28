using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Vertex buffer descriptor request
    /// </summary>
    class BufferDescriptorRequestVertices : IBufferDescriptorRequest
    {
        /// <inheritdoc/>
        public string Id { get; set; }
        /// <inheritdoc/>
        public bool Dynamic { get; set; }
        /// <inheritdoc/>
        public BufferDescriptorRequestActions Action { get; set; } = BufferDescriptorRequestActions.None;
        /// <inheritdoc/>
        public ProcessedStages Processed { get; set; } = ProcessedStages.Requested;
        /// <summary>
        /// Data to assign
        /// </summary>
        public IEnumerable<IVertexData> Data { get; set; }
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        public BufferDescriptor VertexDescriptor { get; set; } = new();
        /// <summary>
        /// Instancing buffer descriptor
        /// </summary>
        public BufferDescriptor InstancingDescriptor { get; set; } = null;

        /// <inheritdoc/>
        public void Process(BufferManager bufferManager)
        {
            Processed = ProcessedStages.InProcess;

            if (Action == BufferDescriptorRequestActions.Add)
            {
                Add(bufferManager);
            }
            else if (Action == BufferDescriptorRequestActions.Remove)
            {
                Remove(bufferManager);
            }

            Processed = ProcessedStages.Processed;
        }
        /// <summary>
        /// Adds the descriptor to the buffer manager
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        private void Add(BufferManager bufferManager)
        {
            if (Data?.Any() != true)
            {
                Logger.WriteTrace(this, $"Add BufferDescriptor {(Dynamic ? "dynamic" : "static")} discarded no data [{Id}]");
                return;
            }

            BufferManagerVertices descriptor;

            VertexTypes vType = Data.First().VertexType;
            bool instanced = InstancingDescriptor != null;

            Logger.WriteTrace(this, $"Add BufferDescriptor {(Dynamic ? "dynamic" : "static")} {vType} Instanced {instanced} [{Id}]");

            int slot = bufferManager.FindVertexBufferDescription(vType, Dynamic);
            if (slot < 0)
            {
                descriptor = new BufferManagerVertices(vType, Dynamic);
                slot = bufferManager.AddVertexBufferDescription(descriptor);
            }
            else
            {
                descriptor = bufferManager.GetVertexBufferDescription(slot);
                descriptor.ReallocationNeeded = true;
            }

            if (InstancingDescriptor != null)
            {
                //Additive only
                descriptor.InstancingDescriptor = InstancingDescriptor;
            }

            descriptor.AddDescriptor(VertexDescriptor, Id, slot, Data);
        }
        /// <summary>
        /// Removes the descriptor from de internal buffers of the buffer manager
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        private void Remove(BufferManager bufferManager)
        {
            if (VertexDescriptor?.Ready == true)
            {
                var descriptor = bufferManager.GetVertexBufferDescription(VertexDescriptor.BufferDescriptionIndex);

                Logger.WriteTrace(this, $"Remove BufferDescriptor {(descriptor.Dynamic ? "dynamic" : "static")} {descriptor.Type} [{VertexDescriptor.Id}]");

                descriptor.RemoveDescriptor(VertexDescriptor);
                descriptor.ReallocationNeeded = true;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Id} => {Action} {Processed}; {VertexDescriptor}";
        }
    }
}
