using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    public class NavMeshTileBuildContext
    {
        public TileCacheLayer Layer { get; set; }
        public TileCacheContourSet LCSet { get; set; }
        public TileCachePolyMesh LMesh { get; set; }

        private static bool CanMerge(int oldRegId, int newRegId, LayerMonotoneRegion[] regs, int nregs)
        {
            int count = 0;
            for (int i = 0; i < nregs; ++i)
            {
                LayerMonotoneRegion reg = regs[i];
                if (reg.RegId != oldRegId)
                {
                    continue;
                }
                int nnei = reg.NNeis;
                for (int j = 0; j < nnei; ++j)
                {
                    if (regs[reg.Neis[j]].RegId == newRegId)
                    {
                        count++;
                    }
                }
            }
            return count == 1;
        }

        public bool BuildTileCacheRegions(int walkableClimb)
        {
            int w = Layer.Header.Width;
            int h = Layer.Header.Height;

            var layerRegs = Helper.CreateArray(w * h, 0xff);

            int nsweeps = w;
            LayerSweepSpan[] sweeps = new LayerSweepSpan[nsweeps];

            // Partition walkable area into monotone regions.
            int[] prevCount = new int[256];
            int regId = 0;

            for (int y = 0; y < h; ++y)
            {
                if (regId > 0)
                {
                    for (int i = 0; i < regId; i++)
                    {
                        prevCount[i] = 0;
                    }
                }
                int sweepId = 0;

                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    if (Layer.Areas[idx] == AreaTypes.RC_NULL_AREA)
                    {
                        continue;
                    }

                    int sid = 0xff;

                    // -x
                    int xidx = (x - 1) + y * w;
                    if (x > 0 && Layer.IsConnected(idx, xidx, walkableClimb))
                    {
                        int layerReg = layerRegs[xidx];
                        if (layerReg != 0xff)
                        {
                            sid = layerReg;
                        }
                    }

                    if (sid == 0xff)
                    {
                        sid = sweepId++;
                        sweeps[sid].Nei = 0xff;
                        sweeps[sid].NS = 0;
                    }

                    // -y
                    int yidx = x + (y - 1) * w;
                    if (y > 0 && Layer.IsConnected(idx, yidx, walkableClimb))
                    {
                        int nr = layerRegs[yidx];
                        if (nr != 0xff)
                        {
                            // Set neighbour when first valid neighbour is encoutered.
                            if (sweeps[sid].NS == 0)
                            {
                                sweeps[sid].Nei = nr;
                            }

                            if (sweeps[sid].Nei == nr)
                            {
                                // Update existing neighbour
                                sweeps[sid].NS++;
                                prevCount[nr]++;
                            }
                            else
                            {
                                // This is hit if there is nore than one neighbour.
                                // Invalidate the neighbour.
                                sweeps[sid].Nei = 0xff;
                            }
                        }
                    }

                    layerRegs[idx] = sid;
                }

                // Create unique ID.
                for (int i = 0; i < sweepId; ++i)
                {
                    // If the neighbour is set and there is only one continuous connection to it,
                    // the sweep will be merged with the previous one, else new region is created.
                    if (sweeps[i].Nei != 0xff && prevCount[sweeps[i].Nei] == sweeps[i].NS)
                    {
                        sweeps[i].Id = sweeps[i].Nei;
                    }
                    else
                    {
                        if (regId == 255)
                        {
                            // Region ID's overflow.
                            return false;
                        }
                        sweeps[i].Id = regId++;
                    }
                }

                // Remap local sweep ids to region ids.
                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    if (layerRegs[idx] != 0xff)
                    {
                        layerRegs[idx] = sweeps[layerRegs[idx]].Id;
                    }
                }
            }

            // Allocate and init layer regions.
            int nregs = regId;
            LayerMonotoneRegion[] regs = Helper.CreateArray(nregs, () =>
            {
                return new LayerMonotoneRegion()
                {
                    Area = 0,
                    Neis = new int[LayerMonotoneRegion.DT_LAYER_MAX_NEIS],
                    NNeis = 0,
                    RegId = 0xff,
                    AreaId = AreaTypes.RC_NULL_AREA,
                };
            });

            // Find region neighbours.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    int ri = layerRegs[idx];
                    if (ri == 0xff)
                    {
                        continue;
                    }

                    // Update area.
                    regs[ri].Area++;
                    regs[ri].AreaId = Layer.Areas[idx];

                    // Update neighbours
                    int ymi = x + (y - 1) * w;
                    if (y > 0 && Layer.IsConnected(idx, ymi, walkableClimb))
                    {
                        int rai = layerRegs[ymi];
                        if (rai != 0xff && rai != ri)
                        {
                            regs[ri].AddUniqueLast(rai);
                            regs[rai].AddUniqueLast(ri);
                        }
                    }
                }
            }

            for (int i = 0; i < nregs; ++i)
            {
                regs[i].RegId = i;
            }

            for (int i = 0; i < nregs; ++i)
            {
                LayerMonotoneRegion reg = regs[i];

                int merge = -1;
                int mergea = 0;
                for (int j = 0; j < reg.NNeis; ++j)
                {
                    int nei = reg.Neis[j];
                    LayerMonotoneRegion regn = regs[nei];
                    if (reg.RegId == regn.RegId)
                    {
                        continue;
                    }
                    if (reg.AreaId != regn.AreaId)
                    {
                        continue;
                    }
                    if (regn.Area > mergea && CanMerge(reg.RegId, regn.RegId, regs, nregs))
                    {
                        mergea = regn.Area;
                        merge = nei;
                    }
                }
                if (merge != -1)
                {
                    int oldId = reg.RegId;
                    int newId = regs[merge].RegId;
                    for (int j = 0; j < nregs; ++j)
                    {
                        if (regs[j].RegId == oldId)
                        {
                            regs[j].RegId = newId;
                        }
                    }
                }
            }

            // Compact ids.
            int[] remap = Helper.CreateArray(256, 0);
            // Find number of unique regions.
            regId = 0;
            for (int i = 0; i < nregs; ++i)
            {
                remap[regs[i].RegId] = 1;
            }
            for (int i = 0; i < 256; ++i)
            {
                if (remap[i] != 0x00)
                {
                    remap[i] = regId++;
                }
            }
            // Remap ids.
            for (int i = 0; i < nregs; ++i)
            {
                regs[i].RegId = remap[regs[i].RegId];
            }

            for (int i = 0; i < w * h; ++i)
            {
                if (layerRegs[i] != 0xff)
                {
                    layerRegs[i] = regs[layerRegs[i]].RegId;
                }
            }

            SetLayerRegs(layerRegs, regId);

            return true;
        }
        public bool BuildTileCacheContours(int walkableClimb, float maxError)
        {
            int w = Layer.Header.Width;
            int h = Layer.Header.Height;

            var lcset = new TileCacheContourSet
            {
                NConts = Layer.RegCount,
                Conts = new TileCacheContour[Layer.RegCount],
            };

            // Allocate temp buffer for contour tracing.
            int maxTempVerts = (w + h) * 2 * 2; // Twice around the layer.

            var tempVerts = new Int4[maxTempVerts];
            var tempPoly = new IndexedPolygon(maxTempVerts);

            var temp = new TempContour(tempVerts, maxTempVerts, tempPoly, maxTempVerts);

            // Find contours.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    int ri = Layer.Regs[idx];
                    if (ri == 0xff)
                    {
                        continue;
                    }

                    var cont = lcset.Conts[ri];

                    if (cont.NVerts > 0)
                    {
                        continue;
                    }

                    cont.Reg = ri;
                    cont.Area = Layer.Areas[idx];

                    if (!Layer.WalkContour(x, y, temp))
                    {
                        // Too complex contour.
                        // Note: If you hit here ofte, try increasing 'maxTempVerts'.
                        return false;
                    }

                    temp.SimplifyContour(maxError);

                    // Store contour.
                    cont.NVerts = temp.Nverts;
                    if (cont.NVerts > 0)
                    {
                        cont.Verts = new Int4[temp.Nverts];

                        for (int i = 0, j = temp.Nverts - 1; i < temp.Nverts; j = i++)
                        {
                            var v = temp.Verts[j];
                            var vn = temp.Verts[i];
                            int nei = vn.W; // The neighbour reg is stored at segment vertex of a segment. 
                            bool shouldRemove = false;
                            int lh = Layer.GetCornerHeight(v.X, v.Y, v.Z, walkableClimb, ref shouldRemove);

                            var dst = new Int4()
                            {
                                X = v.X,
                                Y = lh,
                                Z = v.Z,
                                W = 0x0f,
                            };

                            // Store portal direction and remove status to the fourth component.
                            if (nei != 0xff && nei >= 0xf8)
                            {
                                dst.W = nei - 0xf8;
                            }
                            if (shouldRemove)
                            {
                                dst.W |= 0x80;
                            }

                            cont.Verts[j] = dst;
                        }
                    }

                    lcset.Conts[ri] = cont;
                }
            }

            LCSet = lcset;

            return true;
        }
        public bool BuildTileCachePolyMesh()
        {
            int maxVertices = 0;
            int maxTris = 0;
            int maxVertsPerCont = 0;
            for (int i = 0; i < LCSet.NConts; ++i)
            {
                // Skip null contours.
                if (LCSet.Conts[i].NVerts < 3) continue;
                maxVertices += LCSet.Conts[i].NVerts;
                maxTris += LCSet.Conts[i].NVerts - 2;
                maxVertsPerCont = Math.Max(maxVertsPerCont, LCSet.Conts[i].NVerts);
            }

            var mesh = new TileCachePolyMesh
            {
                NVP = DetourUtils.DT_VERTS_PER_POLYGON,
                Verts = Helper.CreateArray(maxVertices, () => new Int3()),
                Polys = new IndexedPolygon[maxTris],
                Areas = new SamplePolyAreas[maxTris],
                Flags = new SamplePolyFlagTypes[maxTris],
                NVerts = 0,
                NPolys = 0
            };

            int[] vflags = new int[maxVertices];
            int[] firstVert = Helper.CreateArray(TileCache.VERTEX_BUCKET_COUNT2, TileCache.DT_TILECACHE_NULL_IDX);
            int[] nextVert = Helper.CreateArray(maxVertices, 0);
            int[] indices = new int[maxVertsPerCont];

            for (int i = 0; i < LCSet.NConts; ++i)
            {
                var cont = LCSet.Conts[i];

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

                int ntris = TriangulateHelper.Triangulate(cont.Verts, ref indices, out var tris);
                if (ntris <= 0)
                {
                    Logger.WriteWarning(nameof(NavMeshTileBuildContext), $"Polygon contour triangulation error: Index {i} - {cont}");

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
                    var t = tris[j];
                    if (t.X != t.Y && t.X != t.Z && t.Y != t.Z)
                    {
                        polys[npolys] = new IndexedPolygon(DetourUtils.DT_VERTS_PER_POLYGON);
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
                int maxVertsPerPoly = DetourUtils.DT_VERTS_PER_POLYGON;
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
                    var p = new IndexedPolygon(DetourUtils.DT_VERTS_PER_POLYGON * 2);//Polygon with adjacency
                    var q = polys[j];
                    for (int k = 0; k < DetourUtils.DT_VERTS_PER_POLYGON; ++k)
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
                    if (!IndexedPolygon.CanRemoveVertex(mesh.Polys, mesh.NPolys, i, TileCachePolyMesh.MAX_REM_EDGES))
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
            if (!mesh.BuildMeshAdjacency(LCSet))
            {
                throw new EngineException("Adjacency failed.");
            }

            LMesh = mesh;

            return true;
        }
        private void SetLayerRegs(int[] layerRegs, int regId)
        {
            var layer = Layer;
            layer.RegCount = regId;
            layer.Regs = layerRegs;
            Layer = layer;
        }
    }
}
