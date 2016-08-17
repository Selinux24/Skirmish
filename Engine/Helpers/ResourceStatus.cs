using System.Collections.Generic;

namespace Engine.Helpers
{
    /// <summary>
    /// Resource status class
    /// </summary>
    public class ResourceStatus : List<ResourceStatusItem>
    {
        /// <summary>
        /// Adds a new resource status item
        /// </summary>
        /// <param name="size">Size in bytes</param>
        /// <param name="elements">Number of elements</param>
        public void Add(long size, int elements)
        {
            base.Add(new ResourceStatusItem() { Size = size, Elements = elements });
        }
        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            long size = 0;
            int elements = 0;
            for (int i = 0; i < base.Count; i++)
            {
                size += base[i].Size;
                elements += base[i].Elements;
            }

            return string.Format("{0}; Size {1:0.0}KB; Elements {2}", base.Count, (float)size / 1024.0f, elements);
        }
    }
}
