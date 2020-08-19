using SharpDX;
using System;

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
        private static bool IntersectSegCountour(Int4 d0, Int4 d1, int i, Int4[] verts, int n)
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
            for (int i = 0; i < NHoles; i++)
            {
                Holes[i].Contour.FindLeftMostVertex(out var minx, out var minz, out var leftmost);
                Holes[i].MinX = minx;
                Holes[i].MinZ = minz;
                Holes[i].Leftmost = leftmost;
            }

            Array.Sort(Holes, ContourHole.DefaultComparer);

            int maxVerts = Outline.NVertices;
            for (int i = 0; i < NHoles; i++)
            {
                maxVerts += Holes[i].Contour.NVertices;
            }

            PotentialDiagonal[] diags = Helper.CreateArray(maxVerts, new PotentialDiagonal()
            {
                Dist = int.MinValue,
                Vert = int.MinValue,
            });

            var outline = Outline;

            // Merge holes into the outline one by one.
            for (int i = 0; i < NHoles; i++)
            {
                var hole = Holes[i].Contour;

                int index = -1;
                int bestVertex = Holes[i].Leftmost;
                for (int iter = 0; iter < hole.NVertices; iter++)
                {
                    // Find potential diagonals.
                    // The 'best' vertex must be in the cone described by 3 cosequtive vertices of the outline.
                    // ..o j-1
                    //   |
                    //   |   * best
                    //   |
                    // j o-----o j+1
                    //         :
                    int ndiags = 0;
                    var corner = hole.Vertices[bestVertex];
                    for (int j = 0; j < outline.NVertices; j++)
                    {
                        if (RecastUtils.InCone(j, outline.NVertices, outline.Vertices, corner))
                        {
                            int dx = outline.Vertices[j].X - corner.X;
                            int dz = outline.Vertices[j].Z - corner.Z;
                            diags[ndiags].Vert = j;
                            diags[ndiags].Dist = dx * dx + dz * dz;
                            ndiags++;
                        }
                    }
                    // Sort potential diagonals by distance, we want to make the connection as short as possible.
                    Array.Sort(diags, 0, ndiags, PotentialDiagonal.DefaultComparer);

                    // Find a diagonal that is not intersecting the outline not the remaining holes.
                    index = -1;
                    for (int j = 0; j < ndiags; j++)
                    {
                        var pt = outline.Vertices[diags[j].Vert];
                        bool intersect = IntersectSegCountour(pt, corner, diags[i].Vert, outline.Vertices, outline.NVertices);
                        for (int k = i; k < NHoles && !intersect; k++)
                        {
                            intersect |= IntersectSegCountour(pt, corner, -1, Holes[k].Contour.Vertices, Holes[k].Contour.NVertices);
                        }
                        if (!intersect)
                        {
                            index = diags[j].Vert;
                            break;
                        }
                    }
                    // If found non-intersecting diagonal, stop looking.
                    if (index != -1)
                    {
                        break;
                    }
                    // All the potential diagonals for the current vertex were intersecting, try next vertex.
                    bestVertex = (bestVertex + 1) % hole.NVertices;
                }

                if (index == -1)
                {
                    Logger.WriteWarning($"Failed to find merge points for {Outline} and {hole}.");
                }
                else
                {
                    Contour.Merge(Outline, hole, index, bestVertex);
                }
            }
        }
    }
}
