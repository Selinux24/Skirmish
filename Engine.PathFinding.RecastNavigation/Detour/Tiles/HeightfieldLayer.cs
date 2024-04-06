using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Heightfield layer
    /// </summary>
    struct HeightfieldLayer
    {
        /// <summary>
        /// Null Id
        /// </summary>
        const int NULL_ID = 0xff;

        /// <summary>
        /// The heightfield. [Size: width * height]
        /// </summary>
        private int[] heights;
        /// <summary>
        /// Area ids. [Size: Same as #heights]
        /// </summary>
        private AreaTypes[] areas;
        /// <summary>
        /// Packed neighbor connection information. [Size: Same as #heights]
        /// </summary>
        private int[] cons;

        /// <summary>
        /// Height min range
        /// </summary>
        private int hMin;
        /// <summary>
        /// Height max range
        /// </summary>
        private int hMax;
        /// <summary>
        /// Minx usable sub-region.
        /// </summary>
        private int minX;
        /// <summary>
        /// Maxx usable sub-region.
        /// </summary>
        private int maxX;
        /// <summary>
        /// Miny usable sub-region.
        /// </summary>
        private int minY;
        /// <summary>
        /// Maxy usable sub-region.
        /// </summary>
        private int maxY;

        /// <summary>
        /// The size of each cell. (On the xz-plane.)
        /// </summary>
        public float CellSize { get; private set; }
        /// <summary>
        /// The height of each cell. (The minimum increment along the y-axis.)
        /// </summary>
        public float CellHeight { get; private set; }
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox Bounds { get; private set; }
        /// <summary>
        /// Width of the layer.
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Height of the layer.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Creates a tile cache layer data instance
        /// </summary>
        /// <param name="x">X tile coordinate</param>
        /// <param name="y">Y tile coordinate</param>
        /// <param name="i">Layer index</param>
        public TileCacheData Create(int x, int y, int i)
        {
            return new()
            {
                Header = CreateHeader(x, y, i),
                Data = CreateData(),
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
            return new()
            {
                // Tile layer location in the navmesh.
                TX = x,
                TY = y,
                TLayer = i,

                // Tile info.
                Width = Width,
                Height = Height,
                Bounds = Bounds,
                HMin = hMin,
                HMax = hMax,
                MinX = minX,
                MaxX = maxX,
                MinY = minY,
                MaxY = maxY,
            };
        }
        /// <summary>
        /// Builds a tile cache layer
        /// </summary>
        private TileCacheLayerData CreateData()
        {
            return new TileCacheLayerData()
            {
                Heights = heights,
                Areas = areas,
                Connections = cons,
            };
        }
     
        /// <summary>
        /// Copies height and area from compact heightfield. 
        /// </summary>
        /// <param name="data">Layer data</param>
        /// <param name="layerId">Layer id</param>
        public void CopyFromLayerData(HeightfieldLayerData data, int layerId)
        {
            int gridSize = data.LayerWidth * data.LayerHeight;

            heights = Helper.CreateArray(gridSize, NULL_ID);
            areas = Helper.CreateArray(gridSize, AreaTypes.RC_NULL_AREA);
            cons = Helper.CreateArray(gridSize, 0x00);

            Width = data.LayerWidth;
            Height = data.LayerHeight;
            CellSize = data.CellSize;
            CellHeight = data.CellHeight;

            // Find layer height bounds.
            var (lbbox, hmin, hmax) = data.GetLayerBounds(layerId);
            Bounds = lbbox;
            hMin = hmin;
            hMax = hmax;

            // Update usable data region.
            minX = Width;
            maxX = 0;
            minY = Height;
            maxY = 0;

            foreach (var (x, y) in GridUtils.Iterate(data.LayerWidth, data.LayerHeight))
            {
                // Update data bounds.
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);

                int idx = x + y * data.LayerWidth;

                foreach (var (cx, cy, s, a) in data.IterateLayerCellSpans(x, y, layerId))
                {
                    // Store height and area type.
                    heights[idx] = s.Y - hmin;
                    areas[idx] = a;

                    // Check connection.
                    cons[idx] = data.CheckConnection(s, cx, cy, layerId, idx, hmin, heights);
                }
            }

            if (minX > maxX)
            {
                minX = maxX = 0;
            }

            if (minY > maxY)
            {
                minY = maxY = 0;
            }
        }
    }
}
