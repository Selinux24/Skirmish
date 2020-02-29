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
        public int I { get; set; }
        /// <summary>
        /// Node
        /// </summary>
        public int N { get; set; }
        /// <summary>
        /// Bounding rectangle Min
        /// </summary>
        public Vector2 Bmin { get; set; }
        /// <summary>
        /// Bounding rectangle Max
        /// </summary>
        public Vector2 Bmax { get; set; }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Index {0}; Count {1}; Min {2} Max {3}", this.I, this.N, this.Bmin, this.Bmax);
        }
    }
}
