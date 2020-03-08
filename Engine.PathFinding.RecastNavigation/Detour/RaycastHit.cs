using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Provides information about raycast hit
    /// </summary>
    public class RaycastHit
    {
        /// <summary>
        /// The hit parameter. (FLT_MAX if no wall hit.)
        /// </summary>
        public float T { get; set; }
        /// <summary>
        /// The normal of the nearest wall hit. [(x, y, z)]
        /// </summary>
        public Vector3 HitNormal { get; set; }
        /// <summary>
        /// The index of the edge on the final polygon where the wall was hit.
        /// </summary>
        public int HitEdgeIndex { get; set; }
        /// <summary>
        /// Pointer to an array of reference ids of the visited polygons. [opt]
        /// </summary>
        public int[] Path { get; set; }
        /// <summary>
        /// The number of visited polygons. [opt]
        /// </summary>
        public int PathCount { get; set; }
        /// <summary>
        /// The maximum number of polygons the @p path array can hold.
        /// </summary>
        public int MaxPath { get; set; }
        /// <summary>
        /// The cost of the path until hit.
        /// </summary>
        public float PathCost { get; set; }
    }
}
