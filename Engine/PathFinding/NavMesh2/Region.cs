using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh2
{
    public class Region
    {
        /// <summary>
        /// Number of spans belonging to this region
        /// </summary>
        public int spanCount;
        /// <summary>
        /// ID of the region
        /// </summary>
        public int id;
        /// <summary>
        /// Area type.
        /// </summary>
        public TileCacheAreas areaType;
        public bool remap;
        public bool visited;
        public bool overlap;
        public bool connectsToBorder;
        public int ymin, ymax;
        public List<int> connections;
        public List<int> floors;

        public Region(int i)
        {
            spanCount = 0;
            id = i;
            areaType = TileCacheAreas.NullArea;
            remap = false;
            visited = false;
            overlap = false;
            connectsToBorder = false;
            ymin = int.MaxValue;
            ymax = 0;
        }
    };
}
