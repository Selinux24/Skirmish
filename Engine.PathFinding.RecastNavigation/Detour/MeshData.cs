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
