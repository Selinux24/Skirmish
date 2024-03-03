using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Delaunay hull
    /// </summary>
    class DelaunayHull
    {
        /// <summary>
        /// Unidefined
        /// </summary>
        const int EV_UNDEF = -1;
        /// <summary>
        /// Hull
        /// </summary>
        const int EV_HULL = -2;

        /// <summary>
        /// Delaunay edge
        /// </summary>
        class DelaunayEdge
        {
            /// <summary>
            /// First point index
            /// </summary>
            public int Point0 { get; set; }
            /// <summary>
            /// Second point index
            /// </summary>
            public int Point1 { get; set; }
            /// <summary>
            /// First face index (triangle)
            /// </summary>
            public int Face0 { get; set; }
            /// <summary>
            /// Second face index (triangle)
            /// </summary>
            public int Face1 { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="point0">First point index</param>
            /// <param name="point1">Second point index</param>
            /// <param name="face0">First face index</param>
            /// <param name="face1">Second face index</param>
            public DelaunayEdge(int point0, int point1, int face0, int face1)
            {
                Point0 = point0;
                Point1 = point1;
                Face0 = face0;
                Face1 = face1;
            }

            /// <summary>
            /// Updates the left face
            /// </summary>
            /// <param name="s">S point index</param>
            /// <param name="t">T point index</param>
            /// <param name="f">New face index</param>
            public void UpdateLeftFace(int s, int t, int f)
            {
                if (Point0 == s && Point1 == t && Face0 == EV_UNDEF)
                {
                    Face0 = f;
                }
                else if (Point1 == s && Point0 == t && Face1 == EV_UNDEF)
                {
                    Face1 = f;
                }
            }
            /// <summary>
            /// Updates the specified triangle collection
            /// </summary>
            /// <param name="tris">Triangle collection</param>
            public void UpdateTris(Int3[] tris)
            {
                if (Face1 >= 0)
                {
                    // Left face
                    var t = tris[Face1];
                    if (t.X == -1)
                    {
                        t.X = Point0;
                        t.Y = Point1;
                    }
                    else if (t.X == Point1)
                    {
                        t.Z = Point0;
                    }
                    else if (t.Y == Point0)
                    {
                        t.Z = Point1;
                    }
                    tris[Face1] = t;
                }

                if (Face0 >= 0)
                {
                    // Right
                    var t = tris[Face0];
                    if (t.X == -1)
                    {
                        t.X = Point1;
                        t.Y = Point0;
                    }
                    else if (t.X == Point0)
                    {
                        t.Z = Point1;
                    }
                    else if (t.Y == Point1)
                    {
                        t.Z = Point0;
                    }
                    tris[Face0] = t;
                }
            }
            /// <summary>
            /// Gets whether the specified point is the same than the current instance
            /// </summary>
            /// <param name="s">S point index</param>
            /// <param name="t">T point index</param>
            public bool IsSameEdge(int s, int t)
            {
                if ((Point0 == s && Point1 == t) || (Point0 == t && Point1 == s))
                {
                    return true;
                }

                return false;
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                string face0 = Face0 switch
                {
                    EV_HULL => "hull",
                    EV_UNDEF => "undefined",
                    _ => $"{Face0}",
                };

                string face1 = Face1 switch
                {
                    EV_UNDEF => "undefined",
                    _ => $"{Face1}",
                };

                return $"Point0={Point0}; Point1={Point1}; Face0={face0}; Face1={face1}";
            }
        }

        /// <summary>
        /// Maximum edges
        /// </summary>
        private readonly int maxEdges;
        /// <summary>
        /// Edge list
        /// </summary>
        private readonly List<DelaunayEdge> edges = new();
        /// <summary>
        /// Number of faces
        /// </summary>
        private int faces;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxEdges">Max edges</param>
        public DelaunayHull(int maxEdges)
        {
            this.maxEdges = maxEdges;
        }

        /// <summary>
        /// Builds a hull
        /// </summary>
        /// <param name="pts">Point list</param>
        /// <param name="hull">Hull indices</param>
        public static DelaunayHull Build(Vector3[] pts, int[] hull)
        {
            int max = pts.Length * 10;
            var dhull = new DelaunayHull(max);

            int nhull = hull.Length;
            for (int i = 0, j = nhull - 1; i < nhull; j = i++)
            {
                dhull.AddEdge(hull[j], hull[i], EV_HULL, EV_UNDEF);
            }

            int currentEdge = 0;
            while (currentEdge < dhull.edges.Count)
            {
                if (dhull.edges[currentEdge].Face0 == EV_UNDEF)
                {
                    dhull.CompleteFacet(pts, currentEdge);
                }
                if (dhull.edges[currentEdge].Face1 == EV_UNDEF)
                {
                    dhull.CompleteFacet(pts, currentEdge);
                }
                currentEdge++;
            }

            return dhull;
        }
        /// <summary>
        /// Filter triangles
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        /// <returns>Returns the filtered list</returns>
        private static Int3[] FilterTris(Int3[] triangles)
        {
            // Copy array
            var tris = triangles.ToArray();

            for (int i = 0; i < tris.Length; ++i)
            {
                var t = tris[i];
                if (t.X == -1 || t.Y == -1 || t.Z == -1)
                {
                    Logger.WriteWarning(nameof(DelaunayHull), $"delaunayHull: Removing dangling face {i} [{t.X},{t.Y},{t.Z}].");
                    tris[i] = tris[^1];
                    Array.Resize(ref tris, tris.Length - 1);
                    i--;
                }
            }

            return tris;
        }
        /// <summary>
        /// Circum circle
        /// </summary>
        private static (Vector3 Center, float Radius) CircumCircle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            // Calculate the circle relative to p1, to avoid some precision issues.
            var v1 = Vector3.Zero;
            var v2 = Vector3.Subtract(p2, p1);
            var v3 = Vector3.Subtract(p3, p1);

            float cp = Utils.VCross2D(v1, v2, v3);
            if (Math.Abs(cp) <= Utils.ZeroTolerance)
            {
                return (p1, 0);
            }

            float v1Sq = Vector2.Dot(v1.XZ(), v1.XZ());
            float v2Sq = Vector2.Dot(v2.XZ(), v2.XZ());
            float v3Sq = Vector2.Dot(v3.XZ(), v3.XZ());

            float cX = (v1Sq * (v2.Z - v3.Z) + v2Sq * (v3.Z - v1.Z) + v3Sq * (v1.Z - v2.Z)) / (2 * cp);
            float cZ = (v1Sq * (v3.X - v2.X) + v2Sq * (v1.X - v3.X) + v3Sq * (v2.X - v1.X)) / (2 * cp);
            var center = new Vector3(cX, 0, cZ);

            float radius = Utils.Distance2D(center, v1);
            center = Vector3.Add(center, p1);

            return (center, radius);
        }

        /// <summary>
        /// Gets the triangle list
        /// </summary>
        public Int3[] GetTris()
        {
            Int3[] tris = Helper.CreateArray(faces, new Int3(-1, -1, -1));

            for (int i = 0; i < edges.Count; ++i)
            {
                var e = edges[i];

                e.UpdateTris(tris);
            }

            return FilterTris(tris);
        }

        /// <summary>
        /// Adds an edge
        /// </summary>
        private void AddEdge(int s, int t, int l, int r)
        {
            if (edges.Count >= maxEdges)
            {
                Logger.WriteWarning(this, $"addEdge: Too many edges ({edges.Count}/{maxEdges}).");
                return;
            }

            // Add edge if not already in the triangulation.
            int e = FindEdge(s, t);
            if (e == EV_UNDEF)
            {
                edges.Add(new(s, t, l, r));
            }
        }
        /// <summary>
        /// Completes a facet
        /// </summary>
        /// <param name="pts">Point list</param>
        /// <param name="e">Edge index</param>
        private void CompleteFacet(Vector3[] pts, int e)
        {
            // Cache s and t.
            int s, t;
            if (edges[e].Face0 == EV_UNDEF)
            {
                s = edges[e].Point0;
                t = edges[e].Point1;
            }
            else if (edges[e].Face1 == EV_UNDEF)
            {
                s = edges[e].Point1;
                t = edges[e].Point0;
            }
            else
            {
                // Edge already completed.
                return;
            }

            // Find best point on left of edge.
            int pt = FindBestPointOnCircleFromPoint(s, t, pts);

            // Add new triangle or update edge info if s-t is on hull.
            if (pt >= pts.Length)
            {
                edges[e].UpdateLeftFace(s, t, EV_HULL);

                return;
            }

            // Update face information of edge being completed.
            edges[e].UpdateLeftFace(s, t, faces);

            // Add new edge or update face info of old edge.
            e = FindEdge(pt, s);
            if (e == EV_UNDEF)
            {
                AddEdge(pt, s, faces, EV_UNDEF);
            }
            else
            {
                edges[e].UpdateLeftFace(pt, s, faces);
            }

            // Add new edge or update face info of old edge.
            e = FindEdge(t, pt);
            if (e == EV_UNDEF)
            {
                AddEdge(t, pt, faces, EV_UNDEF);
            }
            else
            {
                edges[e].UpdateLeftFace(t, pt, faces);
            }

            faces++;
        }
        /// <summary>
        /// Finds best point on cicle from point list
        /// </summary>
        private int FindBestPointOnCircleFromPoint(int s, int t, Vector3[] pts)
        {
            const float EPS = 1e-5f;

            int pt = pts.Length;
            Vector3 c = Vector3.Zero;
            float r = -1;
            for (int u = 0; u < pts.Length; ++u)
            {
                if (u == s || u == t)
                {
                    continue;
                }

                if (Utils.VCross2D(pts[s], pts[t], pts[u]) <= EPS)
                {
                    continue;
                }

                if (PointOnCircleFromPoint(s, t, u, pts, c, r))
                {
                    //Circle valid. Update
                    pt = u;
                    var (center, radius) = CircumCircle(pts[s], pts[t], pts[u]);
                    c = center;
                    r = radius;
                }
            }

            return pt;
        }
        /// <summary>
        /// Point on circle from point
        /// </summary>
        private bool PointOnCircleFromPoint(int s, int t, int u, Vector3[] pts, Vector3 c, float r)
        {
            const float Tolerance = 0.001f;

            if (r < 0)
            {
                // The circle is not updated yet, do it now.
                return true;
            }

            float d = Utils.Distance2D(c, pts[u]);
            if (d > r * (1 + Tolerance))
            {
                // Outside current circumcircle, skip.
                return false;
            }

            if (d < r * (1 - Tolerance))
            {
                // Inside safe circumcircle, update circle.
                return true;
            }

            // Inside epsilon circum circle, do extra tests to make sure the edge is valid.
            // s-u and t-u cannot overlap with s-pt nor t-pt if they exists.
            if (OverlapEdges(s, u, pts))
            {
                return false;
            }
            if (OverlapEdges(t, u, pts))
            {
                return false;
            }

            // Edge is valid.
            return true;
        }
        /// <summary>
        /// Overlap edges
        /// </summary>
        private bool OverlapEdges(int s, int t, Vector3[] pts)
        {
            for (int i = 0; i < edges.Count; ++i)
            {
                var e = edges[i];
                int s0 = e.Point0;
                int t0 = e.Point1;

                // Same or connected edges do not overlap.
                if (s0 == s || s0 == t || t0 == s || t0 == t)
                {
                    continue;
                }

                if (Utils.OverlapSegSeg2D(pts[s0], pts[t0], pts[s], pts[t]))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Finds edge
        /// </summary>
        private int FindEdge(int s, int t)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i].IsSameEdge(s, t))
                {
                    return i;
                }
            }

            return EV_UNDEF;
        }
    }
}
