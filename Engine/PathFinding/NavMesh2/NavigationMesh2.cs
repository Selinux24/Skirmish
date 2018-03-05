using SharpDX;
using System;
using System.Collections.Generic;
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
                    int ntiles = RasterizeTileLayers(
                        x, y, settings, cfg,
                        geometry,
                        out TileCacheData[] tiles);

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

        private static void CalcGridSize(BoundingBox b, float cellSize, out int w, out int h)
        {
            w = (int)((b.Maximum.X - b.Minimum.X) / cellSize + 0.5f);
            h = (int)((b.Maximum.Z - b.Minimum.Z) / cellSize + 0.5f);
        }
        private static int RasterizeTileLayers(int tx, int ty, BuildSettings settings, Config cfg, InputGeometry geometry, out TileCacheData[] tiles)
        {
            tiles = new TileCacheData[TileCache.MaxLayers];

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

                Helper.InitializeArray<TileCacheAreas>(rc.triareas, TileCacheAreas.NullArea);

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

            rc.chf = BuildCompactHeightfield(tcfg.WalkableHeight, tcfg.WalkableClimb, rc.solid);

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
            for (int i = 0; i < Math.Min(rc.lset.nlayers, TileCache.MaxLayers); i++)
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
            for (int i = 0; i < Math.Min(rc.ntiles, TileCache.MaxLayers); i++)
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
                    for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; ++i)
                    {
                        CompactSpan s = chf.spans[i];
                        if (chf.areas[i] == (byte)TileCacheAreas.NullArea)
                        {
                            continue;
                        }

                        if ((int)s.y >= miny && (int)s.y <= maxy)
                        {
                            Vector3 p = new Vector3();
                            p[0] = chf.boundingBox.Minimum[0] + (x + 0.5f) * chf.cs;
                            p[1] = 0;
                            p[2] = chf.boundingBox.Minimum[2] + (z + 0.5f) * chf.cs;

                            if (PointInPoly(nverts, verts, p))
                            {
                                chf.areas[i] = areaId;
                            }
                        }
                    }
                }
            }
        }
        private static bool PointInPoly(int nvert, Vector3[] verts, Vector3 p)
        {
            bool c = false;

            for (int i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                Vector3 vi = verts[i];
                Vector3 vj = verts[j];
                if (((vi[2] > p[2]) != (vj[2] > p[2])) &&
                    (p[0] < (vj[0] - vi[0]) * (p[2] - vi[2]) / (vj[2] - vi[2]) + vi[0]))
                {
                    c = !c;
                }
            }

            return c;
        }
        private static bool BuildHeightfieldLayers(CompactHeightfield chf, int borderSize, int walkableHeight, out HeightfieldLayerSet lset)
        {
            lset = new HeightfieldLayerSet();

            int w = chf.width;
            int h = chf.height;

            byte[] srcReg = Helper.CreateArray<byte>(chf.spanCount, 0xff);

            int nsweeps = chf.width;
            LayerSweepSpan[] sweeps = Helper.CreateArray<LayerSweepSpan>(nsweeps, new LayerSweepSpan());

            // Partition walkable area into monotone regions.
            byte regId = 0;

            for (int y = borderSize; y < h - borderSize; ++y)
            {
                int[] prevCount = Helper.CreateArray<int>(256, 0);
                byte sweepId = 0;

                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    CompactCell c = chf.cells[x + y * w];

                    for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; ++i)
                    {
                        CompactSpan s = chf.spans[i];
                        if (chf.areas[i] == (byte)TileCacheAreas.NullArea) continue;

                        byte sid = 0xff;

                        // -x
                        if (GetCon(s, 0) != Constants.NotConnected)
                        {
                            int ax = x + GetDirOffsetX(0);
                            int ay = y + GetDirOffsetY(0);
                            int ai = (int)chf.cells[ax + ay * w].index + GetCon(s, 0);
                            if (chf.areas[ai] != (byte)TileCacheAreas.NullArea && srcReg[ai] != 0xff)
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
                            int ax = x + GetDirOffsetX(3);
                            int ay = y + GetDirOffsetY(3);
                            int ai = (int)chf.cells[ax + ay * w].index + GetCon(s, 3);
                            byte nr = srcReg[ai];
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
                    CompactCell c = chf.cells[x + y * w];
                    for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; ++i)
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
            LayerRegion[] regs = Helper.CreateArray<LayerRegion>(nregs, () => (LayerRegion.Default));

            // Find region neighbours and overlapping regions.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    CompactCell c = chf.cells[x + y * w];

                    byte[] lregs = new byte[LayerRegion.MaxLayers];
                    int nlregs = 0;

                    for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; ++i)
                    {
                        CompactSpan s = chf.spans[i];
                        byte ri = srcReg[i];
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
                                int ax = x + GetDirOffsetX(dir);
                                int ay = y + GetDirOffsetY(dir);
                                int ai = (int)chf.cells[ax + ay * w].index + GetCon(s, dir);
                                byte rai = srcReg[ai];
                                if (rai != 0xff && rai != ri)
                                {
                                    // Don't check return value -- if we cannot add the neighbor
                                    // it will just cause a few more regions to be created, which
                                    // is fine.
                                    AddUnique(regs[ri].neis, ref regs[ri].nneis, LayerRegion.MaxNeighbors, rai);
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
                                LayerRegion ri = regs[lregs[i]];
                                LayerRegion rj = regs[lregs[j]];

                                if (!AddUnique(ri.layers, ref ri.nlayers, LayerRegion.MaxLayers, lregs[j]) ||
                                    !AddUnique(rj.layers, ref rj.nlayers, LayerRegion.MaxLayers, lregs[i]))
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
            byte layerId = 0;

            int MaxStack = 64;
            byte[] stack = new byte[MaxStack];
            int nstack = 0;

            for (int i = 0; i < nregs; ++i)
            {
                LayerRegion root = regs[i];

                // Skip already visited.
                if (root.layerId != 0xff)
                {
                    continue;
                }

                // Start search.
                root.layerId = layerId;
                root.isBase = true;

                nstack = 0;
                stack[nstack++] = (byte)i;

                while (nstack != 0)
                {
                    // Pop front
                    LayerRegion reg = regs[stack[0]];
                    nstack--;
                    for (int j = 0; j < nstack; ++j)
                    {
                        stack[j] = stack[j + 1];
                    }

                    int nneis = reg.nneis;
                    for (int j = 0; j < nneis; ++j)
                    {
                        byte nei = reg.neis[j];
                        LayerRegion regn = regs[nei];

                        // Skip already visited.
                        if (regn.layerId != 0xff)
                        {
                            continue;
                        }

                        // Skip if the neighbour is overlapping root region.
                        if (Contains(root.layers, root.nlayers, nei))
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
                                if (!AddUnique(root.layers, ref root.nlayers, LayerRegion.MaxLayers, regn.layers[k]))
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
            ushort mergeHeight = (ushort)(walkableHeight * 4);

            for (int i = 0; i < nregs; ++i)
            {
                LayerRegion ri = regs[i];

                if (!ri.isBase)
                {
                    continue;
                }

                byte newId = ri.layerId;

                for (; ; )
                {
                    byte oldId = 0xff;

                    for (int j = 0; j < nregs; ++j)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        LayerRegion rj = regs[j];
                        if (!rj.isBase)
                        {
                            continue;
                        }

                        // Skip if the regions are not close to each other.
                        if (!OverlapRange(ri.ymin, (ushort)(ri.ymax + mergeHeight), rj.ymin, (ushort)(rj.ymax + mergeHeight)))
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
                            if (Contains(ri.layers, ri.nlayers, (byte)k))
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
                        LayerRegion rj = regs[j];

                        if (rj.layerId == oldId)
                        {
                            rj.isBase = false;
                            // Remap layerIds.
                            rj.layerId = newId;
                            // Add overlaid layers from 'rj' to 'ri'.
                            for (int k = 0; k < rj.nlayers; ++k)
                            {
                                if (!AddUnique(ri.layers, ref ri.nlayers, LayerRegion.MaxLayers, rj.layers[k]))
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
            byte[] remap = new byte[256];

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
                byte curId = (byte)i;

                HeightfieldLayer layer = lset.layers[i];

                int gridSize = lw * lh;

                layer.heights = Helper.CreateArray<byte>(gridSize, 0xff);
                layer.areas = Helper.CreateArray(gridSize, TileCacheAreas.NullArea);
                layer.cons = Helper.CreateArray<byte>(gridSize, 0x00);

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

                layer.width = (byte)lw;
                layer.height = (byte)lh;
                layer.cs = chf.cs;
                layer.ch = chf.ch;

                // Adjust the bbox to fit the heightfield.
                layer.boundingBox = new BoundingBox(bmin, bmax);
                layer.boundingBox.Minimum[1] = bmin[1] + hmin * chf.ch;
                layer.boundingBox.Maximum[1] = bmin[1] + hmax * chf.ch;
                layer.hmin = (ushort)hmin;
                layer.hmax = (ushort)hmax;

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
                        CompactCell c = chf.cells[cx + cy * w];
                        for (int j = (int)c.index, nj = (int)(c.index + c.count); j < nj; ++j)
                        {
                            CompactSpan s = chf.spans[j];
                            // Skip unassigned regions.
                            if (srcReg[j] == 0xff)
                            {
                                continue;
                            }

                            // Skip of does nto belong to current layer.
                            byte lid = regs[srcReg[j]].layerId;
                            if (lid != curId)
                            {
                                continue;
                            }

                            // Update data bounds.
                            layer.minx = (byte)Math.Min(layer.minx, x);
                            layer.maxx = (byte)Math.Max(layer.maxx, x);
                            layer.miny = (byte)Math.Min(layer.miny, y);
                            layer.maxy = (byte)Math.Max(layer.maxy, y);

                            // Store height and area type.
                            int idx = x + y * lw;
                            layer.heights[idx] = (byte)(s.y - hmin);
                            layer.areas[idx] = chf.areas[j];

                            // Check connection.
                            byte portal = 0;
                            byte con = 0;
                            for (int dir = 0; dir < 4; ++dir)
                            {
                                if (GetCon(s, dir) != Constants.NotConnected)
                                {
                                    int ax = cx + GetDirOffsetX(dir);
                                    int ay = cy + GetDirOffsetY(dir);
                                    int ai = (int)chf.cells[ax + ay * w].index + GetCon(s, dir);
                                    byte alid = (byte)(srcReg[ai] != 0xff ? regs[srcReg[ai]].layerId : 0xff);
                                    // Portal mask
                                    if (chf.areas[ai] != (byte)TileCacheAreas.NullArea && lid != alid)
                                    {
                                        portal |= (byte)(1 << dir);

                                        // Update height so that it matches on both sides of the portal.
                                        CompactSpan ass = chf.spans[ai];
                                        if (ass.y > hmin)
                                        {
                                            layer.heights[idx] = Math.Max(layer.heights[idx], (byte)(ass.y - hmin));
                                        }
                                    }
                                    // Valid connection mask
                                    if (chf.areas[ai] != (byte)TileCacheAreas.NullArea && lid == alid)
                                    {
                                        int nx = ax - borderSize;
                                        int ny = ay - borderSize;
                                        if (nx >= 0 && ny >= 0 && nx < lw && ny < lh)
                                        {
                                            con |= (byte)(1 << dir);
                                        }
                                    }
                                }
                            }

                            layer.cons[idx] = (byte)((portal << 4) | con);
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
        private static bool OverlapRange(ushort amin, ushort amax, ushort bmin, ushort bmax)
        {
            return (amin > bmax || amax < bmin) ? false : true;
        }
        private static bool Contains(byte[] a, byte an, byte v)
        {
            int n = (int)an;

            for (int i = 0; i < n; ++i)
            {
                if (a[i] == v)
                {
                    return true;
                }
            }

            return false;
        }
        private static bool AddUnique(byte[] a, ref byte an, int anMax, byte v)
        {
            if (Contains(a, an, v))
            {
                return true;
            }

            if ((int)an >= anMax)
            {
                return false;
            }

            a[an] = v;
            an++;

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
                        bool walkable = s.area != (byte)TileCacheAreas.NullArea;

                        // If current span is not walkable, but there is walkable span just below it, mark the span above it walkable too.
                        if (!walkable && previousWalkable)
                        {
                            if (Math.Abs((int)s.smax - (int)ps.smax) <= walkableClimb)
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
                        if (s.area == (byte)TileCacheAreas.NullArea)
                        {
                            continue;
                        }

                        int bot = (int)(s.smax);
                        int top = s.next != null ? (int)(s.next.smin) : int.MaxValue;

                        // Find neighbours minimum height.
                        int minh = int.MaxValue;

                        // Min and max height of accessible neighbours.
                        int asmin = (int)s.smax;
                        int asmax = (int)s.smax;

                        for (int dir = 0; dir < 4; ++dir)
                        {
                            // Skip neighbours which are out of bounds.
                            int dx = x + GetDirOffsetX(dir);
                            int dy = y + GetDirOffsetY(dir);
                            if (dx < 0 || dy < 0 || dx >= w || dy >= h)
                            {
                                minh = Math.Min(minh, -walkableClimb - bot);
                                continue;
                            }

                            // From minus infinity to the first span.
                            Span ns = solid.spans[dx + dy * w];
                            int nbot = -walkableClimb;
                            int ntop = ns != null ? (int)ns.smin : int.MaxValue;

                            // Skip neightbour if the gap between the spans is too small.
                            if (Math.Min(top, ntop) - Math.Max(bot, nbot) > walkableHeight)
                            {
                                minh = Math.Min(minh, nbot - bot);
                            }

                            // Rest of the spans.
                            ns = solid.spans[dx + dy * w];
                            while (ns != null)
                            {
                                nbot = (int)ns.smax;
                                ntop = ns.next != null ? (int)ns.next.smin : int.MaxValue;

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
                            s.area = (byte)TileCacheAreas.NullArea;
                        }
                        else if ((asmax - asmin) > walkableClimb)
                        {
                            // If the difference between all neighbours is too large, we are at steep slope, mark the span as ledge.
                            s.area = (byte)TileCacheAreas.NullArea;
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
                    for (Span s = solid.spans[x + y * w]; s != null; s = s.next)
                    {
                        int bot = (int)(s.smax);
                        int top = s.next != null ? (int)(s.next.smin) : int.MaxValue;

                        if ((top - bot) <= walkableHeight)
                        {
                            s.area = (byte)TileCacheAreas.NullArea;
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
            byte[] dist = Helper.CreateArray<byte>(chf.spanCount, 0xff);

            // Mark boundary cells.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    CompactCell c = chf.cells[x + y * w];
                    for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; ++i)
                    {
                        if (chf.areas[i] == (byte)TileCacheAreas.NullArea)
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
                                    int nx = x + GetDirOffsetX(dir);
                                    int ny = y + GetDirOffsetY(dir);
                                    int nidx = (int)chf.cells[nx + ny * w].index + GetCon(s, dir);
                                    if (chf.areas[nidx] != (byte)TileCacheAreas.NullArea)
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

            byte nd;

            // Pass 1
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    CompactCell c = chf.cells[x + y * w];
                    for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; ++i)
                    {
                        CompactSpan s = chf.spans[i];
                        if (GetCon(s, 0) != Constants.NotConnected)
                        {
                            // (-1,0)
                            int ax = x + GetDirOffsetX(0);
                            int ay = y + GetDirOffsetY(0);
                            int ai = (int)chf.cells[ax + ay * w].index + GetCon(s, 0);
                            CompactSpan asp = chf.spans[ai];
                            nd = (byte)Math.Min((int)dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (-1,-1)
                            if (GetCon(asp, 3) != Constants.NotConnected)
                            {
                                int aax = ax + GetDirOffsetX(3);
                                int aay = ay + GetDirOffsetY(3);
                                int aai = (int)chf.cells[aax + aay * w].index + GetCon(asp, 3);
                                nd = (byte)Math.Min((int)dist[aai] + 3, 255);
                                if (nd < dist[i])
                                {
                                    dist[i] = nd;
                                }
                            }
                        }
                        if (GetCon(s, 3) != Constants.NotConnected)
                        {
                            // (0,-1)
                            int ax = x + GetDirOffsetX(3);
                            int ay = y + GetDirOffsetY(3);
                            int ai = (int)chf.cells[ax + ay * w].index + GetCon(s, 3);
                            CompactSpan asp = chf.spans[ai];
                            nd = (byte)Math.Min((int)dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (1,-1)
                            if (GetCon(asp, 2) != Constants.NotConnected)
                            {
                                int aax = ax + GetDirOffsetX(2);
                                int aay = ay + GetDirOffsetY(2);
                                int aai = (int)chf.cells[aax + aay * w].index + GetCon(asp, 2);
                                nd = (byte)Math.Min((int)dist[aai] + 3, 255);
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
                    CompactCell c = chf.cells[x + y * w];
                    for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; ++i)
                    {
                        CompactSpan s = chf.spans[i];
                        if (GetCon(s, 2) != Constants.NotConnected)
                        {
                            // (1,0)
                            int ax = x + GetDirOffsetX(2);
                            int ay = y + GetDirOffsetY(2);
                            int ai = (int)chf.cells[ax + ay * w].index + GetCon(s, 2);
                            CompactSpan asp = chf.spans[ai];
                            nd = (byte)Math.Min((int)dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (1,1)
                            if (GetCon(asp, 1) != Constants.NotConnected)
                            {
                                int aax = ax + GetDirOffsetX(1);
                                int aay = ay + GetDirOffsetY(1);
                                int aai = (int)chf.cells[aax + aay * w].index + GetCon(asp, 1);
                                nd = (byte)Math.Min((int)dist[aai] + 3, 255);
                                if (nd < dist[i])
                                {
                                    dist[i] = nd;
                                }
                            }
                        }
                        if (GetCon(s, 1) != Constants.NotConnected)
                        {
                            // (0,1)
                            int ax = x + GetDirOffsetX(1);
                            int ay = y + GetDirOffsetY(1);
                            int ai = (int)chf.cells[ax + ay * w].index + GetCon(s, 1);
                            CompactSpan asp = chf.spans[ai];
                            nd = (byte)Math.Min((int)dist[ai] + 2, 255);
                            if (nd < dist[i])
                            {
                                dist[i] = nd;
                            }

                            // (-1,1)
                            if (GetCon(asp, 0) != Constants.NotConnected)
                            {
                                int aax = ax + GetDirOffsetX(0);
                                int aay = ay + GetDirOffsetY(0);
                                int aai = (int)chf.cells[aax + aay * w].index + GetCon(asp, 0);
                                nd = (byte)Math.Min((int)dist[aai] + 3, 255);
                                if (nd < dist[i])
                                {
                                    dist[i] = nd;
                                }
                            }
                        }
                    }
                }
            }

            byte thr = (byte)(radius * 2);
            for (int i = 0; i < chf.spanCount; ++i)
            {
                if (dist[i] < thr)
                {
                    chf.areas[i] = (byte)TileCacheAreas.NullArea;
                }
            }

            dist = null;

            return true;
        }
        private static int GetDirOffsetX(int dir)
        {
            int[] offset = new[] { -1, 0, 1, 0, };
            return offset[dir & 0x03];
        }
        private static int GetDirOffsetY(int dir)
        {
            int[] offset = new[] { 0, 1, 0, -1 };
            return offset[dir & 0x03];
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
                DividePoly(inb, zp1, zp2, cz + cs, 2);
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
                    DividePoly(zp1, xp1, xp2, cx + cs, 0);
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
                    ushort ismin = (ushort)MathUtil.Clamp((int)Math.Floor(minY * ich), 0, Span.SpanMaxHeight);
                    ushort ismax = (ushort)MathUtil.Clamp((int)Math.Ceiling(maxY * ich), ismin + 1, Span.SpanMaxHeight);

                    if (!AddSpan(hf, x, y, ismin, ismax, area, flagMergeThr))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        private static bool AddSpan(Heightfield hf, int x, int y, ushort smin, ushort smax, TileCacheAreas area, int flagMergeThr)
        {
            int idx = x + y * hf.width;

            Span s = new Span
            {
                smin = smin,
                smax = smax,
                area = area,
                next = null
            };

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
                        s.area = (TileCacheAreas)Math.Max((byte)s.area, (byte)cur.area);
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
        private static void FreeSpan(Heightfield hf, Span cur)
        {
            if (cur == null) return;

            // Add the node in front of the free list.
            cur.next = hf.freelist;
            hf.freelist = cur;
        }
        private static void DividePoly(
            List<Vector3> inPoly,
            List<Vector3> outPoly1,
            List<Vector3> outPoly2,
            float x, int axis)
        {
            float[] d = new float[inPoly.Count];
            for (int i = 0; i < inPoly.Count; i++)
            {
                d[i] = x - inPoly[i][axis];
            }

            for (int i = 0, j = inPoly.Count - 1; i < inPoly.Count; j = i, i++)
            {
                bool ina = d[j] >= 0;
                bool inb = d[i] >= 0;
                if (ina != inb)
                {
                    float s = d[j] / (d[j] - d[i]);
                    Vector3 v;
                    v.X = inPoly[j].X + (inPoly[i].X - inPoly[j].X) * s;
                    v.Y = inPoly[j].Y + (inPoly[i].Y - inPoly[j].Y) * s;
                    v.Z = inPoly[j].Z + (inPoly[i].Z - inPoly[j].Z) * s;
                    outPoly1.Add(v);
                    outPoly2.Add(v);

                    // add the i'th point to the right polygon. Do NOT add points that are on the dividing line
                    // since these were already added above
                    if (d[i] > 0)
                    {
                        outPoly1.Add(inPoly[i]);
                    }
                    else if (d[i] < 0)
                    {
                        outPoly2.Add(inPoly[i]);
                    }
                }
                else // same side
                {
                    // add the i'th point to the right polygon. Addition is done even for points on the dividing line
                    if (d[i] >= 0)
                    {
                        outPoly1.Add(inPoly[i]);

                        if (d[i] != 0)
                        {
                            continue;
                        }
                    }

                    outPoly2.Add(inPoly[i]);
                }
            }
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
        private static CompactHeightfield BuildCompactHeightfield(int walkableHeight, int walkableClimb, Heightfield hf)
        {
            CompactHeightfield chf = new CompactHeightfield();

            int w = hf.width;
            int h = hf.height;
            int spanCount = GetHeightFieldSpanCount(hf);

            // Fill in header.
            chf.width = w;
            chf.height = h;
            chf.spanCount = spanCount;
            chf.walkableHeight = walkableHeight;
            chf.walkableClimb = walkableClimb;
            chf.maxRegions = 0;
            chf.boundingBox = hf.boundingBox;
            chf.boundingBox.Maximum.Y += walkableHeight * hf.ch;
            chf.cs = hf.cs;
            chf.ch = hf.ch;
            chf.cells = new CompactCell[w * h];
            chf.spans = new CompactSpan[spanCount];
            chf.areas = new TileCacheAreas[spanCount];

            // Fill in cells and spans.
            int idx = 0;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    Span s = hf.spans[x + y * w];

                    // If there are no spans at this cell, just leave the data to index=0, count=0.
                    if (s == null) continue;

                    CompactCell c = new CompactCell();
                    c.index = (uint)idx;
                    c.count = 0;
                    while (s != null)
                    {
                        if (s.area != (byte)TileCacheAreas.NullArea)
                        {
                            int bot = (int)s.smax;
                            int top = s.next != null ? (int)s.next.smin : int.MaxValue;
                            chf.spans[idx].y = (ushort)MathUtil.Clamp(bot, 0, 0xffff);
                            chf.spans[idx].h = (byte)MathUtil.Clamp(top - bot, 0, 0xff);
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
                    CompactCell c = chf.cells[x + y * w];
                    for (int i = (int)c.index, ni = (int)(c.index + c.count); i < ni; i++)
                    {
                        CompactSpan s = chf.spans[i];

                        for (int dir = 0; dir < 4; dir++)
                        {
                            SetCon(ref s, dir, Constants.NotConnected);
                            int nx = x + GetDirOffsetX(dir);
                            int ny = y + GetDirOffsetY(dir);
                            // First check that the neighbour cell is in bounds.
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                            {
                                continue;
                            }

                            // Iterate over all neighbour spans and check if any of the is
                            // accessible from current cell.
                            CompactCell nc = chf.cells[nx + ny * w];
                            for (int k = (int)nc.index, nk = (int)(nc.index + nc.count); k < nk; ++k)
                            {
                                CompactSpan ns = chf.spans[k];
                                int bot = Math.Max(s.y, ns.y);
                                int top = (int)Math.Min(s.y + s.h, ns.y + ns.h);

                                // Check that the gap between the spans is walkable,
                                // and that the climb height between the gaps is not too high.
                                if ((top - bot) >= walkableHeight && Math.Abs((int)ns.y - (int)s.y) <= walkableClimb)
                                {
                                    // Mark direction as walkable.
                                    int lidx = k - (int)nc.index;
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

            return chf;
        }
        private static void SetCon(ref CompactSpan s, int dir, int i)
        {
            uint shift = (uint)dir * 6;
            uint con = s.con;
            s.con = (uint)(((int)con & ~(0x3f << (int)shift)) | ((i & 0x3f) << (int)shift));
        }
        private static int GetCon(CompactSpan s, int dir)
        {
            uint shift = (uint)dir * 6;
            return ((int)s.con >> (int)shift) & 0x3f;
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
                        if (s.area != (byte)TileCacheAreas.NullArea)
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
                bool overlap = CheckOverlapRect(bmin, bmax, node.bmin, node.bmax);
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
        private static bool CheckOverlapRect(Vector2 amin, Vector2 amax, Vector2 bmin, Vector2 bmax)
        {
            bool overlap = true;
            overlap = (amin.X > bmax.X || amax.X < bmin.X) ? false : overlap;
            overlap = (amin.Y > bmax.Y || amax.Y < bmin.Y) ? false : overlap;
            return overlap;
        }
        private static int CalcLayerBufferSize(int gridWidth, int gridHeight)
        {
            int headerSize = Helper.Align4(TileCacheLayerHeader.Size);
            int gridSize = gridWidth * gridHeight;

            return headerSize + gridSize * 4;
        }

        private Vector3 m_orig;
        private float m_tileWidth;
        private float m_tileHeight;
        private int m_maxTiles;
        private int m_tileLutSize;
        private int m_tileLutMask;
        private MeshTile[] m_tiles;
        private MeshTile[] m_posLookup;
        private MeshTile m_nextFree = null;
        private int m_tileBits;
        private int m_polyBits;
        private int m_saltBits;

        /// <summary>
        /// Constructor
        /// </summary>
        public NavigationMesh2()
        {

        }

        public void Init(NavMeshParams nmparams)
        {
            m_orig = nmparams.Origin;
            m_tileWidth = nmparams.TileWidth;
            m_tileHeight = nmparams.TileHeight;

            // Init tiles
            m_maxTiles = nmparams.MaxTiles;
            m_tileLutSize = Helper.NextPowerOfTwo(nmparams.MaxTiles / 4);
            if (m_tileLutSize == 0) m_tileLutSize = 1;
            m_tileLutMask = m_tileLutSize - 1;

            m_tiles = new MeshTile[m_maxTiles];
            m_posLookup = new MeshTile[m_tileLutSize];

            m_nextFree = null;
            for (int i = m_maxTiles - 1; i >= 0; --i)
            {
                m_tiles[i] = new MeshTile
                {
                    salt = 1,
                    next = m_nextFree
                };
                m_nextFree = m_tiles[i];
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
        public MeshTile GetTileRefAt(int x, int y, int layer)
        {
            // Find tile based on hash.
            int h = TileCache.ComputeTileHash(x, y, m_tileLutMask);
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
        public bool AddTile(MeshData data, TileFlags flags, ref int lastRef, out int result)
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
                if (tileIndex >= m_maxTiles)
                {
                    return false;
                }
                // Try to find the specific tile id from the free list.
                MeshTile target = m_tiles[tileIndex];
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
            int h = TileCache.ComputeTileHash(header.x, header.y, m_tileLutMask);
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
            int h = ComputeTileHash(x, y, m_tileLutMask);
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
            int h = ComputeTileHash(x, y, m_tileLutMask);
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
        private int ComputeTileHash(int x, int y, int mask)
        {
            uint h1 = 0x8da6b343; // Large multiplicative constants;
            uint h2 = 0xd8163841; // here arbitrarily chosen primes
            uint n = (uint)(h1 * x + h2 * y);
            return (int)(n & mask);
        }
        private int DecodePolyIdTile(int r)
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
        private int GetPolyRefBase(MeshTile tile)
        {
            if (tile == null) return 0;
            int it = Array.IndexOf(m_tiles, tile);
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
                Vector3i bmin = new Vector3i();
                Vector3i bmax = new Vector3i();
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

                    bool overlap = OverlapQuantBounds(bmin, bmax, node.bmin, node.bmax);
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
                    if (OverlapBounds(qmin, qmax, bmin, bmax))
                    {
                        if (n < maxPolys)

                            polys[n++] = bse | i;
                    }
                }
                return n;
            }
        }
        private bool OverlapQuantBounds(Vector3i amin, Vector3i amax, Vector3i bmin, Vector3i bmax)
        {
            bool overlap = true;
            overlap = (amin.X > bmax.X || amax.X < bmin.X) ? false : overlap;
            overlap = (amin.Y > bmax.Y || amax.Y < bmin.Y) ? false : overlap;
            overlap = (amin.Z > bmax.Z || amax.Z < bmin.Z) ? false : overlap;
            return overlap;
        }
        private bool OverlapBounds(Vector3 amin, Vector3 amax, Vector3 bmin, Vector3 bmax)
        {
            bool overlap = true;
            overlap = (amin.X > bmax.X || amax.X < bmin.X) ? false : overlap;
            overlap = (amin.Y > bmax.Y || amax.Y < bmin.Y) ? false : overlap;
            overlap = (amin.Z > bmax.Z || amax.Z < bmin.Z) ? false : overlap;
            return overlap;
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
            PolyDetail pd = tile.detailMeshes[ip];

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
                Trianglei t = tile.detailTris[(pd.triBase + j)];
                Vector3[] v = new Vector3[3];
                for (int k = 0; k < 3; ++k)
                {
                    if (t[k] < poly.vertCount)
                        v[k] = tile.verts[poly.verts[t[k]]];
                    else
                        v[k] = tile.detailVerts[(pd.vertBase + (t[k] - poly.vertCount))];
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
            tile = m_tiles[it];
            poly = m_tiles[it].polys[ip];
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

                            // Compress portal limits to a byte value.
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

            CalcSlabEndPoints(va, vb, out Vector2 amin, out Vector2 amax, side);
            float apos = GetSlabCoord(va, side);

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
                    float bpos = GetSlabCoord(vc, side);

                    // Segments are not close enough.
                    if (Math.Abs(apos - bpos) > 0.01f)
                        continue;

                    // Check if the segments touch.
                    CalcSlabEndPoints(vc, vd, out Vector2 bmin, out Vector2 bmax, side);

                    if (!OverlapSlabs(amin, amax, bmin, bmax, 0.01f, tile.header.walkableClimb)) continue;

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
        private static void CalcSlabEndPoints(Vector3 va, Vector3 vb, out Vector2 bmin, out Vector2 bmax, int side)
        {
            bmin = new Vector2();
            bmax = new Vector2();

            if (side == 0 || side == 4)
            {
                if (va[2] < vb[2])
                {
                    bmin[0] = va[2];
                    bmin[1] = va[1];
                    bmax[0] = vb[2];
                    bmax[1] = vb[1];
                }
                else
                {
                    bmin[0] = vb[2];
                    bmin[1] = vb[1];
                    bmax[0] = va[2];
                    bmax[1] = va[1];
                }
            }
            else if (side == 2 || side == 6)
            {
                if (va[0] < vb[0])
                {
                    bmin[0] = va[0];
                    bmin[1] = va[1];
                    bmax[0] = vb[0];
                    bmax[1] = vb[1];
                }
                else
                {
                    bmin[0] = vb[0];
                    bmin[1] = vb[1];
                    bmax[0] = va[0];
                    bmax[1] = va[1];
                }
            }
        }
        private static float GetSlabCoord(Vector3 va, int side)
        {
            if (side == 0 || side == 4)
                return va[0];
            else if (side == 2 || side == 6)
                return va[2];
            return 0;
        }
        private static bool OverlapSlabs(Vector2 amin, Vector2 amax, Vector2 bmin, Vector2 bmax, float px, float py)
        {
            // Check for horizontal overlap.
            // The segment is shrunken a little so that slabs which touch
            // at end points are not connected.
            float minx = Math.Max(amin[0] + px, bmin[0] + px);
            float maxx = Math.Min(amax[0] - px, bmax[0] - px);
            if (minx > maxx)
                return false;

            // Check vertical overlap.
            float ad = (amax[1] - amin[1]) / (amax[0] - amin[0]);
            float ak = amin[1] - ad * amin[0];
            float bd = (bmax[1] - bmin[1]) / (bmax[0] - bmin[0]);
            float bk = bmin[1] - bd * bmin[0];
            float aminy = ad * minx + ak;
            float amaxy = ad * maxx + ak;
            float bminy = bd * minx + bk;
            float bmaxy = bd * maxx + bk;
            float dmin = bminy - aminy;
            float dmax = bmaxy - amaxy;

            // Crossing segments always overlap.
            if (dmin * dmax < 0)
                return true;

            // Check for overlap at endpoints.
            float thr = (float)Math.Sqrt(py * 2);
            if (dmin * dmin <= thr || dmax * dmax <= thr)
                return true;

            return false;
        }
        private int GetTileRef(MeshTile tile)
        {
            if (tile == null) return 0;
            int it = Array.IndexOf(m_tiles, tile);
            return EncodePolyId(tile.salt, it, 0);
        }
        public bool RemoveTile(MeshTile tile, MeshData data, int dataSize)
        {
            if (tile == null)
            {
                return false;
            }

            // Remove tile from hash lookup.
            int h = ComputeTileHash(tile.header.x, tile.header.y, m_tileLutMask);
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

            GraphNode.Id = 0;

            for (int i = 0; i < m_tiles.Length; i++)
            {
                nodes.AddRange(GraphNode.Build(m_tiles[i]));
                GraphNode.Id++;
            }

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
    }

    public class GraphNode : IGraphNode
    {
        public static int Id = 0;

        public static GraphNode[] Build(MeshTile tile)
        {
            List<GraphNode> nodes = new List<GraphNode>();

            if (tile.header.magic == Constants.Magic)
            {
                for (int i = 0; i < tile.polys.Length; i++)
                {
                    var poly = tile.polys[i];
                    var p = new Polygon(poly.vertCount);

                    for (int n = 0; n < poly.vertCount; n++)
                    {
                        p[n] = tile.verts[poly.verts[n]];
                    }

                    nodes.Add(new GraphNode()
                    {
                        Polygon = p,
                        RegionId = Id,
                        TotalCost = 1,
                    });
                }
            }

            return nodes.ToArray();
        }

        public Polygon Polygon;

        public Vector3 Center
        {
            get { return Polygon.Center; }
        }

        public int RegionId { get; set; }

        public float TotalCost { get; set; }

        public bool Contains(Vector3 point, out float distance)
        {
            distance = float.MaxValue;
            return Polygon.Contains(point);
        }

        public Vector3[] GetPoints()
        {
            return Polygon.Points;
        }
    }
}
