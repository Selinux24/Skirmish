using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour.Tiles;

    /// <summary>
    /// Config
    /// </summary>
    public struct Config
    {
        /// <summary>
        /// Agent type
        /// </summary>
        public Agent Agent { get; set; }

        /// <summary>
        /// The width of the field along the x-axis. [Limit: >= 0] [Units: vx]
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// The height of the field along the z-axis. [Limit: >= 0] [Units: vx]
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// The width/height size of tile's on the xz-plane. [Limit: >= 0] [Units: vx]
        /// </summary>
        public int TileSize { get; set; }
        /// <summary>
        /// The size of the non-navigable border around the heightfield. [Limit: >=0] [Units: vx]
        /// </summary>
        public int BorderSize { get; set; }
        /// <summary>
        /// The xz-plane cell size to use for fields. [Limit: > 0] [Units: wu] 
        /// </summary>
        public float CellSize { get; set; }
        /// <summary>
        /// The y-axis cell size to use for fields. [Limit: > 0] [Units: wu]
        /// </summary>
        public float CellHeight { get; set; }
        /// <summary>
        /// The bounds of the field's AABB. [(x, y, z)] [Units: wu]
        /// </summary>
        public BoundingBox BoundingBox { get; set; }
        /// <summary>
        /// The maximum slope that is considered walkable. [Limits: 0 <= value < 90] [Units: Degrees] 
        /// </summary>
        public float WalkableSlopeAngle { get; set; }
        /// <summary>
        /// Minimum floor to 'ceiling' height that will still allow the floor area to be considered walkable. [Limit: >= 3] [Units: vx] 
        /// </summary>
        public int WalkableHeight { get; set; }
        /// <summary>
        /// Maximum ledge height that is considered to still be traversable. [Limit: >=0] [Units: vx] 
        /// </summary>
        public int WalkableClimb { get; set; }
        /// <summary>
        /// The distance to erode/shrink the walkable area of the heightfield away from obstructions.  [Limit: >=0] [Units: vx] 
        /// </summary>
        public int WalkableRadius { get; set; }
        /// <summary>
        /// The maximum allowed length for contour edges along the border of the mesh. [Limit: >=0] [Units: vx] 
        /// </summary>
        public int MaxEdgeLen { get; set; }
        /// <summary>
        /// The maximum distance a simplfied contour's border edges should deviate the original raw contour. [Limit: >=0] [Units: vx]
        /// </summary>
        public float MaxSimplificationError { get; set; }
        /// <summary>
        /// The minimum number of cells allowed to form isolated island areas. [Limit: >=0] [Units: vx] 
        /// </summary>
        public int MinRegionArea { get; set; }
        /// <summary>
        /// Any regions with a span count smaller than this value will, if possible, be merged with larger regions. [Limit: >=0] [Units: vx] 
        /// </summary>
        public int MergeRegionArea { get; set; }
        /// <summary>
        /// The maximum number of vertices allowed for polygons generated during the contour to polygon conversion process. [Limit: >= 3] 
        /// </summary>
        public int MaxVertsPerPoly { get; set; }
        /// <summary>
        /// Sets the sampling distance to use when generating the detail mesh. (For height detail only.) [Limits: 0 or >= 0.9] [Units: wu] 
        /// </summary>
        public float DetailSampleDist { get; set; }
        /// <summary>
        /// The maximum distance the detail mesh surface should deviate from heightfield data. (For height detail only.) [Limit: >=0] [Units: wu] 
        /// </summary>
        public float DetailSampleMaxError { get; set; }

        /// <summary>
        /// Filter low hanging obstacles when generation
        /// </summary>
        public bool FilterLowHangingObstacles { get; set; }
        /// <summary>
        /// Filter ledge spans when generation
        /// </summary>
        public bool FilterLedgeSpans { get; set; }
        /// <summary>
        /// Filter walkable low hight spans when generation
        /// </summary>
        public bool FilterWalkableLowHeightSpans { get; set; }
        /// <summary>
        /// Partition type
        /// </summary>
        public SamplePartitionTypes PartitionType { get; set; }
        /// <summary>
        /// Use tile cache
        /// </summary>
        public bool UseTileCache { get; set; }
        /// <summary>
        /// Build all tiles from the beginning
        /// </summary>
        public bool BuildAllTiles { get; set; }
        /// <summary>
        /// Tile cache parameters
        /// </summary>
        public TileCacheParams TileCacheParams { get; set; }

        /// <summary>
        /// Updates the tile bounds
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public void UpdateTileBounds(int x, int y)
        {
            // Tile bounds.
            float tcs = TileSize * CellSize;

            var bbox = new BoundingBox();

            bbox.Minimum.X = BoundingBox.Minimum.X + x * tcs;
            bbox.Minimum.Y = BoundingBox.Minimum.Y;
            bbox.Minimum.Z = BoundingBox.Minimum.Z + y * tcs;

            bbox.Maximum.X = BoundingBox.Minimum.X + (x + 1) * tcs;
            bbox.Maximum.Y = BoundingBox.Maximum.Y;
            bbox.Maximum.Z = BoundingBox.Minimum.Z + (y + 1) * tcs;

            bbox.Minimum.X -= BorderSize * CellSize;
            bbox.Minimum.Z -= BorderSize * CellSize;
            bbox.Maximum.X += BorderSize * CellSize;
            bbox.Maximum.Z += BorderSize * CellSize;

            BoundingBox = bbox;
        }
    }
}
