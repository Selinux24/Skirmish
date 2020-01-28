
namespace Engine.Common
{
    /// <summary>
    /// Buffer descriptor into the BufferManager
    /// </summary>
    public class BufferDescriptor
    {
        /// <summary>
        /// Buffer Id
        /// </summary>
        public string Id { get; set; } = null;
        /// <summary>
        /// Buffer slot index
        /// </summary>
        public int Slot { get; set; } = -1;
        /// <summary>
        /// Buffer index offset
        /// </summary>
        public int Offset { get; set; } = -1;
        /// <summary>
        /// Item Count
        /// </summary>
        public int Count { get; set; } = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public BufferDescriptor()
        {

        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return $"Id: {Id ?? "Empty"}; Slot: {Slot}; Offset: {Offset}; Count: {Count}; Next: {Offset + Count};";
        }
    }
}
