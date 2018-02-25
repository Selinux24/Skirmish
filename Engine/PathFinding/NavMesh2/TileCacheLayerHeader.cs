using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.PathFinding.NavMesh2
{
    [StructLayout(LayoutKind.Sequential)]
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

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            if (this.magic == 0 && this.version == 0)
            {
                return "Empty;";
            }

            if (this.magic != TileCacheMagic)
            {
                return "Invalid;";
            }

            return string.Format("tx {0:000}; ty {1:000}; tlayer {2:000};", 
                this.tx, this.ty, this.tlayer);
        }
    };
}
