namespace Engine.Common
{
    /// <summary>
    /// Vertex buffer description
    /// </summary>
    public class BufferDescriptor
    {
        /// <summary>
        /// Vertex buffer slot
        /// </summary>
        public int Slot;
        /// <summary>
        /// Vertex buffer offset
        /// </summary>
        public int Offset;
        /// <summary>
        /// Vertices count
        /// </summary>
        public int Count;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        public BufferDescriptor(int slot, int offset, int count)
        {
            this.Slot = slot;
            this.Offset = offset;
            this.Count = count;
        }
    }
}
