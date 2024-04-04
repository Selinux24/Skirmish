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
        /// Magic number
        /// </summary>
        const int DT_TILECACHE_MAGIC = 'D' << 24 | 'T' << 16 | 'L' << 8 | 'R';
        /// <summary>
        /// Version number
        /// </summary>
        const int DT_TILECACHE_VERSION = 1;

        /// <summary>
        /// Data magic
        /// </summary>
        public int Magic { get; private set; } = DT_TILECACHE_MAGIC;
        /// <summary>
        /// Data version
        /// </summary>
        public int Version { get; private set; } = DT_TILECACHE_VERSION;
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
        public BoundingBox Bounds { get; set; }
        /// <summary>
        /// Width of the layer.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height of the layer.
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Height min range
        /// </summary>
        public int HMin { get; set; }
        /// <summary>
        /// Height max range
        /// </summary>
        public int HMax { get; set; }
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
        /// Constructor
        /// </summary>
        public TileCacheLayerHeader()
        {

        }

        /// <summary>
        /// Calculates the tile header bounds
        /// </summary>
        /// <param name="cs">Cell size</param>
        public readonly BoundingBox GetTightTileBounds(float cs)
        {
            var bmin = new Vector3();
            var bmax = new Vector3();

            bmin.X = Bounds.Minimum.X + MinX * cs;
            bmin.Y = Bounds.Minimum.Y;
            bmin.Z = Bounds.Minimum.Z + MinY * cs;
            bmax.X = Bounds.Minimum.X + (MaxX + 1) * cs;
            bmax.Y = Bounds.Maximum.Y;
            bmax.Z = Bounds.Minimum.Z + (MaxY + 1) * cs;

            return new(bmin, bmax);
        }
        /// <summary>
        /// Validates the header magic number and version
        /// </summary>
        public readonly bool IsValid()
        {
            if (Magic != DT_TILECACHE_MAGIC)
            {
                return false;
            }
            if (Version != DT_TILECACHE_VERSION)
            {
                return false;
            }

            return true;
        }
     
        /// <inheritdoc/>
        public override readonly string ToString()
        {
            if (Magic == 0 && Version == 0)
            {
                return "Empty;";
            }

            if (Magic != DT_TILECACHE_MAGIC)
            {
                return "Invalid;";
            }

            return $"tx {TX:000}; ty {TY:000}; tlayer {TLayer:000};";
        }
    }
}
