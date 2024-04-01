using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Height field
    /// </summary>
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
        public List<SpanPool> Pools = [];
        /// <summary>
        /// The next free span.
        /// </summary>
        public Span FreeList;

        /// <summary>
        /// Builds a new empty heightfield
        /// </summary>
        /// <param name="cfg">Config</param>
        /// <returns>Returns a new heightfield</returns>
        public static Heightfield Build(Config cfg)
        {
            return new()
            {
                Width = cfg.Width,
                Height = cfg.Height,
                BoundingBox = cfg.BoundingBox,
                CellSize = cfg.CellSize,
                CellHeight = cfg.CellHeight,
                Spans = new Span[cfg.Width * cfg.Height],
            };
        }

        /// <summary>
        /// Rasterizes the specified triangle list
        /// </summary>
        /// <param name="tris">Triangle list</param>
        /// <param name="walkableSlopeAngle">Slope angle</param>
        /// <param name="walkableClimb">Maximum climb</param>
        public void Rasterize(Triangle[] tris, float walkableSlopeAngle, int walkableClimb)
        {
            // Rasterizer settings
            RasterizerSettings settings = new()
            {
                WalkableSlopeAngle = walkableSlopeAngle,
                WalkableClimb = walkableClimb,
                Width = Width,
                Height = Height,
                CellSize = CellSize,
                CellHeight = CellHeight,
                Bounds = BoundingBox,
            };

            // Rasterize triangles.
            var rData = Rasterizer.Rasterize(tris, settings);
            foreach (var r in rData)
            {
                AddSpan(r);
            }
        }

        /// <summary>
        /// Iterates over the span list
        /// </summary>
        public IEnumerable<(int x, int y, Span span)> IterateSpans()
        {
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    var span = Spans[x + y * Width];
                    if (span == null)
                    {
                        // If there are no spans at this cell, just leave the data to index=0, count=0.
                        continue;
                    }

                    yield return (x, y, span);
                }
            }
        }
        /// <summary>
        /// Iterates over the span list and it's next span
        /// </summary>
        public IEnumerable<(int x, int y, Span span)> IterateSpansWithNexts()
        {
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    for (var s = Spans[x + y * Width]; s != null; s = s.Next)
                    {
                        yield return (x, y, s);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the span count
        /// </summary>
        /// <returns>Returns the span count</returns>
        public int GetSpanCount()
        {
            int spanCount = 0;

            foreach (var (x, y, s) in IterateSpansWithNexts())
            {
                // Skip non walkable spans.
                if (s.Area == AreaTypes.RC_NULL_AREA)
                {
                    continue;
                }

                spanCount++;
            }

            return spanCount;
        }
        /// <summary>
        /// Allocates a new span
        /// </summary>
        /// <param name="smin">Lower limit</param>
        /// <param name="smax">Upper limit</param>
        /// <param name="area">Area type</param>
        /// <returns>Returns the new span</returns>
        private Span AllocSpan(int smin, int smax, AreaTypes area)
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
            s.Initialize(smin, smax, area);

            return s;
        }
        /// <summary>
        /// Frees the specified span
        /// </summary>
        /// <param name="cur">Span</param>
        private void FreeSpan(Span cur)
        {
            if (cur == null) return;

            // Add the node in front of the free list.
            cur.Next = FreeList;
            FreeList = cur;
        }
        /// <summary>
        /// Adds a span to the heightfield
        /// </summary>
        /// <param name="data">Rasterize data</param>
        public void AddSpan(RasterizeData data)
        {
            int x = data.X;
            int y = data.Y;
            int smin = data.SMin;
            int smax = data.SMax;
            AreaTypes area = data.Area;
            int flagMergeThr = data.FlagMergeThr;

            int idx = x + y * Width;

            Span s = AllocSpan(smin, smax, area);

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
                if (cur.Min > s.Max)
                {
                    // Current span is further than the new span, break.
                    break;
                }

                if (cur.Max < s.Min)
                {
                    // Current span is before the new span advance.
                    prev = cur;
                    cur = cur.Next;

                    continue;
                }

                // Merge spans.
                s.Merge(cur, flagMergeThr);

                // Remove current span.
                var next = cur.Next;
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
        /// Filter heightfield
        /// </summary>
        /// <param name="cfg">Configuration</param>
        public void FilterHeightfield(Config cfg)
        {
            // Once all geometry is rasterized, we do initial pass of filtering to
            // remove unwanted overhangs caused by the conservative rasterization
            // as well as filter spans where the character cannot possibly stand.
            if (cfg.FilterLowHangingObstacles)
            {
                FilterLowHangingWalkableObstacles(cfg.WalkableClimb);
            }
            if (cfg.FilterLedgeSpans)
            {
                FilterLedgeSpans(cfg.WalkableHeight, cfg.WalkableClimb);
            }
            if (cfg.FilterWalkableLowHeightSpans)
            {
                FilterWalkableLowHeightSpans(cfg.WalkableHeight);
            }
        }
        /// <summary>
        /// Filters the low-hanging obstables
        /// </summary>
        /// <param name="walkableClimb">Walkable climb</param>
        private void FilterLowHangingWalkableObstacles(int walkableClimb)
        {
            foreach (var (_, _, span) in IterateSpans())
            {
                bool previousWalkable = false;
                AreaTypes previousArea = AreaTypes.RC_NULL_AREA;

                Span ps = null;
                for (Span s = span; s != null; ps = s, s = s.Next)
                {
                    bool walkable = s.Area != AreaTypes.RC_NULL_AREA;

                    // If current span is not walkable, but there is walkable span just below it, mark the span above it walkable too.
                    if (!walkable && previousWalkable && Math.Abs(s.Max - ps.Max) <= walkableClimb)
                    {
                        s.Area = previousArea;
                    }

                    // Copy walkable flag so that it cannot propagate past multiple non-walkable objects.
                    previousWalkable = walkable;
                    previousArea = s.Area;
                }
            }
        }
        /// <summary>
        /// Filtes the ledge spans
        /// </summary>
        /// <param name="walkableHeight">Walkable height</param>
        /// <param name="walkableClimb">Walkable climb</param>
        private void FilterLedgeSpans(int walkableHeight, int walkableClimb)
        {
            // Mark border spans.
            foreach (var (x, y, s) in IterateSpansWithNexts())
            {
                // Skip non walkable spans.
                if (s.Area == AreaTypes.RC_NULL_AREA)
                {
                    continue;
                }

                FilterLedgeSpan(s, x, y, Width, Height, walkableHeight, walkableClimb);
            }
        }
        /// <summary>
        /// Filter ledge span
        /// </summary>
        /// <param name="span">Span</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="width">Span width</param>
        /// <param name="height">Span height</param>
        /// <param name="walkableHeight">Walkable height</param>
        /// <param name="walkableClimb">Walkable climb</param>
        private void FilterLedgeSpan(Span span, int x, int y, int width, int height, int walkableHeight, int walkableClimb)
        {
            // Find neighbours minimum height.
            int minh = int.MaxValue;

            // Min and max height of accessible neighbours.
            int asmin = span.Max;
            int asmax = span.Max;

            for (int dir = 0; dir < 4; ++dir)
            {
                // Skip neighbours which are out of bounds.
                int dx = x + GridUtils.GetDirOffsetX(dir);
                int dy = y + GridUtils.GetDirOffsetY(dir);
                if (dx < 0 || dy < 0 || dx >= width || dy >= height)
                {
                    minh = Math.Min(minh, -walkableClimb - span.Max);
                    continue;
                }

                var ns = Spans[dx + dy * width];
                var (MinHeight, AsMin, AsMax) = span.FindMinimumHeight(ns, walkableHeight, walkableClimb, minh, asmin, asmax);
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
        private void FilterWalkableLowHeightSpans(int walkableHeight)
        {
            // Remove walkable flag from spans which do not have enough space above them for the agent to stand there.
            foreach (var (_, _, s) in IterateSpansWithNexts())
            {
                // Skip non walkable spans.
                if (s.Area == AreaTypes.RC_NULL_AREA)
                {
                    continue;
                }

                int bot = s.Max;
                int top = s.Next?.Min ?? int.MaxValue;

                if ((top - bot) <= walkableHeight)
                {
                    s.Area = AreaTypes.RC_NULL_AREA;
                }
            }
        }
    }
}
