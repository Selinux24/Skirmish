using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    static class Recast
    {
        #region Constants

        /// <summary>
        /// Defines the number of bits allocated to rcSpan::smin and rcSpan::smax.
        /// </summary>
        public const int RC_SPAN_HEIGHT_BITS = 13;
        /// <summary>
        /// Defines the maximum value for rcSpan::smin and rcSpan::smax.
        /// </summary>
        public const int RC_SPAN_MAX_HEIGHT = (1 << RC_SPAN_HEIGHT_BITS) - 1;
        /// <summary>
        /// The number of spans allocated per span spool.
        /// </summary>
        public const int RC_SPANS_PER_POOL = 2048;
        /// <summary>
        /// Heighfield border flag.
        /// If a heightfield region ID has this bit set, then the region is a border 
        /// region and its spans are considered unwalkable.
        /// (Used during the region and contour build process.)
        /// </summary>
        public const int RC_BORDER_REG = 0x8000;
        /// <summary>
        /// Polygon touches multiple regions.
        /// If a polygon has this region ID it was merged with or created
        /// from polygons of different regions during the polymesh
        /// build step that removes redundant border vertices. 
        /// (Used during the polymesh and detail polymesh build processes)
        /// </summary>
        public const int RC_MULTIPLE_REGS = 0;
        /// <summary>
        /// Border vertex flag.
        /// If a region ID has this bit set, then the associated element lies on
        /// a tile border. If a contour vertex's region ID has this bit set, the 
        /// vertex will later be removed in order to match the segments and vertices 
        /// at tile boundaries.
        /// (Used during the build process.)
        /// </summary>
        public const int RC_BORDER_VERTEX = 0x10000;
        /// <summary>
        /// Area border flag.
        /// If a region ID has this bit set, then the associated element lies on
        /// the border of an area.
        /// (Used during the region and contour build process.)
        /// </summary>
        public const int RC_AREA_BORDER = 0x20000;
        /// <summary>
        /// Applied to the region id field of contour vertices in order to extract the region id.
        /// The region id field of a vertex may have several flags applied to it.  So the
        /// fields value can't be used directly.
        /// </summary>
        public const int RC_CONTOUR_REG_MASK = 0xffff;
        /// <summary>
        /// An value which indicates an invalid index within a mesh.
        /// </summary>
        public const int RC_MESH_NULL_IDX = 0xffff;
        /// <summary>
        /// The value returned by #rcGetCon if the specified direction is not connected
        /// to another span. (Has no neighbor.)
        /// </summary>
        public const int RC_NOT_CONNECTED = 0x3f;

        public const int RC_NULL_NEI = 0xffff;
        public const int RC_UNSET_HEIGHT = 0xffff;

        public const int VERTEX_BUCKET_COUNT = (1 << 12);

        #endregion

        #region RECAST

        public static void CalcBounds(Vector3[] verts, int nv, out Vector3 bmin, out Vector3 bmax)
        {
            // Calculate bounding box.
            bmin = verts[0];
            bmax = verts[0];
            for (int i = 1; i < nv; ++i)
            {
                Vector3 v = verts[i];
                bmin = Vector3.Min(bmin, v);
                bmax = Vector3.Max(bmax, v);
            }
        }
        public static void CalcGridSize(BoundingBox b, float cellSize, out int w, out int h)
        {
            w = (int)((b.Maximum.X - b.Minimum.X) / cellSize + 0.5f);
            h = (int)((b.Maximum.Z - b.Minimum.Z) / cellSize + 0.5f);
        }
        public static Heightfield CreateHeightfield(int width, int height, BoundingBox bbox, float cs, float ch)
        {
            return new Heightfield
            {
                width = width,
                height = height,
                boundingBox = bbox,
                cs = cs,
                ch = ch,
                spans = new Span[width * height],
            };
        }
        public static int MarkWalkableTriangles(float walkableSlopeAngle, Triangle[] tris, TileCacheAreas[] areas)
        {
            float walkableThr = (float)Math.Cos(walkableSlopeAngle / 180.0f * MathUtil.Pi);

            int count = 0;

            for (int i = 0; i < tris.Length; i++)
            {
                var tri = tris[i];
                Vector3 norm = tri.Normal;

                // Check if the face is walkable.
                if (norm.Y > walkableThr)
                {
                    areas[i] = TileCacheAreas.RC_WALKABLE_AREA;
                    count++;
                }
            }

            return count;
        }
        public static void ClearUnwalkableTriangles(float walkableSlopeAngle, Vector3[] verts, int nv, Triangle[] tris, int nt, TileCacheAreas[] areas)
        {
            float walkableThr = (float)Math.Cos(walkableSlopeAngle / 180.0f * MathUtil.Pi);

            for (int i = 0; i < nt; ++i)
            {
                var tri = tris[i];
                Vector3 norm = tri.Normal;

                // Check if the face is walkable.
                if (norm[1] <= walkableThr)
                {
                    areas[i] = TileCacheAreas.RC_NULL_AREA;
                }
            }
        }
        public static int GetHeightFieldSpanCount(Heightfield hf)
        {
            int w = hf.width;
            int h = hf.height;

            int spanCount = 0;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    for (Span s = hf.spans[x + y * w]; s != null; s = s.next)
                    {
                        if (s.area != TileCacheAreas.RC_NULL_AREA)
                        {
                            spanCount++;
                        }
                    }
                }
            }

            return spanCount;
        }
        public static bool BuildCompactHeightfield(int walkableHeight, int walkableClimb, Heightfield hf, out CompactHeightfield chf)
        {
            int w = hf.width;
            int h = hf.height;
            int spanCount = GetHeightFieldSpanCount(hf);
            var bbox = hf.boundingBox;
            bbox.Maximum.Y += walkableHeight * hf.ch;

            // Fill in header.
            chf = new CompactHeightfield
            {
                Width = w,
                Height = h,
                SpanCount = spanCount,
                WalkableHeight = walkableHeight,
                WalkableClimb = walkableClimb,
                MaxRegions = 0,
                BoundingBox = bbox,
                CS = hf.cs,
                CH = hf.ch,
                Cells = new CompactCell[w * h],
                Spans = new CompactSpan[spanCount],
                Areas = new TileCacheAreas[spanCount]
            };

            // Fill in cells and spans.
            int idx = 0;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var s = hf.spans[x + y * w];

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
                        if (s.area != TileCacheAreas.RC_NULL_AREA)
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
            int maxLayers = RC_NOT_CONNECTED - 1;
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
                            SetCon(ref s, dir, RC_NOT_CONNECTED);
                            int nx = x + GetDirOffsetX(dir);
                            int ny = y + GetDirOffsetY(dir);
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

                                    SetCon(ref s, dir, lidx);
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

            return true;
        }
        public static void SetCon(ref CompactSpan s, int dir, int i)
        {
            int shift = dir * 6;
            int con = s.Con;
            s.Con = (con & ~(0x3f << shift)) | ((i & 0x3f) << shift);
        }
        public static int GetCon(CompactSpan s, int dir)
        {
            int shift = dir * 6;
            return (s.Con >> shift) & 0x3f;
        }
        public static int GetDirOffsetX(int dir)
        {
            int[] offset = new[] { -1, 0, 1, 0, };
            return offset[dir & 0x03];
        }
        public static int GetDirOffsetY(int dir)
        {
            int[] offset = new[] { 0, 1, 0, -1 };
            return offset[dir & 0x03];
        }
        public static int GetDirForOffset(int x, int y)
        {
            int[] dirs = { 3, 0, -1, 2, 1 };
            return dirs[((y + 1) << 1) + x];
        }

        #endregion

        #region RECASTAREA

        /// <summary>
        /// Basically, any spans that are closer to a boundary or obstruction than the specified radius are marked as unwalkable.
        /// </summary>
        /// <param name="radius">Radius</param>
        /// <param name="chf">Compact height field</param>
        /// <returns>Returns always true</returns>
        /// <remarks>
        /// This method is usually called immediately after the heightfield has been built.
        /// </remarks>
        public static bool ErodeWalkableArea(int radius, CompactHeightfield chf)
        {
            int w = chf.Width;
            int h = chf.Height;

            // Init distance.
            int[] dist = Helper.CreateArray(chf.SpanCount, 0xff);

            // Mark boundary cells.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    CompactCell c = chf.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        if (chf.Areas[i] == TileCacheAreas.RC_NULL_AREA)
                        {
                            dist[i] = 0;
                        }
                        else
                        {
                            CompactSpan s = chf.Spans[i];
                            int nc = 0;
                            for (int dir = 0; dir < 4; ++dir)
                            {
                                if (GetCon(s, dir) != RC_NOT_CONNECTED)
                                {
                                    int nx = x + GetDirOffsetX(dir);
                                    int ny = y + GetDirOffsetY(dir);
                                    int nidx = chf.Cells[nx + ny * w].Index + GetCon(s, dir);
                                    if (chf.Areas[nidx] != TileCacheAreas.RC_NULL_AREA)
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
                    CompactCell c = chf.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        CompactSpan s = chf.Spans[i];
                        if (GetCon(s, 0) != RC_NOT_CONNECTED)
                        {
                            // (-1,0)
                            int ax = x + GetDirOffsetX(0);
                            int ay = y + GetDirOffsetY(0);
                            int ai = chf.Cells[ax + ay * w].Index + GetCon(s, 0);
                            CompactSpan asp = chf.Spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (-1,-1)
                            if (GetCon(asp, 3) != RC_NOT_CONNECTED)
                            {
                                int aax = ax + GetDirOffsetX(3);
                                int aay = ay + GetDirOffsetY(3);
                                int aai = chf.Cells[aax + aay * w].Index + GetCon(asp, 3);
                                nd = Math.Min(dist[aai] + 3, 255);
                                if (nd < dist[i])
                                {
                                    dist[i] = nd;
                                }
                            }
                        }
                        if (GetCon(s, 3) != RC_NOT_CONNECTED)
                        {
                            // (0,-1)
                            int ax = x + GetDirOffsetX(3);
                            int ay = y + GetDirOffsetY(3);
                            int ai = chf.Cells[ax + ay * w].Index + GetCon(s, 3);
                            CompactSpan asp = chf.Spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (1,-1)
                            if (GetCon(asp, 2) != RC_NOT_CONNECTED)
                            {
                                int aax = ax + GetDirOffsetX(2);
                                int aay = ay + GetDirOffsetY(2);
                                int aai = chf.Cells[aax + aay * w].Index + GetCon(asp, 2);
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
                    var c = chf.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = chf.Spans[i];
                        if (GetCon(s, 2) != RC_NOT_CONNECTED)
                        {
                            // (1,0)
                            int ax = x + GetDirOffsetX(2);
                            int ay = y + GetDirOffsetY(2);
                            int ai = chf.Cells[ax + ay * w].Index + GetCon(s, 2);
                            var asp = chf.Spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (1,1)
                            if (GetCon(asp, 1) != RC_NOT_CONNECTED)
                            {
                                int aax = ax + GetDirOffsetX(1);
                                int aay = ay + GetDirOffsetY(1);
                                int aai = chf.Cells[aax + aay * w].Index + GetCon(asp, 1);
                                nd = Math.Min(dist[aai] + 3, 255);
                                if (nd < dist[i])
                                {
                                    dist[i] = nd;
                                }
                            }
                        }
                        if (GetCon(s, 1) != RC_NOT_CONNECTED)
                        {
                            // (0,1)
                            int ax = x + GetDirOffsetX(1);
                            int ay = y + GetDirOffsetY(1);
                            int ai = chf.Cells[ax + ay * w].Index + GetCon(s, 1);
                            var asp = chf.Spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (-1,1)
                            if (GetCon(asp, 0) != RC_NOT_CONNECTED)
                            {
                                int aax = ax + GetDirOffsetX(0);
                                int aay = ay + GetDirOffsetY(0);
                                int aai = chf.Cells[aax + aay * w].Index + GetCon(asp, 0);
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
            for (int i = 0; i < chf.SpanCount; ++i)
            {
                if (dist[i] < thr)
                {
                    chf.Areas[i] = TileCacheAreas.RC_NULL_AREA;
                }
            }

            return true;
        }

        public static void InsertSort(TileCacheAreas[] a, int n)
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
        /// This filter is usually applied after applying area id's using functions such as MarkBoxArea, MarkConvexPolyArea, and MarkCylinderArea.
        /// </summary>
        /// <param name="chf">Compact height field</param>
        /// <returns>Returns always true</returns>
        public static bool MedianFilterWalkableArea(CompactHeightfield chf)
        {
            int w = chf.Width;
            int h = chf.Height;

            // Init distance.
            TileCacheAreas[] areas = Helper.CreateArray(chf.SpanCount, (TileCacheAreas)0xff);

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = chf.Spans[i];
                        if (chf.Areas[i] == TileCacheAreas.RC_NULL_AREA)
                        {
                            areas[i] = chf.Areas[i];
                            continue;
                        }

                        TileCacheAreas[] nei = new TileCacheAreas[9];
                        for (int j = 0; j < 9; ++j)
                        {
                            nei[j] = chf.Areas[i];
                        }

                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (GetCon(s, dir) != RC_NOT_CONNECTED)
                            {
                                int ax = x + GetDirOffsetX(dir);
                                int ay = y + GetDirOffsetY(dir);
                                int ai = chf.Cells[ax + ay * w].Index + GetCon(s, dir);
                                if (chf.Areas[ai] != TileCacheAreas.RC_NULL_AREA)
                                {
                                    nei[dir * 2 + 0] = chf.Areas[ai];
                                }

                                var a = chf.Spans[ai];
                                int dir2 = (dir + 1) & 0x3;
                                if (GetCon(a, dir2) != RC_NOT_CONNECTED)
                                {
                                    int ax2 = ax + GetDirOffsetX(dir2);
                                    int ay2 = ay + GetDirOffsetY(dir2);
                                    int ai2 = chf.Cells[ax2 + ay2 * w].Index + GetCon(a, dir2);
                                    if (chf.Areas[ai2] != TileCacheAreas.RC_NULL_AREA)
                                    {
                                        nei[dir * 2 + 1] = chf.Areas[ai2];
                                    }
                                }
                            }
                        }
                        InsertSort(nei, 9);
                        areas[i] = nei[4];
                    }
                }
            }

            Array.Copy(areas, chf.Areas, chf.SpanCount);

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
        public static void MarkBoxArea(Vector3 bmin, Vector3 bmax, TileCacheAreas areaId, CompactHeightfield chf)
        {
            int minx = (int)((bmin.X - chf.BoundingBox.Minimum.X) / chf.CS);
            int miny = (int)((bmin.Y - chf.BoundingBox.Minimum.Y) / chf.CH);
            int minz = (int)((bmin.Z - chf.BoundingBox.Minimum.Z) / chf.CS);
            int maxx = (int)((bmax.X - chf.BoundingBox.Minimum.X) / chf.CS);
            int maxy = (int)((bmax.Y - chf.BoundingBox.Minimum.Y) / chf.CH);
            int maxz = (int)((bmax.Z - chf.BoundingBox.Minimum.Z) / chf.CS);

            if (maxx < 0) return;
            if (minx >= chf.Width) return;
            if (maxz < 0) return;
            if (minz >= chf.Height) return;

            if (minx < 0) minx = 0;
            if (maxx >= chf.Width) maxx = chf.Width - 1;
            if (minz < 0) minz = 0;
            if (maxz >= chf.Height) maxz = chf.Height - 1;

            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    var c = chf.Cells[x + z * chf.Width];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = chf.Spans[i];
                        if (s.Y >= miny && s.Y <= maxy && chf.Areas[i] != TileCacheAreas.RC_NULL_AREA)
                        {
                            chf.Areas[i] = areaId;
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
        public static bool PointInPoly(int nvert, Vector3[] verts, Vector3 p)
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
        public static void MarkConvexPolyArea(Vector3[] verts, int nverts, float hmin, float hmax, TileCacheAreas areaId, CompactHeightfield chf)
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

            int minx = (int)((bmin[0] - chf.BoundingBox.Minimum[0]) / chf.CS);
            int miny = (int)((bmin[1] - chf.BoundingBox.Minimum[1]) / chf.CH);
            int minz = (int)((bmin[2] - chf.BoundingBox.Minimum[2]) / chf.CS);
            int maxx = (int)((bmax[0] - chf.BoundingBox.Minimum[0]) / chf.CS);
            int maxy = (int)((bmax[1] - chf.BoundingBox.Minimum[1]) / chf.CH);
            int maxz = (int)((bmax[2] - chf.BoundingBox.Minimum[2]) / chf.CS);

            if (maxx < 0) return;
            if (minx >= chf.Width) return;
            if (maxz < 0) return;
            if (minz >= chf.Height) return;

            if (minx < 0) minx = 0;
            if (maxx >= chf.Width) maxx = chf.Width - 1;
            if (minz < 0) minz = 0;
            if (maxz >= chf.Height) maxz = chf.Height - 1;


            // TODO: Optimize.
            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    CompactCell c = chf.Cells[x + z * chf.Width];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        CompactSpan s = chf.Spans[i];
                        if (chf.Areas[i] == TileCacheAreas.RC_NULL_AREA)
                        {
                            continue;
                        }

                        if (s.Y >= miny && s.Y <= maxy)
                        {
                            Vector3 p = new Vector3();
                            p[0] = chf.BoundingBox.Minimum[0] + (x + 0.5f) * chf.CS;
                            p[1] = 0;
                            p[2] = chf.BoundingBox.Minimum[2] + (z + 0.5f) * chf.CS;

                            if (PointInPoly(nverts, verts, p))
                            {
                                chf.Areas[i] = areaId;
                            }
                        }
                    }
                }
            }
        }

        public static int OffsetPoly(Vector3[] verts, int nverts, float offset, out Vector3[] outVerts, int maxOutVerts)
        {
            outVerts = new Vector3[maxOutVerts];

            float MITER_LIMIT = 1.20f;

            int n = 0;

            for (int i = 0; i < nverts; i++)
            {
                int a = (i + nverts - 1) % nverts;
                int b = i;
                int c = (i + 1) % nverts;
                Vector3 va = verts[a];
                Vector3 vb = verts[b];
                Vector3 vc = verts[c];
                float dx0 = vb.X - va.X;
                float dy0 = vb.Z - va.Z;
                float d0 = dx0 * dx0 + dy0 * dy0;
                if (d0 > 1e-6f)
                {
                    d0 = 1.0f / (float)Math.Sqrt(d0);
                    dx0 *= d0;
                    dy0 *= d0;
                }
                float dx1 = vc.X - vb.X;
                float dy1 = vc.Z - vb.Z;
                float d1 = dx1 * dx1 + dy1 * dy1;
                if (d1 > 1e-6f)
                {
                    d1 = 1.0f / (float)Math.Sqrt(d1);
                    dx1 *= d1;
                    dy1 *= d1;
                }
                float dlx0 = -dy0;
                float dly0 = dx0;
                float dlx1 = -dy1;
                float dly1 = dx1;
                float cross = dx1 * dy0 - dx0 * dy1;
                float dmx = (dlx0 + dlx1) * 0.5f;
                float dmy = (dly0 + dly1) * 0.5f;
                float dmr2 = dmx * dmx + dmy * dmy;
                bool bevel = dmr2 * MITER_LIMIT * MITER_LIMIT < 1.0f;
                if (dmr2 > 1e-6f)
                {
                    float scale = 1.0f / dmr2;
                    dmx *= scale;
                    dmy *= scale;
                }

                if (bevel && cross < 0.0f)
                {
                    if (n + 2 >= maxOutVerts)
                    {
                        return 0;
                    }
                    float d = (1.0f - (dx0 * dx1 + dy0 * dy1)) * 0.5f;
                    outVerts[n].X = vb.X + (-dlx0 + dx0 * d) * offset;
                    outVerts[n].Y = vb.Y;
                    outVerts[n].Z = vb.Z + (-dly0 + dy0 * d) * offset;
                    n++;
                    outVerts[n].X = vb.X + (-dlx1 - dx1 * d) * offset;
                    outVerts[n].Y = vb.Y;
                    outVerts[n].Z = vb.Z + (-dly1 - dy1 * d) * offset;
                    n++;
                }
                else
                {
                    if (n + 1 >= maxOutVerts)
                    {
                        return 0;
                    }
                    outVerts[n].X = vb.X - dmx * offset;
                    outVerts[n].Y = vb.Y;
                    outVerts[n].Z = vb.Z - dmy * offset;
                    n++;
                }
            }

            return n;
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
        public static void MarkCylinderArea(Vector3 pos, float r, float h, TileCacheAreas areaId, CompactHeightfield chf)
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

            int minx = (int)((bmin.X - chf.BoundingBox.Minimum.X) / chf.CS);
            int miny = (int)((bmin.Y - chf.BoundingBox.Minimum.Y) / chf.CH);
            int minz = (int)((bmin.Z - chf.BoundingBox.Minimum.Z) / chf.CS);
            int maxx = (int)((bmax.X - chf.BoundingBox.Minimum.X) / chf.CS);
            int maxy = (int)((bmax.Y - chf.BoundingBox.Minimum.Y) / chf.CH);
            int maxz = (int)((bmax.Z - chf.BoundingBox.Minimum.Z) / chf.CS);

            if (maxx < 0) return;
            if (minx >= chf.Width) return;
            if (maxz < 0) return;
            if (minz >= chf.Height) return;

            if (minx < 0) minx = 0;
            if (maxx >= chf.Width) maxx = chf.Width - 1;
            if (minz < 0) minz = 0;
            if (maxz >= chf.Height) maxz = chf.Height - 1;


            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    var c = chf.Cells[x + z * chf.Width];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = chf.Spans[i];

                        if (chf.Areas[i] == TileCacheAreas.RC_NULL_AREA)
                        {
                            continue;
                        }

                        if (s.Y >= miny && s.Y <= maxy)
                        {
                            float sx = chf.BoundingBox.Minimum.X + (x + 0.5f) * chf.CS;
                            float sz = chf.BoundingBox.Minimum.Z + (z + 0.5f) * chf.CS;
                            float dx = sx - pos.X;
                            float dz = sz - pos.Z;

                            if (dx * dx + dz * dz < r2)
                            {
                                chf.Areas[i] = areaId;
                            }
                        }
                    }
                }
            }
        }


        #endregion

        #region RECASTREGION

        private static void CalculateDistanceField(CompactHeightfield chf, int[] src, out int maxDist)
        {
            int w = chf.Width;
            int h = chf.Height;

            // Init distance and points.
            for (int i = 0; i < chf.SpanCount; ++i)
            {
                src[i] = 0xffff;
            }

            // Mark boundary cells.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = chf.Spans[i];
                        var area = chf.Areas[i];

                        int nc = 0;
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (GetCon(s, dir) != RC_NOT_CONNECTED)
                            {
                                int ax = x + GetDirOffsetX(dir);
                                int ay = y + GetDirOffsetY(dir);
                                int ai = chf.Cells[ax + ay * w].Index + GetCon(s, dir);
                                if (area == chf.Areas[ai])
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
                    var c = chf.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = chf.Spans[i];

                        if (GetCon(s, 0) != RC_NOT_CONNECTED)
                        {
                            // (-1,0)
                            int ax = x + GetDirOffsetX(0);
                            int ay = y + GetDirOffsetY(0);
                            int ai = chf.Cells[ax + ay * w].Index + GetCon(s, 0);
                            var a = chf.Spans[ai];
                            if (src[ai] + 2 < src[i])
                            {
                                src[i] = src[ai] + 2;
                            }

                            // (-1,-1)
                            if (GetCon(a, 3) != RC_NOT_CONNECTED)
                            {
                                int aax = ax + GetDirOffsetX(3);
                                int aay = ay + GetDirOffsetY(3);
                                int aai = chf.Cells[aax + aay * w].Index + GetCon(a, 3);
                                if (src[aai] + 3 < src[i])
                                {
                                    src[i] = src[aai] + 3;
                                }
                            }
                        }
                        if (GetCon(s, 3) != RC_NOT_CONNECTED)
                        {
                            // (0,-1)
                            int ax = x + GetDirOffsetX(3);
                            int ay = y + GetDirOffsetY(3);
                            int ai = chf.Cells[ax + ay * w].Index + GetCon(s, 3);
                            var a = chf.Spans[ai];
                            if (src[ai] + 2 < src[i])
                            {
                                src[i] = src[ai] + 2;
                            }

                            // (1,-1)
                            if (GetCon(a, 2) != RC_NOT_CONNECTED)
                            {
                                int aax = ax + GetDirOffsetX(2);
                                int aay = ay + GetDirOffsetY(2);
                                int aai = chf.Cells[aax + aay * w].Index + GetCon(a, 2);
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
                    var c = chf.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = chf.Spans[i];

                        if (GetCon(s, 2) != RC_NOT_CONNECTED)
                        {
                            // (1,0)
                            int ax = x + GetDirOffsetX(2);
                            int ay = y + GetDirOffsetY(2);
                            int ai = chf.Cells[ax + ay * w].Index + GetCon(s, 2);
                            var a = chf.Spans[ai];
                            if (src[ai] + 2 < src[i])
                            {
                                src[i] = src[ai] + 2;
                            }

                            // (1,1)
                            if (GetCon(a, 1) != RC_NOT_CONNECTED)
                            {
                                int aax = ax + GetDirOffsetX(1);
                                int aay = ay + GetDirOffsetY(1);
                                int aai = chf.Cells[aax + aay * w].Index + GetCon(a, 1);
                                if (src[aai] + 3 < src[i])
                                {
                                    src[i] = src[aai] + 3;
                                }
                            }
                        }
                        if (GetCon(s, 1) != RC_NOT_CONNECTED)
                        {
                            // (0,1)
                            int ax = x + GetDirOffsetX(1);
                            int ay = y + GetDirOffsetY(1);
                            int ai = chf.Cells[ax + ay * w].Index + GetCon(s, 1);
                            var a = chf.Spans[ai];
                            if (src[ai] + 2 < src[i])
                            {
                                src[i] = src[ai] + 2;
                            }

                            // (-1,1)
                            if (GetCon(a, 0) != RC_NOT_CONNECTED)
                            {
                                int aax = ax + GetDirOffsetX(0);
                                int aay = ay + GetDirOffsetY(0);
                                int aai = chf.Cells[aax + aay * w].Index + GetCon(a, 0);
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
            for (int i = 0; i < chf.SpanCount; ++i)
            {
                maxDist = Math.Max(src[i], maxDist);
            }
        }
        private static int[] BoxBlur(CompactHeightfield chf, int thr, int[] src, int[] dst)
        {
            int w = chf.Width;
            int h = chf.Height;

            thr *= 2;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = chf.Spans[i];
                        var cd = src[i];
                        if (cd <= thr)
                        {
                            dst[i] = cd;
                            continue;
                        }

                        int d = cd;
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (GetCon(s, dir) != RC_NOT_CONNECTED)
                            {
                                int ax = x + GetDirOffsetX(dir);
                                int ay = y + GetDirOffsetY(dir);
                                int ai = chf.Cells[ax + ay * w].Index + GetCon(s, dir);
                                d += src[ai];

                                var a = chf.Spans[ai];
                                int dir2 = (dir + 1) & 0x3;
                                if (GetCon(a, dir2) != RC_NOT_CONNECTED)
                                {
                                    int ax2 = ax + GetDirOffsetX(dir2);
                                    int ay2 = ay + GetDirOffsetY(dir2);
                                    int ai2 = chf.Cells[ax2 + ay2 * w].Index + GetCon(a, dir2);
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
        private static bool FloodRegion(int x, int y, int i, int level, int r, CompactHeightfield chf, int[] srcReg, int[] srcDist, List<int> stack)
        {
            int w = chf.Width;

            var area = chf.Areas[i];

            // Flood fill mark region.
            stack.Clear();
            stack.Add(x);
            stack.Add(y);
            stack.Add(i);
            srcReg[i] = r;
            srcDist[i] = 0;

            int lev = level >= 2 ? level - 2 : 0;
            int count = 0;

            while (stack.Count > 0)
            {
                int ci = stack.Pop();
                int cy = stack.Pop();
                int cx = stack.Pop();

                var cs = chf.Spans[ci];

                // Check if any of the neighbours already have a valid region set.
                int ar = 0;
                for (int dir = 0; dir < 4; ++dir)
                {
                    // 8 connected
                    if (GetCon(cs, dir) != RC_NOT_CONNECTED)
                    {
                        int ax = cx + GetDirOffsetX(dir);
                        int ay = cy + GetDirOffsetY(dir);
                        int ai = chf.Cells[ax + ay * w].Index + GetCon(cs, dir);
                        if (chf.Areas[ai] != area)
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

                        var a = chf.Spans[ai];

                        int dir2 = (dir + 1) & 0x3;
                        if (GetCon(a, dir2) != RC_NOT_CONNECTED)
                        {
                            int ax2 = ax + GetDirOffsetX(dir2);
                            int ay2 = ay + GetDirOffsetY(dir2);
                            int ai2 = chf.Cells[ax2 + ay2 * w].Index + GetCon(a, dir2);
                            if (chf.Areas[ai2] != area)
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
                    if (GetCon(cs, dir) != RC_NOT_CONNECTED)
                    {
                        int ax = cx + GetDirOffsetX(dir);
                        int ay = cy + GetDirOffsetY(dir);
                        int ai = chf.Cells[ax + ay * w].Index + GetCon(cs, dir);
                        if (chf.Areas[ai] != area)
                        {
                            continue;
                        }
                        if (chf.Dist[ai] >= lev && srcReg[ai] == 0)
                        {
                            srcReg[ai] = r;
                            srcDist[ai] = 0;
                            stack.Add(ax);
                            stack.Add(ay);
                            stack.Add(ai);
                        }
                    }
                }
            }

            return count > 0;
        }
        private static int[] ExpandRegions(int maxIter, int level, CompactHeightfield chf, int[] srcReg, int[] srcDist, int[] dstReg, int[] dstDist, List<int> stack, bool fillStack)
        {
            int w = chf.Width;
            int h = chf.Height;

            if (fillStack)
            {
                // Find cells revealed by the raised level.
                stack.Clear();
                for (int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        var c = chf.Cells[x + y * w];
                        for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                        {
                            if (chf.Dist[i] >= level && srcReg[i] == 0 && chf.Areas[i] != TileCacheAreas.RC_NULL_AREA)
                            {
                                stack.Add(x);
                                stack.Add(y);
                                stack.Add(i);
                            }
                        }
                    }
                }
            }
            else // use cells in the input stack
            {
                // mark all cells which already have a region
                for (int j = 0; j < stack.Count; j += 3)
                {
                    int i = stack[j + 2];
                    if (srcReg[i] != 0)
                    {
                        stack[j + 2] = -1;
                    }
                }
            }

            int iter = 0;
            while (stack.Count > 0)
            {
                int failed = 0;

                Array.Copy(srcReg, dstReg, chf.SpanCount);
                Array.Copy(srcDist, dstDist, chf.SpanCount);

                for (int j = 0; j < stack.Count; j += 3)
                {
                    int x = stack[j + 0];
                    int y = stack[j + 1];
                    int i = stack[j + 2];
                    if (i < 0)
                    {
                        failed++;
                        continue;
                    }

                    int r = srcReg[i];
                    int d2 = int.MaxValue;
                    var area = chf.Areas[i];
                    var s = chf.Spans[i];
                    for (int dir = 0; dir < 4; ++dir)
                    {
                        if (GetCon(s, dir) == RC_NOT_CONNECTED) continue;
                        int ax = x + GetDirOffsetX(dir);
                        int ay = y + GetDirOffsetY(dir);
                        int ai = chf.Cells[ax + ay * w].Index + GetCon(s, dir);
                        if (chf.Areas[ai] != area) continue;
                        if (srcReg[ai] > 0 && (srcReg[ai] & RC_BORDER_REG) == 0 && srcDist[ai] + 2 < d2)
                        {
                            r = srcReg[ai];
                            d2 = srcDist[ai] + 2;
                        }
                    }
                    if (r != 0)
                    {
                        stack[j + 2] = -1; // mark as used
                        dstReg[i] = r;
                        dstDist[i] = d2;
                    }
                    else
                    {
                        failed++;
                    }
                }

                // rcSwap source and dest.
                Helper.Swap(ref srcReg, ref dstReg);
                Helper.Swap(ref srcDist, ref dstDist);

                if (failed * 3 == stack.Count)
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

            return srcReg;
        }
        private static void SortCellsByLevel(int startLevel, CompactHeightfield chf, int[] srcReg, int nbStacks, List<List<int>> stacks, int loglevelsPerStack) // the levels per stack (2 in our case) as a bit shift
        {
            int w = chf.Width;
            int h = chf.Height;
            startLevel = startLevel >> loglevelsPerStack;

            for (int j = 0; j < nbStacks; ++j)
            {
                stacks[j].Clear();
            }

            // put all cells in the level range into the appropriate stacks
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        if (chf.Areas[i] == TileCacheAreas.RC_NULL_AREA || srcReg[i] != 0)
                        {
                            continue;
                        }

                        int level = chf.Dist[i] >> loglevelsPerStack;
                        int sId = startLevel - level;
                        if (sId >= nbStacks)
                        {
                            continue;
                        }
                        if (sId < 0)
                        {
                            sId = 0;
                        }

                        stacks[sId].Add(x);
                        stacks[sId].Add(y);
                        stacks[sId].Add(i);
                    }
                }
            }
        }
        private static void AppendStacks(List<int> srcStack, List<int> dstStack, int[] srcReg)
        {
            for (int j = 0; j < srcStack.Count; j += 3)
            {
                int i = srcStack[j + 2];
                if ((i < 0) || (srcReg[i] != 0))
                {
                    continue;
                }
                dstStack.Add(srcStack[j]);
                dstStack.Add(srcStack[j + 1]);
                dstStack.Add(srcStack[j + 2]);
            }
        }
        private static void RemoveAdjacentNeighbours(Region reg)
        {
            // Remove adjacent duplicates.
            for (int i = 0; i < reg.connections.Count && reg.connections.Count > 1;)
            {
                int ni = (i + 1) % reg.connections.Count;
                if (reg.connections[i] == reg.connections[ni])
                {
                    // Remove duplicate
                    for (int j = i; j < reg.connections.Count - 1; ++j)
                    {
                        reg.connections[j] = reg.connections[j + 1];
                    }
                    reg.connections.RemoveAt(reg.connections.Count - 1);
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
            for (int i = 0; i < reg.connections.Count; ++i)
            {
                if (reg.connections[i] == oldId)
                {
                    reg.connections[i] = newId;
                    neiChanged = true;
                }
            }
            for (int i = 0; i < reg.floors.Count; ++i)
            {
                if (reg.floors[i] == oldId)
                {
                    reg.floors[i] = newId;
                }
            }
            if (neiChanged)
            {
                RemoveAdjacentNeighbours(reg);
            }
        }
        private static bool CanMergeWithRegion(Region rega, Region regb)
        {
            if (rega.areaType != regb.areaType)
            {
                return false;
            }
            int n = 0;
            for (int i = 0; i < rega.connections.Count; ++i)
            {
                if (rega.connections[i] == regb.id)
                {
                    n++;
                }
            }
            if (n > 1)
            {
                return false;
            }
            for (int i = 0; i < rega.floors.Count; ++i)
            {
                if (rega.floors[i] == regb.id)
                {
                    return false;
                }
            }
            return true;
        }
        private static void AddUniqueFloorRegion(Region reg, int n)
        {
            for (int i = 0; i < reg.floors.Count; ++i)
            {
                if (reg.floors[i] == n)
                {
                    return;
                }
            }
            reg.floors.Add(n);
        }
        private static bool MergeRegions(Region rega, Region regb)
        {
            int aid = rega.id;
            int bid = regb.id;

            // Duplicate current neighbourhood.
            List<int> acon = new List<int>(rega.connections);
            List<int> bcon = regb.connections;

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
            rega.connections.Clear();
            for (int i = 0, ni = acon.Count; i < ni - 1; ++i)
            {
                rega.connections.Add(acon[(insa + 1 + i) % ni]);
            }

            for (int i = 0, ni = bcon.Count; i < ni - 1; ++i)
            {
                rega.connections.Add(bcon[(insb + 1 + i) % ni]);
            }

            RemoveAdjacentNeighbours(rega);

            for (int j = 0; j < regb.floors.Count; ++j)
            {
                AddUniqueFloorRegion(rega, regb.floors[j]);
            }
            rega.spanCount += regb.spanCount;
            regb.spanCount = 0;
            regb.connections.Clear();

            return true;
        }
        private static bool IsRegionConnectedToBorder(Region reg)
        {
            // Region is connected to border if
            // one of the neighbours is null id.
            for (int i = 0; i < reg.connections.Count; ++i)
            {
                if (reg.connections[i] == 0)
                {
                    return true;
                }
            }
            return false;
        }
        private static bool IsSolidEdge(CompactHeightfield chf, int[] srcReg, int x, int y, int i, int dir)
        {
            var s = chf.Spans[i];
            int r = 0;
            if (GetCon(s, dir) != RC_NOT_CONNECTED)
            {
                int ax = x + GetDirOffsetX(dir);
                int ay = y + GetDirOffsetY(dir);
                int ai = chf.Cells[ax + ay * chf.Width].Index + GetCon(s, dir);
                r = srcReg[ai];
            }
            if (r == srcReg[i])
            {
                return false;
            }
            return true;
        }
        private static void WalkContour(int x, int y, int i, CompactHeightfield chf, int[] flags, out List<Int4> points)
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

            var area = chf.Areas[i];

            int iter = 0;
            while (++iter < 40000)
            {
                if ((flags[i] & (1 << dir)) != 0)
                {
                    // Choose the edge corner
                    bool isAreaBorder = false;
                    int px = x;
                    int py = GetCornerHeight(x, y, i, dir, chf, out bool isBorderVertex);
                    int pz = y;
                    switch (dir)
                    {
                        case 0: pz++; break;
                        case 1: px++; pz++; break;
                        case 2: px++; break;
                    }
                    int r = 0;
                    var s = chf.Spans[i];
                    if (GetCon(s, dir) != RC_NOT_CONNECTED)
                    {
                        int ax = x + GetDirOffsetX(dir);
                        int ay = y + GetDirOffsetY(dir);
                        int ai = chf.Cells[ax + ay * chf.Width].Index + GetCon(s, dir);
                        r = chf.Spans[ai].Reg;
                        if (area != chf.Areas[ai])
                        {
                            isAreaBorder = true;
                        }
                    }
                    if (isBorderVertex)
                    {
                        r |= RC_BORDER_VERTEX;
                    }
                    if (isAreaBorder)
                    {
                        r |= RC_AREA_BORDER;
                    }
                    points.Add(new Int4(px, py, pz, r));

                    flags[i] &= ~(1 << dir); // Remove visited edges
                    dir = (dir + 1) & 0x3;  // Rotate CW
                }
                else
                {
                    int ni = -1;
                    int nx = x + GetDirOffsetX(dir);
                    int ny = y + GetDirOffsetY(dir);
                    var s = chf.Spans[i];
                    if (GetCon(s, dir) != RC_NOT_CONNECTED)
                    {
                        var nc = chf.Cells[nx + ny * chf.Width];
                        ni = nc.Index + GetCon(s, dir);
                    }
                    if (ni == -1)
                    {
                        // Should not happen.
                        return;
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
        }
        private static bool MergeAndFilterRegions(int minRegionArea, int mergeRegionSize, int maxRegionId, CompactHeightfield chf, int[] srcReg, out int[] overlaps, out int maxRegionIdResult)
        {
            int w = chf.Width;
            int h = chf.Height;

            int nreg = maxRegionId + 1;
            Region[] regions = new Region[nreg];

            // Construct regions
            for (int i = 0; i < nreg; ++i)
            {
                regions[i] = new Region(i);
            }

            // Find edge of a region and find connections around the contour.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        int r = srcReg[i];
                        if (r == 0 || r >= nreg)
                        {
                            continue;
                        }

                        var reg = regions[r];
                        reg.spanCount++;

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
                                reg.overlap = true;
                            }
                            AddUniqueFloorRegion(reg, floorId);
                        }

                        // Have found contour
                        if (reg.connections.Count > 0)
                            continue;

                        reg.areaType = chf.Areas[i];

                        // Check if this cell is next to a border.
                        int ndir = -1;
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (IsSolidEdge(chf, srcReg, x, y, i, dir))
                            {
                                ndir = dir;
                                break;
                            }
                        }

                        if (ndir != -1)
                        {
                            // The cell is at border.
                            // Walk around the contour to find all the neighbours.
                            WalkContour(x, y, i, ndir, chf, srcReg, reg.connections);
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
                if (reg.id == 0 || (reg.id & RC_BORDER_REG) != 0)
                {
                    continue;
                }
                if (reg.spanCount == 0)
                {
                    continue;
                }
                if (reg.visited)
                {
                    continue;
                }

                // Count the total size of all the connected regions.
                // Also keep track of the regions connects to a tile border.
                bool connectsToBorder = false;
                int spanCount = 0;
                stack.Clear();
                trace.Clear();

                reg.visited = true;
                stack.Add(i);

                while (stack.Count > 0)
                {
                    // Pop
                    int ri = stack.Pop();

                    var creg = regions[ri];

                    spanCount += creg.spanCount;
                    trace.Add(ri);

                    for (int j = 0; j < creg.connections.Count; ++j)
                    {
                        if ((creg.connections[j] & RC_BORDER_REG) != 0)
                        {
                            connectsToBorder = true;
                            continue;
                        }
                        var neireg = regions[creg.connections[j]];
                        if (neireg.visited)
                        {
                            continue;
                        }
                        if (neireg.id == 0 || (neireg.id & RC_BORDER_REG) != 0)
                        {
                            continue;
                        }
                        // Visit
                        stack.Add(neireg.id);
                        neireg.visited = true;
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
                        regions[trace[j]].spanCount = 0;
                        regions[trace[j]].id = 0;
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
                    if (reg.id == 0 || (reg.id & RC_BORDER_REG) != 0)
                    {
                        continue;
                    }
                    if (reg.overlap)
                    {
                        continue;
                    }
                    if (reg.spanCount == 0)
                    {
                        continue;
                    }

                    // Check to see if the region should be merged.
                    if (reg.spanCount > mergeRegionSize && IsRegionConnectedToBorder(reg))
                    {
                        continue;
                    }

                    // Small region with more than 1 connection.
                    // Or region which is not connected to a border at all.
                    // Find smallest neighbour region that connects to this one.
                    int smallest = int.MaxValue;
                    int mergeId = reg.id;
                    for (int j = 0; j < reg.connections.Count; ++j)
                    {
                        if ((reg.connections[j] & RC_BORDER_REG) != 0)
                        {
                            continue;
                        }

                        var mreg = regions[reg.connections[j]];
                        if (mreg.id == 0 || (mreg.id & RC_BORDER_REG) != 0 || mreg.overlap)
                        {
                            continue;
                        }

                        if (mreg.spanCount < smallest &&
                            CanMergeWithRegion(reg, mreg) &&
                            CanMergeWithRegion(mreg, reg))
                        {
                            smallest = mreg.spanCount;
                            mergeId = mreg.id;
                        }
                    }
                    // Found new id.
                    if (mergeId != reg.id)
                    {
                        int oldId = reg.id;
                        var target = regions[mergeId];

                        // Merge neighbours.
                        if (MergeRegions(target, reg))
                        {
                            // Fixup regions pointing to current region.
                            for (int j = 0; j < nreg; ++j)
                            {
                                if (regions[j].id == 0 || (regions[j].id & RC_BORDER_REG) != 0)
                                {
                                    continue;
                                }

                                // If another region was already merged into current region
                                // change the nid of the previous region too.
                                if (regions[j].id == oldId)
                                {
                                    regions[j].id = mergeId;
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
                regions[i].remap = false;
                if (regions[i].id == 0)
                {
                    // Skip nil regions.
                    continue;
                }
                if ((regions[i].id & RC_BORDER_REG) != 0)
                {
                    // Skip external regions.
                    continue;
                }
                regions[i].remap = true;
            }

            int regIdGen = 0;
            for (int i = 0; i < nreg; ++i)
            {
                if (!regions[i].remap)
                {
                    continue;
                }
                int oldId = regions[i].id;
                int newId = ++regIdGen;
                for (int j = i; j < nreg; ++j)
                {
                    if (regions[j].id == oldId)
                    {
                        regions[j].id = newId;
                        regions[j].remap = false;
                    }
                }
            }
            maxRegionIdResult = regIdGen;

            // Remap regions.
            for (int i = 0; i < chf.SpanCount; ++i)
            {
                if ((srcReg[i] & RC_BORDER_REG) == 0)
                {
                    srcReg[i] = regions[srcReg[i]].id;
                }
            }

            // Return regions that we found to be overlapping.
            List<int> lOverlaps = new List<int>();
            for (int i = 0; i < nreg; ++i)
            {
                if (regions[i].overlap)
                {
                    lOverlaps.Add(regions[i].id);
                }
            }
            overlaps = lOverlaps.ToArray();

            for (int i = 0; i < nreg; ++i)
            {
                regions[i] = null;
            }

            return true;
        }
        private static void AddUniqueConnection(Region reg, int n)
        {
            for (int i = 0; i < reg.connections.Count; ++i)
            {
                if (reg.connections[i] == n)
                {
                    return;
                }
            }

            reg.connections.Add(n);
        }
        private static bool MergeAndFilterLayerRegions(int minRegionArea, int maxRegionId, CompactHeightfield chf, int[] srcReg, out int[] overlaps, out int maxRegionIdResult)
        {
            overlaps = null;

            int w = chf.Width;
            int h = chf.Height;

            int nreg = maxRegionId + 1;
            Region[] regions = new Region[nreg];

            // Construct regions
            for (int i = 0; i < nreg; ++i)
            {
                regions[i] = new Region(i);
            }

            // Find region neighbours and overlapping regions.
            List<int> lregs = new List<int>(32);
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.Cells[x + y * w];

                    lregs.Clear();

                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = chf.Spans[i];
                        int ri = srcReg[i];
                        if (ri == 0 || ri >= nreg)
                        {
                            continue;
                        }
                        var reg = regions[ri];

                        reg.spanCount++;

                        reg.ymin = Math.Min(reg.ymin, s.Y);
                        reg.ymax = Math.Max(reg.ymax, s.Y);

                        // Collect all region layers.
                        lregs.Add(ri);

                        // Update neighbours
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (GetCon(s, dir) != RC_NOT_CONNECTED)
                            {
                                int ax = x + GetDirOffsetX(dir);
                                int ay = y + GetDirOffsetY(dir);
                                int ai = chf.Cells[ax + ay * w].Index + GetCon(s, dir);
                                int rai = srcReg[ai];
                                if (rai > 0 && rai < nreg && rai != ri)
                                {
                                    AddUniqueConnection(reg, rai);
                                }
                                if ((rai & RC_BORDER_REG) != 0)
                                {
                                    reg.connectsToBorder = true;
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
                regions[i].id = 0;
            }

            // Merge montone regions to create non-overlapping areas.
            List<int> stack = new List<int>(32);
            for (int i = 1; i < nreg; ++i)
            {
                var root = regions[i];
                // Skip already visited.
                if (root.id != 0)
                {
                    continue;
                }

                // Start search.
                root.id = layerId;

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

                    int ncons = reg.connections.Count;
                    for (int j = 0; j < ncons; ++j)
                    {
                        int nei = reg.connections[j];
                        var regn = regions[nei];
                        // Skip already visited.
                        if (regn.id != 0)
                        {
                            continue;
                        }
                        // Skip if the neighbour is overlapping root region.
                        bool overlap = false;
                        for (int k = 0; k < root.floors.Count; k++)
                        {
                            if (root.floors[k] == nei)
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
                        regn.id = layerId;
                        // Merge current layers to root.
                        for (int k = 0; k < regn.floors.Count; ++k)
                        {
                            AddUniqueFloorRegion(root, regn.floors[k]);
                        }
                        root.ymin = Math.Min(root.ymin, regn.ymin);
                        root.ymax = Math.Max(root.ymax, regn.ymax);
                        root.spanCount += regn.spanCount;
                        regn.spanCount = 0;
                        root.connectsToBorder = root.connectsToBorder || regn.connectsToBorder;
                    }
                }

                layerId++;
            }

            // Remove small regions
            for (int i = 0; i < nreg; ++i)
            {
                if (regions[i].spanCount > 0 && regions[i].spanCount < minRegionArea && !regions[i].connectsToBorder)
                {
                    int reg = regions[i].id;
                    for (int j = 0; j < nreg; ++j)
                    {
                        if (regions[j].id == reg)
                        {
                            regions[j].id = 0;
                        }
                    }
                }
            }

            // Compress region Ids.
            for (int i = 0; i < nreg; ++i)
            {
                regions[i].remap = false;
                if (regions[i].id == 0)
                {
                    // Skip nil regions.
                    continue;
                }
                if ((regions[i].id & RC_BORDER_REG) != 0)
                {
                    // Skip external regions.
                    continue;
                }
                regions[i].remap = true;
            }

            int regIdGen = 0;
            for (int i = 0; i < nreg; ++i)
            {
                if (!regions[i].remap)
                {
                    continue;
                }
                int oldId = regions[i].id;
                int newId = ++regIdGen;
                for (int j = i; j < nreg; ++j)
                {
                    if (regions[j].id == oldId)
                    {
                        regions[j].id = newId;
                        regions[j].remap = false;
                    }
                }
            }
            maxRegionIdResult = regIdGen;

            // Remap regions.
            for (int i = 0; i < chf.SpanCount; ++i)
            {
                if ((srcReg[i] & RC_BORDER_REG) == 0)
                {
                    srcReg[i] = regions[srcReg[i]].id;
                }
            }

            for (int i = 0; i < nreg; ++i)
            {
                regions[i] = null;
            }

            return true;
        }
        public static bool BuildDistanceField(CompactHeightfield chf)
        {
            chf.Dist = null;

            int[] src = new int[chf.SpanCount];

            CalculateDistanceField(chf, src, out var maxDistance);
            chf.MaxDistance = maxDistance;

            int[] dst = new int[chf.SpanCount];

            // Blur
            if (BoxBlur(chf, 1, src, dst) != src)
            {
                Helper.Swap(ref src, ref dst);
            }

            // Store distance.
            chf.Dist = src;

            return true;
        }
        private static void PaintRectRegion(int minx, int maxx, int miny, int maxy, int regId, CompactHeightfield chf, int[] srcReg)
        {
            int w = chf.Width;
            for (int y = miny; y < maxy; ++y)
            {
                for (int x = minx; x < maxx; ++x)
                {
                    var c = chf.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        if (chf.Areas[i] != TileCacheAreas.RC_NULL_AREA)
                        {
                            srcReg[i] = regId;
                        }
                    }
                }
            }
        }
        public static bool BuildRegionsMonotone(CompactHeightfield chf, int borderSize, int minRegionArea, int mergeRegionArea)
        {
            int w = chf.Width;
            int h = chf.Height;
            int id = 1;

            int[] srcReg = new int[chf.SpanCount];

            int nsweeps = Math.Max(chf.Width, chf.Height);
            SweepSpan[] sweeps = new SweepSpan[nsweeps];

            // Mark border regions.
            if (borderSize > 0)
            {
                // Make sure border will not overflow.
                int bw = Math.Min(w, borderSize);
                int bh = Math.Min(h, borderSize);
                // Paint regions
                PaintRectRegion(0, bw, 0, h, id | RC_BORDER_REG, chf, srcReg); id++;
                PaintRectRegion(w - bw, w, 0, h, id | RC_BORDER_REG, chf, srcReg); id++;
                PaintRectRegion(0, w, 0, bh, id | RC_BORDER_REG, chf, srcReg); id++;
                PaintRectRegion(0, w, h - bh, h, id | RC_BORDER_REG, chf, srcReg); id++;

                chf.BorderSize = borderSize;
            }

            // Sweep one line at a time.
            for (int y = borderSize; y < h - borderSize; ++y)
            {
                // Collect spans from this row.
                int[] prev = new int[id + 1];
                int rid = 1;

                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    var c = chf.Cells[x + y * w];

                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = chf.Spans[i];
                        if (chf.Areas[i] == TileCacheAreas.RC_NULL_AREA)
                        {
                            continue;
                        }

                        // -x
                        int previd = 0;
                        if (GetCon(s, 0) != RC_NOT_CONNECTED)
                        {
                            int ax = x + GetDirOffsetX(0);
                            int ay = y + GetDirOffsetY(0);
                            int ai = chf.Cells[ax + ay * w].Index + GetCon(s, 0);
                            if ((srcReg[ai] & RC_BORDER_REG) == 0 && chf.Areas[i] == chf.Areas[ai])
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
                        if (GetCon(s, 3) != RC_NOT_CONNECTED)
                        {
                            int ax = x + GetDirOffsetX(3);
                            int ay = y + GetDirOffsetY(3);
                            int ai = chf.Cells[ax + ay * w].Index + GetCon(s, 3);
                            if (srcReg[ai] != 0 && (srcReg[ai] & RC_BORDER_REG) == 0 && chf.Areas[i] == chf.Areas[ai])
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
                    var c = chf.Cells[x + y * w];

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
            chf.MaxRegions = id;
            var merged = MergeAndFilterRegions(minRegionArea, mergeRegionArea, id, chf, srcReg, out int[] overlaps, out var maxRegionId);
            chf.MaxRegions = maxRegionId;

            if (!merged)
            {
                return false;
            }

            // Monotone partitioning does not generate overlapping regions.

            // Store the result out.
            for (int i = 0; i < chf.SpanCount; ++i)
            {
                chf.Spans[i].Reg = srcReg[i];
            }

            return true;
        }
        public static bool BuildRegions(CompactHeightfield chf, int borderSize, int minRegionArea, int mergeRegionArea)
        {
            int w = chf.Width;
            int h = chf.Height;

            int LOG_NB_STACKS = 3;
            int NB_STACKS = 1 << LOG_NB_STACKS;
            List<List<int>> lvlStacks = new List<List<int>>();
            for (int i = 0; i < NB_STACKS; ++i)
            {
                lvlStacks.Add(new List<int>());
            }

            List<int> stack = new List<int>();

            int[] srcReg = new int[chf.SpanCount];
            int[] srcDist = new int[chf.SpanCount];
            int[] dstReg = new int[chf.SpanCount];
            int[] dstDist = new int[chf.SpanCount];

            int regionId = 1;
            int level = (chf.MaxDistance + 1) & ~1;

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
                PaintRectRegion(0, bw, 0, h, (regionId | RC_BORDER_REG), chf, srcReg); regionId++;
                PaintRectRegion(w - bw, w, 0, h, (regionId | RC_BORDER_REG), chf, srcReg); regionId++;
                PaintRectRegion(0, w, 0, bh, (regionId | RC_BORDER_REG), chf, srcReg); regionId++;
                PaintRectRegion(0, w, h - bh, h, (regionId | RC_BORDER_REG), chf, srcReg); regionId++;

                chf.BorderSize = borderSize;
            }

            int sId = -1;
            while (level > 0)
            {
                level = level >= 2 ? level - 2 : 0;
                sId = (sId + 1) & (NB_STACKS - 1);

                if (sId == 0)
                {
                    SortCellsByLevel(level, chf, srcReg, NB_STACKS, lvlStacks, 1);
                }
                else
                {
                    AppendStacks(lvlStacks[sId - 1], lvlStacks[sId], srcReg); // copy left overs from last level
                }

                // Expand current regions until no empty connected cells found.
                if (ExpandRegions(expandIters, level, chf, srcReg, srcDist, dstReg, dstDist, lvlStacks[sId], false) != srcReg)
                {
                    Helper.Swap(ref srcReg, ref dstReg);
                    Helper.Swap(ref srcDist, ref dstDist);
                }

                // Mark new regions with IDs.
                for (int j = 0; j < lvlStacks[sId].Count; j += 3)
                {
                    int x = lvlStacks[sId][j];
                    int y = lvlStacks[sId][j + 1];
                    int i = lvlStacks[sId][j + 2];
                    if (i >= 0 && srcReg[i] == 0)
                    {
                        var floodRes = FloodRegion(x, y, i, level, regionId, chf, srcReg, srcDist, stack);
                        if (floodRes)
                        {
                            if (regionId == 0xFFFF)
                            {
                                throw new EngineException("rcBuildRegions: Region ID overflow");
                            }

                            regionId++;
                        }
                    }
                }
            }

            // Expand current regions until no empty connected cells found.
            if (ExpandRegions(expandIters * 8, 0, chf, srcReg, srcDist, dstReg, dstDist, stack, true) != srcReg)
            {
                Helper.Swap(ref srcReg, ref dstReg);
                Helper.Swap(ref srcDist, ref dstDist);
            }

            // Merge regions and filter out smalle regions.
            chf.MaxRegions = regionId;
            var merged = MergeAndFilterRegions(minRegionArea, mergeRegionArea, regionId, chf, srcReg, out int[] overlaps, out int maxRegionId);
            chf.MaxRegions = maxRegionId;
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
            for (int i = 0; i < chf.SpanCount; ++i)
            {
                chf.Spans[i].Reg = srcReg[i];
            }

            return true;
        }
        public static bool BuildLayerRegions(CompactHeightfield chf, int borderSize, int minRegionArea)
        {
            int w = chf.Width;
            int h = chf.Height;
            int id = 1;

            int[] srcReg = new int[chf.SpanCount];

            int nsweeps = Math.Max(chf.Width, chf.Height);
            SweepSpan[] sweeps = Helper.CreateArray(nsweeps, new SweepSpan());

            // Mark border regions.
            if (borderSize > 0)
            {
                // Make sure border will not overflow.
                int bw = Math.Min(w, borderSize);
                int bh = Math.Min(h, borderSize);
                // Paint regions
                PaintRectRegion(0, bw, 0, h, id | RC_BORDER_REG, chf, srcReg); id++;
                PaintRectRegion(w - bw, w, 0, h, id | RC_BORDER_REG, chf, srcReg); id++;
                PaintRectRegion(0, w, 0, bh, id | RC_BORDER_REG, chf, srcReg); id++;
                PaintRectRegion(0, w, h - bh, h, id | RC_BORDER_REG, chf, srcReg); id++;

                chf.BorderSize = borderSize;
            }

            // Sweep one line at a time.
            for (int y = borderSize; y < h - borderSize; ++y)
            {
                // Collect spans from this row.
                int[] prev = new int[256];
                int rid = 1;

                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    var c = chf.Cells[x + y * w];

                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        var s = chf.Spans[i];
                        if (chf.Areas[i] == TileCacheAreas.RC_NULL_AREA)
                        {
                            continue;
                        }

                        // -x
                        int previd = 0;
                        if (GetCon(s, 0) != RC_NOT_CONNECTED)
                        {
                            int ax = x + GetDirOffsetX(0);
                            int ay = y + GetDirOffsetY(0);
                            int ai = chf.Cells[ax + ay * w].Index + GetCon(s, 0);
                            if ((srcReg[ai] & RC_BORDER_REG) == 0 && chf.Areas[i] == chf.Areas[ai])
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
                        if (GetCon(s, 3) != RC_NOT_CONNECTED)
                        {
                            int ax = x + GetDirOffsetX(3);
                            int ay = y + GetDirOffsetY(3);
                            int ai = chf.Cells[ax + ay * w].Index + GetCon(s, 3);
                            if (srcReg[ai] != 0 && (srcReg[ai] & RC_BORDER_REG) == 0 && chf.Areas[i] == chf.Areas[ai])
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
                    var c = chf.Cells[x + y * w];

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
            chf.MaxRegions = id;
            var merged = MergeAndFilterLayerRegions(minRegionArea, id, chf, srcReg, out int[] overlaps, out int maxRegionId);
            chf.MaxRegions = maxRegionId;
            if (!merged)
            {
                return false;
            }

            // Store the result out.
            for (int i = 0; i < chf.SpanCount; ++i)
            {
                chf.Spans[i].Reg = srcReg[i];
            }

            return true;
        }

        #endregion

        #region RECASTFILTER

        public static void FilterLowHangingWalkableObstacles(int walkableClimb, Heightfield solid)
        {
            int w = solid.width;
            int h = solid.height;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool previousWalkable = false;
                    TileCacheAreas previousArea = TileCacheAreas.RC_NULL_AREA;

                    Span ps = null;

                    for (Span s = solid.spans[x + y * w]; s != null; ps = s, s = s.next)
                    {
                        bool walkable = s.area != TileCacheAreas.RC_NULL_AREA;

                        // If current span is not walkable, but there is walkable span just below it, mark the span above it walkable too.
                        if (!walkable && previousWalkable && Math.Abs(s.smax - ps.smax) <= walkableClimb)
                        {
                            s.area = previousArea;
                        }

                        // Copy walkable flag so that it cannot propagate past multiple non-walkable objects.
                        previousWalkable = walkable;
                        previousArea = s.area;
                    }
                }
            }
        }
        public static void FilterLedgeSpans(int walkableHeight, int walkableClimb, Heightfield solid)
        {
            int w = solid.width;
            int h = solid.height;

            // Mark border spans.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    for (Span s = solid.spans[x + y * w]; s != null; s = s.next)
                    {
                        // Skip non walkable spans.
                        if (s.area == TileCacheAreas.RC_NULL_AREA)
                        {
                            continue;
                        }

                        int bot = s.smax;
                        int top = s.next != null ? s.next.smin : int.MaxValue;

                        // Find neighbours minimum height.
                        int minh = int.MaxValue;

                        // Min and max height of accessible neighbours.
                        int asmin = s.smax;
                        int asmax = s.smax;

                        for (int dir = 0; dir < 4; ++dir)
                        {
                            // Skip neighbours which are out of bounds.
                            int dx = x + GetDirOffsetX(dir);
                            int dy = y + GetDirOffsetY(dir);
                            if (dx < 0 || dy < 0 || dx >= w || dy >= h)
                            {
                                minh = Math.Min(minh, -walkableClimb - bot);
                                continue;
                            }

                            // From minus infinity to the first span.
                            var ns = solid.spans[dx + dy * w];
                            int nbot = -walkableClimb;
                            int ntop = ns != null ? ns.smin : int.MaxValue;

                            // Skip neightbour if the gap between the spans is too small.
                            if (Math.Min(top, ntop) - Math.Max(bot, nbot) > walkableHeight)
                            {
                                minh = Math.Min(minh, nbot - bot);
                            }

                            // Rest of the spans.
                            ns = solid.spans[dx + dy * w];
                            while (ns != null)
                            {
                                nbot = ns.smax;
                                ntop = ns.next != null ? ns.next.smin : int.MaxValue;

                                // Skip neightbour if the gap between the spans is too small.
                                if (Math.Min(top, ntop) - Math.Max(bot, nbot) > walkableHeight)
                                {
                                    minh = Math.Min(minh, nbot - bot);

                                    // Find min/max accessible neighbour height. 
                                    if (Math.Abs(nbot - bot) <= walkableClimb)
                                    {
                                        if (nbot < asmin) asmin = nbot;
                                        if (nbot > asmax) asmax = nbot;
                                    }

                                }

                                ns = ns.next;
                            }
                        }

                        if (minh < -walkableClimb)
                        {
                            // The current span is close to a ledge if the drop to any neighbour span is less than the walkableClimb.
                            s.area = TileCacheAreas.RC_NULL_AREA;
                        }
                        else if ((asmax - asmin) > walkableClimb)
                        {
                            // If the difference between all neighbours is too large, we are at steep slope, mark the span as ledge.
                            s.area = TileCacheAreas.RC_NULL_AREA;
                        }
                    }
                }
            }
        }
        public static void FilterWalkableLowHeightSpans(int walkableHeight, Heightfield solid)
        {
            int w = solid.width;
            int h = solid.height;

            // Remove walkable flag from spans which do not have enough space above them for the agent to stand there.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    for (var s = solid.spans[x + y * w]; s != null; s = s.next)
                    {
                        int bot = s.smax;
                        int top = s.next != null ? s.next.smin : int.MaxValue;

                        if ((top - bot) <= walkableHeight)
                        {
                            s.area = TileCacheAreas.RC_NULL_AREA;
                        }
                    }
                }
            }
        }

        #endregion

        #region RECASTRASTERIZATION

        public static bool OverlapBounds(Vector3 amin, Vector3 amax, Vector3 bmin, Vector3 bmax)
        {
            return
                !(amin.X > bmax.X || amax.X < bmin.X) &&
                !(amin.Y > bmax.Y || amax.Y < bmin.Y) &&
                !(amin.Z > bmax.Z || amax.Z < bmin.Z);
        }
        public static bool OverlapInterval(int amin, int amax, int bmin, int bmax)
        {
            if (amax < bmin) return false;
            if (amin > bmax) return false;
            return true;
        }
        public static Span AllocSpan(Heightfield hf)
        {
            // If running out of memory, allocate new page and update the freelist.
            if (hf.freelist == null || hf.freelist.next == null)
            {
                // Create new page.
                // Allocate memory for the new pool.
                SpanPool pool = new SpanPool
                {
                    // Add the pool into the list of pools.
                    next = hf.pools.Count > 0 ? hf.pools.Last() : null
                };
                hf.pools.Add(pool);
                // Add new items to the free list.
                Span freelist = hf.freelist;
                int itIndex = RC_SPANS_PER_POOL;
                do
                {
                    var it = pool.items[--itIndex];
                    it.next = freelist;
                    freelist = it;
                }
                while (itIndex > 0);
                hf.freelist = pool.items[itIndex];
            }

            // Pop item from in front of the free list.
            Span s = hf.freelist;
            hf.freelist = hf.freelist.next;
            return s;
        }
        public static void FreeSpan(Heightfield hf, Span cur)
        {
            if (cur == null) return;

            // Add the node in front of the free list.
            cur.next = hf.freelist;
            hf.freelist = cur;
        }
        public static bool AddSpan(Heightfield hf, int x, int y, int smin, int smax, TileCacheAreas area, int flagMergeThr)
        {
            int idx = x + y * hf.width;

            Span s = AllocSpan(hf);
            s.smin = smin;
            s.smax = smax;
            s.area = area;
            s.next = null;

            // Empty cell, add the first span.
            if (hf.spans[idx] == null)
            {
                hf.spans[idx] = s;
                return true;
            }
            Span prev = null;
            Span cur = hf.spans[idx];

            // Insert and merge spans.
            while (cur != null)
            {
                if (cur.smin > s.smax)
                {
                    // Current span is further than the new span, break.
                    break;
                }
                else if (cur.smax < s.smin)
                {
                    // Current span is before the new span advance.
                    prev = cur;
                    cur = cur.next;
                }
                else
                {
                    // Merge spans.
                    if (cur.smin < s.smin)
                    {
                        s.smin = cur.smin;
                    }
                    if (cur.smax > s.smax)
                    {
                        s.smax = cur.smax;
                    }

                    // Merge flags.
                    if (Math.Abs(s.smax - cur.smax) <= flagMergeThr)
                    {
                        s.area = (TileCacheAreas)Math.Max((int)s.area, (int)cur.area);
                    }

                    // Remove current span.
                    Span next = cur.next;
                    FreeSpan(hf, cur);
                    if (prev != null)
                    {
                        prev.next = next;
                    }
                    else
                    {
                        hf.spans[idx] = next;
                    }

                    cur = next;
                }
            }

            // Insert new span.
            if (prev != null)
            {
                s.next = prev.next;
                prev.next = s;
            }
            else
            {
                s.next = hf.spans[idx];
                hf.spans[idx] = s;
            }

            return true;
        }
        public static void DividePoly(List<Vector3> inPoly, List<Vector3> outPoly1, List<Vector3> outPoly2, float x, int axis)
        {
            float[] d = new float[inPoly.Count];
            for (int i = 0; i < inPoly.Count; i++)
            {
                d[i] = x - inPoly[i][axis];
            }

            for (int i = 0, j = inPoly.Count - 1; i < inPoly.Count; j = i, i++)
            {
                bool ina = d[j] >= 0;
                bool inb = d[i] >= 0;
                if (ina != inb)
                {
                    float s = d[j] / (d[j] - d[i]);
                    Vector3 v;
                    v.X = inPoly[j].X + (inPoly[i].X - inPoly[j].X) * s;
                    v.Y = inPoly[j].Y + (inPoly[i].Y - inPoly[j].Y) * s;
                    v.Z = inPoly[j].Z + (inPoly[i].Z - inPoly[j].Z) * s;
                    outPoly1.Add(v);
                    outPoly2.Add(v);

                    // add the i'th point to the right polygon. Do NOT add points that are on the dividing line
                    // since these were already added above
                    if (d[i] > 0)
                    {
                        outPoly1.Add(inPoly[i]);
                    }
                    else if (d[i] < 0)
                    {
                        outPoly2.Add(inPoly[i]);
                    }
                }
                else // same side
                {
                    // add the i'th point to the right polygon. Addition is done even for points on the dividing line
                    if (d[i] >= 0)
                    {
                        outPoly1.Add(inPoly[i]);

                        if (d[i] != 0)
                        {
                            continue;
                        }
                    }

                    outPoly2.Add(inPoly[i]);
                }
            }
        }
        private static bool RasterizeTri(Triangle tri, TileCacheAreas area, Heightfield hf, BoundingBox b, float cs, float ics, float ich, int flagMergeThr)
        {
            int w = hf.width;
            int h = hf.height;
            float by = b.GetY();

            // Calculate the bounding box of the triangle.
            var t = BoundingBox.FromPoints(tri.GetVertices());

            // If the triangle does not touch the bbox of the heightfield, skip the triagle.
            if (b.Contains(t) == ContainmentType.Disjoint)
            {
                return true;
            }

            // Calculate the footprint of the triangle on the grid's y-axis
            int y0 = (int)((t.Minimum.Z - b.Minimum.Z) * ics);
            int y1 = (int)((t.Maximum.Z - b.Minimum.Z) * ics);
            y0 = MathUtil.Clamp(y0, 0, h - 1);
            y1 = MathUtil.Clamp(y1, 0, h - 1);

            // Clip the triangle into all grid cells it touches.
            List<Vector3> inb = new List<Vector3>(tri.GetVertices());
            List<Vector3> zp1 = new List<Vector3>();
            List<Vector3> zp2 = new List<Vector3>();
            List<Vector3> xp1 = new List<Vector3>();
            List<Vector3> xp2 = new List<Vector3>();

            for (int y = y0; y <= y1; ++y)
            {
                // Clip polygon to row. Store the remaining polygon as well
                zp1.Clear();
                zp2.Clear();
                float cz = b.Minimum.Z + y * cs;
                DividePoly(inb, zp1, zp2, cz + cs, 2);
                Helper.Swap(ref inb, ref zp2);
                if (zp1.Count < 3) continue;

                // find the horizontal bounds in the row
                float minX = zp1[0].X;
                float maxX = zp1[0].X;
                for (int i = 1; i < zp1.Count; i++)
                {
                    minX = Math.Min(minX, zp1[i].X);
                    maxX = Math.Max(maxX, zp1[i].X);
                }
                int x0 = (int)((minX - b.Minimum.X) * ics);
                int x1 = (int)((maxX - b.Minimum.X) * ics);
                x0 = MathUtil.Clamp(x0, 0, w - 1);
                x1 = MathUtil.Clamp(x1, 0, w - 1);

                for (int x = x0; x <= x1; ++x)
                {
                    // Clip polygon to column. store the remaining polygon as well
                    xp1.Clear();
                    xp2.Clear();
                    float cx = b.Minimum.X + x * cs;
                    DividePoly(zp1, xp1, xp2, cx + cs, 0);
                    Helper.Swap(ref zp1, ref xp2);
                    if (xp1.Count < 3) continue;

                    // Calculate min and max of the span.
                    float minY = xp1[0].Y;
                    float maxY = xp1[0].Y;
                    for (int i = 1; i < xp1.Count; ++i)
                    {
                        minY = Math.Min(minY, xp1[i].Y);
                        maxY = Math.Max(maxY, xp1[i].Y);
                    }
                    minY -= b.Minimum.Y;
                    maxY -= b.Minimum.Y;
                    // Skip the span if it is outside the heightfield bbox
                    if (maxY < 0.0f) continue;
                    if (minY > by) continue;
                    // Clamp the span to the heightfield bbox.
                    if (minY < 0.0f) minY = 0;
                    if (maxY > by) maxY = by;

                    // Snap the span to the heightfield height grid.
                    int ismin = MathUtil.Clamp((int)Math.Floor(minY * ich), 0, Span.SpanMaxHeight);
                    int ismax = MathUtil.Clamp((int)Math.Ceiling(maxY * ich), ismin + 1, Span.SpanMaxHeight);

                    if (!AddSpan(hf, x, y, ismin, ismax, area, flagMergeThr))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        public static bool RasterizeTriangle(Triangle tri, TileCacheAreas area, Heightfield solid, int flagMergeThr)
        {
            float ics = 1.0f / solid.cs;
            float ich = 1.0f / solid.ch;

            if (!RasterizeTri(tri, area, solid, solid.boundingBox, solid.cs, ics, ich, flagMergeThr))
            {
                return false;
            }

            return true;
        }
        public static bool RasterizeTriangles(Heightfield solid, int flagMergeThr, Triangle[] tris, TileCacheAreas[] areas)
        {
            float ics = 1.0f / solid.cs;
            float ich = 1.0f / solid.ch;

            // Rasterize triangles.
            for (int i = 0; i < tris.Length; ++i)
            {
                // Rasterize.
                if (!RasterizeTri(tris[i], areas[i], solid, solid.boundingBox, solid.cs, ics, ich, flagMergeThr))
                {
                    throw new EngineException("rcRasterizeTriangles: Out of memory.");
                }
            }

            return true;
        }

        #endregion

        #region RECASTCONTOUR

        public static int GetCornerHeight(int x, int y, int i, int dir, CompactHeightfield chf, out bool isBorderVertex)
        {
            isBorderVertex = false;

            var s = chf.Spans[i];
            int ch = s.Y;
            int dirp = (dir + 1) & 0x3;

            int[] regs = { 0, 0, 0, 0 };

            // Combine region and area codes in order to prevent
            // border vertices which are in between two areas to be removed.
            regs[0] = chf.Spans[i].Reg | ((int)chf.Areas[i] << 16);

            if (GetCon(s, dir) != RC_NOT_CONNECTED)
            {
                int ax = x + GetDirOffsetX(dir);
                int ay = y + GetDirOffsetY(dir);
                int ai = chf.Cells[ax + ay * chf.Width].Index + GetCon(s, dir);
                var a = chf.Spans[ai];
                ch = Math.Max(ch, a.Y);
                regs[1] = chf.Spans[ai].Reg | ((int)chf.Areas[ai] << 16);
                if (GetCon(a, dirp) != RC_NOT_CONNECTED)
                {
                    int ax2 = ax + GetDirOffsetX(dirp);
                    int ay2 = ay + GetDirOffsetY(dirp);
                    int ai2 = chf.Cells[ax2 + ay2 * chf.Width].Index + GetCon(a, dirp);
                    var as2 = chf.Spans[ai2];
                    ch = Math.Max(ch, as2.Y);
                    regs[2] = chf.Spans[ai2].Reg | ((int)chf.Areas[ai2] << 16);
                }
            }
            if (GetCon(s, dirp) != RC_NOT_CONNECTED)
            {
                int ax = x + GetDirOffsetX(dirp);
                int ay = y + GetDirOffsetY(dirp);
                int ai = chf.Cells[ax + ay * chf.Width].Index + GetCon(s, dirp);
                var a = chf.Spans[ai];
                ch = Math.Max(ch, a.Y);
                regs[3] = chf.Spans[ai].Reg | ((int)chf.Areas[ai] << 16);
                if (GetCon(a, dir) != RC_NOT_CONNECTED)
                {
                    int ax2 = ax + GetDirOffsetX(dir);
                    int ay2 = ay + GetDirOffsetY(dir);
                    int ai2 = chf.Cells[ax2 + ay2 * chf.Width].Index + GetCon(a, dir);
                    var as2 = chf.Spans[ai2];
                    ch = Math.Max(ch, as2.Y);
                    regs[2] = chf.Spans[ai2].Reg | ((int)chf.Areas[ai2] << 16);
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
        public static void WalkContour(int x, int y, int i, int dir, CompactHeightfield chf, int[] srcReg, List<int> cont)
        {
            int startDir = dir;
            int starti = i;

            var ss = chf.Spans[i];
            int curReg = 0;
            if (GetCon(ss, dir) != RC_NOT_CONNECTED)
            {
                int ax = x + GetDirOffsetX(dir);
                int ay = y + GetDirOffsetY(dir);
                int ai = chf.Cells[ax + ay * chf.Width].Index + GetCon(ss, dir);
                curReg = srcReg[ai];
            }
            cont.Add(curReg);

            int iter = 0;
            while (++iter < 40000)
            {
                var s = chf.Spans[i];

                if (IsSolidEdge(chf, srcReg, x, y, i, dir))
                {
                    // Choose the edge corner
                    int r = 0;
                    if (GetCon(s, dir) != RC_NOT_CONNECTED)
                    {
                        int ax = x + GetDirOffsetX(dir);
                        int ay = y + GetDirOffsetY(dir);
                        int ai = chf.Cells[ax + ay * chf.Width].Index + GetCon(s, dir);
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
                    int nx = x + GetDirOffsetX(dir);
                    int ny = y + GetDirOffsetY(dir);
                    if (GetCon(s, dir) != RC_NOT_CONNECTED)
                    {
                        var nc = chf.Cells[nx + ny * chf.Width];
                        ni = nc.Index + GetCon(s, dir);
                    }
                    if (ni == -1)
                    {
                        // Should not happen.
                        return;
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
        public static float DistancePtSeg(int x, int z, int px, int pz, int qx, int qz)
        {
            float pqx = (qx - px);
            float pqz = (qz - pz);
            float dx = (x - px);
            float dz = (z - pz);
            float d = pqx * pqx + pqz * pqz;
            float t = pqx * dx + pqz * dz;
            if (d > 0)
            {
                t /= d;
            }
            if (t < 0)
            {
                t = 0;
            }
            else if (t > 1)
            {
                t = 1;
            }

            dx = px + t * pqx - x;
            dz = pz + t * pqz - z;

            return dx * dx + dz * dz;
        }
        private static void SimplifyContour(List<Int4> points, List<Int4> simplified, float maxError, int maxEdgeLen, BuildContoursFlagTypes buildFlags)
        {
            // Add initial points.
            bool hasConnections = false;
            for (int i = 0; i < points.Count; i++)
            {
                if ((points[i].W & RC_CONTOUR_REG_MASK) != 0)
                {
                    hasConnections = true;
                    break;
                }
            }

            if (hasConnections)
            {
                // The contour has some portals to other regions.
                // Add a new point to every location where the region changes.
                for (int i = 0, ni = points.Count; i < ni; ++i)
                {
                    int ii = (i + 1) % ni;
                    bool differentRegs = (points[i].W & RC_CONTOUR_REG_MASK) != (points[ii].W & RC_CONTOUR_REG_MASK);
                    bool areaBorders = (points[i].W & RC_AREA_BORDER) != (points[ii].W & RC_AREA_BORDER);
                    if (differentRegs || areaBorders)
                    {
                        simplified.Add(new Int4(points[i].X, points[i].Y, points[i].Z, i));
                    }
                }
            }

            if (simplified.Count == 0)
            {
                // If there is no connections at all,
                // create some initial points for the simplification process.
                // Find lower-left and upper-right vertices of the contour.
                int llx = points[0].X;
                int lly = points[0].Y;
                int llz = points[0].Z;
                int lli = 0;
                int urx = points[0].X;
                int ury = points[0].Y;
                int urz = points[0].Z;
                int uri = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    int x = points[i].X;
                    int y = points[i].Y;
                    int z = points[i].Z;
                    if (x < llx || (x == llx && z < llz))
                    {
                        llx = x;
                        lly = y;
                        llz = z;
                        lli = i;
                    }
                    if (x > urx || (x == urx && z > urz))
                    {
                        urx = x;
                        ury = y;
                        urz = z;
                        uri = i;
                    }
                }
                simplified.Add(new Int4(llx, lly, llz, lli));
                simplified.Add(new Int4(urx, ury, urz, uri));
            }

            // Add points until all raw points are within
            // error tolerance to the simplified shape.
            int pn = points.Count;
            for (int i = 0; i < simplified.Count;)
            {
                int ii = (i + 1) % (simplified.Count);

                int ax = simplified[i].X;
                int az = simplified[i].Z;
                int ai = simplified[i].W;

                int bx = simplified[ii].X;
                int bz = simplified[ii].Z;
                int bi = simplified[ii].W;

                // Find maximum deviation from the segment.
                float maxd = 0;
                int maxi = -1;
                int ci, cinc, endi;

                // Traverse the segment in lexilogical order so that the
                // max deviation is calculated similarly when traversing
                // opposite segments.
                if (bx > ax || (bx == ax && bz > az))
                {
                    cinc = 1;
                    ci = (ai + cinc) % pn;
                    endi = bi;
                }
                else
                {
                    cinc = pn - 1;
                    ci = (bi + cinc) % pn;
                    endi = ai;
                    Helper.Swap(ref ax, ref bx);
                    Helper.Swap(ref az, ref bz);
                }

                // Tessellate only outer edges or edges between areas.
                if ((points[ci].W & RC_CONTOUR_REG_MASK) == 0 ||
                    (points[ci].W & RC_AREA_BORDER) != 0)
                {
                    while (ci != endi)
                    {
                        float d = DistancePtSeg(points[ci].X, points[ci].Z, ax, az, bx, bz);
                        if (d > maxd)
                        {
                            maxd = d;
                            maxi = ci;
                        }
                        ci = (ci + cinc) % pn;
                    }
                }

                // If the max deviation is larger than accepted error,
                // add new point, else continue to next segment.
                if (maxi != -1 && maxd > (maxError * maxError))
                {
                    // Add the point.
                    simplified.Insert(i + 1, new Int4(points[maxi].X, points[maxi].Y, points[maxi].Z, maxi));
                }
                else
                {
                    ++i;
                }
            }

            // Split too long edges.
            if (maxEdgeLen > 0 && (buildFlags & (BuildContoursFlagTypes.RC_CONTOUR_TESS_WALL_EDGES | BuildContoursFlagTypes.RC_CONTOUR_TESS_AREA_EDGES)) != 0)
            {
                for (int i = 0; i < simplified.Count;)
                {
                    int ii = (i + 1) % (simplified.Count);

                    int ax = simplified[i].X;
                    int az = simplified[i].Z;
                    int ai = simplified[i].W;

                    int bx = simplified[ii].X;
                    int bz = simplified[ii].Z;
                    int bi = simplified[ii].W;

                    // Find maximum deviation from the segment.
                    int maxi = -1;
                    int ci = (ai + 1) % pn;

                    // Tessellate only outer edges or edges between areas.
                    bool tess = false;
                    // Wall edges.
                    if ((buildFlags & BuildContoursFlagTypes.RC_CONTOUR_TESS_WALL_EDGES) != 0 &&
                        (points[ci].W & RC_CONTOUR_REG_MASK) == 0)
                    {
                        tess = true;
                    }
                    // Edges between areas.
                    if ((buildFlags & BuildContoursFlagTypes.RC_CONTOUR_TESS_AREA_EDGES) != 0 &&
                        (points[ci].W & RC_AREA_BORDER) != 0)
                    {
                        tess = true;
                    }

                    if (tess)
                    {
                        int dx = bx - ax;
                        int dz = bz - az;
                        if (dx * dx + dz * dz > maxEdgeLen * maxEdgeLen)
                        {
                            // Round based on the segments in lexilogical order so that the
                            // max tesselation is consistent regardles in which direction
                            // segments are traversed.
                            int n = bi < ai ? (bi + pn - ai) : (bi - ai);
                            if (n > 1)
                            {
                                if (bx > ax || (bx == ax && bz > az))
                                {
                                    maxi = (ai + n / 2) % pn;
                                }
                                else
                                {
                                    maxi = (ai + (n + 1) / 2) % pn;
                                }
                            }
                        }
                    }

                    // If the max deviation is larger than accepted error,
                    // add new point, else continue to next segment.
                    if (maxi != -1)
                    {
                        // Add the point.
                        simplified.Insert(i + 1, new Int4(points[maxi].X, points[maxi].Y, points[maxi].Z, maxi));
                    }
                    else
                    {
                        ++i;
                    }
                }
            }

            for (int i = 0; i < simplified.Count; ++i)
            {
                // The edge vertex flag is take from the current raw point,
                // and the neighbour region is take from the next raw point.
                var sv = simplified[i];
                int ai = (sv.W + 1) % pn;
                int bi = sv.W;
                sv.W = (points[ai].W & (RC_CONTOUR_REG_MASK | RC_AREA_BORDER)) | (points[bi].W & RC_BORDER_VERTEX);
                simplified[i] = sv;
            }
        }
        private static int CalcAreaOfPolygon2D(Int4[] verts, int nverts)
        {
            int area = 0;
            for (int i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                var vi = verts[i];
                var vj = verts[j];
                area += vi.X * vj.Z - vj.X * vi.Z;
            }
            return (area + 1) / 2;
        }
        private static bool IntersectSegCountour(Int4 d0, Int4 d1, int i, int n, Int4[] verts)
        {
            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = Next(k, n);
                // Skip edges incident to i.
                if (i == k || i == k1)
                {
                    continue;
                }
                var p0 = verts[k];
                var p1 = verts[k1];
                if (d0 == p0 || d1 == p0 || d0 == p1 || d1 == p1)
                {
                    continue;
                }

                if (Intersect(d0, d1, p0, p1))
                {
                    return true;
                }
            }
            return false;
        }
        private static void RemoveDegenerateSegments(List<Int4> simplified)
        {
            // Remove adjacent vertices which are equal on xz-plane,
            // or else the triangulator will get confused.
            int npts = simplified.Count;
            for (int i = 0; i < npts; ++i)
            {
                int ni = Next(i, npts);

                if (simplified[i] == simplified[ni])
                {
                    // Degenerate segment, remove.
                    for (int j = i; j < simplified.Count - 1; ++j)
                    {
                        simplified[j] = simplified[(j + 1)];
                    }
                    simplified.Clear();
                    npts--;
                }
            }
        }
        private static bool MergeContours(Contour ca, Contour cb, int ia, int ib)
        {
            int maxVerts = ca.nverts + cb.nverts + 2;
            Int4[] verts = new Int4[maxVerts];

            int nv = 0;

            // Copy contour A.
            for (int i = 0; i <= ca.nverts; ++i)
            {
                verts[nv++] = ca.verts[((ia + i) % ca.nverts)];
            }

            // Copy contour B
            for (int i = 0; i <= cb.nverts; ++i)
            {
                verts[nv++] = cb.verts[((ib + i) % cb.nverts)];
            }

            ca.verts = verts;
            ca.nverts = nv;

            cb.verts = null;
            cb.nverts = 0;

            return true;
        }
        private static void FindLeftMostVertex(Contour contour, out int minx, out int minz, out int leftmost)
        {
            minx = contour.verts[0].X;
            minz = contour.verts[0].Z;
            leftmost = 0;
            for (int i = 1; i < contour.nverts; i++)
            {
                int x = contour.verts[i].X;
                int z = contour.verts[i].Z;
                if (x < minx || (x == minx && z < minz))
                {
                    minx = x;
                    minz = z;
                    leftmost = i;
                }
            }
        }
        private static int CompareHoles(ContourHole va, ContourHole vb)
        {
            ContourHole a = va;
            ContourHole b = vb;
            if (a.MinX == b.MinX)
            {
                if (a.MinZ < b.MinZ)
                {
                    return -1;
                }
                if (a.MinZ > b.MinZ)
                {
                    return 1;
                }
            }
            else
            {
                if (a.MinX < b.MinX)
                {
                    return -1;
                }
                if (a.MinX > b.MinX)
                {
                    return 1;
                }
            }
            return 0;
        }
        private static int CompareDiagDist(PotentialDiagonal va, PotentialDiagonal vb)
        {
            PotentialDiagonal a = va;
            PotentialDiagonal b = vb;
            if (a.Dist < b.Dist)
            {
                return -1;
            }
            if (a.Dist > b.Dist)
            {
                return 1;
            }
            return 0;
        }
        private static void MergeRegionHoles(ContourRegion region)
        {
            // Sort holes from left to right.
            for (int i = 0; i < region.nholes; i++)
            {
                FindLeftMostVertex(region.holes[i].Contour, out var minx, out var minz, out var leftmost);
                region.holes[i].MinX = minx;
                region.holes[i].MinZ = minz;
                region.holes[i].Leftmost = leftmost;
            }

            Array.Sort(region.holes, (va, vb) =>
            {
                var a = va;
                var b = vb;
                if (a.MinX == b.MinX)
                {
                    if (a.MinZ < b.MinZ) return -1;
                    if (a.MinZ > b.MinZ) return 1;
                }
                else
                {
                    if (a.MinX < b.MinX) return -1;
                    if (a.MinX > b.MinX) return 1;
                }
                return 0;
            });

            int maxVerts = region.outline.nverts;
            for (int i = 0; i < region.nholes; i++)
            {
                maxVerts += region.holes[i].Contour.nverts;
            }

            PotentialDiagonal[] diags = Helper.CreateArray(maxVerts, new PotentialDiagonal()
            {
                Dist = int.MinValue,
                Vert = int.MinValue,
            });

            var outline = region.outline;

            // Merge holes into the outline one by one.
            for (int i = 0; i < region.nholes; i++)
            {
                var hole = region.holes[i].Contour;

                int index = -1;
                int bestVertex = region.holes[i].Leftmost;
                for (int iter = 0; iter < hole.nverts; iter++)
                {
                    // Find potential diagonals.
                    // The 'best' vertex must be in the cone described by 3 cosequtive vertices of the outline.
                    // ..o j-1
                    //   |
                    //   |   * best
                    //   |
                    // j o-----o j+1
                    //         :
                    int ndiags = 0;
                    var corner = hole.verts[bestVertex];
                    for (int j = 0; j < outline.nverts; j++)
                    {
                        if (InCone(j, outline.nverts, outline.verts, corner))
                        {
                            int dx = outline.verts[j].X - corner.X;
                            int dz = outline.verts[j].Z - corner.Z;
                            diags[ndiags].Vert = j;
                            diags[ndiags].Dist = dx * dx + dz * dz;
                            ndiags++;
                        }
                    }
                    // Sort potential diagonals by distance, we want to make the connection as short as possible.
                    Array.Sort(diags, 0, ndiags, PotentialDiagonal.DefaultComparer);

                    // Find a diagonal that is not intersecting the outline not the remaining holes.
                    index = -1;
                    for (int j = 0; j < ndiags; j++)
                    {
                        var pt = outline.verts[diags[j].Vert];
                        bool intersect = IntersectSegCountour(pt, corner, diags[i].Vert, outline.nverts, outline.verts);
                        for (int k = i; k < region.nholes && !intersect; k++)
                        {
                            intersect |= IntersectSegCountour(pt, corner, -1, region.holes[k].Contour.nverts, region.holes[k].Contour.verts);
                        }
                        if (!intersect)
                        {
                            index = diags[j].Vert;
                            break;
                        }
                    }
                    // If found non-intersecting diagonal, stop looking.
                    if (index != -1)
                    {
                        break;
                    }
                    // All the potential diagonals for the current vertex were intersecting, try next vertex.
                    bestVertex = (bestVertex + 1) % hole.nverts;
                }

                if (index == -1)
                {
                    Console.WriteLine($"Failed to find merge points for {region.outline} and {hole}.");
                }
                else if (!MergeContours(region.outline, hole, index, bestVertex))
                {
                    Console.WriteLine($"Failed to merge contours {region.outline} and {hole}.");
                }
            }
        }
        public static bool BuildContours(CompactHeightfield chf, float maxError, int maxEdgeLen, BuildContoursFlagTypes buildFlags, out ContourSet cset)
        {
            int w = chf.Width;
            int h = chf.Height;
            int borderSize = chf.BorderSize;
            int maxContours = Math.Max(chf.MaxRegions, 8);

            var bmin = chf.BoundingBox.Minimum;
            var bmax = chf.BoundingBox.Maximum;
            if (borderSize > 0)
            {
                // If the heightfield was build with bordersize, remove the offset.
                float pad = borderSize * chf.CS;
                bmin.X += pad;
                bmin.Z += pad;
                bmax.X -= pad;
                bmax.Z -= pad;
            }

            cset = new ContourSet
            {
                bmin = bmin,
                bmax = bmax,
                cs = chf.CS,
                ch = chf.CH,
                width = chf.Width - chf.BorderSize * 2,
                height = chf.Height - chf.BorderSize * 2,
                borderSize = chf.BorderSize,
                maxError = maxError,
                conts = new Contour[maxContours],
                nconts = 0
            };

            int[] flags = new int[chf.SpanCount];

            // Mark boundaries.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        int res = 0;
                        var s = chf.Spans[i];
                        if (chf.Spans[i].Reg == 0 || (chf.Spans[i].Reg & RC_BORDER_REG) != 0)
                        {
                            flags[i] = 0;
                            continue;
                        }
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            int r = 0;
                            if (GetCon(s, dir) != RC_NOT_CONNECTED)
                            {
                                int ax = x + GetDirOffsetX(dir);
                                int ay = y + GetDirOffsetY(dir);
                                int ai = chf.Cells[ax + ay * w].Index + GetCon(s, dir);
                                r = chf.Spans[ai].Reg;
                            }
                            if (r == chf.Spans[i].Reg)
                            {
                                res |= (1 << dir);
                            }
                        }
                        flags[i] = res ^ 0xf; // Inverse, mark non connected edges.
                    }
                }
            }

            List<Int4> verts = new List<Int4>();
            List<Int4> simplified = new List<Int4>();

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        if (flags[i] == 0 || flags[i] == 0xf)
                        {
                            flags[i] = 0;
                            continue;
                        }
                        int reg = chf.Spans[i].Reg;
                        if (reg == 0 || (reg & RC_BORDER_REG) != 0)
                        {
                            continue;
                        }
                        var area = chf.Areas[i];

                        verts.Clear();
                        simplified.Clear();

                        WalkContour(x, y, i, chf, flags, out verts);

                        SimplifyContour(verts, simplified, maxError, maxEdgeLen, buildFlags);
                        RemoveDegenerateSegments(simplified);

                        // Store region->contour remap info.
                        // Create contour.
                        if (simplified.Count >= 3)
                        {
                            if (cset.nconts >= maxContours)
                            {
                                // Allocate more contours.
                                // This happens when a region has holes.
                                Contour[] newConts = new Contour[maxContours * 2];
                                for (int j = 0; j < cset.nconts; ++j)
                                {
                                    newConts[j] = cset.conts[j];
                                }
                                cset.conts = newConts;
                            }

                            var cont = new Contour
                            {
                                nverts = simplified.Count,
                                verts = simplified.ToArray(),
                                nrverts = verts.Count,
                                rverts = verts.ToArray(),
                                reg = reg,
                                area = area
                            };

                            if (borderSize > 0)
                            {
                                // If the heightfield was build with bordersize, remove the offset.
                                for (int j = 0; j < cont.nverts; ++j)
                                {
                                    var v = cont.verts[j];
                                    v.X -= borderSize;
                                    v.Z -= borderSize;
                                    cont.verts[j] = v;
                                }

                                // If the heightfield was build with bordersize, remove the offset.
                                for (int j = 0; j < cont.nrverts; ++j)
                                {
                                    var v = cont.rverts[j];
                                    v.X -= borderSize;
                                    v.Z -= borderSize;
                                    cont.rverts[j] = v;
                                }
                            }

                            cset.conts[cset.nconts++] = cont;
                        }
                    }
                }
            }

            // Merge holes if needed.
            if (cset.nconts > 0)
            {
                // Calculate winding of all polygons.
                int[] winding = new int[cset.nconts];
                int nholes = 0;
                for (int i = 0; i < cset.nconts; ++i)
                {
                    var cont = cset.conts[i];
                    // If the contour is wound backwards, it is a hole.
                    winding[i] = CalcAreaOfPolygon2D(cont.verts, cont.nverts) < 0 ? -1 : 1;
                    if (winding[i] < 0)
                    {
                        nholes++;
                    }
                }

                if (nholes > 0)
                {
                    // Collect outline contour and holes contours per region.
                    // We assume that there is one outline and multiple holes.
                    int nregions = chf.MaxRegions + 1;
                    var regions = Helper.CreateArray(nregions, () => { return new ContourRegion(); });
                    var holes = Helper.CreateArray(cset.nconts, () => { return new ContourHole(); });

                    for (int i = 0; i < cset.nconts; ++i)
                    {
                        var cont = cset.conts[i];
                        // Positively would contours are outlines, negative holes.
                        if (winding[i] > 0)
                        {
                            if (regions[cont.reg].outline != null)
                            {
                                Console.WriteLine($"Multiple outlines for region {cont.reg}");
                            }
                            regions[cont.reg].outline = cont;
                        }
                        else
                        {
                            regions[cont.reg].nholes++;
                        }
                    }
                    int index = 0;
                    for (int i = 0; i < nregions; i++)
                    {
                        if (regions[i].nholes > 0)
                        {
                            regions[i].holes = new ContourHole[regions[i].nholes];
                            Array.Copy(holes, index, regions[i].holes, 0, regions[i].nholes);
                            index += regions[i].nholes;
                            regions[i].nholes = 0;
                        }
                    }
                    for (int i = 0; i < cset.nconts; ++i)
                    {
                        var cont = cset.conts[i];
                        var reg = regions[cont.reg];
                        if (winding[i] < 0)
                        {
                            reg.holes[reg.nholes++].Contour = cont;
                        }
                    }

                    // Finally merge each regions holes into the outline.
                    for (int i = 0; i < nregions; i++)
                    {
                        var reg = regions[i];
                        if (reg.nholes == 0)
                        {
                            continue;
                        }

                        if (reg.outline != null)
                        {
                            MergeRegionHoles(reg);
                        }
                        else
                        {
                            // The region does not have an outline.
                            // This can happen if the contour becames selfoverlapping because of too aggressive simplification settings.
                            Console.WriteLine($"Bad outline for region {i}, contour simplification is likely too aggressive.");
                        }
                    }
                }

            }

            return true;
        }

        #endregion

        #region RECASTLAYERS

        public static bool OverlapRange(int amin, int amax, int bmin, int bmax)
        {
            return !(amin > bmax || amax < bmin);
        }
        public static bool Contains(int[] a, int an, int v)
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
        public static bool AddUniqueLayer(ref LayerRegion layer, int anMax, int v)
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
        public static bool AddUniqueNei(ref LayerRegion layer, int anMax, int v)
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
        public static bool BuildHeightfieldLayers(CompactHeightfield chf, int borderSize, int walkableHeight, out HeightfieldLayerSet lset)
        {
            lset = new HeightfieldLayerSet();

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
                        if (chf.Areas[i] == TileCacheAreas.RC_NULL_AREA) continue;

                        int sid = 0xff;

                        // -x
                        if (GetCon(s, 0) != RC_NOT_CONNECTED)
                        {
                            int ax = x + GetDirOffsetX(0);
                            int ay = y + GetDirOffsetY(0);
                            int ai = chf.Cells[ax + ay * w].Index + GetCon(s, 0);
                            if (chf.Areas[ai] != TileCacheAreas.RC_NULL_AREA && srcReg[ai] != 0xff)
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
                        if (GetCon(s, 3) != RC_NOT_CONNECTED)
                        {
                            int ax = x + GetDirOffsetX(3);
                            int ay = y + GetDirOffsetY(3);
                            int ai = chf.Cells[ax + ay * w].Index + GetCon(s, 3);
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
                            if (GetCon(s, dir) != RC_NOT_CONNECTED)
                            {
                                int ax = x + GetDirOffsetX(dir);
                                int ay = y + GetDirOffsetY(dir);
                                int ai = chf.Cells[ax + ay * w].Index + GetCon(s, dir);
                                int rai = srcReg[ai];
                                if (rai != 0xff && rai != ri)
                                {
                                    // Don't check return value -- if we cannot add the neighbor
                                    // it will just cause a few more regions to be created, which
                                    // is fine.
                                    AddUniqueNei(ref regs[ri], LayerRegion.MaxNeighbors, rai);
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
                return true;
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
                layer.Areas = Helper.CreateArray(gridSize, TileCacheAreas.RC_NULL_AREA);
                layer.Cons = Helper.CreateArray(gridSize, 0x00);

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
                layer.CS = chf.CS;
                layer.CH = chf.CH;

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
                                if (GetCon(s, dir) != RC_NOT_CONNECTED)
                                {
                                    int ax = cx + GetDirOffsetX(dir);
                                    int ay = cy + GetDirOffsetY(dir);
                                    int ai = chf.Cells[ax + ay * w].Index + GetCon(s, dir);
                                    int alid = (srcReg[ai] != 0xff ? regs[srcReg[ai]].LayerId : 0xff);
                                    // Portal mask
                                    if (chf.Areas[ai] != TileCacheAreas.RC_NULL_AREA && lid != alid)
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
                                    if (chf.Areas[ai] != TileCacheAreas.RC_NULL_AREA && lid == alid)
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

                            layer.Cons[idx] = ((portal << 4) | con);
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

            return true;
        }

        #endregion

        #region RECASTMESH

        public static bool BuildMeshAdjacency(Polygoni[] polys, int npolys, int nverts, int vertsPerPoly)
        {
            // Based on code by Eric Lengyel from:
            // http://www.terathon.com/code/edges.php

            int maxEdgeCount = npolys * vertsPerPoly;
            int[] firstEdge = new int[nverts];
            int[] nextEdge = new int[maxEdgeCount];
            int edgeCount = 0;

            Edge[] edges = new Edge[maxEdgeCount];

            for (int i = 0; i < nverts; i++)
            {
                firstEdge[i] = RC_MESH_NULL_IDX;
            }
            for (int i = 0; i < maxEdgeCount; i++)
            {
                nextEdge[i] = RC_MESH_NULL_IDX;
            }

            for (int i = 0; i < npolys; ++i)
            {
                var t = polys[i];
                for (int j = 0; j < vertsPerPoly; ++j)
                {
                    if (t[j] == RC_MESH_NULL_IDX) break;
                    int v0 = t[j];
                    int v1 = (j + 1 >= vertsPerPoly || t[j + 1] == RC_MESH_NULL_IDX) ? t[0] : t[j + 1];
                    if (v0 < v1)
                    {
                        Edge edge = new Edge()
                        {
                            Vert = new int[2],
                            PolyEdge = new int[2],
                            Poly = new int[2],
                        };
                        edge.Vert[0] = v0;
                        edge.Vert[1] = v1;
                        edge.Poly[0] = i;
                        edge.PolyEdge[0] = j;
                        edge.Poly[1] = i;
                        edge.PolyEdge[1] = 0;
                        edges[edgeCount] = edge;
                        // Insert edge
                        nextEdge[edgeCount] = firstEdge[v0];
                        firstEdge[v0] = edgeCount;
                        edgeCount++;
                    }
                }
            }

            for (int i = 0; i < npolys; ++i)
            {
                var t = polys[i];
                for (int j = 0; j < vertsPerPoly; ++j)
                {
                    if (t[j] == RC_MESH_NULL_IDX) break;
                    int v0 = t[j];
                    int v1 = (j + 1 >= vertsPerPoly || t[j + 1] == RC_MESH_NULL_IDX) ? t[0] : t[j + 1];
                    if (v0 > v1)
                    {
                        for (int e = firstEdge[v1]; e != RC_MESH_NULL_IDX; e = nextEdge[e])
                        {
                            Edge edge = edges[e];
                            if (edge.Vert[1] == v0 && edge.Poly[0] == edge.Poly[1])
                            {
                                edge.Poly[1] = i;
                                edge.PolyEdge[1] = j;
                                break;
                            }
                        }
                    }
                }
            }

            // Store adjacency
            for (int i = 0; i < edgeCount; ++i)
            {
                Edge e = edges[i];
                if (e.Poly[0] != e.Poly[1])
                {
                    var p0 = polys[e.Poly[0]];
                    var p1 = polys[e.Poly[1]];
                    p0[vertsPerPoly + e.PolyEdge[0]] = e.Poly[1];
                    p1[vertsPerPoly + e.PolyEdge[1]] = e.Poly[0];
                }
            }

            return true;
        }
        public static int ComputeVertexHash(int x, int y, int z)
        {
            uint h1 = 0x8da6b343; // Large multiplicative constants
            uint h2 = 0xd8163841; // here arbitrarily chosen primes
            uint h3 = 0xcb1ab31f;
            uint n = (uint)(h1 * x + h2 * y + h3 * z);
            return (int)(n & (VERTEX_BUCKET_COUNT - 1));
        }
        public static int AddVertex(int x, int y, int z, PolyMesh mesh, int[] firstVert, int[] nextVert)
        {
            int bucket = ComputeVertexHash(x, 0, z);
            int i = firstVert[bucket];

            while (i != -1)
            {
                var v = mesh.Verts[i];
                if (v.X == x && (Math.Abs(v.Y - y) <= 2) && v.Z == z)
                {
                    return i;
                }
                i = nextVert[i]; // next
            }

            // Could not find, create new.
            i = mesh.NVerts; mesh.NVerts++;
            mesh.Verts[i] = new Int3(x, y, z);
            nextVert[i] = firstVert[bucket];
            firstVert[bucket] = i;

            return i;
        }
        public static bool DiagonalieLoose(int i, int j, int n, Int4[] verts, int[] indices)
        {
            Int4 d0 = verts[(indices[i] & 0x0fffffff)];
            Int4 d1 = verts[(indices[j] & 0x0fffffff)];

            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = Next(k, n);
                // Skip edges incident to i or j
                if (!((k == i) || (k1 == i) || (k == j) || (k1 == j)))
                {
                    Int4 p0 = verts[(indices[k] & 0x0fffffff)];
                    Int4 p1 = verts[(indices[k1] & 0x0fffffff)];

                    if (Vequal(d0, p0) || Vequal(d1, p0) || Vequal(d0, p1) || Vequal(d1, p1))
                    {
                        continue;
                    }

                    if (IntersectProp(d0, d1, p0, p1))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public static bool InConeLoose(int i, int j, int n, Int4[] verts, int[] indices)
        {
            Int4 pi = verts[(indices[i] & 0x0fffffff) * 4];
            Int4 pj = verts[(indices[j] & 0x0fffffff) * 4];
            Int4 pi1 = verts[(indices[Next(i, n)] & 0x0fffffff)];
            Int4 pin1 = verts[(indices[Prev(i, n)] & 0x0fffffff)];

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn(pin1, pi, pi1))
            {
                return LeftOn(pi, pj, pin1) && LeftOn(pj, pi, pi1);
            }
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn(pi, pj, pi1) && LeftOn(pj, pi, pin1));
        }
        public static bool DiagonalLoose(int i, int j, int n, Int4[] verts, int[] indices)
        {
            return InConeLoose(i, j, n, verts, indices) && DiagonalieLoose(i, j, n, verts, indices);
        }
        public static int Triangulate(int n, Int4[] verts, ref int[] indices, out Int3[] tris)
        {
            int ntris = 0;

            // The last bit of the index is used to indicate if the vertex can be removed.
            for (int i = 0; i < n; i++)
            {
                int i1 = Next(i, n);
                int i2 = Next(i1, n);
                if (Diagonal(i, i2, n, verts, indices))
                {
                    indices[i1] |= 0x8000;
                }
            }

            List<Int3> dst = new List<Int3>();

            while (n > 3)
            {
                int minLen = -1;
                int mini = -1;
                for (int ix = 0; ix < n; ix++)
                {
                    int i1x = Next(ix, n);
                    if ((indices[i1x] & 0x8000) != 0)
                    {
                        var p0 = verts[(indices[ix] & 0x7fff)];
                        var p2 = verts[(indices[Next(i1x, n)] & 0x7fff)];

                        int dx = p2.X - p0.X;
                        int dz = p2.Z - p0.Z;
                        int len = dx * dx + dz * dz;
                        if (minLen < 0 || len < minLen)
                        {
                            minLen = len;
                            mini = ix;
                        }
                    }
                }

                if (mini == -1)
                {
                    // Should not happen.
                    tris = null;
                    return -ntris;
                }

                int i = mini;
                int i1 = Next(i, n);
                int i2 = Next(i1, n);

                dst.Add(new Int3()
                {
                    X = indices[i] & 0x7fff,
                    Y = indices[i1] & 0x7fff,
                    Z = indices[i2] & 0x7fff
                });
                ntris++;

                // Removes P[i1] by copying P[i+1]...P[n-1] left one index.
                n--;
                for (int k = i1; k < n; k++)
                {
                    indices[k] = indices[k + 1];
                }

                if (i1 >= n) i1 = 0;
                i = Prev(i1, n);
                // Update diagonal flags.
                if (Diagonal(Prev(i, n), i1, n, verts, indices))
                {
                    indices[i] |= 0x8000;
                }
                else
                {
                    indices[i] &= 0x7fff;
                }

                if (Diagonal(i, Next(i1, n), n, verts, indices))
                {
                    indices[i1] |= 0x8000;
                }
                else
                {
                    indices[i1] &= 0x7fff;
                }
            }

            // Append the remaining triangle.
            dst.Add(new Int3
            {
                X = indices[0] & 0x7fff,
                Y = indices[1] & 0x7fff,
                Z = indices[2] & 0x7fff,
            });
            ntris++;

            tris = dst.ToArray();

            return ntris;
        }
        public static int CountPolyVerts(Polygoni p)
        {
            for (int i = 0; i < Detour.DT_VERTS_PER_POLYGON; ++i)
            {
                if (p[i] == RC_MESH_NULL_IDX)
                {
                    return i;
                }
            }

            return Detour.DT_VERTS_PER_POLYGON;
        }
        public static bool Uleft(Int3 a, Int3 b, Int3 c)
        {
            return (b.X - a.X) * (c.Z - a.Z) - (c.X - a.X) * (b.Z - a.Z) < 0;
        }
        public static int GetPolyMergeValue(Polygoni pa, Polygoni pb, Int3[] verts, out int ea, out int eb)
        {
            ea = -1;
            eb = -1;

            int na = CountPolyVerts(pa);
            int nb = CountPolyVerts(pb);

            // If the merged polygon would be too big, do not merge.
            if (na + nb - 2 > Detour.DT_VERTS_PER_POLYGON)
            {
                return -1;
            }

            // Check if the polygons share an edge.
            for (int i = 0; i < na; ++i)
            {
                int va0 = pa[i];
                int va1 = pa[(i + 1) % na];
                if (va0 > va1)
                {
                    Helper.Swap(ref va0, ref va1);
                }
                for (int j = 0; j < nb; ++j)
                {
                    int vb0 = pb[j];
                    int vb1 = pb[(j + 1) % nb];
                    if (vb0 > vb1)
                    {
                        Helper.Swap(ref vb0, ref vb1);
                    }
                    if (va0 == vb0 && va1 == vb1)
                    {
                        ea = i;
                        eb = j;
                        break;
                    }
                }
            }

            // No common edge, cannot merge.
            if (ea == -1 || eb == -1)
            {
                return -1;
            }

            // Check to see if the merged polygon would be convex.
            int va, vb, vc;

            va = pa[(ea + na - 1) % na];
            vb = pa[ea];
            vc = pb[(eb + 2) % nb];
            if (!Uleft(verts[va], verts[vb], verts[vc]))
            {
                return -1;
            }

            va = pb[(eb + nb - 1) % nb];
            vb = pb[eb];
            vc = pa[(ea + 2) % na];
            if (!Uleft(verts[va], verts[vb], verts[vc]))
            {
                return -1;
            }

            va = pa[ea];
            vb = pa[(ea + 1) % na];

            int dx = verts[va][0] - verts[vb][0];
            int dy = verts[va][2] - verts[vb][2];

            return dx * dx + dy * dy;
        }
        public static Polygoni MergePolys(Polygoni pa, Polygoni pb, int ea, int eb)
        {
            int na = CountPolyVerts(pa);
            int nb = CountPolyVerts(pb);

            var tmp = new Polygoni(Math.Max(Detour.DT_VERTS_PER_POLYGON, na - 1 + nb - 1));

            // Merge polygons.
            int n = 0;
            // Add pa
            for (int i = 0; i < na - 1; ++i)
            {
                tmp[n++] = pa[(ea + 1 + i) % na];
            }
            // Add pb
            for (int i = 0; i < nb - 1; ++i)
            {
                tmp[n++] = pb[(eb + 1 + i) % nb];
            }

            return tmp;
        }
        public static void PushFront<T>(T v, T[] arr, ref int an)
        {
            an++;
            for (int i = an - 1; i > 0; --i)
            {
                arr[i] = arr[i - 1];
            }
            arr[0] = v;
        }
        public static void PushBack<T>(T v, T[] arr, ref int an)
        {
            arr[an] = v;
            an++;
        }
        private static bool CanRemoveVertex(PolyMesh mesh, int rem)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = 0;
            int numTouchedVerts = 0;
            int numRemainingEdges = 0;
            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];
                int nv = CountPolyVerts(p);
                int numRemoved = 0;
                int numVerts = 0;
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] == rem)
                    {
                        numTouchedVerts++;
                        numRemoved++;
                    }
                    numVerts++;
                }
                if (numRemoved != 0)
                {
                    numRemovedVerts += numRemoved;
                    numRemainingEdges += numVerts - (numRemoved + 1);
                }
            }

            // There would be too few edges remaining to create a polygon.
            // This can happen for example when a tip of a triangle is marked
            // as deletion, but there are no other polys that share the vertex.
            // In this case, the vertex should not be removed.
            if (numRemainingEdges <= 2)
            {
                return false;
            }

            // Find edges which share the removed vertex.
            int maxEdges = numTouchedVerts * 2;
            int nedges = 0;
            Int3[] edges = new Int3[maxEdges];

            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];
                int nv = CountPolyVerts(p);

                // Collect edges which touches the removed vertex.
                for (int j = 0, k = nv - 1; j < nv; k = j++)
                {
                    if (p[j] == rem || p[k] == rem)
                    {
                        // Arrange edge so that a=rem.
                        int a = p[j], b = p[k];
                        if (b == rem)
                        {
                            Helper.Swap(ref a, ref b);
                        }

                        // Check if the edge exists
                        bool exists = false;
                        for (int m = 0; m < nedges; ++m)
                        {
                            var e = edges[m];
                            if (e[1] == b)
                            {
                                // Exists, increment vertex share count.
                                e[2]++;
                                exists = true;
                            }
                        }
                        // Add new edge.
                        if (!exists)
                        {
                            var e = new Int3();
                            e[0] = a;
                            e[1] = b;
                            e[2] = 1;
                            edges[nedges] = e;
                            nedges++;
                        }
                    }
                }
            }

            // There should be no more than 2 open edges.
            // This catches the case that two non-adjacent polygons
            // share the removed vertex. In that case, do not remove the vertex.
            int numOpenEdges = 0;
            for (int i = 0; i < nedges; ++i)
            {
                if (edges[i][2] < 2)
                {
                    numOpenEdges++;
                }
            }
            if (numOpenEdges > 2)
            {
                return false;
            }

            return true;
        }
        private static bool RemoveVertex(PolyMesh mesh, int rem, int maxTris)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = 0;
            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];
                int nv = CountPolyVerts(p);
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] == rem)
                    {
                        numRemovedVerts++;
                    }
                }
            }

            int nedges = 0;
            Int4[] edges = new Int4[numRemovedVerts];
            int nhole = 0;
            int[] hole = new int[numRemovedVerts];
            int nhreg = 0;
            int[] hreg = new int[numRemovedVerts];
            int nharea = 0;
            SamplePolyAreas[] harea = new SamplePolyAreas[numRemovedVerts];

            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];
                int nv = CountPolyVerts(p);
                bool hasRem = false;
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] == rem) hasRem = true;
                }
                if (hasRem)
                {
                    // Collect edges which does not touch the removed vertex.
                    for (int j = 0, k = nv - 1; j < nv; k = j++)
                    {
                        if (p[j] != rem && p[k] != rem)
                        {
                            var e = new Int4(p[k], p[j], mesh.Regs[i], (int)mesh.Areas[i]);
                            edges[nedges] = e;
                            nedges++;
                        }
                    }
                    // Remove the polygon.
                    var p2 = mesh.Polys[mesh.NPolys - 1];
                    if (p != p2)
                    {
                        mesh.Polys[i] = mesh.Polys[mesh.NPolys - 1];
                    }
                    mesh.Polys[mesh.NPolys - 1] = null;
                    mesh.Regs[i] = mesh.Regs[mesh.NPolys - 1];
                    mesh.Areas[i] = mesh.Areas[mesh.NPolys - 1];
                    mesh.NPolys--;
                    --i;
                }
            }

            // Remove vertex.
            for (int i = rem; i < mesh.NVerts - 1; ++i)
            {
                mesh.Verts[i] = mesh.Verts[(i + 1)];
            }
            mesh.NVerts--;

            // Adjust indices to match the removed vertex layout.
            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];
                int nv = CountPolyVerts(p);
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] > rem) p[j]--;
                }
            }
            for (int i = 0; i < nedges; ++i)
            {
                if (edges[i].X > rem) edges[i].X--;
                if (edges[i].Y > rem) edges[i].Y--;
            }

            if (nedges == 0)
            {
                return true;
            }

            // Start with one vertex, keep appending connected
            // segments to the start and end of the hole.
            PushBack(edges[0].X, hole, ref nhole);
            PushBack(edges[0].Z, hreg, ref nhreg);
            PushBack((SamplePolyAreas)edges[0].W, harea, ref nharea);

            while (nedges != 0)
            {
                bool match = false;

                for (int i = 0; i < nedges; ++i)
                {
                    int ea = edges[i].X;
                    int eb = edges[i].Y;
                    int r = edges[i].Z;
                    SamplePolyAreas a = (SamplePolyAreas)edges[i].W;
                    bool add = false;
                    if (hole[0] == eb)
                    {
                        // The segment matches the beginning of the hole boundary.
                        PushFront(ea, hole, ref nhole);
                        PushFront(r, hreg, ref nhreg);
                        PushFront(a, harea, ref nharea);
                        add = true;
                    }
                    else if (hole[nhole - 1] == ea)
                    {
                        // The segment matches the end of the hole boundary.
                        PushBack(eb, hole, ref nhole);
                        PushBack(r, hreg, ref nhreg);
                        PushBack(a, harea, ref nharea);
                        add = true;
                    }
                    if (add)
                    {
                        // The edge segment was added, remove it.
                        edges[i] = edges[(nedges - 1)];
                        nedges--;
                        match = true;
                        i--;
                    }
                }

                if (!match)
                {
                    break;
                }
            }

            var tverts = new Int4[nhole];
            var thole = new int[nhole];

            // Generate temp vertex array for triangulation.
            for (int i = 0; i < nhole; ++i)
            {
                int pi = hole[i];
                tverts[i].X = mesh.Verts[pi].X;
                tverts[i].Y = mesh.Verts[pi].Y;
                tverts[i].Z = mesh.Verts[pi].Z;
                tverts[i].W = 0;
                thole[i] = i;
            }

            // Triangulate the hole.
            int ntris = Triangulate(nhole, tverts, ref thole, out Int3[] tris);
            if (ntris < 0)
            {
                Console.WriteLine("removeVertex: triangulate() returned bad results.");
                ntris = -ntris;
            }

            // Merge the hole triangles back to polygons.
            var polys = new Polygoni[(ntris + 1)];
            var pregs = new int[ntris];
            var pareas = new SamplePolyAreas[ntris];

            // Build initial polygons.
            int npolys = 0;
            for (int j = 0; j < ntris; ++j)
            {
                var t = tris[j];
                if (t.X != t.Y && t.X != t.Z && t.Y != t.Z)
                {
                    polys[npolys][0] = hole[t.X];
                    polys[npolys][1] = hole[t.Y];
                    polys[npolys][2] = hole[t.Z];

                    // If this polygon covers multiple region types then mark it as such
                    if (hreg[t.X] != hreg[t.Y] || hreg[t.Y] != hreg[t.Z])
                    {
                        pregs[npolys] = RC_MULTIPLE_REGS;
                    }
                    else
                    {
                        pregs[npolys] = hreg[t.X];
                    }

                    pareas[npolys] = harea[t.X];
                    npolys++;
                }
            }
            if (npolys == 0)
            {
                return true;
            }

            // Merge polygons.
            int nvp = mesh.NVP;
            if (nvp > 3)
            {
                while (true)
                {
                    // Find best polygons to merge.
                    int bestMergeVal = 0;
                    int bestPa = 0, bestPb = 0, bestEa = 0, bestEb = 0;

                    for (int j = 0; j < npolys - 1; ++j)
                    {
                        var pj = polys[j];
                        for (int k = j + 1; k < npolys; ++k)
                        {
                            var pk = polys[k];
                            int v = GetPolyMergeValue(pj, pk, mesh.Verts, out int ea, out int eb);
                            if (v > bestMergeVal)
                            {
                                bestMergeVal = v;
                                bestPa = j;
                                bestPb = k;
                                bestEa = ea;
                                bestEb = eb;
                            }
                        }
                    }

                    if (bestMergeVal > 0)
                    {
                        // Found best, merge.
                        polys[bestPa] = MergePolys(polys[bestPa], polys[bestPb], bestEa, bestEb);
                        if (pregs[bestPa] != pregs[bestPb])
                        {
                            pregs[bestPa] = RC_MULTIPLE_REGS;
                        }
                        polys[bestPb] = polys[(npolys - 1)];
                        pregs[bestPb] = pregs[npolys - 1];
                        pareas[bestPb] = pareas[npolys - 1];
                        npolys--;
                    }
                    else
                    {
                        // Could not merge any polygons, stop.
                        break;
                    }
                }
            }

            // Store polygons.
            for (int i = 0; i < npolys; ++i)
            {
                if (mesh.NPolys >= maxTris) break;
                var p = mesh.Polys[mesh.NPolys];
                for (int j = 0; j < nvp; ++j)
                {
                    p[j] = polys[i][j];
                }
                mesh.Regs[mesh.NPolys] = pregs[i];
                mesh.Areas[mesh.NPolys] = pareas[i];
                mesh.NPolys++;
                if (mesh.NPolys > maxTris)
                {
                    Console.WriteLine($"removeVertex: Too many polygons {mesh.NPolys} (max:{maxTris}).");
                    return false;
                }
            }

            return true;
        }
        public static bool BuildPolyMesh(ContourSet cset, int tx, int ty, int nvp, out PolyMesh mesh)
        {
            mesh = new PolyMesh
            {
                BMin = cset.bmin,
                BMax = cset.bmax,
                CS = cset.cs,
                CH = cset.ch,
                BorderSize = cset.borderSize,
                MaxEdgeError = cset.maxError
            };

            int maxVertices = 0;
            int maxTris = 0;
            int maxVertsPerCont = 0;
            for (int i = 0; i < cset.nconts; ++i)
            {
                // Skip null contours.
                if (cset.conts[i].nverts < 3) continue;
                maxVertices += cset.conts[i].nverts;
                maxTris += cset.conts[i].nverts - 2;
                maxVertsPerCont = Math.Max(maxVertsPerCont, cset.conts[i].nverts);
            }

            if (maxVertices >= 0xfffe)
            {
                throw new EngineException(string.Format("rcBuildPolyMesh: Too many vertices {0}.", maxVertices));
            }

            int[] vflags = new int[maxVertices];

            mesh.Verts = new Int3[maxVertices];
            mesh.Polys = new Polygoni[maxTris];
            mesh.Regs = new int[maxTris];
            mesh.Areas = new SamplePolyAreas[maxTris];

            mesh.NVerts = 0;
            mesh.NPolys = 0;
            mesh.NVP = nvp;
            mesh.MaxPolys = maxTris;

            int[] nextVert = Helper.CreateArray(maxVertices, 0);
            int[] firstVert = Helper.CreateArray(VERTEX_BUCKET_COUNT, -1);
            int[] indices = new int[maxVertsPerCont];

            for (int i = 0; i < cset.nconts; ++i)
            {
                var cont = cset.conts[i];

                // Skip null contours.
                if (cont.nverts < 3)
                {
                    continue;
                }

                // Triangulate contour
                for (int j = 0; j < cont.nverts; ++j)
                {
                    indices[j] = j;
                }

                int ntris = Triangulate(cont.nverts, cont.verts, ref indices, out Int3[] tris);
                if (ntris <= 0)
                {
                    // Bad triangulation, should not happen.
                    Console.WriteLine($"rcBuildPolyMesh: Bad triangulation Contour {i}.");
                    ntris = -ntris;
                }

                // Add and merge vertices.
                for (int j = 0; j < cont.nverts; ++j)
                {
                    var v = cont.verts[j];
                    indices[j] = AddVertex(v.X, v.Y, v.Z, mesh, firstVert, nextVert);
                    if ((v.W & RC_BORDER_VERTEX) != 0)
                    {
                        // This vertex should be removed.
                        vflags[indices[j]] = 1;
                    }
                }

                // Build initial polygons.
                int npolys = 0;
                Polygoni[] polys = new Polygoni[maxVertsPerCont];
                for (int j = 0; j < ntris; ++j)
                {
                    var t = tris[j];
                    if (t.X != t.Y && t.X != t.Z && t.Y != t.Z)
                    {
                        polys[npolys] = new Polygoni(Detour.DT_VERTS_PER_POLYGON);
                        polys[npolys][0] = indices[t.X];
                        polys[npolys][1] = indices[t.Y];
                        polys[npolys][2] = indices[t.Z];
                        npolys++;
                    }
                }
                if (npolys == 0)
                {
                    continue;
                }

                // Merge polygons.
                if (nvp > 3)
                {
                    while (true)
                    {
                        // Find best polygons to merge.
                        int bestMergeVal = 0;
                        int bestPa = 0, bestPb = 0, bestEa = 0, bestEb = 0;

                        for (int j = 0; j < npolys - 1; ++j)
                        {
                            var pj = polys[j];
                            for (int k = j + 1; k < npolys; ++k)
                            {
                                var pk = polys[k];
                                int v = GetPolyMergeValue(pj, pk, mesh.Verts, out int ea, out int eb);
                                if (v > bestMergeVal)
                                {
                                    bestMergeVal = v;
                                    bestPa = j;
                                    bestPb = k;
                                    bestEa = ea;
                                    bestEb = eb;
                                }
                            }
                        }

                        if (bestMergeVal > 0)
                        {
                            // Found best, merge.
                            polys[bestPa] = MergePolys(polys[bestPa], polys[bestPb], bestEa, bestEb);
                            polys[bestPb] = polys[npolys - 1].Copy();
                            npolys--;
                        }
                        else
                        {
                            // Could not merge any polygons, stop.
                            break;
                        }
                    }
                }

                // Store polygons.
                for (int j = 0; j < npolys; ++j)
                {
                    var p = new Polygoni(nvp * 2); //Polygon with adjacency
                    var q = polys[j];
                    for (int k = 0; k < nvp; ++k)
                    {
                        p[k] = q[k];
                    }
                    mesh.Polys[mesh.NPolys] = p;
                    mesh.Regs[mesh.NPolys] = cont.reg;
                    mesh.Areas[mesh.NPolys] = (SamplePolyAreas)(int)cont.area;
                    mesh.NPolys++;
                    if (mesh.NPolys > maxTris)
                    {
                        throw new EngineException(string.Format("rcBuildPolyMesh: Too many polygons {0} (max:{1}).", mesh.NPolys, maxTris));
                    }
                }
            }

            // Remove edge vertices.
            for (int i = 0; i < mesh.NVerts; ++i)
            {
                if (vflags[i] != 0)
                {
                    if (!CanRemoveVertex(mesh, i))
                    {
                        continue;
                    }
                    if (!RemoveVertex(mesh, i, maxTris))
                    {
                        // Failed to remove vertex
                        throw new EngineException(string.Format("Failed to remove edge vertex {0}.", i));
                    }
                    // Remove vertex
                    // Note: mesh.nverts is already decremented inside removeVertex()!
                    // Fixup vertex flags
                    for (int j = i; j < mesh.NVerts; ++j)
                    {
                        vflags[j] = vflags[j + 1];
                    }
                    --i;
                }
            }

            // Calculate adjacency.
            if (!BuildMeshAdjacency(mesh.Polys, mesh.NPolys, mesh.NVerts, nvp))
            {
                throw new EngineException("Adjacency failed.");
            }

            // Find portal edges
            if (mesh.BorderSize > 0)
            {
                int w = cset.width;
                int h = cset.height;
                for (int i = 0; i < mesh.NPolys; ++i)
                {
                    var p = mesh.Polys[i];
                    for (int j = 0; j < nvp; ++j)
                    {
                        if (p[j] == RC_MESH_NULL_IDX)
                        {
                            break;
                        }
                        // Skip connected edges.
                        if (p[nvp + j] != RC_MESH_NULL_IDX)
                        {
                            continue;
                        }
                        int nj = j + 1;
                        if (nj >= nvp || p[nj] == RC_MESH_NULL_IDX)
                        {
                            nj = 0;
                        }
                        var va = mesh.Verts[p[j]];
                        var vb = mesh.Verts[p[nj]];

                        if (va.X == 0 && vb.X == 0)
                        {
                            p[nvp + j] = 0x8000;
                        }
                        else if (va.Z == h && vb.Z == h)
                        {
                            p[nvp + j] = 0x8000 | 1;
                        }
                        else if (va.X == w && vb.X == w)
                        {
                            p[nvp + j] = 0x8000 | 2;
                        }
                        else if (va.Z == 0 && vb.Z == 0)
                        {
                            p[nvp + j] = 0x8000 | 3;
                        }
                    }
                }
            }

            // Just allocate the mesh flags array. The user is resposible to fill it.
            mesh.Flags = new SamplePolyFlagTypes[mesh.NPolys];

            if (mesh.NVerts > 0xffff)
            {
                throw new EngineException(string.Format("The resulting mesh has too many vertices {0} (max {1}). Data can be corrupted.", mesh.NVerts, 0xffff));
            }
            if (mesh.NPolys > 0xffff)
            {
                throw new EngineException(string.Format("The resulting mesh has too many polygons {0} (max {1}). Data can be corrupted.", mesh.NPolys, 0xffff));
            }

            return true;
        }
        public static bool MergePolyMeshes(PolyMesh[] meshes, int nmeshes, out PolyMesh mesh)
        {
            mesh = null;

            if (nmeshes == 0 || meshes == null)
            {
                return true;
            }

            mesh = new PolyMesh
            {
                NVP = meshes[0].NVP,
                CS = meshes[0].CS,
                CH = meshes[0].CH,
                BMin = meshes[0].BMin,
                BMax = meshes[0].BMax
            };

            int maxVerts = 0;
            int maxPolys = 0;
            int maxVertsPerMesh = 0;
            for (int i = 0; i < nmeshes; ++i)
            {
                mesh.BMin = Vector3.Min(mesh.BMin, meshes[i].BMin);
                mesh.BMax = Vector3.Max(mesh.BMax, meshes[i].BMax);
                maxVertsPerMesh = Math.Max(maxVertsPerMesh, meshes[i].NVerts);
                maxVerts += meshes[i].NVerts;
                maxPolys += meshes[i].NPolys;
            }

            mesh.NVerts = 0;
            mesh.Verts = new Int3[maxVerts];
            mesh.NPolys = 0;
            mesh.Polys = new Polygoni[maxPolys];
            mesh.Regs = new int[maxPolys];
            mesh.Areas = new SamplePolyAreas[maxPolys];
            mesh.Flags = new SamplePolyFlagTypes[maxPolys];

            int[] nextVert = Helper.CreateArray(maxVerts, 0);
            int[] firstVert = Helper.CreateArray(VERTEX_BUCKET_COUNT, -1);
            int[] vremap = Helper.CreateArray(maxVertsPerMesh, 0);

            for (int i = 0; i < nmeshes; ++i)
            {
                var pmesh = meshes[i];

                int ox = (int)Math.Floor((pmesh.BMin.X - mesh.BMin.X) / mesh.CS + 0.5f);
                int oz = (int)Math.Floor((pmesh.BMin.X - mesh.BMin.Z) / mesh.CS + 0.5f);

                bool isMinX = (ox == 0);
                bool isMinZ = (oz == 0);
                bool isMaxX = ((int)Math.Floor((mesh.BMax.X - pmesh.BMax.X) / mesh.CS + 0.5f)) == 0;
                bool isMaxZ = ((int)Math.Floor((mesh.BMax.Z - pmesh.BMax.Z) / mesh.CS + 0.5f)) == 0;
                bool isOnBorder = (isMinX || isMinZ || isMaxX || isMaxZ);

                for (int j = 0; j < pmesh.NVerts; ++j)
                {
                    var v = pmesh.Verts[j];
                    vremap[j] = AddVertex(v.X + ox, v.Y, v.Z + oz, mesh, firstVert, nextVert);
                }

                for (int j = 0; j < pmesh.NPolys; ++j)
                {
                    var tgt = mesh.Polys[mesh.NPolys];
                    var src = pmesh.Polys[j];
                    mesh.Regs[mesh.NPolys] = pmesh.Regs[j];
                    mesh.Areas[mesh.NPolys] = pmesh.Areas[j];
                    mesh.Flags[mesh.NPolys] = pmesh.Flags[j];
                    mesh.NPolys++;
                    for (int k = 0; k < mesh.NVP; ++k)
                    {
                        if (src[k] == RC_MESH_NULL_IDX)
                        {
                            break;
                        }
                        tgt[k] = vremap[src[k]];
                    }

                    if (isOnBorder)
                    {
                        for (int k = mesh.NVP; k < mesh.NVP * 2; ++k)
                        {
                            if ((src[k] & 0x8000) != 0 && src[k] != 0xffff)
                            {
                                int dir = src[k] & 0xf;
                                switch (dir)
                                {
                                    case 0: // Portal x-
                                        if (isMinX) tgt[k] = src[k];
                                        break;
                                    case 1: // Portal z+
                                        if (isMaxZ) tgt[k] = src[k];
                                        break;
                                    case 2: // Portal x+
                                        if (isMaxX) tgt[k] = src[k];
                                        break;
                                    case 3: // Portal z-
                                        if (isMinZ) tgt[k] = src[k];
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            // Calculate adjacency.
            if (!BuildMeshAdjacency(mesh.Polys, mesh.NPolys, mesh.NVerts, mesh.NVP))
            {
                throw new EngineException("rcMergePolyMeshes: Adjacency failed.");
            }

            if (mesh.NVerts > 0xffff)
            {
                throw new EngineException(string.Format("rcMergePolyMeshes: The resulting mesh has too many vertices {0} (max {1}). Data can be corrupted.", mesh.NVerts, 0xffff));
            }
            if (mesh.NPolys > 0xffff)
            {
                throw new EngineException(string.Format("rcMergePolyMeshes: The resulting mesh has too many polygons {0} (max {1}). Data can be corrupted.", mesh.NPolys, 0xffff));
            }

            return true;
        }
        public static bool CopyPolyMesh(PolyMesh src, out PolyMesh dst)
        {
            dst = new PolyMesh
            {
                NVerts = src.NVerts,
                NPolys = src.NPolys,
                MaxPolys = src.NPolys,
                NVP = src.NVP,
                BMin = src.BMin,
                BMax = src.BMax,
                CS = src.CS,
                CH = src.CH,
                BorderSize = src.BorderSize,
                MaxEdgeError = src.MaxEdgeError,
                Verts = new Int3[src.NVerts],
                Polys = new Polygoni[src.NPolys],
                Regs = new int[src.NPolys],
                Areas = new SamplePolyAreas[src.NPolys],
                Flags = new SamplePolyFlagTypes[src.NPolys]
            };

            Array.Copy(src.Verts, dst.Verts, src.NVerts);
            Array.Copy(src.Polys, dst.Polys, src.NPolys);
            Array.Copy(src.Regs, dst.Regs, src.NPolys);
            Array.Copy(src.Areas, dst.Areas, src.NPolys);
            Array.Copy(src.Flags, dst.Flags, src.NPolys);

            return true;
        }

        #endregion

        #region RECASTMESHDETAIL

        public static float VDot2(Vector3 a, Vector3 b)
        {
            return a[0] * b[0] + a[2] * b[2];
        }
        public static float VDistSq2(Vector3 p, Vector3 q)
        {
            float dx = q[0] - p[0];
            float dy = q[2] - p[2];
            return dx * dx + dy * dy;
        }
        public static float VDist2(Vector3 p, Vector3 q)
        {
            return (float)Math.Sqrt(VDistSq2(p, q));
        }
        public static float VCross2(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u1 = p2[0] - p1[0];
            float v1 = p2[2] - p1[2];
            float u2 = p3[0] - p1[0];
            float v2 = p3[2] - p1[2];
            return u1 * v2 - v1 * u2;
        }
        public static bool CircumCircle(Vector3 p1, Vector3 p2, Vector3 p3, out Vector3 c, out float r)
        {
            float EPS = 1e-6f;
            // Calculate the circle relative to p1, to avoid some precision issues.
            Vector3 v1 = new Vector3();
            Vector3 v2 = Vector3.Subtract(p2, p1);
            Vector3 v3 = Vector3.Subtract(p3, p1);

            c = new Vector3();
            float cp = VCross2(v1, v2, v3);
            if (Math.Abs(cp) > EPS)
            {
                float v1Sq = VDot2(v1, v1);
                float v2Sq = VDot2(v2, v2);
                float v3Sq = VDot2(v3, v3);
                c[0] = (v1Sq * (v2[2] - v3[2]) + v2Sq * (v3[2] - v1[2]) + v3Sq * (v1[2] - v2[2])) / (2 * cp);
                c[1] = 0;
                c[2] = (v1Sq * (v3[0] - v2[0]) + v2Sq * (v1[0] - v3[0]) + v3Sq * (v2[0] - v1[0])) / (2 * cp);
                r = VDist2(c, v1);
                c = Vector3.Add(c, p1);
                return true;
            }

            c = p1;
            r = 0;
            return false;
        }
        public static float DistPtTri(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 v0, v1, v2;
            v0 = Vector3.Subtract(c, a);
            v1 = Vector3.Subtract(b, a);
            v2 = Vector3.Subtract(p, a);

            float dot00 = Vector2.Dot(new Vector2(v0.X, v0.Z), new Vector2(v0.X, v0.Z));
            float dot01 = Vector2.Dot(new Vector2(v0.X, v0.Z), new Vector2(v1.X, v1.Z));
            float dot02 = Vector2.Dot(new Vector2(v0.X, v0.Z), new Vector2(v2.X, v2.Z));
            float dot11 = Vector2.Dot(new Vector2(v1.X, v1.Z), new Vector2(v1.X, v1.Z));
            float dot12 = Vector2.Dot(new Vector2(v1.X, v1.Z), new Vector2(v2.X, v2.Z));

            // Compute barycentric coordinates
            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // If point lies inside the triangle, return interpolated y-coord.
            float EPS = float.Epsilon;
            if (u >= -EPS && v >= -EPS && (u + v) <= 1 + EPS)
            {
                float y = a[1] + v0[1] * u + v1[1] * v;
                return Math.Abs(y - p[1]);
            }
            return float.MaxValue;
        }
        public static float DistancePtSeg(Vector3 pt, Vector3 p, Vector3 q)
        {
            float pqx = q.X - p.X;
            float pqy = q.Y - p.Y;
            float pqz = q.Z - p.Z;
            float dx = pt.X - p.X;
            float dy = pt.Y - p.Y;
            float dz = pt.Z - p.Z;
            float d = pqx * pqx + pqy * pqy + pqz * pqz;
            float t = pqx * dx + pqy * dy + pqz * dz;
            if (d > 0)
            {
                t /= d;
            }
            if (t < 0)
            {
                t = 0;
            }
            else if (t > 1)
            {
                t = 1;
            }

            dx = p.X + t * pqx - pt.X;
            dy = p.Y + t * pqy - pt.Y;
            dz = p.Z + t * pqz - pt.Z;

            return dx * dx + dy * dy + dz * dz;
        }
        public static float DistancePtSeg2d(Vector3 pt, Vector3 p, Vector3 q)
        {
            float pqx = q[0] - p[0];
            float pqz = q[2] - p[2];
            float dx = pt[0] - p[0];
            float dz = pt[2] - p[2];
            float d = pqx * pqx + pqz * pqz;
            float t = pqx * dx + pqz * dz;
            if (d > 0)
            {
                t /= d;
            }
            if (t < 0)
            {
                t = 0;
            }
            else if (t > 1)
            {
                t = 1;
            }

            dx = p[0] + t * pqx - pt[0];
            dz = p[2] + t * pqz - pt[2];

            return dx * dx + dz * dz;
        }
        public static float DistToTriMesh(Vector3 p, Vector3[] verts, int nverts, Int4[] tris, int ntris)
        {
            float dmin = float.MaxValue;
            for (int i = 0; i < ntris; ++i)
            {
                var va = verts[tris[i].X];
                var vb = verts[tris[i].Y];
                var vc = verts[tris[i].Z];
                float d = DistPtTri(p, va, vb, vc);
                if (d < dmin)
                {
                    dmin = d;
                }
            }
            if (dmin == float.MaxValue) return -1;
            return dmin;
        }
        public static float DistToPoly(int nvert, Vector3[] verts, Vector3 p)
        {
            float dmin = float.MaxValue;
            bool c = false;
            for (int i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                Vector3 vi = verts[i];
                Vector3 vj = verts[j];
                if (((vi[2] > p[2]) != (vj[2] > p[2])) &&
                    (p[0] < (vj[0] - vi[0]) * (p[2] - vi[2]) / (vj[2] - vi[2]) + vi[0]))
                {
                    c = !c;
                }
                dmin = Math.Min(dmin, DistancePtSeg2d(p, vj, vi));
            }
            return c ? -dmin : dmin;
        }
        private static int GetHeight(float fx, float fy, float fz, float cs, float ics, float ch, int radius, HeightPatch hp)
        {
            int ix = (int)Math.Floor(fx * ics + 0.01f);
            int iz = (int)Math.Floor(fz * ics + 0.01f);
            ix = MathUtil.Clamp(ix - hp.Xmin, 0, hp.Width - 1);
            iz = MathUtil.Clamp(iz - hp.Ymin, 0, hp.Height - 1);
            int h = hp.Data[ix + iz * hp.Width];
            if (h == RC_UNSET_HEIGHT)
            {
                // Special case when data might be bad.
                // Walk adjacent cells in a spiral up to 'radius', and look
                // for a pixel which has a valid height.
                int x = 1, z = 0, dx = 1, dz = 0;
                int maxSize = radius * 2 + 1;
                int maxIter = maxSize * maxSize - 1;

                int nextRingIterStart = 8;
                int nextRingIters = 16;

                float dmin = float.MaxValue;
                for (int i = 0; i < maxIter; i++)
                {
                    int nx = ix + x;
                    int nz = iz + z;

                    if (nx >= 0 && nz >= 0 && nx < hp.Width && nz < hp.Height)
                    {
                        int nh = hp.Data[nx + nz * hp.Width];
                        if (nh != RC_UNSET_HEIGHT)
                        {
                            float d = Math.Abs(nh * ch - fy);
                            if (d < dmin)
                            {
                                h = nh;
                                dmin = d;
                            }
                        }
                    }

                    // We are searching in a grid which looks approximately like this:
                    //  __________
                    // |2 ______ 2|
                    // | |1 __ 1| |
                    // | | |__| | |
                    // | |______| |
                    // |__________|
                    // We want to find the best height as close to the center cell as possible. This means that
                    // if we find a height in one of the neighbor cells to the center, we don't want to
                    // expand further out than the 8 neighbors - we want to limit our search to the closest
                    // of these "rings", but the best height in the ring.
                    // For example, the center is just 1 cell. We checked that at the entrance to the function.
                    // The next "ring" contains 8 cells (marked 1 above). Those are all the neighbors to the center cell.
                    // The next one again contains 16 cells (marked 2). In general each ring has 8 additional cells, which
                    // can be thought of as adding 2 cells around the "center" of each side when we expand the ring.
                    // Here we detect if we are about to enter the next ring, and if we are and we have found
                    // a height, we abort the search.
                    if (i + 1 == nextRingIterStart)
                    {
                        if (h != RC_UNSET_HEIGHT)
                        {
                            break;
                        }

                        nextRingIterStart += nextRingIters;
                        nextRingIters += 8;
                    }

                    if ((x == z) || ((x < 0) && (x == -z)) || ((x > 0) && (x == 1 - z)))
                    {
                        int tmp = dx;
                        dx = -dz;
                        dz = tmp;
                    }
                    x += dx;
                    z += dz;
                }
            }
            return h;
        }
        public static int FindEdge(Int4[] edges, int nedges, int s, int t)
        {
            for (int i = 0; i < nedges; i++)
            {
                var e = edges[i];
                if ((e.X == s && e.Y == t) || (e.X == t && e.Y == s))
                {
                    return i;
                }
            }
            return (int)EdgeValues.EV_UNDEF;
        }
        public static int AddEdge(ref Int4[] edges, ref int nedges, int maxEdges, int s, int t, int l, int r)
        {
            if (nedges >= maxEdges)
            {
                Console.WriteLine($"addEdge: Too many edges ({nedges}/{maxEdges}).");
                return (int)EdgeValues.EV_UNDEF;
            }

            // Add edge if not already in the triangulation.
            int e = FindEdge(edges, nedges, s, t);
            if (e == (int)EdgeValues.EV_UNDEF)
            {
                edges[nedges] = new Int4(s, t, l, r);
                return nedges++;
            }
            else
            {
                return (int)EdgeValues.EV_UNDEF;
            }
        }
        public static void UpdateLeftFace(ref Int4 e, int s, int t, int f)
        {
            if (e[0] == s && e[1] == t && e[2] == (int)EdgeValues.EV_UNDEF)
            {
                e[2] = f;
            }
            else if (e[1] == s && e[0] == t && e[3] == (int)EdgeValues.EV_UNDEF)
            {
                e[3] = f;
            }
        }
        public static int OverlapSegSeg2d(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            float a1 = VCross2(a, b, d);
            float a2 = VCross2(a, b, c);
            if (a1 * a2 < 0.0f)
            {
                float a3 = VCross2(c, d, a);
                float a4 = a3 + a2 - a1;
                if (a3 * a4 < 0.0f)
                {
                    return 1;
                }
            }
            return 0;
        }
        public static bool OverlapEdges(Vector3[] pts, Int4[] edges, int nedges, int s1, int t1)
        {
            for (int i = 0; i < nedges; ++i)
            {
                int s0 = edges[i].X;
                int t0 = edges[i].Y;
                // Same or connected edges do not overlap.
                if (s0 == s1 || s0 == t1 || t0 == s1 || t0 == t1)
                {
                    continue;
                }
                if (OverlapSegSeg2d(pts[s0], pts[t0], pts[s1], pts[t1]) != 0)
                {
                    return true;
                }
            }
            return false;
        }
        public static void CompleteFacet(Vector3[] pts, int npts, ref Int4[] edges, ref int nedges, int maxEdges, ref int nfaces, int e)
        {
            float EPS = float.Epsilon;

            var edge = edges[e];

            // Cache s and t.
            int s, t;
            if (edge[2] == (int)EdgeValues.EV_UNDEF)
            {
                s = edge[0];
                t = edge[1];
            }
            else if (edge[3] == (int)EdgeValues.EV_UNDEF)
            {
                s = edge[1];
                t = edge[0];
            }
            else
            {
                // Edge already completed.
                return;
            }

            // Find best point on left of edge.
            int pt = npts;
            Vector3 c = new Vector3();
            float r = -1;
            for (int u = 0; u < npts; ++u)
            {
                if (u == s || u == t) continue;
                if (VCross2(pts[s], pts[t], pts[u]) > EPS)
                {
                    if (r < 0)
                    {
                        // The circle is not updated yet, do it now.
                        pt = u;
                        CircumCircle(pts[s], pts[t], pts[u], out c, out r);
                        continue;
                    }
                    float d = VDist2(c, pts[u]);
                    float tol = 0.001f;
                    if (d > r * (1 + tol))
                    {
                        // Outside current circumcircle, skip.
                    }
                    else if (d < r * (1 - tol))
                    {
                        // Inside safe circumcircle, update circle.
                        pt = u;
                        CircumCircle(pts[s], pts[t], pts[u], out c, out r);
                    }
                    else
                    {
                        // Inside epsilon circum circle, do extra tests to make sure the edge is valid.
                        // s-u and t-u cannot overlap with s-pt nor t-pt if they exists.
                        if (OverlapEdges(pts, edges, nedges, s, u))
                        {
                            continue;
                        }
                        if (OverlapEdges(pts, edges, nedges, t, u))
                        {
                            continue;
                        }
                        // Edge is valid.
                        pt = u;
                        CircumCircle(pts[s], pts[t], pts[u], out c, out r);
                    }
                }
            }

            // Add new triangle or update edge info if s-t is on hull.
            if (pt < npts)
            {
                // Update face information of edge being completed.
                UpdateLeftFace(ref edges[e], s, t, nfaces);

                // Add new edge or update face info of old edge.
                e = FindEdge(edges, nedges, pt, s);
                if (e == (int)EdgeValues.EV_UNDEF)
                {
                    AddEdge(ref edges, ref nedges, maxEdges, pt, s, nfaces, (int)EdgeValues.EV_UNDEF);
                }
                else
                {
                    UpdateLeftFace(ref edges[e], pt, s, nfaces);
                }

                // Add new edge or update face info of old edge.
                e = FindEdge(edges, nedges, t, pt);
                if (e == (int)EdgeValues.EV_UNDEF)
                {
                    AddEdge(ref edges, ref nedges, maxEdges, t, pt, nfaces, (int)EdgeValues.EV_UNDEF);
                }
                else
                {
                    UpdateLeftFace(ref edges[e], t, pt, nfaces);
                }

                nfaces++;
            }
            else
            {
                UpdateLeftFace(ref edges[e], s, t, (int)EdgeValues.EV_HULL);
            }
        }
        public static void DelaunayHull(int npts, Vector3[] pts, int nhull, int[] hull, List<Int4> outTris, List<Int4> outEdges)
        {
            int nfaces = 0;
            int nedges = 0;
            int maxEdges = npts * 10;
            Int4[] edges = new Int4[maxEdges];

            for (int i = 0, j = nhull - 1; i < nhull; j = i++)
            {
                AddEdge(ref edges, ref nedges, maxEdges, hull[j], hull[i], (int)EdgeValues.EV_HULL, (int)EdgeValues.EV_UNDEF);
            }

            int currentEdge = 0;
            while (currentEdge < nedges)
            {
                if (edges[currentEdge][2] == (int)EdgeValues.EV_UNDEF)
                {
                    CompleteFacet(pts, npts, ref edges, ref nedges, maxEdges, ref nfaces, currentEdge);
                }
                if (edges[currentEdge][3] == (int)EdgeValues.EV_UNDEF)
                {
                    CompleteFacet(pts, npts, ref edges, ref nedges, maxEdges, ref nfaces, currentEdge);
                }
                currentEdge++;
            }

            // Create tris
            Int4[] tris = Helper.CreateArray(nfaces, new Int4(-1, -1, -1, -1));

            for (int i = 0; i < nedges; ++i)
            {
                var e = edges[i];
                if (e.W >= 0)
                {
                    // Left face
                    var t = tris[e[3]];
                    if (t.X == -1)
                    {
                        t.X = e[0];
                        t.Y = e[1];
                    }
                    else if (t.X == e[1])
                    {
                        t.Z = e[0];
                    }
                    else if (t.Y == e[0])
                    {
                        t.Z = e[1];
                    }
                    tris[e[3]] = t;
                }
                if (e[2] >= 0)
                {
                    // Right
                    var t = tris[e[2]];
                    if (t.X == -1)
                    {
                        t.X = e[1];
                        t.Y = e[0];
                    }
                    else if (t.X == e[0])
                    {
                        t.Z = e[1];
                    }
                    else if (t.Y == e[1])
                    {
                        t.Z = e[0];
                    }
                    tris[e[2]] = t;
                }
            }

            for (int i = 0; i < tris.Length; ++i)
            {
                var t = tris[i];
                if (t.X == -1 || t.Y == -1 || t.Z == -1)
                {
                    Console.WriteLine($"delaunayHull: Removing dangling face {i} [{t.X},{t.Y},{t.Z}].");
                    tris[i] = tris[tris.Length - 1];
                    Array.Resize(ref tris, tris.Length - 1);
                    i--;
                }
            }

            outTris.AddRange(tris);
            outEdges.AddRange(edges);
        }
        public static float PolyMinExtent(Vector3[] verts, int nverts)
        {
            float minDist = float.MaxValue;
            for (int i = 0; i < nverts; i++)
            {
                int ni = (i + 1) % nverts;
                Vector3 p1 = verts[i];
                Vector3 p2 = verts[ni];
                float maxEdgeDist = 0;
                for (int j = 0; j < nverts; j++)
                {
                    if (j == i || j == ni) continue;
                    float d = DistancePtSeg2d(verts[j], p1, p2);
                    maxEdgeDist = Math.Max(maxEdgeDist, d);
                }
                minDist = Math.Min(minDist, maxEdgeDist);
            }
            return (float)Math.Sqrt(minDist);
        }
        public static void TriangulateHull(int nverts, Vector3[] verts, int nhull, int[] hull, List<Int4> tris)
        {
            int start = 0, left = 1, right = nhull - 1;

            // Start from an ear with shortest perimeter.
            // This tends to favor well formed triangles as starting point.
            float dmin = 0;
            for (int i = 0; i < nhull; i++)
            {
                int pi = Prev(i, nhull);
                int ni = Next(i, nhull);
                var pv = verts[hull[pi]];
                var cv = verts[hull[i]];
                var nv = verts[hull[ni]];
                float d =
                    Vector2.Distance(new Vector2(pv.X, pv.Z), new Vector2(cv.X, cv.Z)) +
                    Vector2.Distance(new Vector2(cv.X, cv.Z), new Vector2(nv.X, nv.Z)) +
                    Vector2.Distance(new Vector2(nv.X, nv.Z), new Vector2(pv.X, pv.Z));
                if (d < dmin)
                {
                    start = i;
                    left = ni;
                    right = pi;
                    dmin = d;
                }
            }

            // Add first triangle
            tris.Add(new Int4()
            {
                X = hull[start],
                Y = hull[left],
                Z = hull[right],
                W = 0,
            });

            // Triangulate the polygon by moving left or right,
            // depending on which triangle has shorter perimeter.
            // This heuristic was chose emprically, since it seems
            // handle tesselated straight edges well.
            while (Next(left, nhull) != right)
            {
                // Check to see if se should advance left or right.
                int nleft = Next(left, nhull);
                int nright = Prev(right, nhull);

                var cvleft = verts[hull[left]];
                var nvleft = verts[hull[nleft]];
                var cvright = verts[hull[right]];
                var nvright = verts[hull[nright]];
                float dleft =
                    Vector2.Distance(new Vector2(cvleft.X, cvleft.Z), new Vector2(nvleft.X, nvleft.Z)) +
                    Vector2.Distance(new Vector2(nvleft.X, nvleft.Z), new Vector2(cvright.X, cvright.Z));

                float dright =
                    Vector2.Distance(new Vector2(cvright.X, cvright.Z), new Vector2(nvright.X, nvright.Z)) +
                    Vector2.Distance(new Vector2(cvleft.X, cvleft.Z), new Vector2(nvright.X, nvright.Z));

                if (dleft < dright)
                {
                    tris.Add(new Int4()
                    {
                        X = hull[left],
                        Y = hull[nleft],
                        Z = hull[right],
                        W = 0,
                    });

                    left = nleft;
                }
                else
                {
                    tris.Add(new Int4()
                    {
                        X = hull[left],
                        Y = hull[nright],
                        Z = hull[right],
                        W = 0,
                    });

                    right = nright;
                }
            }
        }
        public static float GetJitterX(int i)
        {
            return (((i * 0x8da6b343) & 0xffff) / 65535.0f * 2.0f) - 1.0f;
        }
        public static float GetJitterY(int i)
        {
            return (((i * 0xd8163841) & 0xffff) / 65535.0f * 2.0f) - 1.0f;
        }
        private static bool BuildPolyDetail(Vector3[] inp, int ninp, float sampleDist, float sampleMaxError, int heightSearchRadius, CompactHeightfield chf, HeightPatch hp, Vector3[] verts, out int nverts, out Int4[] outTris)
        {
            List<Int4> edges = new List<Int4>();
            List<Int4> samples = new List<Int4>();
            List<Int4> tris = new List<Int4>();

            int MAX_VERTS = 127;
            int MAX_TRIS = 255;    // Max tris for delaunay is 2n-2-k (n=num verts, k=num hull verts).
            int MAX_VERTS_PER_EDGE = 32;
            Vector3[] edge = new Vector3[(MAX_VERTS_PER_EDGE + 1)];
            int[] hull = new int[MAX_VERTS];
            int nhull = 0;

            nverts = ninp;

            for (int i = 0; i < ninp; ++i)
            {
                verts[i] = inp[i];
            }

            edges.Clear();

            float cs = chf.CS;
            float ics = 1.0f / cs;

            // Calculate minimum extents of the polygon based on input data.
            float minExtent = PolyMinExtent(verts, nverts);

            // Tessellate outlines.
            // This is done in separate pass in order to ensure
            // seamless height values across the ply boundaries.
            if (sampleDist > 0)
            {
                for (int i = 0, j = ninp - 1; i < ninp; j = i++)
                {
                    var vj = inp[j];
                    var vi = inp[i];
                    bool swapped = false;
                    // Make sure the segments are always handled in same order
                    // using lexological sort or else there will be seams.
                    if (Math.Abs(vj[0] - vi[0]) < 1e-6f)
                    {
                        if (vj[2] > vi[2])
                        {
                            Helper.Swap(ref vj, ref vi);
                            swapped = true;
                        }
                    }
                    else
                    {
                        if (vj[0] > vi[0])
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
                    if (nverts + nn >= MAX_VERTS)
                    {
                        nn = MAX_VERTS - 1 - nverts;
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
                        pos.Y = GetHeight(pos.X, pos.Y, pos.Z, cs, ics, chf.CH, heightSearchRadius, hp) * chf.CH;
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
                            float dev = DistancePtSeg(edge[m], va, vb);
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
                            verts[nverts] = edge[idx[k]];
                            hull[nhull++] = nverts;
                            nverts++;
                        }
                    }
                    else
                    {
                        for (int k = 1; k < nidx - 1; ++k)
                        {
                            verts[nverts] = edge[idx[k]];
                            hull[nhull++] = nverts;
                            nverts++;
                        }
                    }
                }
            }

            // If the polygon minimum extent is small (sliver or small triangle), do not try to add internal points.
            if (minExtent < sampleDist * 2)
            {
                TriangulateHull(nverts, verts, nhull, hull, tris);

                outTris = tris.ToArray();

                return true;
            }

            // Tessellate the base mesh.
            // We're using the triangulateHull instead of delaunayHull as it tends to
            // create a bit better triangulation for long thin triangles when there
            // are no internal points.
            TriangulateHull(nverts, verts, nhull, hull, tris);

            if (tris.Count == 0)
            {
                // Could not triangulate the poly, make sure there is some valid data there.
                Console.WriteLine($"buildPolyDetail: Could not triangulate polygon ({nverts} verts).");

                outTris = tris.ToArray();

                return true;
            }

            if (sampleDist > 0)
            {
                // Create sample locations in a grid.
                Vector3 bmin, bmax;
                bmin = inp[0];
                bmax = inp[0];
                for (int i = 1; i < ninp; ++i)
                {
                    bmin = Vector3.Min(bmin, inp[i]);
                    bmax = Vector3.Max(bmax, inp[i]);
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
                        if (DistToPoly(ninp, inp, pt) > -sampleDist / 2) continue;
                        samples.Add(
                            new Int4(
                                x,
                                GetHeight(pt.X, pt.Y, pt.Z, cs, ics, chf.CH, heightSearchRadius, hp),
                                z,
                                0)); // Not added
                    }
                }

                // Add the samples starting from the one that has the most
                // error. The procedure stops when all samples are added
                // or when the max error is within treshold.
                int nsamples = samples.Count;
                for (int iter = 0; iter < nsamples; ++iter)
                {
                    if (nverts >= MAX_VERTS)
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
                        if (s.W != 0) continue; // skip added.
                                                // The sample location is jittered to get rid of some bad triangulations
                                                // which are cause by symmetrical data from the grid structure.
                        Vector3 pt = new Vector3
                        {
                            X = s.X * sampleDist + GetJitterX(i) * cs * 0.1f,
                            Y = s.Y * chf.CH,
                            Z = s.Z * sampleDist + GetJitterY(i) * cs * 0.1f
                        };
                        float d = DistToTriMesh(pt, verts, nverts, tris.ToArray(), tris.Count);
                        if (d < 0) continue; // did not hit the mesh.
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
                    verts[nverts] = bestpt;
                    nverts++;

                    // Create new triangulation.
                    // TODO: Incremental add instead of full rebuild.
                    edges.Clear();
                    tris.Clear();
                    DelaunayHull(nverts, verts, nhull, hull, tris, edges);
                }
            }

            int ntris = tris.Count;
            if (ntris > MAX_TRIS)
            {
                tris.RemoveRange(MAX_TRIS, ntris - MAX_TRIS);
                Console.WriteLine($"rcBuildPolyMeshDetail: Shrinking triangle count from {ntris} to max {MAX_TRIS}.");
            }

            outTris = tris.ToArray();

            return true;
        }
        private static void SeedArrayWithPolyCenter(CompactHeightfield chf, Polygoni poly, int npoly, Int3[] verts, int bs, HeightPatch hp, List<int> array)
        {
            // Note: Reads to the compact heightfield are offset by border size (bs)
            // since border size offset is already removed from the polymesh vertices.

            int[] offset =
            {
                0,0, -1,-1, 0,-1, 1,-1, 1,0, 1,1, 0,1, -1,1, -1,0,
            };

            // Find cell closest to a poly vertex
            int startCellX = 0, startCellY = 0, startSpanIndex = -1;
            int dmin = RC_UNSET_HEIGHT;
            for (int j = 0; j < npoly && dmin > 0; ++j)
            {
                for (int k = 0; k < 9 && dmin > 0; ++k)
                {
                    int ax = verts[poly[j]][0] + offset[k * 2 + 0];
                    int ay = verts[poly[j]][1];
                    int az = verts[poly[j]][2] + offset[k * 2 + 1];
                    if (ax < hp.Xmin || ax >= hp.Xmin + hp.Width ||
                        az < hp.Ymin || az >= hp.Ymin + hp.Height)
                    {
                        continue;
                    }

                    var c = chf.Cells[(ax + bs) + (az + bs) * chf.Width];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni && dmin > 0; ++i)
                    {
                        var s = chf.Spans[i];
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
            for (int j = 0; j < npoly; ++j)
            {
                pcx += verts[poly[j]][0];
                pcy += verts[poly[j]][2];
            }
            pcx /= npoly;
            pcy /= npoly;

            // Use seeds array as a stack for DFS
            array.Clear();
            array.Add(startCellX);
            array.Add(startCellY);
            array.Add(startSpanIndex);

            int[] dirs = { 0, 1, 2, 3 };
            hp.Data = Helper.CreateArray(hp.Width * hp.Height, 0);
            // DFS to move to the center. Note that we need a DFS here and can not just move
            // directly towards the center without recording intermediate nodes, even though the polygons
            // are convex. In very rare we can get stuck due to contour simplification if we do not
            // record nodes.
            int cx = -1, cy = -1, ci = -1;
            while (true)
            {
                if (array.Count < 3)
                {
                    Console.WriteLine("Walk towards polygon center failed to reach center");
                    break;
                }

                ci = array.Pop();
                cy = array.Pop();
                cx = array.Pop();

                if (cx == pcx && cy == pcy)
                {
                    break;
                }

                // If we are already at the correct X-position, prefer direction
                // directly towards the center in the Y-axis; otherwise prefer
                // direction in the X-axis
                int directDir;
                if (cx == pcx)
                {
                    directDir = GetDirForOffset(0, pcy > cy ? 1 : -1);
                }
                else
                {
                    directDir = GetDirForOffset(pcx > cx ? 1 : -1, 0);
                }

                // Push the direct dir last so we start with this on next iteration
                Helper.Swap(ref dirs[directDir], ref dirs[3]);

                var cs = chf.Spans[ci];
                for (int i = 0; i < 4; i++)
                {
                    int dir = dirs[i];
                    if (GetCon(cs, dir) == RC_NOT_CONNECTED)
                    {
                        continue;
                    }

                    int newX = cx + GetDirOffsetX(dir);
                    int newY = cy + GetDirOffsetY(dir);

                    int hpx = newX - hp.Xmin;
                    int hpy = newY - hp.Ymin;
                    if (hpx < 0 || hpx >= hp.Width || hpy < 0 || hpy >= hp.Height)
                    {
                        continue;
                    }

                    if (hp.Data[hpx + hpy * hp.Width] != 0)
                    {
                        continue;
                    }

                    hp.Data[hpx + hpy * hp.Width] = 1;
                    array.Add(newX);
                    array.Add(newY);
                    array.Add(chf.Cells[(newX + bs) + (newY + bs) * chf.Width].Index + GetCon(cs, dir));
                }

                Helper.Swap(ref dirs[directDir], ref dirs[3]);
            }

            array.Clear();
            // getHeightData seeds are given in coordinates with borders
            array.Add(cx + bs);
            array.Add(cy + bs);
            array.Add(ci);

            hp.Data = Helper.CreateArray(hp.Width * hp.Height, 0xff);
            var chs = chf.Spans[ci];
            hp.Data[cx - hp.Xmin + (cy - hp.Ymin) * hp.Width] = chs.Y;
        }
        public static void Push3(List<int> queue, int v1, int v2, int v3)
        {
            queue.Add(v1);
            queue.Add(v2);
            queue.Add(v3);
        }
        private static void GetHeightData(CompactHeightfield chf, Polygoni poly, int npoly, Int3[] verts, int bs, HeightPatch hp, List<int> queue, int region)
        {
            // Note: Reads to the compact heightfield are offset by border size (bs)
            // since border size offset is already removed from the polymesh vertices.

            queue.Clear();
            // Set all heights to RC_UNSET_HEIGHT.
            hp.Data = Helper.CreateArray(hp.Width * hp.Height, RC_UNSET_HEIGHT);

            bool empty = true;

            // We cannot sample from this poly if it was created from polys
            // of different regions. If it was then it could potentially be overlapping
            // with polys of that region and the heights sampled here could be wrong.
            if (region != RC_MULTIPLE_REGS)
            {
                // Copy the height from the same region, and mark region borders
                // as seed points to fill the rest.
                for (int hy = 0; hy < hp.Height; hy++)
                {
                    int y = hp.Ymin + hy + bs;
                    for (int hx = 0; hx < hp.Width; hx++)
                    {
                        int x = hp.Xmin + hx + bs;
                        var c = chf.Cells[x + y * chf.Width];
                        for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                        {
                            var s = chf.Spans[i];
                            if (s.Reg == region)
                            {
                                // Store height
                                hp.Data[hx + hy * hp.Width] = s.Y;
                                empty = false;

                                // If any of the neighbours is not in same region,
                                // add the current location as flood fill start
                                bool border = false;
                                for (int dir = 0; dir < 4; ++dir)
                                {
                                    if (GetCon(s, dir) != RC_NOT_CONNECTED)
                                    {
                                        int ax = x + GetDirOffsetX(dir);
                                        int ay = y + GetDirOffsetY(dir);
                                        int ai = chf.Cells[ax + ay * chf.Width].Index + GetCon(s, dir);
                                        var a = chf.Spans[ai];
                                        if (a.Reg != region)
                                        {
                                            border = true;
                                            break;
                                        }
                                    }
                                }
                                if (border)
                                {
                                    Push3(queue, x, y, i);
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
                SeedArrayWithPolyCenter(chf, poly, npoly, verts, bs, hp, queue);
            }

            int RETRACT_SIZE = 256;
            int head = 0;

            // We assume the seed is centered in the polygon, so a BFS to collect
            // height data will ensure we do not move onto overlapping polygons and
            // sample wrong heights.
            while (head * 3 < queue.Count)
            {
                int cx = queue[head * 3 + 0];
                int cy = queue[head * 3 + 1];
                int ci = queue[head * 3 + 2];
                head++;
                if (head >= RETRACT_SIZE)
                {
                    head = 0;
                    if (queue.Count > RETRACT_SIZE * 3)
                    {
                        queue.RemoveRange(0, RETRACT_SIZE * 3);
                    }
                    queue.Clear();
                }

                var cs = chf.Spans[ci];
                for (int dir = 0; dir < 4; ++dir)
                {
                    if (GetCon(cs, dir) == RC_NOT_CONNECTED)
                    {
                        continue;
                    }

                    int ax = cx + GetDirOffsetX(dir);
                    int ay = cy + GetDirOffsetY(dir);
                    int hx = ax - hp.Xmin - bs;
                    int hy = ay - hp.Ymin - bs;

                    if (hx < 0 || hy < 0 || hx >= hp.Width || hy >= hp.Height)
                    {
                        continue;
                    }

                    if (hp.Data[hx + hy * hp.Width] != RC_UNSET_HEIGHT)
                    {
                        continue;
                    }

                    int ai = chf.Cells[ax + ay * chf.Width].Index + GetCon(cs, dir);
                    var a = chf.Spans[ai];

                    hp.Data[hx + hy * hp.Width] = a.Y;

                    Push3(queue, ax, ay, ai);
                }
            }
        }
        public static int GetEdgeFlags(Vector3 va, Vector3 vb, Vector3[] vpoly, int npoly)
        {
            // Return true if edge (va,vb) is part of the polygon.
            float thrSqr = 0.001f * 0.001f;
            for (int i = 0, j = npoly - 1; i < npoly; j = i++)
            {
                if (DistancePtSeg2d(va, vpoly[j], vpoly[i]) < thrSqr &&
                    DistancePtSeg2d(vb, vpoly[j], vpoly[i]) < thrSqr)
                {
                    return 1;
                }
            }
            return 0;
        }
        public static int GetTriFlags(Vector3 va, Vector3 vb, Vector3 vc, Vector3[] vpoly, int npoly)
        {
            int flags = 0;
            flags |= GetEdgeFlags(va, vb, vpoly, npoly) << 0;
            flags |= GetEdgeFlags(vb, vc, vpoly, npoly) << 2;
            flags |= GetEdgeFlags(vc, va, vpoly, npoly) << 4;
            return flags;
        }
        public static bool BuildPolyMeshDetail(PolyMesh mesh, CompactHeightfield chf, float sampleDist, float sampleMaxError, out PolyMeshDetail dmesh)
        {
            dmesh = null;

            if (mesh.NVerts == 0 || mesh.NPolys == 0)
            {
                return true;
            }

            int nvp = mesh.NVP;
            float cs = mesh.CS;
            float ch = mesh.CH;
            Vector3 orig = mesh.BMin;
            int borderSize = mesh.BorderSize;
            int heightSearchRadius = Math.Max(1, (int)Math.Ceiling(mesh.MaxEdgeError));

            List<int> arr = new List<int>(512);
            Vector3[] verts = new Vector3[256];
            HeightPatch hp = new HeightPatch();
            int nPolyVerts = 0;
            int maxhw = 0, maxhh = 0;

            Int4[] bounds = new Int4[mesh.NPolys];
            Vector3[] poly = new Vector3[nvp];

            // Find max size for a polygon area.
            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];
                int xmin = chf.Width;
                int xmax = 0;
                int ymin = chf.Height;
                int ymax = 0;
                for (int j = 0; j < nvp; ++j)
                {
                    if (p[j] == RC_MESH_NULL_IDX) break;
                    var v = mesh.Verts[p[j]];
                    xmin = Math.Min(xmin, v.X);
                    xmax = Math.Max(xmax, v.X);
                    ymin = Math.Min(ymin, v.Z);
                    ymax = Math.Max(ymax, v.Z);
                    nPolyVerts++;
                }
                xmin = Math.Max(0, xmin - 1);
                xmax = Math.Min(chf.Width, xmax + 1);
                ymin = Math.Max(0, ymin - 1);
                ymax = Math.Min(chf.Height, ymax + 1);
                bounds[i] = new Int4(xmin, xmax, ymin, ymax);
                if (xmin >= xmax || ymin >= ymax) continue;
                maxhw = Math.Max(maxhw, xmax - xmin);
                maxhh = Math.Max(maxhh, ymax - ymin);
            }

            hp.Data = new int[maxhw * maxhh];

            int vcap = nPolyVerts + nPolyVerts / 2;
            int tcap = vcap * 2;

            dmesh = new PolyMeshDetail
            {
                nmeshes = mesh.NPolys,
                meshes = new Int4[mesh.NPolys],
                ntris = 0,
                tris = new Int4[tcap],
                nverts = 0,
                verts = new Vector3[vcap]
            };

            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];

                // Store polygon vertices for processing.
                int npoly = 0;
                for (int j = 0; j < nvp; ++j)
                {
                    if (p[j] == RC_MESH_NULL_IDX) break;
                    var v = mesh.Verts[p[j]];
                    poly[j].X = v.X * cs;
                    poly[j].Y = v.Y * ch;
                    poly[j].Z = v.Z * cs;
                    npoly++;
                }

                // Get the height data from the area of the polygon.
                hp.Xmin = bounds[i].X;
                hp.Ymin = bounds[i].Z;
                hp.Width = bounds[i].Y - bounds[i].X;
                hp.Height = bounds[i].W - bounds[i].Z;
                GetHeightData(chf, p, npoly, mesh.Verts, borderSize, hp, arr, mesh.Regs[i]);

                // Build detail mesh.
                if (!BuildPolyDetail(
                    poly, npoly,
                    sampleDist, sampleMaxError,
                    heightSearchRadius, chf, hp,
                    verts, out int nverts, out Int4[] tris))
                {
                    return false;
                }

                // Move detail verts to world space.
                for (int j = 0; j < nverts; ++j)
                {
                    verts[j].X += orig.X;
                    verts[j].Y += orig.Y + chf.CH; // Is this offset necessary?
                    verts[j].Z += orig.Z;
                }
                // Offset poly too, will be used to flag checking.
                for (int j = 0; j < npoly; ++j)
                {
                    poly[j].X += orig.X;
                    poly[j].Y += orig.Y;
                    poly[j].Z += orig.Z;
                }

                // Store detail submesh.
                int ntris = tris.Length;

                dmesh.meshes[i].X = dmesh.nverts;
                dmesh.meshes[i].Y = nverts;
                dmesh.meshes[i].Z = dmesh.ntris;
                dmesh.meshes[i].W = ntris;

                // Store vertices, allocate more memory if necessary.
                if (dmesh.nverts + nverts > vcap)
                {
                    while (dmesh.nverts + nverts > vcap)
                    {
                        vcap += 256;
                    }

                    Vector3[] newv = new Vector3[vcap];
                    if (dmesh.nverts != 0)
                    {
                        Array.Copy(dmesh.verts, newv, dmesh.nverts);
                    }
                    dmesh.verts = newv;
                }
                for (int j = 0; j < nverts; ++j)
                {
                    dmesh.verts[dmesh.nverts].X = verts[j].X;
                    dmesh.verts[dmesh.nverts].Y = verts[j].Y;
                    dmesh.verts[dmesh.nverts].Z = verts[j].Z;
                    dmesh.nverts++;
                }

                // Store triangles, allocate more memory if necessary.
                if (dmesh.ntris + ntris > tcap)
                {
                    while (dmesh.ntris + ntris > tcap)
                    {
                        tcap += 256;
                    }
                    Int4[] newt = new Int4[tcap];
                    if (dmesh.ntris != 0)
                    {
                        Array.Copy(dmesh.tris, newt, dmesh.ntris);
                    }
                    dmesh.tris = newt;
                }
                for (int j = 0; j < ntris; ++j)
                {
                    var t = tris[j];
                    dmesh.tris[dmesh.ntris].X = t.X;
                    dmesh.tris[dmesh.ntris].Y = t.Y;
                    dmesh.tris[dmesh.ntris].Z = t.Z;
                    dmesh.tris[dmesh.ntris].W = GetTriFlags(verts[t.X], verts[t.Y], verts[t.Z], poly, npoly);
                    dmesh.ntris++;
                }
            }

            return true;
        }
        public static bool MergePolyMeshDetails(PolyMeshDetail[] meshes, int nmeshes, out PolyMeshDetail mesh)
        {
            mesh = new PolyMeshDetail();

            int maxVerts = 0;
            int maxTris = 0;
            int maxMeshes = 0;

            for (int i = 0; i < nmeshes; ++i)
            {
                if (meshes[i] == null)
                {
                    continue;
                }
                maxVerts += meshes[i].nverts;
                maxTris += meshes[i].ntris;
                maxMeshes += meshes[i].nmeshes;
            }

            mesh.nmeshes = 0;
            mesh.meshes = new Int4[maxMeshes];
            mesh.ntris = 0;
            mesh.tris = new Int4[maxTris];
            mesh.nverts = 0;
            mesh.verts = new Vector3[maxVerts];

            // Merge datas.
            for (int i = 0; i < nmeshes; ++i)
            {
                var dm = meshes[i];
                if (dm == null)
                {
                    continue;
                }
                for (int j = 0; j < dm.nmeshes; ++j)
                {
                    var src = dm.meshes[j];

                    var dst = new Int4();
                    dst[0] = mesh.nverts + src[0];
                    dst[1] = src[1];
                    dst[2] = mesh.ntris + src[2];
                    dst[3] = src[3];

                    mesh.meshes[mesh.nmeshes] = dst;
                    mesh.nmeshes++;
                }

                for (int k = 0; k < dm.nverts; ++k)
                {
                    mesh.verts[mesh.nverts] = dm.verts[k];
                    mesh.nverts++;
                }
                for (int k = 0; k < dm.ntris; ++k)
                {
                    mesh.tris[mesh.ntris] = dm.tris[k];
                    mesh.ntris++;
                }
            }

            return true;
        }

        #endregion

        #region COMMON / REPEATED

        /// <summary>
        /// Gets the next index value in a fixed length array
        /// </summary>
        /// <param name="i">Current index</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns the next index</returns>
        public static int Next(int i, int length)
        {
            return i + 1 < length ? i + 1 : 0;
        }
        /// <summary>
        /// Gets the previous index value in a fixed length array
        /// </summary>
        /// <param name="i">Current index</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns the previous index</returns>
        public static int Prev(int i, int length)
        {
            return i - 1 >= 0 ? i - 1 : length - 1;
        }
        public static int Area2(Int4 a, Int4 b, Int4 c)
        {
            return (b.X - a.X) * (c.Z - a.Z) - (c.X - a.X) * (b.Z - a.Z);
        }
        /// <summary>
        /// Exclusive or: true iff exactly one argument is true.
        /// The arguments are negated to ensure that they are 0/1 values.
        /// Then the bitwise Xor operator may apply.
        /// (This idea is due to Michael Baldwin.)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool Xorb(bool x, bool y)
        {
            return !x ^ !y;
        }
        public static bool Left(Int4 aV, Int4 bV, Int4 cV)
        {
            return Area2(aV, bV, cV) < 0;
        }
        public static bool LeftOn(Int4 aV, Int4 bV, Int4 cV)
        {
            return Area2(aV, bV, cV) <= 0;
        }
        public static bool Collinear(Int4 aV, Int4 bV, Int4 cV)
        {
            return Area2(aV, bV, cV) == 0;
        }
        /// <summary>
        /// Returns true iff ab properly intersects cd: they share 
        /// a point interior to both segments.
        /// The properness of the intersection is ensured by using strict leftness.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool IntersectProp(Int4 a, Int4 b, Int4 c, Int4 d)
        {
            // Eliminate improper cases.
            if (Collinear(a, b, c) || Collinear(a, b, d) ||
                Collinear(c, d, a) || Collinear(c, d, b))
                return false;

            return Xorb(Left(a, b, c), Left(a, b, d)) && Xorb(Left(c, d, a), Left(c, d, b));
        }
        /// <summary>
        /// Returns T iff (a,b,c) are collinear and point c lies on the closed segement ab.
        /// </summary>
        /// <param name="aV"></param>
        /// <param name="bV"></param>
        /// <param name="cV"></param>
        /// <returns></returns>
        public static bool Between(Int4 aV, Int4 bV, Int4 cV)
        {
            if (!Collinear(aV, bV, cV))
            {
                return false;
            }

            // If ab not vertical, check betweenness on x; else on y.
            if (aV.X != bV.X)
            {
                return ((aV.X <= cV.X) && (cV.X <= bV.X)) || ((aV.X >= cV.X) && (cV.X >= bV.X));
            }
            else
            {
                return ((aV.Z <= cV.Z) && (cV.Z <= bV.Z)) || ((aV.Z >= cV.Z) && (cV.Z >= bV.Z));
            }
        }
        /// <summary>
        /// Returns true iff segments ab and cd intersect, properly or improperly.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool Intersect(Int4 a, Int4 b, Int4 c, Int4 d)
        {
            if (IntersectProp(a, b, c, d))
                return true;
            else if (Between(a, b, c) || Between(a, b, d) ||
                     Between(c, d, a) || Between(c, d, b))
                return true;
            else
                return false;
        }
        public static bool Vequal(Int4 a, Int4 b)
        {
            return a.X == b.X && a.Z == b.Z;
        }
        public static bool Diagonalie(int i, int j, int n, Int4[] verts, int[] indices)
        {
            var d0 = verts[(indices[i] & 0x7fff)];
            var d1 = verts[(indices[j] & 0x7fff)];

            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = Next(k, n);
                // Skip edges incident to i or j
                if (!((k == i) || (k1 == i) || (k == j) || (k1 == j)))
                {
                    var p0 = verts[(indices[k] & 0x7fff)];
                    var p1 = verts[(indices[k1] & 0x7fff)];

                    if (Vequal(d0, p0) || Vequal(d1, p0) || Vequal(d0, p1) || Vequal(d1, p1))
                        continue;

                    if (Intersect(d0, d1, p0, p1))
                        return false;
                }
            }
            return true;
        }
        public static bool InCone(int i, int j, int n, Int4[] verts, int[] indices)
        {
            var pi = verts[(indices[i] & 0x7fff)];
            var pj = verts[(indices[j] & 0x7fff)];
            var pi1 = verts[(indices[Next(i, n)] & 0x7fff)];
            var pin1 = verts[(indices[Prev(i, n)] & 0x7fff)];

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn(pin1, pi, pi1))
            {
                return Left(pi, pj, pin1) && Left(pj, pi, pi1);
            }
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn(pi, pj, pi1) && LeftOn(pj, pi, pin1));
        }
        public static bool Diagonal(int i, int j, int n, Int4[] verts, int[] indices)
        {
            return InCone(i, j, n, verts, indices) && Diagonalie(i, j, n, verts, indices);
        }
        public static bool InCone(int i, int n, Int4[] verts, Int4 pj)
        {
            var pi = verts[i];
            var pi1 = verts[Next(i, n)];
            var pin1 = verts[Prev(i, n)];

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn(pin1, pi, pi1))
            {
                return Left(pi, pj, pin1) && Left(pj, pi, pi1);
            }
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn(pi, pj, pi1) && LeftOn(pj, pi, pin1));
        }

        #endregion
    }
}
