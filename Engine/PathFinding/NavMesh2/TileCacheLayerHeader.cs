using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.PathFinding.NavMesh2
{
    public struct TileCacheLayerHeader
    {
        public const int TileCacheMagic = 'D' << 24 | 'T' << 16 | 'L' << 8 | 'R';
        public const int TileCacheVersion = 1;

        /// <summary>
        /// Gets the size of the structure in bytes
        /// </summary>
        public readonly static int Size = Marshal.SizeOf(new TileCacheLayerHeader());

        /// <summary>
        /// Data magic
        /// </summary>
        public int magic;
        /// <summary>
        /// Data version
        /// </summary>
        public int version;

        public int tx;

        public int ty;

        public int tlayer;

        public BoundingBox b;
        /// <summary>
        /// Height min range
        /// </summary>
        public ushort hmin;
        /// <summary>
        /// Height max range
        /// </summary>
        public ushort hmax;
        /// <summary>
        /// Width of the layer.
        /// </summary>
        public byte width;
        /// <summary>
        /// Height of the layer.
        /// </summary>
        public byte height;
        /// <summary>
        /// Minx usable sub-region.
        /// </summary>
        public byte minx;
        /// <summary>
        /// Maxx usable sub-region.
        /// </summary>
        public byte maxx;
        /// <summary>
        /// Miny usable sub-region.
        /// </summary>
        public byte miny;
        /// <summary>
        /// Maxy usable sub-region.
        /// </summary>
        public byte maxy;
    };
}
