using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    class DelaunayHull
    {
        public static void Build(IEnumerable<Vector3> pts, IEnumerable<int> hull, out IEnumerable<Int3> outTris, out IEnumerable<Int4> outEdges)
        {
            int npts = pts.Count();
            int nhull = hull.Count();
            int maxEdges = npts * 10;
            DelaunayHull dhull = new DelaunayHull(maxEdges);

            for (int i = 0, j = nhull - 1; i < nhull; j = i++)
            {
                dhull.AddEdge(hull.ElementAt(j), hull.ElementAt(i), (int)EdgeValues.EV_HULL, (int)EdgeValues.EV_UNDEF);
            }

            int currentEdge = 0;
            while (currentEdge < dhull.NEdges)
            {
                if (dhull.Edges[currentEdge][2] == (int)EdgeValues.EV_UNDEF)
                {
                    dhull.CompleteFacet(pts, npts, currentEdge);
                }
                if (dhull.Edges[currentEdge][3] == (int)EdgeValues.EV_UNDEF)
                {
                    dhull.CompleteFacet(pts, npts, currentEdge);
                }
                currentEdge++;
            }

            // Create tris
            Int3[] tris = Helper.CreateArray(dhull.NFaces, new Int3(-1, -1, -1));

            for (int i = 0; i < dhull.NEdges; ++i)
            {
                var e = dhull.Edges[i];
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

            for (int i = 0; i < tris.Length; ++i)
            {
                var t = tris[i];
                if (t.X == -1 || t.Y == -1 || t.Z == -1)
                {
                    Console.WriteLine($"delaunayHull: Removing dangling face {i} [{t.X},{t.Y},{t.Z}].");
                    tris[i] = tris[tris.Length - 1];
                    Array.Resize(ref tris, tris.Length - 1);
                    i--;
                }
            }

            outTris = tris.ToArray();
            outEdges = dhull.Edges.Take(dhull.NEdges).ToArray();
        }
        private static void UpdateLeftFace(ref Int4 e, int s, int t, int f)
        {
            if (e[0] == s && e[1] == t && e[2] == (int)EdgeValues.EV_UNDEF)
            {
                e[2] = f;
            }
            else if (e[1] == s && e[0] == t && e[3] == (int)EdgeValues.EV_UNDEF)
            {
                e[3] = f;
            }
        }
        private static void CircumCircle(Vector3 p1, Vector3 p2, Vector3 p3, out Vector3 center, out float radius)
        {
            float EPS = 1e-6f;

            // Calculate the circle relative to p1, to avoid some precision issues.
            Vector3 v1 = new Vector3();
            Vector3 v2 = Vector3.Subtract(p2, p1);
            Vector3 v3 = Vector3.Subtract(p3, p1);

            float cp = RecastUtils.VCross2(v1, v2, v3);
            if (Math.Abs(cp) > EPS)
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

        public int NFaces { get; set; }
        public int NEdges { get; set; }
        public int MaxEdges { get; set; }
        public Int4[] Edges { get; set; }

        public DelaunayHull(int maxEdges)
        {
            MaxEdges = maxEdges;
            Edges = new Int4[maxEdges];
        }

        public void AddEdge(int s, int t, int l, int r)
        {
            if (NEdges >= MaxEdges)
            {
                Console.WriteLine($"addEdge: Too many edges ({NEdges}/{MaxEdges}).");
                return;
            }

            // Add edge if not already in the triangulation.
            int e = FindEdge(s, t);
            if (e == (int)EdgeValues.EV_UNDEF)
            {
                Edges[NEdges++] = new Int4(s, t, l, r);
            }
        }
        public void CompleteFacet(IEnumerable<Vector3> pts, int npts, int e)
        {
            float EPS = float.Epsilon;

            var edge = Edges[e];

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
            int pt = npts;
            Vector3 c = new Vector3();
            float r = -1;
            for (int u = 0; u < npts; ++u)
            {
                if (u == s || u == t) continue;
                if (RecastUtils.VCross2(pts.ElementAt(s), pts.ElementAt(t), pts.ElementAt(u)) > EPS)
                {
                    if (r < 0)
                    {
                        // The circle is not updated yet, do it now.
                        pt = u;
                        CircumCircle(pts.ElementAt(s), pts.ElementAt(t), pts.ElementAt(u), out c, out r);
                        continue;
                    }
                    float d = Vector2.Distance(c.XZ(), pts.ElementAt(u).XZ());
                    float tol = 0.001f;
                    if (d > r * (1 + tol))
                    {
                        // Outside current circumcircle, skip.
                    }
                    else if (d < r * (1 - tol))
                    {
                        // Inside safe circumcircle, update circle.
                        pt = u;
                        CircumCircle(pts.ElementAt(s), pts.ElementAt(t), pts.ElementAt(u), out c, out r);
                    }
                    else
                    {
                        // Inside epsilon circum circle, do extra tests to make sure the edge is valid.
                        // s-u and t-u cannot overlap with s-pt nor t-pt if they exists.
                        if (OverlapEdges(pts, s, u))
                        {
                            continue;
                        }
                        if (OverlapEdges(pts, t, u))
                        {
                            continue;
                        }
                        // Edge is valid.
                        pt = u;
                        CircumCircle(pts.ElementAt(s), pts.ElementAt(t), pts.ElementAt(u), out c, out r);
                    }
                }
            }

            // Add new triangle or update edge info if s-t is on hull.
            if (pt < npts)
            {
                // Update face information of edge being completed.
                UpdateLeftFace(ref Edges[e], s, t, NFaces);

                // Add new edge or update face info of old edge.
                e = FindEdge(pt, s);
                if (e == (int)EdgeValues.EV_UNDEF)
                {
                    AddEdge(pt, s, NFaces, (int)EdgeValues.EV_UNDEF);
                }
                else
                {
                    UpdateLeftFace(ref Edges[e], pt, s, NFaces);
                }

                // Add new edge or update face info of old edge.
                e = FindEdge(t, pt);
                if (e == (int)EdgeValues.EV_UNDEF)
                {
                    AddEdge(t, pt, NFaces, (int)EdgeValues.EV_UNDEF);
                }
                else
                {
                    UpdateLeftFace(ref Edges[e], t, pt, NFaces);
                }

                NFaces++;
            }
            else
            {
                UpdateLeftFace(ref Edges[e], s, t, (int)EdgeValues.EV_HULL);
            }
        }
        private bool OverlapEdges(IEnumerable<Vector3> pts, int s1, int t1)
        {
            for (int i = 0; i < NEdges; ++i)
            {
                int s0 = Edges.ElementAt(i).X;
                int t0 = Edges.ElementAt(i).Y;
                // Same or connected edges do not overlap.
                if (s0 == s1 || s0 == t1 || t0 == s1 || t0 == t1)
                {
                    continue;
                }
                if (RecastUtils.OverlapSegSeg2d(pts.ElementAt(s0), pts.ElementAt(t0), pts.ElementAt(s1), pts.ElementAt(t1)) != 0)
                {
                    return true;
                }
            }
            return false;
        }
        private int FindEdge(int s, int t)
        {
            for (int i = 0; i < NEdges; i++)
            {
                var e = Edges.ElementAt(i);

                if ((e.X == s && e.Y == t) || (e.X == t && e.Y == s))
                {
                    return i;
                }
            }

            return (int)EdgeValues.EV_UNDEF;
        }
    }
}
