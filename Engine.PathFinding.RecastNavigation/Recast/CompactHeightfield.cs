using SharpDX;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    public class CompactHeightfield
    {
        /// <summary>
        /// The width of the heightfield. (Along the x-axis in cell units.)
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// The height of the heightfield. (Along the z-axis in cell units.)
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// The number of spans in the heightfield.
        /// </summary>
        public int SpanCount { get; set; }
        /// <summary>
        /// The walkable height used during the build of the field.  (See: rcConfig::walkableHeight)
        /// </summary>
        public int WalkableHeight { get; set; }
        /// <summary>
        /// The walkable climb used during the build of the field. (See: rcConfig::walkableClimb)
        /// </summary>
        public int WalkableClimb { get; set; }
        /// <summary>
        /// The AABB border size used during the build of the field. (See: rcConfig::borderSize)
        /// </summary>
        public int BorderSize { get; set; }
        /// <summary>
        /// The maximum distance value of any span within the field.         
        /// </summary>
        public int MaxDistance { get; set; }
        /// <summary>
        /// The maximum region id of any span within the field. 
        /// </summary>
        public int MaxRegions { get; set; }
        /// <summary>
        /// The minimum bounds in world space. [(x, y, z)]
        /// </summary>
        public BoundingBox BoundingBox { get; set; }
        /// <summary>
        /// The size of each cell. (On the xz-plane.)
        /// </summary>
        public float CS { get; set; }
        /// <summary>
        /// The height of each cell. (The minimum increment along the y-axis.)
        /// </summary>
        public float CH { get; set; }
        /// <summary>
        /// Array of cells. [Size: width*height] 
        /// </summary>
        public CompactCell[] Cells { get; set; }
        /// <summary>
        /// Array of spans. [Size: spanCount]
        /// </summary>
        public CompactSpan[] Spans { get; set; }
        /// <summary>
        /// Array containing border distance data. [Size: spanCount]      
        /// </summary>
        public int[] Dist { get; set; }
        /// <summary>
        /// Array containing area id data. [Size: spanCount] 
        /// </summary>
        public AreaTypes[] Areas { get; set; }

        /// <summary>
        /// Marks the geometry areas into the heightfield
        /// </summary>
        /// <param name="geometry">Geometry input</param>
        public void MarkAreas(InputGeometry geometry)
        {
            var vols = geometry.GetAreas();
            var vCount = geometry.GetAreaCount();
            for (int i = 0; i < vCount; i++)
            {
                var vol = vols.ElementAt(i);

                RecastUtils.MarkConvexPolyArea(
                    vol.Vertices, vol.VertexCount,
                    vol.MinHeight, vol.MaxHeight,
                    (AreaTypes)vol.AreaType, this);
            }
        }
    }
}
