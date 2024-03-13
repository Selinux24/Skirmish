using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    public class LocalBoundary
    {
        public const int MAX_LOCAL_POLYS = 16;

        private Vector3 m_center;
        private readonly List<LocalBoundarySegment> m_segs = [];

        private PolyRefs m_polys = null;

        public LocalBoundary()
        {
            m_center = new Vector3(float.MaxValue);
        }

        public void Reset()
        {
            m_center = new Vector3(float.MaxValue);
            m_segs.Clear();
        }
        public void Update(int r, Vector3 pos, float collisionQueryRange, NavMeshQuery navquery, QueryFilter filter)
        {
            int MAX_SEGS_PER_POLY = IndexedPolygon.DT_VERTS_PER_POLYGON * 3;
            float collisionQueryRangeSq = collisionQueryRange * collisionQueryRange;

            if (r <= 0)
            {
                m_center = new Vector3(float.MaxValue);
                return;
            }

            m_center = pos;

            // First query non-overlapping polygons.
            navquery.FindLocalNeighbourhood(r, pos, collisionQueryRange, filter, MAX_LOCAL_POLYS, out m_polys);

            // Secondly, store all polygon edges.
            for (int j = 0; j < m_polys.Count; ++j)
            {
                navquery.GetPolyWallSegments(m_polys.Refs[j], filter, MAX_SEGS_PER_POLY, out var segs);

                foreach (var seg in segs)
                {
                    // Skip too distant segments.
                    float distSqr = Utils.DistancePtSegSqr2D(pos, seg.S1, seg.S2, out _);
                    if (distSqr > collisionQueryRangeSq)
                    {
                        continue;
                    }

                    m_segs.Add(new LocalBoundarySegment
                    {
                        S1 = seg.S1,
                        S2 = seg.S2,
                        D = distSqr,
                    });
                }
            }

            if (m_segs.Count > 1)
            {
                // Sort neighbour based on the distance.
                m_segs.Sort((s1, s2) =>
                {
                    return s1.D.CompareTo(s2.D);
                });
            }
        }
        public bool IsValid(NavMeshQuery navquery, QueryFilter filter)
        {
            if (m_polys.Count <= 0)
            {
                return false;
            }

            // Check that all polygons still pass query filter.
            for (int i = 0; i < m_polys.Count; ++i)
            {
                if (!navquery.IsValidPolyRef(m_polys.Refs[i], filter))
                {
                    return false;
                }
            }

            return true;
        }

        public Vector3 GetCenter()
        {
            return m_center;
        }
        public IEnumerable<LocalBoundarySegment> GetSegments()
        {
            return [.. m_segs];
        }
    }
}