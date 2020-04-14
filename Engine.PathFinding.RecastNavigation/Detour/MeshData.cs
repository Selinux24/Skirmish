using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    using Engine.PathFinding.RecastNavigation.Recast;

    /// <summary>
    /// Mesh data
    /// </summary>
    [Serializable]
    public class MeshData : ISerializable
    {
        public static MeshData CreateNavMeshData(NavMeshCreateParams param)
        {
            if (param.Nvp > DetourUtils.DT_VERTS_PER_POLYGON ||
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

            MeshData data = new MeshData
            {
                // Store header
                Header = new MeshHeader
                {
                    Magic = DetourUtils.DT_NAVMESH_MAGIC,
                    Version = DetourUtils.DT_NAVMESH_VERSION,
                    X = param.TileX,
                    Y = param.TileY,
                    Layer = param.TileLayer,
                    UserId = param.UserId,
                    PolyCount = totPolyCount,
                    VertCount = totVertCount,
                    MaxLinkCount = maxLinkCount,
                    BMin = param.BMin,
                    BMax = param.BMax,
                    DetailMeshCount = param.PolyCount,
                    DetailVertCount = uniqueDetailVertCount,
                    DetailTriCount = detailTriCount,
                    BvQuantFactor = 1.0f / param.CS,
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
                CreateBVTree(param, out var nodes);

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
                    if (p[j] == DetourUtils.MESH_NULL_IDX)
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

        public static int CreateBVTree(NavMeshCreateParams param, out List<BVNode> nodes)
        {
            nodes = new List<BVNode>();

            // Build tree
            float quantFactor = 1 / param.CS;
            BVItem[] items = new BVItem[param.PolyCount];
            for (int i = 0; i < param.PolyCount; i++)
            {
                var it = items[i];
                it.I = i;
                // Calc polygon bounds. Use detail meshes if available.
                if (param.DetailMeshes != null)
                {
                    CalcDetailBounds(ref it, param.DetailMeshes[i], param.DetailVerts, param.BMin, quantFactor);
                }
                else
                {
                    CalcPolygonBounds(ref it, param.Polys[i], param.Nvp, param.Verts, param.CS, param.CH);
                }
                items[i] = it;
            }

            int curNode = 0;
            Subdivide(items, param.PolyCount, 0, param.PolyCount, ref curNode, ref nodes);

            return curNode;
        }
        private static void CalcDetailBounds(ref BVItem it, PolyMeshDetailIndices dm, Vector3[] detailVerts, Vector3 bMin, float quantFactor)
        {
            int vb = dm.VertBase;
            int ndv = dm.VertCount;
            GetMinMaxBounds(detailVerts, vb, ndv, out var bmin, out var bmax);

            // BV-tree uses cs for all dimensions
            it.BMin = new Int3
            {
                X = MathUtil.Clamp((int)((bmin.X - bMin.X) * quantFactor), 0, int.MaxValue),
                Y = MathUtil.Clamp((int)((bmin.Y - bMin.Y) * quantFactor), 0, int.MaxValue),
                Z = MathUtil.Clamp((int)((bmin.Z - bMin.Z) * quantFactor), 0, int.MaxValue)
            };

            it.BMax = new Int3
            {
                X = MathUtil.Clamp((int)((bmax.X - bMin.X) * quantFactor), 0, int.MaxValue),
                Y = MathUtil.Clamp((int)((bmax.Y - bMin.Y) * quantFactor), 0, int.MaxValue),
                Z = MathUtil.Clamp((int)((bmax.Z - bMin.Z) * quantFactor), 0, int.MaxValue)
            };
        }
        private static void GetMinMaxBounds(Vector3[] vectors, int vb, int ndv, out Vector3 bmin, out Vector3 bmax)
        {
            bmin = vectors[vb];
            bmax = vectors[vb];
            for (int j = 1; j < ndv; j++)
            {
                bmin = Vector3.Min(bmin, vectors[vb + j]);
                bmax = Vector3.Max(bmax, vectors[vb + j]);
            }
        }
        private static void CalcPolygonBounds(ref BVItem it, IndexedPolygon p, int nvp, Int3[] verts, float ch, float cs)
        {
            var itBMin = verts[p[0]];
            var itBMax = verts[p[0]];

            for (int j = 1; j < nvp; ++j)
            {
                if (p[j] == DetourUtils.MESH_NULL_IDX) break;
                var x = verts[p[j]].X;
                var y = verts[p[j]].Y;
                var z = verts[p[j]].Z;

                if (x < it.BMin.X) itBMin.X = x;
                if (y < it.BMin.Y) itBMin.Y = y;
                if (z < it.BMin.Z) itBMin.Z = z;

                if (x > it.BMax.X) itBMax.X = x;
                if (y > it.BMax.Y) itBMax.Y = y;
                if (z > it.BMax.Z) itBMax.Z = z;
            }
            // Remap y
            itBMin.Y = (int)Math.Floor(it.BMin.Y * ch / cs);
            itBMax.Y = (int)Math.Ceiling(it.BMax.Y * ch / cs);

            it.BMin = itBMin;
            it.BMax = itBMax;
        }
        private static void Subdivide(BVItem[] items, int nitems, int imin, int imax, ref int curNode, ref List<BVNode> nodes)
        {
            int inum = imax - imin;
            int icur = curNode;

            BVNode node = new BVNode();
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

                int axis = LongestAxis(
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
        private static int LongestAxis(int x, int y, int z)
        {
            int axis = 0;
            int maxVal = x;
            if (y > maxVal)
            {
                axis = 1;
                maxVal = y;
            }
            if (z > maxVal)
            {
                axis = 2;
            }
            return axis;
        }
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
                BVItem it = items[i];
                if (it.BMin.X < bmin.X) bmin.X = it.BMin.X;
                if (it.BMin.Y < bmin.Y) bmin.Y = it.BMin.Y;
                if (it.BMin.Z < bmin.Z) bmin.Z = it.BMin.Z;

                if (it.BMax.X > bmax.X) bmax.X = it.BMax.X;
                if (it.BMax.Y > bmax.Y) bmax.Y = it.BMax.Y;
                if (it.BMax.Z > bmax.Z) bmax.Z = it.BMax.Z;
            }
        }

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

        /// <summary>
        /// Constructor
        /// </summary>
        public MeshData()
        {

        }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization information</param>
        /// <param name="context">Serializatio context</param>
        protected MeshData(SerializationInfo info, StreamingContext context)
        {
            Header = info.GetValue<MeshHeader>("header");

            var navVertsCount = info.GetInt32("navVerts.Count");
            for (int i = 0; i < navVertsCount; i++)
            {
                NavVerts.Add(info.GetVector3(string.Format("navVerts.{0}", i)));
            }

            var navPolysCount = info.GetInt32("navPolys.Count");
            for (int i = 0; i < navPolysCount; i++)
            {
                NavPolys.Add(info.GetValue<Poly>(string.Format("navPolys.{0}", i)));
            }

            var navDMeshesCount = info.GetInt32("navDMeshes.Count");
            for (int i = 0; i < navDMeshesCount; i++)
            {
                NavDMeshes.Add(info.GetValue<PolyDetail>(string.Format("navDMeshes.{0}", i)));
            }

            var navDVertsCount = info.GetInt32("navDVerts.Count");
            for (int i = 0; i < navDVertsCount; i++)
            {
                NavDVerts.Add(info.GetVector3(string.Format("navDVerts.{0}", i)));
            }

            var navDTrisCount = info.GetInt32("navDTris.Count");
            for (int i = 0; i < navDTrisCount; i++)
            {
                NavDTris.Add(info.GetValue<PolyMeshTriangleIndices>(string.Format("navDTris.{0}", i)));
            }

            var navBvtreeCount = info.GetInt32("navBvtree.Count");
            for (int i = 0; i < navBvtreeCount; i++)
            {
                NavBvtree.Add(info.GetValue<BVNode>(string.Format("navBvtree.{0}", i)));
            }

            var offMeshConsCount = info.GetInt32("offMeshCons.Count");
            for (int i = 0; i < offMeshConsCount; i++)
            {
                OffMeshCons.Add(info.GetValue<OffMeshConnection>(string.Format("offMeshCons.{0}", i)));
            }
        }
        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("header", Header);

            info.AddValue("navVerts.Count", NavVerts.Count);
            for (int i = 0; i < NavVerts.Count; i++)
            {
                info.AddVector3(string.Format("navVerts.{0}", i), NavVerts[i]);
            }

            info.AddValue("navPolys.Count", NavPolys.Count);
            for (int i = 0; i < NavPolys.Count; i++)
            {
                info.AddValue(string.Format("navPolys.{0}", i), NavPolys[i]);
            }

            info.AddValue("navDMeshes.Count", NavDMeshes.Count);
            for (int i = 0; i < NavDMeshes.Count; i++)
            {
                info.AddValue(string.Format("navDMeshes.{0}", i), NavDMeshes[i]);
            }

            info.AddValue("navDVerts.Count", NavDVerts.Count);
            for (int i = 0; i < NavDVerts.Count; i++)
            {
                info.AddVector3(string.Format("navDVerts.{0}", i), NavDVerts[i]);
            }

            info.AddValue("navDTris.Count", NavDTris.Count);
            for (int i = 0; i < NavDTris.Count; i++)
            {
                info.AddValue(string.Format("navDTris.{0}", i), NavDTris[i]);
            }

            info.AddValue("navBvtree.Count", NavBvtree.Count);
            for (int i = 0; i < NavBvtree.Count; i++)
            {
                info.AddValue(string.Format("navBvtree.{0}", i), NavBvtree[i]);
            }

            info.AddValue("offMeshCons.Count", OffMeshCons.Count);
            for (int i = 0; i < OffMeshCons.Count; i++)
            {
                info.AddValue(string.Format("offMeshCons.{0}", i), OffMeshCons[i]);
            }
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("header: {0};", Header);
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
                    X = param.BMin.X + iv.X * param.CS,
                    Y = param.BMin.Y + iv.Y * param.CH,
                    Z = param.BMin.Z + iv.Z * param.CS
                };
                NavVerts.Add(v);
            }
        }
        /// <summary>
        /// Stores Off-mesh link vertices from creation parameters
        /// </summary>
        /// <param name="param">Creation parameters</param>
        /// <param name="offMeshConClass">Off-mesh connection classification</param>
        public void StoreOffMeshLinksVertices(NavMeshCreateParams param, Vector2Int[] offMeshConClass)
        {
            int n = 0;
            for (int i = 0; i < param.OffMeshConCount; ++i)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass[i].X == 0xff)
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
        public void StoreOffMeshConnectionVertices(NavMeshCreateParams param, Vector2Int[] offMeshConClass, int offMeshVertsBase)
        {
            int n = 0;
            for (int i = 0; i < param.OffMeshConCount; i++)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass[i].X == 0xff)
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
                PolyDetail dtl = new PolyDetail
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
                PolyDetail dtl = new PolyDetail
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
        public void StoreOffMeshConnections(NavMeshCreateParams param, Vector2Int[] offMeshConClass, int offMeshPolyBase)
        {
            int n = 0;

            for (int i = 0; i < param.OffMeshConCount; ++i)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass[i].X == 0xff)
                {
                    // Copy connection end-points.
                    var con = new OffMeshConnection
                    {
                        Poly = offMeshPolyBase + n,
                        Rad = param.OffMeshCon[i].Radius,
                        Flags = param.OffMeshCon[i].Direction != 0 ? DetourUtils.DT_OFFMESH_CON_BIDIR : 0,
                        Side = offMeshConClass[i].Y,
                        Start = param.OffMeshCon[i].Start,
                        End = param.OffMeshCon[i].End,
                        UserId = param.OffMeshCon[i].Id,
                    };

                    OffMeshCons.Add(con);
                    n++;
                }
            }

        }
    }
}
