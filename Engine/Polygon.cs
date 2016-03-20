using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Polygon class
    /// </summary>
    public class Polygon
    {
        /// <summary>
        /// Shared edge struct
        /// </summary>
        public struct SharedEdge
        {
            /// <summary>
            /// First edge point index in first polygon
            /// </summary>
            public int FirstPoint1;
            /// <summary>
            /// Second edge point index in first polygon
            /// </summary>
            public int FirstPoint2;
            /// <summary>
            /// First edge point index in second polygon
            /// </summary>
            public int SecondPoint1;
            /// <summary>
            /// Second edge point index in second polygon
            /// </summary>
            public int SecondPoint2;
        }

        /// <summary>
        /// Generates a polygon from a triangle
        /// </summary>
        /// <param name="tri">Triangle</param>
        /// <param name="orientation">Orientation for the new polygon</param>
        /// <returns>Returns the new generated polygon</returns>
        public static Polygon FromTriangle(Triangle tri, GeometricOrientation orientation = GeometricOrientation.CounterClockwise)
        {
            Polygon poly = new Polygon(tri.Point1, tri.Point2, tri.Point3);

            poly.Orientation = orientation;

            return poly;
        }
        /// <summary>
        /// Generates a polygon array from a triangle array
        /// </summary>
        /// <param name="tris">Triangle array</param>
        /// <param name="orientation">Orientation for the new polygons of the array</param>
        /// <returns>Returns the new generated polygon array</returns>
        public static Polygon[] FromTriangleList(Triangle[] tris, GeometricOrientation orientation = GeometricOrientation.CounterClockwise)
        {
            Polygon[] polys = new Polygon[tris.Length];

            for (int i = 0; i < tris.Length; i++)
            {
                polys[i] = Polygon.FromTriangle(tris[i], orientation);
            }

            return polys;
        }
        /// <summary>
        /// Gets the geometric orientarion of the specified polygon definition array
        /// </summary>
        /// <param name="points">Point array</param>
        /// <returns>Returns the geometric orientarion of the point array</returns>
        public static GeometricOrientation GetOrientation(Vector3[] points)
        {
            float area = 0;
            for (int i1 = 0; i1 < points.Length; i1++)
            {
                int i2 = i1 + 1;
                if (i2 == points.Length) i2 = 0;

                area += points[i1].X * points[i2].Z - points[i1].Z * points[i2].X;
            }
            if (area > 0) return GeometricOrientation.CounterClockwise;
            if (area < 0) return GeometricOrientation.Clockwise;

            return GeometricOrientation.None;
        }
        /// <summary>
        /// Gets whether the polygon definied by the point array is convex or not
        /// </summary>
        /// <param name="points">Polygon definition array</param>
        /// <returns>Returns true if the polygon defined by the point array is convex</returns>
        public static bool IsConvex(Vector3[] points)
        {
            int numreflex = 0;
            for (int i11 = 0; i11 < points.Length; i11++)
            {
                int i12 = i11 == 0 ? points.Length - 1 : i11 - 1;
                int i13 = i11 == (points.Length - 1) ? 0 : i11 + 1;

                if (GeometryUtil.IsReflex(points[i12], points[i11], points[i13]))
                {
                    numreflex = 1;
                    break;
                }
            }

            return numreflex == 0;
        }
        /// <summary>
        /// Merge two polygons
        /// </summary>
        /// <param name="first">First polygon</param>
        /// <param name="second">Second polygon</param>
        /// <param name="mergeConvex">Sets if the new polygon must be convex or not</param>
        /// <param name="mergedPolygon">The merged polygon</param>
        /// <returns>Returns true if the merge operation finished correctly</returns>
        public static bool Merge(Polygon first, Polygon second, bool mergeConvex, out Polygon mergedPolygon)
        {
            mergedPolygon = null;

            SharedEdge[] sharedEdges;
            if (GetSharedEdges(first, second, out sharedEdges))
            {
                Polygon newpoly = new Polygon(first);
                if (newpoly.Merge(second, sharedEdges, mergeConvex))
                {
                    mergedPolygon = newpoly;
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets the shared edges between the polygons
        /// </summary>
        /// <param name="first">First polygon</param>
        /// <param name="second">Second polygon</param>
        /// <param name="sharedEdges">Returns a shared edge array</param>
        /// <returns>Returns true if the polygons share edges</returns>
        public static bool GetSharedEdges(Polygon first, Polygon second, out SharedEdge[] sharedEdges)
        {
            bool result = false;

            sharedEdges = null;

            List<SharedEdge> resultList = new List<SharedEdge>();

            for (int fp1 = 0; fp1 < first.points.Length; fp1++)
            {
                int sp1 = Array.IndexOf(second.points, first.points[fp1]);
                if (sp1 >= 0)
                {
                    int fp2 = fp1 + 1 < first.points.Length ? fp1 + 1 : 0;
                    int sp2 = sp1 - 1 >= 0 ? sp1 - 1 : second.points.Length - 1;

                    if (first.points[fp2] == second.points[sp2])
                    {
                        resultList.Add(new SharedEdge()
                        {
                            FirstPoint1 = fp1,
                            FirstPoint2 = fp2,
                            SecondPoint1 = sp1,
                            SecondPoint2 = sp2,
                        });

                        result = true;
                    }
                }
            }

            sharedEdges = resultList.ToArray();

            return result;
        }
        /// <summary>
        /// Gets whether the specified polygon contains the point projected into it
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="point">Point</param>
        /// <returns>Returns true if the polygon contains the point projected into it</returns>
        public static bool PointInPoly(Polygon poly, Vector3 point)
        {
            bool c = false;

            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                Vector3 vi = poly[i];
                Vector3 vj = poly[j];
                if (((vi.Z > point.Z) != (vj.Z > point.Z)) &&
                    (point.X < (vj.X - vi.X) * (point.Z - vi.Z) / (vj.Z - vi.Z) + vi.X))
                {
                    c = !c;
                }
            }

            return c;
        }
        /// <summary>
        /// Clips a polygon to a plane using the Sutherland-Hodgman algorithm.
        /// </summary>
        /// <param name="inVertices">The input array of vertices.</param>
        /// <param name="outVertices">The output array of vertices.</param>
        /// <param name="distances">A buffer that stores intermediate data</param>
        /// <param name="numVerts">The number of vertices to read from the arrays.</param>
        /// <param name="planeX">The clip plane's X component.</param>
        /// <param name="planeZ">The clip plane's Z component.</param>
        /// <param name="planeD">The clip plane's D component.</param>
        /// <returns>The number of vertices stored in outVertices.</returns>
        public static int ClipPolygonToPlane(Polygon poly, float planeX, float planeZ, float planeD, out Polygon outPoly)
        {
            Vector3[] inVertices = poly.points;
            Vector3[] outVertices = new Vector3[7];
            float[] distances = new float[poly.Count];

            for (int i = 0; i < poly.Count; i++)
            {
                distances[i] = planeX * inVertices[i].X + planeZ * inVertices[i].Z + planeD;
            }

            int m = 0;
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i, i++)
            {
                bool inj = distances[j] >= 0;
                bool ini = distances[i] >= 0;

                if (inj != ini)
                {
                    float s = distances[j] / (distances[j] - distances[i]);

                    Vector3 temp;
                    Vector3.Subtract(ref inVertices[i], ref inVertices[j], out temp);
                    Vector3.Multiply(ref temp, s, out temp);
                    Vector3.Add(ref inVertices[j], ref temp, out outVertices[m++]);
                }

                if (ini)
                {
                    outVertices[m++] = inVertices[i];
                }
            }

            outPoly = new Polygon(outVertices, 0, m);

            return outPoly.Count;
        }

        /// <summary>
        /// Polygon point list
        /// </summary>
        private Vector3[] points = null;
        /// <summary>
        /// Polygon orientation
        /// </summary>
        private GeometricOrientation orientation = GeometricOrientation.None;

        /// <summary>
        /// Point array
        /// </summary>
        public Vector3[] Points
        {
            get
            {
                return this.points;
            }
            protected set
            {
                this.points = value;

                this.Update();
            }
        }
        /// <summary>
        /// Number of points
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// Gets if the polygon is convex
        /// </summary>
        public bool Convex { get; private set; }
        /// <summary>
        /// Gets or sets the orientation of the polygon
        /// </summary>
        /// <returns>
        /// CounterClockWise : polygon vertices are in counter-clockwise order
        /// ClockWise : polygon vertices are in clockwise order
        /// None : the polygon has no (measurable) area
        /// </returns>
        public GeometricOrientation Orientation
        {
            get
            {
                return this.orientation;
            }
            set
            {
                if (this.orientation != GeometricOrientation.None && (this.orientation != value))
                {
                    Vector3[] invpoints = new Vector3[this.Count];

                    for (int i = 0; i < this.Count; i++)
                    {
                        invpoints[i] = this.points[this.Count - i - 1];
                    }

                    this.points = invpoints;
                    this.orientation = value;
                }
            }
        }
        /// <summary>
        /// Gets the specified point by index
        /// </summary>
        /// <param name="i">Point index</param>
        /// <returns>Returns the specified point by index</returns>
        public Vector3 this[int i]
        {
            get
            {
                return this.Points[i];
            }
            set
            {
                this.Points[i] = value;

                this.Update();
            }
        }
        /// <summary>
        /// Gets or sets whether the polygon has a hole or not
        /// </summary>
        public bool Hole { get; set; }
        /// <summary>
        /// Gets the polygon center
        /// </summary>
        public Vector3 Center { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Polygon()
        {
            this.Points = null;
            this.Hole = false;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="count">Number or vertices</param>
        public Polygon(int count)
        {
            this.Points = new Vector3[count];
            this.Hole = false;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="points">Points</param>
        public Polygon(params Vector3[] points)
        {
            this.Points = points;
            this.Hole = false;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="points">Points</param>
        /// <param name="index">Source index</param>
        /// <param name="length">Total length</param>
        public Polygon(Vector3[] points, int index, int length)
        {
            Vector3[] tmp = new Vector3[length];
            Array.Copy(points, index, tmp, 0, length); 
            this.Points = tmp;
            this.Hole = false;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Source poly</param>
        public Polygon(Polygon source)
        {
            this.Points = new Vector3[source.Points.Length];
            Array.Copy(source.points, this.points, source.Points.Length);

            this.Hole = source.Hole;
        }

        /// <summary>
        /// Updates polygon state
        /// </summary>
        private void Update()
        {
            this.Count = 0;
            this.Convex = false;
            this.orientation = GeometricOrientation.None;
            this.Center = Vector3.Zero;

            if (this.points != null && this.points.Length > 0)
            {
                this.Count = this.points.Length;
                this.Convex = IsConvex(this.points);
                this.orientation = GetOrientation(this.points);

                Vector3 sum = Vector3.Zero;
                Array.ForEach(this.points, p => sum += p);
                this.Center = sum / (float)this.points.Length;
            }
        }
        /// <summary>
        /// Adds a point to collection
        /// </summary>
        private bool Merge(Polygon other, SharedEdge[] sharedEdges, bool mergeConvex)
        {
            if (this.Count == 0)
            {
                this.points = other.Points;
                this.Hole = other.Hole;
                this.Update();

                return true;
            }
            else
            {
                List<Vector3> tmp1 = new List<Vector3>(this.points);
                List<Vector3> tmp2 = new List<Vector3>(other.points);

                //Remove middle shared points from this poly
                //Remove all shared points from other poly
                for (int i = 0; i < sharedEdges.Length; i++)
                {
                    if (i == 0)
                    {
                        tmp2.Remove(other.points[sharedEdges[i].SecondPoint1]);
                    }

                    if (i < sharedEdges.Length - 1)
                    {
                        tmp1.Remove(this.points[sharedEdges[i].FirstPoint2]);
                    }

                    tmp2.Remove(other.points[sharedEdges[i].SecondPoint2]);
                }

                //Adds other poly to this poly at first shared index
                tmp1.InsertRange(sharedEdges[0].FirstPoint1 + 1, tmp2);

                if (!mergeConvex || tmp1.Count < 3 || Polygon.IsConvex(tmp1.ToArray()))
                {
                    this.points = tmp1.ToArray();
                    this.Hole = false;
                    this.Update();

                    return true;
                }

                return false;
            }
        }
        /// <summary>
        /// Clears the polygon points
        /// </summary>
        public void Clear()
        {
            this.points = null;
            this.orientation = GeometricOrientation.None;
            this.Count = 0;
            this.Hole = false;
        }
        /// <summary>
        /// Get the array of edges
        /// </summary>
        /// <returns>Returns the array of edges</returns>
        public Line3[] GetEdges()
        {
            Line3[] edges = new Line3[this.points.Length];

            for (int i = 0; i < this.points.Length; i++)
            {
                if (i < this.points.Length - 1)
                {
                    edges[i] = new Line3(this.points[i], this.points[i + 1]);
                }
                else
                {
                    edges[i] = new Line3(this.points[i], this.points[0]);
                }
            }

            return edges;
        }
        /// <summary>
        /// Gets whether the prolygon contains the specified point into the point list
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Returns true if the polygon contains the specified point into the point list</returns>
        public bool Contains(Vector3 point)
        {
            return Array.Exists(this.points, p => p == point);
        }
        /// <summary>
        /// Remove the specified vertex list from the polygon point list
        /// </summary>
        /// <param name="list">Vertex list</param>
        public void Remove(Vector3[] list)
        {
            List<Vector3> tmp = new List<Vector3>(this.points);

            foreach (var item in list)
            {
                tmp.Remove(item);
            }

            this.points = tmp.ToArray();

            this.Update();
        }
        /// <summary>
        /// Remove unused vertices from the polygon point list
        /// </summary>
        public void RemoveUnused()
        {
            RemoveUnused(new Vector3[] { });
        }
        /// <summary>
        /// Remove unused vertices from the polygon point list
        /// </summary>
        /// <param name="exclusions">Operation excluded vertices</param>
        public void RemoveUnused(Vector3[] exclusions)
        {
            List<Vector3> toRemove = new List<Vector3>();

            Line3[] edges = this.GetEdges();

            for (int i = 1; i < edges.Length; i++)
            {
                Line3 edge1 = edges[i - 1];
                Line3 edge2 = edges[i];

                //Project
                Line2 pEdge1 = new Line2(new Vector2(edge1.Point1.X, edge1.Point1.Z), new Vector2(edge1.Point2.X, edge1.Point2.Z));
                Line2 pEdge2 = new Line2(new Vector2(edge2.Point1.X, edge2.Point1.Z), new Vector2(edge2.Point2.X, edge2.Point2.Z));

                if (pEdge1.Direction == pEdge2.Direction)
                {
                    if (!Array.Exists(exclusions, e => e == edges[i].Point1))
                    {
                        toRemove.Add(edges[i].Point1);
                    }
                }
            }

            if (toRemove.Count > 0) this.Remove(toRemove.ToArray());
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            string text = string.Format("Vertices: {0};", this.Count);

            if (this.Count > 0)
            {
                string tmp = "";
                Array.ForEach(this.points, p =>
                {
                    if (!string.IsNullOrEmpty(tmp)) tmp += " | ";

                    tmp += string.Format("{0}", p);
                });

                text += " " + tmp;
            }

            return text;
        }
    }
}
