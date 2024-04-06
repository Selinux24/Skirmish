using SharpDX;
using System;
using System.Collections.Generic;

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
        /// Enable debug info
        /// </summary>
        public bool EnableDebugInfo { get; set; }

        /// <summary>
        /// Iterates over the coordinates in the specified bounds
        /// </summary>
        /// <param name="bounds">Bounds</param>
        /// <returns>Returns x and y tile coordinates</returns>
        public readonly IEnumerable<(int tx, int ty)> IterateTilesInBounds(BoundingBox bounds)
        {
            float tw = Width * CellSize;
            float th = Height * CellSize;
            int tx0 = (int)Math.Floor((bounds.Minimum.X - Origin.X) / tw);
            int tx1 = (int)Math.Floor((bounds.Maximum.X - Origin.X) / tw);
            int ty0 = (int)Math.Floor((bounds.Minimum.Z - Origin.Z) / th);
            int ty1 = (int)Math.Floor((bounds.Maximum.Z - Origin.Z) / th);

            for (int ty = ty0; ty <= ty1; ++ty)
            {
                for (int tx = tx0; tx <= tx1; ++tx)
                {
                    yield return (tx, ty);
                }
            }
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Origin: {Origin}; CellSize: {CellSize}; CellHeight: {CellHeight}; Width: {Width}; Height: {Height}; WalkableHeight: {WalkableHeight}; WalkableRadius: {WalkableRadius}; WalkableClimb: {WalkableClimb}; MaxSimplificationError: {MaxSimplificationError}; MaxTiles: {MaxTiles}; MaxObstacles: {MaxObstacles};";
        }
    }
}
