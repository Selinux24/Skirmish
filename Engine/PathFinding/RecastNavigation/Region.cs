using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation
{
    public class Region
    {
        /// <summary>
        /// Number of spans belonging to this region
        /// </summary>
        public int spanCount { get; set; }
        /// <summary>
        /// ID of the region
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// Area type.
        /// </summary>
        public TileCacheAreas areaType { get; set; }
        public bool remap { get; set; }
        public bool visited { get; set; }
        public bool overlap { get; set; }
        public bool connectsToBorder { get; set; }
        public int ymin { get; set; }
        public int ymax { get; set; }
        public List<int> connections { get; set; } = new List<int>();
        public List<int> floors { get; set; } = new List<int>();

        public Region(int i)
        {
            spanCount = 0;
            id = i;
            areaType = TileCacheAreas.RC_NULL_AREA;
            remap = false;
            visited = false;
            overlap = false;
            connectsToBorder = false;
            ymin = int.MaxValue;
            ymax = 0;
        }
    };
}
