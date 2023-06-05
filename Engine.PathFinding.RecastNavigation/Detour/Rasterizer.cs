using Engine.PathFinding.RecastNavigation.Recast;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Rasterize helper class
    /// </summary>
    static class Rasterizer
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
        }

        /// <summary>
        /// Rasterizes the specified triangle list
        /// </summary>
        /// <param name="tris">Triangle list</param>
        /// <param name="walkableSlopeAngle">Slope angle</param>
        /// <param name="walkableClimb">Maximum climb</param>
        /// <param name="solid">Target solid</param>
        /// <returns>Returns true if the rasterization finishes correctly</returns>
        public static bool Rasterize(IEnumerable<Triangle> tris, float walkableSlopeAngle, int walkableClimb, Heightfield solid)
        {
            var triareas = MarkWalkableTriangles(walkableSlopeAngle, tris);

            return RasterizeTriangles(solid, walkableClimb, triareas);
        }
        private static IEnumerable<RasterizeItem> MarkWalkableTriangles(float walkableSlopeAngle, IEnumerable<Triangle> tris)
        {
            List<RasterizeItem> res = new List<RasterizeItem>();

            float walkableThr = (float)Math.Cos(walkableSlopeAngle / 180.0f * MathUtil.Pi);

            foreach (var tri in tris)
            {
                // Check if the face is walkable.
                AreaTypes area = tri.Normal.Y > walkableThr ? AreaTypes.RC_WALKABLE_AREA : AreaTypes.RC_NULL_AREA;

                res.Add(new RasterizeItem() { Triangle = tri, AreaType = area });
            }

            return res;
        }
        private static bool RasterizeTriangles(Heightfield solid, int flagMergeThr, IEnumerable<RasterizeItem> items)
        {
            // Rasterize triangles.
            foreach (var item in items)
            {
                // Rasterize.
                if (!RasterizeTriangle(solid, flagMergeThr, item))
                {
                    throw new EngineException("rcRasterizeTriangles: Out of memory.");
                }
            }

            return true;
        }
        private static bool RasterizeTriangle(Heightfield solid, int flagMergeThr, RasterizeItem item)
        {
            float cs = solid.CellSize;
            float ics = 1.0f / solid.CellSize;
            float ich = 1.0f / solid.CellHeight;
            int w = solid.Width;
            int h = solid.Height;
            var b = solid.BoundingBox;
            float by = b.Height;

            // Calculate the bounding box of the triangle.
            var triverts = item.Triangle.GetVertices();
            var t = SharpDXExtensions.BoundingBoxFromPoints(triverts.ToArray());

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
            IEnumerable<Vector3> inb = new List<Vector3>(triverts);

            for (int y = y0; y <= y1; ++y)
            {
                // Clip polygon to row. Store the remaining polygon as well
                float cz = b.Minimum.Z + y * cs;
                var (Zp1, Zp2) = DividePoly(inb, cz + cs, 2);
                Helper.Swap(ref inb, ref Zp2);
                if (Zp1.Count() < 3) continue;

                // find the horizontal bounds in the row
                var (MinX, MaxX) = CalculateSpanMinMaxX(Zp1, b);
                int x0 = (int)(MinX * ics);
                int x1 = (int)(MaxX * ics);
                x0 = MathUtil.Clamp(x0, 0, w - 1);
                x1 = MathUtil.Clamp(x1, 0, w - 1);

                for (int x = x0; x <= x1; ++x)
                {
                    // Clip polygon to column. store the remaining polygon as well
                    float cx = b.Minimum.X + x * cs;
                    var (Xp1, Xp2) = DividePoly(Zp1, cx + cs, 0);
                    Helper.Swap(ref Zp1, ref Xp2);
                    if (Xp1.Count() < 3) continue;

                    // Calculate min and max of the span.
                    var (MinY, MaxY) = CalculateSpanMinMaxY(Xp1, b);
                    float minY = MinY;
                    float maxY = MaxY;
                    // Skip the span if it is outside the heightfield bbox
                    if (SpanOutsideBBox(minY, maxY, by))
                    {
                        continue;
                    }
                    // Clamp the span to the heightfield bbox.
                    SpanClamp(ref minY, ref maxY, by);

                    // Snap the span to the heightfield height grid.
                    int ismin = MathUtil.Clamp((int)Math.Floor(minY * ich), 0, Span.SpanMaxHeight);
                    int ismax = MathUtil.Clamp((int)Math.Ceiling(maxY * ich), ismin + 1, Span.SpanMaxHeight);

                    solid.AddSpan(x, y, ismin, ismax, item.AreaType, flagMergeThr);
                }
            }

            return true;
        }
        private static bool SpanOutsideBBox(float min, float max, float size)
        {
            if (max < 0.0f) return true;
            if (min > size) return true;

            return false;
        }
        private static void SpanClamp(ref float min, ref float max, float size)
        {
            if (min < 0.0f) min = 0;
            if (max > size) max = size;
        }
        private static (float MinX, float MaxX) CalculateSpanMinMaxX(IEnumerable<Vector3> spanVertices, BoundingBox bbox)
        {
            float minX = spanVertices.First().X;
            float maxX = spanVertices.First().X;
            for (int i = 1; i < spanVertices.Count(); i++)
            {
                minX = Math.Min(minX, spanVertices.ElementAt(i).X);
                maxX = Math.Max(maxX, spanVertices.ElementAt(i).X);
            }
            minX -= bbox.Minimum.X;
            maxX -= bbox.Minimum.X;

            return (minX, maxX);
        }
        private static (float MinY, float MaxY) CalculateSpanMinMaxY(IEnumerable<Vector3> spanVertices, BoundingBox bbox)
        {
            float minY = spanVertices.First().Y;
            float maxY = spanVertices.First().Y;
            for (int i = 1; i < spanVertices.Count(); ++i)
            {
                minY = Math.Min(minY, spanVertices.ElementAt(i).Y);
                maxY = Math.Max(maxY, spanVertices.ElementAt(i).Y);
            }
            minY -= bbox.Minimum.Y;
            maxY -= bbox.Minimum.Y;

            return (minY, maxY);
        }
        private static (IEnumerable<Vector3> Poly1, IEnumerable<Vector3> Poly2) DividePoly(IEnumerable<Vector3> inPoly, float x, int axis)
        {
            List<Vector3> outPoly1 = new List<Vector3>();
            List<Vector3> outPoly2 = new List<Vector3>();

            var d = GetPolyVerticesAxis(inPoly, x, axis);

            for (int i = 0, j = inPoly.Count() - 1; i < inPoly.Count(); j = i, i++)
            {
                Vector3 va = inPoly.ElementAt(j);
                Vector3 vb = inPoly.ElementAt(i);

                float na = d.ElementAt(j);
                float nb = d.ElementAt(i);

                bool ina = na >= 0;
                bool inb = nb >= 0;
                if (ina != inb)
                {
                    float s = na / (na - nb);
                    Vector3 v = va + (vb - va) * s;
                    outPoly1.Add(v);
                    outPoly2.Add(v);

                    // add the i'th point to the right polygon. Do NOT add points that are on the dividing line
                    // since these were already added above
                    if (nb > 0)
                    {
                        outPoly1.Add(vb);
                    }
                    else if (nb < 0)
                    {
                        outPoly2.Add(vb);
                    }
                }
                else // same side
                {
                    // add the i'th point to the right polygon. Addition is done even for points on the dividing line
                    if (nb >= 0)
                    {
                        outPoly1.Add(vb);

                        if (nb != 0)
                        {
                            continue;
                        }
                    }

                    outPoly2.Add(vb);
                }
            }

            return (outPoly1, outPoly2);
        }
        private static IEnumerable<float> GetPolyVerticesAxis(IEnumerable<Vector3> inPoly, float x, int axis)
        {
            float[] d = new float[inPoly.Count()];
            for (int i = 0; i < inPoly.Count(); i++)
            {
                d[i] = x - inPoly.ElementAt(i)[axis];
            }

            return d;
        }
    }
}
