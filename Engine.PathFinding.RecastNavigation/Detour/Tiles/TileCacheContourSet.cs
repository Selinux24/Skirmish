using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache contour set
    /// </summary>
    public struct TileCacheContourSet
    {
        /// <summary>
        /// Border vertex flag.
        /// If a region ID has this bit set, then the associated element lies on
        /// a tile border. If a contour vertex's region ID has this bit set, the 
        /// vertex will later be removed in order to match the segments and vertices 
        /// at tile boundaries.
        /// (Used during the build process.)
        /// </summary>
        public const int BORDER_VERTEX = 0x80;
        /// <summary>
        /// Maximum neighbours
        /// </summary>
        const int MAX_NEIS = 16;

        /// <summary>
        /// Number of contours
        /// </summary>
        public int NConts { get; set; }
        /// <summary>
        /// Contour list
        /// </summary>
        public TileCacheContour[] Conts { get; set; }

        /// <summary>
        /// Builds a new contour set
        /// </summary>
        /// <param name="tcl">Tile cache layer</param>
        /// <param name="walkableClimb">Walkable climb value</param>
        /// <param name="maxError">Maximum error value</param>
        /// <param name="cset">Resulting contour set</param>
        public static bool Build(TileCacheLayer tcl, int walkableClimb, float maxError, out TileCacheContourSet cset)
        {
            int w = tcl.Header.Width;
            int h = tcl.Header.Height;

            var lcset = new TileCacheContourSet
            {
                NConts = tcl.RegCount,
                Conts = new TileCacheContour[tcl.RegCount],
            };

            // Allocate temp buffer for contour tracing.
            int maxTempVerts = (w + h) * 2 * 2; // Twice around the layer.

            var tempVerts = new VertexWithNeigbour[maxTempVerts];
            var tempPoly = new IndexedPolygon(maxTempVerts);

            var temp = new TempContour(tempVerts, maxTempVerts, tempPoly);

            // Find contours.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    int ri = tcl.Regs[idx];
                    if (ri == 0xff)
                    {
                        continue;
                    }

                    var cont = lcset.Conts[ri];
                    if (cont.NVertices > 0)
                    {
                        continue;
                    }

                    cont.Reg = ri;
                    cont.Area = tcl.Areas[idx];

                    if (!tcl.WalkContour(x, y, temp))
                    {
                        // Too complex contour.
                        // Note: If you hit here often, try increasing 'maxTempVerts'.
                        cset = new();
                        return false;
                    }

                    var verts = temp.SimplifyContour(maxError);
                    int nverts = verts.Length;

                    // Store contour.
                    cont.StoreVerts(verts, nverts, tcl, walkableClimb);

                    lcset.Conts[ri] = cont;
                }
            }

            cset = lcset;
            return true;
        }
        /// <summary>
        /// Builds the region id list
        /// </summary>
        /// <param name="tcl">Tile cache layer</param>
        /// <param name="walkableClimb">Walkable climb value</param>
        /// <param name="layerRegs">Resulting layer regions</param>
        /// <param name="regId">Last region id</param>
        public static bool BuildRegions(TileCacheLayer tcl, int walkableClimb, out int[] layerRegs, out int regId)
        {
            int w = tcl.Header.Width;
            int h = tcl.Header.Height;

            layerRegs = Helper.CreateArray(w * h, 0xff);

            int nsweeps = w;
            LayerSweepSpan[] sweeps = new LayerSweepSpan[nsweeps];

            // Partition walkable area into monotone regions.
            int[] prevCount = new int[256];
            regId = 0;

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
                    if (tcl.Areas[idx] == AreaTypes.RC_NULL_AREA)
                    {
                        continue;
                    }

                    int sid = 0xff;

                    // -x
                    int xidx = (x - 1) + y * w;
                    if (x > 0 && tcl.IsConnected(idx, xidx, walkableClimb))
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
                    if (y > 0 && tcl.IsConnected(idx, yidx, walkableClimb))
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
                    Neis = new int[MAX_NEIS],
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
                    regs[ri].AreaId = tcl.Areas[idx];

                    // Update neighbours
                    int ymi = x + (y - 1) * w;
                    if (y > 0 && tcl.IsConnected(idx, ymi, walkableClimb))
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

            return true;
        }
        /// <summary>
        /// Gets whether two regions can merge or not
        /// </summary>
        /// <param name="oldRegId">Old region id</param>
        /// <param name="newRegId">New region id</param>
        /// <param name="regs">Region list</param>
        /// <param name="nregs">Number of regions in the list</param>
        private static bool CanMerge(int oldRegId, int newRegId, LayerMonotoneRegion[] regs, int nregs)
        {
            int count = 0;
            for (int i = 0; i < nregs; ++i)
            {
                var reg = regs[i];
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

        /// <summary>
        /// Gets the geometry configuration of the contour set
        /// </summary>
        /// <param name="maxVertices">Maximum vertices</param>
        /// <param name="maxTris">Maximum triangles</param>
        /// <param name="maxVertsPerCont">Maximum vertices per contour</param>
        public readonly void GetGeometryConfiguration(out int maxVertices, out int maxTris, out int maxVertsPerCont)
        {
            maxVertices = 0;
            maxTris = 0;
            maxVertsPerCont = 0;

            for (int i = 0; i < NConts; ++i)
            {
                var nverts = Conts[i].NVertices;

                // Skip null contours.
                if (nverts < 3)
                {
                    continue;
                }

                maxVertices += nverts;
                maxTris += nverts - 2;
                maxVertsPerCont = Math.Max(maxVertsPerCont, nverts);
            }
        }
    }
}
