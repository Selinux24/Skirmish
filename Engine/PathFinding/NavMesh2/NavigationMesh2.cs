using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine.PathFinding.NavMesh2
{
    public class NavigationMesh2 : IGraph
    {
        public static NavigationMesh2 Build(Triangle[] triangles, BuildSettings settings)
        {
            return Build(new InputGeometry(triangles), settings);
        }
        public static NavigationMesh2 Build(InputGeometry geometry, BuildSettings settings)
        {
            if (settings.BuildMode == BuildModesEnum.Solo)
            {
                return BuildSolo(geometry, settings);
            }
            else if (settings.BuildMode == BuildModesEnum.Tiled)
            {
                return BuildTiled(geometry, settings);
            }
            else if (settings.BuildMode == BuildModesEnum.TempObstacles)
            {
                return BuildTempObstacles(geometry, settings);
            }
            else
            {
                throw new EngineException("Bad build mode for NavigationMesh2.");
            }
        }
        private static NavigationMesh2 BuildTempObstacles(InputGeometry geometry, BuildSettings settings)
        {
            var agent = settings.Agents[0];

            var bbox = settings.NavmeshBounds ?? geometry.BoundingBox;

            // Init cache
            CalcGridSize(bbox, settings.CellSize, out int gw, out int gh);
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
                MaxTiles = tw * th * Constants.ExpectedLayersPerTile,
                MaxObstacles = 128,
            };
            var tmproc = new TileCacheMeshProcess(geometry);

            var tileCache = new TileCache();
            tileCache.Init(tcparams, tmproc);

            int tileBits = Math.Min((int)Math.Log(Helper.NextPowerOfTwo(tw * th * Constants.ExpectedLayersPerTile), 2), 14);
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

            var nm = new NavigationMesh2();
            nm.Init(nmparams);

            var nmQuery = new NavMeshQuery();
            nmQuery.Init(nm, settings.MaxNodes);

            int m_cacheLayerCount = 0;
            int m_cacheCompressedSize = 0;
            int m_cacheRawSize = 0;
            int layerBufferSize = CalcLayerBufferSize(tcparams.Width, tcparams.Height);

            for (int y = 0; y < th; y++)
            {
                for (int x = 0; x < tw; x++)
                {
                    int ntiles = RasterizeTileLayers(x, y, settings, cfg, geometry, out TileCacheData[] tiles);

                    for (int i = 0; i < ntiles; ++i)
                    {
                        tileCache.AddTile(tiles[i], TileFlags.FreeData);

                        m_cacheLayerCount++;
                        m_cacheCompressedSize += 0;//tiles[i].DataSize;
                        m_cacheRawSize += layerBufferSize;
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
        private static NavigationMesh2 BuildSolo(InputGeometry geometry, BuildSettings settings)
        {
            var agent = settings.Agents[0];

            var bbox = settings.NavmeshBounds ?? geometry.BoundingBox;

            CalcGridSize(bbox, settings.CellSize, out int width, out int height);

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

            var solid = new Heightfield
            {
                width = cfg.Width,
                height = cfg.Height,
                boundingBox = cfg.BoundingBox,
                cs = cfg.CellSize,
                ch = cfg.CellHeight,
                spans = new Span[cfg.Width * cfg.Height],
            };

            var ntris = geometry.GetChunkyMesh().ntris;
            var tris = geometry.GetChunkyMesh().triangles;
            var triareas = new TileCacheAreas[ntris];

            MarkWalkableTriangles(cfg.WalkableSlopeAngle, tris, triareas);
            if (!RasterizeTriangles(solid, cfg.WalkableClimb, tris, triareas))
            {
                return null;
            }

            if (settings.FilterLowHangingObstacles)
            {
                FilterLowHangingWalkableObstacles(cfg.WalkableClimb, solid);
            }
            if (settings.FilterLedgeSpans)
            {
                FilterLedgeSpans(cfg.WalkableHeight, cfg.WalkableClimb, solid);
            }
            if (settings.FilterWalkableLowHeightSpans)
            {
                FilterWalkableLowHeightSpans(cfg.WalkableHeight, solid);
            }

            if (!BuildCompactHeightfield(cfg.WalkableHeight, cfg.WalkableClimb, solid, out CompactHeightfield chf))
            {
                throw new EngineException("buildNavigation: Could not build compact height field.");
            }

            // Erode the walkable area by agent radius.
            if (!ErodeWalkableArea(cfg.WalkableRadius, chf))
            {
                throw new EngineException("buildNavigation: Could not erode.");
            }

            // (Optional) Mark areas.
            var vols = geometry.GetConvexVolumes();
            for (int i = 0; i < geometry.GetConvexVolumeCount(); ++i)
            {
                MarkConvexPolyArea(
                    vols[i].verts, vols[i].nverts,
                    vols[i].hmin, vols[i].hmax,
                    vols[i].area, chf);
            }

            if (settings.PartitionType == SamplePartitionTypeEnum.Watershed)
            {
                // Prepare for region partitioning, by calculating distance field along the walkable surface.
                if (!BuildDistanceField(chf))
                {
                    throw new EngineException("buildNavigation: Could not build distance field.");
                }

                // Partition the walkable surface into simple regions without holes.
                if (!BuildRegions(chf, 0, cfg.MinRegionArea, cfg.MergeRegionArea))
                {
                    throw new EngineException("buildNavigation: Could not build watershed regions.");
                }
            }
            else if (settings.PartitionType == SamplePartitionTypeEnum.Monotone)
            {
                // Partition the walkable surface into simple regions without holes.
                // Monotone partitioning does not need distancefield.
                if (!BuildRegionsMonotone(chf, 0, cfg.MinRegionArea, cfg.MergeRegionArea))
                {
                    throw new EngineException("buildNavigation: Could not build monotone regions.");
                }
            }
            else if (settings.PartitionType == SamplePartitionTypeEnum.Layers)
            {
                // Partition the walkable surface into simple regions without holes.
                if (!BuildLayerRegions(chf, 0, cfg.MinRegionArea))
                {
                    throw new EngineException("buildNavigation: Could not build layer regions.");
                }
            }

            if (!BuildContours(chf, cfg.MaxSimplificationError, cfg.MaxEdgeLen, BuildContoursFlags.RC_CONTOUR_TESS_WALL_EDGES, out ContourSet cset))
            {
                throw new EngineException("buildNavigation: Could not create contours.");
            }

            if (!BuildPolyMesh(cset, cfg.MaxVertsPerPoly, out PolyMesh pmesh))
            {
                throw new EngineException("buildNavigation: Could not triangulate contours.");
            }

            if (!BuildPolyMeshDetail(pmesh, chf, cfg.DetailSampleDist, cfg.DetailSampleMaxError, out PolyMeshDetail dmesh))
            {
                throw new EngineException("buildNavigation: Could not build detail mesh.");
            }

            if (cfg.MaxVertsPerPoly <= Constants.VertsPerPolygon)
            {
                // Update poly flags from areas.
                for (int i = 0; i < pmesh.npolys; ++i)
                {
                    if ((int)pmesh.areas[i] == (int)TileCacheAreas.WalkableArea)
                    {
                        pmesh.areas[i] = SamplePolyAreas.SAMPLE_POLYAREA_GROUND;
                    }

                    if (pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_GROUND ||
                        pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_GRASS ||
                        pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_ROAD)
                    {
                        pmesh.flags[i] = SamplePolyFlags.SAMPLE_POLYFLAGS_WALK;
                    }
                    else if (pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_WATER)
                    {
                        pmesh.flags[i] = SamplePolyFlags.SAMPLE_POLYFLAGS_SWIM;
                    }
                    else if (pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_DOOR)
                    {
                        pmesh.flags[i] = SamplePolyFlags.SAMPLE_POLYFLAGS_WALK | SamplePolyFlags.SAMPLE_POLYFLAGS_DOOR;
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
                    offMeshConVerts = geometry.GetOffMeshConnectionVerts(),
                    offMeshConRad = geometry.GetOffMeshConnectionRads(),
                    offMeshConDir = geometry.GetOffMeshConnectionDirs(),
                    offMeshConAreas = geometry.GetOffMeshConnectionAreas(),
                    offMeshConFlags = geometry.GetOffMeshConnectionFlags(),
                    offMeshConUserID = geometry.GetOffMeshConnectionId(),
                    offMeshConCount = geometry.GetOffMeshConnectionCount(),
                    walkableHeight = agent.Height,
                    walkableRadius = agent.Radius,
                    walkableClimb = agent.MaxClimb,
                    bmin = pmesh.bmin,
                    bmax = pmesh.bmax,
                    cs = cfg.CellSize,
                    ch = cfg.CellHeight,
                    buildBvTree = true
                };

                if (!CreateNavMeshData(param, out MeshData navData))
                {
                    throw new EngineException("Could not build Detour navmesh.");
                }

                var nm = new NavigationMesh2();
                nm.Init(navData, TileFlags.FreeData);

                var mmQuery = new NavMeshQuery();
                mmQuery.Init(nm, settings.MaxNodes);
                return nm;
            }

            return null;
        }
        private static NavigationMesh2 BuildTiled(InputGeometry geometry, BuildSettings settings)
        {
            var agent = settings.Agents[0];

            var bbox = settings.NavmeshBounds ?? geometry.BoundingBox;

            // Init cache
            CalcGridSize(bbox, settings.CellSize, out int gw, out int gh);
            int ts = (int)settings.TileSize;
            int tw = (gw + ts - 1) / ts;
            int th = (gh + ts - 1) / ts;

            int tileBits = Math.Min((int)Math.Log(Helper.NextPowerOfTwo(tw * th * Constants.ExpectedLayersPerTile), 2), 14);
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

            var nm = new NavigationMesh2();
            nm.Init(nmparams);

            var nmQuery = new NavMeshQuery();
            nmQuery.Init(nm, settings.MaxNodes);

            BuildAllTiles(geometry, settings, agent, nm);

            return nm;
        }

        private static bool BuildDistanceField(CompactHeightfield chf)
        {
            if (chf.dist != null)
            {
                chf.dist = null;
            }

            int[] src = new int[chf.spanCount];
            {
                CalculateDistanceField(chf, src, out chf.maxDistance);
            }

            int[] dst = new int[chf.spanCount];
            {
                // Blur
                if (BoxBlur(chf, 1, src, dst) != src)
                {
                    Helper.Swap(ref src, ref dst);
                }

                // Store distance.
                chf.dist = src;
            }

            dst = null;

            return true;
        }
        private static void CalculateDistanceField(CompactHeightfield chf, int[] src, out int maxDist)
        {
            int w = chf.width;
            int h = chf.height;

            // Init distance and points.
            for (int i = 0; i < chf.spanCount; ++i)
            {
                src[i] = 0xffff;
            }

            // Mark boundary cells.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.cells[x + y * w];
                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        var s = chf.spans[i];
                        var area = chf.areas[i];

                        int nc = 0;
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (GetCon(s, dir) != Constants.NotConnected)
                            {
                                int ax = x + PolyUtils.GetDirOffsetX(dir);
                                int ay = y + PolyUtils.GetDirOffsetY(dir);
                                int ai = chf.cells[ax + ay * w].index + GetCon(s, dir);
                                if (area == chf.areas[ai])
                                {
                                    nc++;
                                }
                            }
                        }
                        if (nc != 4)
                        {
                            src[i] = 0;
                        }
                    }
                }
            }

            // Pass 1
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.cells[x + y * w];
                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        var s = chf.spans[i];

                        if (GetCon(s, 0) != Constants.NotConnected)
                        {
                            // (-1,0)
                            int ax = x + PolyUtils.GetDirOffsetX(0);
                            int ay = y + PolyUtils.GetDirOffsetY(0);
                            int ai = chf.cells[ax + ay * w].index + GetCon(s, 0);
                            var a = chf.spans[ai];
                            if (src[ai] + 2 < src[i])
                            {
                                src[i] = src[ai] + 2;
                            }

                            // (-1,-1)
                            if (GetCon(a, 3) != Constants.NotConnected)
                            {
                                int aax = ax + PolyUtils.GetDirOffsetX(3);
                                int aay = ay + PolyUtils.GetDirOffsetY(3);
                                int aai = chf.cells[aax + aay * w].index + GetCon(a, 3);
                                if (src[aai] + 3 < src[i])
                                {
                                    src[i] = src[aai] + 3;
                                }
                            }
                        }
                        if (GetCon(s, 3) != Constants.NotConnected)
                        {
                            // (0,-1)
                            int ax = x + PolyUtils.GetDirOffsetX(3);
                            int ay = y + PolyUtils.GetDirOffsetY(3);
                            int ai = chf.cells[ax + ay * w].index + GetCon(s, 3);
                            var a = chf.spans[ai];
                            if (src[ai] + 2 < src[i])
                            {
                                src[i] = src[ai] + 2;
                            }

                            // (1,-1)
                            if (GetCon(a, 2) != Constants.NotConnected)
                            {
                                int aax = ax + PolyUtils.GetDirOffsetX(2);
                                int aay = ay + PolyUtils.GetDirOffsetY(2);
                                int aai = chf.cells[aax + aay * w].index + GetCon(a, 2);
                                if (src[aai] + 3 < src[i])
                                {
                                    src[i] = src[aai] + 3;
                                }
                            }
                        }
                    }
                }
            }

            // Pass 2
            for (int y = h - 1; y >= 0; --y)
            {
                for (int x = w - 1; x >= 0; --x)
                {
                    var c = chf.cells[x + y * w];
                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        var s = chf.spans[i];

                        if (GetCon(s, 2) != Constants.NotConnected)
                        {
                            // (1,0)
                            int ax = x + PolyUtils.GetDirOffsetX(2);
                            int ay = y + PolyUtils.GetDirOffsetY(2);
                            int ai = chf.cells[ax + ay * w].index + GetCon(s, 2);
                            var a = chf.spans[ai];
                            if (src[ai] + 2 < src[i])
                            {
                                src[i] = src[ai] + 2;
                            }

                            // (1,1)
                            if (GetCon(a, 1) != Constants.NotConnected)
                            {
                                int aax = ax + PolyUtils.GetDirOffsetX(1);
                                int aay = ay + PolyUtils.GetDirOffsetY(1);
                                int aai = chf.cells[aax + aay * w].index + GetCon(a, 1);
                                if (src[aai] + 3 < src[i])
                                {
                                    src[i] = src[aai] + 3;
                                }
                            }
                        }
                        if (GetCon(s, 1) != Constants.NotConnected)
                        {
                            // (0,1)
                            int ax = x + PolyUtils.GetDirOffsetX(1);
                            int ay = y + PolyUtils.GetDirOffsetY(1);
                            int ai = chf.cells[ax + ay * w].index + GetCon(s, 1);
                            var a = chf.spans[ai];
                            if (src[ai] + 2 < src[i])
                            {
                                src[i] = src[ai] + 2;
                            }

                            // (-1,1)
                            if (GetCon(a, 0) != Constants.NotConnected)
                            {
                                int aax = ax + PolyUtils.GetDirOffsetX(0);
                                int aay = ay + PolyUtils.GetDirOffsetY(0);
                                int aai = chf.cells[aax + aay * w].index + GetCon(a, 0);
                                if (src[aai] + 3 < src[i])
                                {
                                    src[i] = src[aai] + 3;
                                }
                            }
                        }
                    }
                }
            }

            maxDist = 0;
            for (int i = 0; i < chf.spanCount; ++i)
            {
                maxDist = Math.Max(src[i], maxDist);
            }
        }
        private static int[] BoxBlur(CompactHeightfield chf, int thr, int[] src, int[] dst)
        {
            int w = chf.width;
            int h = chf.height;

            thr *= 2;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.cells[x + y * w];
                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        var s = chf.spans[i];
                        var cd = src[i];
                        if (cd <= thr)
                        {
                            dst[i] = cd;
                            continue;
                        }

                        int d = cd;
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (GetCon(s, dir) != Constants.NotConnected)
                            {
                                int ax = x + PolyUtils.GetDirOffsetX(dir);
                                int ay = y + PolyUtils.GetDirOffsetY(dir);
                                int ai = chf.cells[ax + ay * w].index + GetCon(s, dir);
                                d += src[ai];

                                var a = chf.spans[ai];
                                int dir2 = (dir + 1) & 0x3;
                                if (GetCon(a, dir2) != Constants.NotConnected)
                                {
                                    int ax2 = ax + PolyUtils.GetDirOffsetX(dir2);
                                    int ay2 = ay + PolyUtils.GetDirOffsetY(dir2);
                                    int ai2 = chf.cells[ax2 + ay2 * w].index + GetCon(a, dir2);
                                    d += src[ai2];
                                }
                                else
                                {
                                    d += cd;
                                }
                            }
                            else
                            {
                                d += cd * 2;
                            }
                        }
                        dst[i] = ((d + 5) / 9);
                    }
                }
            }

            return dst;
        }
        private static bool BuildRegions(CompactHeightfield chf, int borderSize, int minRegionArea, int mergeRegionArea)
        {
            int w = chf.width;
            int h = chf.height;

            int LOG_NB_STACKS = 3;
            int NB_STACKS = 1 << LOG_NB_STACKS;
            List<List<int>> lvlStacks = new List<List<int>>();
            for (int i = 0; i < NB_STACKS; ++i)
            {
                lvlStacks.Add(new List<int>());
            }

            List<int> stack = new List<int>();
            List<int> visited = new List<int>();

            int[] srcReg = new int[chf.spanCount];
            int[] srcDist = new int[chf.spanCount];
            int[] dstReg = new int[chf.spanCount];
            int[] dstDist = new int[chf.spanCount];

            int regionId = 1;
            int level = (chf.maxDistance + 1) & ~1;

            // TODO: Figure better formula, expandIters defines how much the 
            // watershed "overflows" and simplifies the regions. Tying it to
            // agent radius was usually good indication how greedy it could be.
            //	const int expandIters = 4 + walkableRadius * 2;
            const int expandIters = 8;

            if (borderSize > 0)
            {
                // Make sure border will not overflow.
                int bw = Math.Min(w, borderSize);
                int bh = Math.Min(h, borderSize);

                // Paint regions
                PaintRectRegion(0, bw, 0, h, (regionId | Constants.RC_BORDER_REG), chf, srcReg); regionId++;
                PaintRectRegion(w - bw, w, 0, h, (regionId | Constants.RC_BORDER_REG), chf, srcReg); regionId++;
                PaintRectRegion(0, w, 0, bh, (regionId | Constants.RC_BORDER_REG), chf, srcReg); regionId++;
                PaintRectRegion(0, w, h - bh, h, (regionId | Constants.RC_BORDER_REG), chf, srcReg); regionId++;

                chf.borderSize = borderSize;
            }

            int sId = -1;
            while (level > 0)
            {
                level = level >= 2 ? level - 2 : 0;
                sId = (sId + 1) & (NB_STACKS - 1);

                if (sId == 0)
                {
                    SortCellsByLevel(level, chf, srcReg, NB_STACKS, lvlStacks, 1);
                }
                else
                {
                    AppendStacks(lvlStacks[sId - 1], lvlStacks[sId], srcReg); // copy left overs from last level
                }

                {
                    // Expand current regions until no empty connected cells found.
                    if (ExpandRegions(expandIters, level, chf, srcReg, srcDist, dstReg, dstDist, lvlStacks[sId], false) != srcReg)
                    {
                        Helper.Swap(ref srcReg, ref dstReg);
                        Helper.Swap(ref srcDist, ref dstDist);
                    }
                }

                {
                    // Mark new regions with IDs.
                    for (int j = 0; j < lvlStacks[sId].Count; j += 3)
                    {
                        int x = lvlStacks[sId][j];
                        int y = lvlStacks[sId][j + 1];
                        int i = lvlStacks[sId][j + 2];
                        if (i >= 0 && srcReg[i] == 0)
                        {
                            if (FloodRegion(x, y, i, level, regionId, chf, srcReg, srcDist, stack))
                            {
                                if (regionId == 0xFFFF)
                                {
                                    throw new EngineException("rcBuildRegions: Region ID overflow");
                                }

                                regionId++;
                            }
                        }
                    }
                }
            }

            // Expand current regions until no empty connected cells found.
            if (ExpandRegions(expandIters * 8, 0, chf, srcReg, srcDist, dstReg, dstDist, stack, true) != srcReg)
            {
                Helper.Swap(ref srcReg, ref dstReg);
                Helper.Swap(ref srcDist, ref dstDist);
            }

            {
                // Merge regions and filter out smalle regions.
                chf.maxRegions = regionId;
                if (!MergeAndFilterRegions(minRegionArea, mergeRegionArea, ref chf.maxRegions, chf, srcReg, out int[] overlaps))
                {
                    return false;
                }

                // If overlapping regions were found during merging, split those regions.
                if (overlaps.Length > 0)
                {
                    throw new EngineException(string.Format("rcBuildRegions: {0} overlapping regions", overlaps.Length));
                }
            }

            // Write the result out.
            for (int i = 0; i < chf.spanCount; ++i)
            {
                chf.spans[i].reg = srcReg[i];
            }

            return true;
        }
        private static void PaintRectRegion(int minx, int maxx, int miny, int maxy, int regId, CompactHeightfield chf, int[] srcReg)
        {
            int w = chf.width;
            for (int y = miny; y < maxy; ++y)
            {
                for (int x = minx; x < maxx; ++x)
                {
                    var c = chf.cells[x + y * w];
                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        if (chf.areas[i] != TileCacheAreas.NullArea)
                        {
                            srcReg[i] = regId;
                        }
                    }
                }
            }
        }
        private static void SortCellsByLevel(int startLevel, CompactHeightfield chf, int[] srcReg, int nbStacks, List<List<int>> stacks, int loglevelsPerStack) // the levels per stack (2 in our case) as a bit shift
        {
            int w = chf.width;
            int h = chf.height;
            startLevel = startLevel >> loglevelsPerStack;

            for (int j = 0; j < nbStacks; ++j)
            {
                stacks[j].Clear();
            }

            // put all cells in the level range into the appropriate stacks
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.cells[x + y * w];
                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        if (chf.areas[i] == TileCacheAreas.NullArea || srcReg[i] != 0)
                        {
                            continue;
                        }

                        int level = chf.dist[i] >> loglevelsPerStack;
                        int sId = startLevel - level;
                        if (sId >= nbStacks)
                        {
                            continue;
                        }
                        if (sId < 0)
                        {
                            sId = 0;
                        }

                        stacks[sId].Add(x);
                        stacks[sId].Add(y);
                        stacks[sId].Add(i);
                    }
                }
            }
        }
        private static void AppendStacks(List<int> srcStack, List<int> dstStack, int[] srcReg)
        {
            for (int j = 0; j < srcStack.Count; j += 3)
            {
                int i = srcStack[j + 2];
                if ((i < 0) || (srcReg[i] != 0))
                {
                    continue;
                }
                dstStack.Add(srcStack[j]);
                dstStack.Add(srcStack[j + 1]);
                dstStack.Add(srcStack[j + 2]);
            }
        }
        private static int[] ExpandRegions(int maxIter, int level, CompactHeightfield chf, int[] srcReg, int[] srcDist, int[] dstReg, int[] dstDist, List<int> stack, bool fillStack)
        {
            int w = chf.width;
            int h = chf.height;

            if (fillStack)
            {
                // Find cells revealed by the raised level.
                stack.Clear();
                for (int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        var c = chf.cells[x + y * w];
                        for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                        {
                            if (chf.dist[i] >= level && srcReg[i] == 0 && chf.areas[i] != TileCacheAreas.NullArea)
                            {
                                stack.Add(x);
                                stack.Add(y);
                                stack.Add(i);
                            }
                        }
                    }
                }
            }
            else // use cells in the input stack
            {
                // mark all cells which already have a region
                for (int j = 0; j < stack.Count; j += 3)
                {
                    int i = stack[j + 2];
                    if (srcReg[i] != 0)
                    {
                        stack[j + 2] = -1;
                    }
                }
            }

            int iter = 0;
            while (stack.Count > 0)
            {
                int failed = 0;

                Array.Copy(srcReg, dstReg, chf.spanCount);
                Array.Copy(srcDist, dstDist, chf.spanCount);

                for (int j = 0; j < stack.Count; j += 3)
                {
                    int x = stack[j + 0];
                    int y = stack[j + 1];
                    int i = stack[j + 2];
                    if (i < 0)
                    {
                        failed++;
                        continue;
                    }

                    int r = srcReg[i];
                    int d2 = int.MaxValue;
                    var area = chf.areas[i];
                    var s = chf.spans[i];
                    for (int dir = 0; dir < 4; ++dir)
                    {
                        if (GetCon(s, dir) == Constants.NotConnected) continue;
                        int ax = x + PolyUtils.GetDirOffsetX(dir);
                        int ay = y + PolyUtils.GetDirOffsetY(dir);
                        int ai = chf.cells[ax + ay * w].index + GetCon(s, dir);
                        if (chf.areas[ai] != area) continue;
                        if (srcReg[ai] > 0 && (srcReg[ai] & Constants.RC_BORDER_REG) == 0)
                        {
                            if (srcDist[ai] + 2 < d2)
                            {
                                r = srcReg[ai];
                                d2 = srcDist[ai] + 2;
                            }
                        }
                    }
                    if (r != 0)
                    {
                        stack[j + 2] = -1; // mark as used
                        dstReg[i] = r;
                        dstDist[i] = d2;
                    }
                    else
                    {
                        failed++;
                    }
                }

                // rcSwap source and dest.
                Helper.Swap(ref srcReg, ref dstReg);
                Helper.Swap(ref srcDist, ref dstDist);

                if (failed * 3 == stack.Count)
                {
                    break;
                }

                if (level > 0)
                {
                    ++iter;
                    if (iter >= maxIter)
                    {
                        break;
                    }
                }
            }

            return srcReg;
        }
        private static bool FloodRegion(int x, int y, int i, int level, int r, CompactHeightfield chf, int[] srcReg, int[] srcDist, List<int> stack)
        {
            int w = chf.width;

            var area = chf.areas[i];

            // Flood fill mark region.
            stack.Clear();
            stack.Add(x);
            stack.Add(y);
            stack.Add(i);
            srcReg[i] = r;
            srcDist[i] = 0;

            int lev = level >= 2 ? level - 2 : 0;
            int count = 0;

            while (stack.Count > 0)
            {
                int ci = stack.Pop();
                int cy = stack.Pop();
                int cx = stack.Pop();

                var cs = chf.spans[ci];

                // Check if any of the neighbours already have a valid region set.
                int ar = 0;
                for (int dir = 0; dir < 4; ++dir)
                {
                    // 8 connected
                    if (GetCon(cs, dir) != Constants.NotConnected)
                    {
                        int ax = cx + PolyUtils.GetDirOffsetX(dir);
                        int ay = cy + PolyUtils.GetDirOffsetY(dir);
                        int ai = chf.cells[ax + ay * w].index + GetCon(cs, dir);
                        if (chf.areas[ai] != area)
                        {
                            continue;
                        }
                        int nr = srcReg[ai];
                        if ((nr & Constants.RC_BORDER_REG) != 0) // Do not take borders into account.
                        {
                            continue;
                        }
                        if (nr != 0 && nr != r)
                        {
                            ar = nr;
                            break;
                        }

                        var a = chf.spans[ai];

                        int dir2 = (dir + 1) & 0x3;
                        if (GetCon(a, dir2) != Constants.NotConnected)
                        {
                            int ax2 = ax + PolyUtils.GetDirOffsetX(dir2);
                            int ay2 = ay + PolyUtils.GetDirOffsetY(dir2);
                            int ai2 = chf.cells[ax2 + ay2 * w].index + GetCon(a, dir2);
                            if (chf.areas[ai2] != area)
                            {
                                continue;
                            }
                            int nr2 = srcReg[ai2];
                            if (nr2 != 0 && nr2 != r)
                            {
                                ar = nr2;
                                break;
                            }
                        }
                    }
                }
                if (ar != 0)
                {
                    srcReg[ci] = 0;
                    continue;
                }

                count++;

                // Expand neighbours.
                for (int dir = 0; dir < 4; ++dir)
                {
                    if (GetCon(cs, dir) != Constants.NotConnected)
                    {
                        int ax = cx + PolyUtils.GetDirOffsetX(dir);
                        int ay = cy + PolyUtils.GetDirOffsetY(dir);
                        int ai = chf.cells[ax + ay * w].index + GetCon(cs, dir);
                        if (chf.areas[ai] != area)
                        {
                            continue;
                        }
                        if (chf.dist[ai] >= lev && srcReg[ai] == 0)
                        {
                            srcReg[ai] = r;
                            srcDist[ai] = 0;
                            stack.Add(ax);
                            stack.Add(ay);
                            stack.Add(ai);
                        }
                    }
                }
            }

            return count > 0;
        }
        private static bool MergeAndFilterRegions(int minRegionArea, int mergeRegionSize, ref int maxRegionId, CompactHeightfield chf, int[] srcReg, out int[] overlaps)
        {
            int w = chf.width;
            int h = chf.height;

            int nreg = maxRegionId + 1;
            Region[] regions = new Region[nreg];

            // Construct regions
            for (int i = 0; i < nreg; ++i)
            {
                regions[i] = new Region(i);
            }

            // Find edge of a region and find connections around the contour.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.cells[x + y * w];
                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        int r = srcReg[i];
                        if (r == 0 || r >= nreg)
                        {
                            continue;
                        }

                        var reg = regions[r];
                        reg.spanCount++;

                        // Update floors.
                        for (int j = (int)c.index; j < ni; ++j)
                        {
                            if (i == j) continue;
                            int floorId = srcReg[j];
                            if (floorId == 0 || floorId >= nreg)
                            {
                                continue;
                            }
                            if (floorId == r)
                            {
                                reg.overlap = true;
                            }
                            AddUniqueFloorRegion(reg, floorId);
                        }

                        // Have found contour
                        if (reg.connections.Count > 0)
                            continue;

                        reg.areaType = chf.areas[i];

                        // Check if this cell is next to a border.
                        int ndir = -1;
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (IsSolidEdge(chf, srcReg, x, y, i, dir))
                            {
                                ndir = dir;
                                break;
                            }
                        }

                        if (ndir != -1)
                        {
                            // The cell is at border.
                            // Walk around the contour to find all the neighbours.
                            WalkContour(x, y, i, ndir, chf, srcReg, reg.connections);
                        }
                    }
                }
            }

            // Remove too small regions.
            List<int> stack = new List<int>();
            List<int> trace = new List<int>();
            for (int i = 0; i < nreg; ++i)
            {
                var reg = regions[i];
                if (reg.id == 0 || (reg.id & Constants.RC_BORDER_REG) != 0)
                {
                    continue;
                }
                if (reg.spanCount == 0)
                {
                    continue;
                }
                if (reg.visited)
                {
                    continue;
                }

                // Count the total size of all the connected regions.
                // Also keep track of the regions connects to a tile border.
                bool connectsToBorder = false;
                int spanCount = 0;
                stack.Clear();
                trace.Clear();

                reg.visited = true;
                stack.Add(i);

                while (stack.Count > 0)
                {
                    // Pop
                    int ri = stack.Pop();

                    var creg = regions[ri];

                    spanCount += creg.spanCount;
                    trace.Add(ri);

                    for (int j = 0; j < creg.connections.Count; ++j)
                    {
                        if ((creg.connections[j] & Constants.RC_BORDER_REG) != 0)
                        {
                            connectsToBorder = true;
                            continue;
                        }
                        var neireg = regions[creg.connections[j]];
                        if (neireg.visited)
                        {
                            continue;
                        }
                        if (neireg.id == 0 || (neireg.id & Constants.RC_BORDER_REG) != 0)
                        {
                            continue;
                        }
                        // Visit
                        stack.Add(neireg.id);
                        neireg.visited = true;
                    }
                }

                // If the accumulated regions size is too small, remove it.
                // Do not remove areas which connect to tile borders
                // as their size cannot be estimated correctly and removing them
                // can potentially remove necessary areas.
                if (spanCount < minRegionArea && !connectsToBorder)
                {
                    // Kill all visited regions.
                    for (int j = 0; j < trace.Count; ++j)
                    {
                        regions[trace[j]].spanCount = 0;
                        regions[trace[j]].id = 0;
                    }
                }
            }

            // Merge too small regions to neighbour regions.
            int mergeCount = 0;
            do
            {
                mergeCount = 0;
                for (int i = 0; i < nreg; ++i)
                {
                    var reg = regions[i];
                    if (reg.id == 0 || (reg.id & Constants.RC_BORDER_REG) != 0)
                    {
                        continue;
                    }
                    if (reg.overlap)
                    {
                        continue;
                    }
                    if (reg.spanCount == 0)
                    {
                        continue;
                    }

                    // Check to see if the region should be merged.
                    if (reg.spanCount > mergeRegionSize && IsRegionConnectedToBorder(reg))
                    {
                        continue;
                    }

                    // Small region with more than 1 connection.
                    // Or region which is not connected to a border at all.
                    // Find smallest neighbour region that connects to this one.
                    int smallest = int.MaxValue;
                    int mergeId = reg.id;
                    for (int j = 0; j < reg.connections.Count; ++j)
                    {
                        if ((reg.connections[j] & Constants.RC_BORDER_REG) != 0)
                        {
                            continue;
                        }

                        var mreg = regions[reg.connections[j]];
                        if (mreg.id == 0 || (mreg.id & Constants.RC_BORDER_REG) != 0 || mreg.overlap)
                        {
                            continue;
                        }

                        if (mreg.spanCount < smallest &&
                            CanMergeWithRegion(reg, mreg) &&
                            CanMergeWithRegion(mreg, reg))
                        {
                            smallest = mreg.spanCount;
                            mergeId = mreg.id;
                        }
                    }
                    // Found new id.
                    if (mergeId != reg.id)
                    {
                        int oldId = reg.id;
                        var target = regions[mergeId];

                        // Merge neighbours.
                        if (MergeRegions(target, reg))
                        {
                            // Fixup regions pointing to current region.
                            for (int j = 0; j < nreg; ++j)
                            {
                                if (regions[j].id == 0 || (regions[j].id & Constants.RC_BORDER_REG) != 0)
                                {
                                    continue;
                                }

                                // If another region was already merged into current region
                                // change the nid of the previous region too.
                                if (regions[j].id == oldId)
                                {
                                    regions[j].id = mergeId;
                                }

                                // Replace the current region with the new one if the
                                // current regions is neighbour.
                                ReplaceNeighbour(regions[j], oldId, mergeId);
                            }

                            mergeCount++;
                        }
                    }
                }
            }
            while (mergeCount > 0);

            // Compress region Ids.
            for (int i = 0; i < nreg; ++i)
            {
                regions[i].remap = false;
                if (regions[i].id == 0)
                {
                    // Skip nil regions.
                    continue;
                }
                if ((regions[i].id & Constants.RC_BORDER_REG) != 0)
                {
                    // Skip external regions.
                    continue;
                }
                regions[i].remap = true;
            }

            int regIdGen = 0;
            for (int i = 0; i < nreg; ++i)
            {
                if (!regions[i].remap)
                {
                    continue;
                }
                int oldId = regions[i].id;
                int newId = ++regIdGen;
                for (int j = i; j < nreg; ++j)
                {
                    if (regions[j].id == oldId)
                    {
                        regions[j].id = newId;
                        regions[j].remap = false;
                    }
                }
            }
            maxRegionId = regIdGen;

            // Remap regions.
            for (int i = 0; i < chf.spanCount; ++i)
            {
                if ((srcReg[i] & Constants.RC_BORDER_REG) == 0)
                {
                    srcReg[i] = regions[srcReg[i]].id;
                }
            }

            // Return regions that we found to be overlapping.
            List<int> lOverlaps = new List<int>();
            for (int i = 0; i < nreg; ++i)
            {
                if (regions[i].overlap)
                {
                    lOverlaps.Add(regions[i].id);
                }
            }
            overlaps = lOverlaps.ToArray();

            for (int i = 0; i < nreg; ++i)
            {
                regions[i] = null;
            }

            regions = null;

            return true;
        }
        private static void ReplaceNeighbour(Region reg, int oldId, int newId)
        {
            bool neiChanged = false;
            for (int i = 0; i < reg.connections.Count; ++i)
            {
                if (reg.connections[i] == oldId)
                {
                    reg.connections[i] = newId;
                    neiChanged = true;
                }
            }
            for (int i = 0; i < reg.floors.Count; ++i)
            {
                if (reg.floors[i] == oldId)
                {
                    reg.floors[i] = newId;
                }
            }
            if (neiChanged)
            {
                RemoveAdjacentNeighbours(reg);
            }
        }

        private static void CalcGridSize(BoundingBox b, float cellSize, out int w, out int h)
        {
            w = (int)((b.Maximum.X - b.Minimum.X) / cellSize + 0.5f);
            h = (int)((b.Maximum.Z - b.Minimum.Z) / cellSize + 0.5f);
        }
        private static int RasterizeTileLayers(int tx, int ty, BuildSettings settings, Config cfg, InputGeometry geometry, out TileCacheData[] tiles)
        {
            tiles = new TileCacheData[Constants.MaxLayers];

            ChunkyTriMesh chunkyMesh = geometry.GetChunkyMesh();

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

            var rc = new RasterizationContext
            {
                // Allocate voxel heightfield where we rasterize our input data to.
                solid = new Heightfield
                {
                    width = tcfg.Width,
                    height = tcfg.Height,
                    boundingBox = tcfg.BoundingBox,
                    cs = tcfg.CellSize,
                    ch = tcfg.CellHeight,
                    spans = new Span[tcfg.Width * tcfg.Height],
                },

                // Allocate array that can hold triangle flags.
                // If you have multiple meshes you need to process, allocate
                // and array which can hold the max number of triangles you need to process.
                triareas = new TileCacheAreas[chunkyMesh.maxTrisPerChunk],

                tiles = new TileCacheData[RasterizationContext.MaxLayers],
            };

            Vector2 tbmin = new Vector2(tcfg.BoundingBox.Minimum.X, tcfg.BoundingBox.Minimum.Z);
            Vector2 tbmax = new Vector2(tcfg.BoundingBox.Maximum.X, tcfg.BoundingBox.Maximum.Z);

            var cid = GetChunksOverlappingRect(chunkyMesh, tbmin, tbmax);
            if (cid.Count() == 0)
            {
                return 0; // empty
            }

            foreach (var id in cid)
            {
                var tris = chunkyMesh.GetTriangles(id);

                Helper.InitializeArray(rc.triareas, TileCacheAreas.NullArea);

                MarkWalkableTriangles(tcfg.WalkableSlopeAngle, tris, rc.triareas);

                if (!RasterizeTriangles(rc.solid, tcfg.WalkableClimb, tris, rc.triareas))
                {
                    return 0;
                }
            }

            // Once all geometry is rasterized, we do initial pass of filtering to
            // remove unwanted overhangs caused by the conservative rasterization
            // as well as filter spans where the character cannot possibly stand.
            if (settings.FilterLowHangingObstacles)
            {
                FilterLowHangingWalkableObstacles(tcfg.WalkableClimb, rc.solid);
            }
            if (settings.FilterLedgeSpans)
            {
                FilterLedgeSpans(tcfg.WalkableHeight, tcfg.WalkableClimb, rc.solid);
            }
            if (settings.FilterWalkableLowHeightSpans)
            {
                FilterWalkableLowHeightSpans(tcfg.WalkableHeight, rc.solid);
            }

            if (!BuildCompactHeightfield(tcfg.WalkableHeight, tcfg.WalkableClimb, rc.solid, out rc.chf))
            {
                throw new EngineException("buildNavigation: Could not build compact height field.");
            }

            // Erode the walkable area by agent radius.
            if (!ErodeWalkableArea(tcfg.WalkableRadius, rc.chf))
            {
                throw new EngineException("buildNavigation: Could not erode.");
            }

            // (Optional) Mark areas.
            ConvexVolume[] vols = geometry.GetConvexVolumes();
            for (int i = 0; i < geometry.GetConvexVolumeCount(); ++i)
            {
                MarkConvexPolyArea(
                    vols[i].verts, vols[i].nverts,
                    vols[i].hmin, vols[i].hmax,
                    vols[i].area, rc.chf);
            }

            BuildHeightfieldLayers(rc.chf, tcfg.BorderSize, tcfg.WalkableHeight, out rc.lset);

            rc.ntiles = 0;
            for (int i = 0; i < Math.Min(rc.lset.nlayers, Constants.MaxLayers); i++)
            {
                HeightfieldLayer layer = rc.lset.layers[i];

                TileCacheData tile = rc.tiles[rc.ntiles];

                // Store header
                tile.Header = new TileCacheLayerHeader
                {
                    magic = TileCacheLayerHeader.TileCacheMagic,
                    version = TileCacheLayerHeader.TileCacheVersion,

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
                tile.Data = new TileCacheLayerData()
                {
                    heights = layer.heights,
                    areas = layer.areas,
                    cons = layer.cons,
                };

                rc.tiles[rc.ntiles++] = tile;
            }

            // Transfer ownsership of tile data from build context to the caller.
            int n = 0;
            for (int i = 0; i < Math.Min(rc.ntiles, Constants.MaxLayers); i++)
            {
                tiles[n++] = rc.tiles[i];
                rc.tiles[i].Data = TileCacheLayerData.Empty;
            }

            return n;
        }
        private static void MarkConvexPolyArea(Vector3[] verts, int nverts, float hmin, float hmax, TileCacheAreas areaId, CompactHeightfield chf)
        {
            Vector3 bmin = verts[0];
            Vector3 bmax = verts[0];

            for (int i = 1; i < nverts; ++i)
            {
                Vector3.Min(bmin, verts[i * 3]);
                Vector3.Max(bmax, verts[i * 3]);
            }
            bmin[1] = hmin;
            bmax[1] = hmax;

            int minx = (int)((bmin[0] - chf.boundingBox.Minimum[0]) / chf.cs);
            int miny = (int)((bmin[1] - chf.boundingBox.Minimum[1]) / chf.ch);
            int minz = (int)((bmin[2] - chf.boundingBox.Minimum[2]) / chf.cs);
            int maxx = (int)((bmax[0] - chf.boundingBox.Minimum[0]) / chf.cs);
            int maxy = (int)((bmax[1] - chf.boundingBox.Minimum[1]) / chf.ch);
            int maxz = (int)((bmax[2] - chf.boundingBox.Minimum[2]) / chf.cs);

            if (maxx < 0) return;
            if (minx >= chf.width) return;
            if (maxz < 0) return;
            if (minz >= chf.height) return;

            if (minx < 0) minx = 0;
            if (maxx >= chf.width) maxx = chf.width - 1;
            if (minz < 0) minz = 0;
            if (maxz >= chf.height) maxz = chf.height - 1;


            // TODO: Optimize.
            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    CompactCell c = chf.cells[x + z * chf.width];
                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        CompactSpan s = chf.spans[i];
                        if (chf.areas[i] == TileCacheAreas.NullArea)
                        {
                            continue;
                        }

                        if (s.y >= miny && s.y <= maxy)
                        {
                            Vector3 p = new Vector3();
                            p[0] = chf.boundingBox.Minimum[0] + (x + 0.5f) * chf.cs;
                            p[1] = 0;
                            p[2] = chf.boundingBox.Minimum[2] + (z + 0.5f) * chf.cs;

                            if (PolyUtils.PointInPoly(nverts, verts, p))
                            {
                                chf.areas[i] = areaId;
                            }
                        }
                    }
                }
            }
        }
        private static bool BuildHeightfieldLayers(CompactHeightfield chf, int borderSize, int walkableHeight, out HeightfieldLayerSet lset)
        {
            lset = new HeightfieldLayerSet();

            int w = chf.width;
            int h = chf.height;

            int[] srcReg = Helper.CreateArray(chf.spanCount, 0xff);

            int nsweeps = chf.width;
            LayerSweepSpan[] sweeps = Helper.CreateArray(nsweeps, new LayerSweepSpan());

            // Partition walkable area into monotone regions.
            int regId = 0;

            for (int y = borderSize; y < h - borderSize; ++y)
            {
                int[] prevCount = Helper.CreateArray(256, 0);
                int sweepId = 0;

                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    var c = chf.cells[x + y * w];

                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        var s = chf.spans[i];
                        if (chf.areas[i] == TileCacheAreas.NullArea) continue;

                        int sid = 0xff;

                        // -x
                        if (GetCon(s, 0) != Constants.NotConnected)
                        {
                            int ax = x + PolyUtils.GetDirOffsetX(0);
                            int ay = y + PolyUtils.GetDirOffsetY(0);
                            int ai = chf.cells[ax + ay * w].index + GetCon(s, 0);
                            if (chf.areas[ai] != TileCacheAreas.NullArea && srcReg[ai] != 0xff)
                            {
                                sid = srcReg[ai];
                            }
                        }

                        if (sid == 0xff)
                        {
                            sid = sweepId++;
                            sweeps[sid].nei = 0xff;
                            sweeps[sid].ns = 0;
                        }

                        // -y
                        if (GetCon(s, 3) != Constants.NotConnected)
                        {
                            int ax = x + PolyUtils.GetDirOffsetX(3);
                            int ay = y + PolyUtils.GetDirOffsetY(3);
                            int ai = chf.cells[ax + ay * w].index + GetCon(s, 3);
                            int nr = srcReg[ai];
                            if (nr != 0xff)
                            {
                                // Set neighbour when first valid neighbour is encoutered.
                                if (sweeps[sid].ns == 0)
                                    sweeps[sid].nei = nr;

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

                        srcReg[i] = sid;
                    }
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
                            throw new EngineException("rcBuildHeightfieldLayers: Region ID overflow.");
                        }
                        sweeps[i].id = regId++;
                    }
                }

                // Remap local sweep ids to region ids.
                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    var c = chf.cells[x + y * w];
                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        if (srcReg[i] != 0xff)
                        {
                            srcReg[i] = sweeps[srcReg[i]].id;
                        }
                    }
                }
            }

            // Allocate and init layer regions.
            int nregs = regId;
            LayerRegion[] regs = Helper.CreateArray(nregs, () => (LayerRegion.Default));

            // Find region neighbours and overlapping regions.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.cells[x + y * w];

                    int[] lregs = new int[LayerRegion.MaxLayers];
                    int nlregs = 0;

                    for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; ++i)
                    {
                        var s = chf.spans[i];
                        int ri = srcReg[i];
                        if (ri == 0xff)
                        {
                            continue;
                        }

                        regs[ri].ymin = Math.Min(regs[ri].ymin, s.y);
                        regs[ri].ymax = Math.Max(regs[ri].ymax, s.y);

                        // Collect all region layers.
                        if (nlregs < LayerRegion.MaxLayers)
                        {
                            lregs[nlregs++] = ri;
                        }

                        // Update neighbours
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (GetCon(s, dir) != Constants.NotConnected)
                            {
                                int ax = x + PolyUtils.GetDirOffsetX(dir);
                                int ay = y + PolyUtils.GetDirOffsetY(dir);
                                int ai = chf.cells[ax + ay * w].index + GetCon(s, dir);
                                int rai = srcReg[ai];
                                if (rai != 0xff && rai != ri)
                                {
                                    // Don't check return value -- if we cannot add the neighbor
                                    // it will just cause a few more regions to be created, which
                                    // is fine.
                                    PolyUtils.AddUnique(regs[ri].neis, ref regs[ri].nneis, LayerRegion.MaxNeighbors, rai);
                                }
                            }
                        }
                    }

                    // Update overlapping regions.
                    for (int i = 0; i < nlregs - 1; ++i)
                    {
                        for (int j = i + 1; j < nlregs; ++j)
                        {
                            if (lregs[i] != lregs[j])
                            {
                                var ri = regs[lregs[i]];
                                var rj = regs[lregs[j]];

                                if (!PolyUtils.AddUnique(ri.layers, ref ri.nlayers, LayerRegion.MaxLayers, lregs[j]) ||
                                    !PolyUtils.AddUnique(rj.layers, ref rj.nlayers, LayerRegion.MaxLayers, lregs[i]))
                                {
                                    throw new EngineException("rcBuildHeightfieldLayers: layer overflow (too many overlapping walkable platforms). Try increasing RC_MAX_LAYERS.");
                                }

                                regs[lregs[i]] = ri;
                                regs[lregs[j]] = rj;
                            }
                        }
                    }
                }
            }

            // Create 2D layers from regions.
            int layerId = 0;

            int MaxStack = 64;
            int[] stack = new int[MaxStack];
            int nstack = 0;

            for (int i = 0; i < nregs; ++i)
            {
                var root = regs[i];

                // Skip already visited.
                if (root.layerId != 0xff)
                {
                    continue;
                }

                // Start search.
                root.layerId = layerId;
                root.isBase = true;

                nstack = 0;
                stack[nstack++] = i;

                while (nstack != 0)
                {
                    // Pop front
                    var reg = regs[stack[0]];
                    nstack--;
                    for (int j = 0; j < nstack; ++j)
                    {
                        stack[j] = stack[j + 1];
                    }

                    int nneis = reg.nneis;
                    for (int j = 0; j < nneis; ++j)
                    {
                        int nei = reg.neis[j];
                        var regn = regs[nei];

                        // Skip already visited.
                        if (regn.layerId != 0xff)
                        {
                            continue;
                        }

                        // Skip if the neighbour is overlapping root region.
                        if (PolyUtils.Contains(root.layers, root.nlayers, nei))
                        {
                            continue;
                        }

                        // Skip if the height range would become too large.
                        int ymin = Math.Min(root.ymin, regn.ymin);
                        int ymax = Math.Max(root.ymax, regn.ymax);
                        if ((ymax - ymin) >= 255)
                        {
                            continue;
                        }

                        if (nstack < MaxStack)
                        {
                            // Deepen
                            stack[nstack++] = nei;

                            // Mark layer id
                            regn.layerId = layerId;

                            // Merge current layers to root.
                            for (int k = 0; k < regn.nlayers; ++k)
                            {
                                if (!PolyUtils.AddUnique(root.layers, ref root.nlayers, LayerRegion.MaxLayers, regn.layers[k]))
                                {
                                    throw new EngineException("rcBuildHeightfieldLayers: layer overflow (too many overlapping walkable platforms). Try increasing RC_MAX_LAYERS.");
                                }
                            }

                            root.ymin = Math.Min(root.ymin, regn.ymin);
                            root.ymax = Math.Max(root.ymax, regn.ymax);
                        }

                        regs[nei] = regn;
                    }
                }

                regs[i] = root;

                layerId++;
            }

            // Merge non-overlapping regions that are close in height.
            int mergeHeight = walkableHeight * 4;

            for (int i = 0; i < nregs; ++i)
            {
                var ri = regs[i];

                if (!ri.isBase)
                {
                    continue;
                }

                int newId = ri.layerId;

                for (; ; )
                {
                    int oldId = 0xff;

                    for (int j = 0; j < nregs; ++j)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        var rj = regs[j];
                        if (!rj.isBase)
                        {
                            continue;
                        }

                        // Skip if the regions are not close to each other.
                        if (!PolyUtils.OverlapRange(ri.ymin, (ri.ymax + mergeHeight), rj.ymin, (rj.ymax + mergeHeight)))
                        {
                            continue;
                        }

                        // Skip if the height range would become too large.
                        int ymin = Math.Min(ri.ymin, rj.ymin);
                        int ymax = Math.Max(ri.ymax, rj.ymax);
                        if ((ymax - ymin) >= 255)
                        {
                            continue;
                        }

                        // Make sure that there is no overlap when merging 'ri' and 'rj'.
                        bool overlap = false;

                        // Iterate over all regions which have the same layerId as 'rj'
                        for (int k = 0; k < nregs; ++k)
                        {
                            if (regs[k].layerId != rj.layerId)
                            {
                                continue;
                            }

                            // Check if region 'k' is overlapping region 'ri'
                            // Index to 'regs' is the same as region id.
                            if (PolyUtils.Contains(ri.layers, ri.nlayers, k))
                            {
                                overlap = true;
                                break;
                            }
                        }

                        // Cannot merge of regions overlap.
                        if (overlap)
                        {
                            continue;
                        }

                        // Can merge i and j.
                        oldId = rj.layerId;
                        break;
                    }

                    // Could not find anything to merge with, stop.
                    if (oldId == 0xff)
                    {
                        break;
                    }

                    // Merge
                    for (int j = 0; j < nregs; ++j)
                    {
                        var rj = regs[j];

                        if (rj.layerId == oldId)
                        {
                            rj.isBase = false;
                            // Remap layerIds.
                            rj.layerId = newId;
                            // Add overlaid layers from 'rj' to 'ri'.
                            for (int k = 0; k < rj.nlayers; ++k)
                            {
                                if (!PolyUtils.AddUnique(ri.layers, ref ri.nlayers, LayerRegion.MaxLayers, rj.layers[k]))
                                {
                                    throw new EngineException("rcBuildHeightfieldLayers: layer overflow (too many overlapping walkable platforms). Try increasing RC_MAX_LAYERS.");
                                }
                            }

                            // Update height bounds.
                            ri.ymin = Math.Min(ri.ymin, rj.ymin);
                            ri.ymax = Math.Max(ri.ymax, rj.ymax);

                            regs[j] = rj;
                        }
                    }
                }

                regs[i] = ri;
            }

            // Compact layerIds
            int[] remap = new int[256];

            // Find number of unique layers.
            layerId = 0;
            for (int i = 0; i < nregs; i++)
            {
                remap[regs[i].layerId] = 1;
            }

            for (int i = 0; i < 256; i++)
            {
                if (remap[i] != 0)
                {
                    remap[i] = layerId++;
                }
                else
                {
                    remap[i] = 0xff;
                }
            }

            // Remap ids.
            for (int i = 0; i < nregs; ++i)
            {
                regs[i].layerId = remap[regs[i].layerId];
            }

            // No layers, return empty.
            if (layerId == 0)
            {
                return true;
            }

            // Create layers.
            int lw = w - borderSize * 2;
            int lh = h - borderSize * 2;

            // Build contracted bbox for layers.
            Vector3 bmin = chf.boundingBox.Minimum;
            Vector3 bmax = chf.boundingBox.Maximum;
            bmin.X += borderSize * chf.cs;
            bmin.Z += borderSize * chf.cs;
            bmax.X -= borderSize * chf.cs;
            bmax.Z -= borderSize * chf.cs;

            lset.nlayers = layerId;
            lset.layers = new HeightfieldLayer[layerId];

            // Store layers.
            for (int i = 0; i < lset.nlayers; ++i)
            {
                int curId = i;

                var layer = lset.layers[i];

                int gridSize = lw * lh;

                layer.heights = Helper.CreateArray(gridSize, 0xff);
                layer.areas = Helper.CreateArray(gridSize, TileCacheAreas.NullArea);
                layer.cons = Helper.CreateArray(gridSize, 0x00);

                // Find layer height bounds.
                int hmin = 0, hmax = 0;
                for (int j = 0; j < nregs; ++j)
                {
                    if (regs[j].isBase && regs[j].layerId == curId)
                    {
                        hmin = regs[j].ymin;
                        hmax = regs[j].ymax;
                    }
                }

                layer.width = lw;
                layer.height = lh;
                layer.cs = chf.cs;
                layer.ch = chf.ch;

                // Adjust the bbox to fit the heightfield.
                layer.boundingBox = new BoundingBox(bmin, bmax);
                layer.boundingBox.Minimum.Y = bmin.Y + hmin * chf.ch;
                layer.boundingBox.Maximum.Y = bmin.Y + hmax * chf.ch;
                layer.hmin = hmin;
                layer.hmax = hmax;

                // Update usable data region.
                layer.minx = layer.width;
                layer.maxx = 0;
                layer.miny = layer.height;
                layer.maxy = 0;

                // Copy height and area from compact heightfield. 
                for (int y = 0; y < lh; ++y)
                {
                    for (int x = 0; x < lw; ++x)
                    {
                        int cx = borderSize + x;
                        int cy = borderSize + y;
                        var c = chf.cells[cx + cy * w];
                        for (int j = (int)c.index, nj = (int)(c.index + c.count); j < nj; ++j)
                        {
                            var s = chf.spans[j];
                            // Skip unassigned regions.
                            if (srcReg[j] == 0xff)
                            {
                                continue;
                            }

                            // Skip of does nto belong to current layer.
                            int lid = regs[srcReg[j]].layerId;
                            if (lid != curId)
                            {
                                continue;
                            }

                            // Update data bounds.
                            layer.minx = Math.Min(layer.minx, x);
                            layer.maxx = Math.Max(layer.maxx, x);
                            layer.miny = Math.Min(layer.miny, y);
                            layer.maxy = Math.Max(layer.maxy, y);

                            // Store height and area type.
                            int idx = x + y * lw;
                            layer.heights[idx] = (s.y - hmin);
                            layer.areas[idx] = chf.areas[j];

                            // Check connection.
                            int portal = 0;
                            int con = 0;
                            for (int dir = 0; dir < 4; ++dir)
                            {
                                if (GetCon(s, dir) != Constants.NotConnected)
                                {
                                    int ax = cx + PolyUtils.GetDirOffsetX(dir);
                                    int ay = cy + PolyUtils.GetDirOffsetY(dir);
                                    int ai = chf.cells[ax + ay * w].index + GetCon(s, dir);
                                    int alid = (srcReg[ai] != 0xff ? regs[srcReg[ai]].layerId : 0xff);
                                    // Portal mask
                                    if (chf.areas[ai] != TileCacheAreas.NullArea && lid != alid)
                                    {
                                        portal |= (1 << dir);

                                        // Update height so that it matches on both sides of the portal.
                                        var ass = chf.spans[ai];
                                        if (ass.y > hmin)
                                        {
                                            layer.heights[idx] = Math.Max(layer.heights[idx], (ass.y - hmin));
                                        }
                                    }
                                    // Valid connection mask
                                    if (chf.areas[ai] != TileCacheAreas.NullArea && lid == alid)
                                    {
                                        int nx = ax - borderSize;
                                        int ny = ay - borderSize;
                                        if (nx >= 0 && ny >= 0 && nx < lw && ny < lh)
                                        {
                                            con |= (1 << dir);
                                        }
                                    }
                                }
                            }

                            layer.cons[idx] = ((portal << 4) | con);
                        }
                    }
                }

                if (layer.minx > layer.maxx)
                {
                    layer.minx = layer.maxx = 0;
                }

                if (layer.miny > layer.maxy)
                {
                    layer.miny = layer.maxy = 0;
                }

                lset.layers[i] = layer;
            }

            return true;
        }
        private static void FilterLowHangingWalkableObstacles(int walkableClimb, Heightfield solid)
        {
            int w = solid.width;
            int h = solid.height;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool previousWalkable = false;
                    TileCacheAreas previousArea = TileCacheAreas.NullArea;

                    Span ps = null;

                    for (Span s = solid.spans[x + y * w]; s != null; ps = s, s = s.next)
                    {
                        bool walkable = s.area != TileCacheAreas.NullArea;

                        // If current span is not walkable, but there is walkable span just below it, mark the span above it walkable too.
                        if (!walkable && previousWalkable)
                        {
                            if (Math.Abs(s.smax - ps.smax) <= walkableClimb)
                            {
                                s.area = previousArea;
                            }
                        }

                        // Copy walkable flag so that it cannot propagate past multiple non-walkable objects.
                        previousWalkable = walkable;
                        previousArea = s.area;
                    }
                }
            }
        }
        private static void FilterLedgeSpans(int walkableHeight, int walkableClimb, Heightfield solid)
        {
            int w = solid.width;
            int h = solid.height;

            // Mark border spans.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    for (Span s = solid.spans[x + y * w]; s != null; s = s.next)
                    {
                        // Skip non walkable spans.
                        if (s.area == TileCacheAreas.NullArea)
                        {
                            continue;
                        }

                        int bot = s.smax;
                        int top = s.next != null ? s.next.smin : int.MaxValue;

                        // Find neighbours minimum height.
                        int minh = int.MaxValue;

                        // Min and max height of accessible neighbours.
                        int asmin = s.smax;
                        int asmax = s.smax;

                        for (int dir = 0; dir < 4; ++dir)
                        {
                            // Skip neighbours which are out of bounds.
                            int dx = x + PolyUtils.GetDirOffsetX(dir);
                            int dy = y + PolyUtils.GetDirOffsetY(dir);
                            if (dx < 0 || dy < 0 || dx >= w || dy >= h)
                            {
                                minh = Math.Min(minh, -walkableClimb - bot);
                                continue;
                            }

                            // From minus infinity to the first span.
                            var ns = solid.spans[dx + dy * w];
                            int nbot = -walkableClimb;
                            int ntop = ns != null ? ns.smin : int.MaxValue;

                            // Skip neightbour if the gap between the spans is too small.
                            if (Math.Min(top, ntop) - Math.Max(bot, nbot) > walkableHeight)
                            {
                                minh = Math.Min(minh, nbot - bot);
                            }

                            // Rest of the spans.
                            ns = solid.spans[dx + dy * w];
                            while (ns != null)
                            {
                                nbot = ns.smax;
                                ntop = ns.next != null ? ns.next.smin : int.MaxValue;

                                // Skip neightbour if the gap between the spans is too small.
                                if (Math.Min(top, ntop) - Math.Max(bot, nbot) > walkableHeight)
                                {
                                    minh = Math.Min(minh, nbot - bot);

                                    // Find min/max accessible neighbour height. 
                                    if (Math.Abs(nbot - bot) <= walkableClimb)
                                    {
                                        if (nbot < asmin) asmin = nbot;
                                        if (nbot > asmax) asmax = nbot;
                                    }

                                }

                                ns = ns.next;
                            }
                        }

                        if (minh < -walkableClimb)
                        {
                            // The current span is close to a ledge if the drop to any neighbour span is less than the walkableClimb.
                            s.area = TileCacheAreas.NullArea;
                        }
                        else if ((asmax - asmin) > walkableClimb)
                        {
                            // If the difference between all neighbours is too large, we are at steep slope, mark the span as ledge.
                            s.area = TileCacheAreas.NullArea;
                        }
                    }
                }
            }
        }
        private static void FilterWalkableLowHeightSpans(int walkableHeight, Heightfield solid)
        {
            int w = solid.width;
            int h = solid.height;

            // Remove walkable flag from spans which do not have enough space above them for the agent to stand there.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    for (var s = solid.spans[x + y * w]; s != null; s = s.next)
                    {
                        int bot = s.smax;
                        int top = s.next != null ? s.next.smin : int.MaxValue;

                        if ((top - bot) <= walkableHeight)
                        {
                            s.area = TileCacheAreas.NullArea;
                        }
                    }
                }
            }
        }
        private static bool ErodeWalkableArea(int radius, CompactHeightfield chf)
        {
            int w = chf.width;
            int h = chf.height;

            // Init distance.
            int[] dist = Helper.CreateArray(chf.spanCount, 0xff);

            // Mark boundary cells.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    CompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        if (chf.areas[i] == TileCacheAreas.NullArea)
                        {
                            dist[i] = 0;
                        }
                        else
                        {
                            CompactSpan s = chf.spans[i];
                            int nc = 0;
                            for (int dir = 0; dir < 4; ++dir)
                            {
                                if (GetCon(s, dir) != Constants.NotConnected)
                                {
                                    int nx = x + PolyUtils.GetDirOffsetX(dir);
                                    int ny = y + PolyUtils.GetDirOffsetY(dir);
                                    int nidx = chf.cells[nx + ny * w].index + GetCon(s, dir);
                                    if (chf.areas[nidx] != TileCacheAreas.NullArea)
                                    {
                                        nc++;
                                    }
                                }
                            }
                            // At least one missing neighbour.
                            if (nc != 4)
                            {
                                dist[i] = 0;
                            }
                        }
                    }
                }
            }

            int nd;

            // Pass 1
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    CompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        CompactSpan s = chf.spans[i];
                        if (GetCon(s, 0) != Constants.NotConnected)
                        {
                            // (-1,0)
                            int ax = x + PolyUtils.GetDirOffsetX(0);
                            int ay = y + PolyUtils.GetDirOffsetY(0);
                            int ai = chf.cells[ax + ay * w].index + GetCon(s, 0);
                            CompactSpan asp = chf.spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (-1,-1)
                            if (GetCon(asp, 3) != Constants.NotConnected)
                            {
                                int aax = ax + PolyUtils.GetDirOffsetX(3);
                                int aay = ay + PolyUtils.GetDirOffsetY(3);
                                int aai = chf.cells[aax + aay * w].index + GetCon(asp, 3);
                                nd = Math.Min(dist[aai] + 3, 255);
                                if (nd < dist[i])
                                {
                                    dist[i] = nd;
                                }
                            }
                        }
                        if (GetCon(s, 3) != Constants.NotConnected)
                        {
                            // (0,-1)
                            int ax = x + PolyUtils.GetDirOffsetX(3);
                            int ay = y + PolyUtils.GetDirOffsetY(3);
                            int ai = chf.cells[ax + ay * w].index + GetCon(s, 3);
                            CompactSpan asp = chf.spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (1,-1)
                            if (GetCon(asp, 2) != Constants.NotConnected)
                            {
                                int aax = ax + PolyUtils.GetDirOffsetX(2);
                                int aay = ay + PolyUtils.GetDirOffsetY(2);
                                int aai = chf.cells[aax + aay * w].index + GetCon(asp, 2);
                                nd = Math.Min(dist[aai] + 3, 255);
                                if (nd < dist[i])
                                {
                                    dist[i] = nd;
                                }
                            }
                        }
                    }
                }
            }

            // Pass 2
            for (int y = h - 1; y >= 0; --y)
            {
                for (int x = w - 1; x >= 0; --x)
                {
                    var c = chf.cells[x + y * w];
                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        var s = chf.spans[i];
                        if (GetCon(s, 2) != Constants.NotConnected)
                        {
                            // (1,0)
                            int ax = x + PolyUtils.GetDirOffsetX(2);
                            int ay = y + PolyUtils.GetDirOffsetY(2);
                            int ai = chf.cells[ax + ay * w].index + GetCon(s, 2);
                            var asp = chf.spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (1,1)
                            if (GetCon(asp, 1) != Constants.NotConnected)
                            {
                                int aax = ax + PolyUtils.GetDirOffsetX(1);
                                int aay = ay + PolyUtils.GetDirOffsetY(1);
                                int aai = chf.cells[aax + aay * w].index + GetCon(asp, 1);
                                nd = Math.Min(dist[aai] + 3, 255);
                                if (nd < dist[i])
                                {
                                    dist[i] = nd;
                                }
                            }
                        }
                        if (GetCon(s, 1) != Constants.NotConnected)
                        {
                            // (0,1)
                            int ax = x + PolyUtils.GetDirOffsetX(1);
                            int ay = y + PolyUtils.GetDirOffsetY(1);
                            int ai = chf.cells[ax + ay * w].index + GetCon(s, 1);
                            var asp = chf.spans[ai];
                            nd = Math.Min(dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (-1,1)
                            if (GetCon(asp, 0) != Constants.NotConnected)
                            {
                                int aax = ax + PolyUtils.GetDirOffsetX(0);
                                int aay = ay + PolyUtils.GetDirOffsetY(0);
                                int aai = chf.cells[aax + aay * w].index + GetCon(asp, 0);
                                nd = Math.Min(dist[aai] + 3, 255);
                                if (nd < dist[i])
                                {
                                    dist[i] = nd;
                                }
                            }
                        }
                    }
                }
            }

            int thr = radius * 2;
            for (int i = 0; i < chf.spanCount; ++i)
            {
                if (dist[i] < thr)
                {
                    chf.areas[i] = TileCacheAreas.NullArea;
                }
            }

            dist = null;

            return true;
        }
        private static bool RasterizeTriangles(Heightfield solid, int flagMergeThr, Triangle[] tris, TileCacheAreas[] areas)
        {
            float ics = 1.0f / solid.cs;
            float ich = 1.0f / solid.ch;

            // Rasterize triangles.
            for (int i = 0; i < tris.Length; ++i)
            {
                // Rasterize.
                if (!RasterizeTri(tris[i], areas[i], solid, solid.boundingBox, solid.cs, ics, ich, flagMergeThr))
                {
                    throw new EngineException("rcRasterizeTriangles: Out of memory.");
                }
            }

            return true;
        }
        private static bool RasterizeTri(Triangle tri, TileCacheAreas area, Heightfield hf, BoundingBox b, float cs, float ics, float ich, int flagMergeThr)
        {
            int w = hf.width;
            int h = hf.height;
            float by = b.GetY();

            // Calculate the bounding box of the triangle.
            var t = BoundingBox.FromPoints(tri.GetVertices());

            // If the triangle does not touch the bbox of the heightfield, skip the triagle.
            if (b.Contains(t) == ContainmentType.Disjoint)
            {
                return true;
            }

            // Calculate the footprint of the triangle on the grid's y-axis
            int y0 = (int)((t.Minimum.Z - b.Minimum.Z) * ics);
            int y1 = (int)((t.Maximum.Z - b.Minimum.Z) * ics);
            y0 = MathUtil.Clamp(y0, 0, h - 1);
            y1 = MathUtil.Clamp(y1, 0, h - 1);

            // Clip the triangle into all grid cells it touches.
            List<Vector3> inb = new List<Vector3>(tri.GetVertices());
            List<Vector3> zp1 = new List<Vector3>();
            List<Vector3> zp2 = new List<Vector3>();
            List<Vector3> xp1 = new List<Vector3>();
            List<Vector3> xp2 = new List<Vector3>();

            for (int y = y0; y <= y1; ++y)
            {
                // Clip polygon to row. Store the remaining polygon as well
                zp1.Clear();
                zp2.Clear();
                float cz = b.Minimum.Z + y * cs;
                PolyUtils.DividePoly(inb, zp1, zp2, cz + cs, 2);
                Helper.Swap(ref inb, ref zp2);
                if (zp1.Count < 3) continue;

                // find the horizontal bounds in the row
                float minX = zp1[0].X;
                float maxX = zp1[0].X;
                for (int i = 1; i < zp1.Count; i++)
                {
                    minX = Math.Min(minX, zp1[i].X);
                    maxX = Math.Max(maxX, zp1[i].X);
                }
                minX -= b.Minimum.X;
                maxX -= b.Minimum.X;
                int x0 = MathUtil.Clamp((int)(minX * ics), 0, w - 1);
                int x1 = MathUtil.Clamp((int)(maxX * ics), 0, w - 1);

                for (int x = x0; x <= x1; ++x)
                {
                    // Clip polygon to column. store the remaining polygon as well
                    xp1.Clear();
                    xp2.Clear();
                    float cx = b.Minimum.X + x * cs;
                    PolyUtils.DividePoly(zp1, xp1, xp2, cx + cs, 0);
                    Helper.Swap(ref zp1, ref xp2);
                    if (xp1.Count < 3) continue;

                    // Calculate min and max of the span.
                    float minY = xp1[0].Y;
                    float maxY = xp1[0].Y;
                    for (int i = 1; i < xp1.Count; ++i)
                    {
                        minY = Math.Min(minY, xp1[i].Y);
                        maxY = Math.Max(maxY, xp1[i].Y);
                    }
                    minY -= b.Minimum.Y;
                    maxY -= b.Minimum.Y;
                    // Skip the span if it is outside the heightfield bbox
                    if (maxY < 0.0f) continue;
                    if (minY > by) continue;
                    // Clamp the span to the heightfield bbox.
                    if (minY < 0.0f) minY = 0;
                    if (maxY > by) maxY = by;

                    // Snap the span to the heightfield height grid.
                    int ismin = MathUtil.Clamp((int)Math.Floor(minY * ich), 0, Span.SpanMaxHeight);
                    int ismax = MathUtil.Clamp((int)Math.Ceiling(maxY * ich), ismin + 1, Span.SpanMaxHeight);

                    if (!AddSpan(hf, x, y, ismin, ismax, area, flagMergeThr))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        private static bool AddSpan(Heightfield hf, int x, int y, int smin, int smax, TileCacheAreas area, int flagMergeThr)
        {
            int idx = x + y * hf.width;

            Span s = AllocSpan(hf);
            s.smin = smin;
            s.smax = smax;
            s.area = area;
            s.next = null;

            // Empty cell, add the first span.
            if (hf.spans[idx] == null)
            {
                hf.spans[idx] = s;
                return true;
            }
            Span prev = null;
            Span cur = hf.spans[idx];

            // Insert and merge spans.
            while (cur != null)
            {
                if (cur.smin > s.smax)
                {
                    // Current span is further than the new span, break.
                    break;
                }
                else if (cur.smax < s.smin)
                {
                    // Current span is before the new span advance.
                    prev = cur;
                    cur = cur.next;
                }
                else
                {
                    // Merge spans.
                    if (cur.smin < s.smin)
                    {
                        s.smin = cur.smin;
                    }
                    if (cur.smax > s.smax)
                    {
                        s.smax = cur.smax;
                    }

                    // Merge flags.
                    if (Math.Abs((int)s.smax - (int)cur.smax) <= flagMergeThr)
                    {
                        s.area = (TileCacheAreas)Math.Max((int)s.area, (int)cur.area);
                    }

                    // Remove current span.
                    Span next = cur.next;
                    FreeSpan(hf, cur);
                    if (prev != null)
                    {
                        prev.next = next;
                    }
                    else
                    {
                        hf.spans[idx] = next;
                    }

                    cur = next;
                }
            }

            // Insert new span.
            if (prev != null)
            {
                s.next = prev.next;
                prev.next = s;
            }
            else
            {
                s.next = hf.spans[idx];
                hf.spans[idx] = s;
            }

            return true;
        }
        private static Span AllocSpan(Heightfield hf)
        {
            // If running out of memory, allocate new page and update the freelist.
            if (hf.freelist == null || hf.freelist.next == null)
            {
                // Create new page.
                // Allocate memory for the new pool.
                SpanPool pool = new SpanPool
                {
                    // Add the pool into the list of pools.
                    next = hf.pools.Count > 0 ? hf.pools.Last() : null
                };
                hf.pools.Add(pool);
                // Add new items to the free list.
                Span freelist = hf.freelist;
                int itIndex = Constants.RC_SPANS_PER_POOL;
                do
                {
                    var it = pool.items[--itIndex];
                    it.next = freelist;
                    freelist = it;
                }
                while (itIndex > 0);
                hf.freelist = pool.items[itIndex];
            }

            // Pop item from in front of the free list.
            Span s = hf.freelist;
            hf.freelist = hf.freelist.next;
            return s;
        }
        private static void FreeSpan(Heightfield hf, Span cur)
        {
            if (cur == null) return;

            // Add the node in front of the free list.
            cur.next = hf.freelist;
            hf.freelist = cur;
        }
        private static int MarkWalkableTriangles(float walkableSlopeAngle, Triangle[] tris, TileCacheAreas[] areas)
        {
            float walkableThr = (float)Math.Cos(walkableSlopeAngle / 180.0f * MathUtil.Pi);

            int count = 0;

            for (int i = 0; i < tris.Length; i++)
            {
                var tri = tris[i];
                Vector3 norm = tri.Normal;

                // Check if the face is walkable.
                if (norm.Y > walkableThr)
                {
                    areas[i] = TileCacheAreas.WalkableArea;
                    count++;
                }
            }

            return count;
        }
        private static bool BuildCompactHeightfield(int walkableHeight, int walkableClimb, Heightfield hf, out CompactHeightfield chf)
        {
            int w = hf.width;
            int h = hf.height;
            int spanCount = GetHeightFieldSpanCount(hf);
            var bbox = hf.boundingBox;
            bbox.Maximum.Y += walkableHeight * hf.ch;

            // Fill in header.
            chf = new CompactHeightfield
            {
                width = w,
                height = h,
                spanCount = spanCount,
                walkableHeight = walkableHeight,
                walkableClimb = walkableClimb,
                maxRegions = 0,
                boundingBox = bbox,
                cs = hf.cs,
                ch = hf.ch,
                cells = new CompactCell[w * h],
                spans = new CompactSpan[spanCount],
                areas = new TileCacheAreas[spanCount]
            };

            // Fill in cells and spans.
            int idx = 0;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var s = hf.spans[x + y * w];

                    // If there are no spans at this cell, just leave the data to index=0, count=0.
                    if (s == null)
                    {
                        continue;
                    }

                    var c = new CompactCell
                    {
                        index = idx,
                        count = 0
                    };

                    while (s != null)
                    {
                        if (s.area != TileCacheAreas.NullArea)
                        {
                            int bot = s.smax;
                            int top = s.next != null ? s.next.smin : int.MaxValue;
                            chf.spans[idx].y = MathUtil.Clamp(bot, 0, 0xffff);
                            chf.spans[idx].h = MathUtil.Clamp(top - bot, 0, 0xff);
                            chf.areas[idx] = s.area;
                            idx++;
                            c.count++;
                        }

                        s = s.next;
                    }

                    chf.cells[x + y * w] = c;
                }
            }

            // Find neighbour connections.
            int maxLayers = Constants.NotConnected - 1;
            int tooHighNeighbour = 0;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.cells[x + y * w];

                    for (int i = c.index, ni = (c.index + c.count); i < ni; i++)
                    {
                        var s = chf.spans[i];

                        for (int dir = 0; dir < 4; dir++)
                        {
                            SetCon(ref s, dir, Constants.NotConnected);
                            int nx = x + PolyUtils.GetDirOffsetX(dir);
                            int ny = y + PolyUtils.GetDirOffsetY(dir);
                            // First check that the neighbour cell is in bounds.
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                            {
                                continue;
                            }

                            // Iterate over all neighbour spans and check if any of the is
                            // accessible from current cell.
                            var nc = chf.cells[nx + ny * w];

                            for (int k = nc.index, nk = (nc.index + nc.count); k < nk; ++k)
                            {
                                var ns = chf.spans[k];

                                int bot = Math.Max(s.y, ns.y);
                                int top = Math.Min(s.y + s.h, ns.y + ns.h);

                                // Check that the gap between the spans is walkable,
                                // and that the climb height between the gaps is not too high.
                                if ((top - bot) >= walkableHeight && Math.Abs(ns.y - s.y) <= walkableClimb)
                                {
                                    // Mark direction as walkable.
                                    int lidx = k - nc.index;
                                    if (lidx < 0 || lidx > maxLayers)
                                    {
                                        tooHighNeighbour = Math.Max(tooHighNeighbour, lidx);
                                        continue;
                                    }

                                    SetCon(ref s, dir, lidx);
                                    break;
                                }
                            }
                        }

                        chf.spans[i] = s;
                    }
                }
            }

            if (tooHighNeighbour > maxLayers)
            {
                throw new EngineException(string.Format("Heightfield has too many layers {0} (max: {1})", tooHighNeighbour, maxLayers));
            }

            return true;
        }
        private static void SetCon(ref CompactSpan s, int dir, int i)
        {
            int shift = dir * 6;
            int con = s.con;
            s.con = (con & ~(0x3f << shift)) | ((i & 0x3f) << shift);
        }
        private static int GetCon(CompactSpan s, int dir)
        {
            int shift = dir * 6;
            return (s.con >> shift) & 0x3f;
        }
        private static int GetHeightFieldSpanCount(Heightfield hf)
        {
            int w = hf.width;
            int h = hf.height;

            int spanCount = 0;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    for (Span s = hf.spans[x + y * w]; s != null; s = s.next)
                    {
                        if (s.area != TileCacheAreas.NullArea)
                        {
                            spanCount++;
                        }
                    }
                }
            }

            return spanCount;
        }
        private static IEnumerable<int> GetChunksOverlappingRect(ChunkyTriMesh cm, Vector2 bmin, Vector2 bmax)
        {
            List<int> ids = new List<int>();

            // Traverse tree
            int i = 0;
            while (i < cm.nnodes)
            {
                ChunkyTriMeshNode node = cm.nodes[i];
                bool overlap = PolyUtils.CheckOverlapRect(bmin, bmax, node.bmin, node.bmax);
                bool isLeafNode = node.i >= 0;

                if (isLeafNode && overlap)
                {
                    ids.Add(i);
                }

                if (overlap || isLeafNode)
                {
                    i++;
                }
                else
                {
                    int escapeIndex = -node.i;
                    i += escapeIndex;
                }
            }

            return ids;
        }
        private static int CalcLayerBufferSize(int gridWidth, int gridHeight)
        {
            int headerSize = Helper.Align4(TileCacheLayerHeader.Size);
            int gridSize = gridWidth * gridHeight;

            return headerSize + gridSize * 4;
        }
        private static void AddUniqueFloorRegion(Region reg, int n)
        {
            for (int i = 0; i < reg.floors.Count; ++i)
            {
                if (reg.floors[i] == n)
                {
                    return;
                }
            }
            reg.floors.Add(n);
        }
        private static bool IsSolidEdge(CompactHeightfield chf, int[] srcReg, int x, int y, int i, int dir)
        {
            var s = chf.spans[i];
            int r = 0;
            if (GetCon(s, dir) != Constants.NotConnected)
            {
                int ax = x + PolyUtils.GetDirOffsetX(dir);
                int ay = y + PolyUtils.GetDirOffsetY(dir);
                int ai = chf.cells[ax + ay * chf.width].index + GetCon(s, dir);
                r = srcReg[ai];
            }
            if (r == srcReg[i])
            {
                return false;
            }
            return true;
        }
        private static void WalkContour(int x, int y, int i, int dir, CompactHeightfield chf, int[] srcReg, List<int> cont)
        {
            int startDir = dir;
            int starti = i;

            var ss = chf.spans[i];
            int curReg = 0;
            if (GetCon(ss, dir) != Constants.NotConnected)
            {
                int ax = x + PolyUtils.GetDirOffsetX(dir);
                int ay = y + PolyUtils.GetDirOffsetY(dir);
                int ai = chf.cells[ax + ay * chf.width].index + GetCon(ss, dir);
                curReg = srcReg[ai];
            }
            cont.Add(curReg);

            int iter = 0;
            while (++iter < 40000)
            {
                var s = chf.spans[i];

                if (IsSolidEdge(chf, srcReg, x, y, i, dir))
                {
                    // Choose the edge corner
                    int r = 0;
                    if (GetCon(s, dir) != Constants.NotConnected)
                    {
                        int ax = x + PolyUtils.GetDirOffsetX(dir);
                        int ay = y + PolyUtils.GetDirOffsetY(dir);
                        int ai = chf.cells[ax + ay * chf.width].index + GetCon(s, dir);
                        r = srcReg[ai];
                    }
                    if (r != curReg)
                    {
                        curReg = r;
                        cont.Add(curReg);
                    }

                    dir = (dir + 1) & 0x3;  // Rotate CW
                }
                else
                {
                    int ni = -1;
                    int nx = x + PolyUtils.GetDirOffsetX(dir);
                    int ny = y + PolyUtils.GetDirOffsetY(dir);
                    if (GetCon(s, dir) != Constants.NotConnected)
                    {
                        var nc = chf.cells[nx + ny * chf.width];
                        ni = nc.index + GetCon(s, dir);
                    }
                    if (ni == -1)
                    {
                        // Should not happen.
                        return;
                    }
                    x = nx;
                    y = ny;
                    i = ni;
                    dir = (dir + 3) & 0x3;  // Rotate CCW
                }

                if (starti == i && startDir == dir)
                {
                    break;
                }
            }

            // Remove adjacent duplicates.
            if (cont.Count > 1)
            {
                for (int j = 0; j < cont.Count;)
                {
                    int nj = (j + 1) % cont.Count;
                    if (cont[j] == cont[nj])
                    {
                        for (int k = j; k < cont.Count - 1; ++k)
                        {
                            cont[k] = cont[k + 1];
                        }
                        cont.RemoveAt(0);
                    }
                    else
                    {
                        ++j;
                    }
                }
            }
        }
        private static bool IsRegionConnectedToBorder(Region reg)
        {
            // Region is connected to border if
            // one of the neighbours is null id.
            for (int i = 0; i < reg.connections.Count; ++i)
            {
                if (reg.connections[i] == 0)
                {
                    return true;
                }
            }
            return false;
        }
        private static bool CanMergeWithRegion(Region rega, Region regb)
        {
            if (rega.areaType != regb.areaType)
            {
                return false;
            }
            int n = 0;
            for (int i = 0; i < rega.connections.Count; ++i)
            {
                if (rega.connections[i] == regb.id)
                {
                    n++;
                }
            }
            if (n > 1)
            {
                return false;
            }
            for (int i = 0; i < rega.floors.Count; ++i)
            {
                if (rega.floors[i] == regb.id)
                {
                    return false;
                }
            }
            return true;
        }
        private static bool MergeRegions(Region rega, Region regb)
        {
            int aid = rega.id;
            int bid = regb.id;

            // Duplicate current neighbourhood.
            List<int> acon = new List<int>(rega.connections);
            List<int> bcon = regb.connections;

            // Find insertion point on A.
            int insa = -1;
            for (int i = 0; i < acon.Count; ++i)
            {
                if (acon[i] == bid)
                {
                    insa = i;
                    break;
                }
            }
            if (insa == -1)
            {
                return false;
            }

            // Find insertion point on B.
            int insb = -1;
            for (int i = 0; i < bcon.Count; ++i)
            {
                if (bcon[i] == aid)
                {
                    insb = i;
                    break;
                }
            }
            if (insb == -1)
            {
                return false;
            }

            // Merge neighbours.
            rega.connections.Clear();
            for (int i = 0, ni = acon.Count; i < ni - 1; ++i)
            {
                rega.connections.Add(acon[(insa + 1 + i) % ni]);
            }

            for (int i = 0, ni = bcon.Count; i < ni - 1; ++i)
            {
                rega.connections.Add(bcon[(insb + 1 + i) % ni]);
            }

            RemoveAdjacentNeighbours(rega);

            for (int j = 0; j < regb.floors.Count; ++j)
            {
                AddUniqueFloorRegion(rega, regb.floors[j]);
            }
            rega.spanCount += regb.spanCount;
            regb.spanCount = 0;
            regb.connections.Clear();

            return true;
        }
        private static void RemoveAdjacentNeighbours(Region reg)
        {
            // Remove adjacent duplicates.
            for (int i = 0; i < reg.connections.Count && reg.connections.Count > 1;)
            {
                int ni = (i + 1) % reg.connections.Count;
                if (reg.connections[i] == reg.connections[ni])
                {
                    // Remove duplicate
                    for (int j = i; j < reg.connections.Count - 1; ++j)
                    {
                        reg.connections[j] = reg.connections[j + 1];
                    }
                    reg.connections.RemoveAt(reg.connections.Count - 1);
                }
                else
                {
                    ++i;
                }
            }
        }
        private static bool BuildRegionsMonotone(CompactHeightfield chf, int borderSize, int minRegionArea, int mergeRegionArea)
        {
            int w = chf.width;
            int h = chf.height;
            int id = 1;

            int[] srcReg = new int[chf.spanCount];

            int nsweeps = Math.Max(chf.width, chf.height);
            SweepSpan[] sweeps = new SweepSpan[nsweeps];

            // Mark border regions.
            if (borderSize > 0)
            {
                // Make sure border will not overflow.
                int bw = Math.Min(w, borderSize);
                int bh = Math.Min(h, borderSize);
                // Paint regions
                PaintRectRegion(0, bw, 0, h, id | Constants.RC_BORDER_REG, chf, srcReg); id++;
                PaintRectRegion(w - bw, w, 0, h, id | Constants.RC_BORDER_REG, chf, srcReg); id++;
                PaintRectRegion(0, w, 0, bh, id | Constants.RC_BORDER_REG, chf, srcReg); id++;
                PaintRectRegion(0, w, h - bh, h, id | Constants.RC_BORDER_REG, chf, srcReg); id++;

                chf.borderSize = borderSize;
            }

            // Sweep one line at a time.
            for (int y = borderSize; y < h - borderSize; ++y)
            {
                // Collect spans from this row.
                int[] prev = new int[id + 1];
                int rid = 1;

                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    var c = chf.cells[x + y * w];

                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        var s = chf.spans[i];
                        if (chf.areas[i] == TileCacheAreas.NullArea)
                        {
                            continue;
                        }

                        // -x
                        int previd = 0;
                        if (GetCon(s, 0) != Constants.NotConnected)
                        {
                            int ax = x + PolyUtils.GetDirOffsetX(0);
                            int ay = y + PolyUtils.GetDirOffsetY(0);
                            int ai = chf.cells[ax + ay * w].index + GetCon(s, 0);
                            if ((srcReg[ai] & Constants.RC_BORDER_REG) == 0 && chf.areas[i] == chf.areas[ai])
                            {
                                previd = srcReg[ai];
                            }
                        }

                        if (previd == 0)
                        {
                            previd = rid++;
                            sweeps[previd].rid = previd;
                            sweeps[previd].ns = 0;
                            sweeps[previd].nei = 0;
                        }

                        // -y
                        if (GetCon(s, 3) != Constants.NotConnected)
                        {
                            int ax = x + PolyUtils.GetDirOffsetX(3);
                            int ay = y + PolyUtils.GetDirOffsetY(3);
                            int ai = chf.cells[ax + ay * w].index + GetCon(s, 3);
                            if (srcReg[ai] != 0 && (srcReg[ai] & Constants.RC_BORDER_REG) == 0 && chf.areas[i] == chf.areas[ai])
                            {
                                int nr = srcReg[ai];
                                if (sweeps[previd].nei == 0 || sweeps[previd].nei == nr)
                                {
                                    sweeps[previd].nei = nr;
                                    sweeps[previd].ns++;
                                    prev[nr]++;
                                }
                                else
                                {
                                    sweeps[previd].nei = Constants.RC_NULL_NEI;
                                }
                            }
                        }

                        srcReg[i] = previd;
                    }
                }

                // Create unique ID.
                for (int i = 1; i < rid; ++i)
                {
                    if (sweeps[i].nei != Constants.RC_NULL_NEI &&
                        sweeps[i].nei != 0 &&
                        prev[sweeps[i].nei] == sweeps[i].ns)
                    {
                        sweeps[i].id = sweeps[i].nei;
                    }
                    else
                    {
                        sweeps[i].id = id++;
                    }
                }

                // Remap IDs
                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    var c = chf.cells[x + y * w];

                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        if (srcReg[i] > 0 && srcReg[i] < rid)
                        {
                            srcReg[i] = sweeps[srcReg[i]].id;
                        }
                    }
                }
            }

            {
                // Merge regions and filter out small regions.
                chf.maxRegions = id;
                if (!MergeAndFilterRegions(minRegionArea, mergeRegionArea, ref chf.maxRegions, chf, srcReg, out int[] overlaps))
                {
                    return false;
                }

                // Monotone partitioning does not generate overlapping regions.
            }

            // Store the result out.
            for (int i = 0; i < chf.spanCount; ++i)
            {
                chf.spans[i].reg = srcReg[i];
            }

            return true;
        }
        private static bool BuildLayerRegions(CompactHeightfield chf, int borderSize, int minRegionArea)
        {
            int w = chf.width;
            int h = chf.height;
            int id = 1;

            int[] srcReg = new int[chf.spanCount];

            int nsweeps = Math.Max(chf.width, chf.height);
            SweepSpan[] sweeps = Helper.CreateArray(nsweeps, new SweepSpan());

            // Mark border regions.
            if (borderSize > 0)
            {
                // Make sure border will not overflow.
                int bw = Math.Min(w, borderSize);
                int bh = Math.Min(h, borderSize);
                // Paint regions
                PaintRectRegion(0, bw, 0, h, id | Constants.RC_BORDER_REG, chf, srcReg); id++;
                PaintRectRegion(w - bw, w, 0, h, id | Constants.RC_BORDER_REG, chf, srcReg); id++;
                PaintRectRegion(0, w, 0, bh, id | Constants.RC_BORDER_REG, chf, srcReg); id++;
                PaintRectRegion(0, w, h - bh, h, id | Constants.RC_BORDER_REG, chf, srcReg); id++;

                chf.borderSize = borderSize;
            }

            // Sweep one line at a time.
            for (int y = borderSize; y < h - borderSize; ++y)
            {
                // Collect spans from this row.
                int[] prev = new int[256];
                int rid = 1;

                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    var c = chf.cells[x + y * w];

                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        var s = chf.spans[i];
                        if (chf.areas[i] == TileCacheAreas.NullArea)
                        {
                            continue;
                        }

                        // -x
                        int previd = 0;
                        if (GetCon(s, 0) != Constants.NotConnected)
                        {
                            int ax = x + PolyUtils.GetDirOffsetX(0);
                            int ay = y + PolyUtils.GetDirOffsetY(0);
                            int ai = chf.cells[ax + ay * w].index + GetCon(s, 0);
                            if ((srcReg[ai] & Constants.RC_BORDER_REG) == 0 && chf.areas[i] == chf.areas[ai])
                            {
                                previd = srcReg[ai];
                            }
                        }

                        if (previd == 0)
                        {
                            previd = rid++;
                            sweeps[previd].rid = previd;
                            sweeps[previd].ns = 0;
                            sweeps[previd].nei = 0;
                        }

                        // -y
                        if (GetCon(s, 3) != Constants.NotConnected)
                        {
                            int ax = x + PolyUtils.GetDirOffsetX(3);
                            int ay = y + PolyUtils.GetDirOffsetY(3);
                            int ai = chf.cells[ax + ay * w].index + GetCon(s, 3);
                            if (srcReg[ai] != 0 && (srcReg[ai] & Constants.RC_BORDER_REG) == 0 && chf.areas[i] == chf.areas[ai])
                            {
                                int nr = srcReg[ai];
                                if (sweeps[previd].nei == 0 || sweeps[previd].nei == nr)
                                {
                                    sweeps[previd].nei = nr;
                                    sweeps[previd].ns++;
                                    prev[nr]++;
                                }
                                else
                                {
                                    sweeps[previd].nei = Constants.RC_NULL_NEI;
                                }
                            }
                        }

                        srcReg[i] = previd;
                    }
                }

                // Create unique ID.
                for (int i = 1; i < rid; ++i)
                {
                    if (sweeps[i].nei != Constants.RC_NULL_NEI &&
                        sweeps[i].nei != 0 &&
                        prev[sweeps[i].nei] == sweeps[i].ns)
                    {
                        sweeps[i].id = sweeps[i].nei;
                    }
                    else
                    {
                        sweeps[i].id = id++;
                    }
                }

                // Remap IDs
                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    var c = chf.cells[x + y * w];

                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        if (srcReg[i] > 0 && srcReg[i] < rid)
                        {
                            srcReg[i] = sweeps[srcReg[i]].id;
                        }
                    }
                }
            }

            {
                // Merge monotone regions to layers and remove small regions.
                chf.maxRegions = id;
                if (!MergeAndFilterLayerRegions(minRegionArea, ref chf.maxRegions, chf, srcReg, out int[] overlaps))
                {
                    return false;
                }
            }

            // Store the result out.
            for (int i = 0; i < chf.spanCount; ++i)
            {
                chf.spans[i].reg = srcReg[i];
            }

            return true;
        }
        private static bool MergeAndFilterLayerRegions(int minRegionArea, ref int maxRegionId, CompactHeightfield chf, int[] srcReg, out int[] overlaps)
        {
            overlaps = null;

            int w = chf.width;
            int h = chf.height;

            int nreg = maxRegionId + 1;
            Region[] regions = new Region[nreg];

            // Construct regions
            for (int i = 0; i < nreg; ++i)
            {
                regions[i] = new Region(i);
            }

            // Find region neighbours and overlapping regions.
            List<int> lregs = new List<int>(32);
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.cells[x + y * w];

                    lregs.Clear();

                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        var s = chf.spans[i];
                        int ri = srcReg[i];
                        if (ri == 0 || ri >= nreg)
                        {
                            continue;
                        }
                        var reg = regions[ri];

                        reg.spanCount++;

                        reg.ymin = Math.Min(reg.ymin, s.y);
                        reg.ymax = Math.Max(reg.ymax, s.y);

                        // Collect all region layers.
                        lregs.Add(ri);

                        // Update neighbours
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (GetCon(s, dir) != Constants.NotConnected)
                            {
                                int ax = x + PolyUtils.GetDirOffsetX(dir);
                                int ay = y + PolyUtils.GetDirOffsetY(dir);
                                int ai = chf.cells[ax + ay * w].index + GetCon(s, dir);
                                int rai = srcReg[ai];
                                if (rai > 0 && rai < nreg && rai != ri)
                                {
                                    AddUniqueConnection(reg, rai);
                                }
                                if ((rai & Constants.RC_BORDER_REG) != 0)
                                {
                                    reg.connectsToBorder = true;
                                }
                            }
                        }
                    }

                    // Update overlapping regions.
                    for (int i = 0; i < lregs.Count - 1; ++i)
                    {
                        for (int j = i + 1; j < lregs.Count; ++j)
                        {
                            if (lregs[i] != lregs[j])
                            {
                                var ri = regions[lregs[i]];
                                var rj = regions[lregs[j]];
                                AddUniqueFloorRegion(ri, lregs[j]);
                                AddUniqueFloorRegion(rj, lregs[i]);
                            }
                        }
                    }
                }
            }

            // Create 2D layers from regions.
            int layerId = 1;

            for (int i = 0; i < nreg; ++i)
            {
                regions[i].id = 0;
            }

            // Merge montone regions to create non-overlapping areas.
            List<int> stack = new List<int>(32);
            for (int i = 1; i < nreg; ++i)
            {
                var root = regions[i];
                // Skip already visited.
                if (root.id != 0)
                {
                    continue;
                }

                // Start search.
                root.id = layerId;

                stack.Clear();
                stack.Add(i);

                while (stack.Count > 0)
                {
                    // Pop front
                    var reg = regions[stack[0]];
                    for (int j = 0; j < stack.Count - 1; ++j)
                    {
                        stack[j] = stack[j + 1];
                    }
                    stack.Clear();

                    int ncons = reg.connections.Count;
                    for (int j = 0; j < ncons; ++j)
                    {
                        int nei = reg.connections[j];
                        var regn = regions[nei];
                        // Skip already visited.
                        if (regn.id != 0)
                        {
                            continue;
                        }
                        // Skip if the neighbour is overlapping root region.
                        bool overlap = false;
                        for (int k = 0; k < root.floors.Count; k++)
                        {
                            if (root.floors[k] == nei)
                            {
                                overlap = true;
                                break;
                            }
                        }
                        if (overlap)
                        {
                            continue;
                        }

                        // Deepen
                        stack.Add(nei);

                        // Mark layer id
                        regn.id = layerId;
                        // Merge current layers to root.
                        for (int k = 0; k < regn.floors.Count; ++k)
                        {
                            AddUniqueFloorRegion(root, regn.floors[k]);
                        }
                        root.ymin = Math.Min(root.ymin, regn.ymin);
                        root.ymax = Math.Max(root.ymax, regn.ymax);
                        root.spanCount += regn.spanCount;
                        regn.spanCount = 0;
                        root.connectsToBorder = root.connectsToBorder || regn.connectsToBorder;
                    }
                }

                layerId++;
            }

            // Remove small regions
            for (int i = 0; i < nreg; ++i)
            {
                if (regions[i].spanCount > 0 && regions[i].spanCount < minRegionArea && !regions[i].connectsToBorder)
                {
                    int reg = regions[i].id;
                    for (int j = 0; j < nreg; ++j)
                    {
                        if (regions[j].id == reg)
                        {
                            regions[j].id = 0;
                        }
                    }
                }
            }

            // Compress region Ids.
            for (int i = 0; i < nreg; ++i)
            {
                regions[i].remap = false;
                if (regions[i].id == 0)
                {
                    // Skip nil regions.
                    continue;
                }
                if ((regions[i].id & Constants.RC_BORDER_REG) != 0)
                {
                    // Skip external regions.
                    continue;
                }
                regions[i].remap = true;
            }

            int regIdGen = 0;
            for (int i = 0; i < nreg; ++i)
            {
                if (!regions[i].remap)
                {
                    continue;
                }
                int oldId = regions[i].id;
                int newId = ++regIdGen;
                for (int j = i; j < nreg; ++j)
                {
                    if (regions[j].id == oldId)
                    {
                        regions[j].id = newId;
                        regions[j].remap = false;
                    }
                }
            }
            maxRegionId = regIdGen;

            // Remap regions.
            for (int i = 0; i < chf.spanCount; ++i)
            {
                if ((srcReg[i] & Constants.RC_BORDER_REG) == 0)
                {
                    srcReg[i] = regions[srcReg[i]].id;
                }
            }

            for (int i = 0; i < nreg; ++i)
            {
                regions[i] = null;
            }

            regions = null;

            return true;
        }
        private static void AddUniqueConnection(Region reg, int n)
        {
            for (int i = 0; i < reg.connections.Count; ++i)
            {
                if (reg.connections[i] == n)
                {
                    return;
                }
            }

            reg.connections.Add(n);
        }
        private static void WalkContour(int x, int y, int i, CompactHeightfield chf, int[] flags, out List<Int4> points)
        {
            points = new List<Int4>();

            // Choose the first non-connected edge
            int dir = 0;
            while ((flags[i] & (1 << dir)) == 0)
            {
                dir++;
            }

            int startDir = dir;
            int starti = i;

            var area = chf.areas[i];

            int iter = 0;
            while (++iter < 40000)
            {
                if ((flags[i] & (1 << dir)) != 0)
                {
                    // Choose the edge corner
                    bool isAreaBorder = false;
                    int px = x;
                    int py = GetCornerHeight(x, y, i, dir, chf, out bool isBorderVertex);
                    int pz = y;
                    switch (dir)
                    {
                        case 0: pz++; break;
                        case 1: px++; pz++; break;
                        case 2: px++; break;
                    }
                    int r = 0;
                    var s = chf.spans[i];
                    if (GetCon(s, dir) != Constants.NotConnected)
                    {
                        int ax = x + PolyUtils.GetDirOffsetX(dir);
                        int ay = y + PolyUtils.GetDirOffsetY(dir);
                        int ai = chf.cells[ax + ay * chf.width].index + GetCon(s, dir);
                        r = chf.spans[ai].reg;
                        if (area != chf.areas[ai])
                        {
                            isAreaBorder = true;
                        }
                    }
                    if (isBorderVertex)
                    {
                        r |= Constants.RC_BORDER_VERTEX;
                    }
                    if (isAreaBorder)
                    {
                        r |= Constants.RC_AREA_BORDER;
                    }
                    points.Add(new Int4(px, py, pz, r));

                    flags[i] &= ~(1 << dir); // Remove visited edges
                    dir = (dir + 1) & 0x3;  // Rotate CW
                }
                else
                {
                    int ni = -1;
                    int nx = x + PolyUtils.GetDirOffsetX(dir);
                    int ny = y + PolyUtils.GetDirOffsetY(dir);
                    var s = chf.spans[i];
                    if (GetCon(s, dir) != Constants.NotConnected)
                    {
                        var nc = chf.cells[nx + ny * chf.width];
                        ni = nc.index + GetCon(s, dir);
                    }
                    if (ni == -1)
                    {
                        // Should not happen.
                        return;
                    }
                    x = nx;
                    y = ny;
                    i = ni;
                    dir = (dir + 3) & 0x3;  // Rotate CCW
                }

                if (starti == i && startDir == dir)
                {
                    break;
                }
            }
        }
        private static int GetCornerHeight(int x, int y, int i, int dir, CompactHeightfield chf, out bool isBorderVertex)
        {
            isBorderVertex = false;

            var s = chf.spans[i];
            int ch = s.y;
            int dirp = (dir + 1) & 0x3;

            int[] regs = { 0, 0, 0, 0 };

            // Combine region and area codes in order to prevent
            // border vertices which are in between two areas to be removed.
            regs[0] = chf.spans[i].reg | ((int)chf.areas[i] << 16);

            if (GetCon(s, dir) != Constants.NotConnected)
            {
                int ax = x + PolyUtils.GetDirOffsetX(dir);
                int ay = y + PolyUtils.GetDirOffsetY(dir);
                int ai = chf.cells[ax + ay * chf.width].index + GetCon(s, dir);
                var a = chf.spans[ai];
                ch = Math.Max(ch, a.y);
                regs[1] = chf.spans[ai].reg | ((int)chf.areas[ai] << 16);
                if (GetCon(a, dirp) != Constants.NotConnected)
                {
                    int ax2 = ax + PolyUtils.GetDirOffsetX(dirp);
                    int ay2 = ay + PolyUtils.GetDirOffsetY(dirp);
                    int ai2 = chf.cells[ax2 + ay2 * chf.width].index + GetCon(a, dirp);
                    var as2 = chf.spans[ai2];
                    ch = Math.Max(ch, as2.y);
                    regs[2] = chf.spans[ai2].reg | ((int)chf.areas[ai2] << 16);
                }
            }
            if (GetCon(s, dirp) != Constants.NotConnected)
            {
                int ax = x + PolyUtils.GetDirOffsetX(dirp);
                int ay = y + PolyUtils.GetDirOffsetY(dirp);
                int ai = chf.cells[ax + ay * chf.width].index + GetCon(s, dirp);
                var a = chf.spans[ai];
                ch = Math.Max(ch, a.y);
                regs[3] = chf.spans[ai].reg | ((int)chf.areas[ai] << 16);
                if (GetCon(a, dir) != Constants.NotConnected)
                {
                    int ax2 = ax + PolyUtils.GetDirOffsetX(dir);
                    int ay2 = ay + PolyUtils.GetDirOffsetY(dir);
                    int ai2 = chf.cells[ax2 + ay2 * chf.width].index + GetCon(a, dir);
                    var as2 = chf.spans[ai2];
                    ch = Math.Max(ch, as2.y);
                    regs[2] = chf.spans[ai2].reg | ((int)chf.areas[ai2] << 16);
                }
            }

            // Check if the vertex is special edge vertex, these vertices will be removed later.
            for (int j = 0; j < 4; ++j)
            {
                int a = j;
                int b = (j + 1) & 0x3;
                int c = (j + 2) & 0x3;
                int d = (j + 3) & 0x3;

                // The vertex is a border vertex there are two same exterior cells in a row,
                // followed by two interior cells and none of the regions are out of bounds.
                bool twoSameExts = (regs[a] & regs[b] & Constants.RC_BORDER_REG) != 0 && regs[a] == regs[b];
                bool twoInts = ((regs[c] | regs[d]) & Constants.RC_BORDER_REG) == 0;
                bool intsSameArea = (regs[c] >> 16) == (regs[d] >> 16);
                bool noZeros = regs[a] != 0 && regs[b] != 0 && regs[c] != 0 && regs[d] != 0;
                if (twoSameExts && twoInts && intsSameArea && noZeros)
                {
                    isBorderVertex = true;
                    break;
                }
            }

            return ch;
        }
        private static bool BuildContours(CompactHeightfield chf, float maxError, int maxEdgeLen, BuildContoursFlags buildFlags, out ContourSet cset)
        {
            int w = chf.width;
            int h = chf.height;
            int borderSize = chf.borderSize;

            cset = new ContourSet();
            cset.bmin = chf.boundingBox.Minimum;
            cset.bmax = chf.boundingBox.Maximum;
            if (borderSize > 0)
            {
                // If the heightfield was build with bordersize, remove the offset.
                float pad = borderSize * chf.cs;
                cset.bmin[0] += pad;
                cset.bmin[2] += pad;
                cset.bmax[0] -= pad;
                cset.bmax[2] -= pad;
            }
            cset.cs = chf.cs;
            cset.ch = chf.ch;
            cset.width = chf.width - chf.borderSize * 2;
            cset.height = chf.height - chf.borderSize * 2;
            cset.borderSize = chf.borderSize;
            cset.maxError = maxError;

            int maxContours = Math.Max(chf.maxRegions, 8);
            cset.conts = new Contour[maxContours];
            cset.nconts = 0;

            int[] flags = new int[chf.spanCount];

            // Mark boundaries.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.cells[x + y * w];
                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        int res = 0;
                        var s = chf.spans[i];
                        if (chf.spans[i].reg == 0 || (chf.spans[i].reg & Constants.RC_BORDER_REG) != 0)
                        {
                            flags[i] = 0;
                            continue;
                        }
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            int r = 0;
                            if (GetCon(s, dir) != Constants.NotConnected)
                            {
                                int ax = x + PolyUtils.GetDirOffsetX(dir);
                                int ay = y + PolyUtils.GetDirOffsetY(dir);
                                int ai = chf.cells[ax + ay * w].index + GetCon(s, dir);
                                r = chf.spans[ai].reg;
                            }
                            if (r == chf.spans[i].reg)
                            {
                                res |= (1 << dir);
                            }
                        }
                        flags[i] = res ^ 0xf; // Inverse, mark non connected edges.
                    }
                }
            }

            List<Int4> verts = new List<Int4>();
            List<Int4> simplified = new List<Int4>();

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.cells[x + y * w];
                    for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                    {
                        if (flags[i] == 0 || flags[i] == 0xf)
                        {
                            flags[i] = 0;
                            continue;
                        }
                        int reg = chf.spans[i].reg;
                        if (reg == 0 || (reg & Constants.RC_BORDER_REG) != 0)
                        {
                            continue;
                        }
                        var area = chf.areas[i];

                        verts.Clear();
                        simplified.Clear();

                        WalkContour(x, y, i, chf, flags, out verts);

                        SimplifyContour(verts, simplified, maxError, maxEdgeLen, buildFlags);
                        RemoveDegenerateSegments(simplified);

                        // Store region->contour remap info.
                        // Create contour.
                        if (simplified.Count >= 3)
                        {
                            if (cset.nconts >= maxContours)
                            {
                                // Allocate more contours.
                                // This happens when a region has holes.
                                Contour[] newConts = new Contour[maxContours * 2];
                                for (int j = 0; j < cset.nconts; ++j)
                                {
                                    newConts[j] = cset.conts[j];
                                }
                                cset.conts = newConts;
                            }

                            var cont = new Contour
                            {
                                nverts = simplified.Count,
                                verts = simplified.ToArray(),
                                nrverts = verts.Count,
                                rverts = verts.ToArray(),
                                reg = reg,
                                area = area
                            };

                            if (borderSize > 0)
                            {
                                // If the heightfield was build with bordersize, remove the offset.
                                for (int j = 0; j < cont.nverts; ++j)
                                {
                                    var v = cont.verts[j];
                                    v.X -= borderSize;
                                    v.Z -= borderSize;
                                    cont.verts[j] = v;
                                }

                                // If the heightfield was build with bordersize, remove the offset.
                                for (int j = 0; j < cont.nrverts; ++j)
                                {
                                    var v = cont.rverts[j];
                                    v.X -= borderSize;
                                    v.Z -= borderSize;
                                    cont.rverts[j] = v;
                                }
                            }

                            cset.conts[cset.nconts++] = cont;
                        }
                    }
                }
            }

            // Merge holes if needed.
            if (cset.nconts > 0)
            {
                // Calculate winding of all polygons.
                int[] winding = new int[cset.nconts];
                int nholes = 0;
                for (int i = 0; i < cset.nconts; ++i)
                {
                    var cont = cset.conts[i];
                    // If the contour is wound backwards, it is a hole.
                    winding[i] = PolyUtils.CalcAreaOfPolygon2D(cont.verts, cont.nverts) < 0 ? -1 : 1;
                    if (winding[i] < 0)
                    {
                        nholes++;
                    }
                }

                if (nholes > 0)
                {
                    // Collect outline contour and holes contours per region.
                    // We assume that there is one outline and multiple holes.
                    int nregions = chf.maxRegions + 1;
                    var regions = Helper.CreateArray(nregions, () => { return new ContourRegion(); });
                    var holes = Helper.CreateArray(cset.nconts, () => { return new ContourHole(); });

                    for (int i = 0; i < cset.nconts; ++i)
                    {
                        var cont = cset.conts[i];
                        // Positively would contours are outlines, negative holes.
                        if (winding[i] > 0)
                        {
                            if (regions[cont.reg].outline != null)
                            {
                                //ctx->log(RC_LOG_ERROR, "rcBuildContours: Multiple outlines for region %d.", cont.reg);
                            }
                            regions[cont.reg].outline = cont;
                        }
                        else
                        {
                            regions[cont.reg].nholes++;
                        }
                    }
                    int index = 0;
                    for (int i = 0; i < nregions; i++)
                    {
                        if (regions[i].nholes > 0)
                        {
                            regions[i].holes = new[] { holes[index] };
                            index += regions[i].nholes;
                            regions[i].nholes = 0;
                        }
                    }
                    for (int i = 0; i < cset.nconts; ++i)
                    {
                        var cont = cset.conts[i];
                        var reg = regions[cont.reg];
                        if (winding[i] < 0)
                        {
                            reg.holes[reg.nholes++].contour = cont;
                        }
                    }

                    // Finally merge each regions holes into the outline.
                    for (int i = 0; i < nregions; i++)
                    {
                        var reg = regions[i];
                        if (reg.nholes == 0)
                        {
                            continue;
                        }

                        if (reg.outline != null)
                        {
                            MergeRegionHoles(reg);
                        }
                        else
                        {
                            // The region does not have an outline.
                            // This can happen if the contour becaomes selfoverlapping because of
                            // too aggressive simplification settings.
                            //ctx->log(RC_LOG_ERROR, "rcBuildContours: Bad outline for region %d, contour simplification is likely too aggressive.", i);
                        }
                    }
                }

            }

            return true;
        }
        private static void SimplifyContour(List<Int4> points, List<Int4> simplified, float maxError, int maxEdgeLen, BuildContoursFlags buildFlags)
        {
            // Add initial points.
            bool hasConnections = false;
            for (int i = 0; i < points.Count; i++)
            {
                if ((points[i].W & Constants.RC_CONTOUR_REG_MASK) != 0)
                {
                    hasConnections = true;
                    break;
                }
            }

            if (hasConnections)
            {
                // The contour has some portals to other regions.
                // Add a new point to every location where the region changes.
                for (int i = 0, ni = points.Count; i < ni; ++i)
                {
                    int ii = (i + 1) % ni;
                    bool differentRegs = (points[i].W & Constants.RC_CONTOUR_REG_MASK) != (points[ii].W & Constants.RC_CONTOUR_REG_MASK);
                    bool areaBorders = (points[i].W & Constants.RC_AREA_BORDER) != (points[ii].W & Constants.RC_AREA_BORDER);
                    if (differentRegs || areaBorders)
                    {
                        simplified.Add(new Int4(points[i].X, points[i].Y, points[i].Z, i));
                    }
                }
            }

            if (simplified.Count == 0)
            {
                // If there is no connections at all,
                // create some initial points for the simplification process.
                // Find lower-left and upper-right vertices of the contour.
                int llx = points[0].X;
                int lly = points[0].Y;
                int llz = points[0].Z;
                int lli = 0;
                int urx = points[0].X;
                int ury = points[0].Y;
                int urz = points[0].Z;
                int uri = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    int x = points[i].X;
                    int y = points[i].Y;
                    int z = points[i].Z;
                    if (x < llx || (x == llx && z < llz))
                    {
                        llx = x;
                        lly = y;
                        llz = z;
                        lli = i;
                    }
                    if (x > urx || (x == urx && z > urz))
                    {
                        urx = x;
                        ury = y;
                        urz = z;
                        uri = i;
                    }
                }
                simplified.Add(new Int4(llx, lly, llz, lli));
                simplified.Add(new Int4(urx, ury, urz, uri));
            }

            // Add points until all raw points are within
            // error tolerance to the simplified shape.
            int pn = points.Count;
            for (int i = 0; i < simplified.Count;)
            {
                int ii = (i + 1) % (simplified.Count);

                int ax = simplified[i].X;
                int az = simplified[i].Z;
                int ai = simplified[i].W;

                int bx = simplified[ii].X;
                int bz = simplified[ii].Z;
                int bi = simplified[ii].W;

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
                    ci = (ai + cinc) % pn;
                    endi = bi;
                }
                else
                {
                    cinc = pn - 1;
                    ci = (bi + cinc) % pn;
                    endi = ai;
                    Helper.Swap(ref ax, ref bx);
                    Helper.Swap(ref az, ref bz);
                }

                // Tessellate only outer edges or edges between areas.
                if ((points[ci].W & Constants.RC_CONTOUR_REG_MASK) == 0 ||
                    (points[ci].W & Constants.RC_AREA_BORDER) != 0)
                {
                    while (ci != endi)
                    {
                        float d = PolyUtils.DistancePtSeg(points[ci].X, points[ci].Z, ax, az, bx, bz);
                        if (d > maxd)
                        {
                            maxd = d;
                            maxi = ci;
                        }
                        ci = (ci + cinc) % pn;
                    }
                }

                // If the max deviation is larger than accepted error,
                // add new point, else continue to next segment.
                if (maxi != -1 && maxd > (maxError * maxError))
                {
                    // Add the point.
                    simplified.Insert(i + 1, new Int4(points[maxi].X, points[maxi].Y, points[maxi].Z, maxi));
                }
                else
                {
                    ++i;
                }
            }

            // Split too long edges.
            if (maxEdgeLen > 0 && (buildFlags & (BuildContoursFlags.RC_CONTOUR_TESS_WALL_EDGES | BuildContoursFlags.RC_CONTOUR_TESS_AREA_EDGES)) != 0)
            {
                for (int i = 0; i < simplified.Count;)
                {
                    int ii = (i + 1) % (simplified.Count);

                    int ax = simplified[i].X;
                    int az = simplified[i].Z;
                    int ai = simplified[i].W;

                    int bx = simplified[ii].X;
                    int bz = simplified[ii].Z;
                    int bi = simplified[ii].W;

                    // Find maximum deviation from the segment.
                    int maxi = -1;
                    int ci = (ai + 1) % pn;

                    // Tessellate only outer edges or edges between areas.
                    bool tess = false;
                    // Wall edges.
                    if ((buildFlags & BuildContoursFlags.RC_CONTOUR_TESS_WALL_EDGES) != 0 &&
                        (points[ci].W & Constants.RC_CONTOUR_REG_MASK) == 0)
                    {
                        tess = true;
                    }
                    // Edges between areas.
                    if ((buildFlags & BuildContoursFlags.RC_CONTOUR_TESS_AREA_EDGES) != 0 &&
                        (points[ci].W & Constants.RC_AREA_BORDER) != 0)
                    {
                        tess = true;
                    }

                    if (tess)
                    {
                        int dx = bx - ax;
                        int dz = bz - az;
                        if (dx * dx + dz * dz > maxEdgeLen * maxEdgeLen)
                        {
                            // Round based on the segments in lexilogical order so that the
                            // max tesselation is consistent regardles in which direction
                            // segments are traversed.
                            int n = bi < ai ? (bi + pn - ai) : (bi - ai);
                            if (n > 1)
                            {
                                if (bx > ax || (bx == ax && bz > az))
                                {
                                    maxi = (ai + n / 2) % pn;
                                }
                                else
                                {
                                    maxi = (ai + (n + 1) / 2) % pn;
                                }
                            }
                        }
                    }

                    // If the max deviation is larger than accepted error,
                    // add new point, else continue to next segment.
                    if (maxi != -1)
                    {
                        // Add the point.
                        simplified.Insert(i + 1, new Int4(points[maxi].X, points[maxi].Y, points[maxi].Z, maxi));
                    }
                    else
                    {
                        ++i;
                    }
                }
            }

            for (int i = 0; i < simplified.Count; ++i)
            {
                // The edge vertex flag is take from the current raw point,
                // and the neighbour region is take from the next raw point.
                var sv = simplified[i];
                int ai = (sv.W + 1) % pn;
                int bi = sv.W;
                sv.W = (points[ai].W & (Constants.RC_CONTOUR_REG_MASK | Constants.RC_AREA_BORDER)) | (points[bi].W & Constants.RC_BORDER_VERTEX);
                simplified[i] = sv;
            }
        }
        private static void RemoveDegenerateSegments(List<Int4> simplified)
        {
            // Remove adjacent vertices which are equal on xz-plane,
            // or else the triangulator will get confused.
            int npts = simplified.Count;
            for (int i = 0; i < npts; ++i)
            {
                int ni = Helper.Next(i, npts);

                if (simplified[i] == simplified[ni])
                {
                    // Degenerate segment, remove.
                    for (int j = i; j < simplified.Count - 1; ++j)
                    {
                        simplified[j] = simplified[(j + 1)];
                    }
                    simplified.Clear();
                    npts--;
                }
            }
        }
        private static void MergeRegionHoles(ContourRegion region)
        {
            // Sort holes from left to right.
            for (int i = 0; i < region.nholes; i++)
            {
                FindLeftMostVertex(region.holes[i].contour, ref region.holes[i].minx, ref region.holes[i].minz, ref region.holes[i].leftmost);
            }

            Array.Sort(region.holes, (va, vb) =>
            {
                var a = va;
                var b = vb;
                if (a.minx == b.minx)
                {
                    if (a.minz < b.minz) return -1;
                    if (a.minz > b.minz) return 1;
                }
                else
                {
                    if (a.minx < b.minx) return -1;
                    if (a.minx > b.minx) return 1;
                }
                return 0;
            });

            int maxVerts = region.outline.nverts;
            for (int i = 0; i < region.nholes; i++)
            {
                maxVerts += region.holes[i].contour.nverts;
            }

            PotentialDiagonal[] diags = Helper.CreateArray(maxVerts, new PotentialDiagonal()
            {
                dist = int.MinValue,
                vert = int.MinValue,
            });

            var outline = region.outline;

            // Merge holes into the outline one by one.
            for (int i = 0; i < region.nholes; i++)
            {
                var hole = region.holes[i].contour;

                int index = -1;
                int bestVertex = region.holes[i].leftmost;
                for (int iter = 0; iter < hole.nverts; iter++)
                {
                    // Find potential diagonals.
                    // The 'best' vertex must be in the cone described by 3 cosequtive vertices of the outline.
                    // ..o j-1
                    //   |
                    //   |   * best
                    //   |
                    // j o-----o j+1
                    //         :
                    int ndiags = 0;
                    var corner = hole.verts[bestVertex];
                    for (int j = 0; j < outline.nverts; j++)
                    {
                        if (PolyUtils.InCone(j, outline.nverts, outline.verts, corner))
                        {
                            int dx = outline.verts[j].X - corner.X;
                            int dz = outline.verts[j].Z - corner.Z;
                            diags[ndiags].vert = j;
                            diags[ndiags].dist = dx * dx + dz * dz;
                            ndiags++;
                        }
                    }
                    // Sort potential diagonals by distance, we want to make the connection as short as possible.
                    Array.Sort(diags, 0, ndiags, PotentialDiagonal.DefaultComparer);

                    // Find a diagonal that is not intersecting the outline not the remaining holes.
                    index = -1;
                    for (int j = 0; j < ndiags; j++)
                    {
                        var pt = outline.verts[diags[j].vert];
                        bool intersect = IntersectSegCountour(pt, corner, diags[i].vert, outline.nverts, outline.verts);
                        for (int k = i; k < region.nholes && !intersect; k++)
                        {
                            intersect |= IntersectSegCountour(pt, corner, -1, region.holes[k].contour.nverts, region.holes[k].contour.verts);
                        }
                        if (!intersect)
                        {
                            index = diags[j].vert;
                            break;
                        }
                    }
                    // If found non-intersecting diagonal, stop looking.
                    if (index != -1)
                    {
                        break;
                    }
                    // All the potential diagonals for the current vertex were intersecting, try next vertex.
                    bestVertex = (bestVertex + 1) % hole.nverts;
                }

                if (index == -1)
                {
                    //ctx->log(RC_LOG_WARNING, "mergeHoles: Failed to find merge points for %p and %p.", region.outline, hole);
                    continue;
                }
                if (!MergeContours(region.outline, hole, index, bestVertex))
                {
                    //ctx->log(RC_LOG_WARNING, "mergeHoles: Failed to merge contours %p and %p.", region.outline, hole);
                    continue;
                }
            }
        }
        private static void FindLeftMostVertex(Contour contour, ref int minx, ref int minz, ref int leftmost)
        {
            minx = contour.verts[0].X;
            minz = contour.verts[0].Z;
            leftmost = 0;
            for (int i = 1; i < contour.nverts; i++)
            {
                int x = contour.verts[i].X;
                int z = contour.verts[i].Z;
                if (x < minx || (x == minx && z < minz))
                {
                    minx = x;
                    minz = z;
                    leftmost = i;
                }
            }
        }
        private static bool IntersectSegCountour(Int4 d0, Int4 d1, int i, int n, Int4[] verts)
        {
            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = Helper.Next(k, n);
                // Skip edges incident to i.
                if (i == k || i == k1)
                {
                    continue;
                }
                var p0 = verts[k];
                var p1 = verts[k1];
                if (d0 == p0 || d1 == p0 || d0 == p1 || d1 == p1)
                {
                    continue;
                }

                if (PolyUtils.Intersect(d0, d1, p0, p1))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool MergeContours(Contour ca, Contour cb, int ia, int ib)
        {
            int maxVerts = ca.nverts + cb.nverts + 2;
            Int4[] verts = new Int4[maxVerts];

            int nv = 0;

            // Copy contour A.
            for (int i = 0; i <= ca.nverts; ++i)
            {
                verts[nv++] = ca.verts[((ia + i) % ca.nverts)];
            }

            // Copy contour B
            for (int i = 0; i <= cb.nverts; ++i)
            {
                verts[nv++] = cb.verts[((ib + i) % cb.nverts)];
            }

            ca.verts = verts;
            ca.nverts = nv;

            cb.verts = null;
            cb.nverts = 0;

            return true;
        }
        private static bool BuildPolyMesh(ContourSet cset, int nvp, out PolyMesh mesh)
        {
            mesh = new PolyMesh
            {
                bmin = cset.bmin,
                bmax = cset.bmax,
                cs = cset.cs,
                ch = cset.ch,
                borderSize = cset.borderSize,
                maxEdgeError = cset.maxError
            };

            int maxVertices = 0;
            int maxTris = 0;
            int maxVertsPerCont = 0;
            for (int i = 0; i < cset.nconts; ++i)
            {
                // Skip null contours.
                if (cset.conts[i].nverts < 3) continue;
                maxVertices += cset.conts[i].nverts;
                maxTris += cset.conts[i].nverts - 2;
                maxVertsPerCont = Math.Max(maxVertsPerCont, cset.conts[i].nverts);
            }

            if (maxVertices >= 0xfffe)
            {
                throw new EngineException(string.Format("rcBuildPolyMesh: Too many vertices {0}.", maxVertices));
            }

            int[] vflags = new int[maxVertices];

            mesh.verts = new Int3[maxVertices];
            mesh.polys = new Polygoni[maxTris];
            mesh.regs = new int[maxTris];
            mesh.areas = new SamplePolyAreas[maxTris];

            mesh.nverts = 0;
            mesh.npolys = 0;
            mesh.nvp = nvp;
            mesh.maxpolys = maxTris;

            int[] nextVert = Helper.CreateArray(maxVertices, 0);
            int[] firstVert = Helper.CreateArray(Constants.VERTEX_BUCKET_COUNT, Constants.NullIdx);
            int[] indices = new int[maxVertsPerCont];
            Polygoni tmpPoly = new Polygoni(maxVertsPerCont);

            for (int i = 0; i < cset.nconts; ++i)
            {
                var cont = cset.conts[i];

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
                    // Bad triangulation, should not happen.
                    /*			printf("\tconst float bmin[3] = {%ff,%ff,%ff};\n", cset.bmin[0], cset.bmin[1], cset.bmin[2]);
                                printf("\tconst float cs = %ff;\n", cset.cs);
                                printf("\tconst float ch = %ff;\n", cset.ch);
                                printf("\tconst int verts[] = {\n");
                                for (int k = 0; k < cont.nverts; ++k)
                                {
                                    const int* v = &cont.verts[k*4];
                                    printf("\t\t%d,%d,%d,%d,\n", v[0], v[1], v[2], v[3]);
                                }
                                printf("\t};\n\tconst int nverts = sizeof(verts)/(sizeof(int)*4);\n");*/
                    //ctx->log(RC_LOG_WARNING, "rcBuildPolyMesh: Bad triangulation Contour %d.", i);
                    ntris = -ntris;
                }

                // Add and merge vertices.
                for (int j = 0; j < cont.nverts; ++j)
                {
                    var v = cont.verts[j];
                    indices[j] = PolyUtils.AddVertex(v.X, v.Y, v.Z, mesh.verts, firstVert, nextVert, ref mesh.nverts);
                    if ((v.W & Constants.RC_BORDER_VERTEX) != 0)
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
                if (nvp > 3)
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
                    var p = new Polygoni(nvp * 2); //Polygon with adjacency
                    var q = polys[j];
                    for (int k = 0; k < nvp; ++k)
                    {
                        p[k] = q[k];
                    }
                    mesh.polys[mesh.npolys] = p;
                    mesh.regs[mesh.npolys] = cont.reg;
                    mesh.areas[mesh.npolys] = (SamplePolyAreas)(int)cont.area;
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
            if (!PolyUtils.BuildMeshAdjacency(mesh.polys, mesh.npolys, mesh.nverts, nvp))
            {
                throw new EngineException("Adjacency failed.");
            }

            // Find portal edges
            if (mesh.borderSize > 0)
            {
                int w = cset.width;
                int h = cset.height;
                for (int i = 0; i < mesh.npolys; ++i)
                {
                    var p = mesh.polys[i];
                    for (int j = 0; j < nvp; ++j)
                    {
                        if (p[j] == Constants.RC_MESH_NULL_IDX)
                        {
                            break;
                        }
                        // Skip connected edges.
                        if (p[nvp + j] != Constants.RC_MESH_NULL_IDX)
                        {
                            continue;
                        }
                        int nj = j + 1;
                        if (nj >= nvp || p[nj] == Constants.RC_MESH_NULL_IDX)
                        {
                            nj = 0;
                        }
                        var va = mesh.verts[p[j]];
                        var vb = mesh.verts[p[nj]];

                        if (va.X == 0 && vb.X == 0)
                        {
                            p[nvp + j] = 0x8000 | 0;
                        }
                        else if (va.Z == h && vb.Z == h)
                        {
                            p[nvp + j] = 0x8000 | 1;
                        }
                        else if (va.X == w && vb.X == w)
                        {
                            p[nvp + j] = 0x8000 | 2;
                        }
                        else if (va.Z == 0 && vb.Z == 0)
                        {
                            p[nvp + j] = 0x8000 | 3;
                        }
                    }
                }
            }

            // Just allocate the mesh flags array. The user is resposible to fill it.
            mesh.flags = new SamplePolyFlags[mesh.npolys];

            if (mesh.nverts > 0xffff)
            {
                throw new EngineException(string.Format("The resulting mesh has too many vertices {0} (max {1}). Data can be corrupted.", mesh.nverts, 0xffff));
            }
            if (mesh.npolys > 0xffff)
            {
                throw new EngineException(string.Format("The resulting mesh has too many polygons {0} (max {1}). Data can be corrupted.", mesh.npolys, 0xffff));
            }

            return true;
        }
        private static bool BuildPolyMeshDetail(PolyMesh mesh, CompactHeightfield chf, float sampleDist, float sampleMaxError, out PolyMeshDetail dmesh)
        {
            dmesh = null;

            if (mesh.nverts == 0 || mesh.npolys == 0)
            {
                return true;
            }

            int nvp = mesh.nvp;
            float cs = mesh.cs;
            float ch = mesh.ch;
            Vector3 orig = mesh.bmin;
            int borderSize = mesh.borderSize;
            int heightSearchRadius = Math.Max(1, (int)Math.Ceiling(mesh.maxEdgeError));

            List<int> arr = new List<int>(512);
            Vector3[] verts = new Vector3[256];
            HeightPatch hp = new HeightPatch();
            int nPolyVerts = 0;
            int maxhw = 0, maxhh = 0;

            Int4[] bounds = new Int4[mesh.npolys];
            Vector3[] poly = new Vector3[nvp];

            // Find max size for a polygon area.
            for (int i = 0; i < mesh.npolys; ++i)
            {
                var p = mesh.polys[i];
                int xmin = chf.width;
                int xmax = 0;
                int ymin = chf.height;
                int ymax = 0;
                for (int j = 0; j < nvp; ++j)
                {
                    if (p[j] == Constants.RC_MESH_NULL_IDX) break;
                    var v = mesh.verts[p[j]];
                    xmin = Math.Min(xmin, v.X);
                    xmax = Math.Max(xmax, v.X);
                    ymin = Math.Min(ymin, v.Z);
                    ymax = Math.Max(ymax, v.Z);
                    nPolyVerts++;
                }
                xmin = Math.Max(0, xmin - 1);
                xmax = Math.Min(chf.width, xmax + 1);
                ymin = Math.Max(0, ymin - 1);
                ymax = Math.Min(chf.height, ymax + 1);
                bounds[i] = new Int4(xmin, xmax, ymin, ymax);
                if (xmin >= xmax || ymin >= ymax) continue;
                maxhw = Math.Max(maxhw, xmax - xmin);
                maxhh = Math.Max(maxhh, ymax - ymin);
            }

            hp.data = new int[maxhw * maxhh];

            int vcap = nPolyVerts + nPolyVerts / 2;
            int tcap = vcap * 2;

            dmesh = new PolyMeshDetail
            {
                nmeshes = mesh.npolys,
                meshes = new Int4[mesh.npolys],
                ntris = 0,
                tris = new Int4[tcap],
                nverts = 0,
                verts = new Vector3[vcap]
            };

            for (int i = 0; i < mesh.npolys; ++i)
            {
                var p = mesh.polys[i];

                // Store polygon vertices for processing.
                int npoly = 0;
                for (int j = 0; j < nvp; ++j)
                {
                    if (p[j] == Constants.RC_MESH_NULL_IDX) break;
                    var v = mesh.verts[p[j]];
                    poly[j].X = v.X * cs;
                    poly[j].Y = v.Y * ch;
                    poly[j].Z = v.Z * cs;
                    npoly++;
                }

                // Get the height data from the area of the polygon.
                hp.xmin = bounds[i].X;
                hp.ymin = bounds[i].Z;
                hp.width = bounds[i].Y - bounds[i].X;
                hp.height = bounds[i].W - bounds[i].Z;
                GetHeightData(chf, p, npoly, mesh.verts, borderSize, hp, arr, mesh.regs[i]);

                // Build detail mesh.
                if (!BuildPolyDetail(
                    poly, npoly,
                    sampleDist, sampleMaxError,
                    heightSearchRadius, chf, hp,
                    verts, out int nverts, out Int4[] tris))
                {
                    return false;
                }

                // Move detail verts to world space.
                for (int j = 0; j < nverts; ++j)
                {
                    verts[j].X += orig.X;
                    verts[j].Y += orig.Y + chf.ch; // Is this offset necessary?
                    verts[j].Z += orig.Z;
                }
                // Offset poly too, will be used to flag checking.
                for (int j = 0; j < npoly; ++j)
                {
                    poly[j].X += orig.X;
                    poly[j].Y += orig.Y;
                    poly[j].Z += orig.Z;
                }

                // Store detail submesh.
                int ntris = tris.Length;

                dmesh.meshes[i].X = dmesh.nverts;
                dmesh.meshes[i].Y = nverts;
                dmesh.meshes[i].Z = dmesh.ntris;
                dmesh.meshes[i].W = ntris;

                // Store vertices, allocate more memory if necessary.
                if (dmesh.nverts + nverts > vcap)
                {
                    while (dmesh.nverts + nverts > vcap)
                    {
                        vcap += 256;
                    }

                    Vector3[] newv = new Vector3[vcap];
                    if (dmesh.nverts != 0)
                    {
                        Array.Copy(dmesh.verts, newv, dmesh.nverts);
                    }
                    dmesh.verts = newv;
                }
                for (int j = 0; j < nverts; ++j)
                {
                    dmesh.verts[dmesh.nverts].X = verts[j].X;
                    dmesh.verts[dmesh.nverts].Y = verts[j].Y;
                    dmesh.verts[dmesh.nverts].Z = verts[j].Z;
                    dmesh.nverts++;
                }

                // Store triangles, allocate more memory if necessary.
                if (dmesh.ntris + ntris > tcap)
                {
                    while (dmesh.ntris + ntris > tcap)
                    {
                        tcap += 256;
                    }
                    Int4[] newt = new Int4[tcap];
                    if (dmesh.ntris != 0)
                    {
                        Array.Copy(dmesh.tris, newt, dmesh.ntris);
                    }
                    dmesh.tris = newt;
                }
                for (int j = 0; j < ntris; ++j)
                {
                    var t = tris[j];
                    dmesh.tris[dmesh.ntris].X = t.X;
                    dmesh.tris[dmesh.ntris].Y = t.Y;
                    dmesh.tris[dmesh.ntris].Z = t.Z;
                    dmesh.tris[dmesh.ntris].W = PolyUtils.GetTriFlags(verts[t.X], verts[t.Y], verts[t.Z], poly, npoly);
                    dmesh.ntris++;
                }
            }

            return true;
        }
        private static void GetHeightData(CompactHeightfield chf, Polygoni poly, int npoly, Int3[] verts, int bs, HeightPatch hp, List<int> queue, int region)
        {
            // Note: Reads to the compact heightfield are offset by border size (bs)
            // since border size offset is already removed from the polymesh vertices.

            queue.Clear();
            // Set all heights to RC_UNSET_HEIGHT.
            hp.data = Helper.CreateArray(hp.width * hp.height, Constants.RC_UNSET_HEIGHT);

            bool empty = true;

            // We cannot sample from this poly if it was created from polys
            // of different regions. If it was then it could potentially be overlapping
            // with polys of that region and the heights sampled here could be wrong.
            if (region != Constants.RC_MULTIPLE_REGS)
            {
                // Copy the height from the same region, and mark region borders
                // as seed points to fill the rest.
                for (int hy = 0; hy < hp.height; hy++)
                {
                    int y = hp.ymin + hy + bs;
                    for (int hx = 0; hx < hp.width; hx++)
                    {
                        int x = hp.xmin + hx + bs;
                        var c = chf.cells[x + y * chf.width];
                        for (int i = c.index, ni = (c.index + c.count); i < ni; ++i)
                        {
                            var s = chf.spans[i];
                            if (s.reg == region)
                            {
                                // Store height
                                hp.data[hx + hy * hp.width] = s.y;
                                empty = false;

                                // If any of the neighbours is not in same region,
                                // add the current location as flood fill start
                                bool border = false;
                                for (int dir = 0; dir < 4; ++dir)
                                {
                                    if (GetCon(s, dir) != Constants.NotConnected)
                                    {
                                        int ax = x + PolyUtils.GetDirOffsetX(dir);
                                        int ay = y + PolyUtils.GetDirOffsetY(dir);
                                        int ai = chf.cells[ax + ay * chf.width].index + GetCon(s, dir);
                                        var a = chf.spans[ai];
                                        if (a.reg != region)
                                        {
                                            border = true;
                                            break;
                                        }
                                    }
                                }
                                if (border)
                                {
                                    PolyUtils.Push3(queue, x, y, i);
                                }
                                break;
                            }
                        }
                    }
                }
            }

            // if the polygon does not contain any points from the current region (rare, but happens)
            // or if it could potentially be overlapping polygons of the same region,
            // then use the center as the seed point.
            if (empty)
            {
                SeedArrayWithPolyCenter(chf, poly, npoly, verts, bs, hp, queue);
            }

            int RETRACT_SIZE = 256;
            int head = 0;

            // We assume the seed is centered in the polygon, so a BFS to collect
            // height data will ensure we do not move onto overlapping polygons and
            // sample wrong heights.
            while (head * 3 < queue.Count)
            {
                int cx = queue[head * 3 + 0];
                int cy = queue[head * 3 + 1];
                int ci = queue[head * 3 + 2];
                head++;
                if (head >= RETRACT_SIZE)
                {
                    head = 0;
                    if (queue.Count > RETRACT_SIZE * 3)
                    {
                        queue.RemoveRange(0, RETRACT_SIZE * 3);
                    }
                    queue.Clear();
                }

                var cs = chf.spans[ci];
                for (int dir = 0; dir < 4; ++dir)
                {
                    if (GetCon(cs, dir) == Constants.NotConnected) continue;

                    int ax = cx + PolyUtils.GetDirOffsetX(dir);
                    int ay = cy + PolyUtils.GetDirOffsetY(dir);
                    int hx = ax - hp.xmin - bs;
                    int hy = ay - hp.ymin - bs;

                    if (hx < 0 || hy < 0 || hx >= hp.width || hy >= hp.height)
                    {
                        continue;
                    }

                    if (hp.data[hx + hy * hp.width] != Constants.RC_UNSET_HEIGHT)
                    {
                        continue;
                    }

                    int ai = chf.cells[ax + ay * chf.width].index + GetCon(cs, dir);
                    var a = chf.spans[ai];

                    hp.data[hx + hy * hp.width] = a.y;

                    PolyUtils.Push3(queue, ax, ay, ai);
                }
            }
        }
        private static void SeedArrayWithPolyCenter(CompactHeightfield chf, Polygoni poly, int npoly, Int3[] verts, int bs, HeightPatch hp, List<int> array)
        {
            // Note: Reads to the compact heightfield are offset by border size (bs)
            // since border size offset is already removed from the polymesh vertices.

            int[] offset =
            {
                0,0, -1,-1, 0,-1, 1,-1, 1,0, 1,1, 0,1, -1,1, -1,0,
            };

            // Find cell closest to a poly vertex
            int startCellX = 0, startCellY = 0, startSpanIndex = -1;
            int dmin = Constants.RC_UNSET_HEIGHT;
            for (int j = 0; j < npoly && dmin > 0; ++j)
            {
                for (int k = 0; k < 9 && dmin > 0; ++k)
                {
                    int ax = verts[poly[j]][0] + offset[k * 2 + 0];
                    int ay = verts[poly[j]][1];
                    int az = verts[poly[j]][2] + offset[k * 2 + 1];
                    if (ax < hp.xmin || ax >= hp.xmin + hp.width ||
                        az < hp.ymin || az >= hp.ymin + hp.height)
                    {
                        continue;
                    }

                    var c = chf.cells[(ax + bs) + (az + bs) * chf.width];
                    for (int i = c.index, ni = (c.index + c.count); i < ni && dmin > 0; ++i)
                    {
                        var s = chf.spans[i];
                        int d = Math.Abs(ay - s.y);
                        if (d < dmin)
                        {
                            startCellX = ax;
                            startCellY = az;
                            startSpanIndex = i;
                            dmin = d;
                        }
                    }
                }
            }

            // Find center of the polygon
            int pcx = 0, pcy = 0;
            for (int j = 0; j < npoly; ++j)
            {
                pcx += verts[poly[j]][0];
                pcy += verts[poly[j]][2];
            }
            pcx /= npoly;
            pcy /= npoly;

            // Use seeds array as a stack for DFS
            array.Clear();
            array.Add(startCellX);
            array.Add(startCellY);
            array.Add(startSpanIndex);

            int[] dirs = { 0, 1, 2, 3 };
            hp.data = Helper.CreateArray(hp.width * hp.height, 0);
            // DFS to move to the center. Note that we need a DFS here and can not just move
            // directly towards the center without recording intermediate nodes, even though the polygons
            // are convex. In very rare we can get stuck due to contour simplification if we do not
            // record nodes.
            int cx = -1, cy = -1, ci = -1;
            while (true)
            {
                if (array.Count < 3)
                {
                    //ctx->log(RC_LOG_WARNING, "Walk towards polygon center failed to reach center");
                    break;
                }

                ci = array.Pop();
                cy = array.Pop();
                cx = array.Pop();

                if (cx == pcx && cy == pcy)
                {
                    break;
                }

                // If we are already at the correct X-position, prefer direction
                // directly towards the center in the Y-axis; otherwise prefer
                // direction in the X-axis
                int directDir;
                if (cx == pcx)
                {
                    directDir = PolyUtils.GetDirForOffset(0, pcy > cy ? 1 : -1);
                }
                else
                {
                    directDir = PolyUtils.GetDirForOffset(pcx > cx ? 1 : -1, 0);
                }

                // Push the direct dir last so we start with this on next iteration
                Helper.Swap(ref dirs[directDir], ref dirs[3]);

                var cs = chf.spans[ci];
                for (int i = 0; i < 4; i++)
                {
                    int dir = dirs[i];
                    if (GetCon(cs, dir) == Constants.NotConnected)
                    {
                        continue;
                    }

                    int newX = cx + PolyUtils.GetDirOffsetX(dir);
                    int newY = cy + PolyUtils.GetDirOffsetY(dir);

                    int hpx = newX - hp.xmin;
                    int hpy = newY - hp.ymin;
                    if (hpx < 0 || hpx >= hp.width || hpy < 0 || hpy >= hp.height)
                    {
                        continue;
                    }

                    if (hp.data[hpx + hpy * hp.width] != 0)
                    {
                        continue;
                    }

                    hp.data[hpx + hpy * hp.width] = 1;
                    array.Add(newX);
                    array.Add(newY);
                    array.Add(chf.cells[(newX + bs) + (newY + bs) * chf.width].index + GetCon(cs, dir));
                }

                Helper.Swap(ref dirs[directDir], ref dirs[3]);
            }

            array.Clear();
            // getHeightData seeds are given in coordinates with borders
            array.Add(cx + bs);
            array.Add(cy + bs);
            array.Add(ci);

            hp.data = Helper.CreateArray(hp.width * hp.height, 0xff);
            var chs = chf.spans[ci];
            hp.data[cx - hp.xmin + (cy - hp.ymin) * hp.width] = chs.y;
        }
        private static bool BuildPolyDetail(Vector3[] inp, int ninp, float sampleDist, float sampleMaxError, int heightSearchRadius, CompactHeightfield chf, HeightPatch hp, Vector3[] verts, out int nverts, out Int4[] outTris)
        {
            nverts = 0;
            outTris = null;

            List<Int4> edges = new List<Int4>();
            List<Int4> samples = new List<Int4>();
            List<Int4> tris = new List<Int4>();

            int MAX_VERTS = 127;
            int MAX_TRIS = 255;    // Max tris for delaunay is 2n-2-k (n=num verts, k=num hull verts).
            int MAX_VERTS_PER_EDGE = 32;
            Vector3[] edge = new Vector3[(MAX_VERTS_PER_EDGE + 1)];
            int[] hull = new int[MAX_VERTS];
            int nhull = 0;

            nverts = ninp;

            for (int i = 0; i < ninp; ++i)
            {
                verts[i] = inp[i];
            }

            edges.Clear();

            float cs = chf.cs;
            float ics = 1.0f / cs;

            // Calculate minimum extents of the polygon based on input data.
            float minExtent = PolyUtils.PolyMinExtent(verts, nverts);

            // Tessellate outlines.
            // This is done in separate pass in order to ensure
            // seamless height values across the ply boundaries.
            if (sampleDist > 0)
            {
                for (int i = 0, j = ninp - 1; i < ninp; j = i++)
                {
                    var vj = inp[j];
                    var vi = inp[i];
                    bool swapped = false;
                    // Make sure the segments are always handled in same order
                    // using lexological sort or else there will be seams.
                    if (Math.Abs(vj[0] - vi[0]) < 1e-6f)
                    {
                        if (vj[2] > vi[2])
                        {
                            Helper.Swap(ref vj, ref vi);
                            swapped = true;
                        }
                    }
                    else
                    {
                        if (vj[0] > vi[0])
                        {
                            Helper.Swap(ref vj, ref vi);
                            swapped = true;
                        }
                    }
                    // Create samples along the edge.
                    float dx = vi.X - vj.X;
                    float dy = vi.Y - vj.Y;
                    float dz = vi.Z - vj.Z;
                    float d = (float)Math.Sqrt(dx * dx + dz * dz);
                    int nn = 1 + (int)Math.Floor(d / sampleDist);
                    if (nn >= MAX_VERTS_PER_EDGE) nn = MAX_VERTS_PER_EDGE - 1;
                    if (nverts + nn >= MAX_VERTS)
                    {
                        nn = MAX_VERTS - 1 - nverts;
                    }

                    for (int k = 0; k <= nn; ++k)
                    {
                        float u = ((float)k / (float)nn);
                        Vector3 pos = new Vector3
                        {
                            X = vj.X + dx * u,
                            Y = vj.Y + dy * u,
                            Z = vj.Z + dz * u
                        };
                        pos.Y = GetHeight(pos.X, pos.Y, pos.Z, cs, ics, chf.ch, heightSearchRadius, hp) * chf.ch;
                        edge[k] = pos;
                    }
                    // Simplify samples.
                    int[] idx = new int[MAX_VERTS_PER_EDGE];
                    idx[0] = 0;
                    idx[1] = nn;
                    int nidx = 2;
                    for (int k = 0; k < nidx - 1;)
                    {
                        int a = idx[k];
                        int b = idx[k + 1];
                        var va = edge[a];
                        var vb = edge[b];
                        // Find maximum deviation along the segment.
                        float maxd = 0;
                        int maxi = -1;
                        for (int m = a + 1; m < b; ++m)
                        {
                            float dev = PolyUtils.DistancePtSeg(edge[m], va, vb);
                            if (dev > maxd)
                            {
                                maxd = dev;
                                maxi = m;
                            }
                        }
                        // If the max deviation is larger than accepted error,
                        // add new point, else continue to next segment.
                        if (maxi != -1 && maxd > (sampleMaxError * sampleMaxError))
                        {
                            for (int m = nidx; m > k; --m)
                            {
                                idx[m] = idx[m - 1];
                            }
                            idx[k + 1] = maxi;
                            nidx++;
                        }
                        else
                        {
                            ++k;
                        }
                    }

                    hull[nhull++] = j;
                    // Add new vertices.
                    if (swapped)
                    {
                        for (int k = nidx - 2; k > 0; --k)
                        {
                            verts[nverts] = edge[idx[k]];
                            hull[nhull++] = nverts;
                            nverts++;
                        }
                    }
                    else
                    {
                        for (int k = 1; k < nidx - 1; ++k)
                        {
                            verts[nverts] = edge[idx[k]];
                            hull[nhull++] = nverts;
                            nverts++;
                        }
                    }
                }
            }

            // If the polygon minimum extent is small (sliver or small triangle), do not try to add internal points.
            if (minExtent < sampleDist * 2)
            {
                PolyUtils.TriangulateHull(nverts, verts, nhull, hull, tris);

                outTris = tris.ToArray();

                return true;
            }

            // Tessellate the base mesh.
            // We're using the triangulateHull instead of delaunayHull as it tends to
            // create a bit better triangulation for long thin triangles when there
            // are no internal points.
            PolyUtils.TriangulateHull(nverts, verts, nhull, hull, tris);

            if (tris.Count == 0)
            {
                // Could not triangulate the poly, make sure there is some valid data there.
                //ctx->log(RC_LOG_WARNING, "buildPolyDetail: Could not triangulate polygon (%d verts).", nverts);

                outTris = tris.ToArray();

                return true;
            }

            if (sampleDist > 0)
            {
                // Create sample locations in a grid.
                Vector3 bmin, bmax;
                bmin = inp[0];
                bmax = inp[0];
                for (int i = 1; i < ninp; ++i)
                {
                    bmin = Vector3.Min(bmin, inp[i]);
                    bmax = Vector3.Max(bmax, inp[i]);
                }
                int x0 = (int)Math.Floor(bmin.X / sampleDist);
                int x1 = (int)Math.Ceiling(bmax.X / sampleDist);
                int z0 = (int)Math.Floor(bmin.Z / sampleDist);
                int z1 = (int)Math.Ceiling(bmax.Z / sampleDist);
                samples.Clear();
                for (int z = z0; z < z1; ++z)
                {
                    for (int x = x0; x < x1; ++x)
                    {
                        Vector3 pt = new Vector3
                        {
                            X = x * sampleDist,
                            Y = (bmax.Y + bmin.Y) * 0.5f,
                            Z = z * sampleDist
                        };
                        // Make sure the samples are not too close to the edges.
                        if (PolyUtils.DistToPoly(ninp, inp, pt) > -sampleDist / 2) continue;
                        samples.Add(
                            new Int4(
                                x,
                                GetHeight(pt.X, pt.Y, pt.Z, cs, ics, chf.ch, heightSearchRadius, hp),
                                z,
                                0)); // Not added
                    }
                }

                // Add the samples starting from the one that has the most
                // error. The procedure stops when all samples are added
                // or when the max error is within treshold.
                int nsamples = samples.Count;
                for (int iter = 0; iter < nsamples; ++iter)
                {
                    if (nverts >= MAX_VERTS)
                    {
                        break;
                    }

                    // Find sample with most error.
                    Vector3 bestpt = new Vector3();
                    float bestd = 0;
                    int besti = -1;
                    for (int i = 0; i < nsamples; ++i)
                    {
                        var s = samples[i];
                        if (s.W != 0) continue; // skip added.
                        // The sample location is jittered to get rid of some bad triangulations
                        // which are cause by symmetrical data from the grid structure.
                        Vector3 pt = new Vector3
                        {
                            X = s.X * sampleDist + PolyUtils.GetJitterX(i) * cs * 0.1f,
                            Y = s.Y * chf.ch,
                            Z = s.Z * sampleDist + PolyUtils.GetJitterY(i) * cs * 0.1f
                        };
                        float d = PolyUtils.DistToTriMesh(pt, verts, nverts, tris.ToArray(), tris.Count);
                        if (d < 0) continue; // did not hit the mesh.
                        if (d > bestd)
                        {
                            bestd = d;
                            besti = i;
                            bestpt = pt;
                        }
                    }
                    // If the max error is within accepted threshold, stop tesselating.
                    if (bestd <= sampleMaxError || besti == -1)
                    {
                        break;
                    }
                    // Mark sample as added.
                    var sb = samples[besti];
                    sb.W = 1;
                    samples[besti] = sb;
                    // Add the new sample point.
                    verts[nverts] = bestpt;
                    nverts++;

                    // Create new triangulation.
                    // TODO: Incremental add instead of full rebuild.
                    edges.Clear();
                    tris.Clear();
                    PolyUtils.DelaunayHull(nverts, verts, nhull, hull, tris, edges);
                }
            }

            int ntris = tris.Count;
            if (ntris > MAX_TRIS)
            {
                tris.RemoveRange(MAX_TRIS, ntris - MAX_TRIS);
                //ctx->log(RC_LOG_ERROR, "rcBuildPolyMeshDetail: Shrinking triangle count from %d to max %d.", ntris, MAX_TRIS);
            }

            outTris = tris.ToArray();

            return true;
        }
        private static int GetHeight(float fx, float fy, float fz, float cs, float ics, float ch, int radius, HeightPatch hp)
        {
            int ix = (int)Math.Floor(fx * ics + 0.01f);
            int iz = (int)Math.Floor(fz * ics + 0.01f);
            ix = MathUtil.Clamp(ix - hp.xmin, 0, hp.width - 1);
            iz = MathUtil.Clamp(iz - hp.ymin, 0, hp.height - 1);
            int h = hp.data[ix + iz * hp.width];
            if (h == Constants.RC_UNSET_HEIGHT)
            {
                // Special case when data might be bad.
                // Walk adjacent cells in a spiral up to 'radius', and look
                // for a pixel which has a valid height.
                int x = 1, z = 0, dx = 1, dz = 0;
                int maxSize = radius * 2 + 1;
                int maxIter = maxSize * maxSize - 1;

                int nextRingIterStart = 8;
                int nextRingIters = 16;

                float dmin = float.MaxValue;
                for (int i = 0; i < maxIter; i++)
                {
                    int nx = ix + x;
                    int nz = iz + z;

                    if (nx >= 0 && nz >= 0 && nx < hp.width && nz < hp.height)
                    {
                        int nh = hp.data[nx + nz * hp.width];
                        if (nh != Constants.RC_UNSET_HEIGHT)
                        {
                            float d = Math.Abs(nh * ch - fy);
                            if (d < dmin)
                            {
                                h = nh;
                                dmin = d;
                            }
                        }
                    }

                    // We are searching in a grid which looks approximately like this:
                    //  __________
                    // |2 ______ 2|
                    // | |1 __ 1| |
                    // | | |__| | |
                    // | |______| |
                    // |__________|
                    // We want to find the best height as close to the center cell as possible. This means that
                    // if we find a height in one of the neighbor cells to the center, we don't want to
                    // expand further out than the 8 neighbors - we want to limit our search to the closest
                    // of these "rings", but the best height in the ring.
                    // For example, the center is just 1 cell. We checked that at the entrance to the function.
                    // The next "ring" contains 8 cells (marked 1 above). Those are all the neighbors to the center cell.
                    // The next one again contains 16 cells (marked 2). In general each ring has 8 additional cells, which
                    // can be thought of as adding 2 cells around the "center" of each side when we expand the ring.
                    // Here we detect if we are about to enter the next ring, and if we are and we have found
                    // a height, we abort the search.
                    if (i + 1 == nextRingIterStart)
                    {
                        if (h != Constants.RC_UNSET_HEIGHT)
                        {
                            break;
                        }

                        nextRingIterStart += nextRingIters;
                        nextRingIters += 8;
                    }

                    if ((x == z) || ((x < 0) && (x == -z)) || ((x > 0) && (x == 1 - z)))
                    {
                        int tmp = dx;
                        dx = -dz;
                        dz = tmp;
                    }
                    x += dx;
                    z += dz;
                }
            }
            return h;
        }
        private static bool CanRemoveVertex(PolyMesh mesh, int rem)
        {
            int nvp = mesh.nvp;

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

            // Find edges which share the removed vertex.
            int maxEdges = numTouchedVerts * 2;
            int nedges = 0;
            Int3[] edges = new Int3[maxEdges];

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
                            var e = new Int3();
                            e[0] = a;
                            e[1] = b;
                            e[2] = 1;
                            edges[nedges] = e;
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
        private static bool RemoveVertex(PolyMesh mesh, int rem, int maxTris)
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
            Int4[] edges = new Int4[numRemovedVerts];
            int nhole = 0;
            int[] hole = new int[numRemovedVerts];
            int nhreg = 0;
            int[] hreg = new int[numRemovedVerts];
            int nharea = 0;
            SamplePolyAreas[] harea = new SamplePolyAreas[numRemovedVerts];

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
                            var e = new Int4(p[k], p[j], mesh.regs[i], (int)mesh.areas[i]);
                            edges[nedges] = e;
                            nedges++;
                        }
                    }
                    // Remove the polygon.
                    var p2 = mesh.polys[mesh.npolys - 1];
                    if (p != p2)
                    {
                        //memcpy(p, p2, sizeof(unsigned short) * nvp);
                    }
                    //memset(p + nvp, 0xff, sizeof(unsigned short) * nvp);
                    mesh.regs[i] = mesh.regs[mesh.npolys - 1];
                    mesh.areas[i] = mesh.areas[mesh.npolys - 1];
                    mesh.npolys--;
                    --i;
                }
            }

            // Remove vertex.
            for (int i = rem; i < mesh.nverts - 1; ++i)
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
            PolyUtils.PushBack(edges[0].Z, hreg, nhreg);
            PolyUtils.PushBack((SamplePolyAreas)edges[0].W, harea, nharea);

            while (nedges != 0)
            {
                bool match = false;

                for (int i = 0; i < nedges; ++i)
                {
                    int ea = edges[i].X;
                    int eb = edges[i].Y;
                    int r = edges[i].Z;
                    SamplePolyAreas a = (SamplePolyAreas)edges[i].W;
                    bool add = false;
                    if (hole[0] == eb)
                    {
                        // The segment matches the beginning of the hole boundary.
                        PolyUtils.PushFront(ea, hole, nhole);
                        PolyUtils.PushFront(r, hreg, nhreg);
                        PolyUtils.PushFront(a, harea, nharea);
                        add = true;
                    }
                    else if (hole[nhole - 1] == ea)
                    {
                        // The segment matches the end of the hole boundary.
                        PolyUtils.PushBack(eb, hole, nhole);
                        PolyUtils.PushBack(r, hreg, nhreg);
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

            var tverts = new Int4[nhole];
            var thole = new int[nhole];

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
                //ctx->log(RC_LOG_WARNING, "removeVertex: triangulate() returned bad results.");
                ntris = -ntris;
            }

            // Merge the hole triangles back to polygons.
            var polys = new Polygoni[(ntris + 1)];
            var pregs = new int[ntris];
            var pareas = new SamplePolyAreas[ntris];

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

                    // If this polygon covers multiple region types then mark it as such
                    if (hreg[t.X] != hreg[t.Y] || hreg[t.Y] != hreg[t.Z])
                    {
                        pregs[npolys] = Constants.RC_MULTIPLE_REGS;
                    }
                    else
                    {
                        pregs[npolys] = hreg[t.X];
                    }

                    pareas[npolys] = harea[t.X];
                    npolys++;
                }
            }
            if (npolys == 0)
            {
                return true;
            }

            // Merge polygons.
            int nvp = mesh.nvp;
            if (nvp > 3)
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
                        if (pregs[bestPa] != pregs[bestPb])
                        {
                            pregs[bestPa] = Constants.RC_MULTIPLE_REGS;
                        }
                        polys[bestPb] = polys[(npolys - 1)];
                        pregs[bestPb] = pregs[npolys - 1];
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
                for (int j = 0; j < nvp; ++j)
                {
                    p[j] = polys[i][j];
                }
                mesh.regs[mesh.npolys] = pregs[i];
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

        private static void BuildAllTiles(InputGeometry geom, BuildSettings settings, Agent agent, NavigationMesh2 navMesh)
        {
            var bbox = geom.BoundingBox;
            CalcGridSize(bbox, settings.CellSize, out int gw, out int gh);
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

                    MeshData data = BuildTileMesh(x, y, lastBuiltBbox, geom, settings, agent);
                    if (data != null)
                    {
                        // Remove any previous data (navmesh owns and deletes the data).
                        navMesh.RemoveTile(navMesh.GetTileRefAt(x, y, 0), data, 0);
                        // Let the navmesh own the data.
                        navMesh.AddTile(data, TileFlags.FreeData, 0, out int result);
                    }
                }
            }
        }
        private static MeshData BuildTileMesh(int tx, int ty, BoundingBox bbox, InputGeometry geometry, BuildSettings settings, Agent agent)
        {
            var chunkyMesh = geometry.GetChunkyMesh();

            int walkableRadius = (int)Math.Ceiling(agent.Radius / settings.CellSize);
            int tileSize = (int)settings.TileSize;
            int borderSize = walkableRadius + 3;

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
            bbox.Minimum.X -= borderSize * settings.CellSize;
            bbox.Minimum.Z -= borderSize * settings.CellSize;
            bbox.Maximum.X += borderSize * settings.CellSize;
            bbox.Maximum.Z += borderSize * settings.CellSize;

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

            // Allocate voxel heightfield where we rasterize our input data to.
            var solid = new Heightfield
            {
                width = cfg.Width,
                height = cfg.Height,
                boundingBox = cfg.BoundingBox,
                cs = cfg.CellSize,
                ch = cfg.CellHeight,
                spans = new Span[cfg.Width * cfg.Height],
            };

            // Allocate array that can hold triangle flags.
            // If you have multiple meshes you need to process, allocate
            // and array which can hold the max number of triangles you need to process.
            TileCacheAreas[] triareas = new TileCacheAreas[chunkyMesh.maxTrisPerChunk];

            Vector2 tbmin = new Vector2(cfg.BoundingBox.Minimum.X, cfg.BoundingBox.Minimum.Z);
            Vector2 tbmax = new Vector2(cfg.BoundingBox.Maximum.X, cfg.BoundingBox.Maximum.Z);
            var cid = GetChunksOverlappingRect(chunkyMesh, tbmin, tbmax);
            if (cid.Count() == 0)
            {
                return null; // empty
            }

            foreach (var id in cid)
            {
                var tris = chunkyMesh.GetTriangles(id);

                Helper.InitializeArray(triareas, TileCacheAreas.NullArea);

                MarkWalkableTriangles(cfg.WalkableSlopeAngle, tris, triareas);

                if (!RasterizeTriangles(solid, cfg.WalkableClimb, tris, triareas))
                {
                    return null;
                }
            }

            // Once all geometry is rasterized, we do initial pass of filtering to
            // remove unwanted overhangs caused by the conservative rasterization
            // as well as filter spans where the character cannot possibly stand.
            if (settings.FilterLowHangingObstacles)
            {
                FilterLowHangingWalkableObstacles(cfg.WalkableClimb, solid);
            }
            if (settings.FilterLedgeSpans)
            {
                FilterLedgeSpans(cfg.WalkableHeight, cfg.WalkableClimb, solid);
            }
            if (settings.FilterWalkableLowHeightSpans)
            {
                FilterWalkableLowHeightSpans(cfg.WalkableHeight, solid);
            }

            // Compact the heightfield so that it is faster to handle from now on.
            // This will result more cache coherent data as well as the neighbours
            // between walkable cells will be calculated.
            if (!BuildCompactHeightfield(cfg.WalkableHeight, cfg.WalkableClimb, solid, out CompactHeightfield chf))
            {
                return null;
            }

            // Erode the walkable area by agent radius.
            if (!ErodeWalkableArea(cfg.WalkableRadius, chf))
            {
                return null;
            }

            // (Optional) Mark areas.
            ConvexVolume[] vols = geometry.GetConvexVolumes();
            for (int i = 0; i < geometry.GetConvexVolumeCount(); ++i)
            {
                MarkConvexPolyArea(
                    vols[i].verts, vols[i].nverts,
                    vols[i].hmin, vols[i].hmax,
                    vols[i].area, chf);
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
                if (!BuildDistanceField(chf))
                {
                    return null;
                }

                // Partition the walkable surface into simple regions without holes.
                if (!BuildRegions(chf, cfg.BorderSize, cfg.MinRegionArea, cfg.MergeRegionArea))
                {
                    return null;
                }
            }
            else if (settings.PartitionType == SamplePartitionTypeEnum.Monotone)
            {
                // Partition the walkable surface into simple regions without holes.
                // Monotone partitioning does not need distancefield.
                if (!BuildRegionsMonotone(chf, cfg.BorderSize, cfg.MinRegionArea, cfg.MergeRegionArea))
                {
                    return null;
                }
            }
            else if (settings.PartitionType == SamplePartitionTypeEnum.Layers)
            {
                // Partition the walkable surface into simple regions without holes.
                if (!BuildLayerRegions(chf, cfg.BorderSize, cfg.MinRegionArea))
                {
                    return null;
                }
            }

            // Create contours.
            if (!BuildContours(chf, cfg.MaxSimplificationError, cfg.MaxEdgeLen, BuildContoursFlags.RC_CONTOUR_TESS_WALL_EDGES, out ContourSet cset))
            {
                return null;
            }

            if (cset.nconts == 0)
            {
                return null;
            }

            // Build polygon navmesh from the contours.
            if (!BuildPolyMesh(cset, cfg.MaxVertsPerPoly, out PolyMesh pmesh))
            {
                return null;
            }

            // Build detail mesh.
            if (!BuildPolyMeshDetail(pmesh, chf, cfg.DetailSampleDist, cfg.DetailSampleMaxError, out PolyMeshDetail dmesh))
            {
                return null;
            }

            if (cfg.MaxVertsPerPoly <= Constants.VertsPerPolygon)
            {
                if (pmesh.nverts >= 0xffff)
                {
                    // The vertex indices are ushorts, and cannot point to more than 0xffff vertices.
                    return null;
                }

                // Update poly flags from areas.
                for (int i = 0; i < pmesh.npolys; ++i)
                {
                    if ((int)pmesh.areas[i] == (int)TileCacheAreas.WalkableArea)
                    {
                        pmesh.areas[i] = SamplePolyAreas.SAMPLE_POLYAREA_GROUND;
                    }

                    if (pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_GROUND ||
                        pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_GRASS ||
                        pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_ROAD)
                    {
                        pmesh.flags[i] = SamplePolyFlags.SAMPLE_POLYFLAGS_WALK;
                    }
                    else if (pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_WATER)
                    {
                        pmesh.flags[i] = SamplePolyFlags.SAMPLE_POLYFLAGS_SWIM;
                    }
                    else if (pmesh.areas[i] == SamplePolyAreas.SAMPLE_POLYAREA_DOOR)
                    {
                        pmesh.flags[i] = SamplePolyFlags.SAMPLE_POLYFLAGS_WALK | SamplePolyFlags.SAMPLE_POLYFLAGS_DOOR;
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
                    offMeshConVerts = geometry.GetOffMeshConnectionVerts(),
                    offMeshConRad = geometry.GetOffMeshConnectionRads(),
                    offMeshConDir = geometry.GetOffMeshConnectionDirs(),
                    offMeshConAreas = geometry.GetOffMeshConnectionAreas(),
                    offMeshConFlags = geometry.GetOffMeshConnectionFlags(),
                    offMeshConUserID = geometry.GetOffMeshConnectionId(),
                    offMeshConCount = geometry.GetOffMeshConnectionCount(),
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

                if (!CreateNavMeshData(param, out MeshData navData))
                {
                    return null;
                }

                return navData;
            }

            return null;
        }

        public static bool CreateNavMeshData(NavMeshCreateParams param, out MeshData outData)
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
                    offMeshConClass[i + 0] = PolyUtils.ClassifyOffMeshPoint(p0, bmin, bmax);
                    offMeshConClass[i + 1] = PolyUtils.ClassifyOffMeshPoint(p1, bmin, bmax);

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
                        var verts = param.detailVerts.Skip(vb + nv).Take(ndv - nv);
                        data.navDVerts.AddRange(verts);
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
                        var t = new Int4
                        {
                            X = 0,
                            Y = (j - 1),
                            Z = j,
                            // Bit for each edge that belongs to poly boundary.
                            W = (1 << 2)
                        };
                        if (j == 2) t.W |= (1 << 0);
                        if (j == nv - 1) t.W |= (1 << 4);
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
                        flags = param.offMeshConDir[i] != 0 ? Constants.DT_OFFMESH_CON_BIDIR : 0,
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
        public static int CreateBVTree(NavMeshCreateParams param, ref List<BVNode> nodes)
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
                    Vector3 bmin = param.detailVerts[vb];
                    Vector3 bmax = param.detailVerts[vb];

                    for (int j = 1; j < ndv; j++)
                    {
                        bmin = Vector3.Min(bmin, param.detailVerts[vb + j]);
                        bmax = Vector3.Max(bmax, param.detailVerts[vb + j]);
                    }

                    // BV-tree uses cs for all dimensions
                    it.bmin.X = MathUtil.Clamp((int)((bmin.X - param.bmin.X) * quantFactor), 0, 0xffff);
                    it.bmin.Y = MathUtil.Clamp((int)((bmin.Y - param.bmin.Y) * quantFactor), 0, 0xffff);
                    it.bmin.Z = MathUtil.Clamp((int)((bmin.Z - param.bmin.Z) * quantFactor), 0, 0xffff);

                    it.bmax.X = MathUtil.Clamp((int)((bmax.X - param.bmin.X) * quantFactor), 0, 0xffff);
                    it.bmax.Y = MathUtil.Clamp((int)((bmax.Y - param.bmin.Y) * quantFactor), 0, 0xffff);
                    it.bmax.Z = MathUtil.Clamp((int)((bmax.Z - param.bmin.Z) * quantFactor), 0, 0xffff);
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
                items[i] = it;
            }

            int curNode = 0;
            Subdivide(items, param.polyCount, 0, param.polyCount, ref curNode, ref nodes);

            items = null;

            return curNode;
        }
        private static void Subdivide(BVItem[] items, int nitems, int imin, int imax, ref int curNode, ref List<BVNode> nodes)
        {
            int inum = imax - imin;
            int icur = curNode;

            BVNode node = new BVNode();
            nodes.Add(node);
            curNode++;

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

                int axis = PolyUtils.LongestAxis(
                    node.bmax.X - node.bmin.X,
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
        private static void CalcExtends(BVItem[] items, int nitems, int imin, int imax, ref Int3 bmin, ref Int3 bmax)
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

        public static void SaveFile(string path, NavigationMesh2 mesh)
        {
            List<byte> buffer = new List<byte>();

            NavMeshSetHeader header = new NavMeshSetHeader
            {
                magic = Constants.Magic,
                version = Constants.Version,
                numTiles = 0,
                param = mesh.m_params,
            };

            List<NavMeshTileHeader> tileHeaders = new List<NavMeshTileHeader>();

            // Store header and tiles.
            for (int i = 0; i < mesh.MaxTiles; ++i)
            {
                var tile = mesh.Tiles[i];
                if (tile == null || tile.header.magic != Constants.Magic || tile.data == null) continue;

                header.numTiles++;
                tileHeaders.Add(new NavMeshTileHeader
                {
                    tile = tile.data,
                    dataSize = tile.dataSize
                });
            }

            header.numTiles = tileHeaders.Count;

            NavMeshFile file = new NavMeshFile()
            {
                header = header,
                tileHeaders = tileHeaders.ToArray(),
            };

            File.WriteAllBytes(path, file.Compress());
        }
        public static NavigationMesh2 LoadFile(string path)
        {
            byte[] buffer = File.ReadAllBytes(path);

            var nmFile = buffer.Decompress<NavMeshFile>();

            NavigationMesh2 mesh = new NavigationMesh2();

            mesh.Init(nmFile.header.param);

            // Read tiles.
            for (int i = 0; i < nmFile.header.numTiles; ++i)
            {
                NavMeshTileHeader tileHeader = nmFile.tileHeaders[i];

                mesh.AddTile(tileHeader.tile, TileFlags.FreeData, 0, out int result);
            }

            return mesh;
        }

        private NavMeshParams m_params;
        private Vector3 m_orig;
        private float m_tileWidth;
        private float m_tileHeight;
        private int m_tileLutSize;
        private int m_tileLutMask;
        private MeshTile[] m_posLookup;
        private MeshTile m_nextFree = null;
        private int m_tileBits;
        private int m_polyBits;
        private int m_saltBits;

        public int MaxTiles { get; set; }
        public MeshTile[] Tiles { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public NavigationMesh2()
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
            m_tileLutSize = Helper.NextPowerOfTwo(nmparams.MaxTiles / 4);
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
        public bool Init(MeshData data, TileFlags flags)
        {
            // Make sure the data is in right format.
            MeshHeader header = data.header;
            if (header.magic != Constants.Magic)
            {
                return false;
            }
            if (header.version != Constants.Version)
            {
                return false;
            }

            NavMeshParams param = new NavMeshParams();
            param.Origin = header.bmin;
            param.TileWidth = header.bmax[0] - header.bmin[0];
            param.TileHeight = header.bmax[2] - header.bmin[2];
            param.MaxTiles = 1;
            param.MaxPolys = header.polyCount;

            Init(param);

            return AddTile(data, flags, 0, out int result);
        }
        public MeshTile GetTileRefAt(int x, int y, int layer)
        {
            // Find tile based on hash.
            int h = PolyUtils.ComputeTileHash(x, y, m_tileLutMask);
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
        public bool AddTile(MeshData data, TileFlags flags, int lastRef, out int result)
        {
            result = -1;

            // Make sure the data is in right format.
            MeshHeader header = data.header;
            if (header.magic != Constants.Magic)
            {
                return false;
            }
            if (header.version != Constants.Version)
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
                if (tile != target)
                {
                    return false;
                }
                // Remove from freelist
                if (prev == null)
                {
                    m_nextFree = tile.next;
                }
                else
                {
                    prev.next = tile.next;
                }

                // Restore salt.
                tile.salt = DecodePolyIdSalt(lastRef);
            }

            // Make sure we could allocate a tile.
            if (tile == null)
            {
                return false;
            }

            // Insert tile into the position lut.
            int h = PolyUtils.ComputeTileHash(header.x, header.y, m_tileLutMask);
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
            tile.links[header.maxLinkCount - 1].next = Constants.DT_NULL_LINK;
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
                    ConnectExtLinks(neis[j], tile, OppositeTile(i));
                    ConnectExtOffMeshLinks(tile, neis[j], i);
                    ConnectExtOffMeshLinks(neis[j], tile, OppositeTile(i));
                }
            }

            result = GetTileRef(tile);

            return true;
        }
        private MeshTile GetTileAt(int x, int y, int layer)
        {
            // Find tile based on hash.
            int h = PolyUtils.ComputeTileHash(x, y, m_tileLutMask);
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
        private int GetTilesAt(int x, int y, MeshTile[] tiles, int maxTiles)
        {
            int n = 0;

            // Find tile based on hash.
            int h = PolyUtils.ComputeTileHash(x, y, m_tileLutMask);
            MeshTile tile = m_posLookup[h];
            while (tile != null)
            {
                if (tile.header.x == x &&
                    tile.header.y == y)
                {
                    if (n < maxTiles)

                        tiles[n++] = tile;
                }
                tile = tile.next;
            }

            return n;
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
            };

            return GetTilesAt(nx, ny, tiles, maxTiles);
        }
        internal int DecodePolyIdTile(int r)
        {
            int tileMask = (1 << m_tileBits) - 1;
            return ((r >> m_polyBits) & tileMask);
        }
        private int DecodePolyIdSalt(int r)
        {
            int saltMask = (1 << m_saltBits) - 1;
            return ((r >> (m_polyBits + m_tileBits)) & saltMask);
        }
        private int EncodePolyId(int salt, int it, int ip)
        {
            return (salt << (m_polyBits + m_tileBits)) | (it << m_polyBits) | ip;
        }
        private void ConnectIntLinks(MeshTile tile)
        {
            if (tile == null) return;

            int bse = GetPolyRefBase(tile);

            for (int i = 0; i < tile.header.polyCount; ++i)
            {
                var poly = tile.polys[i];
                poly.firstLink = Constants.DT_NULL_LINK;

                if (poly.Type == PolyTypes.OffmeshConnection)
                {
                    continue;
                }

                // Build edge links backwards so that the links will be
                // in the linked list from lowest index to highest.
                for (int j = poly.vertCount - 1; j >= 0; --j)
                {
                    // Skip hard and non-internal edges.
                    if (poly.neis[j] == 0 || (poly.neis[j] & Constants.DT_EXT_LINK) != 0) continue;

                    int idx = AllocLink(tile);
                    if (idx != Constants.DT_NULL_LINK)
                    {
                        var link = new Link
                        {
                            nref = (bse | (poly.neis[j] - 1)),
                            edge = j,
                            side = 0xff,
                            bmin = 0,
                            bmax = 0,
                            // Add to linked list.
                            next = poly.firstLink,
                        };
                        poly.firstLink = idx;
                        tile.links[idx] = link;
                    }
                }
            }
        }
        internal int GetPolyRefBase(MeshTile tile)
        {
            if (tile == null) return 0;
            int it = Array.IndexOf(Tiles, tile);
            return EncodePolyId(tile.salt, it, 0);
        }
        private int AllocLink(MeshTile tile)
        {
            if (tile.linksFreeList == Constants.DT_NULL_LINK)
            {
                return Constants.DT_NULL_LINK;
            }
            int link = tile.linksFreeList;
            tile.linksFreeList = tile.links[link].next;
            return link;
        }
        private void BaseOffMeshLinks(MeshTile tile)
        {
            if (tile == null) return;

            int bse = GetPolyRefBase(tile);

            // Base off-mesh connection start points.
            for (int i = 0; i < tile.header.offMeshConCount; ++i)
            {
                var con = tile.offMeshCons[i];
                var poly = tile.polys[con.poly];

                Vector3 halfExtents = new Vector3(new float[] { con.rad, tile.header.walkableClimb, con.rad });

                // Find polygon to connect to.
                Vector3 p = con.pos[0]; // First vertex
                Vector3 nearestPt = new Vector3();
                int r = FindNearestPolyInTile(tile, p, halfExtents, nearestPt);
                if (r == 0) continue;
                // findNearestPoly may return too optimistic results, further check to make sure. 
                if (Math.Sqrt(nearestPt[0] - p[0]) + Math.Sqrt(nearestPt[2] - p[2]) > Math.Sqrt(con.rad))
                {
                    continue;
                }
                // Make sure the location is on current mesh.
                var v = tile.verts[poly.verts[0]];
                v = nearestPt;

                // Link off-mesh connection to target poly.
                int idx = AllocLink(tile);
                if (idx != Constants.DT_NULL_LINK)
                {
                    var link = new Link
                    {
                        nref = r,
                        edge = 0,
                        side = 0xff,
                        bmin = 0,
                        bmax = 0,
                        // Add to linked list.
                        next = poly.firstLink
                    };
                    tile.links[idx] = link;
                    poly.firstLink = idx;
                }

                // Start end-point is always connect back to off-mesh connection. 
                int tidx = AllocLink(tile);
                if (tidx != Constants.DT_NULL_LINK)
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
                        next = landPoly.firstLink
                    };
                    tile.links[tidx] = link;
                    landPoly.firstLink = tidx;
                }
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
                int bse = GetPolyRefBase(tile);
                int n = 0;
                while (nodeIndex < endIndex)
                {
                    var node = tile.bvTree[nodeIndex];
                    var end = tile.bvTree[endIndex];

                    bool overlap = PolyUtils.OverlapQuantBounds(bmin, bmax, node.bmin, node.bmax);
                    bool isLeafNode = node.i >= 0;

                    if (isLeafNode && overlap)
                    {
                        if (n < maxPolys)
                            polys[n++] = bse | node.i;
                    }

                    if (overlap || isLeafNode)
                        nodeIndex++;
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
                Vector3 bmin = new Vector3();
                Vector3 bmax = new Vector3();
                int n = 0;
                int bse = GetPolyRefBase(tile);
                for (int i = 0; i < tile.header.polyCount; ++i)
                {
                    Poly p = tile.polys[i];
                    // Do not return off-mesh connection polygons.
                    if (p.Type == PolyTypes.OffmeshConnection)
                        continue;
                    // Calc polygon bounds.
                    Vector3 v = tile.verts[p.verts[0]];
                    bmin = v;
                    bmax = v;
                    for (int j = 1; j < p.vertCount; ++j)
                    {
                        v = tile.verts[p.verts[j]];
                        bmin = Vector3.Min(bmin, v);
                        bmax = Vector3.Max(bmax, v);
                    }
                    if (PolyUtils.OverlapBounds(qmin, qmax, bmin, bmax))
                    {
                        if (n < maxPolys)

                            polys[n++] = bse | i;
                    }
                }
                return n;
            }
        }
        private void ClosestPointOnPoly(int r, Vector3 pos, out Vector3 closest, out bool posOverPoly)
        {
            GetTileAndPolyByRefUnsafe(r, out MeshTile tile, out Poly poly);

            // Off-mesh connections don't have detail polygons.
            if (poly.Type == PolyTypes.OffmeshConnection)
            {
                Vector3 v0 = tile.verts[poly.verts[0]];
                Vector3 v1 = tile.verts[poly.verts[1]];
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
            Vector3[] verts = new Vector3[Constants.VertsPerPolygon];
            float[] edged = new float[Constants.VertsPerPolygon];
            float[] edget = new float[Constants.VertsPerPolygon];
            int nv = poly.vertCount;
            for (int i = 0; i < nv; ++i)
            {
                verts[i] = tile.verts[poly.verts[i]];
            }

            closest = pos;
            if (!DistancePtPolyEdgesSqr(pos, verts, nv, out edged, out edget))
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
                    if (t[k] < poly.vertCount)
                    {
                        v[k] = tile.verts[poly.verts[t[k]]];
                    }
                    else
                    {
                        v[k] = tile.detailVerts[(pd.vertBase + (t[k] - poly.vertCount))];
                    }
                }
                if (ClosestHeightPointTriangle(closest, v[0], v[1], v[2], out float h))
                {
                    closest[1] = h;
                    break;
                }
            }
        }
        private void GetTileAndPolyByRefUnsafe(int r, out MeshTile tile, out Poly poly)
        {
            DecodePolyId(r, out int salt, out int it, out int ip);
            tile = Tiles[it];
            poly = Tiles[it].polys[ip];
        }
        private void DecodePolyId(int r, out int salt, out int it, out int ip)
        {
            int saltMask = (1 << m_saltBits) - 1;
            int tileMask = (1 << m_tileBits) - 1;
            int polyMask = (1 << m_polyBits) - 1;
            salt = ((r >> (m_polyBits + m_tileBits)) & saltMask);
            it = ((r >> m_polyBits) & tileMask);
            ip = (r & polyMask);
        }
        private bool DistancePtPolyEdgesSqr(Vector3 pt, Vector3[] verts, int nverts, out float[] ed, out float[] et)
        {
            ed = new float[nverts];
            et = new float[nverts];

            // TODO: Replace pnpoly with triArea2D tests?
            int i, j;
            bool c = false;
            for (i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                var vi = verts[i];
                var vj = verts[j];
                if (((vi[2] > pt[2]) != (vj[2] > pt[2])) && (pt[0] < (vj[0] - vi[0]) * (pt[2] - vi[2]) / (vj[2] - vi[2]) + vi[0]))
                {
                    c = !c;
                }
                ed[j] = DistancePtSegSqr2D(pt, vj, vi, out et[j]);
            }
            return c;
        }
        private float DistancePtSegSqr2D(Vector3 pt, Vector3 p, Vector3 q, out float t)
        {
            float pqx = q[0] - p[0];
            float pqz = q[2] - p[2];
            float dx = pt[0] - p[0];
            float dz = pt[2] - p[2];
            float d = pqx * pqx + pqz * pqz;
            t = pqx * dx + pqz * dz;
            if (d > 0) t /= d;
            if (t < 0) t = 0;
            else if (t > 1) t = 1;
            dx = p[0] + t * pqx - pt[0];
            dz = p[2] + t * pqz - pt[2];
            return dx * dx + dz * dz;
        }
        private bool ClosestHeightPointTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c, out float h)
        {
            h = float.MaxValue;

            Vector3 v0 = Vector3.Subtract(c, a);
            Vector3 v1 = Vector3.Subtract(b, a);
            Vector3 v2 = Vector3.Subtract(p, a);

            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            // Compute barycentric coordinates
            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // The (sloppy) epsilon is needed to allow to get height of points which
            // are interpolated along the edges of the triangles.
            float EPS = 1e-4f;

            // If point lies inside the triangle, return interpolated ycoord.
            if (u >= -EPS && v >= -EPS && (u + v) <= 1 + EPS)
            {
                h = a[1] + v0[1] * u + v1[1] * v;
                return true;
            }

            return false;
        }
        private void ConnectExtOffMeshLinks(MeshTile tile, MeshTile target, int side)
        {
            if (tile == null) return;

            // Connect off-mesh links.
            // We are interested on links which land from target tile to this tile.
            int oppositeSide = (side == -1) ? 0xff : OppositeTile(side);

            for (int i = 0; i < target.header.offMeshConCount; ++i)
            {
                var targetCon = target.offMeshCons[i];
                if (targetCon.side != oppositeSide)
                {
                    continue;
                }

                var targetPoly = target.polys[targetCon.poly];
                // Skip off-mesh connections which start location could not be connected at all.
                if (targetPoly.firstLink == Constants.DT_NULL_LINK)
                {
                    continue;
                }

                Vector3 halfExtents = new Vector3(new float[] { targetCon.rad, target.header.walkableClimb, targetCon.rad });

                // Find polygon to connect to.
                Vector3 p = targetCon.pos[1];
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
                target.verts[targetPoly.verts[1]] = nearestPt;

                // Link off-mesh connection to target poly.
                int idx = AllocLink(target);
                if (idx != Constants.DT_NULL_LINK)
                {
                    var link = new Link
                    {
                        nref = r,
                        edge = 1,
                        side = oppositeSide,
                        bmin = 0,
                        bmax = 0,
                        // Add to linked list.
                        next = targetPoly.firstLink
                    };
                    target.links[idx] = link;
                    targetPoly.firstLink = idx;
                }

                // Link target poly to off-mesh connection.
                if ((targetCon.flags & Constants.DT_OFFMESH_CON_BIDIR) != 0)
                {
                    int tidx = AllocLink(tile);
                    if (tidx != Constants.DT_NULL_LINK)
                    {
                        var landPolyIdx = DecodePolyIdPoly(r);
                        var landPoly = tile.polys[landPolyIdx];
                        var link = new Link
                        {
                            nref = (GetPolyRefBase(target) | (targetCon.poly)),
                            edge = 0xff,
                            side = (side == -1 ? 0xff : side),
                            bmin = 0,
                            bmax = 0,
                            // Add to linked list.
                            next = landPoly.firstLink
                        };
                        tile.links[tidx] = link;
                        landPoly.firstLink = tidx;
                    }
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
                //		unsigned short m = DT_EXT_LINK | (unsigned short)side;

                int nv = poly.vertCount;
                for (int j = 0; j < nv; ++j)
                {
                    // Skip non-portal edges.
                    if ((poly.neis[j] & Constants.DT_EXT_LINK) == 0)
                    {
                        continue;
                    }

                    int dir = (int)(poly.neis[j] & 0xff);
                    if (side != -1 && dir != side)
                    {
                        continue;
                    }

                    // Create new links
                    var va = tile.verts[poly.verts[j]];
                    var vb = tile.verts[poly.verts[(j + 1) % nv]];
                    int nnei = FindConnectingPolys(va, vb, target, OppositeTile(dir), out int[] nei, out float[] neia, 4);
                    for (int k = 0; k < nnei; ++k)
                    {
                        int idx = AllocLink(tile);
                        if (idx != Constants.DT_NULL_LINK)
                        {
                            var link = new Link
                            {
                                nref = nei[k],
                                edge = j,
                                side = dir,
                                next = poly.firstLink
                            };
                            poly.firstLink = idx;

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
        private int OppositeTile(int side)
        {
            return (side + 4) & 0x7;
        }
        private int DecodePolyIdPoly(int r)
        {
            int polyMask = (1 << m_polyBits) - 1;
            return (r & polyMask);
        }
        private int FindConnectingPolys(Vector3 va, Vector3 vb, MeshTile tile, int side, out int[] con, out float[] conarea, int maxcon)
        {
            con = new int[maxcon];
            conarea = new float[maxcon * 2];

            if (tile == null) return 0;

            PolyUtils.CalcSlabEndPoints(va, vb, out Vector2 amin, out Vector2 amax, side);
            float apos = PolyUtils.GetSlabCoord(va, side);

            // Remove links pointing to 'side' and compact the links array. 
            int m = Constants.DT_EXT_LINK | side;
            int n = 0;

            int bse = GetPolyRefBase(tile);

            for (int i = 0; i < tile.header.polyCount; ++i)
            {
                Poly poly = tile.polys[i];
                int nv = poly.vertCount;
                for (int j = 0; j < nv; ++j)
                {
                    // Skip edges which do not point to the right side.
                    if (poly.neis[j] != m) continue;

                    Vector3 vc = tile.verts[poly.verts[j]];
                    Vector3 vd = tile.verts[poly.verts[(j + 1) % nv]];
                    float bpos = PolyUtils.GetSlabCoord(vc, side);

                    // Segments are not close enough.
                    if (Math.Abs(apos - bpos) > 0.01f)
                        continue;

                    // Check if the segments touch.
                    PolyUtils.CalcSlabEndPoints(vc, vd, out Vector2 bmin, out Vector2 bmax, side);

                    if (!PolyUtils.OverlapSlabs(amin, amax, bmin, bmax, 0.01f, tile.header.walkableClimb)) continue;

                    // Add return value.
                    if (n < maxcon)
                    {
                        conarea[n * 2 + 0] = Math.Max(amin[0], bmin[0]);
                        conarea[n * 2 + 1] = Math.Min(amax[0], bmax[0]);
                        con[n] = bse | i;
                        n++;
                    }
                    break;
                }
            }
            return n;
        }
        private int GetTileRef(MeshTile tile)
        {
            if (tile == null) return 0;
            int it = Array.IndexOf(Tiles, tile);
            return EncodePolyId(tile.salt, it, 0);
        }
        public bool RemoveTile(MeshTile tile, MeshData data, int dataSize)
        {
            if (tile == null)
            {
                return false;
            }

            // Remove tile from hash lookup.
            int h = PolyUtils.ComputeTileHash(tile.header.x, tile.header.y, m_tileLutMask);
            MeshTile prev = null;
            MeshTile cur = m_posLookup[h];
            while (cur != null)
            {
                if (cur == tile)
                {
                    if (prev != null)
                        prev.next = cur.next;
                    else
                        m_posLookup[h] = cur.next;
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
            if ((tile.flags & TileFlags.FreeData) != 0)
            {
                // Owns data
                tile.data = null;
                tile.dataSize = 0;
                data = null;
                dataSize = 0;
            }
            else
            {
                data = tile.data;
                dataSize = tile.dataSize;
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
        private void UnconnectLinks(MeshTile tile, MeshTile target)
        {
            if (tile == null || target == null) return;

            int targetNum = DecodePolyIdTile(GetTileRef(target));

            for (int i = 0; i < tile.header.polyCount; ++i)
            {
                Poly poly = tile.polys[i];
                int j = poly.firstLink;
                int pj = Constants.DT_NULL_LINK;
                while (j != Constants.DT_NULL_LINK)
                {
                    if (DecodePolyIdTile((int)tile.links[j].nref) == targetNum)
                    {
                        // Remove link.
                        int nj = tile.links[j].next;
                        if (pj == Constants.DT_NULL_LINK)
                            poly.firstLink = nj;
                        else
                            tile.links[pj].next = nj;
                        FreeLink(tile, j);
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
        private void FreeLink(MeshTile tile, int link)
        {
            tile.links[link].next = tile.linksFreeList;
            tile.linksFreeList = link;
        }

        public IGraphNode[] GetNodes(AgentType agent)
        {
            List<GraphNode> nodes = new List<GraphNode>();

            nodes.AddRange(GraphNode.Build(this));

            return nodes.ToArray();
        }
        public Vector3[] FindPath(AgentType agent, Vector3 from, Vector3 to)
        {
            return null;
        }
        public bool IsWalkable(AgentType agent, Vector3 position, out Vector3? nearest)
        {
            nearest = null;
            return false;
        }
        public void Save(string fileName)
        {
            SaveFile(fileName, this);
        }
        public void Load(string fileName)
        {
            var nm = LoadFile(fileName);
            if (nm != null)
            {
                this.m_params = nm.m_params;
                this.m_orig = nm.m_orig;
                this.m_tileWidth = nm.m_tileWidth;
                this.m_tileHeight = nm.m_tileHeight;
                this.m_tileLutSize = nm.m_tileLutSize;
                this.m_tileLutMask = nm.m_tileLutMask;
                this.m_posLookup = nm.m_posLookup;
                this.m_nextFree = nm.m_nextFree;
                this.m_tileBits = nm.m_tileBits;
                this.m_polyBits = nm.m_polyBits;
                this.m_saltBits = nm.m_saltBits;
                this.MaxTiles = nm.MaxTiles;
                this.Tiles = nm.Tiles;
            }
        }
    }

    public class GraphNode : IGraphNode
    {
        public static GraphNode[] Build(NavigationMesh2 mesh)
        {
            List<GraphNode> nodes = new List<GraphNode>();

            for (int i = 0; i < mesh.MaxTiles; ++i)
            {
                var tile = mesh.Tiles[i];
                if (tile.header.magic != Constants.Magic) continue;

                for (int t = 0; t < tile.header.polyCount; t++)
                {
                    var p = tile.polys[t];
                    if (p.Type == PolyTypes.OffmeshConnection) continue;

                    var bse = mesh.GetPolyRefBase(tile);

                    int tileNum = mesh.DecodePolyIdTile(bse);
                    var tileColor = Helper.IntToCol(tileNum, 128);

                    var pd = tile.detailMeshes[t];

                    List<Triangle> tris = new List<Triangle>();

                    for (int j = 0; j < pd.triCount; ++j)
                    {
                        var dt = tile.detailTris[(pd.triBase + j)];
                        Vector3[] triVerts = new Vector3[3];
                        for (int k = 0; k < 3; ++k)
                        {
                            if (dt[k] < p.vertCount)
                            {
                                triVerts[k] = tile.verts[p.verts[dt[k]]];
                            }
                            else
                            {
                                triVerts[k] = tile.detailVerts[(pd.vertBase + dt[k] - p.vertCount)];
                            }
                        }

                        tris.Add(new Triangle(triVerts[0], triVerts[1], triVerts[2]));
                    }

                    nodes.Add(new GraphNode()
                    {
                        Triangles = tris.ToArray(),
                        TotalCost = 1,
                        Color = tileColor,
                    });
                }
            }

            return nodes.ToArray();
        }

        public Triangle[] Triangles;

        public Vector3 Center
        {
            get
            {
                Vector3 center = Vector3.Zero;

                foreach (var tri in Triangles)
                {
                    center += tri.Center;
                }

                return center / Math.Max(1, Triangles.Length);
            }
        }

        public Color4 Color { get; set; }

        public float TotalCost { get; set; }

        public bool Contains(Vector3 point, out float distance)
        {
            distance = float.MaxValue;
            foreach (var tri in Triangles)
            {
                if (Intersection.PointInPoly(point, tri.GetVertices()))
                {
                    float d = Intersection.PointToTriangle(point, tri.Point1, tri.Point2, tri.Point3);
                    if (d == 0)
                    {
                        distance = 0;
                        return true;
                    }

                    distance = Math.Min(distance, d);
                }
            }

            return false;
        }

        public Vector3[] GetPoints()
        {
            List<Vector3> vList = new List<Vector3>();

            foreach (var tri in Triangles)
            {
                vList.AddRange(tri.GetVertices());
            }

            return vList.ToArray();
        }
    }
}
