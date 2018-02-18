using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.NavMesh2
{
    public class NavMesh
    {
        const int ExpectedLayersPerTile = 4;
        const byte WalkableArea = 63;
        const byte AreaNull = 0;
        const int NotConnected = 0x3f;

        private static void CalcGridSize(BoundingBox b, float cellSize, out int w, out int h)
        {
            w = (int)((b.Maximum.X - b.Minimum.X) / cellSize + 0.5f);
            h = (int)((b.Maximum.Z - b.Minimum.Z) / cellSize + 0.5f);
        }

        public static NavMesh Build(Triangle[] triangles, BuildSettings settings)
        {
            return Build(new InputGeometry(triangles), settings);
        }

        public static NavMesh Build(InputGeometry geometry, BuildSettings settings)
        {
            var agent = settings.Agents[0];

            var bbox = settings.NavmeshBounds ?? geometry.BoundingBox;

            // Init cache
            int gw;
            int gh;
            CalcGridSize(bbox, settings.CellSize, out gw, out gh);
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
                MaxTiles = tw * th * ExpectedLayersPerTile,
                MaxObstacles = 128,
            };

            var tc = new TileCache();
            tc.Init(tcparams);

            int tileBits = Math.Min((int)Math.Log(Helper.NextPowerOfTwo(tw * th * ExpectedLayersPerTile), 2), 14);
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

            var nmQuery = new NavMeshQuery();
            nmQuery.Init(nm, settings.MaxNodes);

            var tileCache = new TileCache();

            int m_cacheLayerCount = 0;
            int m_cacheCompressedSize = 0;
            int m_cacheRawSize = 0;
            int layerBufferSize = CalcLayerBufferSize(tcparams.Width, tcparams.Height);

            for (int y = 0; y < th; y++)
            {
                for (int x = 0; x < tw; x++)
                {
                    TileCacheData[] tiles;
                    int ntiles = RasterizeTileLayers(x, y, settings, cfg, geometry, out tiles);

                    for (int i = 0; i < ntiles; ++i)
                    {
                        tileCache.AddTile(tiles[i], 1);

                        m_cacheLayerCount++;
                        m_cacheCompressedSize += tiles[i].DataSize;
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

        private Vector3 m_orig;
        private float m_tileWidth;
        private float m_tileHeight;
        private int m_maxTiles;
        private int m_tileLutSize;
        private int m_tileLutMask;
        private MeshTile[] m_tiles;
        private MeshTile[] m_posLookup;
        private MeshTile m_nextFree = null;
        private uint m_tileBits;
        private uint m_polyBits;
        private uint m_saltBits;

        /// <summary>
        /// Constructor
        /// </summary>
        public NavMesh()
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
                    Salt = 1,
                    Next = m_nextFree
                };
                m_nextFree = m_tiles[i];
            }

            // Init ID generator values.
            m_tileBits = (uint)Math.Log(Helper.NextPowerOfTwo(nmparams.MaxTiles), 2);
            m_polyBits = (uint)Math.Log(Helper.NextPowerOfTwo(nmparams.MaxPolys), 2);
            // Only allow 31 salt bits, since the salt mask is calculated using 32bit uint and it will overflow.
            m_saltBits = Math.Min(31, 32 - m_tileBits - m_polyBits);

            if (m_saltBits < 10)
            {
                throw new EngineException("DT_INVALID_PARAM");
            }
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
                triareas = new byte[chunkyMesh.maxTrisPerChunk],

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

                Helper.InitializeArray<byte>(rc.triareas, 0);

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

            FastLZCompressor comp;
            rc.ntiles = 0;
            for (int i = 0; i < Math.Min(rc.lset.nlayers, TileCache.MaxLayers); i++)
            {
                TileCacheData tile = rc.tiles[rc.ntiles++];
                HeightfieldLayer layer = rc.lset.layers[i];

                // Store header
                TileCacheLayerHeader header;
                header.magic = TileCacheLayerHeader.TileCacheMagic;
                header.version = TileCacheLayerHeader.TileCacheVersion;

                // Tile layer location in the navmesh.
                header.tx = tx;
                header.ty = ty;
                header.tlayer = i;
                header.b = layer.boundingBox;

                // Tile info.
                header.width = layer.width;
                header.height = layer.height;
                header.minx = layer.minx;
                header.maxx = layer.maxx;
                header.miny = layer.miny;
                header.maxy = layer.maxy;
                header.hmin = layer.hmin;
                header.hmax = layer.hmax;

                // TODO: Here we go!
                if (!BuildTileCacheLayer(
                    ref comp, ref header,
                    layer.heights, layer.areas, layer.cons,
                    ref tile.Data, ref tile.DataSize))
                {
                    return 0;
                }
            }

            // Transfer ownsership of tile data from build context to the caller.
            int n = 0;
            for (int i = 0; i < Math.Min(rc.ntiles, TileCache.MaxLayers); i++)
            {
                tiles[n++] = rc.tiles[i];
                rc.tiles[i].Data = null;
                rc.tiles[i].DataSize = 0;
            }

            return n;
        }

        private static void MarkConvexPolyArea(Vector3[] verts, int nverts, float hmin, float hmax, byte areaId, CompactHeightfield chf)
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
                        if (chf.areas[i] == AreaNull)
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

        private static bool BuildTileCacheLayer(
            ref FastLZCompressor comp, ref TileCacheLayerHeader header,
            byte[] heights, byte[] areas, byte[] cons,
            ref byte[] data, ref int dataSize)
        {
            throw new NotImplementedException();
        }

        private static bool BuildHeightfieldLayers(CompactHeightfield chf, int borderSize, int walkableHeight, out HeightfieldLayerSet lset)
        {
            lset = null;

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
                        if (chf.areas[i] == AreaNull) continue;

                        byte sid = 0xff;

                        // -x
                        if (GetCon(s, 0) != NotConnected)
                        {
                            int ax = x + GetDirOffsetX(0);
                            int ay = y + GetDirOffsetY(0);
                            int ai = (int)chf.cells[ax + ay * w].index + GetCon(s, 0);
                            if (chf.areas[ai] != AreaNull && srcReg[ai] != 0xff)
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
                        if (GetCon(s, 3) != NotConnected)
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
                            if (GetCon(s, dir) != NotConnected)
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

            lset = new HeightfieldLayerSet
            {
                nlayers = layerId,
                layers = new HeightfieldLayer[layerId],
            };

            // Store layers.
            for (int i = 0; i < lset.nlayers; ++i)
            {
                byte curId = (byte)i;

                HeightfieldLayer layer = lset.layers[i];

                int gridSize = lw * lh;

                layer.heights = Helper.CreateArray<byte>(gridSize, 0xff);
                layer.areas = Helper.CreateArray<byte>(gridSize, 0x00);
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
                                if (GetCon(s, dir) != NotConnected)
                                {
                                    int ax = cx + GetDirOffsetX(dir);
                                    int ay = cy + GetDirOffsetY(dir);
                                    int ai = (int)chf.cells[ax + ay * w].index + GetCon(s, dir);
                                    byte alid = (byte)(srcReg[ai] != 0xff ? regs[srcReg[ai]].layerId : 0xff);
                                    // Portal mask
                                    if (chf.areas[ai] != AreaNull && lid != alid)
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
                                    if (chf.areas[ai] != AreaNull && lid == alid)
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
                    byte previousArea = AreaNull;

                    Span ps = null;

                    for (Span s = solid.spans[x + y * w]; s != null; ps = s, s = s.next)
                    {
                        bool walkable = s.area != AreaNull;

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
                        if (s.area == AreaNull)
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
                            s.area = AreaNull;
                        }
                        else if ((asmax - asmin) > walkableClimb)
                        {
                            // If the difference between all neighbours is too large, we are at steep slope, mark the span as ledge.
                            s.area = AreaNull;
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
                            s.area = AreaNull;
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
                        if (chf.areas[i] == AreaNull)
                        {
                            dist[i] = 0;
                        }
                        else
                        {
                            CompactSpan s = chf.spans[i];
                            int nc = 0;
                            for (int dir = 0; dir < 4; ++dir)
                            {
                                if (GetCon(s, dir) != NotConnected)
                                {
                                    int nx = x + GetDirOffsetX(dir);
                                    int ny = y + GetDirOffsetY(dir);
                                    int nidx = (int)chf.cells[nx + ny * w].index + GetCon(s, dir);
                                    if (chf.areas[nidx] != AreaNull)
                                    {
                                        nc++;
                                    }
                                }
                            }
                            // At least one missing neighbour.
                            if (nc != 4)
                                dist[i] = 0;
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

                        if (GetCon(s, 0) != NotConnected)
                        {
                            // (-1,0)
                            int ax = x + GetDirOffsetX(0);
                            int ay = y + GetDirOffsetY(0);
                            int ai = (int)chf.cells[ax + ay * w].index + GetCon(s, 0);
                            CompactSpan asp = chf.spans[ai];
                            nd = (byte)Math.Min((int)dist[ai] + 2, 255);
                            if (nd < dist[i])
                                dist[i] = nd;

                            // (-1,-1)
                            if (GetCon(asp, 3) != NotConnected)
                            {
                                int aax = ax + GetDirOffsetX(3);
                                int aay = ay + GetDirOffsetY(3);
                                int aai = (int)chf.cells[aax + aay * w].index + GetCon(asp, 3);
                                nd = (byte)Math.Min((int)dist[aai] + 3, 255);
                                if (nd < dist[i])
                                    dist[i] = nd;
                            }
                        }
                        if (GetCon(s, 3) != NotConnected)
                        {
                            // (0,-1)
                            int ax = x + GetDirOffsetX(3);
                            int ay = y + GetDirOffsetY(3);
                            int ai = (int)chf.cells[ax + ay * w].index + GetCon(s, 3);
                            CompactSpan asp = chf.spans[ai];
                            nd = (byte)Math.Min((int)dist[ai] + 2, 255);
                            if (nd < dist[i])

                                dist[i] = nd;

                            // (1,-1)
                            if (GetCon(asp, 2) != NotConnected)
                            {
                                int aax = ax + GetDirOffsetX(2);
                                int aay = ay + GetDirOffsetY(2);
                                int aai = (int)chf.cells[aax + aay * w].index + GetCon(asp, 2);
                                nd = (byte)Math.Min((int)dist[aai] + 3, 255);
                                if (nd < dist[i])

                                    dist[i] = nd;
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

                        if (GetCon(s, 2) != NotConnected)
                        {
                            // (1,0)
                            int ax = x + GetDirOffsetX(2);
                            int ay = y + GetDirOffsetY(2);
                            int ai = (int)chf.cells[ax + ay * w].index + GetCon(s, 2);
                            CompactSpan asp = chf.spans[ai];
                            nd = (byte)Math.Min((int)dist[ai] + 2, 255);
                            if (nd < dist[i])

                                dist[i] = nd;

                            // (1,1)
                            if (GetCon(asp, 1) != NotConnected)
                            {
                                int aax = ax + GetDirOffsetX(1);
                                int aay = ay + GetDirOffsetY(1);
                                int aai = (int)chf.cells[aax + aay * w].index + GetCon(asp, 1);
                                nd = (byte)Math.Min((int)dist[aai] + 3, 255);
                                if (nd < dist[i])

                                    dist[i] = nd;
                            }
                        }
                        if (GetCon(s, 1) != NotConnected)
                        {
                            // (0,1)
                            int ax = x + GetDirOffsetX(1);
                            int ay = y + GetDirOffsetY(1);
                            int ai = (int)chf.cells[ax + ay * w].index + GetCon(s, 1);
                            CompactSpan asp = chf.spans[ai];
                            nd = (byte)Math.Min((int)dist[ai] + 2, 255);
                            if (nd < dist[i])

                                dist[i] = nd;

                            // (-1,1)
                            if (GetCon(asp, 0) != NotConnected)
                            {
                                int aax = ax + GetDirOffsetX(0);
                                int aay = ay + GetDirOffsetY(0);
                                int aai = (int)chf.cells[aax + aay * w].index + GetCon(asp, 0);
                                nd = (byte)Math.Min((int)dist[aai] + 3, 255);
                                if (nd < dist[i])

                                    dist[i] = nd;
                            }
                        }
                    }
                }
            }

            byte thr = (byte)(radius * 2);
            for (int i = 0; i < chf.spanCount; ++i)
                if (dist[i] < thr)
                    chf.areas[i] = AreaNull;


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

        private static bool RasterizeTriangles(Heightfield solid, int flagMergeThr, Triangle[] tris, byte[] areas)
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

        private static bool RasterizeTri(Triangle tri, byte area, Heightfield hf, BoundingBox b, float cs, float ics, float ich, int flagMergeThr)
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

        private static bool AddSpan(Heightfield hf, int x, int y, ushort smin, ushort smax, byte area, int flagMergeThr)
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
                        s.area = Math.Max(s.area, cur.area);
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

        private static int MarkWalkableTriangles(float walkableSlopeAngle, Triangle[] tris, byte[] areas)
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
                    areas[i] = WalkableArea;
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
            chf.areas = new byte[spanCount];

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
                        if (s.area != AreaNull)
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
            int maxLayers = NotConnected - 1;
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
                            SetCon(ref s, dir, NotConnected);
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
                        if (s.area != AreaNull)
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
    }
}
