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
        /// Builds a navigation mesh
        /// </summary>
        /// <param name="geometry">Input geometry</param>
        /// <param name="settings">Build settings</param>
        /// <param name="agent">Agent</param>
        /// <returns>Returns the navigation mesh</returns>
        public static NavMesh Build(InputGeometry geometry, BuildSettings settings, Agent agent)
        {
            if (settings.BuildMode == BuildModes.Solo)
            {
                return BuildSolo(geometry, settings, agent);
            }
            else if (settings.BuildMode == BuildModes.Tiled)
            {
                return BuildTiled(geometry, settings, agent);
            }
            else
            {
                throw new EngineException("Bad build mode for NavigationMesh.");
            }
        }
        /// <summary>
        /// Builds a navigation mesh
        /// </summary>
        /// <param name="geometry">Input geometry</param>
        /// <param name="settings">Build settings</param>
        /// <param name="agent">Agent</param>
        /// <returns>Returns the navigation mesh</returns>
        private static NavMesh BuildSolo(InputGeometry geometry, BuildSettings settings, Agent agent)
        {
            var bbox = settings.Bounds ?? geometry.BoundingBox;

            // Generation params.
            var cfg = settings.GetSoloConfig(agent, bbox);

            if (cfg.MaxVertsPerPoly > DetourUtils.DT_VERTS_PER_POLYGON)
            {
                throw new EngineException($"BuildSolo: {cfg.MaxVertsPerPoly} is bigger than DetourUtils.DT_VERTS_PER_POLYGON ({DetourUtils.DT_VERTS_PER_POLYGON}).");
            }

            var solid = Heightfield.Build(cfg.Width, cfg.Height, cfg.BoundingBox, cfg.CellSize, cfg.CellHeight);

            var tris = geometry.ChunkyMesh.GetTriangles();
            var triareas = MarkWalkableTriangles(cfg.WalkableSlopeAngle, tris);
            if (!solid.RasterizeTriangles(cfg.WalkableClimb, tris, triareas))
            {
                return null;
            }

            // Performs the heightfield filters
            FilterHeightfield(solid, cfg);

            // Compact the heightfield so that it is faster to handle from now on.
            // This will result more cache coherent data as well as the neighbours
            // between walkable cells will be calculated.
            var chf = CompactHeightfield.Build(cfg.WalkableHeight, cfg.WalkableClimb, solid);
            if (chf == null)
            {
                throw new EngineException("buildNavigation: Could not build compact height field.");
            }

            // Erode the walkable area by agent radius.
            if (!chf.ErodeWalkableArea(cfg.WalkableRadius))
            {
                throw new EngineException("buildNavigation: Could not erode.");
            }

            // Mark areas.
            chf.MarkAreas(geometry);

            // Sample partition
            SamplePartition(chf, cfg);

            // Create contours.
            var cset = ContourSet.BuildContours(chf, cfg.MaxSimplificationError, cfg.MaxEdgeLen, BuildContoursFlagTypes.TessellateWallEdges);
            if (cset == null)
            {
                throw new EngineException("buildNavigation: Could not create contours.");
            }

            var pmesh = PolyMesh.Build(cset, cfg.MaxVertsPerPoly);
            if (pmesh == null)
            {
                throw new EngineException("buildNavigation: Could not triangulate contours.");
            }

            // Build polygon navmesh from the contours.
            var dmesh = PolyMeshDetail.BuildPolyMeshDetail(pmesh, chf, cfg.DetailSampleDist, cfg.DetailSampleMaxError);
            if (dmesh == null)
            {
                throw new EngineException("buildNavigation: Could not build detail mesh.");
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
                DetailMeshes = dmesh.Meshes.ToArray(),
                DetailVerts = dmesh.Verts.ToArray(),
                DetailVertsCount = dmesh.Verts.Count,
                DetailTris = dmesh.Tris.ToArray(),
                DetailTriCount = dmesh.Tris.Count,
                OffMeshCon = geometry.GetConnections()?.ToArray(),
                OffMeshConCount = geometry.GetConnectionCount(),
                WalkableHeight = cfg.Agent.Height,
                WalkableRadius = cfg.Agent.Radius,
                WalkableClimb = cfg.Agent.MaxClimb,
                BMin = pmesh.BMin,
                BMax = pmesh.BMax,
                CS = cfg.CellSize,
                CH = cfg.CellHeight,
                BuildBvTree = true
            };

            MeshData navData = MeshData.CreateNavMeshData(param);
            if (navData == null)
            {
                throw new EngineException("Could not build Detour navmesh.");
            }

            // Make sure the data is in right format.
            MeshHeader header = navData.Header;
            if (header.Magic != DetourUtils.DT_NAVMESH_MAGIC)
            {
                throw new EngineException("Bad header Magic value");
            }
            if (header.Version != DetourUtils.DT_NAVMESH_VERSION)
            {
                throw new EngineException("Bad header Version value");
            }

            NavMeshParams nvParams = new NavMeshParams
            {
                Origin = header.BMin,
                TileWidth = header.BMax[0] - header.BMin[0],
                TileHeight = header.BMax[2] - header.BMin[2],
                MaxTiles = 1,
                MaxPolys = header.PolyCount
            };

            var nm = new NavMesh(nvParams);
            nm.AddTile(navData, TileFlagTypes.FreeData, 0);
            return nm;
        }
        /// <summary>
        /// Builds a tile navigation mesh
        /// </summary>
        /// <param name="geometry">Input geometry</param>
        /// <param name="settings">Build settings</param>
        /// <param name="agent">Agent</param>
        /// <returns>Returns the navigation mesh</returns>
        private static NavMesh BuildTiled(InputGeometry geometry, BuildSettings settings, Agent agent)
        {
            var bbox = settings.Bounds ?? geometry.BoundingBox;

            // Init cache
            BuildSettings.CalcGridSize(bbox, settings.CellSize, out int gridWidth, out int gridHeight);
            int tSize = (int)settings.TileSize;
            int tWidth = (gridWidth + tSize - 1) / tSize;
            int tHeight = (gridHeight + tSize - 1) / tSize;
            float tileCellSize = settings.TileCellSize;

            int tBits = Math.Min((int)Math.Log(Helper.NextPowerOfTwo(tWidth * tHeight), 2), 14);
            int pBits = 22 - tBits;
            int maxTileCount = 1 << tBits;
            int maxPolysPerTile = 1 << pBits;

            var nmparams = new NavMeshParams()
            {
                Origin = bbox.Minimum,
                TileWidth = tileCellSize,
                TileHeight = tileCellSize,
                MaxTiles = maxTileCount,
                MaxPolys = maxPolysPerTile,
            };

            var nm = new NavMesh(nmparams);

            if (settings.UseTileCache)
            {
                // Generation params.
                var cfg = settings.GetTileCacheConfig(agent, bbox);

                var tmproc = new TileCacheMeshProcess(geometry);

                nm.tileCache = new TileCache(nm, tmproc, cfg.TileCacheParams);

                if (settings.BuildAllTiles)
                {
                    BuildTileCache(nm.tileCache, geometry, tWidth, tHeight, cfg);
                }
            }
            else
            {
                // Generation params.
                TileParams tileParams = new TileParams
                {
                    Width = tWidth,
                    Height = tHeight,
                    CellSize = tileCellSize,
                    Bounds = bbox,
                };

                if (settings.BuildAllTiles)
                {
                    BuildAllTiles(nm, geometry, settings, agent, tileParams);
                }
            }

            return nm;
        }

        /// <summary>
        /// Build all tiles
        /// </summary>
        /// <param name="navMesh">Navigation mesh</param>
        /// <param name="geometry">Input geometry</param>
        /// <param name="settings">Build settings</param>
        /// <param name="agent">Agent</param>
        /// <param name="tileParams">Tile parameters</param>
        private static void BuildAllTiles(NavMesh navMesh, InputGeometry geometry, BuildSettings settings, Agent agent, TileParams tileParams)
        {
            for (int y = 0; y < tileParams.Height; y++)
            {
                for (int x = 0; x < tileParams.Width; x++)
                {
                    BoundingBox tileBounds = GetTileBounds(x, y, tileParams.CellSize, tileParams.Bounds);

                    // Init build configuration
                    Config cfg = settings.GetTiledConfig(agent, tileBounds);

                    var data = BuildTileMesh(x, y, geometry, cfg);
                    if (data != null)
                    {
                        // Remove any previous data (navmesh owns and deletes the data).
                        navMesh.RemoveTile(x, y, 0);

                        // Let the navmesh own the data.
                        navMesh.AddTile(data, TileFlagTypes.FreeData, 0);
                    }
                }
            }
        }
        /// <summary>
        /// Builds a tile mesh
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="geometry">Input geometry</param>
        /// <param name="cfg">Configuration</param>
        /// <returns>Returns the generated mesh data</returns>
        private static MeshData BuildTileMesh(int x, int y, InputGeometry geometry, Config cfg)
        {
            if (cfg.MaxVertsPerPoly > DetourUtils.DT_VERTS_PER_POLYGON)
            {
                throw new EngineException($"BuildTileMesh: {cfg.MaxVertsPerPoly} is bigger than DetourUtils.DT_VERTS_PER_POLYGON ({DetourUtils.DT_VERTS_PER_POLYGON}).");
            }

            var chunkyMesh = geometry.ChunkyMesh;

            // Allocate voxel heightfield where we rasterize our input data to.
            var solid = Heightfield.Build(cfg.Width, cfg.Height, cfg.BoundingBox, cfg.CellSize, cfg.CellHeight);

            Vector2 tbmin = new Vector2(cfg.BoundingBox.Minimum.X, cfg.BoundingBox.Minimum.Z);
            Vector2 tbmax = new Vector2(cfg.BoundingBox.Maximum.X, cfg.BoundingBox.Maximum.Z);
            var cid = chunkyMesh.GetChunksOverlappingRect(tbmin, tbmax);
            if (!cid.Any())
            {
                return null; // empty
            }

            foreach (var id in cid)
            {
                var tris = chunkyMesh.GetTriangles(id);

                var triareas = MarkWalkableTriangles(cfg.WalkableSlopeAngle, tris);

                if (!solid.RasterizeTriangles(cfg.WalkableClimb, tris, triareas))
                {
                    return null;
                }
            }

            // Performs the heightfield filters
            FilterHeightfield(solid, cfg);

            // Compact the heightfield so that it is faster to handle from now on.
            // This will result more cache coherent data as well as the neighbours
            // between walkable cells will be calculated.
            var chf = CompactHeightfield.Build(cfg.WalkableHeight, cfg.WalkableClimb, solid);
            if (chf == null)
            {
                return null;
            }

            // Erode the walkable area by agent radius.
            if (!chf.ErodeWalkableArea(cfg.WalkableRadius))
            {
                return null;
            }

            // Mark areas.
            chf.MarkAreas(geometry);

            // Sample partition
            SamplePartition(chf, cfg);

            // Create contours.
            var cset = ContourSet.BuildContours(chf, cfg.MaxSimplificationError, cfg.MaxEdgeLen, BuildContoursFlagTypes.TessellateWallEdges);
            if (cset == null || cset.NConts == 0)
            {
                return null;
            }

            // Build polygon navmesh from the contours.
            var pmesh = PolyMesh.Build(cset, cfg.MaxVertsPerPoly);
            if (pmesh == null)
            {
                return null;
            }

            // Build detail mesh.
            var dmesh = PolyMeshDetail.BuildPolyMeshDetail(pmesh, chf, cfg.DetailSampleDist, cfg.DetailSampleMaxError);
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
                DetailMeshes = dmesh.Meshes.ToArray(),
                DetailVerts = dmesh.Verts.ToArray(),
                DetailVertsCount = dmesh.Verts.Count,
                DetailTris = dmesh.Tris.ToArray(),
                DetailTriCount = dmesh.Tris.Count,
                OffMeshCon = geometry.GetConnections().ToArray(),
                OffMeshConCount = geometry.GetConnectionCount(),
                WalkableHeight = cfg.Agent.Height,
                WalkableRadius = cfg.Agent.Radius,
                WalkableClimb = cfg.Agent.MaxClimb,
                TileX = x,
                TileY = y,
                TileLayer = 0,
                BMin = pmesh.BMin,
                BMax = pmesh.BMax,
                CS = cfg.CellSize,
                CH = cfg.CellHeight,
                BuildBvTree = true
            };

            return MeshData.CreateNavMeshData(param);
        }
        /// <summary>
        /// Builds a tile cache
        /// </summary>
        /// <param name="tileCache">Tile cache to populate</param>
        /// <param name="geometry">Input geometry</param>
        /// <param name="tileWidth">Tile width</param>
        /// <param name="tileHeight">Tile height</param>
        /// <param name="cfg">Configuration</param>
        private static void BuildTileCache(TileCache tileCache, InputGeometry geometry, int tileWidth, int tileHeight, Config cfg)
        {
            for (int y = 0; y < tileHeight; y++)
            {
                for (int x = 0; x < tileWidth; x++)
                {
                    var tiles = RasterizeTileLayers(x, y, geometry, cfg);

                    foreach (var tile in tiles)
                    {
                        tileCache.AddTile(tile, CompressedTileFlagTypes.FreeData);
                    }
                }
            }

            // Build initial meshes
            for (int y = 0; y < tileHeight; y++)
            {
                for (int x = 0; x < tileWidth; x++)
                {
                    tileCache.BuildTilesAt(x, y);
                }
            }
        }
        /// <summary>
        /// Rasterizes the tile layers
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="geometry">Input geometry</param>
        /// <param name="cfg">Configuration</param>
        /// <returns>Returns an array of tile cache data instances</returns>
        private static IEnumerable<TileCacheData> RasterizeTileLayers(int x, int y, InputGeometry geometry, Config cfg)
        {
            List<TileCacheData> tiles = new List<TileCacheData>();

            ChunkyTriMesh chunkyMesh = geometry.ChunkyMesh;

            // Tile bounds.
            float tcs = cfg.TileSize * cfg.CellSize;

            BoundingBox bbox = new BoundingBox();

            bbox.Minimum.X = cfg.BoundingBox.Minimum.X + x * tcs;
            bbox.Minimum.Y = cfg.BoundingBox.Minimum.Y;
            bbox.Minimum.Z = cfg.BoundingBox.Minimum.Z + y * tcs;

            bbox.Maximum.X = cfg.BoundingBox.Minimum.X + (x + 1) * tcs;
            bbox.Maximum.Y = cfg.BoundingBox.Maximum.Y;
            bbox.Maximum.Z = cfg.BoundingBox.Minimum.Z + (y + 1) * tcs;

            bbox.Minimum.X -= cfg.BorderSize * cfg.CellSize;
            bbox.Minimum.Z -= cfg.BorderSize * cfg.CellSize;
            bbox.Maximum.X += cfg.BorderSize * cfg.CellSize;
            bbox.Maximum.Z += cfg.BorderSize * cfg.CellSize;

            cfg.BoundingBox = bbox;

            var solid = Heightfield.Build(cfg.Width, cfg.Height, cfg.BoundingBox, cfg.CellSize, cfg.CellHeight);

            var rc = new RasterizationContext
            {
                // Allocate voxel heightfield where we rasterize our input data to.
                Solid = solid,
                Tiles = new TileCacheData[RasterizationContext.MaxLayers],
            };

            Vector2 tbmin = new Vector2(cfg.BoundingBox.Minimum.X, cfg.BoundingBox.Minimum.Z);
            Vector2 tbmax = new Vector2(cfg.BoundingBox.Maximum.X, cfg.BoundingBox.Maximum.Z);

            var cid = chunkyMesh.GetChunksOverlappingRect(tbmin, tbmax);
            if (!cid.Any())
            {
                return tiles.ToArray(); // empty
            }

            foreach (var id in cid)
            {
                var tris = chunkyMesh.GetTriangles(id);

                rc.TriAreas = MarkWalkableTriangles(cfg.WalkableSlopeAngle, tris).ToArray();

                if (!rc.Solid.RasterizeTriangles(cfg.WalkableClimb, tris, rc.TriAreas))
                {
                    return tiles.ToArray();
                }
            }

            // Once all geometry is rasterized, we do initial pass of filtering to
            // remove unwanted overhangs caused by the conservative rasterization
            // as well as filter spans where the character cannot possibly stand.
            FilterHeightfield(rc.Solid, cfg);

            rc.CompactHeightField = CompactHeightfield.Build(cfg.WalkableHeight, cfg.WalkableClimb, rc.Solid);
            if (rc.CompactHeightField == null)
            {
                throw new EngineException("buildNavigation: Could not build compact height field.");
            }

            // Erode the walkable area by agent radius.
            if (!rc.CompactHeightField.ErodeWalkableArea(cfg.WalkableRadius))
            {
                throw new EngineException("buildNavigation: Could not erode.");
            }

            // Mark areas.
            rc.CompactHeightField.MarkAreas(geometry);

            rc.LayerSet = HeightfieldLayerSet.Build(rc.CompactHeightField, cfg.BorderSize, cfg.WalkableHeight);

            rc.NTiles = 0;
            for (int i = 0; i < Math.Min(rc.LayerSet.NLayers, cfg.MaxLayers); i++)
            {
                var layer = rc.LayerSet.Layers[i];

                var tile = rc.Tiles[rc.NTiles];

                // Store header
                tile.Header = new TileCacheLayerHeader
                {
                    Magic = DetourTileCache.DT_TILECACHE_MAGIC,
                    Version = DetourTileCache.DT_TILECACHE_VERSION,

                    // Tile layer location in the navmesh.
                    TX = x,
                    TY = y,
                    TLayer = i,
                    BBox = layer.BoundingBox,

                    // Tile info.
                    Width = layer.Width,
                    Height = layer.Height,
                    MinX = layer.MinX,
                    MaxX = layer.MaxX,
                    MinY = layer.MinY,
                    MaxY = layer.MaxY,
                    HMin = layer.HMin,
                    HMax = layer.HMax
                };

                // Store data
                DetourTileCache.BuildTileCacheLayer(layer.Heights, layer.Areas, layer.Connections, out var data);

                tile.Data = data;

                rc.Tiles[rc.NTiles++] = tile;
            }

            // Transfer ownsership of tile data from build context to the caller.
            for (int i = 0; i < Math.Min(rc.NTiles, cfg.MaxLayers); i++)
            {
                tiles.Add(rc.Tiles[i]);
                rc.Tiles[i].Data = TileCacheLayerData.Empty;
            }

            return tiles.ToArray();
        }

        /// <summary>
        /// Gets the existing tile definition at the specified position
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="geometry">Input geometry</param>
        /// <param name="settings">Build settings</param>
        /// <param name="x">Resulting x tile coordinate</param>
        /// <param name="y">Resulting y tile coordinate</param>
        /// <param name="tileBounds">Tile bounds</param>
        public static void GetTileAtPosition(Vector3 pos, InputGeometry geometry, BuildSettings settings, out int x, out int y, out BoundingBox tileBounds)
        {
            var bbox = settings.Bounds ?? geometry.BoundingBox;

            x = (int)((pos.X - bbox.Minimum.X) / settings.TileCellSize);
            y = (int)((pos.Z - bbox.Minimum.Z) / settings.TileCellSize);

            tileBounds = GetTileBounds(x, y, settings.TileCellSize, bbox);
        }
        /// <summary>
        /// Gets the tile bounds of the tile at the specified coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="geometry">Input geometry</param>
        /// <param name="settings">Build settings</param>
        /// <returns>Returns the tile bounds</returns>
        public static BoundingBox GetTileBounds(int x, int y, InputGeometry geometry, BuildSettings settings)
        {
            var bbox = settings.Bounds ?? geometry.BoundingBox;

            return GetTileBounds(x, y, settings.TileCellSize, bbox);
        }
        /// <summary>
        /// Gets the tile bounds of the tile at the specified coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="tileSize">Tile size</param>
        /// <param name="bbox">Navigation bounding box</param>
        /// <returns>Returns the tile bounds</returns>
        private static BoundingBox GetTileBounds(int x, int y, float tileSize, BoundingBox bbox)
        {
            BoundingBox tbbox = new BoundingBox();

            tbbox.Minimum.X = bbox.Minimum.X + x * tileSize;
            tbbox.Minimum.Y = bbox.Minimum.Y;
            tbbox.Minimum.Z = bbox.Minimum.Z + y * tileSize;

            tbbox.Maximum.X = bbox.Minimum.X + (x + 1) * tileSize;
            tbbox.Maximum.Y = bbox.Maximum.Y;
            tbbox.Maximum.Z = bbox.Minimum.Z + (y + 1) * tileSize;

            return tbbox;
        }
        /// <summary>
        /// Builds the tile at sepecified coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="geometry">Input geometry</param>
        /// <param name="settings">Build settings</param>
        /// <param name="agent">Agent</param>
        /// <param name="tileBounds">Tile bounds</param>
        public void BuildTileAtPosition(int x, int y, InputGeometry geometry, BuildSettings settings, Agent agent, BoundingBox tileBounds)
        {
            // Remove any previous data (navmesh owns and deletes the data).
            RemoveTile(x, y, 0);

            // Add tile, or leave the location empty.
            var cfg = settings.GetTiledConfig(agent, tileBounds);
            var data = BuildTileMesh(x, y, geometry, cfg);
            if (data != null)
            {
                AddTile(data, TileFlagTypes.FreeData, 0);
            }

            if (settings.UseTileCache && tileCache != null)
            {
                tileCache.BuildTilesAt(x, y);
            }
        }
        /// <summary>
        /// Removes the tile at specified coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="settings">Build settings</param>
        public void RemoveTileAtPosition(int x, int y, BuildSettings settings)
        {
            RemoveTile(x, y, 0);

            if (settings.UseTileCache && tileCache != null)
            {
                tileCache.RemoveTile(x, y, 0);
            }
        }

        /// <summary>
        /// Filters the heighfield
        /// </summary>
        /// <param name="solid">Heighfield</param>
        /// <param name="cfg">Configuration</param>
        private static void FilterHeightfield(Heightfield solid, Config cfg)
        {
            // Once all geometry is rasterized, we do initial pass of filtering to
            // remove unwanted overhangs caused by the conservative rasterization
            // as well as filter spans where the character cannot possibly stand.
            if (cfg.FilterLowHangingObstacles)
            {
                solid.FilterLowHangingWalkableObstacles(cfg.WalkableClimb);
            }
            if (cfg.FilterLedgeSpans)
            {
                solid.FilterLedgeSpans(cfg.WalkableHeight, cfg.WalkableClimb);
            }
            if (cfg.FilterWalkableLowHeightSpans)
            {
                solid.FilterWalkableLowHeightSpans(cfg.WalkableHeight);
            }
        }
        /// <summary>
        /// Partition the heightfield so that we can use simple algorithm later to triangulate the walkable areas.
        /// </summary>
        /// <param name="chf">Compact heightfield</param>
        /// <param name="cfg">Configuration</param>
        /// <remarks>
        /// There are 3 martitioning methods, each with some pros and cons:
        /// 1) Watershed partitioning
        ///   - the classic Recast partitioning
        ///   - creates the nicest tessellation
        ///   - usually slowest
        ///   - partitions the heightfield into nice regions without holes or overlaps
        ///   - the are some corner cases where this method creates produces holes and overlaps
        ///      - holes may appear when a small obstacles is close to large open area (triangulation can handle this)
        ///      - overlaps may occur if you have narrow spiral corridors (i.e stairs), this make triangulation to fail
        ///   * generally the best choice if you precompute the navmesh, use this if you have large open areas
        /// 2) Monotone partioning
        ///   - fastest
        ///   - partitions the heightfield into regions without holes and overlaps (guaranteed)
        ///   - creates long thin polygons, which sometimes causes paths with detours
        ///   * use this if you want fast navmesh generation
        /// 3) Layer partitoining
        ///   - quite fast
        ///   - partitions the heighfield into non-overlapping regions
        ///   - relies on the triangulation code to cope with holes (thus slower than monotone partitioning)
        ///   - produces better triangles than monotone partitioning
        ///   - does not have the corner cases of watershed partitioning
        ///   - can be slow and create a bit ugly tessellation (still better than monotone)
        ///     if you have large open areas with small obstacles (not a problem if you use tiles)
        ///   * good choice to use for tiled navmesh with medium and small sized tiles
        /// </remarks>
        private static void SamplePartition(CompactHeightfield chf, Config cfg)
        {
            if (cfg.PartitionType == SamplePartitionTypes.Watershed)
            {
                // Prepare for region partitioning, by calculating distance field along the walkable surface.
                if (!chf.BuildDistanceField())
                {
                    throw new EngineException("buildNavigation: Could not build distance field.");
                }

                // Partition the walkable surface into simple regions without holes.
                if (!chf.BuildRegions(cfg.BorderSize, cfg.MinRegionArea, cfg.MergeRegionArea))
                {
                    throw new EngineException("buildNavigation: Could not build watershed regions.");
                }
            }
            else if (cfg.PartitionType == SamplePartitionTypes.Monotone)
            {
                // Partition the walkable surface into simple regions without holes.
                // Monotone partitioning does not need distancefield.
                if (!chf.BuildRegionsMonotone(cfg.BorderSize, cfg.MinRegionArea, cfg.MergeRegionArea))
                {
                    throw new EngineException("buildNavigation: Could not build monotone regions.");
                }
            }
            else if (cfg.PartitionType == SamplePartitionTypes.Layers)
            {
                // Partition the walkable surface into simple regions without holes.
                var hasLayers = chf.BuildLayerRegions(cfg.BorderSize, cfg.MinRegionArea);
                if (!hasLayers)
                {
                    throw new EngineException("buildNavigation: Could not build layer regions.");
                }
            }
        }
        /// <summary>
        /// Marks the walkable triangles
        /// </summary>
        /// <param name="walkableSlopeAngle">Slope angle</param>
        /// <param name="tris">Triangle list</param>
        /// <returns>Returns an array of area types</returns>
        private static IEnumerable<AreaTypes> MarkWalkableTriangles(float walkableSlopeAngle, IEnumerable<Triangle> tris)
        {
            List<AreaTypes> triareas = new List<AreaTypes>();

            float walkableThr = (float)Math.Cos(walkableSlopeAngle / 180.0f * MathUtil.Pi);

            for (int i = 0; i < tris.Count(); i++)
            {
                var tri = tris.ElementAt(i);
                Vector3 norm = tri.Normal;

                // Check if the face is walkable.
                if (norm.Y > walkableThr)
                {
                    triareas.Add(AreaTypes.Walkable);
                }
                else
                {
                    triareas.Add(AreaTypes.Unwalkable);
                }
            }

            return triareas;
        }

        /// <summary>
        /// Creates a navigation mesh file from a navigation mesh
        /// </summary>
        /// <param name="navmesh">Navigation mesh</param>
        /// <returns>Returns the navigation mesh file</returns>
        public static NavMeshFile FromNavmesh(NavMesh navmesh)
        {
            NavMeshFile file = new NavMeshFile
            {
                NavMeshParams = navmesh.GetParams(),
                NavMeshData = new List<MeshData>(),

                HasTileCache = navmesh.tileCache != null,
                TileCacheParams = navmesh.tileCache != null ? navmesh.tileCache.GetParams() : new TileCacheParams(),
                TileCacheData = new List<TileCacheData>()
            };

            // Store navmesh tiles.
            var tiles = navmesh.GetTiles();
            for (int i = 0; i < tiles.Count(); ++i)
            {
                var tile = tiles.ElementAt(i);
                if (tile != null)
                {
                    file.NavMeshData.Add(tile.Data);
                }
            }

            if (navmesh.tileCache != null)
            {
                // Store cache tiles.
                var tileCount = navmesh.tileCache.GetTileCount();

                for (int i = 0; i < tileCount; ++i)
                {
                    var tile = navmesh.tileCache.GetTile(i);
                    if (tile != null)
                    {
                        file.TileCacheData.Add(new TileCacheData
                        {
                            Header = tile.Header,
                            Data = tile.Data
                        });
                    }
                }
            }

            return file;
        }
        /// <summary>
        /// Creates a navigation mesh from a navigation mesh file
        /// </summary>
        /// <param name="file">Navigation mesh file</param>
        /// <returns>Returns the navigation mesh</returns>
        public static NavMesh FromNavmeshFile(NavMeshFile file)
        {
            NavMesh navmesh = new NavMesh(file.NavMeshParams);

            foreach (var tile in file.NavMeshData)
            {
                if (tile == null || tile.Header.Magic != DetourUtils.DT_NAVMESH_MAGIC)
                {
                    continue;
                }

                navmesh.AddTile(tile, TileFlagTypes.FreeData, 0);
            }

            if (file.HasTileCache)
            {
                var tmproc = new TileCacheMeshProcess(null);

                navmesh.tileCache = new TileCache(navmesh, tmproc, file.TileCacheParams);

                foreach (var tile in file.TileCacheData)
                {
                    if (tile.Header.Magic != DetourTileCache.DT_TILECACHE_MAGIC)
                    {
                        continue;
                    }

                    navmesh.tileCache.AddTile(tile, CompressedTileFlagTypes.FreeData);
                }
            }

            return navmesh;
        }

        private Vector3 origin;
        private readonly float tileWidth;
        private readonly float tileHeight;
        private readonly int tileLutMask;
        private readonly MeshTile[] posLookup;
        private MeshTile nextFree;
        private readonly int tileBits;
        private readonly int polyBits;
        private readonly int saltBits;
        private NavMeshParams meshParams;
        private readonly MeshTile[] meshTiles;
        private readonly int maxTiles;
        private TileCache tileCache;

        /// <summary>
        /// Constructor
        /// </summary>
        public NavMesh(NavMeshParams nmparams)
        {
            meshParams = nmparams;
            origin = meshParams.Origin;
            tileWidth = meshParams.TileWidth;
            tileHeight = meshParams.TileHeight;

            // Init tiles
            maxTiles = meshParams.MaxTiles;
            var m_tileLutSize = Helper.NextPowerOfTwo(meshParams.MaxTiles / 4);
            if (m_tileLutSize == 0) m_tileLutSize = 1;
            tileLutMask = m_tileLutSize - 1;

            meshTiles = new MeshTile[maxTiles];
            posLookup = new MeshTile[m_tileLutSize];

            nextFree = null;
            for (int i = maxTiles - 1; i >= 0; --i)
            {
                meshTiles[i] = new MeshTile
                {
                    Salt = 1,
                    Next = nextFree
                };
                nextFree = meshTiles[i];
            }

            // Init ID generator values.
            tileBits = (int)Math.Log(Helper.NextPowerOfTwo(meshParams.MaxTiles), 2);
            polyBits = (int)Math.Log(Helper.NextPowerOfTwo(meshParams.MaxPolys), 2);
            // Only allow 31 salt bits, since the salt mask is calculated using 32bit uint and it will overflow.
            saltBits = Math.Min(31, 32 - tileBits - polyBits);

            if (saltBits < 10)
            {
                throw new EngineException("DT_INVALID_PARAM");
            }
        }

        /// <summary>
        /// Gets the navigation mesh parameters
        /// </summary>
        public NavMeshParams GetParams()
        {
            return meshParams;
        }
        /// <summary>
        /// Adds a new tile to the navigation mesh
        /// </summary>
        /// <param name="data">Mesh data</param>
        /// <param name="flags">Tile flags</param>
        /// <param name="lastRef">Last reference</param>
        /// <returns>Returns true if the tile was added</returns>
        public bool AddTile(MeshData data, TileFlagTypes flags, int lastRef)
        {
            // Make sure the data is in right format.
            MeshHeader header = data.Header;
            if (header.Magic != DetourUtils.DT_NAVMESH_MAGIC)
            {
                return false;
            }
            if (header.Version != DetourUtils.DT_NAVMESH_VERSION)
            {
                return false;
            }

            // Make sure the location is free.
            if (GetTileAt(header.X, header.Y, header.Layer) != null)
            {
                return false;
            }

            // Allocate a tile.
            MeshTile tile = AllocateTile(lastRef);

            // Make sure we could allocate a tile.
            if (tile == null)
            {
                return false;
            }

            // Insert tile into the position lut.
            int h = DetourUtils.ComputeTileHash(header.X, header.Y, tileLutMask);
            tile.Next = posLookup[h];
            posLookup[h] = tile;

            tile.Patch(header);

            // If there are no items in the bvtree, reset the tree pointer.
            if (data.NavBvtree == null)
            {
                tile.BvTree = null;
            }

            // Build links freelist
            tile.LinksFreeList = 0;
            tile.Links[header.MaxLinkCount - 1].Next = DetourUtils.DT_NULL_LINK;
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
                    int opposite = DetourUtils.OppositeTile(i);

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
        /// <param name="lastRef">Last reference</param>
        /// <returns>Returns the new tile</returns>
        private MeshTile AllocateTile(int lastRef)
        {
            MeshTile tile = null;

            if (lastRef == 0)
            {
                if (nextFree != null)
                {
                    tile = nextFree;
                    nextFree = tile.Next;
                    tile.Next = null;
                }

                return tile;
            }

            // Try to relocate the tile to specific index with same salt.
            int tileIndex = DecodePolyIdTile(lastRef);
            if (tileIndex >= maxTiles)
            {
                return null;
            }

            // Try to find the specific tile id from the free list.
            MeshTile target = meshTiles[tileIndex];
            MeshTile prev = null;
            tile = nextFree;
            while (tile != null && tile != target)
            {
                prev = tile;
                tile = tile.Next;
            }

            // Could not find the correct location.
            if (tile == target)
            {
                // Remove from freelist
                if (prev == null)
                {
                    nextFree = tile?.Next;
                }
                else
                {
                    prev.Next = tile?.Next;
                }

                // Restore salt.
                if (tile != null)
                {
                    tile.Salt = DecodePolyIdSalt(lastRef);
                }
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
            if ((tile.Flags & TileFlagTypes.FreeData) != 0)
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
            tile.Salt = (tile.Salt + 1) & ((1 << saltBits) - 1);
            if (tile.Salt == 0)
            {
                tile.Salt++;
            }

            // Add to free list.
            tile.Next = nextFree;
            nextFree = tile;

            return true;
        }
        /// <summary>
        /// Removes the tile from the lookup
        /// </summary>
        /// <param name="tile">Tile</param>
        private void RemoveFromHashLookup(MeshTile tile)
        {
            int h = DetourUtils.ComputeTileHash(tile.Header.X, tile.Header.Y, tileLutMask);
            MeshTile prev = null;
            MeshTile cur = posLookup[h];
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
                        posLookup[h] = cur.Next;
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
            x = (int)Math.Floor((pos.X - origin.X) / tileWidth);
            y = (int)Math.Floor((pos.Z - origin.Z) / tileHeight);
        }
        /// <summary>
        /// Gets all tiles
        /// </summary>
        /// <returns>Returns a tile list</returns>
        public IEnumerable<MeshTile> GetTiles()
        {
            return meshTiles.ToArray();
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
            int h = DetourUtils.ComputeTileHash(x, y, tileLutMask);
            var tile = posLookup[h];

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
            int h = DetourUtils.ComputeTileHash(x, y, tileLutMask);
            MeshTile tile = posLookup[h];
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
        public IEnumerable<MeshTile> GetTilesAt(int x, int y, int maxTiles)
        {
            List<MeshTile> tiles = new List<MeshTile>();

            // Find tile based on hash.
            int h = DetourUtils.ComputeTileHash(x, y, tileLutMask);
            var tile = posLookup[h];

            while (tile != null)
            {
                if (tile.Header.X == x && tile.Header.Y == y && tiles.Count < maxTiles)
                {
                    tiles.Add(tile);
                }

                tile = tile.Next;
            }

            return tiles.ToArray();
        }
        /// <summary>
        /// Gets the tile reference of the specified tile
        /// </summary>
        /// <param name="tile">Tile</param>
        /// <returns>Returns the tile reference</returns>
        public int GetTileRef(MeshTile tile)
        {
            if (tile == null) return 0;
            int it = Array.IndexOf(meshTiles, tile);
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
            if (tileIndex >= maxTiles)
            {
                return null;
            }
            var tile = meshTiles[tileIndex];
            if (tile.Salt != tileSalt)
            {
                return null;
            }
            return tile;
        }
        /// <summary>
        /// Gets the maximum number of tiles in the navigation mesh
        /// </summary>
        public int GetMaxTiles()
        {
            return maxTiles;
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
                Tile = meshTiles[it],
                Poly = meshTiles[it].Polys[ip],
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
                Tile = meshTiles[it],
                Poly = meshTiles[it].Polys[ip],
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

            if (it >= maxTiles) return false;
            if (meshTiles[it].Salt != salt || meshTiles[it].Header.Magic != DetourUtils.DT_NAVMESH_MAGIC) return false;
            if (ip >= meshTiles[it].Header.PolyCount) return false;

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

            var tile = meshTiles[it];
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
            var tile = meshTiles[it];
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
            var tile = meshTiles[it];
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
            var tile = meshTiles[it];
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
            var tile = meshTiles[it];
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
            var tile = meshTiles[it];
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
            return (salt << (polyBits + tileBits)) | (it << polyBits) | ip;
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
            int saltMask = (1 << saltBits) - 1;
            return (r >> (polyBits + tileBits)) & saltMask;
        }
        /// <summary>
        /// Decodes the polygon reference
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <returns>Returns the tile index</returns>
        public int DecodePolyIdTile(int r)
        {
            int tileMask = (1 << tileBits) - 1;
            return (r >> polyBits) & tileMask;
        }
        /// <summary>
        /// Decodes the polygon reference
        /// </summary>
        /// <param name="r">Polygon reference</param>
        /// <returns>Returns the polygon index</returns>
        public int DecodePolyIdPoly(int r)
        {
            int polyMask = (1 << polyBits) - 1;
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
        private IEnumerable<MeshTile> GetNeighbourTilesAt(int x, int y, int side, int maxTiles)
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
        private void FindConnectingPolys(Vector3 va, Vector3 vb, MeshTile tile, int side, int maxcon, out IEnumerable<int> con, out IEnumerable<Vector2> conarea)
        {
            List<int> conList = new List<int>();
            List<Vector2> conareaList = new List<Vector2>();

            DetourUtils.CalcSlabEndPoints(va, vb, out Vector2 amin, out Vector2 amax, side);
            float apos = DetourUtils.GetSlabCoord(va, side);

            // Remove links pointing to 'side' and compact the links array. 
            int m = DetourUtils.DT_EXT_LINK | side;
            int bse = GetTileRef(tile);

            var polys = tile.GetPolys();
            for (int i = 0; i < polys.Count(); ++i)
            {
                var poly = tile.Polys[i];
                int nv = poly.VertCount;

                for (int j = 0; j < nv; ++j)
                {
                    // Skip edges which do not point to the right side.
                    if (poly.Neis[j] != m)
                    {
                        continue;
                    }

                    Vector3 vc = tile.GetPolyVertex(poly, j);
                    Vector3 vd = tile.GetPolyVertex(poly, (j + 1) % nv);
                    float bpos = DetourUtils.GetSlabCoord(vc, side);

                    // Segments are not close enough.
                    if (Math.Abs(apos - bpos) > 0.01f)
                    {
                        continue;
                    }

                    // Check if the segments touch.
                    DetourUtils.CalcSlabEndPoints(vc, vd, out Vector2 bmin, out Vector2 bmax, side);

                    if (!DetourUtils.OverlapSlabs(amin, amax, bmin, bmax, 0.01f, tile.Header.WalkableClimb))
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

            con = conList.ToArray();
            conarea = conareaList.ToArray();
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
                poly.FirstLink = DetourUtils.DT_NULL_LINK;

                if (poly.Type == PolyTypes.OffmeshConnection)
                {
                    continue;
                }

                // Build edge links backwards so that the links will be
                // in the linked list from lowest index to highest.
                for (int j = poly.VertCount - 1; j >= 0; --j)
                {
                    // Skip hard and non-internal edges.
                    if (poly.Neis[j] == 0 || (poly.Neis[j] & DetourUtils.DT_EXT_LINK) != 0) continue;

                    int idx = tile.AllocLink();
                    if (idx != DetourUtils.DT_NULL_LINK)
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

                Vector3 halfExtents = new Vector3(new float[] { con.Rad, tile.Header.WalkableClimb, con.Rad });

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
                if (idx != DetourUtils.DT_NULL_LINK)
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
                if (tidx != DetourUtils.DT_NULL_LINK)
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
            foreach (var poly in polys)
            {
                // Create new links.
                CreatePolygonLinks(poly, tile, target, side);
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
                if ((poly.Neis[j] & DetourUtils.DT_EXT_LINK) == 0)
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
                FindConnectingPolys(va, vb, target, DetourUtils.OppositeTile(dir), 4, out var neis, out var neiareas);

                for (int k = 0; k < neis.Count(); k++)
                {
                    var nei = neis.ElementAt(k);
                    int idx = tile.AllocLink();
                    if (idx != DetourUtils.DT_NULL_LINK)
                    {
                        var link = new Link
                        {
                            NRef = nei,
                            Edge = j,
                            Side = dir,
                            Next = poly.FirstLink
                        };
                        poly.FirstLink = idx;

                        // Compress portal limits to an integer value.
                        Vector2Int? bounds = CompressPortalLimits(va, vb, dir, neiareas.ElementAt(k));
                        if (bounds.HasValue)
                        {
                            link.BMin = bounds.Value.X;
                            link.BMax = bounds.Value.Y;
                        }

                        tile.Links[idx] = link;
                    }
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
        private Vector2Int? CompressPortalLimits(Vector3 va, Vector3 vb, int dir, Vector2 neiarea)
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

                Vector2Int res = new Vector2Int
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

                Vector2Int res = new Vector2Int
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
            int oppositeSide = (side == -1) ? 0xff : DetourUtils.OppositeTile(side);

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
            if (targetPoly.FirstLink == DetourUtils.DT_NULL_LINK)
            {
                return;
            }

            Vector3 halfExtents = new Vector3(targetCon.Rad, target.Header.WalkableClimb, targetCon.Rad);

            // Find polygon to connect to.
            Vector3 p = targetCon.End;
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
            if (idx != DetourUtils.DT_NULL_LINK)
            {
                var link = new Link
                {
                    NRef = r,
                    Edge = 1,
                    Side = oppositeSide,
                    BMin = 0,
                    BMax = 0,
                    // Add to linked list.
                    Next = targetPoly.FirstLink
                };
                target.Links[idx] = link;
                targetPoly.FirstLink = idx;
            }

            // Link target poly to off-mesh connection.
            if ((targetCon.Flags & DetourUtils.DT_OFFMESH_CON_BIDIR) != 0)
            {
                int tidx = tile.AllocLink();
                if (tidx != DetourUtils.DT_NULL_LINK)
                {
                    var landPolyIdx = DecodePolyIdPoly(r);
                    var landPoly = tile.Polys[landPolyIdx];
                    var link = new Link
                    {
                        NRef = (GetTileRef(target) | (targetCon.Poly)),
                        Edge = 0xff,
                        Side = (side == -1 ? 0xff : side),
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
        /// Unconnect links
        /// </summary>
        /// <param name="tile">Mesh tile</param>
        /// <param name="target">Tile target</param>
        private void UnconnectLinks(MeshTile tile, MeshTile target)
        {
            int targetNum = DecodePolyIdTile(GetTileRef(target));

            var polys = tile.GetPolys();
            foreach (var poly in polys)
            {
                int j = poly.FirstLink;
                int pj = DetourUtils.DT_NULL_LINK;
                while (j != DetourUtils.DT_NULL_LINK)
                {
                    if (DecodePolyIdTile(tile.Links[j].NRef) == targetNum)
                    {
                        // Remove link.
                        int nj = tile.Links[j].Next;
                        if (pj == DetourUtils.DT_NULL_LINK)
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
        private IEnumerable<int> QueryPolygonsInTile(MeshTile tile, BoundingBox bounds, int maxPolys)
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
        private IEnumerable<int> QueryPolygonsInTileBVTree(MeshTile tile, BoundingBox bounds, int maxPolys)
        {
            List<int> polys = new List<int>(maxPolys);

            int nodeIndex = 0;
            int endIndex = tile.Header.BvNodeCount;
            Vector3 tbmin = tile.Header.BMin;
            Vector3 tbmax = tile.Header.BMax;
            float qfac = tile.Header.BvQuantFactor;

            // Calculate quantized box
            Int3 bmin = new Int3();
            Int3 bmax = new Int3();
            // dtClamp query box to world box.
            float minx = MathUtil.Clamp(bounds.Minimum.X, tbmin.X, tbmax.X) - tbmin.X;
            float miny = MathUtil.Clamp(bounds.Minimum.Y, tbmin.Y, tbmax.Y) - tbmin.Y;
            float minz = MathUtil.Clamp(bounds.Minimum.Z, tbmin.Z, tbmax.Z) - tbmin.Z;
            float maxx = MathUtil.Clamp(bounds.Maximum.X, tbmin.X, tbmax.X) - tbmin.X;
            float maxy = MathUtil.Clamp(bounds.Maximum.Y, tbmin.Y, tbmax.Y) - tbmin.Y;
            float maxz = MathUtil.Clamp(bounds.Maximum.Z, tbmin.Z, tbmax.Z) - tbmin.Z;
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

                bool overlap = DetourUtils.OverlapQuantBounds(bmin, bmax, node.BMin, node.BMax);
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

            return polys;
        }
        /// <summary>
        /// Performs a query in the polygons of the specified tile
        /// </summary>
        /// <param name="tile">Mesh tile</param>
        /// <param name="bounds">Bounds</param>
        /// <param name="maxPolys">Maximum resulting polygons</param>
        /// <returns>Returns a collection of polygon references</returns>
        private IEnumerable<int> QueryPolygonsInTileByRefs(MeshTile tile, BoundingBox bounds, int maxPolys)
        {
            List<int> polys = new List<int>(maxPolys);

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

            return polys;
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

            Vector3 bmin = Vector3.Subtract(center, halfExtents);
            Vector3 bmax = Vector3.Add(center, halfExtents);
            BoundingBox bounds = new BoundingBox(bmin, bmax);

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
                Vector3 diff = Vector3.Subtract(center, closestPtPoly);
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
        /// Gets the closest point on edges
        /// </summary>
        /// <param name="tile">Mest tile</param>
        /// <param name="poly">Polygon</param>
        /// <param name="pos">Position</param>
        /// <param name="onlyBoundary">Use only boundaries or not</param>
        /// <param name="closest">Resulting closest point</param>
        private void ClosestPointOnDetailEdges(MeshTile tile, Poly poly, Vector3 pos, bool onlyBoundary, out Vector3 closest)
        {
            var pd = tile.GetDetailMesh(poly);

            float dmin = float.MaxValue;
            float tmin = 0;
            Vector3 pmin = Vector3.Zero;
            Vector3 pmax = Vector3.Zero;

            int ANY_BOUNDARY_EDGE =
                ((int)DetailTriEdgeFlagTypes.Boundary << 0) |
                ((int)DetailTriEdgeFlagTypes.Boundary << 2) |
                ((int)DetailTriEdgeFlagTypes.Boundary << 4);

            for (int i = 0; i < pd.TriCount; i++)
            {
                var tris = tile.DetailTris[pd.TriBase + i];

                if (onlyBoundary && (tris.Flags & ANY_BOUNDARY_EDGE) == 0)
                {
                    continue;
                }

                Triangle v = tile.GetDetailTri(poly, pd.VertBase, tris);

                for (int k = 0, j = 2; k < 3; j = k++)
                {
                    var edgeFlags = tris.GetDetailTriEdgeFlags(j);

                    if (!edgeFlags.HasFlag(DetailTriEdgeFlagTypes.Boundary) &&
                        (onlyBoundary || tris[j] < tris[k]))
                    {
                        // Only looking at boundary edges and this is internal, or
                        // this is an inner edge that we will see again or have already seen.
                        continue;
                    }

                    float d = DetourUtils.DistancePtSegSqr2D(pos, v[j], v[k], out var t);
                    if (d < dmin)
                    {
                        dmin = d;
                        tmin = t;
                        pmin = v[j];
                        pmax = v[k];
                    }
                }
            }

            closest = Vector3.Lerp(pmin, pmax, tmin);
        }
        /// <summary>
        /// Gets the polygon height
        /// </summary>
        /// <param name="tile">Mesh tile</param>
        /// <param name="poly">Polygon</param>
        /// <param name="pos">Position</param>
        /// <param name="height">Resulting height</param>
        /// <returns>Returns true if the height were found</returns>
        public bool GetPolyHeight(MeshTile tile, Poly poly, Vector3 pos, out float height)
        {
            height = 0;

            // Off-mesh connections do not have detail polys and getting height
            // over them does not make sense.
            if (poly.Type == PolyTypes.OffmeshConnection)
            {
                return false;
            }

            var pd = tile.GetDetailMesh(poly);

            var verts = tile.GetPolyVerts(poly);

            if (!DetourUtils.PointInPolygon(pos, verts))
            {
                return false;
            }

            // Find height at the location.
            for (int j = 0; j < pd.TriCount; j++)
            {
                var t = tile.DetailTris[pd.TriBase + j];
                Vector3[] v = new Vector3[3];
                for (int k = 0; k < 3; ++k)
                {
                    if (t[k] < poly.VertCount)
                    {
                        v[k] = tile.Verts[poly.Verts[t[k]]];
                    }
                    else
                    {
                        v[k] = tile.DetailVerts[(pd.VertBase + (t[k] - poly.VertCount))];
                    }
                }

                if (DetourUtils.ClosestHeightPointTriangle(pos, v[0], v[1], v[2], out float h))
                {
                    height = h;
                    return true;
                }
            }

            // If all triangle checks failed above (can happen with degenerate triangles
            // or larger floating point values) the point is on an edge, so just select
            // closest. This should almost never happen so the extra iteration here is
            // ok.
            ClosestPointOnDetailEdges(tile, poly, pos, false, out var closest);

            height = closest.Y;

            return true;
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

            if (GetPolyHeight(tileRef.Tile, tileRef.Poly, pos, out float h))
            {
                closest.Y = h;
                posOverPoly = true;
                return;
            }

            // Off-mesh connections don't have detail polygons.
            if (tileRef.Poly.Type == PolyTypes.OffmeshConnection)
            {
                Vector3 v0 = tileRef.Tile.Verts[tileRef.Poly.Verts[0]];
                Vector3 v1 = tileRef.Tile.Verts[tileRef.Poly.Verts[1]];
                DetourUtils.DistancePtSegSqr2D(pos, v0, v1, out var t);
                closest = Vector3.Lerp(v0, v1, t);
                return;
            }

            // Outside poly that is not an offmesh connection.
            ClosestPointOnDetailEdges(tileRef.Tile, tileRef.Poly, pos, true, out closest);
        }

        /// <summary>
        /// Adds an obstacle
        /// </summary>
        /// <param name="obstacle">Obstacle</param>
        /// <returns>Returns the obstacle id</returns>
        public int AddObstacle(IObstacle obstacle)
        {
            if (tileCache == null)
            {
                return -1;
            }

            var status = tileCache.AddObstacle(obstacle, out int res);
            if (status == Status.Success)
            {
                return res;
            }

            return -1;
        }
        /// <summary>
        /// Removes an obstacle by obstacle id
        /// </summary>
        /// <param name="obstacleId">Obstacle id</param>
        public void RemoveObstacle(int obstacleId)
        {
            if (tileCache == null)
            {
                return;
            }

            tileCache.RemoveObstacle(obstacleId);
        }
        /// <summary>
        /// Updates the obstacles state
        /// </summary>
        /// <returns>Returns true when all the obstacles were updated</returns>
        public bool UpdateObstacles()
        {
            if (tileCache == null)
            {
                return false;
            }

            var status = tileCache.Update(out bool upToDate);
            if (status.HasFlag(Status.Success))
            {
                return upToDate;
            }

            return false;
        }
    }
}
