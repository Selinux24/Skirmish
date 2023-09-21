using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    public class TempContour
    {
        public Int4[] Verts { get; set; }
        public int Nverts { get; set; }
        public int Cverts { get; set; }
        public IndexedPolygon Poly { get; set; }
        public int Npoly { get; set; }
        public int Cpoly { get; set; }

        public TempContour(Int4[] vbuf, int nvbuf, IndexedPolygon pbuf, int npbuf)
        {
            Verts = vbuf;
            Nverts = 0;
            Cverts = nvbuf;
            Poly = pbuf;
            Npoly = 0;
            Cpoly = npbuf;
        }

        public bool AppendVertex(int x, int y, int z, int r)
        {
            // Try to merge with existing segments.
            if (Nverts > 1)
            {
                var pa = Verts[Nverts - 2];
                var pb = Verts[Nverts - 1];
                if (pb.W == r)
                {
                    if (pa.X == pb.X && pb.X == x)
                    {
                        // The verts are aligned aling x-axis, update z.
                        pb.Y = y;
                        pb.Z = z;
                        Verts[Nverts - 1] = pb;
                        return true;
                    }
                    else if (pa.Z == pb.Z && pb.Z == z)
                    {
                        // The verts are aligned aling z-axis, update x.
                        pb.X = x;
                        pb.Y = y;
                        Verts[Nverts - 1] = pb;
                        return true;
                    }
                }
            }

            // Add new point.
            if (Nverts + 1 > Cverts)
            {
                return false;
            }

            Verts[Nverts] = new Int4(x, y, z, r);
            Nverts++;

            return true;
        }
        public void SimplifyContour(float maxError)
        {
            Npoly = 0;

            for (int i = 0; i < Nverts; ++i)
            {
                int j = (i + 1) % Nverts;
                // Check for start of a wall segment.
                int ra = Verts[j].W;
                int rb = Verts[i].W;
                if (ra != rb)
                {
                    Poly[Npoly++] = i;
                }
            }
            if (Npoly < 2)
            {
                // If there is no transitions at all,
                // create some initial points for the simplification process. 
                // Find lower-left and upper-right vertices of the contour.
                int llx = Verts[0].X;
                int llz = Verts[0].Z;
                int lli = 0;
                int urx = Verts[0].X;
                int urz = Verts[0].Z;
                int uri = 0;
                for (int i = 1; i < Nverts; ++i)
                {
                    int x = Verts[i].X;
                    int z = Verts[i].Z;
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
                Npoly = 0;
                Poly[Npoly++] = lli;
                Poly[Npoly++] = uri;
            }

            // Add points until all raw points are within
            // error tolerance to the simplified shape.
            for (int i = 0; i < Npoly;)
            {
                int ii = (i + 1) % Npoly;

                int ai = Poly[i];
                int ax = Verts[ai].X;
                int az = Verts[ai].Z;

                int bi = Poly[ii];
                int bx = Verts[bi].X;
                int bz = Verts[bi].Z;

                // Find maximum deviation from the segment.
                float maxd = 0;
                int maxi = -1;
                int ci, cinc, endi;

                // Traverse the segment in lexilogical order so that the
                // max deviation is calculated similarly when traversing
                // opposite segments.
                if (bx > ax || (bx == ax && bz > az))
                {
                    cinc = 1;
                    ci = (ai + cinc) % Nverts;
                    endi = bi;
                }
                else
                {
                    cinc = Nverts - 1;
                    ci = (bi + cinc) % Nverts;
                    endi = ai;
                }

                // Tessellate only outer edges or edges between areas.
                while (ci != endi)
                {
                    float d = Utils.DistancePtSeg2D(Verts[ci].X, Verts[ci].Z, ax, az, bx, bz);
                    if (d > maxd)
                    {
                        maxd = d;
                        maxi = ci;
                    }
                    ci = (ci + cinc) % Nverts;
                }

                // If the max deviation is larger than accepted error,
                // add new point, else continue to next segment.
                if (maxi != -1 && maxd > (maxError * maxError))
                {
                    Npoly++;
                    for (int j = Npoly - 1; j > i; --j)
                    {
                        Poly[j] = Poly[j - 1];
                    }
                    Poly[i + 1] = maxi;
                }
                else
                {
                    i++;
                }
            }

            // Remap vertices
            int start = 0;
            for (int i = 1; i < Npoly; ++i)
            {
                if (Poly[i] < Poly[start])
                {
                    start = i;
                }
            }

            Nverts = 0;
            for (int i = 0; i < Npoly; ++i)
            {
                int j = (start + i) % Npoly;
                var src = Verts[Poly[j]];
                Verts[Nverts++] = new Int4()
                {
                    X = src.X,
                    Y = src.Y,
                    Z = src.Z,
                    W = src.W,
                };
            }
        }
    }
}
