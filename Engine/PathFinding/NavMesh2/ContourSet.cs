using SharpDX;

namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Represents a group of related contours.
    /// </summary>
    public class ContourSet
    {
        /// <summary>
        /// An array of the contours in the set. [Size: #nconts]
        /// </summary>
        public Contour[] conts;
        /// <summary>
        /// The number of contours in the set.
        /// </summary>
        public int nconts;
        /// <summary>
        /// The minimum bounds in world space. [(x, y, z)]
        /// </summary>
        public Vector3 bmin;
        /// <summary>
        /// The maximum bounds in world space. [(x, y, z)]
        /// </summary>
        public Vector3 bmax;
        /// <summary>
        /// The size of each cell. (On the xz-plane.)
        /// </summary>
        public float cs;
        /// <summary>
        /// The height of each cell. (The minimum increment along the y-axis.)
        /// </summary>
        public float ch;
        /// <summary>
        /// The width of the set. (Along the x-axis in cell units.) 
        /// </summary>
        public int width;
        /// <summary>
        /// The height of the set. (Along the z-axis in cell units.) 
        /// </summary>
        public int height;
        /// <summary>
        /// The AABB border size used to generate the source data from which the contours were derived.
        /// </summary>
        public int borderSize;
        /// <summary>
        /// The max edge error that this contour set was simplified with.
        /// </summary>
        public float maxError;
    };
}
