
namespace Engine.Helpers
{
    /// <summary>
    /// Resource status item
    /// </summary>
    class ResourceStatusItem
    {
        /// <summary>
        /// Resource name
        /// </summary>
        public string Name;
        /// <summary>
        /// Resource usage
        /// </summary>
        public int Usage;
        /// <summary>
        /// Resource binding
        /// </summary>
        public int Binding;
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
            return string.Format(
                "{0} --> Usage {1}; Binding {2}; Size {3}; Elements {4}", 
                this.Name,
                this.Usage,
                this.Binding,
                this.Size, 
                this.Elements);
        }
    }
}
