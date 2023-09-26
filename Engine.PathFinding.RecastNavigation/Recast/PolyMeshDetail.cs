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
        public List<PolyMeshDetailIndices> Meshes { get; set; } = new();
        /// <summary>
        /// The mesh vertices.
        /// </summary>
        public List<Vector3> Vertices { get; set; } = new();
        /// <summary>
        /// The mesh triangles.
        /// </summary>
        public List<PolyMeshTriangleIndices> Triangles { get; set; } = new();

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
            if (mesh.NVerts == 0 || mesh.NPolys == 0)
            {
                return null;
            }

            Vector3 orig = mesh.BMin;
            int borderSize = mesh.BorderSize;
            int heightSearchRadius = Math.Max(1, (int)Math.Ceiling(mesh.MaxEdgeError));

            var (Bounds, MaxHWidth, MaxHHeight) = mesh.FindBounds(chf);
            var bounds = Bounds;
            int maxhw = MaxHWidth;
            int maxhh = MaxHHeight;

            var hp = new HeightPatch()
            {
                Data = new int[maxhw * maxhh],
            };

            var dmesh = new PolyMeshDetail();

            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var iPoly = mesh.Polys[i];
                var region = mesh.Regs[i];
                var b = bounds[i];

                // Store polygon vertices for processing.
                var poly = mesh.BuildPolyVertices(iPoly);

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
                verts = Utils.MoveToWorldSpace(verts, orig, chf.CellHeight);

                // Offset poly too, will be used to flag checking.
                poly = Utils.MoveToWorldSpace(poly, orig);

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
                var triIndices = PolyMeshTriangleIndices.BuildTriangleList(tris, verts, poly);
                dmesh.Triangles.AddRange(triIndices);
            }

            return dmesh;
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
    }
}
