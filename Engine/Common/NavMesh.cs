using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Engine.Common
{
    using Engine.PathFinding;

    /// <summary>
    /// Navigation Mesh
    /// </summary>
    public class NavMesh : Graph
    {
        /// <summary>
        /// Vertex partition info
        /// </summary>
        class PartitionVertex
        {
            public bool IsActive;
            public bool IsConvex;
            public bool IsEar;

            public Vector3 Point;
            public float Angle;
            public PartitionVertex Previous;
            public PartitionVertex Next;
        }

        /// <summary>
        /// Static test method
        /// </summary>
        public static void Test()
        {
            {
                Polygon poly1 = new Polygon(new[]{
                    new Vector3(1,0,-3),
                    new Vector3(2,0,-3),
                    new Vector3(2,0,-2),
                    new Vector3(2,0,-1),
                    new Vector3(2,0,0),
                    new Vector3(1,0,-1),
                    new Vector3(0,0,-2),
                    new Vector3(0,0,-3),
                });

                Polygon poly2 = new Polygon(new[]{
                    new Vector3(0,0,-2),
                    new Vector3(1,0,-1),
                    new Vector3(2,0,0),
                    new Vector3(1,0,0),
                    new Vector3(0,0,0),
                    new Vector3(0,0,-1),
                });

                Polygon poly;
                if (Polygon.Merge(poly1, poly2, true, out poly))
                {

                }
            }

            {
                int side = 1;
                int index = 0;

                Vector3[] points = new Vector3[(side + 1) * (side + 1)];

                index = 0;
                for (float y = 0; y < side + 1; y++)
                {
                    for (float x = 0; x < side + 1; x++)
                    {
                        points[index++] = new Vector3(x - ((float)side / 2f), 0, -(y - ((float)side / 2f)));
                    }
                }

                int hole = 2;

                Triangle[] tris = new Triangle[(side * side * 2)];

                index = 0;
                for (int y = 0; y < side; y++)
                {
                    for (int x = 0; x < side; x++)
                    {
                        if (y == hole && x == hole) continue;

                        int i0 = ((y + 0) * (side + 1)) + x;
                        int i1 = ((y + 1) * (side + 1)) + x;
                        int i2 = ((y + 0) * (side + 1)) + x + 1;
                        int i3 = ((y + 1) * (side + 1)) + x + 1;

                        tris[index++] = new Triangle(points[i0], points[i1], points[i2]);
                        tris[index++] = new Triangle(points[i1], points[i3], points[i2]);
                    }
                }

                NavMesh nm = NavMesh.Build(tris, 0);
            }



            {
                Polygon poly = new Polygon(8);
                poly[0] = new Vector3(+1, 0, +1);
                poly[1] = new Vector3(+0, 0, +1);
                poly[2] = new Vector3(-1, 0, +1);
                poly[3] = new Vector3(-1, 0, +0);
                poly[4] = new Vector3(-1, 0, -1);
                poly[5] = new Vector3(+0, 0, -1);
                poly[6] = new Vector3(+1, 0, -1);
                poly[7] = new Vector3(+0.5f, 0, +0);

                poly.Orientation = GeometricOrientation.CounterClockwise;

                Polygon[] parts;
                if (NavMesh.ConvexPartition(new[] { poly }, out parts))
                {
                    Polygon[] mergedPolis;
                    NavMesh.MergeConvex(parts, out mergedPolis);

                    Line3[] edges = mergedPolis[0].GetEdges();
                }
            }

            {
                Vector3 v0 = new Vector3(-1, 0, 1);
                Vector3 v1 = new Vector3(0, 0.5f, 1);
                Vector3 v2 = new Vector3(1, 0, 1);
                Vector3 v3 = new Vector3(-1, 0, 0);
                Vector3 v4 = new Vector3(0, 0.5f, 0);
                Vector3 v5 = new Vector3(0.5f, 0, 0);
                Vector3 v6 = new Vector3(-1, 0, -1);
                Vector3 v7 = new Vector3(0, 0.5f, -1);
                Vector3 v8 = new Vector3(1, 0, -1);

                Triangle[] tris = new Triangle[8];
                tris[0] = new Triangle(v0, v3, v1);
                tris[1] = new Triangle(v1, v3, v4);
                tris[2] = new Triangle(v1, v4, v2);
                tris[3] = new Triangle(v2, v4, v5);
                tris[4] = new Triangle(v3, v6, v4);
                tris[5] = new Triangle(v4, v6, v7);
                tris[6] = new Triangle(v4, v7, v5);
                tris[7] = new Triangle(v5, v7, v8);

                NavMesh nm = NavMesh.Build(tris, 0);
            }

            {
                Vector3 v0 = new Vector3(-1, 0, 1);
                Vector3 v1 = new Vector3(0, 0.5f, 1);
                Vector3 v2 = new Vector3(1, 0, 1);
                Vector3 v3 = new Vector3(-1, 0, 0);
                Vector3 v4 = new Vector3(0, 0.5f, 0);
                Vector3 v5 = new Vector3(0.5f, 0, 0);
                Vector3 v6 = new Vector3(-1, 0, -1);
                Vector3 v7 = new Vector3(0, 0.5f, -1);
                Vector3 v8 = new Vector3(1, 0, -1);

                Triangle[] tris = new Triangle[6];
                tris[0] = new Triangle(v0, v3, v1);
                tris[1] = new Triangle(v1, v3, v4);
                tris[2] = new Triangle(v1, v4, v2);
                tris[3] = new Triangle(v2, v4, v5);
                tris[4] = new Triangle(v4, v7, v5);
                tris[5] = new Triangle(v5, v7, v8);

                NavMesh nm = NavMesh.Build(tris, 0);
            }

            {
                Triangle[] tris = new Triangle[16];
                tris[0] = new Triangle(new Vector3(-2, 0, 2), new Vector3(-2, 0, 1), new Vector3(-1, 0, 2));
                tris[1] = new Triangle(new Vector3(-2, 0, 1), new Vector3(-1, 0, 1), new Vector3(-1, 0, 2));
                tris[2] = new Triangle(new Vector3(-1, 0, 2), new Vector3(-1, 0, 1), new Vector3(1, 0, 2));
                tris[3] = new Triangle(new Vector3(-1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 2));
                tris[4] = new Triangle(new Vector3(1, 0, 2), new Vector3(1, 0, 1), new Vector3(2, 0, 2));
                tris[5] = new Triangle(new Vector3(1, 0, 1), new Vector3(2, 0, 1), new Vector3(2, 0, 2));
                tris[6] = new Triangle(new Vector3(-2, 0, 1), new Vector3(-2, 0, -1), new Vector3(-1, 0, 1));
                tris[7] = new Triangle(new Vector3(-2, 0, -1), new Vector3(-1, 0, -1), new Vector3(-1, 0, 1));
                tris[8] = new Triangle(new Vector3(1, 0, 1), new Vector3(1, 0, -1), new Vector3(2, 0, 1));
                tris[9] = new Triangle(new Vector3(1, 0, -1), new Vector3(2, 0, -1), new Vector3(2, 0, 1));
                tris[10] = new Triangle(new Vector3(-2, 0, -1), new Vector3(-2, 0, -2), new Vector3(-1, 0, -1));
                tris[11] = new Triangle(new Vector3(-2, 0, -2), new Vector3(-1, 0, -2), new Vector3(-1, 0, -1));
                tris[12] = new Triangle(new Vector3(-1, 0, -1), new Vector3(-1, 0, -2), new Vector3(1, 0, -1));
                tris[13] = new Triangle(new Vector3(-1, 0, -2), new Vector3(1, 0, -2), new Vector3(1, 0, -1));
                tris[14] = new Triangle(new Vector3(1, 0, -1), new Vector3(1, 0, -2), new Vector3(2, 0, -1));
                tris[15] = new Triangle(new Vector3(1, 0, -2), new Vector3(2, 0, -2), new Vector3(2, 0, -1));

                NavMesh nm = NavMesh.Build(tris, 0);
            }
        }

        /// <summary>
        /// Navigation Mesh Build
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <param name="angle">Maximum angle of node</param>
        /// <returns>Returns a navigation mesh</returns>
        public static NavMesh Build(VertexData[] vertices, uint[] indices, float angle = MathUtil.PiOverFour)
        {
            int tris = indices.Length / 3;

            Triangle[] triangles = new Triangle[tris];

            int index = 0;
            for (int i = 0; i < tris; i++)
            {
                triangles[i] = new Triangle(
                    vertices[indices[index++]].Position.Value,
                    vertices[indices[index++]].Position.Value,
                    vertices[indices[index++]].Position.Value);
            }

            return Build(triangles, angle);
        }
        /// <summary>
        /// Navigation Mesh Build
        /// </summary>
        /// <param name="triangles">List of triangles</param>
        /// <param name="angle">Maximum angle of node</param>
        /// <returns>Returns a navigation mesh</returns>
        public static NavMesh Build(Triangle[] triangles, float angle = MathUtil.PiOverFour)
        {
            NavMesh result = new NavMesh();

            result.HashCode = triangles.GetMd5Sum();

            BoundingBox bbox = BoundingBox.FromPoints(triangles[0].GetCorners());

            //Remove dups
            var tris = Array.FindAll(triangles, t =>
            {
                return (t.Point1 != t.Point2 && t.Point2 != t.Point3 && t.Point1 != t.Point3);
            });

            //Remove by inclination
            //tris = Array.FindAll(triangles, t => t.Inclination >= angle);

            Array.ForEach(tris, t => { bbox = BoundingBox.Merge(bbox, BoundingBox.FromPoints(t.GetCorners())); });

            //Sort by position
            Array.Sort(tris, (t1, t2) =>
            {
                float d1 = Vector3.DistanceSquared(bbox.Minimum, t1.Center);
                float d2 = Vector3.DistanceSquared(bbox.Minimum, t2.Center);

                return d1.CompareTo(d2);
            });

            if (tris != null && tris.Length > 0)
            {
                Polygon[] polys = Polygon.FromTriangleList(tris, GeometricOrientation.CounterClockwise);

                IGraphNode[] nodes = null;

                Polygon[] parts;
                if (NavMesh.ConvexPartition(polys, out parts))
                {
                    Polygon[] mergedPolis;
                    if (NavMesh.MergeConvex(parts, out mergedPolis))
                    {
                        nodes = NavMeshNode.FromPolygonArray(result, mergedPolis);
                        if (nodes != null)
                        {
                            if (nodes.Length == 1)
                            {
                                #region Simplify

                                Polygon poly = ((NavMeshNode)nodes[0]).Poly;

                                poly.RemoveUnused();

                                #endregion
                            }
                            else if (nodes.Length > 1)
                            {
                                #region Connect nodes

                                for (int i = 0; i < nodes.Length; i++)
                                {
                                    Polygon poly1 = ((NavMeshNode)nodes[i]).Poly;

                                    List<Vector3> exclusions = new List<Vector3>();

                                    for (int x = i + 1; x < nodes.Length; x++)
                                    {
                                        Polygon poly2 = ((NavMeshNode)nodes[x]).Poly;

                                        //Get shared edges
                                        Polygon.SharedEdge[] sharedEdges;
                                        if (Polygon.GetSharedEdges(poly1, poly2, out sharedEdges))
                                        {
                                            if (sharedEdges.Length > 1)
                                            {
                                                Vector3 v1 = poly1[sharedEdges[0].FirstPoint1];
                                                Vector3 v2 = poly1[sharedEdges[sharedEdges.Length - 1].FirstPoint2];

                                                result.connections.Add(new NavMeshConnectionInfo()
                                                {
                                                    First = i,
                                                    Second = x,
                                                    Segment = new Line3(v1, v2),
                                                });

                                                exclusions.AddRange(new[] { v1, v2 });

                                                List<Vector3> toRemove = new List<Vector3>();

                                                for (int s = 0; s < sharedEdges.Length - 1; s++)
                                                {
                                                    toRemove.Add(poly1[sharedEdges[s].FirstPoint2]);
                                                }

                                                poly1.Remove(toRemove.ToArray());
                                                poly2.Remove(toRemove.ToArray());
                                            }
                                            else
                                            {
                                                Vector3 v1 = poly1[sharedEdges[0].FirstPoint1];
                                                Vector3 v2 = poly1[sharedEdges[0].FirstPoint2];

                                                result.connections.Add(new NavMeshConnectionInfo()
                                                {
                                                    First = i,
                                                    Second = x,
                                                    Segment = new Line3(v1, v2),
                                                });

                                                exclusions.AddRange(new[] { v1, v2 });
                                            }
                                        }
                                    }

                                    poly1.RemoveUnused(exclusions.ToArray());
                                }

                                #endregion
                            }
                        }
                    }
                }

                result.Nodes = nodes;
            }

            return result;
        }
        /// <summary>
        /// Merge a list of convex polygons
        /// </summary>
        /// <param name="input">Input polygons</param>
        /// <param name="output">Output polygons</param>
        /// <returns>Returns a list of convex polygons</returns>
        public static bool MergeConvex(Polygon[] input, out Polygon[] output)
        {
            output = null;

            bool merged = false;
            List<Polygon> outputList = new List<Polygon>();
            List<Polygon> mergedList = new List<Polygon>();

            if (input != null && input.Length > 1)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    Polygon current = input[i];

                    if (mergedList.Contains(current)) continue;

                    mergedList.Add(current);

                    Polygon newPoly = new Polygon(current);
                    List<int> indexes = new List<int>();
                    indexes.Add(i);

                    bool marker = true;
                    while (marker)
                    {
                        marker = false;

                        //Find polys
                        Polygon[] joints = Array.FindAll(input, j =>
                            (j != current) &&
                            (!mergedList.Contains(j)) &&
                            (Array.Exists(newPoly.Points, pp => j.Contains(pp))));
                        for (int j = 0; j < joints.Length; j++)
                        {
                            Polygon toMerge = joints[j];

                            Polygon mergedPoly;
                            if (Polygon.Merge(newPoly, toMerge, true, out mergedPoly))
                            {
                                newPoly = mergedPoly;
                                mergedList.Add(toMerge);
                                merged = true;

                                marker = true;
                                indexes.Add(Array.IndexOf(input, joints[j]));
                            }
                        }
                    }

                    outputList.Add(newPoly);
                }

                if (merged && outputList.Count > 1)
                {
                    return MergeConvex(outputList.ToArray(), out output);
                }
                else
                {
                    output = outputList.ToArray();

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Simple heuristic procedure for removing holes from a list of polygons
        /// Works by creating a diagonal from the rightmost hole vertex to some visible vertex
        /// </summary>
        /// <param name="inpolys">A list of polygons that can contain holes</param>
        /// <param name="outpolys">A list of polygons without holes</param>
        /// <returns>Returns true on success, false on failure</returns>
        /// <remarks>
        /// Time complexity: O(h*(n^2)), h is the number of holes, n is the number of vertices
        /// Space complexity: O(n)
        /// 
        /// Vertices of all non-hole polys have to be in counter-clockwise order
        /// Vertices of all hole polys have to be in clockwise order
        /// </remarks>
        public static bool RemoveHoles(Polygon[] inpolys, out Polygon[] outpolys)
        {
            outpolys = null;

            //Check for trivial case (no holes)
            if (!Array.Exists(inpolys, p => p.Hole == true))
            {
                outpolys = inpolys;

                return true;
            }

            List<Polygon> polygonList = new List<Polygon>(inpolys);
            bool hasHoles = false;
            int holePointIndex = 0;
            Polygon hole = null;
            while (true)
            {
                //Find the hole point with the largest x
                hasHoles = false;
                foreach (var poly in polygonList)
                {
                    if (!poly.Hole) continue;

                    if (!hasHoles)
                    {
                        hasHoles = true;
                        hole = poly;
                        holePointIndex = 0;
                    }

                    for (int i = 0; i < poly.Count; i++)
                    {
                        if (poly[i].X > hole[holePointIndex].X)
                        {
                            hole = poly;
                            holePointIndex = i;
                        }
                    }
                }

                if (!hasHoles) break;

                Vector3 holePoint = hole[holePointIndex];
                Vector3 bestPolyPoint = Vector3.Zero;
                Polygon bestPoly = null;
                bool pointFound = false;
                int polyPointIndex = 0;
                foreach (var poly1 in polygonList)
                {
                    if (poly1.Hole) continue;

                    for (int i = 0; i < poly1.Count; i++)
                    {
                        if (poly1[i].X <= holePoint.X) continue;

                        if (!GeometryUtil.InCone(
                            poly1[(i + poly1.Count - 1) % (poly1.Count)],
                            poly1[i],
                            poly1[(i + 1) % (poly1.Count)],
                            holePoint))
                        {
                            continue;
                        }

                        Vector3 polyPoint = poly1[i];
                        if (pointFound)
                        {
                            Vector3 v1 = Vector3.Normalize(polyPoint - holePoint);
                            Vector3 v2 = Vector3.Normalize(bestPolyPoint - holePoint);
                            if (v2.X > v1.X) continue;
                        }

                        bool pointVisible = true;
                        foreach (var poly2 in polygonList)
                        {
                            if (poly2.Hole) continue;

                            for (int i2 = 0; i2 < poly2.Count; i2++)
                            {
                                Vector3 linep1 = poly2[i2];
                                Vector3 linep2 = poly2[(i2 + 1) % (poly2.Count)];
                                if (GeometryUtil.Intersects(holePoint, polyPoint, linep1, linep2))
                                {
                                    pointVisible = false;
                                    break;
                                }
                            }

                            if (!pointVisible) break;
                        }

                        if (pointVisible)
                        {
                            pointFound = true;
                            bestPolyPoint = polyPoint;
                            bestPoly = poly1;
                            polyPointIndex = i;
                        }
                    }
                }

                if (!pointFound) return false;

                {
                    Polygon newpoly = new Polygon(hole.Count + bestPoly.Count + 2);
                    int i2 = 0;
                    for (int i = 0; i <= polyPointIndex; i++)
                    {
                        newpoly[i2] = bestPoly[i];
                        i2++;
                    }
                    for (int i = 0; i <= hole.Count; i++)
                    {
                        newpoly[i2] = hole[(i + holePointIndex) % hole.Count];
                        i2++;
                    }
                    for (int i = polyPointIndex; i < bestPoly.Count; i++)
                    {
                        newpoly[i2] = bestPoly[i];
                        i2++;
                    }

                    polygonList.Add(newpoly);
                }
            }

            outpolys = polygonList.ToArray();

            return true;
        }
        /// <summary>
        /// Partitions a list of polygons into convex parts by using Hertel-Mehlhorn algorithm
        /// </summary>
        /// <param name="inpolys">An input list of polygons to be partitioned</param>
        /// <param name="parts">Resulting list of convex polygons</param>
        /// <returns>Returns true on success, false on failure</returns>
        /// <remarks>
        /// The algorithm gives at most four times the number of parts as the optimal algorithm
        /// However, in practice it works much better than that and often gives optimal partition
        /// Uses triangulation obtained by ear clipping as intermediate result
        /// 
        /// Time complexity O(n^2), n is the number of vertices
        /// Space complexity: O(n)
        /// 
        /// Vertices of all non-hole polys have to be in counter-clockwise order
        /// Vertices of all hole polys have to be in clockwise order
        /// </remarks>
        public static bool ConvexPartition(Polygon[] inpolys, out Polygon[] parts)
        {
            parts = null;

            Polygon[] outpolys;
            if (RemoveHoles(inpolys, out outpolys))
            {
                List<Polygon> partList = new List<Polygon>();

                foreach (var poly in outpolys)
                {
                    Polygon[] polyParts;
                    if (ConvexPartition(poly, out polyParts))
                    {
                        if (polyParts != null && polyParts.Length > 0)
                        {
                            partList.AddRange(polyParts);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                parts = partList.ToArray();

                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Partitions a polygon into convex polygons by using Hertel-Mehlhorn algorithm
        /// </summary>
        /// <param name="poly">An input polygon to be partitioned</param>
        /// <param name="parts">Resulting list of convex polygons</param>
        /// <returns>Returns true on success, false on failure</returns>
        /// <remarks>
        /// The algorithm gives at most four times the number of parts as the optimal algorithm
        /// However, in practice it works much better than that and often gives optimal partition
        /// Uses triangulation obtained by ear clipping as intermediate result
        /// 
        /// Time complexity O(n^2), n is the number of vertices
        /// Space complexity: O(n)
        /// 
        /// Vertices have to be in counter-clockwise order
        /// </remarks>
        private static bool ConvexPartition(Polygon poly, out Polygon[] parts)
        {
            parts = null;

            if (poly.Convex)
            {
                //Polygon already convex
                parts = new Polygon[] { poly };
                return true;
            }
            else
            {
                Polygon[] triangles;
                if (Triangulate(poly, out triangles))
                {
                    for (int i = 0; i < triangles.Length; i++)
                    {
                        Polygon tri1 = triangles[i];
                        for (int i11 = 0; i11 < tri1.Count; i11++)
                        {
                            Vector3 d1 = tri1[i11];
                            int i12 = (i11 + 1) % (tri1.Count);
                            Vector3 d2 = tri1[i12];

                            Polygon tri2 = null;
                            int i21 = -1;
                            int i22 = -1;
                            bool isdiagonal = false;
                            for (int j = i; j < triangles.Count(); j++)
                            {
                                if (i == j) continue;

                                tri2 = triangles[j];

                                for (i21 = 0; i21 < tri2.Count; i21++)
                                {
                                    if ((d2.X != tri2[i21].X) || (d2.Y != tri2[i21].Y)) continue;

                                    i22 = (i21 + 1) % (tri2.Count);

                                    if ((d1.X != tri2[i22].X) || (d1.Y != tri2[i22].Y)) continue;

                                    isdiagonal = true;

                                    break;
                                }

                                if (isdiagonal) break;
                            }

                            if (!isdiagonal) continue;

                            Vector3 p1, p2, p3;
                            int i13, i23;

                            if (i11 == 0) i13 = tri1.Count - 1; else i13 = i11 - 1;
                            if (i22 == (tri2.Count - 1)) i23 = 0; else i23 = i22 + 1;
                            p1 = tri1[i13];
                            p2 = tri1[i11];
                            p3 = tri2[i23];
                            if (!GeometryUtil.IsConvex(p1, p2, p3)) continue;

                            if (i12 == (tri1.Count - 1)) i13 = 0; else i13 = i12 + 1;
                            if (i21 == 0) i23 = tri2.Count - 1; else i23 = i21 - 1;
                            p1 = tri2[i23];
                            p2 = tri1[i12];
                            p3 = tri1[i13];
                            if (!GeometryUtil.IsConvex(p1, p2, p3)) continue;

                            Polygon newpoly = new Polygon(tri1.Count + tri2.Count - 2);
                            int k = 0;
                            for (int j = i12; j != i11; j = (j + 1) % (tri1.Count))
                            {
                                newpoly[k] = tri1[j];
                                k++;
                            }
                            for (int j = i22; j != i21; j = (j + 1) % (tri2.Count))
                            {
                                newpoly[k] = tri2[j];
                                k++;
                            }

                            tri1 = newpoly;
                            i11 = -1;

                            continue;
                        }
                    }

                    parts = triangles;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// Triangulates a polygon by ear clipping
        /// </summary>
        /// <param name="poly">An input polygon to be triangulated</param>
        /// <param name="triangles">A list of triangles (result)</param>
        /// <returns>Returns true on success, false on failure</returns>
        /// <remarks>
        /// Vertices have to be in counter-clockwise order
        /// 
        /// Time complexity O(n^2), n is the number of vertices
        /// Space complexity: O(n)
        /// </remarks>
        private static bool Triangulate(Polygon poly, out Polygon[] triangles)
        {
            if (poly.Count < 3)
            {
                //Impossible
                triangles = null;
                return false;
            }
            else if (poly.Count == 3)
            {
                //It's a triangle
                triangles = new Polygon[] { poly };
                return true;
            }
            else
            {
                //Triangulate

                List<Polygon> triangleList = new List<Polygon>();

                //Initialize one partition per vertex
                PartitionVertex[] vPart = new PartitionVertex[poly.Count];
                for (int i = 0; i < poly.Count; i++)
                {
                    vPart[i] = new PartitionVertex();
                }

                for (int i = 0; i < poly.Count; i++)
                {
                    vPart[i].IsActive = true;
                    vPart[i].Point = poly[i];

                    if (i == (poly.Count - 1))
                    {
                        vPart[i].Next = vPart[0];
                    }
                    else
                    {
                        vPart[i].Next = vPart[i + 1];
                    }

                    if (i == 0)
                    {
                        vPart[i].Previous = vPart[poly.Count - 1];
                    }
                    else
                    {
                        vPart[i].Previous = vPart[i - 1];
                    }
                }

                for (int i = 0; i < poly.Count; i++)
                {
                    UpdatePartitionVertex(vPart[i], vPart, poly.Count);
                }

                for (int i = 0; i < poly.Count - 3; i++)
                {
                    PartitionVertex ear = null;
                    bool earfound = false;

                    //Find the most extruded ear
                    for (int j = 0; j < poly.Count; j++)
                    {
                        if (!vPart[j].IsActive) continue;

                        if (!vPart[j].IsEar) continue;

                        if (!earfound)
                        {
                            earfound = true;
                            ear = vPart[j];
                        }
                        else if (vPart[j].Angle > ear.Angle)
                        {
                            ear = vPart[j];
                        }
                    }

                    if (!earfound)
                    {
                        triangles = null;
                        return false;
                    }

                    triangleList.Add(new Polygon(ear.Previous.Point, ear.Point, ear.Next.Point));

                    ear.IsActive = false;
                    ear.Previous.Next = ear.Next;
                    ear.Next.Previous = ear.Previous;

                    if (i == poly.Count - 4) break;

                    UpdatePartitionVertex(ear.Previous, vPart, poly.Count);
                    UpdatePartitionVertex(ear.Next, vPart, poly.Count);
                }

                for (int i = 0; i < poly.Count; i++)
                {
                    if (vPart[i].IsActive)
                    {
                        triangleList.Add(new Polygon(vPart[i].Previous.Point, vPart[i].Point, vPart[i].Next.Point));
                        break;
                    }
                }

                triangles = triangleList.ToArray();
                return true;
            }
        }
        /// <summary>
        /// Updates partition vertex info
        /// </summary>
        /// <param name="v">Partition vertex</param>
        /// <param name="vertices">Partition list</param>
        /// <param name="numvertices">Number of vertices to update</param>
        private static void UpdatePartitionVertex(PartitionVertex v, PartitionVertex[] vertices, int numvertices)
        {
            PartitionVertex v1 = v.Previous;
            PartitionVertex v3 = v.Next;

            Vector3 vec1 = Vector3.Normalize(v1.Point - v.Point);
            Vector3 vec3 = Vector3.Normalize(v3.Point - v.Point);

            v.Angle = vec1.X * vec3.X + vec1.Z * vec3.Z;
            v.IsConvex = GeometryUtil.IsConvex(v1.Point, v.Point, v3.Point);

            if (v.IsConvex)
            {
                v.IsEar = true;
                for (int i = 0; i < numvertices; i++)
                {
                    if ((vertices[i].Point.X == v.Point.X) && (vertices[i].Point.Z == v.Point.Z)) continue;
                    if ((vertices[i].Point.X == v1.Point.X) && (vertices[i].Point.Z == v1.Point.Z)) continue;
                    if ((vertices[i].Point.X == v3.Point.X) && (vertices[i].Point.Z == v3.Point.Z)) continue;
                    if (GeometryUtil.IsInside(v1.Point, v.Point, v3.Point, vertices[i].Point))
                    {
                        v.IsEar = false;
                        break;
                    }
                }
            }
            else
            {
                v.IsEar = false;
            }
        }
        /// <summary>
        /// Save navigation mesh to file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="nm">Navigation mesh</param>
        public static void Save(string fileName, NavMesh nm)
        {
            using (var fs = File.OpenWrite(fileName))
            {
                using (var wr = new BinaryWriter(fs, Encoding.UTF8))
                {
                    wr.Write(nm.Nodes.Length);

                    foreach (NavMeshNode node in nm.Nodes)
                    {
                        wr.Write(node.Poly.Count);
                        for (int i = 0; i < node.Poly.Count; i++)
                        {
                            wr.Write(node.Poly[i].X);
                            wr.Write(node.Poly[i].Y);
                            wr.Write(node.Poly[i].Z);
                        }
                    }

                    wr.Write(nm.Connections.Length);

                    foreach (NavMeshConnectionInfo connection in nm.Connections)
                    {
                        wr.Write(connection.First);
                        wr.Write(connection.Second);
                        wr.Write(connection.Segment.Point1.X);
                        wr.Write(connection.Segment.Point1.Y);
                        wr.Write(connection.Segment.Point1.Z);
                        wr.Write(connection.Segment.Point2.X);
                        wr.Write(connection.Segment.Point2.Y);
                        wr.Write(connection.Segment.Point2.Z);
                    }
                }
            }
        }
        /// <summary>
        /// Load navigation mesh from file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Returns the loaded navigation mesh</returns>
        public static NavMesh Load(string fileName)
        {
            using (var fs = File.OpenRead(fileName))
            {
                using (var rd = new BinaryReader(fs, Encoding.UTF8))
                {
                    return Load(rd);
                }
            }
        }
        /// <summary>
        /// Load navigation mesh from memory stream
        /// </summary>
        /// <param name="stream">Memory stream</param>
        /// <returns>Returns the loaded navigation mesh</returns>
        public static NavMesh Load(MemoryStream stream)
        {
            using (var rd = new BinaryReader(stream, Encoding.UTF8))
            {
                return Load(rd);
            }
        }
        /// <summary>
        /// Load navigation mesh from a binary reader
        /// </summary>
        /// <param name="rd">File Binary reader</param>
        /// <returns>Returns the loaded navigation mesh</returns>
        public static NavMesh Load(BinaryReader rd)
        {
            NavMesh nm = new NavMesh();

            var nodeCount = rd.ReadInt32();
            NavMeshNode[] nodes = new NavMeshNode[nodeCount];

            for (int n = 0; n < nodeCount; n++)
            {
                var vCount = rd.ReadInt32();
                Vector3[] points = new Vector3[vCount];

                for (int p = 0; p < vCount; p++)
                {
                    points[p] = new Vector3(rd.ReadSingle(), rd.ReadSingle(), rd.ReadSingle());
                }

                nodes[n] = new NavMeshNode(nm, new Polygon(points));
            }

            nm.Nodes = nodes;

            var connectionCount = rd.ReadInt32();
            NavMeshConnectionInfo[] connections = new NavMeshConnectionInfo[connectionCount];

            for (int n = 0; n < connectionCount; n++)
            {
                var poly1 = rd.ReadInt32();
                var poly2 = rd.ReadInt32();
                var point1 = new Vector3(rd.ReadSingle(), rd.ReadSingle(), rd.ReadSingle());
                var point2 = new Vector3(rd.ReadSingle(), rd.ReadSingle(), rd.ReadSingle());

                connections[n] = new NavMeshConnectionInfo()
                {
                    First = poly1,
                    Second = poly2,
                    Segment = new Line3(point1, point2),
                };
            }

            nm.Connections = connections;

            return nm;
        }

        /// <summary>
        /// Polygon connection list
        /// </summary>
        private List<NavMeshConnectionInfo> connections = new List<NavMeshConnectionInfo>();

        /// <summary>
        /// Hash code
        /// </summary>
        public string HashCode;
        /// <summary>
        /// Connectios
        /// </summary>
        public NavMeshConnectionInfo[] Connections
        {
            get
            {
                return this.connections.ToArray();
            }
            set
            {
                this.connections.Clear();

                if (value != null && value.Length > 0)
                {
                    this.connections.AddRange(value);
                }
            }
        }

        /// <summary>
        /// Gets a list of connected nodes from specified node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns a list of connected nodes from specified node</returns>
        public IGraphNode[] GetConnections(IGraphNode node)
        {
            int index = Array.IndexOf(this.Nodes, node);
            if (index >= 0)
            {
                //Find all connections for the specified node index
                var conns = this.connections.FindAll(c => c.First == index || c.Second == index);
                if (conns != null && conns.Count > 0)
                {
                    var result = new List<IGraphNode>();

                    for (int i = 0; i < conns.Count; i++)
                    {
                        if (conns[i].First != index)
                        {
                            //Add first connection node to result
                            var n1 = this.Nodes[conns[i].First];
                            if (!result.Contains(n1)) result.Add(n1);
                        }

                        if (conns[i].Second != index)
                        {
                            //Add second connection node to result
                            var n2 = this.Nodes[conns[i].Second];
                            if (!result.Contains(n2)) result.Add(n2);
                        }
                    }

                    return result.ToArray();
                }
            }

            return new IGraphNode[] { };
        }
        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation</returns>
        public override string ToString()
        {
            return string.Format("Nodes {0}; Connections {1};", this.Nodes.Length, this.connections.Count);
        }
    }

    /// <summary>
    /// Polygon connection info
    /// </summary>
    public class NavMeshConnectionInfo
    {
        /// <summary>
        /// First node index
        /// </summary>
        public int First;
        /// <summary>
        /// Second node index
        /// </summary>
        public int Second;
        /// <summary>
        /// Connection segment
        /// </summary>
        public Line3 Segment;

        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation</returns>
        public override string ToString()
        {
            return string.Format("First: {0}; Second: {1}; Segment: {2}", this.First, this.Second, this.Segment);
        }
    }
}
