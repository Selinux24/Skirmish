using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Contour region
    /// </summary>
    public class ContourRegion
    {
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
        /// Gets whether the specified segments intersects
        /// </summary>
        /// <param name="d0">First segment</param>
        /// <param name="d1">Second segment</param>
        /// <param name="i">Incident vertex index</param>
        /// <param name="verts">Vertex list</param>
        /// <param name="n">Number of vertices</param>
        /// <returns></returns>
        private static bool IntersectSegCountour(ContourVertex d0, ContourVertex d1, int i, ContourVertex[] verts, int n)
        {
            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = ArrayUtils.Next(k, n);

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

                if (TriangulationHelper.Intersect2D(d0.Coords, d1.Coords, p0.Coords, p1.Coords))
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Find potential diagonals
        /// </summary>
        /// <param name="corner">Corner</param>
        /// <param name="outline">Contour</param>
        /// <returns>Returns a list of potential diagonals</returns>
        private static PotentialDiagonal[] FindPotentialDiagonals(ContourVertex corner, Contour outline)
        {
            var diags = new List<PotentialDiagonal>();

            var verts = ContourVertex.ToInt3List(outline.Vertices);
            int n = outline.NVertices;

            for (int i = 0; i < n; i++)
            {
                var a = verts[i];
                var b = verts[ArrayUtils.Next(i, n)];
                var c = verts[ArrayUtils.Prev(i, n)];

                if (TriangulationHelper.InCone2D(a, b, c, corner.Coords))
                {
                    int dx = verts[i].X - corner.X;
                    int dz = verts[i].Z - corner.Z;
                    diags.Add(new() { Vert = i, Dist = dx * dx + dz * dz });
                }
            }

            // Sort potential diagonals by distance, we want to make the connection as short as possible.
            diags.Sort(PotentialDiagonal.DefaultComparer);

            return diags.ToArray();
        }

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
        /// <summary>
        /// Sort holes
        /// </summary>
        private void SortHoles()
        {
            for (int i = 0; i < NHoles; i++)
            {
                Holes[i].Contour.FindLeftMostVertex(out var minx, out var minz, out var leftmost);
                Holes[i].MinX = minx;
                Holes[i].MinZ = minz;
                Holes[i].Leftmost = leftmost;
            }

            Array.Sort(Holes, ContourHole.Comparer);
        }
        /// <summary>
        /// Find best vertex
        /// </summary>
        /// <param name="hole">Contour hole</param>
        /// <param name="outline">Contour</param>
        /// <param name="i">Index</param>
        /// <returns>Returns the bests vertices and indexes</returns>
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
        /// <summary>
        /// Find best index
        /// </summary>
        /// <param name="i">Index</param>
        /// <param name="corner">Corner</param>
        /// <param name="outline">Contour</param>
        /// <param name="diags">List of potential diagonals</param>
        private int FindBestIndex(int i, ContourVertex corner, Contour outline, PotentialDiagonal[] diags)
        {
            int bestIndex = -1;

            for (int j = 0; j < diags.Length; j++)
            {
                var pt = outline.Vertices[diags[j].Vert];

                bool intersect = IntersectSegCountour(pt, corner, diags[i].Vert, outline.Vertices, outline.NVertices);
                for (int k = i; k < NHoles && !intersect; k++)
                {
                    intersect |= IntersectSegCountour(pt, corner, -1, Holes[k].Contour.Vertices, Holes[k].Contour.NVertices);
                }

                if (!intersect)
                {
                    bestIndex = diags[j].Vert;
                    break;
                }
            }

            return bestIndex;
        }
    }
}
