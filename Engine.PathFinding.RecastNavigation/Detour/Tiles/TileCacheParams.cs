using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache parameters
    /// </summary>
    [Serializable]
    public struct TileCacheParams
    {
        /// <summary>
        /// Origin
        /// </summary>
        public Vector3 Origin { get; set; }
        /// <summary>
        /// Cell size
        /// </summary>
        public float CellSize { get; set; }
        /// <summary>
        /// Cell height
        /// </summary>
        public float CellHeight { get; set; }
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Walkable height
        /// </summary>
        public float WalkableHeight { get; set; }
        /// <summary>
        /// Walkable radius
        /// </summary>
        public float WalkableRadius { get; set; }
        /// <summary>
        /// Walkable climb
        /// </summary>
        public float WalkableClimb { get; set; }
        /// <summary>
        /// Maximum simplification error
        /// </summary>
        public float MaxSimplificationError { get; set; }
        /// <summary>
        /// Maximum tiles
        /// </summary>
        public int MaxTiles { get; set; }
        /// <summary>
        /// Tile width
        /// </summary>
        public int TileWidth { get; set; }
        /// <summary>
        /// Tile height
        /// </summary>
        public int TileHeight { get; set; }
        /// <summary>
        /// Maximum obstacles
        /// </summary>
        public int MaxObstacles { get; set; }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Origin: {0}; CellSize: {1}; CellHeight: {2}; Width: {3}; Height: {4}; WalkableHeight: {5}; WalkableRadius: {6}; WalkableClimb: {7}; MaxSimplificationError: {8}; MaxTiles: {9}; MaxObstacles: {10};",
                Origin,
                CellSize, CellHeight,
                Width, Height,
                WalkableHeight, WalkableRadius, WalkableClimb,
                MaxSimplificationError, MaxTiles, MaxObstacles);
        }
    }
}
