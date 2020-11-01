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
            var t = BoundingBox.FromPoints(triverts.ToArray());

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
            List<Vector3> inb = new List<Vector3>(triverts);
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

                    solid.AddSpan(x, y, ismin, ismax, item.AreaType, flagMergeThr);
                }
            }

            return true;
        }
        private static void DividePoly(List<Vector3> inPoly, List<Vector3> outPoly1, List<Vector3> outPoly2, float x, int axis)
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
    }
}
