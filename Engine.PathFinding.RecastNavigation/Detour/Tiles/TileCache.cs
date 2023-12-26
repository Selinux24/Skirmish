﻿using SharpDX;
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
        /// <summary>
        /// Maximum tiles
        /// </summary>
        const int MAX_TILES = 32;
        /// <summary>
        /// Maximum number of requests
        /// </summary>
        const int MAX_REQUESTS = 64;
        /// <summary>
        /// Maximum number of updates
        /// </summary>
        const int MAX_UPDATE = 64;
        /// <summary>
        /// Maximum number of touched tiles
        /// </summary>
        const int MAX_TOUCHED_TILES = 8;

        /// <summary>
        /// Navigation mesh
        /// </summary>
        private readonly NavMesh m_navMesh;
        /// <summary>
        /// Parameters
        /// </summary>
        private TileCacheParams m_params;
        /// <summary>
        /// Mesh processor
        /// </summary>
        private readonly TileCacheMeshProcess m_tmproc;
        /// <summary>
        /// Obstacle list
        /// </summary>
        private readonly TileCacheObstacle[] m_obstacles = null;
        /// <summary>
        /// Next free obstacle
        /// </summary>
        private int m_nextFreeObstacle;
        /// <summary>
        /// Tile lut mask
        /// </summary>
        private readonly int m_tileLutMask;
        /// <summary>
        /// Compressed tile list
        /// </summary>
        private readonly CompressedTile[] m_tiles = null;
        /// <summary>
        /// Look up compressed tile list
        /// </summary>
        private readonly CompressedTile[] m_posLookup = null;
        /// <summary>
        /// Next free tile
        /// </summary>
        private CompressedTile m_nextFreeTile = null;
        /// <summary>
        /// Tile bits
        /// </summary>
        private readonly int m_tileBits;
        /// <summary>
        /// Salt bits
        /// </summary>
        private readonly int m_saltBits;
        /// <summary>
        /// Obstacle requests
        /// </summary>
        private readonly List<ObstacleRequest> m_reqs = new();
        /// <summary>
        /// Compressed tiles to update
        /// </summary>
        private readonly List<CompressedTile> m_update = new();

        /// <summary>
        /// Constructor
        /// </summary>
        public TileCache(NavMesh navMesh, TileCacheMeshProcess tmproc, TileCacheParams tcparams)
        {
            m_navMesh = navMesh;
            m_tmproc = tmproc;
            m_params = tcparams;

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

        /// <summary>
        /// Encodes an obstacle id.
        /// </summary>
        private static int EncodeObstacleId(int salt, int it)
        {
            return (salt << 16) | it;
        }
        /// <summary>
        /// Decodes an obstacle salt.
        /// </summary>
        private static int DecodeObstacleIdSalt(int r)
        {
            int saltMask = (1 << 16) - 1;
            return (r >> 16) & saltMask;
        }
        /// <summary>
        /// Decodes an obstacle id.
        /// </summary>
        private static int DecodeObstacleIdObstacle(int r)
        {
            int tileMask = (1 << 16) - 1;
            return r & tileMask;
        }

        /// <summary>
        /// Gets the tile-cache parameters
        /// </summary>
        /// <returns>Returns the parameters used to create de tile-cache</returns>
        public TileCacheParams GetParams()
        {
            return m_params;
        }

        /// <summary>
        /// Builds the tile chache
        /// </summary>
        /// <param name="geometry">Input geometry</param>
        /// <param name="cfg">Configuration</param>
        /// <param name="progressCallback">Optional progress callback</param>
        public void BuildTileCache(InputGeometry geometry, Config cfg, Action<float> progressCallback)
        {
            float total = m_params.TileHeight * m_params.TileWidth * 2;
            int curr = 0;

            for (int y = 0; y < m_params.TileHeight; y++)
            {
                for (int x = 0; x < m_params.TileWidth; x++)
                {
                    var tiles = TileCacheData.RasterizeTileLayers(x, y, geometry, cfg);

                    foreach (var tile in tiles)
                    {
                        AddTile(tile, CompressedTileFlagTypes.DT_COMPRESSEDTILE_FREE_DATA);
                    }

                    progressCallback?.Invoke(++curr / total);
                }
            }

            // Build initial meshes
            for (int y = 0; y < m_params.TileHeight; y++)
            {
                for (int x = 0; x < m_params.TileWidth; x++)
                {
                    BuildTilesAt(x, y);

                    progressCallback?.Invoke(++curr / total);
                }
            }
        }

        /// <summary>
        /// Gets maximum tile count
        /// </summary>
        /// <returns>Returns the maximum tile count</returns>
        public int GetTileCount()
        {
            return m_params.MaxTiles;
        }
        /// <summary>
        /// Gets a tile by index
        /// </summary>
        /// <param name="i">Index</param>
        /// <returns>Returns a tile</returns>
        public CompressedTile GetTile(int i)
        {
            return m_tiles[i];
        }
        /// <summary>
        /// Gets a tile by reference
        /// </summary>
        /// <param name="r">Reference</param>
        /// <returns>Returns a tile</returns>
        public CompressedTile GetTileByRef(int r)
        {
            if (r == 0)
            {
                return null;
            }

            int idx = DecodeTileIdTile(r);
            if (idx >= m_params.MaxTiles)
            {
                return null;
            }

            var tile = m_tiles[idx];
            int salt = DecodeTileIdSalt(r);
            if (tile.Salt != salt)
            {
                return null;
            }

            return tile;
        }
        /// <summary>
        /// Gets the tile reference
        /// </summary>
        /// <param name="tile">Tile</param>
        /// <returns>Returns the tile reference</returns>
        public int GetTileRef(CompressedTile tile)
        {
            if (tile == null)
            {
                return 0;
            }

            int idx = Array.IndexOf(m_tiles, tile);

            return EncodeTileId(tile.Salt, idx);
        }

        /// <summary>
        /// Gets the tile list
        /// </summary>
        /// <returns>Returns a tile list</returns>
        public CompressedTile[] GetTiles()
        {
            return m_tiles?.ToArray() ?? Array.Empty<CompressedTile>();
        }
        /// <summary>
        /// Gets the tiles at the specified coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="maxTiles">Maximum tiles to return</param>
        /// <returns>Returns a list of tiles</returns>
        public CompressedTile[] GetTilesAt(int x, int y, int maxTiles)
        {
            var tiles = new List<CompressedTile>();

            // Find tile based on hash.
            int h = Utils.ComputeTileHash(x, y, m_tileLutMask);
            var tile = m_posLookup[h];
            while (tile != null)
            {
                if (tile.Header.TX == x && tile.Header.TY == y)
                {
                    tiles.Add(tile);
                }

                if (maxTiles > 0 && tiles.Count >= maxTiles)
                {
                    break;
                }

                tile = tile.Next;
            }

            return tiles.ToArray();
        }
        /// <summary>
        /// Gets the tile at coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="tlayer">Layer number</param>
        /// <returns>Returns a tile</returns>
        public CompressedTile GetTileAt(int x, int y, int tlayer)
        {
            // Find tile based on hash.
            int h = Utils.ComputeTileHash(x, y, m_tileLutMask);
            var tile = m_posLookup[h];
            while (tile != null)
            {
                if (tile.Header.TX == x && tile.Header.TY == y && tile.Header.TLayer == tlayer)
                {
                    return tile;
                }

                tile = tile.Next;
            }

            return null;
        }

        /// <summary>
        /// Adds a new tile to the tile cache
        /// </summary>
        /// <param name="data">Tile data</param>
        /// <param name="flags">Tile flags</param>
        /// <param name="failIfExists">Fail if the tile location is not free</param>
        /// <returns>Returns the new tile</returns>
        public CompressedTile AddTile(TileCacheData data, CompressedTileFlagTypes flags, bool failIfExists = true)
        {
            var header = data.Header;

            // Make sure the data is in right format.
            if (!header.IsValid())
            {
                throw new EngineException($"Wrong header {header.Magic}-{header.Version}");
            }

            // Make sure the location is free.
            if (GetTileAt(header.TX, header.TY, header.TLayer) != null)
            {
                return failIfExists ? throw new EngineException("DT_FAILURE") : new CompressedTile();
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
            int h = Utils.ComputeTileHash(header.TX, header.TY, m_tileLutMask);
            tile.Next = m_posLookup[h];
            m_posLookup[h] = tile;

            // Init tile.
            tile.Header = data.Header;
            tile.Data = data.Data;
            tile.Flags = flags;

            return tile;
        }
        /// <summary>
        /// Removes a tile
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="tlayer">Layer number</param>
        public Status RemoveTile(int x, int y, int tlayer)
        {
            var t = GetTileAt(x, y, tlayer);
            if (t != null)
            {
                return RemoveTile(t, out _);
            }

            return Status.DT_SUCCESS;
        }
        /// <summary>
        /// Removes a tile
        /// </summary>
        /// <param name="tile">Tile to remove</param>
        /// <param name="data">Layer data</param>
        public Status RemoveTile(CompressedTile tile, out TileCacheLayerData data)
        {
            data = TileCacheLayerData.Empty;

            // Remove tile from hash lookup.
            int h = Utils.ComputeTileHash(tile.Header.TX, tile.Header.TY, m_tileLutMask);
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
        /// <summary>
        /// Removes a tile by reference
        /// </summary>
        /// <param name="r">Tile reference</param>
        /// <param name="data">Returns the layer data</param>
        public Status RemoveTile(int r, out TileCacheLayerData data)
        {
            data = TileCacheLayerData.Empty;

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

            return RemoveTile(tile, out data);
        }
        /// <summary>
        /// Removes all the tile layers in the coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public Status RemoveTiles(int x, int y)
        {
            Status res = Status.DT_SUCCESS;

            var tiles = GetTilesAt(x, y, MAX_TILES);
            foreach (var t in tiles)
            {
                var pStatus = RemoveTile(t, out _);
                if (!pStatus.HasFlag(Status.DT_SUCCESS))
                {
                    res |= pStatus;
                }
            }

            return res;
        }

        /// <summary>
        /// Gets the maximum obstacle count
        /// </summary>
        /// <returns>Returns the maximum obstacle count</returns>
        public int GetObstacleCount()
        {
            return m_params.MaxObstacles;
        }
        /// <summary>
        /// Gets an obstacle by index
        /// </summary>
        /// <param name="i">Index</param>
        /// <returns>Returns an obstacle</returns>
        public TileCacheObstacle GetObstacle(int i)
        {
            return m_obstacles[i];
        }
        /// <summary>
        /// Gets an obstacle by reference
        /// </summary>
        /// <param name="r">Reference</param>
        /// <returns>Returns an obstacle</returns>
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
        /// <summary>
        /// Gets the obstacle reference
        /// </summary>
        /// <param name="ob">Obstacle</param>
        /// <returns>Returns the obstacle reference</returns>
        public int GetObstacleRef(TileCacheObstacle ob)
        {
            if (ob == null)
            {
                return 0;
            }

            int idx = Array.IndexOf(m_obstacles, ob);

            return EncodeObstacleId(ob.Salt, idx);
        }

        /// <summary>
        /// Adds a new obstacle request
        /// </summary>
        /// <param name="obstacle">Obstacle</param>
        /// <param name="result">Resulting obstacle reference</param>
        public Status AddObstacle(IObstacle obstacle, out int result)
        {
            result = 0;

            if (m_reqs.Count >= MAX_REQUESTS)
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
                Obstacle = obstacle,
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
        /// <summary>
        /// Adds a new remove request of an obstacle by reference
        /// </summary>
        /// <param name="r">Reference</param>
        public Status RemoveObstacle(int r)
        {
            if (r == 0 || m_reqs.Count >= MAX_REQUESTS)
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
        /// <summary>
        /// Returns whether the tile cache is updating or not
        /// </summary>
        public bool Updating()
        {
            return (m_update.Count + m_reqs.Count) > 0;
        }

        /// <summary>
        /// Updates the tile-cache
        /// </summary>
        /// <param name="upToDate">Returns true if the instance is up to date (No requests to perform)</param>
        public Status Update(out bool upToDate, out bool cacheUpdated)
        {
            bool updating = Updating();

            if (m_update.Count == 0)
            {
                // Process requests.
                ProcessRequests();
            }

            Status status = ProcessUpdates();

            upToDate = m_update.Count == 0 && m_reqs.Count == 0;
            cacheUpdated = updating != Updating();

            return status;
        }
        /// <summary>
        /// Process requests
        /// </summary>
        private void ProcessRequests()
        {
            if (!m_reqs.Any())
            {
                return;
            }

            // Copy requests & obstacles
            var reqs = m_reqs.ToArray();
            var obs = m_obstacles.ToArray();
            m_reqs.Clear();

            // Process requests.
            foreach (var req in reqs)
            {
                int idx = DecodeObstacleIdObstacle(req.NRef);
                if (idx >= m_params.MaxObstacles)
                {
                    continue;
                }
                var ob = obs[idx];
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
        }
        /// <summary>
        /// Procees add obstable request
        /// </summary>
        /// <param name="ob">Obstacle</param>
        private void ProcessRequestAdd(TileCacheObstacle ob)
        {
            // Find touched tiles.
            var bbox = ob.GetObstacleBounds();

            var tiles = QueryTiles(bbox, MAX_TOUCHED_TILES);
            ob.Touched.AddRange(tiles);

            // Add tiles to update list.
            ob.Pending.Clear();
            foreach (var touched in ob.Touched)
            {
                if (m_update.Count >= MAX_UPDATE)
                {
                    continue;
                }

                if (!m_update.Contains(touched))
                {
                    m_update.Add(touched);
                }
                ob.Pending.Add(touched);
            }
        }
        /// <summary>
        /// Procees remove obstable request
        /// </summary>
        /// <param name="ob">Obstacle</param>
        private void ProcessRequestRemove(TileCacheObstacle ob)
        {
            // Prepare to remove obstacle.
            ob.State = ObstacleState.DT_OBSTACLE_REMOVING;

            // Add tiles to update list.
            ob.Pending.Clear();
            foreach (var touched in ob.Touched)
            {
                if (m_update.Count >= MAX_UPDATE)
                {
                    continue;
                }

                if (!m_update.Contains(touched))
                {
                    m_update.Add(touched);
                }
                ob.Pending.Add(touched);
            }
        }
        /// <summary>
        /// Process updates
        /// </summary>
        private Status ProcessUpdates()
        {
            // Process updates
            if (m_update.Count == 0)
            {
                return Status.DT_SUCCESS;
            }

            Status status = Status.DT_SUCCESS;

            // Build mesh
            var r = m_update[0];
            if (!BuildTile(r))
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

                if (ob.ProcessUpdate(r, m_nextFreeObstacle))
                {
                    m_nextFreeObstacle = i;
                }
            }

            return status;
        }
        /// <summary>
        /// Query tiles
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="maxResults">Maximum results</param>
        /// <returns>Returns the compressed tiles into the bounding box</returns>
        private CompressedTile[] QueryTiles(BoundingBox bbox, int maxResults)
        {
            var results = new List<CompressedTile>();

            float tw = m_params.Width * m_params.CellSize;
            float th = m_params.Height * m_params.CellSize;
            int tx0 = (int)Math.Floor((bbox.Minimum.X - m_params.Origin.X) / tw);
            int tx1 = (int)Math.Floor((bbox.Maximum.X - m_params.Origin.X) / tw);
            int ty0 = (int)Math.Floor((bbox.Minimum.Z - m_params.Origin.Z) / th);
            int ty1 = (int)Math.Floor((bbox.Maximum.Z - m_params.Origin.Z) / th);

            for (int ty = ty0; ty <= ty1; ++ty)
            {
                for (int tx = tx0; tx <= tx1; ++tx)
                {
                    var tiles = GetTilesAt(tx, ty, MAX_TILES);

                    foreach (var tile in tiles)
                    {
                        CalcTightTileBounds(tile.Header, out Vector3 tbmin, out Vector3 tbmax);

                        if (Utils.OverlapBounds(bbox.Minimum, bbox.Maximum, tbmin, tbmax) && results.Count < maxResults)
                        {
                            results.Add(tile);
                        }
                    }
                }
            }

            return results.ToArray();
        }
        /// <summary>
        /// Calculates the tile header bounds
        /// </summary>
        /// <param name="header">Header</param>
        /// <param name="bmin">Resulting the minimum bounding box point</param>
        /// <param name="bmax">Resulting the maximum bounding box point</param>
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

        /// <summary>
        /// Builds the tiles at the specified coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public bool BuildTilesAt(int x, int y)
        {
            // Get all tiles
            var tiles = GetTilesAt(x, y, 0);

            bool res = true;
            foreach (var tile in tiles)
            {
                if (!BuildTile(tile))
                {
                    res = false;
                }
            }

            return res;
        }
        /// <summary>
        /// Builds the specified tile
        /// </summary>
        /// <param name="tile">Tile</param>
        private bool BuildTile(CompressedTile tile)
        {
            var bc = new NavMeshTileBuildContext
            {
                // Decompress tile layer data.
                Layer = tile.Decompress(),
            };

            // Reset untouched obstacles
            var obs = m_obstacles.Where(o => o.State == ObstacleState.DT_OBSTACLE_PROCESSED && !o.Touched.Any());
            foreach (var ob in obs)
            {
                ProcessRequestAdd(ob);
            }

            // Rasterize obstacles.
            for (int i = 0; i < m_params.MaxObstacles; ++i)
            {
                m_obstacles[i].Rasterize(bc, tile, m_params.CellSize, m_params.CellHeight);
            }

            int walkableClimbVx = (int)(m_params.WalkableClimb / m_params.CellHeight);

            // Build navmesh
            if (!bc.Layer.BuildTileCacheRegions(walkableClimbVx, out var layerRegs, out var regId))
            {
                return false;
            }
            bc.SetLayerRegs(layerRegs, regId);

            if (!bc.Layer.BuildTileCacheContours(walkableClimbVx, m_params.MaxSimplificationError, out var lcset))
            {
                return false;
            }
            bc.LCSet = lcset;

            bc.LMesh = TileCachePolyMesh.Build(bc.LCSet);

            // Early out if the mesh tile is empty.
            if (bc.LMesh.NPolys == 0)
            {
                // Remove existing tile.
                m_navMesh.RemoveTile(tile.Header.TX, tile.Header.TY, tile.Header.TLayer);

                return true;
            }

            var param = new NavMeshCreateParams
            {
                Verts = bc.LMesh.Verts,
                VertCount = bc.LMesh.NVerts,
                Polys = bc.LMesh.Polys,
                PolyAreas = bc.LMesh.Areas,
                PolyFlags = bc.LMesh.Flags,
                PolyCount = bc.LMesh.NPolys,
                Nvp = IndexedPolygon.DT_VERTS_PER_POLYGON,
                DetailMeshes = null,
                DetailVerts = null,
                DetailVertsCount = 0,
                DetailTris = null,
                DetailTriCount = 0,
                OffMeshCon = null,
                OffMeshConCount = 0,
                WalkableHeight = m_params.WalkableHeight,
                WalkableRadius = m_params.WalkableRadius,
                WalkableClimb = m_params.WalkableClimb,
                Bounds = tile.Header.BBox,
                CellSize = m_params.CellSize,
                CellHeight = m_params.CellHeight,
                BuildBvTree = false,

                TileX = tile.Header.TX,
                TileY = tile.Header.TY,
                TileLayer = tile.Header.TLayer,
            };

            m_tmproc?.Process(ref param, bc);

            // Remove existing tile.
            m_navMesh.RemoveTile(tile.Header.TX, tile.Header.TY, tile.Header.TLayer);

            var navData = MeshData.CreateNavMeshData(param);
            if (navData == null)
            {
                // Leave the location empty.
                return true;
            }

            // Add new tile
            return m_navMesh.AddTile(navData, TileFlagTypes.DT_TILE_FREE_DATA);
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
            return (r >> m_tileBits) & saltMask;
        }
        /// <summary>
        /// Decodes a tile id.
        /// </summary>
        private int DecodeTileIdTile(int r)
        {
            int tileMask = (1 << m_tileBits) - 1;
            return r & tileMask;
        }
    }
}
