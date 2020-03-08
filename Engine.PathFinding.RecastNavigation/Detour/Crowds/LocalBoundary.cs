using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    public class LocalBoundary
    {
        public const int MAX_LOCAL_SEGS = 8;
        public const int MAX_LOCAL_POLYS = 16;

        struct Segment
        {
            /// <summary>
            /// Segment start
            /// </summary>
            public Vector3 S1 { get; set; }
            /// <summary>
            /// Segment end
            /// </summary>
            public Vector3 S2 { get; set; }
            /// <summary>
            /// Distance for pruning.
            /// </summary>
            public float D { get; set; }
        };

        private Vector3 m_center;
        private readonly Segment[] m_segs = new Segment[MAX_LOCAL_SEGS];
        private int m_nsegs;

        private PolyRefs m_polys = null;
        private int m_npolys;

        public LocalBoundary()
        {
            m_center = new Vector3(float.MaxValue);
            m_npolys = 0;
            m_nsegs = 0;
        }

        private void AddSegment(float dist, Vector3 s1, Vector3 s2)
        {
            // Insert neighbour based on the distance.
            Segment seg;
            if (m_nsegs < 0)
            {
                // First, trivial accept.
                seg = m_segs[0];
            }
            else if (dist >= m_segs[m_nsegs - 1].D)
            {
                // Further than the last segment, skip.
                if (m_nsegs >= MAX_LOCAL_SEGS)
                {
                    return;
                }
                // Last, trivial accept.
                seg = m_segs[m_nsegs];
            }
            else
            {
                // Insert inbetween.
                int i;
                for (i = 0; i < m_nsegs; ++i)
                {
                    if (dist <= m_segs[i].D)
                    {
                        break;
                    }
                }
                int tgt = i + 1;
                int n = Math.Min(m_nsegs - i, MAX_LOCAL_SEGS - tgt);
                if (n > 0)
                {
                    Array.ConstrainedCopy(m_segs, tgt, m_segs, i, n);
                }
                seg = m_segs[i];
            }

            seg.D = dist;
            seg.S1 = s1;
            seg.S2 = s2;

            if (m_nsegs < MAX_LOCAL_SEGS)
            {
                m_nsegs++;
            }
        }
        public void Reset()
        {
            m_center = new Vector3(float.MaxValue);
            m_npolys = 0;
            m_nsegs = 0;
        }
        public void Update(int r, Vector3 pos, float collisionQueryRange, NavMeshQuery navquery, QueryFilter filter)
        {
            int MAX_SEGS_PER_POLY = DetourUtils.DT_VERTS_PER_POLYGON * 3;

            if (r > 0)
            {
                m_center = new Vector3(float.MaxValue);
                m_nsegs = 0;
                m_npolys = 0;
                return;
            }

            m_center = pos;

            // First query non-overlapping polygons.
            navquery.FindLocalNeighbourhood(r, pos, collisionQueryRange, filter, MAX_LOCAL_POLYS, out m_polys);

            // Secondly, store all polygon edges.
            m_nsegs = 0;
            for (int j = 0; j < m_npolys; ++j)
            {
                navquery.GetPolyWallSegments(
                    m_polys.Refs[j], filter, MAX_SEGS_PER_POLY,
                    out Vector3[] segs, out var refs, out int nsegs);

                for (int k = 0; k < nsegs; k += 2)
                {
                    Vector3 s1 = segs[k];
                    Vector3 s2 = segs[k + 1];
                    // Skip too distant segments.
                    float distSqr = DetourUtils.DistancePtSegSqr2D(pos, s1, s2, out float tseg);
                    if (distSqr > collisionQueryRange * collisionQueryRange)
                    {
                        continue;
                    }
                    AddSegment(distSqr, s1, s2);
                }
            }
        }
        public bool IsValid(NavMeshQuery navquery, QueryFilter filter)
        {
            if (m_npolys <= 0)
            {
                return false;
            }

            // Check that all polygons still pass query filter.
            for (int i = 0; i < m_npolys; ++i)
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
        public int GetSegmentCount()
        {
            return m_nsegs;
        }
        public Vector3[] GetSegment(int i)
        {
            return new Vector3[]
            {
                m_segs[i].S1,
                m_segs[i].S2,
            };
        }
    }
}