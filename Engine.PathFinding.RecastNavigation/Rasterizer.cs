﻿using SharpDX;
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
        /// <param name="walkableSlopeAngle">Slope angle in degrees</param>
        /// <returns>Returns a rasterize item collection</returns>
        private static RasterizeItem[] MarkWalkableTriangles(Triangle[] tris, float walkableSlopeAngle)
        {
            RasterizeItem[] res = new RasterizeItem[tris.Length];

            float walkableThr = MathF.Cos(walkableSlopeAngle / 180.0f * MathF.PI);

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
            var bounds = settings.Bounds;

            var polyBounds = item.Triangle.GetBounds();
            if (bounds.Contains(polyBounds) == ContainmentType.Disjoint)
            {
                // If the triangle does not touch the bbox of the heightfield, skip the triangle.
                yield break;
            }

            AddDebugData(new() { Triangle = item.Triangle, });

            float ics = 1.0f / settings.CellSize;
            float ich = 1.0f / settings.CellHeight;
            int h = settings.Height;
            int flagMergeThr = settings.WalkableClimb;
            float by = bounds.Height;

            // Calculate the footprint of the triangle on the grid's z-axis
            var (z0, z1) = FindZFootPrint(polyBounds, bounds, ics, h);

            // Clip the triangle into all grid cells it touches.
            var poly = item.Triangle.GetVertices().ToArray();
            foreach (var (x, z, p) in SubdividePoly(z0, z1, poly, settings))
            {
                // Calculate min and max of the span.
                var (minY, maxY) = CalculateSpanMinMax(p, Axis.Y);
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
                int ismin = MathUtil.Clamp((int)MathF.Floor(minY * ich), 0, RasterizerSettings.MaxHeight);
                int ismax = MathUtil.Clamp((int)MathF.Ceiling(maxY * ich), ismin + 1, RasterizerSettings.MaxHeight);

                yield return new(x, z, ismin, ismax, item.AreaType, flagMergeThr);
            }
        }
        /// <summary>
        /// Subdivides the specified poly into rows
        /// </summary>
        /// <param name="z0">Z index from</param>
        /// <param name="z1">Z index to</param>
        /// <param name="poly">Polygon vertices</param>
        /// <param name="settings">Rasterizer settings</param>
        /// <returns>Returns a list of x, z coordinates and the span polygon</returns>
        private static IEnumerable<(int X, int Z, Vector3[] Poly)> SubdividePoly(int z0, int z1, Vector3[] poly, RasterizerSettings settings)
        {
            float cs = settings.CellSize;
            float ics = 1.0f / cs;
            int w = settings.Width;
            var bounds = settings.Bounds;

            for (int z = z0; z <= z1; ++z)
            {
                // Clip polygon to row. Store the remaining polygon as well
                float cz = bounds.Minimum.Z + z * cs;
                var (row, p1) = DividePoly(poly, Axis.Z, cz + cs);

                RasterizerDivisionData divZ = new()
                {
                    SourcePoly = [.. poly],
                    DividedPolys = [row, p1],
                };

                (poly, _) = (p1, poly);
                if (row.Length < 3) continue;
                if (z < 0) continue;

                AddDivisionData(divZ);

                // find the horizontal bounds in the row
                var (found, x0, x1) = FindXFootPrint(row, bounds, ics, w);
                if (!found)
                {
                    continue;
                }

                foreach (var (x, p) in SubdivideRow(x0, x1, row, settings))
                {
                    yield return (x, z, p);
                }
            }
        }
        /// <summary>
        /// Subdivides the specified row into spans
        /// </summary>
        /// <param name="x0">X index from</param>
        /// <param name="x1">X index to</param>
        /// <param name="row">Row</param>
        /// <param name="settings">Rasterizer settings</param>
        /// <returns>Returns a list of x coordinates and the span polygon</returns>
        private static IEnumerable<(int X, Vector3[] Poly)> SubdivideRow(int x0, int x1, Vector3[] row, RasterizerSettings settings)
        {
            float cs = settings.CellSize;
            var bounds = settings.Bounds;

            for (int x = x0; x <= x1; ++x)
            {
                // Clip polygon to column. store the remaining polygon as well
                float cx = bounds.Minimum.X + x * cs;
                var (xp1, xp2) = DividePoly(row, 0, cx + cs);

                RasterizerDivisionData divX = new()
                {
                    SourcePoly = [.. row],
                    DividedPolys = [xp1, xp2],
                };

                (row, _) = (xp2, row);
                if (xp1.Length < 3) continue;
                if (x < 0) continue;

                AddDivisionData(divX);

                yield return (x, xp1);
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
        /// Calculates the span axis sizes
        /// </summary>
        /// <param name="vertices">Vertex list</param>
        /// <param name="axis">Axis</param>
        private static (float Min, float Max) CalculateSpanMinMax(Vector3[] vertices, Axis axis)
        {
            int a = (int)axis;

            float min = vertices[0][a];
            float max = vertices[0][a];

            for (int i = 1; i < vertices.Length; i++)
            {
                min = MathF.Min(min, vertices[i][a]);
                max = MathF.Max(max, vertices[i][a]);
            }

            return (min, max);
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
            int z0 = (int)MathF.Round((t.Minimum.Z - b.Minimum.Z) * ics);
            int z1 = (int)MathF.Round((t.Maximum.Z - b.Minimum.Z) * ics);
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
            var (minX, maxX) = CalculateSpanMinMax(poly, Axis.X);
            int x0 = (int)MathF.Round((minX - b.Minimum.X) * ics);
            int x1 = (int)MathF.Round((maxX - b.Minimum.X) * ics);
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
        /// <param name="poly">Polygon vertices to divide</param>
        /// <param name="axis">Division axis</param>
        /// <param name="axisOffset">Axis offset</param>
        /// <returns>Returns the resulting polygons</returns>
        private static (Vector3[] Poly1, Vector3[] Poly2) DividePoly(Vector3[] poly, Axis axis, float axisOffset)
        {
            List<Vector3> outPoly1 = [];
            List<Vector3> outPoly2 = [];

            var vertAxisDelta = poly.Select(p => axisOffset - p[(int)axis]).ToArray();

            for (int vertA = 0, vertB = poly.Length - 1; vertA < poly.Length; vertB = vertA, vertA++)
            {
                var va = poly[vertA];
                var vb = poly[vertB];

                float na = vertAxisDelta[vertA];
                float nb = vertAxisDelta[vertB];

                bool sameSide = (na >= 0) == (nb >= 0);
                if (!sameSide)
                {
                    // If the two vertices are on the same side of the separating axis
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

                    continue;
                }

                // same side

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

            return (outPoly1.ToArray(), outPoly2.ToArray());
        }

        /// <summary>
        /// Adds debug date to the collection
        /// </summary>
        /// <param name="data">Data</param>
        private static void AddDebugData(RasterizerTriangleData data)
        {
            if (!EnableDebug)
            {
                return;
            }

            DebugData.Add(data);
        }
        /// <summary>
        /// Adds debug division data to the last debug data instance
        /// </summary>
        /// <param name="data">Division data</param>
        private static void AddDivisionData(RasterizerDivisionData data)
        {
            if (!EnableDebug)
            {
                return;
            }

            if (DebugData.Count <= 0)
            {
                return;
            }

            DebugData[^1].Divisions.Add(data);
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
