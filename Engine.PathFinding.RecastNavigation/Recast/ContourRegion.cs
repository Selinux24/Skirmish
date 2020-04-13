using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    public class ContourRegion
    {
        private static bool IntersectSegCountour(Int4 d0, Int4 d1, int i, int n, Int4[] verts)
        {
            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = RecastUtils.Next(k, n);
                // Skip edges incident to i.
                if (i == k || i == k1)
                {
                    continue;
                }
                var p0 = verts[k];
                var p1 = verts[k1];
                if (d0 == p0 || d1 == p0 || d0 == p1 || d1 == p1)
                {
                    continue;
                }

                if (RecastUtils.Intersect(d0, d1, p0, p1))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool MergeContours(Contour ca, Contour cb, int ia, int ib)
        {
            int maxVerts = ca.NVerts + cb.NVerts + 2;
            Int4[] verts = new Int4[maxVerts];

            int nv = 0;

            // Copy contour A.
            for (int i = 0; i <= ca.NVerts; ++i)
            {
                verts[nv++] = ca.Verts[((ia + i) % ca.NVerts)];
            }

            // Copy contour B
            for (int i = 0; i <= cb.NVerts; ++i)
            {
                verts[nv++] = cb.Verts[((ib + i) % cb.NVerts)];
            }

            ca.Verts = verts;
            ca.NVerts = nv;

            cb.Verts = null;
            cb.NVerts = 0;

            return true;
        }
        private static void FindLeftMostVertex(Contour contour, out int minx, out int minz, out int leftmost)
        {
            minx = contour.Verts[0].X;
            minz = contour.Verts[0].Z;
            leftmost = 0;
            for (int i = 1; i < contour.NVerts; i++)
            {
                int x = contour.Verts[i].X;
                int z = contour.Verts[i].Z;
                if (x < minx || (x == minx && z < minz))
                {
                    minx = x;
                    minz = z;
                    leftmost = i;
                }
            }
        }

        public Contour Outline { get; set; }
        public ContourHole[] Holes { get; set; }
        public int NHoles { get; set; }

        public void MergeRegionHoles()
        {
            // Sort holes from left to right.
            for (int i = 0; i < this.NHoles; i++)
            {
                FindLeftMostVertex(this.Holes[i].Contour, out var minx, out var minz, out var leftmost);
                this.Holes[i].MinX = minx;
                this.Holes[i].MinZ = minz;
                this.Holes[i].Leftmost = leftmost;
            }

            Array.Sort(this.Holes, ContourHole.DefaultComparer);

            int maxVerts = this.Outline.NVerts;
            for (int i = 0; i < this.NHoles; i++)
            {
                maxVerts += this.Holes[i].Contour.NVerts;
            }

            var outline = this.Outline;

            // Merge holes into the outline one by one.
            for (int i = 0; i < this.NHoles; i++)
            {
                var hole = this.Holes[i].Contour;

                int index = -1;
                int bestVertex = this.Holes[i].Leftmost;
                for (int iter = 0; iter < hole.NVerts; iter++)
                {
                    // Find potential diagonals.
                    FindPotentialDiagonals(hole, outline, bestVertex, maxVerts, out var diags, out var ndiags, out var corner);

                    // Find a diagonal that is not intersecting the outline not the remaining holes.
                    index = FindMergeDiagonal(i, outline, diags, ndiags, corner);
                    if (index != -1)
                    {
                        // If found non-intersecting diagonal, stop looking.
                        break;
                    }

                    // All the potential diagonals for the current vertex were intersecting, try next vertex.
                    bestVertex = (bestVertex + 1) % hole.NVerts;
                }

                if (index == -1)
                {
                    Console.WriteLine($"Failed to find merge points for {this.Outline} and {hole}.");
                }
                else if (!MergeContours(this.Outline, hole, index, bestVertex))
                {
                    Console.WriteLine($"Failed to merge contours {this.Outline} and {hole}.");
                }
            }
        }
        private void FindPotentialDiagonals(Contour hole, Contour outline, int bestVertex, int maxVerts, out PotentialDiagonal[] diags, out int ndiags, out Int4 corner)
        {
            diags = Helper.CreateArray(maxVerts, new PotentialDiagonal()
            {
                Dist = int.MinValue,
                Vert = int.MinValue,
            });

            // The 'best' vertex must be in the cone described by 3 cosequtive vertices of the outline.
            // ..o j-1
            //   |
            //   |   * best
            //   |
            // j o-----o j+1
            //         :
            ndiags = 0;
            corner = hole.Verts[bestVertex];
            for (int j = 0; j < outline.NVerts; j++)
            {
                if (RecastUtils.InCone(j, outline.NVerts, outline.Verts, corner))
                {
                    int dx = outline.Verts[j].X - corner.X;
                    int dz = outline.Verts[j].Z - corner.Z;
                    diags[ndiags].Vert = j;
                    diags[ndiags].Dist = dx * dx + dz * dz;
                    ndiags++;
                }
            }
            // Sort potential diagonals by distance, we want to make the connection as short as possible.
            Array.Sort(diags, 0, ndiags, PotentialDiagonal.DefaultComparer);
        }
        private int FindMergeDiagonal(int i, Contour outline, PotentialDiagonal[] diags, int ndiags, Int4 corner)
        {
            int index = -1;
            for (int j = 0; j < ndiags; j++)
            {
                var pt = outline.Verts[diags[j].Vert];
                bool intersect = IntersectSegCountour(pt, corner, diags[i].Vert, outline.NVerts, outline.Verts);
                for (int k = i; k < this.NHoles && !intersect; k++)
                {
                    intersect |= IntersectSegCountour(pt, corner, -1, this.Holes[k].Contour.NVerts, this.Holes[k].Contour.Verts);
                }
                if (!intersect)
                {
                    index = diags[j].Vert;
                    break;
                }
            }

            return index;
        }
    }
}
