using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache header
    /// </summary>
    [Serializable]
    public struct TileCacheLayerHeader
    {
        /// <summary>
        /// Data magic
        /// </summary>
        public int Magic { get; set; }
        /// <summary>
        /// Data version
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// Tile x
        /// </summary>
        public int TX { get; set; }
        /// <summary>
        /// Tile y
        /// </summary>
        public int TY { get; set; }
        /// <summary>
        /// Tile layer
        /// </summary>
        public int TLayer { get; set; }
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox BBox { get; set; }
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
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            if (this.Magic == 0 && this.Version == 0)
            {
                return "Empty;";
            }

            if (this.Magic != DetourTileCache.DT_TILECACHE_MAGIC)
            {
                return "Invalid;";
            }

            return string.Format("tx {0:000}; ty {1:000}; tlayer {2:000};",
                this.TX, this.TY, this.TLayer);
        }
    }
}
