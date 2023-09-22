using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    class Heightfield
    {
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
        public List<SpanPool> Pools = new();
        /// <summary>
        /// The next free span.
        /// </summary>
        public Span FreeList;

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
        private static (int MinHeight, int AsMin, int AsMax) FindMinimumHeight(Span nSpan, Span span, int walkableHeight, int walkableClimb, int minHeight, int minimumAccessibleNeigbor, int maximumAccessibleNeigbor)
        {
            int minh = minHeight;
            int asmin = minimumAccessibleNeigbor;
            int asmax = maximumAccessibleNeigbor;

            int bot = span.SMax;
            int top = span.Next != null ? span.Next.SMin : int.MaxValue;

            // From minus infinity to the first span.
            int nbot = -walkableClimb;
            int ntop = nSpan != null ? nSpan.SMin : int.MaxValue;

            // Skip neightbour if the gap between the spans is too small.
            if (Math.Min(top, ntop) - Math.Max(bot, nbot) > walkableHeight)
            {
                minh = Math.Min(minh, nbot - bot);
            }

            var ns = nSpan;
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

            return (minh, asmin, asmax);
        }

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
                    for (Span s = Spans[x + y * w]; s != null; s = s.Next)
                    {
                        if (s.Area != AreaTypes.RC_NULL_AREA)
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
            if (FreeList == null || FreeList.Next == null)
            {
                // Create new page.
                var pool = new SpanPool();
                Pools.Add(pool);

                // Add new items to the free list.
                FreeList = pool.Add(FreeList);
            }

            // Pop item from in front of the free list.
            var s = FreeList;
            FreeList = FreeList.Next;
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
            cur.Next = FreeList;
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
            s.SMin = smin;
            s.SMax = smax;
            s.Area = area;
            s.Next = null;

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
                        Spans[idx] = next;
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
                s.Next = Spans[idx];
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

                    for (Span s = Spans[x + y * w]; s != null; ps = s, s = s.Next)
                    {
                        bool walkable = s.Area != AreaTypes.RC_NULL_AREA;

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
                    for (Span s = Spans[x + y * Width]; s != null; s = s.Next)
                    {
                        // Skip non walkable spans.
                        if (s.Area == AreaTypes.RC_NULL_AREA)
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
            int asmin = span.SMax;
            int asmax = span.SMax;

            for (int dir = 0; dir < 4; ++dir)
            {
                // Skip neighbours which are out of bounds.
                int dx = x + ContourSet.GetDirOffsetX(dir);
                int dy = y + ContourSet.GetDirOffsetY(dir);
                if (dx < 0 || dy < 0 || dx >= width || dy >= height)
                {
                    minh = Math.Min(minh, -walkableClimb - span.SMax);
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
                span.Area = AreaTypes.RC_NULL_AREA;
            }
            else if ((asmax - asmin) > walkableClimb)
            {
                // If the difference between all neighbours is too large, we are at steep slope, mark the span as ledge.
                span.Area = AreaTypes.RC_NULL_AREA;
            }
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
                    for (var s = Spans[x + y * w]; s != null; s = s.Next)
                    {
                        int bot = s.SMax;
                        int top = s.Next != null ? s.Next.SMin : int.MaxValue;

                        if ((top - bot) <= walkableHeight)
                        {
                            s.Area = AreaTypes.RC_NULL_AREA;
                        }
                    }
                }
            }
        }
    }
}
