using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    class CompactHeightfield
    {
        public static CompactHeightfield Build(int walkableHeight, int walkableClimb, Heightfield hf)
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
                CS = hf.CellSize,
                CH = hf.CellHeight,
            };

            // Fill in cells and spans.
            chf.FillCells(hf);

            // Find neighbour connections.
            chf.FindNeighbourConnections();

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
        private static void AddUniqueFloorRegion(Region reg, int n)
        {
            for (int i = 0; i < reg.Floors.Count; ++i)
            {
                if (reg.Floors[i] == n)
                {
                    return;
                }
            }
            reg.Floors.Add(n);
        }
        private static void AddUniqueConnection(Region reg, int n)
        {
            for (int i = 0; i < reg.Connections.Count; ++i)
            {
                if (reg.Connections[i] == n)
                {
                    return;
                }
            }

            reg.Connections.Add(n);
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
        private static void RemoveAdjacentNeighbours(Region reg)
        {
            // Remove adjacent duplicates.
            for (int i = 0; i < reg.Connections.Count && reg.Connections.Count > 1;)
            {
                int ni = (i + 1) % reg.Connections.Count;
                if (reg.Connections[i] == reg.Connections[ni])
                {
                    // Remove duplicate
                    for (int j = i; j < reg.Connections.Count - 1; ++j)
                    {
                        reg.Connections[j] = reg.Connections[j + 1];
                    }
                    reg.Connections.RemoveAt(reg.Connections.Count - 1);
                }
                else
                {
                    ++i;
                }
            }
        }
        private static void ReplaceNeighbour(Region reg, int oldId, int newId)
        {
            bool neiChanged = false;
            for (int i = 0; i < reg.Connections.Count; ++i)
            {
                if (reg.Connections[i] == oldId)
                {
                    reg.Connections[i] = newId;
                    neiChanged = true;
                }
            }
            for (int i = 0; i < reg.Floors.Count; ++i)
            {
                if (reg.Floors[i] == oldId)
                {
                    reg.Floors[i] = newId;
                }
            }
            if (neiChanged)
            {
                RemoveAdjacentNeighbours(reg);
            }
        }
        private static bool CanMergeWithRegion(Region rega, Region regb)
        {
            if (rega.AreaType != regb.AreaType)
            {
                return false;
            }
            int n = 0;
            for (int i = 0; i < rega.Connections.Count; ++i)
            {
                if (rega.Connections[i] == regb.Id)
                {
                    n++;
                }
            }
            if (n > 1)
            {
                return false;
            }
            for (int i = 0; i < rega.Floors.Count; ++i)
            {
                if (rega.Floors[i] == regb.Id)
                {
                    return false;
                }
            }
            return true;
        }
        private static bool MergeRegions(Region rega, Region regb)
        {
            int aid = rega.Id;
            int bid = regb.Id;

            // Duplicate current neighbourhood.
            List<int> acon = new List<int>(rega.Connections);
            List<int> bcon = regb.Connections;

            // Find insertion point on A.
            int insa = -1;
            for (int i = 0; i < acon.Count; ++i)
            {
                if (acon[i] == bid)
                {
                    insa = i;
                    break;
                }
            }
            if (insa == -1)
            {
                return false;
            }

            // Find insertion point on B.
            int insb = -1;
            for (int i = 0; i < bcon.Count; ++i)
            {
                if (bcon[i] == aid)
                {
                    insb = i;
                    break;
                }
            }
            if (insb == -1)
            {
                return false;
            }

            // Merge neighbours.
            rega.Connections.Clear();
            for (int i = 0, ni = acon.Count; i < ni - 1; ++i)
            {
                rega.Connections.Add(acon[(insa + 1 + i) % ni]);
            }

            for (int i = 0, ni = bcon.Count; i < ni - 1; ++i)
            {
                rega.Connections.Add(bcon[(insb + 1 + i) % ni]);
            }

            RemoveAdjacentNeighbours(rega);

            for (int j = 0; j < regb.Floors.Count; ++j)
            {
                AddUniqueFloorRegion(rega, regb.Floors[j]);
            }
            rega.SpanCount += regb.SpanCount;
            regb.SpanCount = 0;
            regb.Connections.Clear();

            return true;
        }
        private static bool IsRegionConnectedToBorder(Region reg)
        {
            // Region is connected to border if
            // one of the neighbours is null id.
            for (int i = 0; i < reg.Connections.Count; ++i)
            {
                if (reg.Connections[i] == 0)
                {
                    return true;
                }
            }
            return false;
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
        public float CS { get; set; }
        /// <summary>
        /// The height of each cell. (The minimum increment along the y-axis.)
        /// </summary>
        public float CH { get; set; }
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
        public int[] Dist { get; set; }
        /// <summary>
        /// Array containing area id data. [Size: spanCount] 
        /// </summary>
        public AreaTypes[] Areas { get; set; }


        private void FillCells(Heightfield hf)
        {
            int w = this.Width;
            int h = this.Height;
            int spanCount = this.SpanCount;

            this.Cells = new CompactCell[w * h];
            this.Spans = new CompactSpan[spanCount];
            this.Areas = new AreaTypes[spanCount];

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
                        if (s.Area != AreaTypes.Unwalkable)
                        {
                            int bot = s.SMax;
                            int top = s.Next != null ? s.Next.SMin : int.MaxValue;
                            this.Spans[idx].Y = MathUtil.Clamp(bot, 0, 0xffff);
                            this.Spans[idx].H = MathUtil.Clamp(top - bot, 0, 0xff);
                            this.Areas[idx] = s.Area;
                            idx++;
                            c.Count++;
                        }

                        s = s.Next;
                    }

                    this.Cells[x + y * w] = c;
                }
            }
        }

        private void FindNeighbourConnections()
        {
            int w = this.Width;
            int h = this.Height;

            int maxLayers = CompactSpan.RC_NOT_CONNECTED - 1;
            int tooHighNeighbour = 0;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = this.Cells[x + y * w];

                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; i++)
                    {
                        var s = this.Spans[i];

                        for (int dir = 0; dir < 4; dir++)
                        {
                            s.SetCon(dir, CompactSpan.RC_NOT_CONNECTED);
                            int nx = x + DirectionUtils.GetDirOffsetX(dir);
                            int ny = y + DirectionUtils.GetDirOffsetY(dir);
                            // First check that the neighbour cell is in bounds.
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                            {
                                continue;
                            }

                            // Iterate over all neighbour spans and check if any of the is
                            // accessible from current cell.
                            var nc = this.Cells[nx + ny * w];

                            for (int k = nc.Index, nk = (nc.Index + nc.Count); k < nk; ++k)
                            {
                                var ns = this.Spans[k];

                                int bot = Math.Max(s.Y, ns.Y);
                                int top = Math.Min(s.Y + s.H, ns.Y + ns.H);

                                // Check that the gap between the spans is walkable,
                                // and that the climb height between the gaps is not too high.
                                if ((top - bot) >= this.WalkableHeight && Math.Abs(ns.Y - s.Y) <= this.WalkableClimb)
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

                        this.Spans[i] = s;
                    }
                }
            }

            if (tooHighNeighbour > maxLayers)
            {
                throw new EngineException(string.Format("Heightfield has too many layers {0} (max: {1})", tooHighNeighbour, maxLayers));
            }
        }
        /// <summary>
        /// Basically, any spans that are closer to a boundary or obstruction than the specified radius are marked as unwalkable.
        /// </summary>
        /// <param name="radius">Radius</param>
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
                        if (this.Areas[i] == AreaTypes.Unwalkable)
                        {
                            dist[i] = 0;
                        }
                        else
                        {
                            CompactSpan s = this.Spans[i];
                            int nc = 0;
                            for (int dir = 0; dir < 4; ++dir)
                            {
                                if (s.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
                                {
                                    int nx = x + DirectionUtils.GetDirOffsetX(dir);
                                    int ny = y + DirectionUtils.GetDirOffsetY(dir);
                                    int nidx = this.Cells[nx + ny * w].Index + s.GetCon(dir);
                                    if (this.Areas[nidx] != AreaTypes.Unwalkable)
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
                        if (s.GetCon(0) != CompactSpan.RC_NOT_CONNECTED)
                        {
                            // (-1,0)
                            int ax = x + DirectionUtils.GetDirOffsetX(0);
                            int ay = y + DirectionUtils.GetDirOffsetY(0);
                            int ai = this.Cells[ax + ay * w].Index + s.GetCon(0);
                            CompactSpan asp = this.Spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (-1,-1)
                            if (asp.GetCon(3) != CompactSpan.RC_NOT_CONNECTED)
                            {
                                int aax = ax + DirectionUtils.GetDirOffsetX(3);
                                int aay = ay + DirectionUtils.GetDirOffsetY(3);
                                int aai = this.Cells[aax + aay * w].Index + asp.GetCon(3);
                                nd = Math.Min(dist[aai] + 3, 255);
                                if (nd < dist[i])
                                {
                                    dist[i] = nd;
                                }
                            }
                        }
                        if (s.GetCon(3) != CompactSpan.RC_NOT_CONNECTED)
                        {
                            // (0,-1)
                            int ax = x + DirectionUtils.GetDirOffsetX(3);
                            int ay = y + DirectionUtils.GetDirOffsetY(3);
                            int ai = this.Cells[ax + ay * w].Index + s.GetCon(3);
                            CompactSpan asp = this.Spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (1,-1)
                            if (asp.GetCon(2) != CompactSpan.RC_NOT_CONNECTED)
                            {
                                int aax = ax + DirectionUtils.GetDirOffsetX(2);
                                int aay = ay + DirectionUtils.GetDirOffsetY(2);
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
                        if (s.GetCon(2) != CompactSpan.RC_NOT_CONNECTED)
                        {
                            // (1,0)
                            int ax = x + DirectionUtils.GetDirOffsetX(2);
                            int ay = y + DirectionUtils.GetDirOffsetY(2);
                            int ai = this.Cells[ax + ay * w].Index + s.GetCon(2);
                            var asp = this.Spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (1,1)
                            if (asp.GetCon(1) != CompactSpan.RC_NOT_CONNECTED)
                            {
                                int aax = ax + DirectionUtils.GetDirOffsetX(1);
                                int aay = ay + DirectionUtils.GetDirOffsetY(1);
                                int aai = this.Cells[aax + aay * w].Index + asp.GetCon(1);
                                nd = Math.Min(dist[aai] + 3, 255);
                                if (nd < dist[i])
                                {
                                    dist[i] = nd;
                                }
                            }
                        }
                        if (s.GetCon(1) != CompactSpan.RC_NOT_CONNECTED)
                        {
                            // (0,1)
                            int ax = x + DirectionUtils.GetDirOffsetX(1);
                            int ay = y + DirectionUtils.GetDirOffsetY(1);
                            int ai = this.Cells[ax + ay * w].Index + s.GetCon(1);
                            var asp = this.Spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (-1,1)
                            if (asp.GetCon(0) != CompactSpan.RC_NOT_CONNECTED)
                            {
                                int aax = ax + DirectionUtils.GetDirOffsetX(0);
                                int aay = ay + DirectionUtils.GetDirOffsetY(0);
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
                    this.Areas[i] = AreaTypes.Unwalkable;
                }
            }

            return true;
        }
        /// <summary>
        /// This filter is usually applied after applying area id's using functions such as MarkBoxArea, MarkConvexPolyArea, and MarkCylinderArea.
        /// </summary>
        /// <returns>Returns always true</returns>
        public bool MedianFilterWalkableArea()
        {
            int w = this.Width;
            int h = this.Height;

            // Init distance.
            AreaTypes[] areas = Helper.CreateArray(this.SpanCount, (AreaTypes)0xff);

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = this.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = this.Spans[i];
                        if (this.Areas[i] == AreaTypes.Unwalkable)
                        {
                            areas[i] = this.Areas[i];
                            continue;
                        }

                        AreaTypes[] nei = new AreaTypes[9];
                        for (int j = 0; j < 9; ++j)
                        {
                            nei[j] = this.Areas[i];
                        }

                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (s.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
                            {
                                int ax = x + DirectionUtils.GetDirOffsetX(dir);
                                int ay = y + DirectionUtils.GetDirOffsetY(dir);
                                int ai = this.Cells[ax + ay * w].Index + s.GetCon(dir);
                                if (this.Areas[ai] != AreaTypes.Unwalkable)
                                {
                                    nei[dir * 2 + 0] = this.Areas[ai];
                                }

                                var a = this.Spans[ai];
                                int dir2 = DirectionUtils.RotateCW(dir);
                                if (a.GetCon(dir2) != CompactSpan.RC_NOT_CONNECTED)
                                {
                                    int ax2 = ax + DirectionUtils.GetDirOffsetX(dir2);
                                    int ay2 = ay + DirectionUtils.GetDirOffsetY(dir2);
                                    int ai2 = this.Cells[ax2 + ay2 * w].Index + a.GetCon(dir2);
                                    if (this.Areas[ai2] != AreaTypes.Unwalkable)
                                    {
                                        nei[dir * 2 + 1] = this.Areas[ai2];
                                    }
                                }
                            }
                        }
                        InsertSort(nei, 9);
                        areas[i] = nei[4];
                    }
                }
            }

            Array.Copy(areas, this.Areas, this.SpanCount);

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmin"></param>
        /// <param name="bmax"></param>
        /// <param name="areaId"></param>
        /// <remarks>
        /// The value of spacial parameters are in world units.
        /// </remarks>
        public void MarkBoxArea(Vector3 bmin, Vector3 bmax, AreaTypes areaId)
        {
            int minx = (int)((bmin.X - this.BoundingBox.Minimum.X) / this.CS);
            int miny = (int)((bmin.Y - this.BoundingBox.Minimum.Y) / this.CH);
            int minz = (int)((bmin.Z - this.BoundingBox.Minimum.Z) / this.CS);
            int maxx = (int)((bmax.X - this.BoundingBox.Minimum.X) / this.CS);
            int maxy = (int)((bmax.Y - this.BoundingBox.Minimum.Y) / this.CH);
            int maxz = (int)((bmax.Z - this.BoundingBox.Minimum.Z) / this.CS);

            if (maxx < 0) return;
            if (minx >= this.Width) return;
            if (maxz < 0) return;
            if (minz >= this.Height) return;

            if (minx < 0) minx = 0;
            if (maxx >= this.Width) maxx = this.Width - 1;
            if (minz < 0) minz = 0;
            if (maxz >= this.Height) maxz = this.Height - 1;

            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    var c = this.Cells[x + z * this.Width];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = this.Spans[i];
                        if (s.Y >= miny && s.Y <= maxy && this.Areas[i] != AreaTypes.Unwalkable)
                        {
                            this.Areas[i] = areaId;
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
            bmin[1] = hmin;
            bmax[1] = hmax;

            int minx = (int)((bmin[0] - this.BoundingBox.Minimum[0]) / this.CS);
            int miny = (int)((bmin[1] - this.BoundingBox.Minimum[1]) / this.CH);
            int minz = (int)((bmin[2] - this.BoundingBox.Minimum[2]) / this.CS);
            int maxx = (int)((bmax[0] - this.BoundingBox.Minimum[0]) / this.CS);
            int maxy = (int)((bmax[1] - this.BoundingBox.Minimum[1]) / this.CH);
            int maxz = (int)((bmax[2] - this.BoundingBox.Minimum[2]) / this.CS);

            if (maxx < 0) return;
            if (minx >= this.Width) return;
            if (maxz < 0) return;
            if (minz >= this.Height) return;

            if (minx < 0) minx = 0;
            if (maxx >= this.Width) maxx = this.Width - 1;
            if (minz < 0) minz = 0;
            if (maxz >= this.Height) maxz = this.Height - 1;


            // TODO: Optimize.
            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    CompactCell c = this.Cells[x + z * this.Width];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        CompactSpan s = this.Spans[i];
                        if (this.Areas[i] == AreaTypes.Unwalkable)
                        {
                            continue;
                        }

                        if (s.Y >= miny && s.Y <= maxy)
                        {
                            Vector3 p = new Vector3();
                            p[0] = this.BoundingBox.Minimum[0] + (x + 0.5f) * this.CS;
                            p[1] = 0;
                            p[2] = this.BoundingBox.Minimum[2] + (z + 0.5f) * this.CS;

                            if (PointInPoly(nverts, verts, p))
                            {
                                this.Areas[i] = areaId;
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

            int minx = (int)((bmin.X - this.BoundingBox.Minimum.X) / this.CS);
            int miny = (int)((bmin.Y - this.BoundingBox.Minimum.Y) / this.CH);
            int minz = (int)((bmin.Z - this.BoundingBox.Minimum.Z) / this.CS);
            int maxx = (int)((bmax.X - this.BoundingBox.Minimum.X) / this.CS);
            int maxy = (int)((bmax.Y - this.BoundingBox.Minimum.Y) / this.CH);
            int maxz = (int)((bmax.Z - this.BoundingBox.Minimum.Z) / this.CS);

            if (maxx < 0) return;
            if (minx >= this.Width) return;
            if (maxz < 0) return;
            if (minz >= this.Height) return;

            if (minx < 0) minx = 0;
            if (maxx >= this.Width) maxx = this.Width - 1;
            if (minz < 0) minz = 0;
            if (maxz >= this.Height) maxz = this.Height - 1;

            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    var c = this.Cells[x + z * this.Width];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = this.Spans[i];

                        if (this.Areas[i] == AreaTypes.Unwalkable)
                        {
                            continue;
                        }

                        if (s.Y >= miny && s.Y <= maxy)
                        {
                            float sx = this.BoundingBox.Minimum.X + (x + 0.5f) * this.CS;
                            float sz = this.BoundingBox.Minimum.Z + (z + 0.5f) * this.CS;
                            float dx = sx - pos.X;
                            float dz = sz - pos.Z;

                            if (dx * dx + dz * dz < r2)
                            {
                                this.Areas[i] = areaId;
                            }
                        }
                    }
                }
            }
        }
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

        public void CalculateDistanceField(int[] src, out int maxDist)
        {
            int w = this.Width;
            int h = this.Height;

            // Init distance and points.
            for (int i = 0; i < this.SpanCount; ++i)
            {
                src[i] = int.MaxValue;
            }

            // Mark boundary cells.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = this.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = this.Spans[i];
                        var area = this.Areas[i];

                        int nc = 0;
                        for (int dir = 0; dir < 4; dir++)
                        {
                            if (s.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
                            {
                                int ax = x + DirectionUtils.GetDirOffsetX(dir);
                                int ay = y + DirectionUtils.GetDirOffsetY(dir);
                                int ai = this.Cells[ax + ay * w].Index + s.GetCon(dir);
                                if (area == this.Areas[ai])
                                {
                                    nc++;
                                }
                            }
                        }
                        if (nc != 4)
                        {
                            src[i] = 0;
                        }
                    }
                }
            }

            // Pass 1
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = this.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = this.Spans[i];

                        if (s.GetCon(0) != CompactSpan.RC_NOT_CONNECTED)
                        {
                            // (-1,0)
                            int ax = x + DirectionUtils.GetDirOffsetX(0);
                            int ay = y + DirectionUtils.GetDirOffsetY(0);
                            int ai = this.Cells[ax + ay * w].Index + s.GetCon(0);
                            var a = this.Spans[ai];
                            if (src[ai] + 2 < src[i])
                            {
                                src[i] = src[ai] + 2;
                            }

                            // (-1,-1)
                            if (a.GetCon(3) != CompactSpan.RC_NOT_CONNECTED)
                            {
                                int aax = ax + DirectionUtils.GetDirOffsetX(3);
                                int aay = ay + DirectionUtils.GetDirOffsetY(3);
                                int aai = this.Cells[aax + aay * w].Index + a.GetCon(3);
                                if (src[aai] + 3 < src[i])
                                {
                                    src[i] = src[aai] + 3;
                                }
                            }
                        }
                        if (s.GetCon(3) != CompactSpan.RC_NOT_CONNECTED)
                        {
                            // (0,-1)
                            int ax = x + DirectionUtils.GetDirOffsetX(3);
                            int ay = y + DirectionUtils.GetDirOffsetY(3);
                            int ai = this.Cells[ax + ay * w].Index + s.GetCon(3);
                            var a = this.Spans[ai];
                            if (src[ai] + 2 < src[i])
                            {
                                src[i] = src[ai] + 2;
                            }

                            // (1,-1)
                            if (a.GetCon(2) != CompactSpan.RC_NOT_CONNECTED)
                            {
                                int aax = ax + DirectionUtils.GetDirOffsetX(2);
                                int aay = ay + DirectionUtils.GetDirOffsetY(2);
                                int aai = this.Cells[aax + aay * w].Index + a.GetCon(2);
                                if (src[aai] + 3 < src[i])
                                {
                                    src[i] = src[aai] + 3;
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

                        if (s.GetCon(2) != CompactSpan.RC_NOT_CONNECTED)
                        {
                            // (1,0)
                            int ax = x + DirectionUtils.GetDirOffsetX(2);
                            int ay = y + DirectionUtils.GetDirOffsetY(2);
                            int ai = this.Cells[ax + ay * w].Index + s.GetCon(2);
                            var a = this.Spans[ai];
                            if (src[ai] + 2 < src[i])
                            {
                                src[i] = src[ai] + 2;
                            }

                            // (1,1)
                            if (a.GetCon(1) != CompactSpan.RC_NOT_CONNECTED)
                            {
                                int aax = ax + DirectionUtils.GetDirOffsetX(1);
                                int aay = ay + DirectionUtils.GetDirOffsetY(1);
                                int aai = this.Cells[aax + aay * w].Index + a.GetCon(1);
                                if (src[aai] + 3 < src[i])
                                {
                                    src[i] = src[aai] + 3;
                                }
                            }
                        }
                        if (s.GetCon(1) != CompactSpan.RC_NOT_CONNECTED)
                        {
                            // (0,1)
                            int ax = x + DirectionUtils.GetDirOffsetX(1);
                            int ay = y + DirectionUtils.GetDirOffsetY(1);
                            int ai = this.Cells[ax + ay * w].Index + s.GetCon(1);
                            var a = this.Spans[ai];
                            if (src[ai] + 2 < src[i])
                            {
                                src[i] = src[ai] + 2;
                            }

                            // (-1,1)
                            if (a.GetCon(0) != CompactSpan.RC_NOT_CONNECTED)
                            {
                                int aax = ax + DirectionUtils.GetDirOffsetX(0);
                                int aay = ay + DirectionUtils.GetDirOffsetY(0);
                                int aai = this.Cells[aax + aay * w].Index + a.GetCon(0);
                                if (src[aai] + 3 < src[i])
                                {
                                    src[i] = src[aai] + 3;
                                }
                            }
                        }
                    }
                }
            }

            maxDist = 0;
            for (int i = 0; i < this.SpanCount; ++i)
            {
                maxDist = Math.Max(src[i], maxDist);
            }
        }

        public int[] BoxBlur(int thr, int[] src, int[] dst)
        {
            int w = this.Width;
            int h = this.Height;

            thr *= 2;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = this.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = this.Spans[i];
                        var cd = src[i];
                        if (cd <= thr)
                        {
                            dst[i] = cd;
                            continue;
                        }

                        int d = cd;
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (s.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
                            {
                                int ax = x + DirectionUtils.GetDirOffsetX(dir);
                                int ay = y + DirectionUtils.GetDirOffsetY(dir);
                                int ai = this.Cells[ax + ay * w].Index + s.GetCon(dir);
                                d += src[ai];

                                var a = this.Spans[ai];
                                int dir2 = DirectionUtils.RotateCW(dir);
                                if (a.GetCon(dir2) != CompactSpan.RC_NOT_CONNECTED)
                                {
                                    int ax2 = ax + DirectionUtils.GetDirOffsetX(dir2);
                                    int ay2 = ay + DirectionUtils.GetDirOffsetY(dir2);
                                    int ai2 = this.Cells[ax2 + ay2 * w].Index + a.GetCon(dir2);
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

        public bool FloodRegion(LevelStackEntry entry, int level, int r, int[] srcReg, int[] srcDist, List<LevelStackEntry> stack)
        {
            int w = this.Width;

            var area = this.Areas[entry.Index];

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

                var cs = this.Spans[ci];

                // Check if any of the neighbours already have a valid region set.
                int ar = 0;
                for (int dir = 0; dir < 4; ++dir)
                {
                    // 8 connected
                    if (cs.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
                    {
                        int ax = cx + DirectionUtils.GetDirOffsetX(dir);
                        int ay = cy + DirectionUtils.GetDirOffsetY(dir);
                        int ai = this.Cells[ax + ay * w].Index + cs.GetCon(dir);
                        if (this.Areas[ai] != area)
                        {
                            continue;
                        }
                        int nr = srcReg[ai];
                        if ((nr & RecastUtils.RC_BORDER_REG) != 0) // Do not take borders into account.
                        {
                            continue;
                        }
                        if (nr != 0 && nr != r)
                        {
                            ar = nr;
                            break;
                        }

                        var a = this.Spans[ai];

                        int dir2 = DirectionUtils.RotateCW(dir);
                        if (a.GetCon(dir2) != CompactSpan.RC_NOT_CONNECTED)
                        {
                            int ax2 = ax + DirectionUtils.GetDirOffsetX(dir2);
                            int ay2 = ay + DirectionUtils.GetDirOffsetY(dir2);
                            int ai2 = this.Cells[ax2 + ay2 * w].Index + a.GetCon(dir2);
                            if (this.Areas[ai2] != area)
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
                    if (cs.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
                    {
                        int ax = cx + DirectionUtils.GetDirOffsetX(dir);
                        int ay = cy + DirectionUtils.GetDirOffsetY(dir);
                        int ai = this.Cells[ax + ay * w].Index + cs.GetCon(dir);
                        if (this.Areas[ai] != area)
                        {
                            continue;
                        }
                        if (this.Dist[ai] >= lev && srcReg[ai] == 0)
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

        public void ExpandRegions(int maxIter, int level, int[] srcReg, int[] srcDist, List<LevelStackEntry> stack, bool fillStack)
        {
            int w = this.Width;
            int h = this.Height;

            if (fillStack)
            {
                // Find cells revealed by the raised level.
                stack.Clear();
                for (int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        var c = this.Cells[x + y * w];
                        for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                        {
                            if (this.Dist[i] >= level && srcReg[i] == 0 && this.Areas[i] != AreaTypes.Unwalkable)
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
                    var area = this.Areas[i];
                    var s = this.Spans[i];
                    for (int dir = 0; dir < 4; ++dir)
                    {
                        if (s.GetCon(dir) == CompactSpan.RC_NOT_CONNECTED)
                        {
                            continue;
                        }

                        int ax = x + DirectionUtils.GetDirOffsetX(dir);
                        int ay = y + DirectionUtils.GetDirOffsetY(dir);
                        int ai = this.Cells[ax + ay * w].Index + s.GetCon(dir);
                        if (this.Areas[ai] != area)
                        {
                            continue;
                        }

                        if (srcReg[ai] > 0 && (srcReg[ai] & RecastUtils.RC_BORDER_REG) == 0 && srcDist[ai] + 2 < d2)
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

        public void SortCellsByLevel(int startLevel, int[] srcReg, int nbStacks, List<List<LevelStackEntry>> stacks, int loglevelsPerStack) // the levels per stack (2 in our case) as a bit shift
        {
            int w = this.Width;
            int h = this.Height;
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
                    var c = this.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; i++)
                    {
                        if (this.Areas[i] == AreaTypes.Unwalkable || srcReg[i] != 0)
                        {
                            continue;
                        }

                        int level = this.Dist[i] >> loglevelsPerStack;
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

        public bool IsSolidEdge(int[] srcReg, int x, int y, int i, int dir)
        {
            var s = this.Spans[i];
            int r = 0;
            if (s.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
            {
                int ax = x + DirectionUtils.GetDirOffsetX(dir);
                int ay = y + DirectionUtils.GetDirOffsetY(dir);
                int ai = this.Cells[ax + ay * this.Width].Index + s.GetCon(dir);
                r = srcReg[ai];
            }
            if (r == srcReg[i])
            {
                return false;
            }
            return true;
        }

        public void WalkContour(int x, int y, int i, int[] flags, out List<Int4> points)
        {
            points = new List<Int4>();

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
                    int py = GetCornerHeight(x, y, i, dir, out bool isBorderVertex);
                    int pz = y;
                    switch (dir)
                    {
                        case 0: pz++; break;
                        case 1: px++; pz++; break;
                        case 2: px++; break;
                    }
                    int r = 0;
                    var s = this.Spans[i];
                    if (s.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
                    {
                        int ax = x + DirectionUtils.GetDirOffsetX(dir);
                        int ay = y + DirectionUtils.GetDirOffsetY(dir);
                        int ai = this.Cells[ax + ay * this.Width].Index + s.GetCon(dir);
                        r = this.Spans[ai].Reg;
                        if (area != this.Areas[ai])
                        {
                            isAreaBorder = true;
                        }
                    }
                    if (isBorderVertex)
                    {
                        r |= RecastUtils.RC_BORDER_VERTEX;
                    }
                    if (isAreaBorder)
                    {
                        r |= RecastUtils.RC_AREA_BORDER;
                    }
                    points.Add(new Int4(px, py, pz, r));

                    flags[i] &= ~(1 << dir); // Remove visited edges
                    // Rotate CW
                    dir = DirectionUtils.RotateCW(dir);
                }
                else
                {
                    int ni = -1;
                    int nx = x + DirectionUtils.GetDirOffsetX(dir);
                    int ny = y + DirectionUtils.GetDirOffsetY(dir);
                    var s = this.Spans[i];
                    if (s.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
                    {
                        var nc = this.Cells[nx + ny * this.Width];
                        ni = nc.Index + s.GetCon(dir);
                    }
                    if (ni == -1)
                    {
                        // Should not happen.
                        return;
                    }
                    x = nx;
                    y = ny;
                    i = ni;
                    // Rotate CCW
                    dir = DirectionUtils.RotateCCW(dir);
                }

                if (starti == i && startDir == dir)
                {
                    break;
                }
            }
        }

        public void WalkContour(int x, int y, int i, int dir, int[] srcReg, List<int> cont)
        {
            int startDir = dir;
            int starti = i;

            var ss = this.Spans[i];
            int curReg = 0;
            if (ss.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
            {
                int ax = x + DirectionUtils.GetDirOffsetX(dir);
                int ay = y + DirectionUtils.GetDirOffsetY(dir);
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
                    if (s.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
                    {
                        int ax = x + DirectionUtils.GetDirOffsetX(dir);
                        int ay = y + DirectionUtils.GetDirOffsetY(dir);
                        int ai = this.Cells[ax + ay * this.Width].Index + s.GetCon(dir);
                        r = srcReg[ai];
                    }
                    if (r != curReg)
                    {
                        curReg = r;
                        cont.Add(curReg);
                    }
                    // Rotate CW
                    dir = DirectionUtils.RotateCW(dir);
                }
                else
                {
                    int ni = -1;
                    int nx = x + DirectionUtils.GetDirOffsetX(dir);
                    int ny = y + DirectionUtils.GetDirOffsetY(dir);
                    if (s.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
                    {
                        var nc = this.Cells[nx + ny * this.Width];
                        ni = nc.Index + s.GetCon(dir);
                    }
                    if (ni == -1)
                    {
                        // Should not happen.
                        return;
                    }
                    x = nx;
                    y = ny;
                    i = ni;

                    // Rotate CCW
                    dir = DirectionUtils.RotateCCW(dir);
                }

                if (starti == i && startDir == dir)
                {
                    break;
                }
            }

            // Remove adjacent duplicates.
            if (cont.Count > 1)
            {
                for (int j = 0; j < cont.Count;)
                {
                    int nj = (j + 1) % cont.Count;
                    if (cont[j] == cont[nj])
                    {
                        for (int k = j; k < cont.Count - 1; ++k)
                        {
                            cont[k] = cont[k + 1];
                        }
                        cont.RemoveAt(0);
                    }
                    else
                    {
                        ++j;
                    }
                }
            }
        }

        public bool MergeAndFilterRegions(int minRegionArea, int mergeRegionSize, int maxRegionId, int[] srcReg, out int[] overlaps, out int maxRegionIdResult)
        {
            int w = this.Width;
            int h = this.Height;

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
                    var c = this.Cells[x + y * w];
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
                            AddUniqueFloorRegion(reg, floorId);
                        }

                        // Have found contour
                        if (reg.Connections.Count > 0)
                            continue;

                        reg.AreaType = this.Areas[i];

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
                            WalkContour(x, y, i, ndir, srcReg, reg.Connections);
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
                if (reg.Id == 0 || (reg.Id & RecastUtils.RC_BORDER_REG) != 0)
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

                    for (int j = 0; j < creg.Connections.Count; ++j)
                    {
                        if ((creg.Connections[j] & RecastUtils.RC_BORDER_REG) != 0)
                        {
                            connectsToBorder = true;
                            continue;
                        }
                        var neireg = regions[creg.Connections[j]];
                        if (neireg.Visited)
                        {
                            continue;
                        }
                        if (neireg.Id == 0 || (neireg.Id & RecastUtils.RC_BORDER_REG) != 0)
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
                    if (reg.Id == 0 || (reg.Id & RecastUtils.RC_BORDER_REG) != 0)
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
                    if (reg.SpanCount > mergeRegionSize && IsRegionConnectedToBorder(reg))
                    {
                        continue;
                    }

                    // Small region with more than 1 connection.
                    // Or region which is not connected to a border at all.
                    // Find smallest neighbour region that connects to this one.
                    int smallest = int.MaxValue;
                    int mergeId = reg.Id;
                    for (int j = 0; j < reg.Connections.Count; ++j)
                    {
                        if ((reg.Connections[j] & RecastUtils.RC_BORDER_REG) != 0)
                        {
                            continue;
                        }

                        var mreg = regions[reg.Connections[j]];
                        if (mreg.Id == 0 || (mreg.Id & RecastUtils.RC_BORDER_REG) != 0 || mreg.Overlap)
                        {
                            continue;
                        }

                        if (mreg.SpanCount < smallest &&
                            CanMergeWithRegion(reg, mreg) &&
                            CanMergeWithRegion(mreg, reg))
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
                        if (MergeRegions(target, reg))
                        {
                            // Fixup regions pointing to current region.
                            for (int j = 0; j < nreg; ++j)
                            {
                                if (regions[j].Id == 0 || (regions[j].Id & RecastUtils.RC_BORDER_REG) != 0)
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
                                ReplaceNeighbour(regions[j], oldId, mergeId);
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
                if ((regions[i].Id & RecastUtils.RC_BORDER_REG) != 0)
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
            for (int i = 0; i < this.SpanCount; ++i)
            {
                if ((srcReg[i] & RecastUtils.RC_BORDER_REG) == 0)
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

        public bool MergeAndFilterLayerRegions(int minRegionArea, int maxRegionId, int[] srcReg, out int maxRegionIdResult)
        {
            int w = this.Width;
            int h = this.Height;

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
                    var c = this.Cells[x + y * w];

                    lregs.Clear();

                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = this.Spans[i];
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
                            if (s.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
                            {
                                int ax = x + DirectionUtils.GetDirOffsetX(dir);
                                int ay = y + DirectionUtils.GetDirOffsetY(dir);
                                int ai = this.Cells[ax + ay * w].Index + s.GetCon(dir);
                                int rai = srcReg[ai];
                                if (rai > 0 && rai < nreg && rai != ri)
                                {
                                    AddUniqueConnection(reg, rai);
                                }
                                if ((rai & RecastUtils.RC_BORDER_REG) != 0)
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
                                AddUniqueFloorRegion(ri, lregs[j]);
                                AddUniqueFloorRegion(rj, lregs[i]);
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

                    int ncons = reg.Connections.Count;
                    for (int j = 0; j < ncons; ++j)
                    {
                        int nei = reg.Connections[j];
                        var regn = regions[nei];
                        // Skip already visited.
                        if (regn.Id != 0)
                        {
                            continue;
                        }
                        // Skip if the neighbour is overlapping root region.
                        bool overlap = false;
                        for (int k = 0; k < root.Floors.Count; k++)
                        {
                            if (root.Floors[k] == nei)
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
                        for (int k = 0; k < regn.Floors.Count; ++k)
                        {
                            AddUniqueFloorRegion(root, regn.Floors[k]);
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
                if ((regions[i].Id & RecastUtils.RC_BORDER_REG) != 0)
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
            for (int i = 0; i < this.SpanCount; ++i)
            {
                if ((srcReg[i] & RecastUtils.RC_BORDER_REG) == 0)
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

        public bool BuildDistanceField()
        {
            this.Dist = null;

            int[] src = new int[this.SpanCount];

            CalculateDistanceField(src, out var maxDistance);
            this.MaxDistance = maxDistance;

            int[] dst = new int[this.SpanCount];

            // Blur
            if (BoxBlur(1, src, dst) != src)
            {
                Helper.Swap(ref src, ref dst);
            }

            // Store distance.
            this.Dist = src;

            return true;
        }

        public void PaintRectRegion(int minx, int maxx, int miny, int maxy, int regId, int[] srcReg)
        {
            int w = this.Width;
            for (int y = miny; y < maxy; ++y)
            {
                for (int x = minx; x < maxx; ++x)
                {
                    var c = this.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        if (this.Areas[i] != AreaTypes.Unwalkable)
                        {
                            srcReg[i] = regId;
                        }
                    }
                }
            }
        }

        public bool BuildRegionsMonotone(int borderSize, int minRegionArea, int mergeRegionArea)
        {
            int w = this.Width;
            int h = this.Height;
            int id = 1;

            int[] srcReg = new int[this.SpanCount];

            int nsweeps = Math.Max(this.Width, this.Height);
            SweepSpan[] sweeps = new SweepSpan[nsweeps];

            // Mark border regions.
            if (borderSize > 0)
            {
                // Make sure border will not overflow.
                int bw = Math.Min(w, borderSize);
                int bh = Math.Min(h, borderSize);
                // Paint regions
                PaintRectRegion(0, bw, 0, h, id | RecastUtils.RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(w - bw, w, 0, h, id | RecastUtils.RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(0, w, 0, bh, id | RecastUtils.RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(0, w, h - bh, h, id | RecastUtils.RC_BORDER_REG, srcReg); id++;
            }

            this.BorderSize = borderSize;

            // Sweep one line at a time.
            for (int y = borderSize; y < h - borderSize; ++y)
            {
                // Collect spans from this row.
                int[] prev = new int[id + 1];
                int rid = 1;

                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    var c = this.Cells[x + y * w];

                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = this.Spans[i];
                        if (this.Areas[i] == AreaTypes.Unwalkable)
                        {
                            continue;
                        }

                        // -x
                        int previd = 0;
                        if (s.GetCon(0) != CompactSpan.RC_NOT_CONNECTED)
                        {
                            int ax = x + DirectionUtils.GetDirOffsetX(0);
                            int ay = y + DirectionUtils.GetDirOffsetY(0);
                            int ai = this.Cells[ax + ay * w].Index + s.GetCon(0);
                            if ((srcReg[ai] & RecastUtils.RC_BORDER_REG) == 0 && this.Areas[i] == this.Areas[ai])
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
                        if (s.GetCon(3) != CompactSpan.RC_NOT_CONNECTED)
                        {
                            int ax = x + DirectionUtils.GetDirOffsetX(3);
                            int ay = y + DirectionUtils.GetDirOffsetY(3);
                            int ai = this.Cells[ax + ay * w].Index + s.GetCon(3);
                            if (srcReg[ai] != 0 && (srcReg[ai] & RecastUtils.RC_BORDER_REG) == 0 && this.Areas[i] == this.Areas[ai])
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
                                    sweeps[previd].Nei = RecastUtils.RC_NULL_NEI;
                                }
                            }
                        }

                        srcReg[i] = previd;
                    }
                }

                // Create unique ID.
                for (int i = 1; i < rid; ++i)
                {
                    if (sweeps[i].Nei != RecastUtils.RC_NULL_NEI &&
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
                    var c = this.Cells[x + y * w];

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
            this.MaxRegions = id;
            var merged = MergeAndFilterRegions(minRegionArea, mergeRegionArea, id, srcReg, out _, out var maxRegionId);
            this.MaxRegions = maxRegionId;

            if (!merged)
            {
                return false;
            }

            // Monotone partitioning does not generate overlapping regions.

            // Store the result out.
            for (int i = 0; i < this.SpanCount; ++i)
            {
                this.Spans[i].Reg = srcReg[i];
            }

            return true;
        }

        public bool BuildRegions(int borderSize, int minRegionArea, int mergeRegionArea)
        {
            int w = this.Width;
            int h = this.Height;

            int LOG_NB_STACKS = 3;
            int NB_STACKS = 1 << LOG_NB_STACKS;
            List<List<LevelStackEntry>> lvlStacks = new List<List<LevelStackEntry>>();
            for (int i = 0; i < NB_STACKS; i++)
            {
                lvlStacks.Add(new List<LevelStackEntry>());
            }

            List<LevelStackEntry> stack = new List<LevelStackEntry>();

            int[] srcReg = new int[this.SpanCount];
            int[] srcDist = new int[this.SpanCount];

            int regionId = 1;
            int level = (this.MaxDistance + 1) & ~1;

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
                PaintRectRegion(0, bw, 0, h, (regionId | RecastUtils.RC_BORDER_REG), srcReg); regionId++;
                PaintRectRegion(w - bw, w, 0, h, (regionId | RecastUtils.RC_BORDER_REG), srcReg); regionId++;
                PaintRectRegion(0, w, 0, bh, (regionId | RecastUtils.RC_BORDER_REG), srcReg); regionId++;
                PaintRectRegion(0, w, h - bh, h, (regionId | RecastUtils.RC_BORDER_REG), srcReg); regionId++;
            }

            this.BorderSize = borderSize;

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
            this.MaxRegions = regionId;
            var merged = MergeAndFilterRegions(minRegionArea, mergeRegionArea, regionId, srcReg, out int[] overlaps, out int maxRegionId);
            this.MaxRegions = maxRegionId;
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
            for (int i = 0; i < this.SpanCount; ++i)
            {
                this.Spans[i].Reg = srcReg[i];
            }

            return true;
        }

        public bool BuildLayerRegions(int borderSize, int minRegionArea)
        {
            int w = this.Width;
            int h = this.Height;
            int id = 1;

            int[] srcReg = new int[this.SpanCount];

            int nsweeps = Math.Max(this.Width, this.Height);
            SweepSpan[] sweeps = Helper.CreateArray(nsweeps, new SweepSpan());

            // Mark border regions.
            if (borderSize > 0)
            {
                // Make sure border will not overflow.
                int bw = Math.Min(w, borderSize);
                int bh = Math.Min(h, borderSize);
                // Paint regions
                PaintRectRegion(0, bw, 0, h, id | RecastUtils.RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(w - bw, w, 0, h, id | RecastUtils.RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(0, w, 0, bh, id | RecastUtils.RC_BORDER_REG, srcReg); id++;
                PaintRectRegion(0, w, h - bh, h, id | RecastUtils.RC_BORDER_REG, srcReg); id++;
            }

            this.BorderSize = borderSize;

            // Sweep one line at a time.
            for (int y = borderSize; y < h - borderSize; ++y)
            {
                // Collect spans from this row.
                int[] prev = new int[1024];
                int rid = 1;

                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    var c = this.Cells[x + y * w];

                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = this.Spans[i];
                        if (this.Areas[i] == AreaTypes.Unwalkable)
                        {
                            continue;
                        }

                        // -x
                        int previd = 0;
                        if (s.GetCon(0) != CompactSpan.RC_NOT_CONNECTED)
                        {
                            int ax = x + DirectionUtils.GetDirOffsetX(0);
                            int ay = y + DirectionUtils.GetDirOffsetY(0);
                            int ai = this.Cells[ax + ay * w].Index + s.GetCon(0);
                            if ((srcReg[ai] & RecastUtils.RC_BORDER_REG) == 0 && this.Areas[i] == this.Areas[ai])
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
                        if (s.GetCon(3) != CompactSpan.RC_NOT_CONNECTED)
                        {
                            int ax = x + DirectionUtils.GetDirOffsetX(3);
                            int ay = y + DirectionUtils.GetDirOffsetY(3);
                            int ai = this.Cells[ax + ay * w].Index + s.GetCon(3);
                            if (srcReg[ai] != 0 && (srcReg[ai] & RecastUtils.RC_BORDER_REG) == 0 && this.Areas[i] == this.Areas[ai])
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
                                    sweeps[previd].Nei = RecastUtils.RC_NULL_NEI;
                                }
                            }
                        }

                        srcReg[i] = previd;
                    }
                }

                // Create unique ID.
                for (int i = 1; i < rid; ++i)
                {
                    if (sweeps[i].Nei != RecastUtils.RC_NULL_NEI &&
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
                    var c = this.Cells[x + y * w];

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
            this.MaxRegions = id;
            var merged = MergeAndFilterLayerRegions(minRegionArea, id, srcReg, out int maxRegionId);
            this.MaxRegions = maxRegionId;
            if (!merged)
            {
                return false;
            }

            // Store the result out.
            for (int i = 0; i < this.SpanCount; ++i)
            {
                this.Spans[i].Reg = srcReg[i];
            }

            return true;
        }

        public int GetCornerHeight(int x, int y, int i, int dir, out bool isBorderVertex)
        {
            isBorderVertex = false;

            var s = this.Spans[i];
            int ch = s.Y;
            int dirp = DirectionUtils.RotateCW(dir);

            int[] regs = { 0, 0, 0, 0 };

            // Combine region and area codes in order to prevent
            // border vertices which are in between two areas to be removed.
            regs[0] = this.Spans[i].Reg | ((int)this.Areas[i] << 16);

            if (s.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
            {
                int ax = x + DirectionUtils.GetDirOffsetX(dir);
                int ay = y + DirectionUtils.GetDirOffsetY(dir);
                int ai = this.Cells[ax + ay * this.Width].Index + s.GetCon(dir);
                var a = this.Spans[ai];
                ch = Math.Max(ch, a.Y);
                regs[1] = this.Spans[ai].Reg | ((int)this.Areas[ai] << 16);
                if (a.GetCon(dirp) != CompactSpan.RC_NOT_CONNECTED)
                {
                    int ax2 = ax + DirectionUtils.GetDirOffsetX(dirp);
                    int ay2 = ay + DirectionUtils.GetDirOffsetY(dirp);
                    int ai2 = this.Cells[ax2 + ay2 * this.Width].Index + a.GetCon(dirp);
                    var as2 = this.Spans[ai2];
                    ch = Math.Max(ch, as2.Y);
                    regs[2] = this.Spans[ai2].Reg | ((int)this.Areas[ai2] << 16);
                }
            }
            if (s.GetCon(dirp) != CompactSpan.RC_NOT_CONNECTED)
            {
                int ax = x + DirectionUtils.GetDirOffsetX(dirp);
                int ay = y + DirectionUtils.GetDirOffsetY(dirp);
                int ai = this.Cells[ax + ay * this.Width].Index + s.GetCon(dirp);
                var a = this.Spans[ai];
                ch = Math.Max(ch, a.Y);
                regs[3] = this.Spans[ai].Reg | ((int)this.Areas[ai] << 16);
                if (a.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
                {
                    int ax2 = ax + DirectionUtils.GetDirOffsetX(dir);
                    int ay2 = ay + DirectionUtils.GetDirOffsetY(dir);
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
                bool twoSameExts = (regs[a] & regs[b] & RecastUtils.RC_BORDER_REG) != 0 && regs[a] == regs[b];
                bool twoInts = ((regs[c] | regs[d]) & RecastUtils.RC_BORDER_REG) == 0;
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
    }
}
