using SharpDX;
using System;

namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Tile Cache
    /// </summary>
    public class TileCache
    {
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
            int h = PolyUtils.ComputeTileHash(header.tx, header.ty, m_tileLutMask);
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
            int h = PolyUtils.ComputeTileHash(tx, ty, m_tileLutMask);
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
            int h = PolyUtils.ComputeTileHash(tx, ty, m_tileLutMask);
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

            if (!NavigationMesh2.CreateNavMeshData(param, out MeshData navData))
            {
                return false;
            }

            // Remove existing tile.
            navmesh.RemoveTile(navmesh.GetTileRefAt(tile.Header.tx, tile.Header.ty, tile.Header.tlayer), null, 0);

            // Add new tile, or leave the location empty.
            if (navData != null)
            {
                // Let the navmesh own the data.
                if (!navmesh.AddTile(navData, TileFlags.FreeData, 0, out int result))
                {
                    navData = null;

                    return false;
                }
            }

            return true;
        }

        private static bool DecompressTileCacheLayer(TileCacheLayerHeader header, TileCacheLayerData data, int dataSize, out TileCacheLayer layer)
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
        private static bool MarkBoxArea(ref TileCacheLayer layer, Vector3 orig, float cs, float ch, Vector3 center, Vector3 halfExtents, Vector2 rotAux, TileCacheAreas areaId)
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
        private static bool MarkBoxArea(ref TileCacheLayer layer, Vector3 orig, float cs, float ch, Vector3 bmin, Vector3 bmax, TileCacheAreas areaId)
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
        private static bool MarkCylinderArea(ref TileCacheLayer layer, Vector3 orig, float cs, float ch, Vector3 pos, float radius, float height, TileCacheAreas areaId)
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
        private static bool BuildTileCacheRegions(ref TileCacheLayer layer, int walkableClimb)
        {
            int w = layer.header.width;
            int h = layer.header.height;

            layer.regs = Helper.CreateArray(w * h, 0xff);

            int nsweeps = w;
            LayerSweepSpan[] sweeps = new LayerSweepSpan[nsweeps];

            // Partition walkable area into monotone regions.
            int[] prevCount = new int[256];
            int regId = 0;

            for (int y = 0; y < h; ++y)
            {
                if (regId > 0)
                {
                    for (int i = 0; i < regId; i++)
                    {
                        prevCount[i] = 0;
                    }
                }
                int sweepId = 0;

                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    if (layer.areas[idx] == TileCacheAreas.NullArea)
                    {
                        continue;
                    }

                    int sid = 0xff;

                    // -x
                    int xidx = (x - 1) + y * w;
                    if (x > 0 && IsConnected(layer, idx, xidx, walkableClimb))
                    {
                        if (layer.regs[xidx] != 0xff)
                        {
                            sid = layer.regs[xidx];
                        }
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
                        int nr = layer.regs[yidx];
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
                    int ri = layer.regs[idx];
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
                        int rai = layer.regs[ymi];
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
                regs[i].regId = i;
            }

            for (int i = 0; i < nregs; ++i)
            {
                LayerMonotoneRegion reg = regs[i];

                int merge = -1;
                int mergea = 0;
                for (int j = 0; j < reg.nneis; ++j)
                {
                    int nei = reg.neis[j];
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
                    int oldId = reg.regId;
                    int newId = regs[merge].regId;
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
            int[] remap = Helper.CreateArray(256, 0);
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
        private static bool IsConnected(TileCacheLayer layer, int ia, int ib, int walkableClimb)
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
        private static void AddUniqueLast(ref int[] a, ref int an, int v)
        {
            int n = an;
            if (n > 0 && a[n - 1] == v)
            {
                return;
            }
            a[an] = v;
            an++;
        }
        private static bool CanMerge(int oldRegId, int newRegId, LayerMonotoneRegion[] regs, int nregs)
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
        private static bool BuildTileCacheContours(TileCacheLayer layer, int walkableClimb, float maxError, out TileCacheContourSet lcset)
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

            var tempVerts = new Int4[maxTempVerts];
            var tempPoly = new Polygoni(maxTempVerts);

            var temp = new TempContour(tempVerts, maxTempVerts, tempPoly, maxTempVerts);

            // Find contours.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    int ri = layer.regs[idx];
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
                        cont.verts = new Int4[temp.nverts];

                        for (int i = 0, j = temp.nverts - 1; i < temp.nverts; j = i++)
                        {
                            var v = temp.verts[j];
                            var vn = temp.verts[i];
                            int nei = vn.W; // The neighbour reg is stored at segment vertex of a segment. 
                            bool shouldRemove = false;
                            int lh = GetCornerHeight(layer, v.X, v.Y, v.Z, walkableClimb, ref shouldRemove);

                            var dst = new Int4()
                            {
                                X = v.X,
                                Y = lh,
                                Z = v.Z,
                                W = 0x0f,
                            };

                            // Store portal direction and remove status to the fourth component.
                            if (nei != 0xff && nei >= 0xf8)
                            {
                                dst.W = nei - 0xf8;
                            }
                            if (shouldRemove)
                            {
                                dst.W |= 0x80;
                            }

                            cont.verts[j] = dst;
                        }
                    }

                    lcset.conts[ri] = cont;
                }
            }

            return true;
        }
        private static int GetCornerHeight(TileCacheLayer layer, int x, int y, int z, int walkableClimb, ref bool shouldRemove)
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
        private static bool WalkContour(TileCacheLayer layer, int x, int y, TempContour cont)
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
                    nx = x + PolyUtils.GetDirOffsetX(dir);
                    ny = y + PolyUtils.GetDirOffsetY(dir);
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
        private static int GetNeighbourReg(TileCacheLayer layer, int ax, int ay, int dir)
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

            int bx = ax + PolyUtils.GetDirOffsetX(dir);
            int by = ay + PolyUtils.GetDirOffsetY(dir);
            int ib = bx + by * w;

            return layer.regs[ib];
        }
        private static bool AppendVertex(TempContour cont, int x, int y, int z, int r)
        {
            // Try to merge with existing segments.
            if (cont.nverts > 1)
            {
                var pa = cont.verts[cont.nverts - 2];
                var pb = cont.verts[cont.nverts - 1];
                if (pb.W == r)
                {
                    if (pa.X == pb.X && pb.X == x)
                    {
                        // The verts are aligned aling x-axis, update z.
                        pb.Y = y;
                        pb.Z = z;
                        cont.verts[cont.nverts - 1] = pb;
                        return true;
                    }
                    else if (pa.Z == pb.Z && pb.Z == z)
                    {
                        // The verts are aligned aling z-axis, update x.
                        pb.X = x;
                        pb.Y = y;
                        cont.verts[cont.nverts - 1] = pb;
                        return true;
                    }
                }
            }

            // Add new point.
            if (cont.nverts + 1 > cont.cverts)
            {
                return false;
            }

            cont.verts[cont.nverts] = new Int4(x, y, z, r);
            cont.nverts++;

            return true;
        }
        private static void SimplifyContour(TempContour cont, float maxError)
        {
            cont.npoly = 0;

            for (int i = 0; i < cont.nverts; ++i)
            {
                int j = (i + 1) % cont.nverts;
                // Check for start of a wall segment.
                int ra = cont.verts[j].W;
                int rb = cont.verts[i].W;
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
                    float d = PolyUtils.DistancePtSeg(cont.verts[ci].X, cont.verts[ci].Z, ax, az, bx, bz);
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
                cont.verts[cont.nverts++] = new Int4()
                {
                    X = src.X,
                    Y = src.Y,
                    Z = src.Z,
                    W = src.W,
                };
            }
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
                verts = Helper.CreateArray(maxVertices, () => new Int3()),
                polys = new Polygoni[maxTris],
                areas = new SamplePolyAreas[maxTris],
                flags = new SamplePolyFlags[maxTris],
                nverts = 0,
                npolys = 0
            };

            int[] vflags = new int[maxVertices];
            int[] firstVert = Helper.CreateArray(Constants.VertexBucketCount2, Constants.NullIdx);
            int[] nextVert = Helper.CreateArray(maxVertices, 0);
            int[] indices = new int[maxVertsPerCont];

            for (int i = 0; i < lcset.nconts; ++i)
            {
                var cont = lcset.conts[i];

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

                int ntris = PolyUtils.Triangulate(cont.nverts, cont.verts, ref indices, out Int3[] tris);
                if (ntris <= 0)
                {
                    // TODO: issue warning!
                    ntris = -ntris;
                }

                // Add and merge vertices.
                for (int j = 0; j < cont.nverts; ++j)
                {
                    var v = cont.verts[j];
                    indices[j] = PolyUtils.AddVertex(v.X, v.Y, v.Z, mesh.verts, firstVert, nextVert, ref mesh.nverts);
                    if ((v.W & 0x80) != 0)
                    {
                        // This vertex should be removed.
                        vflags[indices[j]] = 1;
                    }
                }

                // Build initial polygons.
                int npolys = 0;
                Polygoni[] polys = new Polygoni[maxVertsPerCont];
                for (int j = 0; j < ntris; ++j)
                {
                    var t = tris[j];
                    if (t.X != t.Y && t.X != t.Z && t.Y != t.Z)
                    {
                        polys[npolys] = new Polygoni(Constants.VertsPerPolygon);
                        polys[npolys][0] = indices[t.X];
                        polys[npolys][1] = indices[t.Y];
                        polys[npolys][2] = indices[t.Z];
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
                                int v = PolyUtils.GetPolyMergeValue(pj, pk, mesh.verts, out int ea, out int eb);
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
                            polys[bestPa] = PolyUtils.MergePolys(polys[bestPa], polys[bestPb], bestEa, bestEb);
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
                    var p = new Polygoni(Constants.VertsPerPolygon * 2);//Polygon with adjacency
                    var q = polys[j];
                    for (int k = 0; k < Constants.VertsPerPolygon; ++k)
                    {
                        p[k] = q[k];
                    }
                    mesh.polys[mesh.npolys] = p;
                    mesh.areas[mesh.npolys] = (SamplePolyAreas)cont.area;
                    mesh.npolys++;
                    if (mesh.npolys > maxTris)
                    {
                        throw new EngineException(string.Format("rcBuildPolyMesh: Too many polygons {0} (max:{1}).", mesh.npolys, maxTris));
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
                        // Failed to remove vertex
                        throw new EngineException(string.Format("Failed to remove edge vertex {0}.", i));
                    }
                    // Remove vertex
                    // Note: mesh.nverts is already decremented inside removeVertex()!
                    // Fixup vertex flags
                    for (int j = i; j < mesh.nverts; ++j)
                    {
                        vflags[j] = vflags[j + 1];
                    }
                    --i;
                }
            }

            // Calculate adjacency.
            if (!PolyUtils.BuildMeshAdjacency(mesh.polys, mesh.npolys, mesh.verts, mesh.nverts, lcset))
            {
                throw new EngineException("Adjacency failed.");
            }

            return true;
        }
        private static bool CanRemoveVertex(TileCachePolyMesh mesh, int rem)
        {
            // Count number of polygons to remove.
            int numRemovedVerts = 0;
            int numTouchedVerts = 0;
            int numRemainingEdges = 0;
            for (int i = 0; i < mesh.npolys; ++i)
            {
                var p = mesh.polys[i];
                int nv = PolyUtils.CountPolyVerts(p);
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
            if (maxEdges > Constants.MaxRemEdges)
            {
                return false;
            }

            // Find edges which share the removed vertex.
            Int3[] edges = new Int3[Constants.MaxRemEdges];
            int nedges = 0;

            for (int i = 0; i < mesh.npolys; ++i)
            {
                var p = mesh.polys[i];
                int nv = PolyUtils.CountPolyVerts(p);

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
                            var e = edges[m];
                            if (e.Y == b)
                            {
                                // Exists, increment vertex share count.
                                e.Z++;
                                exists = true;
                            }
                            edges[m] = e;
                        }
                        // Add new edge.
                        if (!exists)
                        {
                            edges[nedges] = new Int3(a, b, 1);
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
                if (edges[i].Z < 2)
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
                var p = mesh.polys[i];
                int nv = PolyUtils.CountPolyVerts(p);
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] == rem)
                    {
                        numRemovedVerts++;
                    }
                }
            }

            int nedges = 0;
            Int3[] edges = new Int3[Constants.MaxRemEdges];
            int nhole = 0;
            int[] hole = new int[Constants.MaxRemEdges];
            int nharea = 0;
            SamplePolyAreas[] harea = new SamplePolyAreas[Constants.MaxRemEdges];

            for (int i = 0; i < mesh.npolys; ++i)
            {
                var p = mesh.polys[i];
                int nv = PolyUtils.CountPolyVerts(p);
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
                            if (nedges >= Constants.MaxRemEdges)
                            {
                                return false;
                            }
                            var e = new Int3(p[k], p[j], (int)mesh.areas[i]);
                            edges[nedges] = e;
                            nedges++;
                        }
                    }
                    // Remove the polygon.
                    var p2 = mesh.polys[mesh.npolys - 1];
                    if (p != p2)
                    {
                        //memcpy(p, p2, sizeof(unsigned short) * MAX_VERTS_PER_POLY);
                    }
                    //memset(p + MAX_VERTS_PER_POLY, 0xff, sizeof(unsigned short) * MAX_VERTS_PER_POLY);

                    mesh.areas[i] = mesh.areas[mesh.npolys - 1];
                    mesh.npolys--;
                    --i;
                }
            }

            // Remove vertex.
            for (int i = rem; i < mesh.nverts; ++i)
            {
                mesh.verts[i] = mesh.verts[(i + 1)];
            }
            mesh.nverts--;

            // Adjust indices to match the removed vertex layout.
            for (int i = 0; i < mesh.npolys; ++i)
            {
                var p = mesh.polys[i];
                int nv = PolyUtils.CountPolyVerts(p);
                for (int j = 0; j < nv; ++j)
                {
                    if (p[j] > rem) p[j]--;
                }
            }
            for (int i = 0; i < nedges; ++i)
            {
                if (edges[i].X > rem) edges[i].X--;
                if (edges[i].Y > rem) edges[i].Y--;
            }

            if (nedges == 0)
            {
                return true;
            }

            // Start with one vertex, keep appending connected
            // segments to the start and end of the hole.
            PolyUtils.PushBack(edges[0].X, hole, nhole);
            PolyUtils.PushBack((SamplePolyAreas)edges[0].Z, harea, nharea);

            while (nedges != 0)
            {
                bool match = false;

                for (int i = 0; i < nedges; ++i)
                {
                    int ea = edges[i].X;
                    int eb = edges[i].Y;

                    SamplePolyAreas a = (SamplePolyAreas)edges[i].Z;
                    bool add = false;
                    if (hole[0] == eb)
                    {
                        // The segment matches the beginning of the hole boundary.
                        if (nhole >= Constants.MaxRemEdges)
                        {
                            return false;
                        }
                        PolyUtils.PushFront(ea, hole, nhole);

                        PolyUtils.PushFront(a, harea, nharea);
                        add = true;
                    }
                    else if (hole[nhole - 1] == ea)
                    {
                        // The segment matches the end of the hole boundary.
                        if (nhole >= Constants.MaxRemEdges)
                        {
                            return false;
                        }
                        PolyUtils.PushBack(eb, hole, nhole);

                        PolyUtils.PushBack(a, harea, nharea);
                        add = true;
                    }
                    if (add)
                    {
                        // The edge segment was added, remove it.
                        edges[i] = edges[(nedges - 1)];
                        nedges--;
                        match = true;
                        i--;
                    }
                }

                if (!match)
                {
                    break;
                }
            }

            var tverts = new Int4[Constants.MaxRemEdges];
            var thole = new int[Constants.MaxRemEdges];

            // Generate temp vertex array for triangulation.
            for (int i = 0; i < nhole; ++i)
            {
                int pi = hole[i];
                tverts[i].X = mesh.verts[pi].X;
                tverts[i].Y = mesh.verts[pi].Y;
                tverts[i].Z = mesh.verts[pi].Z;
                tverts[i].W = 0;
                thole[i] = i;
            }

            // Triangulate the hole.
            int ntris = PolyUtils.Triangulate(nhole, tverts, ref thole, out Int3[] tris);
            if (ntris < 0)
            {
                // TODO: issue warning!
                ntris = -ntris;
            }

            if (ntris > Constants.MaxRemEdges)
            {
                return false;
            }

            // Merge the hole triangles back to polygons.
            var polys = new Polygoni[Constants.MaxRemEdges];

            var pareas = new SamplePolyAreas[Constants.MaxRemEdges];

            // Build initial polygons.
            int npolys = 0;
            for (int j = 0; j < ntris; ++j)
            {
                var t = tris[j];
                if (t.X != t.Y && t.X != t.Z && t.Y != t.Z)
                {
                    polys[npolys][0] = hole[t.X];
                    polys[npolys][1] = hole[t.Y];
                    polys[npolys][2] = hole[t.Z];

                    pareas[npolys] = harea[t.X];
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
                            int v = PolyUtils.GetPolyMergeValue(pj, pk, mesh.verts, out int ea, out int eb);
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
                        polys[bestPa] = PolyUtils.MergePolys(polys[bestPa], polys[bestPb], bestEa, bestEb);
                        polys[bestPb] = polys[npolys - 1];
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
                var p = mesh.polys[mesh.npolys];
                for (int j = 0; j < Constants.VertsPerPolygon; ++j)
                {
                    p[j] = polys[i][j];
                }

                mesh.areas[mesh.npolys] = pareas[i];
                mesh.npolys++;
                if (mesh.npolys > maxTris)
                {
                    //ctx->log(RC_LOG_ERROR, "removeVertex: Too many polygons %d (max:%d).", mesh.npolys, maxTris);
                    return false;
                }
            }

            return true;
        }
    }
}
