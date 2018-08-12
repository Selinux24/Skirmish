using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Tile Cache
    /// </summary>
    public class TileCache
    {
        private TileCacheParams m_params;
        private TileCacheMeshProcess m_tmproc;
        private TileCacheObstacle[] m_obstacles = null;
        private int m_nextFreeObstacle = -1;
        private int m_tileLutSize;
        private int m_tileLutMask;
        private CompressedTile[] m_tiles = null;
        private CompressedTile[] m_posLookup = null;
        private CompressedTile m_nextFreeTile = null;
        private int m_tileBits;
        private int m_saltBits;
        private readonly ObstacleRequest[] m_reqs = new ObstacleRequest[DetourTileCache.MAX_REQUESTS];
        private int m_nreqs;
        private readonly CompressedTile[] m_update = new CompressedTile[DetourTileCache.MAX_UPDATE];
        private int m_nupdate;

        /// <summary>
        /// Constructor
        /// </summary>
        public TileCache()
        {

        }

        public TileCacheParams GetParams() { return m_params; }
        public int GetTileCount() { return m_params.MaxTiles; }
        public CompressedTile GetTile(int i) { return m_tiles[i]; }
        public int GetObstacleCount() { return m_params.MaxObstacles; }
        public TileCacheObstacle GetObstacle(int i) { return m_obstacles[i]; }
        public TileCacheObstacle GetObstacleByRef(int r)
        {
            if (r == 0)
            {
                return null;
            }
            int idx = DecodeObstacleIdObstacle(r);
            if (idx >= m_params.MaxObstacles)
            {
                return null;
            }
            var ob = m_obstacles[idx];
            int salt = DecodeObstacleIdSalt(r);
            if (ob.Salt != salt)
            {
                return null;
            }

            return ob;
        }
        public int GetObstacleRef(TileCacheObstacle ob)
        {
            if (ob == null) return 0;

            int idx = Array.IndexOf(m_obstacles, ob);

            return EncodeObstacleId(ob.Salt, idx);
        }

        public void Init(TileCacheParams tcparams, TileCacheMeshProcess tmproc)
        {
            m_params = tcparams;
            m_tmproc = tmproc;

            // Alloc space for obstacles.
            m_obstacles = new TileCacheObstacle[tcparams.MaxObstacles];
            m_nextFreeObstacle = -1;
            for (int i = tcparams.MaxObstacles - 1; i >= 0; i--)
            {
                m_obstacles[i] = new TileCacheObstacle
                {
                    Salt = 1,
                    Next = m_nextFreeObstacle
                };
                m_nextFreeObstacle = i;
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

        public int GetTilesAt(int tx, int ty, out CompressedTile[] tiles, int maxTiles)
        {
            tiles = new CompressedTile[maxTiles];

            int n = 0;

            // Find tile based on hash.
            int h = Detour.ComputeTileHash(tx, ty, m_tileLutMask);
            var tile = m_posLookup[h];
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
        private CompressedTile GetTileAt(int tx, int ty, int tlayer)
        {
            // Find tile based on hash.
            int h = Detour.ComputeTileHash(tx, ty, m_tileLutMask);
            var tile = m_posLookup[h];
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
        public int GetTileRef(CompressedTile tile)
        {
            if (tile == null)
            {
                return 0;
            }
            int it = Array.IndexOf(m_tiles, tile);

            return EncodeTileId(tile.Salt, it);
        }
        public CompressedTile GetTileByRef(int r)
        {
            if (r == 0)
            {
                return null;
            }
            int tileIndex = DecodeTileIdTile(r);
            int tileSalt = DecodeTileIdSalt(r);
            if (tileIndex >= m_params.MaxTiles)
            {
                return null;
            }
            var tile = m_tiles[tileIndex];
            if (tile.Salt != tileSalt)
            {
                return null;
            }

            return tile;
        }

        public CompressedTile AddTile(TileCacheData data, CompressedTileFlags flags)
        {
            // Make sure the data is in right format.
            var header = data.Header;
            if (header.magic != DetourTileCache.DT_TILECACHE_MAGIC)
            {
                throw new EngineException("DT_WRONG_MAGIC");
            }
            if (header.version != DetourTileCache.DT_TILECACHE_VERSION)
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
            int h = Detour.ComputeTileHash(header.tx, header.ty, m_tileLutMask);
            tile.Next = m_posLookup[h];
            m_posLookup[h] = tile;

            // Init tile.
            tile.Header = data.Header;
            tile.Data = data.Data;
            tile.Flags = flags;

            return tile;
        }
        public Status RemoveTile(int r, out TileCacheLayerData data, out int dataSize)
        {
            data = TileCacheLayerData.Empty;
            dataSize = 0;

            if (r == 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }
            int tileIndex = DecodeTileIdTile(r);
            int tileSalt = DecodeTileIdSalt(r);
            if (tileIndex >= m_params.MaxTiles)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }
            var tile = m_tiles[tileIndex];
            if (tile.Salt != tileSalt)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            // Remove tile from hash lookup.
            int h = DetourTileCache.ComputeTileHash(tile.Header.tx, tile.Header.ty, m_tileLutMask);
            CompressedTile prev = null;
            CompressedTile cur = m_posLookup[h];
            while (cur != null)
            {
                if (cur == tile)
                {
                    if (prev != null)
                    {
                        prev.Next = cur.Next;
                    }
                    else
                    {
                        m_posLookup[h] = cur.Next;
                    }
                    break;
                }
                prev = cur;
                cur = cur.Next;
            }

            // Reset tile.
            if ((tile.Flags & CompressedTileFlags.DT_COMPRESSEDTILE_FREE_DATA) != 0)
            {
                // Owns data
                tile.Data = TileCacheLayerData.Empty;
            }
            else
            {
                data = tile.Data;
            }

            tile.Header = new TileCacheLayerHeader();
            tile.Data = new TileCacheLayerData();
            tile.Flags = 0;

            // Update salt, salt should never be zero.
            tile.Salt = (tile.Salt + 1) & ((1 << m_saltBits) - 1);
            if (tile.Salt == 0)
            {
                tile.Salt++;
            }

            // Add to free list.
            tile.Next = m_nextFreeTile;
            m_nextFreeTile = tile;

            return Status.DT_SUCCESS;
        }

        public Status AddObstacle(Vector3 pos, float radius, float height, out int result)
        {
            result = 0;

            if (m_nreqs >= DetourTileCache.MAX_REQUESTS)
            {
                return Status.DT_FAILURE | Status.DT_BUFFER_TOO_SMALL;
            }

            int ob = -1;
            if (m_nextFreeObstacle >= 0)
            {
                ob = m_nextFreeObstacle;
                m_nextFreeObstacle = m_obstacles[ob].Next;
                m_obstacles[ob].Next = -1;
            }
            if (ob == -1)
            {
                return Status.DT_FAILURE;
            }

            int salt = m_obstacles[ob].Salt;
            m_obstacles[ob] = new TileCacheObstacle
            {
                Salt = salt,
                state = ObstacleState.DT_OBSTACLE_PROCESSING,
                type = ObstacleType.DT_OBSTACLE_CYLINDER,
                cylinder = new ObstacleCylinder
                {
                    pos = pos,
                    radius = radius,
                    height = height
                }
            };

            var req = new ObstacleRequest
            {
                action = ObstacleRequestAction.REQUEST_ADD,
                nref = GetObstacleRef(m_obstacles[ob]),
            };
            m_reqs[m_nreqs++] = req;

            result = req.nref;

            return Status.DT_SUCCESS;
        }
        public Status AddBoxObstacle(Vector3 bmin, Vector3 bmax, out int result)
        {
            result = 0;

            if (m_nreqs >= DetourTileCache.MAX_REQUESTS)
            {
                return Status.DT_FAILURE | Status.DT_BUFFER_TOO_SMALL;
            }

            int ob = -1;
            if (m_nextFreeObstacle >= 0)
            {
                ob = m_nextFreeObstacle;
                m_nextFreeObstacle = m_obstacles[ob].Next;
                m_obstacles[ob].Next = -1;
            }
            if (ob == -1)
            {
                return Status.DT_FAILURE;
            }

            int salt = m_obstacles[ob].Salt;
            m_obstacles[ob] = new TileCacheObstacle
            {
                Salt = salt,
                state = ObstacleState.DT_OBSTACLE_PROCESSING,
                type = ObstacleType.DT_OBSTACLE_BOX,
                box = new ObstacleBox
                {
                    bmin = bmin,
                    bmax = bmax
                }
            };

            var req = new ObstacleRequest
            {
                action = ObstacleRequestAction.REQUEST_ADD,
                nref = GetObstacleRef(m_obstacles[ob])
            };
            m_reqs[m_nreqs++] = req;

            result = req.nref;

            return Status.DT_SUCCESS;
        }
        public Status AddBoxObstacle(Vector3 center, Vector3 halfExtents, float yRadians, out int result)
        {
            result = 0;

            if (m_nreqs >= DetourTileCache.MAX_REQUESTS)
            {
                return Status.DT_FAILURE | Status.DT_BUFFER_TOO_SMALL;
            }

            int ob = -1;
            if (m_nextFreeObstacle >= 0)
            {
                ob = m_nextFreeObstacle;
                m_nextFreeObstacle = m_obstacles[ob].Next;
                m_obstacles[ob].Next = -1;
            }
            if (ob == -1)
            {
                return Status.DT_FAILURE;
            }

            int salt = m_obstacles[ob].Salt;
            m_obstacles[ob] = new TileCacheObstacle
            {
                Salt = salt,
                state = ObstacleState.DT_OBSTACLE_PROCESSING,
                type = ObstacleType.DT_OBSTACLE_ORIENTED_BOX,
                orientedBox = new ObstacleOrientedBox
                {
                    center = center,
                    halfExtents = halfExtents
                }
            };

            float coshalf = (float)Math.Cos(0.5f * yRadians);
            float sinhalf = (float)Math.Sin(-0.5f * yRadians);
            m_obstacles[ob].orientedBox.rotAux.X = coshalf * sinhalf;
            m_obstacles[ob].orientedBox.rotAux.Y = coshalf * coshalf - 0.5f;

            var req = new ObstacleRequest
            {
                action = ObstacleRequestAction.REQUEST_ADD,
                nref = GetObstacleRef(m_obstacles[ob])
            };
            m_reqs[m_nreqs++] = req;

            result = req.nref;

            return Status.DT_SUCCESS;
        }
        public Status RemoveObstacle(int r)
        {
            if (r == 0)
            {
                return Status.DT_SUCCESS;
            }
            if (m_nreqs >= DetourTileCache.MAX_REQUESTS)
            {
                return Status.DT_FAILURE | Status.DT_BUFFER_TOO_SMALL;
            }

            var req = new ObstacleRequest
            {
                action = ObstacleRequestAction.REQUEST_REMOVE,
                nref = r
            };
            m_reqs[m_nreqs++] = req;

            return Status.DT_SUCCESS;
        }

        public Status AddOffmeshConnection(Vector3 from, Vector3 to, out int result)
        {
            result = -1;

            return Status.DT_FAILURE;
        }
        public Status RemoveOffmeshConnection(int r)
        {
            return Status.DT_FAILURE;
        }

        public Status QueryTiles(Vector3 bmin, Vector3 bmax, out CompressedTile[] results, out int resultCount, int maxResults)
        {
            results = new CompressedTile[maxResults];

            int MAX_TILES = 32;

            int n = 0;

            float tw = m_params.Width * m_params.CellSize;
            float th = m_params.Height * m_params.CellSize;
            int tx0 = (int)Math.Floor((bmin.X - m_params.Origin.X) / tw);
            int tx1 = (int)Math.Floor((bmax.X - m_params.Origin.X) / tw);
            int ty0 = (int)Math.Floor((bmin.Z - m_params.Origin.Z) / th);
            int ty1 = (int)Math.Floor((bmax.Z - m_params.Origin.Z) / th);

            for (int ty = ty0; ty <= ty1; ++ty)
            {
                for (int tx = tx0; tx <= tx1; ++tx)
                {
                    int ntiles = GetTilesAt(tx, ty, out CompressedTile[] tiles, MAX_TILES);

                    for (int i = 0; i < ntiles; ++i)
                    {
                        var tile = tiles[i];
                        CalcTightTileBounds(tile.Header, out Vector3 tbmin, out Vector3 tbmax);

                        if (Detour.OverlapBounds(bmin, bmax, tbmin, tbmax))
                        {
                            if (n < maxResults)
                            {
                                results[n++] = tiles[i];
                            }
                        }
                    }
                }
            }

            resultCount = n;

            return Status.DT_SUCCESS;
        }
        public Status Update(float dt, NavMesh navmesh, out bool upToDate)
        {
            upToDate = false;

            if (m_nupdate == 0)
            {
                // Process requests.
                for (int i = 0; i < m_nreqs; ++i)
                {
                    var req = m_reqs[i];

                    int idx = DecodeObstacleIdObstacle(req.nref);
                    if (idx >= m_params.MaxObstacles)
                    {
                        continue;
                    }
                    var ob = m_obstacles[idx];
                    int salt = DecodeObstacleIdSalt(req.nref);
                    if (ob.Salt != salt)
                    {
                        continue;
                    }

                    if (req.action == ObstacleRequestAction.REQUEST_ADD)
                    {
                        // Find touched tiles.
                        GetObstacleBounds(ob, out Vector3 bmin, out Vector3 bmax);

                        QueryTiles(bmin, bmax, out ob.touched, out ob.ntouched, DetourTileCache.DT_MAX_TOUCHED_TILES);
                        // Add tiles to update list.
                        ob.npending = 0;
                        for (int j = 0; j < ob.ntouched; ++j)
                        {
                            if (m_nupdate < DetourTileCache.MAX_UPDATE)
                            {
                                if (!DetourTileCache.Contains(m_update, m_nupdate, ob.touched[j]))
                                {
                                    m_update[m_nupdate++] = ob.touched[j];
                                }
                                ob.pending[ob.npending++] = ob.touched[j];
                            }
                        }
                    }
                    else if (req.action == ObstacleRequestAction.REQUEST_REMOVE)
                    {
                        // Prepare to remove obstacle.
                        ob.state = ObstacleState.DT_OBSTACLE_REMOVING;
                        // Add tiles to update list.
                        ob.npending = 0;
                        for (int j = 0; j < ob.ntouched; ++j)
                        {
                            if (m_nupdate < DetourTileCache.MAX_UPDATE)
                            {
                                if (!DetourTileCache.Contains(m_update, m_nupdate, ob.touched[j]))
                                {
                                    m_update[m_nupdate++] = ob.touched[j];
                                }
                                ob.pending[ob.npending++] = ob.touched[j];
                            }
                        }
                    }
                }

                m_nreqs = 0;
            }

            Status status = Status.DT_SUCCESS;
            // Process updates
            if (m_nupdate != 0)
            {
                // Build mesh
                var r = m_update[0];
                if (!BuildNavMeshTile(r, navmesh))
                {
                    status = Status.DT_FAILURE;
                }
                m_nupdate--;
                if (m_nupdate > 0)
                {
                    Array.Copy(m_update, 1, m_update, 0, m_nupdate);
                }

                // Update obstacle states.
                for (int i = 0; i < m_params.MaxObstacles; ++i)
                {
                    var ob = m_obstacles[i];
                    if (ob.state == ObstacleState.DT_OBSTACLE_PROCESSING || ob.state == ObstacleState.DT_OBSTACLE_REMOVING)
                    {
                        // Remove handled tile from pending list.
                        for (int j = 0; j < ob.npending; j++)
                        {
                            if (ob.pending[j] == r)
                            {
                                ob.pending[j] = ob.pending[ob.npending - 1];
                                ob.npending--;
                                break;
                            }
                        }

                        // If all pending tiles processed, change state.
                        if (ob.npending == 0)
                        {
                            if (ob.state == ObstacleState.DT_OBSTACLE_PROCESSING)
                            {
                                ob.state = ObstacleState.DT_OBSTACLE_PROCESSED;
                            }
                            else if (ob.state == ObstacleState.DT_OBSTACLE_REMOVING)
                            {
                                ob.state = ObstacleState.DT_OBSTACLE_EMPTY;
                                // Update salt, salt should never be zero.
                                ob.Salt = (ob.Salt + 1) & ((1 << 16) - 1);
                                if (ob.Salt == 0)
                                {
                                    ob.Salt++;
                                }
                                // Return obstacle to free list.
                                ob.Next = m_nextFreeObstacle;
                                m_nextFreeObstacle = i;
                            }
                        }
                    }
                }
            }

            upToDate = m_nupdate == 0 && m_nreqs == 0;

            return status;
        }

        public bool BuildNavMeshTilesAt(int tx, int ty, NavMesh navmesh)
        {
            int MAX_TILES = 32;
            int ntiles = GetTilesAt(tx, ty, out CompressedTile[] tiles, MAX_TILES);

            for (int i = 0; i < ntiles; ++i)
            {
                if (!BuildNavMeshTile(tiles[i], navmesh))
                {
                    return false;
                }
            }

            return true;
        }
        public bool BuildNavMeshTile(CompressedTile tile, NavMesh navmesh)
        {
            NavMeshTileBuildContext bc = new NavMeshTileBuildContext();
            int walkableClimbVx = (int)(m_params.WalkableClimb / m_params.CellHeight);

            // Decompress tile layer data.
            if (!DetourTileCache.DecompressTileCacheLayer(tile.Header, tile.Data, 0, out bc.layer))
            {
                return false;
            }

            // Rasterize obstacles.
            for (int i = 0; i < m_params.MaxObstacles; ++i)
            {
                var ob = m_obstacles[i];

                if (ob.state == ObstacleState.DT_OBSTACLE_EMPTY || ob.state == ObstacleState.DT_OBSTACLE_REMOVING)
                {
                    continue;
                }

                if (DetourTileCache.Contains(ob.touched, ob.ntouched, tile))
                {
                    if (ob.type == ObstacleType.DT_OBSTACLE_CYLINDER)
                    {
                        DetourTileCache.MarkCylinderArea(ref bc.layer,
                            tile.Header.b.Minimum, m_params.CellSize, m_params.CellHeight,
                            ob.cylinder.pos, ob.cylinder.radius, ob.cylinder.height, 0);
                    }
                    else if (ob.type == ObstacleType.DT_OBSTACLE_BOX)
                    {
                        DetourTileCache.MarkBoxArea(ref bc.layer,
                            tile.Header.b.Minimum, m_params.CellSize, m_params.CellHeight,
                            ob.box.bmin, ob.box.bmax, 0);
                    }
                    else if (ob.type == ObstacleType.DT_OBSTACLE_ORIENTED_BOX)
                    {
                        DetourTileCache.MarkBoxArea(ref bc.layer,
                            tile.Header.b.Minimum, m_params.CellSize, m_params.CellHeight,
                            ob.orientedBox.center, ob.orientedBox.halfExtents, ob.orientedBox.rotAux, 0);
                    }
                }
            }

            // Build navmesh
            if (!DetourTileCache.BuildTileCacheRegions(ref bc.layer, walkableClimbVx))
            {
                return false;
            }

            if (!DetourTileCache.BuildTileCacheContours(bc.layer, walkableClimbVx, m_params.MaxSimplificationError, out bc.lcset))
            {
                return false;
            }

            if (!DetourTileCache.BuildTileCachePolyMesh(bc.lcset, out bc.lmesh))
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
                nvp = Detour.DT_VERTS_PER_POLYGON,
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
                bmax = tile.Header.b.Maximum,
            };

            if (m_tmproc != null)
            {
                m_tmproc.Process(ref param, ref bc.lmesh.areas, ref bc.lmesh.flags);
            }

            if (!Detour.CreateNavMeshData(param, out MeshData navData))
            {
                return false;
            }

            // Remove existing tile.
            navmesh.RemoveTile(navmesh.GetTileRefAt(tile.Header.tx, tile.Header.ty, tile.Header.tlayer), null, 0);

            // Add new tile, or leave the location empty.
            if (navData != null)
            {
                // Let the navmesh own the data.
                if (!navmesh.AddTile(navData, TileFlags.DT_TILE_FREE_DATA, 0, out int result))
                {
                    navData = null;

                    return false;
                }
            }

            return true;
        }

        private void CalcTightTileBounds(TileCacheLayerHeader header, out Vector3 bmin, out Vector3 bmax)
        {
            bmin = new Vector3();
            bmax = new Vector3();

            float cs = m_params.CellSize;
            bmin.X = header.b.Minimum.X + header.minx * cs;
            bmin.Y = header.b.Minimum.Y;
            bmin.Z = header.b.Minimum.Z + header.miny * cs;
            bmax.X = header.b.Minimum.X + (header.maxx + 1) * cs;
            bmax.Y = header.b.Maximum.Y;
            bmax.Z = header.b.Minimum.Z + (header.maxy + 1) * cs;
        }
        private void GetObstacleBounds(TileCacheObstacle ob, out Vector3 bmin, out Vector3 bmax)
        {
            bmin = new Vector3();
            bmax = new Vector3();

            if (ob.type == ObstacleType.DT_OBSTACLE_CYLINDER)
            {
                var cl = ob.cylinder;

                bmin.X = cl.pos.X - cl.radius;
                bmin.Y = cl.pos.Y;
                bmin.Z = cl.pos.Z - cl.radius;
                bmax.X = cl.pos.X + cl.radius;
                bmax.Y = cl.pos.Y + cl.height;
                bmax.Z = cl.pos.Z + cl.radius;
            }
            else if (ob.type == ObstacleType.DT_OBSTACLE_BOX)
            {
                bmin = ob.box.bmin;
                bmax = ob.box.bmax;
            }
            else if (ob.type == ObstacleType.DT_OBSTACLE_ORIENTED_BOX)
            {
                var orientedBox = ob.orientedBox;

                float maxr = 1.41f * Math.Max(orientedBox.halfExtents.X, orientedBox.halfExtents.Z);
                bmin.X = orientedBox.center.X - maxr;
                bmax.X = orientedBox.center.X + maxr;
                bmin.Y = orientedBox.center.Y - orientedBox.halfExtents.Y;
                bmax.Y = orientedBox.center.Y + orientedBox.halfExtents.Y;
                bmin.Z = orientedBox.center.Z - maxr;
                bmax.Z = orientedBox.center.Z + maxr;
            }
        }

        /// <summary>
        /// Encodes a tile id.
        /// </summary>
        private int EncodeTileId(int salt, int it)
        {
            return (salt << m_tileBits) | it;
        }
        /// <summary>
        /// Decodes a tile salt.
        /// </summary>
        private int DecodeTileIdSalt(int r)
        {

            int saltMask = (1 << m_saltBits) - 1;
            return ((r >> m_tileBits) & saltMask);
        }
        /// <summary>
        /// Decodes a tile id.
        /// </summary>
        private int DecodeTileIdTile(int r)
        {
            int tileMask = (1 << m_tileBits) - 1;
            return (r & tileMask);
        }
        /// <summary>
        /// Encodes an obstacle id.
        /// </summary>
        private int EncodeObstacleId(int salt, int it)
        {
            return (salt << 16) | it;
        }
        /// <summary>
        /// Decodes an obstacle salt.
        /// </summary>
        private int DecodeObstacleIdSalt(int r)
        {
            int saltMask = (1 << 16) - 1;
            return ((r >> 16) & saltMask);
        }
        /// <summary>
        /// Decodes an obstacle id.
        /// </summary>
        private int DecodeObstacleIdObstacle(int r)
        {
            int tileMask = (1 << 16) - 1;
            return (r & tileMask);
        }
    }
}
