using SharpDX;
using System;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Heightfield layer set
    /// </summary>
    class HeightfieldLayerSet
    {
        /// <summary>
        /// Builds a new heightfield layer set
        /// </summary>
        /// <param name="chf">Compact heightfield</param>
        /// <param name="borderSize">Border size</param>
        /// <param name="walkableHeight">Walkable height</param>
        /// <returns>Returns the new heightfield layer set</returns>
        public static HeightfieldLayerSet Build(CompactHeightfield chf, int borderSize, int walkableHeight)
        {
            var ldata = HeightfieldLayerData.Build(chf, borderSize, walkableHeight);
            if (ldata == null)
            {
                return new HeightfieldLayerSet();
            }

            return StoreLayers(ldata);
        }
        private static HeightfieldLayerSet StoreLayers(HeightfieldLayerData ldata)
        {
            HeightfieldLayerSet lset = new HeightfieldLayerSet
            {
                NLayers = ldata.LayerId,
                Layers = new HeightfieldLayer[ldata.LayerId]
            };

            for (int i = 0; i < lset.NLayers; ++i)
            {
                var layer = lset.Layers[i];

                // Copy height and area from compact heightfield. 
                ldata.CopyToLayer(ref layer, i);

                lset.Layers[i] = layer;
            }

            return lset;
        }
        private static bool OverlapRange(int amin, int amax, int bmin, int bmax)
        {
            return !(amin > bmax || amax < bmin);
        }

        class HeightfieldLayerData
        {
            public CompactHeightfield Heightfield;
            public int BorderSize;
            public int WalkableHeight;

            public int Width;
            public int Height;

            public int LayerWidth;
            public int LayerHeight;

            public BoundingBox BoundingBox;

            public LayerRegion[] Regions;
            public int NRegions;
            public int[] SourceRegions;

            public int LayerId;

            public static HeightfieldLayerData Build(CompactHeightfield chf, int borderSize, int walkableHeight)
            {
                int w = chf.Width;
                int h = chf.Height;

                // Create layers.
                int lw = w - borderSize * 2;
                int lh = h - borderSize * 2;

                // Build contracted bbox for layers.
                Vector3 bmin = chf.BoundingBox.Minimum;
                Vector3 bmax = chf.BoundingBox.Maximum;
                bmin.X += borderSize * chf.CellSize;
                bmin.Z += borderSize * chf.CellSize;
                bmax.X -= borderSize * chf.CellSize;
                bmax.Z -= borderSize * chf.CellSize;

                HeightfieldLayerData ldata = new HeightfieldLayerData()
                {
                    Heightfield = chf,
                    BorderSize = borderSize,
                    WalkableHeight = walkableHeight,
                    Width = w,
                    Height = h,
                    LayerWidth = lw,
                    LayerHeight = lh,
                    BoundingBox = new BoundingBox(bmin, bmax),
                };

                // Partition walkable area into monotone regions.
                int regId = ldata.GenerateRegions(chf);

                // Allocate and init layer regions.
                ldata.AllocateRegions(chf, regId);

                // Create 2D layers from regions.
                ldata.Create2Dlayers();

                // Merge non-overlapping regions that are close in height.
                ldata.MergeCloseRegions();

                // Compact layerIds
                ldata.CompactLayers();
                if (ldata.LayerId == 0)
                {
                    // No layers, return empty.
                    return null;
                }

                return ldata;
            }

            private int GenerateRegions(CompactHeightfield chf)
            {
                SourceRegions = Helper.CreateArray(chf.SpanCount, 0xff);

                int nsweeps = chf.Width;
                LayerSweepSpan[] sweeps = Helper.CreateArray(nsweeps, new LayerSweepSpan());

                int regId = 0;

                for (int y = BorderSize; y < Height - BorderSize; ++y)
                {
                    int[] prevCount = Helper.CreateArray(256, 0);
                    int sweepId = 0;

                    for (int x = BorderSize; x < Width - BorderSize; ++x)
                    {
                        var c = chf.Cells[x + y * Width];

                        for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                        {
                            var s = chf.Spans[i];
                            if (chf.Areas[i] == AreaTypes.RC_NULL_AREA) continue;

                            int sid = 0xff;

                            // -x
                            if (s.GetCon(0) != ContourSet.RC_NOT_CONNECTED)
                            {
                                int ax = x + RecastUtils.GetDirOffsetX(0);
                                int ay = y + RecastUtils.GetDirOffsetY(0);
                                int ai = chf.Cells[ax + ay * Width].Index + s.GetCon(0);
                                if (chf.Areas[ai] != AreaTypes.RC_NULL_AREA && SourceRegions[ai] != 0xff)
                                {
                                    sid = SourceRegions[ai];
                                }
                            }

                            if (sid == 0xff)
                            {
                                sid = sweepId++;
                                sweeps[sid].Nei = 0xff;
                                sweeps[sid].NS = 0;
                            }

                            // -y
                            if (s.GetCon(3) != ContourSet.RC_NOT_CONNECTED)
                            {
                                int ax = x + RecastUtils.GetDirOffsetX(3);
                                int ay = y + RecastUtils.GetDirOffsetY(3);
                                int ai = chf.Cells[ax + ay * Width].Index + s.GetCon(3);
                                int nr = SourceRegions[ai];
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

                            SourceRegions[i] = sid;
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
                    for (int x = BorderSize; x < Width - BorderSize; ++x)
                    {
                        var c = chf.Cells[x + y * Width];
                        for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                        {
                            if (SourceRegions[i] != 0xff)
                            {
                                SourceRegions[i] = sweeps[SourceRegions[i]].Id;
                            }
                        }
                    }
                }

                return regId;
            }
            private void AllocateRegions(CompactHeightfield chf, int nregions)
            {
                NRegions = nregions;
                Regions = Helper.CreateArray(NRegions, () => LayerRegion.Default);

                // Find region neighbours and overlapping regions.
                for (int y = 0; y < Height; ++y)
                {
                    for (int x = 0; x < Width; ++x)
                    {
                        var c = chf.Cells[x + y * Width];

                        int[] lregs = new int[LayerRegion.MaxLayers];
                        int nlregs = 0;

                        for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                        {
                            var s = chf.Spans[i];
                            int ri = SourceRegions[i];
                            if (ri == 0xff)
                            {
                                continue;
                            }

                            Regions[ri].YMin = Math.Min(Regions[ri].YMin, s.Y);
                            Regions[ri].YMax = Math.Max(Regions[ri].YMax, s.Y);

                            // Collect all region layers.
                            if (nlregs < LayerRegion.MaxLayers)
                            {
                                lregs[nlregs++] = ri;
                            }

                            // Update neighbours
                            for (int dir = 0; dir < 4; ++dir)
                            {
                                if (s.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
                                {
                                    int ax = x + RecastUtils.GetDirOffsetX(dir);
                                    int ay = y + RecastUtils.GetDirOffsetY(dir);
                                    int ai = chf.Cells[ax + ay * Width].Index + s.GetCon(dir);
                                    int rai = SourceRegions[ai];
                                    if (rai != 0xff && rai != ri)
                                    {
                                        // Don't check return value -- if we cannot add the neighbor
                                        // it will just cause a few more regions to be created, which
                                        // is fine.
                                        Regions[ri].AddUniqueNei(rai);
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
                                    var ri = Regions[lregs[i]];
                                    var rj = Regions[lregs[j]];

                                    if (!ri.AddUniqueLayer(lregs[j]) || !rj.AddUniqueLayer(lregs[i]))
                                    {
                                        throw new EngineException("rcBuildHeightfieldLayers: layer overflow (too many overlapping walkable platforms). Try increasing RC_MAX_LAYERS.");
                                    }

                                    Regions[lregs[i]] = ri;
                                    Regions[lregs[j]] = rj;
                                }
                            }
                        }
                    }
                }
            }
            private void Create2Dlayers()
            {
                int layerId = 0;

                int MaxStack = 64;
                int[] stack = new int[MaxStack];
                int nstack;

                for (int i = 0; i < NRegions; ++i)
                {
                    var root = Regions[i];

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
                        var reg = Regions[stack[0]];
                        nstack--;
                        for (int j = 0; j < nstack; ++j)
                        {
                            stack[j] = stack[j + 1];
                        }

                        int nneis = reg.NNeis;
                        for (int j = 0; j < nneis; ++j)
                        {
                            int nei = reg.Neis[j];
                            var regn = Regions[nei];

                            // Skip already visited.
                            if (regn.LayerId != 0xff)
                            {
                                continue;
                            }

                            // Skip if the neighbour is overlapping root region.
                            if (root.ContainsLayer(nei))
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
                                    if (!root.AddUniqueLayer(regn.Layers[k]))
                                    {
                                        throw new EngineException("rcBuildHeightfieldLayers: layer overflow (too many overlapping walkable platforms). Try increasing RC_MAX_LAYERS.");
                                    }
                                }

                                root.YMin = Math.Min(root.YMin, regn.YMin);
                                root.YMax = Math.Max(root.YMax, regn.YMax);
                            }

                            Regions[nei] = regn;
                        }
                    }

                    Regions[i] = root;

                    layerId++;
                }
            }
            private void MergeCloseRegions()
            {
                int mergeHeight = WalkableHeight * 4;

                for (int i = 0; i < NRegions; ++i)
                {
                    var ri = Regions[i];

                    if (!ri.IsBase)
                    {
                        continue;
                    }

                    int newId = ri.LayerId;

                    while (true)
                    {
                        int oldId = 0xff;

                        for (int j = 0; j < NRegions; ++j)
                        {
                            if (i == j)
                            {
                                continue;
                            }

                            var rj = Regions[j];
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
                            for (int k = 0; k < NRegions; ++k)
                            {
                                if (Regions[k].LayerId != rj.LayerId)
                                {
                                    continue;
                                }

                                // Check if region 'k' is overlapping region 'ri'
                                // Index to 'regs' is the same as region id.
                                if (ri.ContainsLayer(k))
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
                        for (int j = 0; j < NRegions; ++j)
                        {
                            var rj = Regions[j];

                            if (rj.LayerId == oldId)
                            {
                                rj.IsBase = false;
                                // Remap layerIds.
                                rj.LayerId = newId;
                                // Add overlaid layers from 'rj' to 'ri'.
                                for (int k = 0; k < rj.NLayers; ++k)
                                {
                                    if (!ri.AddUniqueLayer(rj.Layers[k]))
                                    {
                                        throw new EngineException("rcBuildHeightfieldLayers: layer overflow (too many overlapping walkable platforms). Try increasing RC_MAX_LAYERS.");
                                    }
                                }

                                // Update height bounds.
                                ri.YMin = Math.Min(ri.YMin, rj.YMin);
                                ri.YMax = Math.Max(ri.YMax, rj.YMax);

                                Regions[j] = rj;
                            }
                        }
                    }

                    Regions[i] = ri;
                }
            }
            private void CompactLayers()
            {
                int[] remap = new int[256];

                // Find number of unique layers.
                LayerId = 0;
                for (int i = 0; i < NRegions; i++)
                {
                    remap[Regions[i].LayerId] = 1;
                }

                for (int i = 0; i < 256; i++)
                {
                    if (remap[i] != 0)
                    {
                        remap[i] = LayerId++;
                    }
                    else
                    {
                        remap[i] = 0xff;
                    }
                }

                // Remap ids.
                for (int i = 0; i < NRegions; ++i)
                {
                    Regions[i].LayerId = remap[Regions[i].LayerId];
                }
            }

            public void CopyToLayer(ref HeightfieldLayer layer, int curId)
            {
                int gridSize = LayerWidth * LayerHeight;

                layer.Heights = Helper.CreateArray(gridSize, 0xff);
                layer.Areas = Helper.CreateArray(gridSize, AreaTypes.RC_NULL_AREA);
                layer.Cons = Helper.CreateArray(gridSize, 0x00);

                // Find layer height bounds.
                var (hmin, hmax) = FindLayerHeightBounds(curId);

                layer.Width = LayerWidth;
                layer.Height = LayerHeight;
                layer.CS = Heightfield.CellSize;
                layer.CH = Heightfield.CellHeight;

                // Adjust the bbox to fit the heightfield.
                var lbbox = BoundingBox;
                lbbox.Minimum.Y = BoundingBox.Minimum.Y + hmin * Heightfield.CellHeight;
                lbbox.Maximum.Y = BoundingBox.Minimum.Y + hmax * Heightfield.CellHeight;
                layer.BoundingBox = lbbox;
                layer.HMin = hmin;
                layer.HMax = hmax;

                // Update usable data region.
                layer.MinX = layer.Width;
                layer.MaxX = 0;
                layer.MinY = layer.Height;
                layer.MaxY = 0;

                for (int y = 0; y < LayerHeight; ++y)
                {
                    for (int x = 0; x < LayerWidth; ++x)
                    {
                        CopyToLayer(ref layer, x, y, curId, hmin);
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
            }
            private void CopyToLayer(ref HeightfieldLayer layer, int x, int y, int curId, int hmin)
            {
                int cx = BorderSize + x;
                int cy = BorderSize + y;
                var c = Heightfield.Cells[cx + cy * Width];
                for (int j = c.Index, nj = (c.Index + c.Count); j < nj; ++j)
                {
                    var s = Heightfield.Spans[j];

                    // Skip unassigned regions.
                    if (SourceRegions[j] == 0xff)
                    {
                        continue;
                    }

                    // Skip of does nto belong to current layer.
                    int lid = Regions[SourceRegions[j]].LayerId;
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
                    int idx = x + y * LayerWidth;
                    layer.Heights[idx] = (s.Y - hmin);
                    layer.Areas[idx] = Heightfield.Areas[j];

                    // Check connection.
                    int portal = 0;
                    int con = 0;
                    for (int dir = 0; dir < 4; ++dir)
                    {
                        if (s.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
                        {
                            int ax = cx + RecastUtils.GetDirOffsetX(dir);
                            int ay = cy + RecastUtils.GetDirOffsetY(dir);
                            int ai = Heightfield.Cells[ax + ay * Width].Index + s.GetCon(dir);
                            int alid = (SourceRegions[ai] != 0xff ? Regions[SourceRegions[ai]].LayerId : 0xff);
                            // Portal mask
                            if (Heightfield.Areas[ai] != AreaTypes.RC_NULL_AREA && lid != alid)
                            {
                                portal |= (1 << dir);

                                // Update height so that it matches on both sides of the portal.
                                var ass = Heightfield.Spans[ai];
                                if (ass.Y > hmin)
                                {
                                    layer.Heights[idx] = Math.Max(layer.Heights[idx], (ass.Y - hmin));
                                }
                            }
                            // Valid connection mask
                            if (Heightfield.Areas[ai] != AreaTypes.RC_NULL_AREA && lid == alid)
                            {
                                int nx = ax - BorderSize;
                                int ny = ay - BorderSize;
                                if (nx >= 0 && ny >= 0 && nx < LayerWidth && ny < LayerHeight)
                                {
                                    con |= (1 << dir);
                                }
                            }
                        }
                    }

                    layer.Cons[idx] = (portal << 4) | con;
                }
            }
            private (int Min, int Max) FindLayerHeightBounds(int curId)
            {
                int hmin = 0;
                int hmax = 0;

                for (int j = 0; j < NRegions; ++j)
                {
                    var region = Regions.ElementAt(j);

                    if (region.IsBase && region.LayerId == curId)
                    {
                        hmin = region.YMin;
                        hmax = region.YMax;
                    }
                }

                return (hmin, hmax);
            }
        }

        /// <summary>
        /// Layer list
        /// </summary>
        public HeightfieldLayer[] Layers { get; set; }
        /// <summary>
        /// Number of layers
        /// </summary>
        public int NLayers { get; set; }
    }
}
