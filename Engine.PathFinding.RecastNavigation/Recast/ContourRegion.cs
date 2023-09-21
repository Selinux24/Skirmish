using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Contour region
    /// </summary>
    public class ContourRegion
    {
        /// <summary>
        /// Gets whether the specified segments intersects
        /// </summary>
        /// <param name="d0">First segment</param>
        /// <param name="d1">Second segment</param>
        /// <param name="i">Incident vertex index</param>
        /// <param name="verts">Vertex list</param>
        /// <param name="n">Number of vertices</param>
        /// <returns></returns>
        private static bool IntersectSegCountour(Int4 d0, Int4 d1, int i, IEnumerable<Int4> verts, int n)
        {
            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = Utils.Next(k, n);
                // Skip edges incident to i.
                if (i == k || i == k1)
                {
                    continue;
                }
                var p0 = verts.ElementAt(k);
                var p1 = verts.ElementAt(k1);
                if (d0 == p0 || d1 == p0 || d0 == p1 || d1 == p1)
                {
                    continue;
                }

                if (TriangulationHelper.Intersect(d0, d1, p0, p1))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Contour outline
        /// </summary>
        public Contour Outline { get; set; }
        /// <summary>
        /// Hole list
        /// </summary>
        public ContourHole[] Holes { get; set; }
        /// <summary>
        /// Number of holes
        /// </summary>
        public int NHoles { get; set; }

        /// <summary>
        /// Merges the region holes
        /// </summary>
        public void MergeRegionHoles()
        {
            // Sort holes from left to right.
            SortHoles();

            // Merge holes into the outline one by one.
            for (int i = 0; i < NHoles; i++)
            {
                var hole = Holes[i];

                var (BestVertex, BestIndex) = FindBestVertex(hole, Outline, i);

                if (BestIndex == -1)
                {
                    Logger.WriteWarning(this, $"Failed to find merge points for {Outline} and {hole.Contour}.");
                }
                else
                {
                    Contour.Merge(Outline, hole.Contour, BestIndex, BestVertex);
                }
            }
        }
        private void SortHoles()
        {
            for (int i = 0; i < NHoles; i++)
            {
                Holes[i].Contour.FindLeftMostVertex(out var minx, out var minz, out var leftmost);
                Holes[i].MinX = minx;
                Holes[i].MinZ = minz;
                Holes[i].Leftmost = leftmost;
            }

            Array.Sort(Holes, ContourHole.DefaultComparer);
        }
        private (int BestVertex, int BestIndex) FindBestVertex(ContourHole hole, Contour outline, int i)
        {
            int index = -1;
            int bestVertex = hole.Leftmost;

            for (int iter = 0; iter < hole.Contour.NVertices; iter++)
            {
                // Find potential diagonals.
                // The 'best' vertex must be in the cone described by 3 cosequtive vertices of the outline.
                // ..o j-1
                //   |
                //   |   * best
                //   |
                // j o-----o j+1
                //         :
                var corner = hole.Contour.Vertices[bestVertex];
                var diags = FindPotentialDiagonals(corner, outline);

                // Find a diagonal that is not intersecting the outline not the remaining holes.
                int bestIndex = FindBestIndex(i, corner, outline, diags);

                // If found non-intersecting diagonal, stop looking.
                if (bestIndex != -1)
                {
                    index = bestIndex;
                    break;
                }

                // All the potential diagonals for the current vertex were intersecting, try next vertex.
                bestVertex = (bestVertex + 1) % hole.Contour.NVertices;
            }

            return (bestVertex, index);
        }
        private static IEnumerable<PotentialDiagonal> FindPotentialDiagonals(Int4 corner, Contour outline)
        {
            var diags = new List<PotentialDiagonal>();

            for (int j = 0; j < outline.NVertices; j++)
            {
                if (TriangulationHelper.InCone(j, outline.NVertices, outline.Vertices, corner))
                {
                    int dx = outline.Vertices[j].X - corner.X;
                    int dz = outline.Vertices[j].Z - corner.Z;
                    var pd = new PotentialDiagonal { Vert = j, Dist = dx * dx + dz * dz };
                    diags.Add(pd);
                }
            }

            // Sort potential diagonals by distance, we want to make the connection as short as possible.
            diags.Sort(PotentialDiagonal.DefaultComparer);

            return diags;
        }
        private int FindBestIndex(int i, Int4 corner, Contour outline, IEnumerable<PotentialDiagonal> diags)
        {
            int bestIndex = -1;
            for (int j = 0; j < diags.Count(); j++)
            {
                var pt = outline.Vertices[diags.ElementAt(j).Vert];
                bool intersect = IntersectSegCountour(pt, corner, diags.ElementAt(i).Vert, outline.Vertices, outline.NVertices);
                for (int k = i; k < NHoles && !intersect; k++)
                {
                    intersect |= IntersectSegCountour(pt, corner, -1, Holes[k].Contour.Vertices, Holes[k].Contour.NVertices);
                }
                if (!intersect)
                {
                    bestIndex = diags.ElementAt(j).Vert;
                    break;
                }
            }

            return bestIndex;
        }
    }
}
