using Engine.PathFinding.RecastNavigation.Detour.Tiles;
using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Navigation mesh parameters
    /// </summary>
    [Serializable]
    public struct NavMeshParams
    {
        /// <summary>
        /// Origin
        /// </summary>
        public Vector3 Origin { get; set; }
        /// <summary>
        /// Tile width
        /// </summary>
        public float TileWidth { get; set; }
        /// <summary>
        /// Tile height
        /// </summary>
        public float TileHeight { get; set; }
        /// <summary>
        /// Maximum tiles
        /// </summary>
        public int MaxTiles { get; set; }
        /// <summary>
        /// Maximum polygons
        /// </summary>
        public int MaxPolys { get; set; }

        /// <summary>
        /// Gets the navigation mesh parameters for "solo" creation
        /// </summary>
        /// <param name="generationBounds">Generation bounds</param>
        /// <param name="polyCount">Maximum polygon count</param>
        /// <returns>Returns the navigation mesh parameters</returns>
        public static NavMeshParams GetNavMeshParamsSolo(BoundingBox generationBounds, int polyCount)
        {
            return new NavMeshParams
            {
                Origin = generationBounds.Minimum,
                TileWidth = generationBounds.Maximum.X - generationBounds.Minimum.X,
                TileHeight = generationBounds.Maximum.Z - generationBounds.Minimum.Z,
                MaxTiles = 1,
                MaxPolys = polyCount,
            };
        }
        /// <summary>
        /// Gets the navigation mesh parameters for "tiled" creation
        /// </summary>
        /// <param name="settings">Build settings</param>
        /// <param name="generationBounds">Generation bounds</param>
        /// <returns>Returns the navigation mesh parameters</returns>
        public static NavMeshParams GetNavMeshParamsTiled(BoundingBox generationBounds, BuildSettings settings)
        {
            Config.CalcGridSize(generationBounds, settings.CellSize, out int gridWidth, out int gridHeight);
            int tileSize = (int)settings.TileSize;
            int tileWidth = (gridWidth + tileSize - 1) / tileSize;
            int tileHeight = (gridHeight + tileSize - 1) / tileSize;
            float tileCellSize = settings.TileCellSize;

            int tileBitSize;
            if (settings.UseTileCache)
            {
                tileBitSize = tileWidth * tileHeight * TileCacheParams.EXPECTED_LAYERS_PER_TILE;
            }
            else
            {
                tileBitSize = tileWidth * tileHeight;
            }
            int tileBits = Math.Min((int)Math.Log(Helper.NextPowerOfTwo(tileBitSize), 2), 14);
            int polyBits = 22 - tileBits;
            int maxTiles = 1 << tileBits;
            int maxPolysPerTile = 1 << polyBits;

            return new NavMeshParams
            {
                Origin = generationBounds.Minimum,
                TileWidth = tileCellSize,
                TileHeight = tileCellSize,
                MaxTiles = maxTiles,
                MaxPolys = maxPolysPerTile,
            };
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Origin: {Origin}; TileWidth: {TileWidth}; TileHeight: {TileHeight}; MaxTiles: {MaxTiles}; MaxPolys: {MaxPolys};";
        }
    }
}
