using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// A more memory-compact heightfield that stores open spans of voxels instead of closed ones.
    /// </summary>
    public class CompactHeightField
    {
        /// <summary>
        /// Gets the width of the <see cref="CompactHeightField"/> in voxel units.
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Gets the height of the <see cref="CompactHeightField"/> in voxel units.
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Gets the length of the <see cref="CompactHeightField"/> in voxel units.
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
        public CompactHeightFieldCell[] Cells { get; private set; }
        /// <summary>
        /// Gets the spans.
        /// </summary>
        public CompactHeightFieldSpan[] Spans { get; private set; }
        /// <summary>
        /// Gets the area flags.
        /// </summary>
        public Area[] Areas { get; private set; }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of <see cref="CompactHeightFieldSpan"/> of the spans at a specified coordiante.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="CompactHeightFieldSpan"/>.</returns>
        public IEnumerable<CompactHeightFieldSpan> this[int x, int y]
        {
            get
            {
                CompactHeightFieldCell c = this.Cells[y * this.Width + x];

                int end = c.StartIndex + c.Count;
                for (int i = c.StartIndex; i < end; i++)
                {
                    yield return this.Spans[i];
                }
            }
        }
        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of <see cref="CompactHeightFieldSpan"/>s at a specified index.
        /// </summary>
        /// <param name="i">The index.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="CompactHeightFieldSpan"/>.</returns>
        public IEnumerable<CompactHeightFieldSpan> this[int i]
        {
            get
            {
                CompactHeightFieldCell c = this.Cells[i];

                int end = c.StartIndex + c.Count;
                for (int j = c.StartIndex; j < end; j++)
                {
                    yield return this.Spans[j];
                }
            }
        }
        /// <summary>
        /// Gets the <see cref="CompactHeightFieldSpan"/> specified by the reference.
        /// </summary>
        /// <param name="spanRef">A reference to a span in this <see cref="CompactHeightField"/>.</param>
        /// <returns>The referenced span.</returns>
        public CompactHeightFieldSpan this[CompactHeightFieldSpanReference spanRef]
        {
            get
            {
                return this.Spans[spanRef.Index];
            }
        }

        /// <summary>
        /// Builds a new compact height field from a height field
        /// </summary>
        /// <param name="heightField">Height field</param>
        /// <param name="settings">Generation settings</param>
        /// <returns>Returns the new generated compact height field</returns>
        public static CompactHeightField Build(HeightField heightField, NavigationMeshGenerationSettings settings)
        {
            var ch = new CompactHeightField(heightField, settings.VoxelAgentHeight, settings.VoxelMaxClimb);
            ch.Erode(settings.VoxelAgentRadius);
            ch.BuildDistanceField();
            ch.BuildRegions(0, settings.MinRegionSize, settings.MergedRegionSize);

            return ch;
        }
        /// <summary>
        /// Merge two stacks to get a single stack.
        /// </summary>
        /// <param name="source">The original stack</param>
        /// <param name="destination">The new stack</param>
        /// <param name="regions">Region ids</param>
        private static void AppendStacks(List<CompactHeightFieldSpanReference> source, List<CompactHeightFieldSpanReference> destination, RegionId[] regions)
        {
            for (int j = 0; j < source.Count; j++)
            {
                var spanRef = source[j];
                if (spanRef.Index < 0 || regions[spanRef.Index] != 0)
                {
                    continue;
                }

                destination.Add(spanRef);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactHeightField"/> class.
        /// </summary>
        /// <param name="field">A <see cref="HeightField"/> to build from.</param>
        /// <param name="walkableHeight">The maximum difference in height to filter.</param>
        /// <param name="walkableClimb">The maximum difference in slope to filter.</param>
        public CompactHeightField(HeightField field, int walkableHeight, int walkableClimb)
        {
            this.Bounds = field.Bounds;
            this.Width = field.Width;
            this.Height = field.Height;
            this.Length = field.Length;
            this.CellSize = field.CellSizeXZ;
            this.CellHeight = field.CellHeight;

            int spanCount = field.SpanCount;
            this.Cells = new CompactHeightFieldCell[this.Width * this.Length];
            this.Spans = new CompactHeightFieldSpan[spanCount];
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

                CompactHeightFieldCell c = new CompactHeightFieldCell(spanIndex, 0);

                //convert the closed spans to open spans
                int lastInd = fs.Count - 1;
                for (int j = 0; j < lastInd; j++)
                {
                    var s = fs[j];
                    if (s.Area.IsWalkable)
                    {
                        CompactHeightFieldSpan res;
                        CompactHeightFieldSpan.FromMinMax(s.Maximum, fs[j + 1].Minimum, out res);
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
                    this.Spans[spanIndex] = new CompactHeightFieldSpan(fs[lastInd].Maximum, int.MaxValue);
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
                    CompactHeightFieldCell c = this.Cells[z * this.Width + x];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        CompactHeightFieldSpan s = this.Spans[i];

                        for (var dir = Direction.West; dir <= Direction.South; dir++)
                        {
                            int dx = x + dir.GetHorizontalOffset();
                            int dz = z + dir.GetVerticalOffset();

                            if (dx < 0 || dz < 0 || dx >= this.Width || dz >= this.Length)
                            {
                                continue;
                            }

                            CompactHeightFieldCell dc = this.Cells[dz * this.Width + dx];
                            for (int j = dc.StartIndex, cellEnd = dc.StartIndex + dc.Count; j < cellEnd; j++)
                            {
                                CompactHeightFieldSpan ds = this.Spans[j];

                                int overlapBottom, overlapTop;
                                CompactHeightFieldSpan.OverlapMin(ref s, ref ds, out overlapBottom);
                                CompactHeightFieldSpan.OverlapMax(ref s, ref ds, out overlapTop);

                                //Make sure that the agent can walk to the next span and that the span isn't a huge drop or climb
                                if ((overlapTop - overlapBottom) >= walkableHeight && Math.Abs(ds.Minimum - s.Minimum) <= walkableClimb)
                                {
                                    int con = j - dc.StartIndex;
                                    CompactHeightFieldSpan.SetConnection(dir, con, ref this.Spans[i]);
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
            List<CompactHeightFieldSpanReference>[] stacks = new List<CompactHeightFieldSpanReference>[StackCount];
            for (int i = 0; i < stacks.Length; i++)
            {
                stacks[i] = new List<CompactHeightFieldSpanReference>(1024);
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
                    CompactHeightFieldCell c = this.Cells[x + z * this.Width];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        CompactHeightFieldSpan s = this.Spans[i];

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
                                int di = this.Cells[dx + dz * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref s, dir);
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

            var verts = new List<ContourVertexi>();
            var simplified = new List<ContourVertexi>();

            for (int z = 0; z < this.Length; z++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    CompactHeightFieldCell c = this.Cells[x + z * this.Width];
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

                        var spanRef = new CompactHeightFieldSpanReference(x, z, i);
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
                    CompactHeightFieldCell c = this.Cells[x + y * this.Width];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        CompactHeightFieldSpanReference spanRef = new CompactHeightFieldSpanReference(x, y, i);

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
                    CompactHeightFieldCell c = this.Cells[y * this.Width + x];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        CompactHeightFieldSpan s = this.Spans[i];
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
                                int di = this.Cells[dx + dy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref s, dir);
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
                    CompactHeightFieldCell c = this.Cells[y * this.Width + x];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        CompactHeightFieldSpan s = this.Spans[i];

                        if (s.IsConnected(Direction.West))
                        {
                            //(-1, 0)
                            int dx = x + Direction.West.GetHorizontalOffset();
                            int dy = y + Direction.West.GetVerticalOffset();
                            int di = this.Cells[dx + dy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref s, Direction.West);
                            CompactHeightFieldSpan ds = this.Spans[di];
                            if (src[di] + 2 < src[i])
                            {
                                src[i] = src[di] + 2;
                            }

                            //(-1, -1)
                            if (ds.IsConnected(Direction.South))
                            {
                                int ddx = dx + Direction.South.GetHorizontalOffset();
                                int ddy = dy + Direction.South.GetVerticalOffset();
                                int ddi = this.Cells[ddx + ddy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref ds, Direction.South);
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
                            int di = this.Cells[dx + dy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref s, Direction.South);
                            CompactHeightFieldSpan ds = this.Spans[di];
                            if (src[di] + 2 < src[i])
                            {
                                src[i] = src[di] + 2;
                            }

                            //(1, -1)
                            if (ds.IsConnected(Direction.East))
                            {
                                int ddx = dx + Direction.East.GetHorizontalOffset();
                                int ddy = dy + Direction.East.GetVerticalOffset();
                                int ddi = this.Cells[ddx + ddy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref ds, Direction.East);
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
                    CompactHeightFieldCell c = this.Cells[y * this.Width + x];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        CompactHeightFieldSpan s = this.Spans[i];

                        if (s.IsConnected(Direction.East))
                        {
                            //(1, 0)
                            int dx = x + Direction.East.GetHorizontalOffset();
                            int dy = y + Direction.East.GetVerticalOffset();
                            int di = this.Cells[dx + dy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref s, Direction.East);
                            CompactHeightFieldSpan ds = this.Spans[di];
                            if (src[di] + 2 < src[i])
                            {
                                src[i] = src[di] + 2;
                            }

                            //(1, 1)
                            if (ds.IsConnected(Direction.North))
                            {
                                int ddx = dx + Direction.North.GetHorizontalOffset();
                                int ddy = dy + Direction.North.GetVerticalOffset();
                                int ddi = this.Cells[ddx + ddy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref ds, Direction.North);
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
                            int di = this.Cells[dx + dy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref s, Direction.North);
                            CompactHeightFieldSpan ds = this.Spans[di];
                            if (src[di] + 2 < src[i])
                            {
                                src[i] = src[di] + 2;
                            }

                            //(-1, 1)
                            if (ds.IsConnected(Direction.West))
                            {
                                int ddx = dx + Direction.West.GetHorizontalOffset();
                                int ddy = dy + Direction.West.GetVerticalOffset();
                                int ddi = this.Cells[ddx + ddy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref ds, Direction.West);
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
                    CompactHeightFieldCell c = this.Cells[y * this.Width + x];
                    for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                    {
                        CompactHeightFieldSpan s = this.Spans[i];
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
                                int di = this.Cells[dy * this.Width + dx].StartIndex + CompactHeightFieldSpan.GetConnection(ref s, dir);

                                d += buffer[di];

                                CompactHeightFieldSpan ds = this.Spans[di];
                                Direction dir2 = dir.NextClockwise();
                                if (ds.IsConnected(dir2))
                                {
                                    int dx2 = dx + dir2.GetHorizontalOffset();
                                    int dy2 = dy + dir2.GetVerticalOffset();
                                    int di2 = this.Cells[dy2 * this.Width + dx2].StartIndex + CompactHeightFieldSpan.GetConnection(ref ds, dir2);

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
        private void ExpandRegions(RegionId[] regions, int[] floodDistances, int maxIterations, int level, List<CompactHeightFieldSpanReference> stack = null, RegionId[] regionBuffer = null, int[] distanceBuffer = null)
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
                stack = new List<CompactHeightFieldSpanReference>();
                for (int y = 0; y < this.Length; y++)
                {
                    for (int x = 0; x < this.Width; x++)
                    {
                        CompactHeightFieldCell c = this.Cells[x + y * this.Width];
                        for (int i = c.StartIndex, end = c.StartIndex + c.Count; i < end; i++)
                        {
                            //a cell is being expanded to if it's distance is greater than the current level,
                            //but no region has been asigned yet. It must also not be in a null area.
                            if (this.Distances[i] >= level && regions[i] == 0 && this.Areas[i].IsWalkable)
                            {
                                stack.Add(new CompactHeightFieldSpanReference(x, y, i));
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
                        stack[j] = CompactHeightFieldSpanReference.Null;
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
                    CompactHeightFieldSpanReference spanRef = stack[j];
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
                    CompactHeightFieldSpan s = this.Spans[i];

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
                        int di = this.Cells[dx + dy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref s, dir);

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
                        stack[j] = CompactHeightFieldSpanReference.Null;
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
        private bool FloodRegion(RegionId[] regions, int[] floodDistances, int regionIndex, int level, ref CompactHeightFieldSpanReference start)
        {
            //TODO this method should always return true, make it not return a bool?
            //flood fill mark region
            Stack<CompactHeightFieldSpanReference> stack = new Stack<CompactHeightFieldSpanReference>();
            stack.Push(start);

            Area area = this.Areas[start.Index];
            regions[start.Index] = new RegionId(regionIndex);
            floodDistances[start.Index] = 0;

            int lev = level >= 2 ? level - 2 : 0;
            int count = 0;

            while (stack.Count > 0)
            {
                CompactHeightFieldSpanReference cell = stack.Pop();
                CompactHeightFieldSpan cs = this.Spans[cell.Index];

                //check if any of the neighbors already have a valid reigon set
                RegionId ar = RegionId.Null;
                for (var dir = Direction.West; dir <= Direction.South; dir++)
                {
                    //8 connected
                    if (cs.IsConnected(dir))
                    {
                        int dx = cell.X + dir.GetHorizontalOffset();
                        int dy = cell.Y + dir.GetVerticalOffset();
                        int di = this.Cells[dx + dy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref cs, dir);

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

                        CompactHeightFieldSpan ds = this.Spans[di];
                        Direction dir2 = dir.NextClockwise();
                        if (ds.IsConnected(dir2))
                        {
                            int dx2 = dx + dir2.GetHorizontalOffset();
                            int dy2 = dy + dir2.GetVerticalOffset();
                            int di2 = this.Cells[dx2 + dy2 * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref ds, dir2);

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
                        int di = this.Cells[dx + dy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref cs, dir);

                        if (this.Areas[di] != area)
                        {
                            continue;
                        }

                        if (this.Distances[di] >= lev && regions[di] == 0)
                        {
                            regions[di] = new RegionId(regionIndex);
                            floodDistances[di] = 0;
                            stack.Push(new CompactHeightFieldSpanReference(dx, dy, di));
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
        private bool IsSolidEdge(RegionId[] regions, ref CompactHeightFieldSpanReference spanRef, Direction dir)
        {
            CompactHeightFieldSpan s = this.Spans[spanRef.Index];
            RegionId r = RegionId.Null;

            if (s.IsConnected(dir))
            {
                int dx = spanRef.X + dir.GetHorizontalOffset();
                int dy = spanRef.Y + dir.GetVerticalOffset();
                int di = this.Cells[dx + dy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref s, dir);
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
        private void WalkContour(RegionId[] regions, CompactHeightFieldSpanReference spanRef, Direction dir, List<RegionId> cont)
        {
            Direction startDir = dir;
            int starti = spanRef.Index;

            CompactHeightFieldSpan ss = this.Spans[starti];
            RegionId curReg = RegionId.Null;

            if (ss.IsConnected(dir))
            {
                int dx = spanRef.X + dir.GetHorizontalOffset();
                int dy = spanRef.Y + dir.GetVerticalOffset();
                int di = this.Cells[dx + dy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref ss, dir);
                curReg = regions[di];
            }

            cont.Add(curReg);

            int iter = 0;
            while (++iter < 40000)
            {
                CompactHeightFieldSpan s = this.Spans[spanRef.Index];

                if (IsSolidEdge(regions, ref spanRef, dir))
                {
                    //choose the edge corner
                    RegionId r = RegionId.Null;
                    if (s.IsConnected(dir))
                    {
                        int dx = spanRef.X + dir.GetHorizontalOffset();
                        int dy = spanRef.Y + dir.GetVerticalOffset();
                        int di = this.Cells[dx + dy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref s, dir);
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
                        CompactHeightFieldCell dc = this.Cells[dx + dy * this.Width];
                        di = dc.StartIndex + CompactHeightFieldSpan.GetConnection(ref s, dir);
                    }

                    if (di == -1)
                    {
                        //shouldn't happen
                        return;
                    }

                    spanRef = new CompactHeightFieldSpanReference(dx, dy, di);
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
                    CompactHeightFieldCell c = this.Cells[x + y * this.Width];
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
        private void SortCellsByLevel(RegionId[] regions, List<CompactHeightFieldSpanReference>[] stacks, int startlevel, int numStacks, int logLevelsPerStack)
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
                    CompactHeightFieldCell c = this.Cells[y * this.Width + x];
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

                        stacks[sId].Add(new CompactHeightFieldSpanReference(x, y, i));
                    }
                }
            }
        }
        /// <summary>
        /// Initial generation of the contours
        /// </summary>
        /// <param name="spanReference">A referecne to the span to start walking from.</param>
        /// <param name="flags">An array of flags determinining </param>
        /// <param name="points">The vertices of a contour.</param>
        private void WalkContour(CompactHeightFieldSpanReference spanReference, EdgeFlags[] flags, List<ContourVertexi> points)
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
                    CompactHeightFieldSpan s = this[spanReference];
                    if (s.IsConnected(dir))
                    {
                        int dx = spanReference.X + dir.GetHorizontalOffset();
                        int dy = spanReference.Y + dir.GetVerticalOffset();
                        int di = this.Cells[dx + dy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref s, dir);
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
                    points.Add(new ContourVertexi(px, py, pz, r));

                    EdgeFlagsHelper.RemoveEdge(ref flags[spanReference.Index], dir);	// remove visited edges
                    dir = dir.NextClockwise();			// rotate clockwise
                }
                else
                {
                    //get a new cell(x, y) and span index(i)
                    int di = -1;
                    int dx = spanReference.X + dir.GetHorizontalOffset();
                    int dy = spanReference.Y + dir.GetVerticalOffset();

                    CompactHeightFieldSpan s = this[spanReference];
                    if (s.IsConnected(dir))
                    {
                        CompactHeightFieldCell dc = this.Cells[dx + dy * this.Width];
                        di = dc.StartIndex + CompactHeightFieldSpan.GetConnection(ref s, dir);
                    }

                    if (di == -1)
                    {
                        // shouldn't happen
                        // TODO if this shouldn't happen, this check shouldn't be necessary.
                        throw new InvalidOperationException("Something went wrong");
                    }

                    spanReference = new CompactHeightFieldSpanReference(dx, dy, di);
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
        private int GetCornerHeight(CompactHeightFieldSpanReference sr, Direction dir, out bool isBorderVertex)
        {
            isBorderVertex = false;

            CompactHeightFieldSpan s = this[sr];
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
                int di = this.Cells[dx + dy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref s, dir);
                CompactHeightFieldSpan ds = this.Spans[di];

                cornerHeight = Math.Max(cornerHeight, ds.Minimum);
                cornerRegs[1] = this.Spans[di].Region;
                cornerAreas[1] = this.Areas[di];

                //get neighbor of neighbor's span
                if (ds.IsConnected(dirp))
                {
                    int dx2 = dx + dirp.GetHorizontalOffset();
                    int dy2 = dy + dirp.GetVerticalOffset();
                    int di2 = this.Cells[dx2 + dy2 * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref ds, dirp);
                    CompactHeightFieldSpan ds2 = this.Spans[di2];

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
                int di = this.Cells[dx + dy * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref s, dirp);
                CompactHeightFieldSpan ds = this.Spans[di];

                cornerHeight = Math.Max(cornerHeight, ds.Minimum);
                cornerRegs[3] = ds.Region;
                cornerAreas[3] = this.Areas[di];

                //get neighbor of neighbor's span
                if (ds.IsConnected(dir))
                {
                    int dx2 = dx + dir.GetHorizontalOffset();
                    int dy2 = dy + dir.GetVerticalOffset();
                    int di2 = this.Cells[dx2 + dy2 * this.Width].StartIndex + CompactHeightFieldSpan.GetConnection(ref ds, dir);
                    CompactHeightFieldSpan ds2 = this.Spans[di2];

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

        #region Helper classes

        /// <summary>
        /// An enum similar to <see cref="Direction"/>, but with the ability to store multiple directions.
        /// </summary>
        [Flags]
        private enum EdgeFlags : byte
        {
            /// <summary>
            /// No edges are selected.
            /// </summary>
            None = 0x0,
            /// <summary>
            /// The west edge is selected.
            /// </summary>
            West = 0x1,
            /// <summary>
            /// The north edge is selected.
            /// </summary>
            North = 0x2,
            /// <summary>
            /// The east edge is selected.
            /// </summary>
            East = 0x4,
            /// <summary>
            /// The south edge is selected.
            /// </summary>
            South = 0x8,
            /// <summary>
            /// All of the edges are selected.
            /// </summary>
            All = West | North | East | South
        }

        /// <summary>
        /// A static class with helper functions to modify instances of the <see cref="EdgeFlags"/> enum.
        /// </summary>
        private static class EdgeFlagsHelper
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

        #endregion
    }
}
