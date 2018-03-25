using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Chunky TriMesh Node
    /// </summary>
    public struct ChunkyTriMeshNode
    {
        /// <summary>
        /// Index
        /// </summary>
        public int i;
        /// <summary>
        /// Node
        /// </summary>
        public int n;
        /// <summary>
        /// Bounding rectangle Min
        /// </summary>
        public Vector2 bmin;
        /// <summary>
        /// Bounding rectangle Max
        /// </summary>
        public Vector2 bmax;

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Index {0}; Count {1}; Min {2} Max {3}", this.i, this.n, this.bmin, this.bmax);
        }
    }
}
