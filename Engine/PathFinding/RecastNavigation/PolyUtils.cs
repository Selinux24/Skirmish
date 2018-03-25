using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.PathFinding.RecastNavigation
{
    static class PolyUtils
    {
        public static int ComputeVertexHash2(int x, int y, int z)
        {
            uint h1 = 0x8da6b343; // Large multiplicative constants;
            uint h2 = 0xd8163841; // here arbitrarily chosen primes
            uint h3 = 0xcb1ab31f;
            uint n = (uint)(h1 * x + h2 * y + h3 * z);
            return (int)(n & (Constants.VertexBucketCount2 - 1));
        }
        public static int GetDirForOffset(int x, int y)
        {
            int[] dirs = { 3, 0, -1, 2, 1 };
            return dirs[((y + 1) << 1) + x];
        }
        public static int GetDirOffsetX(int dir)
        {
            int[] offset = new[] { -1, 0, 1, 0, };
            return offset[dir & 0x03];
        }
        public static int GetDirOffsetY(int dir)
        {
            int[] offset = new[] { 0, 1, 0, -1 };
            return offset[dir & 0x03];
        }
        public static int OppositeTile(int side)
        {
            return (side + 4) & 0x7;
        }
        public static int Triangulate(int n, Int4[] verts, ref int[] indices, out Int3[] tris)
        {
            int ntris = 0;

            // The last bit of the index is used to indicate if the vertex can be removed.
            for (int i = 0; i < n; i++)
            {
                int i1 = Helper.Next(i, n);
                int i2 = Helper.Next(i1, n);
                if (Diagonal(i, i2, n, verts, indices))
                {
                    indices[i1] |= 0x8000;
                }
            }

            List<Int3> dst = new List<Int3>();

            while (n > 3)
            {
                int minLen = -1;
                int mini = -1;
                for (int ix = 0; ix < n; ix++)
                {
                    int i1x = Helper.Next(ix, n);
                    if ((indices[i1x] & 0x8000) != 0)
                    {
                        var p0 = verts[(indices[ix] & 0x7fff)];
                        var p2 = verts[(indices[Helper.Next(i1x, n)] & 0x7fff)];

                        int dx = p2.X - p0.X;
                        int dz = p2.Z - p0.Z;
                        int len = dx * dx + dz * dz;
                        if (minLen < 0 || len < minLen)
                        {
                            minLen = len;
                            mini = ix;
                        }
                    }
                }

                if (mini == -1)
                {
                    // Should not happen.
                    tris = null;
                    return -ntris;
                }

                int i = mini;
                int i1 = Helper.Next(i, n);
                int i2 = Helper.Next(i1, n);

                dst.Add(new Int3()
                {
                    X = indices[i] & 0x7fff,
                    Y = indices[i1] & 0x7fff,
                    Z = indices[i2] & 0x7fff
                });
                ntris++;

                // Removes P[i1] by copying P[i+1]...P[n-1] left one index.
                n--;
                for (int k = i1; k < n; k++)
                {
                    indices[k] = indices[k + 1];
                }

                if (i1 >= n) i1 = 0;
                i = Helper.Prev(i1, n);
                // Update diagonal flags.
                if (Diagonal(Helper.Prev(i, n), i1, n, verts, indices))
                {
                    indices[i] |= 0x8000;
                }
                else
                {
                    indices[i] &= 0x7fff;
                }

                if (Diagonal(i, Helper.Next(i1, n), n, verts, indices))
                {
                    indices[i1] |= 0x8000;
                }
                else
                {
                    indices[i1] &= 0x7fff;
                }
            }

            // Append the remaining triangle.
            dst.Add(new Int3
            {
                X = indices[0] & 0x7fff,
                Y = indices[1] & 0x7fff,
                Z = indices[2] & 0x7fff,
            });
            ntris++;

            tris = dst.ToArray();

            return ntris;
        }
        public static bool Diagonal(int i, int j, int n, Int4[] verts, int[] indices)
        {
            return InCone(i, j, n, verts, indices) && Diagonalie(i, j, n, verts, indices);
        }
        public static bool InCone(int i, int n, Int4[] verts, Int4 pj)
        {
            var pi = verts[i];
            var pi1 = verts[Helper.Next(i, n)];
            var pin1 = verts[Helper.Prev(i, n)];

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn(pin1, pi, pi1))
            {
                return Left(pi, pj, pin1) && Left(pj, pi, pi1);
            }
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn(pi, pj, pi1) && LeftOn(pj, pi, pin1));
        }
        public static bool InCone(int i, int j, int n, Int4[] verts, int[] indices)
        {
            var pi = verts[(indices[i] & 0x7fff)];
            var pj = verts[(indices[j] & 0x7fff)];
            var pi1 = verts[(indices[Helper.Next(i, n)] & 0x7fff)];
            var pin1 = verts[(indices[Helper.Prev(i, n)] & 0x7fff)];

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn(pin1, pi, pi1))
            {
                return Left(pi, pj, pin1) && Left(pj, pi, pi1);
            }
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn(pi, pj, pi1) && LeftOn(pj, pi, pin1));
        }
        public static bool LeftOn(Int4 a, Int4 b, Int4 c)
        {
            return Area2(a, b, c) <= 0;
        }
        public static int Area2(Int4 a, Int4 b, Int4 c)
        {
            return (b.X - a.X) * (c.Z - a.Z) - (c.X - a.X) * (b.Z - a.Z);
        }
        public static bool Left(Int4 a, Int4 b, Int4 c)
        {
            return Area2(a, b, c) < 0;
        }
        public static bool Diagonalie(int i, int j, int n, Int4[] verts, int[] indices)
        {
            var d0 = verts[(indices[i] & 0x7fff)];
            var d1 = verts[(indices[j] & 0x7fff)];

            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = Helper.Next(k, n);
                // Skip edges incident to i or j
                if (!((k == i) || (k1 == i) || (k == j) || (k1 == j)))
                {
                    var p0 = verts[(indices[k] & 0x7fff)];
                    var p1 = verts[(indices[k1] & 0x7fff)];

                    if (Vequal(d0, p0) || Vequal(d1, p0) || Vequal(d0, p1) || Vequal(d1, p1))
                        continue;

                    if (Intersect(d0, d1, p0, p1))
                        return false;
                }
            }
            return true;
        }
        public static bool Vequal(Int4 a, Int4 b)
        {
            return a.X == b.X && a.Z == b.Z;
        }
        /// <summary>
        /// Returns true iff segments ab and cd intersect, properly or improperly.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool Intersect(Int4 a, Int4 b, Int4 c, Int4 d)
        {
            if (IntersectProp(a, b, c, d))
                return true;
            else if (Between(a, b, c) || Between(a, b, d) ||
                     Between(c, d, a) || Between(c, d, b))
                return true;
            else
                return false;
        }
        /// <summary>
        /// Returns true iff ab properly intersects cd: they share 
        /// a point interior to both segments.
        /// The properness of the intersection is ensured by using strict leftness.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool IntersectProp(Int4 a, Int4 b, Int4 c, Int4 d)
        {
            // Eliminate improper cases.
            if (Collinear(a, b, c) || Collinear(a, b, d) ||
                Collinear(c, d, a) || Collinear(c, d, b))
                return false;

            return Xorb(Left(a, b, c), Left(a, b, d)) && Xorb(Left(c, d, a), Left(c, d, b));
        }
        public static bool Collinear(Int4 a, Int4 b, Int4 c)
        {
            return Area2(a, b, c) == 0;
        }
        /// <summary>
        /// Exclusive or: true iff exactly one argument is true.
        /// The arguments are negated to ensure that they are 0/1 values.
        /// Then the bitwise Xor operator may apply.
        /// (This idea is due to Michael Baldwin.)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool Xorb(bool x, bool y)
        {
            return !x ^ !y;
        }
        /// <summary>
        /// Returns T iff (a,b,c) are collinear and point c lies on the closed segement ab.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool Between(Int4 a, Int4 b, Int4 c)
        {
            if (!Collinear(a, b, c))
            {
                return false;
            }

            // If ab not vertical, check betweenness on x; else on y.
            if (a.X != b.X)
            {
                return ((a.X <= c.X) && (c.X <= b.X)) || ((a.X >= c.X) && (c.X >= b.X));
            }
            else
            {
                return ((a.Z <= c.Z) && (c.Z <= b.Z)) || ((a.Z >= c.Z) && (c.Z >= b.Z));
            }
        }
        public static int AddVertex(int x, int y, int z, Int3[] verts, int[] firstVert, int[] nextVert, ref int nv)
        {
            int bucket = ComputeVertexHash2(x, 0, z);
            int i = firstVert[bucket];

            while (i != Constants.NullIdx)
            {
                var vx = verts[i];
                if (vx.X == x && vx.Z == z && (Math.Abs(vx.Y - y) <= 2))
                {
                    return i;
                }
                i = nextVert[i]; // next
            }

            // Could not find, create new.
            i = nv; nv++;
            verts[i] = new Int3(x, y, z);
            nextVert[i] = firstVert[bucket];
            firstVert[bucket] = i;

            return i;
        }
        public static int GetPolyMergeValue(Polygoni pa, Polygoni pb, Int3[] verts, out int ea, out int eb)
        {
            ea = -1;
            eb = -1;

            int na = CountPolyVerts(pa);
            int nb = CountPolyVerts(pb);

            // If the merged polygon would be too big, do not merge.
            if (na + nb - 2 > Constants.DT_VERTS_PER_POLYGON)
            {
                return -1;
            }

            // Check if the polygons share an edge.
            for (int i = 0; i < na; ++i)
            {
                int va0 = pa[i];
                int va1 = pa[(i + 1) % na];
                if (va0 > va1)
                {
                    Helper.Swap(ref va0, ref va1);
                }
                for (int j = 0; j < nb; ++j)
                {
                    int vb0 = pb[j];
                    int vb1 = pb[(j + 1) % nb];
                    if (vb0 > vb1)
                    {
                        Helper.Swap(ref vb0, ref vb1);
                    }
                    if (va0 == vb0 && va1 == vb1)
                    {
                        ea = i;
                        eb = j;
                        break;
                    }
                }
            }

            // No common edge, cannot merge.
            if (ea == -1 || eb == -1)
            {
                return -1;
            }

            // Check to see if the merged polygon would be convex.
            int va, vb, vc;

            va = pa[(ea + na - 1) % na];
            vb = pa[ea];
            vc = pb[(eb + 2) % nb];
            if (!Uleft(verts[va], verts[vb], verts[vc]))
            {
                return -1;
            }

            va = pb[(eb + nb - 1) % nb];
            vb = pb[eb];
            vc = pa[(ea + 2) % na];
            if (!Uleft(verts[va], verts[vb], verts[vc]))
            {
                return -1;
            }

            va = pa[ea];
            vb = pa[(ea + 1) % na];

            int dx = verts[va][0] - verts[vb][0];
            int dy = verts[va][2] - verts[vb][2];

            return dx * dx + dy * dy;
        }
        public static int CountPolyVerts(Polygoni p)
        {
            for (int i = 0; i < Constants.DT_VERTS_PER_POLYGON; ++i)
            {
                if (p[i] == Constants.NullIdx)
                {
                    return i;
                }
            }

            return Constants.DT_VERTS_PER_POLYGON;
        }
        public static Polygoni MergePolys(Polygoni pa, Polygoni pb, int ea, int eb)
        {
            int na = CountPolyVerts(pa);
            int nb = CountPolyVerts(pb);

            var tmp = new Polygoni(Math.Max(Constants.DT_VERTS_PER_POLYGON, na - 1 + nb - 1));

            // Merge polygons.
            int n = 0;
            // Add pa
            for (int i = 0; i < na - 1; ++i)
            {
                tmp[n++] = pa[(ea + 1 + i) % na];
            }
            // Add pb
            for (int i = 0; i < nb - 1; ++i)
            {
                tmp[n++] = pb[(eb + 1 + i) % nb];
            }

            return tmp;
        }
        public static bool Uleft(Int3 a, Int3 b, Int3 c)
        {
            return (b.X - a.X) * (c.Z - a.Z) - (c.X - a.X) * (b.Z - a.Z) < 0;
        }
        public static bool BuildMeshAdjacency(Polygoni[] polys, int npolys, Int3[] verts, int nverts, TileCacheContourSet lcset)
        {
            // Based on code by Eric Lengyel from:
            // http://www.terathon.com/code/edges.php

            int maxEdgeCount = npolys * Constants.DT_VERTS_PER_POLYGON;
            int[] firstEdge = new int[nverts];
            int[] nextEdge = new int[maxEdgeCount];
            int edgeCount = 0;

            Edge[] edges = new Edge[maxEdgeCount];

            for (int i = 0; i < nverts; i++)
            {
                firstEdge[i] = Constants.NullIdx;
            }
            for (int i = 0; i < maxEdgeCount; i++)
            {
                nextEdge[i] = Constants.NullIdx;
            }

            for (int i = 0; i < npolys; ++i)
            {
                var t = polys[i];
                for (int j = 0; j < Constants.DT_VERTS_PER_POLYGON; ++j)
                {
                    if (t[j] == Constants.NullIdx) break;
                    int v0 = t[j];
                    int v1 = (j + 1 >= Constants.DT_VERTS_PER_POLYGON || t[j + 1] == Constants.NullIdx) ? t[0] : t[j + 1];
                    if (v0 < v1)
                    {
                        Edge edge = new Edge()
                        {
                            vert = new int[2],
                            polyEdge = new int[2],
                            poly = new int[2],
                        };
                        edge.vert[0] = v0;
                        edge.vert[1] = v1;
                        edge.poly[0] = i;
                        edge.polyEdge[0] = j;
                        edge.poly[1] = i;
                        edge.polyEdge[1] = 0xff;
                        edges[edgeCount] = edge;
                        // Insert edge
                        nextEdge[edgeCount] = firstEdge[v0];
                        firstEdge[v0] = edgeCount;
                        edgeCount++;
                    }
                }
            }

            for (int i = 0; i < npolys; ++i)
            {
                var t = polys[i];
                for (int j = 0; j < Constants.DT_VERTS_PER_POLYGON; ++j)
                {
                    if (t[j] == Constants.NullIdx) break;
                    int v0 = t[j];
                    int v1 = (j + 1 >= Constants.DT_VERTS_PER_POLYGON || t[j + 1] == Constants.NullIdx) ? t[0] : t[j + 1];
                    if (v0 > v1)
                    {
                        bool found = false;
                        for (int e = firstEdge[v1]; e != Constants.NullIdx; e = nextEdge[e])
                        {
                            Edge edge = edges[e];
                            if (edge.vert[1] == v0 && edge.poly[0] == edge.poly[1])
                            {
                                edge.poly[1] = i;
                                edge.polyEdge[1] = j;
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            // Matching edge not found, it is an open edge, add it.
                            Edge edge = new Edge()
                            {
                                vert = new int[2],
                                polyEdge = new int[2],
                                poly = new int[2],
                            };
                            edge.vert[0] = v1;
                            edge.vert[1] = v0;
                            edge.poly[0] = i;
                            edge.polyEdge[0] = j;
                            edge.poly[1] = i;
                            edge.polyEdge[1] = 0xff;
                            edges[edgeCount] = edge;
                            // Insert edge
                            nextEdge[edgeCount] = firstEdge[v1];
                            firstEdge[v1] = edgeCount;
                            edgeCount++;
                        }
                    }
                }
            }

            // Mark portal edges.
            for (int i = 0; i < lcset.nconts; ++i)
            {
                TileCacheContour cont = lcset.conts[i];
                if (cont.nverts < 3)
                {
                    continue;
                }

                for (int j = 0, k = cont.nverts - 1; j < cont.nverts; k = j++)
                {
                    var va = cont.verts[k];
                    var vb = cont.verts[j];
                    int dir = va.W & 0xf;
                    if (dir == 0xf)
                    {
                        continue;
                    }

                    if (dir == 0 || dir == 2)
                    {
                        // Find matching vertical edge
                        int x = va.X;
                        int zmin = va.Z;
                        int zmax = vb.Z;
                        if (zmin > zmax)
                        {
                            Helper.Swap(ref zmin, ref zmax);
                        }

                        for (int m = 0; m < edgeCount; ++m)
                        {
                            Edge e = edges[m];
                            // Skip connected edges.
                            if (e.poly[0] != e.poly[1])
                            {
                                continue;
                            }
                            var eva = verts[e.vert[0]];
                            var evb = verts[e.vert[1]];
                            if (eva.X == x && evb.X == x)
                            {
                                int ezmin = eva.Z;
                                int ezmax = evb.Z;
                                if (ezmin > ezmax)
                                {
                                    Helper.Swap(ref ezmin, ref ezmax);
                                }
                                if (OverlapRangeExl(zmin, zmax, ezmin, ezmax))
                                {
                                    // Reuse the other polyedge to store dir.
                                    e.polyEdge[1] = dir;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Find matching vertical edge
                        int z = va.Z;
                        int xmin = va.X;
                        int xmax = vb.X;
                        if (xmin > xmax)
                        {
                            Helper.Swap(ref xmin, ref xmax);
                        }
                        for (int m = 0; m < edgeCount; ++m)
                        {
                            Edge e = edges[m];
                            // Skip connected edges.
                            if (e.poly[0] != e.poly[1])
                            {
                                continue;
                            }
                            var eva = verts[e.vert[0]];
                            var evb = verts[e.vert[1]];
                            if (eva.Z == z && evb.Z == z)
                            {
                                int exmin = eva.X;
                                int exmax = evb.X;
                                if (exmin > exmax)
                                {
                                    Helper.Swap(ref exmin, ref exmax);
                                }
                                if (OverlapRangeExl(xmin, xmax, exmin, exmax))
                                {
                                    // Reuse the other polyedge to store dir.
                                    e.polyEdge[1] = dir;
                                }
                            }
                        }
                    }
                }
            }

            // Store adjacency
            for (int i = 0; i < edgeCount; ++i)
            {
                Edge e = edges[i];
                if (e.poly[0] != e.poly[1])
                {
                    var p0 = polys[e.poly[0]];
                    var p1 = polys[e.poly[1]];
                    p0[Constants.DT_VERTS_PER_POLYGON + e.polyEdge[0]] = e.poly[1];
                    p1[Constants.DT_VERTS_PER_POLYGON + e.polyEdge[1]] = e.poly[0];
                }
                else if (e.polyEdge[1] != 0xff)
                {
                    var p0 = polys[e.poly[0]];
                    p0[Constants.DT_VERTS_PER_POLYGON + e.polyEdge[0]] = 0x8000 | e.polyEdge[1];
                }
            }

            return true;
        }
        public static bool BuildMeshAdjacency(Polygoni[] polys, int npolys, int nverts, int vertsPerPoly)
        {
            // Based on code by Eric Lengyel from:
            // http://www.terathon.com/code/edges.php

            int maxEdgeCount = npolys * vertsPerPoly;
            int[] firstEdge = new int[nverts];
            int[] nextEdge = new int[maxEdgeCount];
            int edgeCount = 0;

            Edge[] edges = new Edge[maxEdgeCount];

            for (int i = 0; i < nverts; i++)
            {
                firstEdge[i] = Constants.NullIdx;
            }
            for (int i = 0; i < maxEdgeCount; i++)
            {
                nextEdge[i] = Constants.NullIdx;
            }

            for (int i = 0; i < npolys; ++i)
            {
                var t = polys[i];
                for (int j = 0; j < vertsPerPoly; ++j)
                {
                    if (t[j] == Constants.NullIdx) break;
                    int v0 = t[j];
                    int v1 = (j + 1 >= vertsPerPoly || t[j + 1] == Constants.NullIdx) ? t[0] : t[j + 1];
                    if (v0 < v1)
                    {
                        Edge edge = new Edge()
                        {
                            vert = new int[2],
                            polyEdge = new int[2],
                            poly = new int[2],
                        };
                        edge.vert[0] = v0;
                        edge.vert[1] = v1;
                        edge.poly[0] = i;
                        edge.polyEdge[0] = j;
                        edge.poly[1] = i;
                        edge.polyEdge[1] = 0;
                        edges[edgeCount] = edge;
                        // Insert edge
                        nextEdge[edgeCount] = firstEdge[v0];
                        firstEdge[v0] = edgeCount;
                        edgeCount++;
                    }
                }
            }

            for (int i = 0; i < npolys; ++i)
            {
                var t = polys[i];
                for (int j = 0; j < vertsPerPoly; ++j)
                {
                    if (t[j] == Constants.NullIdx) break;
                    int v0 = t[j];
                    int v1 = (j + 1 >= vertsPerPoly || t[j + 1] == Constants.NullIdx) ? t[0] : t[j + 1];
                    if (v0 > v1)
                    {
                        for (int e = firstEdge[v1]; e != Constants.NullIdx; e = nextEdge[e])
                        {
                            Edge edge = edges[e];
                            if (edge.vert[1] == v0 && edge.poly[0] == edge.poly[1])
                            {
                                edge.poly[1] = i;
                                edge.polyEdge[1] = j;
                                break;
                            }
                        }
                    }
                }
            }

            // Store adjacency
            for (int i = 0; i < edgeCount; ++i)
            {
                Edge e = edges[i];
                if (e.poly[0] != e.poly[1])
                {
                    var p0 = polys[e.poly[0]];
                    var p1 = polys[e.poly[1]];
                    p0[vertsPerPoly + e.polyEdge[0]] = e.poly[1];
                    p1[vertsPerPoly + e.polyEdge[1]] = e.poly[0];
                }
            }

            return true;
        }
        public static bool OverlapRangeExl(int amin, int amax, int bmin, int bmax)
        {
            return (amin >= bmax || amax <= bmin) ? false : true;
        }
        public static float DistancePtSeg(int x, int z, int px, int pz, int qx, int qz)
        {
            float pqx = (qx - px);
            float pqz = (qz - pz);
            float dx = (x - px);
            float dz = (z - pz);
            float d = pqx * pqx + pqz * pqz;
            float t = pqx * dx + pqz * dz;
            if (d > 0)
            {
                t /= d;
            }
            if (t < 0)
            {
                t = 0;
            }
            else if (t > 1)
            {
                t = 1;
            }

            dx = px + t * pqx - x;
            dz = pz + t * pqz - z;

            return dx * dx + dz * dz;
        }
        public static float DistancePtSeg(Vector3 pt, Vector3 p, Vector3 q)
        {
            float pqx = q.X - p.X;
            float pqy = q.Y - p.Y;
            float pqz = q.Z - p.Z;
            float dx = pt.X - p.X;
            float dy = pt.Y - p.Y;
            float dz = pt.Z - p.Z;
            float d = pqx * pqx + pqy * pqy + pqz * pqz;
            float t = pqx * dx + pqy * dy + pqz * dz;
            if (d > 0)
            {
                t /= d;
            }
            if (t < 0)
            {
                t = 0;
            }
            else if (t > 1)
            {
                t = 1;
            }

            dx = p.X + t * pqx - pt.X;
            dy = p.Y + t * pqy - pt.Y;
            dz = p.Z + t * pqz - pt.Z;

            return dx * dx + dy * dy + dz * dz;
        }
        public static float DistancePtSeg2d(Vector3 pt, Vector3 p, Vector3 q)
        {
            float pqx = q[0] - p[0];
            float pqz = q[2] - p[2];
            float dx = pt[0] - p[0];
            float dz = pt[2] - p[2];
            float d = pqx * pqx + pqz * pqz;
            float t = pqx * dx + pqz * dz;
            if (d > 0)
            {
                t /= d;
            }
            if (t < 0)
            {
                t = 0;
            }
            else if (t > 1)
            {
                t = 1;
            }

            dx = p[0] + t * pqx - pt[0];
            dz = p[2] + t * pqz - pt[2];

            return dx * dx + dz * dz;
        }
        public static float DistPtTri(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 v0, v1, v2;
            v0 = Vector3.Subtract(c, a);
            v1 = Vector3.Subtract(b, a);
            v2 = Vector3.Subtract(p, a);

            float dot00 = Vector2.Dot(new Vector2(v0.X, v0.Z), new Vector2(v0.X, v0.Z));
            float dot01 = Vector2.Dot(new Vector2(v0.X, v0.Z), new Vector2(v1.X, v1.Z));
            float dot02 = Vector2.Dot(new Vector2(v0.X, v0.Z), new Vector2(v2.X, v2.Z));
            float dot11 = Vector2.Dot(new Vector2(v1.X, v1.Z), new Vector2(v1.X, v1.Z));
            float dot12 = Vector2.Dot(new Vector2(v1.X, v1.Z), new Vector2(v2.X, v2.Z));

            // Compute barycentric coordinates
            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // If point lies inside the triangle, return interpolated y-coord.
            float EPS = float.Epsilon;
            if (u >= -EPS && v >= -EPS && (u + v) <= 1 + EPS)
            {
                float y = a[1] + v0[1] * u + v1[1] * v;
                return Math.Abs(y - p[1]);
            }
            return float.MaxValue;
        }
        public static float PolyMinExtent(Vector3[] verts, int nverts)
        {
            float minDist = float.MaxValue;
            for (int i = 0; i < nverts; i++)
            {
                int ni = (i + 1) % nverts;
                Vector3 p1 = verts[i];
                Vector3 p2 = verts[ni];
                float maxEdgeDist = 0;
                for (int j = 0; j < nverts; j++)
                {
                    if (j == i || j == ni) continue;
                    float d = DistancePtSeg2d(verts[j], p1, p2);
                    maxEdgeDist = Math.Max(maxEdgeDist, d);
                }
                minDist = Math.Min(minDist, maxEdgeDist);
            }
            return (float)Math.Sqrt(minDist);
        }
        public static float DistToPoly(int nvert, Vector3[] verts, Vector3 p)
        {
            float dmin = float.MaxValue;
            bool c = false;
            for (int i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                Vector3 vi = verts[i];
                Vector3 vj = verts[j];
                if (((vi[2] > p[2]) != (vj[2] > p[2])) &&
                    (p[0] < (vj[0] - vi[0]) * (p[2] - vi[2]) / (vj[2] - vi[2]) + vi[0]))
                {
                    c = !c;
                }
                dmin = Math.Min(dmin, DistancePtSeg2d(p, vj, vi));
            }
            return c ? -dmin : dmin;
        }
        public static float DistToTriMesh(Vector3 p, Vector3[] verts, int nverts, Int4[] tris, int ntris)
        {
            float dmin = float.MaxValue;
            for (int i = 0; i < ntris; ++i)
            {
                var va = verts[tris[i].X];
                var vb = verts[tris[i].Y];
                var vc = verts[tris[i].Z];
                float d = DistPtTri(p, va, vb, vc);
                if (d < dmin)
                {
                    dmin = d;
                }
            }
            if (dmin == float.MaxValue) return -1;
            return dmin;
        }
        /// <summary>
        /// Derives the signed xz-plane area of the triangle ABC, or the relationship of line AB to point C.
        /// </summary>
        /// <param name="a">Vertex A. [(x, y, z)]</param>
        /// <param name="b">Vertex B. [(x, y, z)]</param>
        /// <param name="c">Vertex C. [(x, y, z)]</param>
        /// <returns>The signed xz-plane area of the triangle</returns>
        public static float TriArea2D(Vector3 a, Vector3 b, Vector3 c)
        {
            float abx = b.X - a.X;
            float abz = b.Z - a.Z;
            float acx = c.X - a.X;
            float acz = c.Z - a.Z;
            return acx * abz - abx * acz;
        }
        public static void PushFront<T>(T v, T[] arr, int an)
        {
            an++;
            for (int i = an - 1; i > 0; --i)
            {
                arr[i] = arr[i - 1];
            }
            arr[0] = v;
        }
        public static void PushBack<T>(T v, T[] arr, int an)
        {
            arr[an] = v;
            an++;
        }
        public static float GetJitterX(int i)
        {
            return (((i * 0x8da6b343) & 0xffff) / 65535.0f * 2.0f) - 1.0f;
        }
        public static float GetJitterY(int i)
        {
            return (((i * 0xd8163841) & 0xffff) / 65535.0f * 2.0f) - 1.0f;
        }
        public static void TriangulateHull(int nverts, Vector3[] verts, int nhull, int[] hull, List<Int4> tris)
        {
            int start = 0, left = 1, right = nhull - 1;

            // Start from an ear with shortest perimeter.
            // This tends to favor well formed triangles as starting point.
            float dmin = 0;
            for (int i = 0; i < nhull; i++)
            {
                int pi = Helper.Prev(i, nhull);
                int ni = Helper.Next(i, nhull);
                var pv = verts[hull[pi]];
                var cv = verts[hull[i]];
                var nv = verts[hull[ni]];
                float d =
                    Vector2.Distance(new Vector2(pv.X, pv.Z), new Vector2(cv.X, cv.Z)) +
                    Vector2.Distance(new Vector2(cv.X, cv.Z), new Vector2(nv.X, nv.Z)) +
                    Vector2.Distance(new Vector2(nv.X, nv.Z), new Vector2(pv.X, pv.Z));
                if (d < dmin)
                {
                    start = i;
                    left = ni;
                    right = pi;
                    dmin = d;
                }
            }

            // Add first triangle
            tris.Add(new Int4()
            {
                X = hull[start],
                Y = hull[left],
                Z = hull[right],
                W = 0,
            });

            // Triangulate the polygon by moving left or right,
            // depending on which triangle has shorter perimeter.
            // This heuristic was chose emprically, since it seems
            // handle tesselated straight edges well.
            while (Helper.Next(left, nhull) != right)
            {
                // Check to see if se should advance left or right.
                int nleft = Helper.Next(left, nhull);
                int nright = Helper.Prev(right, nhull);

                var cvleft = verts[hull[left]];
                var nvleft = verts[hull[nleft]];
                var cvright = verts[hull[right]];
                var nvright = verts[hull[nright]];
                float dleft =
                    Vector2.Distance(new Vector2(cvleft.X, cvleft.Z), new Vector2(nvleft.X, nvleft.Z)) +
                    Vector2.Distance(new Vector2(nvleft.X, nvleft.Z), new Vector2(cvright.X, cvright.Z));

                float dright =
                    Vector2.Distance(new Vector2(cvright.X, cvright.Z), new Vector2(nvright.X, nvright.Z)) +
                    Vector2.Distance(new Vector2(cvleft.X, cvleft.Z), new Vector2(nvright.X, nvright.Z));

                if (dleft < dright)
                {
                    tris.Add(new Int4()
                    {
                        X = hull[left],
                        Y = hull[nleft],
                        Z = hull[right],
                        W = 0,
                    });

                    left = nleft;
                }
                else
                {
                    tris.Add(new Int4()
                    {
                        X = hull[left],
                        Y = hull[nright],
                        Z = hull[right],
                        W = 0,
                    });

                    right = nright;
                }
            }
        }
        public static void DelaunayHull(int npts, Vector3[] pts, int nhull, int[] hull, List<Int4> outTris, List<Int4> outEdges)
        {
            int nfaces = 0;
            int nedges = 0;
            int maxEdges = npts * 10;
            Int4[] edges = new Int4[maxEdges];

            for (int i = 0, j = nhull - 1; i < nhull; j = i++)
            {
                AddEdge(ref edges, ref nedges, maxEdges, hull[j], hull[i], (int)EdgeValues.EV_HULL, (int)EdgeValues.EV_UNDEF);
            }

            int currentEdge = 0;
            while (currentEdge < nedges)
            {
                if (edges[currentEdge][2] == (int)EdgeValues.EV_UNDEF)
                {
                    CompleteFacet(pts, npts, ref edges, ref nedges, maxEdges, ref nfaces, currentEdge);
                }
                if (edges[currentEdge][3] == (int)EdgeValues.EV_UNDEF)
                {
                    CompleteFacet(pts, npts, ref edges, ref nedges, maxEdges, ref nfaces, currentEdge);
                }
                currentEdge++;
            }

            // Create tris
            Int4[] tris = Helper.CreateArray(nfaces, new Int4(-1, -1, -1, -1));

            for (int i = 0; i < nedges; ++i)
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

            for (int i = 0; i < tris.Length; ++i)
            {
                var t = tris[i];
                if (t.X == -1 || t.Y == -1 || t.Z == -1)
                {
                    //ctx->log(RC_LOG_WARNING, "delaunayHull: Removing dangling face %d [%d,%d,%d].", i, t[0], t[1], t[2]);
                    tris[i] = tris[tris.Length - 1];
                    Array.Resize(ref tris, tris.Length - 1);
                    i--;
                }
            }

            outTris.AddRange(tris);
            outEdges.AddRange(edges);
        }
        public static int AddEdge(ref Int4[] edges, ref int nedges, int maxEdges, int s, int t, int l, int r)
        {
            if (nedges >= maxEdges)
            {
                //ctx->log(RC_LOG_ERROR, "addEdge: Too many edges (%d/%d).", nedges, maxEdges);
                return (int)EdgeValues.EV_UNDEF;
            }

            // Add edge if not already in the triangulation.
            int e = FindEdge(edges, nedges, s, t);
            if (e == (int)EdgeValues.EV_UNDEF)
            {
                edges[nedges] = new Int4(s, t, l, r);
                return nedges++;
            }
            else
            {
                return (int)EdgeValues.EV_UNDEF;
            }
        }
        public static int FindEdge(Int4[] edges, int nedges, int s, int t)
        {
            for (int i = 0; i < nedges; i++)
            {
                var e = edges[i];
                if ((e.X == s && e.Y == t) || (e.X == t && e.Y == s))
                {
                    return i;
                }
            }
            return (int)EdgeValues.EV_UNDEF;
        }
        public static void CompleteFacet(Vector3[] pts, int npts, ref Int4[] edges, ref int nedges, int maxEdges, ref int nfaces, int e)
        {
            float EPS = float.Epsilon;

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
            int pt = npts;
            Vector3 c = new Vector3();
            float r = -1;
            for (int u = 0; u < npts; ++u)
            {
                if (u == s || u == t) continue;
                if (VCross2(pts[s], pts[t], pts[u]) > EPS)
                {
                    if (r < 0)
                    {
                        // The circle is not updated yet, do it now.
                        pt = u;
                        CircumCircle(pts[s], pts[t], pts[u], out c, out r);
                        continue;
                    }
                    float d = VDist2(c, pts[u]);
                    float tol = 0.001f;
                    if (d > r * (1 + tol))
                    {
                        // Outside current circumcircle, skip.
                        continue;
                    }
                    else if (d < r * (1 - tol))
                    {
                        // Inside safe circumcircle, update circle.
                        pt = u;
                        CircumCircle(pts[s], pts[t], pts[u], out c, out r);
                    }
                    else
                    {
                        // Inside epsilon circum circle, do extra tests to make sure the edge is valid.
                        // s-u and t-u cannot overlap with s-pt nor t-pt if they exists.
                        if (OverlapEdges(pts, edges, nedges, s, u))
                        {
                            continue;
                        }
                        if (OverlapEdges(pts, edges, nedges, t, u))
                        {
                            continue;
                        }
                        // Edge is valid.
                        pt = u;
                        CircumCircle(pts[s], pts[t], pts[u], out c, out r);
                    }
                }
            }

            // Add new triangle or update edge info if s-t is on hull.
            if (pt < npts)
            {
                // Update face information of edge being completed.
                UpdateLeftFace(ref edges[e], s, t, nfaces);

                // Add new edge or update face info of old edge.
                e = FindEdge(edges, nedges, pt, s);
                if (e == (int)EdgeValues.EV_UNDEF)
                {
                    AddEdge(ref edges, ref nedges, maxEdges, pt, s, nfaces, (int)EdgeValues.EV_UNDEF);
                }
                else
                {
                    UpdateLeftFace(ref edges[e], pt, s, nfaces);
                }

                // Add new edge or update face info of old edge.
                e = FindEdge(edges, nedges, t, pt);
                if (e == (int)EdgeValues.EV_UNDEF)
                {
                    AddEdge(ref edges, ref nedges, maxEdges, t, pt, nfaces, (int)EdgeValues.EV_UNDEF);
                }
                else
                {
                    UpdateLeftFace(ref edges[e], t, pt, nfaces);
                }

                nfaces++;
            }
            else
            {
                UpdateLeftFace(ref edges[e], s, t, (int)EdgeValues.EV_HULL);
            }
        }
        public static float VCross2(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u1 = p2[0] - p1[0];
            float v1 = p2[2] - p1[2];
            float u2 = p3[0] - p1[0];
            float v2 = p3[2] - p1[2];
            return u1 * v2 - v1 * u2;
        }
        public static bool CircumCircle(Vector3 p1, Vector3 p2, Vector3 p3, out Vector3 c, out float r)
        {
            float EPS = 1e-6f;
            // Calculate the circle relative to p1, to avoid some precision issues.
            Vector3 v1 = new Vector3();
            Vector3 v2 = Vector3.Subtract(p2, p1);
            Vector3 v3 = Vector3.Subtract(p3, p1);

            c = new Vector3();
            float cp = VCross2(v1, v2, v3);
            if (Math.Abs(cp) > EPS)
            {
                float v1Sq = VDot2(v1, v1);
                float v2Sq = VDot2(v2, v2);
                float v3Sq = VDot2(v3, v3);
                c[0] = (v1Sq * (v2[2] - v3[2]) + v2Sq * (v3[2] - v1[2]) + v3Sq * (v1[2] - v2[2])) / (2 * cp);
                c[1] = 0;
                c[2] = (v1Sq * (v3[0] - v2[0]) + v2Sq * (v1[0] - v3[0]) + v3Sq * (v2[0] - v1[0])) / (2 * cp);
                r = VDist2(c, v1);
                c = Vector3.Add(c, p1);
                return true;
            }

            c = p1;
            r = 0;
            return false;
        }
        public static float VDot2(Vector3 a, Vector3 b)
        {
            return a[0] * b[0] + a[2] * b[2];
        }
        public static float VDistSq2(Vector3 p, Vector3 q)
        {
            float dx = q[0] - p[0];
            float dy = q[2] - p[2];
            return dx * dx + dy * dy;
        }
        public static float VDist2(Vector3 p, Vector3 q)
        {
            return (float)Math.Sqrt(VDistSq2(p, q));
        }
        public static void UpdateLeftFace(ref Int4 e, int s, int t, int f)
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
        public static bool OverlapEdges(Vector3[] pts, Int4[] edges, int nedges, int s1, int t1)
        {
            for (int i = 0; i < nedges; ++i)
            {
                int s0 = edges[i].X;
                int t0 = edges[i].Y;
                // Same or connected edges do not overlap.
                if (s0 == s1 || s0 == t1 || t0 == s1 || t0 == t1)
                {
                    continue;
                }
                if (OverlapSegSeg2d(pts[s0], pts[t0], pts[s1], pts[t1]) != 0)
                {
                    return true;
                }
            }
            return false;
        }
        public static int OverlapSegSeg2d(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            float a1 = VCross2(a, b, d);
            float a2 = VCross2(a, b, c);
            if (a1 * a2 < 0.0f)
            {
                float a3 = VCross2(c, d, a);
                float a4 = a3 + a2 - a1;
                if (a3 * a4 < 0.0f)
                {
                    return 1;
                }
            }
            return 0;
        }
        public static int ClassifyOffMeshPoint(Vector3 pt, Vector3 bmin, Vector3 bmax)
        {
            int XP = 1 << 0;
            int ZP = 1 << 1;
            int XM = 1 << 2;
            int ZM = 1 << 3;

            int outcode = 0;
            outcode |= (pt[0] >= bmax[0]) ? XP : 0;
            outcode |= (pt[2] >= bmax[2]) ? ZP : 0;
            outcode |= (pt[0] < bmin[0]) ? XM : 0;
            outcode |= (pt[2] < bmin[2]) ? ZM : 0;

            if (XP != 0) return 0;
            if ((XP | ZP) != 0) return 1;
            if (ZP != 0) return 2;
            if ((XM | ZP) != 0) return 3;
            if (XM != 0) return 4;
            if ((XM | ZM) != 0) return 5;
            if (ZM != 0) return 6;
            if ((XP | ZM) != 0) return 7;

            return 0xff;
        }
        public static bool PointInPoly(int nvert, Vector3[] verts, Vector3 p)
        {
            bool c = false;

            for (int i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                Vector3 vi = verts[i];
                Vector3 vj = verts[j];
                if (((vi[2] > p[2]) != (vj[2] > p[2])) &&
                    (p[0] < (vj[0] - vi[0]) * (p[2] - vi[2]) / (vj[2] - vi[2]) + vi[0]))
                {
                    c = !c;
                }
            }

            return c;
        }
        public static int GetTriFlags(Vector3 va, Vector3 vb, Vector3 vc, Vector3[] vpoly, int npoly)
        {
            int flags = 0;
            flags |= GetEdgeFlags(va, vb, vpoly, npoly) << 0;
            flags |= GetEdgeFlags(vb, vc, vpoly, npoly) << 2;
            flags |= GetEdgeFlags(vc, va, vpoly, npoly) << 4;
            return flags;
        }
        public static int GetEdgeFlags(Vector3 va, Vector3 vb, Vector3[] vpoly, int npoly)
        {
            // Return true if edge (va,vb) is part of the polygon.
            float thrSqr = 0.001f * 0.001f;
            for (int i = 0, j = npoly - 1; i < npoly; j = i++)
            {
                if (DistancePtSeg2d(va, vpoly[j], vpoly[i]) < thrSqr &&
                    DistancePtSeg2d(vb, vpoly[j], vpoly[i]) < thrSqr)
                {
                    return 1;
                }
            }
            return 0;
        }
        public static bool CheckOverlapRect(Vector2 amin, Vector2 amax, Vector2 bmin, Vector2 bmax)
        {
            bool overlap = true;
            overlap = (amin.X > bmax.X || amax.X < bmin.X) ? false : overlap;
            overlap = (amin.Y > bmax.Y || amax.Y < bmin.Y) ? false : overlap;
            return overlap;
        }
        public static void DividePoly(List<Vector3> inPoly, List<Vector3> outPoly1, List<Vector3> outPoly2, float x, int axis)
        {
            float[] d = new float[inPoly.Count];
            for (int i = 0; i < inPoly.Count; i++)
            {
                d[i] = x - inPoly[i][axis];
            }

            for (int i = 0, j = inPoly.Count - 1; i < inPoly.Count; j = i, i++)
            {
                bool ina = d[j] >= 0;
                bool inb = d[i] >= 0;
                if (ina != inb)
                {
                    float s = d[j] / (d[j] - d[i]);
                    Vector3 v;
                    v.X = inPoly[j].X + (inPoly[i].X - inPoly[j].X) * s;
                    v.Y = inPoly[j].Y + (inPoly[i].Y - inPoly[j].Y) * s;
                    v.Z = inPoly[j].Z + (inPoly[i].Z - inPoly[j].Z) * s;
                    outPoly1.Add(v);
                    outPoly2.Add(v);

                    // add the i'th point to the right polygon. Do NOT add points that are on the dividing line
                    // since these were already added above
                    if (d[i] > 0)
                    {
                        outPoly1.Add(inPoly[i]);
                    }
                    else if (d[i] < 0)
                    {
                        outPoly2.Add(inPoly[i]);
                    }
                }
                else // same side
                {
                    // add the i'th point to the right polygon. Addition is done even for points on the dividing line
                    if (d[i] >= 0)
                    {
                        outPoly1.Add(inPoly[i]);

                        if (d[i] != 0)
                        {
                            continue;
                        }
                    }

                    outPoly2.Add(inPoly[i]);
                }
            }
        }
        public static int CalcAreaOfPolygon2D(Int4[] verts, int nverts)
        {
            int area = 0;
            for (int i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                var vi = verts[i];
                var vj = verts[j];
                area += vi.X * vj.Z - vj.X * vi.Z;
            }
            return (area + 1) / 2;
        }
        public static bool OverlapRange(int amin, int amax, int bmin, int bmax)
        {
            return (amin > bmax || amax < bmin) ? false : true;
        }
        public static bool Contains(int[] a, int an, int v)
        {
            int n = an;

            for (int i = 0; i < n; ++i)
            {
                if (a[i] == v)
                {
                    return true;
                }
            }

            return false;
        }
        public static bool AddUnique(int[] a, ref int an, int anMax, int v)
        {
            if (Contains(a, an, v))
            {
                return true;
            }

            if (an >= anMax)
            {
                return false;
            }

            a[an] = v;
            an++;

            return true;
        }
        public static void Push3(List<int> queue, int v1, int v2, int v3)
        {
            queue.Add(v1);
            queue.Add(v2);
            queue.Add(v3);
        }
        public static int LongestAxis(int x, int y, int z)
        {
            int axis = 0;
            int maxVal = x;
            if (y > maxVal)
            {
                axis = 1;
                maxVal = y;
            }
            if (z > maxVal)
            {
                axis = 2;
            }
            return axis;
        }
        public static bool OverlapQuantBounds(Int3 amin, Int3 amax, Int3 bmin, Int3 bmax)
        {
            bool overlap = true;
            overlap = (amin.X > bmax.X || amax.X < bmin.X) ? false : overlap;
            overlap = (amin.Y > bmax.Y || amax.Y < bmin.Y) ? false : overlap;
            overlap = (amin.Z > bmax.Z || amax.Z < bmin.Z) ? false : overlap;
            return overlap;
        }
        public static bool DistancePtPolyEdgesSqr(Vector3 pt, Vector3[] verts, int nverts, out float[] ed, out float[] et)
        {
            ed = new float[nverts];
            et = new float[nverts];

            // TODO: Replace pnpoly with triArea2D tests?
            int i, j;
            bool c = false;
            for (i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                var vi = verts[i];
                var vj = verts[j];
                if (((vi.Z > pt.Z) != (vj.Z > pt.Z)) &&
                    (pt.X < (vj.X - vi.X) * (pt.Z - vi.Z) / (vj.Z - vi.Z) + vi.X))
                {
                    c = !c;
                }
                ed[j] = DistancePtSegSqr2D(pt, vj, vi, out et[j]);
            }
            return c;
        }
        public static float DistancePtSegSqr2D(Vector3 pt, Vector3 p, Vector3 q, out float t)
        {
            float pqx = q.X - p.X;
            float pqz = q.Z - p.Z;
            float dx = pt.X - p.X;
            float dz = pt.Z - p.Z;
            float d = pqx * pqx + pqz * pqz;
            t = pqx * dx + pqz * dz;
            if (d > 0) t /= d;
            if (t < 0) t = 0;
            else if (t > 1) t = 1;
            dx = p.X + t * pqx - pt.X;
            dz = p.Z + t * pqz - pt.Z;
            return dx * dx + dz * dz;
        }
        public static bool ClosestHeightPointTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c, out float h)
        {
            h = float.MaxValue;

            Vector3 v0 = Vector3.Subtract(c, a);
            Vector3 v1 = Vector3.Subtract(b, a);
            Vector3 v2 = Vector3.Subtract(p, a);

            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            // Compute barycentric coordinates
            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // The (sloppy) epsilon is needed to allow to get height of points which
            // are interpolated along the edges of the triangles.
            float EPS = 1e-4f;

            // If point lies inside the triangle, return interpolated ycoord.
            if (u >= -EPS && v >= -EPS && (u + v) <= 1 + EPS)
            {
                h = a.Y + v0.Y * u + v1.Y * v;
                return true;
            }

            return false;
        }
        public static int HashRef(int a)
        {
            a += ~(a << 15);
            a ^= (a >> 10);
            a += (a << 3);
            a ^= (a >> 6);
            a += ~(a << 11);
            a ^= (a >> 16);
            return a;
        }
        /// <summary>
        /// Returns a random point in a convex polygon.
        /// Adapted from Graphics Gems article.
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="npts"></param>
        /// <param name="areas"></param>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <param name="outPoint"></param>
        public static void RandomPointInConvexPoly(Vector3[] pts, int npts, out float[] areas, float s, float t, out Vector3 outPoint)
        {
            areas = new float[Constants.DT_VERTS_PER_POLYGON];

            // Calc triangle araes
            float areasum = 0.0f;
            for (int i = 2; i < npts; i++)
            {
                areas[i] = PolyUtils.TriArea2D(pts[0], pts[(i - 1)], pts[i]);
                areasum += Math.Max(0.001f, areas[i]);
            }
            // Find sub triangle weighted by area.
            float thr = s * areasum;
            float acc = 0.0f;
            float u = 1.0f;
            int tri = npts - 1;
            for (int i = 2; i < npts; i++)
            {
                float dacc = areas[i];
                if (thr >= acc && thr < (acc + dacc))
                {
                    u = (thr - acc) / dacc;
                    tri = i;
                    break;
                }
                acc += dacc;
            }

            float v = (float)Math.Sqrt(t);

            float a = 1 - v;
            float b = (1 - u) * v;
            float c = u * v;
            Vector3 pa = pts[0];
            Vector3 pb = pts[(tri - 1)];
            Vector3 pc = pts[tri];

            outPoint = a * pa + b * pb + c * pc;
        }
        public static bool IntersectSegmentPoly2D(Vector3 p0, Vector3 p1, Vector3[] verts, int nverts, out float tmin, out float tmax, out int segMin, out int segMax)
        {
            float EPS = 0.00000001f;

            tmin = 0;
            tmax = 1;
            segMin = -1;
            segMax = -1;

            Vector3 dir = Vector3.Subtract(p1, p0);

            for (int i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                Vector3 edge = Vector3.Subtract(verts[i], verts[j]);
                Vector3 diff = Vector3.Subtract(p0, verts[j]);
                float n = Vperp2D(edge, diff);
                float d = Vperp2D(dir, edge);
                if (Math.Abs(d) < EPS)
                {
                    // S is nearly parallel to this edge
                    if (n < 0)
                    {
                        return false;
                    }
                    else
                    {
                        continue;
                    }
                }
                float t = n / d;
                if (d < 0)
                {
                    // segment S is entering across this edge
                    if (t > tmin)
                    {
                        tmin = t;
                        segMin = j;
                        // S enters after leaving polygon
                        if (tmin > tmax)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    // segment S is leaving across this edge
                    if (t < tmax)
                    {
                        tmax = t;
                        segMax = j;
                        // S leaves before entering polygon
                        if (tmax < tmin)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
        public static float Vperp2D(Vector3 u, Vector3 v)
        {
            return u.Z * v.X - u.X * v.Z;
        }
        public static float VperpXZ(Vector3 a, Vector3 b)
        {
            return a.X * b.Z - a.Z * b.X;
        }
        public static bool Vequal(Vector3 p0, Vector3 p1)
        {
            float thr = (float)Math.Pow(1.0f / 16384.0f, 2);
            float d = Vector3.DistanceSquared(p0, p1);
            return d < thr;
        }
        public static bool IntersectSegSeg2D(Vector3 ap, Vector3 aq, Vector3 bp, Vector3 bq, out float s, out float t)
        {
            s = 0;
            t = 0;

            Vector3 u = Vector3.Subtract(aq, ap);
            Vector3 v = Vector3.Subtract(bq, bp);
            Vector3 w = Vector3.Subtract(ap, bp);
            float d = VperpXZ(u, v);
            if (Math.Abs(d) < 1e-6f) return false;
            s = VperpXZ(v, w) / d;
            t = VperpXZ(u, w) / d;
            return true;
        }
        /// <summary>
        /// All points are projected onto the xz-plane, so the y-values are ignored.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="verts"></param>
        /// <param name="nverts"></param>
        /// <returns></returns>
        public static bool PointInPolygon(Vector3 pt, Vector3[] verts, int nverts)
        {
            // TODO: Replace pnpoly with triArea2D tests?
            int i, j;
            bool c = false;
            for (i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                Vector3 vi = verts[i];
                Vector3 vj = verts[j];
                if (((vi.Z > pt.Z) != (vj.Z > pt.Z)) &&
                    (pt.X < (vj.X - vi.X) * (pt.Z - vi.Z) / (vj.Z - vi.Z) + vi.X))
                {
                    c = !c;
                }
            }
            return c;
        }
        public static bool OverlapPolyPoly2D(Vector3[] polya, int npolya, Vector3[] polyb, int npolyb)
        {
            float eps = 1e-4f;

            for (int i = 0, j = npolya - 1; i < npolya; j = i++)
            {
                Vector3 va = polya[j];
                Vector3 vb = polya[i];
                Vector3 n = new Vector3(vb.Z - va.Z, 0, -(vb.X - va.X));
                ProjectPoly(n, polya, npolya, out float amin, out float amax);
                ProjectPoly(n, polyb, npolyb, out float bmin, out float bmax);
                if (!OverlapRange(amin, amax, bmin, bmax, eps))
                {
                    // Found separating axis
                    return false;
                }
            }
            for (int i = 0, j = npolyb - 1; i < npolyb; j = i++)
            {
                Vector3 va = polyb[j];
                Vector3 vb = polyb[i];
                Vector3 n = new Vector3(vb.Z - va.Z, 0, -(vb.X - va.X));
                ProjectPoly(n, polya, npolya, out float amin, out float amax);
                ProjectPoly(n, polyb, npolyb, out float bmin, out float bmax);
                if (!OverlapRange(amin, amax, bmin, bmax, eps))
                {
                    // Found separating axis
                    return false;
                }
            }
            return true;
        }
        public static void ProjectPoly(Vector3 axis, Vector3[] poly, int npoly, out float rmin, out float rmax)
        {
            rmin = rmax = Vector2.Dot(axis.XZ(), poly[0].XZ());
            for (int i = 1; i < npoly; ++i)
            {
                float d = Vector2.Dot(axis.XZ(), poly[i].XZ());
                rmin = Math.Min(rmin, d);
                rmax = Math.Max(rmax, d);
            }
        }
        public static bool OverlapRange(float amin, float amax, float bmin, float bmax, float eps)
        {
            return ((amin + eps) > bmax || (amax - eps) < bmin) ? false : true;
        }
    }
}
