using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX;

namespace Engine.Geometry
{
    /// <summary>
    /// A more memory-compact heightfield that stores open spans of voxels instead of closed ones.
    /// </summary>
    public class CompactHeightfield
    {
        /// <summary>
        /// Gets the width of the <see cref="CompactHeightfield"/> in voxel units.
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Gets the height of the <see cref="CompactHeightfield"/> in voxel units.
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Gets the length of the <see cref="CompactHeightfield"/> in voxel units.
        /// </summary>
        public int Length { get; private set; }
        /// <summary>
        /// Gets the world-space bounding box.
        /// </summary>
        public BoundingBox Bounds { get; private set; }
        /// <summary>
        /// Gets the world-space size of a cell in the XZ plane.
        /// </summary>
        public float CellSize { get; private set; }
        /// <summary>
        /// Gets the world-space size of a cell in the Y direction.
        /// </summary>
        public float CellHeight { get; private set; }
        /// <summary>
        /// Gets the maximum distance to a border based on the distance field. This value is undefined prior to
        /// calling <see cref="BuildDistanceField"/>.
        /// </summary>
        public int MaxDistance { get; private set; }
        /// <summary>
        /// Gets an array of distances from a span to the nearest border. This value is undefined prior to calling
        /// <see cref="BuildDistanceField"/>.
        /// </summary>
        public int[] Distances { get; private set; }
        /// <summary>
        /// Gets the size of the border.
        /// </summary>
        public int BorderSize { get; private set; }
        /// <summary>
        /// Gets the maximum number of allowed regions.
        /// </summary>
        public int MaxRegions { get; private set; }
        /// <summary>
        /// Gets the cells.
        /// </summary>
        public CompactCell[] Cells { get; private set; }
        /// <summary>
        /// Gets the spans.
        /// </summary>
        public CompactSpan[] Spans { get; private set; }
        /// <summary>
        /// Gets the area flags.
        /// </summary>
        public Area[] Areas { get; private set; }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of <see cref="CompactSpan"/> of the spans at a specified coordiante.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="CompactSpan"/>.</returns>
        public IEnumerable<CompactSpan> this[int x, int y]
        {
            get
            {
                CompactCell c = this.Cells[y * this.Width + x];

                int end = c.StartIndex + c.Count;
                for (int i = c.StartIndex; i < end; i++)
                {
                    yield return this.Spans[i];
                }
            }
        }
        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of <see cref="CompactSpan"/>s at a specified index.
        /// </summary>
        /// <param name="i">The index.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="CompactSpan"/>.</returns>
        public IEnumerable<CompactSpan> this[int i]
        {
            get
            {
                CompactCell c = this.Cells[i];

                int end = c.StartIndex + c.Count;
                for (int j = c.StartIndex; j < end; j++)
                {
                    yield return this.Spans[j];
                }
            }
        }
        /// <summary>
        /// Gets the <see cref="CompactSpan"/> specified by the reference.
        /// </summary>
        /// <param name="spanRef">A reference to a span in this <see cref="CompactHeightfield"/>.</param>
        /// <returns>The referenced span.</returns>
        public CompactSpan this[CompactSpanReference spanRef]
        {
            get
            {
                return this.Spans[spanRef.Index];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactHeightfield"/> class.
        /// </summary>
        /// <param name="field">A <see cref="Heightfield"/> to build from.</param>
        /// <param name="walkableHeight">The maximum difference in height to filter.</param>
        /// <param name="walkableClimb">The maximum difference in slope to filter.</param>
        public CompactHeightfield(Heightfield field, int walkableHeight, int walkableClimb)
        {
            this.Bounds = field.Bounds;
            this.Width = field.Width;
            this.Height = field.Height;
            this.Length = field.Length;
            this.CellSize = field.CellSizeXZ;
            this.CellHeight = field.CellHeight;

            int spanCount = field.SpanCount;
            this.Cells = new CompactCell[this.Width * this.Length];
            this.Spans = new CompactSpan[spanCount];
            this.Areas = new Area[spanCount];

            //iterate over the Heightfield's cells
            int spanIndex = 0;
            for (int i = 0; i < this.Cells.Length; i++)
            {
                //get the heightfield span list, skip if empty
                var fs = field[i].Spans;
                if (fs.Count == 0)
                {
                    continue;
                }

                CompactCell c = new CompactCell(spanIndex, 0);

                //convert the closed spans to open spans
                int lastInd = fs.Count - 1;
                for (int j = 0; j < lastInd; j++)
                {
                    var s = fs[j];
                    if (s.Area.IsWalkable)
                    {
                        CompactSpan res;
                        CompactSpan.FromMinMax(s.Maximum, fs[j + 1].Minimum, out res);
                        this.Spans[spanIndex] = res;
                        this.Areas[spanIndex] = s.Area;
                        spanIndex++;
                        c.Count++;
                    }
                }

                //the last closed span that has an "infinite" height
                var lastS = fs[lastInd];
                if (lastS.Area.IsWalkable)
                {
                    this.Spans[spanIndex] = new CompactSpan(fs[lastInd].Maximum, int.MaxValue);
                    this.Areas[spanIndex] = lastS.Area;
                    spanIndex++;
                    c.Count++;
                }

                this.Cells[i] = c;
            }

            //set neighbor connections
            for (int z = 0; z < this.Length; z++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    CompactCell c = this.Cells[z * this.Width + x];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        CompactSpan s = this.Spans[i];

                        for (var dir = Direction.West; dir <= Direction.South; dir++)
                        {
                            int dx = x + dir.GetHorizontalOffset();
                            int dz = z + dir.GetVerticalOffset();

                            if (dx < 0 || dz < 0 || dx >= this.Width || dz >= this.Length)
                            {
                                continue;
                            }

                            CompactCell dc = this.Cells[dz * this.Width + dx];
                            for (int j = dc.StartIndex, cellEnd = dc.StartIndex + dc.Count; j < cellEnd; j++)
                            {
                                CompactSpan ds = this.Spans[j];

                                int overlapBottom, overlapTop;
                                CompactSpan.OverlapMin(ref s, ref ds, out overlapBottom);
                                CompactSpan.OverlapMax(ref s, ref ds, out overlapTop);

                                //Make sure that the agent can walk to the next span and that the span isn't a huge drop or climb
                                if ((overlapTop - overlapBottom) >= walkableHeight && Math.Abs(ds.Minimum - s.Minimum) <= walkableClimb)
                                {
                                    int con = j - dc.StartIndex;
                                    CompactSpan.SetConnection(dir, con, ref this.Spans[i]);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Builds a distance field, or the distance to the nearest unwalkable area.
        /// </summary>
        public void BuildDistanceField()
        {
            if (this.Distances == null)
            {
                this.Distances = new int[this.Spans.Length];
            }

            //fill up all the values in src
            this.CalculateDistanceField(this.Distances);

            //blur the distances
            this.BoxBlur(this.Distances, 1);

            //find the maximum distance
            this.MaxDistance = 0;
            for (int i = 0; i < this.Distances.Length; i++)
            {
                this.MaxDistance = Math.Max(this.Distances[i], this.MaxDistance);
            }
        }
        /// <summary>
        /// Erodes the walkable areas in the map.
        /// </summary>
        /// <remarks>
        /// If you have already called <see cref="BuildDistanceField"/>, it will automatically be called again after
        /// erosion because it needs to be recalculated.
        /// </remarks>
        /// <param name="radius">The radius to erode from unwalkable areas.</param>
        public void Erode(int radius)
        {
            radius *= 2;

            //get a distance field
            int[] dists = new int[this.Spans.Length];
            this.CalculateDistanceField(dists);

            //erode close-to-null areas to null areas.
            for (int i = 0; i < this.Spans.Length; i++)
            {
                if (dists[i] < radius)
                {
                    this.Areas[i] = Area.Null;
                }
            }

            //marking areas as null changes the distance field, so recalculate it.
            if (this.Distances != null)
            {
                this.BuildDistanceField();
            }
        }
        /// <summary>
        /// The central method for building regions, which consists of connected, non-overlapping walkable spans.
        /// </summary>
        /// <param name="borderSize">The border size</param>
        /// <param name="minRegionArea">If smaller than this value, region will be null</param>
        /// <param name="mergeRegionArea">Reduce unneccesarily small regions</param>
        public void BuildRegions(int borderSize, int minRegionArea, int mergeRegionArea)
        {
            if (this.Distances == null)
            {
                this.BuildDistanceField();
            }

            const int LogStackCount = 3;
            const int StackCount = 1 << LogStackCount;
            List<CompactSpanReference>[] stacks = new List<CompactSpanReference>[StackCount];
            for (int i = 0; i < stacks.Length; i++)
            {
                stacks[i] = new List<CompactSpanReference>(1024);
            }

            RegionId[] regions = new RegionId[this.Spans.Length];
            int[] floodDistances = new int[this.Spans.Length];

            RegionId[] regionBuffer = new RegionId[this.Spans.Length];
            int[] distanceBuffer = new int[this.Spans.Length];

            int regionIndex = 1;
            int level = ((this.MaxDistance + 1) / 2) * 2;

            const int ExpandIters = 8;
            if (borderSize > 0)
            {
                //make sure border doesn't overflow
                int borderWidth = Math.Min(this.Width, borderSize);
                int borderHeight = Math.Min(this.Length, borderSize);

                //fill regions
                this.FillRectangleRegion(regions, new RegionId(regionIndex++, RegionFlags.Border), 0, borderWidth, 0, this.Length);
                this.FillRectangleRegion(regions, new RegionId(regionIndex++, RegionFlags.Border), this.Width - borderWidth, this.Width, 0, this.Length);
                this.FillRectangleRegion(regions, new RegionId(regionIndex++, RegionFlags.Border), 0, this.Width, 0, borderHeight);
                this.FillRectangleRegion(regions, new RegionId(regionIndex++, RegionFlags.Border), 0, this.Width, this.Length - borderHeight, this.Length);

                this.BorderSize = borderSize;
            }

            int stackId = -1;
            while (level > 0)
            {
                level = level >= 2 ? level - 2 : 0;
                stackId = (stackId + 1) & (StackCount - 1);

                if (stackId == 0)
                {
                    SortCellsByLevel(regions, stacks, level, StackCount, 1);
                }
                else
                {
                    AppendStacks(stacks[stackId - 1], stacks[stackId], regions);
                }

                //expand current regions until no new empty connected cells found
                this.ExpandRegions(regions, floodDistances, ExpandIters, level, stacks[stackId], regionBuffer, distanceBuffer);

                //mark new regions with ids
                for (int j = 0; j < stacks[stackId].Count; j++)
                {
                    var spanRef = stacks[stackId][j];
                    if (spanRef.Index >= 0 && regions[spanRef.Index] == 0)
                    {
                        if (this.FloodRegion(regions, floodDistances, regionIndex, level, ref spanRef))
                        {
                            regionIndex++;
                        }
                    }
                }
            }

            //expand current regions until no new empty connected cells found
            this.ExpandRegions(regions, floodDistances, ExpandIters * 8, 0, null, regionBuffer, distanceBuffer);

            //filter out small regions
            this.MaxRegions = this.FilterSmallRegions(regions, minRegionArea, mergeRegionArea, regionIndex);

            //write the result out
            for (int i = 0; i < this.Spans.Length; i++)
            {
                this.Spans[i].Region = regions[i];
            }
        }
        /// <summary>
        /// Merge two stacks to get a single stack.
        /// </summary>
        /// <param name="source">The original stack</param>
        /// <param name="destination">The new stack</param>
        /// <param name="regions">Region ids</param>
        private static void AppendStacks(List<CompactSpanReference> source, List<CompactSpanReference> destination, RegionId[] regions)
        {
            for (int j = 0; j < source.Count; j++)
            {
                var spanRef = source[j];
                if (spanRef.Index < 0 || regions[spanRef.Index] != 0)
                    continue;

                destination.Add(spanRef);
            }
        }
        /// <summary>
        /// Discards regions that are too small. 
        /// </summary>
        /// <param name="regionIds">region data</param>
        /// <param name="minRegionArea">The minimum area a region can have</param>
        /// <param name="mergeRegionSize">The size of the regions after merging</param>
        /// <param name="maxRegionId">determines the number of regions available</param>
        /// <returns>The reduced number of regions.</returns>
        private int FilterSmallRegions(RegionId[] regionIds, int minRegionArea, int mergeRegionSize, int maxRegionId)
        {
            int numRegions = maxRegionId + 1;
            Region[] regions = new Region[numRegions];

            //construct regions
            for (int i = 0; i < numRegions; i++)
            {
                regions[i] = new Region(i);
            }

            //find edge of a region and find connections around a contour
            for (int y = 0; y < this.Length; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    CompactCell c = this.Cells[x + y * this.Width];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        CompactSpanReference spanRef = new CompactSpanReference(x, y, i);

                        //HACK since the border region flag makes r negative, I changed r == 0 to r <= 0. Figure out exactly what maxRegionId's purpose is and see if Region.IsBorderOrNull is all we need.
                        int r = (int)regionIds[i];
                        if (r <= 0 || (int)r >= numRegions)
                        {
                            continue;
                        }

                        Region reg = regions[(int)r];
                        reg.SpanCount++;

                        //update floors
                        for (int j = c.StartIndex; j < end; j++)
                        {
                            if (i == j) continue;
                            RegionId floorId = regionIds[j];
                            if (floorId == 0 || (int)floorId >= numRegions)
                            {
                                continue;
                            }

                            reg.AddUniqueFloorRegion(floorId);
                        }

                        //have found contour
                        if (reg.Connections.Count > 0)
                        {
                            continue;
                        }

                        reg.AreaType = this.Areas[i];

                        //check if this cell is next to a border
                        for (var dir = Direction.West; dir <= Direction.South; dir++)
                        {
                            if (this.IsSolidEdge(regionIds, ref spanRef, dir))
                            {
                                //The cell is at a border. 
                                //Walk around contour to find all neighbors
                                this.WalkContour(regionIds, spanRef, dir, reg.Connections);
                                break;
                            }
                        }
                    }
                }
            }

            //Remove too small regions
            Stack<RegionId> stack = new Stack<RegionId>();
            List<RegionId> trace = new List<RegionId>();
            for (int i = 0; i < numRegions; i++)
            {
                Region reg = regions[i];
                if (reg.IsBorderOrNull || reg.SpanCount == 0 || reg.Visited)
                {
                    continue;
                }

                //count the total size of all connected regions
                //also keep track of the regions connections to a tile border
                bool connectsToBorder = false;
                int spanCount = 0;
                stack.Clear();
                trace.Clear();

                reg.Visited = true;
                stack.Push(reg.Id);

                while (stack.Count > 0)
                {
                    //pop
                    RegionId ri = stack.Pop();

                    Region creg = regions[(int)ri];

                    spanCount += creg.SpanCount;
                    trace.Add(ri);

                    for (int j = 0; j < creg.Connections.Count; j++)
                    {
                        if (RegionId.HasFlags(creg.Connections[j], RegionFlags.Border))
                        {
                            connectsToBorder = true;
                            continue;
                        }

                        Region neiReg = regions[(int)creg.Connections[j]];
                        if (neiReg.Visited || neiReg.IsBorderOrNull)
                        {
                            continue;
                        }

                        //visit
                        stack.Push(neiReg.Id);
                        neiReg.Visited = true;
                    }
                }

                //if the accumulated region size is too small, remove it
                //do not remove areas which connect to tile borders as their size can't be estimated correctly
                //and removing them can potentially remove necessary areas
                if (spanCount < minRegionArea && !connectsToBorder)
                {
                    //kill all visited regions
                    for (int j = 0; j < trace.Count; j++)
                    {
                        int index = (int)trace[j];

                        regions[index].SpanCount = 0;
                        regions[index].Id = RegionId.Null;
                    }
                }
            }

            //Merge too small regions to neighbor regions
            int mergeCount = 0;
            do
            {
                mergeCount = 0;
                for (int i = 0; i < numRegions; i++)
                {
                    Region reg = regions[i];
                    if (reg.IsBorderOrNull || reg.SpanCount == 0)
                    {
                        continue;
                    }

                    //check to see if region should be merged
                    if (reg.SpanCount > mergeRegionSize && reg.IsConnectedToBorder())
                    {
                        continue;
                    }

                    //small region with more than one connection or region which is not connected to border at all
                    //find smallest neighbor that connects to this one
                    int smallest = int.MaxValue;
                    RegionId mergeId = reg.Id;
                    for (int j = 0; j < reg.Connections.Count; j++)
                    {
                        if (RegionId.HasFlags(reg.Connections[j], RegionFlags.Border))
                        {
                            continue;
                        }

                        Region mreg = regions[(int)reg.Connections[j]];
                        if (mreg.IsBorderOrNull)
                        {
                            continue;
                        }

                        if (mreg.SpanCount < smallest && reg.CanMergeWith(mreg) && mreg.CanMergeWith(reg))
                        {
                            smallest = mreg.SpanCount;
                            mergeId = mreg.Id;
                        }
                    }

                    //found new id
                    if (mergeId != reg.Id)
                    {
                        RegionId oldId = reg.Id;
                        Region target = regions[(int)mergeId];

                        //merge regions
                        if (target.MergeWithRegion(reg))
                        {
                            //fix regions pointing to current region
                            for (int j = 0; j < numRegions; j++)
                            {
                                if (regions[j].IsBorderOrNull)
                                {
                                    continue;
                                }

                                //if another regions was already merged into current region
                                //change the nid of the previous region too
                                if (regions[j].Id == oldId)
                                {
                                    regions[j].Id = mergeId;
                                }

                                //replace current region with new one if current region is neighbor
                                regions[j].ReplaceNeighbor(oldId, mergeId);
                            }

                            mergeCount++;
                        }
                    }
                }
            }
            while (mergeCount > 0);

            //Compress region ids
            for (int i = 0; i < numRegions; i++)
            {
                regions[i].Remap = false;

                if (regions[i].IsBorderOrNull)
                {
                    continue;
                }

                regions[i].Remap = true;
            }

            int regIdGen = 0;
            for (int i = 0; i < numRegions; i++)
            {
                if (!regions[i].Remap)
                {
                    continue;
                }

                RegionId oldId = regions[i].Id;
                RegionId newId = new RegionId(++regIdGen);
                for (int j = i; j < numRegions; j++)
                {
                    if (regions[j].Id == oldId)
                    {
                        regions[j].Id = newId;
                        regions[j].Remap = false;
                    }
                }
            }

            //Remap regions
            for (int i = 0; i < this.Spans.Length; i++)
            {
                if (!RegionId.HasFlags(regionIds[i], RegionFlags.Border))
                {
                    regionIds[i] = regions[(int)regionIds[i]].Id;
                }
            }

            return regIdGen;
        }
        /// <summary>
        /// A distance field estimates how far each span is from its nearest border span. This data is needed for region generation.
        /// </summary>
        /// <param name="src">Array of values, each corresponding to an individual span</param>
        private void CalculateDistanceField(int[] src)
        {
            //initialize distance and points
            for (int i = 0; i < this.Spans.Length; i++)
            {
                src[i] = int.MaxValue;
            }

            //mark boundary cells
            for (int y = 0; y < this.Length; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    CompactCell c = this.Cells[y * this.Width + x];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        CompactSpan s = this.Spans[i];
                        Area area = this.Areas[i];

                        bool isBoundary = false;
                        if (s.ConnectionCount != 4)
                        {
                            isBoundary = true;
                        }
                        else
                        {
                            for (var dir = Direction.West; dir <= Direction.South; dir++)
                            {
                                int dx = x + dir.GetHorizontalOffset();
                                int dy = y + dir.GetVerticalOffset();
                                int di = this.Cells[dx + dy * this.Width].StartIndex + CompactSpan.GetConnection(ref s, dir);
                                if (area != this.Areas[di])
                                {
                                    isBoundary = true;
                                    break;
                                }
                            }
                        }

                        if (isBoundary)
                        {
                            src[i] = 0;
                        }
                    }
                }
            }

            //pass 1
            for (int y = 0; y < this.Length; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    CompactCell c = this.Cells[y * this.Width + x];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        CompactSpan s = this.Spans[i];

                        if (s.IsConnected(Direction.West))
                        {
                            //(-1, 0)
                            int dx = x + Direction.West.GetHorizontalOffset();
                            int dy = y + Direction.West.GetVerticalOffset();
                            int di = this.Cells[dx + dy * this.Width].StartIndex + CompactSpan.GetConnection(ref s, Direction.West);
                            CompactSpan ds = this.Spans[di];
                            if (src[di] + 2 < src[i])
                            {
                                src[i] = src[di] + 2;
                            }

                            //(-1, -1)
                            if (ds.IsConnected(Direction.South))
                            {
                                int ddx = dx + Direction.South.GetHorizontalOffset();
                                int ddy = dy + Direction.South.GetVerticalOffset();
                                int ddi = this.Cells[ddx + ddy * this.Width].StartIndex + CompactSpan.GetConnection(ref ds, Direction.South);
                                if (src[ddi] + 3 < src[i])
                                {
                                    src[i] = src[ddi] + 3;
                                }
                            }
                        }

                        if (s.IsConnected(Direction.South))
                        {
                            //(0, -1)
                            int dx = x + Direction.South.GetHorizontalOffset();
                            int dy = y + Direction.South.GetVerticalOffset();
                            int di = this.Cells[dx + dy * this.Width].StartIndex + CompactSpan.GetConnection(ref s, Direction.South);
                            CompactSpan ds = this.Spans[di];
                            if (src[di] + 2 < src[i])
                            {
                                src[i] = src[di] + 2;
                            }

                            //(1, -1)
                            if (ds.IsConnected(Direction.East))
                            {
                                int ddx = dx + Direction.East.GetHorizontalOffset();
                                int ddy = dy + Direction.East.GetVerticalOffset();
                                int ddi = this.Cells[ddx + ddy * this.Width].StartIndex + CompactSpan.GetConnection(ref ds, Direction.East);
                                if (src[ddi] + 3 < src[i])
                                {
                                    src[i] = src[ddi] + 3;
                                }
                            }
                        }
                    }
                }
            }

            //pass 2
            for (int y = this.Length - 1; y >= 0; y--)
            {
                for (int x = this.Width - 1; x >= 0; x--)
                {
                    CompactCell c = this.Cells[y * this.Width + x];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        CompactSpan s = this.Spans[i];

                        if (s.IsConnected(Direction.East))
                        {
                            //(1, 0)
                            int dx = x + Direction.East.GetHorizontalOffset();
                            int dy = y + Direction.East.GetVerticalOffset();
                            int di = this.Cells[dx + dy * this.Width].StartIndex + CompactSpan.GetConnection(ref s, Direction.East);
                            CompactSpan ds = this.Spans[di];
                            if (src[di] + 2 < src[i])
                            {
                                src[i] = src[di] + 2;
                            }

                            //(1, 1)
                            if (ds.IsConnected(Direction.North))
                            {
                                int ddx = dx + Direction.North.GetHorizontalOffset();
                                int ddy = dy + Direction.North.GetVerticalOffset();
                                int ddi = this.Cells[ddx + ddy * this.Width].StartIndex + CompactSpan.GetConnection(ref ds, Direction.North);
                                if (src[ddi] + 3 < src[i])
                                {
                                    src[i] = src[ddi] + 3;
                                }
                            }
                        }

                        if (s.IsConnected(Direction.North))
                        {
                            //(0, 1)
                            int dx = x + Direction.North.GetHorizontalOffset();
                            int dy = y + Direction.North.GetVerticalOffset();
                            int di = this.Cells[dx + dy * this.Width].StartIndex + CompactSpan.GetConnection(ref s, Direction.North);
                            CompactSpan ds = this.Spans[di];
                            if (src[di] + 2 < src[i])
                            {
                                src[i] = src[di] + 2;
                            }

                            //(-1, 1)
                            if (ds.IsConnected(Direction.West))
                            {
                                int ddx = dx + Direction.West.GetHorizontalOffset();
                                int ddy = dy + Direction.West.GetVerticalOffset();
                                int ddi = this.Cells[ddx + ddy * this.Width].StartIndex + CompactSpan.GetConnection(ref ds, Direction.West);
                                if (src[ddi] + 3 < src[i])
                                {
                                    src[i] = src[ddi] + 3;
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Part of building the distance field. It may or may not return an array equal to src.
        /// </summary>
        /// <param name="distances">The original distances.</param>
        /// <param name="threshold">The distance threshold below which no blurring occurs.</param>
        /// <param name="buffer">A buffer that is at least the same length as <see cref="distances"/> for working memory.</param>
        private void BoxBlur(int[] distances, int threshold, int[] buffer = null)
        {
            threshold *= 2;

            //if the optional buffer parameter wasn't passed in, or is too small, make a new one.
            if (buffer == null || buffer.Length < distances.Length)
                buffer = new int[distances.Length];

            Buffer.BlockCopy(distances, 0, buffer, 0, distances.Length * sizeof(int));

            //horizontal pass
            for (int y = 0; y < this.Length; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    CompactCell c = this.Cells[y * this.Width + x];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        CompactSpan s = this.Spans[i];
                        int cellDist = buffer[i];

                        //if the distance is below the threshold, skip the span.
                        if (cellDist <= threshold)
                        {
                            continue;
                        }

                        //iterate the full neighborhood of 8 spans.
                        int d = cellDist;
                        for (Direction dir = Direction.West; dir <= Direction.South; dir++)
                        {
                            if (s.IsConnected(dir))
                            {
                                int dx = x + dir.GetHorizontalOffset();
                                int dy = y + dir.GetVerticalOffset();
                                int di = this.Cells[dy * this.Width + dx].StartIndex + CompactSpan.GetConnection(ref s, dir);

                                d += buffer[di];

                                CompactSpan ds = this.Spans[di];
                                Direction dir2 = dir.NextClockwise();
                                if (ds.IsConnected(dir2))
                                {
                                    int dx2 = dx + dir2.GetHorizontalOffset();
                                    int dy2 = dy + dir2.GetVerticalOffset();
                                    int di2 = this.Cells[dy2 * this.Width + dx2].StartIndex + CompactSpan.GetConnection(ref ds, dir2);

                                    d += buffer[di2];
                                }
                                else
                                {
                                    d += cellDist;
                                }
                            }
                            else
                            {
                                //add the center span if there's no connection.
                                d += cellDist * 2;
                            }
                        }

                        //save new value to destination
                        distances[i] = (d + 5) / 9;
                    }
                }
            }
        }
        /// <summary>
        /// Expands regions to include spans above a specified water level.
        /// </summary>
        /// <param name="regions">The array of region IDs.</param>
        /// <param name="floodDistances">The array of flooding distances.</param>
        /// <param name="maxIterations">The maximum number of allowed iterations before breaking.</param>
        /// <param name="level">The current water level.</param>
        /// <param name="stack">A stack of span references that are being expanded.</param>
        /// <param name="regionBuffer">A buffer to store region IDs. Must be at least the same size as <c>regions</c>.</param>
        /// <param name="distanceBuffer">A buffer to store flood distances. Must be at least the same size as <c>floodDistances</c>.</param>
        private void ExpandRegions(RegionId[] regions, int[] floodDistances, int maxIterations, int level, List<CompactSpanReference> stack = null, RegionId[] regionBuffer = null, int[] distanceBuffer = null)
        {
            //generate buffers if they're not passed in or if they're too small.
            if (regionBuffer == null || regionBuffer.Length < regions.Length)
            {
                regionBuffer = new RegionId[regions.Length];
            }

            if (distanceBuffer == null || distanceBuffer.Length < floodDistances.Length)
            {
                distanceBuffer = new int[floodDistances.Length];
            }

            //copy existing data into the buffers.
            Array.Copy(regions, 0, regionBuffer, 0, regions.Length);
            Array.Copy(floodDistances, 0, distanceBuffer, 0, floodDistances.Length);

            //find cells that are being expanded to.
            if (stack == null)
            {
                stack = new List<CompactSpanReference>();
                for (int y = 0; y < this.Length; y++)
                {
                    for (int x = 0; x < this.Width; x++)
                    {
                        CompactCell c = this.Cells[x + y * this.Width];
                        for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                        {
                            //a cell is being expanded to if it's distance is greater than the current level,
                            //but no region has been asigned yet. It must also not be in a null area.
                            if (this.Distances[i] >= level && regions[i] == 0 && this.Areas[i].IsWalkable)
                            {
                                stack.Add(new CompactSpanReference(x, y, i));
                            }
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < stack.Count; j++)
                {
                    if (regions[stack[j].Index] != 0)
                    {
                        stack[j] = CompactSpanReference.Null;
                    }
                }
            }

            //assign regions to all the cells that are being expanded to.
            //will run until it's done or it runs maxIterations times.
            int iter = 0;
            while (stack.Count > 0)
            {
                //spans in the stack that are skipped:
                // - assigned a region ID in an earlier iteration
                // - not neighboring any spans with region IDs
                int skipped = 0;

                for (int j = 0; j < stack.Count; j++)
                {
                    CompactSpanReference spanRef = stack[j];
                    int x = spanRef.X;
                    int y = spanRef.Y;
                    int i = spanRef.Index;

                    //skip regions already assigned to
                    if (i < 0)
                    {
                        skipped++;
                        continue;
                    }

                    RegionId r = regions[i];
                    Area area = this.Areas[i];
                    CompactSpan s = this.Spans[i];

                    //search direct neighbors for the one with the smallest distance value
                    int minDist = int.MaxValue;
                    for (var dir = Direction.West; dir <= Direction.South; dir++)
                    {
                        if (!s.IsConnected(dir))
                        {
                            continue;
                        }

                        int dx = x + dir.GetHorizontalOffset();
                        int dy = y + dir.GetVerticalOffset();
                        int di = this.Cells[dx + dy * this.Width].StartIndex + CompactSpan.GetConnection(ref s, dir);

                        if (this.Areas[di] != area)
                        {
                            continue;
                        }

                        //compare distance to previous best
                        RegionId ri = regions[di];
                        int dist = floodDistances[di];
                        if (!(ri.IsNull || RegionId.HasFlags(ri, RegionFlags.Border)))
                        {
                            //set region and distance if better
                            if (dist + 2 < minDist)
                            {
                                r = ri;
                                minDist = dist + 2;
                            }
                        }
                    }

                    if (r != 0)
                    {
                        //set the region and distance for this span
                        regionBuffer[i] = r;
                        distanceBuffer[i] = minDist;

                        //mark this item in the stack as assigned for the next iteration.
                        stack[j] = CompactSpanReference.Null;
                    }
                    else
                    {
                        //skip spans that don't neighbor any regions
                        skipped++;
                    }
                }

                //if the entire stack is being skipped, we're done.
                if (skipped == stack.Count)
                {
                    break;
                }

                //Copy from the buffers back to the original arrays. This is done after each iteration
                //because changing it in-place has some side effects for the other spans in the stack.
                Array.Copy(regionBuffer, 0, regions, 0, regions.Length);
                Array.Copy(distanceBuffer, 0, floodDistances, 0, floodDistances.Length);

                if (level > 0)
                {
                    //if we hit maxIterations before expansion is done, break out anyways.
                    ++iter;
                    if (iter >= maxIterations)
                    {
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Floods the regions at a certain level
        /// </summary>
        /// <param name="regions">source region</param>
        /// <param name="floodDistances">source distances</param>
        /// <param name="regionIndex">region id</param>
        /// <param name="level">current level</param>
        /// <param name="start">A reference to the starting span.</param>
        /// <returns>Always true.</returns>
        private bool FloodRegion(RegionId[] regions, int[] floodDistances, int regionIndex, int level, ref CompactSpanReference start)
        {
            //TODO this method should always return true, make it not return a bool?
            //flood fill mark region
            Stack<CompactSpanReference> stack = new Stack<CompactSpanReference>();
            stack.Push(start);

            Area area = this.Areas[start.Index];
            regions[start.Index] = new RegionId(regionIndex);
            floodDistances[start.Index] = 0;

            int lev = level >= 2 ? level - 2 : 0;
            int count = 0;

            while (stack.Count > 0)
            {
                CompactSpanReference cell = stack.Pop();
                CompactSpan cs = this.Spans[cell.Index];

                //check if any of the neighbors already have a valid reigon set
                RegionId ar = RegionId.Null;
                for (var dir = Direction.West; dir <= Direction.South; dir++)
                {
                    //8 connected
                    if (cs.IsConnected(dir))
                    {
                        int dx = cell.X + dir.GetHorizontalOffset();
                        int dy = cell.Y + dir.GetVerticalOffset();
                        int di = this.Cells[dx + dy * this.Width].StartIndex + CompactSpan.GetConnection(ref cs, dir);

                        if (this.Areas[di] != area)
                        {
                            continue;
                        }

                        RegionId nr = regions[di];

                        if (RegionId.HasFlags(nr, RegionFlags.Border))
                        {
                            //skip borders
                            continue;
                        }

                        if (nr != 0 && nr != regionIndex)
                        {
                            ar = nr;
                            break;
                        }

                        CompactSpan ds = this.Spans[di];
                        Direction dir2 = dir.NextClockwise();
                        if (ds.IsConnected(dir2))
                        {
                            int dx2 = dx + dir2.GetHorizontalOffset();
                            int dy2 = dy + dir2.GetVerticalOffset();
                            int di2 = this.Cells[dx2 + dy2 * this.Width].StartIndex + CompactSpan.GetConnection(ref ds, dir2);

                            if (this.Areas[di2] != area)
                            {
                                continue;
                            }

                            RegionId nr2 = regions[di2];
                            if (nr2 != 0 && nr2 != regionIndex)
                            {
                                ar = nr2;
                                break;
                            }
                        }
                    }
                }

                if (ar != 0)
                {
                    regions[cell.Index] = RegionId.Null;
                    continue;
                }

                count++;

                //expand neighbors
                for (var dir = Direction.West; dir <= Direction.South; dir++)
                {
                    if (cs.IsConnected(dir))
                    {
                        int dx = cell.X + dir.GetHorizontalOffset();
                        int dy = cell.Y + dir.GetVerticalOffset();
                        int di = this.Cells[dx + dy * this.Width].StartIndex + CompactSpan.GetConnection(ref cs, dir);

                        if (this.Areas[di] != area)
                        {
                            continue;
                        }

                        if (this.Distances[di] >= lev && regions[di] == 0)
                        {
                            regions[di] = new RegionId(regionIndex);
                            floodDistances[di] = 0;
                            stack.Push(new CompactSpanReference(dx, dy, di));
                        }
                    }
                }
            }

            return count > 0;
        }
        /// <summary>
        /// Checks whether the edge from a span in a direction is a solid edge.
        /// A solid edge is an edge between two regions.
        /// </summary>
        /// <param name="regions">The region ID array.</param>
        /// <param name="spanRef">A reference to the span connected to the edge.</param>
        /// <param name="dir">The direction of the edge.</param>
        /// <returns>A value indicating whether the described edge is solid.</returns>
        private bool IsSolidEdge(RegionId[] regions, ref CompactSpanReference spanRef, Direction dir)
        {
            CompactSpan s = this.Spans[spanRef.Index];
            RegionId r = RegionId.Null;

            if (s.IsConnected(dir))
            {
                int dx = spanRef.X + dir.GetHorizontalOffset();
                int dy = spanRef.Y + dir.GetVerticalOffset();
                int di = this.Cells[dx + dy * this.Width].StartIndex + CompactSpan.GetConnection(ref s, dir);
                r = regions[di];
            }

            if (r == regions[spanRef.Index])
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Try to visit all the spans. May be needed in filtering small regions. 
        /// </summary>
        /// <param name="regions">an array of region values</param>
        /// <param name="spanRef">The span to start walking from.</param>
        /// <param name="dir">The direction to start walking in.</param>
        /// <param name="cont">A collection of regions to append to.</param>
        private void WalkContour(RegionId[] regions, CompactSpanReference spanRef, Direction dir, List<RegionId> cont)
        {
            Direction startDir = dir;
            int starti = spanRef.Index;

            CompactSpan ss = this.Spans[starti];
            RegionId curReg = RegionId.Null;

            if (ss.IsConnected(dir))
            {
                int dx = spanRef.X + dir.GetHorizontalOffset();
                int dy = spanRef.Y + dir.GetVerticalOffset();
                int di = this.Cells[dx + dy * this.Width].StartIndex + CompactSpan.GetConnection(ref ss, dir);
                curReg = regions[di];
            }

            cont.Add(curReg);

            int iter = 0;
            while (++iter < 40000)
            {
                CompactSpan s = this.Spans[spanRef.Index];

                if (IsSolidEdge(regions, ref spanRef, dir))
                {
                    //choose the edge corner
                    RegionId r = RegionId.Null;
                    if (s.IsConnected(dir))
                    {
                        int dx = spanRef.X + dir.GetHorizontalOffset();
                        int dy = spanRef.Y + dir.GetVerticalOffset();
                        int di = this.Cells[dx + dy * this.Width].StartIndex + CompactSpan.GetConnection(ref s, dir);
                        r = regions[di];
                    }

                    if (r != curReg)
                    {
                        curReg = r;
                        cont.Add(curReg);
                    }

                    dir = dir.NextClockwise(); //rotate clockwise
                }
                else
                {
                    int di = -1;
                    int dx = spanRef.X + dir.GetHorizontalOffset();
                    int dy = spanRef.Y + dir.GetVerticalOffset();

                    if (s.IsConnected(dir))
                    {
                        CompactCell dc = this.Cells[dx + dy * this.Width];
                        di = dc.StartIndex + CompactSpan.GetConnection(ref s, dir);
                    }

                    if (di == -1)
                    {
                        //shouldn't happen
                        return;
                    }

                    spanRef = new CompactSpanReference(dx, dy, di);
                    dir = dir.NextCounterClockwise(); //rotate counterclockwise
                }

                if (starti == spanRef.Index && startDir == dir)
                {
                    break;
                }
            }

            //remove adjacent duplicates
            if (cont.Count > 1)
            {
                for (int j = 0; j < cont.Count; )
                {
                    //next element
                    int nj = (j + 1) % cont.Count;

                    //adjacent duplicate found
                    if (cont[j] == cont[nj])
                    {
                        cont.RemoveAt(j);
                    }
                    else
                    {
                        j++;
                    }
                }
            }
        }
        /// <summary>
        /// Fill in a rectangular area with a region ID. Spans in a null area are skipped.
        /// </summary>
        /// <param name="regions">The region ID array.</param>
        /// <param name="newRegionId">The ID to fill in.</param>
        /// <param name="left">The left edge of the rectangle.</param>
        /// <param name="right">The right edge of the rectangle.</param>
        /// <param name="bottom">The bottom edge of the rectangle.</param>
        /// <param name="top">The top edge of the rectangle.</param>
        private void FillRectangleRegion(RegionId[] regions, RegionId newRegionId, int left, int right, int bottom, int top)
        {
            for (int y = bottom; y < top; y++)
            {
                for (int x = left; x < right; x++)
                {
                    CompactCell c = this.Cells[x + y * this.Width];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        if (this.Areas[i].IsWalkable)
                        {
                            regions[i] = newRegionId;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Sort the compact spans
        /// </summary>
        /// <param name="regions">Region data</param>
        /// <param name="stacks">Temporary stack of CompactSpanReference values</param>
        /// <param name="startlevel">Starting level</param>
        /// <param name="numStacks">The number of layers</param>
        /// <param name="logLevelsPerStack">log base 2 of stack levels</param>
        private void SortCellsByLevel(RegionId[] regions, List<CompactSpanReference>[] stacks, int startlevel, int numStacks, int logLevelsPerStack)
        {
            startlevel = startlevel >> logLevelsPerStack;
            for (int j = 0; j < numStacks; j++)
            {
                stacks[j].Clear();
            }

            for (int y = 0; y < this.Length; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    CompactCell c = this.Cells[y * this.Width + x];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        if (!this.Areas[i].IsWalkable || !regions[i].IsNull)
                        {
                            continue;
                        }

                        int level = this.Distances[i] >> logLevelsPerStack;
                        int sId = startlevel - level;
                        if (sId >= numStacks)
                        {
                            continue;
                        }

                        if (sId < 0)
                        {
                            sId = 0;
                        }

                        stacks[sId].Add(new CompactSpanReference(x, y, i));
                    }
                }
            }
        }
        /// <summary>
        /// Builds a set of <see cref="Contour"/>s around the generated regions. Must be called after regions are generated.
        /// </summary>
        /// <param name="maxError">The maximum allowed deviation in a simplified contour from a raw one.</param>
        /// <param name="maxEdgeLength">The maximum edge length.</param>
        /// <param name="buildFlags">Flags that change settings for the build process.</param>
        /// <returns>A <see cref="ContourSet"/> containing one contour per region.</returns>
        public ContourSet BuildContourSet(float maxError, int maxEdgeLength, ContourBuildFlags buildFlags)
        {
            BoundingBox contourSetBounds = this.Bounds;
            if (this.BorderSize > 0)
            {
                //remove offset
                float pad = this.BorderSize * this.CellSize;
                contourSetBounds.Minimum.X += pad;
                contourSetBounds.Minimum.Z += pad;
                contourSetBounds.Maximum.X -= pad;
                contourSetBounds.Maximum.Z -= pad;
            }

            int contourSetWidth = this.Width - this.BorderSize * 2;
            int contourSetLength = this.Length - this.BorderSize * 2;

            int maxContours = Math.Max(this.MaxRegions, 8);
            var contours = new List<Contour>(maxContours);

            EdgeFlags[] flags = new EdgeFlags[this.Spans.Length];

            //Modify flags array by using the CompactHeightfield data
            //mark boundaries
            for (int z = 0; z < this.Length; z++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    //loop through all the spans in the cell
                    CompactCell c = this.Cells[x + z * this.Width];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        CompactSpan s = this.Spans[i];

                        //set the flag to 0 if the region is a border region or null.
                        if (s.Region.IsNull || RegionId.HasFlags(s.Region, RegionFlags.Border))
                        {
                            flags[i] = 0;
                            continue;
                        }

                        //go through all the neighboring cells
                        for (var dir = Direction.West; dir <= Direction.South; dir++)
                        {
                            //obtain region id
                            RegionId r = RegionId.Null;
                            if (s.IsConnected(dir))
                            {
                                int dx = x + dir.GetHorizontalOffset();
                                int dz = z + dir.GetVerticalOffset();
                                int di = this.Cells[dx + dz * this.Width].StartIndex + CompactSpan.GetConnection(ref s, dir);
                                r = this.Spans[di].Region;
                            }

                            //region ids are equal
                            if (r == s.Region)
                            {
                                //res marks all the internal edges
                                EdgeFlagsHelper.AddEdge(ref flags[i], dir);
                            }
                        }

                        //flags represents all the nonconnected edges, edges that are only internal
                        //the edges need to be between different regions
                        EdgeFlagsHelper.FlipEdges(ref flags[i]);
                    }
                }
            }

            var verts = new List<ContourVertex>();
            var simplified = new List<ContourVertex>();

            for (int z = 0; z < this.Length; z++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    CompactCell c = this.Cells[x + z * this.Width];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        //flags is either 0000 or 1111
                        //in other words, not connected at all 
                        //or has all connections, which means span is in the middle and thus not an edge.
                        if (flags[i] == EdgeFlags.None || flags[i] == EdgeFlags.All)
                        {
                            flags[i] = EdgeFlags.None;
                            continue;
                        }

                        var spanRef = new CompactSpanReference(x, z, i);
                        RegionId reg = this[spanRef].Region;
                        if (reg.IsNull || RegionId.HasFlags(reg, RegionFlags.Border))
                        {
                            continue;
                        }

                        //reset each iteration
                        verts.Clear();
                        simplified.Clear();

                        //Walk along a contour, then build it
                        WalkContour(spanRef, flags, verts);
                        Contour.Simplify(verts, simplified, maxError, maxEdgeLength, buildFlags);
                        Contour.RemoveDegenerateSegments(simplified);
                        Contour contour = new Contour(simplified, reg, this.Areas[i], this.BorderSize);

                        if (!contour.IsNull)
                        {
                            contours.Add(contour);
                        }
                    }
                }
            }

            //Check and merge bad contours
            for (int i = 0; i < contours.Count; i++)
            {
                Contour cont = contours[i];

                //Check if contour is backwards
                if (cont.Area2D < 0)
                {
                    //Find another contour to merge with
                    int mergeIndex = -1;
                    for (int j = 0; j < contours.Count; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        //Must have at least one vertex, the same region ID, and be going forwards.
                        Contour contj = contours[j];
                        if (contj.Vertices.Length != 0 && contj.RegionId == cont.RegionId && contj.Area2D > 0)
                        {
                            mergeIndex = j;
                            break;
                        }
                    }

                    //Merge if found.
                    if (mergeIndex != -1)
                    {
                        contours[mergeIndex].MergeWith(cont);
                        contours.RemoveAt(i);
                        i--;
                    }
                }
            }

            return new ContourSet(contours, contourSetBounds, contourSetWidth, contourSetLength);
        }
        /// <summary>
        /// Initial generation of the contours
        /// </summary>
        /// <param name="spanReference">A referecne to the span to start walking from.</param>
        /// <param name="flags">An array of flags determinining </param>
        /// <param name="points">The vertices of a contour.</param>
        private void WalkContour(CompactSpanReference spanReference, EdgeFlags[] flags, List<ContourVertex> points)
        {
            Direction dir = Direction.West;

            //find the first direction that has a connection 
            while (!EdgeFlagsHelper.IsConnected(ref flags[spanReference.Index], dir))
            {
                dir++;
            }

            Direction startDir = dir;
            int startIndex = spanReference.Index;

            Area area = this.Areas[startIndex];

            //TODO make the max iterations value a variable
            int iter = 0;
            while (++iter < 40000)
            {
                // this direction is connected
                if (EdgeFlagsHelper.IsConnected(ref flags[spanReference.Index], dir))
                {
                    // choose the edge corner
                    bool isBorderVertex;
                    bool isAreaBorder = false;

                    int px = spanReference.X;
                    int py = GetCornerHeight(spanReference, dir, out isBorderVertex);
                    int pz = spanReference.Y;

                    switch (dir)
                    {
                        case Direction.West:
                            pz++;
                            break;
                        case Direction.North:
                            px++;
                            pz++;
                            break;
                        case Direction.East:
                            px++;
                            break;
                    }

                    RegionId r = RegionId.Null;
                    CompactSpan s = this[spanReference];
                    if (s.IsConnected(dir))
                    {
                        int dx = spanReference.X + dir.GetHorizontalOffset();
                        int dy = spanReference.Y + dir.GetVerticalOffset();
                        int di = this.Cells[dx + dy * this.Width].StartIndex + CompactSpan.GetConnection(ref s, dir);
                        r = this.Spans[di].Region;
                        if (area != this.Areas[di])
                        {
                            isAreaBorder = true;
                        }
                    }

                    // apply flags if neccessary
                    if (isBorderVertex)
                    {
                        r = RegionId.WithFlags(r, RegionFlags.VertexBorder);
                    }

                    if (isAreaBorder)
                    {
                        r = RegionId.WithFlags(r, RegionFlags.AreaBorder);
                    }

                    //save the point
                    points.Add(new ContourVertex(px, py, pz, r));

                    EdgeFlagsHelper.RemoveEdge(ref flags[spanReference.Index], dir);	// remove visited edges
                    dir = dir.NextClockwise();			// rotate clockwise
                }
                else
                {
                    //get a new cell(x, y) and span index(i)
                    int di = -1;
                    int dx = spanReference.X + dir.GetHorizontalOffset();
                    int dy = spanReference.Y + dir.GetVerticalOffset();

                    CompactSpan s = this[spanReference];
                    if (s.IsConnected(dir))
                    {
                        CompactCell dc = this.Cells[dx + dy * this.Width];
                        di = dc.StartIndex + CompactSpan.GetConnection(ref s, dir);
                    }

                    if (di == -1)
                    {
                        // shouldn't happen
                        // TODO if this shouldn't happen, this check shouldn't be necessary.
                        throw new InvalidOperationException("Something went wrong");
                    }

                    spanReference = new CompactSpanReference(dx, dy, di);
                    dir = dir.NextCounterClockwise(); // rotate counterclockwise
                }

                if (startIndex == spanReference.Index && startDir == dir)
                {
                    break;
                }
            }
        }
        /// <summary>
        /// Helper method for WalkContour function
        /// </summary>
        /// <param name="sr">The span to get the corner height for.</param>
        /// <param name="dir">The direction to get the corner height from.</param>
        /// <param name="isBorderVertex">Determine whether the vertex is a border or not.</param>
        /// <returns>The corner height.</returns>
        private int GetCornerHeight(CompactSpanReference sr, Direction dir, out bool isBorderVertex)
        {
            isBorderVertex = false;

            CompactSpan s = this[sr];
            int cornerHeight = s.Minimum;
            Direction dirp = dir.NextClockwise(); //new clockwise direction

            RegionId[] cornerRegs = new RegionId[4];
            Area[] cornerAreas = new Area[4];

            //combine region and area codes in order to prevent border vertices, which are in between two areas, to be removed 
            cornerRegs[0] = s.Region;
            cornerAreas[0] = this.Areas[sr.Index];

            if (s.IsConnected(dir))
            {
                //get neighbor span
                int dx = sr.X + dir.GetHorizontalOffset();
                int dy = sr.Y + dir.GetVerticalOffset();
                int di = this.Cells[dx + dy * this.Width].StartIndex + CompactSpan.GetConnection(ref s, dir);
                CompactSpan ds = this.Spans[di];

                cornerHeight = Math.Max(cornerHeight, ds.Minimum);
                cornerRegs[1] = this.Spans[di].Region;
                cornerAreas[1] = this.Areas[di];

                //get neighbor of neighbor's span
                if (ds.IsConnected(dirp))
                {
                    int dx2 = dx + dirp.GetHorizontalOffset();
                    int dy2 = dy + dirp.GetVerticalOffset();
                    int di2 = this.Cells[dx2 + dy2 * this.Width].StartIndex + CompactSpan.GetConnection(ref ds, dirp);
                    CompactSpan ds2 = this.Spans[di2];

                    cornerHeight = Math.Max(cornerHeight, ds2.Minimum);
                    cornerRegs[2] = ds2.Region;
                    cornerAreas[2] = this.Areas[di2];
                }
            }

            //get neighbor span
            if (s.IsConnected(dirp))
            {
                int dx = sr.X + dirp.GetHorizontalOffset();
                int dy = sr.Y + dirp.GetVerticalOffset();
                int di = this.Cells[dx + dy * this.Width].StartIndex + CompactSpan.GetConnection(ref s, dirp);
                CompactSpan ds = this.Spans[di];

                cornerHeight = Math.Max(cornerHeight, ds.Minimum);
                cornerRegs[3] = ds.Region;
                cornerAreas[3] = this.Areas[di];

                //get neighbor of neighbor's span
                if (ds.IsConnected(dir))
                {
                    int dx2 = dx + dir.GetHorizontalOffset();
                    int dy2 = dy + dir.GetVerticalOffset();
                    int di2 = this.Cells[dx2 + dy2 * this.Width].StartIndex + CompactSpan.GetConnection(ref ds, dir);
                    CompactSpan ds2 = this.Spans[di2];

                    cornerHeight = Math.Max(cornerHeight, ds2.Minimum);
                    cornerRegs[2] = ds2.Region;
                    cornerAreas[2] = this.Areas[di2];
                }
            }

            //check if vertex is special edge vertex
            //if so, these vertices will be removed later
            for (int j = 0; j < 4; j++)
            {
                int a = j;
                int b = (j + 1) % 4;
                int c = (j + 2) % 4;
                int d = (j + 3) % 4;

                RegionId ra = cornerRegs[a], rb = cornerRegs[b], rc = cornerRegs[c], rd = cornerRegs[d];
                Area aa = cornerAreas[a], ab = cornerAreas[b], ac = cornerAreas[c], ad = cornerAreas[d];

                //the vertex is a border vertex if:
                //two same exterior cells in a row followed by two interior cells and none of the regions are out of bounds
                bool twoSameExteriors = RegionId.HasFlags(ra, RegionFlags.Border) && RegionId.HasFlags(rb, RegionFlags.Border) && (ra == rb && aa == ab);
                bool twoSameInteriors = !(RegionId.HasFlags(rc, RegionFlags.Border) || RegionId.HasFlags(rd, RegionFlags.Border));
                bool intsSameArea = ac == ad;
                bool noZeros = ra != 0 && rb != 0 && rc != 0 && rd != 0 && aa != 0 && ab != 0 && ac != 0 && ad != 0;
                if (twoSameExteriors && twoSameInteriors && intsSameArea && noZeros)
                {
                    isBorderVertex = true;
                    break;
                }
            }

            return cornerHeight;
        }
    }

    /// <summary>
    /// Represents a cell in a <see cref="CompactHeightfield"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CompactCell
    {
        /// <summary>
        /// The starting index of spans in a <see cref="CompactHeightfield"/> for this cell.
        /// </summary>
        public int StartIndex;
        /// <summary>
        /// The number of spans in a <see cref="CompactHeightfield"/> for this cell.
        /// </summary>
        public int Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactCell"/> struct.
        /// </summary>
        /// <param name="start">The start index.</param>
        /// <param name="count">The count.</param>
        public CompactCell(int start, int count)
        {
            StartIndex = start;
            Count = count;
        }
    }

    /// <summary>
    /// Represents a voxel span in a <see cref="CompactHeightfield"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CompactSpan
    {
        /// <summary>
        /// A constant that means there is no connection for the values <see cref="ConnectionWest"/>,
        /// <see cref="ConnectionNorth"/>, <see cref="ConnectionEast"/>, and <see cref="ConnectionSouth"/>.
        /// </summary>
        private const byte NotConnected = 0xff;

        /// <summary>
        /// If two CompactSpans overlap, find the minimum of the new overlapping CompactSpans.
        /// </summary>
        /// <param name="left">The first CompactSpan</param>
        /// <param name="right">The second CompactSpan</param>
        /// <param name="min">The minimum of the overlapping ComapctSpans</param>
        public static void OverlapMin(ref CompactSpan left, ref CompactSpan right, out int min)
        {
            min = Math.Max(left.Minimum, right.Minimum);
        }
        /// <summary>
        /// If two CompactSpans overlap, find the maximum of the new overlapping CompactSpans.
        /// </summary>
        /// <param name="left">The first CompactSpan</param>
        /// <param name="right">The second CompactSpan</param>
        /// <param name="max">The maximum of the overlapping CompactSpans</param>
        public static void OverlapMax(ref CompactSpan left, ref CompactSpan right, out int max)
        {
            if (left.Height == int.MaxValue)
            {
                if (right.Height == int.MaxValue)
                    max = int.MaxValue;
                else
                    max = right.Minimum + right.Height;
            }
            else if (right.Height == int.MaxValue)
                max = left.Minimum + left.Height;
            else
                max = Math.Min(left.Minimum + left.Height, right.Minimum + right.Height);
        }
        /// <summary>
        /// Creates a <see cref="CompactSpan"/> from a minimum boundary and a maximum boundary.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>A <see cref="CompactSpan"/>.</returns>
        public static CompactSpan FromMinMax(int min, int max)
        {
            CompactSpan s;
            FromMinMax(min, max, out s);
            return s;
        }
        /// <summary>
        /// Creates a <see cref="CompactSpan"/> from a minimum boundary and a maximum boundary.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="span">A <see cref="CompactSpan"/>.</param>
        public static void FromMinMax(int min, int max, out CompactSpan span)
        {
            span.Minimum = min;
            span.Height = max - min;
            span.ConnectionWest = NotConnected;
            span.ConnectionNorth = NotConnected;
            span.ConnectionEast = NotConnected;
            span.ConnectionSouth = NotConnected;
            span.Region = RegionId.Null;
        }
        /// <summary>
        /// Sets connection data to a span contained in a neighboring cell.
        /// </summary>
        /// <param name="dir">The direction of the cell.</param>
        /// <param name="i">The index of the span in the neighboring cell.</param>
        /// <param name="s">The <see cref="CompactSpan"/> to set the data for.</param>
        public static void SetConnection(Direction dir, int i, ref CompactSpan s)
        {
            if (i >= NotConnected)
                throw new ArgumentOutOfRangeException("Index of connecting span is too high to be stored. Try increasing cell height.", "i");

            switch (dir)
            {
                case Direction.West:
                    s.ConnectionWest = (byte)i;
                    break;
                case Direction.North:
                    s.ConnectionNorth = (byte)i;
                    break;
                case Direction.East:
                    s.ConnectionEast = (byte)i;
                    break;
                case Direction.South:
                    s.ConnectionSouth = (byte)i;
                    break;
                default:
                    throw new ArgumentException("dir isn't a valid Direction.");
            }
        }
        /// <summary>
        /// Un-sets connection data from a neighboring cell.
        /// </summary>
        /// <param name="dir">The direction of the cell.</param>
        /// <param name="s">The <see cref="CompactSpan"/> to set the data for.</param>
        public static void UnsetConnection(Direction dir, ref CompactSpan s)
        {
            switch (dir)
            {
                case Direction.West:
                    s.ConnectionWest = NotConnected;
                    break;
                case Direction.North:
                    s.ConnectionNorth = NotConnected;
                    break;
                case Direction.East:
                    s.ConnectionEast = NotConnected;
                    break;
                case Direction.South:
                    s.ConnectionSouth = NotConnected;
                    break;
                default:
                    throw new ArgumentException("dir isn't a valid Direction.");
            }
        }
        /// <summary>
        /// Gets the connection data for a neighboring cell in a specified direction.
        /// </summary>
        /// <param name="s">The <see cref="CompactSpan"/> to get the connection data from.</param>
        /// <param name="dir">The direction.</param>
        /// <returns>The index of the span in the neighboring cell.</returns>
        public static int GetConnection(ref CompactSpan s, Direction dir)
        {
            switch (dir)
            {
                case Direction.West:
                    return s.ConnectionWest;
                case Direction.North:
                    return s.ConnectionNorth;
                case Direction.East:
                    return s.ConnectionEast;
                case Direction.South:
                    return s.ConnectionSouth;
                default:
                    throw new ArgumentException("dir isn't a valid Direction.");
            }
        }

        /// <summary>
        /// The number of voxels contained in the span.
        /// </summary>
        public int Height;
        /// <summary>
        /// The span minimum.
        /// </summary>
        public int Minimum;
        /// <summary>
        /// Gets the upper bound of the span.
        /// </summary>
        public int Maximum
        {
            get
            {
                return Minimum + Height;
            }
        }
        /// <summary>
        /// A byte representing the index of the connected span in the cell directly to the west.
        /// </summary>
        public byte ConnectionWest;
        /// <summary>
        /// A byte representing the index of the connected span in the cell directly to the north.
        /// </summary>
        public byte ConnectionNorth;
        /// <summary>
        /// A byte representing the index of the connected span in the cell directly to the east.
        /// </summary>
        public byte ConnectionEast;
        /// <summary>
        /// A byte representing the index of the connected span in the cell directly to the south.
        /// </summary>
        public byte ConnectionSouth;
        /// <summary>
        /// The region the span belongs to.
        /// </summary>
        public RegionId Region;
        /// <summary>
        /// Gets a value indicating whether the span has an upper bound or goes to "infinity".
        /// </summary>
        public bool HasUpperBound
        {
            get
            {
                return Height != int.MaxValue;
            }
        }
        /// <summary>
        /// Gets the number of connections the current CompactSpan has with its neighbors.
        /// </summary>
        public int ConnectionCount
        {
            get
            {
                int count = 0;
                if (ConnectionWest != NotConnected)
                    count++;
                if (ConnectionNorth != NotConnected)
                    count++;
                if (ConnectionEast != NotConnected)
                    count++;
                if (ConnectionSouth != NotConnected)
                    count++;

                return count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactSpan"/> struct.
        /// </summary>
        /// <param name="minimum">The span minimum.</param>
        /// <param name="height">The number of voxels the span contains.</param>
        public CompactSpan(int minimum, int height)
        {
            this.Minimum = minimum;
            this.Height = height;
            this.ConnectionWest = NotConnected;
            this.ConnectionNorth = NotConnected;
            this.ConnectionEast = NotConnected;
            this.ConnectionSouth = NotConnected;
            this.Region = RegionId.Null;
        }

        /// <summary>
        /// Gets the connection data for a neighboring call in a specified direction.
        /// </summary>
        /// <param name="dir">The direction.</param>
        /// <returns>The index of the span in the neighboring cell.</returns>
        public int GetConnection(Direction dir)
        {
            return GetConnection(ref this, dir);
        }
        /// <summary>
        /// Gets a value indicating whether the span is connected to another span in a specified direction.
        /// </summary>
        /// <param name="dir">The direction.</param>
        /// <returns>A value indicating whether the specified direction has a connected span.</returns>
        public bool IsConnected(Direction dir)
        {
            switch (dir)
            {
                case Direction.West:
                    return ConnectionWest != NotConnected;
                case Direction.North:
                    return ConnectionNorth != NotConnected;
                case Direction.East:
                    return ConnectionEast != NotConnected;
                case Direction.South:
                    return ConnectionSouth != NotConnected;
                default:
                    throw new ArgumentException("dir isn't a valid Direction.");
            }
        }
    }

    /// <summary>
    /// A reference to a <see cref="CompactSpan"/> in a <see cref="CompactHeightfield"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CompactSpanReference : IEquatable<CompactSpanReference>
    {
        /// <summary>
        /// A "null" reference is one with a negative index.
        /// </summary>
        public static readonly CompactSpanReference Null = new CompactSpanReference(0, 0, -1);

        /// <summary>
        /// Compares two instances of <see cref="CompactSpanReference"/> for equality.
        /// </summary>
        /// <remarks>
        /// If both references have a negative <see cref="Index"/>, they are considered equal, as both would be considered "null".
        /// </remarks>
        /// <param name="left">A reference.</param>
        /// <param name="right">Another reference.</param>
        /// <returns>A value indicating whether the two references are equal.</returns>
        public static bool operator ==(CompactSpanReference left, CompactSpanReference right)
        {
            //A negative index is considered null.
            //these two cases quickly compare null references.
            bool leftNull = left.Index < 0, rightNull = right.Index < 0;
            if (leftNull && rightNull)
                return true;
            else if (leftNull ^ rightNull)
                return false;

            //if the references are not null, 
            else if (left.X == right.X && left.Y == right.Y && left.Index == right.Index)
                return true;

            return false;
        }
        /// <summary>
        /// Compare two instances of <see cref="CompactSpanReference"/> for inequality.
        /// </summary>
        /// <remarks>
        /// If both references have a negative <see cref="Index"/>, they are considered equal, as both would be considered "null".
        /// </remarks>
        /// <param name="left">A reference.</param>
        /// <param name="right">Another reference.</param>
        /// <returns>A value indicating whether the two references are not equal.</returns>
        public static bool operator !=(CompactSpanReference left, CompactSpanReference right)
        {
            return !(left == right);
        }

        /// <summary>
        /// The X coordinate of the referenced span.
        /// </summary>
        public readonly int X;
        /// <summary>
        /// The Y coordinate of the referenced span.
        /// </summary>
        public readonly int Y;
        /// <summary>
        /// The index of the referenced span in the spans array.
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactSpanReference"/> struct.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="i">The index of the span in the spans array.</param>
        public CompactSpanReference(int x, int y, int i)
        {
            this.X = x;
            this.Y = y;
            this.Index = i;
        }

        /// <summary>
        /// Compares this instance to another instance of <see cref="CompactSpanReference"/> for equality.
        /// </summary>
        /// <param name="other">Another instance of <see cref="CompactSpanReference"/>.</param>
        /// <returns>A value indicating whether this instance and another instance are equal.</returns>
        public bool Equals(CompactSpanReference other)
        {
            return this == other;
        }
        /// <summary>
        /// Compares this instance to another object for equality.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns>A value indicating whether the object is equal to this instance.</returns>
        public override bool Equals(object obj)
        {
            CompactSpanReference? r = obj as CompactSpanReference?;
            if (r.HasValue)
                return this == r.Value;
            return false;
        }
        /// <summary>
        /// Gets a hash code unique to this instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            //TODO should "null" references all have the same hash?
            int hash = 27;
            hash = (13 * hash) + X.GetHashCode();
            hash = (13 * hash) + Y.GetHashCode();
            hash = (13 * hash) + Index.GetHashCode();

            return hash;
        }
    }

    /// <summary>
    /// Flags that can be applied to a region.
    /// </summary>
    [Flags]
    public enum RegionFlags
    {
        /// <summary>
        /// The border flag
        /// </summary>
        Border = 0x20000000,
        /// <summary>
        /// The vertex border flag
        /// </summary>
        VertexBorder = 0x40000000,
        /// <summary>
        /// The area border flag
        /// </summary>
        AreaBorder = unchecked((int)0x80000000)
    }

    /// <summary>
    /// A <see cref="RegionId"/> is an identifier with flags marking borders.
    /// </summary>
    [Serializable]
    public struct RegionId : IEquatable<RegionId>, IEquatable<int>
    {
        /// <summary>
        /// A bitmask 
        /// </summary>
        public const int MaskId = 0x1fffffff;

        /// <summary>
        /// A null region is one with an ID of 0.
        /// </summary>
        public static readonly RegionId Null = new RegionId(0, 0);

        /// <summary>
        /// Creates a new <see cref="RegionId"/> from a value that contains both the region ID and the flags.
        /// </summary>
        /// <param name="bits">The int containing <see cref="RegionId"/> data.</param>
        /// <returns>A new instance of the <see cref="RegionId"/> struct with the specified data.</returns>
        public static RegionId FromRawBits(int bits)
        {
            RegionId id;
            id.bits = bits;
            return id;
        }
        /// <summary>
        /// Creates a new <see cref="RegionId"/> with extra flags.
        /// </summary>
        /// <param name="region">The region to add flags to.</param>
        /// <param name="flags">The flags to add.</param>
        /// <returns>A new instance of the <see cref="RegionId"/> struct with extra flags.</returns>
        public static RegionId WithFlags(RegionId region, RegionFlags flags)
        {
            if ((RegionFlags)((int)flags & ~MaskId) != flags)
                throw new ArgumentException("flags", "The provide region flags are invalid.");

            RegionFlags newFlags = region.Flags | flags;
            return RegionId.FromRawBits((region.bits & MaskId) | (int)newFlags);
        }
        /// <summary>
        /// Creates a new instance of the <see cref="RegionId"/> class without any flags set.
        /// </summary>
        /// <param name="region">The region to use.</param>
        /// <returns>A new instance of the <see cref="RegionId"/> struct without any flags set.</returns>
        public static RegionId WithoutFlags(RegionId region)
        {
            return new RegionId(region.Id);
        }
        /// <summary>
        /// Creates a new instance of the <see cref="RegionId"/> class without certain flags set.
        /// </summary>
        /// <param name="region">The region to use.</param>
        /// <param name="flags">The flags to unset.</param>
        /// <returns>A new instnace of the <see cref="RegionId"/> struct without certain flags set.</returns>
        public static RegionId WithoutFlags(RegionId region, RegionFlags flags)
        {
            if ((RegionFlags)((int)flags & ~MaskId) != flags)
                throw new ArgumentException("flags", "The provide region flags are invalid.");

            RegionFlags newFlags = region.Flags & ~flags;
            return RegionId.FromRawBits((region.bits & MaskId) | (int)newFlags);
        }
        /// <summary>
        /// Checks if a region has certain flags.
        /// </summary>
        /// <param name="region">The region to check.</param>
        /// <param name="flags">The flags to check.</param>
        /// <returns>A value indicating whether the region has all of the specified flags.</returns>
        public static bool HasFlags(RegionId region, RegionFlags flags)
        {
            return (region.Flags & flags) != 0;
        }

        /// <summary>
        /// Compares an instance of <see cref="RegionId"/> with an integer for equality.
        /// </summary>
        /// <remarks>
        /// This checks for both the ID and flags set on the region. If you want to only compare the IDs, use the
        /// following code:
        /// <code>
        /// RegionId left = ...;
        /// int right = ...;
        /// if (left.Id == right)
        /// {
        ///    // ...
        /// }
        /// </code>
        /// </remarks>
        /// <param name="left">An instance of <see cref="RegionId"/>.</param>
        /// <param name="right">An integer.</param>
        /// <returns>A value indicating whether the two values are equal.</returns>
        public static bool operator ==(RegionId left, int right)
        {
            return left.Equals(right);
        }
        /// <summary>
        /// Compares an instance of <see cref="RegionId"/> with an integer for inequality.
        /// </summary>
        /// <remarks>
        /// This checks for both the ID and flags set on the region. If you want to only compare the IDs, use the
        /// following code:
        /// <code>
        /// RegionId left = ...;
        /// int right = ...;
        /// if (left.Id != right)
        /// {
        ///    // ...
        /// }
        /// </code>
        /// </remarks>
        /// <param name="left">An instance of <see cref="RegionId"/>.</param>
        /// <param name="right">An integer.</param>
        /// <returns>A value indicating whether the two values are unequal.</returns>
        public static bool operator !=(RegionId left, int right)
        {
            return !(left == right);
        }
        /// <summary>
        /// Compares two instances of <see cref="RegionId"/> for equality.
        /// </summary>
        /// <remarks>
        /// This checks for both the ID and flags set on the regions. If you want to only compare the IDs, use the
        /// following code:
        /// <code>
        /// RegionId left = ...;
        /// RegionId right = ...;
        /// if (left.Id == right.Id)
        /// {
        ///    // ...
        /// }
        /// </code>
        /// </remarks>
        /// <param name="left">An instance of <see cref="RegionId"/>.</param>
        /// <param name="right">Another instance of <see cref="RegionId"/>.</param>
        /// <returns>A value indicating whether the two instances are equal.</returns>
        public static bool operator ==(RegionId left, RegionId right)
        {
            return left.Equals(right);
        }
        /// <summary>
        /// Compares two instances of <see cref="RegionId"/> for inequality.
        /// </summary>
        /// <remarks>
        /// This checks for both the ID and flags set on the regions. If you want to only compare the IDs, use the
        /// following code:
        /// <code>
        /// RegionId left = ...;
        /// RegionId right = ...;
        /// if (left.Id != right.Id)
        /// {
        ///    // ...
        /// }
        /// </code>
        /// </remarks>
        /// <param name="left">An instance of <see cref="RegionId"/>.</param>
        /// <param name="right">Another instance of <see cref="RegionId"/>.</param>
        /// <returns>A value indicating whether the two instances are unequal.</returns>
        public static bool operator !=(RegionId left, RegionId right)
        {
            return !(left == right);
        }
        /// <summary>
        /// Converts an instance of <see cref="RegionId"/> to an integer containing both the ID and the flags.
        /// </summary>
        /// <param name="id">An instance of <see cref="RegionId"/>.</param>
        /// <returns>An integer.</returns>
        public static explicit operator int(RegionId id)
        {
            return id.bits;
        }

        /// <summary>
        /// The internal storage of a <see cref="RegionId"/>. The <see cref="RegionFlags"/> portion are the most
        /// significant bits, the integer identifier are the least significant bits, marked by <see cref="MaskId"/>.
        /// </summary>
        private int bits;
        /// <summary>
        /// Gets the ID of the region without any flags.
        /// </summary>
        public int Id
        {
            get
            {
                return bits & MaskId;
            }
        }
        /// <summary>
        /// Gets the flags set for this region.
        /// </summary>
        public RegionFlags Flags
        {
            get
            {
                return (RegionFlags)(bits & ~MaskId);
            }
        }
        /// <summary>
        /// Gets a value indicating whether the region is the null region (ID == 0).
        /// </summary>
        public bool IsNull
        {
            get
            {
                return (bits & MaskId) == 0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegionId"/> struct without any flags.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public RegionId(int id)
            : this(id, 0)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RegionId"/> struct.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flags"></param>
        public RegionId(int id, RegionFlags flags)
        {
            int masked = id & MaskId;

            if (masked != id)
                throw new ArgumentOutOfRangeException("id", "The provided id is outside of the valid range. The 3 most significant bits must be 0. Maybe you wanted RegionId.FromRawBits()?");

            if ((RegionFlags)((int)flags & ~MaskId) != flags)
                throw new ArgumentException("flags", "The provide region flags are invalid.");

            bits = masked | (int)flags;
        }

        /// <summary>
        /// Compares this instance with another instance of <see cref="RegionId"/> for equality, including flags.
        /// </summary>
        /// <param name="other">An instance of <see cref="RegionId"/>.</param>
        /// <returns>A value indicating whether the two instances are equal.</returns>
        public bool Equals(RegionId other)
        {
            bool thisNull = this.IsNull;
            bool otherNull = other.IsNull;

            if (thisNull && otherNull)
                return true;
            else if (thisNull ^ otherNull)
                return false;
            else
                return this.bits == other.bits;
        }
        /// <summary>
        /// Compares this instance with another an intenger for equality, including flags.
        /// </summary>
        /// <param name="other">An integer.</param>
        /// <returns>A value indicating whether the two instances are equal.</returns>
        public bool Equals(int other)
        {
            RegionId otherId;
            otherId.bits = other;

            return this.Equals(otherId);
        }
        /// <summary>
        /// Compares this instance with an object for equality.
        /// </summary>
        /// <param name="obj">An object</param>
        /// <returns>A value indicating whether the two instances are equal.</returns>
        public override bool Equals(object obj)
        {
            var regObj = obj as RegionId?;
            var intObj = obj as int?;

            if (regObj.HasValue)
                return this.Equals(regObj.Value);
            else if (intObj.HasValue)
                return this.Equals(intObj.Value);
            else
                return false;
        }
        /// <summary>
        /// Gets a unique hash code for this instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            if (IsNull)
                return 0;

            return bits.GetHashCode();
        }
        /// <summary>
        /// Gets a human-readable version of this instance.
        /// </summary>
        /// <returns>A string representing this instance.</returns>
        public override string ToString()
        {
            return "{ Id: " + Id + ", Flags: " + Flags + "}";
        }
    }

    /// <summary>
    /// A Region contains a group of adjacent spans.
    /// </summary>
    public class Region
    {
        /// <summary>
        /// Gets or sets the number of spans
        /// </summary>
        public int SpanCount { get; set; }
        /// <summary>
        /// Gets or sets the region id 
        /// </summary>
        public RegionId Id { get; set; }
        /// <summary>
        /// Gets or sets the AreaType of this region
        /// </summary>
        public Area AreaType { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this region has been remapped or not
        /// </summary>
        public bool Remap { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this region has been visited or not
        /// </summary>
        public bool Visited { get; set; }
        /// <summary>
        /// Gets the list of floor regions
        /// </summary>
        public List<RegionId> FloorRegions { get; private set; }
        /// <summary>
        /// Gets the list of connected regions
        /// </summary>
        public List<RegionId> Connections { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the region is a border region.
        /// </summary>
        public bool IsBorder
        {
            get
            {
                return RegionId.HasFlags(Id, RegionFlags.Border);
            }
        }
        /// <summary>
        /// Gets a value indicating whether the region is either a border region or the null region.
        /// </summary>
        public bool IsBorderOrNull
        {
            get
            {
                return Id.IsNull || IsBorder;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Region" /> class.
        /// </summary>
        /// <param name="idNum">The id</param>
        public Region(int idNum)
        {
            SpanCount = 0;
            Id = new RegionId(idNum);
            AreaType = 0;
            Remap = false;
            Visited = false;

            Connections = new List<RegionId>();
            FloorRegions = new List<RegionId>();
        }

        /// <summary>
        /// Remove adjacent connections if there is a duplicate
        /// </summary>
        public void RemoveAdjacentNeighbors()
        {
            if (Connections.Count <= 1)
                return;

            // Remove adjacent duplicates.
            for (int i = 0; i < Connections.Count; i++)
            {
                //get the next i
                int ni = (i + 1) % Connections.Count;

                //remove duplicate if found
                if (Connections[i] == Connections[ni])
                {
                    Connections.RemoveAt(i);
                    i--;
                }
            }
        }
        /// <summary>
        /// Replace all connection and floor values 
        /// </summary>
        /// <param name="oldId">The value you want to replace</param>
        /// <param name="newId">The new value that will be used</param>
        public void ReplaceNeighbor(RegionId oldId, RegionId newId)
        {
            //replace the connections
            bool neiChanged = false;
            for (int i = 0; i < Connections.Count; ++i)
            {
                if (Connections[i] == oldId)
                {
                    Connections[i] = newId;
                    neiChanged = true;
                }
            }

            //replace the floors
            for (int i = 0; i < FloorRegions.Count; ++i)
            {
                if (FloorRegions[i] == oldId)
                    FloorRegions[i] = newId;
            }

            //make sure to remove adjacent neighbors
            if (neiChanged)
                RemoveAdjacentNeighbors();
        }
        /// <summary>
        /// Determine whether this region can merge with another region.
        /// </summary>
        /// <param name="otherRegion">The other region to merge with</param>
        /// <returns>True if the two regions can be merged, false if otherwise</returns>
        public bool CanMergeWith(Region otherRegion)
        {
            //make sure areas are the same
            if (AreaType != otherRegion.AreaType)
                return false;

            //count the number of connections to the other region
            int n = 0;
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i] == otherRegion.Id)
                    n++;
            }

            //make sure there's only one connection
            if (n > 1)
                return false;

            //make sure floors are separate
            if (FloorRegions.Contains(otherRegion.Id))
                return false;

            return true;
        }
        /// <summary>
        /// Only add a floor if it hasn't been added already
        /// </summary>
        /// <param name="n">The value of the floor</param>
        public void AddUniqueFloorRegion(RegionId n)
        {
            if (!FloorRegions.Contains(n))
                FloorRegions.Add(n);
        }
        /// <summary>
        /// Merge two regions into one. Needs good testing
        /// </summary>
        /// <param name="otherRegion">The region to merge with</param>
        /// <returns>True if merged successfully, false if otherwise</returns>
        public bool MergeWithRegion(Region otherRegion)
        {
            RegionId thisId = Id;
            RegionId otherId = otherRegion.Id;

            // Duplicate current neighborhood.
            List<RegionId> thisConnected = new List<RegionId>();
            for (int i = 0; i < Connections.Count; ++i)
                thisConnected.Add(Connections[i]);
            List<RegionId> otherConnected = otherRegion.Connections;

            // Find insertion point on this region
            int insertInThis = -1;
            for (int i = 0; i < thisConnected.Count; ++i)
            {
                if (thisConnected[i] == otherId)
                {
                    insertInThis = i;
                    break;
                }
            }

            if (insertInThis == -1)
                return false;

            // Find insertion point on the other region
            int insertInOther = -1;
            for (int i = 0; i < otherConnected.Count; ++i)
            {
                if (otherConnected[i] == thisId)
                {
                    insertInOther = i;
                    break;
                }
            }

            if (insertInOther == -1)
                return false;

            // Merge neighbors.
            Connections = new List<RegionId>();
            for (int i = 0, ni = thisConnected.Count; i < ni - 1; ++i)
                Connections.Add(thisConnected[(insertInThis + 1 + i) % ni]);

            for (int i = 0, ni = otherConnected.Count; i < ni - 1; ++i)
                Connections.Add(otherConnected[(insertInOther + 1 + i) % ni]);

            RemoveAdjacentNeighbors();

            for (int j = 0; j < otherRegion.FloorRegions.Count; ++j)
                AddUniqueFloorRegion(otherRegion.FloorRegions[j]);
            SpanCount += otherRegion.SpanCount;
            otherRegion.SpanCount = 0;
            otherRegion.Connections.Clear();

            return true;
        }
        /// <summary>
        /// Test if region is connected to a border
        /// </summary>
        /// <returns>True if connected, false if not</returns>
        public bool IsConnectedToBorder()
        {
            // Region is connected to border if
            // one of the neighbors is null id.
            for (int i = 0; i < Connections.Count; ++i)
            {
                if (Connections[i] == 0)
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// An enum similar to <see cref="Direction"/>, but with the ability to store multiple directions.
    /// </summary>
    [Flags]
    public enum EdgeFlags : byte
    {
        /// <summary>No edges are selected.</summary>
        None = 0x0,
        /// <summary>The west edge is selected.</summary>
        West = 0x1,
        /// <summary>The north edge is selected.</summary>
        North = 0x2,
        /// <summary>The east edge is selected.</summary>
        East = 0x4,
        /// <summary>The south edge is selected.</summary>
        South = 0x8,
        /// <summary>All of the edges are selected.</summary>
        All = West | North | East | South
    }

    /// <summary>
    /// A static class with helper functions to modify instances of the <see cref="EdgeFlags"/> enum.
    /// </summary>
    public static class EdgeFlagsHelper
    {
        /// <summary>
        /// Adds an edge in a specified direction to an instance of <see cref="EdgeFlags"/>.
        /// </summary>
        /// <param name="edges">An existing set of edges.</param>
        /// <param name="dir">The direction to add.</param>
        public static void AddEdge(ref EdgeFlags edges, Direction dir)
        {
            edges |= (EdgeFlags)(1 << (int)dir);
        }
        /// <summary>
        /// Flips the set of edges in an instance of <see cref="EdgeFlags"/>.
        /// </summary>
        /// <param name="edges">An existing set of edges.</param>
        public static void FlipEdges(ref EdgeFlags edges)
        {
            edges ^= EdgeFlags.All;
        }
        /// <summary>
        /// Determines whether an instance of <see cref="EdgeFlags"/> includes an edge in a specified direction.
        /// </summary>
        /// <param name="edges">A set of edges.</param>
        /// <param name="dir">The direction to check for an edge.</param>
        /// <returns>A value indicating whether the set of edges contains an edge in the specified direction.</returns>
        public static bool IsConnected(ref EdgeFlags edges, Direction dir)
        {
            return (edges & (EdgeFlags)(1 << (int)dir)) != EdgeFlags.None;
        }
        /// <summary>
        /// Removes an edge from an instance of <see cref="EdgeFlags"/>.
        /// </summary>
        /// <param name="edges">A set of edges.</param>
        /// <param name="dir">The direction to remove.</param>
        public static void RemoveEdge(ref EdgeFlags edges, Direction dir)
        {
            edges &= (EdgeFlags)(~(1 << (int)dir));
        }
    }
}
