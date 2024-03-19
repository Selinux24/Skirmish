using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Temporal contour helper class
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="verts">Polygon vertices</param>
    /// <param name="cverts">Number of vertices</param>
    /// <param name="poly">Indexed polygon definition</param>
    public class TempContour(VertexWithNeigbour[] verts, int cverts, IndexedPolygon poly)
    {
        /// <summary>
        /// Vertices buffer
        /// </summary>
        private readonly VertexWithNeigbour[] verts = verts;
        /// <summary>
        /// Number of vertices in the buffer
        /// </summary>
        private int nverts = 0;
        /// <summary>
        /// Contour vertices
        /// </summary>
        private readonly int cverts = cverts;
        /// <summary>
        /// Indexed polygon
        /// </summary>
        private readonly IndexedPolygon poly = poly;
        /// <summary>
        /// Number of vertices in the polygon
        /// </summary>
        private int npoly = 0;

        /// <summary>
        /// Appends a vertex to the contour
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        /// <param name="z">Z value</param>
        /// <param name="r">Neighbour reference</param>
        public bool AppendVertex(VertexWithNeigbour v)
        {
            // Try to merge with existing segments.
            if (nverts > 1 && MergeVertex(v))
            {
                return true;
            }

            if (nverts + 1 > cverts)
            {
                // Limit reached
                return false;
            }

            // Add new point.
            verts[nverts++] = v;

            return true;
        }
        /// <summary>
        /// Try to merge with existing segments.
        /// </summary>
        private bool MergeVertex(VertexWithNeigbour v)
        {
            var pa = verts[nverts - 2];
            var pb = verts[nverts - 1];
            if (pb.Nei == v.Nei)
            {
                if (pa.X == pb.X && pb.X == v.X)
                {
                    // The verts are aligned aling x-axis, update z.
                    pb.Y = v.Y;
                    pb.Z = v.Z;
                    verts[nverts - 1] = pb;
                    return true;
                }
                else if (pa.Z == pb.Z && pb.Z == v.Z)
                {
                    // The verts are aligned aling z-axis, update x.
                    pb.X = v.X;
                    pb.Y = v.Y;
                    verts[nverts - 1] = pb;
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Simplifies the contour
        /// </summary>
        /// <param name="maxError">Max error value</param>
        public VertexWithNeigbour[] SimplifyContour(float maxError)
        {
            CheckWallSegment();

            if (npoly < 2)
            {
                // If there is no transitions at all,
                // create some initial points for the simplification process. 
                // Find lower-left and upper-right vertices of the contour.
                AddInitialPointsToPolygon();
            }

            // Add points until all raw points are within error tolerance to the simplified shape.
            FillPoints(maxError);

            // Remap vertices
            RemapVertices();

            // Return simplified vertices
            return verts.Take(nverts).ToArray();
        }
        /// <summary>
        /// Check for start of a wall segment
        /// </summary>
        private void CheckWallSegment()
        {
            npoly = 0;

            for (int i = 0; i < nverts; ++i)
            {
                int j = (i + 1) % nverts;

                // Check for start of a wall segment.
                int ra = verts[j].Nei;
                int rb = verts[i].Nei;
                if (ra != rb)
                {
                    poly.SetVertex(npoly++, i);
                }
            }
        }
        /// <summary>
        /// Creates some initial points for the simplification process
        /// </summary>
        /// <remarks>
        /// Find lower-left and upper-right vertices of the contour.
        /// </remarks>
        private void AddInitialPointsToPolygon()
        {
            int llx = verts[0].X;
            int llz = verts[0].Z;
            int lli = 0;
            int urx = verts[0].X;
            int urz = verts[0].Z;
            int uri = 0;
            for (int i = 1; i < nverts; ++i)
            {
                int x = verts[i].X;
                int z = verts[i].Z;
                if (x < llx || (x == llx && z < llz))
                {
                    llx = x;
                    llz = z;
                    lli = i;
                }
                if (x > urx || (x == urx && z > urz))
                {
                    urx = x;
                    urz = z;
                    uri = i;
                }
            }
            npoly = 0;
            poly.SetVertex(npoly++, lli);
            poly.SetVertex(npoly++, uri);
        }
        /// <summary>
        /// Add points until all raw points are within error tolerance to the simplified shape.
        /// </summary>
        /// <param name="maxError">Maximum error value</param>
        private void FillPoints(float maxError)
        {
            float maxErrorSqr = maxError * maxError;

            for (int i = 0; i < npoly;)
            {
                int ii = (i + 1) % npoly;

                // Find maximum deviation from the segment.
                var (maxi, maxd) = FindMaximumDeviation2D(i, ii);

                // If the max deviation is larger than accepted error,
                // add new point, else continue to next segment.
                if (maxi != -1 && maxd > maxErrorSqr)
                {
                    InsertPointAtPosition(i, maxi);
                }
                else
                {
                    i++;
                }
            }
        }
        /// <summary>
        /// Finds the maximum deviation of the segment
        /// </summary>
        /// <param name="a">Point A index</param>
        /// <param name="b">Point B index</param>
        private (int maxi, float maxd) FindMaximumDeviation2D(int a, int b)
        {
            int maxi = -1;
            float maxd = 0;

            int ai = poly.GetVertex(a);
            int ax = verts[ai].X;
            int az = verts[ai].Z;

            int bi = poly.GetVertex(b);
            int bx = verts[bi].X;
            int bz = verts[bi].Z;

            // Traverse the segment in lexilogical order so that the
            // max deviation is calculated similarly when traversing
            // opposite segments.
            int cinc;
            int ci;
            int endi;
            if (bx > ax || (bx == ax && bz > az))
            {
                cinc = 1;
                ci = (ai + cinc) % nverts;
                endi = bi;
            }
            else
            {
                cinc = nverts - 1;
                ci = (bi + cinc) % nverts;
                endi = ai;
            }

            // Tessellate only outer edges or edges between areas.
            while (ci != endi)
            {
                float d = Utils.DistancePtSegSqr2D(verts[ci].X, verts[ci].Z, ax, az, bx, bz);
                if (d > maxd)
                {
                    maxd = d;
                    maxi = ci;
                }
                ci = (ci + cinc) % nverts;
            }

            return (maxi, maxd);
        }
        /// <summary>
        /// Remap vertices
        /// </summary>
        private void RemapVertices()
        {
            //Look for the start index in the polygon
            int start = 0;
            for (int i = 1; i < npoly; ++i)
            {
                if (poly.GetVertex(i) < poly.GetVertex(start))
                {
                    start = i;
                }
            }

            //Remap from the start position
            nverts = 0;
            for (int i = 0; i < npoly; ++i)
            {
                int j = (start + i) % npoly;
                verts[nverts++] = new(verts[poly.GetVertex(j)]);
            }
        }
        /// <summary>
        /// Reset the contour
        /// </summary>
        public void Reset()
        {
            nverts = 0;
        }
        /// <summary>
        /// Inserts the specified index value at the position index
        /// </summary>
        /// <param name="position">Position in the vertex list</param>
        /// <param name="indexValue">Value</param>
        public void InsertPointAtPosition(int position, int indexValue)
        {
            npoly++;
            for (int j = npoly - 1; j > position; --j)
            {
                poly.SetVertex(j, poly.GetVertex(j - 1));
            }
            poly.SetVertex(position + 1, indexValue);
        }
        /// <summary>
        /// Remove last vertex
        /// </summary>
        public void RemoveLast()
        {
            var pa = verts[nverts - 1];
            var pb = verts[0];
            if (pa.X == pb.X && pa.Z == pb.Z)
            {
                nverts--;
            }
        }
    }
}
