using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Represents a group of related contours.
    /// </summary>
    public class ContourSet
    {
        /// <summary>
        /// An array of the contours in the set. [Size: #nconts]
        /// </summary>
        public Contour[] conts { get; set; }
        /// <summary>
        /// The number of contours in the set.
        /// </summary>
        public int nconts { get; set; }
        /// <summary>
        /// The minimum bounds in world space. [(x, y, z)]
        /// </summary>
        public Vector3 bmin { get; set; }
        /// <summary>
        /// The maximum bounds in world space. [(x, y, z)]
        /// </summary>
        public Vector3 bmax { get; set; }
        /// <summary>
        /// The size of each cell. (On the xz-plane.)
        /// </summary>
        public float cs { get; set; }
        /// <summary>
        /// The height of each cell. (The minimum increment along the y-axis.)
        /// </summary>
        public float ch { get; set; }
        /// <summary>
        /// The width of the set. (Along the x-axis in cell units.) 
        /// </summary>
        public int width { get; set; }
        /// <summary>
        /// The height of the set. (Along the z-axis in cell units.) 
        /// </summary>
        public int height { get; set; }
        /// <summary>
        /// The AABB border size used to generate the source data from which the contours were derived.
        /// </summary>
        public int borderSize { get; set; }
        /// <summary>
        /// The max edge error that this contour set was simplified with.
        /// </summary>
        public float maxError { get; set; }
    };
}
