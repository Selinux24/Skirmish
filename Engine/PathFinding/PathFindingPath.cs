using System;
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
        /// Path identifier
        /// </summary>
        public readonly Guid Id = Guid.NewGuid();
        /// <summary>
        /// Path nodes
        /// </summary>
        public readonly List<Vector3> ReturnPath = new List<Vector3>();
        /// <summary>
        /// Start position
        /// </summary>
        public Vector3 StartPosition
        {
            get
            {
                return this.ReturnPath[0];
            }
        }
        /// <summary>
        /// End position
        /// </summary>
        public Vector3 EndPosition
        {
            get
            {
                return this.ReturnPath[this.ReturnPath.Count - 1];
            }
        }
        /// <summary>
        /// Total distance
        /// </summary>
        public float Distance
        {
            get
            {
                float distance = 0f;

                for (int i = 0; i < this.ReturnPath.Count - 1; i++)
                {
                    distance += Vector3.Distance(this.ReturnPath[i], this.ReturnPath[i + 1]);
                }

                return distance;
            }
        }

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
