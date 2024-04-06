using Engine.PathFinding.RecastNavigation.Recast;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache layer
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="tile">Tile</param>
    public struct TileCacheLayer(CompressedTile tile)
    {
        /// <summary>
        /// Number of allocated regions
        /// </summary>
        private int nregions;
        /// <summary>
        /// Region collection
        /// </summary>
        private int[] regions;
        /// <summary>
        /// Height map
        /// </summary>
        private readonly int[] heights = [.. tile?.Data.Heights ?? []];
        /// <summary>
        /// Areas
        /// </summary>
        private readonly AreaTypes[] areas = [.. tile?.Data.Areas ?? []];
        /// <summary>
        /// Connections
        /// </summary>
        private readonly int[] connections = [.. tile?.Data.Connections ?? []];

        /// <summary>
        /// Header
        /// </summary>
        public TileCacheLayerHeader Header { get; private set; } = tile?.Header ?? default;

        /// <summary>
        /// Gets the layer height at index
        /// </summary>
        /// <param name="index">Index</param>
        public readonly int GetHeight(int index)
        {
            return heights[index];
        }
        /// <summary>
        /// Gets the layer area at index
        /// </summary>
        /// <param name="index">Index</param>
        public readonly AreaTypes GetArea(int index)
        {
            return areas[index];
        }
        /// <summary>
        /// Sets the layer area at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Area value</param>
        public readonly void SetArea(int index, AreaTypes value)
        {
            areas[index] = value;
        }

        /// <summary>
        /// Builds the region id list
        /// </summary>
        /// <param name="walkableClimb">Walkable climb value</param>
        public bool BuildRegions(int walkableClimb)
        {
            if (!BuildMonotoneRegions(walkableClimb, out var layerRegs, out int nregs))
            {
                regions = [];
                nregions = -1;

                return false;
            }

            // Allocate and init layer regions.
            var regs = AllocateLayerRegions(walkableClimb, layerRegs, nregs);

            // Compact ids.
            CompactIds(regs, layerRegs, nregs);

            regions = layerRegs;
            nregions = nregs;

            return true;
        }
        /// <summary>
        /// Partition walkable area into monotone regions.
        /// </summary>
        /// <param name="walkableClimb">Walkable climb value</param>
        /// <param name="layerRegs">Resulting layer regions</param>
        /// <param name="nregs">Region count</param>
        private bool BuildMonotoneRegions(int walkableClimb, out int[] layerRegs, out int nregs)
        {
            int w = Header.Width;
            int h = Header.Height;

            layerRegs = Helper.CreateArray(w * h, LayerMonotoneRegion.NULL_ID);

            int nsweeps = w;
            LayerSweepSpan[] sweeps = new LayerSweepSpan[nsweeps];

            // Partition walkable area into monotone regions.
            int[] samples = new int[256];
            nregs = 0;

            for (int row = 0; row < h; ++row)
            {
                ArrayUtils.ResetArray(samples, nregs, 0);
                int sweepCount = 0;

                for (int col = 0; col < w; ++col)
                {
                    int idx = col + row * w;
                    if (areas[idx] == AreaTypes.RC_NULL_AREA)
                    {
                        continue;
                    }

                    // -x
                    int sid = GetLayerRegX(col, row, w, walkableClimb, layerRegs);
                    if (sid == LayerMonotoneRegion.NULL_ID)
                    {
                        sid = sweepCount++;
                        sweeps[sid] = new()
                        {
                            NeiRegId = LayerMonotoneRegion.NULL_ID,
                            SampleCount = 0,
                        };
                    }

                    // -y
                    int layerReg = GetLayerRegY(col, row, w, walkableClimb, layerRegs);
                    if (layerReg != LayerMonotoneRegion.NULL_ID)
                    {
                        sweeps[sid].Update(layerReg, samples);
                    }

                    layerRegs[idx] = sid;
                }

                // Create unique ID.
                if (!LayerSweepSpan.CreateUniqueIds(sweeps, sweepCount, samples, ref nregs))
                {
                    return false;
                }

                // Remap local sweep ids to region ids.
                RemapRowRegionIds(row, w, layerRegs, sweeps);
            }

            return true;
        }
        /// <summary>
        /// Remaps row region ids
        /// </summary>
        /// <param name="row">Row index</param>
        /// <param name="w">Row width</param>
        /// <param name="layerRegs">Layer regions</param>
        /// <param name="sweeps">Layer sweep spans</param>
        private static void RemapRowRegionIds(int row, int w, int[] layerRegs, LayerSweepSpan[] sweeps)
        {
            for (int x = 0; x < w; ++x)
            {
                int idx = x + row * w;

                if (layerRegs[idx] != LayerMonotoneRegion.NULL_ID)
                {
                    layerRegs[idx] = sweeps[layerRegs[idx]].RegId;
                }
            }
        }
        /// <summary>
        /// Allocate and init layer regions.
        /// </summary>
        /// <param name="walkableClimb">Walkable climb value</param>
        /// <param name="layerRegs">Layer regions</param>
        /// <param name="nregs">Region count</param>
        /// <returns>Returns the allocated region list</returns>
        private readonly LayerMonotoneRegion[] AllocateLayerRegions(int walkableClimb, int[] layerRegs, int nregs)
        {
            int w = Header.Width;
            int h = Header.Height;

            // Allocate and init layer regions.
            LayerMonotoneRegion[] regs = Helper.CreateArray(nregs, () => { return new LayerMonotoneRegion(); });

            // Find region neighbours.
            foreach (var (col, row) in GridUtils.Iterate(w, h))
            {
                int idx = col + row * w;
                int ri = layerRegs[idx];
                if (ri == LayerMonotoneRegion.NULL_ID)
                {
                    continue;
                }

                // Update area.
                regs[ri].AreaId++;
                regs[ri].Area = areas[idx];

                // Update neighbours
                int ymi = col + (row - 1) * w;
                if (row <= 0 || !IsConnected(idx, ymi, walkableClimb))
                {
                    continue;
                }

                int rai = layerRegs[ymi];
                if (rai != LayerMonotoneRegion.NULL_ID && rai != ri)
                {
                    regs[ri].AddUniqueLast(rai);
                    regs[rai].AddUniqueLast(ri);
                }
            }

            LayerMonotoneRegion.InitializeIds(regs, nregs);

            LayerMonotoneRegion.Merge(regs, nregs);

            return regs;
        }
        /// <summary>
        /// Gets whether the specified areas were connected
        /// </summary>
        /// <param name="ia">A index</param>
        /// <param name="ib">B index</param>
        /// <param name="walkableClimb">Walkable climb value</param>
        private readonly bool IsConnected(int ia, int ib, int walkableClimb)
        {
            if (areas[ia] != areas[ib])
            {
                //Different areas
                return false;
            }

            if (Math.Abs(heights[ia] - heights[ib]) > walkableClimb)
            {
                //Too height
                return false;
            }

            return true;
        }
        /// <summary>
        /// Compact ids.
        /// </summary>
        /// <param name="regs">Region list</param>
        /// <param name="layerRegs">Layer regions</param>
        /// <param name="nregs">Region count</param>
        private readonly void CompactIds(LayerMonotoneRegion[] regs, int[] layerRegs, int nregs)
        {
            int w = Header.Width;
            int h = Header.Height;

            int[] remap = Helper.CreateArray(256, 0);

            // Find number of unique regions.
            int regId = 0;
            for (int i = 0; i < nregs; ++i)
            {
                remap[regs[i].RegId] = 1;
            }
            for (int i = 0; i < 256; ++i)
            {
                if (remap[i] != 0x00)
                {
                    remap[i] = regId++;
                }
            }

            // Remap ids.
            for (int i = 0; i < nregs; ++i)
            {
                regs[i].RegId = remap[regs[i].RegId];
            }
            for (int i = 0; i < w * h; ++i)
            {
                if (layerRegs[i] != LayerMonotoneRegion.NULL_ID)
                {
                    layerRegs[i] = regs[layerRegs[i]].RegId;
                }
            }
        }
        /// <summary>
        /// Gets the layer id on the left of current cell
        /// </summary>
        /// <param name="col">Column index</param>
        /// <param name="row">Row index</param>
        /// <param name="w">Row width</param>
        /// <param name="walkableClimb">Walkable climb value</param>
        /// <param name="layerRegs">Layer regions</param>
        private readonly int GetLayerRegX(int col, int row, int w, int walkableClimb, int[] layerRegs)
        {
            if (col <= 0)
            {
                return LayerMonotoneRegion.NULL_ID;
            }

            int idx = col + row * w;
            int xidx = (col - 1) + row * w;

            if (!IsConnected(idx, xidx, walkableClimb))
            {
                return LayerMonotoneRegion.NULL_ID;
            }

            return layerRegs[xidx];
        }
        /// <summary>
        /// Gets the layer id on top of current cell
        /// </summary>
        /// <param name="col">Column index</param>
        /// <param name="row">Row index</param>
        /// <param name="w">Row width</param>
        /// <param name="walkableClimb">Walkable climb value</param>
        /// <param name="layerRegs">Layer regions</param>
        private readonly int GetLayerRegY(int col, int row, int w, int walkableClimb, int[] layerRegs)
        {
            if (row <= 0)
            {
                return LayerMonotoneRegion.NULL_ID;
            }

            int idx = col + row * w;
            int yidx = col + (row - 1) * w;

            if (!IsConnected(idx, yidx, walkableClimb))
            {
                return LayerMonotoneRegion.NULL_ID;
            }

            return layerRegs[yidx];
        }

        /// <summary>
        /// Builds a new contour set
        /// </summary>
        /// <param name="walkableClimb">Walkable climb value</param>
        /// <param name="maxError">Maximum error value</param>
        /// <returns>Resulting contour set</returns>
        public readonly TileCacheContourSet BuildContourSet(int walkableClimb, float maxError)
        {
            int w = Header.Width;
            int h = Header.Height;

            TileCacheContourSet cset = new(nregions);

            // Allocate temp buffer for contour tracing.
            int maxTempVerts = (w + h) * 2 * 2; // Twice around the layer.

            var tempVerts = new VertexWithNeigbour[maxTempVerts];
            var tempPoly = new IndexedPolygon(maxTempVerts, false);

            var temp = new TempContour(tempVerts, maxTempVerts, tempPoly);

            // Find contours.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    int ri = regions[idx];
                    if (ri == LayerMonotoneRegion.NULL_ID)
                    {
                        continue;
                    }

                    var cont = cset.GetContour(ri);
                    if (cont.HasVertices())
                    {
                        continue;
                    }

                    cont.RegionId = ri;
                    cont.Area = areas[idx];

                    if (!WalkContour(temp, x, y, maxError, out var verts))
                    {
                        // Too complex contour.
                        throw new EngineException("Too complex contour, try increasing 'maxTempVerts'.");
                    }

                    // Store contour.
                    cont.StoreVerts(verts, verts.Length, this, walkableClimb);

                    cset.SetContour(ri, cont);
                }
            }

            return cset;
        }
        /// <summary>
        /// Walks the contour
        /// </summary>
        /// <param name="cont">Temporal contour helper</param>
        /// <param name="col">Column index</param>
        /// <param name="row">Row index</param>
        /// <param name="maxError">Max error</param>
        /// <param name="verts">Resulting vertext list</param>
        private readonly bool WalkContour(TempContour cont, int col, int row, float maxError, out VertexWithNeigbour[] verts)
        {
            int w = Header.Width;
            int h = Header.Height;

            cont.Reset();

            int startCol = col;
            int startRow = row;
            int startDir = GetStartWalkDirection(col, row, regions[col + row * w]);
            if (startDir == -1)
            {
                verts = cont.SimplifyContour(maxError);

                return true;
            }

            int dir = startDir;
            int maxIter = w * h;

            int iter = 0;
            while (iter < maxIter)
            {
                int rn = GetNeighbourRegionId(col, row, dir);

                int nCol = col;
                int nRow = row;
                int ndir;
                int idx = col + row * w;

                if (rn != regions[idx])
                {
                    // Solid edge.
                    var se = CreateSolidEdge(col, heights[idx], row, rn, dir);

                    // Try to merge with previous vertex.
                    if (!cont.AppendVertex(se))
                    {
                        verts = [];

                        return false;
                    }

                    ndir = GridUtils.RotateCW(dir);  // Rotate CW
                }
                else
                {
                    // Move to next.
                    nCol = col + GridUtils.GetDirOffsetX(dir);
                    nRow = row + GridUtils.GetDirOffsetY(dir);

                    ndir = GridUtils.RotateCCW(dir); // Rotate CCW
                }

                if (iter > 0 && col == startCol && row == startRow && dir == startDir)
                {
                    break;
                }

                col = nCol;
                row = nRow;
                dir = ndir;

                iter++;
            }

            // Remove last vertex if it is duplicate of the first one.
            cont.RemoveLast();

            verts = cont.SimplifyContour(maxError);

            return true;
        }
        /// <summary>
        /// Gets the starting direction for the walk contour operation
        /// </summary>
        /// <param name="col">Column</param>
        /// <param name="row">Row</param>
        /// <param name="idReg">Region id</param>
        /// <returns>Returns the direction value</returns>
        private readonly int GetStartWalkDirection(int col, int row, int idReg)
        {
            for (int i = 0; i < 4; ++i)
            {
                int dr = GridUtils.RotateCCW(i);
                int rn = GetNeighbourRegionId(col, row, dr);
                if (rn != idReg)
                {
                    return dr;
                }
            }

            return -1;
        }
        /// <summary>
        /// Creates a solid edge from parameters
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        /// <param name="z">Z value</param>
        /// <param name="rn">Region id</param>
        /// <param name="dir">Direction</param>
        private static VertexWithNeigbour CreateSolidEdge(int x, int y, int z, int rn, int dir)
        {
            int px = x;
            int py = y;
            int pz = z;
            switch (dir)
            {
                case 0: pz++; break;
                case 1: px++; pz++; break;
                case 2: px++; break;
            }

            return new(px, py, pz, rn);
        }
        /// <summary>
        /// Gets the neighbour region id
        /// </summary>
        /// <param name="col">Column index</param>
        /// <param name="row">Row index</param>
        /// <param name="dir">Neighbour at direction</param>
        /// <returns>Returns the neighbour region id</returns>
        private readonly int GetNeighbourRegionId(int col, int row, int dir)
        {
            int w = Header.Width;
            int ia = col + row * w;
            int con = connections[ia];

            int conDir = Edge.GetVertexDirection(con);
            int portal = con >> 4;

            if (!IsPortalAtDirection(conDir, dir))
            {
                // No connection, return portal or hard edge.
                if (IsPortalAtDirection(portal, dir))
                {
                    return TileCacheContour.DT_DIR_MASK + dir;
                }
                return LayerMonotoneRegion.NULL_ID;
            }

            int bx = col + GridUtils.GetDirOffsetX(dir);
            int by = row + GridUtils.GetDirOffsetY(dir);
            int ib = bx + by * w;

            return regions[ib];
        }
        /// <summary>
        /// Gets the corner height
        /// </summary>
        /// <param name="v">Vertex</param>
        /// <param name="walkableClimb">Walkable climb value</param>
        /// <param name="shouldRemove">Should remove vertex</param>
        /// <returns>Returns the corner height from height-map</returns>
        public readonly int GetCornerHeight(VertexWithNeigbour v, int walkableClimb, out bool shouldRemove)
        {
            int x = v.X;
            int y = v.Y;
            int z = v.Z;

            int w = Header.Width;
            int h = Header.Height;

            int n = 0;

            int portal = Edge.DT_PORTAL_FLAG;
            int height = 0;
            int preg = LayerMonotoneRegion.NULL_ID;
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
                    int lh = heights[idx];
                    if (Math.Abs(lh - y) > walkableClimb || areas[idx] == AreaTypes.RC_NULL_AREA)
                    {
                        continue;
                    }

                    height = Math.Max(height, lh);
                    portal &= connections[idx] >> 4;
                    if (preg != LayerMonotoneRegion.NULL_ID && preg != regions[idx])
                    {
                        allSameReg = false;
                    }
                    preg = regions[idx];
                    n++;
                }
            }

            shouldRemove = ShouldRemove(n, allSameReg, portal);

            return height;
        }
        /// <summary>
        /// Gets whether the vertex should be removed or not
        /// </summary>
        /// <param name="n">Number of detected vertices</param>
        /// <param name="allSameReg">All detected vertices are from the same region</param>
        /// <param name="con">Connecion</param>
        private static bool ShouldRemove(int n, bool allSameReg, int con)
        {
            if (n <= 1)
            {
                return false;
            }

            if (!allSameReg)
            {
                return false;
            }

            int portalCount = GetPortalCount(con);

            return portalCount == 1;
        }
        /// <summary>
        /// Gets the portal count from the specified connection
        /// </summary>
        /// <param name="con">Connection</param>
        public static int GetPortalCount(int con)
        {
            int portalCount = 0;
            for (int dir = 0; dir < 4; ++dir)
            {
                if (IsPortalAtDirection(con, dir))
                {
                    portalCount++;
                }
            }

            return portalCount;
        }
        /// <summary>
        /// Gets whether the especified connection has a portal at the speficied direction
        /// </summary>
        /// <param name="con">Connection</param>
        /// <param name="dir">Direction</param>
        public static bool IsPortalAtDirection(int con, int dir)
        {
            int mask = 1 << dir;

            return (con & mask) != 0;
        }
    }
}
