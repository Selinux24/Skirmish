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
        /// Builds a new compact heightfield
        /// </summary>
        /// <param name="hf">Heightfield</param>
        /// <param name="walkableHeight">Walkable height</param>
        /// <param name="walkableClimb">Walkable climb</param>
        /// <returns>Returns the new compact heightfield</returns>
        public static CompactHeightfield Build(Heightfield hf, int walkableHeight, int walkableClimb)
        {
            int w = hf.Width;
            int h = hf.Height;
            int spanCount = hf.GetSpanCount();
            var bbox = hf.BoundingBox;
            bbox.Maximum.Y += walkableHeight * hf.CellHeight;

            // Fill in header.
            CompactHeightfield chf = new CompactHeightfield
            {
                Width = w,
                Height = h,
                SpanCount = spanCount,
                WalkableHeight = walkableHeight,
                WalkableClimb = walkableClimb,
                MaxRegions = 0,
                BoundingBox = bbox,
                CellSize = hf.CellSize,
                CellHeight = hf.CellHeight,
                Cells = new CompactCell[w * h],
                Spans = new CompactSpan[spanCount],
                Areas = new AreaTypes[spanCount]
            };

            // Fill in cells and spans.
            int idx = 0;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var s = hf.Spans[x + y * w];

                    // If there are no spans at this cell, just leave the data to index=0, count=0.
                    if (s == null)
                    {
                        continue;
                    }

                    var c = new CompactCell
                    {
                        Index = idx,
                        Count = 0
                    };

                    while (s != null)
                    {
                        if (s.area != AreaTypes.RC_NULL_AREA)
                        {
                            int bot = s.smax;
                            int top = s.next != null ? s.next.smin : int.MaxValue;
                            chf.Spans[idx].Y = MathUtil.Clamp(bot, 0, 0xffff);
                            chf.Spans[idx].H = MathUtil.Clamp(top - bot, 0, 0xff);
                            chf.Areas[idx] = s.area;
                            idx++;
                            c.Count++;
                        }

                        s = s.next;
                    }

                    chf.Cells[x + y * w] = c;
                }
            }

            // Find neighbour connections.
            int maxLayers = ContourSet.RC_NOT_CONNECTED - 1;
            int tooHighNeighbour = 0;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.Cells[x + y * w];

                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; i++)
                    {
                        var s = chf.Spans[i];

                        for (int dir = 0; dir < 4; dir++)
                        {
                            s.SetCon(dir, ContourSet.RC_NOT_CONNECTED);
                            int nx = x + RecastUtils.GetDirOffsetX(dir);
                            int ny = y + RecastUtils.GetDirOffsetY(dir);
                            // First check that the neighbour cell is in bounds.
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                            {
                                continue;
                            }

                            // Iterate over all neighbour spans and check if any of the is
                            // accessible from current cell.
                            var nc = chf.Cells[nx + ny * w];

                            for (int k = nc.Index, nk = (nc.Index + nc.Count); k < nk; ++k)
                            {
                                var ns = chf.Spans[k];

                                int bot = Math.Max(s.Y, ns.Y);
                                int top = Math.Min(s.Y + s.H, ns.Y + ns.H);

                                // Check that the gap between the spans is walkable,
                                // and that the climb height between the gaps is not too high.
                                if ((top - bot) >= walkableHeight && Math.Abs(ns.Y - s.Y) <= walkableClimb)
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

                        chf.Spans[i] = s;
                    }
                }
            }

            if (tooHighNeighbour > maxLayers)
            {
                throw new EngineException(string.Format("Heightfield has too many layers {0} (max: {1})", tooHighNeighbour, maxLayers));
            }

            return chf;
        }
        private static void InsertSort(AreaTypes[] a, int n)
        {
            int i, j;
            for (i = 1; i < n; i++)
            {
                var value = a[i];
                for (j = i - 1; j >= 0 && a[j] > value; j--)
                {
                    a[j + 1] = a[j];
                }
                a[j + 1] = value;
            }
        }
        private static void AppendStacks(List<LevelStackEntry> srcStack, List<LevelStackEntry> dstStack, int[] srcReg)
        {
            for (int j = 0; j < srcStack.Count; j++)
            {
                int i = srcStack[j].Index;
                if ((i < 0) || (srcReg[i] != 0))
                {
                    continue;
                }
                dstStack.Add(srcStack[j]);
            }
        }
        private static float GetJitterX(int i)
        {
            return (((i * 0x8da6b343) & 0xffff) / 65535.0f * 2.0f) - 1.0f;
        }
        private static float GetJitterY(int i)
        {
            return (((i * 0xd8163841) & 0xffff) / 65535.0f * 2.0f) - 1.0f;
        }

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
        /// Marks the geometry areas into the heightfield
        /// </summary>
        /// <param name="geometry">Geometry input</param>
        public void MarkAreas(InputGeometry geometry)
        {
            var vols = geometry.GetAreas();
            var vCount = geometry.GetAreaCount();
            for (int i = 0; i < vCount; i++)
            {
                var vol = vols.ElementAt(i);

                this.MarkConvexPolyArea(
                    vol.Vertices, vol.VertexCount,
                    vol.MinHeight, vol.MaxHeight,
                    (AreaTypes)vol.AreaType);
            }
        }
        /// <summary>
        /// Basically, any spans that are closer to a boundary or obstruction than the specified radius are marked as unwalkable.
        /// </summary>
        /// <param name="radius">Radius</param>
        /// <param name="chf">Compact height field</param>
        /// <returns>Returns always true</returns>
        /// <remarks>
        /// This method is usually called immediately after the heightfield has been built.
        /// </remarks>
        public bool ErodeWalkableArea(int radius)
        {
            int w = this.Width;
            int h = this.Height;

            // Init distance.
            int[] dist = Helper.CreateArray(this.SpanCount, 0xff);

            // Mark boundary cells.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    CompactCell c = this.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        if (this.Areas[i] == AreaTypes.RC_NULL_AREA)
                        {
                            dist[i] = 0;
                        }
                        else
                        {
                            CompactSpan s = this.Spans[i];
                            int nc = 0;
                            for (int dir = 0; dir < 4; ++dir)
                            {
                                if (s.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
                                {
                                    int nx = x + RecastUtils.GetDirOffsetX(dir);
                                    int ny = y + RecastUtils.GetDirOffsetY(dir);
                                    int nidx = this.Cells[nx + ny * w].Index + s.GetCon(dir);
                                    if (this.Areas[nidx] != AreaTypes.RC_NULL_AREA)
                                    {
                                        nc++;
                                    }
                                }
                            }
                            // At least one missing neighbour.
                            if (nc != 4)
                            {
                                dist[i] = 0;
                            }
                        }
                    }
                }
            }

            int nd;

            // Pass 1
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    CompactCell c = this.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        CompactSpan s = this.Spans[i];
                        if (s.GetCon(0) != ContourSet.RC_NOT_CONNECTED)
                        {
                            // (-1,0)
                            int ax = x + RecastUtils.GetDirOffsetX(0);
                            int ay = y + RecastUtils.GetDirOffsetY(0);
                            int ai = this.Cells[ax + ay * w].Index + s.GetCon(0);
                            CompactSpan asp = this.Spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (-1,-1)
                            if (asp.GetCon(3) != ContourSet.RC_NOT_CONNECTED)
                            {
                                int aax = ax + RecastUtils.GetDirOffsetX(3);
                                int aay = ay + RecastUtils.GetDirOffsetY(3);
                                int aai = this.Cells[aax + aay * w].Index + asp.GetCon(3);
                                nd = Math.Min(dist[aai] + 3, 255);
                                if (nd < dist[i])
                                {
                                    dist[i] = nd;
                                }
                            }
                        }
                        if (s.GetCon(3) != ContourSet.RC_NOT_CONNECTED)
                        {
                            // (0,-1)
                            int ax = x + RecastUtils.GetDirOffsetX(3);
                            int ay = y + RecastUtils.GetDirOffsetY(3);
                            int ai = this.Cells[ax + ay * w].Index + s.GetCon(3);
                            CompactSpan asp = this.Spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (1,-1)
                            if (asp.GetCon(2) != ContourSet.RC_NOT_CONNECTED)
                            {
                                int aax = ax + RecastUtils.GetDirOffsetX(2);
                                int aay = ay + RecastUtils.GetDirOffsetY(2);
                                int aai = this.Cells[aax + aay * w].Index + asp.GetCon(2);
                                nd = Math.Min(dist[aai] + 3, 255);
                                if (nd < dist[i])
                                {
                                    dist[i] = nd;
                                }
                            }
                        }
                    }
                }
            }

            // Pass 2
            for (int y = h - 1; y >= 0; --y)
            {
                for (int x = w - 1; x >= 0; --x)
                {
                    var c = this.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = this.Spans[i];
                        if (s.GetCon(2) != ContourSet.RC_NOT_CONNECTED)
                        {
                            // (1,0)
                            int ax = x + RecastUtils.GetDirOffsetX(2);
                            int ay = y + RecastUtils.GetDirOffsetY(2);
                            int ai = this.Cells[ax + ay * w].Index + s.GetCon(2);
                            var asp = this.Spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (1,1)
                            if (asp.GetCon(1) != ContourSet.RC_NOT_CONNECTED)
                            {
                                int aax = ax + RecastUtils.GetDirOffsetX(1);
                                int aay = ay + RecastUtils.GetDirOffsetY(1);
                                int aai = this.Cells[aax + aay * w].Index + asp.GetCon(1);
                                nd = Math.Min(dist[aai] + 3, 255);
                                if (nd < dist[i])
                                {
                                    dist[i] = nd;
                                }
                            }
                        }
                        if (s.GetCon(1) != ContourSet.RC_NOT_CONNECTED)
                        {
                            // (0,1)
                            int ax = x + RecastUtils.GetDirOffsetX(1);
                            int ay = y + RecastUtils.GetDirOffsetY(1);
                            int ai = this.Cells[ax + ay * w].Index + s.GetCon(1);
                            var asp = this.Spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (-1,1)
                            if (asp.GetCon(0) != ContourSet.RC_NOT_CONNECTED)
                            {
                                int aax = ax + RecastUtils.GetDirOffsetX(0);
                                int aay = ay + RecastUtils.GetDirOffsetY(0);
                                int aai = this.Cells[aax + aay * w].Index + asp.GetCon(0);
                                nd = Math.Min(dist[aai] + 3, 255);
                                if (nd < dist[i])
                                {
                                    dist[i] = nd;
                                }
                            }
                        }
                    }
                }
            }

            int thr = radius * 2;
            for (int i = 0; i < this.SpanCount; ++i)
            {
                if (dist[i] < thr)
                {
                    this.Areas[i] = AreaTypes.RC_NULL_AREA;
                }
            }

            return true;
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
            var s = this.Spans[i];
            int r = 0;
            if (s.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
            {
                int ax = x + RecastUtils.GetDirOffsetX(dir);
                int ay = y + RecastUtils.GetDirOffsetY(dir);
                int ai = this.Cells[ax + ay * this.Width].Index + s.GetCon(dir);
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

            var s = this.Spans[i];
            int ch = s.Y;
            int dirp = (dir + 1) & 0x3;

            int[] regs = { 0, 0, 0, 0 };

            // Combine region and area codes in order to prevent
            // border vertices which are in between two areas to be removed.
            regs[0] = this.Spans[i].Reg | ((int)this.Areas[i] << 16);

            if (s.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
            {
                int ax = x + RecastUtils.GetDirOffsetX(dir);
                int ay = y + RecastUtils.GetDirOffsetY(dir);
                int ai = this.Cells[ax + ay * this.Width].Index + s.GetCon(dir);
                var a = this.Spans[ai];
                ch = Math.Max(ch, a.Y);
                regs[1] = this.Spans[ai].Reg | ((int)this.Areas[ai] << 16);
                if (a.GetCon(dirp) != ContourSet.RC_NOT_CONNECTED)
                {
                    int ax2 = ax + RecastUtils.GetDirOffsetX(dirp);
                    int ay2 = ay + RecastUtils.GetDirOffsetY(dirp);
                    int ai2 = this.Cells[ax2 + ay2 * this.Width].Index + a.GetCon(dirp);
                    var as2 = this.Spans[ai2];
                    ch = Math.Max(ch, as2.Y);
                    regs[2] = this.Spans[ai2].Reg | ((int)this.Areas[ai2] << 16);
                }
            }
            if (s.GetCon(dirp) != ContourSet.RC_NOT_CONNECTED)
            {
                int ax = x + RecastUtils.GetDirOffsetX(dirp);
                int ay = y + RecastUtils.GetDirOffsetY(dirp);
                int ai = this.Cells[ax + ay * this.Width].Index + s.GetCon(dirp);
                var a = this.Spans[ai];
                ch = Math.Max(ch, a.Y);
                regs[3] = this.Spans[ai].Reg | ((int)this.Areas[ai] << 16);
                if (a.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
                {
                    int ax2 = ax + RecastUtils.GetDirOffsetX(dir);
                    int ay2 = ay + RecastUtils.GetDirOffsetY(dir);
                    int ai2 = this.Cells[ax2 + ay2 * this.Width].Index + a.GetCon(dir);
                    var as2 = this.Spans[ai2];
                    ch = Math.Max(ch, as2.Y);
                    regs[2] = this.Spans[ai2].Reg | ((int)this.Areas[ai2] << 16);
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
                bool twoSameExts = (regs[a] & regs[b] & ContourSet.RC_BORDER_REG) != 0 && regs[a] == regs[b];
                bool twoInts = ((regs[c] | regs[d]) & ContourSet.RC_BORDER_REG) == 0;
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
        public void GetHeightData(IndexedPolygon poly, Int3[] verts, int borderSize, HeightPatch hp, int region)
        {
            List<HeightDataItem> queue = new List<HeightDataItem>(512);

            // Set all heights to RC_UNSET_HEIGHT.
            hp.Data = Helper.CreateArray(hp.Bounds.Width * hp.Bounds.Height, HeightPatch.RC_UNSET_HEIGHT);

            bool empty = true;

            // We cannot sample from this poly if it was created from polys
            // of different regions. If it was then it could potentially be overlapping
            // with polys of that region and the heights sampled here could be wrong.
            if (!IndexedPolygon.HasMultipleRegions(region))
            {
                // Copy the height from the same region, and mark region borders
                // as seed points to fill the rest.
                for (int hy = 0; hy < hp.Bounds.Height; hy++)
                {
                    int y = hp.Bounds.Y + hy + borderSize;
                    for (int hx = 0; hx < hp.Bounds.Width; hx++)
                    {
                        int x = hp.Bounds.X + hx + borderSize;
                        var c = Cells[x + y * Width];
                        for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                        {
                            var s = Spans[i];
                            if (s.Reg == region)
                            {
                                // Store height
                                hp.Data[hx + hy * hp.Bounds.Width] = s.Y;
                                empty = false;

                                // If any of the neighbours is not in same region,
                                // add the current location as flood fill start
                                bool border = false;
                                for (int dir = 0; dir < 4; ++dir)
                                {
                                    if (s.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
                                    {
                                        int ax = x + RecastUtils.GetDirOffsetX(dir);
                                        int ay = y + RecastUtils.GetDirOffsetY(dir);
                                        int ai = Cells[ax + ay * Width].Index + s.GetCon(dir);
                                        var a = Spans[ai];
                                        if (a.Reg != region)
                                        {
                                            border = true;
                                            break;
                                        }
                                    }
                                }
                                if (border)
                                {
                                    queue.Add(new HeightDataItem
                                    {
                                        X = x,
                                        Y = y,
                                        I = i
                                    });
                                }
                                break;
                            }
                        }
                    }
                }
            }

            // if the polygon does not contain any points from the current region (rare, but happens)
            // or if it could potentially be overlapping polygons of the same region,
            // then use the center as the seed point.
            if (empty)
            {
                queue.AddRange(SeedArrayWithPolyCenter(poly, verts, borderSize, hp));
            }

            int RETRACT_SIZE = 256;
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
                for (int dir = 0; dir < 4; dir++)
                {
                    if (cs.GetCon(dir) == ContourSet.RC_NOT_CONNECTED)
                    {
                        continue;
                    }

                    int ax = c.X + RecastUtils.GetDirOffsetX(dir);
                    int ay = c.Y + RecastUtils.GetDirOffsetY(dir);
                    int hx = ax - hp.Bounds.X - borderSize;
                    int hy = ay - hp.Bounds.Y - borderSize;

                    if (hx < 0 || hy < 0 || hx >= hp.Bounds.Width || hy >= hp.Bounds.Height)
                    {
                        continue;
                    }

                    if (hp.Data[hx + hy * hp.Bounds.Width] != HeightPatch.RC_UNSET_HEIGHT)
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
        public void BuildPolyDetail(IEnumerable<Vector3> polygon, BuildPolyDetailParams param, HeightPatch hp, out Vector3[] outVerts, out Int3[] outTris)
        {
            float sampleDist = param.SampleDist;
            float sampleMaxError = param.SampleMaxError;
            int heightSearchRadius = param.HeightSearchRadius;
            int ninp = polygon.Count();
            List<Vector3> verts = new List<Vector3>();
            List<Int4> edges = new List<Int4>();
            List<Int4> samples = new List<Int4>();
            List<Int3> tris = new List<Int3>();

            int MAX_VERTS = 127;
            int MAX_TRIS = 255;    // Max tris for delaunay is 2n-2-k (n=num verts, k=num hull verts).
            int MAX_VERTS_PER_EDGE = 32;
            Vector3[] edge = new Vector3[(MAX_VERTS_PER_EDGE + 1)];
            int[] hull = new int[MAX_VERTS];
            int nhull = 0;

            for (int i = 0; i < ninp; i++)
            {
                verts.Add(polygon.ElementAt(i));
            }

            edges.Clear();

            float cs = CellSize;
            float ics = 1.0f / cs;

            // Calculate minimum extents of the polygon based on input data.
            float minExtent = RecastUtils.PolyMinExtent(verts.ToArray());

            // Tessellate outlines.
            // This is done in separate pass in order to ensure
            // seamless height values across the ply boundaries.
            if (sampleDist > 0)
            {
                for (int i = 0, j = ninp - 1; i < ninp; j = i++)
                {
                    var vj = polygon.ElementAt(j);
                    var vi = polygon.ElementAt(i);
                    bool swapped = false;
                    // Make sure the segments are always handled in same order
                    // using lexological sort or else there will be seams.
                    if (Math.Abs(vj.X - vi.X) < 1e-6f)
                    {
                        if (vj.Z > vi.Z)
                        {
                            Helper.Swap(ref vj, ref vi);
                            swapped = true;
                        }
                    }
                    else
                    {
                        if (vj.X > vi.X)
                        {
                            Helper.Swap(ref vj, ref vi);
                            swapped = true;
                        }
                    }
                    // Create samples along the edge.
                    float dx = vi.X - vj.X;
                    float dy = vi.Y - vj.Y;
                    float dz = vi.Z - vj.Z;
                    float d = (float)Math.Sqrt(dx * dx + dz * dz);
                    int nn = 1 + (int)Math.Floor(d / sampleDist);
                    if (nn >= MAX_VERTS_PER_EDGE) nn = MAX_VERTS_PER_EDGE - 1;
                    if (verts.Count + nn >= MAX_VERTS)
                    {
                        nn = MAX_VERTS - 1 - verts.Count;
                    }

                    for (int k = 0; k <= nn; ++k)
                    {
                        float u = ((float)k / (float)nn);
                        Vector3 pos = new Vector3
                        {
                            X = vj.X + dx * u,
                            Y = vj.Y + dy * u,
                            Z = vj.Z + dz * u
                        };
                        pos.Y = hp.GetHeight(pos, ics, CellHeight, heightSearchRadius) * CellHeight;
                        edge[k] = pos;
                    }
                    // Simplify samples.
                    int[] idx = new int[MAX_VERTS_PER_EDGE];
                    idx[0] = 0;
                    idx[1] = nn;
                    int nidx = 2;
                    for (int k = 0; k < nidx - 1;)
                    {
                        int a = idx[k];
                        int b = idx[k + 1];
                        var va = edge[a];
                        var vb = edge[b];
                        // Find maximum deviation along the segment.
                        float maxd = 0;
                        int maxi = -1;
                        for (int m = a + 1; m < b; ++m)
                        {
                            float dev = RecastUtils.DistancePtSeg(edge[m], va, vb);
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

                    hull[nhull++] = j;
                    // Add new vertices.
                    if (swapped)
                    {
                        for (int k = nidx - 2; k > 0; --k)
                        {
                            verts.Add(edge[idx[k]]);
                            hull[nhull++] = verts.Count - 1;
                        }
                    }
                    else
                    {
                        for (int k = 1; k < nidx - 1; ++k)
                        {
                            verts.Add(edge[idx[k]]);
                            hull[nhull++] = verts.Count - 1;
                        }
                    }
                }
            }

            // If the polygon minimum extent is small (sliver or small triangle), do not try to add internal points.
            if (minExtent < sampleDist * 2)
            {
                RecastUtils.TriangulateHull(verts.ToArray(), nhull, hull, ninp, tris);

                outVerts = verts.ToArray();
                outTris = tris.ToArray();

                return;
            }

            // Tessellate the base mesh.
            // We're using the triangulateHull instead of delaunayHull as it tends to
            // create a bit better triangulation for long thin triangles when there
            // are no internal points.
            RecastUtils.TriangulateHull(verts.ToArray(), nhull, hull, ninp, tris);

            if (tris.Count == 0)
            {
                // Could not triangulate the poly, make sure there is some valid data there.
                Logger.WriteWarning($"buildPolyDetail: Could not triangulate polygon ({verts.Count} verts).");

                outVerts = verts.ToArray();
                outTris = tris.ToArray();

                return;
            }

            if (sampleDist > 0)
            {
                // Create sample locations in a grid.
                Vector3 bmin, bmax;
                bmin = polygon.ElementAt(0);
                bmax = polygon.ElementAt(0);
                for (int i = 1; i < ninp; ++i)
                {
                    bmin = Vector3.Min(bmin, polygon.ElementAt(i));
                    bmax = Vector3.Max(bmax, polygon.ElementAt(i));
                }
                int x0 = (int)Math.Floor(bmin.X / sampleDist);
                int x1 = (int)Math.Ceiling(bmax.X / sampleDist);
                int z0 = (int)Math.Floor(bmin.Z / sampleDist);
                int z1 = (int)Math.Ceiling(bmax.Z / sampleDist);
                samples.Clear();
                for (int z = z0; z < z1; ++z)
                {
                    for (int x = x0; x < x1; ++x)
                    {
                        Vector3 pt = new Vector3
                        {
                            X = x * sampleDist,
                            Y = (bmax.Y + bmin.Y) * 0.5f,
                            Z = z * sampleDist
                        };

                        // Make sure the samples are not too close to the edges.
                        if (RecastUtils.DistToPoly(polygon, pt) > -sampleDist / 2)
                        {
                            continue;
                        }

                        int y = hp.GetHeight(pt, ics, CellHeight, heightSearchRadius);

                        var sample = new Int4(x, y, z, 0);

                        samples.Add(sample); // Not added
                    }
                }

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
                    Vector3 bestpt = new Vector3();
                    float bestd = 0;
                    int besti = -1;
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

                        Vector3 pt = new Vector3
                        {
                            X = s.X * sampleDist + GetJitterX(i) * cs * 0.1f,
                            Y = s.Y * CellHeight,
                            Z = s.Z * sampleDist + GetJitterY(i) * cs * 0.1f
                        };
                        float d = RecastUtils.DistToTriMesh(verts, tris, pt);
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
                    DelaunayHull.Build(verts, hull.Take(nhull), out var dTris, out var dEdges);
                    edges.Clear();
                    edges.AddRange(dEdges);
                    tris.Clear();
                    tris.AddRange(dTris);
                }
            }

            int ntris = tris.Count;
            if (ntris > MAX_TRIS)
            {
                tris.RemoveRange(MAX_TRIS, ntris - MAX_TRIS);
                Logger.WriteDebug($"rcBuildPolyMeshDetail: Shrinking triangle count from {ntris} to max {MAX_TRIS}.");
            }

            outVerts = verts.ToArray();
            outTris = tris.ToArray();
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
        private IEnumerable<HeightDataItem> SeedArrayWithPolyCenter(IndexedPolygon poly, Int3[] verts, int borderSize, HeightPatch hp)
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
            List<HeightDataItem> array = new List<HeightDataItem>(512)
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
            HeightDataItem hdItem = new HeightDataItem();
            while (true)
            {
                if (array.Count < 1)
                {
                    Logger.WriteWarning("Walk towards polygon center failed to reach center");
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
                    directDir = RecastUtils.GetDirForOffset(0, pcy > hdItem.Y ? 1 : -1);
                }
                else
                {
                    directDir = RecastUtils.GetDirForOffset(pcx > hdItem.X ? 1 : -1, 0);
                }

                // Push the direct dir last so we start with this on next iteration
                Helper.Swap(ref dirs[directDir], ref dirs[3]);

                var cs = Spans[hdItem.I];
                for (int i = 0; i < 4; i++)
                {
                    int dir = dirs[i];
                    if (cs.GetCon(dir) == ContourSet.RC_NOT_CONNECTED)
                    {
                        continue;
                    }

                    int newX = hdItem.X + RecastUtils.GetDirOffsetX(dir);
                    int newY = hdItem.Y + RecastUtils.GetDirOffsetY(dir);

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

            hp.Data = Helper.CreateArray(hp.Bounds.Width * hp.Bounds.Height, 0xff);
            var chs = Spans[hdItem.I];
            hp.Data[hdItem.X - hp.Bounds.X + (hdItem.Y - hp.Bounds.Y) * hp.Bounds.Width] = chs.Y;

            return array;
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
        public IEnumerable<int> WalkContour(int x, int y, int i, int dir, int[] srcReg)
        {
            List<int> cont = new List<int>();

            int startDir = dir;
            int starti = i;

            var ss = this.Spans[i];
            int curReg = 0;
            if (ss.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
            {
                int ax = x + RecastUtils.GetDirOffsetX(dir);
                int ay = y + RecastUtils.GetDirOffsetY(dir);
                int ai = this.Cells[ax + ay * this.Width].Index + ss.GetCon(dir);
                curReg = srcReg[ai];
            }
            cont.Add(curReg);

            int iter = 0;
            while (++iter < 40000)
            {
                var s = this.Spans[i];

                if (IsSolidEdge(srcReg, x, y, i, dir))
                {
                    // Choose the edge corner
                    int r = 0;
                    if (s.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
                    {
                        int ax = x + RecastUtils.GetDirOffsetX(dir);
                        int ay = y + RecastUtils.GetDirOffsetY(dir);
                        int ai = this.Cells[ax + ay * this.Width].Index + s.GetCon(dir);
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
                    int nx = x + RecastUtils.GetDirOffsetX(dir);
                    int ny = y + RecastUtils.GetDirOffsetY(dir);
                    if (s.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
                    {
                        var nc = this.Cells[nx + ny * this.Width];
                        ni = nc.Index + s.GetCon(dir);
                    }
                    if (ni == -1)
                    {
                        // Should not happen.
                        return new int[] { };
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

            return cont;
        }
        /// <summary>
        /// Walks the edge contour
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="i">Index</param>
        /// <param name="flags">Edge flags</param>
        /// <returns>Returns the edge contour list</returns>
        public IEnumerable<Int4> WalkContour(int x, int y, int i, int[] flags)
        {
            List<Int4> points = new List<Int4>();

            // Choose the first non-connected edge
            int dir = 0;
            while ((flags[i] & (1 << dir)) == 0)
            {
                dir++;
            }

            int startDir = dir;
            int starti = i;

            var area = this.Areas[i];

            int iter = 0;
            while (++iter < 40000)
            {
                if ((flags[i] & (1 << dir)) != 0)
                {
                    // Choose the edge corner
                    bool isAreaBorder = false;
                    int px = x;
                    int py = this.GetCornerHeight(x, y, i, dir, out bool isBorderVertex);
                    int pz = y;
                    switch (dir)
                    {
                        case 0: pz++; break;
                        case 1: px++; pz++; break;
                        case 2: px++; break;
                    }
                    int r = 0;
                    var s = this.Spans[i];
                    if (s.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
                    {
                        int ax = x + RecastUtils.GetDirOffsetX(dir);
                        int ay = y + RecastUtils.GetDirOffsetY(dir);
                        int ai = this.Cells[ax + ay * this.Width].Index + s.GetCon(dir);
                        r = this.Spans[ai].Reg;
                        if (area != this.Areas[ai])
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
                    points.Add(new Int4(px, py, pz, r));

                    flags[i] &= ~(1 << dir); // Remove visited edges
                    dir = (dir + 1) & 0x3;  // Rotate CW
                }
                else
                {
                    int ni = -1;
                    int nx = x + RecastUtils.GetDirOffsetX(dir);
                    int ny = y + RecastUtils.GetDirOffsetY(dir);
                    var s = this.Spans[i];
                    if (s.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
                    {
                        var nc = this.Cells[nx + ny * this.Width];
                        ni = nc.Index + s.GetCon(dir);
                    }
                    if (ni == -1)
                    {
                        // Should not happen.
                        return new Int4[] { };
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

            return points;
        }
        /// <summary>
        /// This filter is usually applied after applying area id's using functions such as MarkBoxArea, MarkConvexPolyArea, and MarkCylinderArea.
        /// </summary>
        /// <param name="chf">Compact height field</param>
        /// <returns>Returns always true</returns>
        public bool MedianFilterWalkableArea()
        {
            int w = Width;
            int h = Height;

            // Init distance.
            AreaTypes[] areas = Helper.CreateArray(SpanCount, (AreaTypes)0xff);

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = Spans[i];
                        if (Areas[i] == AreaTypes.RC_NULL_AREA)
                        {
                            areas[i] = Areas[i];
                            continue;
                        }

                        AreaTypes[] nei = new AreaTypes[9];
                        for (int j = 0; j < 9; ++j)
                        {
                            nei[j] = Areas[i];
                        }

                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (s.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
                            {
                                int ax = x + RecastUtils.GetDirOffsetX(dir);
                                int ay = y + RecastUtils.GetDirOffsetY(dir);
                                int ai = Cells[ax + ay * w].Index + s.GetCon(dir);
                                if (Areas[ai] != AreaTypes.RC_NULL_AREA)
                                {
                                    nei[dir * 2 + 0] = Areas[ai];
                                }

                                var a = Spans[ai];
                                int dir2 = (dir + 1) & 0x3;
                                if (a.GetCon(dir2) != ContourSet.RC_NOT_CONNECTED)
                                {
                                    int ax2 = ax + RecastUtils.GetDirOffsetX(dir2);
                                    int ay2 = ay + RecastUtils.GetDirOffsetY(dir2);
                                    int ai2 = Cells[ax2 + ay2 * w].Index + a.GetCon(dir2);
                                    if (Areas[ai2] != AreaTypes.RC_NULL_AREA)
                                    {
                                        nei[dir * 2 + 1] = Areas[ai2];
                                    }
                                }
                            }
                        }
                        InsertSort(nei, 9);
                        areas[i] = nei[4];
                    }
                }
            }

            Array.Copy(areas, Areas, SpanCount);

            return true;
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
        public void MarkBoxArea(Vector3 bmin, Vector3 bmax, AreaTypes areaId)
        {
            int minx = (int)((bmin.X - BoundingBox.Minimum.X) / CellSize);
            int miny = (int)((bmin.Y - BoundingBox.Minimum.Y) / CellHeight);
            int minz = (int)((bmin.Z - BoundingBox.Minimum.Z) / CellSize);
            int maxx = (int)((bmax.X - BoundingBox.Minimum.X) / CellSize);
            int maxy = (int)((bmax.Y - BoundingBox.Minimum.Y) / CellHeight);
            int maxz = (int)((bmax.Z - BoundingBox.Minimum.Z) / CellSize);

            if (maxx < 0) return;
            if (minx >= Width) return;
            if (maxz < 0) return;
            if (minz >= Height) return;

            if (minx < 0) minx = 0;
            if (maxx >= Width) maxx = Width - 1;
            if (minz < 0) minz = 0;
            if (maxz >= Height) maxz = Height - 1;

            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    var c = Cells[x + z * Width];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = Spans[i];
                        if (s.Y >= miny && s.Y <= maxy && Areas[i] != AreaTypes.RC_NULL_AREA)
                        {
                            Areas[i] = areaId;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="nverts"></param>
        /// <param name="hmin"></param>
        /// <param name="hmax"></param>
        /// <param name="areaId"></param>
        /// <param name="chf"></param>
        /// <remarks>
        /// The value of spacial parameters are in world units.
        /// The y-values of the polygon vertices are ignored. So the polygon is effectively projected onto the xz-plane at hmin, then extruded to hmax.
        /// </remarks>
        public void MarkConvexPolyArea(Vector3[] verts, int nverts, float hmin, float hmax, AreaTypes areaId)
        {
            Vector3 bmin = verts[0];
            Vector3 bmax = verts[0];
            for (int i = 1; i < nverts; ++i)
            {
                Vector3.Min(bmin, verts[i * 3]);
                Vector3.Max(bmax, verts[i * 3]);
            }
            bmin.Y = hmin;
            bmax.Y = hmax;

            int minx = (int)((bmin.X - BoundingBox.Minimum.X) / CellSize);
            int miny = (int)((bmin.Y - BoundingBox.Minimum.Y) / CellHeight);
            int minz = (int)((bmin.Z - BoundingBox.Minimum.Z) / CellSize);
            int maxx = (int)((bmax.X - BoundingBox.Minimum.X) / CellSize);
            int maxy = (int)((bmax.Y - BoundingBox.Minimum.Y) / CellHeight);
            int maxz = (int)((bmax.Z - BoundingBox.Minimum.Z) / CellSize);

            if (maxx < 0) return;
            if (minx >= Width) return;
            if (maxz < 0) return;
            if (minz >= Height) return;

            if (minx < 0) minx = 0;
            if (maxx >= Width) maxx = Width - 1;
            if (minz < 0) minz = 0;
            if (maxz >= Height) maxz = Height - 1;

            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    CompactCell c = Cells[x + z * Width];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        CompactSpan s = Spans[i];
                        if (Areas[i] == AreaTypes.RC_NULL_AREA)
                        {
                            continue;
                        }

                        if (s.Y >= miny && s.Y <= maxy)
                        {
                            Vector3 p = new Vector3
                            {
                                X = BoundingBox.Minimum.X + (x + 0.5f) * CellSize,
                                Y = 0,
                                Z = BoundingBox.Minimum.Z + (z + 0.5f) * CellSize
                            };

                            if (PointInPoly(nverts, verts, p))
                            {
                                Areas[i] = areaId;
                            }
                        }
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
        public void MarkCylinderArea(Vector3 pos, float r, float h, AreaTypes areaId)
        {
            Vector3 bmin = new Vector3();
            Vector3 bmax = new Vector3();
            bmin.X = pos.X - r;
            bmin.Y = pos.Y;
            bmin.Z = pos.Z - r;
            bmax.X = pos.X + r;
            bmax.Y = pos.Y + h;
            bmax.Z = pos.Z + r;
            float r2 = r * r;

            int minx = (int)((bmin.X - BoundingBox.Minimum.X) / CellSize);
            int miny = (int)((bmin.Y - BoundingBox.Minimum.Y) / CellHeight);
            int minz = (int)((bmin.Z - BoundingBox.Minimum.Z) / CellSize);
            int maxx = (int)((bmax.X - BoundingBox.Minimum.X) / CellSize);
            int maxy = (int)((bmax.Y - BoundingBox.Minimum.Y) / CellHeight);
            int maxz = (int)((bmax.Z - BoundingBox.Minimum.Z) / CellSize);

            if (maxx < 0) return;
            if (minx >= Width) return;
            if (maxz < 0) return;
            if (minz >= Height) return;

            if (minx < 0) minx = 0;
            if (maxx >= Width) maxx = Width - 1;
            if (minz < 0) minz = 0;
            if (maxz >= Height) maxz = Height - 1;

            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    var c = Cells[x + z * Width];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = Spans[i];

                        if (Areas[i] == AreaTypes.RC_NULL_AREA)
                        {
                            continue;
                        }

                        if (s.Y >= miny && s.Y <= maxy)
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
            }
        }
        /// <summary>
        /// Gets if the specified point is in the polygon
        /// </summary>
        /// <param name="nvert">Number of vertices in the polygon</param>
        /// <param name="verts">Polygon vertices</param>
        /// <param name="p">The point</param>
        /// <returns>Returns true if the point p is into the polygon, ignoring the Y component of p</returns>
        private static bool PointInPoly(int nvert, Vector3[] verts, Vector3 p)
        {
            bool c = false;

            for (int i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                var vi = verts[i];
                var vj = verts[j];
                if (((vi.Z > p.Z) != (vj.Z > p.Z)) &&
                    (p.X < (vj.X - vi.X) * (p.Z - vi.Z) / (vj.Z - vi.Z) + vi.X))
                {
                    c = !c;
                }
            }

            return c;
        }
        /// <summary>
        /// Builds the distance field
        /// </summary>
        public void BuildDistanceField()
        {
            int[] src = CalculateDistanceField();

            int[] dst = BoxBlur(1, src);

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

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = Spans[i];
                        var cd = src[i];
                        if (cd <= thr)
                        {
                            dst[i] = cd;
                            continue;
                        }

                        int d = cd;
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (s.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
                            {
                                int ax = x + RecastUtils.GetDirOffsetX(dir);
                                int ay = y + RecastUtils.GetDirOffsetY(dir);
                                int ai = Cells[ax + ay * w].Index + s.GetCon(dir);
                                d += src[ai];

                                var a = Spans[ai];
                                int dir2 = (dir + 1) & 0x3;
                                if (a.GetCon(dir2) != ContourSet.RC_NOT_CONNECTED)
                                {
                                    int ax2 = ax + RecastUtils.GetDirOffsetX(dir2);
                                    int ay2 = ay + RecastUtils.GetDirOffsetY(dir2);
                                    int ai2 = Cells[ax2 + ay2 * w].Index + a.GetCon(dir2);
                                    d += src[ai2];
                                }
                                else
                                {
                                    d += cd;
                                }
                            }
                            else
                            {
                                d += cd * 2;
                            }
                        }
                        dst[i] = ((d + 5) / 9);
                    }
                }
            }

            return dst;
        }
        /// <summary>
        /// Calculates the distance field
        /// </summary>
        /// <returns>Returns the array of distances</returns>
        public int[] CalculateDistanceField()
        {
            int[] res = new int[SpanCount];

            int w = Width;
            int h = Height;

            // Init distance and points.
            for (int i = 0; i < SpanCount; ++i)
            {
                res[i] = int.MaxValue;
            }

            // Mark boundary cells.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = Spans[i];
                        var area = Areas[i];

                        int nc = 0;
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (s.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
                            {
                                int ax = x + RecastUtils.GetDirOffsetX(dir);
                                int ay = y + RecastUtils.GetDirOffsetY(dir);
                                int ai = Cells[ax + ay * w].Index + s.GetCon(dir);
                                if (area == Areas[ai])
                                {
                                    nc++;
                                }
                            }
                        }
                        if (nc != 4)
                        {
                            res[i] = 0;
                        }
                    }
                }
            }

            // Pass 1
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = Spans[i];

                        if (s.GetCon(0) != ContourSet.RC_NOT_CONNECTED)
                        {
                            // (-1,0)
                            int ax = x + RecastUtils.GetDirOffsetX(0);
                            int ay = y + RecastUtils.GetDirOffsetY(0);
                            int ai = Cells[ax + ay * w].Index + s.GetCon(0);
                            var a = Spans[ai];
                            if (res[ai] + 2 < res[i])
                            {
                                res[i] = res[ai] + 2;
                            }

                            // (-1,-1)
                            if (a.GetCon(3) != ContourSet.RC_NOT_CONNECTED)
                            {
                                int aax = ax + RecastUtils.GetDirOffsetX(3);
                                int aay = ay + RecastUtils.GetDirOffsetY(3);
                                int aai = Cells[aax + aay * w].Index + a.GetCon(3);
                                if (res[aai] + 3 < res[i])
                                {
                                    res[i] = res[aai] + 3;
                                }
                            }
                        }
                        if (s.GetCon(3) != ContourSet.RC_NOT_CONNECTED)
                        {
                            // (0,-1)
                            int ax = x + RecastUtils.GetDirOffsetX(3);
                            int ay = y + RecastUtils.GetDirOffsetY(3);
                            int ai = Cells[ax + ay * w].Index + s.GetCon(3);
                            var a = Spans[ai];
                            if (res[ai] + 2 < res[i])
                            {
                                res[i] = res[ai] + 2;
                            }

                            // (1,-1)
                            if (a.GetCon(2) != ContourSet.RC_NOT_CONNECTED)
                            {
                                int aax = ax + RecastUtils.GetDirOffsetX(2);
                                int aay = ay + RecastUtils.GetDirOffsetY(2);
                                int aai = Cells[aax + aay * w].Index + a.GetCon(2);
                                if (res[aai] + 3 < res[i])
                                {
                                    res[i] = res[aai] + 3;
                                }
                            }
                        }
                    }
                }
            }

            // Pass 2
            for (int y = h - 1; y >= 0; --y)
            {
                for (int x = w - 1; x >= 0; --x)
                {
                    var c = Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = Spans[i];

                        if (s.GetCon(2) != ContourSet.RC_NOT_CONNECTED)
                        {
                            // (1,0)
                            int ax = x + RecastUtils.GetDirOffsetX(2);
                            int ay = y + RecastUtils.GetDirOffsetY(2);
                            int ai = Cells[ax + ay * w].Index + s.GetCon(2);
                            var a = Spans[ai];
                            if (res[ai] + 2 < res[i])
                            {
                                res[i] = res[ai] + 2;
                            }

                            // (1,1)
                            if (a.GetCon(1) != ContourSet.RC_NOT_CONNECTED)
                            {
                                int aax = ax + RecastUtils.GetDirOffsetX(1);
                                int aay = ay + RecastUtils.GetDirOffsetY(1);
                                int aai = Cells[aax + aay * w].Index + a.GetCon(1);
                                if (res[aai] + 3 < res[i])
                                {
                                    res[i] = res[aai] + 3;
                                }
                            }
                        }
                        if (s.GetCon(1) != ContourSet.RC_NOT_CONNECTED)
                        {
                            // (0,1)
                            int ax = x + RecastUtils.GetDirOffsetX(1);
                            int ay = y + RecastUtils.GetDirOffsetY(1);
                            int ai = Cells[ax + ay * w].Index + s.GetCon(1);
                            var a = Spans[ai];
                            if (res[ai] + 2 < res[i])
                            {
                                res[i] = res[ai] + 2;
                            }

                            // (-1,1)
                            if (a.GetCon(0) != ContourSet.RC_NOT_CONNECTED)
                            {
                                int aax = ax + RecastUtils.GetDirOffsetX(0);
                                int aay = ay + RecastUtils.GetDirOffsetY(0);
                                int aai = Cells[aax + aay * w].Index + a.GetCon(0);
                                if (res[aai] + 3 < res[i])
                                {
                                    res[i] = res[aai] + 3;
                                }
                            }
                        }
                    }
                }
            }

            int maxDist = 0;
            for (int i = 0; i < SpanCount; ++i)
            {
                maxDist = Math.Max(res[i], maxDist);
            }

            MaxDistance = maxDist;

            return res;
        }
        /// <summary>
        /// Builds monotone regions
        /// </summary>
        /// <param name="borderSize">Border size</param>
        /// <param name="minRegionArea">Minimum region area</param>
        /// <param name="mergeRegionArea">Merge region area</param>
        /// <returns>Returns true when the region were correctly built</returns>
        public bool BuildRegionsMonotone(int borderSize, int minRegionArea, int mergeRegionArea)
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
                PaintRectRegion(0, bw, 0, h, id | ContourSet.RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(w - bw, w, 0, h, id | ContourSet.RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(0, w, 0, bh, id | ContourSet.RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(0, w, h - bh, h, id | ContourSet.RC_BORDER_REG, srcReg); id++;
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
                        if (s.GetCon(0) != ContourSet.RC_NOT_CONNECTED)
                        {
                            int ax = x + RecastUtils.GetDirOffsetX(0);
                            int ay = y + RecastUtils.GetDirOffsetY(0);
                            int ai = Cells[ax + ay * w].Index + s.GetCon(0);
                            if ((srcReg[ai] & ContourSet.RC_BORDER_REG) == 0 && Areas[i] == Areas[ai])
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
                        if (s.GetCon(3) != ContourSet.RC_NOT_CONNECTED)
                        {
                            int ax = x + RecastUtils.GetDirOffsetX(3);
                            int ay = y + RecastUtils.GetDirOffsetY(3);
                            int ai = Cells[ax + ay * w].Index + s.GetCon(3);
                            if (srcReg[ai] != 0 && (srcReg[ai] & ContourSet.RC_BORDER_REG) == 0 && Areas[i] == Areas[ai])
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
        /// Builds regions
        /// </summary>
        /// <param name="borderSize">Border size</param>
        /// <param name="minRegionArea">Minimum region area</param>
        /// <param name="mergeRegionArea">Merge region area</param>
        /// <returns>Returns true when the region were correctly built</returns>
        public bool BuildRegions(int borderSize, int minRegionArea, int mergeRegionArea)
        {
            int w = Width;
            int h = Height;

            int LOG_NB_STACKS = 3;
            int NB_STACKS = 1 << LOG_NB_STACKS;
            List<List<LevelStackEntry>> lvlStacks = new List<List<LevelStackEntry>>(NB_STACKS);
            for (int i = 0; i < NB_STACKS; i++)
            {
                lvlStacks.Add(new List<LevelStackEntry>());
            }

            List<LevelStackEntry> stack = new List<LevelStackEntry>();

            int[] srcReg = new int[SpanCount];
            int[] srcDist = new int[SpanCount];

            int regionId = 1;
            int level = (MaxDistance + 1) & ~1;

            // TODO: Figure better formula, expandIters defines how much the 
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
                PaintRectRegion(0, bw, 0, h, (regionId | ContourSet.RC_BORDER_REG), srcReg); regionId++;
                PaintRectRegion(w - bw, w, 0, h, (regionId | ContourSet.RC_BORDER_REG), srcReg); regionId++;
                PaintRectRegion(0, w, 0, bh, (regionId | ContourSet.RC_BORDER_REG), srcReg); regionId++;
                PaintRectRegion(0, w, h - bh, h, (regionId | ContourSet.RC_BORDER_REG), srcReg); regionId++;
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
                    AppendStacks(lvlStacks[sId - 1], lvlStacks[sId], srcReg); // copy left overs from last level
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
        /// Builds the layer regions
        /// </summary>
        /// <param name="borderSize">Border size</param>
        /// <param name="minRegionArea">Minimum region area</param>
        /// <returns>Returns true when the region were correctly built</returns>
        public bool BuildLayerRegions(int borderSize, int minRegionArea)
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
                PaintRectRegion(0, bw, 0, h, id | ContourSet.RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(w - bw, w, 0, h, id | ContourSet.RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(0, w, 0, bh, id | ContourSet.RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(0, w, h - bh, h, id | ContourSet.RC_BORDER_REG, srcReg); id++;
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
                        if (s.GetCon(0) != ContourSet.RC_NOT_CONNECTED)
                        {
                            int ax = x + RecastUtils.GetDirOffsetX(0);
                            int ay = y + RecastUtils.GetDirOffsetY(0);
                            int ai = Cells[ax + ay * w].Index + s.GetCon(0);
                            if ((srcReg[ai] & ContourSet.RC_BORDER_REG) == 0 && Areas[i] == Areas[ai])
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
                        if (s.GetCon(3) != ContourSet.RC_NOT_CONNECTED)
                        {
                            int ax = x + RecastUtils.GetDirOffsetX(3);
                            int ay = y + RecastUtils.GetDirOffsetY(3);
                            int ai = Cells[ax + ay * w].Index + s.GetCon(3);
                            if (srcReg[ai] != 0 && (srcReg[ai] & ContourSet.RC_BORDER_REG) == 0 && Areas[i] == Areas[ai])
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
                    if (cs.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
                    {
                        int ax = cx + RecastUtils.GetDirOffsetX(dir);
                        int ay = cy + RecastUtils.GetDirOffsetY(dir);
                        int ai = Cells[ax + ay * w].Index + cs.GetCon(dir);
                        if (Areas[ai] != area)
                        {
                            continue;
                        }
                        int nr = srcReg[ai];
                        if ((nr & ContourSet.RC_BORDER_REG) != 0) // Do not take borders into account.
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
                        if (a.GetCon(dir2) != ContourSet.RC_NOT_CONNECTED)
                        {
                            int ax2 = ax + RecastUtils.GetDirOffsetX(dir2);
                            int ay2 = ay + RecastUtils.GetDirOffsetY(dir2);
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
                    if (cs.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
                    {
                        int ax = cx + RecastUtils.GetDirOffsetX(dir);
                        int ay = cy + RecastUtils.GetDirOffsetY(dir);
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
        private void ExpandRegions(int maxIter, int level, int[] srcReg, int[] srcDist, List<LevelStackEntry> stack, bool fillStack)
        {
            int w = Width;
            int h = Height;

            if (fillStack)
            {
                // Find cells revealed by the raised level.
                stack.Clear();
                for (int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        var c = Cells[x + y * w];
                        for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                        {
                            if (BorderDistances[i] >= level && srcReg[i] == 0 && Areas[i] != AreaTypes.RC_NULL_AREA)
                            {
                                stack.Add(new LevelStackEntry { X = x, Y = y, Index = i });
                            }
                        }
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

            List<RecastRegionDirtyEntry> dirtyEntries = new List<RecastRegionDirtyEntry>();
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
                        if (s.GetCon(dir) == ContourSet.RC_NOT_CONNECTED) continue;
                        int ax = x + RecastUtils.GetDirOffsetX(dir);
                        int ay = y + RecastUtils.GetDirOffsetY(dir);
                        int ai = Cells[ax + ay * w].Index + s.GetCon(dir);
                        if (Areas[ai] != area) continue;
                        if (srcReg[ai] > 0 && (srcReg[ai] & ContourSet.RC_BORDER_REG) == 0 && srcDist[ai] + 2 < d2)
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
        private void SortCellsByLevel(int startLevel, int[] srcReg, int nbStacks, List<List<LevelStackEntry>> stacks, int loglevelsPerStack)
        {
            int w = Width;
            int h = Height;
            startLevel >>= loglevelsPerStack;

            for (int j = 0; j < nbStacks; j++)
            {
                stacks[j].Clear();
            }

            // put all cells in the level range into the appropriate stacks
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var c = Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; i++)
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
            }
        }
        private void PaintRectRegion(int minx, int maxx, int miny, int maxy, int regId, int[] srcReg)
        {
            int w = Width;
            for (int y = miny; y < maxy; ++y)
            {
                for (int x = minx; x < maxx; ++x)
                {
                    var c = Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        if (Areas[i] != AreaTypes.RC_NULL_AREA)
                        {
                            srcReg[i] = regId;
                        }
                    }
                }
            }
        }
        private bool MergeAndFilterLayerRegions(int minRegionArea, int maxRegionId, int[] srcReg, out int maxRegionIdResult)
        {
            int w = Width;
            int h = Height;

            int nreg = maxRegionId + 1;
            List<Region> regions = new List<Region>(nreg);

            // Construct regions
            for (int i = 0; i < nreg; ++i)
            {
                regions.Add(new Region(i));
            }

            // Find region neighbours and overlapping regions.
            List<int> lregs = new List<int>(32);
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
                            if (s.GetCon(dir) != ContourSet.RC_NOT_CONNECTED)
                            {
                                int ax = x + RecastUtils.GetDirOffsetX(dir);
                                int ay = y + RecastUtils.GetDirOffsetY(dir);
                                int ai = Cells[ax + ay * w].Index + s.GetCon(dir);
                                int rai = srcReg[ai];
                                if (rai > 0 && rai < nreg && rai != ri)
                                {
                                    reg.AddUniqueConnection(rai);
                                }
                                if ((rai & ContourSet.RC_BORDER_REG) != 0)
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
            List<int> stack = new List<int>(32);
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
                if ((regions[i].Id & ContourSet.RC_BORDER_REG) != 0)
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
                if ((srcReg[i] & ContourSet.RC_BORDER_REG) == 0)
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
        private bool MergeAndFilterRegions(int minRegionArea, int mergeRegionSize, int maxRegionId, int[] srcReg, out int[] overlaps, out int maxRegionIdResult)
        {
            int w = Width;
            int h = Height;

            int nreg = maxRegionId + 1;
            List<Region> regions = new List<Region>(nreg);

            // Construct regions
            for (int i = 0; i < nreg; ++i)
            {
                regions.Add(new Region(i));
            }

            // Find edge of a region and find connections around the contour.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        int r = srcReg[i];
                        if (r == 0 || r >= nreg)
                        {
                            continue;
                        }

                        var reg = regions[r];
                        reg.SpanCount++;

                        // Update floors.
                        for (int j = c.Index; j < ni; ++j)
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
                }
            }

            // Remove too small regions.
            List<int> stack = new List<int>();
            List<int> trace = new List<int>();
            for (int i = 0; i < nreg; ++i)
            {
                var reg = regions[i];
                if (reg.Id == 0 || (reg.Id & ContourSet.RC_BORDER_REG) != 0)
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
                        if ((connection & ContourSet.RC_BORDER_REG) != 0)
                        {
                            connectsToBorder = true;
                            continue;
                        }
                        var neireg = regions[connection];
                        if (neireg.Visited)
                        {
                            continue;
                        }
                        if (neireg.Id == 0 || (neireg.Id & ContourSet.RC_BORDER_REG) != 0)
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
                    if (reg.Id == 0 || (reg.Id & ContourSet.RC_BORDER_REG) != 0)
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
                        if ((connection & ContourSet.RC_BORDER_REG) != 0)
                        {
                            continue;
                        }

                        var mreg = regions[connection];
                        if (mreg.Id == 0 || (mreg.Id & ContourSet.RC_BORDER_REG) != 0 || mreg.Overlap)
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
                                if (regions[j].Id == 0 || (regions[j].Id & ContourSet.RC_BORDER_REG) != 0)
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
                if ((regions[i].Id & ContourSet.RC_BORDER_REG) != 0)
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
                if ((srcReg[i] & ContourSet.RC_BORDER_REG) == 0)
                {
                    srcReg[i] = regions[srcReg[i]].Id;
                }
            }

            // Return regions that we found to be overlapping.
            List<int> lOverlaps = new List<int>();
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
    }
}
