
namespace Engine.Common
{
    /// <summary>
    /// Buffer descriptor into the BufferManager
    /// </summary>
    public class BufferDescriptor
    {
        /// <summary>
        /// Buffer slot index
        /// </summary>
        public int Slot { get; set; }
        /// <summary>
        /// Buffer index offset
        /// </summary>
        public int Offset { get; set; }
        /// <summary>
        /// Item Count
        /// </summary>
        public int Count { get; set; }

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
