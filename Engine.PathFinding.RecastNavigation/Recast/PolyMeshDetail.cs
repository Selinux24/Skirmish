using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Contains triangle meshes that represent detailed height data associated with the polygons in its associated polygon mesh object.
    /// </summary>
    class PolyMeshDetail
    {
        /// <summary>
        /// The sub-mesh data.
        /// </summary>
        public List<PolyMeshDetailIndices> Meshes { get; set; } = new List<PolyMeshDetailIndices>();
        /// <summary>
        /// The mesh vertices.
        /// </summary>
        public List<Vector3> Vertices { get; set; } = new List<Vector3>();
        /// <summary>
        /// The mesh triangles.
        /// </summary>
        public List<PolyMeshTriangleIndices> Triangles { get; set; } = new List<PolyMeshTriangleIndices>();

        /// <summary>
        /// Builds a new polygon mesh detail
        /// </summary>
        /// <param name="mesh">Polygon mesh</param>
        /// <param name="chf">Compact heightfield</param>
        /// <param name="sampleDist">Sample distance</param>
        /// <param name="sampleMaxError">Sample maximum error</param>
        /// <returns>Returns the new polygon mesh detail</returns>
        public static PolyMeshDetail Build(PolyMesh mesh, CompactHeightfield chf, float sampleDist, float sampleMaxError)
        {
            PolyMeshDetail dmesh;

            if (mesh.NVerts == 0 || mesh.NPolys == 0)
            {
                return null;
            }

            Vector3 orig = mesh.BMin;
            int borderSize = mesh.BorderSize;
            int heightSearchRadius = Math.Max(1, (int)Math.Ceiling(mesh.MaxEdgeError));

            var (Bounds, MaxHWidth, MaxHHeight) = FindBounds(mesh, chf);
            var bounds = Bounds;
            int maxhw = MaxHWidth;
            int maxhh = MaxHHeight;

            var hp = new HeightPatch()
            {
                Data = new int[maxhw * maxhh],
            };

            dmesh = new PolyMeshDetail();

            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var iPoly = mesh.Polys[i];
                var region = mesh.Regs[i];
                var b = bounds[i];

                // Store polygon vertices for processing.
                var poly = BuildPolyVertices(iPoly, mesh);

                // Get the height data from the area of the polygon.
                hp.Bounds = new Rectangle(b.X, b.Z, b.Y - b.X, b.W - b.Z);

                chf.GetHeightData(iPoly, mesh.Verts, hp, borderSize, region);

                // Build detail mesh.
                var param = new BuildPolyDetailParams
                {
                    SampleDist = sampleDist,
                    SampleMaxError = sampleMaxError,
                    HeightSearchRadius = heightSearchRadius,
                };
                chf.BuildPolyDetail(poly, param, hp, out var verts, out var tris);

                // Move detail verts to world space.
                verts = MoveToWorldSpace(verts, orig, chf.CellHeight);

                // Offset poly too, will be used to flag checking.
                poly = MoveToWorldSpace(poly, orig);

                // Store detail submesh.
                dmesh.Meshes.Add(new PolyMeshDetailIndices
                {
                    VertBase = dmesh.Vertices.Count,
                    VertCount = verts.Length,
                    TriBase = dmesh.Triangles.Count,
                    TriCount = tris.Length,
                });

                // Store vertices
                dmesh.Vertices.AddRange(verts);

                // Store triangles
                var triIndices = BuildTriangleList(tris, verts, poly);
                dmesh.Triangles.AddRange(triIndices);
            }

            return dmesh;
        }
        private static (Int4[] Bounds, int MaxHWidth, int MaxHHeight) FindBounds(PolyMesh mesh, CompactHeightfield chf)
        {
            var bounds = new List<Int4>();

            int nPolyVerts = 0;
            int maxhw = 0;
            int maxhh = 0;

            // Find max size for a polygon area.
            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];

                var (XMin, XMax, YMin, YMax, PolyVerts) = FindMaxSizeArea(mesh, chf, p);

                bounds.Add(new Int4(XMin, XMax, YMin, YMax));
                nPolyVerts += PolyVerts;

                // Try to store max size
                if (XMin >= XMax || YMin >= YMax)
                {
                    continue;
                }

                maxhw = Math.Max(maxhw, XMax - XMin);
                maxhh = Math.Max(maxhh, YMax - YMin);
            }

            return (bounds.ToArray(), maxhw, maxhh);
        }
        private static (int XMin, int XMax, int YMin, int YMax, int PolyVerts) FindMaxSizeArea(PolyMesh mesh, CompactHeightfield chf, IndexedPolygon p)
        {
            int xmin = chf.Width;
            int xmax = 0;
            int ymin = chf.Height;
            int ymax = 0;
            int polyVerts = 0;

            for (int j = 0; j < mesh.NVP; ++j)
            {
                if (p[j] == IndexedPolygon.RC_MESH_NULL_IDX)
                {
                    break;
                }

                var v = mesh.Verts[p[j]];
                xmin = Math.Min(xmin, v.X);
                xmax = Math.Max(xmax, v.X);
                ymin = Math.Min(ymin, v.Z);
                ymax = Math.Max(ymax, v.Z);
                polyVerts++;
            }
            xmin = Math.Max(0, xmin - 1);
            xmax = Math.Min(chf.Width, xmax + 1);
            ymin = Math.Max(0, ymin - 1);
            ymax = Math.Min(chf.Height, ymax + 1);

            return (xmin, xmax, ymin, ymax, polyVerts);
        }
        private static Vector3[] BuildPolyVertices(IndexedPolygon p, PolyMesh mesh)
        {
            var res = new List<Vector3>();

            float cs = mesh.CS;
            float ch = mesh.CH;

            for (int j = 0; j < mesh.NVP; ++j)
            {
                if (p[j] == IndexedPolygon.RC_MESH_NULL_IDX)
                {
                    break;
                }

                var v = mesh.Verts[p[j]];
                var pv = new Vector3(v.X * cs, v.Y * ch, v.Z * cs);

                res.Add(pv);
            }

            return res.ToArray();
        }
        private static Vector3[] MoveToWorldSpace(Vector3[] verts, Vector3 orig, float cellHeight)
        {
            var res = new List<Vector3>();

            for (int j = 0; j < verts.Length; ++j)
            {
                var v = verts[j] + orig;
                v.Y += cellHeight;// Is this offset necessary?

                res.Add(v);
            }

            return res.ToArray();
        }
        private static Vector3[] MoveToWorldSpace(Vector3[] poly, Vector3 orig)
        {
            var res = new List<Vector3>();

            for (int j = 0; j < poly.Length; ++j)
            {
                var p = poly[j] + orig;

                res.Add(p);
            }

            return res.ToArray();
        }
        private static PolyMeshTriangleIndices[] BuildTriangleList(Int3[] tris, Vector3[] verts, Vector3[] poly)
        {
            var res = new List<PolyMeshTriangleIndices>();

            foreach (var t in tris)
            {
                res.Add(new PolyMeshTriangleIndices
                {
                    Point1 = t.X,
                    Point2 = t.Y,
                    Point3 = t.Z,
                    Flags = GetTriFlags(verts[t.X], verts[t.Y], verts[t.Z], poly),
                });
            }

            return res.ToArray();
        }

        /// <summary>
        /// Merges a list of polygon mesh details
        /// </summary>
        /// <param name="meshes">Mesh list</param>
        /// <returns>Returns the merged polygon mesh detail</returns>
        public static PolyMeshDetail Merge(PolyMeshDetail[] meshes)
        {
            var res = new PolyMeshDetail();

            int maxVerts = 0;
            int maxTris = 0;
            int maxMeshes = 0;

            foreach (var mesh in meshes)
            {
                if (mesh == null)
                {
                    continue;
                }
                maxVerts += mesh.Vertices.Count;
                maxTris += mesh.Triangles.Count;
                maxMeshes += mesh.Meshes.Count;
            }

            // Merge datas.
            foreach (var dm in meshes)
            {
                if (dm == null)
                {
                    continue;
                }

                foreach (var src in dm.Meshes)
                {
                    var dst = new PolyMeshDetailIndices
                    {
                        VertBase = res.Vertices.Count + src.VertBase,
                        VertCount = src.VertCount,
                        TriBase = res.Triangles.Count + src.TriBase,
                        TriCount = src.TriCount,
                    };

                    res.Meshes.Add(dst);
                }

                res.Vertices.AddRange(dm.Vertices);

                res.Triangles.AddRange(dm.Triangles);
            }

            return res;
        }
        private static int GetEdgeFlags(Vector3 va, Vector3 vb, Vector3[] vpoly)
        {
            int npoly = vpoly.Length;

            // Return true if edge (va,vb) is part of the polygon.
            float thrSqr = 0.001f * 0.001f;
            for (int i = 0, j = npoly - 1; i < npoly; j = i++)
            {
                var vi = vpoly[i];
                var vj = vpoly[j];
                if (RecastUtils.DistancePtSeg2D(va, vj, vi) < thrSqr &&
                    RecastUtils.DistancePtSeg2D(vb, vj, vi) < thrSqr)
                {
                    return 1;
                }
            }
            return 0;
        }
        private static int GetTriFlags(Vector3 va, Vector3 vb, Vector3 vc, Vector3[] vpoly)
        {
            int flags = 0;
            flags |= GetEdgeFlags(va, vb, vpoly) << 0;
            flags |= GetEdgeFlags(vb, vc, vpoly) << 2;
            flags |= GetEdgeFlags(vc, va, vpoly) << 4;
            return flags;
        }
    }
}
