using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Defines polygon filtering and traversal costs for navigation mesh query operations.
    /// </summary>
    public class QueryFilter
    {
        /// <summary>
        /// The maximum number of user defined area ids.
        /// </summary>
        const int DT_MAX_AREAS = 64;

        /// <summary>
        /// Cost per area type. (Used by default implementation.)
        /// </summary>
        public float[] AreaCost { get; set; }
        /// <summary>
        /// Flags for polygons that can be visited. (Used by default implementation.)
        /// </summary>
        public SamplePolyFlagTypes IncludeFlags { get; set; }
        /// <summary>
        /// Flags for polygons that should not be visted. (Used by default implementation.)
        /// </summary>
        public SamplePolyFlagTypes ExcludeFlags { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public QueryFilter()
        {
            AreaCost = Helper.CreateArray(DT_MAX_AREAS, 1.0f);

            SetAreaCost(SamplePolyAreas.Ground, 1.0f);
            SetAreaCost(SamplePolyAreas.Water, 10.0f);
            SetAreaCost(SamplePolyAreas.Road, 1.0f);
            SetAreaCost(SamplePolyAreas.Door, 1.0f);
            SetAreaCost(SamplePolyAreas.Grass, 2.0f);
            SetAreaCost(SamplePolyAreas.Jump, 1.5f);

            IncludeFlags = SamplePolyFlagTypes.Walk;
            ExcludeFlags = SamplePolyFlagTypes.None;
        }

        /// <summary>
        /// Returns true if the polygon can be visited. (I.e. Is traversable.)
        /// </summary>
        /// <param name="polyFlags">Sample polygon flags.</param>
        /// <returns>Returns true if the filter pass</returns>
        public virtual bool PassFilter(SamplePolyFlagTypes polyFlags)
        {
            return
                (polyFlags & IncludeFlags) != 0 &&
                (polyFlags & ExcludeFlags) == 0;
        }
        /// <summary>
        /// Returns cost to move from the beginning to the end of a line segment that is fully contained within a polygon.
        /// </summary>
        /// <param name="pa">The start position on the edge of the previous and current polygon. [(x, y, z)]</param>
        /// <param name="pb">The end position on the edge of the current and next polygon. [(x, y, z)]</param>
        /// <param name="prev">The tile containing the previous polygon. [opt]</param>
        /// <param name="cur">The tile containing the current polygon.</param>
        /// <param name="next">The tile containing the next polygon. [opt]</param>
        /// <returns></returns>
        public virtual float GetCost(Vector3 pa, Vector3 pb, TileRef prev, TileRef cur, TileRef next)
        {
            return Vector3.Distance(pa, pb) * AreaCost[(int)cur.Poly.Area];
        }

        /// <summary>
        /// Returns the traversal cost of the area.
        /// </summary>
        /// <param name="i">The id of the area.</param>
        /// <returns>The traversal cost of the area.</returns>
        public float GetAreaCost(SamplePolyAreas i) { return AreaCost[(int)i]; }
        /// <summary>
        /// Sets the traversal cost of the area.
        /// </summary>
        /// <param name="i">The id of the area.</param>
        /// <param name="cost">The new cost of traversing the area.</param>
        public void SetAreaCost(SamplePolyAreas i, float cost) { AreaCost[(int)i] = cost; }

        /// <summary>
        /// Returns the include flags for the filter.
        /// Any polygons that include one or more of these flags will be included in the operation.
        /// </summary>
        /// <returns></returns>
        public SamplePolyFlagTypes GetIncludeFlags() { return IncludeFlags; }
        /// <summary>
        /// Sets the include flags for the filter.
        /// </summary>
        /// <param name="flags">The new flags.</param>
        public void SetIncludeFlags(SamplePolyFlagTypes flags) { IncludeFlags = flags; }

        /// <summary>
        /// Returns the exclude flags for the filter.
        /// Any polygons that include one ore more of these flags will be excluded from the operation.
        /// </summary>
        /// <returns></returns>
        public SamplePolyFlagTypes GetExcludeFlags() { return ExcludeFlags; }
        /// <summary>
        /// Sets the exclude flags for the filter.
        /// </summary>
        /// <param name="flags">The new flags.</param>
        public void SetExcludeFlags(SamplePolyFlagTypes flags) { ExcludeFlags = flags; }

        /// <summary>
        /// Evaluates area type and returns spected flag type
        /// </summary>
        /// <param name="area">Area type</param>
        /// <returns>Returns the flag type</returns>
        public static SamplePolyFlagTypes EvaluateArea(SamplePolyAreas area)
        {
            if (area == SamplePolyAreas.Ground ||
                area == SamplePolyAreas.Grass ||
                area == SamplePolyAreas.Road)
            {
                return SamplePolyFlagTypes.Walk;
            }
            else if (area == SamplePolyAreas.Water)
            {
                return SamplePolyFlagTypes.Swim;
            }
            else if (area == SamplePolyAreas.Door)
            {
                return SamplePolyFlagTypes.Walk | SamplePolyFlagTypes.Door;
            }

            return SamplePolyFlagTypes.None;
        }
    }
}
