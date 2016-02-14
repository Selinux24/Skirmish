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
            public int FirstPoint1;
            public int FirstPoint2;
            public int SecondPoint1;
            public int SecondPoint2;
        }

        public static Polygon FromTriangle(Triangle tri, GeometricOrientation orientation = GeometricOrientation.CounterClockwise)
        {
            Polygon poly = new Polygon(tri.Point1, tri.Point2, tri.Point3);

            poly.Orientation = orientation;

            return poly;
        }

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
                        sharedEdge.FirstPoint1 = fp1;
                        sharedEdge.FirstPoint2 = fp2;
                        sharedEdge.SecondPoint1 = sp1;
                        sharedEdge.SecondPoint2 = sp2;

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

        private Vector3[] points = null;
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
        /// <param name="p1">First point</param>
        /// <param name="p2">Second point</param>
        /// <param name="p3">Third point</param>
        public Polygon(Vector3 p1, Vector3 p2, Vector3 p3)
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
                List<Vector3> v = new List<Vector3>(this.points);
                List<Vector3> toMerge = new List<Vector3>(poly.Count - 2);

                //Find shared points in new poly
                for (int i = 0; i < poly.Count; i++)
                {
                    if (i != mergeInfo.SecondPoint1 && i != mergeInfo.SecondPoint2)
                    {
                        toMerge.Add(poly[i]);
                    }
                }

                v.InsertRange(mergeInfo.FirstPoint1 + 1, toMerge);

                Vector3[] copy = v.ToArray();

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

        public bool Contains(Vector3 point)
        {
            return Array.Exists(this.points, p => p == point);
        }

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

        public void RemoveUnused()
        {
            RemoveUnused(new Vector3[] { });
        }

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
    }
}
