using Engine.PathFinding.RecastNavigation.Recast;
using SharpDX;
using System;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    public struct TileCacheContourSet
    {
        public int NConts { get; set; }
        public TileCacheContour[] Conts { get; set; }

        public bool BuildTileCachePolyMesh(out TileCachePolyMesh mesh)
        {
            int maxVertices = 0;
            int maxTris = 0;
            int maxVertsPerCont = 0;
            for (int i = 0; i < NConts; ++i)
            {
                // Skip null contours.
                if (Conts[i].NVerts < 3) continue;
                maxVertices += Conts[i].NVerts;
                maxTris += Conts[i].NVerts - 2;
                maxVertsPerCont = Math.Max(maxVertsPerCont, Conts[i].NVerts);
            }

            mesh = new TileCachePolyMesh
            {
                NVP = NavMeshCreateParams.DT_VERTS_PER_POLYGON,
                Verts = Helper.CreateArray(maxVertices, () => new Int3()),
                Polys = new IndexedPolygon[maxTris],
                Areas = new SamplePolyAreas[maxTris],
                Flags = new SamplePolyFlagTypes[maxTris],
                NVerts = 0,
                NPolys = 0
            };

            int[] vflags = new int[maxVertices];
            int[] firstVert = Helper.CreateArray(TileCachePolyMesh.VERTEX_BUCKET_COUNT2, TileCachePolyMesh.DT_TILECACHE_NULL_IDX);
            int[] nextVert = Helper.CreateArray(maxVertices, 0);
            int[] indices = new int[maxVertsPerCont];

            for (int i = 0; i < NConts; ++i)
            {
                var cont = Conts[i];

                // Skip null contours.
                if (cont.NVerts < 3)
                {
                    continue;
                }

                // Triangulate contour
                for (int j = 0; j < cont.NVerts; ++j)
                {
                    indices[j] = j;
                }

                int ntris = TriangulationHelper.Triangulate(cont.Verts, ref indices, out var tris);
                if (ntris <= 0)
                {
                    Logger.WriteWarning(nameof(TileCacheContourSet), $"Polygon contour triangulation error: Index {i} - {cont}");

                    ntris = -ntris;
                }

                // Add and merge vertices.
                for (int j = 0; j < cont.NVerts; ++j)
                {
                    var v = cont.Verts[j];
                    indices[j] = mesh.AddVertex(v.X, v.Y, v.Z, firstVert, nextVert);
                    if ((v.W & 0x80) != 0)
                    {
                        // This vertex should be removed.
                        vflags[indices[j]] = 1;
                    }
                }

                // Build initial polygons.
                int npolys = 0;
                IndexedPolygon[] polys = new IndexedPolygon[maxVertsPerCont];
                for (int j = 0; j < ntris; ++j)
                {
                    var t = tris.ElementAt(j);
                    if (t.X != t.Y && t.X != t.Z && t.Y != t.Z)
                    {
                        polys[npolys] = new IndexedPolygon(NavMeshCreateParams.DT_VERTS_PER_POLYGON);
                        polys[npolys][0] = indices[t.X];
                        polys[npolys][1] = indices[t.Y];
                        polys[npolys][2] = indices[t.Z];
                        npolys++;
                    }
                }
                if (npolys == 0)
                {
                    continue;
                }

                // Merge polygons.
                int maxVertsPerPoly = NavMeshCreateParams.DT_VERTS_PER_POLYGON;
                if (maxVertsPerPoly > 3)
                {
                    while (true)
                    {
                        // Find best polygons to merge.
                        int bestMergeVal = 0;
                        int bestPa = 0, bestPb = 0, bestEa = 0, bestEb = 0;

                        for (int j = 0; j < npolys - 1; ++j)
                        {
                            var pj = polys[j];
                            for (int k = j + 1; k < npolys; ++k)
                            {
                                var pk = polys[k];
                                int v = IndexedPolygon.GetMergeValue(pj, pk, mesh.Verts, out int ea, out int eb);
                                if (v > bestMergeVal)
                                {
                                    bestMergeVal = v;
                                    bestPa = j;
                                    bestPb = k;
                                    bestEa = ea;
                                    bestEb = eb;
                                }
                            }
                        }

                        if (bestMergeVal > 0)
                        {
                            // Found best, merge.
                            polys[bestPa] = IndexedPolygon.Merge(polys[bestPa], polys[bestPb], bestEa, bestEb);
                            polys[bestPb] = polys[npolys - 1].Copy();
                            npolys--;
                        }
                        else
                        {
                            // Could not merge any polygons, stop.
                            break;
                        }
                    }
                }

                // Store polygons.
                for (int j = 0; j < npolys; ++j)
                {
                    var p = new IndexedPolygon(NavMeshCreateParams.DT_VERTS_PER_POLYGON * 2);//Polygon with adjacency
                    var q = polys[j];
                    for (int k = 0; k < NavMeshCreateParams.DT_VERTS_PER_POLYGON; ++k)
                    {
                        p[k] = q[k];
                    }
                    mesh.Polys[mesh.NPolys] = p;
                    mesh.Areas[mesh.NPolys] = (SamplePolyAreas)cont.Area;
                    mesh.NPolys++;
                    if (mesh.NPolys > maxTris)
                    {
                        throw new EngineException(string.Format("rcBuildPolyMesh: Too many polygons {0} (max:{1}).", mesh.NPolys, maxTris));
                    }
                }
            }

            // Remove edge vertices.
            for (int i = 0; i < mesh.NVerts; ++i)
            {
                if (vflags[i] != 0)
                {
                    if (!mesh.CanRemoveVertex(i))
                    {
                        continue;
                    }
                    if (!mesh.RemoveVertex(i, maxTris))
                    {
                        // Failed to remove vertex
                        throw new EngineException(string.Format("Failed to remove edge vertex {0}.", i));
                    }
                    // Remove vertex
                    // Note: mesh.nverts is already decremented inside removeVertex()!
                    // Fixup vertex flags
                    for (int j = i; j < mesh.NVerts; ++j)
                    {
                        vflags[j] = vflags[j + 1];
                    }
                    --i;
                }
            }

            // Calculate adjacency.
            if (!mesh.BuildMeshAdjacency(this))
            {
                throw new EngineException("Adjacency failed.");
            }

            return true;
        }
    }
}
