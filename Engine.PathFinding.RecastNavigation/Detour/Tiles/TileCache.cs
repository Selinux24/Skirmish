using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile Cache
    /// </summary>
    public class TileCache
    {
        private TileCacheParams m_params;
        private readonly TileCacheMeshProcess m_tmproc;
        private readonly TileCacheObstacle[] m_obstacles = null;
        private int m_nextFreeObstacle;
        private readonly int m_tileLutMask;
        private readonly CompressedTile[] m_tiles = null;
        private readonly CompressedTile[] m_posLookup = null;
        private CompressedTile m_nextFreeTile = null;
        private readonly int m_tileBits;
        private readonly int m_saltBits;
        private readonly List<ObstacleRequest> m_reqs = new List<ObstacleRequest>();
        private readonly List<CompressedTile> m_update = new List<CompressedTile>();

        /// <summary>
        /// Constructor
        /// </summary>
        public TileCache(TileCacheParams tcparams, TileCacheMeshProcess tmproc)
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
            var m_tileLutSize = Helper.NextPowerOfTwo(tcparams.MaxTiles / 4);
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

        private IEnumerable<CompressedTile> GetTilesAt(int x, int y, int maxTiles)
        {
            List<CompressedTile> tiles = new List<CompressedTile>();

            // Find tile based on hash.
            int h = DetourUtils.ComputeTileHash(x, y, m_tileLutMask);
            var tile = m_posLookup[h];
            while (tile != null)
            {
                if (tile.Header.TX == x && tile.Header.TY == y && tiles.Count < maxTiles)
                {
                    tiles.Add(tile);
                }

                tile = tile.Next;
            }

            return tiles.ToArray();
        }
        private CompressedTile GetTileAt(int tx, int ty, int tlayer)
        {
            // Find tile based on hash.
            int h = DetourUtils.ComputeTileHash(tx, ty, m_tileLutMask);
            var tile = m_posLookup[h];
            while (tile != null)
            {
                if (tile.Header.TX == tx &&
                    tile.Header.TY == ty &&
                    tile.Header.TLayer == tlayer)
                {
                    return tile;
                }
                tile = tile.Next;
            }

            return null;
        }

        public CompressedTile AddTile(TileCacheData data, CompressedTileFlagTypes flags)
        {
            // Make sure the data is in right format.
            var header = data.Header;
            if (header.Magic != DetourTileCache.DT_TILECACHE_MAGIC)
            {
                throw new EngineException("DT_WRONG_MAGIC");
            }
            if (header.Version != DetourTileCache.DT_TILECACHE_VERSION)
            {
                throw new EngineException("DT_WRONG_VERSION");
            }

            // Make sure the location is free.
            if (GetTileAt(header.TX, header.TY, header.TLayer) != null)
            {
                throw new EngineException("DT_FAILURE");
            }

            // Allocate a tile.
            CompressedTile tile;
            if (m_nextFreeTile != null)
            {
                tile = m_nextFreeTile;
                m_nextFreeTile = tile.Next;
                tile.Next = null;
            }
            else
            {
                tile = new CompressedTile();
            }

            // Insert tile into the position lut.
            int h = DetourUtils.ComputeTileHash(header.TX, header.TY, m_tileLutMask);
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
            int h = DetourTileCache.ComputeTileHash(tile.Header.TX, tile.Header.TY, m_tileLutMask);
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
            if ((tile.Flags & CompressedTileFlagTypes.DT_COMPRESSEDTILE_FREE_DATA) != 0)
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

            if (m_reqs.Count >= DetourTileCache.MAX_REQUESTS)
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
                State = ObstacleState.DT_OBSTACLE_PROCESSING,
                Type = ObstacleType.DT_OBSTACLE_CYLINDER,
                Cylinder = new ObstacleCylinder
                {
                    Pos = pos,
                    Radius = radius,
                    Height = height
                }
            };

            var req = new ObstacleRequest
            {
                Action = ObstacleRequestAction.REQUEST_ADD,
                NRef = GetObstacleRef(m_obstacles[ob]),
            };
            m_reqs.Add(req);

            result = req.NRef;

            return Status.DT_SUCCESS;
        }
        public Status AddBoxObstacle(Vector3 bmin, Vector3 bmax, out int result)
        {
            result = 0;

            if (m_reqs.Count >= DetourTileCache.MAX_REQUESTS)
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
                State = ObstacleState.DT_OBSTACLE_PROCESSING,
                Type = ObstacleType.DT_OBSTACLE_BOX,
                Box = new ObstacleBox
                {
                    BMin = bmin,
                    BMax = bmax
                }
            };

            var req = new ObstacleRequest
            {
                Action = ObstacleRequestAction.REQUEST_ADD,
                NRef = GetObstacleRef(m_obstacles[ob])
            };
            m_reqs.Add(req);

            result = req.NRef;

            return Status.DT_SUCCESS;
        }
        public Status AddBoxObstacle(Vector3 center, Vector3 halfExtents, float yRadians, out int result)
        {
            result = 0;

            if (m_reqs.Count >= DetourTileCache.MAX_REQUESTS)
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

            float coshalf = (float)Math.Cos(0.5f * yRadians);
            float sinhalf = (float)Math.Sin(-0.5f * yRadians);
            var rotAux = new Vector2(coshalf * sinhalf, coshalf * coshalf - 0.5f);

            int salt = m_obstacles[ob].Salt;
            m_obstacles[ob] = new TileCacheObstacle
            {
                Salt = salt,
                State = ObstacleState.DT_OBSTACLE_PROCESSING,
                Type = ObstacleType.DT_OBSTACLE_ORIENTED_BOX,
                OrientedBox = new ObstacleOrientedBox
                {
                    Center = center,
                    HalfExtents = halfExtents,
                    RotAux = rotAux,
                }
            };

            var req = new ObstacleRequest
            {
                Action = ObstacleRequestAction.REQUEST_ADD,
                NRef = GetObstacleRef(m_obstacles[ob])
            };
            m_reqs.Add(req);

            result = req.NRef;

            return Status.DT_SUCCESS;
        }
        public Status RemoveObstacle(int r)
        {
            if (r == 0)
            {
                return Status.DT_SUCCESS;
            }
            if (m_reqs.Count >= DetourTileCache.MAX_REQUESTS)
            {
                return Status.DT_FAILURE | Status.DT_BUFFER_TOO_SMALL;
            }

            var req = new ObstacleRequest
            {
                Action = ObstacleRequestAction.REQUEST_REMOVE,
                NRef = r
            };
            m_reqs.Add(req);

            return Status.DT_SUCCESS;
        }

        public Status Update(NavMesh navmesh, out bool upToDate)
        {
            if (m_update.Count == 0)
            {
                // Process requests.
                ProcessRequests();
            }

            Status status = ProcessUpdates(navmesh);

            upToDate = m_update.Count == 0 && m_reqs.Count == 0;

            return status;
        }
        private void ProcessRequests()
        {
            // Process requests.
            foreach (var req in m_reqs)
            {
                int idx = DecodeObstacleIdObstacle(req.NRef);
                if (idx >= m_params.MaxObstacles)
                {
                    continue;
                }
                var ob = m_obstacles[idx];
                int salt = DecodeObstacleIdSalt(req.NRef);
                if (ob.Salt != salt)
                {
                    continue;
                }

                if (req.Action == ObstacleRequestAction.REQUEST_ADD)
                {
                    ProcessRequestAdd(ob);
                }
                else if (req.Action == ObstacleRequestAction.REQUEST_REMOVE)
                {
                    ProcessRequestRemove(ob);
                }
            }

            m_reqs.Clear();
        }
        private void ProcessRequestAdd(TileCacheObstacle ob)
        {
            // Find touched tiles.
            GetObstacleBounds(ob, out Vector3 bmin, out Vector3 bmax);

            var touched = QueryTiles(bmin, bmax, DetourTileCache.DT_MAX_TOUCHED_TILES);
            ob.Touched = touched.ToArray();
            ob.NTouched = touched.Count();

            // Add tiles to update list.
            ob.NPending = 0;
            for (int j = 0; j < ob.NTouched; ++j)
            {
                if (m_update.Count < DetourTileCache.MAX_UPDATE)
                {
                    if (!m_update.Contains(ob.Touched[j]))
                    {
                        m_update.Add(ob.Touched[j]);
                    }
                    ob.Pending[ob.NPending++] = ob.Touched[j];
                }
            }
        }
        private void ProcessRequestRemove(TileCacheObstacle ob)
        {
            // Prepare to remove obstacle.
            ob.State = ObstacleState.DT_OBSTACLE_REMOVING;
            // Add tiles to update list.
            ob.NPending = 0;
            for (int j = 0; j < ob.NTouched; ++j)
            {
                if (m_update.Count < DetourTileCache.MAX_UPDATE)
                {
                    if (!m_update.Contains(ob.Touched[j]))
                    {
                        m_update.Add(ob.Touched[j]);
                    }
                    ob.Pending[ob.NPending++] = ob.Touched[j];
                }
            }
        }
        private Status ProcessUpdates(NavMesh navmesh)
        {
            Status status = Status.DT_SUCCESS;

            // Process updates
            if (m_update.Count != 0)
            {
                // Build mesh
                var r = m_update[0];
                if (!BuildNavMeshTile(r, navmesh))
                {
                    status = Status.DT_FAILURE;
                }

                if (m_update.Count > 0)
                {
                    m_update.RemoveAt(0);
                }

                // Update obstacle states.
                var pendingObstacles = m_obstacles
                    .Where(ob => ob.State == ObstacleState.DT_OBSTACLE_PROCESSING || ob.State == ObstacleState.DT_OBSTACLE_REMOVING)
                    .ToArray();

                for (int i = 0; i < pendingObstacles.Length; ++i)
                {
                    var ob = pendingObstacles[i];

                    bool processed = ProcessObstacleUpdate(ob, r);
                    if (processed)
                    {
                        // Return obstacle to free list.
                        ob.Next = m_nextFreeObstacle;
                        m_nextFreeObstacle = i;
                    }
                }
            }

            return status;
        }
        private bool ProcessObstacleUpdate(TileCacheObstacle ob, CompressedTile r)
        {
            // Remove handled tile from pending list.
            for (int j = 0; j < ob.NPending; j++)
            {
                if (ob.Pending[j] == r)
                {
                    ob.Pending[j] = ob.Pending[ob.NPending - 1];
                    ob.NPending--;
                    break;
                }
            }

            // If all pending tiles processed, change state.
            if (ob.NPending == 0)
            {
                if (ob.State == ObstacleState.DT_OBSTACLE_PROCESSING)
                {
                    ob.State = ObstacleState.DT_OBSTACLE_PROCESSED;
                }
                else if (ob.State == ObstacleState.DT_OBSTACLE_REMOVING)
                {
                    ob.State = ObstacleState.DT_OBSTACLE_EMPTY;
                    // Update salt, salt should never be zero.
                    ob.Salt = (ob.Salt + 1) & ((1 << 16) - 1);
                    if (ob.Salt == 0)
                    {
                        ob.Salt++;
                    }

                    return true;
                }
            }

            return false;
        }
        private IEnumerable<CompressedTile> QueryTiles(Vector3 bmin, Vector3 bmax, int maxResults)
        {
            List<CompressedTile> results = new List<CompressedTile>();

            int MAX_TILES = 32;

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
                    var tiles = GetTilesAt(tx, ty, MAX_TILES);

                    foreach (var tile in tiles)
                    {
                        CalcTightTileBounds(tile.Header, out Vector3 tbmin, out Vector3 tbmax);

                        if (DetourUtils.OverlapBounds(bmin, bmax, tbmin, tbmax) && results.Count < maxResults)
                        {
                            results.Add(tile);
                        }
                    }
                }
            }

            return results.ToArray();
        }

        public bool BuildNavMeshTilesAt(int x, int y, NavMesh navmesh)
        {
            int MAX_TILES = 32;
            var tiles = GetTilesAt(x, y, MAX_TILES);

            foreach (var tile in tiles)
            {
                if (!BuildNavMeshTile(tile, navmesh))
                {
                    return false;
                }
            }

            return true;
        }
        private bool BuildNavMeshTile(CompressedTile tile, NavMesh navmesh)
        {
            // Decompress tile layer data.
            if (!DetourTileCache.DecompressTileCacheLayer(tile.Header, tile.Data, out var layer))
            {
                return false;
            }
            NavMeshTileBuildContext bc = new NavMeshTileBuildContext
            {
                Layer = layer
            };

            // Rasterize obstacles.
            for (int i = 0; i < m_params.MaxObstacles; ++i)
            {
                RasterizeObstacle(bc, tile, m_obstacles[i]);
            }

            int walkableClimbVx = (int)(m_params.WalkableClimb / m_params.CellHeight);

            // Build navmesh
            if (!DetourTileCache.BuildTileCacheRegions(bc, walkableClimbVx))
            {
                return false;
            }

            if (!DetourTileCache.BuildTileCacheContours(bc, walkableClimbVx, m_params.MaxSimplificationError))
            {
                return false;
            }

            if (!DetourTileCache.BuildTileCachePolyMesh(bc))
            {
                return false;
            }

            // Early out if the mesh tile is empty.
            if (bc.LMesh.NPolys == 0)
            {
                // Remove existing tile.
                navmesh.RemoveTile(navmesh.GetTileRefAt(tile.Header.TX, tile.Header.TY, tile.Header.TLayer));
                return true;
            }

            var param = new NavMeshCreateParams
            {
                Verts = bc.LMesh.Verts,
                VertCount = bc.LMesh.NVerts,
                Polys = bc.LMesh.Polys,
                PolyAreas = bc.LMesh.Areas,
                PolyFlags = bc.LMesh.Flags,
                polyCount = bc.LMesh.NPolys,
                nvp = DetourUtils.DT_VERTS_PER_POLYGON,
                walkableHeight = m_params.WalkableHeight,
                walkableRadius = m_params.WalkableRadius,
                walkableClimb = m_params.WalkableClimb,
                tileX = tile.Header.TX,
                tileY = tile.Header.TY,
                tileLayer = tile.Header.TLayer,
                cs = m_params.CellSize,
                ch = m_params.CellHeight,
                buildBvTree = false,
                bmin = tile.Header.BBox.Minimum,
                bmax = tile.Header.BBox.Maximum,
            };

            if (m_tmproc != null)
            {
                m_tmproc.Process(ref param, bc);
            }

            if (!DetourUtils.CreateNavMeshData(param, out MeshData navData))
            {
                return false;
            }

            // Remove existing tile.
            var tileRef = navmesh.GetTileRefAt(tile.Header.TX, tile.Header.TY, tile.Header.TLayer);
            navmesh.RemoveTile(tileRef);

            // Leave the location empty.
            if (navData == null)
            {
                return true;
            }

            // Add new tile
            return navmesh.AddTile(navData, TileFlagTypes.DT_TILE_FREE_DATA, 0);
        }
        private void RasterizeObstacle(NavMeshTileBuildContext bc, CompressedTile tile, TileCacheObstacle ob)
        {
            if (ob.State == ObstacleState.DT_OBSTACLE_EMPTY || ob.State == ObstacleState.DT_OBSTACLE_REMOVING)
            {
                return;
            }

            if (!Array.Exists(ob.Touched, (t) => t == tile))
            {
                return;
            }

            switch (ob.Type)
            {
                case ObstacleType.DT_OBSTACLE_CYLINDER:
                    DetourTileCache.MarkCylinderArea(
                        bc,
                        tile.Header.BBox.Minimum, m_params.CellSize, m_params.CellHeight,
                        ob.Cylinder, 0);
                    break;
                case ObstacleType.DT_OBSTACLE_BOX:
                    DetourTileCache.MarkBoxArea(
                        bc,
                        tile.Header.BBox.Minimum, m_params.CellSize, m_params.CellHeight,
                        ob.Box, 0);
                    break;
                case ObstacleType.DT_OBSTACLE_ORIENTED_BOX:
                    DetourTileCache.MarkBoxArea(
                        bc,
                        tile.Header.BBox.Minimum, m_params.CellSize, m_params.CellHeight,
                        ob.OrientedBox, 0);
                    break;
            }
        }

        private void CalcTightTileBounds(TileCacheLayerHeader header, out Vector3 bmin, out Vector3 bmax)
        {
            bmin = new Vector3();
            bmax = new Vector3();

            float cs = m_params.CellSize;
            bmin.X = header.BBox.Minimum.X + header.MinX * cs;
            bmin.Y = header.BBox.Minimum.Y;
            bmin.Z = header.BBox.Minimum.Z + header.MinY * cs;
            bmax.X = header.BBox.Minimum.X + (header.MaxX + 1) * cs;
            bmax.Y = header.BBox.Maximum.Y;
            bmax.Z = header.BBox.Minimum.Z + (header.MaxY + 1) * cs;
        }
        private void GetObstacleBounds(TileCacheObstacle ob, out Vector3 bmin, out Vector3 bmax)
        {
            bmin = new Vector3();
            bmax = new Vector3();

            if (ob.Type == ObstacleType.DT_OBSTACLE_CYLINDER)
            {
                var cl = ob.Cylinder;

                bmin.X = cl.Pos.X - cl.Radius;
                bmin.Y = cl.Pos.Y;
                bmin.Z = cl.Pos.Z - cl.Radius;
                bmax.X = cl.Pos.X + cl.Radius;
                bmax.Y = cl.Pos.Y + cl.Height;
                bmax.Z = cl.Pos.Z + cl.Radius;
            }
            else if (ob.Type == ObstacleType.DT_OBSTACLE_BOX)
            {
                bmin = ob.Box.BMin;
                bmax = ob.Box.BMax;
            }
            else if (ob.Type == ObstacleType.DT_OBSTACLE_ORIENTED_BOX)
            {
                var orientedBox = ob.OrientedBox;

                float maxr = 1.41f * Math.Max(orientedBox.HalfExtents.X, orientedBox.HalfExtents.Z);
                bmin.X = orientedBox.Center.X - maxr;
                bmax.X = orientedBox.Center.X + maxr;
                bmin.Y = orientedBox.Center.Y - orientedBox.HalfExtents.Y;
                bmax.Y = orientedBox.Center.Y + orientedBox.HalfExtents.Y;
                bmin.Z = orientedBox.Center.Z - maxr;
                bmax.Z = orientedBox.Center.Z + maxr;
            }
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
