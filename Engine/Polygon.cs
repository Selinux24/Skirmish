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
        public struct SharedEdge
        {
            public int SharedFirstPoint1;
            public int SharedFirstPoint2;
            public int SharedSecondPoint1;
            public int SharedSecondPoint2;
        }

        public static Polygon FromTriangle(Triangle tri, GeometricOrientation orientation = GeometricOrientation.CounterClockwise)
        {
            Polygon poly = new Polygon(
                new Vector2(tri.Point1.X, tri.Point1.Z),
                new Vector2(tri.Point2.X, tri.Point2.Z),
                new Vector2(tri.Point3.X, tri.Point3.Z));

            poly.Orientation = orientation;

            return poly;
        }

        public static GeometricOrientation GetOrientation(Vector2[] points)
        {
            float area = 0;
            for (int i1 = 0; i1 < points.Length; i1++)
            {
                int i2 = i1 + 1;
                if (i2 == points.Length) i2 = 0;

                area += points[i1].X * points[i2].Y - points[i1].Y * points[i2].X;
            }
            if (area > 0) return GeometricOrientation.CounterClockwise;
            if (area < 0) return GeometricOrientation.Clockwise;

            return GeometricOrientation.None;
        }
        public static bool IsConvex(Vector2[] points)
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
        public static bool Merge(Polygon first, Polygon second, bool mergeConvex, out Polygon mergedPolygon)
        {
            mergedPolygon = null;

            SharedEdge sharedEdge;
            if (ShareAnEdgeWith(first, second, out sharedEdge))
            {
                Polygon newpoly = new Polygon(first);
                if (newpoly.Merge(second, sharedEdge, mergeConvex))
                {
                    mergedPolygon = newpoly;
                    return true;
                }
            }

            return false;
        }
        private static bool ShareAnEdgeWith(Polygon first, Polygon second, out SharedEdge sharedEdge)
        {
            bool result = false;
            sharedEdge = new SharedEdge();

            for (int fp1 = 0; fp1 < first.points.Length; fp1++)
            {
                int sp1 = Array.IndexOf(second.points, first.points[fp1]);
                if (sp1 >= 0)
                {
                    int fp2 = fp1 + 1 < first.points.Length ? fp1 + 1 : 0;
                    int sp2 = sp1 - 1 >= 0 ? sp1 - 1 : second.points.Length - 1;

                    if (first.points[fp2] == second.points[sp2])
                    {
                        sharedEdge.SharedFirstPoint1 = fp1;
                        sharedEdge.SharedFirstPoint2 = fp2;
                        sharedEdge.SharedSecondPoint1 = sp1;
                        sharedEdge.SharedSecondPoint2 = sp2;

                        if (result == false)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }

            return result;
        }
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
                            SharedFirstPoint1 = fp1,
                            SharedFirstPoint2 = fp2,
                            SharedSecondPoint1 = sp1,
                            SharedSecondPoint2 = sp2,
                        });

                        result = true;
                    }
                }
            }

            sharedEdges = resultList.ToArray();

            return result;
        }

        private Vector2[] points = null;
        private GeometricOrientation orientation = GeometricOrientation.None;

        /// <summary>
        /// Point array
        /// </summary>
        public Vector2[] Points
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
                    Vector2[] invpoints = new Vector2[this.Count];

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
        public Vector2 this[int i]
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
            this.Points = new Vector2[count];
            this.Hole = false;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p1">First point</param>
        /// <param name="p2">Second point</param>
        /// <param name="p3">Third point</param>
        public Polygon(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            this.Points = new[] { p1, p2, p3 };
            this.Hole = false;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Source poly</param>
        public Polygon(Polygon source)
        {
            this.Points = source.Points;
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

            if (this.points != null && this.points.Length > 0)
            {
                this.Count = this.points.Length;
                this.Convex = IsConvex(this.points);
                this.orientation = GetOrientation(this.points);
            }
        }
        /// <summary>
        /// Adds a point to collection
        /// </summary>
        private bool Merge(Polygon poly, SharedEdge mergeInfo, bool mergeConvex)
        {
            if (this.Count == 0)
            {
                this.Points = poly.Points;
                this.Hole = poly.Hole;
                return true;
            }
            else
            {
                List<Vector2> v = new List<Vector2>(this.points);
                List<Vector2> toMerge = new List<Vector2>(poly.Count - 2);

                //Find shared points in new poly
                for (int i = 0; i < poly.Count; i++)
                {
                    if (i != mergeInfo.SharedSecondPoint1 && i != mergeInfo.SharedSecondPoint2)
                    {
                        toMerge.Add(poly[i]);
                    }
                }

                v.InsertRange(mergeInfo.SharedFirstPoint1 + 1, toMerge);

                Vector2[] copy = v.ToArray();

                if (!mergeConvex || copy.Length < 3 || Polygon.IsConvex(copy))
                {
                    this.Points = copy;
                    this.Hole = false;
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

        public Line2[] GetEdges()
        {
            Line2[] edges = new Line2[this.points.Length];

            for (int i = 0; i < this.points.Length; i++)
            {
                if (i < this.points.Length - 1)
                {
                    edges[i] = new Line2(this.points[i], this.points[i + 1]);
                }
                else
                {
                    edges[i] = new Line2(this.points[i], this.points[0]);
                }
            }

            return edges;
        }

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

        public bool Contains(Vector2 point)
        {
            return Array.Exists(this.points, p => p == point);
        }

        public void Remove(Vector2[] list)
        {
            List<Vector2> tmp = new List<Vector2>(this.points);

            foreach (var item in list)
            {
                tmp.Remove(item);
            }

            this.points = tmp.ToArray();

            this.Update();
        }

        public void Remove(int[] list)
        {
            List<Vector2> tmp = new List<Vector2>(this.points);

            foreach (var item in list)
            {
                tmp.Remove(this.points[item]);
            }

            this.points = tmp.ToArray();

            this.Update();
        }

        internal static SharedEdge Simplify(SharedEdge[] sharedEdges, ref Polygon poly1, ref Polygon poly2)
        {
            if (sharedEdges == null || sharedEdges.Length == 0) return new SharedEdge();

            if (sharedEdges.Length == 1)
            {
                return sharedEdges[0];
            }
            else
            {
                int[] points1 = new int[sharedEdges.Length - 1];
                int[] points2 = new int[sharedEdges.Length - 1];

                for (int i = 0; i < sharedEdges.Length - 1; i++)
                {
                    points1[i] = sharedEdges[i].SharedFirstPoint2;
                    points2[i] = sharedEdges[i].SharedSecondPoint2;
                }

                poly1.Remove(points1);
                poly2.Remove(points2);

                SharedEdge edge;
                if (Polygon.ShareAnEdgeWith(poly1, poly2, out edge))
                {
                    return edge;
                }
                else
                {
                    throw new Exception("Bad shared edge collection");
                }
            }
        }
    }
}
