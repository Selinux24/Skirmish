using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    public struct HeightfieldLayer
    {
        /// <summary>
        /// The size of each cell. (On the xz-plane.)
        /// </summary>
        public float CS { get; set; }
        /// <summary>
        /// The height of each cell. (The minimum increment along the y-axis.)
        /// </summary>
        public float CH { get; set; }
        /// <summary>
        /// The heightfield. [Size: width * height]
        /// </summary>
        public int[] Heights { get; set; }
        /// <summary>
        /// Area ids. [Size: Same as #heights]
        /// </summary>
        public AreaTypes[] Areas { get; set; }
        /// <summary>
        /// Packed neighbor connection information. [Size: Same as #heights]
        /// </summary>
        public int[] Cons { get; set; }
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; set; }
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
    }
}
