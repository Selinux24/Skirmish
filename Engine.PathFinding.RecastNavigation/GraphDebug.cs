using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Graph debug helper
    /// </summary>
    public struct GraphDebug : IGraphDebug
    {
        /// <inheritdoc/>
        public IGraph Graph { get; private set; }
        /// <inheritdoc/>
        public AgentType Agent { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graph">Graph</param>
        /// <param name="agent">Agent</param>
        public GraphDebug(IGraph graph, AgentType agent)
        {
            Graph = graph;
            Agent = agent;
        }

        /// <inheritdoc/>
        public readonly IEnumerable<(int Id, string Information)> GetAvailableDebugInformation()
        {
            return Enum
                .GetValues<GraphDebugTypes>()
                .Except(new[] { GraphDebugTypes.None })
                .Select(v => ((int)v, v.ToString()));
        }
        /// <inheritdoc/>
        public readonly Dictionary<Color4, IEnumerable<Triangle>> GetInfo(int id)
        {
            GraphDebugTypes debugType = (GraphDebugTypes)id;

            if (debugType == GraphDebugTypes.Nodes)
            {
                var nodes = Graph.GetNodes(Agent).OfType<GraphNode>();
                if (!nodes.Any())
                {
                    return new();
                }

                return nodes
                    .GroupBy(n => n.Color)
                    .ToDictionary(keySelector => keySelector.Key, elementSelector => elementSelector.SelectMany(gn => gn.Triangles).AsEnumerable());
            }

            return new();
        }
    }
}
