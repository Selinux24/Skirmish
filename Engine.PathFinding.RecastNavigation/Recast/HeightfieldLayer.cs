using Engine.PathFinding.RecastNavigation.Detour.Tiles;
using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Heightfield layer
    /// </summary>
    struct HeightfieldLayer
    {
        /// <summary>
        /// The size of each cell. (On the xz-plane.)
        /// </summary>
        public float CS { get; set; }
        /// <summary>
        /// The height of each cell. (The minimum increment along the y-axis.)
        /// </summary>
        public float CH { get; set; }
        /// <summary>
        /// The heightfield. [Size: width * height]
        /// </summary>
        public int[] Heights { get; set; }
        /// <summary>
        /// Area ids. [Size: Same as #heights]
        /// </summary>
        public AreaTypes[] Areas { get; set; }
        /// <summary>
        /// Packed neighbor connection information. [Size: Same as #heights]
        /// </summary>
        public int[] Cons { get; set; }
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; set; }
        /// <summary>
        /// Height min range
        /// </summary>
        public int HMin { get; set; }
        /// <summary>
        /// Height max range
        /// </summary>
        public int HMax { get; set; }
        /// <summary>
        /// Width of the layer.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height of the layer.
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Minx usable sub-region.
        /// </summary>
        public int MinX { get; set; }
        /// <summary>
        /// Maxx usable sub-region.
        /// </summary>
        public int MaxX { get; set; }
        /// <summary>
        /// Miny usable sub-region.
        /// </summary>
        public int MinY { get; set; }
        /// <summary>
        /// Maxy usable sub-region.
        /// </summary>
        public int MaxY { get; set; }

        /// <summary>
        /// Creates a tile cache layer data instance
        /// </summary>
        /// <param name="x">X tile coordinate</param>
        /// <param name="y">Y tile coordinate</param>
        /// <param name="i">Layer index</param>
        public TileCacheData Create(int x, int y, int i)
        {
            var header = CreateHeader(x, y, i);
            var data = CreateData();

            return new TileCacheData
            {
                Header = header,
                Data = data,
            };
        }
        /// <summary>
        /// Creates a tile cache layer header
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="i"></param>
        private TileCacheLayerHeader CreateHeader(int x, int y, int i)
        {
            return new TileCacheLayerHeader
            {
                // Tile layer location in the navmesh.
                TX = x,
                TY = y,
                TLayer = i,
                BBox = BoundingBox,

                // Tile info.
                Width = Width,
                Height = Height,
                MinX = MinX,
                MaxX = MaxX,
                MinY = MinY,
                MaxY = MaxY,
                HMin = HMin,
                HMax = HMax
            };
        }
        /// <summary>
        /// Builds a tile cache layer
        /// </summary>
        private TileCacheLayerData CreateData()
        {
            return new TileCacheLayerData()
            {
                Heights = Heights,
                Areas = Areas,
                Connections = Cons,
            };
        }

        /// <summary>
        /// Copy height and area from compact heightfield. 
        /// </summary>
        public void CopyToLayer(HeightfieldLayerData data, int curId)
        {
            int gridSize = data.LayerWidth * data.LayerHeight;

            Heights = Helper.CreateArray(gridSize, 0xff);
            Areas = Helper.CreateArray(gridSize, AreaTypes.RC_NULL_AREA);
            Cons = Helper.CreateArray(gridSize, 0x00);

            // Find layer height bounds.
            var (hmin, hmax) = data.FindLayerHeightBounds(curId);

            Width = data.LayerWidth;
            Height = data.LayerHeight;
            CS = data.Heightfield.CellSize;
            CH = data.Heightfield.CellHeight;

            // Adjust the bbox to fit the heightfield.
            var lbbox = data.BoundingBox;
            lbbox.Minimum.Y = data.BoundingBox.Minimum.Y + hmin * data.Heightfield.CellHeight;
            lbbox.Maximum.Y = data.BoundingBox.Minimum.Y + hmax * data.Heightfield.CellHeight;
            BoundingBox = lbbox;
            HMin = hmin;
            HMax = hmax;

            // Update usable data region.
            MinX = Width;
            MaxX = 0;
            MinY = Height;
            MaxY = 0;

            for (int y = 0; y < data.LayerHeight; ++y)
            {
                for (int x = 0; x < data.LayerWidth; ++x)
                {
                    CopyToLayerCell(data, x, y, curId, hmin);
                }
            }

            if (MinX > MaxX)
            {
                MinX = MaxX = 0;
            }

            if (MinY > MaxY)
            {
                MinY = MaxY = 0;
            }
        }
        /// <summary>
        /// Copy height and area from compact heightfield cell. 
        /// </summary>
        private void CopyToLayerCell(HeightfieldLayerData data, int x, int y, int curId, int hmin)
        {
            int cx = data.BorderSize + x;
            int cy = data.BorderSize + y;
            var c = data.Heightfield.Cells[cx + cy * data.Width];

            for (int j = c.Index, nj = c.Index + c.Count; j < nj; ++j)
            {
                var s = data.Heightfield.Spans[j];

                // Skip unassigned regions.
                if (data.SourceRegions[j] == 0xff)
                {
                    continue;
                }

                // Skip of does nto belong to current layer.
                int lid = data.Regions[data.SourceRegions[j]].LayerId;
                if (lid != curId)
                {
                    continue;
                }

                // Update data bounds.
                MinX = Math.Min(MinX, x);
                MaxX = Math.Max(MaxX, x);
                MinY = Math.Min(MinY, y);
                MaxY = Math.Max(MaxY, y);

                // Store height and area type.
                int idx = x + y * data.LayerWidth;
                Heights[idx] = s.Y - hmin;
                Areas[idx] = data.Heightfield.Areas[j];

                // Check connection.
                CheckConnection(s, cx, cy, lid, idx, hmin, data);
            }
        }
        /// <summary>
        /// Check connection.
        /// </summary>
        private readonly void CheckConnection(CompactSpan s, int cx, int cy, int layerId, int layerIndex, int hmin, HeightfieldLayerData data)
        {
            int portal = 0;
            int con = 0;

            for (int dir = 0; dir < 4; ++dir)
            {
                int d = s.GetCon(dir);
                if (d == ContourSet.RC_NOT_CONNECTED)
                {
                    continue;
                }

                int ax = cx + Utils.GetDirOffsetX(dir);
                int ay = cy + Utils.GetDirOffsetY(dir);
                int ai = data.Heightfield.Cells[ax + ay * data.Width].Index + d;
                int alid = data.SourceRegions[ai] != 0xff ? data.Regions[data.SourceRegions[ai]].LayerId : 0xff;

                // Portal mask
                if (data.Heightfield.Areas[ai] != AreaTypes.RC_NULL_AREA && layerId != alid)
                {
                    portal |= 1 << dir;

                    // Update height so that it matches on both sides of the portal.
                    var ass = data.Heightfield.Spans[ai];
                    if (ass.Y > hmin)
                    {
                        Heights[layerIndex] = Math.Max(Heights[layerIndex], ass.Y - hmin);
                    }
                }

                // Valid connection mask
                if (data.Heightfield.Areas[ai] != AreaTypes.RC_NULL_AREA && layerId == alid)
                {
                    int nx = ax - data.BorderSize;
                    int ny = ay - data.BorderSize;
                    if (nx >= 0 && ny >= 0 && nx < data.LayerWidth && ny < data.LayerHeight)
                    {
                        con |= 1 << dir;
                    }
                }
            }

            Cons[layerIndex] = (portal << 4) | con;
        }
    }
}
