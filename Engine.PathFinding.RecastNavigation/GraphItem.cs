using System;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Graph item
    /// </summary>
    [Serializable]
    class GraphItem
    {
        /// <summary>
        /// Id counter
        /// </summary>
        private static int ID = 0;
        /// <summary>
        /// Gets the next Id
        /// </summary>
        /// <returns>Returns the next Id</returns>
        private static int GetNextId()
        {
            return ++ID;
        }

        /// <summary>
        /// Graph item id
        /// </summary>
        public readonly int Id;
        /// <summary>
        /// Index list per agent
        /// </summary>
        public Tuple<GraphAgentType, int>[] Indices { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GraphItem()
        {
            Id = GetNextId();
        }
    }
}
