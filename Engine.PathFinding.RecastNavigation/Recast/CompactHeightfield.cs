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
        /// Null neighbour index
        /// </summary>
        const int RC_NULL_NEI = -1;
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
        /// The value returned by #rcGetCon if the specified direction is not connected
        /// to another span. (Has no neighbor.)
        /// </summary>
        public const int RC_NOT_CONNECTED = 0x3f;

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

        #region Iterators

        /// <summary>
        /// Enumerates the specified compact cell list
        /// </summary>
        /// <param name="cells">Compact cell list</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <returns>Returns each compact cell to evaulate</returns>
        private static IEnumerable<(int x, int y, int i, CompactCell c)> IterateCells(IEnumerable<CompactCell> cells, int w, int h)
        {
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = cells.ElementAt(x + y * w);

                    for (int i = c.Index, ni = c.Index + c.Count; i < ni; ++i)
                    {
                        yield return (x, y, i, c);
                    }
                }
            }
        }
        /// <summary>
        /// Enumerates the specified compact cell list in reverse order
        /// </summary>
        /// <param name="cells">Compact cell list</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <returns>Returns each compact cell to evaulate</returns>
        private static IEnumerable<(int x, int y, int i, CompactCell c)> IterateCellsReverse(IEnumerable<CompactCell> cells, int w, int h)
        {
            for (int y = h - 1; y >= 0; --y)
            {
                for (int x = w - 1; x >= 0; --x)
                {
                    var c = cells.ElementAt(x + y * w);

                    for (int i = c.Index, ni = c.Index + c.Count; i < ni; ++i)
                    {
                        yield return (x, y, i, c);
                    }
                }
            }
        }
        /// <summary>
        /// Enumerates the specified compact cell list, if is contained into the bounds
        /// </summary>
        /// <param name="cells">Compact cell list</param>
        /// <param name="w">Width</param>
        /// <param name="minx">Minimum x bound's coordinate</param>
        /// <param name="miny">Minimum y bound's coordinate</param>
        /// <param name="maxx">Maximum x bound's coordinate</param>
        /// <param name="maxy">Maximum y bound's coordinate</param>
        /// <returns>Returns each compact cell to evaulate</returns>
        private static IEnumerable<(int x, int y, int i, CompactCell c)> IterateCellsBounds(IEnumerable<CompactCell> cells, int w, int minx, int miny, int maxx, int maxy)
        {
            for (int y = miny; y < maxy; ++y)
            {
                for (int x = minx; x < maxx; ++x)
                {
                    var c = cells.ElementAt(x + y * w);

                    for (int i = c.Index, ni = c.Index + c.Count; i < ni; ++i)
                    {
                        yield return (x, y, i, c);
                    }
                }
            }
        }
        /// <summary>
        /// Enumerates the specified compact cell list
        /// </summary>
        /// <param name="cells">Compact cell list</param>
        /// <param name="w">Width</param>
        /// <param name="hp">Height patch</param>
        /// <param name="borderSize">Border size</param>
        /// <returns>Returns each compact cell to evaulate</returns>
        private static IEnumerable<(int x, int y, CompactCell cell, int hx, int hy)> IterateHeightPatch(IEnumerable<CompactCell> cells, int w, HeightPatch hp, int borderSize)
        {
            for (int hy = 0; hy < hp.Bounds.Height; hy++)
            {
                int y = hp.Bounds.Y + hy + borderSize;
                for (int hx = 0; hx < hp.Bounds.Width; hx++)
                {
                    int x = hp.Bounds.X + hx + borderSize;
                    var c = cells.ElementAt(x + y * w);

                    yield return (x, y, c, hx, hy);
                }
            }
        }
        private static IEnumerable<(int x, int y, Span span)> IterateSpans(Span[] spans, int w, int h)
        {
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var span = spans[x + y * w];

                    // If there are no spans at this cell, just leave the data to index=0, count=0.
                    if (span == null)
                    {
                        continue;
                    }

                    yield return (x, y, span);
                }
            }
        }
        private static IEnumerable<int> IterateSpanConnections(CompactSpan cs)
        {
            for (int dir = 0; dir < 4; dir++)
            {
                if (cs.GetCon(dir) == RC_NOT_CONNECTED)
                {
                    continue;
                }

                yield return dir;
            }
        }

        #endregion

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
            var dstStack = new List<LevelStackEntry>();

            foreach (var stack in srcStack)
            {
                int i = stack.Index;
                if ((i < 0) || (srcReg[i] != 0))
                {
                    continue;
                }

                dstStack.Add(stack);
            }

            return dstStack.ToArray();
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
            if (Math.Abs(rb.X - ra.X) < 1e-6f)
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
            float sampleMaxError = param.SampleMaxError;

            int[] idx = new int[MAX_VERTS_PER_EDGE];
            idx[0] = 0;
            idx[1] = nn;
            int nidx = 2;
            for (int k = 0; k < nidx - 1;)
            {
                int a = idx[k];
                int b = idx[k + 1];
                var va = edgeSamples[a];
                var vb = edgeSamples[b];

                // Find maximum deviation along the segment.
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

                // If the max deviation is larger than accepted error,
                // add new point, else continue to next segment.
                if (maxi != -1 && maxd > (sampleMaxError * sampleMaxError))
                {
                    for (int m = nidx; m > k; --m)
                    {
                        idx[m] = idx[m - 1];
                    }
                    idx[k + 1] = maxi;
                    nidx++;
                }
                else
                {
                    ++k;
                }
            }

            return idx.Take(nidx).ToArray();
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
        /// <param name="spans">Heightfield span list</param>
        /// <param name="spanCount">Heightfield span count</param>
        public void FillCellsAndSpans(Span[] spans, int spanCount)
        {
            Cells = new CompactCell[Width * Height];
            Spans = new CompactSpan[spanCount];
            Areas = new AreaTypes[spanCount];

            // Fill in cells and spans.
            int idx = 0;
            foreach (var (x, y, span) in IterateSpans(spans, Width, Height))
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
                        int top = s.Next != null ? s.Next.SMin : int.MaxValue;
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
            int maxLayers = RC_NOT_CONNECTED - 1;
            int tooHighNeighbour = 0;
            foreach (var (x, y, i, _) in IterateCells(Cells, Width, Height))
            {
                Spans[i] = FindConnections(x, y, Spans[i], maxLayers, ref tooHighNeighbour);
            }

            if (tooHighNeighbour > maxLayers)
            {
                throw new EngineException(string.Format("Heightfield has too many layers {0} (max: {1})", tooHighNeighbour, maxLayers));
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
                s.SetCon(dir, RC_NOT_CONNECTED);
                int nx = x + Utils.GetDirOffsetX(dir);
                int ny = y + Utils.GetDirOffsetY(dir);

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
            var vCount = geometry.GetAreaCount();
            if (vCount == 0)
            {
                return;
            }

            var vols = geometry.GetAreas();
            for (int i = 0; i < vCount; i++)
            {
                var vol = vols[i];

                if (vol is IGraphAreaPolygon polyArea)
                {
                    MarkConvexPolyArea(
                        polyArea.Vertices.ToArray(),
                        polyArea.MinHeight, polyArea.MaxHeight,
                        (AreaTypes)polyArea.AreaType);
                }
                else if (vol is IGraphAreaCylinder cylinderArea)
                {
                    MarkCylinderArea(
                        cylinderArea.Position,
                        cylinderArea.Radius,
                        cylinderArea.Height,
                        (AreaTypes)cylinderArea.AreaType);
                }
                else if (vol is IGraphAreaBox boxArea)
                {
                    MarkBoxArea(
                        boxArea.BMin,
                        boxArea.BMax,
                        (AreaTypes)boxArea.AreaType);
                }
            }

            MedianFilterWalkableArea();
        }
        /// <summary>
        /// Gets the area bounds
        /// </summary>
        /// <param name="bmin">Area min</param>
        /// <param name="bmax">Area max</param>
        /// <param name="min">Resulting bounds min</param>
        /// <param name="max">Resulting bounds max</param>
        private void GetAreaBounds(Vector3 bmin, Vector3 bmax, out Int3 min, out Int3 max)
        {
            min = new Int3();
            max = new Int3();

            min.X = (int)((bmin.X - BoundingBox.Minimum.X) / CellSize);
            min.Y = (int)((bmin.Y - BoundingBox.Minimum.Y) / CellHeight);
            min.Z = (int)((bmin.Z - BoundingBox.Minimum.Z) / CellSize);
            max.X = (int)((bmax.X - BoundingBox.Minimum.X) / CellSize);
            max.Y = (int)((bmax.Y - BoundingBox.Minimum.Y) / CellHeight);
            max.Z = (int)((bmax.Z - BoundingBox.Minimum.Z) / CellSize);

            if (max.X < 0) return;
            if (min.X >= Width) return;
            if (max.Z < 0) return;
            if (min.Z >= Height) return;

            min.X = Math.Max(0, min.X);
            max.X = Math.Min(Width, max.X);
            min.Z = Math.Max(0, min.Z);
            max.Z = Math.Min(Height, max.Z);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmin"></param>
        /// <param name="bmax"></param>
        /// <param name="areaId"></param>
        /// <param name="chf"></param>
        /// <remarks>
        /// The value of spacial parameters are in world units.
        /// </remarks>
        private void MarkBoxArea(Vector3 bmin, Vector3 bmax, AreaTypes areaId)
        {
            GetAreaBounds(bmin, bmax, out var min, out var max);

            foreach (var (_, _, i, _) in IterateCellsBounds(Cells, Width, min.X, min.Z, max.X, max.Z))
            {
                if (Areas[i] == AreaTypes.RC_NULL_AREA)
                {
                    continue;
                }

                var sy = Spans[i].Y;
                if (sy >= min.Y && sy <= max.Y)
                {
                    Areas[i] = areaId;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon">Polygon vertices</param>
        /// <param name="hmin">Minimum height</param>
        /// <param name="hmax">Maximum height</param>
        /// <param name="areaId">Area id</param>
        /// <remarks>
        /// The value of spacial parameters are in world units.
        /// The y-values of the polygon vertices are ignored. So the polygon is effectively projected onto the xz-plane at hmin, then extruded to hmax.
        /// </remarks>
        private void MarkConvexPolyArea(Vector3[] polygon, float hmin, float hmax, AreaTypes areaId)
        {
            Utils.GetPolygonBounds(polygon, out var bmin, out var bmax);
            bmin.Y = hmin;
            bmax.Y = hmax;

            GetAreaBounds(bmin, bmax, out var min, out var max);

            foreach (var (x, z, i, _) in IterateCellsBounds(Cells, Width, min.X, min.Z, max.X, max.Z))
            {
                if (Areas[i] == AreaTypes.RC_NULL_AREA)
                {
                    continue;
                }

                var sy = Spans[i].Y;
                if (sy >= min.Y && sy <= max.Y)
                {
                    var p = new Vector3
                    {
                        X = BoundingBox.Minimum.X + (x + 0.5f) * CellSize,
                        Y = 0,
                        Z = BoundingBox.Minimum.Z + (z + 0.5f) * CellSize
                    };

                    if (Utils.PointInPolygon2D(p, polygon))
                    {
                        Areas[i] = areaId;
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="r"></param>
        /// <param name="h"></param>
        /// <param name="areaId"></param>
        /// <param name="chf"></param>
        /// <remarks>
        /// The value of spacial parameters are in world units.
        /// </remarks>
        private void MarkCylinderArea(Vector3 pos, float r, float h, AreaTypes areaId)
        {
            Utils.GetCylinderBounds(pos, r, h, out var bmin, out var bmax);
            float r2 = r * r;

            GetAreaBounds(bmin, bmax, out var min, out var max);

            foreach (var (x, z, i, _) in IterateCellsBounds(Cells, Width, min.X, min.Z, max.X, max.Z))
            {
                if (Areas[i] == AreaTypes.RC_NULL_AREA)
                {
                    continue;
                }

                var sy = Spans[i].Y;
                if (sy >= min.Y && sy <= max.Y)
                {
                    float sx = BoundingBox.Minimum.X + (x + 0.5f) * CellSize;
                    float sz = BoundingBox.Minimum.Z + (z + 0.5f) * CellSize;
                    float dx = sx - pos.X;
                    float dz = sz - pos.Z;

                    if (dx * dx + dz * dz < r2)
                    {
                        Areas[i] = areaId;
                    }
                }
            }
        }
        /// <summary>
        /// This filter is usually applied after applying area id's using functions such as MarkBoxArea, MarkConvexPolyArea, and MarkCylinderArea.
        /// </summary>
        /// <returns>Returns always true</returns>
        private void MedianFilterWalkableArea()
        {
            int w = Width;
            int h = Height;

            // Init distance.
            var areas = Helper.CreateArray(SpanCount, (AreaTypes)0xff);

            foreach (var (x, y, i, _) in IterateCells(Cells, w, h))
            {
                if (Areas[i] == AreaTypes.RC_NULL_AREA)
                {
                    areas[i] = Areas[i];
                    continue;
                }

                var nei = Helper.CreateArray(9, Areas[i]);

                var s = Spans[i];

                for (int dir = 0; dir < 4; ++dir)
                {
                    if (s.GetCon(dir) == RC_NOT_CONNECTED)
                    {
                        continue;
                    }

                    int ax = x + Utils.GetDirOffsetX(dir);
                    int ay = y + Utils.GetDirOffsetY(dir);
                    int ai = Cells[ax + ay * w].Index + s.GetCon(dir);
                    if (Areas[ai] != AreaTypes.RC_NULL_AREA)
                    {
                        nei[dir * 2 + 0] = Areas[ai];
                    }

                    var a = Spans[ai];
                    int dir2 = (dir + 1) & 0x3;
                    if (a.GetCon(dir2) == RC_NOT_CONNECTED)
                    {
                        continue;
                    }

                    int ax2 = ax + Utils.GetDirOffsetX(dir2);
                    int ay2 = ay + Utils.GetDirOffsetY(dir2);
                    int ai2 = Cells[ax2 + ay2 * w].Index + a.GetCon(dir2);
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
            int w = Width;
            int h = Height;

            // Init distance.
            int[] dist = MarkBoundaryCellsNotNullAreas();

            // Pass 1
            foreach (var (x, y, i, _) in IterateCells(Cells, w, h))
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
            foreach (var (x, y, i, _) in IterateCellsReverse(Cells, w, h))
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

            if (s.GetCon(dir1) == RC_NOT_CONNECTED)
            {
                return false;
            }

            bool updated = false;
            int nd;

            int ax = x + Utils.GetDirOffsetX(dir1);
            int ay = y + Utils.GetDirOffsetY(dir1);
            int ai = Cells[ax + ay * Width].Index + s.GetCon(dir1);
            var asp = Spans[ai];
            nd = Math.Min(dist[ai] + 2, 255);
            if (nd < d)
            {
                d = nd;
                updated = true;
            }

            if (asp.GetCon(dir2) == RC_NOT_CONNECTED)
            {
                return updated;
            }

            int aax = ax + Utils.GetDirOffsetX(dir2);
            int aay = ay + Utils.GetDirOffsetY(dir2);
            int aai = Cells[aax + aay * Width].Index + asp.GetCon(dir2);
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
            foreach (var (x, y, i, _) in IterateCells(Cells, Width, Height))
            {
                if (Areas[i] == AreaTypes.RC_NULL_AREA)
                {
                    dist[i] = 0;
                    continue;
                }

                var s = Spans[i];
                int nc = 0;
                for (int dir = 0; dir < 4; ++dir)
                {
                    if (s.GetCon(dir) == RC_NOT_CONNECTED)
                    {
                        continue;
                    }

                    int nx = x + Utils.GetDirOffsetX(dir);
                    int ny = y + Utils.GetDirOffsetY(dir);
                    int nidx = Cells[nx + ny * Width].Index + s.GetCon(dir);
                    if (Areas[nidx] != AreaTypes.RC_NULL_AREA)
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
        /// <param name="srcReg">Region list</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="i">Index</param>
        /// <param name="dir">Direction</param>
        /// <returns>Returns true if the specified edge is solid</returns>
        private bool IsSolidEdge(int[] srcReg, int x, int y, int i, int dir)
        {
            var s = Spans[i];
            int r = 0;
            if (s.GetCon(dir) != RC_NOT_CONNECTED)
            {
                int ax = x + Utils.GetDirOffsetX(dir);
                int ay = y + Utils.GetDirOffsetY(dir);
                int ai = Cells[ax + ay * Width].Index + s.GetCon(dir);
                r = srcReg[ai];
            }

            if (r == srcReg[i])
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
            isBorderVertex = false;

            var s = Spans[i];
            int ch = s.Y;
            int dirp = (dir + 1) & 0x3;

            int[] regs = { 0, 0, 0, 0 };

            // Combine region and area codes in order to prevent
            // border vertices which are in between two areas to be removed.
            regs[0] = Spans[i].Reg | ((int)Areas[i] << 16);

            if (s.GetCon(dir) != RC_NOT_CONNECTED)
            {
                int ax = x + Utils.GetDirOffsetX(dir);
                int ay = y + Utils.GetDirOffsetY(dir);
                int ai = Cells[ax + ay * Width].Index + s.GetCon(dir);
                var a = Spans[ai];
                ch = Math.Max(ch, a.Y);
                regs[1] = Spans[ai].Reg | ((int)Areas[ai] << 16);
                if (a.GetCon(dirp) != RC_NOT_CONNECTED)
                {
                    int ax2 = ax + Utils.GetDirOffsetX(dirp);
                    int ay2 = ay + Utils.GetDirOffsetY(dirp);
                    int ai2 = Cells[ax2 + ay2 * Width].Index + a.GetCon(dirp);
                    var as2 = Spans[ai2];
                    ch = Math.Max(ch, as2.Y);
                    regs[2] = Spans[ai2].Reg | ((int)Areas[ai2] << 16);
                }
            }
            if (s.GetCon(dirp) != RC_NOT_CONNECTED)
            {
                int ax = x + Utils.GetDirOffsetX(dirp);
                int ay = y + Utils.GetDirOffsetY(dirp);
                int ai = Cells[ax + ay * Width].Index + s.GetCon(dirp);
                var a = Spans[ai];
                ch = Math.Max(ch, a.Y);
                regs[3] = Spans[ai].Reg | ((int)Areas[ai] << 16);
                if (a.GetCon(dir) != RC_NOT_CONNECTED)
                {
                    int ax2 = ax + Utils.GetDirOffsetX(dir);
                    int ay2 = ay + Utils.GetDirOffsetY(dir);
                    int ai2 = Cells[ax2 + ay2 * Width].Index + a.GetCon(dir);
                    var as2 = Spans[ai2];
                    ch = Math.Max(ch, as2.Y);
                    regs[2] = Spans[ai2].Reg | ((int)Areas[ai2] << 16);
                }
            }

            // Check if the vertex is special edge vertex, these vertices will be removed later.
            for (int j = 0; j < 4; ++j)
            {
                int a = j;
                int b = (j + 1) & 0x3;
                int c = (j + 2) & 0x3;
                int d = (j + 3) & 0x3;

                // The vertex is a border vertex there are two same exterior cells in a row,
                // followed by two interior cells and none of the regions are out of bounds.
                bool twoSameExts = (regs[a] & regs[b] & RC_BORDER_REG) != 0 && regs[a] == regs[b];
                bool twoInts = ((regs[c] | regs[d]) & RC_BORDER_REG) == 0;
                bool intsSameArea = (regs[c] >> 16) == (regs[d] >> 16);
                bool noZeros = regs[a] != 0 && regs[b] != 0 && regs[c] != 0 && regs[d] != 0;
                if (twoSameExts && twoInts && intsSameArea && noZeros)
                {
                    isBorderVertex = true;
                    break;
                }
            }

            return ch;
        }

        /// <summary>
        /// Gets the height data
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="verts">Vertex indices</param>
        /// <param name="borderSize">Border size</param>
        /// <param name="hp">Height patch</param>
        /// <param name="region">Region index</param>
        /// <remarks>
        /// Reads to the compact heightfield are offset by border size, since border size offset is already removed from the polymesh vertices.
        /// </remarks>
        public void GetHeightData(IndexedPolygon poly, Int3[] verts, HeightPatch hp, int borderSize, int region)
        {
            var queue = new List<HeightDataItem>(512);

            // Set all heights to RC_UNSET_HEIGHT.
            hp.Data = Helper.CreateArray(hp.Bounds.Width * hp.Bounds.Height, HeightPatch.RC_UNSET_HEIGHT);

            bool empty = true;

            // We cannot sample from this poly if it was created from polys
            // of different regions. If it was then it could potentially be overlapping
            // with polys of that region and the heights sampled here could be wrong.
            if (!IndexedPolygon.HasMultipleRegions(region))
            {
                empty = GetHeightDataFromMultipleRegions(hp, borderSize, region, out var qItems);

                queue.AddRange(qItems);
            }

            // if the polygon does not contain any points from the current region (rare, but happens)
            // or if it could potentially be overlapping polygons of the same region,
            // then use the center as the seed point.
            if (empty)
            {
                queue.AddRange(SeedArrayWithPolyCenter(poly, verts, borderSize, hp));
            }

            ProcessHeightDataQueue(hp, borderSize, queue);
        }
        /// <summary>
        /// Gets the heigth data of the specified region
        /// </summary>
        /// <param name="hp">Height patch</param>
        /// <param name="borderSize">Border size</param>
        /// <param name="region">Region id</param>
        /// <param name="dataItems">Resulting data item list</param>
        /// <returns>Returns true if the resulting heigth data is empty</returns>
        private bool GetHeightDataFromMultipleRegions(HeightPatch hp, int borderSize, int region, out HeightDataItem[] dataItems)
        {
            bool empty = true;

            var queue = new List<HeightDataItem>();

            // Copy the height from the same region, and mark region borders
            // as seed points to fill the rest.
            foreach (var (x, y, cell, hx, hy) in IterateHeightPatch(Cells, Width, hp, borderSize))
            {
                for (int i = cell.Index, ni = cell.Index + cell.Count; i < ni; ++i)
                {
                    var s = Spans[i];
                    if (s.Reg != region)
                    {
                        continue;
                    }

                    // Store height
                    hp.Data[hx + hy * hp.Bounds.Width] = s.Y;
                    empty = false;

                    // If any of the neighbours is not in same region,
                    // add the current location as flood fill start
                    bool border = DetectBorder(x, y, region, s);
                    if (border)
                    {
                        queue.Add(new HeightDataItem { X = x, Y = y, I = i });
                    }

                    break;
                }
            }

            dataItems = queue.ToArray();

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
            for (int dir = 0; dir < 4; ++dir)
            {
                if (s.GetCon(dir) == RC_NOT_CONNECTED)
                {
                    continue;
                }

                int ax = x + Utils.GetDirOffsetX(dir);
                int ay = y + Utils.GetDirOffsetY(dir);
                int ai = Cells[ax + ay * Width].Index + s.GetCon(dir);
                var a = Spans[ai];
                if (a.Reg != region)
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
        /// <param name="borderSize">Border size</param>
        /// <param name="queue">Data item queue to process</param>
        private void ProcessHeightDataQueue(HeightPatch hp, int borderSize, List<HeightDataItem> queue)
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
                foreach (var dir in IterateSpanConnections(cs))
                {
                    int ax = c.X + Utils.GetDirOffsetX(dir);
                    int ay = c.Y + Utils.GetDirOffsetY(dir);
                    int hx = ax - hp.Bounds.X - borderSize;
                    int hy = ay - hp.Bounds.Y - borderSize;

                    if (!hp.CompareBounds(hx, hy))
                    {
                        continue;
                    }

                    int ai = Cells[ax + ay * Width].Index + cs.GetCon(dir);
                    var a = Spans[ai];

                    hp.Data[hx + hy * hp.Bounds.Width] = a.Y;

                    queue.Add(new HeightDataItem { X = ax, Y = ay, I = ai });
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
                outTris = TriangulationHelper.TriangulateHull(verts, hull);

                return;
            }

            // Tessellate the base mesh.
            // We're using the triangulateHull instead of delaunayHull as it tends to
            // create a bit better triangulation for long thin triangles when there
            // are no internal points.
            var tris = TriangulationHelper.TriangulateHull(verts, hull);
            if (!tris.Any())
            {
                // Could not triangulate the poly, make sure there is some valid data there.
                Logger.WriteWarning(this, $"buildPolyDetail: Could not triangulate polygon ({verts.Length} verts).");

                outVerts = verts;
                outTris = Array.Empty<Int3>();

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
            var hullList = new List<int>(MAX_VERTS);

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

            hull = hullList.ToArray();

            return verts.ToArray();
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
            var edgeList = new List<Vector3>(MAX_VERTS_PER_EDGE + 1);

            float sampleDist = param.SampleDist;
            int heightSearchRadius = param.HeightSearchRadius;

            float cs = CellSize;
            float ics = 1.0f / cs;

            float dx = vi.X - vj.X;
            float dy = vi.Y - vj.Y;
            float dz = vi.Z - vj.Z;
            float d = (float)Math.Sqrt(dx * dx + dz * dz);
            nn = 1 + (int)Math.Floor(d / sampleDist);
            if (nn >= MAX_VERTS_PER_EDGE)
            {
                nn = MAX_VERTS_PER_EDGE - 1;
            }
            if (npolys + nn >= MAX_VERTS)
            {
                nn = MAX_VERTS - 1 - npolys;
            }

            for (int k = 0; k <= nn; ++k)
            {
                float u = k / (float)nn;
                var pos = new Vector3
                {
                    X = vj.X + dx * u,
                    Y = vj.Y + dy * u,
                    Z = vj.Z + dz * u
                };
                pos.Y = hp.GetHeight(pos, ics, CellHeight, heightSearchRadius) * CellHeight;
                edgeList.Add(pos);
            }

            edge = edgeList.ToArray();
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
            var samples = new List<Int4>(InitializeSamples(polygon, param, hp));

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
                var (bestpt, bestd, besti) = FindSampleWithMostError(samples.ToArray(), sampleDist, verts.ToArray(), triList.ToArray());

                // If the max error is within accepted threshold, stop tesselating.
                if (bestd <= sampleMaxError || besti == -1)
                {
                    break;
                }

                // Mark sample as added.
                var sb = samples[besti];
                sb.W = 1;
                samples[besti] = sb;

                // Add the new sample point.
                verts.Add(bestpt);

                // Create new triangulation. Full rebuild.
                var dhull = DelaunayHull.Build(verts.ToArray(), hull);
                var dTris = dhull.GetTris();

                triList.Clear();
                triList.AddRange(dTris);
            }

            newTris = triList.ToArray();

            return verts.ToArray();
        }
        /// <summary>
        /// Initialize samples
        /// </summary>
        /// <param name="polygon">Polygon vertices</param>
        /// <param name="param">Build polygon parameters</param>
        /// <param name="hp">Height patch</param>
        private Int4[] InitializeSamples(Vector3[] polygon, BuildPolyDetailParams param, HeightPatch hp)
        {
            float sampleDist = param.SampleDist;
            int heightSearchRadius = param.HeightSearchRadius;

            Utils.GetPolygonBounds(polygon, out var bmin, out var bmax);

            int x0 = (int)Math.Floor(bmin.X / sampleDist);
            int x1 = (int)Math.Ceiling(bmax.X / sampleDist);
            int z0 = (int)Math.Floor(bmin.Z / sampleDist);
            int z1 = (int)Math.Ceiling(bmax.Z / sampleDist);

            float cs = CellSize;
            float ics = 1.0f / cs;

            var samples = new List<Int4>();

            for (int z = z0; z < z1; ++z)
            {
                for (int x = x0; x < x1; ++x)
                {
                    var pt = new Vector3
                    {
                        X = x * sampleDist,
                        Y = (bmax.Y + bmin.Y) * 0.5f,
                        Z = z * sampleDist
                    };

                    // Make sure the samples are not too close to the edges.
                    if (Utils.DistancePtPoly2D(pt, polygon) > -sampleDist / 2)
                    {
                        continue;
                    }

                    int y = hp.GetHeight(pt, ics, CellHeight, heightSearchRadius);

                    var sample = new Int4(x, y, z, 0);

                    samples.Add(sample); // Not added
                }
            }

            return samples.ToArray();
        }
        /// <summary>
        /// Finds samples with most error
        /// </summary>
        /// <param name="samples">Sample list</param>
        /// <param name="sampleDist">Sample distance</param>
        /// <param name="verts">Vertices</param>
        /// <param name="tris">Triangles</param>
        private (Vector3 bestpt, float bestd, int besti) FindSampleWithMostError(Int4[] samples, float sampleDist, Vector3[] verts, Int3[] tris)
        {
            var bestpt = Vector3.Zero;
            float bestd = 0;
            int besti = -1;

            float cs = CellSize;

            int nsamples = samples.Length;
            for (int i = 0; i < nsamples; ++i)
            {
                var s = samples[i];
                if (s.W != 0)
                {
                    // skip added.
                    // The sample location is jittered to get rid of some bad triangulations
                    // which are cause by symmetrical data from the grid structure.
                    continue;
                }

                var pt = new Vector3
                {
                    X = s.X * sampleDist + Utils.GetJitterX(i) * cs * 0.1f,
                    Y = s.Y * CellHeight,
                    Z = s.Z * sampleDist + Utils.GetJitterY(i) * cs * 0.1f
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
        /// <param name="borderSize">Border size</param>
        /// <param name="hp">Height patch</param>
        /// <returns>Returns a height data item array</returns>
        /// <remarks>
        /// Reads to the compact heightfield are offset by border size since border size offset is already removed from the polymesh vertices.
        /// </remarks>
        private HeightDataItem[] SeedArrayWithPolyCenter(IndexedPolygon poly, Int3[] verts, int borderSize, HeightPatch hp)
        {
            int[] offset =
            {
                +0, +0,
                -1, -1,
                +0, -1,
                +1, -1,
                +1, +0,
                +1, +1,
                +0, +1,
                -1, +1,
                -1, +0,
            };

            var polyIndices = poly.GetVertices();

            // Find cell closest to a poly vertex
            int startCellX = 0, startCellY = 0, startSpanIndex = -1;
            int dmin = HeightPatch.RC_UNSET_HEIGHT;
            for (int j = 0; j < polyIndices.Length && dmin > 0; ++j)
            {
                for (int k = 0; k < 9 && dmin > 0; ++k)
                {
                    int ax = verts[polyIndices[j]].X + offset[k * 2 + 0];
                    int ay = verts[polyIndices[j]].Y;
                    int az = verts[polyIndices[j]].Z + offset[k * 2 + 1];
                    if (ax < hp.Bounds.X || ax >= hp.Bounds.X + hp.Bounds.Width ||
                        az < hp.Bounds.Y || az >= hp.Bounds.Y + hp.Bounds.Height)
                    {
                        continue;
                    }

                    var c = Cells[(ax + borderSize) + (az + borderSize) * Width];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni && dmin > 0; ++i)
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

            // Find center of the polygon
            int pcx = 0, pcy = 0;
            for (int j = 0; j < polyIndices.Length; j++)
            {
                pcx += verts[polyIndices[j]].X;
                pcy += verts[polyIndices[j]].Z;
            }
            pcx /= polyIndices.Length;
            pcy /= polyIndices.Length;

            // Use seeds array as a stack for DFS
            var array = new List<HeightDataItem>(512)
            {
                new HeightDataItem
                {
                    X = startCellX,
                    Y = startCellY,
                    I = startSpanIndex
                }
            };

            int[] dirs = { 0, 1, 2, 3 };
            hp.Data = Helper.CreateArray(hp.Bounds.Width * hp.Bounds.Height, 0);
            // DFS to move to the center. Note that we need a DFS here and can not just move
            // directly towards the center without recording intermediate nodes, even though the polygons
            // are convex. In very rare we can get stuck due to contour simplification if we do not
            // record nodes.
            var hdItem = new HeightDataItem();
            while (true)
            {
                if (array.Count < 1)
                {
                    Logger.WriteWarning(this, "Walk towards polygon center failed to reach center");
                    break;
                }

                hdItem = array.Pop();

                if (hdItem.X == pcx && hdItem.Y == pcy)
                {
                    break;
                }

                // If we are already at the correct X-position, prefer direction
                // directly towards the center in the Y-axis; otherwise prefer
                // direction in the X-axis
                int directDir;
                if (hdItem.X == pcx)
                {
                    directDir = Utils.GetDirForOffset(0, pcy > hdItem.Y ? 1 : -1);
                }
                else
                {
                    directDir = Utils.GetDirForOffset(pcx > hdItem.X ? 1 : -1, 0);
                }

                // Push the direct dir last so we start with this on next iteration
                Helper.Swap(ref dirs[directDir], ref dirs[3]);

                var cs = Spans[hdItem.I];
                for (int i = 0; i < 4; i++)
                {
                    int dir = dirs[i];
                    if (cs.GetCon(dir) == RC_NOT_CONNECTED)
                    {
                        continue;
                    }

                    int newX = hdItem.X + Utils.GetDirOffsetX(dir);
                    int newY = hdItem.Y + Utils.GetDirOffsetY(dir);

                    int hpx = newX - hp.Bounds.X;
                    int hpy = newY - hp.Bounds.Y;
                    if (hpx < 0 || hpx >= hp.Bounds.Width || hpy < 0 || hpy >= hp.Bounds.Height)
                    {
                        continue;
                    }

                    if (hp.Data[hpx + hpy * hp.Bounds.Width] != 0)
                    {
                        continue;
                    }

                    hp.Data[hpx + hpy * hp.Bounds.Width] = 1;
                    int index = Cells[(newX + borderSize) + (newY + borderSize) * Width].Index + cs.GetCon(dir);
                    array.Add(new HeightDataItem
                    {
                        X = newX,
                        Y = newY,
                        I = index
                    });
                }

                Helper.Swap(ref dirs[directDir], ref dirs[3]);
            }

            array.Clear();

            // getHeightData seeds are given in coordinates with borders
            array.Add(new HeightDataItem
            {
                X = hdItem.X + borderSize,
                Y = hdItem.Y + borderSize,
                I = hdItem.I,
            });

            hp.Data = Helper.CreateArray(hp.Bounds.Width * hp.Bounds.Height, SPAN_MAX_HEIGHT);
            var chs = Spans[hdItem.I];
            hp.Data[hdItem.X - hp.Bounds.X + (hdItem.Y - hp.Bounds.Y) * hp.Bounds.Width] = chs.Y;

            return array.ToArray();
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
            // Prepare for region partitioning, by calculating distance field along the walkable surface.
            BuildDistanceField();

            int w = Width;
            int h = Height;

            int LOG_NB_STACKS = 3;
            int NB_STACKS = 1 << LOG_NB_STACKS;
            var lvlStacks = new List<LevelStackEntry>[NB_STACKS];
            for (int i = 0; i < NB_STACKS; i++)
            {
                lvlStacks[i] = new List<LevelStackEntry>();
            }

            var stack = new List<LevelStackEntry>();

            int[] srcReg = new int[SpanCount];
            int[] srcDist = new int[SpanCount];

            int regionId = 1;
            int level = (MaxDistance + 1) & ~1;

            // Figure better formula, expandIters defines how much the 
            // watershed "overflows" and simplifies the regions. Tying it to
            // agent radius was usually good indication how greedy it could be.
            //	const int expandIters = 4 + walkableRadius * 2
            const int expandIters = 8;

            if (borderSize > 0)
            {
                // Make sure border will not overflow.
                int bw = Math.Min(w, borderSize);
                int bh = Math.Min(h, borderSize);

                // Paint regions
                PaintRectRegion(0, bw, 0, h, (regionId | RC_BORDER_REG), srcReg); regionId++;
                PaintRectRegion(w - bw, w, 0, h, (regionId | RC_BORDER_REG), srcReg); regionId++;
                PaintRectRegion(0, w, 0, bh, (regionId | RC_BORDER_REG), srcReg); regionId++;
                PaintRectRegion(0, w, h - bh, h, (regionId | RC_BORDER_REG), srcReg); regionId++;
            }

            BorderSize = borderSize;

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
                    var stacks = AppendStacks(lvlStacks[sId - 1].ToArray(), srcReg); // copy left overs from last level
                    lvlStacks[sId].AddRange(stacks);
                }

                // Expand current regions until no empty connected cells found.
                ExpandRegions(expandIters, level, srcReg, srcDist, lvlStacks[sId], false);

                // Mark new regions with IDs.
                for (int j = 0; j < lvlStacks[sId].Count; j++)
                {
                    var current = lvlStacks[sId][j];
                    int x = current.X;
                    int y = current.Y;
                    int i = current.Index;
                    if (i >= 0 && srcReg[i] == 0)
                    {
                        var entry = new LevelStackEntry { X = x, Y = y, Index = i };
                        var floodRes = FloodRegion(entry, level, regionId, srcReg, srcDist, stack);
                        if (floodRes)
                        {
                            if (regionId == int.MaxValue)
                            {
                                throw new EngineException("rcBuildRegions: Region ID overflow");
                            }

                            regionId++;
                        }
                    }
                }
            }

            // Expand current regions until no empty connected cells found.
            ExpandRegions(expandIters * 8, 0, srcReg, srcDist, stack, true);

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
                throw new EngineException(string.Format("rcBuildRegions: {0} overlapping regions", overlaps.Length));
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

            var dst = BoxBlur(1, src);

            // Blur and Store distance.
            if (dst != src)
            {
                BorderDistances = dst;
            }
            else
            {
                BorderDistances = src;
            }
        }
        /// <summary>
        /// Calculates the distance field
        /// </summary>
        /// <returns>Returns the array of distances</returns>
        public int[] CalculateDistanceField()
        {
            if (SpanCount <= 0)
            {
                return Array.Empty<int>();
            }

            int w = Width;
            int h = Height;

            // Mark boundary cells.
            int[] res = MarkBoundaryCells(w, h);

            // Pass 1
            foreach (var (x, y, i, _) in IterateCells(Cells, w, h))
            {
                res = TestConn(x, y, i, w, 0, 3, res);
                res = TestConn(x, y, i, w, 3, 2, res);
            }

            // Pass 2
            foreach (var (x, y, i, _) in IterateCellsReverse(Cells, w, h))
            {
                res = TestConn(x, y, i, w, 2, 1, res);
                res = TestConn(x, y, i, w, 1, 0, res);
            }

            MaxDistance = res.Max();

            return res;
        }
        /// <summary>
        /// Marks boundary cells
        /// </summary>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        private int[] MarkBoundaryCells(int w, int h)
        {
            // Init distance and points.
            int[] res = Helper.CreateArray(SpanCount, int.MaxValue);

            // Mark boundary cells.
            foreach (var (x, y, i, _) in IterateCells(Cells, w, h))
            {
                var s = Spans[i];
                var area = Areas[i];

                int nc = 0;
                for (int dir = 0; dir < 4; ++dir)
                {
                    if (s.GetCon(dir) == RC_NOT_CONNECTED)
                    {
                        continue;
                    }

                    int ax = x + Utils.GetDirOffsetX(dir);
                    int ay = y + Utils.GetDirOffsetY(dir);
                    int ai = Cells[ax + ay * w].Index + s.GetCon(dir);
                    if (area == Areas[ai])
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

            int w = Width;
            int h = Height;

            thr *= 2;

            foreach (var (x, y, i, _) in IterateCells(Cells, w, h))
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
                    if (s.GetCon(dir) == RC_NOT_CONNECTED)
                    {
                        d += cd * 2;

                        continue;
                    }

                    int ax = x + Utils.GetDirOffsetX(dir);
                    int ay = y + Utils.GetDirOffsetY(dir);
                    int ai = Cells[ax + ay * w].Index + s.GetCon(dir);
                    d += src[ai];

                    var a = Spans[ai];
                    int dir2 = (dir + 1) & 0x3;
                    if (a.GetCon(dir2) == RC_NOT_CONNECTED)
                    {
                        d += cd;

                        continue;
                    }

                    int ax2 = ax + Utils.GetDirOffsetX(dir2);
                    int ay2 = ay + Utils.GetDirOffsetY(dir2);
                    int ai2 = Cells[ax2 + ay2 * w].Index + a.GetCon(dir2);
                    d += src[ai2];
                }

                dst[i] = (d + 5) / 9;
            }

            return dst;
        }
        /// <summary>
        /// Test connections
        /// </summary>
        private int[] TestConn(int x, int y, int i, int w, int id1, int id2, int[] arr)
        {
            int[] res = arr.ToArray();

            var s = Spans[i];
            if (s.GetCon(id1) == RC_NOT_CONNECTED)
            {
                return res;
            }

            int ax = x + Utils.GetDirOffsetX(id1);
            int ay = y + Utils.GetDirOffsetY(id1);
            int ai = Cells[ax + ay * w].Index + s.GetCon(id1);
            if (res[ai] + 2 < res[i])
            {
                res[i] = res[ai] + 2;
            }

            var a = Spans[ai];
            if (a.GetCon(id2) == RC_NOT_CONNECTED)
            {
                return res;
            }

            int aax = ax + Utils.GetDirOffsetX(id2);
            int aay = ay + Utils.GetDirOffsetY(id2);
            int aai = Cells[aax + aay * w].Index + a.GetCon(id2);
            if (res[aai] + 3 < res[i])
            {
                res[i] = res[aai] + 3;
            }

            return res;
        }
        /// <summary>
        /// Flood region
        /// </summary>
        private bool FloodRegion(LevelStackEntry entry, int level, int r, int[] srcReg, int[] srcDist, List<LevelStackEntry> stack)
        {
            int w = Width;

            var area = Areas[entry.Index];

            // Flood fill mark region.
            stack.Clear();
            stack.Add(entry);
            srcReg[entry.Index] = r;
            srcDist[entry.Index] = 0;

            int lev = level >= 2 ? level - 2 : 0;
            int count = 0;

            while (stack.Count > 0)
            {
                var back = stack.Pop();

                int cx = back.X;
                int cy = back.Y;
                int ci = back.Index;

                var cs = Spans[ci];

                // Check if any of the neighbours already have a valid region set.
                int ar = 0;
                for (int dir = 0; dir < 4; ++dir)
                {
                    // 8 connected
                    if (cs.GetCon(dir) != RC_NOT_CONNECTED)
                    {
                        int ax = cx + Utils.GetDirOffsetX(dir);
                        int ay = cy + Utils.GetDirOffsetY(dir);
                        int ai = Cells[ax + ay * w].Index + cs.GetCon(dir);
                        if (Areas[ai] != area)
                        {
                            continue;
                        }
                        int nr = srcReg[ai];
                        if ((nr & RC_BORDER_REG) != 0) // Do not take borders into account.
                        {
                            continue;
                        }
                        if (nr != 0 && nr != r)
                        {
                            ar = nr;
                            break;
                        }

                        var a = Spans[ai];

                        int dir2 = (dir + 1) & 0x3;
                        if (a.GetCon(dir2) != RC_NOT_CONNECTED)
                        {
                            int ax2 = ax + Utils.GetDirOffsetX(dir2);
                            int ay2 = ay + Utils.GetDirOffsetY(dir2);
                            int ai2 = Cells[ax2 + ay2 * w].Index + a.GetCon(dir2);
                            if (Areas[ai2] != area)
                            {
                                continue;
                            }
                            int nr2 = srcReg[ai2];
                            if (nr2 != 0 && nr2 != r)
                            {
                                ar = nr2;
                                break;
                            }
                        }
                    }
                }
                if (ar != 0)
                {
                    srcReg[ci] = 0;
                    continue;
                }

                count++;

                // Expand neighbours.
                for (int dir = 0; dir < 4; ++dir)
                {
                    if (cs.GetCon(dir) != RC_NOT_CONNECTED)
                    {
                        int ax = cx + Utils.GetDirOffsetX(dir);
                        int ay = cy + Utils.GetDirOffsetY(dir);
                        int ai = Cells[ax + ay * w].Index + cs.GetCon(dir);
                        if (Areas[ai] != area)
                        {
                            continue;
                        }
                        if (BorderDistances[ai] >= lev && srcReg[ai] == 0)
                        {
                            srcReg[ai] = r;
                            srcDist[ai] = 0;
                            stack.Add(new LevelStackEntry { X = ax, Y = ay, Index = ai });
                        }
                    }
                }
            }

            return count > 0;
        }
        /// <summary>
        /// Expand region
        /// </summary>
        private void ExpandRegions(int maxIter, int level, int[] srcReg, int[] srcDist, List<LevelStackEntry> stack, bool fillStack)
        {
            int w = Width;
            int h = Height;

            if (fillStack)
            {
                // Find cells revealed by the raised level.
                stack.Clear();

                foreach (var (x, y, i, _) in IterateCells(Cells, w, h))
                {
                    if (Areas[i] == AreaTypes.RC_NULL_AREA)
                    {
                        continue;
                    }

                    if (BorderDistances[i] >= level && srcReg[i] == 0)
                    {
                        stack.Add(new LevelStackEntry { X = x, Y = y, Index = i });
                    }
                }
            }
            else // use cells in the input stack
            {
                // mark all cells which already have a region
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

            var dirtyEntries = new List<RecastRegionDirtyEntry>();
            int iter = 0;
            while (stack.Count > 0)
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

                    int r = srcReg[i];
                    int d2 = int.MaxValue;
                    var area = Areas[i];
                    var s = Spans[i];
                    for (int dir = 0; dir < 4; ++dir)
                    {
                        if (s.GetCon(dir) == RC_NOT_CONNECTED) continue;
                        int ax = x + Utils.GetDirOffsetX(dir);
                        int ay = y + Utils.GetDirOffsetY(dir);
                        int ai = Cells[ax + ay * w].Index + s.GetCon(dir);
                        if (Areas[ai] != area) continue;
                        if (srcReg[ai] > 0 && (srcReg[ai] & RC_BORDER_REG) == 0 && srcDist[ai] + 2 < d2)
                        {
                            r = srcReg[ai];
                            d2 = srcDist[ai] + 2;
                        }
                    }
                    if (r != 0)
                    {
                        current.Index = -1; // mark as used
                        stack[j] = current;

                        dirtyEntries.Add(new RecastRegionDirtyEntry { Index = i, Region = r, Distance2 = d2 });
                    }
                    else
                    {
                        failed++;
                    }
                }

                for (int i = 0; i < dirtyEntries.Count; i++)
                {
                    int idx = dirtyEntries[i].Index;
                    srcReg[idx] = dirtyEntries[i].Region;
                    srcDist[idx] = dirtyEntries[i].Distance2;
                }

                if (failed == stack.Count)
                {
                    break;
                }

                if (level > 0)
                {
                    ++iter;
                    if (iter >= maxIter)
                    {
                        break;
                    }
                }
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
            int w = Width;
            int h = Height;
            startLevel >>= loglevelsPerStack;

            for (int j = 0; j < nbStacks; j++)
            {
                stacks[j].Clear();
            }

            // put all cells in the level range into the appropriate stacks
            foreach (var (x, y, i, _) in IterateCells(Cells, w, h))
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

                stacks[sId].Add(new LevelStackEntry { X = x, Y = y, Index = i });
            }
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
            int w = Width;
            int h = Height;
            int id = 1;

            int[] srcReg = new int[SpanCount];

            int nsweeps = Math.Max(Width, Height);
            SweepSpan[] sweeps = new SweepSpan[nsweeps];

            // Mark border regions.
            if (borderSize > 0)
            {
                // Make sure border will not overflow.
                int bw = Math.Min(w, borderSize);
                int bh = Math.Min(h, borderSize);
                // Paint regions
                PaintRectRegion(0, bw, 0, h, id | RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(w - bw, w, 0, h, id | RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(0, w, 0, bh, id | RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(0, w, h - bh, h, id | RC_BORDER_REG, srcReg); id++;
            }

            BorderSize = borderSize;

            // Sweep one line at a time.
            for (int y = borderSize; y < h - borderSize; ++y)
            {
                // Collect spans from this row.
                int[] prev = new int[id + 1];
                int rid = 1;

                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    var c = Cells[x + y * w];

                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = Spans[i];
                        if (Areas[i] == AreaTypes.RC_NULL_AREA)
                        {
                            continue;
                        }

                        // -x
                        int previd = 0;
                        if (s.GetCon(0) != RC_NOT_CONNECTED)
                        {
                            int ax = x + Utils.GetDirOffsetX(0);
                            int ay = y + Utils.GetDirOffsetY(0);
                            int ai = Cells[ax + ay * w].Index + s.GetCon(0);
                            if ((srcReg[ai] & RC_BORDER_REG) == 0 && Areas[i] == Areas[ai])
                            {
                                previd = srcReg[ai];
                            }
                        }

                        if (previd == 0)
                        {
                            previd = rid++;
                            sweeps[previd].RId = previd;
                            sweeps[previd].NS = 0;
                            sweeps[previd].Nei = 0;
                        }

                        // -y
                        if (s.GetCon(3) != RC_NOT_CONNECTED)
                        {
                            int ax = x + Utils.GetDirOffsetX(3);
                            int ay = y + Utils.GetDirOffsetY(3);
                            int ai = Cells[ax + ay * w].Index + s.GetCon(3);
                            if (srcReg[ai] != 0 && (srcReg[ai] & RC_BORDER_REG) == 0 && Areas[i] == Areas[ai])
                            {
                                int nr = srcReg[ai];
                                if (sweeps[previd].Nei == 0 || sweeps[previd].Nei == nr)
                                {
                                    sweeps[previd].Nei = nr;
                                    sweeps[previd].NS++;
                                    prev[nr]++;
                                }
                                else
                                {
                                    sweeps[previd].Nei = RC_NULL_NEI;
                                }
                            }
                        }

                        srcReg[i] = previd;
                    }
                }

                // Create unique ID.
                for (int i = 1; i < rid; ++i)
                {
                    if (sweeps[i].Nei != RC_NULL_NEI &&
                        sweeps[i].Nei != 0 &&
                        prev[sweeps[i].Nei] == sweeps[i].NS)
                    {
                        sweeps[i].Id = sweeps[i].Nei;
                    }
                    else
                    {
                        sweeps[i].Id = id++;
                    }
                }

                // Remap IDs
                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    var c = Cells[x + y * w];

                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        if (srcReg[i] > 0 && srcReg[i] < rid)
                        {
                            srcReg[i] = sweeps[srcReg[i]].Id;
                        }
                    }
                }
            }

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
        /// Builds regions using layers method
        /// </summary>
        /// <param name="borderSize">Border size</param>
        /// <param name="minRegionArea">Minimum region area</param>
        /// <returns>Returns true when the region were correctly built</returns>
        private bool BuildRegionsLayer(int borderSize, int minRegionArea)
        {
            int w = Width;
            int h = Height;
            int id = 1;

            int[] srcReg = new int[SpanCount];

            int nsweeps = Math.Max(Width, Height);
            SweepSpan[] sweeps = Helper.CreateArray(nsweeps, new SweepSpan());

            // Mark border regions.
            if (borderSize > 0)
            {
                // Make sure border will not overflow.
                int bw = Math.Min(w, borderSize);
                int bh = Math.Min(h, borderSize);
                // Paint regions
                PaintRectRegion(0, bw, 0, h, id | RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(w - bw, w, 0, h, id | RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(0, w, 0, bh, id | RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(0, w, h - bh, h, id | RC_BORDER_REG, srcReg); id++;
            }

            BorderSize = borderSize;

            // Sweep one line at a time.
            for (int y = borderSize; y < h - borderSize; ++y)
            {
                // Collect spans from this row.
                int[] prev = new int[1024];
                int rid = 1;

                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    var c = Cells[x + y * w];

                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = Spans[i];
                        if (Areas[i] == AreaTypes.RC_NULL_AREA)
                        {
                            continue;
                        }

                        // -x
                        int previd = 0;
                        if (s.GetCon(0) != RC_NOT_CONNECTED)
                        {
                            int ax = x + Utils.GetDirOffsetX(0);
                            int ay = y + Utils.GetDirOffsetY(0);
                            int ai = Cells[ax + ay * w].Index + s.GetCon(0);
                            if ((srcReg[ai] & RC_BORDER_REG) == 0 && Areas[i] == Areas[ai])
                            {
                                previd = srcReg[ai];
                            }
                        }

                        if (previd == 0)
                        {
                            previd = rid++;
                            sweeps[previd].RId = previd;
                            sweeps[previd].NS = 0;
                            sweeps[previd].Nei = 0;
                        }

                        // -y
                        if (s.GetCon(3) != RC_NOT_CONNECTED)
                        {
                            int ax = x + Utils.GetDirOffsetX(3);
                            int ay = y + Utils.GetDirOffsetY(3);
                            int ai = Cells[ax + ay * w].Index + s.GetCon(3);
                            if (srcReg[ai] != 0 && (srcReg[ai] & RC_BORDER_REG) == 0 && Areas[i] == Areas[ai])
                            {
                                int nr = srcReg[ai];
                                if (sweeps[previd].Nei == 0 || sweeps[previd].Nei == nr)
                                {
                                    sweeps[previd].Nei = nr;
                                    sweeps[previd].NS++;
                                    prev[nr]++;
                                }
                                else
                                {
                                    sweeps[previd].Nei = RC_NULL_NEI;
                                }
                            }
                        }

                        srcReg[i] = previd;
                    }
                }

                // Create unique ID.
                for (int i = 1; i < rid; ++i)
                {
                    if (sweeps[i].Nei != RC_NULL_NEI &&
                        sweeps[i].Nei != 0 &&
                        prev[sweeps[i].Nei] == sweeps[i].NS)
                    {
                        sweeps[i].Id = sweeps[i].Nei;
                    }
                    else
                    {
                        sweeps[i].Id = id++;
                    }
                }

                // Remap IDs
                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    var c = Cells[x + y * w];

                    for (int i = c.Index, ni = c.Index + c.Count; i < ni; ++i)
                    {
                        if (srcReg[i] > 0 && srcReg[i] < rid)
                        {
                            srcReg[i] = sweeps[srcReg[i]].Id;
                        }
                    }
                }
            }

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
            int w = Width;
            int h = Height;

            int nreg = maxRegionId + 1;
            var regions = new List<Region>(nreg);

            // Construct regions
            for (int i = 0; i < nreg; ++i)
            {
                regions.Add(new Region(i));
            }

            // Find region neighbours and overlapping regions.
            var lregs = new List<int>(32);
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = Cells[x + y * w];

                    lregs.Clear();

                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = Spans[i];
                        int ri = srcReg[i];
                        if (ri == 0 || ri >= nreg)
                        {
                            continue;
                        }
                        var reg = regions[ri];

                        reg.SpanCount++;

                        reg.YMin = Math.Min(reg.YMin, s.Y);
                        reg.YMax = Math.Max(reg.YMax, s.Y);

                        // Collect all region layers.
                        lregs.Add(ri);

                        // Update neighbours
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (s.GetCon(dir) != RC_NOT_CONNECTED)
                            {
                                int ax = x + Utils.GetDirOffsetX(dir);
                                int ay = y + Utils.GetDirOffsetY(dir);
                                int ai = Cells[ax + ay * w].Index + s.GetCon(dir);
                                int rai = srcReg[ai];
                                if (rai > 0 && rai < nreg && rai != ri)
                                {
                                    reg.AddUniqueConnection(rai);
                                }
                                if ((rai & RC_BORDER_REG) != 0)
                                {
                                    reg.ConnectsToBorder = true;
                                }
                            }
                        }
                    }

                    // Update overlapping regions.
                    for (int i = 0; i < lregs.Count - 1; ++i)
                    {
                        for (int j = i + 1; j < lregs.Count; ++j)
                        {
                            if (lregs[i] != lregs[j])
                            {
                                var ri = regions[lregs[i]];
                                var rj = regions[lregs[j]];
                                ri.AddUniqueFloorRegion(lregs[j]);
                                rj.AddUniqueFloorRegion(lregs[i]);
                            }
                        }
                    }
                }
            }

            // Create 2D layers from regions.
            int layerId = 1;

            for (int i = 0; i < nreg; ++i)
            {
                regions[i].Id = 0;
            }

            // Merge montone regions to create non-overlapping areas.
            var stack = new List<int>(32);
            for (int i = 1; i < nreg; ++i)
            {
                var root = regions[i];
                // Skip already visited.
                if (root.Id != 0)
                {
                    continue;
                }

                // Start search.
                root.Id = layerId;

                stack.Clear();
                stack.Add(i);

                while (stack.Count > 0)
                {
                    // Pop front
                    var reg = regions[stack[0]];
                    for (int j = 0; j < stack.Count - 1; ++j)
                    {
                        stack[j] = stack[j + 1];
                    }
                    stack.Clear();

                    var cons = reg.GetConnections();
                    foreach (var nei in cons)
                    {
                        var regn = regions[nei];
                        // Skip already visited.
                        if (regn.Id != 0)
                        {
                            continue;
                        }
                        // Skip if the neighbour is overlapping root region.
                        bool overlap = false;
                        var rootFloors = root.GetFloors();
                        foreach (var floor in rootFloors)
                        {
                            if (floor == nei)
                            {
                                overlap = true;
                                break;
                            }
                        }
                        if (overlap)
                        {
                            continue;
                        }

                        // Deepen
                        stack.Add(nei);

                        // Mark layer id
                        regn.Id = layerId;
                        // Merge current layers to root.
                        var regnFloors = regn.GetFloors();
                        foreach (var floor in regnFloors)
                        {
                            root.AddUniqueFloorRegion(floor);
                        }
                        root.YMin = Math.Min(root.YMin, regn.YMin);
                        root.YMax = Math.Max(root.YMax, regn.YMax);
                        root.SpanCount += regn.SpanCount;
                        regn.SpanCount = 0;
                        root.ConnectsToBorder = root.ConnectsToBorder || regn.ConnectsToBorder;
                    }
                }

                layerId++;
            }

            // Remove small regions
            for (int i = 0; i < nreg; ++i)
            {
                if (regions[i].SpanCount > 0 && regions[i].SpanCount < minRegionArea && !regions[i].ConnectsToBorder)
                {
                    int reg = regions[i].Id;
                    for (int j = 0; j < nreg; ++j)
                    {
                        if (regions[j].Id == reg)
                        {
                            regions[j].Id = 0;
                        }
                    }
                }
            }

            // Compress region Ids.
            for (int i = 0; i < nreg; ++i)
            {
                regions[i].Remap = false;
                if (regions[i].Id == 0)
                {
                    // Skip nil regions.
                    continue;
                }
                if ((regions[i].Id & RC_BORDER_REG) != 0)
                {
                    // Skip external regions.
                    continue;
                }
                regions[i].Remap = true;
            }

            int regIdGen = 0;
            for (int i = 0; i < nreg; ++i)
            {
                if (!regions[i].Remap)
                {
                    continue;
                }
                int oldId = regions[i].Id;
                int newId = ++regIdGen;
                for (int j = i; j < nreg; ++j)
                {
                    if (regions[j].Id == oldId)
                    {
                        regions[j].Id = newId;
                        regions[j].Remap = false;
                    }
                }
            }
            maxRegionIdResult = regIdGen;

            // Remap regions.
            for (int i = 0; i < SpanCount; ++i)
            {
                if ((srcReg[i] & RC_BORDER_REG) == 0)
                {
                    srcReg[i] = regions[srcReg[i]].Id;
                }
            }

            for (int i = 0; i < nreg; ++i)
            {
                regions[i] = null;
            }

            return true;
        }
        /// <summary>
        /// Paint rectangle region
        /// </summary>
        private void PaintRectRegion(int minx, int maxx, int miny, int maxy, int regId, int[] srcReg)
        {
            int w = Width;

            foreach (var (_, _, i, _) in IterateCellsBounds(Cells, w, minx, miny, maxx, maxy))
            {
                if (Areas[i] != AreaTypes.RC_NULL_AREA)
                {
                    srcReg[i] = regId;
                }
            }
        }
        /// <summary>
        /// Merge and filter region
        /// </summary>
        private bool MergeAndFilterRegions(int minRegionArea, int mergeRegionSize, int maxRegionId, int[] srcReg, out int[] overlaps, out int maxRegionIdResult)
        {
            int w = Width;
            int h = Height;

            int nreg = maxRegionId + 1;
            var regions = new List<Region>(nreg);

            // Construct regions
            for (int i = 0; i < nreg; ++i)
            {
                regions.Add(new Region(i));
            }

            // Find edge of a region and find connections around the contour.
            foreach (var (x, y, i, c) in IterateCells(Cells, w, h))
            {
                int r = srcReg[i];
                if (r == 0 || r >= nreg)
                {
                    continue;
                }

                var reg = regions[r];
                reg.SpanCount++;

                // Update floors.
                for (int j = c.Index; j < c.Index + c.Count; ++j)
                {
                    if (i == j) continue;
                    int floorId = srcReg[j];
                    if (floorId == 0 || floorId >= nreg)
                    {
                        continue;
                    }
                    if (floorId == r)
                    {
                        reg.Overlap = true;
                    }
                    reg.AddUniqueFloorRegion(floorId);
                }

                // Have found contour
                if (reg.GetConnectionCount() > 0)
                {
                    continue;
                }

                reg.AreaType = Areas[i];

                // Check if this cell is next to a border.
                int ndir = -1;
                for (int dir = 0; dir < 4; ++dir)
                {
                    if (IsSolidEdge(srcReg, x, y, i, dir))
                    {
                        ndir = dir;
                        break;
                    }
                }

                if (ndir != -1)
                {
                    // The cell is at border.
                    // Walk around the contour to find all the neighbours.
                    var neighbours = WalkContour(x, y, i, ndir, srcReg);
                    if (neighbours.Any())
                    {
                        reg.AddConnections(neighbours);
                    }
                }
            }

            // Remove too small regions.
            var stack = new List<int>();
            var trace = new List<int>();
            for (int i = 0; i < nreg; ++i)
            {
                var reg = regions[i];
                if (reg.Id == 0 || (reg.Id & RC_BORDER_REG) != 0)
                {
                    continue;
                }
                if (reg.SpanCount == 0)
                {
                    continue;
                }
                if (reg.Visited)
                {
                    continue;
                }

                // Count the total size of all the connected regions.
                // Also keep track of the regions connects to a tile border.
                bool connectsToBorder = false;
                int spanCount = 0;
                stack.Clear();
                trace.Clear();

                reg.Visited = true;
                stack.Add(i);

                while (stack.Count > 0)
                {
                    // Pop
                    int ri = stack.Pop();

                    var creg = regions[ri];

                    spanCount += creg.SpanCount;
                    trace.Add(ri);

                    var connections = creg.GetConnections();
                    foreach (var connection in connections)
                    {
                        if ((connection & RC_BORDER_REG) != 0)
                        {
                            connectsToBorder = true;
                            continue;
                        }
                        var neireg = regions[connection];
                        if (neireg.Visited)
                        {
                            continue;
                        }
                        if (neireg.Id == 0 || (neireg.Id & RC_BORDER_REG) != 0)
                        {
                            continue;
                        }
                        // Visit
                        stack.Add(neireg.Id);
                        neireg.Visited = true;
                    }
                }

                // If the accumulated regions size is too small, remove it.
                // Do not remove areas which connect to tile borders
                // as their size cannot be estimated correctly and removing them
                // can potentially remove necessary areas.
                if (spanCount < minRegionArea && !connectsToBorder)
                {
                    // Kill all visited regions.
                    for (int j = 0; j < trace.Count; ++j)
                    {
                        regions[trace[j]].SpanCount = 0;
                        regions[trace[j]].Id = 0;
                    }
                }
            }

            // Merge too small regions to neighbour regions.
            int mergeCount;
            do
            {
                mergeCount = 0;
                for (int i = 0; i < nreg; ++i)
                {
                    var reg = regions[i];
                    if (reg.Id == 0 || (reg.Id & RC_BORDER_REG) != 0)
                    {
                        continue;
                    }
                    if (reg.Overlap)
                    {
                        continue;
                    }
                    if (reg.SpanCount == 0)
                    {
                        continue;
                    }

                    // Check to see if the region should be merged.
                    if (reg.SpanCount > mergeRegionSize && reg.IsRegionConnectedToBorder())
                    {
                        continue;
                    }

                    // Small region with more than 1 connection.
                    // Or region which is not connected to a border at all.
                    // Find smallest neighbour region that connects to this one.
                    int smallest = int.MaxValue;
                    int mergeId = reg.Id;
                    var connections = reg.GetConnections();
                    foreach (var connection in connections)
                    {
                        if ((connection & RC_BORDER_REG) != 0)
                        {
                            continue;
                        }

                        var mreg = regions[connection];
                        if (mreg.Id == 0 || (mreg.Id & RC_BORDER_REG) != 0 || mreg.Overlap)
                        {
                            continue;
                        }

                        if (mreg.SpanCount < smallest &&
                            Region.CanMergeWithRegion(reg, mreg) &&
                            Region.CanMergeWithRegion(mreg, reg))
                        {
                            smallest = mreg.SpanCount;
                            mergeId = mreg.Id;
                        }
                    }
                    // Found new id.
                    if (mergeId != reg.Id)
                    {
                        int oldId = reg.Id;
                        var target = regions[mergeId];

                        // Merge neighbours.
                        if (Region.MergeRegions(target, reg))
                        {
                            // Fixup regions pointing to current region.
                            for (int j = 0; j < nreg; ++j)
                            {
                                if (regions[j].Id == 0 || (regions[j].Id & RC_BORDER_REG) != 0)
                                {
                                    continue;
                                }

                                // If another region was already merged into current region
                                // change the nid of the previous region too.
                                if (regions[j].Id == oldId)
                                {
                                    regions[j].Id = mergeId;
                                }

                                // Replace the current region with the new one if the
                                // current regions is neighbour.
                                regions[j].ReplaceNeighbour(oldId, mergeId);
                            }

                            mergeCount++;
                        }
                    }
                }
            }
            while (mergeCount > 0);

            // Compress region Ids.
            for (int i = 0; i < nreg; ++i)
            {
                regions[i].Remap = false;
                if (regions[i].Id == 0)
                {
                    // Skip nil regions.
                    continue;
                }
                if ((regions[i].Id & RC_BORDER_REG) != 0)
                {
                    // Skip external regions.
                    continue;
                }
                regions[i].Remap = true;
            }

            int regIdGen = 0;
            for (int i = 0; i < nreg; ++i)
            {
                if (!regions[i].Remap)
                {
                    continue;
                }
                int oldId = regions[i].Id;
                int newId = ++regIdGen;
                for (int j = i; j < nreg; ++j)
                {
                    if (regions[j].Id == oldId)
                    {
                        regions[j].Id = newId;
                        regions[j].Remap = false;
                    }
                }
            }
            maxRegionIdResult = regIdGen;

            // Remap regions.
            for (int i = 0; i < SpanCount; ++i)
            {
                if ((srcReg[i] & RC_BORDER_REG) == 0)
                {
                    srcReg[i] = regions[srcReg[i]].Id;
                }
            }

            // Return regions that we found to be overlapping.
            var lOverlaps = new List<int>();
            for (int i = 0; i < nreg; ++i)
            {
                if (regions[i].Overlap)
                {
                    lOverlaps.Add(regions[i].Id);
                }
            }
            overlaps = lOverlaps.ToArray();

            for (int i = 0; i < nreg; ++i)
            {
                regions[i] = null;
            }

            return true;
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
            int w = Width;

            var c = Cells[x + y * w];
            for (int i = c.Index, ni = c.Index + c.Count; i < ni; ++i)
            {
                if (Spans[i].Reg == 0 || (Spans[i].Reg & RC_BORDER_REG) != 0)
                {
                    flags[i] = 0;
                    continue;
                }

                int res = 0;
                var s = Spans[i];
                for (int dir = 0; dir < 4; ++dir)
                {
                    int r = 0;
                    if (s.GetCon(dir) != RC_NOT_CONNECTED)
                    {
                        int ax = x + Utils.GetDirOffsetX(dir);
                        int ay = y + Utils.GetDirOffsetY(dir);
                        int ai = Cells[ax + ay * w].Index + s.GetCon(dir);
                        r = Spans[ai].Reg;
                    }
                    if (r == Spans[i].Reg)
                    {
                        res |= 1 << dir;
                    }
                }
                flags[i] = res ^ 0xf; // Inverse, mark non connected edges.
            }
        }

        /// <summary>
        /// Adds a new compact cell
        /// </summary>
        /// <param name="x">X cell coordinate</param>
        /// <param name="y">Y cell coordinate</param>
        /// <param name="flags">Flags to update</param>
        public (int Reg, AreaTypes Area, Int4[] RawVerts)[] BuildCompactCells(int x, int y, int[] flags)
        {
            List<(int Reg, AreaTypes Area, Int4[] RawVerts)> res = new();

            int w = Width;

            var c = Cells[x + y * w];
            for (int i = c.Index, ni = c.Index + c.Count; i < ni; ++i)
            {
                if (flags[i] == 0 || flags[i] == 0xf)
                {
                    flags[i] = 0;
                    continue;
                }

                int reg = Spans[i].Reg;
                if (reg == 0 || (reg & RC_BORDER_REG) != 0)
                {
                    continue;
                }

                var area = Areas[i];
                var verts = WalkContour(x, y, i, flags);

                res.Add((reg, area, verts));
            }

            return res.ToArray();
        }
        /// <summary>
        /// Walks the contour
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="i">Index</param>
        /// <param name="dir">Direction</param>
        /// <param name="srcReg">Region list</param>
        /// <returns>Returns the contour list</returns>
        private int[] WalkContour(int x, int y, int i, int dir, int[] srcReg)
        {
            var cont = new List<int>();

            int startDir = dir;
            int starti = i;

            var ss = Spans[i];
            int curReg = 0;
            if (ss.GetCon(dir) != RC_NOT_CONNECTED)
            {
                int ax = x + Utils.GetDirOffsetX(dir);
                int ay = y + Utils.GetDirOffsetY(dir);
                int ai = Cells[ax + ay * Width].Index + ss.GetCon(dir);
                curReg = srcReg[ai];
            }
            cont.Add(curReg);

            int iter = 0;
            while (++iter < 40000)
            {
                var s = Spans[i];

                if (IsSolidEdge(srcReg, x, y, i, dir))
                {
                    // Choose the edge corner
                    int r = 0;
                    if (s.GetCon(dir) != RC_NOT_CONNECTED)
                    {
                        int ax = x + Utils.GetDirOffsetX(dir);
                        int ay = y + Utils.GetDirOffsetY(dir);
                        int ai = Cells[ax + ay * Width].Index + s.GetCon(dir);
                        r = srcReg[ai];
                    }

                    if (r != curReg)
                    {
                        curReg = r;
                        cont.Add(curReg);
                    }

                    dir = (dir + 1) & 0x3;  // Rotate CW
                }
                else
                {
                    int ni = -1;
                    int nx = x + Utils.GetDirOffsetX(dir);
                    int ny = y + Utils.GetDirOffsetY(dir);
                    if (s.GetCon(dir) != RC_NOT_CONNECTED)
                    {
                        var nc = Cells[nx + ny * Width];
                        ni = nc.Index + s.GetCon(dir);
                    }

                    if (ni == -1)
                    {
                        // Should not happen.
                        return Array.Empty<int>();
                    }

                    x = nx;
                    y = ny;
                    i = ni;
                    dir = (dir + 3) & 0x3;  // Rotate CCW
                }

                if (starti == i && startDir == dir)
                {
                    break;
                }
            }

            return cont.ToArray();
        }
        /// <summary>
        /// Walks the edge contour
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="i">Index</param>
        /// <param name="flags">Edge flags</param>
        /// <returns>Returns the edge contour list</returns>
        private Int4[] WalkContour(int x, int y, int i, int[] flags)
        {
            var points = new List<Int4>();

            // Choose the first non-connected edge
            int dir = 0;
            while ((flags[i] & (1 << dir)) == 0)
            {
                dir++;
            }

            int startDir = dir;
            int starti = i;

            var area = Areas[i];

            int iter = 0;
            while (++iter < 40000)
            {
                if ((flags[i] & (1 << dir)) != 0)
                {
                    // Choose the edge corner
                    var pt = GetEdgeCorner(x, y, i, dir, area);
                    points.Add(pt);

                    flags[i] &= ~(1 << dir); // Remove visited edges
                    dir = (dir + 1) & 0x3;  // Rotate CW
                }
                else
                {
                    int ni = -1;
                    int nx = x + Utils.GetDirOffsetX(dir);
                    int ny = y + Utils.GetDirOffsetY(dir);
                    var s = Spans[i];
                    if (s.GetCon(dir) != RC_NOT_CONNECTED)
                    {
                        var nc = Cells[nx + ny * Width];
                        ni = nc.Index + s.GetCon(dir);
                    }
                    if (ni == -1)
                    {
                        // Should not happen.
                        return Array.Empty<Int4>();
                    }
                    x = nx;
                    y = ny;
                    i = ni;
                    dir = (dir + 3) & 0x3;  // Rotate CCW
                }

                if (starti == i && startDir == dir)
                {
                    break;
                }
            }

            return points.ToArray();
        }
        /// <summary>
        /// Gets the corner edge
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="i">Index</param>
        /// <param name="dir">Direction</param>
        /// <param name="area">Area type</param>
        private Int4 GetEdgeCorner(int x, int y, int i, int dir, AreaTypes area)
        {
            bool isAreaBorder = false;
            int px = x;
            int py = GetCornerHeight(x, y, i, dir, out bool isBorderVertex);
            int pz = y;
            switch (dir)
            {
                case 0: pz++; break;
                case 1: px++; pz++; break;
                case 2: px++; break;
            }
            int r = 0;
            var s = Spans[i];
            if (s.GetCon(dir) != RC_NOT_CONNECTED)
            {
                int ax = x + Utils.GetDirOffsetX(dir);
                int ay = y + Utils.GetDirOffsetY(dir);
                int ai = Cells[ax + ay * Width].Index + s.GetCon(dir);
                r = Spans[ai].Reg;
                if (area != Areas[ai])
                {
                    isAreaBorder = true;
                }
            }
            if (isBorderVertex)
            {
                r |= ContourSet.RC_BORDER_VERTEX;
            }
            if (isAreaBorder)
            {
                r |= ContourSet.RC_AREA_BORDER;
            }

            return new(px, py, pz, r);
        }
    }
}
