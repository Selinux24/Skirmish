
namespace Engine.Helpers
{
    /// <summary>
    /// Resource status item
    /// </summary>
    public class ResourceStatusItem
    {
        /// <summary>
        /// Size in bytes
        /// </summary>
        public long Size;
        /// <summary>
        /// Number of elements
        /// </summary>
        public int Elements;

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Size {0}; Elements {1}", this.Size, this.Elements);
        }
    }
}
