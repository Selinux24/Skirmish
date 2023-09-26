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
        const float Tolerance = 0.001f;

        /// <summary>
        /// Maximum edges
        /// </summary>
        private readonly int maxEdges;
        /// <summary>
        /// Edge list
        /// </summary>
        private readonly List<Int4> edges = new();
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
                dhull.AddEdge(hull[j], hull[i], (int)EdgeValues.EV_HULL, (int)EdgeValues.EV_UNDEF);
            }

            int currentEdge = 0;
            while (currentEdge < dhull.edges.Count)
            {
                if (dhull.edges[currentEdge][2] == (int)EdgeValues.EV_UNDEF)
                {
                    dhull.CompleteFacet(pts, currentEdge);
                }
                if (dhull.edges[currentEdge][3] == (int)EdgeValues.EV_UNDEF)
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
        /// Update left face
        /// </summary>
        /// <param name="edge">Edge</param>
        private static Int4 UpdateLeftFace(Int4 edge, int s, int t, int f)
        {
            // Copy edge
            var e = edge;

            if (e.X == s && e.Y == t && e.Z == (int)EdgeValues.EV_UNDEF)
            {
                e.Z = f;
            }
            else if (e.Y == s && e.X == t && e.W == (int)EdgeValues.EV_UNDEF)
            {
                e.W = f;
            }

            return e;
        }
        /// <summary>
        /// Circum circle
        /// </summary>
        private static void CircumCircle(Vector3 p1, Vector3 p2, Vector3 p3, out Vector3 center, out float radius)
        {
            const float eps = 1e-6f;

            // Calculate the circle relative to p1, to avoid some precision issues.
            var v1 = new Vector3();
            var v2 = Vector3.Subtract(p2, p1);
            var v3 = Vector3.Subtract(p3, p1);

            float cp = Utils.VCross2D(v1, v2, v3);
            if (Math.Abs(cp) > eps)
            {
                float v1Sq = Vector2.Dot(v1.XZ(), v1.XZ());
                float v2Sq = Vector2.Dot(v2.XZ(), v2.XZ());
                float v3Sq = Vector2.Dot(v3.XZ(), v3.XZ());

                center = Vector3.Zero;
                center.X = (v1Sq * (v2.Z - v3.Z) + v2Sq * (v3.Z - v1.Z) + v3Sq * (v1.Z - v2.Z)) / (2 * cp);
                center.Y = 0;
                center.Z = (v1Sq * (v3.X - v2.X) + v2Sq * (v1.X - v3.X) + v3Sq * (v2.X - v1.X)) / (2 * cp);

                radius = Vector2.Distance(center.XZ(), v1.XZ());
                center = Vector3.Add(center, p1);
            }
            else
            {
                center = p1;
                radius = 0;
            }
        }

        /// <summary>
        /// Gets the edge list
        /// </summary>
        public Int4[] GetEdges()
        {
            return edges.ToArray();
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
                if (e.W >= 0)
                {
                    // Left face
                    var t = tris[e[3]];
                    if (t.X == -1)
                    {
                        t.X = e[0];
                        t.Y = e[1];
                    }
                    else if (t.X == e[1])
                    {
                        t.Z = e[0];
                    }
                    else if (t.Y == e[0])
                    {
                        t.Z = e[1];
                    }
                    tris[e[3]] = t;
                }
                if (e[2] >= 0)
                {
                    // Right
                    var t = tris[e[2]];
                    if (t.X == -1)
                    {
                        t.X = e[1];
                        t.Y = e[0];
                    }
                    else if (t.X == e[0])
                    {
                        t.Z = e[1];
                    }
                    else if (t.Y == e[1])
                    {
                        t.Z = e[0];
                    }
                    tris[e[2]] = t;
                }
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
            if (e == (int)EdgeValues.EV_UNDEF)
            {
                edges.Add(new Int4(s, t, l, r));
            }
        }
        /// <summary>
        /// Completes a facet
        /// </summary>
        /// <param name="pts">Point list</param>
        /// <param name="e">Edge index</param>
        private void CompleteFacet(Vector3[] pts, int e)
        {
            var edge = edges[e];

            // Cache s and t.
            int s, t;
            if (edge[2] == (int)EdgeValues.EV_UNDEF)
            {
                s = edge[0];
                t = edge[1];
            }
            else if (edge[3] == (int)EdgeValues.EV_UNDEF)
            {
                s = edge[1];
                t = edge[0];
            }
            else
            {
                // Edge already completed.
                return;
            }

            // Find best point on left of edge.
            int pt = FindBestPointOnLeft(s, t, pts);

            pt = FindBestPointOnCircleFromPoint(s, t, pt, pts);

            // Add new triangle or update edge info if s-t is on hull.
            if (pt >= pts.Length)
            {
                edges[e] = UpdateLeftFace(edges[e], s, t, (int)EdgeValues.EV_HULL);

                return;
            }

            // Update face information of edge being completed.
            edges[e] = UpdateLeftFace(edges[e], s, t, faces);

            // Add new edge or update face info of old edge.
            e = FindEdge(pt, s);
            if (e == (int)EdgeValues.EV_UNDEF)
            {
                AddEdge(pt, s, faces, (int)EdgeValues.EV_UNDEF);
            }
            else
            {
                edges[e] = UpdateLeftFace(edges[e], pt, s, faces);
            }

            // Add new edge or update face info of old edge.
            e = FindEdge(t, pt);
            if (e == (int)EdgeValues.EV_UNDEF)
            {
                AddEdge(t, pt, faces, (int)EdgeValues.EV_UNDEF);
            }
            else
            {
                edges[e] = UpdateLeftFace(edges[e], t, pt, faces);
            }

            faces++;
        }
        /// <summary>
        /// Finds best point on left
        /// </summary>
        private int FindBestPointOnLeft(int s, int t, Vector3[] pts)
        {
            return FindBestPointOnCircleFromPoint(s, t, pts.Length, pts);
        }
        /// <summary>
        /// Finds best point on cicle from point
        /// </summary>
        private int FindBestPointOnCircleFromPoint(int s, int t, int point, Vector3[] pts)
        {
            int pt = point;
            Vector3 c = Vector3.Zero;
            float r = -1;
            for (int u = 0; u < pts.Length; ++u)
            {
                if (u == s || u == t)
                {
                    continue;
                }

                if (Utils.VCross2D(pts[s], pts[t], pts[u]) <= float.Epsilon)
                {
                    continue;
                }

                if (PointOnCircleFromPoint(s, t, u, pts, c, r))
                {
                    //Circle valid. Update
                    pt = u;
                    CircumCircle(pts[s], pts[t], pts[u], out c, out r);
                }
            }

            return pt;
        }
        /// <summary>
        /// Point on circle from point
        /// </summary>
        private bool PointOnCircleFromPoint(int s, int t, int u, Vector3[] pts, Vector3 c, float r)
        {
            if (r < 0)
            {
                // The circle is not updated yet, do it now.
                return true;
            }

            float d = Vector2.Distance(c.XZ(), pts[u].XZ());
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
                int s0 = e.X;
                int t0 = e.Y;
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
                var e = edges[i];

                if ((e.X == s && e.Y == t) || (e.X == t && e.Y == s))
                {
                    return i;
                }
            }

            return (int)EdgeValues.EV_UNDEF;
        }
    }
}
