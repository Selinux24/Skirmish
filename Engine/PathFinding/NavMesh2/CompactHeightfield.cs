using SharpDX;

namespace Engine.PathFinding.NavMesh2
{
    public struct CompactHeightfield
    {
        /// <summary>
        /// The width of the heightfield. (Along the x-axis in cell units.)
        /// </summary>
        public int width;
        /// <summary>
        /// The height of the heightfield. (Along the z-axis in cell units.)
        /// </summary>
        public int height;
        /// <summary>
        /// The number of spans in the heightfield.
        /// </summary>
        public int spanCount;
        /// <summary>
        /// The walkable height used during the build of the field.  (See: rcConfig::walkableHeight)
        /// </summary>
        public int walkableHeight;
        /// <summary>
        /// The walkable climb used during the build of the field. (See: rcConfig::walkableClimb)
        /// </summary>
        public int walkableClimb;
        /// <summary>
        /// The AABB border size used during the build of the field. (See: rcConfig::borderSize)
        /// </summary>
        public int borderSize;
        /// <summary>
        /// The maximum distance value of any span within the field.         
        /// </summary>
        public ushort maxDistance;
        /// <summary>
        /// The maximum region id of any span within the field. 
        /// </summary>
        public ushort maxRegions;
        /// <summary>
        /// The minimum bounds in world space. [(x, y, z)]
        /// </summary>
        public BoundingBox boundingBox;
        /// <summary>
        /// The size of each cell. (On the xz-plane.)
        /// </summary>
        public float cs;
        /// <summary>
        /// The height of each cell. (The minimum increment along the y-axis.)
        /// </summary>
        public float ch;
        /// <summary>
        /// Array of cells. [Size: width*height] 
        /// </summary>
        public CompactCell[] cells;
        /// <summary>
        /// Array of spans. [Size: spanCount]
        /// </summary>
        public CompactSpan[] spans;
        /// <summary>
        /// Array containing border distance data. [Size: spanCount]      
        /// </summary>
        public ushort[] dist;
        /// <summary>
        /// Array containing area id data. [Size: spanCount] 
        /// </summary>
        public TileCacheAreas[] areas;
    }
}
