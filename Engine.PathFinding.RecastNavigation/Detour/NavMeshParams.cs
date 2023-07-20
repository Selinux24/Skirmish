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
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override readonly string ToString()
        {
            return $"Origin: {Origin}; TileWidth: {TileWidth}; TileHeight: {TileHeight}; MaxTiles: {MaxTiles}; MaxPolys: {MaxPolys};";
        }
    }
}
