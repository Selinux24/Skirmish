using System.Collections.Generic;

namespace Engine.Helpers
{
    /// <summary>
    /// Resource status class
    /// </summary>
    class ResourceStatus : List<ResourceStatusItem>
    {
        /// <summary>
        /// Total resources size
        /// </summary>
        public long Size { get; private set; }
        /// <summary>
        /// Total resources elements
        /// </summary>
        public int Elements { get; private set; }

        /// <summary>
        /// Adds a new resource status item
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="usage">Resource usage</param>
        /// <param name="binding">Resource binding</param>
        /// <param name="size">Size in bytes</param>
        /// <param name="elements">Number of elements</param>
        public void Add(string name, int usage, int binding, long size, int elements)
        {
            Add(new ResourceStatusItem()
            {
                Name = name,
                Usage = usage,
                Binding = binding,
                Size = size,
                Elements = elements
            });

            Size = 0;
            Elements = 0;

            for (int i = 0; i < Count; i++)
            {
                Size += base[i]?.Size ?? 0;
                Elements += base[i]?.Elements ?? 0;
            }
        }
        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return $"{Count}; Size {Size / 1024.0f:0.0}KB; Elements {Elements}";
        }
    }
}
