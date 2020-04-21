using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    class HeightfieldLayerSet
    {
        public static HeightfieldLayerSet Build(CompactHeightfield chf, int borderSize, int walkableHeight)
        {
            HeightfieldLayerSet lset = new HeightfieldLayerSet();

            int w = chf.Width;
            int h = chf.Height;

            int[] srcReg = Helper.CreateArray(chf.SpanCount, 0xff);

            int nsweeps = chf.Width;
            LayerSweepSpan[] sweeps = Helper.CreateArray(nsweeps, new LayerSweepSpan());

            // Partition walkable area into monotone regions.
            int regId = 0;

            for (int y = borderSize; y < h - borderSize; ++y)
            {
                int[] prevCount = Helper.CreateArray(256, 0);
                int sweepId = 0;

                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    var c = chf.Cells[x + y * w];

                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = chf.Spans[i];
                        if (chf.Areas[i] == AreaTypes.Unwalkable) continue;

                        int sid = 0xff;

                        // -x
                        if (s.GetCon(0) != CompactSpan.RC_NOT_CONNECTED)
                        {
                            int ax = x + DirectionUtils.GetDirOffsetX(0);
                            int ay = y + DirectionUtils.GetDirOffsetY(0);
                            int ai = chf.Cells[ax + ay * w].Index + s.GetCon(0);
                            if (chf.Areas[ai] != AreaTypes.Unwalkable && srcReg[ai] != 0xff)
                            {
                                sid = srcReg[ai];
                            }
                        }

                        if (sid == 0xff)
                        {
                            sid = sweepId++;
                            sweeps[sid].Nei = 0xff;
                            sweeps[sid].NS = 0;
                        }

                        // -y
                        if (s.GetCon(3) != CompactSpan.RC_NOT_CONNECTED)
                        {
                            int ax = x + DirectionUtils.GetDirOffsetX(3);
                            int ay = y + DirectionUtils.GetDirOffsetY(3);
                            int ai = chf.Cells[ax + ay * w].Index + s.GetCon(3);
                            int nr = srcReg[ai];
                            if (nr != 0xff)
                            {
                                // Set neighbour when first valid neighbour is encoutered.
                                if (sweeps[sid].NS == 0)
                                    sweeps[sid].Nei = nr;

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

                        srcReg[i] = sid;
                    }
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
                            throw new EngineException("rcBuildHeightfieldLayers: Region ID overflow.");
                        }
                        sweeps[i].Id = regId++;
                    }
                }

                // Remap local sweep ids to region ids.
                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    var c = chf.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        if (srcReg[i] != 0xff)
                        {
                            srcReg[i] = sweeps[srcReg[i]].Id;
                        }
                    }
                }
            }

            // Allocate and init layer regions.
            int nregs = regId;
            LayerRegion[] regs = Helper.CreateArray(nregs, () => (LayerRegion.Default));

            // Find region neighbours and overlapping regions.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.Cells[x + y * w];

                    int[] lregs = new int[LayerRegion.MaxLayers];
                    int nlregs = 0;

                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = chf.Spans[i];
                        int ri = srcReg[i];
                        if (ri == 0xff)
                        {
                            continue;
                        }

                        regs[ri].YMin = Math.Min(regs[ri].YMin, s.Y);
                        regs[ri].YMax = Math.Max(regs[ri].YMax, s.Y);

                        // Collect all region layers.
                        if (nlregs < LayerRegion.MaxLayers)
                        {
                            lregs[nlregs++] = ri;
                        }

                        // Update neighbours
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (s.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
                            {
                                int ax = x + DirectionUtils.GetDirOffsetX(dir);
                                int ay = y + DirectionUtils.GetDirOffsetY(dir);
                                int ai = chf.Cells[ax + ay * w].Index + s.GetCon(dir);
                                int rai = srcReg[ai];
                                if (rai != 0xff && rai != ri)
                                {
                                    // Don't check return value -- if we cannot add the neighbor
                                    // it will just cause a few more regions to be created, which
                                    // is fine.
                                    bool neiAdded = AddUniqueNei(ref regs[ri], LayerRegion.MaxNeighbors, rai);
                                    if (!neiAdded)
                                    {
                                        Console.WriteLine($"Neighbour {regs[ri]} not added.");
                                    }
                                }
                            }
                        }
                    }

                    // Update overlapping regions.
                    for (int i = 0; i < nlregs - 1; ++i)
                    {
                        for (int j = i + 1; j < nlregs; ++j)
                        {
                            if (lregs[i] != lregs[j])
                            {
                                var ri = regs[lregs[i]];
                                var rj = regs[lregs[j]];

                                if (!AddUniqueLayer(ref ri, LayerRegion.MaxLayers, lregs[j]) ||
                                    !AddUniqueLayer(ref rj, LayerRegion.MaxLayers, lregs[i]))
                                {
                                    throw new EngineException("rcBuildHeightfieldLayers: layer overflow (too many overlapping walkable platforms). Try increasing RC_MAX_LAYERS.");
                                }

                                regs[lregs[i]] = ri;
                                regs[lregs[j]] = rj;
                            }
                        }
                    }
                }
            }

            // Create 2D layers from regions.
            int layerId = 0;

            int MaxStack = 64;
            int[] stack = new int[MaxStack];
            int nstack = 0;

            for (int i = 0; i < nregs; ++i)
            {
                var root = regs[i];

                // Skip already visited.
                if (root.LayerId != 0xff)
                {
                    continue;
                }

                // Start search.
                root.LayerId = layerId;
                root.IsBase = true;

                nstack = 0;
                stack[nstack++] = i;

                while (nstack != 0)
                {
                    // Pop front
                    var reg = regs[stack[0]];
                    nstack--;
                    for (int j = 0; j < nstack; ++j)
                    {
                        stack[j] = stack[j + 1];
                    }

                    int nneis = reg.NNeis;
                    for (int j = 0; j < nneis; ++j)
                    {
                        int nei = reg.Neis[j];
                        var regn = regs[nei];

                        // Skip already visited.
                        if (regn.LayerId != 0xff)
                        {
                            continue;
                        }

                        // Skip if the neighbour is overlapping root region.
                        if (Contains(root.Layers, root.NLayers, nei))
                        {
                            continue;
                        }

                        // Skip if the height range would become too large.
                        int ymin = Math.Min(root.YMin, regn.YMin);
                        int ymax = Math.Max(root.YMax, regn.YMax);
                        if ((ymax - ymin) >= 255)
                        {
                            continue;
                        }

                        if (nstack < MaxStack)
                        {
                            // Deepen
                            stack[nstack++] = nei;

                            // Mark layer id
                            regn.LayerId = layerId;

                            // Merge current layers to root.
                            for (int k = 0; k < regn.NLayers; ++k)
                            {
                                if (!AddUniqueLayer(ref root, LayerRegion.MaxLayers, regn.Layers[k]))
                                {
                                    throw new EngineException("rcBuildHeightfieldLayers: layer overflow (too many overlapping walkable platforms). Try increasing RC_MAX_LAYERS.");
                                }
                            }

                            root.YMin = Math.Min(root.YMin, regn.YMin);
                            root.YMax = Math.Max(root.YMax, regn.YMax);
                        }

                        regs[nei] = regn;
                    }
                }

                regs[i] = root;

                layerId++;
            }

            // Merge non-overlapping regions that are close in height.
            int mergeHeight = walkableHeight * 4;

            for (int i = 0; i < nregs; ++i)
            {
                var ri = regs[i];

                if (!ri.IsBase)
                {
                    continue;
                }

                int newId = ri.LayerId;

                while (true)
                {
                    int oldId = 0xff;

                    for (int j = 0; j < nregs; ++j)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        var rj = regs[j];
                        if (!rj.IsBase)
                        {
                            continue;
                        }

                        // Skip if the regions are not close to each other.
                        if (!OverlapRange(ri.YMin, (ri.YMax + mergeHeight), rj.YMin, (rj.YMax + mergeHeight)))
                        {
                            continue;
                        }

                        // Skip if the height range would become too large.
                        int ymin = Math.Min(ri.YMin, rj.YMin);
                        int ymax = Math.Max(ri.YMax, rj.YMax);
                        if ((ymax - ymin) >= 255)
                        {
                            continue;
                        }

                        // Make sure that there is no overlap when merging 'ri' and 'rj'.
                        bool overlap = false;

                        // Iterate over all regions which have the same layerId as 'rj'
                        for (int k = 0; k < nregs; ++k)
                        {
                            if (regs[k].LayerId != rj.LayerId)
                            {
                                continue;
                            }

                            // Check if region 'k' is overlapping region 'ri'
                            // Index to 'regs' is the same as region id.
                            if (Contains(ri.Layers, ri.NLayers, k))
                            {
                                overlap = true;
                                break;
                            }
                        }

                        // Cannot merge of regions overlap.
                        if (overlap)
                        {
                            continue;
                        }

                        // Can merge i and j.
                        oldId = rj.LayerId;
                        break;
                    }

                    // Could not find anything to merge with, stop.
                    if (oldId == 0xff)
                    {
                        break;
                    }

                    // Merge
                    for (int j = 0; j < nregs; ++j)
                    {
                        var rj = regs[j];

                        if (rj.LayerId == oldId)
                        {
                            rj.IsBase = false;
                            // Remap layerIds.
                            rj.LayerId = newId;
                            // Add overlaid layers from 'rj' to 'ri'.
                            for (int k = 0; k < rj.NLayers; ++k)
                            {
                                if (!AddUniqueLayer(ref ri, LayerRegion.MaxLayers, rj.Layers[k]))
                                {
                                    throw new EngineException("rcBuildHeightfieldLayers: layer overflow (too many overlapping walkable platforms). Try increasing RC_MAX_LAYERS.");
                                }
                            }

                            // Update height bounds.
                            ri.YMin = Math.Min(ri.YMin, rj.YMin);
                            ri.YMax = Math.Max(ri.YMax, rj.YMax);

                            regs[j] = rj;
                        }
                    }
                }

                regs[i] = ri;
            }

            // Compact layerIds
            int[] remap = new int[256];

            // Find number of unique layers.
            layerId = 0;
            for (int i = 0; i < nregs; i++)
            {
                remap[regs[i].LayerId] = 1;
            }

            for (int i = 0; i < 256; i++)
            {
                if (remap[i] != 0)
                {
                    remap[i] = layerId++;
                }
                else
                {
                    remap[i] = 0xff;
                }
            }

            // Remap ids.
            for (int i = 0; i < nregs; ++i)
            {
                regs[i].LayerId = remap[regs[i].LayerId];
            }

            // No layers, return empty.
            if (layerId == 0)
            {
                return lset;
            }

            // Create layers.
            int lw = w - borderSize * 2;
            int lh = h - borderSize * 2;

            // Build contracted bbox for layers.
            Vector3 bmin = chf.BoundingBox.Minimum;
            Vector3 bmax = chf.BoundingBox.Maximum;
            bmin.X += borderSize * chf.CS;
            bmin.Z += borderSize * chf.CS;
            bmax.X -= borderSize * chf.CS;
            bmax.Z -= borderSize * chf.CS;

            lset.NLayers = layerId;
            lset.Layers = new HeightfieldLayer[layerId];

            // Store layers.
            for (int i = 0; i < lset.NLayers; ++i)
            {
                int curId = i;

                var layer = lset.Layers[i];

                int gridSize = lw * lh;

                layer.Heights = Helper.CreateArray(gridSize, 0xff);
                layer.Areas = Helper.CreateArray(gridSize, AreaTypes.Unwalkable);
                layer.Connections = Helper.CreateArray(gridSize, 0x00);

                // Find layer height bounds.
                int hmin = 0, hmax = 0;
                for (int j = 0; j < nregs; ++j)
                {
                    if (regs[j].IsBase && regs[j].LayerId == curId)
                    {
                        hmin = regs[j].YMin;
                        hmax = regs[j].YMax;
                    }
                }

                layer.Width = lw;
                layer.Height = lh;
                layer.CellSize = chf.CS;
                layer.CellHeight = chf.CH;

                // Adjust the bbox to fit the heightfield.
                var lbbox = new BoundingBox(bmin, bmax);
                lbbox.Minimum.Y = bmin.Y + hmin * chf.CH;
                lbbox.Maximum.Y = bmin.Y + hmax * chf.CH;
                layer.BoundingBox = lbbox;
                layer.HMin = hmin;
                layer.HMax = hmax;

                // Update usable data region.
                layer.MinX = layer.Width;
                layer.MaxX = 0;
                layer.MinY = layer.Height;
                layer.MaxY = 0;

                // Copy height and area from compact heightfield. 
                for (int y = 0; y < lh; ++y)
                {
                    for (int x = 0; x < lw; ++x)
                    {
                        int cx = borderSize + x;
                        int cy = borderSize + y;
                        var c = chf.Cells[cx + cy * w];
                        for (int j = c.Index, nj = (c.Index + c.Count); j < nj; ++j)
                        {
                            var s = chf.Spans[j];
                            // Skip unassigned regions.
                            if (srcReg[j] == 0xff)
                            {
                                continue;
                            }

                            // Skip of does nto belong to current layer.
                            int lid = regs[srcReg[j]].LayerId;
                            if (lid != curId)
                            {
                                continue;
                            }

                            // Update data bounds.
                            layer.MinX = Math.Min(layer.MinX, x);
                            layer.MaxX = Math.Max(layer.MaxX, x);
                            layer.MinY = Math.Min(layer.MinY, y);
                            layer.MaxY = Math.Max(layer.MaxY, y);

                            // Store height and area type.
                            int idx = x + y * lw;
                            layer.Heights[idx] = (s.Y - hmin);
                            layer.Areas[idx] = chf.Areas[j];

                            // Check connection.
                            int portal = 0;
                            int con = 0;
                            for (int dir = 0; dir < 4; ++dir)
                            {
                                if (s.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
                                {
                                    int ax = cx + DirectionUtils.GetDirOffsetX(dir);
                                    int ay = cy + DirectionUtils.GetDirOffsetY(dir);
                                    int ai = chf.Cells[ax + ay * w].Index + s.GetCon(dir);
                                    int alid = (srcReg[ai] != 0xff ? regs[srcReg[ai]].LayerId : 0xff);
                                    // Portal mask
                                    if (chf.Areas[ai] != AreaTypes.Unwalkable && lid != alid)
                                    {
                                        portal |= (1 << dir);

                                        // Update height so that it matches on both sides of the portal.
                                        var ass = chf.Spans[ai];
                                        if (ass.Y > hmin)
                                        {
                                            layer.Heights[idx] = Math.Max(layer.Heights[idx], (ass.Y - hmin));
                                        }
                                    }
                                    // Valid connection mask
                                    if (chf.Areas[ai] != AreaTypes.Unwalkable && lid == alid)
                                    {
                                        int nx = ax - borderSize;
                                        int ny = ay - borderSize;
                                        if (nx >= 0 && ny >= 0 && nx < lw && ny < lh)
                                        {
                                            con |= (1 << dir);
                                        }
                                    }
                                }
                            }

                            layer.Connections[idx] = ((portal << 4) | con);
                        }
                    }
                }

                if (layer.MinX > layer.MaxX)
                {
                    layer.MinX = layer.MaxX = 0;
                }

                if (layer.MinY > layer.MaxY)
                {
                    layer.MinY = layer.MaxY = 0;
                }

                lset.Layers[i] = layer;
            }

            return lset;
        }
        private static bool AddUniqueLayer(ref LayerRegion layer, int anMax, int v)
        {
            if (Contains(layer.Layers, layer.NLayers, v))
            {
                return true;
            }

            if (layer.NLayers >= anMax)
            {
                return false;
            }

            layer.Layers[layer.NLayers] = v;
            layer.NLayers++;

            return true;
        }
        private static bool AddUniqueNei(ref LayerRegion layer, int anMax, int v)
        {
            if (Contains(layer.Neis, layer.NNeis, v))
            {
                return true;
            }

            if (layer.NNeis >= anMax)
            {
                return false;
            }

            layer.Neis[layer.NNeis] = v;
            layer.NNeis++;

            return true;
        }
        private static bool OverlapRange(int amin, int amax, int bmin, int bmax)
        {
            return !(amin > bmax || amax < bmin);
        }
        private static bool Contains(int[] a, int an, int v)
        {
            int n = an;

            for (int i = 0; i < n; ++i)
            {
                if (a[i] == v)
                {
                    return true;
                }
            }

            return false;
        }

        public HeightfieldLayer[] Layers { get; set; }
        public int NLayers { get; set; }
    }
}
