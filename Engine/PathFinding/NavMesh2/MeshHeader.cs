using SharpDX;

namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Provides high level information related to a dtMeshTile object.
    /// </summary>
    public struct MeshHeader
    {
        /// <summary>
        /// Tile magic number. (Used to identify the data format.)
        /// </summary>
        public int magic;
        /// <summary>
        /// Tile data format version number.
        /// </summary>
        public int version;
        /// <summary>
        /// The x-position of the tile within the dtNavMesh tile grid. (x, y, layer)
        /// </summary>
        public int x;
        /// <summary>
        /// The y-position of the tile within the dtNavMesh tile grid. (x, y, layer)
        /// </summary>
        public int y;
        /// <summary>
        /// The layer of the tile within the dtNavMesh tile grid. (x, y, layer)
        /// </summary>
        public int layer;
        /// <summary>
        /// The user defined id of the tile.
        /// </summary>
        public int userId;
        /// <summary>
        /// The number of polygons in the tile.
        /// </summary>
        public int polyCount;
        /// <summary>
        /// The number of vertices in the tile.
        /// </summary>
        public int vertCount;
        /// <summary>
        /// The number of allocated links.
        /// </summary>
        public int maxLinkCount;
        /// <summary>
        /// The number of sub-meshes in the detail mesh.
        /// </summary>
        public int detailMeshCount;

        /// <summary>
        /// The number of unique vertices in the detail mesh. (In addition to the polygon vertices.)
        /// </summary>
        public int detailVertCount;
        /// <summary>
        /// The number of triangles in the detail mesh.
        /// </summary>
        public int detailTriCount;
        /// <summary>
        /// The number of bounding volume nodes. (Zero if bounding volumes are disabled.)
        /// </summary>
        public int bvNodeCount;
        /// <summary>
        /// The number of off-mesh connections.
        /// </summary>
        public int offMeshConCount;
        /// <summary>
        /// The index of the first polygon which is an off-mesh connection.
        /// </summary>
        public int offMeshBase;
        /// <summary>
        /// The height of the agents using the tile.
        /// </summary>
        public float walkableHeight;
        /// <summary>
        /// The radius of the agents using the tile.
        /// </summary>
        public float walkableRadius;
        /// <summary>
        /// The maximum climb height of the agents using the tile.
        /// </summary>
        public float walkableClimb;
        /// <summary>
        /// The minimum bounds of the tile's AABB. [(x, y, z)]
        /// </summary>
        public Vector3 bmin;
        /// <summary>
        /// The maximum bounds of the tile's AABB. [(x, y, z)]
        /// </summary>
        public Vector3 bmax;

        /// <summary>
        /// The bounding volume quantization factor.
        /// </summary>
        public float bvQuantFactor;
    };
}
