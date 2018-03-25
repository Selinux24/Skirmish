using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Provides information about raycast hit
    /// </summary>
    public class RaycastHit
    {
        /// <summary>
        /// The hit parameter. (FLT_MAX if no wall hit.)
        /// </summary>
        public float t;
        /// <summary>
        /// The normal of the nearest wall hit. [(x, y, z)]
        /// </summary>
        public Vector3 hitNormal;
        /// <summary>
        /// The index of the edge on the final polygon where the wall was hit.
        /// </summary>
        public int hitEdgeIndex;
        /// <summary>
        /// Pointer to an array of reference ids of the visited polygons. [opt]
        /// </summary>
        public int[] path;
        /// <summary>
        /// The number of visited polygons. [opt]
        /// </summary>
        public int pathCount;
        /// <summary>
        /// The maximum number of polygons the @p path array can hold.
        /// </summary>
        public int maxPath;
        /// <summary>
        /// The cost of the path until hit.
        /// </summary>
        public float pathCost;
    }
}
