using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Mesh tile
    /// </summary>
    public class MeshTile
    {
        /// <summary>
        /// Counter describing modifications to the tile.
        /// </summary>
        public int Salt { get; set; }
        /// <summary>
        /// Index to the next free link.
        /// </summary>
        public int LinksFreeList { get; set; }
        /// <summary>
        /// The tile header.
        /// </summary>
        public MeshHeader Header { get; set; }
        /// <summary>
        /// The tile polygons. [Size: dtMeshHeader::polyCount]
        /// </summary>
        public Poly[] Polys { get; set; }
        /// <summary>
        /// The tile vertices. [Size: dtMeshHeader::vertCount]
        /// </summary>
        public Vector3[] Verts { get; set; }
        /// <summary>
        /// The tile links. [Size: dtMeshHeader::maxLinkCount]
        /// </summary>
        public Link[] Links { get; set; }
        /// <summary>
        /// The tile's detail sub-meshes. [Size: dtMeshHeader::detailMeshCount]
        /// </summary>
        public PolyDetail[] DetailMeshes { get; set; }
        /// <summary>
        /// The detail mesh's unique vertices. [(x, y, z) * dtMeshHeader::detailVertCount]
        /// </summary>
        public Vector3[] DetailVerts { get; set; }
        /// <summary>
        /// The detail mesh's triangles. [(vertA, vertB, vertC) * dtMeshHeader::detailTriCount]
        /// </summary>
        public Int4[] DetailTris { get; set; }
        /// <summary>
        /// The tile bounding volume nodes. [Size: dtMeshHeader::bvNodeCount]
        /// (Will be null if bounding volumes are disabled.)
        /// </summary>
        public BVNode[] BvTree { get; set; }
        /// <summary>
        /// The tile off-mesh connections. [Size: dtMeshHeader::offMeshConCount]
        /// </summary>
        public OffMeshConnection[] OffMeshCons { get; set; }
        /// <summary>
        /// The tile data. (Not directly accessed under normal situations.)
        /// </summary>
        public MeshData Data { get; set; }
        /// <summary>
        /// Size of the tile data.
        /// </summary>
        public int DataSize { get; set; }
        /// <summary>
        /// Tile flags. (See: #dtTileFlags)
        /// </summary>
        public TileFlagTypes Flags { get; set; }
        /// <summary>
        /// The next free tile, or the next tile in the spatial grid.
        /// </summary>
        public MeshTile Next { get; set; }
        /// <summary>
        /// Gets the specified mesh polygon area
        /// </summary>
        /// <param name="p">Polygon</param>
        /// <returns>Returns the area of the polygon</returns>
        public float GetPolyArea(Poly p)
        {
            // Calc area of the polygon.
            float polyArea = 0.0f;
            for (int j = 2; j < p.VertCount; ++j)
            {
                var va = Verts[p.Verts[0]];
                var vb = Verts[p.Verts[j - 1]];
                var vc = Verts[p.Verts[j]];
                polyArea += DetourUtils.TriArea2D(va, vb, vc);
            }

            return polyArea;
        }
        /// <summary>
        /// Gets the polygon vertices
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <returns>Returns the vertex list</returns>
        public Vector3[] GetPolyVerts(Poly poly)
        {
            Vector3[] verts = new Vector3[poly.VertCount];

            for (int j = 0; j < poly.VertCount; j++)
            {
                verts[j] = Verts[poly.Verts[j]];
            }

            return verts;
        }
        /// <summary>
        /// Find link that points to neighbour reference.
        /// </summary>
        /// <param name="fromPoly">Polygon</param>
        /// <param name="r">Neighbour reference</param>
        /// <param name="left">Left position</param>
        /// <param name="right">Right position</param>
        /// <returns>Returns the status</returns>
        public Status FindLinkToNeighbour(Poly fromPoly, int r, out Vector3 left, out Vector3 right)
        {
            left = Vector3.Zero;
            right = Vector3.Zero;

            for (int i = fromPoly.FirstLink; i != DetourUtils.DT_NULL_LINK; i = Links[i].Next)
            {
                if (Links[i].NRef == r)
                {
                    int v = Links[i].Edge;
                    left = Verts[fromPoly.Verts[v]];
                    right = Verts[fromPoly.Verts[v]];

                    return Status.DT_SUCCESS;
                }
            }

            return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
        }

        /// <summary>
        /// Patch header pointers
        /// </summary>
        /// <param name="header">Header</param>
        public void Patch(MeshHeader header)
        {
            Verts = new Vector3[header.VertCount];
            Polys = new Poly[header.PolyCount];
            Links = new Link[header.MaxLinkCount];
            DetailMeshes = new PolyDetail[header.DetailMeshCount];
            DetailVerts = new Vector3[header.DetailVertCount];
            DetailTris = new Int4[header.DetailTriCount];
            BvTree = new BVNode[header.BvNodeCount];
            OffMeshCons = new OffMeshConnection[header.OffMeshConCount];
        }
        /// <summary>
        /// Set mesh data
        /// </summary>
        /// <param name="data">Mesh data</param>
        public void SetData(MeshData data)
        {
            this.Data = data;

            if (data.NavVerts.Count > 0) Verts = data.NavVerts.ToArray();
            if (data.NavPolys.Count > 0) Polys = data.NavPolys.ToArray();

            if (data.NavDMeshes.Count > 0) DetailMeshes = data.NavDMeshes.ToArray();
            if (data.NavDVerts.Count > 0) DetailVerts = data.NavDVerts.ToArray();
            if (data.NavDTris.Count > 0) DetailTris = data.NavDTris.ToArray();
            if (data.NavBvtree.Count > 0) BvTree = data.NavBvtree.ToArray();
            if (data.OffMeshCons.Count > 0) OffMeshCons = data.OffMeshCons.ToArray();
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Salt: {0}; Links: {1}; Flags: {2}; Header: {3}; Data: {4}",
                Salt, LinksFreeList, Flags, Header, Data);
        }
    }
}
