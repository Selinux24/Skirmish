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

        public bool BuildNavMeshTilesAt(int tx, int ty, NavigationMesh2 navmesh)
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
        private bool BuildNavMeshTile(CompressedTile tile, NavigationMesh2 navmesh)
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
                var ob = m_obstacles[i];

                if (ob.state == ObstacleState.Empty || ob.state == ObstacleState.Removing)
                {
                    continue;
                }

                if (Contains(ob.touched, ob.ntouched, tile))
                {
                    if (ob.type == ObstacleType.Cylinder)
                    {
                        MarkCylinderArea(ref bc.layer,
                            tile.Header.b.Minimum, m_params.CellSize, m_params.CellHeight,
                            ob.cylinder.pos, ob.cylinder.radius, ob.cylinder.height, 0);
                    }
                    else if (ob.type == ObstacleType.Box)
                    {
                        MarkBoxArea(ref bc.layer,
                            tile.Header.b.Minimum, m_params.CellSize, m_params.CellHeight,
                            ob.box.bmin, ob.box.bmax, 0);
                    }
                    else if (ob.type == ObstacleType.OrientedBox)
                    {
                        MarkBoxArea(ref bc.layer,
                            tile.Header.b.Minimum, m_params.CellSize, m_params.CellHeight,
                            ob.orientedBox.center, ob.orientedBox.halfExtents, ob.orientedBox.rotAux, 0);
                    }
                }
            }

            // Build navmesh
            if (!BuildTileCacheRegions(ref bc.layer, walkableClimbVx))
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
                navmesh.RemoveTile(navmesh.GetTileRefAt(tile.Header.tx, tile.Header.ty, tile.Header.tlayer), null, 0);
                return true;
            }

            var param = new NavMeshCreateParams
            {
                verts = bc.lmesh.verts,
                vertCount = bc.lmesh.nverts,
                polys = bc.lmesh.polys,
                polyAreas = bc.lmesh.areas,
                polyFlags = bc.lmesh.flags,
                polyCount = bc.lmesh.npolys,
                nvp = Constants.VertsPerPolygon,
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

            if (!CreateNavMeshData(param, out MeshData navData))
            {
                return false;
            }

            // Remove existing tile.
            navmesh.RemoveTile(navmesh.GetTileRefAt(tile.Header.tx, tile.Header.ty, tile.Header.tlayer), null, 0);

            // Add new tile, or leave the location empty.
            if (navData != null)
            {
                // Let the navmesh own the data.
                int lastRef = 0;
                if (!navmesh.AddTile(navData, TileFlags.FreeData, ref lastRef, out int result))
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
        private bool MarkBoxArea(ref TileCacheLayer layer, Vector3 orig, float cs, float ch, Vector3 center, Vector3 halfExtents, Vector2 rotAux, TileCacheAreas areaId)
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
        private bool MarkBoxArea(ref TileCacheLayer layer, Vector3 orig, float cs, float ch, Vector3 bmin, Vector3 bmax, TileCacheAreas areaId)
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
        private bool MarkCylinderArea(ref TileCacheLayer layer, Vector3 orig, float cs, float ch, Vector3 pos, float radius, float height, TileCacheAreas areaId)
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
        private bool BuildTileCacheRegions(ref TileCacheLayer layer, int walkableClimb)
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
            LayerMonotoneRegion[] regs = Helper.CreateArray(nregs, LayerMonotoneRegion.CreateEmpty());

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

            lcset = new TileCacheContourSet
            {
                nconts = layer.regCount,
                conts = new TileCacheContour[layer.regCount],
            };

            // Allocate temp buffer for contour tracing.
            int maxTempVerts = (w + h) * 2 * 2; // Twice around the layer.

            var tempVerts = new Trianglei[maxTempVerts];
            var tempPoly = new Polygoni(maxTempVerts);

            var temp = new TempContour(tempVerts, maxTempVerts, tempPoly, maxTempVerts);

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

                    var cont = lcset.conts[ri];

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
                        cont.verts = new Trianglei[temp.nverts];

                        for (int i = 0, j = temp.nverts - 1; i < temp.nverts; j = i++)
                        {
                            var v = temp.verts[j];
                            var vn = temp.verts[i];
                            int nei = vn.R; // The neighbour reg is stored at segment vertex of a segment. 
                            bool shouldRemove = false;
                            int lh = GetCornerHeight(layer, v.X, v.Y, v.Z, walkableClimb, ref shouldRemove);

                            var dst = new Trianglei()
                            {
                                X = v.X,
                                Y = lh,
                                Z = v.Z,
                            };

                            // Store portal direction and remove status to the fourth component.
                            dst.R = 0x0f;
                            if (nei != 0xff && nei >= 0xf8)
                            {
                                dst.R = nei - 0xf8;
                            }
                            if (shouldRemove)
                            {
                                dst.R |= 0x80;
                            }

                            cont.verts[j] = dst;
                        }
                    }

                    lcset.conts[ri] = cont;
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
            var pa = cont.verts[cont.nverts - 1];
            var pb = cont.verts[0];
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
                var pa = cont.verts[cont.nverts - 2];
                var pb = cont.verts[cont.nverts - 1];
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

            cont.verts[cont.nverts] = new Trianglei(x, y, z, r);
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
                int ra = cont.verts[j].R;
                int rb = cont.verts[i].R;
                if (ra != rb)
                {
                    cont.poly[cont.npoly++] = i;
                }
            }
            if (cont.npoly < 2)
            {
                // If there is no transitions at all,
                // create some initial points for the simplification process. 
                // Find lower-left and upper-right vertices of the contour.
                int llx = cont.verts[0].X;
                int llz = cont.verts[0].Z;
                int lli = 0;
                int urx = cont.verts[0].X;
                int urz = cont.verts[0].Z;
                int uri = 0;
                for (int i = 1; i < cont.nverts; ++i)
                {
                    int x = cont.verts[i].X;
                    int z = cont.verts[i].Z;
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
                cont.poly[cont.npoly++] = lli;
                cont.poly[cont.npoly++] = uri;
            }

            // Add points until all raw points are within
            // error tolerance to the simplified shape.
            for (int i = 0; i < cont.npoly;)
            {
                int ii = (i + 1) % cont.npoly;

                int ai = cont.poly[i];
                int ax = cont.verts[ai].X;
                int az = cont.verts[ai].Z;

                int bi = cont.poly[ii];
                int bx = cont.verts[bi].X;
                int bz = cont.verts[bi].Z;

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
                    float d = DistancePtSeg(cont.verts[ci].X, cont.verts[ci].Z, ax, az, bx, bz);
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
                    cont.poly[i + 1] = maxi;
                }
                else
                {
                    i++;
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
                var src = cont.verts[cont.poly[j]];
                cont.verts[cont.nverts++] = new Trianglei()
                {
                    X = src.X,
                    Y = src.Y,
                    Z = src.Z,
                    R = src.R,
                };
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

            mesh = new TileCachePolyMesh
            {
                nvp = Constants.VertsPerPolygon,
                verts = Helper.CreateArray(maxVertices, () => new Trianglei()),
                polys = new Polygoni[maxTris],
                areas = new SamplePolyAreas[maxTris],
                flags = new SamplePolyFlags[maxTris],
                nverts = 0,
                npolys = 0
            };

            byte[] vflags = new byte[maxVertices];

            var firstVert = new Polygoni(VertexBucketCount2);
            for (int i = 0; i < VertexBucketCount2; ++i)
            {
                firstVert[i] = Constants.NullIdx;
            }

            var nextVert = new Polygoni(maxVertices);
            var indices = new int[maxVertsPerCont];
            var polys = new Polygoni[maxVertsPerCont];

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
                    indices[j] = j;
                }

                int ntris = Triangulate(cont.nverts, cont.verts, ref indices, out Trianglei[] tris);
                if (ntris <= 0)
                {
                    // TODO: issue warning!
                    ntris = -ntris;
                }

                // Add and merge vertices.
                for (int j = 0; j < cont.nverts; ++j)
                {
                    var v = cont.verts[j];
                    indices[j] = AddVertex(v.X, v.Y, v.Z, mesh.verts, firstVert, nextVert, ref mesh.nverts);
                    if ((v[3] & 0x80) != 0)
                    {
                        // This vertex should be removed.
                        vflags[indices[j]] = 1;
                    }
                }

                // Build initial polygons.
                int npolys = 0;
                polys = new Polygoni[maxVertsPerCont];
                for (int j = 0; j < ntris; ++j)
                {
                    var t = tris[j];
                    if (t.X != t.Y && t.X != t.Z && t.Y != t.Z)
                    {
                        polys[npolys] = new Polygoni(Constants.VertsPerPolygon);
                        polys[npolys][0] = indices[t[0]];
                        polys[npolys][1] = indices[t[1]];
                        polys[npolys][2] = indices[t[2]];
                        npolys++;
                    }
                }
                if (npolys == 0)
                {
                    continue;
                }

                // Merge polygons.
                int maxVertsPerPoly = Constants.VertsPerPolygon;
                if (maxVertsPerPoly > 3)
                {
                    for (; ; )
                    {
                        // Find best polygons to merge.
                        int bestMergeVal = 0;
                        int bestPa = 0, bestPb = 0, bestEa = 0, bestEb = 0;

                        for (int j = 0; j < npolys - 1; ++j)
                        {
                            var pj = polys[j];
                            for (int k = j + 1; k < npolys; ++k)
                            {
                                var pk = polys[k];
                                int v = GetPolyMergeValue(pj, pk, mesh.verts, out int ea, out int eb);
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
                            polys[bestPa] = MergePolys(polys[bestPa], polys[bestPb], bestEa, bestEb);
                            polys[bestPb] = polys[npolys - 1].Copy();
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
                    var p = new Polygoni(Constants.VertsPerPolygon * 2);
                    var q = polys[j];
                    for (int k = 0; k < Constants.VertsPerPolygon; ++k)
                    {
                        p[k] = q[k];
                    }
                    mesh.polys[mesh.npolys] = p;
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
        private static int Triangulate(int n, Trianglei[] verts, ref int[] indices, out Trianglei[] tris)
        {
            int ntris = 0;

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

            List<Trianglei> dst = new List<Trianglei>();

            while (n > 3)
            {
                int minLen = -1;
                int mini = -1;
                for (int ix = 0; ix < n; ix++)
                {
                    int i1x = Next(ix, n);
                    if ((indices[i1x] & 0x8000) != 0)
                    {
                        var p0 = verts[(indices[ix] & 0x7fff)];
                        var p2 = verts[(indices[Next(i1x, n)] & 0x7fff)];

                        int dx = p2.X - p0.X;
                        int dz = p2.Z - p0.Z;
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
                    tris = null;
                    return -ntris;
                }

                int i = mini;
                int i1 = Next(i, n);
                int i2 = Next(i1, n);

                dst.Add(new Trianglei()
                {
                    X = indices[i] & 0x7fff,
                    Y = indices[i1] & 0x7fff,
                    Z = indices[i2] & 0x7fff
                });
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
            dst.Add(new Trianglei
            {
                X = indices[0] & 0x7fff,
                Y = indices[1] & 0x7fff,
                Z = indices[2] & 0x7fff,
            });
            ntris++;

            tris = dst.ToArray();
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
        private static bool Diagonal(int i, int j, int n, Trianglei[] verts, int[] indices)
        {
            return InCone(i, j, n, verts, indices) && Diagonalie(i, j, n, verts, indices);
        }
        private static bool InCone(int i, int j, int n, Trianglei[] verts, int[] indices)
        {
            var pi = verts[(indices[i] & 0x7fff)];
            var pj = verts[(indices[j] & 0x7fff)];
            var pi1 = verts[(indices[Next(i, n)] & 0x7fff)];
            var pin1 = verts[(indices[Prev(i, n)] & 0x7fff)];

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn(pin1, pi, pi1))
            {
                return Left(pi, pj, pin1) && Left(pj, pi, pi1);
            }
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn(pi, pj, pi1) && LeftOn(pj, pi, pin1));
        }
        private static bool LeftOn(Trianglei a, Trianglei b, Trianglei c)
        {
            return Area2(a, b, c) <= 0;
        }
        private static int Area2(Trianglei a, Trianglei b, Trianglei c)
        {
            return (b.X - a.X) * (c.Z - a.Z) - (c.X - a.X) * (b.Z - a.Z);
        }
        private static bool Left(Trianglei a, Trianglei b, Trianglei c)
        {
            return Area2(a, b, c) < 0;
        }
        private static bool Diagonalie(int i, int j, int n, Trianglei[] verts, int[] indices)
        {
            var d0 = verts[(indices[i] & 0x7fff)];
            var d1 = verts[(indices[j] & 0x7fff)];

            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = Next(k, n);
                // Skip edges incident to i or j
                if (!((k == i) || (k1 == i) || (k == j) || (k1 == j)))
                {
                    var p0 = verts[(indices[k] & 0x7fff)];
                    var p1 = verts[(indices[k1] & 0x7fff)];

                    if (Vequal(d0, p0) || Vequal(d1, p0) || Vequal(d0, p1) || Vequal(d1, p1))
                        continue;

                    if (Intersect(d0, d1, p0, p1))
                        return false;
                }
            }
            return true;
        }
        private static bool Vequal(Trianglei a, Trianglei b)
        {
            return a.X == b.X && a.Z == b.Z;
        }
        /// <summary>
        /// Returns true iff segments ab and cd intersect, properly or improperly.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static bool Intersect(Trianglei a, Trianglei b, Trianglei c, Trianglei d)
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
        private static bool IntersectProp(Trianglei a, Trianglei b, Trianglei c, Trianglei d)
        {
            // Eliminate improper cases.
            if (Collinear(a, b, c) || Collinear(a, b, d) ||
                Collinear(c, d, a) || Collinear(c, d, b))
                return false;

            return Xorb(Left(a, b, c), Left(a, b, d)) && Xorb(Left(c, d, a), Left(c, d, b));
        }
        private static bool Collinear(Trianglei a, Trianglei b, Trianglei c)
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
        private static bool Between(Trianglei a, Trianglei b, Trianglei c)
        {
            if (!Collinear(a, b, c))
                return false;
            // If ab not vertical, check betweenness on x; else on y.
            if (a.X != b.X)
                return ((a.X <= c.X) && (c.X <= b.X)) || ((a.X >= c.X) && (c.X >= b.X));
            else
                return ((a.Z <= c.Z) && (c.Z <= b.Z)) || ((a.Z >= c.Z) && (c.Z >= b.Z));
        }
        private static int AddVertex(int x, int y, int z, Trianglei[] verts, Polygoni firstVert, Polygoni nextVert, ref int nv)
        {
            int bucket = ComputeVertexHash2(x, 0, z);
            int i = firstVert[bucket];

            while (i != Constants.NullIdx)
            {
                var vx = verts[i];
                if (vx.X == x && vx.Z == z && (Math.Abs(vx.Y - y) <= 2))
                {
                    return i;
                }
                i = nextVert[i]; // next
            }

            // Could not find, create new.
            i = nv; nv++;
            var v = new Trianglei();
            v[0] = x;
            v[1] = y;
            v[2] = z;
            verts[i] = v;
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
        private static int GetPolyMergeValue(Polygoni pa, Polygoni pb, Trianglei[] verts, out int ea, out int eb)
        {
            ea = -1;
            eb = -1;

            int na = CountPolyVerts(pa);
            int nb = CountPolyVerts(pb);

            // If the merged polygon would be too big, do not merge.
            if (na + nb - 2 > Constants.VertsPerPolygon)
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
        private static int CountPolyVerts(Polygoni p)
        {
            for (int i = 0; i < Constants.VertsPerPolygon; ++i)
            {
                if (p[i] == Constants.NullIdx)
                {
                    return i;
                }
            }

            return Constants.VertsPerPolygon;
        }
        private static bool Uleft(Trianglei a, Trianglei b, Trianglei c)
        {
            return (b.X - a.X) * (c.Z - a.Z) - (c.X - a.X) * (b.Z - a.Z) < 0;
        }
        private static Polygoni MergePolys(Polygoni pa, Polygoni pb, int ea, int eb)
        {
            var tmp = new Polygoni(Constants.VertsPerPolygon);

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
                var p = mesh.polys[i * Constants.VertsPerPolygon * 2];
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
                var p = mesh.polys[i * Constants.VertsPerPolygon * 2];
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
                var p = mesh.polys[i * Constants.VertsPerPolygon * 2];
                int nv = CountPolyVerts(p);
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] == rem)
                        numRemovedVerts++;
                }
            }

            int nedges = 0;
            List<int>[] edges = new List<int>[MaxRemEdges * 3];
            int nhole = 0;
            int[] hole = new int[MaxRemEdges];
            int nharea = 0;
            int[] harea = new int[MaxRemEdges];

            for (int i = 0; i < mesh.npolys; ++i)
            {
                var p = mesh.polys[i * Constants.VertsPerPolygon * 2];
                int nv = CountPolyVerts(p);
                bool hasRem = false;
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] == rem) hasRem = true;
                }
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
                            var e = edges[nedges];
                            e[0] = p[k];
                            e[1] = p[j];
                            e[2] = (int)mesh.areas[i];
                            nedges++;
                        }
                    }
                    // Remove the polygon.
                    var p2 = mesh.polys[(mesh.npolys - 1) * Constants.VertsPerPolygon * 2];
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
                var p = mesh.polys[i * Constants.VertsPerPolygon * 2];
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


            var tverts = new Trianglei[MaxRemEdges];
            var tpoly = new int[MaxRemEdges];

            // Generate temp vertex array for triangulation.
            for (int i = 0; i < nhole; ++i)
            {
                int pi = hole[i];
                tverts[i].X = mesh.verts[pi].X;
                tverts[i].Y = mesh.verts[pi].Y;
                tverts[i].Z = mesh.verts[pi].Z;
                tverts[i].R = 0;
                tpoly[i] = i;
            }

            // Triangulate the hole.
            int ntris = Triangulate(nhole, tverts, ref tpoly, out Trianglei[] tris);
            if (ntris < 0)
            {
                // TODO: issue warning!
                ntris = -ntris;
            }

            if (ntris > MaxRemEdges)
            {
                return false;
            }

            Polygoni[] polys = new Polygoni[MaxRemEdges];
            int[] pareas = new int[MaxRemEdges];

            // Build initial polygons.
            int npolys = 0;
            for (int j = 0; j < ntris; ++j)
            {
                var t = tris[j];
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
            int maxVertsPerPoly = Constants.VertsPerPolygon;
            if (maxVertsPerPoly > 3)
            {
                for (; ; )
                {
                    // Find best polygons to merge.
                    int bestMergeVal = 0;
                    int bestPa = 0, bestPb = 0, bestEa = 0, bestEb = 0;

                    for (int j = 0; j < npolys - 1; ++j)
                    {
                        var pj = polys[j];
                        for (int k = j + 1; k < npolys; ++k)
                        {
                            var pk = polys[k];
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
                        var pa = polys[bestPa];
                        var pb = polys[bestPb];
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
                var p = mesh.polys[mesh.npolys * 2];
                //memset(p, 0xff, sizeof(unsigned short) * MAX_VERTS_PER_POLY * 2);
                for (int j = 0; j < Constants.VertsPerPolygon; ++j)
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
        private static bool BuildMeshAdjacency(Polygoni[] polys, int npolys, Trianglei[] verts, int nverts, TileCacheContourSet lcset)
        {
            // Based on code by Eric Lengyel from:
            // http://www.terathon.com/code/edges.php

            int maxEdgeCount = npolys * Constants.VertsPerPolygon;
            int[] firstEdge = new int[nverts];
            int[] nextEdge = new int[maxEdgeCount];
            int edgeCount = 0;

            Edge[] edges = new Edge[maxEdgeCount];

            for (int i = 0; i < nverts; i++)
            {
                firstEdge[i] = Constants.NullIdx;
            }
            for (int i = 0; i < maxEdgeCount; i++)
            {
                nextEdge[i] = Constants.NullIdx;
            }

            for (int i = 0; i < npolys; ++i)
            {
                var t = polys[i];
                for (int j = 0; j < Constants.VertsPerPolygon; ++j)
                {
                    if (t[j] == Constants.NullIdx) break;
                    int v0 = t[j];
                    int v1 = (j + 1 >= Constants.VertsPerPolygon || t[j + 1] == Constants.NullIdx) ? t[0] : t[j + 1];
                    if (v0 < v1)
                    {
                        Edge edge = new Edge()
                        {
                            vert = new int[2],
                            polyEdge = new int[2],
                            poly = new int[2],
                        };
                        edge.vert[0] = v0;
                        edge.vert[1] = v1;
                        edge.poly[0] = i;
                        edge.polyEdge[0] = j;
                        edge.poly[1] = i;
                        edge.polyEdge[1] = 0xff;
                        edges[edgeCount] = edge;
                        // Insert edge
                        nextEdge[edgeCount] = firstEdge[v0];
                        firstEdge[v0] = edgeCount;
                        edgeCount++;
                    }
                }
            }

            for (int i = 0; i < npolys; ++i)
            {
                var t = polys[i];
                for (int j = 0; j < Constants.VertsPerPolygon; ++j)
                {
                    if (t[j] == Constants.NullIdx) break;
                    int v0 = t[j];
                    int v1 = (j + 1 >= Constants.VertsPerPolygon || t[j + 1] == Constants.NullIdx) ? t[0] : t[j + 1];
                    if (v0 > v1)
                    {
                        bool found = false;
                        for (int e = firstEdge[v1]; e != Constants.NullIdx; e = nextEdge[e])
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
                            Edge edge = new Edge()
                            {
                                vert = new int[2],
                                polyEdge = new int[2],
                                poly = new int[2],
                            };
                            edge.vert[0] = v1;
                            edge.vert[1] = v0;
                            edge.poly[0] = i;
                            edge.polyEdge[0] = j;
                            edge.poly[1] = i;
                            edge.polyEdge[1] = 0xff;
                            edges[edgeCount] = edge;
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
                    var va = cont.verts[k];
                    var vb = cont.verts[j];
                    int dir = va.R & 0xf;
                    if (dir == 0xf)
                    {
                        continue;
                    }

                    if (dir == 0 || dir == 2)
                    {
                        // Find matching vertical edge
                        int x = va.X;
                        int zmin = va.Z;
                        int zmax = vb.Z;
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
                            var eva = verts[e.vert[0]];
                            var evb = verts[e.vert[1]];
                            if (eva.X == x && evb.X == x)
                            {
                                int ezmin = eva.Z;
                                int ezmax = evb.Z;
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
                        int z = va.Z;
                        int xmin = va.X;
                        int xmax = vb.X;
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
                            var eva = verts[e.vert[0]];
                            var evb = verts[e.vert[1]];
                            if (eva.Z == z && evb.Z == z)
                            {
                                int exmin = eva.X;
                                int exmax = evb.X;
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
                    var p0 = polys[e.poly[0]];
                    var p1 = polys[e.poly[1]];
                    p0[Constants.VertsPerPolygon + e.polyEdge[0]] = e.poly[1];
                    p1[Constants.VertsPerPolygon + e.polyEdge[1]] = e.poly[0];
                }
                else if (e.polyEdge[1] != 0xff)
                {
                    var p0 = polys[e.poly[0]];
                    p0[Constants.VertsPerPolygon + e.polyEdge[0]] = 0x8000 | e.polyEdge[1];
                }
            }

            return true;
        }
        private static bool OverlapRangeExl(int amin, int amax, int bmin, int bmax)
        {
            return (amin >= bmax || amax <= bmin) ? false : true;
        }


        private bool CreateNavMeshData(NavMeshCreateParams param, out MeshData outData)
        {
            outData = null;

            if (param.nvp > Constants.VertsPerPolygon)
                return false;
            if (param.vertCount >= 0xffff)
                return false;
            if (param.vertCount == 0 || param.verts == null)
                return false;
            if (param.polyCount == 0 || param.polys == null)
                return false;

            int nvp = param.nvp;

            // Classify off-mesh connection points. We store only the connections
            // whose start point is inside the tile.
            int[] offMeshConClass = null;
            int storedOffMeshConCount = 0;
            int offMeshConLinkCount = 0;

            if (param.offMeshConCount > 0)
            {
                offMeshConClass = new int[param.offMeshConCount * 2];

                // Find tight heigh bounds, used for culling out off-mesh start locations.
                float hmin = float.MaxValue;
                float hmax = float.MinValue;

                if (param.detailVerts != null && param.detailVertsCount > 0)
                {
                    for (int i = 0; i < param.detailVertsCount; ++i)
                    {
                        var h = param.detailVerts[i].Y;
                        hmin = Math.Min(hmin, h);
                        hmax = Math.Max(hmax, h);
                    }
                }
                else
                {
                    for (int i = 0; i < param.vertCount; ++i)
                    {
                        var iv = param.verts[i];
                        float h = param.bmin[1] + iv[1] * param.ch;
                        hmin = Math.Min(hmin, h);
                        hmax = Math.Max(hmax, h);
                    }
                }
                hmin -= param.walkableClimb;
                hmax += param.walkableClimb;
                Vector3 bmin = param.bmin;
                Vector3 bmax = param.bmax;
                bmin.Y = hmin;
                bmax.Y = hmax;

                for (int i = 0; i < param.offMeshConCount; ++i)
                {
                    var p0 = param.offMeshConVerts[(i + 0)];
                    var p1 = param.offMeshConVerts[(i + 1)];
                    offMeshConClass[i + 0] = ClassifyOffMeshPoint(p0, bmin, bmax);
                    offMeshConClass[i + 1] = ClassifyOffMeshPoint(p1, bmin, bmax);

                    // Zero out off-mesh start positions which are not even potentially touching the mesh.
                    if (offMeshConClass[i * 2 + 0] == 0xff)
                    {
                        if (p0[1] < bmin[1] || p0[1] > bmax[1])
                        {
                            offMeshConClass[i * 2 + 0] = 0;
                        }
                    }

                    // Cound how many links should be allocated for off-mesh connections.
                    if (offMeshConClass[i * 2 + 0] == 0xff)
                        offMeshConLinkCount++;
                    if (offMeshConClass[i * 2 + 1] == 0xff)
                        offMeshConLinkCount++;
                    if (offMeshConClass[i * 2 + 0] == 0xff)
                        storedOffMeshConCount++;
                }
            }

            // Off-mesh connectionss are stored as polygons, adjust values.
            int totPolyCount = param.polyCount + storedOffMeshConCount;
            int totVertCount = param.vertCount + storedOffMeshConCount * 2;

            // Find portal edges which are at tile borders.
            int edgeCount = 0;
            int portalCount = 0;
            for (int i = 0; i < param.polyCount; ++i)
            {
                var p = param.polys[i];
                for (int j = 0; j < nvp; ++j)
                {
                    if (p[j] == Constants.NullIdx) break;
                    edgeCount++;

                    if ((p[nvp + j] & 0x8000) != 0)
                    {
                        var dir = p[nvp + j] & 0xf;
                        if (dir != 0xf)
                            portalCount++;
                    }
                }
            }

            int maxLinkCount = edgeCount + portalCount * 2 + offMeshConLinkCount * 2;

            // Find unique detail vertices.
            int uniqueDetailVertCount = 0;
            int detailTriCount = 0;
            if (param.detailMeshes != null)
            {
                // Has detail mesh, count unique detail vertex count and use input detail tri count.
                detailTriCount = param.detailTriCount;
                for (int i = 0; i < param.polyCount; ++i)
                {
                    var p = param.polys[i];
                    var ndv = param.detailMeshes[i].Y;
                    int nv = 0;
                    for (int j = 0; j < nvp; ++j)
                    {
                        if (p[j] == Constants.NullIdx) break;
                        nv++;
                    }
                    ndv -= nv;
                    uniqueDetailVertCount += ndv;
                }
            }
            else
            {
                // No input detail mesh, build detail mesh from nav polys.
                uniqueDetailVertCount = 0; // No extra detail verts.
                detailTriCount = 0;
                for (int i = 0; i < param.polyCount; ++i)
                {
                    var p = param.polys[i];
                    int nv = 0;
                    for (int j = 0; j < nvp; ++j)
                    {
                        if (p[j] == Constants.NullIdx) break;
                        nv++;
                    }
                    detailTriCount += nv - 2;
                }
            }

            MeshData data = new MeshData
            {
                // Store header
                header = new MeshHeader
                {
                    magic = Constants.Magic,
                    version = Constants.Version,
                    x = param.tileX,
                    y = param.tileY,
                    layer = param.tileLayer,
                    userId = param.userId,
                    polyCount = totPolyCount,
                    vertCount = totVertCount,
                    maxLinkCount = maxLinkCount,
                    bmin = param.bmin,
                    bmax = param.bmax,
                    detailMeshCount = param.polyCount,
                    detailVertCount = uniqueDetailVertCount,
                    detailTriCount = detailTriCount,
                    bvQuantFactor = 1.0f / param.cs,
                    offMeshBase = param.polyCount,
                    walkableHeight = param.walkableHeight,
                    walkableRadius = param.walkableRadius,
                    walkableClimb = param.walkableClimb,
                    offMeshConCount = storedOffMeshConCount,
                    bvNodeCount = param.buildBvTree ? param.polyCount * 2 : 0
                }
            };

            int offMeshVertsBase = param.vertCount;
            int offMeshPolyBase = param.polyCount;

            // Store vertices
            // Mesh vertices
            for (int i = 0; i < param.vertCount; ++i)
            {
                var iv = param.verts[i];
                var v = new Vector3
                {
                    X = param.bmin.X + iv.X * param.cs,
                    Y = param.bmin.Y + iv.Y * param.ch,
                    Z = param.bmin.Z + iv.Z * param.cs
                };
                data.navVerts.Add(v);
            }
            // Off-mesh link vertices.
            int n = 0;
            for (int i = 0; i < param.offMeshConCount; ++i)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass[i * 2 + 0] == 0xff)
                {
                    var linkv = param.offMeshConVerts[i * 2];
                    var v = data.navVerts[(offMeshVertsBase + n * 2) * 3];
                    v[0] = linkv[0];
                    v[3] = linkv[3];
                    n++;
                }
            }

            // Store polygons
            // Mesh polys
            int srcIndex = 0;
            for (int i = 0; i < param.polyCount; ++i)
            {
                var src = param.polys[srcIndex];

                Poly p = new Poly
                {
                    vertCount = 0,
                    flags = param.polyFlags[i]
                };
                p.Area = param.polyAreas[i];
                p.Type = PolyTypes.Ground;
                for (int j = 0; j < nvp; ++j)
                {
                    if (src[j] == Constants.NullIdx) break;
                    p.verts[j] = src[j];
                    if ((src[nvp + j] & 0x8000) != 0)
                    {
                        // Border or portal edge.
                        var dir = src[nvp + j] & 0xf;
                        if (dir == 0xf) // Border
                            p.neis[j] = 0;
                        else if (dir == 0) // Portal x-
                            p.neis[j] = Constants.DT_EXT_LINK | 4;
                        else if (dir == 1) // Portal z+
                            p.neis[j] = Constants.DT_EXT_LINK | 2;
                        else if (dir == 2) // Portal x+
                            p.neis[j] = Constants.DT_EXT_LINK | 0;
                        else if (dir == 3) // Portal z-
                            p.neis[j] = Constants.DT_EXT_LINK | 6;
                    }
                    else
                    {
                        // Normal connection
                        p.neis[j] = src[nvp + j] + 1;
                    }

                    p.vertCount++;
                }
                data.navPolys.Add(p);
                srcIndex++;
            }
            // Off-mesh connection vertices.
            n = 0;
            for (int i = 0; i < param.offMeshConCount; ++i)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass[i * 2 + 0] == 0xff)
                {
                    Poly p = data.navPolys[offMeshPolyBase + n];
                    p.vertCount = 2;
                    p.verts[0] = (offMeshVertsBase + n * 2 + 0);
                    p.verts[1] = (offMeshVertsBase + n * 2 + 1);
                    p.flags = param.offMeshConFlags[i];
                    p.Area = param.offMeshConAreas[i];
                    p.Type = PolyTypes.OffmeshConnection;
                    n++;
                }
            }

            // Store detail meshes and vertices.
            // The nav polygon vertices are stored as the first vertices on each mesh.
            // We compress the mesh data by skipping them and using the navmesh coordinates.
            if (param.detailMeshes != null)
            {
                for (int i = 0; i < param.polyCount; ++i)
                {
                    int vb = param.detailMeshes[i][0];
                    int ndv = param.detailMeshes[i][1];
                    int nv = data.navPolys[i].vertCount;
                    PolyDetail dtl = new PolyDetail
                    {
                        vertBase = data.navDVerts.Count,
                        vertCount = (ndv - nv),
                        triBase = param.detailMeshes[i][2],
                        triCount = param.detailMeshes[i][3]
                    };
                    // Copy vertices except the first 'nv' verts which are equal to nav poly verts.
                    if (ndv - nv != 0)
                    {
                        data.navDVerts.Add(param.detailVerts[(vb + nv)]);
                    }
                    data.navDMeshes.Add(dtl);
                }
                // Store triangles.
                data.navDTris.AddRange(param.detailTris);
            }
            else
            {
                // Create dummy detail mesh by triangulating polys.
                int tbase = 0;
                for (int i = 0; i < param.polyCount; ++i)
                {
                    int nv = data.navPolys[i].vertCount;
                    PolyDetail dtl = new PolyDetail
                    {
                        vertBase = 0,
                        vertCount = 0,
                        triBase = tbase,
                        triCount = (nv - 2)
                    };
                    // Triangulate polygon (local indices).
                    for (int j = 2; j < nv; ++j)
                    {
                        var t = new Trianglei
                        {
                            X = 0,
                            Y = (j - 1),
                            Z = j,
                            // Bit for each edge that belongs to poly boundary.
                            R = (1 << 2)
                        };
                        if (j == 2) t.R |= (1 << 0);
                        if (j == nv - 1) t.R |= (1 << 4);
                        tbase++;

                        data.navDTris.Add(t);
                    }
                    data.navDMeshes.Add(dtl);
                }
            }

            // Store and create BVtree.
            if (param.buildBvTree)
            {
                CreateBVTree(param, ref data.navBvtree);
            }

            // Store Off-Mesh connections.
            n = 0;
            for (int i = 0; i < param.offMeshConCount; ++i)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass[i * 2 + 0] == 0xff)
                {
                    var con = new OffMeshConnection
                    {
                        poly = offMeshPolyBase + n,
                        rad = param.offMeshConRad[i],
                        flags = param.offMeshConDir[i] != 0 ? (uint)Constants.DT_OFFMESH_CON_BIDIR : 0,
                        side = offMeshConClass[i * 2 + 1]
                    };

                    // Copy connection end-points.
                    var endPts1 = param.offMeshConVerts[i + 0];
                    var endPts2 = param.offMeshConVerts[i + 1];
                    con.pos[0] = endPts1;
                    con.pos[1] = endPts2;
                    if (param.offMeshConUserID != null)
                    {
                        con.userId = param.offMeshConUserID[i];
                    }
                    data.offMeshCons.Add(con);
                    n++;
                }
            }

            offMeshConClass = null;

            outData = data;

            return true;
        }
        private int ClassifyOffMeshPoint(Vector3 pt, Vector3 bmin, Vector3 bmax)
        {
            int XP = 1 << 0;
            int ZP = 1 << 1;
            int XM = 1 << 2;
            int ZM = 1 << 3;

            int outcode = 0;
            outcode |= (pt[0] >= bmax[0]) ? XP : 0;
            outcode |= (pt[2] >= bmax[2]) ? ZP : 0;
            outcode |= (pt[0] < bmin[0]) ? XM : 0;
            outcode |= (pt[2] < bmin[2]) ? ZM : 0;

            if (XP != 0) return 0;
            if ((XP | ZP) != 0) return 1;
            if (ZP != 0) return 2;
            if ((XM | ZP) != 0) return 3;
            if (XM != 0) return 4;
            if ((XM | ZM) != 0) return 5;
            if (ZM != 0) return 6;
            if ((XP | ZM) != 0) return 7;

            return 0xff;
        }
        private int CreateBVTree(NavMeshCreateParams param, ref List<BVNode> nodes)
        {
            // Build tree
            float quantFactor = 1 / param.cs;
            BVItem[] items = new BVItem[param.polyCount];
            for (int i = 0; i < param.polyCount; i++)
            {
                BVItem it = items[i];
                it.i = i;
                // Calc polygon bounds. Use detail meshes if available.
                if (param.detailMeshes != null)
                {
                    int vb = param.detailMeshes[i][0];
                    int ndv = param.detailMeshes[i][1];
                    Vector3 bmin;
                    Vector3 bmax;

                    var dv = param.detailVerts[vb];
                    bmin = dv;
                    bmax = dv;

                    for (int j = 1; j < ndv; j++)
                    {
                        Vector3.Min(bmin, param.detailVerts[j]);
                        Vector3.Max(bmax, param.detailVerts[j]);
                    }

                    // BV-tree uses cs for all dimensions
                    it.bmin.X = MathUtil.Clamp((int)((bmin[0] - param.bmin[0]) * quantFactor), 0, 0xffff);
                    it.bmin.Y = MathUtil.Clamp((int)((bmin[1] - param.bmin[1]) * quantFactor), 0, 0xffff);
                    it.bmin.Z = MathUtil.Clamp((int)((bmin[2] - param.bmin[2]) * quantFactor), 0, 0xffff);

                    it.bmax.X = MathUtil.Clamp((int)((bmax[0] - param.bmin[0]) * quantFactor), 0, 0xffff);
                    it.bmax.Y = MathUtil.Clamp((int)((bmax[1] - param.bmin[1]) * quantFactor), 0, 0xffff);
                    it.bmax.Z = MathUtil.Clamp((int)((bmax[2] - param.bmin[2]) * quantFactor), 0, 0xffff);
                }
                else
                {
                    var p = param.polys[i];
                    it.bmin.X = it.bmax.X = param.verts[p[0]].X;
                    it.bmin.Y = it.bmax.Y = param.verts[p[0]].Y;
                    it.bmin.Z = it.bmax.Z = param.verts[p[0]].Z;

                    for (int j = 1; j < param.nvp; ++j)
                    {
                        if (p[j] == Constants.NullIdx) break;
                        var x = param.verts[p[j]].X;
                        var y = param.verts[p[j]].Y;
                        var z = param.verts[p[j]].Z;

                        if (x < it.bmin.X) it.bmin.X = x;
                        if (y < it.bmin.Y) it.bmin.Y = y;
                        if (z < it.bmin.Z) it.bmin.Z = z;

                        if (x > it.bmax.X) it.bmax.X = x;
                        if (y > it.bmax.Y) it.bmax.Y = y;
                        if (z > it.bmax.Z) it.bmax.Z = z;
                    }
                    // Remap y
                    it.bmin.Y = (int)Math.Floor(it.bmin.Y * param.ch / param.cs);
                    it.bmax.Y = (int)Math.Ceiling(it.bmax.Y * param.ch / param.cs);
                }
            }

            int curNode = 0;
            Subdivide(items, param.polyCount, 0, param.polyCount, ref curNode, ref nodes);

            items = null;

            return curNode;
        }
        private void Subdivide(BVItem[] items, int nitems, int imin, int imax, ref int curNode, ref List<BVNode> nodes)
        {
            int inum = imax - imin;
            int icur = curNode;

            BVNode node = nodes[curNode++];

            if (inum == 1)
            {
                // Leaf
                node.bmin.X = items[imin].bmin.X;
                node.bmin.Y = items[imin].bmin.Y;
                node.bmin.Z = items[imin].bmin.Z;

                node.bmax.X = items[imin].bmax.X;
                node.bmax.Y = items[imin].bmax.Y;
                node.bmax.Z = items[imin].bmax.Z;

                node.i = items[imin].i;
            }
            else
            {
                // Split
                CalcExtends(items, nitems, imin, imax, ref node.bmin, ref node.bmax);

                int axis = LongestAxis(node.bmax.X - node.bmin.X,
                                       node.bmax.Y - node.bmin.Y,
                                       node.bmax.Z - node.bmin.Z);

                if (axis == 0)
                {
                    // Sort along x-axis
                    Array.Sort(items, imin, inum, BVItem.XComparer);
                }
                else if (axis == 1)
                {
                    // Sort along y-axis
                    Array.Sort(items, imin, inum, BVItem.YComparer);
                }
                else
                {
                    // Sort along z-axis
                    Array.Sort(items, imin, inum, BVItem.ZComparer);
                }

                int isplit = imin + inum / 2;

                // Left
                Subdivide(items, nitems, imin, isplit, ref curNode, ref nodes);
                // Right
                Subdivide(items, nitems, isplit, imax, ref curNode, ref nodes);

                int iescape = curNode - icur;
                // Negative index means escape.
                node.i = -iescape;
            }
        }
        private void CalcExtends(BVItem[] items, int nitems, int imin, int imax, ref Vector3i bmin, ref Vector3i bmax)
        {
            bmin.X = items[imin].bmin.X;
            bmin.Y = items[imin].bmin.Y;
            bmin.Z = items[imin].bmin.Z;

            bmax.X = items[imin].bmax.X;
            bmax.Y = items[imin].bmax.Y;
            bmax.Z = items[imin].bmax.Z;

            for (int i = imin + 1; i < imax; ++i)
            {
                BVItem it = items[i];
                if (it.bmin.X < bmin.X) bmin.X = it.bmin.X;
                if (it.bmin.Y < bmin.Y) bmin.Y = it.bmin.Y;
                if (it.bmin.Z < bmin.Z) bmin.Z = it.bmin.Z;

                if (it.bmax.X > bmax.X) bmax.X = it.bmax.X;
                if (it.bmax.Y > bmax.Y) bmax.Y = it.bmax.Y;
                if (it.bmax.Z > bmax.Z) bmax.Z = it.bmax.Z;
            }
        }
        private int LongestAxis(int x, int y, int z)
        {
            int axis = 0;
            int maxVal = x;
            if (y > maxVal)
            {
                axis = 1;
                maxVal = y;
            }
            if (z > maxVal)
            {
                axis = 2;
            }
            return axis;
        }
    }
}
