using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

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


        public IEnumerable<Poly> GetPolys()
        {
            return Polys.Take(Header.PolyCount).ToArray();
        }
        /// <summary>
        /// Gets the polygon specified by index
        /// </summary>
        /// <param name="index">Polygon index</param>
        /// <returns>Returns the polygon</returns>
        public Poly GetPoly(int index)
        {
            return Polys[index];
        }
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
        public IEnumerable<Vector3> GetPolyVerts(Poly poly)
        {
            Vector3[] verts = new Vector3[poly.VertCount];

            for (int j = 0; j < poly.VertCount; j++)
            {
                verts[j] = Verts[poly.Verts[j]];
            }

            return verts;
        }
        /// <summary>
        /// Calculates the bounds of the Polygon
        /// </summary>
        /// <param name="p">Polygon</param>
        /// <returns>Returns a bounding box</returns>
        public BoundingBox GetPolyBounds(Poly p)
        {
            var v = Verts[p.Verts[0]];
            Vector3 bmin = v;
            Vector3 bmax = v;
            for (int j = 1; j < p.VertCount; ++j)
            {
                v = Verts[p.Verts[j]];
                bmin = Vector3.Min(bmin, v);
                bmax = Vector3.Max(bmax, v);
            }

            return new BoundingBox(bmin, bmax);
        }
        /// <summary>
        /// Gets the polygon vertex by Index
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="index">Index</param>
        /// <returns>Returns the polygon vertex</returns>
        public Vector3 GetPolyVertex(Poly poly, int index)
        {
            return Verts[poly.Verts[index]];
        }
        /// <summary>
        /// Sets the vertex position
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="index">Index</param>
        /// <param name="v">The new vertex value</param>
        public void SetPolyVertex(Poly poly, int index, Vector3 v)
        {
            Verts[poly.Verts[index]] = v;
        }


        public IEnumerable<OffMeshConnection> GetOffMeshConnections()
        {
            return OffMeshCons.Take(Header.OffMeshConCount).ToArray();
        }
        /// <summary>
        /// Gets the off-mesh connection by off-mesh connection index
        /// </summary>
        /// <param name="index">Off-mesh connection index</param>
        /// <returns>Returns the off-mesh connection</returns>
        public OffMeshConnection GetOffMeshConnection(int index)
        {
            return OffMeshCons[index];
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
        /// Gets whether the polygon is a off-mesh connection or not 
        /// </summary>
        /// <param name="index">Polygon index</param>
        /// <returns>Returns true if the polygon is a off-mesh connection</returns>
        public bool IsOffMeshConnectionByPolygon(int index)
        {
            if (index >= Header.PolyCount)
            {
                return false;
            }

            return Polys[index].Type == PolyTypes.OffmeshConnection;
        }
        /// <summary>
        /// Gets the off-mesh connection by polygon index
        /// </summary>
        /// <param name="index">Polygon index</param>
        /// <returns>Returns the off.mesh connection, or null if the polygon is not a off-mesh connection</returns>
        public OffMeshConnection GetOffMeshConnectionByPolygon(int index)
        {
            // Make sure that the current poly is indeed off-mesh link.
            if (!IsOffMeshConnectionByPolygon(index))
            {
                return null;
            }

            return OffMeshCons[index - Header.OffMeshBase];
        }

        public bool FindOffMeshConnectionEndpoints(Poly poly, int prevRef, out Vector3 startPos, out Vector3 endPos)
        {
            startPos = Vector3.Zero;
            endPos = Vector3.Zero;

            // Make sure that the current poly is indeed off-mesh link.
            if (poly.Type != PolyTypes.OffmeshConnection)
            {
                return false;
            }

            int idx0 = 0;
            int idx1 = 1;

            // Find link that points to first vertex.
            for (int i = poly.FirstLink; i != DetourUtils.DT_NULL_LINK; i = Links[i].Next)
            {
                if (Links[i].Edge == 0)
                {
                    if (Links[i].NRef != prevRef)
                    {
                        idx0 = 1;
                        idx1 = 0;
                    }
                    break;
                }
            }

            startPos = GetPolyVertex(poly, idx0);
            endPos = GetPolyVertex(poly, idx1);

            return true;
        }


        public PolyDetail GetDetailMesh(Poly poly)
        {
            int ip = Array.IndexOf(Polys, poly);
            return DetailMeshes[ip];
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
