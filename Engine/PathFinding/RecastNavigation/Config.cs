using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Config
    /// </summary>
    public struct Config
    {
        /// <summary>
        /// The width of the field along the x-axis. [Limit: >= 0] [Units: vx]
        /// </summary>
        public int Width;
        /// <summary>
        /// The height of the field along the z-axis. [Limit: >= 0] [Units: vx]
        /// </summary>
        public int Height;
        /// <summary>
        /// The width/height size of tile's on the xz-plane. [Limit: >= 0] [Units: vx]
        /// </summary>
        public int TileSize;
        /// <summary>
        /// The size of the non-navigable border around the heightfield. [Limit: >=0] [Units: vx]
        /// </summary>
        public int BorderSize;
        /// <summary>
        /// The xz-plane cell size to use for fields. [Limit: > 0] [Units: wu] 
        /// </summary>
        public float CellSize;
        /// <summary>
        /// The y-axis cell size to use for fields. [Limit: > 0] [Units: wu]
        /// </summary>
        public float CellHeight;
        /// <summary>
        /// The bounds of the field's AABB. [(x, y, z)] [Units: wu]
        /// </summary>
        public BoundingBox BoundingBox;
        /// <summary>
        /// The maximum slope that is considered walkable. [Limits: 0 <= value < 90] [Units: Degrees] 
        /// </summary>
        public float WalkableSlopeAngle;
        /// <summary>
        /// Minimum floor to 'ceiling' height that will still allow the floor area to be considered walkable. [Limit: >= 3] [Units: vx] 
        /// </summary>
        public int WalkableHeight;
        /// <summary>
        /// Maximum ledge height that is considered to still be traversable. [Limit: >=0] [Units: vx] 
        /// </summary>
        public int WalkableClimb;
        /// <summary>
        /// The distance to erode/shrink the walkable area of the heightfield away from obstructions.  [Limit: >=0] [Units: vx] 
        /// </summary>
        public int WalkableRadius;
        /// <summary>
        /// The maximum allowed length for contour edges along the border of the mesh. [Limit: >=0] [Units: vx] 
        /// </summary>
        public int MaxEdgeLen;
        /// <summary>
        /// The maximum distance a simplfied contour's border edges should deviate the original raw contour. [Limit: >=0] [Units: vx]
        /// </summary>
        public float MaxSimplificationError;
        /// <summary>
        /// The minimum number of cells allowed to form isolated island areas. [Limit: >=0] [Units: vx] 
        /// </summary>
        public int MinRegionArea;
        /// <summary>
        /// Any regions with a span count smaller than this value will, if possible, be merged with larger regions. [Limit: >=0] [Units: vx] 
        /// </summary>
        public int MergeRegionArea;
        /// <summary>
        /// The maximum number of vertices allowed for polygons generated during the contour to polygon conversion process. [Limit: >= 3] 
        /// </summary>
        public int MaxVertsPerPoly;
        /// <summary>
        /// Sets the sampling distance to use when generating the detail mesh. (For height detail only.) [Limits: 0 or >= 0.9] [Units: wu] 
        /// </summary>
        public float DetailSampleDist;
        /// <summary>
        /// The maximum distance the detail mesh surface should deviate from heightfield data. (For height detail only.) [Limit: >=0] [Units: wu] 
        /// </summary>
        public float DetailSampleMaxError;
    }
}
