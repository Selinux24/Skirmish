using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Rasterizer
    /// </summary>
    public static class Rasterizer
    {
        /// <summary>
        /// Divided polygons
        /// </summary>
        public static List<RasterizerTriangleData> DebugData { get; private set; } = [];
        /// <summary>
        /// Enables debug information
        /// </summary>
        public static bool EnableDebug { get; set; } = false;


        private static void AddDebugData(RasterizerTriangleData data)
        {
            if (!EnableDebug)
            {
                return;
            }

            DebugData.Add(data);
        }
        private static void AddDivisionData(RasterizerDivisionData data)
        {
            if (!EnableDebug)
            {
                return;
            }

            DebugData[^1].Divisions.Add(data);
        }

        /// <summary>
        /// Rasterizes the specified triangle list
        /// </summary>
        /// <param name="tris">Triangle list</param>
        /// <param name="settings">Settings</param>
        /// <returns>Returns true if the rasterization finishes correctly</returns>
        public static IEnumerable<RasterizeData> Rasterize(Triangle[] tris, RasterizerSettings settings)
        {
            if (EnableDebug)
            {
                DebugData.Clear();
            }

            if (tris?.Length <= 0)
            {
                yield break;
            }

            // Rasterize triangles.
            var rItems = MarkWalkableTriangles(tris, settings.WalkableSlopeAngle);
            foreach (var rItem in rItems)
            {
                // Rasterize.
                var rData = RasterizeTriangle(rItem, settings);
                foreach (var r in rData)
                {
                    yield return r;
                }
            }
        }

        /// <summary>
        /// Marks a walkable triangle list
        /// </summary>
        /// <param name="tris">Triangle list</param>
        /// <param name="walkableSlopeAngle">Slope angle</param>
        /// <returns>Returns a rasterize item collection</returns>
        private static RasterizeItem[] MarkWalkableTriangles(Triangle[] tris, float walkableSlopeAngle)
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
        /// Rasterizes the specified item
        /// </summary>
        /// <param name="item">Rasterize item</param>
        /// <param name="settings">Settings</param>
        private static IEnumerable<RasterizeData> RasterizeTriangle(RasterizeItem item, RasterizerSettings settings)
        {
            int w = settings.Width;
            int h = settings.Height;
            float cs = settings.CellSize;
            float ch = settings.CellHeight;
            float ics = 1.0f / cs;
            float ich = 1.0f / ch;
            var bounds = settings.Bounds;
            float by = bounds.Height;
            int flagMergeThr = settings.WalkableClimb;
            int maxH = RasterizerSettings.MaxHeight;

            // Calculate the bounding box of the triangle.
            var inb = item.Triangle.GetVertices().ToArray();
            var triBounds = item.Triangle.GetBounds();

            AddDebugData(new() { Triangle = item.Triangle, });

            // If the triangle does not touch the bbox of the heightfield, skip the triagle.
            if (bounds.Contains(triBounds) == ContainmentType.Disjoint)
            {
                yield break;
            }

            // Calculate the footprint of the triangle on the grid's z-axis
            var (z0, z1) = FindZFootPrint(triBounds, bounds, ics, h);

            // Clip the triangle into all grid cells it touches.
            for (int z = z0; z <= z1; ++z)
            {
                // Clip polygon to row. Store the remaining polygon as well
                float cz = bounds.Minimum.Z + z * cs;
                var (inRow, zp1) = DividePoly(inb, cz + cs, 2);

                RasterizerDivisionData divZ = new()
                {
                    SourcePoly = [.. inb],
                    DividedPolys = [inRow, zp1],
                };

                inb = zp1;
                if (inRow.Length < 3) continue;
                if (z < 0) continue;

                AddDivisionData(divZ);

                // find the horizontal bounds in the row
                var (found, x0, x1) = FindXFootPrint(inRow, bounds, ics, w);
                if (!found)
                {
                    continue;
                }

                for (int x = x0; x <= x1; ++x)
                {
                    // Clip polygon to column. store the remaining polygon as well
                    float cx = bounds.Minimum.X + x * cs;
                    var (xp1, xp2) = DividePoly(inRow, cx + cs, 0);

                    RasterizerDivisionData divX = new()
                    {
                        SourcePoly = [.. inRow],
                        DividedPolys = [xp1, xp2],
                    };

                    inRow = xp2;
                    if (xp1.Length < 3) continue;
                    if (x < 0) continue;

                    AddDivisionData(divX);

                    // Calculate min and max of the span.
                    var (minY, maxY) = CalculateSpanMinMaxY(xp1);
                    minY -= bounds.Minimum.Y;
                    maxY -= bounds.Minimum.Y;

                    if (SpanOutsideBBox(minY, maxY, by))
                    {
                        // Skip the span if it is outside the heightfield bbox
                        continue;
                    }

                    // Clamp the span to the heightfield bbox.
                    SpanClamp(ref minY, ref maxY, by);

                    // Snap the span to the heightfield height grid.
                    int ismin = MathUtil.Clamp((int)MathF.Floor(minY * ich), 0, maxH);
                    int ismax = MathUtil.Clamp((int)MathF.Ceiling(maxY * ich), ismin + 1, maxH);

                    yield return new(x, z, ismin, ismax, item.AreaType, flagMergeThr);
                }
            }
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

            var inVertAxisDelta = GetPolyVerticesAxisDelta(poly, axisOffset, axis);

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
                    var v = Vector3.Lerp(vb, va, s);
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
        /// Gets the axis delta polygon vertices
        /// </summary>
        private static float[] GetPolyVerticesAxisDelta(Vector3[] poly, float axisOffset, int axis)
        {
            float[] d = new float[poly.Length];

            for (int i = 0; i < poly.Length; i++)
            {
                d[i] = axisOffset - poly[i][axis];
            }

            return d;
        }
    }

    /// <summary>
    /// Rasterizer debug triangle data
    /// </summary>
    public class RasterizerTriangleData
    {
        /// <summary>
        /// Base triangle
        /// </summary>
        public Triangle Triangle { get; set; }
        /// <summary>
        /// Rasterized division data
        /// </summary>
        public List<RasterizerDivisionData> Divisions { get; set; } = [];
    }

    /// <summary>
    /// Rasterizer debug division data
    /// </summary>
    public class RasterizerDivisionData
    {
        /// <summary>
        /// Source polygon to subdivide
        /// </summary>
        public Vector3[] SourcePoly { get; set; } = [];
        /// <summary>
        /// Results of the subdivision
        /// </summary>
        public List<Vector3[]> DividedPolys { get; set; } = [];
    }
}
