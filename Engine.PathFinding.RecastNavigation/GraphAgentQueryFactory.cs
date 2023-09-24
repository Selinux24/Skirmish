using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;

    /// <summary>
    /// Graph agent query
    /// </summary>
    class GraphAgentQueryFactory : IDisposable
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
        public GraphAgentQueryFactory()
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~GraphAgentQueryFactory()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
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
                Agent = null;
                NavMesh = null;
            }
        }

        /// <summary>
        /// Creates a navigation mesh query for the agent in the navigation mesh
        /// </summary>
        /// <returns>Returns the new navigation mesh query</returns>
        public NavMeshQuery CreateQuery()
        {
            return new NavMeshQuery(NavMesh, MaxNodes);
        }
        /// <summary>
        /// Removes the tiles in the list
        /// </summary>
        /// <param name="tiles">Tile list</param>
        public void RemoveTiles(IEnumerable<UpdateTileData> tiles)
        {
            foreach (var tile in tiles)
            {
                NavMesh.RemoveTilesAtPosition(tile.X, tile.Y);
            }
        }
    }
}
