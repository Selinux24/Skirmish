
namespace Engine.PathFinding.NavMesh2
{
    public struct TileCachePolyMesh
    {
        public int nvp;
        /// <summary>
        /// Number of vertices.
        /// </summary>
        public int nverts;
        /// <summary>
        /// Number of polygons.
        /// </summary>
        public int npolys;
        /// <summary>
        /// Vertices of the mesh, 3 elements per vertex.
        /// </summary>
        public int[][] verts;
        /// <summary>
        /// Polygons of the mesh, nvp*2 elements per polygon.
        /// </summary>
        public int[][] polys;
        /// <summary>
        /// Per polygon flags.
        /// </summary>
        public SamplePolyFlags[] flags;
        /// <summary>
        /// Area ID of polygons.
        /// </summary>
        public SamplePolyAreas[] areas;
    }
}
