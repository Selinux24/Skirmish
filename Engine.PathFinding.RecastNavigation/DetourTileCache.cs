using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation
{
    static class DetourTileCache
    {
        #region Constants

        public const int VERTEX_BUCKET_COUNT2 = (1 << 8);
        public const int MAX_REQUESTS = 64;
        public const int MAX_UPDATE = 64;
        public const int MAX_VERTS_PER_POLY = 6;    // TODO: use the DT_VERTS_PER_POLYGON
        public const int MAX_REM_EDGES = 48;        // TODO: make this an expression.
        public const int DT_LAYER_MAX_NEIS = 16;
        public const int DT_MAX_TOUCHED_TILES = 8;
        public const int DT_TILECACHE_MAGIC = 'D' << 24 | 'T' << 16 | 'L' << 8 | 'R'; ///< 'DTLR';
        public const int DT_TILECACHE_VERSION = 1;
        public const int DT_TILECACHE_NULL_AREA = 0;
        public const int DT_TILECACHE_WALKABLE_AREA = 63;
        public const int DT_TILECACHE_NULL_IDX = 0xffff;

        #endregion

        #region DETOURTILECACHEBUILDER

        public static bool BuildTileCacheLayer(int[] heights, TileCacheAreas[] areas, int[] cons, out TileCacheLayerData data)
        {
            data = new TileCacheLayerData()
            {
                heights = heights,
                areas = areas,
                cons = cons,
            };

            return true;
        }
        public static bool DecompressTileCacheLayer(TileCacheLayerHeader header, TileCacheLayerData data, int dataSize, out TileCacheLayer layer)
        {
            layer = new TileCacheLayer()
            {
                Header = header,
                Areas = null,
                Heights = null,
                Cons = null,
                Regs = null,
                RegCount = 0,
            };

            if (data.areas != null && data.areas.Length > 0)
            {
                layer.Areas = new TileCacheAreas[data.areas.Length];
                Array.Copy(data.areas, layer.Areas, data.areas.Length);
            }

            if (data.heights != null && data.heights.Length > 0)
            {
                layer.Heights = new int[data.heights.Length];
                Array.Copy(data.heights, layer.Heights, data.heights.Length);
            }

            if (data.cons != null && data.cons.Length > 0)
            {
                layer.Cons = new int[data.cons.Length];
                Array.Copy(data.cons, layer.Cons, data.cons.Length);
            }

            return true;
        }
        public static bool MarkCylinderArea(NavMeshTileBuildContext tc, Vector3 orig, float cs, float ch, Vector3 pos, float radius, float height, TileCacheAreas areaId)
        {
            Vector3 bmin = new Vector3();
            Vector3 bmax = new Vector3();
            bmin.X = pos.X - radius;
            bmin.Y = pos.Y;
            bmin.Z = pos.Z - radius;
            bmax.X = pos.X + radius;
            bmax.Y = pos.Y + height;
            bmax.Z = pos.Z + radius;
            float r2 = (float)Math.Pow(radius / cs + 0.5f, 2.0f);

            int w = tc.Layer.Header.Width;
            int h = tc.Layer.Header.Height;
            float ics = 1.0f / cs;
            float ich = 1.0f / ch;

            float px = (pos.X - orig.X) * ics;
            float pz = (pos.Z - orig.Z) * ics;

            int minx = (int)Math.Floor((bmin.X - orig.X) * ics);
            int miny = (int)Math.Floor((bmin.Y - orig.Y) * ich);
            int minz = (int)Math.Floor((bmin.Z - orig.Z) * ics);
            int maxx = (int)Math.Floor((bmax.X - orig.X) * ics);
            int maxy = (int)Math.Floor((bmax.Y - orig.Y) * ich);
            int maxz = (int)Math.Floor((bmax.Z - orig.Z) * ics);

            if (maxx < 0) return true;
            if (minx >= w) return true;
            if (maxz < 0) return true;
            if (minz >= h) return true;

            if (minx < 0) minx = 0;
            if (maxx >= w) maxx = w - 1;
            if (minz < 0) minz = 0;
            if (maxz >= h) maxz = h - 1;

            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    float dx = (x + 0.5f) - px;
                    float dz = (z + 0.5f) - pz;
                    if (dx * dx + dz * dz > r2)
                    {
                        continue;
                    }
                    int y = tc.Layer.Heights[x + z * w];
                    if (y < miny || y > maxy)
                    {
                        continue;
                    }
                    tc.Layer.Areas[x + z * w] = areaId;
                }
            }

            return true;
        }
        public static bool MarkBoxArea(NavMeshTileBuildContext tc, Vector3 orig, float cs, float ch, Vector3 center, Vector3 halfExtents, Vector2 rotAux, TileCacheAreas areaId)
        {
            int w = tc.Layer.Header.Width;
            int h = tc.Layer.Header.Height;
            float ics = 1.0f / cs;
            float ich = 1.0f / ch;

            float cx = (center.X - orig.X) * ics;
            float cz = (center.Z - orig.Z) * ics;

            float maxr = 1.41f * Math.Max(halfExtents.X, halfExtents.Z);
            int minx = (int)Math.Floor(cx - maxr * ics);
            int maxx = (int)Math.Floor(cx + maxr * ics);
            int minz = (int)Math.Floor(cz - maxr * ics);
            int maxz = (int)Math.Floor(cz + maxr * ics);
            int miny = (int)Math.Floor((center.Y - halfExtents.Y - orig.Y) * ich);
            int maxy = (int)Math.Floor((center.Y + halfExtents.Y - orig.Y) * ich);

            if (maxx < 0) return true;
            if (minx >= w) return true;
            if (maxz < 0) return true;
            if (minz >= h) return true;

            if (minx < 0) minx = 0;
            if (maxx >= w) maxx = w - 1;
            if (minz < 0) minz = 0;
            if (maxz >= h) maxz = h - 1;

            float xhalf = halfExtents.X * ics + 0.5f;
            float zhalf = halfExtents.Z * ics + 0.5f;

            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    float x2 = 2.0f * (x - cx);
                    float z2 = 2.0f * (z - cz);
                    float xrot = rotAux.Y * x2 + rotAux.X * z2;
                    if (xrot > xhalf || xrot < -xhalf)
                    {
                        continue;
                    }
                    float zrot = rotAux.Y * z2 - rotAux.X * x2;
                    if (zrot > zhalf || zrot < -zhalf)
                    {
                        continue;
                    }
                    int y = tc.Layer.Heights[x + z * w];
                    if (y < miny || y > maxy)
                    {
                        continue;
                    }
                    tc.Layer.Areas[x + z * w] = areaId;
                }
            }

            return true;
        }
        public static bool MarkBoxArea(NavMeshTileBuildContext tc, Vector3 orig, float cs, float ch, Vector3 bmin, Vector3 bmax, TileCacheAreas areaId)
        {
            int w = tc.Layer.Header.Width;
            int h = tc.Layer.Header.Height;
            float ics = 1.0f / cs;
            float ich = 1.0f / ch;

            int minx = (int)Math.Floor((bmin.X - orig.X) * ics);
            int miny = (int)Math.Floor((bmin.Y - orig.Y) * ich);
            int minz = (int)Math.Floor((bmin.Z - orig.Z) * ics);
            int maxx = (int)Math.Floor((bmax.X - orig.X) * ics);
            int maxy = (int)Math.Floor((bmax.Y - orig.Y) * ich);
            int maxz = (int)Math.Floor((bmax.Z - orig.Z) * ics);

            if (maxx < 0) return true;
            if (minx >= w) return true;
            if (maxz < 0) return true;
            if (minz >= h) return true;

            if (minx < 0) minx = 0;
            if (maxx >= w) maxx = w - 1;
            if (minz < 0) minz = 0;
            if (maxz >= h) maxz = h - 1;

            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    int y = tc.Layer.Heights[x + z * w];
                    if (y < miny || y > maxy)
                    {
                        continue;
                    }
                    tc.Layer.Areas[x + z * w] = areaId;
                }
            }

            return true;
        }
        public static bool BuildTileCacheRegions(NavMeshTileBuildContext bc, int walkableClimb)
        {
            int w = bc.Layer.Header.Width;
            int h = bc.Layer.Header.Height;

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
                    if (bc.Layer.Areas[idx] == TileCacheAreas.RC_NULL_AREA)
                    {
                        continue;
                    }

                    int sid = 0xff;

                    // -x
                    int xidx = (x - 1) + y * w;
                    if (x > 0 && IsConnected(bc.Layer, idx, xidx, walkableClimb))
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
                    if (y > 0 && IsConnected(bc.Layer, idx, yidx, walkableClimb))
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
                    Neis = new int[DT_LAYER_MAX_NEIS],
                    NNeis = 0,
                    RegId = 0xff,
                    AreaId = TileCacheAreas.RC_NULL_AREA,
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
                    regs[ri].AreaId = bc.Layer.Areas[idx];

                    // Update neighbours
                    int ymi = x + (y - 1) * w;
                    if (y > 0 && IsConnected(bc.Layer, idx, ymi, walkableClimb))
                    {
                        int rai = layerRegs[ymi];
                        if (rai != 0xff && rai != ri)
                        {
                            AddUniqueLast(ref regs[ri], rai);
                            AddUniqueLast(ref regs[rai], ri);
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

            bc.SetLayerRegs(layerRegs, regId);

            return true;
        }
        public static bool BuildTileCacheContours(NavMeshTileBuildContext bc, int walkableClimb, float maxError)
        {
            int w = bc.Layer.Header.Width;
            int h = bc.Layer.Header.Height;

            var lcset = new TileCacheContourSet
            {
                NConts = bc.Layer.RegCount,
                Conts = new TileCacheContour[bc.Layer.RegCount],
            };

            // Allocate temp buffer for contour tracing.
            int maxTempVerts = (w + h) * 2 * 2; // Twice around the layer.

            var tempVerts = new Int4[maxTempVerts];
            var tempPoly = new Polygoni(maxTempVerts);

            var temp = new TempContour(tempVerts, maxTempVerts, tempPoly, maxTempVerts);

            // Find contours.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    int ri = bc.Layer.Regs[idx];
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
                    cont.Area = bc.Layer.Areas[idx];

                    if (!WalkContour(bc.Layer, x, y, temp))
                    {
                        // Too complex contour.
                        // Note: If you hit here ofte, try increasing 'maxTempVerts'.
                        return false;
                    }

                    SimplifyContour(temp, maxError);

                    // Store contour.
                    cont.NVerts = temp.nverts;
                    if (cont.NVerts > 0)
                    {
                        cont.Verts = new Int4[temp.nverts];

                        for (int i = 0, j = temp.nverts - 1; i < temp.nverts; j = i++)
                        {
                            var v = temp.verts[j];
                            var vn = temp.verts[i];
                            int nei = vn.W; // The neighbour reg is stored at segment vertex of a segment. 
                            bool shouldRemove = false;
                            int lh = GetCornerHeight(bc.Layer, v.X, v.Y, v.Z, walkableClimb, ref shouldRemove);

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

            bc.LCSet = lcset;

            return true;
        }
        public static bool BuildTileCachePolyMesh(NavMeshTileBuildContext bc)
        {
            int maxVertices = 0;
            int maxTris = 0;
            int maxVertsPerCont = 0;
            for (int i = 0; i < bc.LCSet.NConts; ++i)
            {
                // Skip null contours.
                if (bc.LCSet.Conts[i].NVerts < 3) continue;
                maxVertices += bc.LCSet.Conts[i].NVerts;
                maxTris += bc.LCSet.Conts[i].NVerts - 2;
                maxVertsPerCont = Math.Max(maxVertsPerCont, bc.LCSet.Conts[i].NVerts);
            }

            // TODO: warn about too many vertices?

            var mesh = new TileCachePolyMesh
            {
                NVP = Detour.DT_VERTS_PER_POLYGON,
                Verts = Helper.CreateArray(maxVertices, () => new Int3()),
                Polys = new Polygoni[maxTris],
                Areas = new SamplePolyAreas[maxTris],
                Flags = new SamplePolyFlagTypes[maxTris],
                NVerts = 0,
                NPolys = 0
            };

            int[] vflags = new int[maxVertices];
            int[] firstVert = Helper.CreateArray(VERTEX_BUCKET_COUNT2, DT_TILECACHE_NULL_IDX);
            int[] nextVert = Helper.CreateArray(maxVertices, 0);
            int[] indices = new int[maxVertsPerCont];

            for (int i = 0; i < bc.LCSet.NConts; ++i)
            {
                var cont = bc.LCSet.Conts[i];

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

                int ntris = Recast.Triangulate(cont.NVerts, cont.Verts, ref indices, out Int3[] tris);
                if (ntris <= 0)
                {
                    // TODO: issue warning!
                    ntris = -ntris;
                }

                // Add and merge vertices.
                for (int j = 0; j < cont.NVerts; ++j)
                {
                    var v = cont.Verts[j];
                    indices[j] = AddVertex(v.X, v.Y, v.Z, ref mesh, firstVert, nextVert);
                    if ((v.W & 0x80) != 0)
                    {
                        // This vertex should be removed.
                        vflags[indices[j]] = 1;
                    }
                }

                // Build initial polygons.
                int npolys = 0;
                Polygoni[] polys = new Polygoni[maxVertsPerCont];
                for (int j = 0; j < ntris; ++j)
                {
                    var t = tris[j];
                    if (t.X != t.Y && t.X != t.Z && t.Y != t.Z)
                    {
                        polys[npolys] = new Polygoni(Detour.DT_VERTS_PER_POLYGON);
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
                int maxVertsPerPoly = Detour.DT_VERTS_PER_POLYGON;
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
                                int v = Recast.GetPolyMergeValue(pj, pk, mesh.Verts, out int ea, out int eb);
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
                            polys[bestPa] = Recast.MergePolys(polys[bestPa], polys[bestPb], bestEa, bestEb);
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
                    var p = new Polygoni(Detour.DT_VERTS_PER_POLYGON * 2);//Polygon with adjacency
                    var q = polys[j];
                    for (int k = 0; k < Detour.DT_VERTS_PER_POLYGON; ++k)
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
                    if (!CanRemoveVertex(mesh, i))
                    {
                        continue;
                    }
                    if (!RemoveVertex(mesh, i, maxTris))
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
            if (!BuildMeshAdjacency(mesh.Polys, mesh.NPolys, mesh.Verts, mesh.NVerts, bc.LCSet))
            {
                throw new EngineException("Adjacency failed.");
            }

            bc.LMesh = mesh;

            return true;
        }
        public static int GetDirOffsetX(int dir)
        {
            int[] offset = new[] { -1, 0, 1, 0, };
            return offset[dir & 0x03];
        }
        public static int GetDirOffsetY(int dir)
        {
            int[] offset = new[] { 0, 1, 0, -1 };
            return offset[dir & 0x03];
        }
        public static bool OverlapRangeExl(int amin, int amax, int bmin, int bmax)
        {
            return !(amin >= bmax || amax <= bmin);
        }
        public static void AddUniqueLast(ref LayerMonotoneRegion reg, int v)
        {
            int n = reg.NNeis;
            if (n > 0 && reg.Neis[n - 1] == v)
            {
                return;
            }
            reg.Neis[reg.NNeis] = v;
            reg.NNeis++;
        }
        public static bool IsConnected(TileCacheLayer layer, int ia, int ib, int walkableClimb)
        {
            if (layer.Areas[ia] != layer.Areas[ib])
            {
                return false;
            }
            if (Math.Abs(layer.Heights[ia] - layer.Heights[ib]) > walkableClimb)
            {
                return false;
            }
            return true;
        }
        public static bool CanMerge(int oldRegId, int newRegId, LayerMonotoneRegion[] regs, int nregs)
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
        public static bool AppendVertex(TempContour cont, int x, int y, int z, int r)
        {
            // Try to merge with existing segments.
            if (cont.nverts > 1)
            {
                var pa = cont.verts[cont.nverts - 2];
                var pb = cont.verts[cont.nverts - 1];
                if (pb.W == r)
                {
                    if (pa.X == pb.X && pb.X == x)
                    {
                        // The verts are aligned aling x-axis, update z.
                        pb.Y = y;
                        pb.Z = z;
                        cont.verts[cont.nverts - 1] = pb;
                        return true;
                    }
                    else if (pa.Z == pb.Z && pb.Z == z)
                    {
                        // The verts are aligned aling z-axis, update x.
                        pb.X = x;
                        pb.Y = y;
                        cont.verts[cont.nverts - 1] = pb;
                        return true;
                    }
                }
            }

            // Add new point.
            if (cont.nverts + 1 > cont.cverts)
            {
                return false;
            }

            cont.verts[cont.nverts] = new Int4(x, y, z, r);
            cont.nverts++;

            return true;
        }
        public static int GetNeighbourReg(TileCacheLayer layer, int ax, int ay, int dir)
        {
            int w = layer.Header.Width;
            int ia = ax + ay * w;

            int con = layer.Cons[ia] & 0xf;
            int portal = layer.Cons[ia] >> 4;
            int mask = (1 << dir);

            if ((con & mask) == 0)
            {
                // No connection, return portal or hard edge.
                if ((portal & mask) != 0)
                {
                    return 0xf8 + dir;
                }
                return 0xff;
            }

            int bx = ax + Recast.GetDirOffsetX(dir);
            int by = ay + Recast.GetDirOffsetY(dir);
            int ib = bx + by * w;

            return layer.Regs[ib];
        }
        public static bool WalkContour(TileCacheLayer layer, int x, int y, TempContour cont)
        {
            int w = layer.Header.Width;
            int h = layer.Header.Height;

            cont.nverts = 0;

            int startX = x;
            int startY = y;
            int startDir = -1;

            for (int i = 0; i < 4; ++i)
            {
                int dr = (i + 3) & 3;
                int rn = GetNeighbourReg(layer, x, y, dr);
                if (rn != layer.Regs[x + y * w])
                {
                    startDir = dr;
                    break;
                }
            }
            if (startDir == -1)
            {
                return true;
            }

            int dir = startDir;
            int maxIter = w * h;

            int iter = 0;
            while (iter < maxIter)
            {
                int rn = GetNeighbourReg(layer, x, y, dir);

                int nx = x;
                int ny = y;
                int ndir;

                if (rn != layer.Regs[x + y * w])
                {
                    // Solid edge.
                    int px = x;
                    int pz = y;
                    switch (dir)
                    {
                        case 0: pz++; break;
                        case 1: px++; pz++; break;
                        case 2: px++; break;
                    }

                    // Try to merge with previous vertex.
                    if (!AppendVertex(cont, px, layer.Heights[x + y * w], pz, rn))
                    {
                        return false;
                    }

                    ndir = (dir + 1) & 0x3;  // Rotate CW
                }
                else
                {
                    // Move to next.
                    nx = x + Recast.GetDirOffsetX(dir);
                    ny = y + Recast.GetDirOffsetY(dir);
                    ndir = (dir + 3) & 0x3; // Rotate CCW
                }

                if (iter > 0 && x == startX && y == startY && dir == startDir)
                {
                    break;
                }

                x = nx;
                y = ny;
                dir = ndir;

                iter++;
            }

            // Remove last vertex if it is duplicate of the first one.
            var pa = cont.verts[cont.nverts - 1];
            var pb = cont.verts[0];
            if (pa[0] == pb[0] && pa[2] == pb[2])
            {
                cont.nverts--;
            }

            return true;
        }
        public static float DistancePtSeg(int x, int z, int px, int pz, int qx, int qz)
        {
            float pqx = (qx - px);
            float pqz = (qz - pz);
            float dx = (x - px);
            float dz = (z - pz);
            float d = pqx * pqx + pqz * pqz;
            float t = pqx * dx + pqz * dz;
            if (d > 0)
            {
                t /= d;
            }
            if (t < 0)
            {
                t = 0;
            }
            else if (t > 1)
            {
                t = 1;
            }

            dx = px + t * pqx - x;
            dz = pz + t * pqz - z;

            return dx * dx + dz * dz;
        }
        public static void SimplifyContour(TempContour cont, float maxError)
        {
            cont.npoly = 0;

            for (int i = 0; i < cont.nverts; ++i)
            {
                int j = (i + 1) % cont.nverts;
                // Check for start of a wall segment.
                int ra = cont.verts[j].W;
                int rb = cont.verts[i].W;
                if (ra != rb)
                {
                    cont.poly[cont.npoly++] = i;
                }
            }
            if (cont.npoly < 2)
            {
                // If there is no transitions at all,
                // create some initial points for the simplification process. 
                // Find lower-left and upper-right vertices of the contour.
                int llx = cont.verts[0].X;
                int llz = cont.verts[0].Z;
                int lli = 0;
                int urx = cont.verts[0].X;
                int urz = cont.verts[0].Z;
                int uri = 0;
                for (int i = 1; i < cont.nverts; ++i)
                {
                    int x = cont.verts[i].X;
                    int z = cont.verts[i].Z;
                    if (x < llx || (x == llx && z < llz))
                    {
                        llx = x;
                        llz = z;
                        lli = i;
                    }
                    if (x > urx || (x == urx && z > urz))
                    {
                        urx = x;
                        urz = z;
                        uri = i;
                    }
                }
                cont.npoly = 0;
                cont.poly[cont.npoly++] = lli;
                cont.poly[cont.npoly++] = uri;
            }

            // Add points until all raw points are within
            // error tolerance to the simplified shape.
            for (int i = 0; i < cont.npoly;)
            {
                int ii = (i + 1) % cont.npoly;

                int ai = cont.poly[i];
                int ax = cont.verts[ai].X;
                int az = cont.verts[ai].Z;

                int bi = cont.poly[ii];
                int bx = cont.verts[bi].X;
                int bz = cont.verts[bi].Z;

                // Find maximum deviation from the segment.
                float maxd = 0;
                int maxi = -1;
                int ci, cinc, endi;

                // Traverse the segment in lexilogical order so that the
                // max deviation is calculated similarly when traversing
                // opposite segments.
                if (bx > ax || (bx == ax && bz > az))
                {
                    cinc = 1;
                    ci = (ai + cinc) % cont.nverts;
                    endi = bi;
                }
                else
                {
                    cinc = cont.nverts - 1;
                    ci = (bi + cinc) % cont.nverts;
                    endi = ai;
                }

                // Tessellate only outer edges or edges between areas.
                while (ci != endi)
                {
                    float d = Recast.DistancePtSeg(cont.verts[ci].X, cont.verts[ci].Z, ax, az, bx, bz);
                    if (d > maxd)
                    {
                        maxd = d;
                        maxi = ci;
                    }
                    ci = (ci + cinc) % cont.nverts;
                }

                // If the max deviation is larger than accepted error,
                // add new point, else continue to next segment.
                if (maxi != -1 && maxd > (maxError * maxError))
                {
                    cont.npoly++;
                    for (int j = cont.npoly - 1; j > i; --j)
                    {
                        cont.poly[j] = cont.poly[j - 1];
                    }
                    cont.poly[i + 1] = maxi;
                }
                else
                {
                    i++;
                }
            }

            // Remap vertices
            int start = 0;
            for (int i = 1; i < cont.npoly; ++i)
            {
                if (cont.poly[i] < cont.poly[start])
                {
                    start = i;
                }
            }

            cont.nverts = 0;
            for (int i = 0; i < cont.npoly; ++i)
            {
                int j = (start + i) % cont.npoly;
                var src = cont.verts[cont.poly[j]];
                cont.verts[cont.nverts++] = new Int4()
                {
                    X = src.X,
                    Y = src.Y,
                    Z = src.Z,
                    W = src.W,
                };
            }
        }
        public static int GetCornerHeight(TileCacheLayer layer, int x, int y, int z, int walkableClimb, ref bool shouldRemove)
        {
            int w = layer.Header.Width;
            int h = layer.Header.Height;

            int n = 0;

            int portal = 0xf;
            int height = 0;
            int preg = 0xff;
            bool allSameReg = true;

            for (int dz = -1; dz <= 0; ++dz)
            {
                for (int dx = -1; dx <= 0; ++dx)
                {
                    int px = x + dx;
                    int pz = z + dz;
                    if (px >= 0 && pz >= 0 && px < w && pz < h)
                    {
                        int idx = px + pz * w;
                        int lh = layer.Heights[idx];
                        if (Math.Abs(lh - y) <= walkableClimb && layer.Areas[idx] != TileCacheAreas.RC_NULL_AREA)
                        {
                            height = Math.Max(height, lh);
                            portal &= (layer.Cons[idx] >> 4);
                            if (preg != 0xff && preg != layer.Regs[idx])
                            {
                                allSameReg = false;
                            }
                            preg = layer.Regs[idx];
                            n++;
                        }
                    }
                }
            }

            int portalCount = 0;
            for (int dir = 0; dir < 4; ++dir)
            {
                if ((portal & (1 << dir)) != 0)
                {
                    portalCount++;
                }
            }

            shouldRemove = false;
            if (n > 1 && portalCount == 1 && allSameReg)
            {
                shouldRemove = true;
            }

            return height;
        }
        public static int ComputeVertexHash2(int x, int y, int z)
        {
            uint h1 = 0x8da6b343; // Large multiplicative constants
            uint h2 = 0xd8163841; // here arbitrarily chosen primes
            uint h3 = 0xcb1ab31f;
            uint n = (uint)(h1 * x + h2 * y + h3 * z);
            return (int)(n & (VERTEX_BUCKET_COUNT2 - 1));
        }
        public static int AddVertex(int x, int y, int z, ref TileCachePolyMesh mesh, int[] firstVert, int[] nextVert)
        {
            int bucket = ComputeVertexHash2(x, 0, z);
            int i = firstVert[bucket];

            while (i != DT_TILECACHE_NULL_IDX)
            {
                var vx = mesh.Verts[i];
                if (vx.X == x && vx.Z == z && (Math.Abs(vx.Y - y) <= 2))
                {
                    return i;
                }
                i = nextVert[i]; // next
            }

            // Could not find, create new.
            i = mesh.NVerts; mesh.NVerts++;
            mesh.Verts[i] = new Int3(x, y, z);
            nextVert[i] = firstVert[bucket];
            firstVert[bucket] = i;

            return i;
        }
        public static bool BuildMeshAdjacency(Polygoni[] polys, int npolys, Int3[] verts, int nverts, TileCacheContourSet lcset)
        {
            // Based on code by Eric Lengyel from:
            // http://www.terathon.com/code/edges.php

            int maxEdgeCount = npolys * Detour.DT_VERTS_PER_POLYGON;
            int[] firstEdge = new int[nverts];
            int[] nextEdge = new int[maxEdgeCount];
            int edgeCount = 0;

            Edge[] edges = new Edge[maxEdgeCount];

            for (int i = 0; i < nverts; i++)
            {
                firstEdge[i] = DT_TILECACHE_NULL_IDX;
            }
            for (int i = 0; i < maxEdgeCount; i++)
            {
                nextEdge[i] = DT_TILECACHE_NULL_IDX;
            }

            for (int i = 0; i < npolys; ++i)
            {
                var t = polys[i];
                for (int j = 0; j < Detour.DT_VERTS_PER_POLYGON; ++j)
                {
                    if (t[j] == DT_TILECACHE_NULL_IDX) break;
                    int v0 = t[j];
                    int v1 = (j + 1 >= Detour.DT_VERTS_PER_POLYGON || t[j + 1] == DT_TILECACHE_NULL_IDX) ? t[0] : t[j + 1];
                    if (v0 < v1)
                    {
                        Edge edge = new Edge()
                        {
                            Vert = new int[2],
                            PolyEdge = new int[2],
                            Poly = new int[2],
                        };
                        edge.Vert[0] = v0;
                        edge.Vert[1] = v1;
                        edge.Poly[0] = i;
                        edge.PolyEdge[0] = j;
                        edge.Poly[1] = i;
                        edge.PolyEdge[1] = 0xff;
                        edges[edgeCount] = edge;
                        // Insert edge
                        nextEdge[edgeCount] = firstEdge[v0];
                        firstEdge[v0] = edgeCount;
                        edgeCount++;
                    }
                }
            }

            for (int i = 0; i < npolys; ++i)
            {
                var t = polys[i];
                for (int j = 0; j < Detour.DT_VERTS_PER_POLYGON; ++j)
                {
                    if (t[j] == DT_TILECACHE_NULL_IDX) break;
                    int v0 = t[j];
                    int v1 = (j + 1 >= Detour.DT_VERTS_PER_POLYGON || t[j + 1] == DT_TILECACHE_NULL_IDX) ? t[0] : t[j + 1];
                    if (v0 > v1)
                    {
                        bool found = false;
                        for (int e = firstEdge[v1]; e != DT_TILECACHE_NULL_IDX; e = nextEdge[e])
                        {
                            Edge edge = edges[e];
                            if (edge.Vert[1] == v0 && edge.Poly[0] == edge.Poly[1])
                            {
                                edge.Poly[1] = i;
                                edge.PolyEdge[1] = j;
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            // Matching edge not found, it is an open edge, add it.
                            Edge edge = new Edge()
                            {
                                Vert = new int[2],
                                PolyEdge = new int[2],
                                Poly = new int[2],
                            };
                            edge.Vert[0] = v1;
                            edge.Vert[1] = v0;
                            edge.Poly[0] = i;
                            edge.PolyEdge[0] = j;
                            edge.Poly[1] = i;
                            edge.PolyEdge[1] = 0xff;
                            edges[edgeCount] = edge;
                            // Insert edge
                            nextEdge[edgeCount] = firstEdge[v1];
                            firstEdge[v1] = edgeCount;
                            edgeCount++;
                        }
                    }
                }
            }

            // Mark portal edges.
            for (int i = 0; i < lcset.NConts; ++i)
            {
                TileCacheContour cont = lcset.Conts[i];
                if (cont.NVerts < 3)
                {
                    continue;
                }

                for (int j = 0, k = cont.NVerts - 1; j < cont.NVerts; k = j++)
                {
                    var va = cont.Verts[k];
                    var vb = cont.Verts[j];
                    int dir = va.W & 0xf;
                    if (dir == 0xf)
                    {
                        continue;
                    }

                    if (dir == 0 || dir == 2)
                    {
                        // Find matching vertical edge
                        int x = va.X;
                        int zmin = va.Z;
                        int zmax = vb.Z;
                        if (zmin > zmax)
                        {
                            Helper.Swap(ref zmin, ref zmax);
                        }

                        for (int m = 0; m < edgeCount; ++m)
                        {
                            Edge e = edges[m];
                            // Skip connected edges.
                            if (e.Poly[0] != e.Poly[1])
                            {
                                continue;
                            }
                            var eva = verts[e.Vert[0]];
                            var evb = verts[e.Vert[1]];
                            if (eva.X == x && evb.X == x)
                            {
                                int ezmin = eva.Z;
                                int ezmax = evb.Z;
                                if (ezmin > ezmax)
                                {
                                    Helper.Swap(ref ezmin, ref ezmax);
                                }
                                if (OverlapRangeExl(zmin, zmax, ezmin, ezmax))
                                {
                                    // Reuse the other polyedge to store dir.
                                    e.PolyEdge[1] = dir;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Find matching vertical edge
                        int z = va.Z;
                        int xmin = va.X;
                        int xmax = vb.X;
                        if (xmin > xmax)
                        {
                            Helper.Swap(ref xmin, ref xmax);
                        }
                        for (int m = 0; m < edgeCount; ++m)
                        {
                            Edge e = edges[m];
                            // Skip connected edges.
                            if (e.Poly[0] != e.Poly[1])
                            {
                                continue;
                            }
                            var eva = verts[e.Vert[0]];
                            var evb = verts[e.Vert[1]];
                            if (eva.Z == z && evb.Z == z)
                            {
                                int exmin = eva.X;
                                int exmax = evb.X;
                                if (exmin > exmax)
                                {
                                    Helper.Swap(ref exmin, ref exmax);
                                }
                                if (OverlapRangeExl(xmin, xmax, exmin, exmax))
                                {
                                    // Reuse the other polyedge to store dir.
                                    e.PolyEdge[1] = dir;
                                }
                            }
                        }
                    }
                }
            }

            // Store adjacency
            for (int i = 0; i < edgeCount; ++i)
            {
                Edge e = edges[i];
                if (e.Poly[0] != e.Poly[1])
                {
                    var p0 = polys[e.Poly[0]];
                    var p1 = polys[e.Poly[1]];
                    p0[Detour.DT_VERTS_PER_POLYGON + e.PolyEdge[0]] = e.Poly[1];
                    p1[Detour.DT_VERTS_PER_POLYGON + e.PolyEdge[1]] = e.Poly[0];
                }
                else if (e.PolyEdge[1] != 0xff)
                {
                    var p0 = polys[e.Poly[0]];
                    p0[Detour.DT_VERTS_PER_POLYGON + e.PolyEdge[0]] = 0x8000 | e.PolyEdge[1];
                }
            }

            return true;
        }
        private static bool CanRemoveVertex(TileCachePolyMesh mesh, int rem)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = 0;
            int numTouchedVerts = 0;
            int numRemainingEdges = 0;
            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];
                int nv = Recast.CountPolyVerts(p);
                int numRemoved = 0;
                int numVerts = 0;
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] == rem)
                    {
                        numTouchedVerts++;
                        numRemoved++;
                    }
                    numVerts++;
                }
                if (numRemoved != 0)
                {
                    numRemovedVerts += numRemoved;
                    numRemainingEdges += numVerts - (numRemoved + 1);
                }
            }

            // There would be too few edges remaining to create a polygon.
            // This can happen for example when a tip of a triangle is marked
            // as deletion, but there are no other polys that share the vertex.
            // In this case, the vertex should not be removed.
            if (numRemainingEdges <= 2)
            {
                return false;
            }

            // Check that there is enough memory for the test.
            int maxEdges = numTouchedVerts * 2;
            if (maxEdges > MAX_REM_EDGES)
            {
                return false;
            }

            // Find edges which share the removed vertex.
            Int3[] edges = new Int3[MAX_REM_EDGES];
            int nedges = 0;

            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];
                int nv = Recast.CountPolyVerts(p);

                // Collect edges which touches the removed vertex.
                for (int j = 0, k = nv - 1; j < nv; k = j++)
                {
                    if (p[j] == rem || p[k] == rem)
                    {
                        // Arrange edge so that a=rem.
                        int a = p[j], b = p[k];
                        if (b == rem)
                        {
                            Helper.Swap(ref a, ref b);
                        }

                        // Check if the edge exists
                        bool exists = false;
                        for (int m = 0; m < nedges; ++m)
                        {
                            var e = edges[m];
                            if (e.Y == b)
                            {
                                // Exists, increment vertex share count.
                                e.Z++;
                                exists = true;
                            }
                            edges[m] = e;
                        }
                        // Add new edge.
                        if (!exists)
                        {
                            edges[nedges] = new Int3(a, b, 1);
                            nedges++;
                        }
                    }
                }
            }

            // There should be no more than 2 open edges.
            // This catches the case that two non-adjacent polygons
            // share the removed vertex. In that case, do not remove the vertex.
            int numOpenEdges = 0;
            for (int i = 0; i < nedges; ++i)
            {
                if (edges[i].Z < 2)
                {
                    numOpenEdges++;
                }
            }
            if (numOpenEdges > 2)
            {
                return false;
            }

            return true;
        }
        private static bool RemoveVertex(TileCachePolyMesh mesh, int rem, int maxTris)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = 0;
            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];
                int nv = Recast.CountPolyVerts(p);
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] == rem)
                    {
                        numRemovedVerts++;
                    }
                }
            }

            int nedges = 0;
            Int3[] edges = new Int3[MAX_REM_EDGES];
            int nhole = 0;
            int[] hole = new int[MAX_REM_EDGES];
            int nharea = 0;
            SamplePolyAreas[] harea = new SamplePolyAreas[MAX_REM_EDGES];

            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];
                int nv = Recast.CountPolyVerts(p);
                bool hasRem = false;
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] == rem) hasRem = true;
                }
                if (hasRem)
                {
                    // Collect edges which does not touch the removed vertex.
                    for (int j = 0, k = nv - 1; j < nv; k = j++)
                    {
                        if (p[j] != rem && p[k] != rem)
                        {
                            if (nedges >= MAX_REM_EDGES)
                            {
                                return false;
                            }
                            var e = new Int3(p[k], p[j], (int)mesh.Areas[i]);
                            edges[nedges] = e;
                            nedges++;
                        }
                    }
                    // Remove the polygon.
                    mesh.Polys[i] = mesh.Polys[mesh.NPolys - 1];
                    mesh.Polys[mesh.NPolys - 1] = null;
                    mesh.Areas[i] = mesh.Areas[mesh.NPolys - 1];
                    mesh.NPolys--;
                    --i;
                }
            }

            // Remove vertex.
            for (int i = rem; i < mesh.NVerts; ++i)
            {
                mesh.Verts[i] = mesh.Verts[(i + 1)];
            }
            mesh.NVerts--;

            // Adjust indices to match the removed vertex layout.
            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];
                int nv = Recast.CountPolyVerts(p);
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] > rem) p[j]--;
                }
            }
            for (int i = 0; i < nedges; ++i)
            {
                if (edges[i].X > rem) edges[i].X--;
                if (edges[i].Y > rem) edges[i].Y--;
            }

            if (nedges == 0)
            {
                return true;
            }

            // Start with one vertex, keep appending connected
            // segments to the start and end of the hole.
            Recast.PushBack(edges[0].X, hole, ref nhole);
            Recast.PushBack((SamplePolyAreas)edges[0].Z, harea, ref nharea);

            while (nedges != 0)
            {
                bool match = false;

                for (int i = 0; i < nedges; ++i)
                {
                    int ea = edges[i].X;
                    int eb = edges[i].Y;

                    SamplePolyAreas a = (SamplePolyAreas)edges[i].Z;
                    bool add = false;
                    if (hole[0] == eb)
                    {
                        // The segment matches the beginning of the hole boundary.
                        if (nhole >= MAX_REM_EDGES)
                        {
                            return false;
                        }
                        Recast.PushFront(ea, hole, ref nhole);

                        Recast.PushFront(a, harea, ref nharea);
                        add = true;
                    }
                    else if (hole[nhole - 1] == ea)
                    {
                        // The segment matches the end of the hole boundary.
                        if (nhole >= MAX_REM_EDGES)
                        {
                            return false;
                        }
                        Recast.PushBack(eb, hole, ref nhole);

                        Recast.PushBack(a, harea, ref nharea);
                        add = true;
                    }
                    if (add)
                    {
                        // The edge segment was added, remove it.
                        edges[i] = edges[(nedges - 1)];
                        nedges--;
                        match = true;
                        i--;
                    }
                }

                if (!match)
                {
                    break;
                }
            }

            var tverts = new Int4[MAX_REM_EDGES];
            var thole = new int[MAX_REM_EDGES];

            // Generate temp vertex array for triangulation.
            for (int i = 0; i < nhole; ++i)
            {
                int pi = hole[i];
                tverts[i].X = mesh.Verts[pi].X;
                tverts[i].Y = mesh.Verts[pi].Y;
                tverts[i].Z = mesh.Verts[pi].Z;
                tverts[i].W = 0;
                thole[i] = i;
            }

            // Triangulate the hole.
            int ntris = Recast.Triangulate(nhole, tverts, ref thole, out Int3[] tris);
            if (ntris < 0)
            {
                // TODO: issue warning!
                ntris = -ntris;
            }

            if (ntris > MAX_REM_EDGES)
            {
                return false;
            }

            // Merge the hole triangles back to polygons.
            var polys = new Polygoni[MAX_REM_EDGES];

            var pareas = new SamplePolyAreas[MAX_REM_EDGES];

            // Build initial polygons.
            int npolys = 0;
            for (int j = 0; j < ntris; ++j)
            {
                var t = tris[j];
                if (t.X != t.Y && t.X != t.Z && t.Y != t.Z)
                {
                    polys[npolys][0] = hole[t.X];
                    polys[npolys][1] = hole[t.Y];
                    polys[npolys][2] = hole[t.Z];

                    pareas[npolys] = harea[t.X];
                    npolys++;
                }
            }
            if (npolys == 0)
            {
                return true;
            }

            // Merge polygons.
            int maxVertsPerPoly = Detour.DT_VERTS_PER_POLYGON;
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
                            int v = Recast.GetPolyMergeValue(pj, pk, mesh.Verts, out int ea, out int eb);
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
                        polys[bestPa] = Recast.MergePolys(polys[bestPa], polys[bestPb], bestEa, bestEb);
                        polys[bestPb] = polys[npolys - 1];
                        pareas[bestPb] = pareas[npolys - 1];
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
            for (int i = 0; i < npolys; ++i)
            {
                if (mesh.NPolys >= maxTris) break;
                var p = mesh.Polys[mesh.NPolys];
                for (int j = 0; j < Detour.DT_VERTS_PER_POLYGON; ++j)
                {
                    p[j] = polys[i][j];
                }

                mesh.Areas[mesh.NPolys] = pareas[i];
                mesh.NPolys++;
                if (mesh.NPolys > maxTris)
                {
                    Console.WriteLine($"removeVertex: Too many polygons {mesh.NPolys} (max:{maxTris}).");
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region DETOURTILECAHE

        public static int ComputeTileHash(int x, int y, int mask)
        {
            uint h1 = 0x8da6b343; // Large multiplicative constants
            uint h2 = 0xd8163841; // here arbitrarily chosen primes
            uint n = h1 * (uint)x + h2 * (uint)y;
            return (int)(n & mask);
        }
        public static bool Contains(CompressedTile[] a, int n, CompressedTile v)
        {
            for (int i = 0; i < n; ++i)
            {
                if (a[i] == v) return true;
            }

            return false;
        }

        #endregion
    }
}
