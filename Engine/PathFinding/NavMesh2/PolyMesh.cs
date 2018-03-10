using SharpDX;

namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Represents a polygon mesh suitable for use in building a navigation mesh.
    /// </summary>
    public class PolyMesh
    {
        /// <summary>
        /// The mesh vertices. [Form: (x, y, z) * #nverts]
        /// </summary>
        public Trianglei[] verts;
        /// <summary>
        /// Polygon and neighbor data. [Length: #maxpolys * 2 * #nvp]
        /// </summary>
        public Polygoni[] polys;
        /// <summary>
        /// The region id assigned to each polygon. [Length: #maxpolys]
        /// </summary>
        public int[] regs;
        /// <summary>
        /// The user defined flags for each polygon. [Length: #maxpolys]
        /// </summary>
        public SamplePolyFlags[] flags;
        /// <summary>
        /// The area id assigned to each polygon. [Length: #maxpolys]
        /// </summary>
        public SamplePolyAreas[] areas;
        /// <summary>
        /// The number of vertices.
        /// </summary>
        public int nverts;
        /// <summary>
        /// The number of polygons.
        /// </summary>
        public int npolys;
        /// <summary>
        /// The number of allocated polygons.
        /// </summary>
        public int maxpolys;
        /// <summary>
        /// The maximum number of vertices per polygon.
        /// </summary>
        public int nvp;
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
        /// The AABB border size used to generate the source data from which the mesh was derived.
        /// </summary>
        public int borderSize;
        /// <summary>
        /// The max error of the polygon edges in the mesh.
        /// </summary>
        public float maxEdgeError;
    }
}
