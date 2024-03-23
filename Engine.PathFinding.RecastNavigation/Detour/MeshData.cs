using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Mesh data
    /// </summary>
    [Serializable]
    public class MeshData
    {
        /// <summary>
        /// Null off-mesh mask
        /// </summary>
        const int NULL_OFFMESH = 0xff;

        /// <summary>
        /// Mesh header
        /// </summary>
        public MeshHeader Header { get; set; }
        /// <summary>
        /// Navigation vertices
        /// </summary>
        public List<Vector3> NavVerts { get; set; } = [];
        /// <summary>
        /// Navigation polygons
        /// </summary>
        public List<Poly> NavPolys { get; set; } = [];
        /// <summary>
        /// Navigation detail meshes
        /// </summary>
        public List<PolyDetail> NavDMeshes { get; set; } = [];
        /// <summary>
        /// Navigation detail vertices
        /// </summary>
        public List<Vector3> NavDVerts { get; set; } = [];
        /// <summary>
        /// Navigation detail triangles
        /// </summary>
        public List<PolyMeshTriangleIndices> NavDTris { get; set; } = [];
        /// <summary>
        /// Navigation BVTree
        /// </summary>
        public List<BVNode> NavBvtree { get; set; } = [];
        /// <summary>
        /// Off-mesh connections
        /// </summary>
        public List<OffMeshConnection> OffMeshCons { get; set; } = [];

        /// <summary>
        /// Build data
        /// </summary>
        internal BuildData BuildData { get; set; }

        /// <summary>
        /// Creates the mesh data
        /// </summary>
        /// <param name="param">Creation parameters</param>
        public static MeshData CreateNavMeshData(NavMeshCreateParams param)
        {
            if (param.Nvp > IndexedPolygon.DT_VERTS_PER_POLYGON ||
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
                    Bounds = param.Bounds,
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
            data.CreateBVTree(param);

            // Store Off-Mesh connections.
            data.StoreOffMeshConnections(param, offMeshConClass, offMeshPolyBase);

            return data;
        }
        /// <summary>
        /// Subdivide the node list
        /// </summary>
        /// <param name="items">Item list</param>
        /// <param name="nitems">Number of items</param>
        /// <param name="imin">Minimum index</param>
        /// <param name="imax">Maximum index</param>
        /// <param name="curNode">Current node</param>
        /// <param name="nodes">List of nodes</param>
        private static void Subdivide(BVItem[] items, int nitems, int imin, int imax, ref int curNode, ref List<BVNode> nodes)
        {
            int inum = imax - imin;
            int icur = curNode;

            var node = new BVNode();
            nodes.Add(node);
            curNode++;

            if (inum == 1)
            {
                // Leaf
                node.BMin = items[imin].BMin;
                node.BMax = items[imin].BMax;
                node.I = items[imin].I;
            }
            else
            {
                // Split
                CalcExtends(items, imin, imax, out var bmin, out var bmax);
                node.BMin = bmin;
                node.BMax = bmax;

                int axis = Utils.LongestAxis(
                    node.BMax.X - node.BMin.X,
                    node.BMax.Y - node.BMin.Y,
                    node.BMax.Z - node.BMin.Z);

                if (axis == 0)
                {
                    // Sort along x-axis
                    Array.Sort(items, imin, inum, BVItem.XComparer);
                }
                else if (axis == 1)
                {
                    // Sort along y-axis
                    Array.Sort(items, imin, inum, BVItem.YComparer);
                }
                else
                {
                    // Sort along z-axis
                    Array.Sort(items, imin, inum, BVItem.ZComparer);
                }

                int isplit = imin + inum / 2;

                // Left
                Subdivide(items, nitems, imin, isplit, ref curNode, ref nodes);
                // Right
                Subdivide(items, nitems, isplit, imax, ref curNode, ref nodes);

                int iescape = curNode - icur;
                // Negative index means escape.
                node.I = -iescape;
            }
        }
        /// <summary>
        /// Calculates node extends
        /// </summary>
        /// <param name="items">Item list</param>
        /// <param name="imin">Minimum index</param>
        /// <param name="imax">Maximum index</param>
        /// <param name="bmin">Resulting minimum bounds point</param>
        /// <param name="bmax">Resulting maximum bounds point</param>
        private static void CalcExtends(BVItem[] items, int imin, int imax, out Int3 bmin, out Int3 bmax)
        {
            bmin.X = items[imin].BMin.X;
            bmin.Y = items[imin].BMin.Y;
            bmin.Z = items[imin].BMin.Z;

            bmax.X = items[imin].BMax.X;
            bmax.Y = items[imin].BMax.Y;
            bmax.Z = items[imin].BMax.Z;

            for (int i = imin + 1; i < imax; ++i)
            {
                var it = items[i];
                if (it.BMin.X < bmin.X) bmin.X = it.BMin.X;
                if (it.BMin.Y < bmin.Y) bmin.Y = it.BMin.Y;
                if (it.BMin.Z < bmin.Z) bmin.Z = it.BMin.Z;

                if (it.BMax.X > bmax.X) bmax.X = it.BMax.X;
                if (it.BMax.Y > bmax.Y) bmax.Y = it.BMax.Y;
                if (it.BMax.Z > bmax.Z) bmax.Z = it.BMax.Z;
            }
        }

        /// <summary>
        /// Creates the BVTree structure
        /// </summary>
        /// <param name="param">Creation parameters</param>
        private void CreateBVTree(NavMeshCreateParams param)
        {
            if (!param.BuildBvTree)
            {
                return;
            }

            var nodes = new List<BVNode>();

            // Build tree
            float quantFactor = 1 / param.CellSize;
            BVItem[] items = new BVItem[param.PolyCount];
            for (int i = 0; i < param.PolyCount; i++)
            {
                var it = items[i];
                it.I = i;
                // Calc polygon bounds. Use detail meshes if available.
                if (param.DetailMeshes != null)
                {
                    it.CalcDetailBounds(param.DetailMeshes[i], param.DetailVerts, param.Bounds.Minimum, quantFactor);
                }
                else
                {
                    it.CalcPolygonBounds(param.Polys[i], param.Nvp, param.Verts, param.CellSize, param.CellHeight);
                }
                items[i] = it;
            }

            int curNode = 0;
            Subdivide(items, param.PolyCount, 0, param.PolyCount, ref curNode, ref nodes);

            NavBvtree.AddRange(nodes);
        }
        /// <summary>
        /// Classify off-mesh connections
        /// </summary>
        /// <param name="param">Creation parameters</param>
        /// <param name="offMeshConClass">Off-mesh connection classification</param>
        /// <param name="storedOffMeshConCount">Resulting stored connection count</param>
        /// <param name="offMeshConLinkCount">Resulting stored link count</param>
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
                if (x == NULL_OFFMESH)
                {
                    if (p0.Y < bmin.Y || p0.Y > bmax.Y)
                    {
                        x = 0;
                    }

                    offMeshConLinkCount++;
                    storedOffMeshConCount++;
                }

                if (y == NULL_OFFMESH)
                {
                    offMeshConLinkCount++;
                }

                offMeshConClass[i].X = x;
                offMeshConClass[i].Y = y;
            }
        }
        /// <summary>
        /// Finds portal edges
        /// </summary>
        /// <param name="param">Creation parameters</param>
        /// <param name="edgeCount">Resulting edge count</param>
        /// <param name="portalCount">Resulting portal count</param>
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
                    if (p.VertexIsNull(j))
                    {
                        break;
                    }

                    edgeCount++;

                    if (p.IsExternalLink(j) && p.HasDirection(j))
                    {
                        portalCount++;
                    }
                }
            }

        }
        /// <summary>
        /// Finds unique detail vertices
        /// </summary>
        /// <param name="param">Creation parameters</param>
        /// <param name="uniqueDetailVertCount">Resulting unique detail vertex count</param>
        /// <param name="detailTriCount">Resulting detail triangle count</param>
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
        /// <summary>
        /// Classify off-mesh point
        /// </summary>
        /// <param name="pt">Point to classify</param>
        /// <param name="bmin">Minimum bounds vertex</param>
        /// <param name="bmax">Maximum bounds vertex</param>
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

            return NULL_OFFMESH;
        }

        /// <summary>
        /// Stores mesh vertices from creation parameters
        /// </summary>
        /// <param name="param">Creation parameters</param>
        private void StoreMeshVertices(NavMeshCreateParams param)
        {
            // Mesh vertices
            for (int i = 0; i < param.VertCount; ++i)
            {
                var iv = param.Verts[i];
                var v = new Vector3
                {
                    X = param.Bounds.Minimum.X + iv.X * param.CellSize,
                    Y = param.Bounds.Minimum.Y + iv.Y * param.CellHeight,
                    Z = param.Bounds.Minimum.Z + iv.Z * param.CellSize
                };
                NavVerts.Add(v);
            }
        }
        /// <summary>
        /// Stores Off-mesh link vertices from creation parameters
        /// </summary>
        /// <param name="param">Creation parameters</param>
        /// <param name="offMeshConClass">Off-mesh connection classification</param>
        private void StoreOffMeshLinksVertices(NavMeshCreateParams param, Vector2Int[] offMeshConClass)
        {
            int n = 0;
            for (int i = 0; i < param.OffMeshConCount; ++i)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass[i].X == NULL_OFFMESH)
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
        private void StoreMeshPolygons(NavMeshCreateParams param)
        {
            for (int i = 0; i < param.PolyCount; i++)
            {
                var p = Poly.Create(param.Polys[i], param.PolyFlags[i], param.PolyAreas[i], param.Nvp);

                NavPolys.Add(p);
            }
        }
        /// <summary>
        /// Stores Off-mesh connection vertices from creation parameters
        /// </summary>
        /// <param name="param">Creation parameters</param>
        /// <param name="offMeshConClass">Off-mesh connection classification</param>
        /// <param name="offMeshVertsBase">Off-mesh vertices base index</param>
        private void StoreOffMeshConnectionVertices(NavMeshCreateParams param, Vector2Int[] offMeshConClass, int offMeshVertsBase)
        {
            int n = 0;
            for (int i = 0; i < param.OffMeshConCount; i++)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass[i].X == NULL_OFFMESH)
                {
                    int start = offMeshVertsBase + (n * 2) + 0;
                    int end = offMeshVertsBase + (n * 2) + 1;

                    var p = Poly.CreateOffMesh(
                        start,
                        end,
                        (SamplePolyFlagTypes)param.OffMeshCon[i].FlagTypes,
                        (SamplePolyAreas)param.OffMeshCon[i].AreaType);

                    NavPolys.Add(p);
                    n++;
                }
            }

        }
        /// <summary>
        /// Stores detail meshes from creation parameters
        /// </summary>
        /// <param name="param">Creation parameters</param>
        private void StoreDetailMeshes(NavMeshCreateParams param)
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
        /// <summary>
        /// Store real detail meshes
        /// </summary>
        /// <param name="param">Creation parameters</param>
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
        /// <summary>
        /// Store dummy detail meshes
        /// </summary>
        /// <param name="param">Creation parameters</param>
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
        private void StoreOffMeshConnections(NavMeshCreateParams param, Vector2Int[] offMeshConClass, int offMeshPolyBase)
        {
            int n = 0;

            for (int i = 0; i < param.OffMeshConCount; ++i)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass[i].X != NULL_OFFMESH)
                {
                    continue;
                }

                // Copy connection end-points.
                var con = new OffMeshConnection
                {
                    Poly = offMeshPolyBase + n,
                    Rad = param.OffMeshCon[i].Radius,
                    Direction = (OffMeshConnectionDirections)param.OffMeshCon[i].Direction,
                    Side = offMeshConClass[i].Y,
                    Start = param.OffMeshCon[i].Start,
                    End = param.OffMeshCon[i].End,
                    UserId = param.OffMeshCon[i].Id,
                };

                OffMeshCons.Add(con);
                n++;
            }

        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Header: {Header};";
        }
    }
}
