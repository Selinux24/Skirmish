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
        public override string ToString()
        {
            return string.Format("Origin: {0}; TileWidth: {1}; TileHeight: {2}; MaxTiles: {3}; MaxPolys: {4};",
                Origin,
                TileWidth, TileHeight,
                MaxTiles, MaxPolys);
        }
    }
}
