using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Contains triangle meshes that represent detailed height data associated with the polygons in its associated polygon mesh object.
    /// </summary>
    class PolyMeshDetail
    {
        public static PolyMeshDetail BuildPolyMeshDetail(PolyMesh mesh, CompactHeightfield chf, float sampleDist, float sampleMaxError)
        {
            if (mesh.NVerts == 0 || mesh.NPolys == 0)
            {
                return null;
            }

            PolyMeshDetail dmesh = new PolyMeshDetail();

            dmesh.StoreData(mesh, chf, sampleDist, sampleMaxError);

            return dmesh;
        }
        private static Rectangle[] GetBounds(PolyMesh mesh, CompactHeightfield chf, out int maxhw, out int maxhh)
        {
            maxhw = 0;
            maxhh = 0;

            Rectangle[] bounds = new Rectangle[mesh.NPolys];
            int nPolyVerts = 0;
            int nvp = mesh.NVP;

            // Find max size for a polygon area.
            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];
                int xmin = chf.Width;
                int xmax = 0;
                int ymin = chf.Height;
                int ymax = 0;
                for (int j = 0; j < nvp; ++j)
                {
                    if (p[j] == RecastUtils.RC_MESH_NULL_IDX)
                    {
                        break;
                    }

                    var v = mesh.Verts[p[j]];
                    xmin = Math.Min(xmin, v.X);
                    xmax = Math.Max(xmax, v.X);
                    ymin = Math.Min(ymin, v.Z);
                    ymax = Math.Max(ymax, v.Z);
                    nPolyVerts++;
                }
                xmin = Math.Max(0, xmin - 1);
                xmax = Math.Min(chf.Width, xmax + 1);
                ymin = Math.Max(0, ymin - 1);
                ymax = Math.Min(chf.Height, ymax + 1);
                bounds[i] = new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
                if (xmin >= xmax || ymin >= ymax)
                {
                    continue;
                }

                maxhw = Math.Max(maxhw, xmax - xmin);
                maxhh = Math.Max(maxhh, ymax - ymin);
            }

            return bounds;
        }
        private static int GetEdgeFlags(Vector3 va, Vector3 vb, IEnumerable<Vector3> vpoly)
        {
            int npoly = vpoly.Count();

            // Return true if edge (va,vb) is part of the polygon.
            float thrSqr = 0.001f * 0.001f;
            for (int i = 0, j = npoly - 1; i < npoly; j = i++)
            {
                var vi = vpoly.ElementAt(i);
                var vj = vpoly.ElementAt(j);
                if (RecastUtils.DistancePtSeg2d(va, vj, vi) < thrSqr &&
                    RecastUtils.DistancePtSeg2d(vb, vj, vi) < thrSqr)
                {
                    return 1;
                }
            }
            return 0;
        }
        private static int GetTriFlags(Vector3 va, Vector3 vb, Vector3 vc, IEnumerable<Vector3> vpoly)
        {
            int flags = 0;
            flags |= GetEdgeFlags(va, vb, vpoly) << 0;
            flags |= GetEdgeFlags(vb, vc, vpoly) << 2;
            flags |= GetEdgeFlags(vc, va, vpoly) << 4;
            return flags;
        }
        private static bool BuildPolyDetail(Vector3[] inp, BuildPolyDetailParams param, CompactHeightfield chf, HeightPatch hp, out Vector3[] outVerts, out Int3[] outTris)
        {
            float sampleDist = param.SampleDist;
            float sampleMaxError = param.SampleMaxError;
            int heightSearchRadius = param.HeightSearchRadius;
            int ninp = inp.Length;
            List<Vector3> verts = new List<Vector3>();
            List<Int4> edges = new List<Int4>();
            List<Int4> samples = new List<Int4>();
            List<Int3> tris = new List<Int3>();

            int MAX_VERTS = 127;
            int MAX_TRIS = 255;    // Max tris for delaunay is 2n-2-k (n=num verts, k=num hull verts).
            int MAX_VERTS_PER_EDGE = 32;
            Vector3[] edge = new Vector3[(MAX_VERTS_PER_EDGE + 1)];
            int[] hull = new int[MAX_VERTS];
            int nhull = 0;

            for (int i = 0; i < ninp; i++)
            {
                verts.Add(inp[i]);
            }

            edges.Clear();

            float cs = chf.CS;
            float ics = 1.0f / cs;

            // Calculate minimum extents of the polygon based on input data.
            float minExtent = PolyMinExtent(verts.ToArray());

            // Tessellate outlines.
            // This is done in separate pass in order to ensure
            // seamless height values across the ply boundaries.
            if (sampleDist > 0)
            {
                for (int i = 0, j = ninp - 1; i < ninp; j = i++)
                {
                    var vj = inp[j];
                    var vi = inp[i];
                    bool swapped = false;
                    // Make sure the segments are always handled in same order
                    // using lexological sort or else there will be seams.
                    if (Math.Abs(vj.X - vi.X) < 1e-6f)
                    {
                        if (vj.Z > vi.Z)
                        {
                            Helper.Swap(ref vj, ref vi);
                            swapped = true;
                        }
                    }
                    else
                    {
                        if (vj.X > vi.X)
                        {
                            Helper.Swap(ref vj, ref vi);
                            swapped = true;
                        }
                    }
                    // Create samples along the edge.
                    float dx = vi.X - vj.X;
                    float dy = vi.Y - vj.Y;
                    float dz = vi.Z - vj.Z;
                    float d = (float)Math.Sqrt(dx * dx + dz * dz);
                    int nn = 1 + (int)Math.Floor(d / sampleDist);
                    if (nn >= MAX_VERTS_PER_EDGE) nn = MAX_VERTS_PER_EDGE - 1;
                    if (verts.Count + nn >= MAX_VERTS)
                    {
                        nn = MAX_VERTS - 1 - verts.Count;
                    }

                    for (int k = 0; k <= nn; ++k)
                    {
                        float u = ((float)k / (float)nn);
                        Vector3 pos = new Vector3
                        {
                            X = vj.X + dx * u,
                            Y = vj.Y + dy * u,
                            Z = vj.Z + dz * u
                        };
                        pos.Y = hp.GetHeight(pos, ics, chf.CH, heightSearchRadius) * chf.CH;
                        edge[k] = pos;
                    }
                    // Simplify samples.
                    int[] idx = new int[MAX_VERTS_PER_EDGE];
                    idx[0] = 0;
                    idx[1] = nn;
                    int nidx = 2;
                    for (int k = 0; k < nidx - 1;)
                    {
                        int a = idx[k];
                        int b = idx[k + 1];
                        var va = edge[a];
                        var vb = edge[b];
                        // Find maximum deviation along the segment.
                        float maxd = 0;
                        int maxi = -1;
                        for (int m = a + 1; m < b; ++m)
                        {
                            float dev = RecastUtils.DistancePtSeg(edge[m], va, vb);
                            if (dev > maxd)
                            {
                                maxd = dev;
                                maxi = m;
                            }
                        }
                        // If the max deviation is larger than accepted error,
                        // add new point, else continue to next segment.
                        if (maxi != -1 && maxd > (sampleMaxError * sampleMaxError))
                        {
                            for (int m = nidx; m > k; --m)
                            {
                                idx[m] = idx[m - 1];
                            }
                            idx[k + 1] = maxi;
                            nidx++;
                        }
                        else
                        {
                            ++k;
                        }
                    }

                    hull[nhull++] = j;
                    // Add new vertices.
                    if (swapped)
                    {
                        for (int k = nidx - 2; k > 0; --k)
                        {
                            verts.Add(edge[idx[k]]);
                            hull[nhull++] = verts.Count - 1;
                        }
                    }
                    else
                    {
                        for (int k = 1; k < nidx - 1; ++k)
                        {
                            verts.Add(edge[idx[k]]);
                            hull[nhull++] = verts.Count - 1;
                        }
                    }
                }
            }

            // If the polygon minimum extent is small (sliver or small triangle), do not try to add internal points.
            if (minExtent < sampleDist * 2)
            {
                TriangulateHull(verts.ToArray(), nhull, hull, ninp, tris);

                outVerts = verts.ToArray();
                outTris = tris.ToArray();

                return true;
            }

            // Tessellate the base mesh.
            // We're using the triangulateHull instead of delaunayHull as it tends to
            // create a bit better triangulation for long thin triangles when there
            // are no internal points.
            TriangulateHull(verts.ToArray(), nhull, hull, ninp, tris);

            if (tris.Count == 0)
            {
                // Could not triangulate the poly, make sure there is some valid data there.
                Console.WriteLine($"buildPolyDetail: Could not triangulate polygon ({verts.Count} verts).");

                outVerts = verts.ToArray();
                outTris = tris.ToArray();

                return true;
            }

            if (sampleDist > 0)
            {
                // Create sample locations in a grid.
                Vector3 bmin, bmax;
                bmin = inp[0];
                bmax = inp[0];
                for (int i = 1; i < ninp; ++i)
                {
                    bmin = Vector3.Min(bmin, inp[i]);
                    bmax = Vector3.Max(bmax, inp[i]);
                }
                int x0 = (int)Math.Floor(bmin.X / sampleDist);
                int x1 = (int)Math.Ceiling(bmax.X / sampleDist);
                int z0 = (int)Math.Floor(bmin.Z / sampleDist);
                int z1 = (int)Math.Ceiling(bmax.Z / sampleDist);
                samples.Clear();
                for (int z = z0; z < z1; ++z)
                {
                    for (int x = x0; x < x1; ++x)
                    {
                        Vector3 pt = new Vector3
                        {
                            X = x * sampleDist,
                            Y = (bmax.Y + bmin.Y) * 0.5f,
                            Z = z * sampleDist
                        };
                        // Make sure the samples are not too close to the edges.
                        if (DistToPoly(ninp, inp, pt) > -sampleDist / 2)
                        {
                            continue;
                        }
                        int y = hp.GetHeight(pt, ics, chf.CH, heightSearchRadius);
                        samples.Add(new Int4(x, y, z, 0)); // Not added
                    }
                }

                // Add the samples starting from the one that has the most
                // error. The procedure stops when all samples are added
                // or when the max error is within treshold.
                int nsamples = samples.Count;
                for (int iter = 0; iter < nsamples; ++iter)
                {
                    if (verts.Count >= MAX_VERTS)
                    {
                        break;
                    }

                    // Find sample with most error.
                    Vector3 bestpt = new Vector3();
                    float bestd = 0;
                    int besti = -1;
                    for (int i = 0; i < nsamples; ++i)
                    {
                        var s = samples[i];
                        if (s.W != 0) continue; // skip added.
                                                // The sample location is jittered to get rid of some bad triangulations
                                                // which are cause by symmetrical data from the grid structure.
                        Vector3 pt = new Vector3
                        {
                            X = s.X * sampleDist + GetJitterX(i) * cs * 0.1f,
                            Y = s.Y * chf.CH,
                            Z = s.Z * sampleDist + GetJitterY(i) * cs * 0.1f
                        };
                        float d = DistToTriMesh(pt, verts.ToArray(), tris.ToArray(), tris.Count);
                        if (d < 0) continue; // did not hit the mesh.
                        if (d > bestd)
                        {
                            bestd = d;
                            besti = i;
                            bestpt = pt;
                        }
                    }
                    // If the max error is within accepted threshold, stop tesselating.
                    if (bestd <= sampleMaxError || besti == -1)
                    {
                        break;
                    }
                    // Mark sample as added.
                    var sb = samples[besti];
                    sb.W = 1;
                    samples[besti] = sb;
                    // Add the new sample point.
                    verts.Add(bestpt);

                    // Create new triangulation.
                    // TODO: Incremental add instead of full rebuild.
                    edges.Clear();
                    tris.Clear();
                    DelaunayHull(verts.ToArray(), nhull, hull, tris, edges);
                }
            }

            int ntris = tris.Count;
            if (ntris > MAX_TRIS)
            {
                tris.RemoveRange(MAX_TRIS, ntris - MAX_TRIS);
                Console.WriteLine($"rcBuildPolyMeshDetail: Shrinking triangle count from {ntris} to max {MAX_TRIS}.");
            }

            outVerts = verts.ToArray();
            outTris = tris.ToArray();

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chf"></param>
        /// <param name="poly"></param>
        /// <param name="verts"></param>
        /// <param name="bs"></param>
        /// <param name="hp"></param>
        /// <param name="region"></param>
        /// <remarks>Reads to the compact heightfield are offset by border size (bs)
        /// since border size offset is already removed from the polymesh vertices.</remarks>
        private static void GetHeightData(CompactHeightfield chf, IndexedPolygon poly, Int3[] verts, int bs, HeightPatch hp, int region)
        {
            List<HeightDataItem> queue = new List<HeightDataItem>(512);

            // Set all heights to RC_UNSET_HEIGHT.
            hp.Data = Helper.CreateArray(hp.Bounds.Width * hp.Bounds.Height, RecastUtils.RC_UNSET_HEIGHT);

            bool empty = true;

            // We cannot sample from this poly if it was created from polys
            // of different regions. If it was then it could potentially be overlapping
            // with polys of that region and the heights sampled here could be wrong.
            if (region != RecastUtils.RC_MULTIPLE_REGS)
            {
                // Copy the height from the same region, and mark region borders
                // as seed points to fill the rest.
                for (int hy = 0; hy < hp.Bounds.Height; hy++)
                {
                    int y = hp.Bounds.Y + hy + bs;
                    for (int hx = 0; hx < hp.Bounds.Width; hx++)
                    {
                        int x = hp.Bounds.X + hx + bs;
                        var c = chf.Cells[x + y * chf.Width];
                        for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                        {
                            var s = chf.Spans[i];
                            if (s.Reg == region)
                            {
                                // Store height
                                hp.Data[hx + hy * hp.Bounds.Width] = s.Y;
                                empty = false;

                                // If any of the neighbours is not in same region,
                                // add the current location as flood fill start
                                bool border = false;
                                for (int dir = 0; dir < 4; ++dir)
                                {
                                    if (s.GetCon(dir) != CompactSpan.RC_NOT_CONNECTED)
                                    {
                                        int ax = x + RecastUtils.GetDirOffsetX(dir);
                                        int ay = y + RecastUtils.GetDirOffsetY(dir);
                                        int ai = chf.Cells[ax + ay * chf.Width].Index + s.GetCon(dir);
                                        var a = chf.Spans[ai];
                                        if (a.Reg != region)
                                        {
                                            border = true;
                                            break;
                                        }
                                    }
                                }
                                if (border)
                                {
                                    queue.Add(new HeightDataItem
                                    {
                                        X = x,
                                        Y = y,
                                        I = i
                                    });
                                }
                                break;
                            }
                        }
                    }
                }
            }

            // if the polygon does not contain any points from the current region (rare, but happens)
            // or if it could potentially be overlapping polygons of the same region,
            // then use the center as the seed point.
            if (empty)
            {
                SeedArrayWithPolyCenter(chf, poly, verts, bs, hp, queue);
            }

            int RETRACT_SIZE = 256;
            int head = 0;

            // We assume the seed is centered in the polygon, so a BFS to collect
            // height data will ensure we do not move onto overlapping polygons and
            // sample wrong heights.
            while (head < queue.Count)
            {
                var c = queue[head];
                head++;
                if (head >= RETRACT_SIZE)
                {
                    head = 0;
                    if (queue.Count > RETRACT_SIZE)
                    {
                        queue.RemoveRange(0, RETRACT_SIZE);
                    }
                    queue.Clear();
                }

                var cs = chf.Spans[c.I];
                for (int dir = 0; dir < 4; dir++)
                {
                    if (cs.GetCon(dir) == CompactSpan.RC_NOT_CONNECTED)
                    {
                        continue;
                    }

                    int ax = c.X + RecastUtils.GetDirOffsetX(dir);
                    int ay = c.Y + RecastUtils.GetDirOffsetY(dir);
                    int hx = ax - hp.Bounds.X - bs;
                    int hy = ay - hp.Bounds.Y - bs;

                    if (hx < 0 || hy < 0 || hx >= hp.Bounds.Width || hy >= hp.Bounds.Height)
                    {
                        continue;
                    }

                    if (hp.Data[hx + hy * hp.Bounds.Width] != RecastUtils.RC_UNSET_HEIGHT)
                    {
                        continue;
                    }

                    int ai = chf.Cells[ax + ay * chf.Width].Index + cs.GetCon(dir);
                    var a = chf.Spans[ai];

                    hp.Data[hx + hy * hp.Bounds.Width] = a.Y;

                    queue.Add(new HeightDataItem { X = ax, Y = ay, I = ai });
                }
            }
        }
        private static void SeedArrayWithPolyCenter(CompactHeightfield chf, IndexedPolygon poly, Int3[] verts, int bs, HeightPatch hp, List<HeightDataItem> array)
        {
            // Note: Reads to the compact heightfield are offset by border size (bs)
            // since border size offset is already removed from the polymesh vertices.

            int[] offset =
            {
                0,0, -1,-1, 0,-1, 1,-1, 1,0, 1,1, 0,1, -1,1, -1,0,
            };

            var polyIndices = poly.GetVertices();
            int npoly = poly.CountPolyVerts();

            // Find cell closest to a poly vertex
            int startCellX = 0, startCellY = 0, startSpanIndex = -1;
            int dmin = RecastUtils.RC_UNSET_HEIGHT;
            for (int j = 0; j < npoly && dmin > 0; ++j)
            {
                for (int k = 0; k < 9 && dmin > 0; ++k)
                {
                    int ax = verts[polyIndices[j]].X + offset[k * 2 + 0];
                    int ay = verts[polyIndices[j]].Y;
                    int az = verts[polyIndices[j]].Z + offset[k * 2 + 1];
                    if (ax < hp.Bounds.X || ax >= hp.Bounds.X + hp.Bounds.Width ||
                        az < hp.Bounds.Y || az >= hp.Bounds.Y + hp.Bounds.Height)
                    {
                        continue;
                    }

                    var c = chf.Cells[(ax + bs) + (az + bs) * chf.Width];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni && dmin > 0; ++i)
                    {
                        var s = chf.Spans[i];
                        int d = Math.Abs(ay - s.Y);
                        if (d < dmin)
                        {
                            startCellX = ax;
                            startCellY = az;
                            startSpanIndex = i;
                            dmin = d;
                        }
                    }
                }
            }

            // Find center of the polygon
            var pCenter = poly.GetCenter(verts);

            // Use seeds array as a stack for DFS
            array.Clear();
            array.Add(new HeightDataItem
            {
                X = startCellX,
                Y = startCellY,
                I = startSpanIndex
            });

            int[] dirs = { 0, 1, 2, 3 };
            hp.Data = Helper.CreateArray(hp.Bounds.Width * hp.Bounds.Height, RecastUtils.RC_UNSET_HEIGHT);

            // DFS to move to the center. Note that we need a DFS here and can not just move
            // directly towards the center without recording intermediate nodes, even though the polygons
            // are convex. In very rare we can get stuck due to contour simplification if we do not
            // record nodes.
            HeightDataItem hdItem = new HeightDataItem();
            while (true)
            {
                if (array.Count < 1)
                {
                    Console.WriteLine("Walk towards polygon center failed to reach center");
                    break;
                }

                hdItem = array.Pop();

                if (hdItem.X == pCenter.X && hdItem.Y == pCenter.Y)
                {
                    break;
                }

                // If we are already at the correct X-position, prefer direction
                // directly towards the center in the Y-axis; otherwise prefer
                // direction in the X-axis
                int directDir;
                if (hdItem.X == pCenter.X)
                {
                    directDir = GetDirForOffset(0, pCenter.Y > hdItem.Y ? 1 : -1);
                }
                else
                {
                    directDir = GetDirForOffset(pCenter.X > hdItem.X ? 1 : -1, 0);
                }

                // Push the direct dir last so we start with this on next iteration
                Helper.Swap(ref dirs[directDir], ref dirs[3]);

                var cs = chf.Spans[hdItem.I];
                for (int i = 0; i < 4; i++)
                {
                    int dir = dirs[i];
                    if (cs.GetCon(dir) == CompactSpan.RC_NOT_CONNECTED)
                    {
                        continue;
                    }

                    int newX = hdItem.X + RecastUtils.GetDirOffsetX(dir);
                    int newY = hdItem.Y + RecastUtils.GetDirOffsetY(dir);

                    int hpx = newX - hp.Bounds.X;
                    int hpy = newY - hp.Bounds.Y;
                    if (hpx < 0 || hpx >= hp.Bounds.Width || hpy < 0 || hpy >= hp.Bounds.Height)
                    {
                        continue;
                    }

                    if (hp.Data[hpx + hpy * hp.Bounds.Width] != 0)
                    {
                        continue;
                    }

                    hp.Data[hpx + hpy * hp.Bounds.Width] = 1;
                    int index = chf.Cells[(newX + bs) + (newY + bs) * chf.Width].Index + cs.GetCon(dir);
                    array.Add(new HeightDataItem
                    {
                        X = newX,
                        Y = newY,
                        I = index
                    });
                }

                Helper.Swap(ref dirs[directDir], ref dirs[3]);
            }
            array.Clear();
            // getHeightData seeds are given in coordinates with borders
            array.Add(new HeightDataItem
            {
                X = hdItem.X + bs,
                Y = hdItem.Y + bs,
                I = hdItem.I,
            });

            hp.Data = Helper.CreateArray(hp.Bounds.Width * hp.Bounds.Height, RecastUtils.RC_UNSET_HEIGHT);
            var chs = chf.Spans[hdItem.I];
            hp.Data[hdItem.X - hp.Bounds.X + (hdItem.Y - hp.Bounds.Y) * hp.Bounds.Width] = chs.Y;
        }
        private static float GetJitterX(int i)
        {
            return (((i * 0x8da6b343) & 0xffff) / 65535.0f * 2.0f) - 1.0f;
        }
        private static float GetJitterY(int i)
        {
            return (((i * 0xd8163841) & 0xffff) / 65535.0f * 2.0f) - 1.0f;
        }
        private static float PolyMinExtent(Vector3[] verts)
        {
            float minDist = float.MaxValue;
            for (int i = 0; i < verts.Length; i++)
            {
                int ni = (i + 1) % verts.Length;
                Vector3 p1 = verts[i];
                Vector3 p2 = verts[ni];
                float maxEdgeDist = 0;
                for (int j = 0; j < verts.Length; j++)
                {
                    if (j == i || j == ni) continue;
                    float d = RecastUtils.DistancePtSeg2d(verts[j], p1, p2);
                    maxEdgeDist = Math.Max(maxEdgeDist, d);
                }
                minDist = Math.Min(minDist, maxEdgeDist);
            }
            return (float)Math.Sqrt(minDist);
        }
        private static void TriangulateHull(Vector3[] verts, int nhull, int[] hull, int nin, List<Int3> tris)
        {
            int start = 0, left = 1, right = nhull - 1;

            // Start from an ear with shortest perimeter.
            // This tends to favor well formed triangles as starting point.
            float dmin = float.MaxValue;
            for (int i = 0; i < nhull; i++)
            {
                if (hull[i] >= nin) continue; // Ears are triangles with original vertices as middle vertex while others are actually line segments on edges
                int pi = RecastUtils.Prev(i, nhull);
                int ni = RecastUtils.Next(i, nhull);
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
            tris.Add(new Int3()
            {
                X = hull[start],
                Y = hull[left],
                Z = hull[right],
            });

            // Triangulate the polygon by moving left or right,
            // depending on which triangle has shorter perimeter.
            // This heuristic was chose emprically, since it seems
            // handle tesselated straight edges well.
            while (RecastUtils.Next(left, nhull) != right)
            {
                // Check to see if se should advance left or right.
                int nleft = RecastUtils.Next(left, nhull);
                int nright = RecastUtils.Prev(right, nhull);

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
                    tris.Add(new Int3()
                    {
                        X = hull[left],
                        Y = hull[nleft],
                        Z = hull[right],
                    });

                    left = nleft;
                }
                else
                {
                    tris.Add(new Int3()
                    {
                        X = hull[left],
                        Y = hull[nright],
                        Z = hull[right],
                    });

                    right = nright;
                }
            }
        }
        private static void DelaunayHull(Vector3[] pts, int nhull, int[] hull, List<Int3> outTris, List<Int4> outEdges)
        {
            int npts = pts.Length;
            int nfaces = 0;
            int nedges = 0;
            int maxEdges = npts * 10;
            Int4[] edges = new Int4[maxEdges];

            for (int i = 0, j = nhull - 1; i < nhull; j = i++)
            {
                nedges = AddEdge(edges, nedges, maxEdges, hull[j], hull[i], (int)EdgeValues.Hull, (int)EdgeValues.Undefined);
            }

            int currentEdge = 0;
            while (currentEdge < nedges)
            {
                if (edges[currentEdge][2] == (int)EdgeValues.Undefined)
                {
                    CompleteFacet(pts, npts, ref edges, ref nedges, maxEdges, ref nfaces, currentEdge);
                }
                if (edges[currentEdge][3] == (int)EdgeValues.Undefined)
                {
                    CompleteFacet(pts, npts, ref edges, ref nedges, maxEdges, ref nfaces, currentEdge);
                }
                currentEdge++;
            }

            // Create tris
            var tris = CreateTrisFromEdges(nfaces, edges, nedges);

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

            outTris.AddRange(tris);
            outEdges.AddRange(edges);
        }
        private static Int3[] CreateTrisFromEdges(int nfaces, Int4[] edges, int nedges)
        {
            Int3[] tris = Helper.CreateArray(nfaces, new Int3(-1, -1, -1));

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

            return tris;
        }
        private static int AddEdge(Int4[] edges, int nedges, int maxEdges, int s, int t, int l, int r)
        {
            if (nedges >= maxEdges)
            {
                Console.WriteLine($"addEdge: Too many edges ({nedges}/{maxEdges}).");

                return nedges;
            }

            // Add edge if not already in the triangulation.
            int e = FindEdge(edges, nedges, s, t);
            if (e == (int)EdgeValues.Undefined)
            {
                edges[nedges++] = new Int4(s, t, l, r);
            }

            return nedges;
        }
        private static int FindEdge(Int4[] edges, int nedges, int s, int t)
        {
            for (int i = 0; i < nedges; i++)
            {
                var e = edges[i];
                if ((e.X == s && e.Y == t) || (e.X == t && e.Y == s))
                {
                    return i;
                }
            }

            return (int)EdgeValues.Undefined;
        }
        private static void CompleteFacet(Vector3[] pts, int npts, ref Int4[] edges, ref int nedges, int maxEdges, ref int nfaces, int e)
        {
            var edge = edges[e];

            // Cache s and t.
            int s, t;
            if (edge[2] == (int)EdgeValues.Undefined)
            {
                s = edge[0];
                t = edge[1];
            }
            else if (edge[3] == (int)EdgeValues.Undefined)
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
            int pt = FindBestPointOnLeft(pts, npts, s, t, edges, nedges);

            // Add new triangle or update edge info if s-t is on hull.
            if (pt < npts)
            {
                // Update face information of edge being completed.
                UpdateLeftFace(ref edges[e], s, t, nfaces);

                // Add new edge or update face info of old edge.
                e = FindEdge(edges, nedges, pt, s);
                if (e == (int)EdgeValues.Undefined)
                {
                    nedges = AddEdge(edges, nedges, maxEdges, pt, s, nfaces, (int)EdgeValues.Undefined);
                }
                else
                {
                    UpdateLeftFace(ref edges[e], pt, s, nfaces);
                }

                // Add new edge or update face info of old edge.
                e = FindEdge(edges, nedges, t, pt);
                if (e == (int)EdgeValues.Undefined)
                {
                    nedges = AddEdge(edges, nedges, maxEdges, t, pt, nfaces, (int)EdgeValues.Undefined);
                }
                else
                {
                    UpdateLeftFace(ref edges[e], t, pt, nfaces);
                }

                nfaces++;
            }
            else
            {
                UpdateLeftFace(ref edges[e], s, t, (int)EdgeValues.Hull);
            }
        }
        private static int FindBestPointOnLeft(Vector3[] pts, int npts, int s, int t, Int4[] edges, int nedges)
        {
            // Find best point on left of edge.
            int pt = npts;
            Vector3 c = new Vector3();
            float r = -1;
            for (int u = 0; u < npts; u++)
            {
                if (u == s || u == t)
                {
                    continue;
                }

                if (RecastUtils.VCross2(pts[s], pts[t], pts[u]) <= float.Epsilon)
                {
                    continue;
                }

                if (r < 0)
                {
                    // The circle is not updated yet, do it now.
                    pt = u;
                    CircumCircle(pts[s], pts[t], pts[u], out c, out r);
                    continue;
                }

                float d = RecastUtils.VDist2(c, pts[u]);
                float tol = 0.001f;
                if (d > r * (1 + tol))
                {
                    // Outside current circumcircle, skip.
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

            return pt;
        }
        private static void UpdateLeftFace(ref Int4 e, int s, int t, int f)
        {
            if (e[0] == s && e[1] == t && e[2] == (int)EdgeValues.Undefined)
            {
                e[2] = f;
            }
            else if (e[1] == s && e[0] == t && e[3] == (int)EdgeValues.Undefined)
            {
                e[3] = f;
            }
        }
        private static bool OverlapEdges(Vector3[] pts, Int4[] edges, int nedges, int s1, int t1)
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
        private static int OverlapSegSeg2d(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            float a1 = RecastUtils.VCross2(a, b, d);
            float a2 = RecastUtils.VCross2(a, b, c);
            if (a1 * a2 < 0.0f)
            {
                float a3 = RecastUtils.VCross2(c, d, a);
                float a4 = a3 + a2 - a1;
                if (a3 * a4 < 0.0f)
                {
                    return 1;
                }
            }
            return 0;
        }
        private static void CircumCircle(Vector3 p1, Vector3 p2, Vector3 p3, out Vector3 c, out float r)
        {
            float EPS = 1e-6f;

            // Calculate the circle relative to p1, to avoid some precision issues.
            Vector3 v1 = new Vector3();
            Vector3 v2 = Vector3.Subtract(p2, p1);
            Vector3 v3 = Vector3.Subtract(p3, p1);

            c = new Vector3();
            float cp = RecastUtils.VCross2(v1, v2, v3);
            if (Math.Abs(cp) > EPS)
            {
                float v1Sq = RecastUtils.VDot2(v1, v1);
                float v2Sq = RecastUtils.VDot2(v2, v2);
                float v3Sq = RecastUtils.VDot2(v3, v3);
                c[0] = (v1Sq * (v2[2] - v3[2]) + v2Sq * (v3[2] - v1[2]) + v3Sq * (v1[2] - v2[2])) / (2 * cp);
                c[1] = 0;
                c[2] = (v1Sq * (v3[0] - v2[0]) + v2Sq * (v1[0] - v3[0]) + v3Sq * (v2[0] - v1[0])) / (2 * cp);
                r = RecastUtils.VDist2(c, v1);
                c = Vector3.Add(c, p1);
                return;
            }

            c = p1;
            r = 0;
        }
        private static float DistToTriMesh(Vector3 p, Vector3[] verts, Int3[] triPoints, int ntris)
        {
            float dmin = float.MaxValue;
            for (int i = 0; i < ntris; ++i)
            {
                var va = verts[triPoints[i].X];
                var vb = verts[triPoints[i].Y];
                var vc = verts[triPoints[i].Z];
                float d = RecastUtils.DistPtTri(p, va, vb, vc);
                if (d < dmin)
                {
                    dmin = d;
                }
            }
            if (dmin == float.MaxValue) return -1;
            return dmin;
        }
        private static float DistToPoly(int nvert, Vector3[] verts, Vector3 p)
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
                dmin = Math.Min(dmin, RecastUtils.DistancePtSeg2d(p, vj, vi));
            }
            return c ? -dmin : dmin;
        }
        private static int GetDirForOffset(int x, int y)
        {
            int[] dirs = { 3, 0, -1, 2, 1 };
            return dirs[((y + 1) << 1) + x];
        }

        public static bool MergePolyMeshDetails(PolyMeshDetail[] meshes, int nmeshes, out PolyMeshDetail mesh)
        {
            mesh = new PolyMeshDetail();

            int maxVerts = 0;
            int maxTris = 0;
            int maxMeshes = 0;

            for (int i = 0; i < nmeshes; ++i)
            {
                if (meshes[i] == null)
                {
                    continue;
                }
                maxVerts += meshes[i].Verts.Count;
                maxTris += meshes[i].Tris.Count;
                maxMeshes += meshes[i].Meshes.Count;
            }

            // Merge datas.
            for (int i = 0; i < nmeshes; ++i)
            {
                var dm = meshes[i];
                if (dm == null)
                {
                    continue;
                }

                foreach (var src in dm.Meshes)
                {
                    var dst = new PolyMeshDetailIndices
                    {
                        VertBase = mesh.Verts.Count + src.VertBase,
                        VertCount = src.VertCount,
                        TriBase = mesh.Tris.Count + src.TriBase,
                        TriCount = src.TriCount,
                    };

                    mesh.Meshes.Add(dst);
                }

                mesh.Verts.AddRange(dm.Verts);

                mesh.Tris.AddRange(dm.Tris);
            }

            return true;
        }

        /// <summary>
        /// The sub-mesh data.
        /// </summary>
        public List<PolyMeshDetailIndices> Meshes { get; set; } = new List<PolyMeshDetailIndices>();
        /// <summary>
        /// The mesh vertices.
        /// </summary>
        public List<Vector3> Verts { get; set; } = new List<Vector3>();
        /// <summary>
        /// The mesh triangles.
        /// </summary>
        public List<PolyMeshTriangleIndices> Tris { get; set; } = new List<PolyMeshTriangleIndices>();


        private void StoreData(PolyMesh mesh, CompactHeightfield chf, float sampleDist, float sampleMaxError)
        {
            int nvp = mesh.NVP;
            float cs = mesh.CS;
            float ch = mesh.CH;
            Vector3 orig = mesh.BMin;
            int borderSize = mesh.BorderSize;
            int heightSearchRadius = Math.Max(1, (int)Math.Ceiling(mesh.MaxEdgeError));

            // Find max size for a polygon area.
            var bounds = GetBounds(mesh, chf, out int maxhw, out int maxhh);

            HeightPatch hp = new HeightPatch
            {
                Data = Helper.CreateArray(maxhw * maxhh, RecastUtils.RC_UNSET_HEIGHT)
            };

            List<Vector3> poly = new List<Vector3>(nvp);

            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];

                // Store polygon vertices for processing.
                poly.Clear();
                for (int j = 0; j < nvp; ++j)
                {
                    if (p[j] == RecastUtils.RC_MESH_NULL_IDX) break;
                    var v = mesh.Verts[p[j]];
                    var pv = new Vector3(v.X * cs, v.Y * ch, v.Z * cs);
                    poly.Add(pv);
                }

                // Get the height data from the area of the polygon.
                hp.Bounds = bounds[i];

                GetHeightData(chf, p, mesh.Verts, borderSize, hp, mesh.Regs[i]);

                // Build detail mesh.
                BuildPolyDetailParams param = new BuildPolyDetailParams
                {
                    SampleDist = sampleDist,
                    SampleMaxError = sampleMaxError,
                    HeightSearchRadius = heightSearchRadius,
                };
                if (!BuildPolyDetail(
                    poly.ToArray(),
                    param, chf, hp,
                    out var verts, out var tris))
                {
                    return;
                }

                // Move detail verts to world space.
                for (int j = 0; j < verts.Length; ++j)
                {
                    verts[j].X += orig.X;
                    verts[j].Y += orig.Y + chf.CH; // Is this offset necessary?
                    verts[j].Z += orig.Z;
                }
                // Offset poly too, will be used to flag checking.
                for (int j = 0; j < poly.Count; ++j)
                {
                    poly[j] += orig;
                }

                // Store detail submesh.
                PolyMeshDetailIndices tmp = new PolyMeshDetailIndices
                {
                    VertBase = this.Verts.Count,
                    VertCount = verts.Length,
                    TriBase = this.Tris.Count,
                    TriCount = tris.Length,
                };
                this.Meshes.Add(tmp);

                this.Verts.AddRange(verts);

                // Store triangles
                foreach (var t in tris)
                {
                    this.Tris.Add(new PolyMeshTriangleIndices
                    {
                        Point1 = t.X,
                        Point2 = t.Y,
                        Point3 = t.Z,
                        Flags = GetTriFlags(verts[t.X], verts[t.Y], verts[t.Z], poly),
                    });
                }
            }
        }
    }
}
