using SharpDX;
using System;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    public class NavMesh
    {
        public const int MAX_LAYERS = 32;
        /// <summary>
        /// This value specifies how many layers (or "floors") each navmesh tile is expected to have.
        /// </summary>
        public const int EXPECTED_LAYERS_PER_TILE = 4;

        public static NavMesh Build(InputGeometry geometry, BuildSettings settings, Agent agent)
        {
            NavMesh res = null;

            if (settings.BuildMode == BuildModesEnum.Solo)
            {
                res = BuildSolo(geometry, settings, agent);
            }
            else if (settings.BuildMode == BuildModesEnum.Tiled)
            {
                res = BuildTiled(geometry, settings, agent);
            }
            else if (settings.BuildMode == BuildModesEnum.TempObstacles)
            {
                res = BuildTempObstacles(geometry, settings, agent);
            }
            else
            {
                throw new EngineException("Bad build mode for NavigationMesh2.");
            }

            return res;
        }
        private static NavMesh BuildSolo(InputGeometry geometry, BuildSettings settings, Agent agent)
        {
            var bbox = settings.Bounds ?? geometry.BoundingBox;

            Recast.CalcGridSize(bbox, settings.CellSize, out int width, out int height);

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

            var solid = Recast.CreateHeightfield(cfg.Width, cfg.Height, cfg.BoundingBox, cfg.CellSize, cfg.CellHeight);

            var ntris = geometry.ChunkyMesh.NTris;
            var tris = geometry.ChunkyMesh.Triangles;
            var triareas = new TileCacheAreas[ntris];

            Recast.MarkWalkableTriangles(cfg.WalkableSlopeAngle, tris, triareas);
            if (!Recast.RasterizeTriangles(solid, cfg.WalkableClimb, tris, triareas))
            {
                return null;
            }

            if (settings.FilterLowHangingObstacles)
            {
                Recast.FilterLowHangingWalkableObstacles(cfg.WalkableClimb, solid);
            }
            if (settings.FilterLedgeSpans)
            {
                Recast.FilterLedgeSpans(cfg.WalkableHeight, cfg.WalkableClimb, solid);
            }
            if (settings.FilterWalkableLowHeightSpans)
            {
                Recast.FilterWalkableLowHeightSpans(cfg.WalkableHeight, solid);
            }

            if (!Recast.BuildCompactHeightfield(cfg.WalkableHeight, cfg.WalkableClimb, solid, out CompactHeightfield chf))
            {
                throw new EngineException("buildNavigation: Could not build compact height field.");
            }

            // Erode the walkable area by agent radius.
            if (!Recast.ErodeWalkableArea(cfg.WalkableRadius, chf))
            {
                throw new EngineException("buildNavigation: Could not erode.");
            }

            // (Optional) Mark areas.
            var vols = geometry.GetAreas();
            for (int i = 0; i < geometry.GetAreaCount(); ++i)
            {
                Recast.MarkConvexPolyArea(
                    vols[i].Vertices, vols[i].VertexCount,
                    vols[i].MinHeight, vols[i].MaxHeight,
                    (TileCacheAreas)vols[i].AreaType, chf);
            }

            if (settings.PartitionType == SamplePartitionTypeEnum.Watershed)
            {
                // Prepare for region partitioning, by calculating distance field along the walkable surface.
                if (!Recast.BuildDistanceField(chf))
                {
                    throw new EngineException("buildNavigation: Could not build distance field.");
                }

                // Partition the walkable surface into simple regions without holes.
                if (!Recast.BuildRegions(chf, 0, cfg.MinRegionArea, cfg.MergeRegionArea))
                {
                    throw new EngineException("buildNavigation: Could not build watershed regions.");
                }
            }
            else if (settings.PartitionType == SamplePartitionTypeEnum.Monotone)
            {
                // Partition the walkable surface into simple regions without holes.
                // Monotone partitioning does not need distancefield.
                if (!Recast.BuildRegionsMonotone(chf, 0, cfg.MinRegionArea, cfg.MergeRegionArea))
                {
                    throw new EngineException("buildNavigation: Could not build monotone regions.");
                }
            }
            else if (settings.PartitionType == SamplePartitionTypeEnum.Layers)
            {
                // Partition the walkable surface into simple regions without holes.
                var hasLayers = Recast.BuildLayerRegions(chf, 0, cfg.MinRegionArea);
                if (!hasLayers)
                {
                    throw new EngineException("buildNavigation: Could not build layer regions.");
                }
            }

            if (!Recast.BuildContours(chf, cfg.MaxSimplificationError, cfg.MaxEdgeLen, BuildContoursFlags.RC_CONTOUR_TESS_WALL_EDGES, out ContourSet cset))
            {
                throw new EngineException("buildNavigation: Could not create contours.");
            }

            if (!Recast.BuildPolyMesh(cset, 0, 0, cfg.MaxVertsPerPoly, out PolyMesh pmesh))
            {
                throw new EngineException("buildNavigation: Could not triangulate contours.");
            }

            if (!Recast.BuildPolyMeshDetail(pmesh, chf, cfg.DetailSampleDist, cfg.DetailSampleMaxError, out PolyMeshDetail dmesh))
            {
                throw new EngineException("buildNavigation: Could not build detail mesh.");
            }

            if (cfg.MaxVertsPerPoly <= Detour.DT_VERTS_PER_POLYGON)
            {
                // Update poly flags from areas.
                for (int i = 0; i < pmesh.npolys; ++i)
                {
                    if ((int)pmesh.areas[i] == (int)TileCacheAreas.RC_WALKABLE_AREA)
                    {
                        pmesh.areas[i] = SamplePolyAreas.SAMPLE_POLYAREA_GROUND;
                    }

                    if (pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_GROUND ||
                        pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_GRASS ||
                        pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_ROAD)
                    {
                        pmesh.flags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK;
                    }
                    else if (pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_WATER)
                    {
                        pmesh.flags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_SWIM;
                    }
                    else if (pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_DOOR)
                    {
                        pmesh.flags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK | SamplePolyFlagTypes.SAMPLE_POLYFLAGS_DOOR;
                    }
                }

                var param = new NavMeshCreateParams
                {
                    verts = pmesh.verts,
                    vertCount = pmesh.nverts,
                    polys = pmesh.polys,
                    polyAreas = pmesh.areas,
                    polyFlags = pmesh.flags,
                    polyCount = pmesh.npolys,
                    nvp = pmesh.nvp,
                    detailMeshes = dmesh.meshes,
                    detailVerts = dmesh.verts,
                    detailVertsCount = dmesh.nverts,
                    detailTris = dmesh.tris,
                    detailTriCount = dmesh.ntris,
                    offMeshCon = geometry.GetConnections(),
                    offMeshConCount = geometry.GetConnectionCount(),
                    walkableHeight = agent.Height,
                    walkableRadius = agent.Radius,
                    walkableClimb = agent.MaxClimb,
                    bmin = pmesh.bmin,
                    bmax = pmesh.bmax,
                    cs = cfg.CellSize,
                    ch = cfg.CellHeight,
                    buildBvTree = true
                };

                if (!Detour.CreateNavMeshData(param, out MeshData navData))
                {
                    throw new EngineException("Could not build Detour navmesh.");
                }

                var nm = new NavMesh();
                nm.Init(navData, TileFlagTypes.DT_TILE_FREE_DATA);
                return nm;
            }

            return null;
        }
        private static NavMesh BuildTiled(InputGeometry geometry, BuildSettings settings, Agent agent)
        {
            var bbox = settings.Bounds ?? geometry.BoundingBox;

            // Init cache
            Recast.CalcGridSize(bbox, settings.CellSize, out int gw, out int gh);
            int ts = (int)settings.TileSize;
            int tw = (gw + ts - 1) / ts;
            int th = (gh + ts - 1) / ts;

            int tileBits = Math.Min((int)Math.Log(Helper.NextPowerOfTwo(tw * th), 2), 14);
            if (tileBits > 14) tileBits = 14;
            int polyBits = 22 - tileBits;
            int maxTiles = 1 << tileBits;
            int maxPolysPerTile = 1 << polyBits;

            var nmparams = new NavMeshParams()
            {
                Origin = bbox.Minimum,
                TileWidth = settings.TileSize * settings.CellSize,
                TileHeight = settings.TileSize * settings.CellSize,
                MaxTiles = maxTiles,
                MaxPolys = maxPolysPerTile,
            };

            var nm = new NavMesh();
            nm.Init(nmparams);

            BuildAllTiles(geometry, settings, agent, nm);

            return nm;
        }
        private static NavMesh BuildTempObstacles(InputGeometry geometry, BuildSettings settings, Agent agent)
        {
            var bbox = settings.Bounds ?? geometry.BoundingBox;

            // Init cache
            Recast.CalcGridSize(bbox, settings.CellSize, out int gw, out int gh);
            int ts = (int)settings.TileSize;
            int tw = (gw + ts - 1) / ts;
            int th = (gh + ts - 1) / ts;

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
                MaxTiles = tw * th * EXPECTED_LAYERS_PER_TILE,
                MaxObstacles = 128,
            };
            var tmproc = new TileCacheMeshProcess(geometry);

            var tileCache = new TileCache();
            tileCache.Init(tcparams, tmproc);

            int tileBits = Math.Min((int)Math.Log(Helper.NextPowerOfTwo(tw * th * EXPECTED_LAYERS_PER_TILE), 2), 14);
            if (tileBits > 14) tileBits = 14;
            int polyBits = 22 - tileBits;
            int maxTiles = 1 << tileBits;
            int maxPolysPerTile = 1 << polyBits;

            var nmparams = new NavMeshParams()
            {
                Origin = bbox.Minimum,
                TileWidth = settings.TileSize * settings.CellSize,
                TileHeight = settings.TileSize * settings.CellSize,
                MaxTiles = maxTiles,
                MaxPolys = maxPolysPerTile,
            };

            var nm = new NavMesh()
            {
                TileCache = tileCache,
            };
            nm.Init(nmparams);

            for (int y = 0; y < th; y++)
            {
                for (int x = 0; x < tw; x++)
                {
                    int ntiles = RasterizeTileLayers(x, y, cfg, geometry, settings, out TileCacheData[] tiles);

                    for (int i = 0; i < ntiles; ++i)
                    {
                        var tile = tileCache.AddTile(tiles[i], CompressedTileFlagTypes.DT_COMPRESSEDTILE_FREE_DATA);
                        if (tile == null)
                        {
                            continue;
                        }
                    }
                }
            }

            // Build initial meshes
            for (int y = 0; y < th; y++)
            {
                for (int x = 0; x < tw; x++)
                {
                    tileCache.BuildNavMeshTilesAt(x, y, nm);
                }
            }

            return nm;
        }

        /*TEMP OBSTACLES SAMPLE*/
        public static int RasterizeTileLayers(int tx, int ty, Config cfg, InputGeometry geometry, BuildSettings settings, out TileCacheData[] tiles)
        {
            tiles = new TileCacheData[MAX_LAYERS];

            ChunkyTriMesh chunkyMesh = geometry.ChunkyMesh;

            // Tile bounds.
            float tcs = cfg.TileSize * cfg.CellSize;

            Config tcfg = cfg;

            tcfg.BoundingBox.Minimum.X = cfg.BoundingBox.Minimum.X + tx * tcs;
            tcfg.BoundingBox.Minimum.Y = cfg.BoundingBox.Minimum.Y;
            tcfg.BoundingBox.Minimum.Z = cfg.BoundingBox.Minimum.Z + ty * tcs;

            tcfg.BoundingBox.Maximum.X = cfg.BoundingBox.Minimum.X + (tx + 1) * tcs;
            tcfg.BoundingBox.Maximum.Y = cfg.BoundingBox.Maximum.Y;
            tcfg.BoundingBox.Maximum.Z = cfg.BoundingBox.Minimum.Z + (ty + 1) * tcs;

            tcfg.BoundingBox.Minimum.X -= tcfg.BorderSize * tcfg.CellSize;
            tcfg.BoundingBox.Minimum.Z -= tcfg.BorderSize * tcfg.CellSize;
            tcfg.BoundingBox.Maximum.X += tcfg.BorderSize * tcfg.CellSize;
            tcfg.BoundingBox.Maximum.Z += tcfg.BorderSize * tcfg.CellSize;

            var solid = Recast.CreateHeightfield(tcfg.Width, tcfg.Height, tcfg.BoundingBox, tcfg.CellSize, tcfg.CellHeight);

            var rc = new RasterizationContext
            {
                // Allocate voxel heightfield where we rasterize our input data to.
                solid = solid,

                // Allocate array that can hold triangle flags.
                // If you have multiple meshes you need to process, allocate
                // and array which can hold the max number of triangles you need to process.
                triareas = new TileCacheAreas[chunkyMesh.MaxTrisPerChunk],

                tiles = new TileCacheData[RasterizationContext.MaxLayers],
            };

            Vector2 tbmin = new Vector2(tcfg.BoundingBox.Minimum.X, tcfg.BoundingBox.Minimum.Z);
            Vector2 tbmax = new Vector2(tcfg.BoundingBox.Maximum.X, tcfg.BoundingBox.Maximum.Z);

            var cid = chunkyMesh.GetChunksOverlappingRect(tbmin, tbmax);
            if (!cid.Any())
            {
                return 0; // empty
            }

            foreach (var id in cid)
            {
                var tris = chunkyMesh.GetTriangles(id);

                Helper.InitializeArray(rc.triareas, TileCacheAreas.RC_NULL_AREA);

                Recast.MarkWalkableTriangles(tcfg.WalkableSlopeAngle, tris, rc.triareas);

                if (!Recast.RasterizeTriangles(rc.solid, tcfg.WalkableClimb, tris, rc.triareas))
                {
                    return 0;
                }
            }

            // Once all geometry is rasterized, we do initial pass of filtering to
            // remove unwanted overhangs caused by the conservative rasterization
            // as well as filter spans where the character cannot possibly stand.
            if (settings.FilterLowHangingObstacles)
            {
                Recast.FilterLowHangingWalkableObstacles(tcfg.WalkableClimb, rc.solid);
            }
            if (settings.FilterLedgeSpans)
            {
                Recast.FilterLedgeSpans(tcfg.WalkableHeight, tcfg.WalkableClimb, rc.solid);
            }
            if (settings.FilterWalkableLowHeightSpans)
            {
                Recast.FilterWalkableLowHeightSpans(tcfg.WalkableHeight, rc.solid);
            }

            if (!Recast.BuildCompactHeightfield(tcfg.WalkableHeight, tcfg.WalkableClimb, rc.solid, out rc.chf))
            {
                throw new EngineException("buildNavigation: Could not build compact height field.");
            }

            // Erode the walkable area by agent radius.
            if (!Recast.ErodeWalkableArea(tcfg.WalkableRadius, rc.chf))
            {
                throw new EngineException("buildNavigation: Could not erode.");
            }

            // (Optional) Mark areas.
            var vols = geometry.GetAreas();
            for (int i = 0; i < geometry.GetAreaCount(); ++i)
            {
                Recast.MarkConvexPolyArea(
                    vols[i].Vertices, vols[i].VertexCount,
                    vols[i].MinHeight, vols[i].MaxHeight,
                    (TileCacheAreas)vols[i].AreaType, rc.chf);
            }

            Recast.BuildHeightfieldLayers(rc.chf, tcfg.BorderSize, tcfg.WalkableHeight, out rc.lset);

            rc.ntiles = 0;
            for (int i = 0; i < Math.Min(rc.lset.nlayers, MAX_LAYERS); i++)
            {
                var layer = rc.lset.layers[i];

                var tile = rc.tiles[rc.ntiles];

                // Store header
                tile.Header = new TileCacheLayerHeader
                {
                    magic = DetourTileCache.DT_TILECACHE_MAGIC,
                    version = DetourTileCache.DT_TILECACHE_VERSION,

                    // Tile layer location in the navmesh.
                    tx = tx,
                    ty = ty,
                    tlayer = i,
                    b = layer.boundingBox,

                    // Tile info.
                    width = layer.width,
                    height = layer.height,
                    minx = layer.minx,
                    maxx = layer.maxx,
                    miny = layer.miny,
                    maxy = layer.maxy,
                    hmin = layer.hmin,
                    hmax = layer.hmax
                };

                // Store data
                DetourTileCache.BuildTileCacheLayer(layer.heights, layer.areas, layer.cons, ref tile.Data);

                rc.tiles[rc.ntiles++] = tile;
            }

            // Transfer ownsership of tile data from build context to the caller.
            int n = 0;
            for (int i = 0; i < Math.Min(rc.ntiles, MAX_LAYERS); i++)
            {
                tiles[n++] = rc.tiles[i];
                rc.tiles[i].Data = TileCacheLayerData.Empty;
            }

            return n;
        }
        public static int CalcLayerBufferSize(int gridWidth, int gridHeight)
        {
            return 0;
        }
        /*TILE MESH SAMPLE*/
        public static bool BuildAllTiles(InputGeometry geom, BuildSettings settings, Agent agent, NavMesh navMesh)
        {
            var bbox = geom.BoundingBox;
            Recast.CalcGridSize(bbox, settings.CellSize, out int gw, out int gh);
            int ts = (int)settings.TileSize;
            int tw = (gw + ts - 1) / ts;
            int th = (gh + ts - 1) / ts;
            float tcs = settings.TileSize * settings.CellSize;

            for (int y = 0; y < th; ++y)
            {
                for (int x = 0; x < tw; ++x)
                {
                    BoundingBox lastBuiltBbox = new BoundingBox();

                    lastBuiltBbox.Minimum.X = bbox.Minimum.X + x * tcs;
                    lastBuiltBbox.Minimum.Y = bbox.Minimum.Y;
                    lastBuiltBbox.Minimum.Z = bbox.Minimum.Z + y * tcs;

                    lastBuiltBbox.Maximum.X = bbox.Minimum.X + (x + 1) * tcs;
                    lastBuiltBbox.Maximum.Y = bbox.Maximum.Y;
                    lastBuiltBbox.Maximum.Z = bbox.Minimum.Z + (y + 1) * tcs;

                    var data = BuildTileMesh(x, y, lastBuiltBbox, geom, settings, agent);
                    if (data != null)
                    {
                        // Remove any previous data (navmesh owns and deletes the data).
                        navMesh.RemoveTile(navMesh.GetTileRefAt(x, y, 0), data, 0);
                        // Let the navmesh own the data.
                        navMesh.AddTile(data, TileFlagTypes.DT_TILE_FREE_DATA, 0, out int result);
                    }
                }
            }

            return true;
        }
        public static MeshData BuildTileMesh(int tx, int ty, BoundingBox bbox, InputGeometry geometry, BuildSettings settings, Agent agent)
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
                BoundingBox = bbox,
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
            cfg.BoundingBox.Minimum.X -= borderSize * settings.CellSize;
            cfg.BoundingBox.Minimum.Z -= borderSize * settings.CellSize;
            cfg.BoundingBox.Maximum.X += borderSize * settings.CellSize;
            cfg.BoundingBox.Maximum.Z += borderSize * settings.CellSize;

            // Allocate voxel heightfield where we rasterize our input data to.
            var solid = Recast.CreateHeightfield(cfg.Width, cfg.Height, cfg.BoundingBox, cfg.CellSize, cfg.CellHeight);

            // Allocate array that can hold triangle flags.
            // If you have multiple meshes you need to process, allocate
            // and array which can hold the max number of triangles you need to process.
            TileCacheAreas[] triareas = new TileCacheAreas[chunkyMesh.MaxTrisPerChunk];

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

                Helper.InitializeArray(triareas, TileCacheAreas.RC_NULL_AREA);

                Recast.MarkWalkableTriangles(cfg.WalkableSlopeAngle, tris, triareas);

                if (!Recast.RasterizeTriangles(solid, cfg.WalkableClimb, tris, triareas))
                {
                    return null;
                }
            }

            // Once all geometry is rasterized, we do initial pass of filtering to
            // remove unwanted overhangs caused by the conservative rasterization
            // as well as filter spans where the character cannot possibly stand.
            if (settings.FilterLowHangingObstacles)
            {
                Recast.FilterLowHangingWalkableObstacles(cfg.WalkableClimb, solid);
            }
            if (settings.FilterLedgeSpans)
            {
                Recast.FilterLedgeSpans(cfg.WalkableHeight, cfg.WalkableClimb, solid);
            }
            if (settings.FilterWalkableLowHeightSpans)
            {
                Recast.FilterWalkableLowHeightSpans(cfg.WalkableHeight, solid);
            }

            // Compact the heightfield so that it is faster to handle from now on.
            // This will result more cache coherent data as well as the neighbours
            // between walkable cells will be calculated.
            if (!Recast.BuildCompactHeightfield(cfg.WalkableHeight, cfg.WalkableClimb, solid, out CompactHeightfield chf))
            {
                return null;
            }

            // Erode the walkable area by agent radius.
            if (!Recast.ErodeWalkableArea(cfg.WalkableRadius, chf))
            {
                return null;
            }

            // (Optional) Mark areas.
            var vols = geometry.GetAreas();
            for (int i = 0; i < geometry.GetAreaCount(); ++i)
            {
                Recast.MarkConvexPolyArea(
                    vols[i].Vertices, vols[i].VertexCount,
                    vols[i].MinHeight, vols[i].MaxHeight,
                    (TileCacheAreas)vols[i].AreaType, chf);
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

            if (settings.PartitionType == SamplePartitionTypeEnum.Watershed)
            {
                // Prepare for region partitioning, by calculating distance field along the walkable surface.
                if (!Recast.BuildDistanceField(chf))
                {
                    return null;
                }

                // Partition the walkable surface into simple regions without holes.
                if (!Recast.BuildRegions(chf, cfg.BorderSize, cfg.MinRegionArea, cfg.MergeRegionArea))
                {
                    return null;
                }
            }
            else if (settings.PartitionType == SamplePartitionTypeEnum.Monotone)
            {
                // Partition the walkable surface into simple regions without holes.
                // Monotone partitioning does not need distancefield.
                if (!Recast.BuildRegionsMonotone(chf, cfg.BorderSize, cfg.MinRegionArea, cfg.MergeRegionArea))
                {
                    return null;
                }
            }
            else if (settings.PartitionType == SamplePartitionTypeEnum.Layers)
            {
                // Partition the walkable surface into simple regions without holes.
                var hasLayerRegions = Recast.BuildLayerRegions(chf, cfg.BorderSize, cfg.MinRegionArea);
                if (!hasLayerRegions)
                {
                    return null;
                }
            }

            // Create contours.
            if (!Recast.BuildContours(chf, cfg.MaxSimplificationError, cfg.MaxEdgeLen, BuildContoursFlags.RC_CONTOUR_TESS_WALL_EDGES, out ContourSet cset))
            {
                return null;
            }

            if (cset.nconts == 0)
            {
                return null;
            }

            // Build polygon navmesh from the contours.
            if (!Recast.BuildPolyMesh(cset, tx, ty, cfg.MaxVertsPerPoly, out PolyMesh pmesh))
            {
                return null;
            }

            // Build detail mesh.
            if (!Recast.BuildPolyMeshDetail(pmesh, chf, cfg.DetailSampleDist, cfg.DetailSampleMaxError, out PolyMeshDetail dmesh))
            {
                return null;
            }

            if (cfg.MaxVertsPerPoly <= Detour.DT_VERTS_PER_POLYGON)
            {
                if (pmesh.nverts >= 0xffff)
                {
                    // The vertex indices are ushorts, and cannot point to more than 0xffff vertices.
                    return null;
                }

                // Update poly flags from areas.
                for (int i = 0; i < pmesh.npolys; ++i)
                {
                    if ((int)pmesh.areas[i] == (int)TileCacheAreas.RC_WALKABLE_AREA)
                    {
                        pmesh.areas[i] = SamplePolyAreas.SAMPLE_POLYAREA_GROUND;
                    }

                    if (pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_GROUND ||
                        pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_GRASS ||
                        pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_ROAD)
                    {
                        pmesh.flags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK;
                    }
                    else if (pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_WATER)
                    {
                        pmesh.flags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_SWIM;
                    }
                    else if (pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_DOOR)
                    {
                        pmesh.flags[i] = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK | SamplePolyFlagTypes.SAMPLE_POLYFLAGS_DOOR;
                    }
                }

                var param = new NavMeshCreateParams
                {
                    verts = pmesh.verts,
                    vertCount = pmesh.nverts,
                    polys = pmesh.polys,
                    polyAreas = pmesh.areas,
                    polyFlags = pmesh.flags,
                    polyCount = pmesh.npolys,
                    nvp = pmesh.nvp,
                    detailMeshes = dmesh.meshes,
                    detailVerts = dmesh.verts,
                    detailVertsCount = dmesh.nverts,
                    detailTris = dmesh.tris,
                    detailTriCount = dmesh.ntris,
                    offMeshCon = geometry.GetConnections(),
                    offMeshConCount = geometry.GetConnectionCount(),
                    walkableHeight = agent.Height,
                    walkableRadius = agent.Radius,
                    walkableClimb = agent.MaxClimb,
                    tileX = tx,
                    tileY = ty,
                    tileLayer = 0,
                    bmin = pmesh.bmin,
                    bmax = pmesh.bmax,
                    cs = cfg.CellSize,
                    ch = cfg.CellHeight,
                    buildBvTree = true
                };

                if (!Detour.CreateNavMeshData(param, out MeshData navData))
                {
                    return null;
                }

                return navData;
            }

            return null;
        }

        private Vector3 m_orig;
        private float m_tileWidth;
        private float m_tileHeight;
        private int m_tileLutMask;
        private MeshTile[] m_posLookup;
        private MeshTile m_nextFree = null;
        private int m_tileBits;
        private int m_polyBits;
        private int m_saltBits;

        private NavMeshParams m_params;
        public TileCache TileCache { get; set; }
        public int MaxTiles { get; set; }
        public MeshTile[] Tiles { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public NavMesh()
        {

        }

        public void Init(NavMeshParams nmparams)
        {
            m_params = nmparams;
            m_orig = nmparams.Origin;
            m_tileWidth = nmparams.TileWidth;
            m_tileHeight = nmparams.TileHeight;

            // Init tiles
            MaxTiles = nmparams.MaxTiles;
            var m_tileLutSize = Helper.NextPowerOfTwo(nmparams.MaxTiles / 4);
            if (m_tileLutSize == 0) m_tileLutSize = 1;
            m_tileLutMask = m_tileLutSize - 1;

            Tiles = new MeshTile[MaxTiles];
            m_posLookup = new MeshTile[m_tileLutSize];

            m_nextFree = null;
            for (int i = MaxTiles - 1; i >= 0; --i)
            {
                Tiles[i] = new MeshTile
                {
                    salt = 1,
                    next = m_nextFree
                };
                m_nextFree = Tiles[i];
            }

            // Init ID generator values.
            m_tileBits = (int)Math.Log(Helper.NextPowerOfTwo(nmparams.MaxTiles), 2);
            m_polyBits = (int)Math.Log(Helper.NextPowerOfTwo(nmparams.MaxPolys), 2);
            // Only allow 31 salt bits, since the salt mask is calculated using 32bit uint and it will overflow.
            m_saltBits = Math.Min(31, 32 - m_tileBits - m_polyBits);

            if (m_saltBits < 10)
            {
                throw new EngineException("DT_INVALID_PARAM");
            }
        }
        public bool Init(MeshData data, TileFlagTypes flags)
        {
            // Make sure the data is in right format.
            MeshHeader header = data.header;
            if (header.magic != Detour.DT_NAVMESH_MAGIC)
            {
                return false;
            }
            if (header.version != Detour.DT_NAVMESH_VERSION)
            {
                return false;
            }

            NavMeshParams param = new NavMeshParams
            {
                Origin = header.bmin,
                TileWidth = header.bmax[0] - header.bmin[0],
                TileHeight = header.bmax[2] - header.bmin[2],
                MaxTiles = 1,
                MaxPolys = header.polyCount
            };

            Init(param);

            return AddTile(data, flags, 0, out int result);
        }
        public NavMeshParams GetParams()
        {
            return m_params;
        }
        public bool AddTile(MeshData data, TileFlagTypes flags, int lastRef, out int result)
        {
            result = -1;

            // Make sure the data is in right format.
            MeshHeader header = data.header;
            if (header.magic != Detour.DT_NAVMESH_MAGIC)
            {
                return false;
            }
            if (header.version != Detour.DT_NAVMESH_VERSION)
            {
                return false;
            }

            // Make sure the location is free.
            if (GetTileAt(header.x, header.y, header.layer) != null)
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
                    m_nextFree = tile.next;
                    tile.next = null;
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
                    tile = tile.next;
                }

                // Could not find the correct location.
                if (tile == target)
                {
                    // Remove from freelist
                    if (prev == null)
                    {
                        m_nextFree = tile?.next;
                    }
                    else
                    {
                        prev.next = tile?.next;
                    }

                    // Restore salt.
                    if (tile != null)
                    {
                        tile.salt = DecodePolyIdSalt(lastRef);
                    }
                }
            }

            // Make sure we could allocate a tile.
            if (tile == null)
            {
                return false;
            }

            // Insert tile into the position lut.
            int h = Detour.ComputeTileHash(header.x, header.y, m_tileLutMask);
            tile.next = m_posLookup[h];
            m_posLookup[h] = tile;

            tile.Patch(header);

            // If there are no items in the bvtree, reset the tree pointer.
            if (data.navBvtree == null)
            {
                tile.bvTree = null;
            }

            // Build links freelist
            tile.linksFreeList = 0;
            tile.links[header.maxLinkCount - 1].next = Detour.DT_NULL_LINK;
            for (int i = 0; i < header.maxLinkCount - 1; ++i)
            {
                tile.links[i].next = i + 1;
            }

            // Init tile.
            tile.header = header;
            tile.SetData(data);
            tile.flags = flags;

            ConnectIntLinks(tile);

            // Base off-mesh connections to their starting polygons and connect connections inside the tile.
            BaseOffMeshLinks(tile);
            ConnectExtOffMeshLinks(tile, tile, -1);

            // Create connections with neighbour tiles.
            int MAX_NEIS = 32;
            MeshTile[] neis = new MeshTile[MAX_NEIS];
            int nneis;

            // Connect with layers in current tile.
            nneis = GetTilesAt(header.x, header.y, neis, MAX_NEIS);
            for (int j = 0; j < nneis; ++j)
            {
                if (neis[j] == tile)
                {
                    continue;
                }

                ConnectExtLinks(tile, neis[j], -1);
                ConnectExtLinks(neis[j], tile, -1);
                ConnectExtOffMeshLinks(tile, neis[j], -1);
                ConnectExtOffMeshLinks(neis[j], tile, -1);
            }

            // Connect with neighbour tiles.
            for (int i = 0; i < 8; ++i)
            {
                nneis = GetNeighbourTilesAt(header.x, header.y, i, neis, MAX_NEIS);
                for (int j = 0; j < nneis; ++j)
                {
                    ConnectExtLinks(tile, neis[j], i);
                    ConnectExtLinks(neis[j], tile, Detour.OppositeTile(i));
                    ConnectExtOffMeshLinks(tile, neis[j], i);
                    ConnectExtOffMeshLinks(neis[j], tile, Detour.OppositeTile(i));
                }
            }

            result = GetTileRef(tile);

            return true;
        }
        public bool RemoveTile(MeshTile tile, MeshData data, int dataSize)
        {
            if (tile == null)
            {
                return false;
            }

            // Remove tile from hash lookup.
            int h = Detour.ComputeTileHash(tile.header.x, tile.header.y, m_tileLutMask);
            MeshTile prev = null;
            MeshTile cur = m_posLookup[h];
            while (cur != null)
            {
                if (cur == tile)
                {
                    if (prev != null)
                    {
                        prev.next = cur.next;
                    }
                    else
                    {
                        m_posLookup[h] = cur.next;
                    }
                    break;
                }
                prev = cur;
                cur = cur.next;
            }

            // Remove connections to neighbour tiles.
            int MAX_NEIS = 32;
            MeshTile[] neis = new MeshTile[MAX_NEIS];
            int nneis;

            // Disconnect from other layers in current tile.
            nneis = GetTilesAt(tile.header.x, tile.header.y, neis, MAX_NEIS);
            for (int j = 0; j < nneis; ++j)
            {
                if (neis[j] == tile) continue;
                UnconnectLinks(neis[j], tile);
            }

            // Disconnect from neighbour tiles.
            for (int i = 0; i < 8; ++i)
            {
                nneis = GetNeighbourTilesAt(tile.header.x, tile.header.y, i, neis, MAX_NEIS);
                for (int j = 0; j < nneis; ++j)
                    UnconnectLinks(neis[j], tile);
            }

            // Reset tile.
            if ((tile.flags & TileFlagTypes.DT_TILE_FREE_DATA) != 0)
            {
                // Owns data
                tile.data = null;
                tile.dataSize = 0;
            }

            tile.header = new MeshHeader();
            tile.flags = 0;
            tile.linksFreeList = 0;
            tile.polys = null;
            tile.verts = null;
            tile.links = null;
            tile.detailMeshes = null;
            tile.detailVerts = null;
            tile.detailTris = null;
            tile.bvTree = null;
            tile.offMeshCons = null;

            // Update salt, salt should never be zero.
            tile.salt = (tile.salt + 1) & ((1 << m_saltBits) - 1);
            if (tile.salt == 0)
                tile.salt++;

            // Add to free list.
            tile.next = m_nextFree;
            m_nextFree = tile;

            return true;
        }
        public void CalcTileLoc(Vector3 pos, out int tx, out int ty)
        {
            tx = (int)Math.Floor((pos.X - m_orig.X) / m_tileWidth);
            ty = (int)Math.Floor((pos.Z - m_orig.Z) / m_tileHeight);
        }
        public MeshTile GetTileAt(int x, int y, int layer)
        {
            // Find tile based on hash.
            int h = Detour.ComputeTileHash(x, y, m_tileLutMask);
            MeshTile tile = m_posLookup[h];
            while (tile != null)
            {
                if (tile.header.x == x &&
                    tile.header.y == y &&
                    tile.header.layer == layer)
                {
                    return tile;
                }
                tile = tile.next;
            }
            return null;
        }
        public int GetTilesAt(int x, int y, MeshTile[] tiles, int maxTiles)
        {
            int n = 0;

            // Find tile based on hash.
            int h = Detour.ComputeTileHash(x, y, m_tileLutMask);
            var tile = m_posLookup[h];
            while (tile != null)
            {
                if (tile.header.x == x && tile.header.y == y && n < maxTiles)
                {
                    tiles[n++] = tile;
                }
                tile = tile.next;
            }

            return n;
        }
        public MeshTile GetTileRefAt(int x, int y, int layer)
        {
            // Find tile based on hash.
            int h = Detour.ComputeTileHash(x, y, m_tileLutMask);
            var tile = m_posLookup[h];
            while (tile != null)
            {
                if (tile.header.x == x &&
                    tile.header.y == y &&
                    tile.header.layer == layer)
                {
                    return tile;
                }
                tile = tile.next;
            }
            return null;
        }
        public int GetTileRef(MeshTile tile)
        {
            if (tile == null) return 0;
            int it = Array.IndexOf(Tiles, tile);
            return EncodePolyId(tile.salt, it, 0);
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
            if (tile.salt != tileSalt)
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
        public bool GetTileAndPolyByRef(int r, out MeshTile tile, out Poly poly)
        {
            tile = null;
            poly = null;

            if (r == 0) return false;
            DecodePolyId(r, out int salt, out int it, out int ip);
            if (it >= MaxTiles) return false;
            if (Tiles[it].salt != salt || Tiles[it].header.magic != Detour.DT_NAVMESH_MAGIC) return false;
            if (ip >= Tiles[it].header.polyCount) return false;
            tile = Tiles[it];
            poly = Tiles[it].polys[ip];
            return true;
        }
        public void GetTileAndPolyByRefUnsafe(int r, out MeshTile tile, out Poly poly)
        {
            DecodePolyId(r, out int salt, out int it, out int ip);
            tile = Tiles[it];
            poly = Tiles[it].polys[ip];
        }
        public bool IsValidPolyRef(int r)
        {
            if (r == 0) return false;

            DecodePolyId(r, out int salt, out int it, out int ip);

            if (it >= MaxTiles) return false;
            if (Tiles[it].salt != salt || Tiles[it].header.magic != Detour.DT_NAVMESH_MAGIC) return false;
            if (ip >= Tiles[it].header.polyCount) return false;

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
            if (Tiles[it].salt != salt || Tiles[it].header.magic != Detour.DT_NAVMESH_MAGIC)
            {
                return false;
            }
            var tile = Tiles[it];
            if (ip >= tile.header.polyCount)
            {
                return false;
            }
            var poly = tile.polys[ip];

            // Make sure that the current poly is indeed off-mesh link.
            if (poly.Type != PolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                return false;
            }

            // Figure out which way to hand out the vertices.
            int idx0 = 0, idx1 = 1;

            // Find link that points to first vertex.
            for (int i = poly.FirstLink; i != Detour.DT_NULL_LINK; i = tile.links[i].next)
            {
                if (tile.links[i].edge == 0)
                {
                    if (tile.links[i].nref != prevRef)
                    {
                        idx0 = 1;
                        idx1 = 0;
                    }
                    break;
                }
            }

            startPos = tile.verts[poly.Verts[idx0]];
            endPos = tile.verts[poly.Verts[idx1]];

            return true;
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
            if (Tiles[it].salt != salt || Tiles[it].header.magic != Detour.DT_NAVMESH_MAGIC)
            {
                return null;
            }
            var tile = Tiles[it];
            if (ip >= tile.header.polyCount)
            {
                return null;
            }
            var poly = tile.polys[ip];

            // Make sure that the current poly is indeed off-mesh link.
            if (poly.Type != PolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                return null;
            }

            int idx = ip - tile.header.offMeshBase;

            return tile.offMeshCons[idx];
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
            if (Tiles[it].salt != salt || Tiles[it].header.magic != Detour.DT_NAVMESH_MAGIC)
            {
                return false;
            }

            var tile = Tiles[it];
            if (ip >= tile.header.polyCount)
            {
                return false;
            }

            var poly = tile.polys[ip];

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
            if (Tiles[it].salt != salt || Tiles[it].header.magic != Detour.DT_NAVMESH_MAGIC)
            {
                return false;
            }

            var tile = Tiles[it];
            if (ip >= tile.header.polyCount)
            {
                return false;
            }

            var poly = tile.polys[ip];

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
            if (Tiles[it].salt != salt || Tiles[it].header.magic != Detour.DT_NAVMESH_MAGIC)
            {
                return false;
            }

            var tile = Tiles[it];
            if (ip >= tile.header.polyCount)
            {
                return false;
            }

            var poly = tile.polys[ip];

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
            if (Tiles[it].salt != salt || Tiles[it].header.magic != Detour.DT_NAVMESH_MAGIC)
            {
                return false;
            }

            var tile = Tiles[it];
            if (ip >= tile.header.polyCount)
            {
                return false;
            }

            var poly = tile.polys[ip];

            resultArea = poly.Area;

            return true;
        }
        //getTileStateSize
        //storeTileState
        //restoreTileState
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

        private int GetNeighbourTilesAt(int x, int y, int side, MeshTile[] tiles, int maxTiles)
        {
            int nx = x, ny = y;
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

            return GetTilesAt(nx, ny, tiles, maxTiles);
        }
        private int FindConnectingPolys(Vector3 va, Vector3 vb, MeshTile tile, int side, out int[] con, out float[] conarea, int maxcon)
        {
            con = new int[maxcon];
            conarea = new float[maxcon * 2];

            if (tile == null) return 0;

            Detour.CalcSlabEndPoints(va, vb, out Vector2 amin, out Vector2 amax, side);
            float apos = Detour.GetSlabCoord(va, side);

            // Remove links pointing to 'side' and compact the links array. 
            int m = Detour.DT_EXT_LINK | side;
            int n = 0;

            int bse = GetTileRef(tile);

            for (int i = 0; i < tile.header.polyCount; ++i)
            {
                Poly poly = tile.polys[i];
                int nv = poly.VertCount;
                for (int j = 0; j < nv; ++j)
                {
                    // Skip edges which do not point to the right side.
                    if (poly.Neis[j] != m) continue;

                    Vector3 vc = tile.verts[poly.Verts[j]];
                    Vector3 vd = tile.verts[poly.Verts[(j + 1) % nv]];
                    float bpos = Detour.GetSlabCoord(vc, side);

                    // Segments are not close enough.
                    if (Math.Abs(apos - bpos) > 0.01f)
                        continue;

                    // Check if the segments touch.
                    Detour.CalcSlabEndPoints(vc, vd, out Vector2 bmin, out Vector2 bmax, side);

                    if (!Detour.OverlapSlabs(amin, amax, bmin, bmax, 0.01f, tile.header.walkableClimb)) continue;

                    // Add return value.
                    if (n < maxcon)
                    {
                        conarea[n * 2 + 0] = Math.Max(amin.X, bmin.X);
                        conarea[n * 2 + 1] = Math.Min(amax.X, bmax.X);
                        con[n] = bse | i;
                        n++;
                    }
                    break;
                }
            }
            return n;
        }
        private void ConnectIntLinks(MeshTile tile)
        {
            if (tile == null) return;

            int bse = GetTileRef(tile);

            for (int i = 0; i < tile.header.polyCount; ++i)
            {
                var poly = tile.polys[i];
                poly.FirstLink = Detour.DT_NULL_LINK;

                if (poly.Type == PolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
                {
                    continue;
                }

                // Build edge links backwards so that the links will be
                // in the linked list from lowest index to highest.
                for (int j = poly.VertCount - 1; j >= 0; --j)
                {
                    // Skip hard and non-internal edges.
                    if (poly.Neis[j] == 0 || (poly.Neis[j] & Detour.DT_EXT_LINK) != 0) continue;

                    int idx = Detour.AllocLink(tile);
                    if (idx != Detour.DT_NULL_LINK)
                    {
                        var link = new Link
                        {
                            nref = (bse | (poly.Neis[j] - 1)),
                            edge = j,
                            side = 0xff,
                            bmin = 0,
                            bmax = 0,
                            // Add to linked list.
                            next = poly.FirstLink,
                        };
                        poly.FirstLink = idx;
                        tile.links[idx] = link;
                    }
                }
            }
        }
        private void BaseOffMeshLinks(MeshTile tile)
        {
            if (tile == null) return;

            int bse = GetTileRef(tile);

            // Base off-mesh connection start points.
            for (int i = 0; i < tile.header.offMeshConCount; ++i)
            {
                var con = tile.offMeshCons[i];
                var poly = tile.polys[con.poly];

                Vector3 halfExtents = new Vector3(new float[] { con.rad, tile.header.walkableClimb, con.rad });

                // Find polygon to connect to.
                Vector3 p = con.start; // First vertex
                Vector3 nearestPt = new Vector3();
                int r = FindNearestPolyInTile(tile, p, halfExtents, nearestPt);
                if (r == 0) continue;
                // findNearestPoly may return too optimistic results, further check to make sure. 
                if (Math.Sqrt(nearestPt[0] - p[0]) + Math.Sqrt(nearestPt[2] - p[2]) > Math.Sqrt(con.rad))
                {
                    continue;
                }
                // Make sure the location is on current mesh.
                tile.verts[poly.Verts[0]] = nearestPt;

                // Link off-mesh connection to target poly.
                int idx = Detour.AllocLink(tile);
                if (idx != Detour.DT_NULL_LINK)
                {
                    var link = new Link
                    {
                        nref = r,
                        edge = 0,
                        side = 0xff,
                        bmin = 0,
                        bmax = 0,
                        // Add to linked list.
                        next = poly.FirstLink
                    };
                    tile.links[idx] = link;
                    poly.FirstLink = idx;
                }

                // Start end-point is always connect back to off-mesh connection. 
                int tidx = Detour.AllocLink(tile);
                if (tidx != Detour.DT_NULL_LINK)
                {
                    var landPolyIdx = DecodePolyIdPoly(r);
                    var landPoly = tile.polys[landPolyIdx];
                    var link = new Link
                    {
                        nref = (bse | (con.poly)),
                        edge = 0xff,
                        side = 0xff,
                        bmin = 0,
                        bmax = 0,
                        // Add to linked list.
                        next = landPoly.FirstLink
                    };
                    tile.links[tidx] = link;
                    landPoly.FirstLink = tidx;
                }
            }
        }
        private void ConnectExtLinks(MeshTile tile, MeshTile target, int side)
        {
            if (tile == null) return;

            // Connect border links.
            for (int i = 0; i < tile.header.polyCount; ++i)
            {
                var poly = tile.polys[i];

                // Create new links.
                int nv = poly.VertCount;
                for (int j = 0; j < nv; ++j)
                {
                    // Skip non-portal edges.
                    if ((poly.Neis[j] & Detour.DT_EXT_LINK) == 0)
                    {
                        continue;
                    }

                    int dir = (poly.Neis[j] & 0xff);
                    if (side != -1 && dir != side)
                    {
                        continue;
                    }

                    // Create new links
                    var va = tile.verts[poly.Verts[j]];
                    var vb = tile.verts[poly.Verts[(j + 1) % nv]];
                    int nnei = FindConnectingPolys(va, vb, target, Detour.OppositeTile(dir), out int[] nei, out float[] neia, 4);
                    for (int k = 0; k < nnei; ++k)
                    {
                        int idx = Detour.AllocLink(tile);
                        if (idx != Detour.DT_NULL_LINK)
                        {
                            var link = new Link
                            {
                                nref = nei[k],
                                edge = j,
                                side = dir,
                                next = poly.FirstLink
                            };
                            poly.FirstLink = idx;

                            // Compress portal limits to an integer value.
                            if (dir == 0 || dir == 4)
                            {
                                float tmin = (neia[k * 2 + 0] - va[2]) / (vb[2] - va[2]);
                                float tmax = (neia[k * 2 + 1] - va[2]) / (vb[2] - va[2]);
                                if (tmin > tmax) Helper.Swap(ref tmin, ref tmax);
                                link.bmin = (int)(MathUtil.Clamp(tmin, 0.0f, 1.0f) * 255.0f);
                                link.bmax = (int)(MathUtil.Clamp(tmax, 0.0f, 1.0f) * 255.0f);
                            }
                            else if (dir == 2 || dir == 6)
                            {
                                float tmin = (neia[k * 2 + 0] - va[0]) / (vb[0] - va[0]);
                                float tmax = (neia[k * 2 + 1] - va[0]) / (vb[0] - va[0]);
                                if (tmin > tmax) Helper.Swap(ref tmin, ref tmax);
                                link.bmin = (int)(MathUtil.Clamp(tmin, 0.0f, 1.0f) * 255.0f);
                                link.bmax = (int)(MathUtil.Clamp(tmax, 0.0f, 1.0f) * 255.0f);
                            }
                            tile.links[idx] = link;
                        }
                    }
                }
            }
        }
        private void ConnectExtOffMeshLinks(MeshTile tile, MeshTile target, int side)
        {
            if (tile == null) return;

            // Connect off-mesh links.
            // We are interested on links which land from target tile to this tile.
            int oppositeSide = (side == -1) ? 0xff : Detour.OppositeTile(side);

            for (int i = 0; i < target.header.offMeshConCount; ++i)
            {
                var targetCon = target.offMeshCons[i];
                if (targetCon.side != oppositeSide)
                {
                    continue;
                }

                var targetPoly = target.polys[targetCon.poly];
                // Skip off-mesh connections which start location could not be connected at all.
                if (targetPoly.FirstLink == Detour.DT_NULL_LINK)
                {
                    continue;
                }

                Vector3 halfExtents = new Vector3(new float[] { targetCon.rad, target.header.walkableClimb, targetCon.rad });

                // Find polygon to connect to.
                Vector3 p = targetCon.end;
                Vector3 nearestPt = new Vector3();
                int r = FindNearestPolyInTile(tile, p, halfExtents, nearestPt);
                if (r == 0)
                {
                    continue;
                }
                // findNearestPoly may return too optimistic results, further check to make sure. 
                if (Math.Sqrt(nearestPt[0] - p[0]) + Math.Sqrt(nearestPt[2] - p[2]) > Math.Sqrt(targetCon.rad))
                {
                    continue;
                }
                // Make sure the location is on current mesh.
                target.verts[targetPoly.Verts[1]] = nearestPt;

                // Link off-mesh connection to target poly.
                int idx = Detour.AllocLink(target);
                if (idx != Detour.DT_NULL_LINK)
                {
                    var link = new Link
                    {
                        nref = r,
                        edge = 1,
                        side = oppositeSide,
                        bmin = 0,
                        bmax = 0,
                        // Add to linked list.
                        next = targetPoly.FirstLink
                    };
                    target.links[idx] = link;
                    targetPoly.FirstLink = idx;
                }

                // Link target poly to off-mesh connection.
                if ((targetCon.flags & Detour.DT_OFFMESH_CON_BIDIR) != 0)
                {
                    int tidx = Detour.AllocLink(tile);
                    if (tidx != Detour.DT_NULL_LINK)
                    {
                        var landPolyIdx = DecodePolyIdPoly(r);
                        var landPoly = tile.polys[landPolyIdx];
                        var link = new Link
                        {
                            nref = (GetTileRef(target) | (targetCon.poly)),
                            edge = 0xff,
                            side = (side == -1 ? 0xff : side),
                            bmin = 0,
                            bmax = 0,
                            // Add to linked list.
                            next = landPoly.FirstLink
                        };
                        tile.links[tidx] = link;
                        landPoly.FirstLink = tidx;
                    }
                }
            }
        }
        private void UnconnectLinks(MeshTile tile, MeshTile target)
        {
            if (tile == null || target == null) return;

            int targetNum = DecodePolyIdTile(GetTileRef(target));

            for (int i = 0; i < tile.header.polyCount; ++i)
            {
                var poly = tile.polys[i];
                int j = poly.FirstLink;
                int pj = Detour.DT_NULL_LINK;
                while (j != Detour.DT_NULL_LINK)
                {
                    if (DecodePolyIdTile(tile.links[j].nref) == targetNum)
                    {
                        // Remove link.
                        int nj = tile.links[j].next;
                        if (pj == Detour.DT_NULL_LINK)
                        {
                            poly.FirstLink = nj;
                        }
                        else
                        {
                            tile.links[pj].next = nj;
                        }
                        Detour.FreeLink(tile, j);
                        j = nj;
                    }
                    else
                    {
                        // Advance
                        pj = j;
                        j = tile.links[j].next;
                    }
                }
            }
        }
        private int QueryPolygonsInTile(MeshTile tile, Vector3 qmin, Vector3 qmax, int[] polys, int maxPolys)
        {
            if (tile.bvTree != null)
            {
                int nodeIndex = 0;
                int endIndex = tile.header.bvNodeCount;
                Vector3 tbmin = tile.header.bmin;
                Vector3 tbmax = tile.header.bmax;
                float qfac = tile.header.bvQuantFactor;

                // Calculate quantized box
                Int3 bmin = new Int3();
                Int3 bmax = new Int3();
                // dtClamp query box to world box.
                float minx = MathUtil.Clamp(qmin.X, tbmin.X, tbmax.X) - tbmin.X;
                float miny = MathUtil.Clamp(qmin.Y, tbmin.Y, tbmax.Y) - tbmin.Y;
                float minz = MathUtil.Clamp(qmin.Z, tbmin.Z, tbmax.Z) - tbmin.Z;
                float maxx = MathUtil.Clamp(qmax.X, tbmin.X, tbmax.X) - tbmin.X;
                float maxy = MathUtil.Clamp(qmax.Y, tbmin.Y, tbmax.Y) - tbmin.Y;
                float maxz = MathUtil.Clamp(qmax.Z, tbmin.Z, tbmax.Z) - tbmin.Z;
                // Quantize
                bmin.X = (int)(qfac * minx) & 0xfffe;
                bmin.Y = (int)(qfac * miny) & 0xfffe;
                bmin.Z = (int)(qfac * minz) & 0xfffe;
                bmax.X = (int)(qfac * maxx + 1) | 1;
                bmax.Y = (int)(qfac * maxy + 1) | 1;
                bmax.Z = (int)(qfac * maxz + 1) | 1;

                // Traverse tree
                int bse = GetTileRef(tile);
                int n = 0;
                while (nodeIndex < endIndex)
                {
                    var node = nodeIndex < tile.bvTree.Length ? tile.bvTree[nodeIndex] : new BVNode();

                    bool overlap = Detour.OverlapQuantBounds(bmin, bmax, node.bmin, node.bmax);
                    bool isLeafNode = node.i >= 0;

                    if (isLeafNode && overlap && n < maxPolys)
                    {
                        polys[n++] = bse | node.i;
                    }

                    if (overlap || isLeafNode)
                    {
                        nodeIndex++;
                    }
                    else
                    {
                        int escapeIndex = -node.i;
                        nodeIndex += escapeIndex;
                    }
                }

                return n;
            }
            else
            {
                Vector3 bmin;
                Vector3 bmax;
                int n = 0;
                int bse = GetTileRef(tile);
                for (int i = 0; i < tile.header.polyCount; ++i)
                {
                    var p = tile.polys[i];
                    // Do not return off-mesh connection polygons.
                    if (p.Type == PolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
                    {
                        continue;
                    }
                    // Calc polygon bounds.
                    Vector3 v = tile.verts[p.Verts[0]];
                    bmin = v;
                    bmax = v;
                    for (int j = 1; j < p.VertCount; ++j)
                    {
                        v = tile.verts[p.Verts[j]];
                        bmin = Vector3.Min(bmin, v);
                        bmax = Vector3.Max(bmax, v);
                    }

                    if (Recast.OverlapBounds(qmin, qmax, bmin, bmax) && n < maxPolys)
                    {
                        polys[n++] = bse | i;
                    }
                }

                return n;
            }
        }
        private int FindNearestPolyInTile(MeshTile tile, Vector3 center, Vector3 halfExtents, Vector3 nearestPt)
        {
            Vector3 bmin = Vector3.Subtract(center, halfExtents);
            Vector3 bmax = Vector3.Add(center, halfExtents);

            // Get nearby polygons from proximity grid.
            int[] polys = new int[128];
            int polyCount = QueryPolygonsInTile(tile, bmin, bmax, polys, 128);

            // Find nearest polygon amongst the nearby polygons.
            int nearest = 0;
            float nearestDistanceSqr = float.MaxValue;
            for (int i = 0; i < polyCount; ++i)
            {
                int r = polys[i];
                float d;
                ClosestPointOnPoly(r, center, out Vector3 closestPtPoly, out bool posOverPoly);

                // If a point is directly over a polygon and closer than
                // climb height, favor that instead of straight line nearest point.
                Vector3 diff = Vector3.Subtract(center, closestPtPoly);
                if (posOverPoly)
                {
                    d = Math.Abs(diff[1]) - tile.header.walkableClimb;
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
        private void ClosestPointOnPoly(int r, Vector3 pos, out Vector3 closest, out bool posOverPoly)
        {
            GetTileAndPolyByRefUnsafe(r, out MeshTile tile, out Poly poly);

            // Off-mesh connections don't have detail polygons.
            if (poly.Type == PolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                Vector3 v0 = tile.verts[poly.Verts[0]];
                Vector3 v1 = tile.verts[poly.Verts[1]];
                float d0 = Vector3.Distance(pos, v0);
                float d1 = Vector3.Distance(pos, v1);
                float u = d0 / (d0 + d1);
                closest = Vector3.Lerp(v0, v1, u);
                posOverPoly = false;
                return;
            }

            int ip = Array.IndexOf(tile.polys, poly);
            var pd = tile.detailMeshes[ip];

            // Clamp point to be inside the polygon.
            Vector3[] verts = new Vector3[Detour.DT_VERTS_PER_POLYGON];
            float[] edged = new float[Detour.DT_VERTS_PER_POLYGON];
            float[] edget = new float[Detour.DT_VERTS_PER_POLYGON];
            int nv = poly.VertCount;
            for (int i = 0; i < nv; ++i)
            {
                verts[i] = tile.verts[poly.Verts[i]];
            }

            closest = pos;
            if (!Detour.DistancePtPolyEdgesSqr(pos, verts, nv, out edged, out edget))
            {
                // Point is outside the polygon, dtClamp to nearest edge.
                float dmin = edged[0];
                int imin = 0;
                for (int i = 1; i < nv; ++i)
                {
                    if (edged[i] < dmin)
                    {
                        dmin = edged[i];
                        imin = i;
                    }
                }
                var va = verts[imin];
                var vb = verts[((imin + 1) % nv)];
                closest = Vector3.Lerp(va, vb, edget[imin]);

                posOverPoly = false;
            }
            else
            {
                posOverPoly = true;
            }

            // Find height at the location.
            for (int j = 0; j < pd.triCount; ++j)
            {
                var t = tile.detailTris[(pd.triBase + j)];
                var v = new Vector3[3];
                for (int k = 0; k < 3; ++k)
                {
                    if (t[k] < poly.VertCount)
                    {
                        v[k] = tile.verts[poly.Verts[t[k]]];
                    }
                    else
                    {
                        v[k] = tile.detailVerts[(pd.vertBase + (t[k] - poly.VertCount))];
                    }
                }
                if (Detour.ClosestHeightPointTriangle(closest, v[0], v[1], v[2], out float h))
                {
                    closest.Y = h;
                    break;
                }
            }
        }

        /*TILE MESH SAMPLE*/
        public void BuildTile(Vector3 pos, InputGeometry geom, BuildSettings settings, Agent agent)
        {
            var bbox = geom.BoundingBox;

            float ts = settings.TileSize * settings.CellSize;
            int tx = (int)((pos.X - bbox.Minimum.X) / ts);
            int ty = (int)((pos.Z - bbox.Minimum.Z) / ts);

            BoundingBox lastBuiltBbox = new BoundingBox();

            lastBuiltBbox.Minimum.X = bbox.Minimum.X + tx * ts;
            lastBuiltBbox.Minimum.Y = bbox.Minimum.Y;
            lastBuiltBbox.Minimum.Z = bbox.Minimum.Z + ty * ts;

            lastBuiltBbox.Maximum.X = bbox.Minimum.X + (tx + 1) * ts;
            lastBuiltBbox.Maximum.Y = bbox.Maximum.Y;
            lastBuiltBbox.Maximum.Z = bbox.Minimum.Z + (ty + 1) * ts;

            var data = BuildTileMesh(tx, ty, lastBuiltBbox, geom, settings, agent);

            // Remove any previous data (navmesh owns and deletes the data).
            RemoveTile(GetTileRefAt(tx, ty, 0), data, 0);

            // Add tile, or leave the location empty.
            if (data != null && !AddTile(data, TileFlagTypes.DT_TILE_FREE_DATA, 0, out int res))
            {
                // Let the navmesh own the data.
                data = null;
            }
        }
        public void GetTilePos(Vector3 pos, InputGeometry geom, BuildSettings settings, out int tx, out int ty)
        {
            var bbox = geom.BoundingBox;

            float ts = settings.TileSize * settings.CellSize;
            tx = (int)((pos.X - bbox.Minimum.X) / ts);
            ty = (int)((pos.Z - bbox.Minimum.Z) / ts);
        }
        public void RemoveTile(Vector3 pos, InputGeometry geom, BuildSettings settings)
        {
            var bbox = geom.BoundingBox;

            float ts = settings.TileSize * settings.CellSize;
            int tx = (int)((pos.X - bbox.Minimum.X) / ts);
            int ty = (int)((pos.Z - bbox.Minimum.Z) / ts);

            BoundingBox lastBuiltBbox = new BoundingBox();

            lastBuiltBbox.Minimum.X = bbox.Minimum.X + tx * ts;
            lastBuiltBbox.Minimum.Y = bbox.Minimum.Y;
            lastBuiltBbox.Minimum.Z = bbox.Minimum.Z + ty * ts;

            lastBuiltBbox.Maximum.X = bbox.Minimum.X + (tx + 1) * ts;
            lastBuiltBbox.Maximum.Y = bbox.Maximum.Y;
            lastBuiltBbox.Maximum.Z = bbox.Minimum.Z + (ty + 1) * ts;

            RemoveTile(GetTileRefAt(tx, ty, 0), null, 0);
        }
    }
}
