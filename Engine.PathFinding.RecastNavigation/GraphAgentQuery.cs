using System;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;

    /// <summary>
    /// Graph agent query
    /// </summary>
    public class GraphAgentQuery : IDisposable
    {
        /// <summary>
        /// Agent
        /// </summary>
        public Agent Agent { get; set; }
        /// <summary>
        /// Navigation mesh
        /// </summary>
        public NavMesh NavMesh { get; set; }
        /// <summary>
        /// Maximum nodes in the query
        /// </summary>
        public int MaxNodes { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GraphAgentQuery()
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~GraphAgentQuery()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Agent = null;
                this.NavMesh = null;
            }
        }

        /// <summary>
        /// Creates a navigation mesh query for the agent in the navigation mesh
        /// </summary>
        /// <returns>Returns the new navigation mesh query</returns>
        public NavMeshQuery CreateQuery()
        {
            var nm = new NavMeshQuery();
            nm.Init(this.NavMesh, this.MaxNodes);
            return nm;
        }
    }
}
