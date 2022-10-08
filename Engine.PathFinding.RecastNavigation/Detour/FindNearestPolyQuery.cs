﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    public class FindNearestPolyQuery : IPolyQuery
    {
        private readonly NavMeshQuery m_query;
        private readonly Vector3 m_center;
        private float m_nearestDistanceSqr;
        private int m_nearestRef;
        private Vector3 m_nearestPoint;

        public FindNearestPolyQuery(NavMeshQuery query, Vector3 center)
        {
            m_query = query;
            m_center = center;
            m_nearestDistanceSqr = float.MaxValue;
            m_nearestRef = 0;
            m_nearestPoint = Vector3.Zero;
        }

        public int NearestRef() { return m_nearestRef; }
        public Vector3 NearestPoint() { return m_nearestPoint; }

        public void Process(MeshTile tile, IEnumerable<int> refs)
        {
            if (refs?.Any() != true)
            {
                return;
            }

            foreach (var r in refs)
            {
                m_query.ClosestPointOnPoly(r, m_center, out Vector3 closestPtPoly, out bool posOverPoly);

                // If a point is directly over a polygon and closer than
                // climb height, favor that instead of straight line nearest point.
                float d;
                Vector3 diff = Vector3.Subtract(m_center, closestPtPoly);
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
    }
}
