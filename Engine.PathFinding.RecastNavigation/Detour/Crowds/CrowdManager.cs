using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Crowd manager
    /// </summary>
    /// <param name="graph">Internal graph</param>
    public class CrowdManager(Graph graph) : ICrowdManager<Crowd, GraphAgentType, CrowdAgent, CrowdAgentParameters>
    {
        /// <summary>
        /// Parent graph
        /// </summary>
        private readonly Graph graph = graph;
        /// <summary>
        /// Crowd list
        /// </summary>
        private readonly List<Crowd> crowds = [];

        /// <inheritdoc/>
        public Crowd AddCrowd<TSettings>(TSettings settings)
            where TSettings : ICrowdParameters<GraphAgentType>
        {
            var navMesh = (graph.CreateAgentQuery(settings.Agent)?.GetAttachedNavMesh()) ?? throw new ArgumentException($"No navigation mesh found for the specified {nameof(settings.Agent)}.", nameof(settings));

            var cr = new Crowd(navMesh, settings);
            crowds.Add(cr);
            return cr;
        }
        /// <inheritdoc/>
        public void RequestMoveCrowd(Crowd crowd, GraphAgentType agent, Vector3 p)
        {
            //Find agent query
            var query = graph.CreateAgentQuery(agent);
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
        public void RequestMoveAgent(Crowd crowd, CrowdAgent crowdAgent, GraphAgentType agent, Vector3 p)
        {
            //Find agent query
            var query = graph.CreateAgentQuery(agent);
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
