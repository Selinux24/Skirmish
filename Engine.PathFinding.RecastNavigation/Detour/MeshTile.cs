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
        /// A value that indicates the entity does not link to anything.
        /// </summary>
        public const int DT_NULL_LINK = int.MaxValue;

        /// <summary>
        /// Tile index
        /// </summary>
        public int Index { get; set; }
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
        public PolyMeshTriangleIndices[] DetailTris { get; set; }
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
        /// Gets the polygon list
        /// </summary>
        public Poly[] GetPolys()
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
        /// Gets the center of the polygon
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <returns>Returns de center position</returns>
        public Vector3 GetPolyCenter(Poly poly)
        {
            Vector3 center = Vector3.Zero;

            var verts = GetPolyVerts(poly);

            if (verts.Length == 0)
            {
                return center;
            }

            foreach (var v in verts)
            {
                center += v;
            }

            center *= 1.0f / verts.Length;

            return center;
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
                polyArea += Utils.TriArea2D(va, vb, vc);
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
            Vector3[] res = new Vector3[poly.VertCount];

            for (int j = 0; j < poly.VertCount; j++)
            {
                res[j] = Verts[poly.Verts[j]];
            }

            return res;
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

        /// <summary>
        /// Gets the off-mesh connection list
        /// </summary>
        public OffMeshConnection[] GetOffMeshConnections()
        {
            return OffMeshCons
                .Take(Header.OffMeshConCount)
                .Where(o => o != null)
                .ToArray();
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
        /// Allocates a link
        /// </summary>
        public int AllocLink()
        {
            if (LinksFreeList == DT_NULL_LINK)
            {
                return DT_NULL_LINK;
            }
            int link = LinksFreeList;
            LinksFreeList = Links[link].Next;
            return link;
        }
        /// <summary>
        /// Frees the specified link
        /// </summary>
        /// <param name="link">Link</param>
        public void FreeLink(int link)
        {
            Links[link].Next = LinksFreeList;
            LinksFreeList = link;
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

            for (int i = fromPoly.FirstLink; i != DT_NULL_LINK; i = Links[i].Next)
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
        /// <summary>
        /// Finds off-mesh connection end points
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="polyRef">Polygon reference</param>
        /// <param name="startPos">Start position</param>
        /// <param name="endPos">End position</param>
        /// <returns>Returns true if the end points were found</returns>
        public bool FindOffMeshConnectionEndpoints(Poly poly, int polyRef, out Vector3 startPos, out Vector3 endPos)
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
            for (int i = poly.FirstLink; i != DT_NULL_LINK; i = Links[i].Next)
            {
                if (Links[i].Edge == 0)
                {
                    if (Links[i].NRef != polyRef)
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

        /// <summary>
        /// Gets the specified polygon detail mesh
        /// </summary>
        /// <param name="poly">Polygon</param>
        public PolyDetail GetDetailMesh(Poly poly)
        {
            int ip = Array.IndexOf(Polys, poly);

            return DetailMeshes[ip];
        }
        /// <summary>
        /// Gets the detail triangle by index
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="vertBase">Detail vertex base index</param>
        /// <param name="tris">Triangle indexes</param>
        /// <returns>Returns the triangle</returns>
        public Triangle GetDetailTri(Poly poly, int vertBase, PolyMeshTriangleIndices tris)
        {
            Vector3[] v = new Vector3[3];

            for (int j = 0; j < 3; ++j)
            {
                if (tris[j] < poly.VertCount)
                {
                    v[j] = GetPolyVertex(poly, tris[j]);
                }
                else
                {
                    v[j] = DetailVerts[vertBase + (tris[j] - poly.VertCount)];
                }
            }

            return new Triangle(v);
        }
        /// <summary>
        /// Gets the detail triangle list
        /// </summary>
        /// <param name="p">Polygon</param>
        /// <returns>Returns a triangle array</returns>
        public List<Triangle> GetDetailTris(Poly p)
        {
            List<Triangle> tris = [];

            var pd = GetDetailMesh(p);

            for (int k = 0; k < pd.TriCount; k++)
            {
                var pmt = DetailTris[pd.TriBase + k];

                tris.Add(GetDetailTri(p, pd.VertBase, pmt));
            }

            return tris;
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
            DetailTris = new PolyMeshTriangleIndices[header.DetailTriCount];
            BvTree = new BVNode[header.BvNodeCount];
            OffMeshCons = new OffMeshConnection[header.OffMeshConCount];
        }
        /// <summary>
        /// Set mesh data
        /// </summary>
        /// <param name="data">Mesh data</param>
        public void SetData(MeshData data)
        {
            Data = data;

            if (data.NavVerts.Count > 0) Verts = [.. data.NavVerts];
            if (data.NavPolys.Count > 0) Polys = [.. data.NavPolys];

            if (data.NavDMeshes.Count > 0) DetailMeshes = [.. data.NavDMeshes];
            if (data.NavDVerts.Count > 0) DetailVerts = [.. data.NavDVerts];
            if (data.NavDTris.Count > 0) DetailTris = [.. data.NavDTris];
            if (data.NavBvtree.Count > 0) BvTree = [.. data.NavBvtree];
            if (data.OffMeshCons.Count > 0) OffMeshCons = [.. data.OffMeshCons];
        }

        /// <summary>
        /// Gets the closest point on edges
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="pos">Position</param>
        /// <param name="onlyBoundary">Use only boundaries or not</param>
        /// <param name="closest">Resulting closest point</param>
        public void ClosestPointOnDetailEdges(Poly poly, Vector3 pos, bool onlyBoundary, out Vector3 closest)
        {
            var pd = GetDetailMesh(poly);

            float dmin = float.MaxValue;
            float tmin = 0;
            var pmin = Vector3.Zero;
            var pmax = Vector3.Zero;

            int ANY_BOUNDARY_EDGE =
                ((int)DetailTriEdgeFlagTypes.Boundary << 0) |
                ((int)DetailTriEdgeFlagTypes.Boundary << 2) |
                ((int)DetailTriEdgeFlagTypes.Boundary << 4);

            for (int i = 0; i < pd.TriCount; i++)
            {
                var tris = DetailTris[pd.TriBase + i];

                if (onlyBoundary && (tris.Flags & ANY_BOUNDARY_EDGE) == 0)
                {
                    continue;
                }

                var v = GetDetailTri(poly, pd.VertBase, tris);

                for (int k = 0, j = 2; k < 3; j = k++)
                {
                    var edgeFlags = tris.GetDetailTriEdgeFlags(j);

                    if (!edgeFlags.HasFlag(DetailTriEdgeFlagTypes.Boundary) &&
                        (onlyBoundary || tris[j] < tris[k]))
                    {
                        // Only looking at boundary edges and this is internal, or
                        // this is an inner edge that we will see again or have already seen.
                        continue;
                    }

                    float d = Utils.DistancePtSegSqr2D(pos, v[j], v[k], out var t);
                    if (d < dmin)
                    {
                        dmin = d;
                        tmin = t;
                        pmin = v[j];
                        pmax = v[k];
                    }
                }
            }

            closest = Vector3.Lerp(pmin, pmax, tmin);
        }
        /// <summary>
        /// Gets the polygon height
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="pos">Position</param>
        /// <param name="height">Resulting height</param>
        /// <returns>Returns true if the height were found</returns>
        public bool GetPolyHeight(Poly poly, Vector3 pos, out float height)
        {
            height = 0;

            // Off-mesh connections do not have detail polys and getting height
            // over them does not make sense.
            if (poly.Type == PolyTypes.OffmeshConnection)
            {
                return false;
            }

            var pd = GetDetailMesh(poly);

            var verts = GetPolyVerts(poly);

            if (!Utils.PointInPolygon2D(pos, [.. verts]))
            {
                return false;
            }

            // Find height at the location.
            for (int j = 0; j < pd.TriCount; j++)
            {
                var t = DetailTris[pd.TriBase + j];
                Vector3[] v = new Vector3[3];
                for (int k = 0; k < 3; ++k)
                {
                    if (t[k] < poly.VertCount)
                    {
                        v[k] = Verts[poly.Verts[t[k]]];
                    }
                    else
                    {
                        v[k] = DetailVerts[(pd.VertBase + (t[k] - poly.VertCount))];
                    }
                }

                if (Utils.ClosestHeightPointTriangle(pos, v[0], v[1], v[2], out float h))
                {
                    height = h;
                    return true;
                }
            }

            // If all triangle checks failed above (can happen with degenerate triangles
            // or larger floating point values) the point is on an edge, so just select
            // closest. This should almost never happen so the extra iteration here is
            // ok.
            ClosestPointOnDetailEdges(poly, pos, false, out var closest);

            height = closest.Y;

            return true;
        }
        /// <summary>
        /// Calculate quantized box
        /// </summary>
        public void CalculateQuantizedBox(BoundingBox bounds, out Int3 bmin, out Int3 bmax)
        {
            var tb = Header.Bounds;
            float qfac = Header.BvQuantFactor;

            // Clamp query box to world box.
            float minx = MathUtil.Clamp(bounds.Minimum.X, tb.Minimum.X, tb.Maximum.X) - tb.Minimum.X;
            float miny = MathUtil.Clamp(bounds.Minimum.Y, tb.Minimum.Y, tb.Maximum.Y) - tb.Minimum.Y;
            float minz = MathUtil.Clamp(bounds.Minimum.Z, tb.Minimum.Z, tb.Maximum.Z) - tb.Minimum.Z;
            float maxx = MathUtil.Clamp(bounds.Maximum.X, tb.Minimum.X, tb.Maximum.X) - tb.Minimum.X;
            float maxy = MathUtil.Clamp(bounds.Maximum.Y, tb.Minimum.Y, tb.Maximum.Y) - tb.Minimum.Y;
            float maxz = MathUtil.Clamp(bounds.Maximum.Z, tb.Minimum.Z, tb.Maximum.Z) - tb.Minimum.Z;

            // Quantize
            bmin = new Int3();
            bmax = new Int3();
            bmin.X = (int)(qfac * minx) & 0xfffe;
            bmin.Y = (int)(qfac * miny) & 0xfffe;
            bmin.Z = (int)(qfac * minz) & 0xfffe;
            bmax.X = (int)(qfac * maxx + 1) | 1;
            bmax.Y = (int)(qfac * maxy + 1) | 1;
            bmax.Z = (int)(qfac * maxz + 1) | 1;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Index: {Index}; Salt: {Salt}; Links: {LinksFreeList}; Flags: {Flags}; Header: {Header}; Data: {Data}";
        }
    }
}
