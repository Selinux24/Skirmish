namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
	/// A neighboring crowd agent
	/// </summary>
	struct CrowdNeighbor
    {
        /// <summary>
        /// Neighbor agent reference
        /// </summary>
        public Agent Neighbor;
        /// <summary>
        /// Distance to neighbor
        /// </summary>
        public float Distance;
    }
}
