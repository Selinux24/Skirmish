using SharpDX;

namespace Engine.PathFinding.NavMesh2
{
    public struct HeightfieldLayer
    {
        /// <summary>
        /// The size of each cell. (On the xz-plane.)
        /// </summary>
        public float cs;
        /// <summary>
        /// The height of each cell. (The minimum increment along the y-axis.)
        /// </summary>
        public float ch;
        /// <summary>
        /// The heightfield. [Size: width * height]
        /// </summary>
        public byte[] heights;
        /// <summary>
        /// Area ids. [Size: Same as #heights]
        /// </summary>
        public TileCacheAreas[] areas;
        /// <summary>
        /// Packed neighbor connection information. [Size: Same as #heights]
        /// </summary>
        public byte[] cons;
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox boundingBox;
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
    }
}
