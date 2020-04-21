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
        public Int4[] verts { get; set; }
        /// <summary>
        /// The number of vertices in the simplified contour. 
        /// </summary>
        public int nverts { get; set; }
        /// <summary>
        /// Raw contour vertex and connection data. [Size: 4 * #nrverts]
        /// </summary>
        public Int4[] rverts { get; set; }
        /// <summary>
        /// The number of vertices in the raw contour. 
        /// </summary>
        public int nrverts { get; set; }
        /// <summary>
        /// The region id of the contour.
        /// </summary>
        public int reg { get; set; }
        /// <summary>
        /// The area id of the contour.
        /// </summary>
        public AreaTypes area { get; set; }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format(
                "Region Id: {0}; Area: {1}; Simplified Verts: {2}; Raw Verts: {3};",
                this.reg, this.area, this.nverts, this.nrverts);
        }
    };
}
