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
            base.Add(new ResourceStatusItem()
            {
                Name = name,
                Usage = usage,
                Binding = binding,
                Size = size,
                Elements = elements
            });

            this.Size = 0;
            this.Elements = 0;

            for (int i = 0; i < base.Count; i++)
            {
                this.Size += base[i].Size;
                this.Elements += base[i].Elements;
            }
        }
        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("{0}; Size {1:0.0}KB; Elements {2}", base.Count, (float)this.Size / 1024.0f, this.Elements);
        }
    }
}
