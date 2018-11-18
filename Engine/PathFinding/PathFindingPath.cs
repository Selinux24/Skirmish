using SharpDX;
using System.Collections.Generic;
using System.Linq;

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
        public List<Vector3> ReturnPath { get; set; } = new List<Vector3>();
        /// <summary>
        /// Normal nodes
        /// </summary>
        public List<Vector3> Normals { get; set; } = new List<Vector3>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="returnPath">Position list</param>
        /// <param name="normals">Normal list</param>
        public PathFindingPath(IEnumerable<Vector3> returnPath, IEnumerable<Vector3> normals)
        {
            if (returnPath?.Any() == true)
            {
                this.ReturnPath.AddRange(returnPath);
            }

            if (normals?.Any() == true)
            {
                this.Normals.AddRange(normals);
            }
        }
    }
}
