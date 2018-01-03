using SharpDX;
using System;

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

        public static NavMesh Build(BuildSettings settings, InputGeometry geometry)
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
                MaxEdgeLen = (int)(settings.EdgeMaxLength / settings.CellHeight),
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
                    RasterizeTileLayers(x, y, settings, cfg, geometry, out tiles);

                    for (int i = 0; i < tiles.Length; ++i)
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
                m_tiles[i] = new MeshTile();
                m_tiles[i].Salt = 1;
                m_tiles[i].Next = m_nextFree;
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

            RasterizationContext rc = new RasterizationContext();

            // Allocate voxel heightfield where we rasterize our input data to.
            rc.solid = CreateHeightfield(tcfg.Width, tcfg.Height, tcfg.BoundingBox, tcfg.CellSize, tcfg.CellHeight);

            // Allocate array that can hold triangle flags.
            // If you have multiple meshes you need to process, allocate
            // and array which can hold the max number of triangles you need to process.
            rc.triareas = new byte[chunkyMesh.maxTrisPerChunk];

            Vector2 tbmin;
            Vector2 tbmax;
            tbmin.X = tcfg.BoundingBox.Minimum.X;
            tbmin.Y = tcfg.BoundingBox.Minimum.Z;
            tbmax.X = tcfg.BoundingBox.Maximum.X;
            tbmax.Y = tcfg.BoundingBox.Maximum.Z;
            int[] cid = new int[512];// TODO: Make grow when returning too many items.
            int ncid = GetChunksOverlappingRect(chunkyMesh, tbmin, tbmax, cid, 512);
            if (ncid == 0)
            {
                return 0; // empty
            }

            for (int i = 0; i < ncid; i++)
            {
                Triangle[] tris = chunkyMesh.GetTriangles(geometry.Triangles, cid[i]);

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
            for (int i = 0; i < vols.Length; ++i)
            {
                MarkConvexPolyArea(
                    vols[i].verts, vols[i].nverts,
                    vols[i].hmin, vols[i].hmax,
                    vols[i].area, rc.chf);
            }

            rc.lset = BuildHeightfieldLayers(rc.chf, tcfg.BorderSize, tcfg.WalkableHeight, rc.lset);

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

                if (!BuildTileCacheLayer(
                    comp,
                    header, layer.heights, layer.areas, layer.cons,
                    tile.Data, tile.DataSize))
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

        private static void MarkConvexPolyArea(float[] verts, int nverts, float hmin, float hmax, int area, CompactHeightfield chf)
        {
            throw new NotImplementedException();
        }

        private static bool BuildTileCacheLayer(FastLZCompressor comp, TileCacheLayerHeader header, byte[] heights, byte[] areas, byte[] cons, byte[] data, int dataSize)
        {
            throw new NotImplementedException();
        }

        private static HeightfieldLayerSet BuildHeightfieldLayers(CompactHeightfield chf, int borderSize, int walkableHeight, HeightfieldLayerSet lset)
        {
            throw new NotImplementedException();
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
                    Span s = solid.spans[x + y * w];

                    while (s != null)
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

                        ps = s;
                        s = s.next;
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
                    Span s = solid.spans[x + y * w];

                    while (s != null)
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
                            int dx = x + GetDirOffsetX(dir);
                            int dy = y + GetDirOffsetY(dir);
                            // Skip neighbours which are out of bounds.
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

                        s = s.next;
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
                    Span s = solid.spans[x + y * w];

                    while (s != null)
                    {
                        int bot = (int)(s.smax);
                        int top = s.next != null ? (int)(s.next.smin) : int.MaxValue;

                        if ((top - bot) <= walkableHeight)
                        {
                            s.area = AreaNull;
                        }

                        s = s.next;
                    }
                }
            }
        }

        private static bool ErodeWalkableArea(int radius, CompactHeightfield chf)
        {
            int w = chf.width;
            int h = chf.height;

            // Init distance.
            byte[] dist = new byte[chf.spanCount];

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
            //TODO: Here we are! -> This never adds a span

            int w = hf.width;
            int h = hf.height;
            float by = b.Maximum.Y - b.Minimum.Y;

            // Calculate the bounding box of the triangle.
            var t = BoundingBox.FromPoints(tri.GetVertices());

            // If the triangle does not touch the bbox of the heightfield, skip the triagle.
            if (t.Contains(b) == ContainmentType.Disjoint)
            {
                return true;
            }

            // Calculate the footprint of the triangle on the grid's y-axis
            int y0 = (int)((t.Minimum.Y - b.Minimum.Y) * ics);
            int y1 = (int)((t.Maximum.Y - b.Minimum.Y) * ics);
            y0 = MathUtil.Clamp(y0, 0, h - 1);
            y1 = MathUtil.Clamp(y1, 0, h - 1);

            // Clip the triangle into all grid cells it touches.
            float[] inb = new float[3 * 4];
            float[] inrow = new float[3 * 4];
            float[] p1 = new float[3 * 4];
            float[] p2 = new float[3 * 4];

            Array.Copy(tri.Point1.ToArray(), 0, inb, 0, 3);
            Array.Copy(tri.Point2.ToArray(), 0, inb, 1 * 3, 3);
            Array.Copy(tri.Point3.ToArray(), 0, inb, 2 * 3, 3);
            int nvrow = 0;
            int nvIn = 3;

            for (int y = y0; y <= y1; ++y)
            {
                // Clip polygon to row. Store the remaining polygon as well
                float cz = b.Minimum.Y + y * cs;
                DividePoly(inb, nvIn, ref inrow, ref nvrow, ref p1, ref nvIn, cz + cs, 2);
                Helper.Swap(ref inb, ref p1);
                if (nvrow < 3) continue;

                // find the horizontal bounds in the row
                float minX = inrow[0], maxX = inrow[0];
                for (int i = 1; i < nvrow; ++i)
                {
                    if (minX > inrow[i * 3]) minX = inrow[i * 3];
                    if (maxX < inrow[i * 3]) maxX = inrow[i * 3];
                }
                int x0 = (int)((minX - b.Minimum.X) * ics);
                int x1 = (int)((maxX - b.Minimum.X) * ics);
                x0 = MathUtil.Clamp(x0, 0, w - 1);
                x1 = MathUtil.Clamp(x1, 0, w - 1);

                int nv = 0;
                int nv2 = nvrow;

                for (int x = x0; x <= x1; ++x)
                {
                    // Clip polygon to column. store the remaining polygon as well
                    float cx = b.Minimum.X + x * cs;
                    DividePoly(inrow, nv2, ref p1, ref nv, ref p2, ref nv2, cx + cs, 0);
                    Helper.Swap(ref inrow, ref p2);
                    if (nv < 3) continue;

                    // Calculate min and max of the span.
                    float smin = p1[1], smax = p1[1];
                    for (int i = 1; i < nv; ++i)
                    {
                        smin = Math.Min(smin, p1[i * 3 + 1]);
                        smax = Math.Min(smax, p1[i * 3 + 1]);
                    }
                    smin -= b.Minimum.Y;
                    smax -= b.Minimum.Y;
                    // Skip the span if it is outside the heightfield bbox
                    if (smax < 0.0f) continue;
                    if (smin > by) continue;
                    // Clamp the span to the heightfield bbox.
                    if (smin < 0.0f) smin = 0;
                    if (smax > by) smax = by;

                    // Snap the span to the heightfield height grid.
                    ushort ismin = (ushort)MathUtil.Clamp((int)Math.Floor(smin * ich), 0, Span.SpanMaxHeight);
                    ushort ismax = (ushort)MathUtil.Clamp((int)Math.Ceiling(smax * ich), ismin + 1, Span.SpanMaxHeight);

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

            Span s = new Span();
            s.smin = smin;
            s.smax = smax;
            s.area = area;
            s.next = null;

            // Empty cell, add the first span.
            if (hf.spans == null)
            {
                hf.spans = new Span[hf.width * hf.height];
                hf.spans[idx] = s;
                return true;
            }
            Span prev = null;
            Span cur = hf.spans[idx];

            // Insert and merge spans.
            while (cur != null)
            {
                if (cur.smin > cur.smax)
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
                        s.smin = cur.smin;
                    if (cur.smax > s.smax)
                        s.smax = cur.smax;

                    // Merge flags.
                    if (Math.Abs((int)s.smax - (int)cur.smax) <= flagMergeThr)
                        s.area = Math.Max(s.area, cur.area);

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

        private static void DividePoly(float[] inn, int nin, ref float[] out1, ref int nout1, ref float[] out2, ref int nout2, float x, int axis)
        {
            float[] d = new float[12];
            for (int i = 0; i < nin; ++i)
            {
                d[i] = x - inn[i * 3 + axis];
            }

            int m = 0, n = 0;
            for (int i = 0, j = nin - 1; i < nin; j = i, ++i)
            {
                bool ina = d[j] >= 0;
                bool inb = d[i] >= 0;
                if (ina != inb)
                {
                    float s = d[j] / (d[j] - d[i]);
                    out1[m * 3 + 0] = inn[j * 3 + 0] + (inn[i * 3 + 0] - inn[j * 3 + 0]) * s;
                    out1[m * 3 + 1] = inn[j * 3 + 1] + (inn[i * 3 + 1] - inn[j * 3 + 1]) * s;
                    out1[m * 3 + 2] = inn[j * 3 + 2] + (inn[i * 3 + 2] - inn[j * 3 + 2]) * s;

                    Array.Copy(out2, n * 3, out1, m * 3, 3);
                    m++;
                    n++;
                    // add the i'th point to the right polygon. Do NOT add points that are on the dividing line
                    // since these were already added above
                    if (d[i] > 0)
                    {

                        Array.Copy(out1, m * 3, inn, i * 3, 3);
                        m++;
                    }
                    else if (d[i] < 0)
                    {

                        Array.Copy(out2, n * 3, inn, i * 3, 3);
                        n++;
                    }
                }
                else // same side
                {
                    // add the i'th point to the right polygon. Addition is done even for points on the dividing line
                    if (d[i] >= 0)
                    {

                        Array.Copy(out1, m * 3, inn, i * 3, 3);
                        m++;

                        if (d[i] != 0) continue;
                    }

                    Array.Copy(out2, n * 3, inn, i * 3, 3);
                    n++;
                }
            }


            nout1 = m;
            nout2 = n;
        }

        private static void MarkWalkableTriangles(float walkableSlopeAngle, Triangle[] tris, byte[] areas)
        {
            float walkableThr = (float)Math.Cos(walkableSlopeAngle / 180.0f * MathUtil.Pi);

            Vector3 norm;

            for (int i = 0; i < tris.Length; i++)
            {
                var tri = tris[i];
                norm = tri.Normal;
                // Check if the face is walkable.
                if (norm[1] > walkableThr)
                {
                    areas[i] = WalkableArea;
                }
            }
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

                    CompactCell c = chf.cells[x + y * w];
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
                }
            }

            // Find neighbour connections.
            int MaxLayers = NotConnected - 1;
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
                            SetCon(s, dir, NotConnected);
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
                                    if (lidx < 0 || lidx > MaxLayers)
                                    {
                                        tooHighNeighbour = Math.Max(tooHighNeighbour, lidx);
                                        continue;
                                    }

                                    SetCon(s, dir, lidx);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (tooHighNeighbour > MaxLayers)
            {
                throw new EngineException(string.Format("Heightfield has too many layers {0} (max: {1})", tooHighNeighbour, MaxLayers));
            }

            return chf;
        }

        private static void SetCon(CompactSpan s, int dir, int i)
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

        private static int GetChunksOverlappingRect(ChunkyTriMesh cm, Vector2 bmin, Vector2 bmax, int[] ids, int maxIds)
        {
            // Traverse tree
            int i = 0;
            int n = 0;
            while (i < cm.nnodes)
            {
                ChunkyTriMeshNode node = cm.nodes[i];
                bool overlap = CheckOverlapRect(bmin, bmax, node.bmin, node.bmax);
                bool isLeafNode = node.i >= 0;

                if (isLeafNode && overlap)
                {
                    if (n < maxIds)
                    {
                        ids[n] = i;
                        n++;
                    }
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

            return n;
        }

        private static bool CheckOverlapRect(Vector2 amin, Vector2 amax, Vector2 bmin, Vector2 bmax)
        {
            bool overlap = true;
            overlap = (amin.X > bmax.X || amax.X < bmin.X) ? false : overlap;
            overlap = (amin.Y > bmax.Y || amax.Y < bmin.Y) ? false : overlap;
            return overlap;
        }

        private static Heightfield CreateHeightfield(int width, int height, BoundingBox boundingBox, float cellSize, float cellHeight)
        {
            Heightfield hf = new Heightfield();

            hf.width = width;
            hf.height = height;
            hf.boundingBox = boundingBox;
            hf.cs = cellSize;
            hf.ch = cellHeight;
            hf.spans = new Span[hf.width * hf.height];

            return hf;
        }

        private static int CalcLayerBufferSize(int gridWidth, int gridHeight)
        {
            int headerSize = Helper.Align4(TileCacheLayerHeader.Size);
            int gridSize = gridWidth * gridHeight;

            return headerSize + gridSize * 4;
        }
    }
}
