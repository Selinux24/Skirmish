using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    class Heightfield
    {
        public static Heightfield Build(int width, int height, BoundingBox bbox, float cs, float ch)
        {
            return new Heightfield
            {
                Width = width,
                Height = height,
                BoundingBox = bbox,
                CellSize = cs,
                CellHeight = ch,
                Spans = new Span[width * height],
            };
        }
        public static void DividePoly(List<Vector3> inPoly, float x, int axis, List<Vector3> outPoly1, List<Vector3> outPoly2)
        {
            float[] d = PrepareAxisList(inPoly, x, axis);

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
        private static float[] PrepareAxisList(List<Vector3> inPoly, float x, int axis)
        {
            float[] d = new float[inPoly.Count];
            for (int i = 0; i < inPoly.Count; i++)
            {
                d[i] = x - inPoly[i][axis];
            }

            return d;
        }

        /// <summary>
        /// The width of the heightfield. (Along the x-axis in cell units.)
        /// </summary>
        public int Width;
        /// <summary>
        /// The height of the heightfield. (Along the z-axis in cell units.)
        /// </summary>
        public int Height;
        /// <summary>
        /// Bounds in world space. [(x, y, z)]
        /// </summary>
        public BoundingBox BoundingBox;
        /// <summary>
        /// The size of each cell. (On the xz-plane.)
        /// </summary>
        public float CellSize;
        /// <summary>
        /// The height of each cell. (The minimum increment along the y-axis.)
        /// </summary>
        public float CellHeight;
        /// <summary>
        /// Heightfield of spans (width*height).
        /// </summary>
        public Span[] Spans;
        /// <summary>
        /// Linked list of span pools.
        /// </summary>
        public List<SpanPool> Pools = new List<SpanPool>();
        /// <summary>
        /// The next free span.
        /// </summary>
        public Span Freelist;


        public void FilterLowHangingWalkableObstacles(int walkableClimb)
        {
            int w = this.Width;
            int h = this.Height;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool previousWalkable = false;
                    AreaTypes previousArea = AreaTypes.Unwalkable;

                    Span ps = null;

                    for (Span s = this.Spans[x + y * w]; s != null; ps = s, s = s.Next)
                    {
                        bool walkable = s.Area != AreaTypes.Unwalkable;

                        // If current span is not walkable, but there is walkable span just below it, mark the span above it walkable too.
                        if (!walkable && previousWalkable && Math.Abs(s.SMax - ps.SMax) <= walkableClimb)
                        {
                            s.Area = previousArea;
                        }

                        // Copy walkable flag so that it cannot propagate past multiple non-walkable objects.
                        previousWalkable = walkable;
                        previousArea = s.Area;
                    }
                }
            }
        }

        public void FilterLedgeSpans(int walkableHeight, int walkableClimb)
        {
            int w = this.Width;
            int h = this.Height;

            // Mark border spans.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    for (Span s = this.Spans[x + y * w]; s != null; s = s.Next)
                    {
                        // Skip non walkable spans.
                        if (s.Area == AreaTypes.Unwalkable)
                        {
                            continue;
                        }

                        int bot = s.SMax;
                        int top = s.Next != null ? s.Next.SMin : int.MaxValue;

                        // Find neighbours minimum height.
                        int minh = int.MaxValue;

                        // Min and max height of accessible neighbours.
                        int asmin = s.SMax;
                        int asmax = s.SMax;

                        for (int dir = 0; dir < 4; ++dir)
                        {
                            // Skip neighbours which are out of bounds.
                            int dx = x + RecastUtils.GetDirOffsetX(dir);
                            int dy = y + RecastUtils.GetDirOffsetY(dir);
                            if (dx < 0 || dy < 0 || dx >= w || dy >= h)
                            {
                                minh = Math.Min(minh, -walkableClimb - bot);
                                continue;
                            }

                            // From minus infinity to the first span.
                            var ns = this.Spans[dx + dy * w];
                            int nbot = -walkableClimb;
                            int ntop = ns != null ? ns.SMin : int.MaxValue;

                            // Skip neightbour if the gap between the spans is too small.
                            if (Math.Min(top, ntop) - Math.Max(bot, nbot) > walkableHeight)
                            {
                                minh = Math.Min(minh, nbot - bot);
                            }

                            // Rest of the spans.
                            ns = this.Spans[dx + dy * w];
                            while (ns != null)
                            {
                                nbot = ns.SMax;
                                ntop = ns.Next != null ? ns.Next.SMin : int.MaxValue;

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

                                ns = ns.Next;
                            }
                        }

                        if (minh < -walkableClimb)
                        {
                            // The current span is close to a ledge if the drop to any neighbour span is less than the walkableClimb.
                            s.Area = AreaTypes.Unwalkable;
                        }
                        else if ((asmax - asmin) > walkableClimb)
                        {
                            // If the difference between all neighbours is too large, we are at steep slope, mark the span as ledge.
                            s.Area = AreaTypes.Unwalkable;
                        }
                    }
                }
            }
        }

        public void FilterWalkableLowHeightSpans(int walkableHeight)
        {
            int w = this.Width;
            int h = this.Height;

            // Remove walkable flag from spans which do not have enough space above them for the agent to stand there.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    for (var s = this.Spans[x + y * w]; s != null; s = s.Next)
                    {
                        int bot = s.SMax;
                        int top = s.Next != null ? s.Next.SMin : int.MaxValue;

                        if ((top - bot) <= walkableHeight)
                        {
                            s.Area = AreaTypes.Unwalkable;
                        }
                    }
                }
            }
        }

        public int GetSpanCount()
        {
            int w = this.Width;
            int h = this.Height;

            int spanCount = 0;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    for (Span s = this.Spans[x + y * w]; s != null; s = s.Next)
                    {
                        if (s.Area != AreaTypes.Unwalkable)
                        {
                            spanCount++;
                        }
                    }
                }
            }

            return spanCount;
        }

        public Span AllocSpan()
        {
            // If running out of memory, allocate new page and update the freelist.
            if (this.Freelist == null || this.Freelist.Next == null)
            {
                // Create new page.
                // Allocate memory for the new pool.
                SpanPool pool = new SpanPool
                {
                    // Add the pool into the list of pools.
                    next = this.Pools.Count > 0 ? this.Pools.Last() : null
                };
                this.Pools.Add(pool);
                // Add new items to the free list.
                Span flist = this.Freelist;
                int itIndex = RecastUtils.RC_SPANS_PER_POOL;
                do
                {
                    var it = pool.items[--itIndex];
                    it.Next = flist;
                    flist = it;
                }
                while (itIndex > 0);
                this.Freelist = pool.items[itIndex];
            }

            // Pop item from in front of the free list.
            Span s = this.Freelist;
            this.Freelist = this.Freelist.Next;
            return s;
        }

        public void FreeSpan(Span cur)
        {
            if (cur == null) return;

            // Add the node in front of the free list.
            cur.Next = this.Freelist;
            this.Freelist = cur;
        }

        public bool AddSpan(int x, int y, int smin, int smax, AreaTypes area, int flagMergeThr)
        {
            int idx = x + y * this.Width;

            Span s = AllocSpan();
            s.SMin = smin;
            s.SMax = smax;
            s.Area = area;
            s.Next = null;

            // Empty cell, add the first span.
            if (this.Spans[idx] == null)
            {
                this.Spans[idx] = s;
                return true;
            }
            Span prev = null;
            Span cur = this.Spans[idx];

            // Insert and merge spans.
            while (cur != null)
            {
                if (cur.SMin > s.SMax)
                {
                    // Current span is further than the new span, break.
                    break;
                }
                else if (cur.SMax < s.SMin)
                {
                    // Current span is before the new span advance.
                    prev = cur;
                    cur = cur.Next;
                }
                else
                {
                    // Merge spans.
                    if (cur.SMin < s.SMin)
                    {
                        s.SMin = cur.SMin;
                    }
                    if (cur.SMax > s.SMax)
                    {
                        s.SMax = cur.SMax;
                    }

                    // Merge flags.
                    if (Math.Abs(s.SMax - cur.SMax) <= flagMergeThr)
                    {
                        s.Area = (AreaTypes)Math.Max((int)s.Area, (int)cur.Area);
                    }

                    // Remove current span.
                    Span next = cur.Next;
                    FreeSpan(cur);
                    if (prev != null)
                    {
                        prev.Next = next;
                    }
                    else
                    {
                        this.Spans[idx] = next;
                    }

                    cur = next;
                }
            }

            // Insert new span.
            if (prev != null)
            {
                s.Next = prev.Next;
                prev.Next = s;
            }
            else
            {
                s.Next = this.Spans[idx];
                this.Spans[idx] = s;
            }

            return true;
        }

        public bool RasterizeTriangle(int flagMergeThr, RasterizeTri tri)
        {
            float cellSize = this.CellSize;
            float ics = 1.0f / this.CellSize;
            float ich = 1.0f / this.CellHeight;
            int w = this.Width;
            int h = this.Height;
            var b = this.BoundingBox;
            float by = b.GetY();

            // Calculate the bounding box of the triangle.
            var t = BoundingBox.FromPoints(tri.Tri.GetVertices());

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
            List<Vector3> inb = new List<Vector3>(tri.Tri.GetVertices());
            List<Vector3> zp1 = new List<Vector3>();
            List<Vector3> zp2 = new List<Vector3>();
            List<Vector3> xp1 = new List<Vector3>();
            List<Vector3> xp2 = new List<Vector3>();

            for (int y = y0; y <= y1; ++y)
            {
                // Clip polygon to row. Store the remaining polygon as well
                zp1.Clear();
                zp2.Clear();
                float cz = b.Minimum.Z + y * cellSize;
                DividePoly(inb, cz + cellSize, 2, zp1, zp2);
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
                    float cx = b.Minimum.X + x * cellSize;
                    DividePoly(zp1, cx + cellSize, 0, xp1, xp2);
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

                    if (!AddSpan(x, y, ismin, ismax, tri.Area, flagMergeThr))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool RasterizeTriangles(int flagMergeThr, IEnumerable<RasterizeTri> tris)
        {
            // Rasterize triangles.
            foreach (var tri in tris)
            {
                // Rasterize.
                if (!RasterizeTriangle(flagMergeThr, tri))
                {
                    throw new EngineException("rcRasterizeTriangles: Out of memory.");
                }
            }

            return true;
        }
    }

    public struct RasterizeTri
    {
        public Triangle Tri { get; set; }
        public AreaTypes Area { get; set; }
    }
}
