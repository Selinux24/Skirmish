using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Crowd manager
    /// </summary>
    /// <param name="graph">Internal graph</param>
    public class CrowdManager(Graph graph) : ICrowdManager<GraphAgentType, CrowdAgent>
    {
        /// <summary>
        /// Parent graph
        /// </summary>
        private readonly Graph graph = graph;
        /// <summary>
        /// Crowd list
        /// </summary>
        private readonly List<ICrowd<GraphAgentType, CrowdAgent>> crowds = [];

        /// <inheritdoc/>
        public void AddCrowd<TCrowd>(TCrowd crowd, CrowdSettings settings) where TCrowd : ICrowd<GraphAgentType, CrowdAgent>
        {
            crowd.Initialize(graph, settings);

            crowds.Add(crowd);
        }
        /// <inheritdoc/>
        public void RequestMoveCrowd<TCrowd>(TCrowd crowd, Vector3 p) where TCrowd : ICrowd<GraphAgentType, CrowdAgent>
        {
            //Find agent query
            var query = graph.CreateAgentQuery(crowd.Agent);
            if (query == null)
            {
                return;
            }

            Status status = query.FindNearestPoly(p, crowd.GetQueryExtents(), crowd.GetFilter(0), out int poly, out Vector3 nP);
            if (status == Status.DT_FAILURE)
            {
                return;
            }

            foreach (var ag in crowd.GetAgents())
            {
                ag.RequestMoveTarget(poly, nP);
            }
        }
        /// <inheritdoc/>
        public void RequestMoveAgent<TCrowd>(TCrowd crowd, CrowdAgent crowdAgent, Vector3 p) where TCrowd : ICrowd<GraphAgentType, CrowdAgent>
        {
            //Find agent query
            var query = graph.CreateAgentQuery(crowd.Agent);
            if (query == null)
            {
                return;
            }

            Status status = query.FindNearestPoly(p, crowd.GetQueryExtents(), crowd.GetFilter(0), out int poly, out Vector3 nP);
            if (status == Status.DT_FAILURE)
            {
                return;
            }

            crowdAgent.RequestMoveTarget(poly, nP);
        }
        /// <inheritdoc/>
        public void UpdateCrowds(IGameTime gameTime)
        {
            foreach (var crowd in crowds)
            {
                crowd.Update(gameTime);
            }
        }
    }
}
