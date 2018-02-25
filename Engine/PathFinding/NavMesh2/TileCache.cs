using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Tile Cache
    /// </summary>
    public class TileCache
    {
        public const int MaxLayers = 32;
        public const int VertsPerPolygon = 6;
        public const int NullIdx = 0xffff;
        public const int VertexBucketCount2 = (1 << 8);
        public const int MaxRemEdges = 48;

        public static int ComputeTileHash(int x, int y, int mask)
        {
            uint h1 = 0x8da6b343; // Large multiplicative constants;
            uint h2 = 0xd8163841; // here arbitrarily chosen primes
            uint n = (uint)(h1 * x + h2 * y);
            return (int)(n & mask);
        }
        private static bool Contains(CompressedTile[] a, int n, CompressedTile v)
        {
            for (int i = 0; i < n; ++i)
            {
                if (a[i] == v) return true;
            }

            return false;
        }

        private TileCacheParams m_params;
        private TileCacheMeshProcess m_tmproc;
        private TileCacheObstacle[] m_obstacles = null;
        private TileCacheObstacle m_nextFreeObstacle = null;
        private int m_tileLutSize;
        private int m_tileLutMask;
        private CompressedTile[] m_tiles = null;
        private CompressedTile[] m_posLookup = null;
        private CompressedTile m_nextFreeTile = null;
        private int m_tileBits;
        private int m_saltBits;

        /// <summary>
        /// Constructor
        /// </summary>
        public TileCache()
        {

        }

        public void Init(TileCacheParams tcparams, TileCacheMeshProcess tmproc)
        {
            m_params = tcparams;
            m_tmproc = tmproc;

            // Alloc space for obstacles.
            m_obstacles = new TileCacheObstacle[tcparams.MaxObstacles];
            m_nextFreeObstacle = null;
            for (int i = tcparams.MaxObstacles - 1; i >= 0; i--)
            {
                m_obstacles[i] = new TileCacheObstacle
                {
                    Salt = 1,
                    Next = m_nextFreeObstacle
                };
                m_nextFreeObstacle = m_obstacles[i];
            }

            // Init tiles
            m_tileLutSize = Helper.NextPowerOfTwo(tcparams.MaxTiles / 4);
            if (m_tileLutSize == 0) m_tileLutSize = 1;
            m_tileLutMask = m_tileLutSize - 1;

            m_tiles = new CompressedTile[tcparams.MaxTiles];
            m_posLookup = new CompressedTile[m_tileLutSize];

            for (int i = tcparams.MaxTiles - 1; i >= 0; i--)
            {
                m_tiles[i] = new CompressedTile
                {
                    Salt = 1,
                    Next = m_nextFreeTile
                };
                m_nextFreeTile = m_tiles[i];
            }

            // Init ID generator values.
            m_tileBits = (int)Math.Log(Helper.NextPowerOfTwo(tcparams.MaxTiles), 2);

            // Only allow 31 salt bits, since the salt mask is calculated using 32bit uint and it will overflow.
            m_saltBits = Math.Min(31, 32 - m_tileBits);
            if (m_saltBits < 10)
            {
                throw new EngineException("NavMesh DT_INVALID_PARAM");
            }
        }

        public CompressedTile AddTile(TileCacheData data, TileFlags flags)
        {
            // Make sure the data is in right format.
            TileCacheLayerHeader header = data.Header;
            if (header.magic != TileCacheLayerHeader.TileCacheMagic)
            {
                throw new EngineException("DT_WRONG_MAGIC");
            }
            if (header.version != TileCacheLayerHeader.TileCacheVersion)
            {
                throw new EngineException("DT_WRONG_VERSION");
            }

            // Make sure the location is free.
            if (GetTileAt(header.tx, header.ty, header.tlayer) != null)
            {
                throw new EngineException("DT_FAILURE");
            }

            // Allocate a tile.
            CompressedTile tile = null;
            if (m_nextFreeTile != null)
            {
                tile = m_nextFreeTile;
                m_nextFreeTile = tile.Next;
                tile.Next = null;
            }

            // Insert tile into the position lut.
            int h = ComputeTileHash(header.tx, header.ty, m_tileLutMask);
            tile.Next = m_posLookup[h];
            m_posLookup[h] = tile;

            // Init tile.
            int headerSize = Helper.Align4(TileCacheLayerHeader.Size);
            tile.Header = data.Header;
            tile.Data = data.Data;
            tile.DataSize = 0;// data.DataSize;
            tile.Compressed = data.Data; //tile.Data.CopyTo(tile.Compressed, headerSize);
            tile.CompressedSize = tile.DataSize - headerSize;
            tile.Flags = flags;

            return tile;
        }
        private CompressedTile GetTileAt(int tx, int ty, int tlayer)
        {
            // Find tile based on hash.
            int h = ComputeTileHash(tx, ty, m_tileLutMask);
            CompressedTile tile = m_posLookup[h];
            while (tile != null)
            {
                if (tile.Header.tx == tx &&
                    tile.Header.ty == ty &&
                    tile.Header.tlayer == tlayer)
                {
                    return tile;
                }
                tile = tile.Next;
            }

            return null;
        }

        public bool BuildNavMeshTilesAt(int tx, int ty, NavMesh navmesh)
        {
            int MAX_TILES = 32;
            CompressedTile[] tiles = new CompressedTile[MAX_TILES];
            int ntiles = GetTilesAt(tx, ty, tiles, MAX_TILES);

            for (int i = 0; i < ntiles; ++i)
            {
                if (!BuildNavMeshTile(tiles[i], navmesh))
                {
                    return false;
                }
            }

            return true;
        }
        private int GetTilesAt(int tx, int ty, CompressedTile[] tiles, int maxTiles)
        {
            int n = 0;

            // Find tile based on hash.
            int h = ComputeTileHash(tx, ty, m_tileLutMask);
            CompressedTile tile = m_posLookup[h];
            while (tile != null)
            {
                if (tile.Header.tx == tx && tile.Header.ty == ty)
                {
                    if (n < maxTiles)
                    {
                        tiles[n++] = tile;
                    }
                }

                tile = tile.Next;
            }

            return n;
        }
        private bool BuildNavMeshTile(CompressedTile tile, NavMesh navmesh)
        {
            NavMeshTileBuildContext bc = new NavMeshTileBuildContext();
            int walkableClimbVx = (int)(m_params.WalkableClimb / m_params.CellHeight);

            // Decompress tile layer data.
            if (!DecompressTileCacheLayer(tile.Header, tile.Data, tile.DataSize, out bc.layer))
            {
                return false;
            }

            // Rasterize obstacles.
            for (int i = 0; i < m_params.MaxObstacles; ++i)
            {
                TileCacheObstacle ob = m_obstacles[i];
                if (ob.state == ObstacleState.Empty || ob.state == ObstacleState.Removing)
                {
                    continue;
                }

                if (Contains(ob.touched, ob.ntouched, tile))
                {
                    if (ob.type == ObstacleType.Cylinder)
                    {
                        MarkCylinderArea(
                            bc.layer, tile.Header.b.Minimum, m_params.CellSize, m_params.CellHeight,
                            ob.cylinder.pos, ob.cylinder.radius, ob.cylinder.height, 0);
                    }
                    else if (ob.type == ObstacleType.Box)
                    {
                        MarkBoxArea(
                            bc.layer, tile.Header.b.Minimum, m_params.CellSize, m_params.CellHeight,
                            ob.box.bmin, ob.box.bmax, 0);
                    }
                    else if (ob.type == ObstacleType.OrientedBox)
                    {
                        MarkBoxArea(
                            bc.layer, tile.Header.b.Minimum, m_params.CellSize, m_params.CellHeight,
                            ob.orientedBox.center, ob.orientedBox.halfExtents, ob.orientedBox.rotAux, 0);
                    }
                }
            }

            // Build navmesh
            if (!BuildTileCacheRegions(bc.layer, walkableClimbVx))
            {
                return false;
            }

            if (!BuildTileCacheContours(bc.layer, walkableClimbVx, m_params.MaxSimplificationError, out bc.lcset))
            {
                return false;
            }

            if (!BuildTileCachePolyMesh(bc.lcset, out bc.lmesh))
            {
                return false;
            }

            // Early out if the mesh tile is empty.
            if (bc.lmesh.npolys == 0)
            {
                // Remove existing tile.
                navmesh.RemoveTile(navmesh.GetTileRefAt(tile.Header.tx, tile.Header.ty, tile.Header.tlayer), 0, 0);
                return true;
            }

            NavMeshCreateParams param = new NavMeshCreateParams
            {
                verts = bc.lmesh.verts,
                vertCount = bc.lmesh.nverts,
                polys = bc.lmesh.polys,
                polyAreas = bc.lmesh.areas,
                polyFlags = bc.lmesh.flags,
                polyCount = bc.lmesh.npolys,
                nvp = VertsPerPolygon,
                walkableHeight = m_params.WalkableHeight,
                walkableRadius = m_params.WalkableRadius,
                walkableClimb = m_params.WalkableClimb,
                tileX = tile.Header.tx,
                tileY = tile.Header.ty,
                tileLayer = tile.Header.tlayer,
                cs = m_params.CellSize,
                ch = m_params.CellHeight,
                buildBvTree = false,
                bmin = tile.Header.b.Minimum,
                bmax = tile.Header.b.Maximum
            };

            if (m_tmproc != null)
            {
                m_tmproc.Process(param, bc.lmesh.areas, bc.lmesh.flags);
            }

            byte[] navData = null;
            int navDataSize = 0;
            if (!CreateNavMeshData(param, out navData, out navDataSize))
            {
                return false;
            }

            // Remove existing tile.
            navmesh.RemoveTile(navmesh.GetTileRefAt(tile.Header.tx, tile.Header.ty, tile.Header.tlayer), 0, 0);

            // Add new tile, or leave the location empty.
            if (navData != null)
            {
                // Let the navmesh own the data.
                if (!navmesh.AddTile(navData, navDataSize, TileFlags.FreeData, 0, 0))
                {
                    navData = null;

                    return false;
                }
            }

            return true;
        }
        private bool DecompressTileCacheLayer(TileCacheLayerHeader header, TileCacheLayerData data, int dataSize, out TileCacheLayer layer)
        {
            layer = new TileCacheLayer()
            {
                header = header,
                areas = data.areas,
                heights = data.heights,
                cons = data.cons,
                regCount = 0,
                regs = null,
            };

            return true;
        }
        private bool MarkBoxArea(TileCacheLayer layer, Vector3 orig, float cs, float ch, Vector3 center, Vector3 halfExtents, Vector2 rotAux, TileCacheAreas areaId)
        {
            int w = layer.header.width;
            int h = layer.header.height;
            float ics = 1.0f / cs;
            float ich = 1.0f / ch;

            float cx = (center.X - orig.X) * ics;
            float cz = (center.Z - orig.Z) * ics;

            float maxr = 1.41f * Math.Max(halfExtents.X, halfExtents.Z);
            int minx = (int)Math.Floor(cx - maxr * ics);
            int maxx = (int)Math.Floor(cx + maxr * ics);
            int minz = (int)Math.Floor(cz - maxr * ics);
            int maxz = (int)Math.Floor(cz + maxr * ics);
            int miny = (int)Math.Floor((center.Y - halfExtents.Y - orig.Y) * ich);
            int maxy = (int)Math.Floor((center.Y + halfExtents.Y - orig.Y) * ich);

            if (maxx < 0) return true;
            if (minx >= w) return true;
            if (maxz < 0) return true;
            if (minz >= h) return true;

            if (minx < 0) minx = 0;
            if (maxx >= w) maxx = w - 1;
            if (minz < 0) minz = 0;
            if (maxz >= h) maxz = h - 1;

            float xhalf = halfExtents.X * ics + 0.5f;
            float zhalf = halfExtents.Z * ics + 0.5f;

            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    float x2 = 2.0f * (x - cx);
                    float z2 = 2.0f * (z - cz);
                    float xrot = rotAux.Y * x2 + rotAux.X * z2;
                    if (xrot > xhalf || xrot < -xhalf)
                    {
                        continue;
                    }
                    float zrot = rotAux.Y * z2 - rotAux.X * x2;
                    if (zrot > zhalf || zrot < -zhalf)
                    {
                        continue;
                    }
                    int y = layer.heights[x + z * w];
                    if (y < miny || y > maxy)
                    {
                        continue;
                    }
                    layer.areas[x + z * w] = areaId;
                }
            }

            return true;
        }
        private bool MarkBoxArea(TileCacheLayer layer, Vector3 orig, float cs, float ch, Vector3 bmin, Vector3 bmax, TileCacheAreas areaId)
        {
            int w = layer.header.width;
            int h = layer.header.height;
            float ics = 1.0f / cs;
            float ich = 1.0f / ch;

            int minx = (int)Math.Floor((bmin.X - orig.X) * ics);
            int miny = (int)Math.Floor((bmin.Y - orig.Y) * ich);
            int minz = (int)Math.Floor((bmin.Z - orig.Z) * ics);
            int maxx = (int)Math.Floor((bmax.X - orig.X) * ics);
            int maxy = (int)Math.Floor((bmax.Y - orig.Y) * ich);
            int maxz = (int)Math.Floor((bmax.Z - orig.Z) * ics);

            if (maxx < 0) return true;
            if (minx >= w) return true;
            if (maxz < 0) return true;
            if (minz >= h) return true;

            if (minx < 0) minx = 0;
            if (maxx >= w) maxx = w - 1;
            if (minz < 0) minz = 0;
            if (maxz >= h) maxz = h - 1;

            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    int y = layer.heights[x + z * w];
                    if (y < miny || y > maxy)
                    {
                        continue;
                    }
                    layer.areas[x + z * w] = areaId;
                }
            }

            return true;
        }
        private bool MarkCylinderArea(TileCacheLayer layer, Vector3 orig, float cs, float ch, Vector3 pos, float radius, float height, TileCacheAreas areaId)
        {
            Vector3 bmin = new Vector3();
            Vector3 bmax = new Vector3();
            bmin.X = pos.X - radius;
            bmin.Y = pos.Y;
            bmin.Z = pos.Z - radius;
            bmax.X = pos.X + radius;
            bmax.Y = pos.Y + height;
            bmax.Z = pos.Z + radius;
            float r2 = (float)Math.Sqrt(radius / cs + 0.5f);

            int w = layer.header.width;
            int h = layer.header.height;
            float ics = 1.0f / cs;
            float ich = 1.0f / ch;

            float px = (pos.X - orig.X) * ics;
            float pz = (pos.Z - orig.Z) * ics;

            int minx = (int)Math.Floor((bmin.X - orig.X) * ics);
            int miny = (int)Math.Floor((bmin.Y - orig.Y) * ich);
            int minz = (int)Math.Floor((bmin.Z - orig.Z) * ics);
            int maxx = (int)Math.Floor((bmax.X - orig.X) * ics);
            int maxy = (int)Math.Floor((bmax.Y - orig.Y) * ich);
            int maxz = (int)Math.Floor((bmax.Z - orig.Z) * ics);

            if (maxx < 0) return true;
            if (minx >= w) return true;
            if (maxz < 0) return true;
            if (minz >= h) return true;

            if (minx < 0) minx = 0;
            if (maxx >= w) maxx = w - 1;
            if (minz < 0) minz = 0;
            if (maxz >= h) maxz = h - 1;

            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    float dx = (x + 0.5f) - px;
                    float dz = (z + 0.5f) - pz;
                    if (dx * dx + dz * dz > r2)
                    {
                        continue;
                    }
                    int y = layer.heights[x + z * w];
                    if (y < miny || y > maxy)
                    {
                        continue;
                    }
                    layer.areas[x + z * w] = areaId;
                }
            }

            return true;
        }
        private bool BuildTileCacheRegions(TileCacheLayer layer, int walkableClimb)
        {
            int w = layer.header.width;
            int h = layer.header.height;

            layer.regs = Helper.CreateArray<byte>(w * h, 0xff);

            int nsweeps = w;
            LayerSweepSpan[] sweeps = new LayerSweepSpan[nsweeps];

            // Partition walkable area into monotone regions.
            byte[] prevCount = new byte[256];
            byte regId = 0;

            for (int y = 0; y < h; ++y)
            {
                if (regId > 0)
                {
                    for (int i = 0; i < regId; i++)
                    {
                        prevCount[i] = 0;
                    }
                }
                byte sweepId = 0;

                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    if (layer.areas[idx] == TileCacheAreas.NullArea)
                    {
                        continue;
                    }

                    byte sid = 0xff;

                    // -x
                    int xidx = (x - 1) + y * w;
                    if (x > 0 && IsConnected(layer, idx, xidx, walkableClimb))
                    {
                        if (layer.regs[xidx] != 0xff)
                            sid = layer.regs[xidx];
                    }

                    if (sid == 0xff)
                    {
                        sid = sweepId++;
                        sweeps[sid].nei = 0xff;
                        sweeps[sid].ns = 0;
                    }

                    // -y
                    int yidx = x + (y - 1) * w;
                    if (y > 0 && IsConnected(layer, idx, yidx, walkableClimb))
                    {
                        byte nr = layer.regs[yidx];
                        if (nr != 0xff)
                        {
                            // Set neighbour when first valid neighbour is encoutered.
                            if (sweeps[sid].ns == 0)
                            {
                                sweeps[sid].nei = nr;
                            }

                            if (sweeps[sid].nei == nr)
                            {
                                // Update existing neighbour
                                sweeps[sid].ns++;
                                prevCount[nr]++;
                            }
                            else
                            {
                                // This is hit if there is nore than one neighbour.
                                // Invalidate the neighbour.
                                sweeps[sid].nei = 0xff;
                            }
                        }
                    }

                    layer.regs[idx] = sid;
                }

                // Create unique ID.
                for (int i = 0; i < sweepId; ++i)
                {
                    // If the neighbour is set and there is only one continuous connection to it,
                    // the sweep will be merged with the previous one, else new region is created.
                    if (sweeps[i].nei != 0xff && prevCount[sweeps[i].nei] == sweeps[i].ns)
                    {
                        sweeps[i].id = sweeps[i].nei;
                    }
                    else
                    {
                        if (regId == 255)
                        {
                            // Region ID's overflow.
                            return false;
                        }
                        sweeps[i].id = regId++;
                    }
                }

                // Remap local sweep ids to region ids.
                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    if (layer.regs[idx] != 0xff)
                    {
                        layer.regs[idx] = sweeps[layer.regs[idx]].id;
                    }
                }
            }

            // Allocate and init layer regions.
            int nregs = regId;
            LayerMonotoneRegion[] regs = new LayerMonotoneRegion[nregs];

            for (int i = 0; i < nregs; ++i)
            {
                regs[i].regId = 0xff;
            }

            // Find region neighbours.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    byte ri = layer.regs[idx];
                    if (ri == 0xff)
                    {
                        continue;
                    }

                    // Update area.
                    regs[ri].area++;
                    regs[ri].areaId = layer.areas[idx];

                    // Update neighbours
                    int ymi = x + (y - 1) * w;
                    if (y > 0 && IsConnected(layer, idx, ymi, walkableClimb))
                    {
                        byte rai = layer.regs[ymi];
                        if (rai != 0xff && rai != ri)
                        {
                            AddUniqueLast(ref regs[ri].neis, ref regs[ri].nneis, rai);
                            AddUniqueLast(ref regs[rai].neis, ref regs[rai].nneis, ri);
                        }
                    }
                }
            }

            for (int i = 0; i < nregs; ++i)
            {
                regs[i].regId = (byte)i;
            }

            for (int i = 0; i < nregs; ++i)
            {
                LayerMonotoneRegion reg = regs[i];

                int merge = -1;
                int mergea = 0;
                for (int j = 0; j < (int)reg.nneis; ++j)
                {
                    byte nei = reg.neis[j];
                    LayerMonotoneRegion regn = regs[nei];
                    if (reg.regId == regn.regId)
                    {
                        continue;
                    }
                    if (reg.areaId != regn.areaId)
                    {
                        continue;
                    }
                    if (regn.area > mergea)
                    {
                        if (CanMerge(reg.regId, regn.regId, regs, nregs))
                        {
                            mergea = regn.area;
                            merge = (int)nei;
                        }
                    }
                }
                if (merge != -1)
                {
                    byte oldId = reg.regId;
                    byte newId = regs[merge].regId;
                    for (int j = 0; j < nregs; ++j)
                    {
                        if (regs[j].regId == oldId)
                        {
                            regs[j].regId = newId;
                        }
                    }
                }
            }

            // Compact ids.
            byte[] remap = Helper.CreateArray<byte>(256, 0);
            // Find number of unique regions.
            regId = 0;
            for (int i = 0; i < nregs; ++i)
            {
                remap[regs[i].regId] = 1;
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
                regs[i].regId = remap[regs[i].regId];
            }

            layer.regCount = regId;

            for (int i = 0; i < w * h; ++i)
            {
                if (layer.regs[i] != 0xff)
                {
                    layer.regs[i] = regs[layer.regs[i]].regId;
                }
            }

            return true;
        }
        private bool IsConnected(TileCacheLayer layer, int ia, int ib, int walkableClimb)
        {
            if (layer.areas[ia] != layer.areas[ib])
            {
                return false;
            }
            if (Math.Abs(layer.heights[ia] - layer.heights[ib]) > walkableClimb)
            {
                return false;
            }
            return true;
        }
        private void AddUniqueLast(ref byte[] a, ref byte an, byte v)
        {
            int n = an;
            if (n > 0 && a[n - 1] == v)
            {
                return;
            }
            a[an] = v;
            an++;
        }
        private bool CanMerge(byte oldRegId, byte newRegId, LayerMonotoneRegion[] regs, int nregs)
        {
            int count = 0;
            for (int i = 0; i < nregs; ++i)
            {
                LayerMonotoneRegion reg = regs[i];
                if (reg.regId != oldRegId)
                {
                    continue;
                }
                int nnei = reg.nneis;
                for (int j = 0; j < nnei; ++j)
                {
                    if (regs[reg.neis[j]].regId == newRegId)
                    {
                        count++;
                    }
                }
            }
            return count == 1;
        }
        private bool BuildTileCacheContours(TileCacheLayer layer, int walkableClimb, float maxError, out TileCacheContourSet lcset)
        {
            int w = layer.header.width;
            int h = layer.header.height;

            lcset.nconts = layer.regCount;
            lcset.conts = new TileCacheContour[lcset.nconts];

            // Allocate temp buffer for contour tracing.
            int maxTempVerts = (w + h) * 2 * 2; // Twice around the layer.

            int[][] tempVerts = new int[maxTempVerts][];
            uint[] tempPoly = new uint[maxTempVerts];

            TempContour temp = new TempContour(tempVerts, maxTempVerts, tempPoly, maxTempVerts);

            // Find contours.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    byte ri = layer.regs[idx];
                    if (ri == 0xff)
                    {
                        continue;
                    }

                    TileCacheContour cont = lcset.conts[ri];

                    if (cont.nverts > 0)
                    {
                        continue;
                    }

                    cont.reg = ri;
                    cont.area = layer.areas[idx];

                    if (!WalkContour(layer, x, y, temp))
                    {
                        // Too complex contour.
                        // Note: If you hit here ofte, try increasing 'maxTempVerts'.
                        return false;
                    }

                    SimplifyContour(temp, maxError);

                    // Store contour.
                    cont.nverts = temp.nverts;
                    if (cont.nverts > 0)
                    {
                        cont.verts = new int[temp.nverts][];

                        for (int i = 0, j = temp.nverts - 1; i < temp.nverts; j = i++)
                        {
                            int[] dst = cont.verts[j];
                            int[] v = temp.verts[j];
                            int[] vn = temp.verts[i];
                            int nei = vn[3]; // The neighbour reg is stored at segment vertex of a segment. 
                            bool shouldRemove = false;
                            int lh = GetCornerHeight(layer, v[0], v[1], v[2], walkableClimb, ref shouldRemove);

                            dst[0] = v[0];
                            dst[1] = lh;
                            dst[2] = v[2];

                            // Store portal direction and remove status to the fourth component.
                            dst[3] = 0x0f;
                            if (nei != 0xff && nei >= 0xf8)
                            {
                                dst[3] = nei - 0xf8;
                            }
                            if (shouldRemove)
                            {
                                dst[3] |= 0x80;
                            }
                        }
                    }
                }
            }

            return true;
        }
        private int GetCornerHeight(TileCacheLayer layer, int x, int y, int z, int walkableClimb, ref bool shouldRemove)
        {
            int w = layer.header.width;
            int h = layer.header.height;

            int n = 0;

            int portal = 0xf;
            int height = 0;
            int preg = 0xff;
            bool allSameReg = true;

            for (int dz = -1; dz <= 0; ++dz)
            {
                for (int dx = -1; dx <= 0; ++dx)
                {
                    int px = x + dx;
                    int pz = z + dz;
                    if (px >= 0 && pz >= 0 && px < w && pz < h)
                    {
                        int idx = px + pz * w;
                        int lh = (int)layer.heights[idx];
                        if (Math.Abs(lh - y) <= walkableClimb && layer.areas[idx] != TileCacheAreas.NullArea)
                        {
                            height = Math.Max(height, lh);
                            portal &= (layer.cons[idx] >> 4);
                            if (preg != 0xff && preg != layer.regs[idx])
                            {
                                allSameReg = false;
                            }
                            preg = layer.regs[idx];
                            n++;
                        }
                    }
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
        private bool WalkContour(TileCacheLayer layer, int x, int y, TempContour cont)
        {
            int w = layer.header.width;
            int h = layer.header.height;

            cont.nverts = 0;

            int startX = x;
            int startY = y;
            int startDir = -1;

            for (int i = 0; i < 4; ++i)
            {
                int dr = (i + 3) & 3;
                int rn = GetNeighbourReg(layer, x, y, dr);
                if (rn != layer.regs[x + y * w])
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
                int rn = GetNeighbourReg(layer, x, y, dir);

                int nx = x;
                int ny = y;
                int ndir = dir;

                if (rn != layer.regs[x + y * w])
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
                    if (!AppendVertex(cont, px, layer.heights[x + y * w], pz, rn))
                    {
                        return false;
                    }

                    ndir = (dir + 1) & 0x3;  // Rotate CW
                }
                else
                {
                    // Move to next.
                    nx = x + GetDirOffsetX(dir);
                    ny = y + GetDirOffsetY(dir);
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
            int[] pa = cont.verts[(cont.nverts - 1) * 4];
            int[] pb = cont.verts[0];
            if (pa[0] == pb[0] && pa[2] == pb[2])
            {
                cont.nverts--;
            }

            return true;
        }
        private int GetNeighbourReg(TileCacheLayer layer, int ax, int ay, int dir)
        {
            int w = layer.header.width;
            int ia = ax + ay * w;

            int con = layer.cons[ia] & 0xf;
            int portal = layer.cons[ia] >> 4;
            int mask = (1 << dir);

            if ((con & mask) == 0)
            {
                // No connection, return portal or hard edge.
                if ((portal & mask) != 0)
                {
                    return 0xf8 + dir;
                }
                return 0xff;
            }

            int bx = ax + GetDirOffsetX(dir);
            int by = ay + GetDirOffsetY(dir);
            int ib = bx + by * w;

            return layer.regs[ib];
        }
        private int GetDirOffsetX(int dir)
        {
            int[] offset = { -1, 0, 1, 0, };
            return offset[dir & 0x03];
        }
        private int GetDirOffsetY(int dir)
        {
            int[] offset = { 0, 1, 0, -1 };
            return offset[dir & 0x03];
        }
        private bool AppendVertex(TempContour cont, int x, int y, int z, int r)
        {
            // Try to merge with existing segments.
            if (cont.nverts > 1)
            {
                int[] pa = cont.verts[cont.nverts - 2];
                int[] pb = cont.verts[cont.nverts - 1];
                if (pb[3] == r)
                {
                    if (pa[0] == pb[0] && (int)pb[0] == x)
                    {
                        // The verts are aligned aling x-axis, update z.
                        pb[1] = y;
                        pb[2] = z;
                        return true;
                    }
                    else if (pa[2] == pb[2] && (int)pb[2] == z)
                    {
                        // The verts are aligned aling z-axis, update x.
                        pb[0] = x;
                        pb[1] = y;
                        return true;
                    }
                }
            }

            // Add new point.
            if (cont.nverts + 1 > cont.cverts)
            {
                return false;
            }

            int[] v = cont.verts[cont.nverts];
            v[0] = x;
            v[1] = y;
            v[2] = z;
            v[3] = r;
            cont.nverts++;

            return true;
        }
        private void SimplifyContour(TempContour cont, float maxError)
        {
            cont.npoly = 0;

            for (int i = 0; i < cont.nverts; ++i)
            {
                int j = (i + 1) % cont.nverts;
                // Check for start of a wall segment.
                int ra = cont.verts[j][3];
                int rb = cont.verts[i][3];
                if (ra != rb)
                {
                    cont.poly[cont.npoly++] = (uint)i;
                }
            }
            if (cont.npoly < 2)
            {
                // If there is no transitions at all,
                // create some initial points for the simplification process. 
                // Find lower-left and upper-right vertices of the contour.
                int llx = cont.verts[0][0];
                int llz = cont.verts[0][2];
                int lli = 0;
                int urx = cont.verts[0][0];
                int urz = cont.verts[0][2];
                int uri = 0;
                for (int i = 1; i < cont.nverts; ++i)
                {
                    int x = cont.verts[i][0];
                    int z = cont.verts[i][2];
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
                cont.npoly = 0;
                cont.poly[cont.npoly++] = (uint)lli;
                cont.poly[cont.npoly++] = (uint)uri;
            }

            // Add points until all raw points are within
            // error tolerance to the simplified shape.
            for (int i = 0; i < cont.npoly;)
            {
                int ii = (i + 1) % cont.npoly;

                int ai = (int)cont.poly[i];
                int ax = (int)cont.verts[ai][0];
                int az = (int)cont.verts[ai][2];

                int bi = (int)cont.poly[ii];
                int bx = (int)cont.verts[bi][0];
                int bz = (int)cont.verts[bi][2];

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
                    ci = (ai + cinc) % cont.nverts;
                    endi = bi;
                }
                else
                {
                    cinc = cont.nverts - 1;
                    ci = (bi + cinc) % cont.nverts;
                    endi = ai;
                }

                // Tessellate only outer edges or edges between areas.
                while (ci != endi)
                {
                    float d = DistancePtSeg(cont.verts[ci][0], cont.verts[ci][2], ax, az, bx, bz);
                    if (d > maxd)
                    {
                        maxd = d;
                        maxi = ci;
                    }
                    ci = (ci + cinc) % cont.nverts;
                }

                // If the max deviation is larger than accepted error,
                // add new point, else continue to next segment.
                if (maxi != -1 && maxd > (maxError * maxError))
                {
                    cont.npoly++;
                    for (int j = cont.npoly - 1; j > i; --j)
                    {
                        cont.poly[j] = cont.poly[j - 1];
                    }
                    cont.poly[i + 1] = (uint)maxi;
                }
                else
                {
                    ++i;
                }
            }

            // Remap vertices
            int start = 0;
            for (int i = 1; i < cont.npoly; ++i)
            {
                if (cont.poly[i] < cont.poly[start])
                {
                    start = i;
                }
            }

            cont.nverts = 0;
            for (int i = 0; i < cont.npoly; ++i)
            {
                int j = (start + i) % cont.npoly;
                int[] src = cont.verts[cont.poly[j]];
                int[] dst = cont.verts[cont.nverts];
                dst[0] = src[0];
                dst[1] = src[1];
                dst[2] = src[2];
                dst[3] = src[3];
                cont.nverts++;
            }
        }
        private float DistancePtSeg(int x, int z, int px, int pz, int qx, int qz)
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

        private static bool BuildTileCachePolyMesh(TileCacheContourSet lcset, out TileCachePolyMesh mesh)
        {
            int maxVertices = 0;
            int maxTris = 0;
            int maxVertsPerCont = 0;
            for (int i = 0; i < lcset.nconts; ++i)
            {
                // Skip null contours.
                if (lcset.conts[i].nverts < 3) continue;
                maxVertices += lcset.conts[i].nverts;
                maxTris += lcset.conts[i].nverts - 2;
                maxVertsPerCont = Math.Max(maxVertsPerCont, lcset.conts[i].nverts);
            }

            // TODO: warn about too many vertices?

            mesh.nvp = VertsPerPolygon;

            byte[] vflags = new byte[maxVertices];

            mesh.verts = new int[maxVertices][];
            mesh.polys = new int[maxTris][];
            mesh.areas = new SamplePolyAreas[maxTris];
            mesh.flags = new SamplePolyFlags[maxTris];
            mesh.nverts = 0;
            mesh.npolys = 0;

            int[] firstVert = new int[VertexBucketCount2];
            for (int i = 0; i < VertexBucketCount2; ++i)
            {
                firstVert[i] = NullIdx;
            }

            int[] nextVert = new int[maxVertices];

            int[][] indices = new int[maxVertsPerCont][];

            int[][] tris = new int[maxVertsPerCont][];

            int[][] polys = new int[maxVertsPerCont][];

            for (int i = 0; i < lcset.nconts; ++i)
            {
                TileCacheContour cont = lcset.conts[i];

                // Skip null contours.
                if (cont.nverts < 3)
                {
                    continue;
                }

                // Triangulate contour
                for (int j = 0; j < cont.nverts; ++j)
                {
                    indices[j] = new[] { j };
                }

                int ntris = Triangulate(cont.nverts, cont.verts, out indices[0], out tris[0]);
                if (ntris <= 0)
                {
                    // TODO: issue warning!
                    ntris = -ntris;
                }

                // Add and merge vertices.
                for (int j = 0; j < cont.nverts; ++j)
                {
                    int[] v = cont.verts[j];
                    indices[j][0] = AddVertex(v[0], v[1], v[2], mesh.verts, firstVert, nextVert, mesh.nverts);
                    if ((v[3] & 0x80) != 0)
                    {
                        // This vertex should be removed.
                        vflags[indices[j][0]] = 1;
                    }
                }

                // Build initial polygons.
                int npolys = 0;
                polys = new int[maxVertsPerCont][];
                for (int j = 0; j < ntris; ++j)
                {
                    int[] t = tris[j];
                    if (t[0] != t[1] && t[0] != t[2] && t[1] != t[2])
                    {
                        polys[npolys][0] = indices[t[0]][0];
                        polys[npolys][1] = indices[t[1]][0];
                        polys[npolys][2] = indices[t[2]][0];
                        npolys++;
                    }
                }
                if (npolys == 0)
                {
                    continue;
                }

                // Merge polygons.
                int maxVertsPerPoly = VertsPerPolygon;
                if (maxVertsPerPoly > 3)
                {
                    for (; ; )
                    {
                        // Find best polygons to merge.
                        int bestMergeVal = 0;
                        int bestPa = 0, bestPb = 0, bestEa = 0, bestEb = 0;

                        for (int j = 0; j < npolys - 1; ++j)
                        {
                            int[] pj = polys[j];
                            for (int k = j + 1; k < npolys; ++k)
                            {
                                int[] pk = polys[k];
                                int ea, eb;
                                int v = GetPolyMergeValue(pj, pk, mesh.verts, out ea, out eb);
                                if (v > bestMergeVal)
                                {
                                    bestMergeVal = v;
                                    bestPa = j;
                                    bestPb = k;
                                    bestEa = ea;
                                    bestEb = eb;
                                }
                            }
                        }

                        if (bestMergeVal > 0)
                        {
                            // Found best, merge.
                            int[] pa = polys[bestPa];
                            int[] pb = polys[bestPb];
                            pa = MergePolys(pa, pb, bestEa, bestEb);
                            //memcpy(pb, &polys[(npolys - 1) * MAX_VERTS_PER_POLY], sizeof(unsigned short) * MAX_VERTS_PER_POLY);
                            npolys--;
                        }
                        else
                        {
                            // Could not merge any polygons, stop.
                            break;
                        }
                    }
                }

                // Store polygons.
                for (int j = 0; j < npolys; ++j)
                {
                    int[] p = mesh.polys[mesh.npolys * 2];
                    int[] q = polys[j];
                    for (int k = 0; k < VertsPerPolygon; ++k)
                    {
                        p[k] = q[k];
                    }
                    mesh.areas[mesh.npolys] = (SamplePolyAreas)(uint)cont.area;
                    mesh.npolys++;
                    if (mesh.npolys > maxTris)
                    {
                        return false;
                    }
                }
            }


            // Remove edge vertices.
            for (int i = 0; i < mesh.nverts; ++i)
            {
                if (vflags[i] != 0)
                {
                    if (!CanRemoveVertex(mesh, i))
                    {
                        continue;
                    }
                    if (!RemoveVertex(mesh, i, maxTris))
                    {
                        return false;
                    }
                    // Remove vertex
                    // Note: mesh.nverts is already decremented inside removeVertex()!
                    for (int j = i; j < mesh.nverts; ++j)
                    {
                        vflags[j] = vflags[j + 1];
                    }
                    --i;
                }
            }

            // Calculate adjacency.
            if (!BuildMeshAdjacency(mesh.polys, mesh.npolys, mesh.verts, mesh.nverts, lcset))
            {
                return false;
            }

            return true;
        }
        private static int Triangulate(int n, int[][] verts, out int[] indices, out int[] tris)
        {
            int ntris = 0;
            indices = null;
            tris = null;

            // The last bit of the index is used to indicate if the vertex can be removed.
            for (int i = 0; i < n; i++)
            {
                int i1 = Next(i, n);
                int i2 = Next(i1, n);
                if (Diagonal(i, i2, n, verts, indices))
                {
                    indices[i1] |= 0x8000;
                }
            }

            while (n > 3)
            {
                int minLen = -1;
                int mini = -1;
                for (int ix = 0; ix < n; ix++)
                {
                    int i1x = Next(ix, n);
                    if ((indices[i1x] & 0x8000) != 0)
                    {
                        int[] p0 = verts[(indices[ix] & 0x7fff)];
                        int[] p2 = verts[(indices[Next(i1x, n)] & 0x7fff)];

                        int dx = p2[0] - p0[0];
                        int dz = p2[2] - p0[2];
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
                    return -ntris;
                }

                int i = mini;
                int i1 = Next(i, n);
                int i2 = Next(i1, n);


                //dst++ = indices[i] & 0x7fff;
                //dst++ = indices[i1] & 0x7fff;
                //dst++ = indices[i2] & 0x7fff;
                ntris++;

                // Removes P[i1] by copying P[i+1]...P[n-1] left one index.
                n--;
                for (int k = i1; k < n; k++)
                    indices[k] = indices[k + 1];

                if (i1 >= n) i1 = 0;
                i = Prev(i1, n);
                // Update diagonal flags.
                if (Diagonal(Prev(i, n), i1, n, verts, indices))
                {
                    indices[i] |= 0x8000;
                }
                else
                {
                    indices[i] &= 0x7fff;
                }

                if (Diagonal(i, Next(i1, n), n, verts, indices))
                {
                    indices[i1] |= 0x8000;
                }
                else
                {
                    indices[i1] &= 0x7fff;
                }
            }

            // Append the remaining triangle.
            //dst++ = indices[0] & 0x7fff;
            //dst++ = indices[1] & 0x7fff;
            //dst++ = indices[2] & 0x7fff;
            ntris++;

            return ntris;
        }
        private static int Prev(int i, int n)
        {
            return i - 1 >= 0 ? i - 1 : n - 1;
        }
        private static int Next(int i, int n)
        {
            return i + 1 < n ? i + 1 : 0;
        }
        private static bool Diagonal(int i, int j, int n, int[][] verts, int[] indices)
        {
            return InCone(i, j, n, verts, indices) && Diagonalie(i, j, n, verts, indices);
        }
        private static bool InCone(int i, int j, int n, int[][] verts, int[] indices)
        {
            int[] pi = verts[(indices[i] & 0x7fff)];
            int[] pj = verts[(indices[j] & 0x7fff)];
            int[] pi1 = verts[(indices[Next(i, n)] & 0x7fff)];
            int[] pin1 = verts[(indices[Prev(i, n)] & 0x7fff)];

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn(pin1, pi, pi1))
            {
                return Left(pi, pj, pin1) && Left(pj, pi, pi1);
            }
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn(pi, pj, pi1) && LeftOn(pj, pi, pin1));
        }
        private static bool LeftOn(int[] a, int[] b, int[] c)
        {
            return Area2(a, b, c) <= 0;
        }
        private static int Area2(int[] a, int[] b, int[] c)
        {
            return (b[0] - a[0]) * (c[2] - a[2]) - (c[0] - a[0]) * (b[2] - a[2]);
        }
        private static bool Left(int[] a, int[] b, int[] c)
        {
            return Area2(a, b, c) < 0;
        }
        private static bool Diagonalie(int i, int j, int n, int[][] verts, int[] indices)
        {
            int[] d0 = verts[(indices[i] & 0x7fff)];
            int[] d1 = verts[(indices[j] & 0x7fff)];

            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = Next(k, n);
                // Skip edges incident to i or j
                if (!((k == i) || (k1 == i) || (k == j) || (k1 == j)))
                {
                    int[] p0 = verts[(indices[k] & 0x7fff)];
                    int[] p1 = verts[(indices[k1] & 0x7fff)];

                    if (Vequal(d0, p0) || Vequal(d1, p0) || Vequal(d0, p1) || Vequal(d1, p1))
                        continue;

                    if (Intersect(d0, d1, p0, p1))
                        return false;
                }
            }
            return true;
        }
        private static bool Vequal(int[] a, int[] b)
        {
            return a[0] == b[0] && a[2] == b[2];
        }
        /// <summary>
        /// Returns true iff segments ab and cd intersect, properly or improperly.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static bool Intersect(int[] a, int[] b, int[] c, int[] d)
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
        private static bool IntersectProp(int[] a, int[] b, int[] c, int[] d)
        {
            // Eliminate improper cases.
            if (Collinear(a, b, c) || Collinear(a, b, d) ||
                Collinear(c, d, a) || Collinear(c, d, b))
                return false;

            return Xorb(Left(a, b, c), Left(a, b, d)) && Xorb(Left(c, d, a), Left(c, d, b));
        }
        private static bool Collinear(int[] a, int[] b, int[] c)
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
        private static bool Xorb(bool x, bool y)
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
        private static bool Between(int[] a, int[] b, int[] c)
        {
            if (!Collinear(a, b, c))
                return false;
            // If ab not vertical, check betweenness on x; else on y.
            if (a[0] != b[0])
                return ((a[0] <= c[0]) && (c[0] <= b[0])) || ((a[0] >= c[0]) && (c[0] >= b[0]));
            else
                return ((a[2] <= c[2]) && (c[2] <= b[2])) || ((a[2] >= c[2]) && (c[2] >= b[2]));
        }
        private static int AddVertex(int x, int y, int z, int[][] verts, int[] firstVert, int[] nextVert, int nv)
        {
            int bucket = ComputeVertexHash2(x, 0, z);
            int i = firstVert[bucket];

            while (i != NullIdx)
            {
                int[] vx = verts[i];
                if (vx[0] == x && vx[2] == z && (Math.Abs(vx[1] - y) <= 2))
                {
                    return i;
                }
                i = nextVert[i]; // next
            }

            // Could not find, create new.
            i = nv; nv++;
            int[] v = verts[i];
            v[0] = x;
            v[1] = y;
            v[2] = z;
            nextVert[i] = firstVert[bucket];
            firstVert[bucket] = i;

            return i;
        }
        private static int ComputeVertexHash2(int x, int y, int z)
        {
            uint h1 = 0x8da6b343; // Large multiplicative constants;
            uint h2 = 0xd8163841; // here arbitrarily chosen primes
            uint h3 = 0xcb1ab31f;
            uint n = (uint)(h1 * x + h2 * y + h3 * z);
            return (int)(n & (VertexBucketCount2 - 1));
        }
        private static int GetPolyMergeValue(int[] pa, int[] pb, int[][] verts, out int ea, out int eb)
        {
            ea = -1;
            eb = -1;

            int na = CountPolyVerts(pa);
            int nb = CountPolyVerts(pb);

            // If the merged polygon would be too big, do not merge.
            if (na + nb - 2 > VertsPerPolygon)
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
        private static int CountPolyVerts(int[] p)
        {
            for (int i = 0; i < VertsPerPolygon; ++i)
            {
                if (p[i] == NullIdx)
                {
                    return i;
                }
            }

            return VertsPerPolygon;
        }
        private static bool Uleft(int[] a, int[] b, int[] c)
        {
            return (b[0] - a[0]) * (c[2] - a[2]) - (c[0] - a[0]) * (b[2] - a[2]) < 0;
        }
        private static int[] MergePolys(int[] pa, int[] pb, int ea, int eb)
        {
            int[] tmp = new int[VertsPerPolygon * 2];

            int na = CountPolyVerts(pa);
            int nb = CountPolyVerts(pb);

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
        private static bool CanRemoveVertex(TileCachePolyMesh mesh, int rem)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = 0;
            int numTouchedVerts = 0;
            int numRemainingEdges = 0;
            for (int i = 0; i < mesh.npolys; ++i)
            {
                int[] p = mesh.polys[i * VertsPerPolygon * 2];
                int nv = CountPolyVerts(p);
                int numRemoved = 0;
                int numVerts = 0;
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] == rem)
                    {
                        numTouchedVerts++;
                        numRemoved++;
                    }
                    numVerts++;
                }
                if (numRemoved != 0)
                {
                    numRemovedVerts += numRemoved;
                    numRemainingEdges += numVerts - (numRemoved + 1);
                }
            }

            // There would be too few edges remaining to create a polygon.
            // This can happen for example when a tip of a triangle is marked
            // as deletion, but there are no other polys that share the vertex.
            // In this case, the vertex should not be removed.
            if (numRemainingEdges <= 2)
            {
                return false;
            }

            // Check that there is enough memory for the test.
            int maxEdges = numTouchedVerts * 2;
            if (maxEdges > MaxRemEdges)
            {
                return false;
            }

            // Find edges which share the removed vertex.
            int[][] edges = new int[MaxRemEdges][];
            int nedges = 0;

            for (int i = 0; i < mesh.npolys; ++i)
            {
                int[] p = mesh.polys[i * VertsPerPolygon * 2];
                int nv = CountPolyVerts(p);

                // Collect edges which touches the removed vertex.
                for (int j = 0, k = nv - 1; j < nv; k = j++)
                {
                    if (p[j] == rem || p[k] == rem)
                    {
                        // Arrange edge so that a=rem.
                        int a = p[j], b = p[k];
                        if (b == rem)
                        {
                            Helper.Swap(ref a, ref b);
                        }

                        // Check if the edge exists
                        bool exists = false;
                        for (int m = 0; m < nedges; ++m)
                        {
                            int[] e = edges[m];
                            if (e[1] == b)
                            {
                                // Exists, increment vertex share count.
                                e[2]++;
                                exists = true;
                            }
                        }
                        // Add new edge.
                        if (!exists)
                        {
                            int[] e = edges[nedges];
                            e[0] = a;
                            e[1] = b;
                            e[2] = 1;
                            nedges++;
                        }
                    }
                }
            }

            // There should be no more than 2 open edges.
            // This catches the case that two non-adjacent polygons
            // share the removed vertex. In that case, do not remove the vertex.
            int numOpenEdges = 0;
            for (int i = 0; i < nedges; ++i)
            {
                if (edges[i][2] < 2)
                {
                    numOpenEdges++;
                }
            }
            if (numOpenEdges > 2)
            {
                return false;
            }

            return true;
        }
        private static bool RemoveVertex(TileCachePolyMesh mesh, int rem, int maxTris)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = 0;
            for (int i = 0; i < mesh.npolys; ++i)
            {
                int[] p = mesh.polys[i * VertsPerPolygon * 2];
                int nv = CountPolyVerts(p);
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] == rem)
                        numRemovedVerts++;
                }
            }

            int nedges = 0;
            int[][] edges = new int[MaxRemEdges * 3][];
            int nhole = 0;
            int[] hole = new int[MaxRemEdges];
            int nharea = 0;
            int[] harea = new int[MaxRemEdges];

            for (int i = 0; i < mesh.npolys; ++i)
            {
                int[] p = mesh.polys[i * VertsPerPolygon * 2];
                int nv = CountPolyVerts(p);
                bool hasRem = false;
                for (int j = 0; j < nv; ++j)
                    if (p[j] == rem) hasRem = true;
                if (hasRem)
                {
                    // Collect edges which does not touch the removed vertex.
                    for (int j = 0, k = nv - 1; j < nv; k = j++)
                    {
                        if (p[j] != rem && p[k] != rem)
                        {
                            if (nedges >= MaxRemEdges)
                            {
                                return false;
                            }
                            int[] e = edges[nedges];
                            e[0] = p[k];
                            e[1] = p[j];
                            e[2] = (int)mesh.areas[i];
                            nedges++;
                        }
                    }
                    // Remove the polygon.
                    int[] p2 = mesh.polys[(mesh.npolys - 1) * VertsPerPolygon * 2];
                    //memcpy(p, p2, sizeof(unsigned short) * MAX_VERTS_PER_POLY);
                    //memset(p + MAX_VERTS_PER_POLY, 0xff, sizeof(unsigned short) * MAX_VERTS_PER_POLY);
                    mesh.areas[i] = mesh.areas[mesh.npolys - 1];
                    mesh.npolys--;
                    --i;
                }
            }

            // Remove vertex.
            for (int i = rem; i < mesh.nverts; ++i)
            {
                mesh.verts[i][0] = mesh.verts[(i + 1)][0];
                mesh.verts[i][1] = mesh.verts[(i + 1)][1];
                mesh.verts[i][2] = mesh.verts[(i + 1)][2];
            }
            mesh.nverts--;

            // Adjust indices to match the removed vertex layout.
            for (int i = 0; i < mesh.npolys; ++i)
            {
                int[] p = mesh.polys[i * VertsPerPolygon * 2];
                int nv = CountPolyVerts(p);
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] > rem) p[j]--;
                }
            }
            for (int i = 0; i < nedges; ++i)
            {
                if (edges[i][0] > rem) edges[i][0]--;
                if (edges[i][1] > rem) edges[i][1]--;
            }

            if (nedges == 0)
            {
                return true;
            }

            // Start with one vertex, keep appending connected
            // segments to the start and end of the hole.
            PushBack(edges[0][0], hole, nhole);
            PushBack(edges[2][0], harea, nharea);

            while (nedges != 0)
            {
                bool match = false;

                for (int i = 0; i < nedges; ++i)
                {
                    int ea = edges[i][0];
                    int eb = edges[i][1];
                    int a = edges[i][2];
                    bool add = false;
                    if (hole[0] == eb)
                    {
                        // The segment matches the beginning of the hole boundary.
                        if (nhole >= MaxRemEdges)
                        {
                            return false;
                        }
                        PushFront(ea, hole, nhole);
                        PushFront(a, harea, nharea);
                        add = true;
                    }
                    else if (hole[nhole - 1] == ea)
                    {
                        // The segment matches the end of the hole boundary.
                        if (nhole >= MaxRemEdges)
                        {
                            return false;
                        }
                        PushBack(eb, hole, nhole);
                        PushBack(a, harea, nharea);
                        add = true;
                    }
                    if (add)
                    {
                        // The edge segment was added, remove it.
                        edges[i][0] = edges[(nedges - 1)][0];
                        edges[i][1] = edges[(nedges - 1)][1];
                        edges[i][2] = edges[(nedges - 1)][2];
                        --nedges;
                        match = true;
                        --i;
                    }
                }

                if (!match)
                {
                    break;
                }
            }


            int[][] tris = new int[MaxRemEdges][];
            int[][] tverts = new int[MaxRemEdges][];
            int[] tpoly = new int[MaxRemEdges];

            // Generate temp vertex array for triangulation.
            for (int i = 0; i < nhole; ++i)
            {
                int pi = hole[i];
                tverts[i][0] = mesh.verts[pi][0];
                tverts[i][1] = mesh.verts[pi][1];
                tverts[i][2] = mesh.verts[pi][2];
                tverts[i][3] = 0;
                tpoly[i] = i;
            }

            // Triangulate the hole.
            int ntris = Triangulate(nhole, tverts, out tpoly, out tris[0]);
            if (ntris < 0)
            {
                // TODO: issue warning!
                ntris = -ntris;
            }

            if (ntris > MaxRemEdges)
            {
                return false;
            }

            int[][] polys = new int[MaxRemEdges][];
            int[] pareas = new int[MaxRemEdges];

            // Build initial polygons.
            int npolys = 0;
            for (int j = 0; j < ntris; ++j)
            {
                int[] t = tris[j];
                if (t[0] != t[1] && t[0] != t[2] && t[1] != t[2])
                {
                    polys[npolys][0] = hole[t[0]];
                    polys[npolys][1] = hole[t[1]];
                    polys[npolys][2] = hole[t[2]];
                    pareas[npolys] = harea[t[0]];
                    npolys++;
                }
            }
            if (npolys == 0)
            {
                return true;
            }

            // Merge polygons.
            int maxVertsPerPoly = VertsPerPolygon;
            if (maxVertsPerPoly > 3)
            {
                for (; ; )
                {
                    // Find best polygons to merge.
                    int bestMergeVal = 0;
                    int bestPa = 0, bestPb = 0, bestEa = 0, bestEb = 0;

                    for (int j = 0; j < npolys - 1; ++j)
                    {
                        int[] pj = polys[j];
                        for (int k = j + 1; k < npolys; ++k)
                        {
                            int[] pk = polys[k];
                            int ea, eb;
                            int v = GetPolyMergeValue(pj, pk, mesh.verts, out ea, out eb);
                            if (v > bestMergeVal)
                            {
                                bestMergeVal = v;
                                bestPa = j;
                                bestPb = k;
                                bestEa = ea;
                                bestEb = eb;
                            }
                        }
                    }

                    if (bestMergeVal > 0)
                    {
                        // Found best, merge.
                        int[] pa = polys[bestPa];
                        int[] pb = polys[bestPb];
                        pa = MergePolys(pa, pb, bestEa, bestEb);
                        //memcpy(pb, &polys[(npolys - 1) * MAX_VERTS_PER_POLY], sizeof(unsigned short) * MAX_VERTS_PER_POLY);
                        pareas[bestPb] = pareas[npolys - 1];
                        npolys--;
                    }
                    else
                    {
                        // Could not merge any polygons, stop.
                        break;
                    }
                }
            }

            // Store polygons.
            for (int i = 0; i < npolys; ++i)
            {
                if (mesh.npolys >= maxTris) break;
                int[] p = mesh.polys[mesh.npolys * 2];
                //memset(p, 0xff, sizeof(unsigned short) * MAX_VERTS_PER_POLY * 2);
                for (int j = 0; j < VertsPerPolygon; ++j)
                {
                    p[j] = polys[i][j];
                }
                mesh.areas[mesh.npolys] = (SamplePolyAreas)pareas[i];
                mesh.npolys++;
                if (mesh.npolys > maxTris)
                {
                    return false;
                }
            }

            return true;
        }
        private static void PushFront(int v, int[] arr, int an)
        {
            an++;
            for (int i = an - 1; i > 0; --i)
                arr[i] = arr[i - 1];
            arr[0] = v;
        }
        private static void PushBack(int v, int[] arr, int an)
        {
            arr[an] = v;
            an++;
        }
        private static bool BuildMeshAdjacency(int[][] polys, int npolys, int[][] verts, int nverts, TileCacheContourSet lcset)
        {
            // Based on code by Eric Lengyel from:
            // http://www.terathon.com/code/edges.php

            int maxEdgeCount = npolys * VertsPerPolygon;
            int[] firstEdge = new int[nverts + maxEdgeCount];
            int[] nextEdge = new int[maxEdgeCount];
            int edgeCount = 0;

            Edge[] edges = new Edge[maxEdgeCount];

            for (int i = 0; i < nverts; i++)
            {
                firstEdge[i] = NullIdx;
            }

            for (int i = 0; i < npolys; ++i)
            {
                int[] t = polys[i * VertsPerPolygon * 2];
                for (int j = 0; j < VertsPerPolygon; ++j)
                {
                    if (t[j] == NullIdx) break;
                    int v0 = t[j];
                    int v1 = (j + 1 >= VertsPerPolygon || t[j + 1] == NullIdx) ? t[0] : t[j + 1];
                    if (v0 < v1)
                    {
                        Edge edge = edges[edgeCount];
                        edge.vert[0] = v0;
                        edge.vert[1] = v1;
                        edge.poly[0] = i;
                        edge.polyEdge[0] = j;
                        edge.poly[1] = i;
                        edge.polyEdge[1] = 0xff;
                        // Insert edge
                        nextEdge[edgeCount] = firstEdge[v0];
                        firstEdge[v0] = edgeCount;
                        edgeCount++;
                    }
                }
            }

            for (int i = 0; i < npolys; ++i)
            {
                int[] t = polys[i * VertsPerPolygon * 2];
                for (int j = 0; j < VertsPerPolygon; ++j)
                {
                    if (t[j] == NullIdx) break;
                    int v0 = t[j];
                    int v1 = (j + 1 >= VertsPerPolygon || t[j + 1] == NullIdx) ? t[0] : t[j + 1];
                    if (v0 > v1)
                    {
                        bool found = false;
                        for (int e = firstEdge[v1]; e != NullIdx; e = nextEdge[e])
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
                            Edge edge = edges[edgeCount];
                            edge.vert[0] = v1;
                            edge.vert[1] = v0;
                            edge.poly[0] = i;
                            edge.polyEdge[0] = j;
                            edge.poly[1] = i;
                            edge.polyEdge[1] = 0xff;
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
                    int[] va = cont.verts[k];
                    int[] vb = cont.verts[j];
                    int dir = va[3] & 0xf;
                    if (dir == 0xf)
                    {
                        continue;
                    }

                    if (dir == 0 || dir == 2)
                    {
                        // Find matching vertical edge
                        int x = va[0];
                        int zmin = va[2];
                        int zmax = vb[2];
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
                            int[] eva = verts[e.vert[0]];
                            int[] evb = verts[e.vert[1]];
                            if (eva[0] == x && evb[0] == x)
                            {
                                int ezmin = eva[2];
                                int ezmax = evb[2];
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
                        int z = va[2];
                        int xmin = va[0];
                        int xmax = vb[0];
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
                            int[] eva = verts[e.vert[0]];
                            int[] evb = verts[e.vert[1]];
                            if (eva[2] == z && evb[2] == z)
                            {
                                int exmin = eva[0];
                                int exmax = evb[0];
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
                    int[] p0 = polys[e.poly[0]];
                    int[] p1 = polys[e.poly[1]];
                    p0[e.polyEdge[0]] = e.poly[1];
                    p1[e.polyEdge[1]] = e.poly[0];
                }
                else if (e.polyEdge[1] != 0xff)
                {
                    int[] p0 = polys[e.poly[0]];
                    p0[e.polyEdge[0]] = 0x8000 | e.polyEdge[1];
                }
            }

            return true;
        }
        private static bool OverlapRangeExl(int amin, int amax, int bmin, int bmax)
        {
            return (amin >= bmax || amax <= bmin) ? false : true;
        }




        private bool CreateNavMeshData(NavMeshCreateParams param, out byte[] navData, out int navDataSize)
        {
            throw new NotImplementedException();
        }
    }
}
