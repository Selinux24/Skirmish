using Engine.PathFinding.RecastNavigation.Recast;
using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache layer
    /// </summary>
    public struct TileCacheLayer
    {
        const int DT_LAYER_MAX_NEIS = 16;

        /// <summary>
        /// Header
        /// </summary>
        public TileCacheLayerHeader Header { get; set; }
        /// <summary>
        /// Region count.
        /// </summary>
        public int RegCount { get; set; }
        /// <summary>
        /// Height map
        /// </summary>
        public int[] Heights { get; set; }
        /// <summary>
        /// Areas
        /// </summary>
        public AreaTypes[] Areas { get; set; }
        /// <summary>
        /// Connections
        /// </summary>
        public int[] Cons { get; set; }
        /// <summary>
        /// Regions
        /// </summary>
        public int[] Regs { get; set; }

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

        public bool BuildTileCacheRegions(int walkableClimb, out int[] layerRegs, out int regId)
        {
            int w = Header.Width;
            int h = Header.Height;

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
                    if (Areas[idx] == AreaTypes.RC_NULL_AREA)
                    {
                        continue;
                    }

                    int sid = 0xff;

                    // -x
                    int xidx = (x - 1) + y * w;
                    if (x > 0 && IsConnected(idx, xidx, walkableClimb))
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
                    if (y > 0 && IsConnected(idx, yidx, walkableClimb))
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
                    regs[ri].AreaId = Areas[idx];

                    // Update neighbours
                    int ymi = x + (y - 1) * w;
                    if (y > 0 && IsConnected(idx, ymi, walkableClimb))
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
        public bool BuildTileCacheContours(int walkableClimb, float maxError, out TileCacheContourSet lcset)
        {
            int w = Header.Width;
            int h = Header.Height;

            lcset = new TileCacheContourSet
            {
                NConts = RegCount,
                Conts = new TileCacheContour[RegCount],
            };

            // Allocate temp buffer for contour tracing.
            int maxTempVerts = (w + h) * 2 * 2; // Twice around the layer.

            var tempVerts = new Int4[maxTempVerts];
            var tempPoly = new IndexedPolygon(maxTempVerts);

            var temp = new TempContour(tempVerts, maxTempVerts, tempPoly);

            // Find contours.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    int ri = Regs[idx];
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
                    cont.Area = Areas[idx];

                    if (!WalkContour(x, y, temp))
                    {
                        // Too complex contour.
                        // Note: If you hit here ofte, try increasing 'maxTempVerts'.
                        return false;
                    }

                    var verts = temp.SimplifyContour(maxError);
                    int nverts = verts.Length;

                    // Store contour.
                    cont.NVerts = nverts;
                    if (cont.NVerts > 0)
                    {
                        cont.Verts = new Int4[nverts];

                        for (int i = 0, j = nverts - 1; i < nverts; j = i++)
                        {
                            var v = verts[j];
                            var vn = verts[i];
                            int nei = vn.W; // The neighbour reg is stored at segment vertex of a segment. 
                            bool shouldRemove = false;
                            int lh = GetCornerHeight(v.X, v.Y, v.Z, walkableClimb, ref shouldRemove);

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

            return true;
        }
        public readonly bool IsConnected(int ia, int ib, int walkableClimb)
        {
            if (Areas[ia] != Areas[ib])
            {
                return false;
            }

            if (Math.Abs(Heights[ia] - Heights[ib]) > walkableClimb)
            {
                return false;
            }

            return true;
        }
        public readonly int GetNeighbourReg(int ax, int ay, int dir)
        {
            int w = Header.Width;
            int ia = ax + ay * w;

            int con = Cons[ia] & 0xf;
            int portal = Cons[ia] >> 4;
            int mask = 1 << dir;

            if ((con & mask) == 0)
            {
                // No connection, return portal or hard edge.
                if ((portal & mask) != 0)
                {
                    return 0xf8 + dir;
                }
                return 0xff;
            }

            int bx = ax + ContourSet.GetDirOffsetX(dir);
            int by = ay + ContourSet.GetDirOffsetY(dir);
            int ib = bx + by * w;

            return Regs[ib];
        }
        public readonly bool WalkContour(int x, int y, TempContour cont)
        {
            int w = Header.Width;
            int h = Header.Height;

            cont.Reset();

            int startX = x;
            int startY = y;
            int startDir = -1;

            for (int i = 0; i < 4; ++i)
            {
                int dr = (i + 3) & 3;
                int rn = GetNeighbourReg(x, y, dr);
                if (rn != Regs[x + y * w])
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
                int rn = GetNeighbourReg(x, y, dir);

                int nx = x;
                int ny = y;
                int ndir;

                if (rn != Regs[x + y * w])
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
                    if (!cont.AppendVertex(px, Heights[x + y * w], pz, rn))
                    {
                        return false;
                    }

                    ndir = (dir + 1) & 0x3;  // Rotate CW
                }
                else
                {
                    // Move to next.
                    nx = x + ContourSet.GetDirOffsetX(dir);
                    ny = y + ContourSet.GetDirOffsetY(dir);
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
            cont.RemoveLast();

            return true;
        }
        public readonly int GetCornerHeight(int x, int y, int z, int walkableClimb, ref bool shouldRemove)
        {
            int w = Header.Width;
            int h = Header.Height;

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
                        int lh = Heights[idx];
                        if (Math.Abs(lh - y) <= walkableClimb && Areas[idx] != AreaTypes.RC_NULL_AREA)
                        {
                            height = Math.Max(height, lh);
                            portal &= Cons[idx] >> 4;
                            if (preg != 0xff && preg != Regs[idx])
                            {
                                allSameReg = false;
                            }
                            preg = Regs[idx];
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
    }
}
