using SharpDX;
using System;

namespace Engine.PathFinding.NavMesh2
{
    public class NavMesh
    {
        const int ExpectedLayersPerTile = 4;
        const byte WalkableArea = 63;

        private static void CalcGridSize(BoundingBox b, float cellSize, out int w, out int h)
        {
            w = (int)((b.Maximum.X - b.Minimum.X) / cellSize + 0.5f);
            h = (int)((b.Maximum.Z - b.Minimum.Z) / cellSize + 0.5f);
        }

        public static NavMesh Build(Settings settings, InputGeometry geometry)
        {
            Agent agent = settings.Agents[0];

            // Init cache
            int gw;
            int gh;
            CalcGridSize(geometry.BoundingBox, settings.CellSize, out gw, out gh);
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
                BoundingBox = geometry.BoundingBox,
            };

            // Tile cache params.
            var tcparams = new TileCacheParams()
            {
                Origin = geometry.BoundingBox.Minimum,
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
                Origin = geometry.BoundingBox.Minimum,
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

        private static int RasterizeTileLayers(int tx, int ty, Settings settings, Config cfg, InputGeometry geometry, out TileCacheData[] tiles)
        {
            tiles = new TileCacheData[TileCache.MaxLayers];

            Vector3[] verts = geometry.Vertices;
            int nverts = verts.Length;
            //TODO: Here we are!
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
                ChunkyTriMeshNode node = chunkyMesh.nodes[cid[i]];
                Triangle[] tris = new Triangle[node.n];
                Array.Copy(chunkyMesh.tris, node.i, tris, 0, node.n);

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

            rc.chf = BuildCompactHeightfield(tcfg.WalkableHeight, tcfg.WalkableClimb, rc.solid, rc.chf);

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

        private static bool BuildTileCacheLayer(FastLZCompressor comp, TileCacheLayerHeader header, char[] heights, char[] areas, char[] cons, char[] data, int dataSize)
        {
            throw new NotImplementedException();
        }

        private static HeightfieldLayerSet BuildHeightfieldLayers(CompactHeightfield chf, int borderSize, int walkableHeight, HeightfieldLayerSet lset)
        {
            throw new NotImplementedException();
        }

        private static bool ErodeWalkableArea(int walkableRadius, CompactHeightfield chf)
        {
            throw new NotImplementedException();
        }

        private static void FilterWalkableLowHeightSpans(int walkableHeight, Heightfield solid)
        {
            throw new NotImplementedException();
        }

        private static void FilterLedgeSpans(int walkableHeight, int walkableClimb, Heightfield solid)
        {
            throw new NotImplementedException();
        }

        private static void FilterLowHangingWalkableObstacles(int walkableClimb, Heightfield solid)
        {
            throw new NotImplementedException();
        }

        private static bool RasterizeTriangles(Heightfield solid, int flagMergeThr, Triangle[] tris, byte[] areas)
        {
            float ics = 1.0f / solid.cs;
            float ich = 1.0f / solid.ch;

            // Rasterize triangles.
            for (int i = 0; i < tris.Length; ++i)
            {
                // Rasterize.
                if (!RasterizeTri(tris[i], areas[i], solid, solid.b, solid.cs, ics, ich, flagMergeThr))
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
            float by = b.Maximum.Y - b.Minimum.Y;

            // Calculate the bounding box of the triangle.
            var t = BoundingBox.FromPoints(tri.GetVertices());

            // If the triangle does not touch the bbox of the heightfield, skip the triagle.
            // TODO: Here we are!
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
                        prev.next = next;
                    else
                        hf.spans[idx] = next;

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

        private static CompactHeightfield BuildCompactHeightfield(int walkableHeight, int walkableClimb, Heightfield solid, CompactHeightfield chf)
        {
            throw new NotImplementedException();
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
            hf.b = boundingBox;
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
