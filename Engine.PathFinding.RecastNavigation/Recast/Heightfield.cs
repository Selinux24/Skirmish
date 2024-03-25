using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Height field
    /// </summary>
    class Heightfield
    {
        /// <summary>
        /// Rasterize item
        /// </summary>
        struct RasterizeItem
        {
            /// <summary>
            /// Triangle
            /// </summary>
            public Triangle Triangle { get; set; }
            /// <summary>
            /// Area type
            /// </summary>
            public AreaTypes AreaType { get; set; }

            /// <inheritdoc/>
            public override readonly string ToString()
            {
                return $"{AreaType} => {Triangle}";
            }
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
            return new Heightfield
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
        /// Marks a walkable triangle list
        /// </summary>
        /// <param name="walkableSlopeAngle">Slope angle</param>
        /// <param name="tris">Triangle list</param>
        /// <returns>Returns a rasterize item collection</returns>
        private static RasterizeItem[] MarkWalkableTriangles(float walkableSlopeAngle, Triangle[] tris)
        {
            RasterizeItem[] res = new RasterizeItem[tris.Length];

            float walkableThr = (float)Math.Cos(walkableSlopeAngle / 180.0f * MathUtil.Pi);

            for (int t = 0; t < tris.Length; t++)
            {
                var tri = tris[t];

                // Check if the face is walkable.
                var area = tri.Normal.Y > walkableThr ? AreaTypes.RC_WALKABLE_AREA : AreaTypes.RC_NULL_AREA;

                res[t] = new() { Triangle = tri, AreaType = area };
            }

            return res;
        }
        /// <summary>
        /// Gets whether the span (min, max) is outside the specified box size
        /// </summary>
        /// <param name="min">Min size</param>
        /// <param name="max">Max size</param>
        /// <param name="size">Size</param>
        private static bool SpanOutsideBBox(float min, float max, float size)
        {
            if (max < 0.0f) return true;
            if (min > size) return true;

            return false;
        }
        /// <summary>
        /// Clamps the span (min, max) into the box size
        /// </summary>
        /// <param name="min">Min size</param>
        /// <param name="max">Max size</param>
        /// <param name="size">Size</param>
        private static void SpanClamp(ref float min, ref float max, float size)
        {
            if (min < 0.0f) min = 0;
            if (max > size) max = size;
        }
        /// <summary>
        /// Calculates the span x sizes
        /// </summary>
        /// <param name="spanVertices">Vertex list</param>
        private static (float MinX, float MaxX) CalculateSpanMinMaxX(Vector3[] spanVertices)
        {
            float minX = spanVertices[0].X;
            float maxX = spanVertices[0].X;

            for (int i = 1; i < spanVertices.Length; i++)
            {
                minX = Math.Min(minX, spanVertices[i].X);
                maxX = Math.Max(maxX, spanVertices[i].X);
            }

            return (minX, maxX);
        }
        /// <summary>
        /// Calculates the span y sizes
        /// </summary>
        /// <param name="spanVertices">Vertex list</param>
        private static (float MinY, float MaxY) CalculateSpanMinMaxY(Vector3[] spanVertices)
        {
            float minY = spanVertices[0].Y;
            float maxY = spanVertices[0].Y;

            for (int i = 1; i < spanVertices.Length; ++i)
            {
                minY = Math.Min(minY, spanVertices[i].Y);
                maxY = Math.Max(maxY, spanVertices[i].Y);
            }

            return (minY, maxY);
        }
        /// <summary>
        /// Finds the z axis foot-print
        /// </summary>
        /// <param name="t">Triangle bounds</param>
        /// <param name="b">Input geometry bounds</param>
        /// <param name="ics">Inverse cell size</param>
        /// <param name="h">Height</param>
        private static (int, int) FindZFootPrint(BoundingBox t, BoundingBox b, float ics, int h)
        {
            int z0 = (int)((t.Minimum.Z - b.Minimum.Z) * ics);
            int z1 = (int)((t.Maximum.Z - b.Minimum.Z) * ics);
            z0 = MathUtil.Clamp(z0, -1, h - 1);
            z1 = MathUtil.Clamp(z1, 0, h - 1);

            return (z0, z1);
        }
        /// <summary>
        /// Finds the x axis foot-print
        /// </summary>
        /// <param name="poly">Polygon vertices</param>
        /// <param name="b">Input geometry bounds</param>
        /// <param name="ics">Inverse cell size</param>
        /// <param name="w">Width</param>
        private static (bool, int, int) FindXFootPrint(Vector3[] poly, BoundingBox b, float ics, int w)
        {
            var (minX, maxX) = CalculateSpanMinMaxX(poly);
            int x0 = (int)((minX - b.Minimum.X) * ics);
            int x1 = (int)((maxX - b.Minimum.X) * ics);
            if (x1 < 0 || x0 >= w)
            {
                return (false, x0, x1);
            }
            x0 = MathUtil.Clamp(x0, -1, w - 1);
            x1 = MathUtil.Clamp(x1, 0, w - 1);

            return (true, x0, x1);
        }
        /// <summary>
        /// Divides the specified polygon along the axis
        /// </summary>
        private static (Vector3[] Poly1, Vector3[] Poly2) DividePoly(Vector3[] poly, float axisOffset, int axis)
        {
            var outPoly1 = new List<Vector3>();
            var outPoly2 = new List<Vector3>();

            var inVertAxisDelta = GetPolyVerticesInvertAxisDelta(poly, axisOffset, axis);

            for (int inVertA = 0, inVertB = poly.Length - 1; inVertA < poly.Length; inVertB = inVertA, inVertA++)
            {
                var va = poly[inVertA];
                var vb = poly[inVertB];

                float na = inVertAxisDelta[inVertA];
                float nb = inVertAxisDelta[inVertB];

                // If the two vertices are on the same side of the separating axis
                bool sameSide = (na >= 0) == (nb >= 0);
                if (!sameSide)
                {
                    float s = nb / (nb - na);
                    var v = vb + (va - vb) * s;
                    outPoly1.Add(v);
                    outPoly2.Add(v);

                    // add the i'th point to the right polygon. Do NOT add points that are on the dividing line
                    // since these were already added above
                    if (na > 0)
                    {
                        outPoly1.Add(va);
                    }
                    else if (na < 0)
                    {
                        outPoly2.Add(va);
                    }
                }
                else // same side
                {
                    // add the i'th point to the right polygon. Addition is done even for points on the dividing line
                    if (na >= 0)
                    {
                        outPoly1.Add(va);

                        if (na != 0)
                        {
                            continue;
                        }
                    }

                    outPoly2.Add(va);
                }
            }

            return (outPoly1.ToArray(), outPoly2.ToArray());
        }
        /// <summary>
        /// Gets the inverse axis delta polygon vertices
        /// </summary>
        private static float[] GetPolyVerticesInvertAxisDelta(Vector3[] poly, float axisOffset, int axis)
        {
            float[] d = new float[poly.Length];
            for (int i = 0; i < poly.Length; i++)
            {
                d[i] = axisOffset - poly[i][axis];
            }

            return d;
        }

        /// <summary>
        /// Iterates over the specified span list
        /// </summary>
        /// <param name="spans">Span list</param>
        public IEnumerable<(int x, int y, Span span)> IterateSpans()
        {
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    var span = Spans[x + y * Width];

                    // If there are no spans at this cell, just leave the data to index=0, count=0.
                    if (span == null)
                    {
                        continue;
                    }

                    yield return (x, y, span);
                }
            }
        }

        /// <summary>
        /// Builds a new compact heightfield
        /// </summary>
        /// <param name="walkableHeight">Walkable height</param>
        /// <param name="walkableClimb">Walkable climb</param>
        /// <returns>Returns the new compact heightfield</returns>
        public CompactHeightfield Build(int walkableHeight, int walkableClimb)
        {
            var bbox = BoundingBox;
            bbox.Maximum.Y += walkableHeight * CellHeight;

            // Fill in header.
            var chf = new CompactHeightfield
            {
                Width = Width,
                Height = Height,
                WalkableHeight = walkableHeight,
                WalkableClimb = walkableClimb,
                MaxRegions = 0,
                BoundingBox = bbox,
                CellSize = CellSize,
                CellHeight = CellHeight,
            };

            // Fill in cells and spans.
            chf.FillCellsAndSpans(this);

            // Find neighbour connections.
            chf.FindNeighbourConnections();

            return chf;
        }

        /// <summary>
        /// Rasterizes the specified triangle list
        /// </summary>
        /// <param name="tris">Triangle list</param>
        /// <param name="walkableSlopeAngle">Slope angle</param>
        /// <param name="walkableClimb">Maximum climb</param>
        /// <param name="solid">Target solid</param>
        /// <returns>Returns true if the rasterization finishes correctly</returns>
        public bool Rasterize(Triangle[] tris, float walkableSlopeAngle, int walkableClimb)
        {
            var triareas = MarkWalkableTriangles(walkableSlopeAngle, tris);

            // Rasterize triangles.
            foreach (var item in triareas)
            {
                // Rasterize.
                if (!RasterizeTriangle(walkableClimb, item))
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Rasterizes the specified item
        /// </summary>
        private bool RasterizeTriangle(int flagMergeThr, RasterizeItem item)
        {
            float cs = CellSize;
            float ics = 1.0f / CellSize;
            float ich = 1.0f / CellHeight;
            int w = Width;
            int h = Height;
            var b = BoundingBox;
            float by = b.Height;

            // Calculate the bounding box of the triangle.
            var triverts = item.Triangle.GetVertices().ToArray();
            var t = SharpDXExtensions.BoundingBoxFromPoints(triverts);

            // If the triangle does not touch the bbox of the heightfield, skip the triagle.
            if (b.Contains(t) == ContainmentType.Disjoint)
            {
                return true;
            }

            // Calculate the footprint of the triangle on the grid's z-axis
            var (z0, z1) = FindZFootPrint(t, b, ics, h);

            // Clip the triangle into all grid cells it touches.
            var inb = triverts.ToArray();

            for (int z = z0; z <= z1; ++z)
            {
                // Clip polygon to row. Store the remaining polygon as well
                float cz = b.Minimum.Z + z * cs;
                var (inRow, zp1) = DividePoly(inb, cz + cs, 2);
                inb = zp1;
                if (inRow.Length < 3) continue;
                if (z < 0) continue;

                // find the horizontal bounds in the row
                var (found, x0, x1) = FindXFootPrint(inRow, b, ics, w);
                if (!found)
                {
                    continue;
                }

                for (int x = x0; x <= x1; ++x)
                {
                    // Clip polygon to column. store the remaining polygon as well
                    float cx = b.Minimum.X + x * cs;
                    var (xp1, xp2) = DividePoly(inRow, cx + cs, 0);
                    inRow = xp2;
                    if (xp1.Length < 3) continue;
                    if (x < 0) continue;

                    // Calculate min and max of the span.
                    var (minY, maxY) = CalculateSpanMinMaxY(xp1);
                    minY -= b.Minimum.Y;
                    maxY -= b.Minimum.Y;

                    if (SpanOutsideBBox(minY, maxY, by))
                    {
                        // Skip the span if it is outside the heightfield bbox
                        continue;
                    }

                    // Clamp the span to the heightfield bbox.
                    SpanClamp(ref minY, ref maxY, by);

                    // Snap the span to the heightfield height grid.
                    int ismin = MathUtil.Clamp((int)MathF.Floor(minY * ich), 0, Span.SpanMaxHeight);
                    int ismax = MathUtil.Clamp((int)MathF.Ceiling(maxY * ich), ismin + 1, Span.SpanMaxHeight);

                    AddSpan(x, z, ismin, ismax, item.AreaType, flagMergeThr);
                }
            }

            return true;
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
                    for (var s = Spans[x + y * w]; s != null; s = s.Next)
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

            s.SMin = smin;
            s.SMax = smax;
            s.Area = area;
            s.Next = null;

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
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="smin">Lower limit</param>
        /// <param name="smax">Upper limit</param>
        /// <param name="area">Area type</param>
        /// <param name="flagMergeThr">Merge threshold</param>
        private void AddSpan(int x, int y, int smin, int smax, AreaTypes area, int flagMergeThr)
        {
            int idx = x + y * Width;

            if (x == 64)
            {
                area = AreaTypes.RC_DEBUG_AREA;
            }

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
                if (cur.SMin > s.SMax)
                {
                    // Current span is further than the new span, break.
                    break;
                }

                if (cur.SMax < s.SMin)
                {
                    // Current span is before the new span advance.
                    prev = cur;
                    cur = cur.Next;

                    continue;
                }

                // Merge spans.
                s.MergeSpans(cur, flagMergeThr);

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
        private void FilterLedgeSpans(int walkableHeight, int walkableClimb)
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
            int asmin = span.SMax;
            int asmax = span.SMax;

            for (int dir = 0; dir < 4; ++dir)
            {
                // Skip neighbours which are out of bounds.
                int dx = x + GridUtils.GetDirOffsetX(dir);
                int dy = y + GridUtils.GetDirOffsetY(dir);
                if (dx < 0 || dy < 0 || dx >= width || dy >= height)
                {
                    minh = Math.Min(minh, -walkableClimb - span.SMax);
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
