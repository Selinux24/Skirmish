
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name} --> Usage {Usage}; Binding {Binding}; Size {Size}; Elements {Elements}";
        }
    }
}
