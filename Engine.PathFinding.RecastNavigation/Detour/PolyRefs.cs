
namespace Engine.PathFinding.RecastNavigation.Detour
{
    public class PolyRefs
    {
        /// <summary>
        /// The reference ids of the polygons touched by the circle.
        /// </summary>
        public int[] Refs { get; set; }
        /// <summary>
        /// The reference ids of the parent polygons for each result. Zero if a result polygon has no parent.
        /// </summary>
        public int[] Parents { get; set; }
        /// <summary>
        /// The search cost from centerPos to the polygon.
        /// </summary>
        public float[] Costs { get; set; }
        /// <summary>
        /// The number of polygons found.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxResult">Max results</param>
        public PolyRefs(int maxResult)
        {
            Refs = new int[maxResult];
            Parents = new int[maxResult];
            Costs = new float[maxResult];
            Count = 0;
        }
    }
}
