using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpDX;

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

        public static NavMesh Build(
            Geometry.PolyMesh polyMesh, Geometry.PolyMeshDetail polyMeshDetail,
            OffMeshConnection[] offMeshCons,
            float cellSize, float cellHeight, int vertsPerPoly, float maxClimb)
        {
            var res = new NavMesh();

            var builder = NavMeshBuilder.Build(polyMesh, polyMeshDetail, offMeshCons, cellSize, cellHeight, vertsPerPoly, maxClimb);

            NavMeshNode[] nodes = new NavMeshNode[builder.NavPolys.Length];
            int nodeIndex = 0;

            foreach (var poly in builder.NavPolys)
            {
                var points = new Vector3[poly.VertCount];
                for (int i = 0; i < poly.VertCount; i++)
                {
                    points[i] = builder.NavVerts[poly.Verts[i]];
                }

                nodes[nodeIndex++] = new NavMeshNode(res, new Polygon(points));
            }

            List<NavMeshConnectionInfo> connections = new List<NavMeshConnectionInfo>();

            foreach (var node in nodes)
            {
                var conns = Array.FindAll(nodes, n =>
                {
                    Polygon.SharedEdge[] sharedEdges;
                    if (n != node && Polygon.GetSharedEdges(node.Poly, n.Poly, out sharedEdges))
                    {
                        var nc = new NavMeshConnectionInfo()
                        {
                            First = Array.IndexOf(nodes, node),
                            Second = Array.IndexOf(nodes, n),
                        };

                        connections.Add(nc);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });
            }

            res.Nodes = nodes;
            res.Connections = connections.ToArray();

            return res;
        }

        public static NavMesh Test(BoundingBox bbox, VertexData[] vertices, uint[] indices)
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

            return Test(bbox, triangles);
        }

        public static NavMesh Test(BoundingBox bbox, Triangle[] triangles)
        {
            //float cellSize = 0.3f;
            //float cellHeight = 0.2f;

            float diagonal = Vector2.Distance(new Vector2(bbox.Maximum.X, bbox.Maximum.Z), new Vector2(bbox.Minimum.X, bbox.Minimum.Z));

            float cellSize = diagonal / 512f;
            float cellHeight = cellSize * 0.66f;
            int walkableHeight = 1;
            int walkableClimb = 1;

            var fh = new Geometry.Heightfield(bbox, cellSize, cellHeight);
            fh.RasterizeTriangles(triangles, Geometry.Area.Default);
            fh.FilterLedgeSpans(walkableHeight * 10, walkableClimb * 2);
            fh.FilterLowHangingWalkableObstacles(walkableClimb * 2);
            fh.FilterWalkableLowHeightSpans(walkableHeight * 10);

            int radius = 1;
            int borderSize = 0;
            int minRegionArea = 16;
            int mergeRegionArea = 40;

            var ch = new Geometry.CompactHeightfield(fh, walkableHeight, walkableClimb);
            ch.Erode(radius);
            ch.BuildDistanceField();
            ch.BuildRegions(borderSize, minRegionArea, mergeRegionArea);

            float maxError = 1.8f;
            int maxEdgeLength = 24;

            var cs = ch.BuildContourSet(maxError, maxEdgeLength, Geometry.ContourBuildFlags.None);

            int vertsPerPoly = 6;

            var pm = new Geometry.PolyMesh(cs, cellSize, cellHeight, borderSize, vertsPerPoly);

            int sampleDist = 6;
            int sampleMaxError = 1;

            var pmd = new Geometry.PolyMeshDetail(pm, ch, sampleDist, sampleMaxError);

            float maxClimb = 0.9f;

            return NavMesh.Build(pm, pmd, null, cellSize, cellHeight, vertsPerPoly, maxClimb);
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
        /// Polygon connection list
        /// </summary>
        private List<NavMeshConnectionInfo> connections = new List<NavMeshConnectionInfo>();

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



    /// <summary>
    /// The NavMeshBuilder class converst PolyMesh and PolyMeshDetail into a different data structure suited for pathfinding.
    /// This class will create tiled data.
    /// </summary>
    public class NavMeshBuilder
    {
        /// <summary>
        /// Flags representing the type of a navmesh polygon.
        /// </summary>
        [Flags]
        public enum PolygonType : byte
        {
            /// <summary>
            /// A polygon that is part of the navmesh.
            /// </summary>
            Ground = 0,
            /// <summary>
            /// An off-mesh connection consisting of two vertices.
            /// </summary>
            OffMeshConnection = 1
        }
        /// <summary>
        /// Uses the PolyMesh polygon data for pathfinding
        /// </summary>
        public class Poly
        {
            /// <summary>
            /// Polygon type
            /// </summary>
            private PolygonType polyType;

            public List<Link> Links { get; private set; }
            /// <summary>
            /// Gets or sets the indices of polygon's vertices
            /// </summary>
            public int[] Verts { get; set; }
            /// <summary>
            /// Gets or sets packed data representing neighbor polygons references and flags for each edge
            /// </summary>
            public int[] Neis { get; set; }
            /// <summary>
            /// Gets or sets a user defined polygon flags
            /// </summary>
            public object Tag { get; set; }
            /// <summary>
            /// Gets or sets the number of vertices
            /// </summary>
            public int VertCount { get; set; }
            /// <summary>
            /// Gets or sets the AreaId
            /// </summary>
            public Geometry.Area Area { get; set; }
            /// <summary>
            /// Gets or sets the polygon type (ground or offmesh)
            /// </summary>
            public PolygonType PolyType
            {
                get
                {
                    return polyType;
                }

                set
                {
                    polyType = value;
                }
            }

            public Poly()
            {
                Links = new List<Link>();
            }
        }

        public static NavMeshBuilder Build(
            Geometry.PolyMesh polyMesh,
            Geometry.PolyMeshDetail polyMeshDetail,
            OffMeshConnection[] offMeshCons,
            float cellSize, float cellHeight, int vertsPerPoly, float maxClimb)
        {
            NavMeshBuilder result = new NavMeshBuilder();

            #region Classify off-mesh connection points

            BoundarySide[] offMeshSides = new BoundarySide[offMeshCons != null ? offMeshCons.Length * 2 : 0];
            int storedOffMeshConCount = 0;
            int offMeshConLinkCount = 0;

            if (offMeshCons != null && offMeshCons.Length > 0)
            {
                //find height bounds
                float hmin = float.MaxValue;
                float hmax = -float.MaxValue;

                if (polyMeshDetail != null)
                {
                    #region With detailed mesh

                    for (int i = 0; i < polyMeshDetail.VertCount; i++)
                    {
                        float h = polyMeshDetail.Verts[i].Y;
                        hmin = Math.Min(hmin, h);
                        hmax = Math.Max(hmax, h);
                    }

                    #endregion
                }
                else
                {
                    #region With mesh

                    for (int i = 0; i < polyMesh.VertCount; i++)
                    {
                        var iv = polyMesh.Verts[i];
                        float h = polyMesh.Bounds.Minimum.Y + iv.Y * cellHeight;
                        hmin = Math.Min(hmin, h);
                        hmax = Math.Max(hmax, h);
                    }

                    #endregion
                }

                hmin -= maxClimb;
                hmax += maxClimb;
                BoundingBox bounds = polyMesh.Bounds;
                bounds.Minimum.Y = hmin;
                bounds.Maximum.Y = hmax;

                for (int i = 0; i < offMeshCons.Length; i++)
                {
                    Vector3 p0 = offMeshCons[i].Pos0;
                    Vector3 p1 = offMeshCons[i].Pos1;

                    offMeshSides[i * 2 + 0] = BoundarySideExtensions.FromPoint(p0, bounds);
                    offMeshSides[i * 2 + 1] = BoundarySideExtensions.FromPoint(p1, bounds);

                    //off-mesh start position isn't touching mesh
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal)
                    {
                        if (p0.Y < bounds.Minimum.Y || p0.Y > bounds.Maximum.Y)
                        {
                            offMeshSides[i * 2 + 0] = 0;
                        }
                    }

                    //count number of links to allocate
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal) offMeshConLinkCount++;
                    if (offMeshSides[i * 2 + 1] == BoundarySide.Internal) offMeshConLinkCount++;
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal) storedOffMeshConCount++;
                }
            }

            #endregion

            #region Find portal edges

            int edgeCount = 0;
            int portalCount = 0;
            for (int i = 0; i < polyMesh.PolyCount; i++)
            {
                var p = polyMesh.Polys[i];
                for (int j = 0; j < vertsPerPoly; j++)
                {
                    if (p.Vertices[j] != Geometry.PolyMesh.NullId)
                    {
                        edgeCount++;

                        if (Geometry.PolyMesh.IsBoundaryEdge(p.NeighborEdges[j]))
                        {
                            int dir = p.NeighborEdges[j] % 16;

                            if (dir != 15) portalCount++;
                        }
                    }
                }
            }

            int maxLinkCount = edgeCount + portalCount * 2 + offMeshConLinkCount * 2;

            #endregion

            //Find unique detail vertices
            int uniqueDetailVertCount = 0;
            int detailTriCount = 0;
            if (polyMeshDetail != null)
            {
                #region With detailed mesh

                detailTriCount = polyMeshDetail.TrisCount;
                for (int i = 0; i < polyMesh.PolyCount; i++)
                {
                    int numDetailVerts = polyMeshDetail.Meshes[i].VertexCount;
                    int numPolyVerts = polyMesh.Polys[i].VertexCount;
                    uniqueDetailVertCount += numDetailVerts - numPolyVerts;
                }

                #endregion
            }
            else
            {
                #region With mesh

                uniqueDetailVertCount = 0;
                detailTriCount = 0;
                for (int i = 0; i < polyMesh.PolyCount; i++)
                {
                    int numPolyVerts = polyMesh.Polys[i].VertexCount;
                    uniqueDetailVertCount += numPolyVerts - 2;
                }

                #endregion
            }

            //store header
            //HACK TiledNavMesh should figure out the X/Y/layer instead of the user maybe?
            //result.header = new PathfindingCommon.NavMeshInfo();
            //header.X = 0;
            //header.Y = 0;
            //header.Layer = 0;
            //header.PolyCount = totPolyCount;
            //header.VertCount = totVertCount;
            //header.MaxLinkCount = maxLinkCount;
            //header.Bounds = polyMesh.Bounds;
            //header.DetailMeshCount = polyMesh.PolyCount;
            //header.DetailVertCount = uniqueDetailVertCount;
            //header.DetailTriCount = detailTriCount;
            //header.OffMeshBase = polyMesh.PolyCount;
            //header.WalkableHeight = settings.AgentHeight;
            //header.WalkableRadius = settings.AgentRadius;
            //header.WalkableClimb = maxClimb;
            //header.OffMeshConCount = storedOffMeshConCount;
            //header.BvNodeCount = settings.BuildBoundingVolumeTree ? polyMesh.PolyCount * 2 : 0;
            //header.BvQuantFactor = 1f / cellSize;

            //off-mesh connections stored as polygons, adjust values
            int offMeshVertsBase = polyMesh.VertCount;
            int offMeshPolyBase = polyMesh.PolyCount;
            int totPolyCount = polyMesh.PolyCount + storedOffMeshConCount;
            int totVertCount = polyMesh.VertCount + storedOffMeshConCount * 2;

            //Allocate data
            result.NavVerts = new Vector3[totVertCount];
            result.NavPolys = new Poly[totPolyCount];
            result.navDMeshes = new Geometry.PolyMeshDetail.MeshData[polyMesh.PolyCount];
            result.navDVerts = new Vector3[uniqueDetailVertCount];
            result.navDTris = new Geometry.PolyMeshDetail.TriangleData[detailTriCount];
            result.offMeshConnections = new OffMeshConnection[storedOffMeshConCount];

            #region Store vertices

            for (int i = 0; i < polyMesh.VertCount; i++)
            {
                var iv = polyMesh.Verts[i];
                result.NavVerts[i].X = polyMesh.Bounds.Minimum.X + iv.X * cellSize;
                result.NavVerts[i].Y = polyMesh.Bounds.Minimum.Y + iv.Y * cellHeight;
                result.NavVerts[i].Z = polyMesh.Bounds.Minimum.Z + iv.Z * cellSize;
            }

            #endregion

            #region Off-mesh link vertices

            if (offMeshCons != null && offMeshCons.Length > 0)
            {
                int n = 0;
                for (int i = 0; i < offMeshCons.Length; i++)
                {
                    //only store connections which start from this tile
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal)
                    {
                        result.NavVerts[offMeshVertsBase + (n * 2 + 0)] = offMeshCons[i].Pos0;
                        result.NavVerts[offMeshVertsBase + (n * 2 + 1)] = offMeshCons[i].Pos1;
                        n++;
                    }
                }
            }

            #endregion

            #region Store vertices

            for (int i = 0; i < polyMesh.PolyCount; i++)
            {
                result.NavPolys[i] = new Poly();
                result.NavPolys[i].VertCount = 0;
                result.NavPolys[i].Tag = polyMesh.Polys[i].Tag;
                result.NavPolys[i].Area = polyMesh.Polys[i].Area;
                result.NavPolys[i].PolyType = PolygonType.Ground;
                result.NavPolys[i].Verts = new int[vertsPerPoly];
                result.NavPolys[i].Neis = new int[vertsPerPoly];

                for (int j = 0; j < vertsPerPoly; j++)
                {
                    if (polyMesh.Polys[i].Vertices[j] != Geometry.PolyMesh.NullId)
                    {
                        result.NavPolys[i].Verts[j] = polyMesh.Polys[i].Vertices[j];
                        if (Geometry.PolyMesh.IsBoundaryEdge(polyMesh.Polys[i].NeighborEdges[j]))
                        {
                            //border or portal edge
                            int dir = polyMesh.Polys[i].NeighborEdges[j] % 16;
                            if (dir == 0xf) result.NavPolys[i].Neis[j] = 0; //border
                            else if (dir == 0) result.NavPolys[i].Neis[j] = Link.External | 4; //portal x-
                            else if (dir == 1) result.NavPolys[i].Neis[j] = Link.External | 2; //portal z+
                            else if (dir == 2) result.NavPolys[i].Neis[j] = Link.External | 0; //portal x+
                            else if (dir == 3) result.NavPolys[i].Neis[j] = Link.External | 6; //portal z-
                        }
                        else
                        {
                            //normal connection
                            result.NavPolys[i].Neis[j] = polyMesh.Polys[i].NeighborEdges[j] + 1;
                        }

                        result.NavPolys[i].VertCount++;
                    }
                }
            }

            #endregion

            #region Off-mesh connection vertices

            if (offMeshCons != null && offMeshCons.Length > 0)
            {
                int n = 0;
                for (int i = 0; i < offMeshCons.Length; i++)
                {
                    //only store connections which start from this tile
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal)
                    {
                        result.NavPolys[offMeshPolyBase + n] = new Poly();
                        result.NavPolys[offMeshPolyBase + n].VertCount = 2;
                        result.NavPolys[offMeshPolyBase + n].Verts = new int[vertsPerPoly];
                        result.NavPolys[offMeshPolyBase + n].Verts[0] = offMeshVertsBase + (n * 2 + 0);
                        result.NavPolys[offMeshPolyBase + n].Verts[1] = offMeshVertsBase + (n * 2 + 1);
                        result.NavPolys[offMeshPolyBase + n].Tag = offMeshCons[i].Flags;
                        result.NavPolys[offMeshPolyBase + n].Area = polyMesh.Polys[offMeshCons[i].Poly].Area;
                        result.NavPolys[offMeshPolyBase + n].PolyType = PolygonType.OffMeshConnection;
                        n++;
                    }
                }
            }

            #endregion

            //store detail meshes and vertices
            if (polyMeshDetail != null)
            {
                #region With detailed mesh

                int vbase = 0;
                List<Vector3> storedDetailVerts = new List<Vector3>();
                for (int i = 0; i < polyMesh.PolyCount; i++)
                {
                    int vb = polyMeshDetail.Meshes[i].VertexIndex;
                    int numDetailVerts = polyMeshDetail.Meshes[i].VertexCount;
                    int numPolyVerts = result.NavPolys[i].VertCount;
                    result.navDMeshes[i].VertexIndex = vbase;
                    result.navDMeshes[i].VertexCount = numDetailVerts - numPolyVerts;
                    result.navDMeshes[i].TriangleIndex = polyMeshDetail.Meshes[i].TriangleIndex;
                    result.navDMeshes[i].TriangleCount = polyMeshDetail.Meshes[i].TriangleCount;

                    //Copy detail vertices 
                    //first 'nv' verts are equal to nav poly verts
                    //the rest are detail verts
                    for (int j = 0; j < result.navDMeshes[i].VertexCount; j++)
                    {
                        storedDetailVerts.Add(polyMeshDetail.Verts[vb + numPolyVerts + j]);
                    }

                    vbase += numDetailVerts - numPolyVerts;
                }

                result.navDVerts = storedDetailVerts.ToArray();

                //store triangles
                for (int j = 0; j < polyMeshDetail.TrisCount; j++)
                {
                    result.navDTris[j] = polyMeshDetail.Tris[j];
                }

                #endregion
            }
            else
            {
                #region With mesh

                //create dummy detail mesh by triangulating polys
                int tbase = 0;
                for (int i = 0; i < polyMesh.PolyCount; i++)
                {
                    int numPolyVerts = result.NavPolys[i].VertCount;
                    result.navDMeshes[i].VertexIndex = 0;
                    result.navDMeshes[i].VertexCount = 0;
                    result.navDMeshes[i].TriangleIndex = tbase;
                    result.navDMeshes[i].TriangleCount = numPolyVerts - 2;

                    //triangulate polygon
                    for (int j = 2; j < numPolyVerts; j++)
                    {
                        result.navDTris[tbase].VertexHash0 = 0;
                        result.navDTris[tbase].VertexHash1 = j - 1;
                        result.navDTris[tbase].VertexHash2 = j;

                        //bit for each edge that belongs to the poly boundary
                        result.navDTris[tbase].Flags = 1 << 2;
                        if (j == 2) result.navDTris[tbase].Flags |= 1 << 0;
                        if (j == numPolyVerts - 1) result.navDTris[tbase].Flags |= 1 << 4;

                        tbase++;
                    }
                }

                #endregion
            }

            #region Store off-mesh connections

            if (offMeshCons != null && offMeshCons.Length > 0)
            {
                int n = 0;
                for (int i = 0; i < result.offMeshConnections.Length; i++)
                {
                    //only store connections which start from this tile
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal)
                    {
                        result.offMeshConnections[n] = new OffMeshConnection();

                        result.offMeshConnections[n].Poly = offMeshPolyBase + n;

                        //copy connection end points
                        result.offMeshConnections[n].Pos0 = offMeshCons[i].Pos0;
                        result.offMeshConnections[n].Pos1 = offMeshCons[i].Pos1;

                        result.offMeshConnections[n].Radius = offMeshCons[i].Radius;
                        result.offMeshConnections[n].Flags = offMeshCons[i].Flags;
                        result.offMeshConnections[n].Side = offMeshSides[i * 2 + 1];
                        result.offMeshConnections[n].Tag = offMeshCons[i].Tag;

                        n++;
                    }
                }
            }

            #endregion

            return result;
        }

        //private PathfindingCommon.NavMeshInfo header;
        public Vector3[] NavVerts;
        public Poly[] NavPolys;
        private Geometry.PolyMeshDetail.MeshData[] navDMeshes;
        private Vector3[] navDVerts;
        private Geometry.PolyMeshDetail.TriangleData[] navDTris;
        //private BVTree navBvTree;
        private OffMeshConnection[] offMeshConnections;
    }
    /// <summary>
    /// A set of flags that define properties about an off-mesh connection.
    /// </summary>
    [Flags]
    public enum OffMeshConnectionFlags : byte
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0x0,
        /// <summary>
        /// The connection is bi-directional.
        /// </summary>
        Bidirectional = 0x1
    }
    /// <summary>
    /// An offmesh connection links two polygons, which are not directly adjacent, but are accessibly through
    /// other means (jumping, climbing, etc...).
    /// </summary>
    public class OffMeshConnection
    {
        /// <summary>
        /// Gets or sets the first endpoint of the connection
        /// </summary>
        public Vector3 Pos0 { get; set; }
        /// <summary>
        /// Gets or sets the second endpoint of the connection
        /// </summary>
        public Vector3 Pos1 { get; set; }
        /// <summary>
        /// Gets or sets the radius
        /// </summary>
        public float Radius { get; set; }
        /// <summary>
        /// Gets or sets the polygon's index
        /// </summary>
        public int Poly { get; set; }
        /// <summary>
        /// Gets or sets the polygon flag
        /// </summary>
        public OffMeshConnectionFlags Flags { get; set; }
        /// <summary>
        /// Gets or sets the endpoint's side
        /// </summary>
        public BoundarySide Side { get; set; }
        /// <summary>
        /// Gets or sets user data for this connection.
        /// </summary>
        public object Tag { get; set; }
    }
    /// <summary>
    /// An enumeration of the different places a point can be relative to a rectangular boundary on the XZ plane.
    /// </summary>
    public enum BoundarySide : byte
    {
        /// <summary>
        /// Not outside of the defined boundary.
        /// </summary>
        Internal = 0xff,
        /// <summary>
        /// Only outside of the defined bondary on the X axis, in the positive direction.
        /// </summary>
        PlusX = 0,
        /// <summary>
        /// Outside of the defined boundary on both the X and Z axes, both in the positive direction.
        /// </summary>
        PlusXPlusZ = 1,
        /// <summary>
        /// Only outside of the defined bondary on the Z axis, in the positive direction.
        /// </summary>
        PlusZ = 2,
        /// <summary>
        /// Outside of the defined boundary on both the X and Z axes, in the negative and positive directions respectively.
        /// </summary>
        MinusXPlusZ = 3,
        /// <summary>
        /// Only outside of the defined bondary on the X axis, in the negative direction.
        /// </summary>
        MinusX = 4,
        /// <summary>
        /// Outside of the defined boundary on both the X and Z axes, both in the negative direction.
        /// </summary>
        MinusXMinusZ = 5,
        /// <summary>
        /// Only outside of the defined bondary on the Z axis, in the negative direction.
        /// </summary>
        MinusZ = 6,
        /// <summary>
        /// Outside of the defined boundary on both the X and Z axes, in the positive and negative directions respectively.
        /// </summary>
        PlusXMinusZ = 7
    }
    /// <summary>
    /// Extension methods for the <see cref="BoundarySide"/> enumeration.
    /// </summary>
    public static class BoundarySideExtensions
    {
        /// <summary>
        /// Gets the side in the exact opposite direction as a specified side.
        /// </summary>
        /// <remarks>
        /// The value <see cref="BoundarySide.Internal"/> will always return <see cref="BoundarySide.Internal"/>.
        /// </remarks>
        /// <param name="side">A side.</param>
        /// <returns>The opposite side.</returns>
        public static BoundarySide GetOpposite(this BoundarySide side)
        {
            if (side == BoundarySide.Internal) return BoundarySide.Internal;

            return (BoundarySide)((int)(side + 4) % 8);
        }
        /// <summary>
        /// Gets the boundary side of a point relative to a bounding box.
        /// </summary>
        /// <param name="pt">A point.</param>
        /// <param name="bounds">A bounding box.</param>
        /// <returns>The point's position relative to the bounding box.</returns>
        public static BoundarySide FromPoint(Vector3 pt, BoundingBox bounds)
        {
            const int PlusX = 0x1;
            const int PlusZ = 0x2;
            const int MinusX = 0x4;
            const int MinusZ = 0x8;

            int outcode = 0;
            outcode |= (pt.X >= bounds.Maximum.X) ? PlusX : 0;
            outcode |= (pt.Z >= bounds.Maximum.Z) ? PlusZ : 0;
            outcode |= (pt.X < bounds.Minimum.X) ? MinusX : 0;
            outcode |= (pt.Z < bounds.Minimum.Z) ? MinusZ : 0;

            switch (outcode)
            {
                case PlusX:
                    return BoundarySide.PlusX;

                case PlusX | PlusZ:
                    return BoundarySide.PlusXPlusZ;

                case PlusZ:
                    return BoundarySide.PlusZ;

                case MinusX | PlusZ:
                    return BoundarySide.MinusXPlusZ;

                case MinusX:
                    return BoundarySide.MinusX;

                case MinusX | MinusZ:
                    return BoundarySide.MinusXMinusZ;

                case MinusZ:
                    return BoundarySide.MinusZ;

                case PlusX | MinusZ:
                    return BoundarySide.PlusXMinusZ;

                default:
                    return BoundarySide.Internal;
            }
        }
    }
    /// <summary>
    /// A link is formed between two polygons in a TiledNavMesh
    /// </summary>
    public class Link
    {
        /// <summary>
        /// Entity links to external entity.
        /// </summary>
        public const int External = unchecked((int)0x80000000);
        /// <summary>
        /// Doesn't link to anything.
        /// </summary>
        public const int Null = unchecked((int)0xffffffff);

        public static bool IsExternal(int link)
        {
            return (link & Link.External) != 0;
        }

        /// <summary>
        /// Gets or sets the neighbor reference (the one it's linked to)
        /// </summary>
        public int Reference { get; set; }
        /// <summary>
        /// Gets or sets the index of polygon edge
        /// </summary>
        public int Edge { get; set; }
        /// <summary>
        /// Gets or sets the polygon side
        /// </summary>
        public BoundarySide Side { get; set; }
        /// <summary>
        /// Gets or sets the minimum Vector3 of the bounding box
        /// </summary>
        public int BMin { get; set; }
        /// <summary>
        /// Gets or sets the maximum Vector3 of the bounding box
        /// </summary>
        public int BMax { get; set; }
    }
}
