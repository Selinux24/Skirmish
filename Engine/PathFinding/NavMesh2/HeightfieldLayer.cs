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
        public int[] heights;
        /// <summary>
        /// Area ids. [Size: Same as #heights]
        /// </summary>
        public TileCacheAreas[] areas;
        /// <summary>
        /// Packed neighbor connection information. [Size: Same as #heights]
        /// </summary>
        public int[] cons;
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox boundingBox;
        /// <summary>
        /// Height min range
        /// </summary>
        public int hmin;
        /// <summary>
        /// Height max range
        /// </summary>
        public int hmax;
        /// <summary>
        /// Width of the layer.
        /// </summary>
        public int width;
        /// <summary>
        /// Height of the layer.
        /// </summary>
        public int height;
        /// <summary>
        /// Minx usable sub-region.
        /// </summary>
        public int minx;
        /// <summary>
        /// Maxx usable sub-region.
        /// </summary>
        public int maxx;
        /// <summary>
        /// Miny usable sub-region.
        /// </summary>
        public int miny;
        /// <summary>
        /// Maxy usable sub-region.
        /// </summary>
        public int maxy;
    }
}
