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
        /// This value specifies how many layers (or "floors") each navmesh tile is expected to have.
        /// </summary>
        public const int EXPECTED_LAYERS_PER_TILE = 4;

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
            int tx0 = (int)MathF.Floor((bounds.Minimum.X - Origin.X) / tw);
            int tx1 = (int)MathF.Floor((bounds.Maximum.X - Origin.X) / tw);
            int ty0 = (int)MathF.Floor((bounds.Minimum.Z - Origin.Z) / th);
            int ty1 = (int)MathF.Floor((bounds.Maximum.Z - Origin.Z) / th);

            for (int ty = ty0; ty <= ty1; ++ty)
            {
                for (int tx = tx0; tx <= tx1; ++tx)
                {
                    yield return (tx, ty);
                }
            }
        }

        /// <summary>
        /// Gets the tile-cache creation parameters
        /// </summary>
        /// <param name="settings">Build settings</param>
        /// <param name="agent">Agent</param>
        /// <param name="generationBounds">Generation bounds</param>
        public static TileCacheParams GetTileCacheParams(BuildSettings settings, GraphAgentType agent, BoundingBox generationBounds)
        {
            Config.CalcGridSize(generationBounds, settings.CellSize, out int gridWidth, out int gridHeight);
            int tileSize = (int)settings.TileSize;
            int tileWidth = (gridWidth + tileSize - 1) / tileSize;
            int tileHeight = (gridHeight + tileSize - 1) / tileSize;

            // Tile cache params.
            return new()
            {
                Origin = generationBounds.Minimum,
                CellSize = settings.CellSize,
                CellHeight = settings.CellHeight,
                Width = tileSize,
                Height = tileSize,
                WalkableHeight = agent.Height,
                WalkableRadius = agent.Radius,
                WalkableClimb = agent.MaxClimb,
                MaxSimplificationError = settings.EdgeMaxError,
                MaxTiles = tileWidth * tileHeight * EXPECTED_LAYERS_PER_TILE,
                TileWidth = tileWidth,
                TileHeight = tileHeight,
                MaxObstacles = 128,
                
                EnableDebugInfo = settings.EnableDebugInfo,
            };
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Origin: {Origin}; CellSize: {CellSize}; CellHeight: {CellHeight}; Width: {Width}; Height: {Height}; WalkableHeight: {WalkableHeight}; WalkableRadius: {WalkableRadius}; WalkableClimb: {WalkableClimb}; MaxSimplificationError: {MaxSimplificationError}; MaxTiles: {MaxTiles}; MaxObstacles: {MaxObstacles};";
        }
    }
}
