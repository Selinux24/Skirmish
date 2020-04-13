using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Represents a simple, non-overlapping contour in field space.
    /// </summary>
    public class Contour
    {
        /// <summary>
        /// Simplified contour vertex and connection data. [Size: 4 * #nverts]
        /// </summary>
        public Int4[] Verts { get; set; }
        /// <summary>
        /// The number of vertices in the simplified contour. 
        /// </summary>
        public int NVerts { get; set; }
        /// <summary>
        /// Raw contour vertex and connection data. [Size: 4 * #nrverts]
        /// </summary>
        public Int4[] RawVerts { get; set; }
        /// <summary>
        /// The number of vertices in the raw contour. 
        /// </summary>
        public int NRawVerts { get; set; }
        /// <summary>
        /// The region id of the contour.
        /// </summary>
        public int Region { get; set; }
        /// <summary>
        /// The area id of the contour.
        /// </summary>
        public AreaTypes Area { get; set; }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format(
                "Region Id: {0}; Area: {1}; Simplified Verts: {2}; Raw Verts: {3};",
                this.Region, this.Area, this.NVerts, this.NRawVerts);
        }
    };
}
