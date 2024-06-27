using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    using Engine.PathFinding.RecastNavigation.Recast;

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
        /// Source regions collection
        /// </summary>
        private int[] sourceRegions;
        /// <summary>
        /// Number of allocated regions
        /// </summary>
        private int nregions;
        /// <summary>
        /// Region collection
        /// </summary>
        private LayerRegion[] regions;
        /// <summary>
        /// Compact heighfield
        /// </summary>
        private CompactHeightfield heightfield;
        /// <summary>
        /// Border size
        /// </summary>
        private int borderSize;
        /// <summary>
        /// Walkable height
        /// </summary>
        private int walkableHeight;

        /// <summary>
        /// Layer id
        /// </summary>
        public int LayerId { get; private set; }
        /// <summary>
        /// Layer width
        /// </summary>
        public int LayerWidth { get; private set; }
        /// <summary>
        /// Layer height
        /// </summary>
        public int LayerHeight { get; private set; }
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Bounds
        /// </summary>
        public BoundingBox Bounds { get; private set; }
        /// <summary>
        /// Cell size
        /// </summary>
        public float CellSize { get { return heightfield?.CellSize ?? 0f; } }
        /// <summary>
        /// Cell height
        /// </summary>
        public float CellHeight { get { return heightfield?.CellHeight ?? 0f; } }

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
            var bmin = chf.Bounds.Minimum;
            var bmax = chf.Bounds.Maximum;
            bmin.X += borderSize * chf.CellSize;
            bmin.Z += borderSize * chf.CellSize;
            bmax.X -= borderSize * chf.CellSize;
            bmax.Z -= borderSize * chf.CellSize;

            HeightfieldLayerData ldata = new()
            {
                heightfield = chf,
                borderSize = borderSize,
                walkableHeight = walkableHeight,

                LayerWidth = lw,
                LayerHeight = lh,

                Width = w,
                Height = h,
                Bounds = new(bmin, bmax),
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
        /// Creates an unique region id.
        /// </summary>
        /// <param name="sweeps">Sweep list</param>
        /// <param name="nsweeps">Number of sweeps in the list</param>
        /// <param name="lastId">Last region id</param>
        /// <param name="samples">Number of samples</param>
        private static int CreateUniqueId(LayerSweepSpan[] sweeps, int nsweeps, int lastId, int[] samples)
        {
            int id = lastId;

            for (int i = 0; i < nsweeps; ++i)
            {
                if (id == 255)
                {
                    throw new EngineException("rcBuildHeightfieldLayers: Region ID overflow.");
                }

                // If the neighbour is set and there is only one continuous connection to it,
                // the sweep will be merged with the previous one, else new region is created.
                if (sweeps[i].NeiRegId != NULL_ID && samples[sweeps[i].NeiRegId] == sweeps[i].SampleCount)
                {
                    sweeps[i].RegId = sweeps[i].NeiRegId;

                    continue;
                }

                sweeps[i].RegId = id++;
            }

            return id;
        }

        /// <summary>
        /// Partition walkable area into monotone regions.
        /// </summary>
        private int GenerateRegions()
        {
            sourceRegions = Helper.CreateArray(heightfield.SpanCount, NULL_ID);

            var sweeps = Helper.CreateArray(heightfield.Width, () => LayerSweepSpan.Empty);

            int regId = 0;

            for (int y = borderSize; y < Height - borderSize; ++y)
            {
                int sweepId = 0;
                int[] samples = Helper.CreateArray(256, 0);

                for (int x = borderSize; x < Width - borderSize; ++x)
                {
                    GenerateRegionCell(x, y, sweepId, samples, sweeps, out sweepId);
                }

                // Create unique ID.
                regId = CreateUniqueId(sweeps, sweepId, regId, samples);

                // Remap local sweep ids to region ids.
                RemapRegionIds(y, sweeps);
            }

            return regId;
        }
        /// <summary>
        /// Generates a region cell
        /// </summary>
        private void GenerateRegionCell(int x, int y, int sweepId, int[] samples, LayerSweepSpan[] sweeps, out int id)
        {
            id = sweepId;

            foreach (var (s, i) in heightfield.IterateCellSpans(x, y))
            {
                if (heightfield.Areas[i] == AreaTypes.RC_NULL_AREA)
                {
                    continue;
                }

                // -x
                if (!TestX(s, x, y, out int sid))
                {
                    // Add sweep
                    sid = id++;
                    sweeps[sid].Reset();
                }

                // -y
                if (TestY(s, x, y, out int nr))
                {
                    // Set neighbour when first valid neighbour is enconutered.
                    sweeps[sid].Update(nr, samples);
                }

                // Store source region
                sourceRegions[i] = sid;
            }
        }
        /// <summary>
        /// Tests -X neighbour
        /// </summary>
        /// <param name="s">Span</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="regionId">Resulting region id</param>
        private bool TestX(CompactSpan s, int x, int y, out int regionId)
        {
            regionId = NULL_ID;

            if (!s.GetCon(0, out int con))
            {
                return false;
            }

            int ai = heightfield.GetNeighbourCellIndex(x, y, 0, con);
            int nr = sourceRegions[ai];
            if (nr == NULL_ID || heightfield.Areas[ai] == AreaTypes.RC_NULL_AREA)
            {
                return false;
            }

            regionId = nr;

            return true;
        }
        /// <summary>
        /// Tests -Y neighbour
        /// </summary>
        /// <param name="s">Span</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="regionId">Resulting region id</param>
        private bool TestY(CompactSpan s, int x, int y, out int regionId)
        {
            regionId = NULL_ID;

            if (!s.GetCon(3, out int con))
            {
                return false;
            }

            int ai = heightfield.GetNeighbourCellIndex(x, y, 3, con);
            int nr = sourceRegions[ai];
            if (nr == NULL_ID)
            {
                return false;
            }

            regionId = nr;

            return true;
        }
        /// <summary>
        /// Remap local sweep ids to region ids.
        /// </summary>
        private void RemapRegionIds(int y, LayerSweepSpan[] sweeps)
        {
            for (int x = borderSize; x < Width - borderSize; ++x)
            {
                var c = heightfield.Cells[x + y * Width];

                for (int i = c.Index, ni = c.Index + c.Count; i < ni; ++i)
                {
                    if (sourceRegions[i] != NULL_ID)
                    {
                        sourceRegions[i] = sweeps[sourceRegions[i]].RegId;
                    }
                }
            }
        }

        /// <summary>
        /// Allocate and init layer regions.
        /// </summary>
        private void AllocateRegions(int nregions)
        {
            this.nregions = nregions;
            regions = Helper.CreateArray(this.nregions, () => LayerRegion.Default);

            // Find region neighbours and overlapping regions.
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    var lregs = AllocateRegionCell(x, y);

                    // Update overlapping regions.
                    UpdateOverlappingRegions(lregs);
                }
            }
        }
        /// <summary>
        /// Allocate and init layer region cell.
        /// </summary>
        private int[] AllocateRegionCell(int x, int y)
        {
            List<int> lregs = new(LayerRegion.MaxLayers);

            foreach (var (s, i) in heightfield.IterateCellSpans(x, y))
            {
                int ri = sourceRegions[i];
                if (ri == NULL_ID)
                {
                    continue;
                }

                regions[ri].YMin = Math.Min(regions[ri].YMin, s.Y);
                regions[ri].YMax = Math.Max(regions[ri].YMax, s.Y);

                // Collect all region layers.
                if (lregs.Count < LayerRegion.MaxLayers)
                {
                    lregs.Add(ri);
                }

                // Update neighbours
                foreach (var item in heightfield.IterateSpanConnections(s, x, y))
                {
                    int rai = sourceRegions[item.ai];
                    if (rai == NULL_ID || rai == ri)
                    {
                        continue;
                    }

                    // Don't check return value -- if we cannot add the neighbor
                    // it will just cause a few more regions to be created, which
                    // is fine.
                    regions[ri].AddUniqueNei(rai);
                }
            }

            return [.. lregs];
        }
        /// <summary>
        /// Update overlapping regions.
        /// </summary>
        /// <param name="lregs">Layer region list</param>
        private void UpdateOverlappingRegions(int[] lregs)
        {
            for (int i = 0; i < lregs.Length - 1; ++i)
            {
                for (int j = i + 1; j < lregs.Length; ++j)
                {
                    if (lregs[i] == lregs[j])
                    {
                        continue;
                    }

                    if (!regions[lregs[i]].AddUniqueLayer(lregs[j]) || !regions[lregs[j]].AddUniqueLayer(lregs[i]))
                    {
                        throw new EngineException("rcBuildHeightfieldLayers: layer overflow (too many overlapping walkable platforms). Try increasing RC_MAX_LAYERS.");
                    }
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

            for (int i = 0; i < nregions; ++i)
            {
                var root = regions[i];

                // Skip already visited.
                if (root.LayerId != NULL_ID)
                {
                    continue;
                }

                // Start search.
                root.LayerId = layerId;
                root.IsBase = true;

                stack.Add(i);

                while (stack.Count != 0)
                {
                    // Pop front
                    var reg = regions[stack.PopFirst()];

                    root = ProcessNeigbors(reg, layerId, root, stack);
                }

                regions[i] = root;

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

                // Skip if the neighbour is overlapping root region.
                if (res.ContainsLayer(nei))
                {
                    continue;
                }

                // Skip already visited.
                if (regions[nei].LayerId != NULL_ID)
                {
                    continue;
                }

                // Skip if the height range would become too large.
                int h = LayerRegion.GetHeightRange(res, regions[nei]);
                if (h >= 255)
                {
                    continue;
                }

                if (stack.Count >= MaxStack)
                {
                    continue;
                }

                // Deepen
                stack.Add(nei);

                // Mark layer id
                regions[nei].LayerId = layerId;

                // Merge current layers to root.
                if (!res.Merge(regions[nei]))
                {
                    throw new EngineException("rcBuildHeightfieldLayers: layer overflow (too many overlapping walkable platforms). Try increasing RC_MAX_LAYERS.");
                }
            }

            return res;
        }

        /// <summary>
        /// Merge non-overlapping regions that are close in height.
        /// </summary>
        private void MergeCloseRegions()
        {
            int mergeHeight = walkableHeight * 4;

            for (int i = 0; i < nregions; ++i)
            {
                var ri = regions[i];

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

                regions[i] = ri;
            }
        }
        /// <summary>
        /// Find overlap region.
        /// </summary>
        private int FindOverlapLayerRegion(LayerRegion region, int layerIndex, int mergeHeight)
        {
            int oldId = NULL_ID;

            for (int j = 0; j < nregions; ++j)
            {
                if (layerIndex == j)
                {
                    continue;
                }

                var rj = regions[j];
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
                int h = LayerRegion.GetHeightRange(region, rj);
                if (h >= 255)
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
            for (int k = 0; k < nregions; ++k)
            {
                if (regions[k].LayerId != rj.LayerId)
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
            for (int j = 0; j < nregions; ++j)
            {
                var rj = regions[j];

                if (rj.LayerId != oldId)
                {
                    continue;
                }

                rj.IsBase = false;

                // Remap layerIds.
                rj.LayerId = newId;

                // Add overlaid layers from 'rj' to 'ri'.
                if (!ri.Merge(rj))
                {
                    throw new EngineException("rcBuildHeightfieldLayers: layer overflow (too many overlapping walkable platforms). Try increasing RC_MAX_LAYERS.");
                }

                regions[j] = rj;
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
            for (int i = 0; i < nregions; i++)
            {
                remap[regions[i].LayerId] = 1;
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
            for (int i = 0; i < nregions; ++i)
            {
                regions[i].LayerId = remap[regions[i].LayerId];
            }
        }

        /// <summary>
        /// Find layer height bounds.
        /// </summary>
        /// <param name="layerId">Layer id</param>
        public (int, int) FindLayerHeightBounds(int layerId)
        {
            int hmin = 0;
            int hmax = 0;

            for (int j = 0; j < nregions; ++j)
            {
                var region = regions[j];

                if (region.IsBase && region.LayerId == layerId)
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
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="layerId">Layer id</param>
        /// <param name="layerIndex">Layer index</param>
        /// <param name="hmin">Minimum height value</param>
        /// <param name="heights">Height map</param>
        /// <returns>Returns the connection value</returns>
        public int CheckConnection(CompactSpan s, int x, int y, int layerId, int layerIndex, int hmin, int[] heights)
        {
            int portal = 0;
            int con = 0;

            foreach (var (dir, ax, ay, ai, area, ass) in heightfield.IterateSpanConnections(s, x, y))
            {
                int alid = sourceRegions[ai] != NULL_ID ? regions[sourceRegions[ai]].LayerId : NULL_ID;

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
                    if (nx >= 0 && ny >= 0 && nx < LayerWidth && ny < LayerHeight)
                    {
                        con |= 1 << dir;
                    }
                }
            }

            return portal << 4 | con;
        }

        /// <summary>
        /// Gets the layer bounds
        /// </summary>
        /// <param name="layerId">Layer id</param>
        /// <returns>Returns the layer bounds, the minimum and the maximum height values</returns>
        public (BoundingBox Bounds, int HMin, int HMax) GetLayerBounds(int layerId)
        {
            // Find layer height bounds.
            var (hmin, hmax) = FindLayerHeightBounds(layerId);

            // Adjust the bbox to fit the heightfield.
            var lbbox = Bounds;
            lbbox.Minimum.Y = Bounds.Minimum.Y + hmin * heightfield.CellHeight;
            lbbox.Maximum.Y = Bounds.Minimum.Y + hmax * heightfield.CellHeight;

            return (lbbox, hmin, hmax);
        }

        /// <summary>
        /// Iterates over the layer data spans
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="layerId">Layer id</param>
        /// <returns>Returns each cell's x,y coordinates, the span and the area</returns>
        public IEnumerable<(int, int, CompactSpan, AreaTypes)> IterateLayerCellSpans(int x, int y, int layerId)
        {
            int cx = borderSize + x;
            int cy = borderSize + y;
            var c = heightfield.Cells[cx + cy * Width];

            for (int i = c.Index, nj = c.Index + c.Count; i < nj; i++)
            {
                // Skip unassigned regions.
                if (sourceRegions[i] == NULL_ID)
                {
                    continue;
                }

                // Skip of does not belong to current layer.
                int lid = regions[sourceRegions[i]].LayerId;
                if (lid != layerId)
                {
                    continue;
                }

                var s = heightfield.Spans[i];
                var a = heightfield.Areas[i];

                yield return (cx, cy, s, a);
            }
        }
    }
}
