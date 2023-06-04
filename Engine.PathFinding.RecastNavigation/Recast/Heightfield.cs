using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    class Heightfield
    {
        /// <summary>
        /// Builds a new empty heightfield
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="bbox">Bounds</param>
        /// <param name="cellSize">Cell size</param>
        /// <param name="cellHeight">Cell height</param>
        /// <returns>Returns a new heightfield</returns>
        public static Heightfield Build(int width, int height, BoundingBox bbox, float cellSize, float cellHeight)
        {
            return new Heightfield
            {
                Width = width,
                Height = height,
                BoundingBox = bbox,
                CellSize = cellSize,
                CellHeight = cellHeight,
                Spans = new Span[width * height],
            };
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
        public Span FreeList;

        /// <summary>
        /// Gets the span count
        /// </summary>
        /// <returns>Returns the span count</returns>
        public int GetSpanCount()
        {
            int w = Width;
            int h = Height;

            int spanCount = 0;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    for (Span s = Spans[x + y * w]; s != null; s = s.next)
                    {
                        if (s.area != AreaTypes.RC_NULL_AREA)
                        {
                            spanCount++;
                        }
                    }
                }
            }

            return spanCount;
        }
        /// <summary>
        /// Allocates a new span
        /// </summary>
        /// <returns>Returns the new span</returns>
        public Span AllocSpan()
        {
            // If running out of memory, allocate new page and update the freelist.
            if (FreeList == null || FreeList.next == null)
            {
                // Create new page.
                // Allocate memory for the new pool.
                SpanPool pool = new SpanPool
                {
                    // Add the pool into the list of pools.
                    next = Pools.Count > 0 ? Pools[^1] : null
                };
                Pools.Add(pool);
                // Add new items to the free list.
                Span freelist = FreeList;
                int itIndex = SpanPool.RC_SPANS_PER_POOL;
                do
                {
                    var it = pool.items[--itIndex];
                    it.next = freelist;
                    freelist = it;
                }
                while (itIndex > 0);
                FreeList = pool.items[itIndex];
            }

            // Pop item from in front of the free list.
            Span s = FreeList;
            FreeList = FreeList.next;
            return s;
        }
        /// <summary>
        /// Frees the specified span
        /// </summary>
        /// <param name="cur">Span</param>
        public void FreeSpan(Span cur)
        {
            if (cur == null) return;

            // Add the node in front of the free list.
            cur.next = FreeList;
            FreeList = cur;
        }
        /// <summary>
        /// Adds a span to the heightfield
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="smin">Lower limit</param>
        /// <param name="smax">Upper limit</param>
        /// <param name="area">Area type</param>
        /// <param name="flagMergeThr">Merge threshold</param>
        public void AddSpan(int x, int y, int smin, int smax, AreaTypes area, int flagMergeThr)
        {
            int idx = x + y * Width;

            Span s = AllocSpan();
            s.smin = smin;
            s.smax = smax;
            s.area = area;
            s.next = null;

            // Empty cell, add the first span.
            if (Spans[idx] == null)
            {
                Spans[idx] = s;
                return;
            }
            Span prev = null;
            Span cur = Spans[idx];

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
                        s.area = (AreaTypes)Math.Max((int)s.area, (int)cur.area);
                    }

                    // Remove current span.
                    Span next = cur.next;
                    FreeSpan(cur);
                    if (prev != null)
                    {
                        prev.next = next;
                    }
                    else
                    {
                        Spans[idx] = next;
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
                s.next = Spans[idx];
                Spans[idx] = s;
            }
        }

        /// <summary>
        /// Filters the low-hanging obstables
        /// </summary>
        /// <param name="walkableClimb">Walkable climb</param>
        public void FilterLowHangingWalkableObstacles(int walkableClimb)
        {
            int w = Width;
            int h = Height;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool previousWalkable = false;
                    AreaTypes previousArea = AreaTypes.RC_NULL_AREA;

                    Span ps = null;

                    for (Span s = Spans[x + y * w]; s != null; ps = s, s = s.next)
                    {
                        bool walkable = s.area != AreaTypes.RC_NULL_AREA;

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
        /// <summary>
        /// Filtes the ledge spans
        /// </summary>
        /// <param name="walkableHeight">Walkable height</param>
        /// <param name="walkableClimb">Walkable climb</param>
        public void FilterLedgeSpans(int walkableHeight, int walkableClimb)
        {
            // Mark border spans.
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    for (Span s = Spans[x + y * Width]; s != null; s = s.next)
                    {
                        // Skip non walkable spans.
                        if (s.area == AreaTypes.RC_NULL_AREA)
                        {
                            continue;
                        }

                        FilterLedgeSpan(s, x, y, Width, Height, walkableHeight, walkableClimb);
                    }
                }
            }
        }
        private void FilterLedgeSpan(Span span, int x, int y, int width, int height, int walkableHeight, int walkableClimb)
        {
            // Find neighbours minimum height.
            int minh = int.MaxValue;

            // Min and max height of accessible neighbours.
            int asmin = span.smax;
            int asmax = span.smax;

            for (int dir = 0; dir < 4; ++dir)
            {
                // Skip neighbours which are out of bounds.
                int dx = x + RecastUtils.GetDirOffsetX(dir);
                int dy = y + RecastUtils.GetDirOffsetY(dir);
                if (dx < 0 || dy < 0 || dx >= width || dy >= height)
                {
                    minh = Math.Min(minh, -walkableClimb - span.smax);
                    continue;
                }

                var ns = Spans[dx + dy * width];
                var (MinHeight, AsMin, AsMax) = FindMinimumHeight(ns, span, walkableHeight, walkableClimb, minh, asmin, asmax);
                minh = MinHeight;
                asmin = AsMin;
                asmax = AsMax;
            }

            if (minh < -walkableClimb)
            {
                // The current span is close to a ledge if the drop to any neighbour span is less than the walkableClimb.
                span.area = AreaTypes.RC_NULL_AREA;
            }
            else if ((asmax - asmin) > walkableClimb)
            {
                // If the difference between all neighbours is too large, we are at steep slope, mark the span as ledge.
                span.area = AreaTypes.RC_NULL_AREA;
            }
        }
        private static (int MinHeight, int AsMin, int AsMax) FindMinimumHeight(Span nSpan, Span span, int walkableHeight, int walkableClimb, int minHeight, int minimumAccessibleNeigbor, int maximumAccessibleNeigbor)
        {
            int minh = minHeight;
            int asmin = minimumAccessibleNeigbor;
            int asmax = maximumAccessibleNeigbor;

            int bot = span.smax;
            int top = span.next != null ? span.next.smin : int.MaxValue;

            // From minus infinity to the first span.
            int nbot = -walkableClimb;
            int ntop = nSpan != null ? nSpan.smin : int.MaxValue;

            // Skip neightbour if the gap between the spans is too small.
            if (Math.Min(top, ntop) - Math.Max(bot, nbot) > walkableHeight)
            {
                minh = Math.Min(minh, nbot - bot);
            }

            var ns = nSpan;
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

            return (minh, asmin, asmax);
        }
        /// <summary>
        /// Filters the low-height spans
        /// </summary>
        /// <param name="walkableHeight">Walkable height</param>
        public void FilterWalkableLowHeightSpans(int walkableHeight)
        {
            int w = Width;
            int h = Height;

            // Remove walkable flag from spans which do not have enough space above them for the agent to stand there.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    for (var s = Spans[x + y * w]; s != null; s = s.next)
                    {
                        int bot = s.smax;
                        int top = s.next != null ? s.next.smin : int.MaxValue;

                        if ((top - bot) <= walkableHeight)
                        {
                            s.area = AreaTypes.RC_NULL_AREA;
                        }
                    }
                }
            }
        }
    }
}
