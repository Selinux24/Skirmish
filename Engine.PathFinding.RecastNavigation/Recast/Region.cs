using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    public class Region
    {
        /// <summary>
        /// Number of spans belonging to this region
        /// </summary>
        public int SpanCount { get; set; }
        /// <summary>
        /// ID of the region
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Area type.
        /// </summary>
        public AreaTypes AreaType { get; set; }
        public bool Remap { get; set; }
        public bool Visited { get; set; }
        public bool Overlap { get; set; }
        public bool ConnectsToBorder { get; set; }
        public int YMin { get; set; }
        public int YMax { get; set; }
        public List<int> Connections { get; set; } = new List<int>();
        public List<int> Floors { get; set; } = new List<int>();

        public Region(int i)
        {
            SpanCount = 0;
            Id = i;
            AreaType = AreaTypes.RC_NULL_AREA;
            Remap = false;
            Visited = false;
            Overlap = false;
            ConnectsToBorder = false;
            YMin = int.MaxValue;
            YMax = 0;
        }
    };
}
