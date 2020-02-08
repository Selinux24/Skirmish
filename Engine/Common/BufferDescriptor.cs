
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
        /// Buffer description index in the buffer manager
        /// </summary>
        public int BufferDescriptionIndex { get; set; } = -1;
        /// <summary>
        /// Offset in the final graphics buffer
        /// </summary>
        public int BufferOffset { get; set; } = -1;
        /// <summary>
        /// Item Count
        /// </summary>
        public int Count { get; set; } = 0;
        /// <summary>
        /// Gets wheter the descriptor is ready for use or not
        /// </summary>
        public bool Ready
        {
            get
            {
                return BufferDescriptionIndex >= 0 && BufferOffset >= 0;
            }
        }

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
            return $"Id: {Id ?? "Empty"}; Ready: {Ready} BufferDescriptionIndex: {BufferDescriptionIndex}; BufferOffset: {BufferOffset}; Count: {Count}; Next: {BufferOffset + Count};";
        }
    }
}
