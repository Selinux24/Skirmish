using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Vertex buffer descriptor request
    /// </summary>
    class BufferDescriptorRequestVertices : IBufferDescriptorRequest
    {
        /// <summary>
        /// Requester Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Data to assign
        /// </summary>
        public IEnumerable<IVertexData> Data { get; set; }
        /// <summary>
        /// Gets or sets wheter the destination buffer must be dynamic or not
        /// </summary>
        public bool Dynamic { get; set; }
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        public BufferDescriptor VertexDescriptor { get; set; } = new BufferDescriptor();
        /// <summary>
        /// Instancing buffer descriptor
        /// </summary>
        public BufferDescriptor InstancingDescriptor { get; set; } = null;
        /// <summary>
        /// Request action
        /// </summary>
        public BufferDescriptorRequestActions Action { get; set; } = BufferDescriptorRequestActions.None;
        /// <summary>
        /// Gets wheter the descriptor is processed into the buffer manager or not
        /// </summary>
        public bool Processed { get; set; } = false;

        /// <summary>
        /// Updates the buffer descriptor
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        public void Process(BufferManager bufferManager)
        {
            if (this.Action == BufferDescriptorRequestActions.Add)
            {
                Add(bufferManager);
            }
            else if (this.Action == BufferDescriptorRequestActions.Remove)
            {
                Remove(bufferManager);
            }

            this.Processed = true;
        }
        /// <summary>
        /// Adds the descriptor to the buffer manager
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        private void Add(BufferManager bufferManager)
        {
            if (this.Data?.Any() != true)
            {
                return;
            }

            VertexBufferDescription descriptor;

            VertexTypes vType = this.Data.First().VertexType;
            bool instanced = this.InstancingDescriptor != null;

            Console.WriteLine($"Add BufferDescriptor {(this.Dynamic ? "dynamic" : "static")} {vType} Instanced {instanced} [{this.Id}]");

            int slot = bufferManager.FindVertexBufferDescription(vType, this.Dynamic);
            if (slot < 0)
            {
                descriptor = new VertexBufferDescription(vType, this.Dynamic);
                slot = bufferManager.AddVertexBufferDescription(descriptor);
            }
            else
            {
                descriptor = bufferManager.GetVertexBufferDescription(slot);
                descriptor.ReallocationNeeded = true;
            }

            if (this.InstancingDescriptor != null)
            {
                //Additive only
                descriptor.InstancingDescriptor = this.InstancingDescriptor;
            }

            descriptor.AddDescriptor(this.VertexDescriptor, this.Id, slot, this.Data);
        }
        /// <summary>
        /// Removes the descriptor from de internal buffers of the buffer manager
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        private void Remove(BufferManager bufferManager)
        {
            if (this.VertexDescriptor?.Ready == true)
            {
                var descriptor = bufferManager.GetVertexBufferDescription(this.VertexDescriptor.BufferDescriptionIndex);

                Console.WriteLine($"Remove BufferDescriptor {(descriptor.Dynamic ? "dynamic" : "static")} {descriptor.Type} [{this.VertexDescriptor.Id}]");

                descriptor.RemoveDescriptor(this.VertexDescriptor);
                descriptor.ReallocationNeeded = true;
            }
        }
    }
}
