using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Height field layer data
    /// </summary>
    class HeightfieldLayerData
    {
        /// <summary>
        /// Maximum stack count
        /// </summary>
        const int MaxStack = 64;
        /// <summary>
        /// Null id
        /// </summary>
        const int NULL_ID = 0xff;

        /// <summary>
        /// Compact heighfield
        /// </summary>
        public CompactHeightfield Heightfield { get; private set; }
        /// <summary>
        /// Border size
        /// </summary>
        public int BorderSize { get; private set; }
        /// <summary>
        /// Walkable height
        /// </summary>
        public int WalkableHeight { get; private set; }
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Layer width
        /// </summary>
        public int LayerWidth { get; private set; }
        /// <summary>
        /// Layer height
        /// </summary>
        public int LayerHeight { get; private set; }
        /// <summary>
        /// Bounds
        /// </summary>
        public BoundingBox BoundingBox { get; private set; }
        /// <summary>
        /// Region collection
        /// </summary>
        public LayerRegion[] Regions { get; private set; }
        /// <summary>
        /// Number of regions
        /// </summary>
        public int NRegions { get; private set; }
        /// <summary>
        /// Source regions collection
        /// </summary>
        public int[] SourceRegions { get; private set; }

        /// <summary>
        /// Layer id
        /// </summary>
        public int LayerId { get; private set; }

        /// <summary>
        /// Creates the heighfield data
        /// </summary>
        /// <param name="chf">Compact heighfield</param>
        /// <param name="borderSize">Border size</param>
        /// <param name="walkableHeight">Walkable height</param>
        public static HeightfieldLayerData Create(CompactHeightfield chf, int borderSize, int walkableHeight)
        {
            int w = chf.Width;
            int h = chf.Height;

            // Create layers.
            int lw = w - borderSize * 2;
            int lh = h - borderSize * 2;

            // Build contracted bbox for layers.
            var bmin = chf.BoundingBox.Minimum;
            var bmax = chf.BoundingBox.Maximum;
            bmin.X += borderSize * chf.CellSize;
            bmin.Z += borderSize * chf.CellSize;
            bmax.X -= borderSize * chf.CellSize;
            bmax.Z -= borderSize * chf.CellSize;

            var ldata = new HeightfieldLayerData()
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
            int regId = ldata.GenerateRegions();

            // Allocate and init layer regions.
            ldata.AllocateRegions(regId);

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
        /// <summary>
        /// Creates an unique ID.
        /// </summary>
        private static LayerSweepSpan[] CreateUniqueId(int[] prevCount, int regId, LayerSweepSpan[] sweeps, int nsweeps, out int id)
        {
            id = regId;

            // Copy array
            var res = sweeps.ToArray();

            for (int i = 0; i < nsweeps; ++i)
            {
                // If the neighbour is set and there is only one continuous connection to it,
                // the sweep will be merged with the previous one, else new region is created.
                if (res[i].NeiRegId != NULL_ID && prevCount[res[i].NeiRegId] == res[i].SampleCount)
                {
                    res[i].RegId = res[i].NeiRegId;

                    continue;
                }

                if (id == 255)
                {
                    throw new EngineException("rcBuildHeightfieldLayers: Region ID overflow.");
                }

                res[i].RegId = id++;
            }

            return res;
        }

        /// <summary>
        /// Partition walkable area into monotone regions.
        /// </summary>
        private int GenerateRegions()
        {
            SourceRegions = Helper.CreateArray(Heightfield.SpanCount, NULL_ID);

            LayerSweepSpan[] sweeps = Helper.CreateArray(Heightfield.Width, new LayerSweepSpan());

            int regId = 0;

            for (int y = BorderSize; y < Height - BorderSize; ++y)
            {
                int[] prevCount = Helper.CreateArray(256, 0);
                int sweepId = 0;

                for (int x = BorderSize; x < Width - BorderSize; ++x)
                {
                    (prevCount, sweeps) = GenerateRegionCell(x, y, sweepId, prevCount, sweeps, out sweepId);
                }

                // Create unique ID.
                sweeps = CreateUniqueId(prevCount, regId, sweeps, sweepId, out regId);

                // Remap local sweep ids to region ids.
                RemapRegionIds(y, sweeps);
            }

            return regId;
        }
        /// <summary>
        /// Generates a region cell
        /// </summary>
        private (int[], LayerSweepSpan[]) GenerateRegionCell(int x, int y, int sweepId, int[] prevCount, LayerSweepSpan[] sweeps, out int id)
        {
            id = sweepId;

            var resCount = prevCount.ToArray();
            var resSweeps = sweeps.ToArray();

            var c = Heightfield.Cells[x + y * Width];

            for (int i = c.Index, ni = c.Index + c.Count; i < ni; ++i)
            {
                var s = Heightfield.Spans[i];
                if (Heightfield.Areas[i] == AreaTypes.RC_NULL_AREA)
                {
                    continue;
                }

                int sid = NULL_ID;

                // -x
                if (s.GetCon(0, out int con))
                {
                    int ax = x + Utils.GetDirOffsetX(0);
                    int ay = y + Utils.GetDirOffsetY(0);
                    int ai = Heightfield.Cells[ax + ay * Width].Index + con;
                    if (Heightfield.Areas[ai] != AreaTypes.RC_NULL_AREA && SourceRegions[ai] != NULL_ID)
                    {
                        sid = SourceRegions[ai];
                    }
                }

                if (sid == NULL_ID)
                {
                    sid = id++;
                    resSweeps[sid].NeiRegId = NULL_ID;
                    resSweeps[sid].SampleCount = 0;
                }

                // -y
                if (s.GetCon(3, out con))
                {
                    int ax = x + Utils.GetDirOffsetX(3);
                    int ay = y + Utils.GetDirOffsetY(3);
                    int ai = Heightfield.Cells[ax + ay * Width].Index + con;
                    int nr = SourceRegions[ai];
                    if (nr != NULL_ID)
                    {
                        // Set neighbour when first valid neighbour is enconutered.
                        resSweeps[sid].Update(nr, resCount);
                    }
                }

                SourceRegions[i] = sid;
            }

            return (resCount, resSweeps);
        }
        /// <summary>
        /// Remap local sweep ids to region ids.
        /// </summary>
        private void RemapRegionIds(int y, LayerSweepSpan[] sweeps)
        {
            for (int x = BorderSize; x < Width - BorderSize; ++x)
            {
                var c = Heightfield.Cells[x + y * Width];

                for (int i = c.Index, ni = c.Index + c.Count; i < ni; ++i)
                {
                    if (SourceRegions[i] != NULL_ID)
                    {
                        SourceRegions[i] = sweeps[SourceRegions[i]].RegId;
                    }
                }
            }
        }

        /// <summary>
        /// Allocate and init layer regions.
        /// </summary>
        private void AllocateRegions(int nregions)
        {
            NRegions = nregions;
            Regions = Helper.CreateArray(NRegions, () => LayerRegion.Default);

            // Find region neighbours and overlapping regions.
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    var lregs = AllocateRegionCell(x, y);

                    // Update overlapping regions.
                    UpdayeOverlappingRegions(lregs);
                }
            }
        }
        /// <summary>
        /// Allocate and init layer region cell.
        /// </summary>
        private int[] AllocateRegionCell(int x, int y)
        {
            var c = Heightfield.Cells[x + y * Width];

            List<int> lregs = new(LayerRegion.MaxLayers);

            for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
            {
                var s = Heightfield.Spans[i];
                int ri = SourceRegions[i];
                if (ri == NULL_ID)
                {
                    continue;
                }

                Regions[ri].YMin = Math.Min(Regions[ri].YMin, s.Y);
                Regions[ri].YMax = Math.Max(Regions[ri].YMax, s.Y);

                // Collect all region layers.
                if (lregs.Count < LayerRegion.MaxLayers)
                {
                    lregs.Add(ri);
                }

                // Update neighbours
                foreach (var item in Heightfield.IterateSpanConnections(s, x, y, Width))
                {
                    int rai = SourceRegions[item.ai];
                    if (rai != NULL_ID && rai != ri)
                    {
                        // Don't check return value -- if we cannot add the neighbor
                        // it will just cause a few more regions to be created, which
                        // is fine.
                        Regions[ri].AddUniqueNei(rai);
                    }
                }
            }

            return lregs.ToArray();
        }
        /// <summary>
        /// Update overlapping regions.
        /// </summary>
        private void UpdayeOverlappingRegions(int[] lregs)
        {
            for (int i = 0; i < lregs.Length - 1; ++i)
            {
                for (int j = i + 1; j < lregs.Length; ++j)
                {
                    var li = lregs[i];
                    var lj = lregs[j];

                    if (li == lj)
                    {
                        continue;
                    }

                    var ri = Regions[li];
                    var rj = Regions[lj];

                    if (!ri.AddUniqueLayer(lj) || !rj.AddUniqueLayer(li))
                    {
                        throw new EngineException("rcBuildHeightfieldLayers: layer overflow (too many overlapping walkable platforms). Try increasing RC_MAX_LAYERS.");
                    }

                    Regions[li] = ri;
                    Regions[lj] = rj;
                }
            }
        }

        /// <summary>
        /// Create 2D layers from regions.
        /// </summary>
        private void Create2Dlayers()
        {
            int layerId = 0;

            List<int> stack = new(MaxStack);

            for (int i = 0; i < NRegions; ++i)
            {
                var root = Regions[i];

                // Skip already visited.
                if (root.LayerId != NULL_ID)
                {
                    continue;
                }

                // Start search.
                root.LayerId = layerId;
                root.IsBase = true;

                stack.Clear();
                stack.Add(i);

                while (stack.Any())
                {
                    // Pop front
                    var reg = Regions[stack[0]];
                    stack.RemoveAt(0);

                    root = ProcessNeigbors(reg, layerId, root, stack);
                }

                Regions[i] = root;

                layerId++;
            }
        }
        /// <summary>
        /// Process layer neighbors
        /// </summary>
        /// <param name="reg">Layer region</param>
        /// <param name="layerId">Layer id</param>
        /// <param name="root">Root region</param>
        /// <param name="stack">Stack list</param>
        /// <returns>Returns the updated root</returns>
        private LayerRegion ProcessNeigbors(LayerRegion reg, int layerId, LayerRegion root, List<int> stack)
        {
            // Copy root
            var res = root;

            int nneis = reg.NNeis;
            for (int j = 0; j < nneis; ++j)
            {
                int nei = reg.Neis[j];
                var regn = Regions[nei];

                // Skip already visited.
                if (regn.LayerId != NULL_ID)
                {
                    continue;
                }

                // Skip if the neighbour is overlapping root region.
                if (res.ContainsLayer(nei))
                {
                    continue;
                }

                // Skip if the height range would become too large.
                int ymin = Math.Min(res.YMin, regn.YMin);
                int ymax = Math.Max(res.YMax, regn.YMax);
                if ((ymax - ymin) >= 255)
                {
                    continue;
                }

                if (stack.Count < MaxStack)
                {
                    // Deepen
                    stack.Add(nei);

                    // Mark layer id
                    regn.LayerId = layerId;

                    // Merge current layers to root.
                    for (int k = 0; k < regn.NLayers; ++k)
                    {
                        if (!res.AddUniqueLayer(regn.Layers[k]))
                        {
                            throw new EngineException("rcBuildHeightfieldLayers: layer overflow (too many overlapping walkable platforms). Try increasing RC_MAX_LAYERS.");
                        }
                    }

                    res.YMin = Math.Min(res.YMin, regn.YMin);
                    res.YMax = Math.Max(res.YMax, regn.YMax);
                }

                Regions[nei] = regn;
            }

            return res;
        }

        /// <summary>
        /// Merge non-overlapping regions that are close in height.
        /// </summary>
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
                    int oldId = FindOverlapLayerRegion(ri, i, mergeHeight);

                    // Could not find anything to merge with, stop.
                    if (oldId == NULL_ID)
                    {
                        break;
                    }

                    // Merge
                    MergeLayerRegion(ref ri, oldId, newId);
                }

                Regions[i] = ri;
            }
        }
        /// <summary>
        /// Find overlap region.
        /// </summary>
        private int FindOverlapLayerRegion(LayerRegion region, int layerIndex, int mergeHeight)
        {
            int oldId = NULL_ID;

            for (int j = 0; j < NRegions; ++j)
            {
                if (layerIndex == j)
                {
                    continue;
                }

                var rj = Regions[j];
                if (!rj.IsBase)
                {
                    continue;
                }

                // Skip if the regions are not close to each other.
                if (!Utils.OverlapRange(region.YMin, region.YMax + mergeHeight, rj.YMin, rj.YMax + mergeHeight))
                {
                    continue;
                }

                // Skip if the height range would become too large.
                int ymin = Math.Min(region.YMin, rj.YMin);
                int ymax = Math.Max(region.YMax, rj.YMax);
                if ((ymax - ymin) >= 255)
                {
                    continue;
                }

                // Make sure that there is no overlap when merging 'ri' and 'rj'.
                bool overlap = LayerRegionOverlap(region, rj);

                // Cannot merge of regions overlap.
                if (overlap)
                {
                    continue;
                }

                // Can merge i and j.
                oldId = rj.LayerId;
                break;
            }

            return oldId;
        }
        /// <summary>
        /// Make sure that there is no overlap when merging 'ri' and 'rj'.
        /// </summary>
        private bool LayerRegionOverlap(LayerRegion ri, LayerRegion rj)
        {
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

            return overlap;
        }
        /// <summary>
        /// Merge
        /// </summary>
        private void MergeLayerRegion(ref LayerRegion ri, int oldId, int newId)
        {
            for (int j = 0; j < NRegions; ++j)
            {
                var rj = Regions[j];

                if (rj.LayerId != oldId)
                {
                    continue;
                }

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

        /// <summary>
        /// Compact layerIds
        /// </summary>
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
                    remap[i] = NULL_ID;
                }
            }

            // Remap ids.
            for (int i = 0; i < NRegions; ++i)
            {
                Regions[i].LayerId = remap[Regions[i].LayerId];
            }
        }

        /// <summary>
        /// Find layer height bounds.
        /// </summary>
        public (int, int) FindLayerHeightBounds(int curId)
        {
            int hmin = 0;
            int hmax = 0;

            for (int j = 0; j < NRegions; ++j)
            {
                var region = Regions[j];

                if (region.IsBase && region.LayerId == curId)
                {
                    hmin = region.YMin;
                    hmax = region.YMax;
                }
            }

            return (hmin, hmax);
        }

        /// <summary>
        /// Checks the connection
        /// </summary>
        /// <param name="s">Compact span</param>
        /// <param name="cx">X position</param>
        /// <param name="cy">Y position</param>
        /// <param name="layerId">Layer id</param>
        /// <param name="layerIndex">Layer index</param>
        /// <param name="hmin">Minimum height value</param>
        /// <param name="heights">Height map</param>
        /// <returns>Returns the connection value</returns>
        public int CheckConnection(CompactSpan s, int cx, int cy, int layerId, int layerIndex, int hmin, int[] heights)
        {
            var sourceRegs = SourceRegions;
            var regs = Regions;
            int borderSize = BorderSize;
            int w = Width;
            int lw = LayerWidth;
            int lh = LayerHeight;

            int portal = 0;
            int con = 0;

            foreach (var (dir, ax, ay, ai, area, ass) in Heightfield.IterateSpanConnections(s, cx, cy, w))
            {
                int alid = sourceRegs[ai] != NULL_ID ? regs[sourceRegs[ai]].LayerId : NULL_ID;

                // Portal mask
                if (area != AreaTypes.RC_NULL_AREA && layerId != alid)
                {
                    portal |= 1 << dir;

                    // Update height so that it matches on both sides of the portal.
                    if (ass.Y > hmin)
                    {
                        heights[layerIndex] = Math.Max(heights[layerIndex], ass.Y - hmin);
                    }
                }

                // Valid connection mask
                if (area != AreaTypes.RC_NULL_AREA && layerId == alid)
                {
                    int nx = ax - borderSize;
                    int ny = ay - borderSize;
                    if (nx >= 0 && ny >= 0 && nx < lw && ny < lh)
                    {
                        con |= 1 << dir;
                    }
                }
            }

            return (portal << 4) | con;
        }
    }
}
