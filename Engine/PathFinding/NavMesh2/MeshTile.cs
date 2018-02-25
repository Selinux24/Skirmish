
namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Mesh tile
    /// </summary>
    public class MeshTile
    {
        /// <summary>
        /// Counter describing modifications to the tile.
        /// </summary>
        public uint salt;
        /// <summary>
        /// Index to the next free link.
        /// </summary>
        public int linksFreeList;
        /// <summary>
        /// The tile header.
        /// </summary>
        public MeshHeader header;
        /// <summary>
        /// The tile polygons. [Size: dtMeshHeader::polyCount]
        /// </summary>
        public Poly[] polys;
        /// <summary>
        /// The tile vertices. [Size: dtMeshHeader::vertCount]
        /// </summary>
        public float[] verts;
        /// <summary>
        /// The tile links. [Size: dtMeshHeader::maxLinkCount]
        /// </summary>
        public Link[] links;
        /// <summary>
        /// The tile's detail sub-meshes. [Size: dtMeshHeader::detailMeshCount]
        /// </summary>
        public PolyDetail[] detailMeshes;

        /// <summary>
        /// The detail mesh's unique vertices. [(x, y, z) * dtMeshHeader::detailVertCount]
        /// </summary>
        public float[] detailVerts;

        /// <summary>
        /// The detail mesh's triangles. [(vertA, vertB, vertC) * dtMeshHeader::detailTriCount]
        /// </summary>
        public int[] detailTris;

        /// <summary>
        /// The tile bounding volume nodes. [Size: dtMeshHeader::bvNodeCount]
        /// (Will be null if bounding volumes are disabled.)
        /// </summary>
        public BVNode bvTree;
        /// <summary>
        /// The tile off-mesh connections. [Size: dtMeshHeader::offMeshConCount]
        /// </summary>
        public OffMeshConnection[] offMeshCons;
        /// <summary>
        /// The tile data. (Not directly accessed under normal situations.)
        /// </summary>
        public byte[] data;
        /// <summary>
        /// Size of the tile data.
        /// </summary>
        public int dataSize;
        /// <summary>
        /// Tile flags. (See: #dtTileFlags)
        /// </summary>
        public int flags;
        /// <summary>
        /// The next free tile, or the next tile in the spatial grid.
        /// </summary>
        public MeshTile next;
    }
}
