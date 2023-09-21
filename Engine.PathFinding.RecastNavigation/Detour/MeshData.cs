using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    using Engine.PathFinding.RecastNavigation.Recast;

    /// <summary>
    /// Mesh data
    /// </summary>
    [Serializable]
    public class MeshData
    {
        /// <summary>
        /// Mesh header
        /// </summary>
        public MeshHeader Header { get; set; }
        /// <summary>
        /// Navigation vertices
        /// </summary>
        public List<Vector3> NavVerts { get; set; } = new List<Vector3>();
        /// <summary>
        /// Navigation polygons
        /// </summary>
        public List<Poly> NavPolys { get; set; } = new List<Poly>();
        /// <summary>
        /// Navigation detail meshes
        /// </summary>
        public List<PolyDetail> NavDMeshes { get; set; } = new List<PolyDetail>();
        /// <summary>
        /// Navigation detail vertices
        /// </summary>
        public List<Vector3> NavDVerts { get; set; } = new List<Vector3>();
        /// <summary>
        /// Navigation detail triangles
        /// </summary>
        public List<PolyMeshTriangleIndices> NavDTris { get; set; } = new List<PolyMeshTriangleIndices>();
        /// <summary>
        /// Navigation BVTree
        /// </summary>
        public List<BVNode> NavBvtree { get; set; } = new List<BVNode>();
        /// <summary>
        /// Off-mesh connections
        /// </summary>
        public List<OffMeshConnection> OffMeshCons { get; set; } = new List<OffMeshConnection>();

        public static MeshData CreateNavMeshData(NavMeshCreateParams param)
        {
            if (param.Nvp > NavMeshCreateParams.DT_VERTS_PER_POLYGON ||
                param.VertCount == 0 || param.Verts == null ||
                param.PolyCount == 0 || param.Polys == null)
            {
                return null;
            }

            // Classify off-mesh connection points. 
            // We store only the connections whose start point is inside the tile.
            ClassifyOffMeshConnections(param, out var offMeshConClass, out int storedOffMeshConCount, out int offMeshConLinkCount);

            // Off-mesh connectionss are stored as polygons, adjust values.
            int totPolyCount = param.PolyCount + storedOffMeshConCount;
            int totVertCount = param.VertCount + storedOffMeshConCount * 2;

            // Find portal edges which are at tile borders.
            FindPortalEdges(param, out int edgeCount, out int portalCount);

            int maxLinkCount = edgeCount + portalCount * 2 + offMeshConLinkCount * 2;

            // Find unique detail vertices.
            FindUniqueDetailVertices(param, out int uniqueDetailVertCount, out int detailTriCount);

            var data = new MeshData
            {
                // Store header
                Header = new MeshHeader
                {
                    Magic = MeshHeader.DT_NAVMESH_MAGIC,
                    Version = MeshHeader.DT_NAVMESH_VERSION,
                    X = param.TileX,
                    Y = param.TileY,
                    Layer = param.TileLayer,
                    UserId = param.UserId,
                    PolyCount = totPolyCount,
                    VertCount = totVertCount,
                    MaxLinkCount = maxLinkCount,
                    Bounds = new BoundingBox(param.BMin, param.BMax),
                    DetailMeshCount = param.PolyCount,
                    DetailVertCount = uniqueDetailVertCount,
                    DetailTriCount = detailTriCount,
                    BvQuantFactor = 1.0f / param.CellSize,
                    OffMeshBase = param.PolyCount,
                    WalkableHeight = param.WalkableHeight,
                    WalkableRadius = param.WalkableRadius,
                    WalkableClimb = param.WalkableClimb,
                    OffMeshConCount = storedOffMeshConCount,
                    BvNodeCount = param.BuildBvTree ? param.PolyCount * 2 : 0
                }
            };

            int offMeshVertsBase = param.VertCount;
            int offMeshPolyBase = param.PolyCount;

            // Store vertices

            // Mesh vertices
            data.StoreMeshVertices(param);

            // Off-mesh link vertices.
            data.StoreOffMeshLinksVertices(param, offMeshConClass);

            // Store polygons

            // Mesh polys
            data.StoreMeshPolygons(param);

            // Off-mesh connection vertices.
            data.StoreOffMeshConnectionVertices(param, offMeshConClass, offMeshVertsBase);

            // Store detail meshes and vertices.
            // The nav polygon vertices are stored as the first vertices on each mesh.
            // We compress the mesh data by skipping them and using the navmesh coordinates.
            data.StoreDetailMeshes(param);

            // Store and create BVtree.
            if (param.BuildBvTree)
            {
                BVNode.CreateBVTree(param, out var nodes);

                data.NavBvtree.AddRange(nodes);
            }

            // Store Off-Mesh connections.
            data.StoreOffMeshConnections(param, offMeshConClass, offMeshPolyBase);

            return data;
        }
        private static void ClassifyOffMeshConnections(NavMeshCreateParams param, out Vector2Int[] offMeshConClass, out int storedOffMeshConCount, out int offMeshConLinkCount)
        {
            // Classify off-mesh connection points. We store only the connections
            // whose start point is inside the tile.
            offMeshConClass = null;
            storedOffMeshConCount = 0;
            offMeshConLinkCount = 0;

            if (param.OffMeshConCount <= 0)
            {
                return;
            }

            offMeshConClass = new Vector2Int[param.OffMeshConCount];

            // Find tight heigh bounds, used for culling out off-mesh start locations.
            var bbox = param.FindBounds();
            Vector3 bmin = bbox.Minimum;
            Vector3 bmax = bbox.Maximum;

            for (int i = 0; i < param.OffMeshConCount; ++i)
            {
                var p0 = param.OffMeshCon[i].Start;
                var p1 = param.OffMeshCon[i].End;
                int x = ClassifyOffMeshPoint(p0, bmin, bmax);
                int y = ClassifyOffMeshPoint(p1, bmin, bmax);

                // Zero out off-mesh start positions which are not even potentially touching the mesh and
                // count how many links should be allocated for off-mesh connections.
                if (x == 0xff)
                {
                    if (p0.Y < bmin.Y || p0.Y > bmax.Y)
                    {
                        x = 0;
                    }

                    offMeshConLinkCount++;
                    storedOffMeshConCount++;
                }

                if (y == 0xff)
                {
                    offMeshConLinkCount++;
                }

                offMeshConClass[i].X = x;
                offMeshConClass[i].Y = y;
            }
        }
        private static void FindPortalEdges(NavMeshCreateParams param, out int edgeCount, out int portalCount)
        {
            edgeCount = 0;
            portalCount = 0;

            int nvp = param.Nvp;

            for (int i = 0; i < param.PolyCount; ++i)
            {
                var p = param.Polys[i];

                for (int j = 0; j < nvp; ++j)
                {
                    if (p[j] == BVItem.MESH_NULL_IDX)
                    {
                        break;
                    }

                    edgeCount++;

                    if ((p[nvp + j] & 0x8000) != 0)
                    {
                        var dir = p[nvp + j] & 0xf;
                        if (dir != 0xf)
                        {
                            portalCount++;
                        }
                    }
                }
            }

        }
        private static void FindUniqueDetailVertices(NavMeshCreateParams param, out int uniqueDetailVertCount, out int detailTriCount)
        {
            // Find unique detail vertices.
            uniqueDetailVertCount = 0;

            if (param.DetailMeshes != null)
            {
                // Has detail mesh, count unique detail vertex count and use input detail tri count.
                detailTriCount = param.DetailTriCount;
                for (int i = 0; i < param.PolyCount; ++i)
                {
                    var p = param.Polys[i];
                    var ndv = param.DetailMeshes[i].VertCount;
                    int nv = p.FindFirstFreeIndex(param.Nvp);
                    ndv -= nv;
                    uniqueDetailVertCount += ndv;
                }
            }
            else
            {
                // No input detail mesh, build detail mesh from nav polys.
                uniqueDetailVertCount = 0; // No extra detail verts.
                detailTriCount = 0;
                for (int i = 0; i < param.PolyCount; ++i)
                {
                    var p = param.Polys[i];
                    int nv = p.FindFirstFreeIndex(param.Nvp);
                    detailTriCount += nv - 2;
                }
            }
        }
        private static int ClassifyOffMeshPoint(Vector3 pt, Vector3 bmin, Vector3 bmax)
        {
            int xp = 1 << 0;
            int zp = 1 << 1;
            int xm = 1 << 2;
            int zm = 1 << 3;

            int outcode = 0;
            outcode |= (pt.X >= bmax.X) ? xp : 0;
            outcode |= (pt.Z >= bmax.Z) ? zp : 0;
            outcode |= (pt.X < bmin.X) ? xm : 0;
            outcode |= (pt.Z < bmin.Z) ? zm : 0;

            if (outcode == xp) return 0;
            if (outcode == (xp | zp)) return 1;
            if (outcode == zp) return 2;
            if (outcode == (xm | zp)) return 3;
            if (outcode == xm) return 4;
            if (outcode == (xm | zm)) return 5;
            if (outcode == zm) return 6;
            if (outcode == (xp | zm)) return 7;

            return 0xff;
        }

        /// <summary>
        /// Stores mesh vertices from creation parameters
        /// </summary>
        /// <param name="param">Creation parameters</param>
        public void StoreMeshVertices(NavMeshCreateParams param)
        {
            // Mesh vertices
            for (int i = 0; i < param.VertCount; ++i)
            {
                var iv = param.Verts[i];
                var v = new Vector3
                {
                    X = param.BMin.X + iv.X * param.CellSize,
                    Y = param.BMin.Y + iv.Y * param.CellHeight,
                    Z = param.BMin.Z + iv.Z * param.CellSize
                };
                NavVerts.Add(v);
            }
        }
        /// <summary>
        /// Stores Off-mesh link vertices from creation parameters
        /// </summary>
        /// <param name="param">Creation parameters</param>
        /// <param name="offMeshConClass">Off-mesh connection classification</param>
        public void StoreOffMeshLinksVertices(NavMeshCreateParams param, IEnumerable<Vector2Int> offMeshConClass)
        {
            int n = 0;
            for (int i = 0; i < param.OffMeshConCount; ++i)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass.ElementAt(i).X == 0xff)
                {
                    var linkv = param.OffMeshCon[i];
                    NavVerts.Add(linkv.Start);
                    NavVerts.Add(linkv.End);
                    n++;
                }
            }
        }
        /// <summary>
        /// Stores mesh polygons from creation parameters
        /// </summary>
        /// <param name="param">Creation parameters</param>
        public void StoreMeshPolygons(NavMeshCreateParams param)
        {
            for (int i = 0; i < param.PolyCount; i++)
            {
                var p = Poly.Create(
                    param.Polys[i],
                    param.PolyFlags[i],
                    param.PolyAreas[i],
                    param.Nvp);

                NavPolys.Add(p);
            }
        }
        /// <summary>
        /// Stores Off-mesh connection vertices from creation parameters
        /// </summary>
        /// <param name="param">Creation parameters</param>
        /// <param name="offMeshConClass">Off-mesh connection classification</param>
        /// <param name="offMeshVertsBase">Off-mesh vertices base index</param>
        public void StoreOffMeshConnectionVertices(NavMeshCreateParams param, IEnumerable<Vector2Int> offMeshConClass, int offMeshVertsBase)
        {
            int n = 0;
            for (int i = 0; i < param.OffMeshConCount; i++)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass.ElementAt(i).X == 0xff)
                {
                    int start = offMeshVertsBase + (n * 2) + 0;
                    int end = offMeshVertsBase + (n * 2) + 1;

                    var p = Poly.Create(
                        start,
                        end,
                        param.OffMeshCon[i].FlagTypes,
                        param.OffMeshCon[i].AreaType);

                    NavPolys.Add(p);
                    n++;
                }
            }

        }
        /// <summary>
        /// Stores detail meshes from creation parameters
        /// </summary>
        /// <param name="param">Creation parameters</param>
        public void StoreDetailMeshes(NavMeshCreateParams param)
        {
            if (param.DetailMeshes != null)
            {
                StoreRealDetailMeshes(param);
            }
            else
            {
                StoreDummyDetailsMeshes(param);
            }

        }
        private void StoreRealDetailMeshes(NavMeshCreateParams param)
        {
            for (int i = 0; i < param.PolyCount; ++i)
            {
                int vb = param.DetailMeshes[i].VertBase;
                int ndv = param.DetailMeshes[i].VertCount;
                int nv = NavPolys[i].VertCount;
                var dtl = new PolyDetail
                {
                    VertBase = NavDVerts.Count,
                    VertCount = (ndv - nv),
                    TriBase = param.DetailMeshes[i].TriBase,
                    TriCount = param.DetailMeshes[i].TriCount,
                };
                // Copy vertices except the first 'nv' verts which are equal to nav poly verts.
                if (ndv - nv != 0)
                {
                    var verts = param.DetailVerts.Skip(vb + nv).Take(ndv - nv);
                    NavDVerts.AddRange(verts);
                }
                NavDMeshes.Add(dtl);
            }
            // Store triangles.
            NavDTris.AddRange(param.DetailTris);
        }
        private void StoreDummyDetailsMeshes(NavMeshCreateParams param)
        {
            // Create dummy detail mesh by triangulating polys.
            int tbase = 0;
            for (int i = 0; i < param.PolyCount; ++i)
            {
                int nv = NavPolys[i].VertCount;
                var dtl = new PolyDetail
                {
                    VertBase = 0,
                    VertCount = 0,
                    TriBase = tbase,
                    TriCount = (nv - 2)
                };
                // Triangulate polygon (local indices).
                for (int j = 2; j < nv; ++j)
                {
                    var t = new PolyMeshTriangleIndices
                    {
                        Point1 = 0,
                        Point2 = j - 1,
                        Point3 = j,
                        // Bit for each edge that belongs to poly boundary.
                        Flags = (1 << 2)
                    };
                    if (j == 2) t.Flags |= (1 << 0);
                    if (j == nv - 1) t.Flags |= (1 << 4);
                    tbase++;

                    NavDTris.Add(t);
                }
                NavDMeshes.Add(dtl);
            }
        }
        /// <summary>
        /// Stores Off-mesh connections from creation parameters
        /// </summary>
        /// <param name="param">Creation parameters</param>
        /// <param name="offMeshConClass">Off-mesh connection classification</param>
        /// <param name="offMeshPolyBase">Off-mesh polygon base index</param>
        public void StoreOffMeshConnections(NavMeshCreateParams param, IEnumerable<Vector2Int> offMeshConClass, int offMeshPolyBase)
        {
            int n = 0;

            for (int i = 0; i < param.OffMeshConCount; ++i)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass.ElementAt(i).X == 0xff)
                {
                    // Copy connection end-points.
                    var con = new OffMeshConnection
                    {
                        Poly = offMeshPolyBase + n,
                        Rad = param.OffMeshCon[i].Radius,
                        Flags = param.OffMeshCon[i].Direction != 0 ? OffMeshConnection.DT_OFFMESH_CON_BIDIR : 0,
                        Side = offMeshConClass.ElementAt(i).Y,
                        Start = param.OffMeshCon[i].Start,
                        End = param.OffMeshCon[i].End,
                        UserId = param.OffMeshCon[i].Id,
                    };

                    OffMeshCons.Add(con);
                    n++;
                }
            }

        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Header: {Header};";
        }
    }
}
