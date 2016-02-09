using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    using Engine.PathFinding;

    public class NavMesh : IGraph<NavmeshNode>
    {
        class PartitionVertex
        {
            public bool IsActive;
            public bool IsConvex;
            public bool IsEar;

            public Vector2 Point;
            public float Angle;
            public PartitionVertex Previous;
            public PartitionVertex Next;
        }

        class ConnectionInfo
        {
            public int Poly1;
            public int Poly2;
            public Line2 Segment;
        }

        public static NavMesh Build(Triangle[] triangles, float angle = MathUtil.PiOverFour)
        {
            NavMesh result = new NavMesh();

            var tris = Array.FindAll(triangles, t => t.Inclination <= angle);
            if (tris != null && tris.Length > 0)
            {
                Polygon[] polys = new Polygon[tris.Length];

                for (int i = 0; i < tris.Length; i++)
                {
                    polys[i] = Polygon.FromTriangle(tris[i], GeometricOrientation.CounterClockwise);
                }

                NavmeshNode[] nodes = null;

                Polygon[] parts;
                if (NavMesh.ConvexPartition(polys, out parts))
                {
                    Polygon[] mergedPolis;
                    if (NavMesh.MergeConvex(parts, out mergedPolis))
                    {
                        nodes = new NavmeshNode[mergedPolis.Length];

                        for (int i = 0; i < nodes.Length; i++)
                        {
                            nodes[i] = new NavmeshNode(mergedPolis[i]);
                        }

                        if (nodes.Length > 1)
                        {
                            //Remove unused vertices from polygons
                            for (int i = 0; i < nodes.Length; i++)
                            {
                                Polygon poly1 = nodes[i].Poly;

                                List<Vector2> toRemove = new List<Vector2>();

                                var edges = poly1.GetEdges();
                                for (int ii = 1; ii < edges.Length; ii++)
                                {
                                    if (edges[ii - 1].Direction == edges[ii].Direction)
                                    {
                                        //Shared point
                                        Vector2 shared = edges[ii].Point1;

                                        if (!Array.Exists(nodes, n => n != nodes[i] && n.Poly.Contains(shared)))
                                        {
                                            //To Remove
                                            toRemove.Add(shared);
                                        }
                                    }
                                }

                                poly1.Remove(toRemove.ToArray());
                            }

                            //Connect nodes
                            for (int i = 0; i < nodes.Length; i++)
                            {
                                Polygon poly1 = nodes[i].Poly;

                                for (int x = i + 1; x < nodes.Length; x++)
                                {
                                    Polygon poly2 = nodes[x].Poly;

                                    //Get shared edges
                                    Polygon.SharedEdge[] sharedEdges;
                                    if (Polygon.GetSharedEdges(poly1, poly2, out sharedEdges))
                                    {
                                        for (int s = 0; s < sharedEdges.Length; s++)
                                        {
                                            //Save join
                                            result.connections.Add(new ConnectionInfo()
                                            {
                                                Poly1 = i,
                                                Poly2 = x,
                                                Segment = new Line2(poly1[sharedEdges[s].SharedFirstPoint1], poly1[sharedEdges[s].SharedFirstPoint2]),
                                            });
                                        }
                                    }
                                }
                            }

                        }
                    }
                }

                result.Nodes = nodes;
            }

            return result;
        }

        public static bool MergeConvex(Polygon[] inpolys, out Polygon[] outpolys)
        {
            outpolys = null;

            bool merged = false;
            List<Polygon> outPolyList = new List<Polygon>();
            List<Polygon> mergedPolys = new List<Polygon>();

            if (inpolys != null && inpolys.Length > 1)
            {
                for (int i = 0; i < inpolys.Length; i++)
                {
                    if (mergedPolys.Contains(inpolys[i])) continue;

                    Polygon newpoly = inpolys[i];

                    for (int j = i + 1; j < inpolys.Length; j++)
                    {
                        if (mergedPolys.Contains(inpolys[j])) continue;

                        Polygon mergedpoly;
                        if (Polygon.Merge(newpoly, inpolys[j], true, out mergedpoly))
                        {
                            mergedPolys.Add(inpolys[j]);
                            newpoly = mergedpoly;
                            merged = true;
                        }
                    }

                    outPolyList.Add(newpoly);
                }

                if (merged)
                {
                    return MergeConvex(outPolyList.ToArray(), out outpolys);
                }
                else
                {
                    outpolys = outPolyList.ToArray();

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

                Vector2 holePoint = hole[holePointIndex];
                Vector2 bestPolyPoint = Vector2.Zero;
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

                        Vector2 polyPoint = poly1[i];
                        if (pointFound)
                        {
                            Vector2 v1 = Vector2.Normalize(polyPoint - holePoint);
                            Vector2 v2 = Vector2.Normalize(bestPolyPoint - holePoint);
                            if (v2.X > v1.X) continue;
                        }

                        bool pointVisible = true;
                        foreach (var poly2 in polygonList)
                        {
                            if (poly2.Hole) continue;

                            for (int i2 = 0; i2 < poly2.Count; i2++)
                            {
                                Vector2 linep1 = poly2[i2];
                                Vector2 linep2 = poly2[(i2 + 1) % (poly2.Count)];
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
                            Vector2 d1 = tri1[i11];
                            int i12 = (i11 + 1) % (tri1.Count);
                            Vector2 d2 = tri1[i12];

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

                            Vector2 p1, p2, p3;
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
                    UpdateVertex(vPart[i], vPart, poly.Count);
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

                    UpdateVertex(ear.Previous, vPart, poly.Count);
                    UpdateVertex(ear.Next, vPart, poly.Count);
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

        private static void UpdateVertex(PartitionVertex v, PartitionVertex[] vertices, int numvertices)
        {
            PartitionVertex v1 = v.Previous;
            PartitionVertex v3 = v.Next;

            Vector2 vec1 = Vector2.Normalize(v1.Point - v.Point);
            Vector2 vec3 = Vector2.Normalize(v3.Point - v.Point);

            v.Angle = vec1.X * vec3.X + vec1.Y * vec3.Y;
            v.IsConvex = GeometryUtil.IsConvex(v1.Point, v.Point, v3.Point);

            if (v.IsConvex)
            {
                v.IsEar = true;
                for (int i = 0; i < numvertices; i++)
                {
                    if ((vertices[i].Point.X == v.Point.X) && (vertices[i].Point.Y == v.Point.Y)) continue;
                    if ((vertices[i].Point.X == v1.Point.X) && (vertices[i].Point.Y == v1.Point.Y)) continue;
                    if ((vertices[i].Point.X == v3.Point.X) && (vertices[i].Point.Y == v3.Point.Y)) continue;
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



        private List<ConnectionInfo> connections = new List<ConnectionInfo>();
    }


    public class NavmeshNode : GraphNode<NavmeshNode>
    {
        public Polygon Poly;

        public NavmeshNode(Polygon poly)
        {
            this.Poly = poly;
        }


        public override bool Contains(Vector3 point, out float distance)
        {
            throw new NotImplementedException();
        }
    }
}
