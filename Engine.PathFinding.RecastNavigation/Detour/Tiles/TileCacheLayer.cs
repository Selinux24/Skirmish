using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache layer
    /// </summary>
    public struct TileCacheLayer
    {
        /// <summary>
        /// Header
        /// </summary>
        public TileCacheLayerHeader Header { get; set; }
        /// <summary>
        /// Region count.
        /// </summary>
        public int RegCount { get; set; }
        /// <summary>
        /// Height map
        /// </summary>
        public int[] Heights { get; set; }
        /// <summary>
        /// Areas
        /// </summary>
        public AreaTypes[] Areas { get; set; }
        /// <summary>
        /// Connections
        /// </summary>
        public int[] Cons { get; set; }
        /// <summary>
        /// Regions
        /// </summary>
        public int[] Regs { get; set; }

        public readonly bool IsConnected(int ia, int ib, int walkableClimb)
        {
            if (Areas[ia] != Areas[ib])
            {
                return false;
            }

            if (Math.Abs(Heights[ia] - Heights[ib]) > walkableClimb)
            {
                return false;
            }

            return true;
        }
        public readonly int GetNeighbourReg(int ax, int ay, int dir)
        {
            int w = Header.Width;
            int ia = ax + ay * w;

            int con = IndexedPolygon.GetVertexDirection(Cons[ia]);
            int portal = Cons[ia] >> 4;
            int mask = 1 << dir;

            if ((con & mask) == 0)
            {
                // No connection, return portal or hard edge.
                if ((portal & mask) != 0)
                {
                    return 0xf8 + dir;
                }
                return 0xff;
            }

            int bx = ax + Utils.GetDirOffsetX(dir);
            int by = ay + Utils.GetDirOffsetY(dir);
            int ib = bx + by * w;

            return Regs[ib];
        }
        public readonly bool WalkContour(int x, int y, TempContour cont)
        {
            int w = Header.Width;
            int h = Header.Height;

            cont.Reset();

            int startX = x;
            int startY = y;
            int startDir = -1;

            for (int i = 0; i < 4; ++i)
            {
                int dr = (i + 3) & 3;
                int rn = GetNeighbourReg(x, y, dr);
                if (rn != Regs[x + y * w])
                {
                    startDir = dr;
                    break;
                }
            }
            if (startDir == -1)
            {
                return true;
            }

            int dir = startDir;
            int maxIter = w * h;

            int iter = 0;
            while (iter < maxIter)
            {
                int rn = GetNeighbourReg(x, y, dir);

                int nx = x;
                int ny = y;
                int ndir;

                if (rn != Regs[x + y * w])
                {
                    // Solid edge.
                    int px = x;
                    int pz = y;
                    switch (dir)
                    {
                        case 0: pz++; break;
                        case 1: px++; pz++; break;
                        case 2: px++; break;
                    }

                    // Try to merge with previous vertex.
                    if (!cont.AppendVertex(px, Heights[x + y * w], pz, rn))
                    {
                        return false;
                    }

                    ndir = (dir + 1) & 0x3;  // Rotate CW
                }
                else
                {
                    // Move to next.
                    nx = x + Utils.GetDirOffsetX(dir);
                    ny = y + Utils.GetDirOffsetY(dir);
                    ndir = (dir + 3) & 0x3; // Rotate CCW
                }

                if (iter > 0 && x == startX && y == startY && dir == startDir)
                {
                    break;
                }

                x = nx;
                y = ny;
                dir = ndir;

                iter++;
            }

            // Remove last vertex if it is duplicate of the first one.
            cont.RemoveLast();

            return true;
        }
        public readonly int GetCornerHeight(int x, int y, int z, int walkableClimb, ref bool shouldRemove)
        {
            int w = Header.Width;
            int h = Header.Height;

            int n = 0;

            int portal = IndexedPolygon.PORTAL_FLAG;
            int height = 0;
            int preg = 0xff;
            bool allSameReg = true;

            for (int dz = -1; dz <= 0; ++dz)
            {
                for (int dx = -1; dx <= 0; ++dx)
                {
                    int px = x + dx;
                    int pz = z + dz;
                    if (px < 0 || pz < 0 || px >= w || pz >= h)
                    {
                        continue;
                    }

                    int idx = px + pz * w;
                    int lh = Heights[idx];
                    if (Math.Abs(lh - y) > walkableClimb || Areas[idx] == AreaTypes.RC_NULL_AREA)
                    {
                        continue;
                    }

                    height = Math.Max(height, lh);
                    portal &= Cons[idx] >> 4;
                    if (preg != 0xff && preg != Regs[idx])
                    {
                        allSameReg = false;
                    }
                    preg = Regs[idx];
                    n++;
                }
            }

            int portalCount = 0;
            for (int dir = 0; dir < 4; ++dir)
            {
                if ((portal & (1 << dir)) != 0)
                {
                    portalCount++;
                }
            }

            shouldRemove = false;
            if (n > 1 && portalCount == 1 && allSameReg)
            {
                shouldRemove = true;
            }

            return height;
        }
    }
}
