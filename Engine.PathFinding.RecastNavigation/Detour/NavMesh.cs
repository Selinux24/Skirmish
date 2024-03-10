using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    using Engine.PathFinding.RecastNavigation.Detour.Tiles;
    using Engine.PathFinding.RecastNavigation.Recast;

    /// <summary>
    /// Navigation mesh
    /// </summary>
    public class NavMesh
    {
        /// <summary>
        /// Navigation parameters
        /// </summary>
        private NavMeshParams m_params;
        /// <summary>
        /// Origin
        /// </summary>
        private Vector3 m_orig;
        /// <summary>
        /// Tile width
        /// </summary>
        private readonly float m_tileWidth;
        /// <summary>
        /// Tile height
        /// </summary>
        private readonly float m_tileHeight;
        /// <summary>
        /// Tile lut mask
        /// </summary>
        private readonly int m_tileLutMask;
        /// <summary>
        /// Position look up list
        /// </summary>
        private readonly MeshTile[] m_posLookup;
        /// <summary>
        /// Next free tile
        /// </summary>
        private MeshTile m_nextFree = null;
        /// <summary>
        /// Tile bits
        /// </summary>
        private readonly int m_tileBits;
        /// <summary>
        /// Poly bits
        /// </summary>
        private readonly int m_polyBits;
        /// <summary>
        /// Salt bits
        /// </summary>
        private readonly int m_saltBits;
        /// <summary>
        /// Build data
        /// </summary>
        private BuildData m_buildData;

        /// <summary>
        /// Mesh tile list
        /// </summary>
        public MeshTile[] Tiles { get; private set; }
        /// <summary>
        /// Maximum tiles
        /// </summary>
        public int MaxTiles { get; private set; }
        /// <summary>
        /// Tile cache
        /// </summary>
        public TileCache TileCache { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public NavMesh(NavMeshParams nmparams)
        {
            m_params = nmparams;

            m_orig = m_params.Origin;
            m_tileWidth = m_params.TileWidth;
            m_tileHeight = m_params.TileHeight;

            // Init tiles
            MaxTiles = m_params.MaxTiles;
            var m_tileLutSize = Helper.NextPowerOfTwo(m_params.MaxTiles / 4);
            if (m_tileLutSize == 0)
            {
                m_tileLutSize = 1;
            }
            m_tileLutMask = m_tileLutSize - 1;

            Tiles = new MeshTile[MaxTiles];
            m_posLookup = new MeshTile[m_tileLutSize];

            m_nextFree = null;
            for (int i = MaxTiles - 1; i >= 0; --i)
            {
                Tiles[i] = new MeshTile
                {
                    Index = i,
                    Salt = 1,
                    Next = m_nextFree
                };
                m_nextFree = Tiles[i];
            }

            // Init ID generator values.
            m_tileBits = (int)Math.Log(Helper.NextPowerOfTwo(m_params.MaxTiles), 2);
            m_polyBits = (int)Math.Log(Helper.NextPowerOfTwo(m_params.MaxPolys), 2);
            // Only allow 31 salt bits, since the salt mask is calculated using 32bit uint and it will overflow.
            m_saltBits = Math.Min(31, 32 - m_tileBits - m_polyBits);

            if (m_saltBits < 10)
            {
                throw new EngineException("DT_INVALID_PARAM");
            }
        }

        /// <summary>
        /// Builds a new navigation mesh
        /// </summary>
        /// <param name="geometry">Input geometry</param>
        /// <param name="settings">Settings</param>
        /// <param name="agent">Agent type</param>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>Returns the new created navigation mesh</returns>
        public static NavMesh Build(InputGeometry geometry, BuildSettings settings, Agent agent, Action<float> progressCallback = null)
        {
            if (settings.BuildMode == BuildModes.Solo)
            {
                return BuildSolo(geometry, settings, agent, progressCallback);
            }
            else if (settings.BuildMode == BuildModes.Tiled)
            {
                return BuildTiled(geometry, settings, agent, progressCallback);
            }
            else
            {
                throw new EngineException("Bad build mode for NavigationMesh.");
            }
        }
        /// <summary>
        /// Builds a solo navigation mesh
        /// </summary>
        /// <param name="geometry">Input geometry</param>
        /// <param name="settings">Settings</param>
        /// <param name="agent">Agent type</param>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>Returns the new created navigation mesh</returns>
        private static NavMesh BuildSolo(InputGeometry geometry, BuildSettings settings, Agent agent, Action<float> progressCallback)
        {
            var bbox = settings.Bounds ?? geometry.BoundingBox;

            // Progress -> pass count
            const int passCount = 13;

            // Generation params.
            var cfg = SoloConfig.GetConfig(settings, agent, bbox);

            var solid = Heightfield.Build(cfg);
            progressCallback?.Invoke(1f / passCount);

            var tris = geometry.ChunkyMesh.GetTriangles();
            if (!solid.Rasterize(tris, cfg.WalkableSlopeAngle, cfg.WalkableClimb))
            {
                throw new EngineException("rcRasterizeTriangles: Out of memory.");
            }
            progressCallback?.Invoke(2f / passCount);

            // Performs the heightfield filters
            solid.FilterHeightfield(cfg);
            progressCallback?.Invoke(3f / passCount);

            // Compact the heightfield so that it is faster to handle from now on.
            // This will result more cache coherent data as well as the neighbours
            // between walkable cells will be calculated.
            var chf = solid.Build(cfg.WalkableHeight, cfg.WalkableClimb);
            progressCallback?.Invoke(4f / passCount);

            // Erode the walkable area by agent radius.
            chf.ErodeWalkableArea(cfg.WalkableRadius);
            progressCallback?.Invoke(5f / passCount);

            // Mark areas.
            chf.MarkAreas(geometry);
            progressCallback?.Invoke(6f / passCount);

            // Sample partition
            chf.SamplePartition(cfg);
            progressCallback?.Invoke(7f / passCount);

            // Create contours.
            var cset = ContourSet.Build(chf, cfg.MaxSimplificationError, cfg.MaxEdgeLen, BuildContoursFlagTypes.RC_CONTOUR_TESS_WALL_EDGES);
            progressCallback?.Invoke(8f / passCount);

            var pmesh = PolyMesh.Build(cset, cfg.MaxVertsPerPoly);
            progressCallback?.Invoke(9f / passCount);

            // Build polygon navmesh from the contours.
            var dmesh = PolyMeshDetail.Build(pmesh, chf, cfg.DetailSampleDist, cfg.DetailSampleMaxError) ?? throw new EngineException("buildNavigation: Could not build detail mesh.");
            if (cfg.MaxVertsPerPoly > IndexedPolygon.DT_VERTS_PER_POLYGON)
            {
                throw new EngineException($"buildNavigation: {cfg.MaxVertsPerPoly} is bigger than {nameof(IndexedPolygon.DT_VERTS_PER_POLYGON)} ({IndexedPolygon.DT_VERTS_PER_POLYGON}).");
            }
            progressCallback?.Invoke(10f / passCount);

            // Update poly flags from areas.
            pmesh.UpdatePolyFlags();
            progressCallback?.Invoke(11f / passCount);

            var param = new NavMeshCreateParams
            {
                Verts = pmesh.Verts,
                VertCount = pmesh.NVerts,
                Polys = pmesh.Polys,
                PolyAreas = pmesh.Areas,
                PolyFlags = pmesh.Flags,
                PolyCount = pmesh.NPolys,
                Nvp = pmesh.NVP,
                DetailMeshes = [.. dmesh.Meshes],
                DetailVerts = [.. dmesh.Vertices],
                DetailVertsCount = dmesh.Vertices.Count,
                DetailTris = [.. dmesh.Triangles],
                DetailTriCount = dmesh.Triangles.Count,
                OffMeshCon = geometry.GetConnections()?.ToArray(),
                OffMeshConCount = geometry.GetConnectionCount(),
                WalkableHeight = cfg.Agent.Height,
                WalkableRadius = cfg.Agent.Radius,
                WalkableClimb = cfg.Agent.MaxClimb,
                Bounds = pmesh.Bounds,
                CellSize = cfg.CellSize,
                CellHeight = cfg.CellHeight,
                BuildBvTree = true,
            };

            var navData = MeshData.CreateNavMeshData(param) ?? throw new EngineException("Could not build Detour navmesh.");
            progressCallback?.Invoke(12f / passCount);

            // Make sure the data is in right format.
            var header = navData.Header;
            if (!header.IsValid())
            {
                throw new EngineException($"Bad header. {header.Magic}-{header.Version}");
            }

            var nvParams = SoloConfig.GetNavMeshParams(header.Bounds, header.PolyCount);

            var nm = new NavMesh(nvParams);
            nm.AddTile(navData, TileFlagTypes.DT_TILE_FREE_DATA);
            progressCallback?.Invoke(13f / passCount);

            if (settings.EnableDebugInfo)
            {
                nm.m_buildData = new()
                {
                    Heightfield = solid,
                };
            }

            return nm;
        }
        /// <summary>
        /// Builds a tiled navigation mesh
        /// </summary>
        /// <param name="geometry">Input geometry</param>
        /// <param name="settings">Settings</param>
        /// <param name="agent">Agent type</param>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>Returns the new created navigation mesh</returns>
        private static NavMesh BuildTiled(InputGeometry geometry, BuildSettings settings, Agent agent, Action<float> progressCallback)
        {
            var bbox = settings.Bounds ?? geometry.BoundingBox;

            var nmParams = TilesConfig.GetNavMeshParams(settings, bbox);
            var nm = new NavMesh(nmParams);

            int passes = 0;
            if (settings.BuildAllTiles)
            {
                passes++;

                if (settings.UseTileCache)
                {
                    passes++;
                }
            }

            if (settings.BuildAllTiles)
            {
                nm.BuildAllTiles(geometry, settings, agent, (progress) =>
                {
                    progressCallback?.Invoke(progress / passes);
                });
            }

            if (settings.UseTileCache)
            {
                // Init cache

                // Generation params.
                var cfg = TilesConfig.GetTileCacheConfig(settings, agent, bbox);

                nm.CreateTileCache(geometry, cfg.TileCacheParams);

                if (settings.BuildAllTiles)
                {
                    nm.TileCache.BuildTileCache(geometry, cfg, (progress) =>
                    {
                        progressCallback?.Invoke((progress / passes) + (1f / passes));
                    });
                }
            }

            return nm;
        }
        /// <summary>
        /// Builds the mesh data
        /// </summary>
        /// <param name="x">X tile coordinate</param>
        /// <param name="y">Y tile coordinate</param>
        /// <param name="geometry">Input geometry</param>
        /// <param name="cfg">Configuration</param>
        private static MeshData BuildTileMesh(int x, int y, InputGeometry geometry, TilesConfig cfg)
        {
            if (cfg.MaxVertsPerPoly > IndexedPolygon.DT_VERTS_PER_POLYGON)
            {
                return null;
            }

            var chunkyMesh = geometry.ChunkyMesh;

            // Allocate voxel heightfield where we rasterize our input data to.
            var solid = Heightfield.Build(cfg);

            var tbmin = new Vector2(cfg.BoundingBox.Minimum.X, cfg.BoundingBox.Minimum.Z);
            var tbmax = new Vector2(cfg.BoundingBox.Maximum.X, cfg.BoundingBox.Maximum.Z);
            var cid = chunkyMesh.GetChunksOverlappingRect(tbmin, tbmax);
            if (cid.Length == 0)
            {
                return null; // empty
            }

            foreach (var id in cid)
            {
                var tris = chunkyMesh.GetTriangles(id);
                if (!solid.Rasterize(tris, cfg.WalkableSlopeAngle, cfg.WalkableClimb))
                {
                    throw new EngineException("rcRasterizeTriangles: Out of memory.");
                }
            }

            // Performs the heightfield filters
            solid.FilterHeightfield(cfg);

            // Compact the heightfield so that it is faster to handle from now on.
            // This will result more cache coherent data as well as the neighbours
            // between walkable cells will be calculated.
            var chf = solid.Build(cfg.WalkableHeight, cfg.WalkableClimb);

            // Erode the walkable area by agent radius.
            chf.ErodeWalkableArea(cfg.WalkableRadius);

            // Mark areas.
            chf.MarkAreas(geometry);

            // Sample partition
            chf.SamplePartition(cfg);

            // Create contours.
            var cset = ContourSet.Build(chf, cfg.MaxSimplificationError, cfg.MaxEdgeLen, BuildContoursFlagTypes.RC_CONTOUR_TESS_WALL_EDGES);
            if (cset.NConts == 0)
            {
                return null;
            }

            // Build polygon navmesh from the contours.
            var pmesh = PolyMesh.Build(cset, cfg.MaxVertsPerPoly);

            // Build detail mesh.
            var dmesh = PolyMeshDetail.Build(pmesh, chf, cfg.DetailSampleDist, cfg.DetailSampleMaxError);
            if (dmesh == null)
            {
                return null;
            }

            // Update poly flags from areas.
            pmesh.UpdatePolyFlags();

            var param = new NavMeshCreateParams
            {
                Verts = pmesh.Verts,
                VertCount = pmesh.NVerts,
                Polys = pmesh.Polys,
                PolyAreas = pmesh.Areas,
                PolyFlags = pmesh.Flags,
                PolyCount = pmesh.NPolys,
                Nvp = pmesh.NVP,
                DetailMeshes = [.. dmesh.Meshes],
                DetailVerts = [.. dmesh.Vertices],
                DetailVertsCount = dmesh.Vertices.Count,
                DetailTris = [.. dmesh.Triangles],
                DetailTriCount = dmesh.Triangles.Count,
                OffMeshCon = geometry.GetConnections().ToArray(),
                OffMeshConCount = geometry.GetConnectionCount(),
                WalkableHeight = cfg.Agent.Height,
                WalkableRadius = cfg.Agent.Radius,
                WalkableClimb = cfg.Agent.MaxClimb,
                TileX = x,
                TileY = y,
                TileLayer = 0,
                Bounds = pmesh.Bounds,
                CellSize = cfg.CellSize,
                CellHeight = cfg.CellHeight,
                BuildBvTree = true
            };

            var meshData = MeshData.CreateNavMeshData(param);

            if (cfg.EnableDebugInfo)
            {
                meshData.BuildData = new()
                {
                    Heightfield = solid,
                };
            }

            return meshData;
        }
        /// <summary>
        /// Gets the tile located at the specified position
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="geom">Input geometry</param>
        /// <param name="settings">Build settings</param>
        /// <param name="x">Resulting x tile coordinate</param>
        /// <param name="y">Resulting y tile coordinate</param>
        /// <param name="tileBounds">Tile bounds</param>
        public static void GetTileAtPosition(Vector3 pos, InputGeometry geom, BuildSettings settings, out int x, out int y, out BoundingBox tileBounds)
        {
            var bbox = settings.Bounds ?? geom.BoundingBox;

            x = (int)((pos.X - bbox.Minimum.X) / settings.TileCellSize);
            y = (int)((pos.Z - bbox.Minimum.Z) / settings.TileCellSize);

            tileBounds = GetTileBounds(x, y, settings.TileCellSize, bbox);
        }
        /// <summary>
        /// Gets the specified tile bounding box
        /// </summary>
        /// <param name="x">X tile coordinate</param>
        /// <param name="y">Y tile coordinate</param>
        /// <param name="geom">Input geometry</param>
        /// <param name="settings">Build settings</param>
        public static BoundingBox GetTileBounds(int x, int y, InputGeometry geom, BuildSettings settings)
        {
            return GetTileBounds(x, y, settings.TileCellSize, settings.Bounds ?? geom.BoundingBox);
        }
        /// <summary>
        /// Gets the specified tile bounding box
        /// </summary>
        /// <param name="x">X tile coordinate</param>
        /// <param name="y">Y tile coordinate</param>
        /// <param name="tileSize">Tile size</param>
        /// <param name="bbox">Bounding box</param>
        private static BoundingBox GetTileBounds(int x, int y, float tileSize, BoundingBox bbox)
        {
            var tbbox = new BoundingBox();

            tbbox.Minimum.X = bbox.Minimum.X + x * tileSize;
            tbbox.Minimum.Y = bbox.Minimum.Y;
            tbbox.Minimum.Z = bbox.Minimum.Z + y * tileSize;

            tbbox.Maximum.X = bbox.Minimum.X + (x + 1) * tileSize;
            tbbox.Maximum.Y = bbox.Maximum.Y;
            tbbox.Maximum.Z = bbox.Minimum.Z + (y + 1) * tileSize;

            return tbbox;
        }
        /// <summary>
        /// Check for horizontal overlap.
        /// </summary>
        private static bool OverlapSlabs(Vector2 amin, Vector2 amax, Vector2 bmin, Vector2 bmax, float px, float py)
        {
            // Check for horizontal overlap.
            // The segment is shrunken a little so that slabs which touch
            // at end points are not connected.
            float minx = Math.Max(amin.X + px, bmin.X + px);
            float maxx = Math.Min(amax.X - px, bmax.X - px);
            if (minx > maxx)
            {
                return false;
            }

            // Check vertical overlap.
            float ad = (amax.Y - amin.Y) / (amax.X - amin.X);
            float ak = amin.Y - ad * amin.X;
            float bd = (bmax.Y - bmin.Y) / (bmax.X - bmin.X);
            float bk = bmin.Y - bd * bmin.X;
            float aminy = ad * minx + ak;
            float amaxy = ad * maxx + ak;
            float bminy = bd * minx + bk;
            float bmaxy = bd * maxx + bk;
            float dmin = bminy - aminy;
            float dmax = bmaxy - amaxy;

            // Crossing segments always overlap.
            if (dmin * dmax < 0)
            {
                return true;
            }

            // Check for overlap at endpoints.
            float thr = (float)Math.Sqrt(py * 2);
            if (dmin * dmin <= thr || dmax * dmax <= thr)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Gets the slab coordinate
        /// </summary>
        private static float GetSlabCoord(Vector3 va, int side)
        {
            if (side == 0 || side == 4)
            {
                return va.X;
            }
            else if (side == 2 || side == 6)
            {
                return va.Z;
            }
            return 0;
        }
        /// <summary>
        /// Calculate slabs end points
        /// </summary>
        private static void CalcSlabEndPoints(Vector3 va, Vector3 vb, out Vector2 bmin, out Vector2 bmax, int side)
        {
            bmin = new Vector2();
            bmax = new Vector2();

            if (side == 0 || side == 4)
            {
                if (va.Z < vb.Z)
                {
                    bmin.X = va.Z;
                    bmin.Y = va.Y;
                    bmax.X = vb.Z;
                    bmax.Y = vb.Y;
                }
                else
                {
                    bmin.X = vb.Z;
                    bmin.Y = vb.Y;
                    bmax.X = va.Z;
                    bmax.Y = va.Y;
                }
            }
            else if (side == 2 || side == 6)
            {
                if (va.X < vb.X)
                {
                    bmin.X = va.X;
                    bmin.Y = va.Y;
                    bmax.X = vb.X;
                    bmax.Y = vb.Y;
                }
                else
                {
                    bmin.X = vb.X;
                    bmin.Y = vb.Y;
                    bmax.X = va.X;
                    bmax.Y = va.Y;
                }
            }
        }
        /// <summary>
        /// Gets the opposite tile of the specified side
        /// </summary>
        /// <param name="side">Side index</param>
        private static int OppositeTile(int side)
        {
            return (side + 4) & 0x7;
        }

        /// <summary>
        /// Builds all the tiles in the mesh
        /// </summary>
        /// <param name="geometry">Input geometry</param>
        /// <param name="settings">Build settings</param>
        /// <param name="agent">Agent type</param>
        /// <param name="progressCallback">Optional progress callback</param>
        private void BuildAllTiles(InputGeometry geometry, BuildSettings settings, Agent agent, Action<float> progressCallback)
        {
            var bbox = settings.Bounds ?? geometry.BoundingBox;

            // Tile generation params.
            var tileParams = TilesConfig.GetTileParams(settings, bbox);

            float totalTiles = tileParams.Height * tileParams.Width;
            int tile = 0;

            for (int y = 0; y < tileParams.Height; y++)
            {
                for (int x = 0; x < tileParams.Width; x++)
                {
                    var tileBounds = GetTileBounds(x, y, tileParams.CellSize, tileParams.Bounds);

                    // Init build configuration
                    var cfg = TilesConfig.GetConfig(settings, agent, tileBounds);
                    cfg.EnableDebugInfo = settings.EnableDebugInfo;

                    var data = BuildTileMesh(x, y, geometry, cfg);
                    if (data != null)
                    {
                        // Remove any previous data (navmesh owns and deletes the data).
                        RemoveTile(x, y, 0);
                        // Let the navmesh own the data.
                        AddTile(data, TileFlagTypes.DT_TILE_FREE_DATA);
                    }

                    progressCallback?.Invoke(++tile / totalTiles);
                }
            }
        }

        /// <summary>
        /// Gets the navigation mesh parameters
        /// </summary>
        public NavMeshParams GetParams()
        {
            return m_params;
        }

        /// <summary>
        /// Builds all tiles at specified tile coordinates
        /// </summary>
        /// <param name="x">X tile coordinate</param>
        /// <param name="y">Y tile coordinate</param>
        /// <param name="geometry">Input geometry</param>
        /// <param name="tiledCfg">Tiled generation config</param>
        /// <param name="tileCacheCfg">Tile cache generation config</param>
        public void BuildTileAtPosition(int x, int y, InputGeometry geometry, TilesConfig tiledCfg, TilesConfig tileCacheCfg)
        {
            // Remove any previous data (navmesh owns and deletes the data).
            RemoveTiles(x, y);

            // Add tile, or leave the location empty.
            var data = BuildTileMesh(x, y, geometry, tiledCfg);
            if (data != null)
            {
                AddTile(data, TileFlagTypes.DT_TILE_FREE_DATA);
            }

            if (TileCache == null)
            {
                return;
            }

            var tiles = TileCacheData.RasterizeTileLayers(x, y, geometry, tileCacheCfg);

            foreach (var tile in tiles)
            {
                TileCache.AddTile(tile, CompressedTileFlagTypes.DT_COMPRESSEDTILE_FREE_DATA, false);
            }

            TileCache.BuildTilesAt(x, y);
        }
        /// <summary>
        /// Removes all tiles at specified tile coordinates
        /// </summary>
        /// <param name="x">X tile coordinate</param>
        /// <param name="y">Y tile coordinate</param>
        public void RemoveTilesAtPosition(int x, int y)
        {
            RemoveTiles(x, y);

            TileCache?.RemoveTiles(x, y);
        }

        /// <summary>
        /// Adds a new tile to the navigation mesh
        /// </summary>
        /// <param name="data">Mesh data</param>
        /// <param name="flags">Tile flags</param>
        /// <returns>Returns true if the tile was added</returns>
        public bool AddTile(MeshData data, TileFlagTypes flags)
        {
            // Make sure the data is in right format.
            var header = data.Header;
            if (!header.IsValid())
            {
                return false;
            }

            // Make sure the location is free.
            if (GetTileAt(header.X, header.Y, header.Layer) != null)
            {
                return false;
            }

            // Allocate a tile.
            MeshTile tile = AllocateTile();

            // Make sure we could allocate a tile.
            if (tile == null)
            {
                return false;
            }

            // Insert tile into the position lut.
            int h = Utils.ComputeTileHash(header.X, header.Y, m_tileLutMask);
            tile.Next = m_posLookup[h];
            m_posLookup[h] = tile;

            tile.Patch(header);

            // If there are no items in the bvtree, reset the tree pointer.
            if (data.NavBvtree == null)
            {
                tile.BvTree = null;
            }

            // Build links freelist
            tile.LinksFreeList = 0;
            tile.Links[header.MaxLinkCount - 1].Next = MeshTile.DT_NULL_LINK;
            for (int i = 0; i < header.MaxLinkCount - 1; ++i)
            {
                tile.Links[i].Next = i + 1;
            }

            // Init tile.
            tile.Header = header;
            tile.SetData(data);
            tile.Flags = flags;

            ConnectIntLinks(tile);

            // Base off-mesh connections to their starting polygons and connect connections inside the tile.
            BaseOffMeshLinks(tile);
            ConnectExtOffMeshLinks(tile, tile, -1);

            // Create connections with neighbour tiles.
            int MAX_NEIS = 32;

            // Connect with layers in current tile.
            var neis = GetTilesAt(header.X, header.Y, MAX_NEIS);
            foreach (var nei in neis)
            {
                if (nei == tile)
                {
                    continue;
                }

                ConnectExtLinks(tile, nei, -1);
                ConnectExtLinks(nei, tile, -1);
                ConnectExtOffMeshLinks(tile, nei, -1);
                ConnectExtOffMeshLinks(nei, tile, -1);
            }

            // Connect with neighbour tiles.
            for (int i = 0; i < 8; i++)
            {
                var sideNeis = GetNeighbourTilesAt(header.X, header.Y, i, MAX_NEIS);
                foreach (var nei in sideNeis)
                {
                    int opposite = OppositeTile(i);

                    ConnectExtLinks(tile, nei, i);
                    ConnectExtLinks(nei, tile, opposite);
                    ConnectExtOffMeshLinks(tile, nei, i);
                    ConnectExtOffMeshLinks(nei, tile, opposite);
                }
            }

            return true;
        }
        /// <summary>
        /// Allocates a new tile
        /// </summary>
        /// <returns>Returns the new tile</returns>
        private MeshTile AllocateTile()
        {
            MeshTile tile = null;

            if (m_nextFree != null)
            {
                tile = m_nextFree;
                m_nextFree = tile.Next;
                tile.Next = null;
            }

            return tile;
        }

        /// <summary>
        /// Removes the tile from the navigation mesh
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="layer">Layer number</param>
        /// <returns>Returns true if the tile was removed or if the tile not exists at all</returns>
        public bool RemoveTile(int x, int y, int layer)
        {
            var meshTile = GetTileAt(x, y, layer);
            if (meshTile == null)
            {
                return true;
            }

            return RemoveTile(meshTile);
        }
        /// <summary>
        /// Removes the tile from the navigation mesh
        /// </summary>
        /// <param name="tile">Tile to remove</param>
        /// <returns>Returns true if the tile was removed</returns>
        public bool RemoveTile(MeshTile tile)
        {
            if (tile == null)
            {
                return false;
            }

            // Remove tile from hash lookup.
            RemoveFromHashLookup(tile);

            // Remove connections to neighbour tiles.
            RemoveConnections(tile);

            // Reset tile.
            if ((tile.Flags & TileFlagTypes.DT_TILE_FREE_DATA) != 0)
            {
                // Owns data
                tile.Data = null;
                tile.DataSize = 0;
            }

            tile.Header = new MeshHeader();
            tile.Flags = 0;
            tile.LinksFreeList = 0;
            tile.Polys = null;
            tile.Verts = null;
            tile.Links = null;
            tile.DetailMeshes = null;
            tile.DetailVerts = null;
            tile.DetailTris = null;
            tile.BvTree = null;
            tile.OffMeshCons = null;

            // Update salt, salt should never be zero.
            tile.Salt = (tile.Salt + 1) & ((1 << m_saltBits) - 1);
            if (tile.Salt == 0)
            {
                tile.Salt++;
            }

            // Add to free list.
            tile.Next = m_nextFree;
            m_nextFree = tile;

            return true;
        }
        /// <summary>
        /// Removes all the tiles in the coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Returns true if all the tiles were removed or if the tiles not exists at all</returns>
        public bool RemoveTiles(int x, int y)
        {
            bool res = true;

            var tiles = GetTilesAt(x, y, 32);
            foreach (var t in tiles)
            {
                if (!RemoveTile(t))
                {
                    res = false;
                }
            }

            return res;
        }
        /// <summary>
        /// Removes the tile from the lookup
        /// </summary>
        /// <param name="tile">Tile</param>
        private void RemoveFromHashLookup(MeshTile tile)
        {
            int h = Utils.ComputeTileHash(tile.Header.X, tile.Header.Y, m_tileLutMask);
            MeshTile prev = null;
            MeshTile cur = m_posLookup[h];
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
        }
        /// <summary>
        /// Removes tile connections
        /// </summary>
        /// <param name="tile">Tile</param>
        private void RemoveConnections(MeshTile tile)
        {
            int MAX_NEIS = 32;

            // Disconnect from other layers in current tile.
            var neis = GetTilesAt(tile.Header.X, tile.Header.Y, MAX_NEIS);
            foreach (var nei in neis)
            {
                if (nei == tile)
                {
                    continue;
                }

                UnconnectLinks(nei, tile);
            }

            // Disconnect from neighbour tiles.
            for (int i = 0; i < 8; i++)
            {
                var sideNeis = GetNeighbourTilesAt(tile.Header.X, tile.Header.Y, i, MAX_NEIS);
                foreach (var nei in sideNeis)
                {
                    UnconnectLinks(nei, tile);
                }
            }
        }

        /// <summary>
        /// Gets the tile location bt position
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="x">Resulting X coordinate</param>
        /// <param name="y">Resulting Y coordinate</param>
        public void CalcTileLoc(Vector3 pos, out int x, out int y)
        {
            x = (int)Math.Floor((pos.X - m_orig.X) / m_tileWidth);
            y = (int)Math.Floor((pos.Z - m_orig.Z) / m_tileHeight);
        }
        /// <summary>
        /// Gets whether exists or not tiles at location
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Returns true if has tiles</returns>
        public bool HasTilesAt(int x, int y)
        {
            // Find tile based on hash.
            int h = Utils.ComputeTileHash(x, y, m_tileLutMask);
            var tile = m_posLookup[h];

            return tile != null;
        }
        /// <summary>
        /// Gets the tile at specified coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="layer">Layer</param>
        /// <returns>Returns the tile</returns>
        public MeshTile GetTileAt(int x, int y, int layer)
        {
            // Find tile based on hash.
            int h = Utils.ComputeTileHash(x, y, m_tileLutMask);
            var tile = m_posLookup[h];
            while (tile != null)
            {
                if (tile.Header.X == x &&
                    tile.Header.Y == y &&
                    tile.Header.Layer == layer)
                {
                    return tile;
                }
                tile = tile.Next;
            }
            return null;
        }
        /// <summary>
        /// Gets the tiles at specified coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="maxTiles">Maximum number of resulting tiles</param>
        /// <returns>Returns a tile collection</returns>
        public MeshTile[] GetTilesAt(int x, int y, int maxTiles)
        {
            var tiles = new List<MeshTile>();

            // Find tile based on hash.
            int h = Utils.ComputeTileHash(x, y, m_tileLutMask);
            var tile = m_posLookup[h];

            while (tile != null)
            {
                if (tile.Header.X == x && tile.Header.Y == y && tiles.Count < maxTiles)
                {
                    tiles.Add(tile);
                }

                tile = tile.Next;
            }

            return [.. tiles];
        }
        /// <summary>
        /// Gets the tile reference of the specified tile
        /// </summary>
        /// <param name="tile">Tile</param>
        /// <returns>Returns the tile reference</returns>
        public int GetTileRef(MeshTile tile)
        {
            if (tile == null) return 0;
            int it = Array.IndexOf(Tiles, tile);
            return EncodePolyId(tile.Salt, it, 0);
        }
        /// <summary>
        /// Gets the polygon by reference
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <returns>Returns the tile</returns>
        public MeshTile GetTileByRef(int r)
        {
            if (r == 0)
            {
                return null;
            }
            int tileIndex = DecodePolyIdTile(r);
            int tileSalt = DecodePolyIdSalt(r);
            if (tileIndex >= MaxTiles)
            {
                return null;
            }
            var tile = Tiles[tileIndex];
            if (tile.Salt != tileSalt)
            {
                return null;
            }
            return tile;
        }

        /// <summary>
        /// Gets the tile descriptor by node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns the tile descriptor for the node</returns>
        public TileRef GetTileAndPolyByNode(Node node)
        {
            TileRef res = GetTileAndPolyByRef(node.Id);
            res.Node = node;
            return res;
        }
        /// <summary>
        /// Gets the tile descriptor by node without verifications
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns the tile descriptor for the node</returns>
        public TileRef GetTileAndPolyByNodeUnsafe(Node node)
        {
            TileRef res = GetTileAndPolyByRefUnsafe(node.Id);
            res.Node = node;
            return res;
        }
        /// <summary>
        /// Gets the tile descriptor by reference
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <returns>Returns the tile descriptor for the reference</returns>
        public TileRef GetTileAndPolyByRef(int r)
        {
            if (!IsValidPolyRef(r, out var it, out var ip))
            {
                return TileRef.Null;
            }

            return new TileRef
            {
                Ref = r,
                Tile = Tiles[it],
                Poly = Tiles[it].Polys[ip],
            };
        }
        /// <summary>
        /// Gets the tile descriptor by reference without verifications
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <returns>Returns the tile descriptor for the reference</returns>
        public TileRef GetTileAndPolyByRefUnsafe(int r)
        {
            DecodePolyId(r, out _, out int it, out int ip);

            return new TileRef
            {
                Ref = r,
                Tile = Tiles[it],
                Poly = Tiles[it].Polys[ip],
            };
        }

        /// <summary>
        /// Gets whether the reference is valid or not
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <returns>Returns true if the reference is valid</returns>
        public bool IsValidPolyRef(int r)
        {
            return IsValidPolyRef(r, out _, out _);
        }
        /// <summary>
        /// Gets whether the reference is valid or not
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <param name="it">Resulting tile index</param>
        /// <param name="ip">Resulting polygon index</param>
        /// <returns>Returns true if the reference is valid</returns>
        public bool IsValidPolyRef(int r, out int it, out int ip)
        {
            it = 0;
            ip = 0;

            if (r == 0) return false;

            DecodePolyId(r, out int salt, out it, out ip);

            if (it >= MaxTiles) return false;
            if (Tiles[it].Salt != salt || !Tiles[it].Header.IsValid()) return false;
            if (ip >= Tiles[it].Header.PolyCount) return false;

            return true;
        }

        /// <summary>
        /// Gets the end points of the off-mesh connections
        /// </summary>
        /// <param name="prevRef">Previous reference</param>
        /// <param name="polyRef">Polygon reference</param>
        /// <param name="startPos">Starting position</param>
        /// <param name="endPos">End position</param>
        /// <returns>Returns true if the end points were found</returns>
        public bool GetOffMeshConnectionPolyEndPoints(int prevRef, int polyRef, out Vector3 startPos, out Vector3 endPos)
        {
            startPos = Vector3.Zero;
            endPos = Vector3.Zero;

            // Get current polygon
            if (!IsValidPolyRef(polyRef, out int it, out int ip))
            {
                return false;
            }

            var tile = Tiles[it];
            var poly = tile.Polys[ip];

            // Figure out which way to hand out the vertices.
            return tile.FindOffMeshConnectionEndpoints(poly, prevRef, out startPos, out endPos);
        }
        /// <summary>
        /// Gets the off-mesh connection by polygon reference
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <returns>Returns the off-mesh connection</returns>
        public OffMeshConnection GetOffMeshConnectionByRef(int r)
        {
            // Get current polygon
            if (!IsValidPolyRef(r, out int it, out int ip))
            {
                return null;
            }
            var tile = Tiles[it];
            return tile.GetOffMeshConnectionByPolygon(ip);
        }

        /// <summary>
        /// Sets the polygon flags by reference
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <param name="flags">Polygon flags</param>
        /// <returns>Returns true if the polygon were found</returns>
        public bool SetPolyFlags(int r, SamplePolyFlagTypes flags)
        {
            if (!IsValidPolyRef(r, out int it, out int ip))
            {
                return false;
            }
            var tile = Tiles[it];
            var poly = tile.Polys[ip];

            // Change flags.
            poly.Flags = flags;

            return true;
        }
        /// <summary>
        /// Gets the polygon flags by reference
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <param name="resultFlags">Resulting flags</param>
        /// <returns>Returns true if the polygon were found</returns>
        public bool GetPolyFlags(int r, out SamplePolyFlagTypes resultFlags)
        {
            resultFlags = 0;

            if (!IsValidPolyRef(r, out int it, out int ip))
            {
                return false;
            }
            var tile = Tiles[it];
            var poly = tile.Polys[ip];

            resultFlags = poly.Flags;

            return true;
        }

        /// <summary>
        /// Sets the polygon area by reference
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <param name="area">Sample area</param>
        /// <returns>Returns true if the polygon were found</returns>
        public bool SetPolyArea(int r, SamplePolyAreas area)
        {
            if (!IsValidPolyRef(r, out int it, out int ip))
            {
                return false;
            }
            var tile = Tiles[it];
            var poly = tile.Polys[ip];

            poly.Area = area;

            return true;
        }
        /// <summary>
        /// Gets the polygon area by reference
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <param name="resultArea">Resulting sample area</param>
        /// <returns>Returns true if the polygon were found</returns>
        public bool GetPolyArea(int r, out SamplePolyAreas resultArea)
        {
            resultArea = 0;

            if (!IsValidPolyRef(r, out int it, out int ip))
            {
                return false;
            }
            var tile = Tiles[it];
            var poly = tile.Polys[ip];

            resultArea = poly.Area;

            return true;
        }

        /// <summary>
        /// Encodes polygon
        /// </summary>
        /// <param name="salt">Salt value</param>
        /// <param name="it">Tile index</param>
        /// <param name="ip">Polygon index</param>
        /// <returns>Returns the polygon reference</returns>
        public int EncodePolyId(int salt, int it, int ip)
        {
            return (salt << (m_polyBits + m_tileBits)) | (it << m_polyBits) | ip;
        }
        /// <summary>
        /// Decodes the polygon reference
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <param name="salt">Salt value</param>
        /// <param name="it">Tile index</param>
        /// <param name="ip">Polygon index</param>
        public void DecodePolyId(int r, out int salt, out int it, out int ip)
        {
            salt = DecodePolyIdSalt(r);
            it = DecodePolyIdTile(r);
            ip = DecodePolyIdPoly(r);
        }
        /// <summary>
        /// Decodes the polygon reference
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <returns>Returns the salt value</returns>
        public int DecodePolyIdSalt(int r)
        {
            int saltMask = (1 << m_saltBits) - 1;
            return (r >> (m_polyBits + m_tileBits)) & saltMask;
        }
        /// <summary>
        /// Decodes the polygon reference
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <returns>Returns the tile index</returns>
        public int DecodePolyIdTile(int r)
        {
            int tileMask = (1 << m_tileBits) - 1;
            return (r >> m_polyBits) & tileMask;
        }
        /// <summary>
        /// Decodes the polygon reference
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <returns>Returns the polygon index</returns>
        public int DecodePolyIdPoly(int r)
        {
            int polyMask = (1 << m_polyBits) - 1;
            return r & polyMask;
        }

        /// <summary>
        /// Get neighbour tiles at the specified coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="side">Side number</param>
        /// <param name="maxTiles">Maximum resulting tiles</param>
        /// <returns>Returns a tile collection</returns>
        private MeshTile[] GetNeighbourTilesAt(int x, int y, int side, int maxTiles)
        {
            int nx = x;
            int ny = y;

            switch (side)
            {
                case 0: nx++; break;
                case 1: nx++; ny++; break;
                case 2: ny++; break;
                case 3: nx--; ny++; break;
                case 4: nx--; break;
                case 5: nx--; ny--; break;
                case 6: ny--; break;
                case 7: nx++; ny--; break;
            }

            return GetTilesAt(nx, ny, maxTiles);
        }
        /// <summary>
        /// Finds connecting polygons of the specified end points
        /// </summary>
        /// <param name="va">End point A vertex</param>
        /// <param name="vb">End point B vertex</param>
        /// <param name="tile">Tile</param>
        /// <param name="side">Side number</param>
        /// <param name="maxcon">Maximum resulting connections</param>
        /// <param name="con">Resulting connections</param>
        /// <param name="conarea">Resulting connection areas</param>
        private void FindConnectingPolys(Vector3 va, Vector3 vb, MeshTile tile, int side, int maxcon, out int[] con, out Vector2[] conarea)
        {
            var conList = new List<int>();
            var conareaList = new List<Vector2>();

            CalcSlabEndPoints(va, vb, out Vector2 amin, out Vector2 amax, side);
            float apos = GetSlabCoord(va, side);

            // Remove links pointing to 'side' and compact the links array. 
            int m = Edge.PointToSide(side);
            int bse = GetTileRef(tile);
            var polys = tile.GetPolys();

            for (int i = 0; i < polys.Length; ++i)
            {
                var poly = polys[i];
                int nv = poly.VertCount;

                for (int j = 0; j < nv; ++j)
                {
                    // Skip edges which do not point to the right side.
                    if (poly.Neis[j] != m)
                    {
                        continue;
                    }

                    var vc = tile.GetPolyVertex(poly, j);
                    var vd = tile.GetPolyVertex(poly, (j + 1) % nv);
                    float bpos = GetSlabCoord(vc, side);

                    // Segments are not close enough.
                    if (Math.Abs(apos - bpos) > 0.01f)
                    {
                        continue;
                    }

                    // Check if the segments touch.
                    CalcSlabEndPoints(vc, vd, out Vector2 bmin, out Vector2 bmax, side);

                    if (!OverlapSlabs(amin, amax, bmin, bmax, 0.01f, tile.Header.WalkableClimb))
                    {
                        continue;
                    }

                    // Add return value.
                    if (conList.Count < maxcon)
                    {
                        conareaList.Add(new Vector2(Math.Max(amin.X, bmin.X), Math.Min(amax.X, bmax.X)));
                        conList.Add(bse | i);
                    }

                    break;
                }
            }

            con = [.. conList];
            conarea = [.. conareaList];
        }
        /// <summary>
        /// Connect internal links
        /// </summary>
        /// <param name="tile">Mesh tile</param>
        private void ConnectIntLinks(MeshTile tile)
        {
            int bse = GetTileRef(tile);

            var polys = tile.GetPolys();
            foreach (var poly in polys)
            {
                poly.FirstLink = MeshTile.DT_NULL_LINK;

                if (poly.Type == PolyTypes.OffmeshConnection)
                {
                    continue;
                }

                // Build edge links backwards so that the links will be
                // in the linked list from lowest index to highest.
                for (int j = poly.VertCount - 1; j >= 0; --j)
                {
                    // Skip hard and non-internal edges.
                    if (poly.Neis[j] == 0 || poly.NeighbourIsExternalLink(j)) continue;

                    int idx = tile.AllocLink();
                    if (idx != MeshTile.DT_NULL_LINK)
                    {
                        var link = new Link
                        {
                            NRef = (bse | (poly.Neis[j] - 1)),
                            Edge = j,
                            Side = 0xff,
                            BMin = 0,
                            BMax = 0,
                            // Add to linked list.
                            Next = poly.FirstLink,
                        };
                        poly.FirstLink = idx;
                        tile.Links[idx] = link;
                    }
                }
            }
        }
        /// <summary>
        /// Base off-mesh connections
        /// </summary>
        /// <param name="tile">Mesh tile</param>
        private void BaseOffMeshLinks(MeshTile tile)
        {
            int bse = GetTileRef(tile);

            // Base off-mesh connection start points.
            var offmesCons = tile.GetOffMeshConnections();
            foreach (var con in offmesCons)
            {
                var poly = tile.GetPoly(con.Poly);

                var halfExtents = new Vector3(con.Rad, tile.Header.WalkableClimb, con.Rad);

                // Find polygon to connect to.
                int r = FindNearestPolyInTile(tile, con.Start, halfExtents, out Vector3 nearestPt);
                if (r == 0)
                {
                    continue;
                }

                // findNearestPoly may return too optimistic results, further check to make sure. 
                if (Math.Sqrt(nearestPt.X - con.Start.X) + Math.Sqrt(nearestPt.Z - con.Start.Z) > Math.Sqrt(con.Rad))
                {
                    continue;
                }
                // Make sure the location is on current mesh.
                tile.SetPolyVertex(poly, 0, nearestPt);

                // Link off-mesh connection to target poly.
                int idx = tile.AllocLink();
                if (idx != MeshTile.DT_NULL_LINK)
                {
                    var link = new Link
                    {
                        NRef = r,
                        Edge = 0,
                        Side = 0xff,
                        BMin = 0,
                        BMax = 0,
                        // Add to linked list.
                        Next = poly.FirstLink
                    };
                    tile.Links[idx] = link;
                    poly.FirstLink = idx;
                }

                // Start end-point is always connect back to off-mesh connection. 
                int tidx = tile.AllocLink();
                if (tidx != MeshTile.DT_NULL_LINK)
                {
                    var landPolyIdx = DecodePolyIdPoly(r);
                    var landPoly = tile.Polys[landPolyIdx];
                    var link = new Link
                    {
                        NRef = (bse | (con.Poly)),
                        Edge = 0xff,
                        Side = 0xff,
                        BMin = 0,
                        BMax = 0,
                        // Add to linked list.
                        Next = landPoly.FirstLink
                    };
                    tile.Links[tidx] = link;
                    landPoly.FirstLink = tidx;
                }
            }
        }
        /// <summary>
        /// Connect external links
        /// </summary>
        /// <param name="tile">Mesh tile</param>
        /// <param name="target">Tile target</param>
        /// <param name="side">Side number</param>
        private void ConnectExtLinks(MeshTile tile, MeshTile target, int side)
        {
            // Connect border links.
            var polys = tile.GetPolys();

            for (int i = 0; i < polys.Length; ++i)
            {
                // Create new links.
                CreatePolygonLinks(polys[i], tile, target, side);
            }
        }
        /// <summary>
        /// Creates new links between polygons
        /// </summary>
        /// <param name="poly">Polygon</param>
        /// <param name="tile">Polygon tile</param>
        /// <param name="target">Target tile</param>
        /// <param name="side">Side number</param>
        private void CreatePolygonLinks(Poly poly, MeshTile tile, MeshTile target, int side)
        {
            int nv = poly.VertCount;
            for (int j = 0; j < nv; ++j)
            {
                // Skip non-portal edges.
                if (poly.NeighbourIsExternalLink(j))
                {
                    continue;
                }

                int dir = poly.GetNeighbourDir(j);
                if (side != -1 && dir != side)
                {
                    continue;
                }

                // Create new links
                var va = tile.GetPolyVertex(poly, j);
                var vb = tile.GetPolyVertex(poly, (j + 1) % nv);
                FindConnectingPolys(va, vb, target, OppositeTile(dir), 4, out var neis, out var neiareas);

                for (int k = 0; k < neis.Length; k++)
                {
                    var nei = neis[k];
                    int idx = tile.AllocLink();
                    if (idx == MeshTile.DT_NULL_LINK)
                    {
                        continue;
                    }

                    var link = new Link
                    {
                        NRef = nei,
                        Edge = j,
                        Side = dir,
                        Next = poly.FirstLink
                    };
                    poly.FirstLink = idx;

                    // Compress portal limits to an integer value.
                    var bounds = CompressPortalLimits(va, vb, dir, neiareas[k]);
                    if (bounds.HasValue)
                    {
                        link.BMin = bounds.Value.X;
                        link.BMax = bounds.Value.Y;
                    }

                    tile.Links[idx] = link;
                }
            }
        }
        /// <summary>
        /// Compress portal limits
        /// </summary>
        /// <param name="va">Portal A point</param>
        /// <param name="vb">Portal B point</param>
        /// <param name="dir">Direction</param>
        /// <param name="neiarea">Neighbour area</param>
        /// <returns>Returns the limit bounds</returns>
        private static Vector2Int? CompressPortalLimits(Vector3 va, Vector3 vb, int dir, Vector2 neiarea)
        {
            // Compress portal limits to an integer value.
            if (dir == 0 || dir == 4)
            {
                float tmin = (neiarea.X - va.Z) / (vb.Z - va.Z);
                float tmax = (neiarea.Y - va.Z) / (vb.Z - va.Z);
                if (tmin > tmax)
                {
                    Helper.Swap(ref tmin, ref tmax);
                }

                var res = new Vector2Int
                {
                    X = (int)(MathUtil.Clamp(tmin, 0.0f, 1.0f) * 255.0f),
                    Y = (int)(MathUtil.Clamp(tmax, 0.0f, 1.0f) * 255.0f)
                };
                return res;
            }
            else if (dir == 2 || dir == 6)
            {
                float tmin = (neiarea.X - va.X) / (vb.X - va.X);
                float tmax = (neiarea.Y - va.X) / (vb.X - va.X);
                if (tmin > tmax)
                {
                    Helper.Swap(ref tmin, ref tmax);
                }

                var res = new Vector2Int
                {
                    X = (int)(MathUtil.Clamp(tmin, 0.0f, 1.0f) * 255.0f),
                    Y = (int)(MathUtil.Clamp(tmax, 0.0f, 1.0f) * 255.0f)
                };
                return res;
            }

            return null;
        }
        /// <summary>
        /// Connect external off-mesh links
        /// </summary>
        /// <param name="tile">Mesh tile</param>
        /// <param name="target">Tile target</param>
        /// <param name="side">Side number</param>
        private void ConnectExtOffMeshLinks(MeshTile tile, MeshTile target, int side)
        {
            // Connect off-mesh links.
            // We are interested on links which land from target tile to this tile.
            int oppositeSide = (side == -1) ? 0xff : OppositeTile(side);

            var offmeshCons = target.GetOffMeshConnections();
            foreach (var targetCon in offmeshCons)
            {
                if (targetCon.Side != oppositeSide)
                {
                    continue;
                }

                // Link off-mesh connection to target poly.
                CreateOffmeshLinks(targetCon, tile, target, side, oppositeSide);
            }
        }
        /// <summary>
        /// Creates the off-mesh connection links
        /// </summary>
        /// <param name="targetCon">Target off-mesh connection</param>
        /// <param name="tile">Mesh tile</param>
        /// <param name="target">Target tile</param>
        /// <param name="side">Side number</param>
        /// <param name="oppositeSide">Oppsite side number</param>
        private void CreateOffmeshLinks(OffMeshConnection targetCon, MeshTile tile, MeshTile target, int side, int oppositeSide)
        {
            var targetPoly = target.Polys[targetCon.Poly];

            // Skip off-mesh connections which start location could not be connected at all.
            if (targetPoly.FirstLink == MeshTile.DT_NULL_LINK)
            {
                return;
            }

            var halfExtents = new Vector3(targetCon.Rad, target.Header.WalkableClimb, targetCon.Rad);

            // Find polygon to connect to.
            var p = targetCon.End;
            int r = FindNearestPolyInTile(tile, p, halfExtents, out Vector3 nearestPt);
            if (r == 0)
            {
                return;
            }

            // findNearestPoly may return too optimistic results, further check to make sure. 
            if (Math.Sqrt(nearestPt.X - p.X) + Math.Sqrt(nearestPt.Z - p.Z) > Math.Sqrt(targetCon.Rad))
            {
                return;
            }

            // Make sure the location is on current mesh.
            target.SetPolyVertex(targetPoly, 1, nearestPt);

            int idx = target.AllocLink();
            if (idx != MeshTile.DT_NULL_LINK)
            {
                target.Links[idx] = new Link
                {
                    NRef = r,
                    Edge = 1,
                    Side = oppositeSide,
                    BMin = 0,
                    BMax = 0,
                    Next = targetPoly.FirstLink, // Add to linked list.
                };
                targetPoly.FirstLink = idx;
            }

            // Link target poly to off-mesh connection.
            if (!targetCon.IsBidirectional())
            {
                return;
            }

            int tidx = tile.AllocLink();
            if (tidx != MeshTile.DT_NULL_LINK)
            {
                var landPolyIdx = DecodePolyIdPoly(r);
                var landPoly = tile.Polys[landPolyIdx];
                tile.Links[tidx] = new Link
                {
                    NRef = GetTileRef(target) | (targetCon.Poly),
                    Edge = 0xff,
                    Side = side == -1 ? 0xff : side,
                    BMin = 0,
                    BMax = 0,
                    Next = landPoly.FirstLink, // Add to linked list.
                };
                landPoly.FirstLink = tidx;
            }
        }
        /// <summary>
        /// Unconnect links
        /// </summary>
        /// <param name="tile">Mesh tile</param>
        /// <param name="target">Tile target</param>
        private void UnconnectLinks(MeshTile tile, MeshTile target)
        {
            int targetNum = DecodePolyIdTile(GetTileRef(target));
            var polys = tile.GetPolys();

            for (int i = 0; i < polys.Length; ++i)
            {
                var poly = polys[i];
                int j = poly.FirstLink;
                int pj = MeshTile.DT_NULL_LINK;
                while (j != MeshTile.DT_NULL_LINK)
                {
                    if (DecodePolyIdTile(tile.Links[j].NRef) == targetNum)
                    {
                        // Remove link.
                        int nj = tile.Links[j].Next;
                        if (pj == MeshTile.DT_NULL_LINK)
                        {
                            poly.FirstLink = nj;
                        }
                        else
                        {
                            tile.Links[pj].Next = nj;
                        }
                        tile.FreeLink(j);
                        j = nj;
                    }
                    else
                    {
                        // Advance
                        pj = j;
                        j = tile.Links[j].Next;
                    }
                }
            }
        }

        /// <summary>
        /// Performs a query in the polygons of the specified tile
        /// </summary>
        /// <param name="tile">Mesh tile</param>
        /// <param name="bounds">Bounds</param>
        /// <param name="maxPolys">Maximum resulting polygons</param>
        /// <returns>Returns a collection of polygon references</returns>
        private int[] QueryPolygonsInTile(MeshTile tile, BoundingBox bounds, int maxPolys)
        {
            if (tile.BvTree?.Length > 0)
            {
                return QueryPolygonsInTileBVTree(tile, bounds, maxPolys);
            }
            else
            {
                return QueryPolygonsInTileByRefs(tile, bounds, maxPolys);
            }
        }
        /// <summary>
        /// Performs a query in the polygons of the specified tile using the existing BVTree
        /// </summary>
        /// <param name="tile">Mesh tile</param>
        /// <param name="bounds">Bounds</param>
        /// <param name="maxPolys">Maximum resulting polygons</param>
        /// <returns>Returns a collection of polygon references</returns>
        private int[] QueryPolygonsInTileBVTree(MeshTile tile, BoundingBox bounds, int maxPolys)
        {
            var polys = new List<int>(maxPolys);

            int nodeIndex = 0;
            int endIndex = tile.Header.BvNodeCount;
            var tb = tile.Header.Bounds;
            float qfac = tile.Header.BvQuantFactor;

            // Calculate quantized box
            var bmin = new Int3();
            var bmax = new Int3();
            // dtClamp query box to world box.
            float minx = MathUtil.Clamp(bounds.Minimum.X, tb.Minimum.X, tb.Maximum.X) - tb.Minimum.X;
            float miny = MathUtil.Clamp(bounds.Minimum.Y, tb.Minimum.Y, tb.Maximum.Y) - tb.Minimum.Y;
            float minz = MathUtil.Clamp(bounds.Minimum.Z, tb.Minimum.Z, tb.Maximum.Z) - tb.Minimum.Z;
            float maxx = MathUtil.Clamp(bounds.Maximum.X, tb.Minimum.X, tb.Maximum.X) - tb.Minimum.X;
            float maxy = MathUtil.Clamp(bounds.Maximum.Y, tb.Minimum.Y, tb.Maximum.Y) - tb.Minimum.Y;
            float maxz = MathUtil.Clamp(bounds.Maximum.Z, tb.Minimum.Z, tb.Maximum.Z) - tb.Minimum.Z;
            // Quantize
            bmin.X = (int)(qfac * minx) & 0xfffe;
            bmin.Y = (int)(qfac * miny) & 0xfffe;
            bmin.Z = (int)(qfac * minz) & 0xfffe;
            bmax.X = (int)(qfac * maxx + 1) | 1;
            bmax.Y = (int)(qfac * maxy + 1) | 1;
            bmax.Z = (int)(qfac * maxz + 1) | 1;

            // Traverse tree
            int bse = GetTileRef(tile);

            while (nodeIndex < endIndex)
            {
                var node = nodeIndex < tile.BvTree.Length ?
                    tile.BvTree[nodeIndex] :
                    new BVNode();

                bool overlap = Utils.OverlapBounds(bmin, bmax, node.BMin, node.BMax);
                bool isLeafNode = node.I >= 0;

                if (isLeafNode && overlap && polys.Count < maxPolys)
                {
                    polys.Add(bse | node.I);
                }

                if (overlap || isLeafNode)
                {
                    nodeIndex++;
                }
                else
                {
                    int escapeIndex = -node.I;
                    nodeIndex += escapeIndex;
                }
            }

            return [.. polys];
        }
        /// <summary>
        /// Performs a query in the polygons of the specified tile
        /// </summary>
        /// <param name="tile">Mesh tile</param>
        /// <param name="bounds">Bounds</param>
        /// <param name="maxPolys">Maximum resulting polygons</param>
        /// <returns>Returns a collection of polygon references</returns>
        private int[] QueryPolygonsInTileByRefs(MeshTile tile, BoundingBox bounds, int maxPolys)
        {
            var polys = new List<int>(maxPolys);

            int bse = GetTileRef(tile);

            for (int i = 0; i < tile.Header.PolyCount; i++)
            {
                var p = tile.Polys[i];

                // Do not return off-mesh connection polygons.
                if (p.Type == PolyTypes.OffmeshConnection)
                {
                    continue;
                }

                // Calc polygon bounds.
                var tileBounds = tile.GetPolyBounds(p);

                if (bounds.Contains(tileBounds) != ContainmentType.Disjoint && polys.Count < maxPolys)
                {
                    polys.Add(bse | i);
                }
            }

            return [.. polys];
        }
        /// <summary>
        /// Finds the nearest polygon in a tile, from the specified position
        /// </summary>
        /// <param name="tile">Mesh tile</param>
        /// <param name="center">Center position</param>
        /// <param name="halfExtents">Query extents</param>
        /// <param name="nearestPt">Resulting point</param>
        /// <returns>Returns the nearest polygon reference</returns>
        private int FindNearestPolyInTile(MeshTile tile, Vector3 center, Vector3 halfExtents, out Vector3 nearestPt)
        {
            nearestPt = Vector3.Zero;

            var bmin = Vector3.Subtract(center, halfExtents);
            var bmax = Vector3.Add(center, halfExtents);
            var bounds = new BoundingBox(bmin, bmax);

            // Get nearby polygons from proximity grid.
            var polys = QueryPolygonsInTile(tile, bounds, 128);

            // Find nearest polygon amongst the nearby polygons.
            int nearest = 0;
            float nearestDistanceSqr = float.MaxValue;

            foreach (var r in polys)
            {
                ClosestPointOnPoly(r, center, out Vector3 closestPtPoly, out bool posOverPoly);

                // If a point is directly over a polygon and closer than
                // climb height, favor that instead of straight line nearest point.
                float d;
                var diff = Vector3.Subtract(center, closestPtPoly);
                if (posOverPoly)
                {
                    d = Math.Abs(diff.Y) - tile.Header.WalkableClimb;
                    d = d > 0 ? d * d : 0;
                }
                else
                {
                    d = diff.LengthSquared();
                }

                if (d < nearestDistanceSqr)
                {
                    nearestPt = closestPtPoly;
                    nearestDistanceSqr = d;
                    nearest = r;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Gets the closest point in a polygon, from the specified position
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <param name="pos">Position</param>
        /// <param name="closest">Resulting closest position</param>
        /// <param name="posOverPoly">Returns true if the resulting position is over de polygon</param>
        public void ClosestPointOnPoly(int r, Vector3 pos, out Vector3 closest, out bool posOverPoly)
        {
            closest = pos;
            posOverPoly = false;

            var tileRef = GetTileAndPolyByRefUnsafe(r);

            if (tileRef.Tile.GetPolyHeight(tileRef.Poly, pos, out float h))
            {
                closest.Y = h;
                posOverPoly = true;
                return;
            }

            // Off-mesh connections don't have detail polygons.
            if (tileRef.Poly.Type == PolyTypes.OffmeshConnection)
            {
                var v0 = tileRef.Tile.Verts[tileRef.Poly.Verts[0]];
                var v1 = tileRef.Tile.Verts[tileRef.Poly.Verts[1]];
                Utils.DistancePtSegSqr2D(pos, v0, v1, out var t);
                closest = Vector3.Lerp(v0, v1, t);
                return;
            }

            // Outside poly that is not an offmesh connection.
            tileRef.Tile.ClosestPointOnDetailEdges(tileRef.Poly, pos, true, out closest);
        }
        /// <summary>
        /// Creates the tile caché
        /// </summary>
        /// <param name="geometry">Input geometry</param>
        /// <param name="cacheParams">Parameters</param>
        public void CreateTileCache(InputGeometry geometry, TileCacheParams cacheParams)
        {
            TileCache = new TileCache(this, new TileCacheMeshProcess(geometry), cacheParams);
        }

        /// <summary>
        /// Gets the navigation mesh build data
        /// </summary>
        internal BuildData GetBuildData(int tx, int ty)
        {
            if (Tiles?.Length == 0)
            {
                //Solo build
                return m_buildData;
            }
            else if (TileCache == null)
            {
                //Tiles build
                return GetTileAt(tx, ty, 0).Data.BuildData;
            }
            else
            {
                //Tile cache build
                return default;
            }
        }
    }
}
