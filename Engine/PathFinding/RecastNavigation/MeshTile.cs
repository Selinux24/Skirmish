using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Mesh tile
    /// </summary>
    public class MeshTile
    {
        /// <summary>
        /// Counter describing modifications to the tile.
        /// </summary>
        public int salt;
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
        public Vector3[] verts;
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
        public Vector3[] detailVerts;
        /// <summary>
        /// The detail mesh's triangles. [(vertA, vertB, vertC) * dtMeshHeader::detailTriCount]
        /// </summary>
        public Int4[] detailTris;
        /// <summary>
        /// The tile bounding volume nodes. [Size: dtMeshHeader::bvNodeCount]
        /// (Will be null if bounding volumes are disabled.)
        /// </summary>
        public BVNode[] bvTree;
        /// <summary>
        /// The tile off-mesh connections. [Size: dtMeshHeader::offMeshConCount]
        /// </summary>
        public OffMeshConnection[] offMeshCons;
        /// <summary>
        /// The tile data. (Not directly accessed under normal situations.)
        /// </summary>
        public MeshData data;
        /// <summary>
        /// Size of the tile data.
        /// </summary>
        public int dataSize;
        /// <summary>
        /// Tile flags. (See: #dtTileFlags)
        /// </summary>
        public TileFlagTypes flags;
        /// <summary>
        /// The next free tile, or the next tile in the spatial grid.
        /// </summary>
        public MeshTile next;

        /// <summary>
        /// Patch header pointers
        /// </summary>
        /// <param name="header">Header</param>
        public void Patch(MeshHeader header)
        {
            verts = new Vector3[header.vertCount];
            polys = new Poly[header.polyCount];
            links = new Link[header.maxLinkCount];
            detailMeshes = new PolyDetail[header.detailMeshCount];
            detailVerts = new Vector3[header.detailVertCount];
            detailTris = new Int4[header.detailTriCount];
            bvTree = new BVNode[header.bvNodeCount];
            offMeshCons = new OffMeshConnection[header.offMeshConCount];
        }
        /// <summary>
        /// Set mesh data
        /// </summary>
        /// <param name="data">Mesh data</param>
        public void SetData(MeshData data)
        {
            this.data = data;

            if (data.NavVerts.Count > 0) verts = data.NavVerts.ToArray();
            if (data.NavPolys.Count > 0) polys = data.NavPolys.ToArray();

            if (data.NavDMeshes.Count > 0) detailMeshes = data.NavDMeshes.ToArray();
            if (data.NavDVerts.Count > 0) detailVerts = data.NavDVerts.ToArray();
            if (data.NavDTris.Count > 0) detailTris = data.NavDTris.ToArray();
            if (data.NavBvtree.Count > 0) bvTree = data.NavBvtree.ToArray();
            if (data.OffMeshCons.Count > 0) offMeshCons = data.OffMeshCons.ToArray();
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Salt: {0}; Links: {1}; Flags: {2}; Header: {3}; Data: {4}",
                salt, linksFreeList, flags, header, data);
        }
    }
}
