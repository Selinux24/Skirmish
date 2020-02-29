using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Represents a polygon mesh suitable for use in building a navigation mesh.
    /// </summary>
    public class PolyMesh
    {
        /// <summary>
        /// The mesh vertices. [Form: (x, y, z) * #nverts]
        /// </summary>
        public Int3[] Verts { get; set; }
        /// <summary>
        /// Polygon and neighbor data. [Length: #maxpolys * 2 * #nvp]
        /// </summary>
        public Polygoni[] Polys { get; set; }
        /// <summary>
        /// The region id assigned to each polygon. [Length: #maxpolys]
        /// </summary>
        public int[] Regs { get; set; }
        /// <summary>
        /// The user defined flags for each polygon. [Length: #maxpolys]
        /// </summary>
        public SamplePolyFlagTypes[] Flags { get; set; }
        /// <summary>
        /// The area id assigned to each polygon. [Length: #maxpolys]
        /// </summary>
        public SamplePolyAreas[] Areas { get; set; }
        /// <summary>
        /// The number of vertices.
        /// </summary>
        public int NVerts { get; set; }
        /// <summary>
        /// The number of polygons.
        /// </summary>
        public int NPolys { get; set; }
        /// <summary>
        /// The number of allocated polygons.
        /// </summary>
        public int MaxPolys { get; set; }
        /// <summary>
        /// The maximum number of vertices per polygon.
        /// </summary>
        public int NVP { get; set; }
        /// <summary>
        /// The minimum bounds in world space. [(x, y, z)]
        /// </summary>
        public Vector3 BMin { get; set; }
        /// <summary>
        /// The maximum bounds in world space. [(x, y, z)]
        /// </summary>
        public Vector3 BMax { get; set; }
        /// <summary>
        /// The size of each cell. (On the xz-plane.)
        /// </summary>
        public float CS { get; set; }
        /// <summary>
        /// The height of each cell. (The minimum increment along the y-axis.)
        /// </summary>
        public float CH { get; set; }
        /// <summary>
        /// The AABB border size used to generate the source data from which the mesh was derived.
        /// </summary>
        public int BorderSize { get; set; }
        /// <summary>
        /// The max error of the polygon edges in the mesh.
        /// </summary>
        public float MaxEdgeError { get; set; }
    }
}
