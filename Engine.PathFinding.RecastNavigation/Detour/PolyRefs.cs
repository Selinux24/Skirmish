
namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="maxResult">Max results</param>
    public class PolyRefs(int maxResult)
    {
        /// <summary>
        /// Maximum result
        /// </summary>
        private readonly int maxResult = maxResult;
        /// <summary>
        /// The reference ids of the polygons touched by the circle.
        /// </summary>
        private readonly int[] Refs = new int[maxResult];
        /// <summary>
        /// The reference ids of the parent polygons for each result. Zero if a result polygon has no parent.
        /// </summary>
        private readonly int[] Parents = new int[maxResult];
        /// <summary>
        /// The search cost from centerPos to the polygon.
        /// </summary>
        private readonly float[] Costs = new float[maxResult];
        /// <summary>
        /// The number of polygons found.
        /// </summary>
        public int Count { get; private set; } = 0;

        /// <summary>
        /// Appends new reference
        /// </summary>
        /// <param name="reference">Reference</param>
        /// <param name="parent">Parent</param>
        /// <param name="cost">Cost</param>
        public bool Append(int reference, int parent, float cost)
        {
            if (Count >= maxResult)
            {
                return false;
            }

            Refs[Count] = reference;
            Parents[Count] = parent;
            Costs[Count] = cost;
            Count++;

            return true;
        }
        /// <summary>
        /// Gets the reference at index
        /// </summary>
        /// <param name="index">Index</param>
        public int GetReference(int index)
        {
            return Refs[index];
        }
    }
}
