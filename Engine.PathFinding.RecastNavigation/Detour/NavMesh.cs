using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    using Engine.PathFinding.RecastNavigation.Detour.Tiles;
    using Engine.PathFinding.RecastNavigation.Recast;

    public class NavMesh
    {
        public const int MAX_LAYERS = 32;
        /// <summary>
        /// This value specifies how many layers (or "floors") each navmesh tile is expected to have.
        /// </summary>
        public const int EXPECTED_LAYERS_PER_TILE = 4;

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
        private static NavMesh BuildSolo(InputGeometry geometry, BuildSettings settings, Agent agent)
        {
            var bbox = settings.Bounds ?? geometry.BoundingBox;

            RecastUtils.CalcGridSize(bbox, settings.CellSize, out int width, out int height);

            // Generation params.
            var cfg = new Config()
            {
                CellSize = settings.CellSize,
                CellHeight = settings.CellHeight,
                WalkableSlopeAngle = agent.MaxSlope,
                WalkableHeight = (int)Math.Ceiling(agent.Height / settings.CellHeight),
                WalkableClimb = (int)Math.Floor(agent.MaxClimb / settings.CellHeight),
                WalkableRadius = (int)Math.Ceiling(agent.Radius / settings.CellSize),
                MaxEdgeLen = (int)(settings.EdgeMaxLength / settings.CellSize),
                MaxSimplificationError = settings.EdgeMaxError,
                MinRegionArea = (int)(settings.RegionMinSize * settings.RegionMinSize),
                MergeRegionArea = (int)(settings.RegionMergeSize * settings.RegionMergeSize),
                MaxVertsPerPoly = settings.VertsPerPoly,
                DetailSampleDist = settings.DetailSampleDist < 0.9f ? 0 : settings.CellSize * settings.DetailSampleDist,
                DetailSampleMaxError = settings.CellHeight * settings.DetailSampleMaxError,
                BoundingBox = bbox,
                Width = width,
                Height = height,
            };

            var solid = RecastUtils.CreateHeightfield(cfg.Width, cfg.Height, cfg.BoundingBox, cfg.CellSize, cfg.CellHeight);

            var ntris = geometry.ChunkyMesh.NTris;
            var tris = geometry.ChunkyMesh.Triangles;
            var triareas = new AreaTypes[ntris];

            RecastUtils.MarkWalkableTriangles(cfg.WalkableSlopeAngle, tris, triareas);
            if (!RecastUtils.RasterizeTriangles(solid, cfg.WalkableClimb, tris, triareas))
            {
                return null;
            }

            if (settings.FilterLowHangingObstacles)
            {
                RecastUtils.FilterLowHangingWalkableObstacles(cfg.WalkableClimb, solid);
            }
            if (settings.FilterLedgeSpans)
            {
                RecastUtils.FilterLedgeSpans(cfg.WalkableHeight, cfg.WalkableClimb, solid);
            }
            if (settings.FilterWalkableLowHeightSpans)
            {
                RecastUtils.FilterWalkableLowHeightSpans(cfg.WalkableHeight, solid);
            }

            if (!RecastUtils.BuildCompactHeightfield(cfg.WalkableHeight, cfg.WalkableClimb, solid, out CompactHeightfield chf))
            {
                throw new EngineException("buildNavigation: Could not build compact height field.");
            }

            // Erode the walkable area by agent radius.
            if (!RecastUtils.ErodeWalkableArea(cfg.WalkableRadius, chf))
            {
                throw new EngineException("buildNavigation: Could not erode.");
            }

            // (Optional) Mark areas.
            var vols = geometry.GetAreas().ToArray();
            for (int i = 0; i < geometry.GetAreaCount(); ++i)
            {
                RecastUtils.MarkConvexPolyArea(
                    vols[i].Vertices, vols[i].VertexCount,
                    vols[i].MinHeight, vols[i].MaxHeight,
                    (AreaTypes)vols[i].AreaType, chf);
            }

            if (settings.PartitionType == SamplePartitionTypes.Watershed)
            {
                // Prepare for region partitioning, by calculating distance field along the walkable surface.
                if (!RecastUtils.BuildDistanceField(chf))
                {
                    throw new EngineException("buildNavigation: Could not build distance field.");
                }

                // Partition the walkable surface into simple regions without holes.
                if (!RecastUtils.BuildRegions(chf, 0, cfg.MinRegionArea, cfg.MergeRegionArea))
                {
                    throw new EngineException("buildNavigation: Could not build watershed regions.");
                }
            }
            else if (settings.PartitionType == SamplePartitionTypes.Monotone)
            {
                // Partition the walkable surface into simple regions without holes.
                // Monotone partitioning does not need distancefield.
                if (!RecastUtils.BuildRegionsMonotone(chf, 0, cfg.MinRegionArea, cfg.MergeRegionArea))
                {
                    throw new EngineException("buildNavigation: Could not build monotone regions.");
                }
            }
            else if (settings.PartitionType == SamplePartitionTypes.Layers)
            {
                // Partition the walkable surface into simple regions without holes.
                var hasLayers = RecastUtils.BuildLayerRegions(chf, 0, cfg.MinRegionArea);
                if (!hasLayers)
                {
                    throw new EngineException("buildNavigation: Could not build layer regions.");
                }
            }

            if (!RecastUtils.BuildContours(chf, cfg.MaxSimplificationError, cfg.MaxEdgeLen, BuildContoursFlagTypes.RC_CONTOUR_TESS_WALL_EDGES, out ContourSet cset))
            {
                throw new EngineException("buildNavigation: Could not create contours.");
            }

            if (!RecastUtils.BuildPolyMesh(cset, cfg.MaxVertsPerPoly, out PolyMesh pmesh))
            {
                throw new EngineException("buildNavigation: Could not triangulate contours.");
            }

            if (!RecastUtils.BuildPolyMeshDetail(pmesh, chf, cfg.DetailSampleDist, cfg.DetailSampleMaxError, out PolyMeshDetail dmesh))
            {
                throw new EngineException("buildNavigation: Could not build detail mesh.");
            }

            if (cfg.MaxVertsPerPoly > DetourUtils.DT_VERTS_PER_POLYGON)
            {
                throw new EngineException($"buildNavigation: {cfg.MaxVertsPerPoly} is bigger than DetourUtils.DT_VERTS_PER_POLYGON ({DetourUtils.DT_VERTS_PER_POLYGON}).");
            }

            // Update poly flags from areas.
            for (int i = 0; i < pmesh.NPolys; ++i)
            {
                if ((int)pmesh.Areas[i] == (int)AreaTypes.RC_WALKABLE_AREA)
                {
                    pmesh.Areas[i] = SamplePolyAreas.SAMPLE_POLYAREA_GROUND;
                }

                if (pmesh.Areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_GROUND ||
                    pmesh.Areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_GRASS ||
                    pmesh.Areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_ROAD)
                {
                    pmesh.Flags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK;
                }
                else if (pmesh.Areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_WATER)
                {
                    pmesh.Flags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_SWIM;
                }
                else if (pmesh.Areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_DOOR)
                {
                    pmesh.Flags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK | SamplePolyFlagTypes.SAMPLE_POLYFLAGS_DOOR;
                }
            }

            var param = new NavMeshCreateParams
            {
                Verts = pmesh.Verts,
                VertCount = pmesh.NVerts,
                Polys = pmesh.Polys,
                PolyAreas = pmesh.Areas,
                PolyFlags = pmesh.Flags,
                polyCount = pmesh.NPolys,
                nvp = pmesh.NVP,
                detailMeshes = dmesh.meshes,
                detailVerts = dmesh.verts,
                detailVertsCount = dmesh.nverts,
                detailTris = dmesh.tris,
                detailTriCount = dmesh.ntris,
                offMeshCon = geometry.GetConnections()?.ToArray(),
                offMeshConCount = geometry.GetConnectionCount(),
                walkableHeight = agent.Height,
                walkableRadius = agent.Radius,
                walkableClimb = agent.MaxClimb,
                bmin = pmesh.BMin,
                bmax = pmesh.BMax,
                cs = cfg.CellSize,
                ch = cfg.CellHeight,
                buildBvTree = true
            };

            if (!DetourUtils.CreateNavMeshData(param, out MeshData navData))
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
            nm.AddTile(navData, TileFlagTypes.DT_TILE_FREE_DATA, 0);
            return nm;
        }
        private static NavMesh BuildTiled(InputGeometry geometry, BuildSettings settings, Agent agent)
        {
            var bbox = settings.Bounds ?? geometry.BoundingBox;

            // Init cache
            RecastUtils.CalcGridSize(bbox, settings.CellSize, out int gridWidth, out int gridHeight);
            int tileSize = (int)settings.TileSize;
            int tileWidth = (gridWidth + tileSize - 1) / tileSize;
            int tileHeight = (gridHeight + tileSize - 1) / tileSize;
            float tileCellSize = settings.TileSize * settings.CellSize;

            int tileBits = Math.Min((int)Math.Log(Helper.NextPowerOfTwo(tileWidth * tileHeight), 2), 14);
            if (tileBits > 14) tileBits = 14;
            int polyBits = 22 - tileBits;
            int maxTiles = 1 << tileBits;
            int maxPolysPerTile = 1 << polyBits;

            var nmparams = new NavMeshParams()
            {
                Origin = bbox.Minimum,
                TileWidth = tileCellSize,
                TileHeight = tileCellSize,
                MaxTiles = maxTiles,
                MaxPolys = maxPolysPerTile,
            };

            var nm = new NavMesh(nmparams);

            TileParams tileParams = new TileParams
            {
                Width = tileWidth,
                Height = tileHeight,
                CellSize = tileCellSize,
                Bounds = bbox,
            };

            if (!BuildAllTiles(nm, tileParams, geometry, settings, agent))
            {
                return null;
            }

            if (settings.UseTileCache)
            {
                nm.TileCache = BuildTileCache(nm, tileWidth, tileHeight, bbox, geometry, settings, agent);
            }

            return nm;
        }

        private static TileCache BuildTileCache(NavMesh nm, int tileWidth, int tileHeight, BoundingBox bbox, InputGeometry geometry, BuildSettings settings, Agent agent)
        {
            // Generation params.
            var walkableHeight = (int)Math.Ceiling(agent.Height / settings.CellHeight);
            var walkableClimb = (int)Math.Floor(agent.MaxClimb / settings.CellHeight);
            var walkableRadius = (int)Math.Ceiling(agent.Radius / settings.CellSize);
            var tileSize = (int)settings.TileSize;
            var borderSize = walkableRadius + 3;
            var cfg = new Config()
            {
                CellSize = settings.CellSize,
                CellHeight = settings.CellHeight,
                WalkableSlopeAngle = agent.MaxSlope,
                WalkableHeight = walkableHeight,
                WalkableClimb = walkableClimb,
                WalkableRadius = walkableRadius,
                MaxEdgeLen = (int)(settings.EdgeMaxLength / settings.CellSize),
                MaxSimplificationError = settings.EdgeMaxError,
                MinRegionArea = (int)(settings.RegionMinSize * settings.RegionMinSize),
                MergeRegionArea = (int)(settings.RegionMergeSize * settings.RegionMergeSize),
                MaxVertsPerPoly = settings.VertsPerPoly,
                TileSize = tileSize,
                BorderSize = borderSize,
                Width = tileSize + borderSize * 2,
                Height = tileSize + borderSize * 2,
                DetailSampleDist = settings.DetailSampleDist < 0.9f ? 0 : settings.CellSize * settings.DetailSampleDist,
                DetailSampleMaxError = settings.CellHeight * settings.DetailSampleMaxError,
                BoundingBox = bbox,
            };

            // Tile cache params.
            var tcparams = new TileCacheParams()
            {
                Origin = bbox.Minimum,
                CellSize = settings.CellSize,
                CellHeight = settings.CellHeight,
                Width = (int)settings.TileSize,
                Height = (int)settings.TileSize,
                WalkableHeight = agent.Height,
                WalkableRadius = agent.Radius,
                WalkableClimb = agent.MaxClimb,
                MaxSimplificationError = settings.EdgeMaxError,
                MaxTiles = tileWidth * tileHeight * EXPECTED_LAYERS_PER_TILE,
                MaxObstacles = 128,
            };
            var tmproc = new TileCacheMeshProcess(geometry);

            var tileCache = new TileCache(tcparams, tmproc);

            for (int y = 0; y < tileHeight; y++)
            {
                for (int x = 0; x < tileWidth; x++)
                {
                    var tiles = RasterizeTileLayers(x, y, cfg, geometry, settings);

                    foreach (var tile in tiles)
                    {
                        tileCache.AddTile(tile, CompressedTileFlagTypes.DT_COMPRESSEDTILE_FREE_DATA);
                    }
                }
            }

            // Build initial meshes
            for (int y = 0; y < tileHeight; y++)
            {
                for (int x = 0; x < tileWidth; x++)
                {
                    tileCache.BuildNavMeshTilesAt(x, y, nm);
                }
            }

            return tileCache;
        }
        private static IEnumerable<TileCacheData> RasterizeTileLayers(int x, int y, Config cfg, InputGeometry geometry, BuildSettings settings)
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

            var solid = RecastUtils.CreateHeightfield(cfg.Width, cfg.Height, cfg.BoundingBox, cfg.CellSize, cfg.CellHeight);

            var rc = new RasterizationContext
            {
                // Allocate voxel heightfield where we rasterize our input data to.
                solid = solid,

                // Allocate array that can hold triangle flags.
                // If you have multiple meshes you need to process, allocate
                // and array which can hold the max number of triangles you need to process.
                triareas = new AreaTypes[chunkyMesh.MaxTrisPerChunk],

                tiles = new TileCacheData[RasterizationContext.MaxLayers],
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

                Helper.InitializeArray(rc.triareas, AreaTypes.RC_NULL_AREA);

                RecastUtils.MarkWalkableTriangles(cfg.WalkableSlopeAngle, tris, rc.triareas);

                if (!RecastUtils.RasterizeTriangles(rc.solid, cfg.WalkableClimb, tris, rc.triareas))
                {
                    return tiles.ToArray();
                }
            }

            // Once all geometry is rasterized, we do initial pass of filtering to
            // remove unwanted overhangs caused by the conservative rasterization
            // as well as filter spans where the character cannot possibly stand.
            if (settings.FilterLowHangingObstacles)
            {
                RecastUtils.FilterLowHangingWalkableObstacles(cfg.WalkableClimb, rc.solid);
            }
            if (settings.FilterLedgeSpans)
            {
                RecastUtils.FilterLedgeSpans(cfg.WalkableHeight, cfg.WalkableClimb, rc.solid);
            }
            if (settings.FilterWalkableLowHeightSpans)
            {
                RecastUtils.FilterWalkableLowHeightSpans(cfg.WalkableHeight, rc.solid);
            }

            if (!RecastUtils.BuildCompactHeightfield(cfg.WalkableHeight, cfg.WalkableClimb, rc.solid, out rc.chf))
            {
                throw new EngineException("buildNavigation: Could not build compact height field.");
            }

            // Erode the walkable area by agent radius.
            if (!RecastUtils.ErodeWalkableArea(cfg.WalkableRadius, rc.chf))
            {
                throw new EngineException("buildNavigation: Could not erode.");
            }

            // (Optional) Mark areas.
            var vols = geometry.GetAreas().ToArray();
            for (int i = 0; i < geometry.GetAreaCount(); ++i)
            {
                RecastUtils.MarkConvexPolyArea(
                    vols[i].Vertices, vols[i].VertexCount,
                    vols[i].MinHeight, vols[i].MaxHeight,
                    (AreaTypes)vols[i].AreaType, rc.chf);
            }

            RecastUtils.BuildHeightfieldLayers(rc.chf, cfg.BorderSize, cfg.WalkableHeight, out rc.lset);

            rc.ntiles = 0;
            for (int i = 0; i < Math.Min(rc.lset.NLayers, MAX_LAYERS); i++)
            {
                var layer = rc.lset.Layers[i];

                var tile = rc.tiles[rc.ntiles];

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
                DetourTileCache.BuildTileCacheLayer(layer.Heights, layer.Areas, layer.Cons, out var data);

                tile.Data = data;

                rc.tiles[rc.ntiles++] = tile;
            }

            // Transfer ownsership of tile data from build context to the caller.
            for (int i = 0; i < Math.Min(rc.ntiles, MAX_LAYERS); i++)
            {
                tiles.Add(rc.tiles[i]);
                rc.tiles[i].Data = TileCacheLayerData.Empty;
            }

            return tiles.ToArray();
        }

        public static void GetTileAtPosition(Vector3 pos, InputGeometry geom, BuildSettings settings, out int x, out int y, out BoundingBox tileBounds)
        {
            var bbox = settings.Bounds ?? geom.BoundingBox;

            float tileCellSize = settings.TileSize * settings.CellSize;
            x = (int)((pos.X - bbox.Minimum.X) / tileCellSize);
            y = (int)((pos.Z - bbox.Minimum.Z) / tileCellSize);

            tileBounds = GetTileBounds(x, y, tileCellSize, bbox);
        }
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
        private static bool BuildAllTiles(NavMesh navMesh, TileParams tileParams, InputGeometry geom, BuildSettings settings, Agent agent)
        {
            for (int y = 0; y < tileParams.Height; y++)
            {
                for (int x = 0; x < tileParams.Width; x++)
                {
                    BoundingBox tileBounds = GetTileBounds(x, y, tileParams.CellSize, tileParams.Bounds);

                    var data = BuildTileMesh(x, y, tileBounds, geom, settings, agent);
                    if (data != null)
                    {
                        // Remove any previous data (navmesh owns and deletes the data).
                        navMesh.RemoveTile(navMesh.GetTileRefAt(x, y, 0));
                        // Let the navmesh own the data.
                        navMesh.AddTile(data, TileFlagTypes.DT_TILE_FREE_DATA, 0);
                    }
                }
            }

            return true;
        }
        private static MeshData BuildTileMesh(int x, int y, BoundingBox tileBounds, InputGeometry geometry, BuildSettings settings, Agent agent)
        {
            var chunkyMesh = geometry.ChunkyMesh;

            int walkableRadius = (int)Math.Ceiling(agent.Radius / settings.CellSize);
            int tileSize = (int)settings.TileSize;
            int borderSize = walkableRadius + 3;

            // Init build configuration from GUI
            Config cfg = new Config
            {
                CellSize = settings.CellSize,
                CellHeight = settings.CellHeight,
                WalkableSlopeAngle = agent.MaxSlope,
                WalkableHeight = (int)Math.Ceiling(agent.Height / settings.CellHeight),
                WalkableClimb = (int)Math.Floor(agent.MaxClimb / settings.CellHeight),
                WalkableRadius = walkableRadius,
                MaxEdgeLen = (int)(settings.EdgeMaxLength / settings.CellSize),
                MaxSimplificationError = settings.EdgeMaxError,
                MinRegionArea = (int)(settings.RegionMinSize * settings.RegionMinSize),     // Note: area = size*size
                MergeRegionArea = (int)(settings.RegionMergeSize * settings.RegionMergeSize), // Note: area = size*size
                MaxVertsPerPoly = settings.VertsPerPoly,
                TileSize = tileSize,
                BorderSize = borderSize, // Reserve enough padding.
                Width = tileSize + borderSize * 2,
                Height = tileSize + borderSize * 2,
                DetailSampleDist = settings.DetailSampleDist < 0.9f ? 0 : settings.CellSize * settings.DetailSampleDist,
                DetailSampleMaxError = settings.CellHeight * settings.DetailSampleMaxError,
                BoundingBox = tileBounds,
            };

            // Expand the heighfield bounding box by border size to find the extents of geometry we need to build this tile.
            //
            // This is done in order to make sure that the navmesh tiles connect correctly at the borders,
            // and the obstacles close to the border work correctly with the dilation process.
            // No polygons (or contours) will be created on the border area.
            //
            // IMPORTANT!
            //
            //   :''''''''':
            //   : +-----+ :
            //   : |     | :
            //   : |     |<--- tile to build
            //   : |     | :  
            //   : +-----+ :<-- geometry needed
            //   :.........:
            //
            // You should use this bounding box to query your input geometry.
            //
            // For example if you build a navmesh for terrain, and want the navmesh tiles to match the terrain tile size
            // you will need to pass in data from neighbour terrain tiles too! In a simple case, just pass in all the 8 neighbours,
            // or use the bounding box below to only pass in a sliver of each of the 8 neighbours.
            var tmpBbox = cfg.BoundingBox;
            tmpBbox.Minimum.X -= borderSize * settings.CellSize;
            tmpBbox.Minimum.Z -= borderSize * settings.CellSize;
            tmpBbox.Maximum.X += borderSize * settings.CellSize;
            tmpBbox.Maximum.Z += borderSize * settings.CellSize;
            cfg.BoundingBox = tmpBbox;

            // Allocate voxel heightfield where we rasterize our input data to.
            var solid = RecastUtils.CreateHeightfield(cfg.Width, cfg.Height, cfg.BoundingBox, cfg.CellSize, cfg.CellHeight);

            // Allocate array that can hold triangle flags.
            // If you have multiple meshes you need to process, allocate
            // and array which can hold the max number of triangles you need to process.
            AreaTypes[] triareas = new AreaTypes[chunkyMesh.MaxTrisPerChunk];

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

                Helper.InitializeArray(triareas, AreaTypes.RC_NULL_AREA);

                RecastUtils.MarkWalkableTriangles(cfg.WalkableSlopeAngle, tris, triareas);

                if (!RecastUtils.RasterizeTriangles(solid, cfg.WalkableClimb, tris, triareas))
                {
                    return null;
                }
            }

            // Once all geometry is rasterized, we do initial pass of filtering to
            // remove unwanted overhangs caused by the conservative rasterization
            // as well as filter spans where the character cannot possibly stand.
            if (settings.FilterLowHangingObstacles)
            {
                RecastUtils.FilterLowHangingWalkableObstacles(cfg.WalkableClimb, solid);
            }
            if (settings.FilterLedgeSpans)
            {
                RecastUtils.FilterLedgeSpans(cfg.WalkableHeight, cfg.WalkableClimb, solid);
            }
            if (settings.FilterWalkableLowHeightSpans)
            {
                RecastUtils.FilterWalkableLowHeightSpans(cfg.WalkableHeight, solid);
            }

            // Compact the heightfield so that it is faster to handle from now on.
            // This will result more cache coherent data as well as the neighbours
            // between walkable cells will be calculated.
            if (!RecastUtils.BuildCompactHeightfield(cfg.WalkableHeight, cfg.WalkableClimb, solid, out CompactHeightfield chf))
            {
                return null;
            }

            // Erode the walkable area by agent radius.
            if (!RecastUtils.ErodeWalkableArea(cfg.WalkableRadius, chf))
            {
                return null;
            }

            // (Optional) Mark areas.
            var vols = geometry.GetAreas().ToArray();
            for (int i = 0; i < geometry.GetAreaCount(); ++i)
            {
                RecastUtils.MarkConvexPolyArea(
                    vols[i].Vertices, vols[i].VertexCount,
                    vols[i].MinHeight, vols[i].MaxHeight,
                    (AreaTypes)vols[i].AreaType, chf);
            }

            // Partition the heightfield so that we can use simple algorithm later to triangulate the walkable areas.
            // There are 3 martitioning methods, each with some pros and cons:
            // 1) Watershed partitioning
            //   - the classic Recast partitioning
            //   - creates the nicest tessellation
            //   - usually slowest
            //   - partitions the heightfield into nice regions without holes or overlaps
            //   - the are some corner cases where this method creates produces holes and overlaps
            //      - holes may appear when a small obstacles is close to large open area (triangulation can handle this)
            //      - overlaps may occur if you have narrow spiral corridors (i.e stairs), this make triangulation to fail
            //   * generally the best choice if you precompute the nacmesh, use this if you have large open areas
            // 2) Monotone partioning
            //   - fastest
            //   - partitions the heightfield into regions without holes and overlaps (guaranteed)
            //   - creates long thin polygons, which sometimes causes paths with detours
            //   * use this if you want fast navmesh generation
            // 3) Layer partitoining
            //   - quite fast
            //   - partitions the heighfield into non-overlapping regions
            //   - relies on the triangulation code to cope with holes (thus slower than monotone partitioning)
            //   - produces better triangles than monotone partitioning
            //   - does not have the corner cases of watershed partitioning
            //   - can be slow and create a bit ugly tessellation (still better than monotone)
            //     if you have large open areas with small obstacles (not a problem if you use tiles)
            //   * good choice to use for tiled navmesh with medium and small sized tiles

            if (settings.PartitionType == SamplePartitionTypes.Watershed)
            {
                // Prepare for region partitioning, by calculating distance field along the walkable surface.
                if (!RecastUtils.BuildDistanceField(chf))
                {
                    return null;
                }

                // Partition the walkable surface into simple regions without holes.
                if (!RecastUtils.BuildRegions(chf, cfg.BorderSize, cfg.MinRegionArea, cfg.MergeRegionArea))
                {
                    return null;
                }
            }
            else if (settings.PartitionType == SamplePartitionTypes.Monotone)
            {
                // Partition the walkable surface into simple regions without holes.
                // Monotone partitioning does not need distancefield.
                if (!RecastUtils.BuildRegionsMonotone(chf, cfg.BorderSize, cfg.MinRegionArea, cfg.MergeRegionArea))
                {
                    return null;
                }
            }
            else if (settings.PartitionType == SamplePartitionTypes.Layers)
            {
                // Partition the walkable surface into simple regions without holes.
                var hasLayerRegions = RecastUtils.BuildLayerRegions(chf, cfg.BorderSize, cfg.MinRegionArea);
                if (!hasLayerRegions)
                {
                    return null;
                }
            }

            // Create contours.
            if (!RecastUtils.BuildContours(chf, cfg.MaxSimplificationError, cfg.MaxEdgeLen, BuildContoursFlagTypes.RC_CONTOUR_TESS_WALL_EDGES, out ContourSet cset))
            {
                return null;
            }

            if (cset.nconts == 0)
            {
                return null;
            }

            // Build polygon navmesh from the contours.
            if (!RecastUtils.BuildPolyMesh(cset, cfg.MaxVertsPerPoly, out PolyMesh pmesh))
            {
                return null;
            }

            // Build detail mesh.
            if (!RecastUtils.BuildPolyMeshDetail(pmesh, chf, cfg.DetailSampleDist, cfg.DetailSampleMaxError, out PolyMeshDetail dmesh))
            {
                return null;
            }

            if (cfg.MaxVertsPerPoly <= DetourUtils.DT_VERTS_PER_POLYGON)
            {
                // Update poly flags from areas.
                for (int i = 0; i < pmesh.NPolys; ++i)
                {
                    if ((int)pmesh.Areas[i] == (int)AreaTypes.RC_WALKABLE_AREA)
                    {
                        pmesh.Areas[i] = SamplePolyAreas.SAMPLE_POLYAREA_GROUND;
                    }

                    if (pmesh.Areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_GROUND ||
                        pmesh.Areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_GRASS ||
                        pmesh.Areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_ROAD)
                    {
                        pmesh.Flags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK;
                    }
                    else if (pmesh.Areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_WATER)
                    {
                        pmesh.Flags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_SWIM;
                    }
                    else if (pmesh.Areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_DOOR)
                    {
                        pmesh.Flags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK | SamplePolyFlagTypes.SAMPLE_POLYFLAGS_DOOR;
                    }
                }

                var param = new NavMeshCreateParams
                {
                    Verts = pmesh.Verts,
                    VertCount = pmesh.NVerts,
                    Polys = pmesh.Polys,
                    PolyAreas = pmesh.Areas,
                    PolyFlags = pmesh.Flags,
                    polyCount = pmesh.NPolys,
                    nvp = pmesh.NVP,
                    detailMeshes = dmesh.meshes,
                    detailVerts = dmesh.verts,
                    detailVertsCount = dmesh.nverts,
                    detailTris = dmesh.tris,
                    detailTriCount = dmesh.ntris,
                    offMeshCon = geometry.GetConnections().ToArray(),
                    offMeshConCount = geometry.GetConnectionCount(),
                    walkableHeight = agent.Height,
                    walkableRadius = agent.Radius,
                    walkableClimb = agent.MaxClimb,
                    tileX = x,
                    tileY = y,
                    tileLayer = 0,
                    bmin = pmesh.BMin,
                    bmax = pmesh.BMax,
                    cs = cfg.CellSize,
                    ch = cfg.CellHeight,
                    buildBvTree = true
                };

                if (!DetourUtils.CreateNavMeshData(param, out MeshData navData))
                {
                    return null;
                }

                return navData;
            }

            return null;
        }

        public void BuildTileAtPosition(int x, int y, BoundingBox tbbox, InputGeometry geom, BuildSettings settings, Agent agent)
        {
            if (settings.UseTileCache && TileCache != null)
            {
                TileCache.BuildNavMeshTilesAt(x, y, this);
            }
            else
            {
                var data = BuildTileMesh(x, y, tbbox, geom, settings, agent);

                // Remove any previous data (navmesh owns and deletes the data).
                RemoveTile(GetTileRefAt(x, y, 0));

                // Add tile, or leave the location empty.
                if (data != null)
                {
                    AddTile(data, TileFlagTypes.DT_TILE_FREE_DATA, 0);
                }
            }
        }
        public void RemoveTileAtPosition(int x, int y, BuildSettings settings)
        {
            if (settings.UseTileCache && TileCache != null)
            {
                var t = GetTileAt(x, y, 0);
                int r = GetTileRef(t);
                TileCache.RemoveTile(r, out _, out _);
            }
            else
            {
                RemoveTile(GetTileRefAt(x, y, 0));
            }
        }

        private Vector3 m_orig;
        private readonly float m_tileWidth;
        private readonly float m_tileHeight;
        private readonly int m_tileLutMask;
        private readonly MeshTile[] m_posLookup;
        private MeshTile m_nextFree = null;
        private readonly int m_tileBits;
        private readonly int m_polyBits;
        private readonly int m_saltBits;
        private NavMeshParams m_params;

        public MeshTile[] Tiles { get; set; }
        public int MaxTiles { get; set; }
        public TileCache TileCache { get; set; }

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
            if (m_tileLutSize == 0) m_tileLutSize = 1;
            m_tileLutMask = m_tileLutSize - 1;

            Tiles = new MeshTile[MaxTiles];
            m_posLookup = new MeshTile[m_tileLutSize];

            m_nextFree = null;
            for (int i = MaxTiles - 1; i >= 0; --i)
            {
                Tiles[i] = new MeshTile
                {
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

        public NavMeshParams GetParams()
        {
            return m_params;
        }
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
            MeshTile tile = null;
            if (lastRef == 0)
            {
                if (m_nextFree != null)
                {
                    tile = m_nextFree;
                    m_nextFree = tile.Next;
                    tile.Next = null;
                }
            }
            else
            {
                // Try to relocate the tile to specific index with same salt.
                int tileIndex = DecodePolyIdTile(lastRef);
                if (tileIndex >= MaxTiles)
                {
                    return false;
                }
                // Try to find the specific tile id from the free list.
                MeshTile target = Tiles[tileIndex];
                MeshTile prev = null;
                tile = m_nextFree;
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
                        m_nextFree = tile?.Next;
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
            }

            // Make sure we could allocate a tile.
            if (tile == null)
            {
                return false;
            }

            // Insert tile into the position lut.
            int h = DetourUtils.ComputeTileHash(header.X, header.Y, m_tileLutMask);
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
        public bool RemoveTile(MeshTile tile)
        {
            if (tile == null)
            {
                return false;
            }

            // Remove tile from hash lookup.
            int h = DetourUtils.ComputeTileHash(tile.Header.X, tile.Header.Y, m_tileLutMask);
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

            // Remove connections to neighbour tiles.
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
        public void CalcTileLoc(Vector3 pos, out int x, out int y)
        {
            x = (int)Math.Floor((pos.X - m_orig.X) / m_tileWidth);
            y = (int)Math.Floor((pos.Z - m_orig.Z) / m_tileHeight);
        }
        public MeshTile GetTileAt(int x, int y, int layer)
        {
            // Find tile based on hash.
            int h = DetourUtils.ComputeTileHash(x, y, m_tileLutMask);
            MeshTile tile = m_posLookup[h];
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
        public IEnumerable<MeshTile> GetTilesAt(int x, int y, int maxTiles)
        {
            List<MeshTile> tiles = new List<MeshTile>();

            // Find tile based on hash.
            int h = DetourUtils.ComputeTileHash(x, y, m_tileLutMask);
            var tile = m_posLookup[h];

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
        public MeshTile GetTileRefAt(int x, int y, int layer)
        {
            // Find tile based on hash.
            int h = DetourUtils.ComputeTileHash(x, y, m_tileLutMask);
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
        public int GetTileRef(MeshTile tile)
        {
            if (tile == null) return 0;
            int it = Array.IndexOf(Tiles, tile);
            return EncodePolyId(tile.Salt, it, 0);
        }
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
        public int GetMaxTiles()
        {
            return MaxTiles;
        }
        public MeshTile GetTile(int i)
        {
            return Tiles[i];
        }
        public TileRef GetTileAndPolyByRef(int r)
        {
            if (r == 0) return TileRef.Null;
            DecodePolyId(r, out int salt, out int it, out int ip);
            if (it >= MaxTiles) return TileRef.Null;
            if (Tiles[it].Salt != salt || Tiles[it].Header.Magic != DetourUtils.DT_NAVMESH_MAGIC) return TileRef.Null;
            if (ip >= Tiles[it].Header.PolyCount) return TileRef.Null;

            return new TileRef
            {
                Ref = r,
                Tile = Tiles[it],
                Poly = Tiles[it].Polys[ip],
            };
        }
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
        public bool IsValidPolyRef(int r)
        {
            if (r == 0) return false;

            DecodePolyId(r, out int salt, out int it, out int ip);

            if (it >= MaxTiles) return false;
            if (Tiles[it].Salt != salt || Tiles[it].Header.Magic != DetourUtils.DT_NAVMESH_MAGIC) return false;
            if (ip >= Tiles[it].Header.PolyCount) return false;

            return true;
        }
        public bool GetOffMeshConnectionPolyEndPoints(int prevRef, int polyRef, out Vector3 startPos, out Vector3 endPos)
        {
            startPos = Vector3.Zero;
            endPos = Vector3.Zero;

            if (polyRef == 0)
            {
                return false;
            }

            // Get current polygon
            DecodePolyId(polyRef, out int salt, out int it, out int ip);
            if (it >= MaxTiles)
            {
                return false;
            }
            if (Tiles[it].Salt != salt || Tiles[it].Header.Magic != DetourUtils.DT_NAVMESH_MAGIC)
            {
                return false;
            }
            var tile = Tiles[it];
            if (ip >= tile.Header.PolyCount)
            {
                return false;
            }
            var poly = tile.Polys[ip];

            // Figure out which way to hand out the vertices.
            return tile.FindOffMeshConnectionEndpoints(poly, prevRef, out startPos, out endPos);
        }
        public OffMeshConnection GetOffMeshConnectionByRef(int r)
        {
            if (r == 0)
            {
                return null;
            }

            // Get current polygon
            DecodePolyId(r, out int salt, out int it, out int ip);
            if (it >= MaxTiles)
            {
                return null;
            }
            if (Tiles[it].Salt != salt || Tiles[it].Header.Magic != DetourUtils.DT_NAVMESH_MAGIC)
            {
                return null;
            }
            var tile = Tiles[it];

            return tile.GetOffMeshConnectionByPolygon(ip);
        }
        public bool SetPolyFlags(int r, SamplePolyFlagTypes flags)
        {
            if (r == 0)
            {
                return false;
            }

            DecodePolyId(r, out int salt, out int it, out int ip);
            if (it >= MaxTiles)
            {
                return false;
            }
            if (Tiles[it].Salt != salt || Tiles[it].Header.Magic != DetourUtils.DT_NAVMESH_MAGIC)
            {
                return false;
            }

            var tile = Tiles[it];
            if (ip >= tile.Header.PolyCount)
            {
                return false;
            }

            var poly = tile.Polys[ip];

            // Change flags.
            poly.Flags = flags;

            return true;
        }
        public bool GetPolyFlags(int r, out SamplePolyFlagTypes resultFlags)
        {
            resultFlags = 0;

            if (r == 0)
            {
                return false;
            }

            DecodePolyId(r, out int salt, out int it, out int ip);
            if (it >= MaxTiles)
            {
                return false;
            }
            if (Tiles[it].Salt != salt || Tiles[it].Header.Magic != DetourUtils.DT_NAVMESH_MAGIC)
            {
                return false;
            }

            var tile = Tiles[it];
            if (ip >= tile.Header.PolyCount)
            {
                return false;
            }

            var poly = tile.Polys[ip];

            resultFlags = poly.Flags;

            return true;
        }
        public bool SetPolyArea(int r, SamplePolyAreas area)
        {
            if (r == 0)
            {
                return false;
            }

            DecodePolyId(r, out int salt, out int it, out int ip);
            if (it >= MaxTiles)
            {
                return false;
            }
            if (Tiles[it].Salt != salt || Tiles[it].Header.Magic != DetourUtils.DT_NAVMESH_MAGIC)
            {
                return false;
            }

            var tile = Tiles[it];
            if (ip >= tile.Header.PolyCount)
            {
                return false;
            }

            var poly = tile.Polys[ip];

            poly.Area = area;

            return true;
        }
        public bool GetPolyArea(int r, out SamplePolyAreas resultArea)
        {
            resultArea = 0;

            if (r == 0)
            {
                return false;
            }

            DecodePolyId(r, out int salt, out int it, out int ip);
            if (it >= MaxTiles)
            {
                return false;
            }
            if (Tiles[it].Salt != salt || Tiles[it].Header.Magic != DetourUtils.DT_NAVMESH_MAGIC)
            {
                return false;
            }

            var tile = Tiles[it];
            if (ip >= tile.Header.PolyCount)
            {
                return false;
            }

            var poly = tile.Polys[ip];

            resultArea = poly.Area;

            return true;
        }
        public int EncodePolyId(int salt, int it, int ip)
        {
            return (salt << (m_polyBits + m_tileBits)) | (it << m_polyBits) | ip;
        }
        public void DecodePolyId(int r, out int salt, out int it, out int ip)
        {
            int saltMask = (1 << m_saltBits) - 1;
            int tileMask = (1 << m_tileBits) - 1;
            int polyMask = (1 << m_polyBits) - 1;
            salt = ((r >> (m_polyBits + m_tileBits)) & saltMask);
            it = ((r >> m_polyBits) & tileMask);
            ip = (r & polyMask);
        }
        public int DecodePolyIdSalt(int r)
        {
            int saltMask = (1 << m_saltBits) - 1;
            return ((r >> (m_polyBits + m_tileBits)) & saltMask);
        }
        public int DecodePolyIdTile(int r)
        {
            int tileMask = (1 << m_tileBits) - 1;
            return ((r >> m_polyBits) & tileMask);
        }
        public int DecodePolyIdPoly(int r)
        {
            int polyMask = (1 << m_polyBits) - 1;
            return (r & polyMask);
        }

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
        private bool FindConnectingPolys(Vector3 va, Vector3 vb, MeshTile tile, int side, int maxcon, out IEnumerable<int> con, out IEnumerable<Vector2> conarea)
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

            return true;
        }
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

                    int idx = DetourUtils.AllocLink(tile);
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
                int idx = DetourUtils.AllocLink(tile);
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
                int tidx = DetourUtils.AllocLink(tile);
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
        private void ConnectExtLinks(MeshTile tile, MeshTile target, int side)
        {
            // Connect border links.
            var polys = tile.GetPolys();
            foreach (var poly in polys)
            {
                // Create new links.
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
                    if (!FindConnectingPolys(va, vb, target, DetourUtils.OppositeTile(dir), 4, out var neis, out var neiareas))
                    {
                        continue;
                    }

                    for (int k = 0; k < neis.Count(); k++)
                    {
                        int idx = DetourUtils.AllocLink(tile);
                        if (idx != DetourUtils.DT_NULL_LINK)
                        {
                            var link = new Link
                            {
                                NRef = neis.ElementAt(k),
                                Edge = j,
                                Side = dir,
                                Next = poly.FirstLink
                            };
                            poly.FirstLink = idx;

                            // Compress portal limits to an integer value.
                            if (dir == 0 || dir == 4)
                            {
                                float tmin = (neiareas.ElementAt(k).X - va.Z) / (vb.Z - va.Z);
                                float tmax = (neiareas.ElementAt(k).Y - va.Z) / (vb.Z - va.Z);
                                if (tmin > tmax) Helper.Swap(ref tmin, ref tmax);
                                link.BMin = (int)(MathUtil.Clamp(tmin, 0.0f, 1.0f) * 255.0f);
                                link.BMax = (int)(MathUtil.Clamp(tmax, 0.0f, 1.0f) * 255.0f);
                            }
                            else if (dir == 2 || dir == 6)
                            {
                                float tmin = (neiareas.ElementAt(k).X - va.X) / (vb.X - va.X);
                                float tmax = (neiareas.ElementAt(k).Y - va.X) / (vb.X - va.X);
                                if (tmin > tmax) Helper.Swap(ref tmin, ref tmax);
                                link.BMin = (int)(MathUtil.Clamp(tmin, 0.0f, 1.0f) * 255.0f);
                                link.BMax = (int)(MathUtil.Clamp(tmax, 0.0f, 1.0f) * 255.0f);
                            }
                            tile.Links[idx] = link;
                        }
                    }
                }
            }
        }
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

                var targetPoly = target.Polys[targetCon.Poly];
                // Skip off-mesh connections which start location could not be connected at all.
                if (targetPoly.FirstLink == DetourUtils.DT_NULL_LINK)
                {
                    continue;
                }

                Vector3 halfExtents = new Vector3(new float[] { targetCon.Rad, target.Header.WalkableClimb, targetCon.Rad });

                // Find polygon to connect to.
                Vector3 p = targetCon.End;
                int r = FindNearestPolyInTile(tile, p, halfExtents, out Vector3 nearestPt);
                if (r == 0)
                {
                    continue;
                }
                // findNearestPoly may return too optimistic results, further check to make sure. 
                if (Math.Sqrt(nearestPt.X - p.X) + Math.Sqrt(nearestPt.Z - p.Z) > Math.Sqrt(targetCon.Rad))
                {
                    continue;
                }
                // Make sure the location is on current mesh.
                target.SetPolyVertex(targetPoly, 1, nearestPt);

                // Link off-mesh connection to target poly.
                int idx = DetourUtils.AllocLink(target);
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
                    int tidx = DetourUtils.AllocLink(tile);
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
        }
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
                        DetourUtils.FreeLink(tile, j);
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
        private void ClosestPointOnDetailEdges(MeshTile tile, Poly poly, Vector3 pos, bool onlyBoundary, out Vector3 closest)
        {
            var pd = tile.GetDetailMesh(poly);

            float dmin = float.MaxValue;
            float tmin = 0;
            Vector3 pmin = Vector3.Zero;
            Vector3 pmax = Vector3.Zero;

            for (int i = 0; i < pd.TriCount; i++)
            {
                var tris = tile.DetailTris[pd.TriBase + i];
                int ANY_BOUNDARY_EDGE =
                    ((int)DetailTriEdgeFlagTypes.Boundary << 0) |
                    ((int)DetailTriEdgeFlagTypes.Boundary << 2) |
                    ((int)DetailTriEdgeFlagTypes.Boundary << 4);

                if (onlyBoundary && (tris.W & ANY_BOUNDARY_EDGE) == 0)
                {
                    continue;
                }

                Vector3[] v = new Vector3[3];
                for (int j = 0; j < 3; ++j)
                {
                    if (tris[j] < poly.VertCount)
                    {
                        v[j] = tile.GetPolyVertex(poly, tris[j]);
                    }
                    else
                    {
                        v[j] = tile.DetailVerts[(pd.VertBase + (tris[j] - poly.VertCount))];
                    }
                }

                for (int k = 0, j = 2; k < 3; j = k++)
                {
                    var edgeFlags = DetourUtils.GetDetailTriEdgeFlags((DetailTriEdgeFlagTypes)tris.W, j);

                    if ((edgeFlags & DetailTriEdgeFlagTypes.Boundary) == 0 &&
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
        internal bool GetPolyHeight(MeshTile tile, Poly poly, Vector3 pos, out float height)
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
        internal void ClosestPointOnPoly(int r, Vector3 pos, out Vector3 closest, out bool posOverPoly)
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
    }
}
