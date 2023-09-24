using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Find nearest polygon query
    /// </summary>
    public class FindNearestPolyQuery : IPolyQuery
    {
        /// <summary>
        /// Navigation query
        /// </summary>
        private readonly NavMeshQuery m_query;
        /// <summary>
        /// Look up center
        /// </summary>
        private readonly Vector3 m_center;
        /// <summary>
        /// Nearest distance squared
        /// </summary>
        private float m_nearestDistanceSqr;
        /// <summary>
        /// Nearest reference
        /// </summary>
        private int m_nearestRef;
        /// <summary>
        /// Nearest point
        /// </summary>
        private Vector3 m_nearestPoint;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="query">Navigation mesh query</param>
        /// <param name="center">Look up center</param>
        public FindNearestPolyQuery(NavMeshQuery query, Vector3 center)
        {
            m_query = query;
            m_center = center;
            m_nearestDistanceSqr = float.MaxValue;
            m_nearestRef = 0;
            m_nearestPoint = Vector3.Zero;
        }

        /// <inheritdoc/>
        public void Process(MeshTile tile, IEnumerable<int> refs)
        {
            if (refs?.Any() != true)
            {
                return;
            }

            foreach (var r in refs)
            {
                m_query.ClosestPointOnPoly(r, m_center, out var closestPtPoly, out bool posOverPoly);

                // If a point is directly over a polygon and closer than
                // climb height, favor that instead of straight line nearest point.
                float d;
                var diff = Vector3.Subtract(m_center, closestPtPoly);
                if (posOverPoly)
                {
                    d = Math.Abs(diff.Y) - tile.Header.WalkableClimb;
                    d = d > 0 ? d * d : 0;
                }
                else
                {
                    d = diff.LengthSquared();
                }

                if (d < m_nearestDistanceSqr)
                {
                    m_nearestPoint = closestPtPoly;

                    m_nearestDistanceSqr = d;
                    m_nearestRef = r;
                }
            }
        }

        /// <summary>
        /// Nearest reference
        /// </summary>
        public int NearestRef()
        {
            return m_nearestRef;
        }
        /// <summary>
        /// Nearest point
        /// </summary>
        public Vector3 NearestPoint()
        {
            return m_nearestPoint;
        }
    }
}
