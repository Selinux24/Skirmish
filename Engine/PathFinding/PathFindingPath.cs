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
        /// Normal nodes
        /// </summary>
        public readonly List<Vector3> Normals = new List<Vector3>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="returnPath">Position list</param>
        /// <param name="normals">Normal list</param>
        public PathFindingPath(Vector3[] returnPath, Vector3[] normals)
        {
            if (returnPath != null && returnPath.Length > 0)
            {
                this.ReturnPath.AddRange(returnPath);
            }

            if (normals != null && normals.Length > 0)
            {
                this.Normals.AddRange(normals);
            }
        }
    }
}
