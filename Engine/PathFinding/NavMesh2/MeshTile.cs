using SharpDX;
using System;

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
        public TileFlags flags;
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

        public void SetData(MeshData data)
        {
            if (data.navVerts.Count > 0) Array.Copy(data.navVerts.ToArray(), verts, data.navVerts.Count);
            if (data.navPolys.Count > 0) Array.Copy(data.navPolys.ToArray(), polys, data.navPolys.Count);

            if (data.navDMeshes.Count > 0) Array.Copy(data.navDMeshes.ToArray(), detailMeshes, data.navDMeshes.Count);
            if (data.navDVerts.Count > 0) Array.Copy(data.navDVerts.ToArray(), detailVerts, data.navDVerts.Count);
            if (data.navDTris.Count > 0) Array.Copy(data.navDTris.ToArray(), detailTris, data.navDTris.Count);
            if (data.navBvtree.Count > 0) Array.Copy(data.navBvtree.ToArray(), bvTree, data.navBvtree.Count);
            if (data.offMeshCons.Count > 0) Array.Copy(data.offMeshCons.ToArray(), offMeshCons, data.offMeshCons.Count);
        }
    }
}
