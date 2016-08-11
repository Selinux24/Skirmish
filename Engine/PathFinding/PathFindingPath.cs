using System.Collections.Generic;
using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// PathFinder path
    /// </summary>
    public class PathFindingPath
    {
        /// <summary>
        /// Path nodes
        /// </summary>
        public readonly List<Vector3> ReturnPath = new List<Vector3>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="returnPath">Position list</param>
        public PathFindingPath(Vector3[] returnPath)
        {
            if (returnPath != null && returnPath.Length > 0)
            {
                this.ReturnPath.AddRange(returnPath);
            }
        }
    }
}
