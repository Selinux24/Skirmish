using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Compact heightfield
    /// </summary>
    class CompactHeightfield
    {
        /// <summary>
        /// Maximum span width
        /// </summary>
        const int SPAN_MAX_WIDTH = 0xffff;
        /// <summary>
        /// Maximum span height
        /// </summary>
        const int SPAN_MAX_HEIGHT = 0xff;
        /// <summary>
        /// Maximum vertex per edge
        /// </summary>
        const int MAX_VERTS_PER_EDGE = 32;
        /// <summary>
        /// Maximum number of vertices
        /// </summary>
        const int MAX_VERTS = 127;
        /// <summary>
        /// Heighfield border flag.
        /// If a heightfield region ID has this bit set, then the region is a border 
        /// region and its spans are considered unwalkable.
        /// (Used during the region and contour build process.)
        /// </summary>
        const int RC_BORDER_REG = 0x8000;

        /// <summary>
        /// Sample vertex
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        struct SampleVertex(int x, int y, int z, bool added)
        {
            /// <summary>
            /// X position index
            /// </summary>
            public int X { get; set; } = x;
            /// <summary>
            /// Y position index
            /// </summary>
            public int Y { get; set; } = y;
            /// <summary>
            /// Z position index
            /// </summary>
            public int Z { get; set; } = z;
            /// <summary>
            /// Sample added
            /// </summary>
            public bool Added { get; set; } = added;

            /// <inheritdoc/>
            public readonly override string ToString()
            {
                return $"X: {X}; Y: {Y}; Z: {Z}; Added: {Added};";
            }
        }

        /// <summary>
        /// Direction values
        /// </summary>
        private readonly int[] dirs = [0, 1, 2, 3];
        /// <summary>
        /// Dirty entries list
        /// </summary>
        private readonly List<RecastRegionDirtyEntry> dirtyEntries = [];

        /// <summary>
        /// The width of the heightfield. (Along the x-axis in cell units.)
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// The height of the heightfield. (Along the z-axis in cell units.)
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// The number of spans in the heightfield.
        /// </summary>
        public int SpanCount { get; set; }
        /// <summary>
        /// The walkable height used during the build of the field.  (See: rcConfig::walkableHeight)
        /// </summary>
        public int WalkableHeight { get; set; }
        /// <summary>
        /// The walkable climb used during the build of the field. (See: rcConfig::walkableClimb)
        /// </summary>
        public int WalkableClimb { get; set; }
        /// <summary>
        /// The AABB border size used during the build of the field. (See: rcConfig::borderSize)
        /// </summary>
        public int BorderSize { get; set; }
        /// <summary>
        /// The maximum distance value of any span within the field.         
        /// </summary>
        public int MaxDistance { get; set; }
        /// <summary>
        /// The maximum region id of any span within the field. 
        /// </summary>
        public int MaxRegions { get; set; }
        /// <summary>
        /// The minimum bounds in world space. [(x, y, z)]
        /// </summary>
        public BoundingBox BoundingBox { get; set; }
        /// <summary>
        /// The size of each cell. (On the xz-plane.)
        /// </summary>
        public float CellSize { get; set; }
        /// <summary>
        /// The height of each cell. (The minimum increment along the y-axis.)
        /// </summary>
        public float CellHeight { get; set; }
        /// <summary>
        /// Array of cells. [Size: width*height] 
        /// </summary>
        public CompactCell[] Cells { get; set; }
        /// <summary>
        /// Array of spans. [Size: spanCount]
        /// </summary>
        public CompactSpan[] Spans { get; set; }
        /// <summary>
        /// Array containing border distance data. [Size: spanCount]      
        /// </summary>
        public int[] BorderDistances { get; set; }
        /// <summary>
        /// Array containing area id data. [Size: spanCount] 
        /// </summary>
        public AreaTypes[] Areas { get; set; }

        /// <summary>
        /// Enumerates the specified compact cell list
        /// </summary>
        /// <returns>Returns each compact cell to evaulate</returns>
        private IEnumerable<(int Column, int Row, int SpanIndex, CompactCell Cell)> IterateCells()
        {
            for (int row = 0; row < Height; ++row)
            {
                for (int col = 0; col < Width; ++col)
                {
                    var cell = Cells[col + row * Width];

                    for (int spanIndex = cell.Index, neiIndex = cell.Index + cell.Count; spanIndex < neiIndex; ++spanIndex)
                    {
                        yield return (col, row, spanIndex, cell);
                    }
                }
            }
        }
        /// <summary>
        /// Enumerates the specified compact cell list in reverse order
        /// </summary>
        /// <returns>Returns each compact cell to evaulate</returns>
        private IEnumerable<(int Column, int Row, int SpanIndex, CompactCell Cell)> IterateCellsReverse()
        {
            for (int row = Height - 1; row >= 0; --row)
            {
                for (int col = Width - 1; col >= 0; --col)
                {
                    var cell = Cells[col + row * Width];

                    for (int spanIndex = cell.Index, neiIndex = cell.Index + cell.Count; spanIndex < neiIndex; ++spanIndex)
                    {
                        yield return (col, row, spanIndex, cell);
                    }
                }
            }
        }
        /// <summary>
        /// Enumerates the specified compact cell list, if is contained into the bounds
        /// </summary>
        /// <param name="minColumn">Minimum x bound's coordinate</param>
        /// <param name="minRow">Minimum y bound's coordinate</param>
        /// <param name="maxColumn">Maximum x bound's coordinate</param>
        /// <param name="maxRow">Maximum y bound's coordinate</param>
        /// <returns>Returns each column, row and span index to evaluate</returns>
        private IEnumerable<(int Column, int Row, int SpanIndex)> IterateCellsSpans(int minColumn, int minRow, int maxColumn, int maxRow)
        {
            for (int row = minRow; row < maxRow; ++row)
            {
                for (int col = minColumn; col < maxColumn; ++col)
                {
                    var c = Cells[col + row * Width];

                    for (int i = c.Index, ni = c.Index + c.Count; i < ni; ++i)
                    {
                        yield return (col, row, i);
                    }
                }
            }
        }
        /// <summary>
        /// Enumerates the specified compact cell list, if is contained into the bounds
        /// </summary>
        /// <param name="minColumn">Minimum x bound's coordinate</param>
        /// <param name="minRow">Minimum y bound's coordinate</param>
        /// <param name="maxColumn">Maximum x bound's coordinate</param>
        /// <param name="maxRow">Maximum y bound's coordinate</param>
        /// <returns>Returns each span index and span center to evaluate</returns>
        private IEnumerable<(int SpanIndex, Vector3 SpanCenter)> IterateCellsSpansAreas(int minColumn, int minRow, int maxColumn, int maxRow)
        {
            foreach (var (col, row, i) in IterateCellsSpans(minColumn, minRow, maxColumn, maxRow))
            {
                if (Areas[i] == AreaTypes.RC_NULL_AREA)
                {
                    continue;
                }

                var sy = Spans[i].Y;
                if (sy < minRow || sy > maxRow)
                {
                    continue;
                }

                var center = new Vector3
                {
                    X = BoundingBox.Minimum.X + (col + 0.5f) * CellSize,
                    Y = sy,
                    Z = BoundingBox.Minimum.Z + (row + 0.5f) * CellSize
                };

                yield return (i, center);
            }
        }
        /// <summary>
        /// Iterates the spans of the cell at coordinates
        /// </summary>
        /// <param name="col">X coordinate</param>
        /// <param name="row">Y coordinate</param>
        /// <returns>Returns the span and the span index</returns>
        public IEnumerable<(CompactSpan Span, int CellIndex)> IterateCellSpans(int col, int row)
        {
            var c = Cells[col + row * Width];

            for (int i = c.Index, ni = c.Index + c.Count; i < ni; ++i)
            {
                yield return (Spans[i], i);
            }
        }
        /// <summary>
        /// Iterates over row cell spans
        /// </summary>
        /// <param name="row">Row index</param>
        /// <returns>Returns the span, the span index and the column index</returns>
        public IEnumerable<(CompactSpan Span, int SpanIndex, int Column)> IterateRowSpans(int row)
        {
            for (int col = BorderSize; col < Width - BorderSize; ++col)
            {
                var c = Cells[col + row * Width];

                for (int i = c.Index, ni = c.Index + c.Count; i < ni; ++i)
                {
                    yield return (Spans[i], i, col);
                }
            }
        }
        /// <summary>
        /// Iterates over the specified span connections
        /// </summary>
        /// <param name="cs">Compact span</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        public IEnumerable<(int dir, int ax, int ay, int ai, AreaTypes area, CompactSpan s)> IterateSpanConnections(CompactSpan cs, int x, int y)
        {
            for (int dir = 0; dir < 4; ++dir)
            {
                if (!cs.GetCon(dir, out int d))
                {
                    continue;
                }

                int ai = GetNeighbourCellIndex(x, y, dir, d, out int ax, out int ay);

                yield return (dir, ax, ay, ai, Areas[ai], Spans[ai]);
            }
        }

        /// <summary>
        /// Inserts an element into the array, maintaining their order
        /// </summary>
        /// <param name="arr">Array</param>
        /// <param name="n">Number of items in the array</param>
        /// <returns>Returns the new array</returns>
        private static AreaTypes[] InsertSort(AreaTypes[] arr, int n)
        {
            //Copy array
            var res = arr.ToArray();

            int i, j;
            for (i = 1; i < n; i++)
            {
                var value = res[i];

                //Make space in the array
                for (j = i - 1; j >= 0 && res[j] > value; j--)
                {
                    res[j + 1] = res[j];
                }

                //Set the value
                res[j + 1] = value;
            }

            return res;
        }
        /// <summary>
        /// Updates the stack list
        /// </summary>
        /// <param name="srcStack">Source stack list</param>
        /// <param name="srcReg">Source region list</param>
        /// <returns>Returns the updated stack list</returns>
        private static LevelStackEntry[] AppendStacks(LevelStackEntry[] srcStack, int[] srcReg)
        {
            List<LevelStackEntry> dstStack = [];

            foreach (var stack in srcStack)
            {
                int i = stack.Index;
                if ((i < 0) || (srcReg[i] != 0))
                {
                    continue;
                }

                dstStack.Add(stack);
            }

            return [.. dstStack];
        }
        /// <summary>
        /// Sorts the specified edge vertices (vi, vj)
        /// </summary>
        /// <param name="a">Source edge vertex A</param>
        /// <param name="b">Source edge vertex B</param>
        /// <param name="ra">Resulting vertex A</param>
        /// <param name="rb">Resulting vertex B</param>
        /// <returns>Returns true if the sorting operation swaps de vertex order</returns>
        private static bool GetPolyVerts(Vector3 a, Vector3 b, out Vector3 ra, out Vector3 rb)
        {
            rb = a;
            ra = b;
            bool swapped = false;

            // Make sure the segments are always handled in same order
            // using lexological sort or else there will be seams.
            if (Math.Abs(rb.X - ra.X) < Utils.ZeroTolerance)
            {
                if (rb.Z > ra.Z)
                {
                    (ra, rb) = (rb, ra);
                    swapped = true;
                }
            }
            else
            {
                if (rb.X > ra.X)
                {
                    (ra, rb) = (rb, ra);
                    swapped = true;
                }
            }

            return swapped;
        }
        /// <summary>
        /// Simlifies the edge samples
        /// </summary>
        /// <param name="edgeSamples">Edge sample point list</param>
        /// <param name="nn">Number of points</param>
        /// <param name="param">Build parameters</param>
        /// <returns>Returns the sample indices</returns>
        private static int[] SimplifySamples(Vector3[] edgeSamples, int nn, BuildPolyDetailParams param)
        {
            float sampleMaxError = param.SampleMaxError * param.SampleMaxError;

            int[] idx = new int[MAX_VERTS_PER_EDGE];
            idx[0] = 0;
            idx[1] = nn;
            int nidx = 2;
            for (int k = 0; k < nidx - 1;)
            {
                int a = idx[k];
                int b = idx[k + 1];

                // Find maximum deviation along the segment.
                var (maxi, maxd) = FindMaximumDeviationAlongSegment(a, b, edgeSamples);

                // If the max deviation is larger than accepted error,
                // add new point, else continue to next segment.
                if (maxi != -1 && maxd > sampleMaxError)
                {
                    for (int m = nidx; m > k; --m)
                    {
                        idx[m] = idx[m - 1];
                    }
                    idx[k + 1] = maxi;
                    nidx++;

                    continue;
                }

                ++k;
            }

            return idx.Take(nidx).ToArray();
        }
        /// <summary>
        /// Finds maximum deviation along the segment
        /// </summary>
        private static (int maxi, float maxd) FindMaximumDeviationAlongSegment(int a, int b, Vector3[] edgeSamples)
        {
            var va = edgeSamples[a];
            var vb = edgeSamples[b];

            float maxd = 0;
            int maxi = -1;
            for (int m = a + 1; m < b; ++m)
            {
                float dev = Utils.DistancePtSeg(edgeSamples[m], va, vb);
                if (dev > maxd)
                {
                    maxd = dev;
                    maxi = m;
                }
            }

            return (maxi, maxd);
        }
        /// <summary>
        /// Gets whether the specified value has the border flag or not
        /// </summary>
        /// <param name="value">Value</param>
        public static bool IsBorder(int value)
        {
            return (value & RC_BORDER_REG) != 0;
        }

        /// <summary>
        /// Remove the border size to the heightfield bounds
        /// </summary>
        public BoundingBox GetBoundsWithBorder()
        {
            var bmin = BoundingBox.Minimum;
            var bmax = BoundingBox.Maximum;
            if (BorderSize <= 0)
            {
                return new(bmin, bmax);
            }

            // If the heightfield was build with bordersize, remove the offset.
            float pad = BorderSize * CellSize;
            bmin.X += pad;
            bmin.Z += pad;
            bmax.X -= pad;
            bmax.Z -= pad;

            return new(bmin, bmax);
        }

        /// <summary>
        /// Partition the heightfield so that we can use simple algorithm later to triangulate the walkable areas.
        /// </summary>
        /// <param name="cfg">Configuration</param>
        /// <remarks>
        /// There are 3 martitioning methods, each with some pros and cons:
        /// 1) Watershed partitioning
        ///   - the classic Recast partitioning
        ///   - creates the nicest tessellation
        ///   - usually slowest
        ///   - partitions the heightfield into nice regions without holes or overlaps
        ///   - the are some corner cases where this method creates produces holes and overlaps
        ///      - holes may appear when a small obstacles is close to large open area (triangulation can handle this)
        ///      - overlaps may occur if you have narrow spiral corridors (i.e stairs), this make triangulation to fail
        ///   * generally the best choice if you precompute the navmesh, use this if you have large open areas
        /// 2) Monotone partioning
        ///   - fastest
        ///   - partitions the heightfield into regions without holes and overlaps (guaranteed)
        ///   - creates long thin polygons, which sometimes causes paths with detours
        ///   * use this if you want fast navmesh generation
        /// 3) Layer partitoining
        ///   - quite fast
        ///   - partitions the heighfield into non-overlapping regions
        ///   - relies on the triangulation code to cope with holes (thus slower than monotone partitioning)
        ///   - produces better triangles than monotone partitioning
        ///   - does not have the corner cases of watershed partitioning
        ///   - can be slow and create a bit ugly tessellation (still better than monotone)
        ///     if you have large open areas with small obstacles (not a problem if you use tiles)
        ///   * good choice to use for tiled navmesh with medium and small sized tiles
        /// </remarks>
        public void SamplePartition(Config cfg)
        {
            if (cfg.PartitionType == SamplePartitionTypes.Watershed)
            {
                // Partition the walkable surface into simple regions without holes.
                bool built = BuildRegionsWatershed(cfg.BorderSize, cfg.MinRegionArea, cfg.MergeRegionArea);
                if (!built)
                {
                    throw new EngineException("buildNavigation: Could not build watershed regions.");
                }
            }
            else if (cfg.PartitionType == SamplePartitionTypes.Monotone)
            {
                // Partition the walkable surface into simple regions without holes.
                // Monotone partitioning does not need distancefield.
                bool built = BuildRegionsMonotone(cfg.BorderSize, cfg.MinRegionArea, cfg.MergeRegionArea);
                if (!built)
                {
                    throw new EngineException("buildNavigation: Could not build monotone regions.");
                }
            }
            else if (cfg.PartitionType == SamplePartitionTypes.Layers)
            {
                // Partition the walkable surface into simple regions without holes.
                bool built = BuildRegionsLayer(cfg.BorderSize, cfg.MinRegionArea);
                if (!built)
                {
                    throw new EngineException("buildNavigation: Could not build layer regions.");
                }
            }
        }

        /// <summary>
        /// Fill in cells and spans.
        /// </summary>
        /// <param name="hf">Heightfield</param>
        public void FillCellsAndSpans(Heightfield hf)
        {
            int spanCount = hf.GetSpanCount();

            Cells = new CompactCell[Width * Height];
            Spans = new CompactSpan[spanCount];
            Areas = new AreaTypes[spanCount];
            SpanCount = spanCount;

            // Fill in cells and spans.
            int idx = 0;
            foreach (var (x, y, span) in hf.IterateSpans())
            {
                var c = new CompactCell
                {
                    Index = idx,
                    Count = 0
                };

                var s = span;
                do
                {
                    if (s.Area != AreaTypes.RC_NULL_AREA)
                    {
                        int bot = s.SMax;
                        int top = s.Next?.SMin ?? int.MaxValue;
                        Spans[idx].Y = MathUtil.Clamp(bot, 0, SPAN_MAX_WIDTH);
                        Spans[idx].H = MathUtil.Clamp(top - bot, 0, SPAN_MAX_HEIGHT);
                        Areas[idx] = s.Area;
                        idx++;
                        c.Count++;
                    }

                    s = s.Next;
                }
                while (s != null);

                Cells[x + y * Width] = c;
            }
        }

        /// <summary>
        /// Find neighbour connections.
        /// </summary>
        public void FindNeighbourConnections()
        {
            // Find neighbour connections.
            int maxLayers = CompactSpan.MaxLayers;
            int tooHighNeighbour = 0;
            foreach (var (x, y, i, _) in IterateCells())
            {
                Spans[i] = FindConnections(x, y, Spans[i], maxLayers, ref tooHighNeighbour);
            }

            if (tooHighNeighbour > maxLayers)
            {
                throw new EngineException($"Heightfield has too many layers {tooHighNeighbour} (max: {maxLayers})");
            }
        }
        /// <summary>
        /// Iterate over all neighbour spans and check if any of the is accessible from current cell.
        /// </summary>
        /// <param name="x">X cell coordinate</param>
        /// <param name="y">Y cell coordinate</param>
        /// <param name="span">Compact span</param>
        /// <param name="maxLayers">Maximum layers</param>
        /// <param name="tooHighNeighbour">Returns the too high neighbour index, if any</param>
        /// <returns>Returns the updated compact span</returns>
        private CompactSpan FindConnections(int x, int y, CompactSpan span, int maxLayers, ref int tooHighNeighbour)
        {
            var s = span;

            for (int dir = 0; dir < 4; dir++)
            {
                s.Disconnect(dir);
                int nx = x + GridUtils.GetDirOffsetX(dir);
                int ny = y + GridUtils.GetDirOffsetY(dir);

                // First check that the neighbour cell is in bounds.
                if (nx < 0 || ny < 0 || nx >= Width || ny >= Height)
                {
                    continue;
                }

                // Iterate over all neighbour spans and check if any of the is
                // accessible from current cell.
                var nc = Cells[nx + ny * Width];

                for (int k = nc.Index, nk = nc.Index + nc.Count; k < nk; ++k)
                {
                    var ns = Spans[k];

                    int bot = Math.Max(s.Y, ns.Y);
                    int top = Math.Min(s.Y + s.H, ns.Y + ns.H);

                    // Check that the gap between the spans is walkable,
                    // and that the climb height between the gaps is not too high.
                    if ((top - bot) >= WalkableHeight && Math.Abs(ns.Y - s.Y) <= WalkableClimb)
                    {
                        // Mark direction as walkable.
                        int lidx = k - nc.Index;
                        if (lidx < 0 || lidx > maxLayers)
                        {
                            tooHighNeighbour = Math.Max(tooHighNeighbour, lidx);
                            continue;
                        }

                        s.SetCon(dir, lidx);
                        break;
                    }
                }
            }

            return s;
        }

        /// <summary>
        /// Marks the geometry areas into the heightfield
        /// </summary>
        /// <param name="geometry">Geometry input</param>
        public void MarkAreas(InputGeometry geometry)
        {
            var areas = geometry.GetAreas();

            foreach (var area in areas)
            {
                var bbox = area.GetBounds();

                if (!GetAreaBounds(bbox, out var min, out var max))
                {
                    return;
                }

                MarkArea(area, min, max, (AreaTypes)area.AreaType);
            }

            MedianFilterWalkableArea();
        }
        /// <summary>
        /// Gets the area bounds
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="min">Resulting bounds min</param>
        /// <param name="max">Resulting bounds max</param>
        private bool GetAreaBounds(BoundingBox bbox, out Int3 min, out Int3 max)
        {
            min = new Int3();
            max = new Int3();

            min.X = (int)((bbox.Minimum.X - BoundingBox.Minimum.X) / CellSize);
            min.Y = (int)((bbox.Minimum.Y - BoundingBox.Minimum.Y) / CellHeight);
            min.Z = (int)((bbox.Minimum.Z - BoundingBox.Minimum.Z) / CellSize);
            max.X = (int)((bbox.Maximum.X - BoundingBox.Minimum.X) / CellSize);
            max.Y = (int)((bbox.Maximum.Y - BoundingBox.Minimum.Y) / CellHeight);
            max.Z = (int)((bbox.Maximum.Z - BoundingBox.Minimum.Z) / CellSize);

            if (max.X < 0) return false;
            if (max.Z < 0) return false;
            if (min.X >= Width) return false;
            if (min.Z >= Height) return false;

            min.X = Math.Max(0, min.X);
            min.Z = Math.Max(0, min.Z);
            if (max.X >= Width) max.X = Width - 1;
            if (max.Z >= Height) max.Z = Height - 1;

            return true;
        }
        /// <summary>
        /// Marks the specified area
        /// </summary>
        /// <param name="graphArea">Graph area</param>
        /// <param name="min">Minimum bound limits</param>
        /// <param name="max">Maximum bound limits</param>
        /// <param name="areaId">Area value to mark</param>
        private void MarkArea(IGraphArea graphArea, Int3 min, Int3 max, AreaTypes areaId)
        {
            switch (graphArea)
            {
                case IGraphAreaPolygon polyArea:
                    MarkConvexPolyArea(polyArea.Vertices, min, max, areaId);
                    return;
                case IGraphAreaCylinder cylinderArea:
                    MarkCylinderArea(cylinderArea.Center, cylinderArea.Radius, min, max, areaId);
                    return;
                case IGraphAreaBox:
                    MarkBoxArea(min, max, areaId);
                    return;
            }
        }
        /// <summary>
        /// Marks the specified box area
        /// </summary>
        /// <param name="min">Minimum bound limits</param>
        /// <param name="max">Maximum bound limits</param>
        /// <param name="areaId">Area value to mark</param>
        /// <remarks>
        /// The value of spacial parameters are in world units.
        /// </remarks>
        private void MarkBoxArea(Int3 min, Int3 max, AreaTypes areaId)
        {
            foreach (var (i, _) in IterateCellsSpansAreas(min.X, min.Z, max.X, max.Z))
            {
                Areas[i] = areaId;
            }
        }
        /// <summary>
        /// Marks the specified polygon area
        /// </summary>
        /// <param name="vertices">polygon vertices</param>
        /// <param name="min">Minimum bound limits</param>
        /// <param name="max">Maximum bound limits</param>
        /// <param name="areaId">Area value to mark</param>
        /// <remarks>
        /// The value of spacial parameters are in world units.
        /// The y-values of the polygon vertices are ignored. So the polygon is effectively projected onto the xz-plane at hmin, then extruded to hmax.
        /// </remarks>
        private void MarkConvexPolyArea(Vector3[] vertices, Int3 min, Int3 max, AreaTypes areaId)
        {
            foreach (var (i, spanCenter) in IterateCellsSpansAreas(min.X, min.Z, max.X, max.Z))
            {
                if (Utils.PointInPolygon2D(spanCenter, vertices))
                {
                    Areas[i] = areaId;
                }
            }
        }
        /// <summary>
        /// Marks the specified cylinder area
        /// </summary>
        /// <param name="center">Cylinder center</param>
        /// <param name="r">Radius</param>
        /// <param name="min">Minimum bound limits</param>
        /// <param name="max">Maximum bound limits</param>
        /// <param name="areaId">Area value to mark</param>
        /// <remarks>
        /// The value of spacial parameters are in world units.
        /// </remarks>
        private void MarkCylinderArea(Vector3 center, float r, Int3 min, Int3 max, AreaTypes areaId)
        {
            float r2 = r * r;

            foreach (var (i, spanCenter) in IterateCellsSpansAreas(min.X, min.Z, max.X, max.Z))
            {
                float dx = spanCenter.X - center.X;
                float dz = spanCenter.Z - center.Z;

                if (dx * dx + dz * dz < r2)
                {
                    Areas[i] = areaId;
                }
            }
        }
        /// <summary>
        /// This filter is usually applied after applying area id's using functions such as MarkBoxArea, MarkConvexPolyArea, and MarkCylinderArea.
        /// </summary>
        private void MedianFilterWalkableArea()
        {
            // Init distance.
            var areas = Helper.CreateArray(SpanCount, AreaTypes.RC_UNDEFINED);

            foreach (var (x, y, i, _) in IterateCells())
            {
                if (Areas[i] == AreaTypes.RC_NULL_AREA)
                {
                    areas[i] = Areas[i];
                    continue;
                }

                var nei = Helper.CreateArray(9, Areas[i]);

                var s = Spans[i];

                foreach (var (dir, ax, ay, _, area, a) in IterateSpanConnections(s, x, y))
                {
                    if (area != AreaTypes.RC_NULL_AREA)
                    {
                        nei[dir * 2 + 0] = area;
                    }

                    int dir2 = GridUtils.RotateCW(dir);
                    if (!a.GetCon(dir2, out int con2))
                    {
                        continue;
                    }

                    int ai2 = GetNeighbourCellIndex(ax, ay, dir2, con2);
                    if (Areas[ai2] != AreaTypes.RC_NULL_AREA)
                    {
                        nei[dir * 2 + 1] = Areas[ai2];
                    }
                }

                nei = InsertSort(nei, 9);
                areas[i] = nei[4];
            }

            Array.Copy(areas, Areas, SpanCount);
        }

        /// <summary>
        /// Basically, any spans that are closer to a boundary or obstruction than the specified radius are marked as unwalkable.
        /// </summary>
        /// <param name="radius">Radius</param>
        /// <remarks>
        /// This method is usually called immediately after the heightfield has been built.
        /// </remarks>
        public void ErodeWalkableArea(int radius)
        {
            // Init distance.
            int[] dist = MarkBoundaryCellsNotNullAreas();

            // Pass 1
            foreach (var (x, y, i, _) in IterateCells())
            {
                // (0,-1) & (1,-1)
                if (CalculateDistance(x, y, i, dist, 0, 3, out int newD1))
                {
                    dist[i] = newD1;
                }

                // (0,-1) & (1,-1)
                if (CalculateDistance(x, y, i, dist, 3, 2, out int newD2))
                {
                    dist[i] = newD2;
                }
            }

            // Pass 2
            foreach (var (x, y, i, _) in IterateCellsReverse())
            {
                // (1,0) & (1,1)
                if (CalculateDistance(x, y, i, dist, 2, 1, out int newD1))
                {
                    dist[i] = newD1;
                }

                // (0,1) & (-1,1)
                if (CalculateDistance(x, y, i, dist, 1, 0, out int newD2))
                {
                    dist[i] = newD2;
                }
            }

            int thr = radius * 2;
            for (int i = 0; i < SpanCount; ++i)
            {
                if (dist[i] < thr)
                {
                    Areas[i] = AreaTypes.RC_NULL_AREA;
                }
            }
        }
        /// <summary>
        /// Calculates the distance map
        /// </summary>
        /// <param name="x">X cell coordinate</param>
        /// <param name="y">Y cell coordinate</param>
        /// <param name="i">Span index</param>
        /// <param name="dist">Distance map</param>
        /// <param name="dir1">Direction 1</param>
        /// <param name="dir2">Direction 2</param>
        /// <param name="d">Returns the new distance</param>
        /// <returns>Returns true if a new distance was found</returns>
        private bool CalculateDistance(int x, int y, int i, int[] dist, int dir1, int dir2, out int d)
        {
            d = dist[i];

            var s = Spans[i];
            if (!s.GetCon(dir1, out int con1))
            {
                return false;
            }

            bool updated = false;

            int ai = GetNeighbourCellIndex(x, y, dir1, con1, out int ax, out int ay);
            int nd = Math.Min(dist[ai] + 2, 255);
            if (nd < d)
            {
                d = nd;
                updated = true;
            }

            var asp = Spans[ai];
            if (!asp.GetCon(dir2, out int con2))
            {
                return updated;
            }

            int aai = GetNeighbourCellIndex(ax, ay, dir2, con2);
            nd = Math.Min(dist[aai] + 3, 255);
            if (nd < d)
            {
                d = nd;
                updated = true;
            }

            return updated;
        }
        /// <summary>
        /// Mark boundary cells.
        /// </summary>
        /// <returns>Returns the distance map</returns>
        private int[] MarkBoundaryCellsNotNullAreas()
        {
            // Init distance.
            int[] dist = Helper.CreateArray(SpanCount, SPAN_MAX_HEIGHT);

            // Mark boundary cells.
            foreach (var (x, y, i, _) in IterateCells())
            {
                if (Areas[i] == AreaTypes.RC_NULL_AREA)
                {
                    dist[i] = 0;
                    continue;
                }

                int nc = 0;
                foreach (var (_, _, _, _, area, _) in IterateSpanConnections(Spans[i], x, y))
                {
                    if (area != AreaTypes.RC_NULL_AREA)
                    {
                        nc++;
                    }
                }

                // At least one missing neighbour.
                if (nc != 4)
                {
                    dist[i] = 0;
                }
            }

            return dist;
        }
        /// <summary>
        /// Returns whether the specified edge is solid
        /// </summary>
        /// <param name="col">X coordinate</param>
        /// <param name="row">Y coordinate</param>
        /// <param name="spanIndex">Index</param>
        /// <param name="dir">Direction</param>
        /// <param name="srcReg">Region list</param>
        /// <returns>Returns true if the specified edge is solid</returns>
        private bool IsSolidEdge(int col, int row, int spanIndex, int dir, int[] srcReg)
        {
            var s = Spans[spanIndex];
            int r = 0;
            if (s.GetCon(dir, out int con))
            {
                int ai = GetNeighbourCellIndex(col, row, dir, con);
                r = srcReg[ai];
            }

            if (r == srcReg[spanIndex])
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Gets the corner height
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="i">Index</param>
        /// <param name="dir">Direction</param>
        /// <param name="isBorderVertex">Returns true if the result vertex is a border</param>
        /// <returns>Returns the corner height</returns>
        private int GetCornerHeight(int x, int y, int i, int dir, out bool isBorderVertex)
        {
            var s = Spans[i];
            int ch = s.Y;
            int dirp = GridUtils.RotateCW(dir);
            int conp;

            int[] regs = [0, 0, 0, 0];

            // Combine region and area codes in order to prevent
            // border vertices which are in between two areas to be removed.
            regs[0] = Spans[i].Reg | ((int)Areas[i] << 16);

            if (s.GetCon(dir, out int con))
            {
                int ai = GetNeighbourCellIndex(x, y, dir, con, out int ax, out int ay);
                var a = Spans[ai];
                ch = Math.Max(ch, a.Y);
                regs[1] = Spans[ai].Reg | ((int)Areas[ai] << 16);
                if (a.GetCon(dirp, out conp))
                {
                    int ai2 = GetNeighbourCellIndex(ax, ay, dirp, conp);
                    var as2 = Spans[ai2];
                    ch = Math.Max(ch, as2.Y);
                    regs[2] = Spans[ai2].Reg | ((int)Areas[ai2] << 16);
                }
            }

            if (s.GetCon(dirp, out conp))
            {
                int ai = GetNeighbourCellIndex(x, y, dirp, conp, out int ax, out int ay);
                var a = Spans[ai];
                ch = Math.Max(ch, a.Y);
                regs[3] = Spans[ai].Reg | ((int)Areas[ai] << 16);
                if (a.GetCon(dir, out con))
                {
                    int ai2 = GetNeighbourCellIndex(ax, ay, dir, con);
                    var as2 = Spans[ai2];
                    ch = Math.Max(ch, as2.Y);
                    regs[2] = Spans[ai2].Reg | ((int)Areas[ai2] << 16);
                }
            }

            // Check if the vertex is special edge vertex, these vertices will be removed later.
            isBorderVertex = IsBorderVertex(regs);

            return ch;
        }
        /// <summary>
        /// Checks whether the vertex is special edge vertex
        /// </summary>
        /// <param name="regs">Regions</param>
        private static bool IsBorderVertex(int[] regs)
        {
            // Check if the vertex is special edge vertex, these vertices will be removed later.
            for (int j = 0; j < 4; ++j)
            {
                int a = j;
                int b = GridUtils.Rotate(j, 1);
                int c = GridUtils.Rotate(j, 2);
                int d = GridUtils.Rotate(j, 3);

                // The vertex is a border vertex there are two same exterior cells in a row,
                // followed by two interior cells and none of the regions are out of bounds.
                bool twoSameExts = IsBorder(regs[a] & regs[b]) && regs[a] == regs[b];
                bool twoInts = !IsBorder(regs[c] | regs[d]);
                bool intsSameArea = (regs[c] >> 16) == (regs[d] >> 16);
                bool noZeros = regs[a] != 0 && regs[b] != 0 && regs[c] != 0 && regs[d] != 0;
                if (twoSameExts && twoInts && intsSameArea && noZeros)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the height data
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="rect">Polygon bounds</param>
        /// <param name="verts">Vertex indices</param>
        /// <param name="region">Region index</param>
        /// <remarks>
        /// Reads to the compact heightfield are offset by border size, since border size offset is already removed from the polymesh vertices.
        /// </remarks>
        /// <returns>Returns the height patch</returns>
        public HeightPatch GetHeightData(IndexedPolygon poly, int region, Int3[] verts, Rectangle rect)
        {
            var queue = new List<HeightDataItem>(512);

            HeightPatch hp = new(rect);

            bool empty = true;

            // We cannot sample from this poly if it was created from polys
            // of different regions. If it was then it could potentially be overlapping
            // with polys of that region and the heights sampled here could be wrong.
            if (!IndexedPolygon.HasMultipleRegions(region))
            {
                empty = GetHeightDataFromMultipleRegions(hp, region, out var qItems);

                queue.AddRange(qItems);
            }

            // if the polygon does not contain any points from the current region (rare, but happens)
            // or if it could potentially be overlapping polygons of the same region,
            // then use the center as the seed point.
            if (empty)
            {
                queue.AddRange(SeedArrayWithPolyCenter(poly, verts, hp));
            }

            ProcessHeightDataQueue(hp, queue);

            return hp;
        }
        /// <summary>
        /// Gets the heigth data of the specified region
        /// </summary>
        /// <param name="hp">Height patch</param>
        /// <param name="regionId">Region id</param>
        /// <param name="dataItems">Resulting data item list</param>
        /// <returns>Returns true if the resulting heigth data is empty</returns>
        private bool GetHeightDataFromMultipleRegions(HeightPatch hp, int regionId, out HeightDataItem[] dataItems)
        {
            bool empty = true;

            List<HeightDataItem> queue = [];

            // Copy the height from the same region, and mark region borders
            // as seed points to fill the rest.
            foreach (var (hx, hy, x, y) in hp.IterateBounds(BorderSize))
            {
                foreach (var (s, i) in IterateCellSpans(x, y))
                {
                    if (s.Reg != regionId)
                    {
                        continue;
                    }

                    // Store height
                    hp.SetHeight(hx, hy, s.Y);
                    empty = false;

                    // If any of the neighbours is not in same region,
                    // add the current location as flood fill start
                    bool border = DetectBorder(x, y, regionId, s);
                    if (border)
                    {
                        queue.Add(new() { X = x, Y = y, I = i });
                    }

                    break;
                }
            }

            dataItems = [.. queue];

            return empty;
        }
        /// <summary>
        /// Finds whether all span neighbors were in other regions than the specified
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="region">Region id</param>
        /// <param name="s">Compact span</param>
        private bool DetectBorder(int x, int y, int region, CompactSpan s)
        {
            foreach (var item in IterateSpanConnections(s, x, y))
            {
                if (item.s.Reg != region)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Process height data item queue
        /// </summary>
        /// <param name="hp">Height patch</param>
        /// <param name="queue">Data item queue to process</param>
        private void ProcessHeightDataQueue(HeightPatch hp, List<HeightDataItem> queue)
        {
            const int RETRACT_SIZE = 256;
            int head = 0;

            // We assume the seed is centered in the polygon, so a BFS to collect
            // height data will ensure we do not move onto overlapping polygons and
            // sample wrong heights.
            while (head < queue.Count)
            {
                var c = queue[head];
                head++;
                if (head >= RETRACT_SIZE)
                {
                    head = 0;
                    if (queue.Count > RETRACT_SIZE)
                    {
                        queue.RemoveRange(0, RETRACT_SIZE);
                    }
                    queue.Clear();
                }

                var cs = Spans[c.I];
                foreach (var (dir, con) in cs.IterateSpanConnections())
                {
                    int ax = c.X + GridUtils.GetDirOffsetX(dir);
                    int ay = c.Y + GridUtils.GetDirOffsetY(dir);
                    int hx = ax - hp.Bounds.X - BorderSize;
                    int hy = ay - hp.Bounds.Y - BorderSize;

                    if (!hp.CompareBounds(hx, hy))
                    {
                        continue;
                    }

                    int ai = Cells[ax + ay * Width].Index + con;
                    var a = Spans[ai];

                    hp.Data[hx + hy * hp.Bounds.Width] = a.Y;

                    queue.Add(new() { X = ax, Y = ay, I = ai });
                }
            }
        }

        /// <summary>
        /// Builds the polygon detail
        /// </summary>
        /// <param name="polygon">Polygon vertices</param>
        /// <param name="param">Build settings</param>
        /// <param name="hp">Height patch</param>
        /// <param name="outVerts">Resulting vertices</param>
        /// <param name="outTris">Resulting triangle indices</param>
        public void BuildPolyDetail(Vector3[] polygon, BuildPolyDetailParams param, HeightPatch hp, out Vector3[] outVerts, out Int3[] outTris)
        {
            //Copy polygon array
            var verts = polygon.ToArray();
            var hull = Array.Empty<int>();

            // Tessellate outlines.
            // This is done in separate pass in order to ensure
            // seamless height values across the ply boundaries.
            float sampleDist = param.SampleDist;
            if (sampleDist > 0)
            {
                verts = TesselateOutlines(verts, param, hp, out hull);
            }

            // Calculate minimum extents of the polygon based on input data.
            float minExtent = Utils.PolyMinExtent2D(verts);

            // If the polygon minimum extent is small (sliver or small triangle), do not try to add internal points.
            if (minExtent < sampleDist * 2)
            {
                outVerts = verts;
                outTris = TriangulationHelper.TriangulateHull(verts, polygon.Length, hull);

                return;
            }

            // Tessellate the base mesh.
            // We're using the triangulateHull instead of delaunayHull as it tends to
            // create a bit better triangulation for long thin triangles when there
            // are no internal points.
            var tris = TriangulationHelper.TriangulateHull(verts, polygon.Length, hull);
            if (tris.Length == 0)
            {
                // Could not triangulate the poly, make sure there is some valid data there.
                Logger.WriteWarning(this, $"buildPolyDetail: Could not triangulate polygon ({verts.Length} verts).");

                outVerts = verts;
                outTris = [];

                return;
            }

            if (sampleDist > 0)
            {
                // Create sample locations in a grid.
                verts = CreateGridSampleLocations(verts, param, hp, hull, tris, out var newTris);
                tris = newTris;
            }

            int ntris = tris.Length;
            int MAX_TRIS = 255;    // Max tris for delaunay is 2n-2-k (n=num verts, k=num hull verts).
            if (ntris > MAX_TRIS)
            {
                tris = tris.Take(MAX_TRIS).ToArray();
                Logger.WriteWarning(this, $"rcBuildPolyMeshDetail: Shrinking triangle count from {ntris} to max {MAX_TRIS}.");
            }

            outVerts = verts;
            outTris = tris;
        }
        /// <summary>
        /// Tessellate outlines.
        /// </summary>
        private Vector3[] TesselateOutlines(Vector3[] polygon, BuildPolyDetailParams param, HeightPatch hp, out int[] hull)
        {
            var verts = polygon.ToList();
            List<int> hullList = new(MAX_VERTS);

            int ninp = polygon.Length;
            for (int i = 0, j = ninp - 1; i < ninp; j = i++)
            {
                bool swapped = GetPolyVerts(polygon[i], polygon[j], out var vi, out var vj);

                // Create samples along the edge.
                CreateSamples(verts.Count, param, hp, vi, vj, out var edgeSamples, out int nn);

                // Simplify samples.
                var idx = SimplifySamples(edgeSamples, nn, param);

                hullList.Add(j);

                // Add new vertices.
                int nidx = idx.Length;
                if (swapped)
                {
                    for (int k = nidx - 2; k > 0; --k)
                    {
                        hullList.Add(verts.Count);
                        verts.Add(edgeSamples[idx[k]]);
                    }
                }
                else
                {
                    for (int k = 1; k < nidx - 1; ++k)
                    {
                        hullList.Add(verts.Count);
                        verts.Add(edgeSamples[idx[k]]);
                    }
                }
            }

            hull = [.. hullList];

            return [.. verts];
        }
        /// <summary>
        /// Creates height patch samples
        /// </summary>
        /// <param name="npolys">Number of polygons</param>
        /// <param name="param">Build parameters</param>
        /// <param name="hp">Height patch</param>
        /// <param name="edge">Edge vertices</param>
        /// <param name="nn">Number of vertices in the edge</param>
        private void CreateSamples(int npolys, BuildPolyDetailParams param, HeightPatch hp, Vector3 vi, Vector3 vj, out Vector3[] edge, out int nn)
        {
            float sampleDist = param.SampleDist;
            int heightSearchRadius = param.HeightSearchRadius;

            float cs = CellSize;
            float ics = 1.0f / cs;

            var vd = vi - vj;
            float d = MathF.Sqrt(vd.X * vd.X + vd.Z * vd.Z);
            nn = 1 + (int)MathF.Floor(d / sampleDist);
            if (nn >= MAX_VERTS_PER_EDGE)
            {
                nn = MAX_VERTS_PER_EDGE - 1;
            }
            if (npolys + nn >= MAX_VERTS)
            {
                nn = MAX_VERTS - 1 - npolys;
            }

            edge = new Vector3[nn + 1];

            for (int k = 0; k <= nn; ++k)
            {
                float u = k / (float)nn;
                var pos = vj + vd * u;
                pos.Y = hp.GetHeight(pos, ics, CellHeight, heightSearchRadius) * CellHeight;

                edge[k] = pos;
            }
        }
        /// <summary>
        /// Create sample locations in a grid
        /// </summary>
        private Vector3[] CreateGridSampleLocations(Vector3[] polygon, BuildPolyDetailParams param, HeightPatch hp, int[] hull, Int3[] tris, out Int3[] newTris)
        {
            float sampleDist = param.SampleDist;
            float sampleMaxError = param.SampleMaxError;

            var verts = new List<Vector3>(polygon);
            var triList = new List<Int3>(tris);

            // Create sample locations in a grid.
            var samples = InitializeSamples(polygon, param, hp);

            // Add the samples starting from the one that has the most
            // error. The procedure stops when all samples are added
            // or when the max error is within treshold.
            int nsamples = samples.Count;
            for (int iter = 0; iter < nsamples; ++iter)
            {
                if (verts.Count >= MAX_VERTS)
                {
                    break;
                }

                // Find sample with most error.
                var (bestpt, bestd, besti) = FindSampleWithMostError(samples, sampleDist, [.. verts], [.. triList]);

                // If the max error is within accepted threshold, stop tesselating.
                if (bestd <= sampleMaxError || besti == -1)
                {
                    break;
                }

                // Mark sample as added.
                var sb = samples[besti];
                sb.Added = true;
                samples[besti] = sb;

                // Add the new sample point.
                verts.Add(bestpt);

                // Create new triangulation. Full rebuild.
                var dTris = TriangulateDelaunay([.. verts], hull);

                triList.Clear();
                triList.AddRange(dTris);
            }

            newTris = [.. triList];

            return [.. verts];
        }
        /// <summary>
        /// Triangulate hull using delaunay
        /// </summary>
        /// <param name="verts">Vertex list</param>
        /// <param name="hull">Hull indices</param>
        private static Int3[] TriangulateDelaunay(Vector3[] verts, int[] hull)
        {
            var dhull = DelaunayHull.Build(verts, hull);

            return dhull.GetTris();
        }
        /// <summary>
        /// Initialize samples
        /// </summary>
        /// <param name="polygon">Polygon vertices</param>
        /// <param name="param">Build polygon parameters</param>
        /// <param name="hp">Height patch</param>
        private List<SampleVertex> InitializeSamples(Vector3[] polygon, BuildPolyDetailParams param, HeightPatch hp)
        {
            var samples = new List<SampleVertex>();

            float sampleDist = param.SampleDist;
            float samplePDist = -sampleDist / 2;
            int heightSearchRadius = param.HeightSearchRadius;

            var bbox = Utils.GetPolygonBounds(polygon);
            float h = (bbox.Maximum.Y + bbox.Minimum.Y) * 0.5f;

            int x0 = (int)Math.Floor(bbox.Minimum.X / sampleDist);
            int x1 = (int)Math.Ceiling(bbox.Maximum.X / sampleDist);
            int z0 = (int)Math.Floor(bbox.Minimum.Z / sampleDist);
            int z1 = (int)Math.Ceiling(bbox.Maximum.Z / sampleDist);

            float cs = CellSize;
            float ics = 1.0f / cs;

            for (int z = z0; z < z1; ++z)
            {
                for (int x = x0; x < x1; ++x)
                {
                    var pt = new Vector3
                    {
                        X = x * sampleDist,
                        Y = h,
                        Z = z * sampleDist
                    };

                    // Make sure the samples are not too close to the edges.
                    var dist = Utils.DistancePtPoly2D(pt, polygon);
                    if (dist > samplePDist)
                    {
                        continue;
                    }

                    int y = hp.GetHeight(pt, ics, CellHeight, heightSearchRadius);

                    samples.Add(new(x, y, z, false)); // Not added
                }
            }

            return samples;
        }
        /// <summary>
        /// Finds samples with most error
        /// </summary>
        /// <param name="samples">Sample list</param>
        /// <param name="sampleDist">Sample distance</param>
        /// <param name="verts">Vertices</param>
        /// <param name="tris">Triangles</param>
        private (Vector3 bestpt, float bestd, int besti) FindSampleWithMostError(List<SampleVertex> samples, float sampleDist, Vector3[] verts, Int3[] tris)
        {
            var bestpt = Vector3.Zero;
            float bestd = 0;
            int besti = -1;

            float cs = CellSize * 0.1f;
            float ch = CellHeight;

            int nsamples = samples.Count;
            for (int i = 0; i < nsamples; ++i)
            {
                var s = samples[i];
                if (s.Added)
                {
                    // skip added.
                    // The sample location is jittered to get rid of some bad triangulations
                    // which are cause by symmetrical data from the grid structure.
                    continue;
                }

                float jitX = Utils.GetJitterX(i);
                float jitY = Utils.GetJitterY(i);

                var pt = new Vector3
                {
                    X = s.X * sampleDist + jitX * cs,
                    Y = s.Y * ch,
                    Z = s.Z * sampleDist + jitY * cs,
                };

                float d = Utils.DistanceTriMesh(pt, verts, tris);
                if (d < 0)
                {
                    continue; // did not hit the mesh.
                }

                if (d > bestd)
                {
                    bestd = d;
                    besti = i;
                    bestpt = pt;
                }
            }

            return (bestpt, bestd, besti);
        }
        /// <summary>
        /// Seeds an array with the polygon centers
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="verts">Vertex indices</param>
        /// <param name="hp">Height patch</param>
        /// <returns>Returns a height data item array</returns>
        /// <remarks>
        /// Reads to the compact heightfield are offset by border size since border size offset is already removed from the polymesh vertices.
        /// </remarks>
        private HeightDataItem[] SeedArrayWithPolyCenter(IndexedPolygon poly, Int3[] verts, HeightPatch hp)
        {
            var bounds = hp.Bounds;

            var (startCellX, startCellY, startSpanIndex) = FindClosestCellToPolyVertex2D(poly, verts, bounds);

            var (centerX, centerY) = poly.GetCenter2D(verts);

            // Use seeds array as a stack for DFS
            var stack = new Stack<HeightDataItem>(512);
            stack.Push(new()
            {
                X = startCellX,
                Y = startCellY,
                I = startSpanIndex
            });

            hp.Data = Helper.CreateArray(bounds.Width * bounds.Height, 0);
            // DFS to move to the center. Note that we need a DFS here and can not just move
            // directly towards the center without recording intermediate nodes, even though the polygons
            // are convex. In very rare we can get stuck due to contour simplification if we do not
            // record nodes.
            var hdItem = new HeightDataItem();
            while (true)
            {
                if (stack.Count < 1)
                {
                    Logger.WriteWarning(this, "Walk towards polygon center failed to reach center");
                    break;
                }

                hdItem = stack.Pop();

                if (hdItem.X == centerX && hdItem.Y == centerY)
                {
                    break;
                }

                // If we are already at the correct X-position, prefer direction
                // directly towards the center in the Y-axis; otherwise prefer
                // direction in the X-axis
                int directDir;
                if (hdItem.X == centerX)
                {
                    directDir = GridUtils.GetDirForOffset(0, centerY > hdItem.Y ? 1 : -1);
                }
                else
                {
                    directDir = GridUtils.GetDirForOffset(centerX > hdItem.X ? 1 : -1, 0);
                }

                // Push the direct dir last so we start with this on next iteration
                (dirs[directDir], dirs[3]) = (dirs[3], dirs[directDir]);

                stack.PushRange(BuildHeightDataItems(hdItem, bounds, dirs, hp));

                (dirs[directDir], dirs[3]) = (dirs[3], dirs[directDir]);
            }

            stack.Clear();

            // getHeightData seeds are given in coordinates with borders
            stack.Push(new HeightDataItem
            {
                X = hdItem.X + BorderSize,
                Y = hdItem.Y + BorderSize,
                I = hdItem.I,
            });

            hp.Data = Helper.CreateArray(bounds.Width * bounds.Height, SPAN_MAX_HEIGHT);
            var chs = Spans[hdItem.I];
            hp.Data[(hdItem.X - bounds.X) + (hdItem.Y - bounds.Y) * bounds.Width] = chs.Y;

            return [.. stack];
        }
        /// <summary>
        /// Build the neighbour's item list of the specified height data item
        /// </summary>
        /// <param name="hdItem">Height data item</param>
        /// <param name="bounds">Bounds</param>
        /// <param name="dirs">Direction list</param>
        /// <param name="hp">Height path to store data</param>
        private IEnumerable<HeightDataItem> BuildHeightDataItems(HeightDataItem hdItem, Rectangle bounds, int[] dirs, HeightPatch hp)
        {
            var cs = Spans[hdItem.I];

            for (int i = 0; i < 4; i++)
            {
                int dir = dirs[i];
                if (!cs.GetCon(dir, out int con))
                {
                    continue;
                }

                int newX = hdItem.X + GridUtils.GetDirOffsetX(dir);
                int newY = hdItem.Y + GridUtils.GetDirOffsetY(dir);

                int hpx = newX - bounds.X;
                int hpy = newY - bounds.Y;
                if (hpx < 0 || hpx >= bounds.Width || hpy < 0 || hpy >= bounds.Height)
                {
                    continue;
                }

                if (hp.Data[hpx + hpy * bounds.Width] != 0)
                {
                    continue;
                }

                hp.Data[hpx + hpy * bounds.Width] = 1;
                int index = Cells[(newX + BorderSize) + (newY + BorderSize) * Width].Index + con;

                yield return new HeightDataItem
                {
                    X = newX,
                    Y = newY,
                    I = index
                };
            }
        }
        /// <summary>
        /// Finds the closest cell to polygon vertex
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="verts">Polygon vertices</param>
        /// <param name="bounds">Bounds</param>
        private (int StartCellX, int StartCellY, int StartSpanIndex) FindClosestCellToPolyVertex2D(IndexedPolygon poly, Int3[] verts, Rectangle bounds)
        {
            int[] offset =
            [
                +0, +0,
                -1, -1,
                +0, -1,
                +1, -1,
                +1, +0,
                +1, +1,
                +0, +1,
                -1, +1,
                -1, +0,
            ];

            // Find cell closest to a poly vertex
            var polyIndices = poly.GetVertices();
            int startCellX = 0;
            int startCellY = 0;
            int startSpanIndex = -1;
            int dmin = -1;
            for (int j = 0; j < polyIndices.Length; ++j)
            {
                var vert = verts[polyIndices[j]];

                for (int k = 0; k < 9 && dmin > 0; ++k)
                {
                    int ax = vert.X + offset[k * 2 + 0];
                    int ay = vert.Y;
                    int az = vert.Z + offset[k * 2 + 1];
                    if (!bounds.Contains(ax, az))
                    {
                        continue;
                    }

                    var c = Cells[(ax + BorderSize + az + BorderSize) * Width];
                    for (int i = c.Index, ni = c.Index + c.Count; i < ni && dmin > 0; ++i)
                    {
                        var s = Spans[i];
                        int d = Math.Abs(ay - s.Y);
                        if (d < dmin)
                        {
                            startCellX = ax;
                            startCellY = az;
                            startSpanIndex = i;
                            dmin = d;
                        }
                    }
                }
            }

            return (startCellX, startCellY, startSpanIndex);
        }

        /// <summary>
        /// Paint rectangle region borders
        /// </summary>
        /// <returns>Returns the region id list and the number of allocated regions</returns>
        private (int[] srcReg, int regionId) PaintRectRegionsBorders()
        {
            int[] srcReg = new int[SpanCount];

            int regionId = 1;

            if (BorderSize <= 0)
            {
                return (srcReg, regionId);
            }

            int w = Width;
            int h = Height;

            // Make sure border will not overflow.
            int bw = Math.Min(w, BorderSize);
            int bh = Math.Min(h, BorderSize);

            // Paint regions
            PaintRectRegionBorders(srcReg, regionId, 0, bw, 0, h); regionId++;
            PaintRectRegionBorders(srcReg, regionId, w - bw, w, 0, h); regionId++;
            PaintRectRegionBorders(srcReg, regionId, 0, w, 0, bh); regionId++;
            PaintRectRegionBorders(srcReg, regionId, 0, w, h - bh, h); regionId++;

            return (srcReg, regionId);
        }
        /// <summary>
        /// Paint rectangle region setting borders
        /// </summary>
        /// <param name="srcReg">Region id list</param>
        /// <param name="regId">Region id to set</param>
        /// <param name="minx">Min area X</param>
        /// <param name="maxx">Max area X</param>
        /// <param name="miny">Min area Y</param>
        /// <param name="maxy">Max area Y</param>
        private void PaintRectRegionBorders(int[] srcReg, int regId, int minx, int maxx, int miny, int maxy)
        {
            foreach (var (_, _, i) in IterateCellsSpans(minx, miny, maxx, maxy))
            {
                if (Areas[i] == AreaTypes.RC_NULL_AREA)
                {
                    continue;
                }

                srcReg[i] = regId | RC_BORDER_REG;
            }
        }

        /// <summary>
        /// Sorts cells by level
        /// </summary>
        /// <param name="startLevel">Start level</param>
        /// <param name="srcReg">Source regions</param>
        /// <param name="nbStacks">Number of stacks</param>
        /// <param name="stacks">Stack list</param>
        /// <param name="loglevelsPerStack">The levels per stack (2 in our case) as a bit shift</param>
        private void SortCellsByLevel(int startLevel, int[] srcReg, int nbStacks, List<LevelStackEntry>[] stacks, int loglevelsPerStack)
        {
            startLevel >>= loglevelsPerStack;

            for (int j = 0; j < nbStacks; j++)
            {
                stacks[j].Clear();
            }

            // put all cells in the level range into the appropriate stacks
            foreach (var (x, y, i, _) in IterateCells())
            {
                if (Areas[i] == AreaTypes.RC_NULL_AREA || srcReg[i] != 0)
                {
                    continue;
                }

                int level = BorderDistances[i] >> loglevelsPerStack;
                int sId = startLevel - level;
                if (sId >= nbStacks)
                {
                    continue;
                }
                if (sId < 0)
                {
                    sId = 0;
                }

                stacks[sId].Add(new() { X = x, Y = y, Index = i });
            }
        }
        /// <summary>
        /// Expand region
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="srcReg">Source regions</param>
        private List<LevelStackEntry> ExpandRegionsFillStack(int level, int[] srcReg)
        {
            // Find cells revealed by the raised level.
            List<LevelStackEntry> stack = [];

            foreach (var (x, y, i, _) in IterateCells())
            {
                if (Areas[i] == AreaTypes.RC_NULL_AREA)
                {
                    continue;
                }

                if (BorderDistances[i] >= level && srcReg[i] == 0)
                {
                    stack.Add(new() { X = x, Y = y, Index = i });
                }
            }

            return stack;
        }
        /// <summary>
        /// Expand region
        /// </summary>
        /// <param name="srcReg">Source regions</param>
        /// <param name="stack">Stack</param>
        /// <remarks>Marks all cells which already have a region</remarks>
        private static void ExpandRegionsWithCells(int[] srcReg, List<LevelStackEntry> stack)
        {
            // use cells in the input stack

            for (int j = 0; j < stack.Count; j++)
            {
                var current = stack[j];

                int i = current.Index;
                if (srcReg[i] != 0)
                {
                    current.Index = -1;

                    stack[j] = current;
                }
            }
        }
        /// <summary>
        /// Process stack
        /// </summary>
        /// <param name="stack">Stack</param>
        /// <param name="srcReg">Source regions</param>
        /// <param name="srcDist">Source distances</param>
        /// <param name="level">Level</param>
        /// <param name="maxIter">Max iterations</param>
        private void ProcessRegionsStack(List<LevelStackEntry> stack, int[] srcReg, int[] srcDist, int level, int maxIter)
        {
            int iter = 0;

            while (stack.Count > 0)
            {
                int failed = BuildDirtyEntries(stack, srcReg, srcDist);
                if (failed == stack.Count)
                {
                    break;
                }

                if (level <= 0)
                {
                    continue;
                }

                ++iter;

                if (iter >= maxIter)
                {
                    break;
                }
            }
        }
        /// <summary>
        /// Finds the nearest region of the span
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="i">Span index</param>
        /// <param name="srcReg">Source regions</param>
        /// <param name="srcDist">Source distances</param>
        private (int RegId, int Distance) FindNearestRegion(int x, int y, int i, int[] srcReg, int[] srcDist)
        {
            int r = srcReg[i];
            int d = int.MaxValue;
            var area = Areas[i];
            var s = Spans[i];
            foreach (var item in IterateSpanConnections(s, x, y))
            {
                if (item.area != area)
                {
                    continue;
                }

                int sr = srcReg[item.ai];
                int sd = srcDist[item.ai];

                if (sr > 0 && !IsBorder(sr) && sd + 2 < d)
                {
                    r = sr;
                    d = sd + 2;
                }
            }

            return (r, d);
        }
        /// <summary>
        /// Builds the dirty entries list of the stack elements
        /// </summary>
        /// <param name="stack">Stack</param>
        /// <param name="srcReg">Source regions</param>
        /// <param name="srcDist">Source distances</param>
        /// <returns>Returns the failed counter</returns>
        private int BuildDirtyEntries(List<LevelStackEntry> stack, int[] srcReg, int[] srcDist)
        {
            int failed = 0;
            dirtyEntries.Clear();

            for (int j = 0; j < stack.Count; j++)
            {
                var current = stack[j];

                int x = current.X;
                int y = current.Y;
                int i = current.Index;
                if (i < 0)
                {
                    failed++;
                    continue;
                }

                var (r, d2) = FindNearestRegion(x, y, i, srcReg, srcDist);
                if (r == 0)
                {
                    failed++;
                    continue;
                }

                current.Index = -1; // mark as used
                stack[j] = current;

                dirtyEntries.Add(new() { Index = i, Region = r, Distance2 = d2 });
            }

            for (int i = 0; i < dirtyEntries.Count; i++)
            {
                int idx = dirtyEntries[i].Index;
                srcReg[idx] = dirtyEntries[i].Region;
                srcDist[idx] = dirtyEntries[i].Distance2;
            }

            return failed;
        }
        /// <summary>
        /// Flood region
        /// </summary>
        /// <param name="level">Level value</param>
        /// <param name="regId">Region id</param>
        /// <param name="area">Area type</param>
        /// <param name="srcReg">Region id list</param>
        /// <param name="srcDist">Distance list</param>
        /// <param name="stack">Stack list</param>
        private bool FloodRegion(int level, int regId, AreaTypes area, int[] srcReg, int[] srcDist, List<LevelStackEntry> stack)
        {
            // Flood fill mark region.
            int lev = level >= 2 ? level - 2 : 0;
            int count = 0;

            while (stack.Count > 0)
            {
                var back = stack.PopLast();

                // Check if any of the neighbours already have a valid region set.
                int ar = FindValidRegionSet(back, area, regId, srcReg);
                if (ar != 0)
                {
                    srcReg[back.Index] = 0;

                    continue;
                }

                count++;

                // Expand neighbours.
                stack.AddRange(ExpandNeighbours(back, area, regId, lev, srcReg, srcDist));
            }

            return count > 0;
        }
        /// <summary>
        /// Checks if any of the neighbours already have a valid region set.
        /// </summary>
        /// <param name="entry">Stack entry</param>
        /// <param name="area">Area type</param>
        /// <param name="regId">Region Id</param>
        /// <param name="srcReg">Source region id list</param>
        private int FindValidRegionSet(LevelStackEntry entry, AreaTypes area, int regId, int[] srcReg)
        {
            int cx = entry.X;
            int cy = entry.Y;
            var cs = Spans[entry.Index];

            int ar = 0;
            foreach (var item in IterateSpanConnections(cs, cx, cy))
            {
                if (item.area != area)
                {
                    continue;
                }

                int nr = srcReg[item.ai];
                if (IsBorder(nr)) // Do not take borders into account.
                {
                    continue;
                }

                if (nr != 0 && nr != regId)
                {
                    ar = nr;
                    break;
                }

                var a = item.s;

                int dir2 = GridUtils.RotateCW(item.dir);
                if (!a.GetCon(dir2, out int con2))
                {
                    continue;
                }

                int ai2 = GetNeighbourCellIndex(item.ax, item.ay, dir2, con2);
                if (Areas[ai2] != area)
                {
                    continue;
                }

                int nr2 = srcReg[ai2];
                if (nr2 != 0 && nr2 != regId)
                {
                    ar = nr2;
                    break;
                }
            }

            return ar;
        }
        /// <summary>
        /// Expands neighbours
        /// </summary>
        /// <param name="entry">Stack entry</param>
        /// <param name="area">Area</param>
        /// <param name="regId">Region id</param>
        /// <param name="lev">Level</param>
        /// <param name="srcReg">Region id list</param>
        /// <param name="srcDist">Distance list</param>
        private IEnumerable<LevelStackEntry> ExpandNeighbours(LevelStackEntry entry, AreaTypes area, int regId, int lev, int[] srcReg, int[] srcDist)
        {
            int cx = entry.X;
            int cy = entry.Y;
            int ci = entry.Index;
            var cs = Spans[ci];

            foreach (var item in IterateSpanConnections(cs, cx, cy))
            {
                if (item.area != area)
                {
                    continue;
                }

                if (BorderDistances[item.ai] >= lev && srcReg[item.ai] == 0)
                {
                    srcReg[item.ai] = regId;
                    srcDist[item.ai] = 0;
                    yield return new() { X = item.ax, Y = item.ay, Index = item.ai };
                }
            }
        }
        /// <summary>
        /// Builds the source region list
        /// </summary>
        /// <param name="borderSize">Border size</param>
        private (int[] regs, int count) BuildSourceRegionsWatershed(int borderSize)
        {
            BorderSize = borderSize;

            // Prepare for region partitioning, by calculating distance field along the walkable surface.
            BuildDistanceField();

            int LOG_NB_STACKS = 3;
            int NB_STACKS = 1 << LOG_NB_STACKS;
            var lvlStacks = Helper.CreateArray(NB_STACKS, () => new List<LevelStackEntry>());
            var stack = new List<LevelStackEntry>();

            // Mark border regions.
            var (srcReg, regionId) = PaintRectRegionsBorders();

            int[] srcDist = new int[SpanCount];
            int level = (MaxDistance + 1) & ~1;

            // Figure better formula, expandIters defines how much the 
            // watershed "overflows" and simplifies the regions. Tying it to
            // agent radius was usually good indication how greedy it could be.
            //	const int expandIters = 4 + walkableRadius * 2
            const int expandIters = 8;

            int sId = -1;
            while (level > 0)
            {
                level = level >= 2 ? level - 2 : 0;
                sId = (sId + 1) & (NB_STACKS - 1);

                if (sId == 0)
                {
                    SortCellsByLevel(level, srcReg, NB_STACKS, lvlStacks, 1);
                }
                else
                {
                    var stacks = AppendStacks([.. lvlStacks[sId - 1]], srcReg); // copy left overs from last level
                    lvlStacks[sId].AddRange(stacks);
                }

                // Expand current regions until no empty connected cells found.
                ExpandRegionsWithCells(srcReg, lvlStacks[sId]);

                // Process the level stack
                ProcessRegionsStack(lvlStacks[sId], srcReg, srcDist, level, expandIters);

                // Mark new regions with IDs.
                regionId = MarkRegionIds(regionId, level, lvlStacks[sId], srcReg, srcDist, stack);
            }

            // Expand current regions until no empty connected cells found.
            var expStack = ExpandRegionsFillStack(0, srcReg);

            // Process the stack
            ProcessRegionsStack(expStack, srcReg, srcDist, level, expandIters * 8);

            return (srcReg, regionId);
        }
        /// <summary>
        /// Marks new regions with IDs.
        /// </summary>
        /// <param name="regionId">Last region id</param>
        /// <param name="level">Level</param>
        /// <param name="lvStack">Level stack</param>
        /// <param name="srcReg">Region id list</param>
        /// <param name="srcDist">Distance list</param>
        /// <param name="stack">Stack</param>
        /// <returns>Returns the new last region id</returns>
        private int MarkRegionIds(int regionId, int level, List<LevelStackEntry> lvStack, int[] srcReg, int[] srcDist, List<LevelStackEntry> stack)
        {
            for (int j = 0; j < lvStack.Count; j++)
            {
                var current = lvStack[j];

                int i = current.Index;
                if (i < 0 || srcReg[i] != 0)
                {
                    continue;
                }

                int x = current.X;
                int y = current.Y;
                var area = Areas[i];

                stack.Clear();
                stack.Add(new() { X = x, Y = y, Index = i });
                srcReg[i] = regionId;
                srcDist[i] = 0;

                var floodRes = FloodRegion(level, regionId, area, srcReg, srcDist, stack);
                if (!floodRes)
                {
                    continue;
                }

                if (regionId == int.MaxValue)
                {
                    throw new EngineException("rcBuildRegions: Region ID overflow");
                }

                regionId++;
            }

            return regionId;
        }
        /// <summary>
        /// Builds the source region list
        /// </summary>
        /// <param name="borderSize">Border size</param>
        private (int[] regs, int count) BuildSourceRegions(int borderSize)
        {
            BorderSize = borderSize;

            // Mark border regions.
            var (srcReg, nregions) = PaintRectRegionsBorders();

            int nsweeps = Math.Max(Width, Height);
            var sweeps = Helper.CreateArray(nsweeps, () => SweepSpan.Empty);

            // Sweep one line at a time.
            for (int row = borderSize; row < Height - borderSize; ++row)
            {
                // Collect spans from this row.
                var (samples, sweepCount) = CollectRowSpans(row, srcReg, nregions, sweeps);

                // Create unique ID.
                SweepSpan.CreateUniqueIds(sweeps, sweepCount, samples, ref nregions);

                // Remap IDs
                RemapRowIDs(row, srcReg, sweeps, sweepCount);
            }

            return (srcReg, nregions);
        }
        /// <summary>
        /// Collects spans for this row
        /// </summary>
        /// <param name="row">Row index</param>
        /// <param name="srcReg">Region id list</param>
        /// <param name="nregions">Number of region ids in the list</param>
        /// <param name="sweeps">Sweep list</param>
        /// <returns>Returns the sample list and the number of sweeps</returns>
        private (int[] samples, int count) CollectRowSpans(int row, int[] srcReg, int nregions, SweepSpan[] sweeps)
        {
            int[] samples = new int[nregions + 1];
            int rid = 1;

            foreach (var (s, i, col) in IterateRowSpans(row))
            {
                if (Areas[i] == AreaTypes.RC_NULL_AREA)
                {
                    continue;
                }

                int previd = 0;

                // -x
                if (s.GetCon(0, out int con))
                {
                    int ai = GetNeighbourCellIndex(col, row, 0, con);
                    int nr = srcReg[ai];
                    if (!IsBorder(nr) && Areas[i] == Areas[ai])
                    {
                        previd = nr;
                    }
                }

                if (previd == 0)
                {
                    previd = rid++;
                    sweeps[previd].Reset();
                }

                // -y
                if (s.GetCon(3, out con))
                {
                    int ai = GetNeighbourCellIndex(col, row, 3, con);
                    int nr = srcReg[ai];
                    if (nr != 0 && !IsBorder(nr) && Areas[i] == Areas[ai])
                    {
                        sweeps[previd].Update(nr, samples);
                    }
                }

                srcReg[i] = previd;
            }

            return (samples, rid);
        }
        /// <summary>
        /// Remaps row ids
        /// </summary>
        /// <param name="row">Row index</param>
        /// <param name="srcReg">Region id list</param>
        /// <param name="sweeps">Sweeps</param>
        /// <param name="sweepCount">Number of sweeps in the list</param>
        private void RemapRowIDs(int row, int[] srcReg, SweepSpan[] sweeps, int sweepCount)
        {
            foreach (var (_, i, _) in IterateRowSpans(row))
            {
                if (srcReg[i] > 0 && srcReg[i] < sweepCount)
                {
                    srcReg[i] = sweeps[srcReg[i]].RegId;
                }
            }
        }

        /// <summary>
        /// Builds regions using watershed method
        /// </summary>
        /// <param name="borderSize">Border size</param>
        /// <param name="minRegionArea">Minimum region area</param>
        /// <param name="mergeRegionArea">Merge region area</param>
        /// <returns>Returns true when the region were correctly built</returns>
        private bool BuildRegionsWatershed(int borderSize, int minRegionArea, int mergeRegionArea)
        {
            var (srcReg, regionId) = BuildSourceRegionsWatershed(borderSize);

            // Merge regions and filter out smalle regions.
            MaxRegions = regionId;
            var merged = MergeAndFilterRegions(minRegionArea, mergeRegionArea, regionId, srcReg, out int[] overlaps, out int maxRegionId);
            MaxRegions = maxRegionId;
            if (!merged)
            {
                return false;
            }

            // If overlapping regions were found during merging, split those regions.
            if (overlaps.Length > 0)
            {
                throw new EngineException($"rcBuildRegions: {overlaps.Length} overlapping regions");
            }

            // Write the result out.
            for (int i = 0; i < SpanCount; ++i)
            {
                Spans[i].Reg = srcReg[i];
            }

            return true;
        }
        /// <summary>
        /// Builds the distance field
        /// </summary>
        private void BuildDistanceField()
        {
            var src = CalculateDistanceField();

            // Blur and Store distance.
            BorderDistances = BoxBlur(1, src);
        }
        /// <summary>
        /// Calculates the distance field
        /// </summary>
        /// <returns>Returns the array of distances</returns>
        public int[] CalculateDistanceField()
        {
            if (SpanCount <= 0)
            {
                return [];
            }

            // Mark boundary cells.
            int[] res = MarkBoundaryCells();

            // Pass 1
            foreach (var (x, y, i, _) in IterateCells())
            {
                res = TestConn(x, y, i, 0, 3, res);
                res = TestConn(x, y, i, 3, 2, res);
            }

            // Pass 2
            foreach (var (x, y, i, _) in IterateCellsReverse())
            {
                res = TestConn(x, y, i, 2, 1, res);
                res = TestConn(x, y, i, 1, 0, res);
            }

            MaxDistance = res.Max();

            return res;
        }
        /// <summary>
        /// Marks boundary cells
        /// </summary>
        private int[] MarkBoundaryCells()
        {
            // Init distance and points.
            int[] res = Helper.CreateArray(SpanCount, int.MaxValue);

            // Mark boundary cells.
            foreach (var (x, y, i, _) in IterateCells())
            {
                var s = Spans[i];
                var area = Areas[i];

                int nc = 0;
                foreach (var item in IterateSpanConnections(s, x, y))
                {
                    if (area == item.area)
                    {
                        nc++;
                    }
                }

                if (nc != 4)
                {
                    res[i] = 0;
                }
            }

            return res;
        }
        /// <summary>
        /// Box blur
        /// </summary>
        /// <param name="thr">Threshold</param>
        /// <param name="src">Distance field</param>
        /// <returns>Returns the blurred distance field</returns>
        private int[] BoxBlur(int thr, int[] src)
        {
            int[] dst = new int[SpanCount];

            thr *= 2;

            foreach (var (x, y, i, _) in IterateCells())
            {
                var cd = src[i];
                if (cd <= thr)
                {
                    dst[i] = cd;
                    continue;
                }

                var s = Spans[i];

                int d = cd;
                for (int dir = 0; dir < 4; ++dir)
                {
                    if (!s.GetCon(dir, out int con))
                    {
                        d += cd * 2;

                        continue;
                    }

                    int ai = GetNeighbourCellIndex(x, y, dir, con, out int ax, out int ay);
                    d += src[ai];

                    var a = Spans[ai];
                    int dir2 = GridUtils.RotateCW(dir);
                    if (!a.GetCon(dir2, out int con2))
                    {
                        d += cd;

                        continue;
                    }

                    int ai2 = GetNeighbourCellIndex(ax, ay, dir2, con2);
                    d += src[ai2];
                }

                dst[i] = (d + 5) / 9;
            }

            return dst;
        }
        /// <summary>
        /// Test connections
        /// </summary>
        private int[] TestConn(int x, int y, int i, int id1, int id2, int[] arr)
        {
            int[] res = [.. arr];

            var s = Spans[i];
            if (!s.GetCon(id1, out int con1))
            {
                return res;
            }

            int ai = GetNeighbourCellIndex(x, y, id1, con1, out int ax, out int ay);
            if (res[ai] + 2 < res[i])
            {
                res[i] = res[ai] + 2;
            }

            var a = Spans[ai];
            if (!a.GetCon(id2, out int con2))
            {
                return res;
            }

            int aai = GetNeighbourCellIndex(ax, ay, id2, con2);
            if (res[aai] + 3 < res[i])
            {
                res[i] = res[aai] + 3;
            }

            return res;
        }

        /// <summary>
        /// Builds regions using monotone method
        /// </summary>
        /// <param name="borderSize">Border size</param>
        /// <param name="minRegionArea">Minimum region area</param>
        /// <param name="mergeRegionArea">Merge region area</param>
        /// <returns>Returns true when the region were correctly built</returns>
        private bool BuildRegionsMonotone(int borderSize, int minRegionArea, int mergeRegionArea)
        {
            var (srcReg, id) = BuildSourceRegions(borderSize);

            // Merge regions and filter out small regions.
            MaxRegions = id;
            var merged = MergeAndFilterRegions(minRegionArea, mergeRegionArea, id, srcReg, out _, out var maxRegionId);
            MaxRegions = maxRegionId;

            if (!merged)
            {
                return false;
            }

            // Monotone partitioning does not generate overlapping regions.

            // Store the result out.
            for (int i = 0; i < SpanCount; ++i)
            {
                Spans[i].Reg = srcReg[i];
            }

            return true;
        }
        /// <summary>
        /// Merge and filter region
        /// </summary>
        private bool MergeAndFilterRegions(int minRegionArea, int mergeRegionSize, int maxRegionId, int[] srcReg, out int[] overlaps, out int maxRegionIdResult)
        {
            // Construct regions
            int nreg = maxRegionId + 1;
            var regions = Region.InitializeRegionList(nreg);

            // Find edge of a region and find connections around the contour.
            foreach (var (col, row, spanIndex, c) in IterateCells())
            {
                int r = srcReg[spanIndex];
                if (r == 0 || r >= nreg)
                {
                    continue;
                }

                var reg = regions[r];

                // Update floors.
                bool foundContour = UpdateFloors(c, spanIndex, r, reg, nreg, srcReg);
                if (foundContour)
                {
                    continue;
                }

                // Check if this cell is next to a border.
                int ndir = GetCellBorderDirection(col, row, spanIndex, srcReg);
                if (ndir == -1)
                {
                    continue;
                }

                // The cell is at border.
                // Walk around the contour to find all the neighbours.
                reg.AddConnections(FindNeighbours(col, row, spanIndex, ndir, srcReg));
            }

            // Remove too small regions.
            Region.RemoveSmallestRegions(regions, minRegionArea);

            // Merge too small regions to neighbour regions.
            Region.MergeSmallRegionsToNeighbours(regions, mergeRegionSize);

            // Compress region Ids.
            maxRegionIdResult = Region.CompressRegionIds(regions);

            // Remap regions.
            Region.RemapRegions(regions, srcReg, SpanCount);

            // Return regions that we found to be overlapping.
            overlaps = Region.GetOverlapingRegions(regions);

            return true;
        }
        /// <summary>
        /// Updates the region floors with specified cell spans
        /// </summary>
        /// <param name="c">Compact cell</param>
        /// <param name="spanIndex">Current span index</param>
        /// <param name="spanRegionId">Span region id</param>
        /// <param name="reg">Region update</param>
        /// <param name="regionCount">Total region count</param>
        /// <param name="srcReg">Region id list</param>
        /// <returns>Returns whether have found contour or not</returns>
        private bool UpdateFloors(CompactCell c, int spanIndex, int spanRegionId, Region reg, int regionCount, int[] srcReg)
        {
            reg.SpanCount++;

            // Update floors.
            for (int i = c.Index; i < c.Index + c.Count; ++i)
            {
                if (spanIndex == i)
                {
                    continue;
                }

                int floorId = srcReg[i];
                if (floorId == 0 || floorId >= regionCount)
                {
                    continue;
                }

                if (floorId == spanRegionId)
                {
                    reg.Overlap = true;
                }

                reg.AddUniqueFloorRegion(floorId);
            }

            // Have found contour
            if (reg.GetConnectionCount() > 0)
            {
                return true;
            }

            reg.AreaType = Areas[spanIndex];

            return false;
        }
        /// <summary>
        /// Checks if this cell is next to a border. 
        /// </summary>
        /// <param name="col">Column</param>
        /// <param name="row">Row</param>
        /// <param name="spanIndex">Span index</param>
        /// <param name="srcReg">Region id list</param>
        /// <returns>Returns the border direction</returns>
        private int GetCellBorderDirection(int col, int row, int spanIndex, int[] srcReg)
        {
            for (int dir = 0; dir < 4; ++dir)
            {
                if (IsSolidEdge(col, row, spanIndex, dir, srcReg))
                {
                    return dir;
                }
            }

            return -1;
        }

        /// <summary>
        /// Builds regions using layers method
        /// </summary>
        /// <param name="borderSize">Border size</param>
        /// <param name="minRegionArea">Minimum region area</param>
        /// <returns>Returns true when the region were correctly built</returns>
        private bool BuildRegionsLayer(int borderSize, int minRegionArea)
        {
            var (srcReg, id) = BuildSourceRegions(borderSize);

            // Merge monotone regions to layers and remove small regions.
            MaxRegions = id;
            var merged = MergeAndFilterLayerRegions(minRegionArea, id, srcReg, out int maxRegionId);
            MaxRegions = maxRegionId;
            if (!merged)
            {
                return false;
            }

            // Store the result out.
            for (int i = 0; i < SpanCount; ++i)
            {
                Spans[i].Reg = srcReg[i];
            }

            return true;
        }
        /// <summary>
        /// Merge and filter layer regions
        /// </summary>
        private bool MergeAndFilterLayerRegions(int minRegionArea, int maxRegionId, int[] srcReg, out int maxRegionIdResult)
        {
            int nreg = maxRegionId + 1;
            var regions = Region.InitializeRegionList(nreg);

            // Find region neighbours and overlapping regions.
            var regs = new List<int>(32);
            foreach (var (x, y) in GridUtils.Iterate(Width, Height))
            {
                regs.Clear();

                foreach (var (s, i) in IterateCellSpans(x, y))
                {
                    int ri = srcReg[i];
                    if (ri == 0 || ri >= nreg)
                    {
                        continue;
                    }

                    // Collect all region layers.
                    regs.Add(ri);

                    // Update neighbours
                    UpdateSpanNeighbours(s, x, y, regions, nreg, ri, srcReg);
                }

                // Update overlapping regions.
                Region.UpdateOverlappingRegionFloors(regions, regs);
            }

            // Merges montone regions to create non-overlapping areas.
            Region.MergeMonotoneRegions(regions);

            // Remove small regions
            Region.RemoveSmallRegions(regions, minRegionArea);

            // Compress region Ids.
            maxRegionIdResult = Region.CompressRegionIds(regions);

            // Remap regions.
            Region.RemapRegions(regions, srcReg, SpanCount);

            return true;
        }
        /// <summary>
        /// Updates span neighbours
        /// </summary>
        /// <param name="s">Span</param>
        /// <param name="col">X coordinate</param>
        /// <param name="row">Y coordinate</param>
        /// <param name="regions">Region list</param>
        /// <param name="maxRegions">Maximum number of regions in the list</param>
        /// <param name="regionId">Region id</param>
        /// <param name="srcReg">Region id list</param>
        private void UpdateSpanNeighbours(CompactSpan s, int col, int row, List<Region> regions, int maxRegions, int regionId, int[] srcReg)
        {
            var reg = regions[regionId];
            reg.SpanCount++;
            reg.YMin = Math.Min(reg.YMin, s.Y);
            reg.YMax = Math.Max(reg.YMax, s.Y);

            foreach (var item in IterateSpanConnections(s, col, row))
            {
                int r = srcReg[item.ai];

                if (r > 0 && r < maxRegions && r != regionId)
                {
                    reg.AddUniqueConnection(r);
                }

                if (IsBorder(r))
                {
                    reg.ConnectsToBorder = true;
                }
            }
        }

        /// <summary>
        /// Initialize flags
        /// </summary>
        public int[] InitializeFlags()
        {
            int[] flags = new int[SpanCount];

            int w = Width;
            int h = Height;

            // Mark boundaries.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    InitializeCellFlags(x, y, flags);
                }
            }

            return flags;
        }
        /// <summary>
        /// Initialize cell flags
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="flags">Flags to update</param>
        private void InitializeCellFlags(int x, int y, int[] flags)
        {
            foreach (var (s, i) in IterateCellSpans(x, y))
            {
                if (s.Reg == 0 || IsBorder(s.Reg))
                {
                    flags[i] = 0;
                    continue;
                }

                int res = 0;
                for (int dir = 0; dir < 4; ++dir)
                {
                    int r = 0;
                    if (s.GetCon(dir, out int con))
                    {
                        int ai = GetNeighbourCellIndex(x, y, dir, con);
                        r = Spans[ai].Reg;
                    }

                    if (r == s.Reg)
                    {
                        res |= 1 << dir;
                    }
                }

                flags[i] = res ^ Contour.RC_PORTAL_FLAG; // Inverse, mark non connected edges.
            }
        }

        /// <summary>
        /// Adds a new compact cell
        /// </summary>
        /// <param name="x">X cell coordinate</param>
        /// <param name="y">Y cell coordinate</param>
        /// <param name="flags">Flags to update</param>
        public (int Reg, AreaTypes Area, ContourVertex[] RawVerts)[] BuildCompactCells(int x, int y, int[] flags)
        {
            List<(int Reg, AreaTypes Area, ContourVertex[] RawVerts)> res = [];

            foreach (var (s, i) in IterateCellSpans(x, y))
            {
                if (flags[i] == 0 || flags[i] == Contour.RC_PORTAL_FLAG)
                {
                    flags[i] = 0;
                    continue;
                }

                int reg = s.Reg;
                if (reg == 0 || IsBorder(reg))
                {
                    continue;
                }

                var area = Areas[i];
                var verts = WalkContour(x, y, i, flags);

                res.Add((reg, area, verts));
            }

            return [.. res];
        }
        /// <summary>
        /// Walks the contour to find neighbours
        /// </summary>
        /// <param name="col">X coordinate</param>
        /// <param name="row">Y coordinate</param>
        /// <param name="spanIndex">Span index</param>
        /// <param name="dir">Direction</param>
        /// <param name="srcReg">Region list</param>
        /// <returns>Returns the neighbour list</returns>
        private int[] FindNeighbours(int col, int row, int spanIndex, int dir, int[] srcReg)
        {
            List<int> cont = [];

            int startIdx = spanIndex;
            int startDir = dir;

            int curReg = 0;
            int ai = GetNeighbourCellIndex(Spans[spanIndex], col, row, dir);
            if (ai != -1)
            {
                curReg = srcReg[ai];
            }
            cont.Add(curReg);

            int iter = 0;
            int iterIdx = spanIndex;
            int iterDir = dir;
            while (++iter < 40000)
            {
                var (iterateNext, newReg) = IterateSpan(ref iterIdx, ref iterDir, ref col, ref row, srcReg);
                if (!iterateNext)
                {
                    // Should not happen.
                    return [];
                }

                if (newReg == -1)
                {
                    continue;
                }

                curReg = newReg;
                cont.Add(curReg);

                if (startIdx == iterIdx && startDir == iterDir)
                {
                    break;
                }
            }

            return [.. cont];
        }
        /// <summary>
        /// Iterates over the specified span
        /// </summary>
        /// <param name="iterIdx">Span index</param>
        /// <param name="iterDir">Span direction</param>
        /// <param name="col">Column</param>
        /// <param name="row">Row</param>
        /// <param name="srcReg">Region id list</param>
        /// <returns>Returns whether the iteration must continue and the new region id</returns>
        private (bool Continue, int NewReg) IterateSpan(ref int iterIdx, ref int iterDir, ref int col, ref int row, int[] srcReg)
        {
            var s = Spans[iterIdx];

            if (IsSolidEdge(col, row, iterIdx, iterDir, srcReg))
            {
                // Choose the edge corner
                int r = 0;
                int ai = GetNeighbourCellIndex(s, col, row, iterDir);
                if (ai != -1)
                {
                    r = srcReg[ai];
                }

                iterDir = GridUtils.RotateCW(iterDir);  // Rotate CW

                return (true, r);
            }
            else
            {
                int ai = GetNeighbourCellIndex(s, col, row, iterDir, out int ax, out int ay);
                if (ai == -1)
                {
                    // Should not happen.
                    return (false, -1);
                }

                col = ax;
                row = ay;
                iterIdx = ai;

                iterDir = GridUtils.RotateCCW(iterDir);  // Rotate CCW

                return (true, -1);
            }
        }
        /// <summary>
        /// Walks the edge contour
        /// </summary>
        /// <param name="col">X coordinate</param>
        /// <param name="row">Y coordinate</param>
        /// <param name="spanIndex">Span index</param>
        /// <param name="flags">Edge flags</param>
        /// <returns>Returns the edge contour list</returns>
        private ContourVertex[] WalkContour(int col, int row, int spanIndex, int[] flags)
        {
            List<ContourVertex> points = [];

            // Choose the first non-connected edge
            int dir = 0;
            while ((flags[spanIndex] & (1 << dir)) == 0)
            {
                dir++;
            }

            var area = Areas[spanIndex];

            int startDir = dir;
            int startIdx = spanIndex;

            int iter = 0;
            int iterDir = dir;
            int iterIdx = spanIndex;
            while (++iter < 40000)
            {
                if ((flags[iterIdx] & (1 << iterDir)) != 0)
                {
                    // Choose the edge corner
                    var pt = GetEdgeCorner(col, row, iterIdx, iterDir, area);
                    points.Add(pt);

                    flags[iterIdx] &= ~(1 << iterDir); // Remove visited edges

                    iterDir = GridUtils.RotateCW(iterDir);  // Rotate CW
                }
                else
                {
                    var s = Spans[iterIdx];
                    int ni = GetNeighbourCellIndex(s, col, row, iterDir, out int nx, out int ny);
                    if (ni == -1)
                    {
                        // Should not happen.
                        return [];
                    }

                    col = nx;
                    row = ny;
                    iterIdx = ni;

                    iterDir = GridUtils.RotateCCW(iterDir);  // Rotate CCW
                }

                if (startIdx == iterIdx && startDir == iterDir)
                {
                    break;
                }
            }

            return [.. points];
        }
        /// <summary>
        /// Gets the corner edge
        /// </summary>
        /// <param name="col">X coordinate</param>
        /// <param name="row">Y coordinate</param>
        /// <param name="spanIndex">Span index</param>
        /// <param name="dir">Direction</param>
        /// <param name="area">Area type</param>
        private ContourVertex GetEdgeCorner(int col, int row, int spanIndex, int dir, AreaTypes area)
        {
            bool isAreaBorder = false;
            int px = col;
            int py = GetCornerHeight(col, row, spanIndex, dir, out bool isBorderVertex);
            int pz = row;
            switch (dir)
            {
                case 0: pz++; break;
                case 1: px++; pz++; break;
                case 2: px++; break;
            }

            int r = 0;

            if (Spans[spanIndex].GetCon(dir, out int con))
            {
                int ai = GetNeighbourCellIndex(col, row, dir, con);
                r = Spans[ai].Reg;
                if (area != Areas[ai])
                {
                    isAreaBorder = true;
                }
            }

            if (isBorderVertex)
            {
                r |= Contour.RC_BORDER_VERTEX;
            }
            if (isAreaBorder)
            {
                r |= Contour.RC_AREA_BORDER;
            }

            return new(px, py, pz, r);
        }

        /// <summary>
        /// Gets the neighbour cell index in the cells array, in the specified direction and connection
        /// </summary>
        /// <param name="col">X coordinate</param>
        /// <param name="row">Y coordinate</param>
        /// <param name="dir">Direction</param>
        /// <param name="con">Connection</param>
        public int GetNeighbourCellIndex(int col, int row, int dir, int con)
        {
            return GetNeighbourCellIndex(col, row, dir, con, out _, out _);
        }
        /// <summary>
        /// Gets the neighbour cell index in the cells array, in the specified direction and connection
        /// </summary>
        /// <param name="col">X coordinate</param>
        /// <param name="row">Y coordinate</param>
        /// <param name="dir">Direction</param>
        /// <param name="con">Connection</param>
        /// <param name="ax">Neighbour cell x coordinate</param>
        /// <param name="ay">Neighbour cell y coordinate</param>
        public int GetNeighbourCellIndex(int col, int row, int dir, int con, out int ax, out int ay)
        {
            ax = col + GridUtils.GetDirOffsetX(dir);
            ay = row + GridUtils.GetDirOffsetY(dir);
            return Cells[ax + ay * Width].Index + con;
        }
        /// <summary>
        /// Gets the neighbour cell index in the cells array in the specified direction
        /// </summary>
        /// <param name="s">Compact span</param>
        /// <param name="col">X coordinate</param>
        /// <param name="row">Y coordinate</param>
        /// <param name="dir">Direction</param>
        public int GetNeighbourCellIndex(CompactSpan s, int col, int row, int dir)
        {
            return GetNeighbourCellIndex(s, col, row, dir, out _, out _);
        }
        /// <summary>
        /// Gets the neighbour cell index in the cells array in the specified direction
        /// </summary>
        /// <param name="s">Compact span</param>
        /// <param name="col">X coordinate</param>
        /// <param name="row">Y coordinate</param>
        /// <param name="dir">Direction</param>
        /// <param name="ax">Neighbour cell x coordinate</param>
        /// <param name="ay">Neighbour cell y coordinate</param>
        public int GetNeighbourCellIndex(CompactSpan s, int col, int row, int dir, out int ax, out int ay)
        {
            ax = col + GridUtils.GetDirOffsetX(dir);
            ay = row + GridUtils.GetDirOffsetY(dir);

            if (s.GetCon(dir, out int con))
            {
                return Cells[ax + ay * Width].Index + con;
            }

            return -1;
        }
    }
}
