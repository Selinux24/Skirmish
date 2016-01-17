using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    public class NavMesh
    {
        List<Polygon> poligons = new List<Polygon>();

        public static NavMesh Build(Triangle[] triangles, float size, float angle = MathUtil.PiOverFour)
        {
            NavMesh result = new NavMesh();

            //Eliminar los triángulos por ángulo

            //Identificar los bordes de los triángulos eliminados
            //Son bordes los que coinciden con triángulos no eliminados

            //Generar polígonos con los bordes

            var tris = Array.FindAll(triangles, t => t.Inclination >= angle);
            if (tris != null && tris.Length > 0)
            {
                Array.ForEach(tris, t =>
                {
                    //Buscar polígono para el triángulo
                    var poly = result.poligons.Find(p => p.Inside(new Vector2(t.Point1.X, t.Point1.Z)));
                });
            }

            return result;
        }
    }

    public class Polygon
    {
        private List<Vector2> vertexList = new List<Vector2>();

        public Vector2[] VertexList
        {
            get
            {
                return this.vertexList.ToArray();
            }
            set
            {
                this.vertexList.Clear();

                if (value != null && value.Length > 0)
                {
                    this.vertexList.AddRange(value);
                }
            }
        }

        public void Add(Vector2 vertex1, Vector2 vertex2, Vector2 vertex3)
        {
            //Si un segmento coincide

            //Si dos segmentos coinciden

            //Si tres segmentos coinciden no añadir nada

            //Si un vértice coincide

            //Si dos vértices coinciden

            //Si tres vértices coinciden
        }

        public bool Inside(Vector2 position, bool toleranceOnOutside = true)
        {
            Vector2 point = position;

            const float epsilon = 0.5f;

            bool inside = false;

            // Must have 3 or more edges
            if (this.vertexList.Count < 3) return false;

            Vector2 oldPoint = this.vertexList[this.vertexList.Count - 1];
            float oldSqDist = Vector2.DistanceSquared(oldPoint, point);

            for (int i = 0; i < this.vertexList.Count; i++)
            {
                Vector2 newPoint = this.vertexList[i];
                float newSqDist = Vector2.DistanceSquared(newPoint, point);

                if (oldSqDist + newSqDist + 2.0f * System.Math.Sqrt(oldSqDist * newSqDist) - Vector2.DistanceSquared(newPoint, oldPoint) < epsilon)
                    return toleranceOnOutside;

                Vector2 left;
                Vector2 right;
                if (newPoint.X > oldPoint.X)
                {
                    left = oldPoint;
                    right = newPoint;
                }
                else
                {
                    left = newPoint;
                    right = oldPoint;
                }

                if (left.X < point.X && point.X <= right.X && (point.Y - left.Y) * (right.X - left.X) < (right.Y - left.Y) * (point.X - left.X))
                    inside = !inside;

                oldPoint = newPoint;
                oldSqDist = newSqDist;
            }

            return inside;
        }
    }

    public enum OrientationEnum : int
    {
        NONE = 0,
        TPPL_CCW = 1,
        TPPL_CW = -1
    }

    /// <summary>
    /// Polygon implemented as an array of points with a 'hole' flag
    /// </summary>
    public class TPPLPoly
    {
        /// <summary>
        /// Point array
        /// </summary>
        public Vector2[] Points { get; protected set; }
        /// <summary>
        /// Number of points
        /// </summary>
        public int Count { get; protected set; }
        /// <summary>
        /// Gets or sets whether the polygon has a hole or not
        /// </summary>
        public bool Hole { get; set; }
        /// <summary>
        /// Gets or sets the orientation of the polygon
        /// </summary>
        /// <returns>
        /// TPPL_CCW : polygon vertices are in counter-clockwise order
        /// TPPL_CW : polygon vertices are in clockwise order
        /// NONE : the polygon has no (measurable) area
        /// </returns>
        public OrientationEnum Orientation
        {
            get
            {
                int i1, i2;
                float area = 0;
                for (i1 = 0; i1 < Count; i1++)
                {
                    i2 = i1 + 1;
                    if (i2 == Count) i2 = 0;
                    area += Points[i1].X * Points[i2].Y - Points[i1].Y * Points[i2].X;
                }
                if (area > 0) return OrientationEnum.TPPL_CCW;
                if (area < 0) return OrientationEnum.TPPL_CW;
                return OrientationEnum.NONE;
            }
            set
            {
                OrientationEnum polyorientation = this.Orientation;
                if (polyorientation != OrientationEnum.NONE && (polyorientation != value))
                {
                    this.Invert();
                }
            }
        }
        /// <summary>
        /// Gets the specified point by index
        /// </summary>
        /// <param name="i">Point index</param>
        /// <returns>Returns the specified point by index</returns>
        public Vector2 this[int i]
        {
            get
            {
                return this.Points[i];
            }
            set
            {
                this.Points[i] = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public TPPLPoly()
        {
            this.Points = null;
            this.Count = 0;
            this.Hole = false;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="src">Source poly</param>
        public TPPLPoly(TPPLPoly src)
        {
            this.Hole = src.Hole;
            this.Count = src.Count;
            this.Points = new Vector2[Count];
            Array.Copy(src.Points, this.Points, Count);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="numpoints">Number of points</param>
        public TPPLPoly(int numpoints)
        {
            this.Init(numpoints);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p1">First point</param>
        /// <param name="p2">Second point</param>
        /// <param name="p3">Third point</param>
        public TPPLPoly(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            this.Triangle(p1, p2, p3);
        }

        /// <summary>
        /// Adds a point to collection
        /// </summary>
        public bool AddPoint(Vector2 point, bool convexResult = true)
        {
            if (this.Count == 0)
            {
                this.Points = new Vector2[1] { point };
                return true;
            }
            else
            {
                if (!Array.Exists(this.Points, p => p == point))
                {
                    Vector2[] copy = new Vector2[this.Count + 1];
                    Array.Copy(this.Points, copy, this.Count);
                    copy[this.Count] = point;

                    if (!convexResult || copy.Length < 3)
                    {
                        this.Points = copy;
                        this.Count++;
                        return true;
                    }
                    else
                    {
                        if (IsConvex(copy))
                        {
                            this.Points = copy;
                            this.Count++;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                return false;
            }
        }

        public static TPPLPoly Merge(TPPLPoly source, TPPLPoly poly, bool convexResult = true)
        {
            TPPLPoly newpoly = new TPPLPoly()
            {
                Points = source.Points,
                Count = source.Count,
                Hole = source.Hole,
            };

            for (int i = 0; i < poly.Count; i++)
            {
                newpoly.AddPoint(poly[i], false);
            }

            if (newpoly.IsConvex())
            {
                return newpoly;
            }

            return null;
        }
        /// <summary>
        /// Clears the polygon points
        /// </summary>
        public void Clear()
        {
            this.Points = null;
            this.Hole = false;
            this.Count = 0;
        }
        /// <summary>
        /// Inits the polygon with numpoints vertices
        /// </summary>
        /// <param name="numpoints">Number of points</param>
        public void Init(int numpoints)
        {
            this.Clear();
            this.Count = numpoints;
            this.Points = new Vector2[numpoints];
        }
        /// <summary>
        /// Creates a triangle with points p1,p2,p3
        /// </summary>
        /// <param name="p1">First point</param>
        /// <param name="p2">Second point</param>
        /// <param name="p3">Third point</param>
        public void Triangle(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            this.Init(3);
            this.Points[0] = p1;
            this.Points[1] = p2;
            this.Points[2] = p3;
        }
        /// <summary>
        /// Inverts the orfer of vertices
        /// </summary>
        public void Invert()
        {
            int i;
            Vector2[] invpoints;

            invpoints = new Vector2[Count];
            for (i = 0; i < Count; i++)
            {
                invpoints[i] = this.Points[Count - i - 1];
            }

            this.Points = invpoints;
        }

        public bool IsConvex()
        {
            return IsConvex(this.Points);
        }

        public bool ShareAnEdgeWith(TPPLPoly poly)
        {
            for (int i1 = 0; i1 < this.Points.Length; i1++)
            {
                int j1 = Array.IndexOf(poly.Points, this.Points[i1]);

                if (j1 >= 0)
                {
                    int i2 = i1 + 1 < this.Points.Length ? i1 + 1 : 0;
                    int j2 = j1 - 1 >= 0 ? j1 - 1 : poly.Points.Length - 1;

                    if (this.Points[i2] == poly.Points[j2]) return true;
                }
            }

            return false;
        }

        private static bool IsConvex(Vector2[] points)
        {
            int numreflex = 0;
            for (int i11 = 0; i11 < points.Length; i11++)
            {
                int i12, i13;

                if (i11 == 0) i12 = points.Length - 1; else i12 = i11 - 1;
                if (i11 == (points.Length - 1)) i13 = 0; else i13 = i11 + 1;

                if (IsReflex(points[i12], points[i11], points[i13]))
                {
                    numreflex = 1;
                    break;
                }
            }

            return numreflex == 0;
        }

        private static bool IsReflex(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float tmp = (p3.Y - p1.Y) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Y - p1.Y);

            return tmp < 0;
        }

        public override string ToString()
        {
            string text = string.Format("Vertices: {0};", this.Count);
            if (this.Count > 0)
            {
                text += " |";
                Array.ForEach(this.Points, p => text += string.Format("{0}|", p));
            }

            return text;
        }
    };

    public class TPPLPartition
    {
        public static bool MergeConvex(List<TPPLPoly> inpolys, out List<TPPLPoly> outpolys)
        {
            //TODO: poly merge algorithm
            // - Find longest shared edge poligons and try merge
            // - If result still convex, merge done
            // - Repeat

            outpolys = new List<TPPLPoly>();

            List<TPPLPoly> mergedPolys = new List<TPPLPoly>();

            bool merged = false;

            if (inpolys != null && inpolys.Count > 1)
            {
                for (int i = 0; i < inpolys.Count; i++)
                {
                    if (mergedPolys.Contains(inpolys[i])) continue;

                    TPPLPoly newpoly = inpolys[i];

                    for (int j = i + 1; j < inpolys.Count; j++)
                    {
                        if (mergedPolys.Contains(inpolys[j])) continue;

                        if (!newpoly.ShareAnEdgeWith(inpolys[j])) continue;

                        var mergedpoly = TPPLPoly.Merge(newpoly, inpolys[j]);
                        if (mergedpoly != null)
                        {
                            mergedPolys.Add(inpolys[j]);
                            newpoly = mergedpoly;
                            merged = true;
                        }
                    }

                    outpolys.Add(newpoly);
                }

                if (merged)
                {
                    return MergeConvex(outpolys, out outpolys);
                }
                else
                {
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
        public static bool RemoveHoles(List<TPPLPoly> inpolys, out List<TPPLPoly> outpolys)
        {
            outpolys = new List<TPPLPoly>();

            //check for trivial case (no holes)
            if (!inpolys.Exists(p => p.Hole == true))
            {
                outpolys.AddRange(inpolys);

                return true;
            }

            List<TPPLPoly> polys = inpolys;
            bool hasholes = false;
            TPPLPoly holeiter = null;
            int holepointindex = 0;
            while (true)
            {
                //find the hole point with the largest x
                hasholes = false;
                foreach (var iter in polys)
                {
                    if (!iter.Hole) continue;

                    if (!hasholes)
                    {
                        hasholes = true;
                        holeiter = iter;
                        holepointindex = 0;
                    }

                    for (int i = 0; i < iter.Count; i++)
                    {
                        if (iter[i].X > holeiter[holepointindex].X)
                        {
                            holeiter = iter;
                            holepointindex = i;
                        }
                    }
                }

                if (!hasholes) break;

                TPPLPoly polyiter = null;
                Vector2 holepoint = holeiter[holepointindex];
                Vector2 bestpolypoint = Vector2.Zero;
                bool pointfound = false;
                int polypointindex = 0;
                foreach (var iter in polys)
                {
                    if (iter.Hole) continue;

                    for (int i = 0; i < iter.Count; i++)
                    {
                        if (iter[i].X <= holepoint.X) continue;

                        if (!InCone(
                            iter[(i + iter.Count - 1) % (iter.Count)],
                            iter[i],
                            iter[(i + 1) % (iter.Count)],
                            holepoint))
                        {
                            continue;
                        }

                        Vector2 polypoint = iter[i];
                        if (pointfound)
                        {
                            Vector2 v1 = Vector2.Normalize(polypoint - holepoint);
                            Vector2 v2 = Vector2.Normalize(bestpolypoint - holepoint);
                            if (v2.X > v1.X) continue;
                        }

                        bool pointvisible = true;
                        foreach (var iter2 in polys)
                        {
                            if (iter2.Hole) continue;

                            for (int i2 = 0; i2 < iter2.Count; i2++)
                            {
                                Vector2 linep1 = iter2[i2];
                                Vector2 linep2 = iter2[(i2 + 1) % (iter2.Count)];
                                if (Intersects(holepoint, polypoint, linep1, linep2))
                                {
                                    pointvisible = false;
                                    break;
                                }
                            }
                            if (!pointvisible) break;
                        }
                        if (pointvisible)
                        {
                            pointfound = true;
                            bestpolypoint = polypoint;
                            polyiter = iter;
                            polypointindex = i;
                        }
                    }
                }

                if (!pointfound) return false;

                {
                    TPPLPoly newpoly = new TPPLPoly(holeiter.Count + polyiter.Count + 2);
                    int i2 = 0;
                    for (int i = 0; i <= polypointindex; i++)
                    {
                        newpoly[i2] = polyiter[i];
                        i2++;
                    }
                    for (int i = 0; i <= holeiter.Count; i++)
                    {
                        newpoly[i2] = holeiter[(i + holepointindex) % holeiter.Count];
                        i2++;
                    }
                    for (int i = polypointindex; i < polyiter.Count; i++)
                    {
                        newpoly[i2] = polyiter[i];
                        i2++;
                    }

                    polys.Add(newpoly);
                }
            }

            foreach (var iter in polys)
            {
                outpolys.Add(iter);
            }

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
        public static bool ConvexPartition_HM(List<TPPLPoly> inpolys, out List<TPPLPoly> parts)
        {
            parts = new List<TPPLPoly>();

            List<TPPLPoly> outpolys;
            if (!RemoveHoles(inpolys, out outpolys)) return false;

            foreach (var iter in outpolys)
            {
                List<TPPLPoly> polyParts;
                if (!ConvexPartition_HM(iter, out polyParts)) return false;
                if (polyParts.Count > 0) parts.AddRange(polyParts);
            }

            return true;
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
        private static bool ConvexPartition_HM(TPPLPoly poly, out List<TPPLPoly> parts)
        {
            parts = new List<TPPLPoly>();

            //check if the poly is already convex
            if (poly.IsConvex())
            {
                parts.Add(poly);
                return true;
            }

            List<TPPLPoly> triangles;
            if (!Triangulate_EC(poly, out triangles)) return false;

            for (int iter1 = 0; iter1 < triangles.Count; iter1++)
            {
                TPPLPoly poly1 = triangles[iter1];
                for (int i11 = 0; i11 < poly1.Count; i11++)
                {
                    Vector2 d1 = poly1[i11];
                    int i12 = (i11 + 1) % (poly1.Count);
                    Vector2 d2 = poly1[i12];

                    TPPLPoly poly2 = null;
                    int i21 = -1;
                    int i22 = -1;
                    bool isdiagonal = false;
                    for (int iter2 = iter1; iter2 < triangles.Count(); iter2++)
                    {
                        if (iter1 == iter2) continue;

                        poly2 = triangles[iter2];

                        for (i21 = 0; i21 < poly2.Count; i21++)
                        {
                            if ((d2.X != poly2[i21].X) || (d2.Y != poly2[i21].Y)) continue;

                            i22 = (i21 + 1) % (poly2.Count);

                            if ((d1.X != poly2[i22].X) || (d1.Y != poly2[i22].Y)) continue;

                            isdiagonal = true;

                            break;
                        }
                        if (isdiagonal) break;
                    }
                    if (!isdiagonal) continue;

                    Vector2 p1, p2, p3;
                    int i13, i23;

                    if (i11 == 0) i13 = poly1.Count - 1; else i13 = i11 - 1;
                    if (i22 == (poly2.Count - 1)) i23 = 0; else i23 = i22 + 1;
                    p1 = poly1[i13];
                    p2 = poly1[i11];
                    p3 = poly2[i23];
                    if (!IsConvex(p1, p2, p3)) continue;

                    if (i12 == (poly1.Count - 1)) i13 = 0; else i13 = i12 + 1;
                    if (i21 == 0) i23 = poly2.Count - 1; else i23 = i21 - 1;
                    p1 = poly2[i23];
                    p2 = poly1[i12];
                    p3 = poly1[i13];
                    if (!IsConvex(p1, p2, p3)) continue;

                    TPPLPoly newpoly = new TPPLPoly(poly1.Count + poly2.Count - 2);
                    int k = 0;
                    for (int j = i12; j != i11; j = (j + 1) % (poly1.Count))
                    {
                        newpoly[k] = poly1[j];
                        k++;
                    }
                    for (int j = i22; j != i21; j = (j + 1) % (poly2.Count))
                    {
                        newpoly[k] = poly2[j];
                        k++;
                    }

                    poly1 = newpoly;
                    i11 = -1;

                    continue;
                }
            }

            foreach (var iter1 in triangles)
            {
                parts.Add(iter1);
            }

            return true;
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
        private static bool Triangulate_EC(TPPLPoly poly, out List<TPPLPoly> triangles)
        {
            triangles = new List<TPPLPoly>();

            if (poly.Count < 3) return false;
            if (poly.Count == 3)
            {
                triangles.Add(poly);
                return true;
            }

            int numvertices = poly.Count;

            PartitionVertex[] vertices = new PartitionVertex[numvertices];
            for (int i = 0; i < numvertices; i++)
            {
                vertices[i] = new PartitionVertex();
            }

            for (int i = 0; i < numvertices; i++)
            {
                vertices[i].IsActive = true;
                vertices[i].P = poly[i];

                if (i == (numvertices - 1))
                {
                    vertices[i].Next = vertices[0];
                }
                else
                {
                    vertices[i].Next = vertices[i + 1];
                }

                if (i == 0)
                {
                    vertices[i].Previous = vertices[numvertices - 1];
                }
                else
                {
                    vertices[i].Previous = vertices[i - 1];
                }
            }
            for (int i = 0; i < numvertices; i++)
            {
                UpdateVertex(vertices[i], vertices, numvertices);
            }

            for (int i = 0; i < numvertices - 3; i++)
            {
                PartitionVertex ear = null;
                bool earfound = false;

                //find the most extruded ear
                for (int j = 0; j < numvertices; j++)
                {
                    if (!vertices[j].IsActive) continue;
                    if (!vertices[j].IsEar) continue;
                    if (!earfound)
                    {
                        earfound = true;
                        ear = vertices[j];
                    }
                    else
                    {
                        if (vertices[j].Angle > ear.Angle)
                        {
                            ear = vertices[j];
                        }
                    }
                }
                if (!earfound)
                {
                    vertices = null;
                    return false;
                }

                TPPLPoly triangle = new TPPLPoly(ear.Previous.P, ear.P, ear.Next.P);
                triangles.Add(triangle);

                ear.IsActive = false;
                ear.Previous.Next = ear.Next;
                ear.Next.Previous = ear.Previous;

                if (i == numvertices - 4) break;

                UpdateVertex(ear.Previous, vertices, numvertices);
                UpdateVertex(ear.Next, vertices, numvertices);
            }
            for (int i = 0; i < numvertices; i++)
            {
                if (vertices[i].IsActive)
                {
                    TPPLPoly triangle = new TPPLPoly(vertices[i].Previous.P, vertices[i].P, vertices[i].Next.P);
                    triangles.Add(triangle);
                    break;
                }
            }

            vertices = null;
            return true;
        }

        private static void UpdateVertex(PartitionVertex v, PartitionVertex[] vertices, int numvertices)
        {
            PartitionVertex v1 = v.Previous;
            PartitionVertex v3 = v.Next;

            Vector2 vec1 = Vector2.Normalize(v1.P - v.P);
            Vector2 vec3 = Vector2.Normalize(v3.P - v.P);

            v.Angle = vec1.X * vec3.X + vec1.Y * vec3.Y;
            v.IsConvex = IsConvex(v1.P, v.P, v3.P);

            if (v.IsConvex)
            {
                v.IsEar = true;
                for (int i = 0; i < numvertices; i++)
                {
                    if ((vertices[i].P.X == v.P.X) && (vertices[i].P.Y == v.P.Y)) continue;
                    if ((vertices[i].P.X == v1.P.X) && (vertices[i].P.Y == v1.P.Y)) continue;
                    if ((vertices[i].P.X == v3.P.X) && (vertices[i].P.Y == v3.P.Y)) continue;
                    if (IsInside(v1.P, v.P, v3.P, vertices[i].P))
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

        private static bool Intersects(Vector2 p11, Vector2 p12, Vector2 p21, Vector2 p22)
        {
            if ((p11.X == p21.X) && (p11.Y == p21.Y)) return false;
            if ((p11.X == p22.X) && (p11.Y == p22.Y)) return false;
            if ((p12.X == p21.X) && (p12.Y == p21.Y)) return false;
            if ((p12.X == p22.X) && (p12.Y == p22.Y)) return false;

            Vector2 v1ort, v2ort, v;
            float dot11, dot12, dot21, dot22;

            v1ort.X = p12.Y - p11.Y;
            v1ort.Y = p11.X - p12.X;

            v2ort.X = p22.Y - p21.Y;
            v2ort.Y = p21.X - p22.X;

            v = p21 - p11;
            dot21 = v.X * v1ort.X + v.Y * v1ort.Y;
            v = p22 - p11;
            dot22 = v.X * v1ort.X + v.Y * v1ort.Y;

            v = p11 - p21;
            dot11 = v.X * v2ort.X + v.Y * v2ort.Y;
            v = p12 - p21;
            dot12 = v.X * v2ort.X + v.Y * v2ort.Y;

            if (dot11 * dot12 > 0) return false;
            if (dot21 * dot22 > 0) return false;

            return true;
        }

        private static bool IsInside(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
        {
            if (IsConvex(p1, p, p2)) return false;
            if (IsConvex(p2, p, p3)) return false;
            if (IsConvex(p3, p, p1)) return false;
            return true;
        }
        private static bool InCone(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
        {
            bool convex = IsConvex(p1, p2, p3);
            if (convex)
            {
                if (!IsConvex(p1, p2, p)) return false;
                if (!IsConvex(p2, p3, p)) return false;
                return true;
            }
            else
            {
                if (IsConvex(p1, p2, p)) return true;
                if (IsConvex(p2, p3, p)) return true;
                return false;
            }
        }
        private static bool IsConvex(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float tmp = (p3.Y - p1.Y) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Y - p1.Y);

            return tmp > 0;
        }
    }

    public class PartitionVertex
    {
        public bool IsActive;
        public bool IsConvex;
        public bool IsEar;

        public Vector2 P;
        public float Angle;
        public PartitionVertex Previous;
        public PartitionVertex Next;
    };
}
