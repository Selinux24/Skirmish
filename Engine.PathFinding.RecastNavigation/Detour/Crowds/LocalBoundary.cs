using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Local boundary
    /// </summary>
    public class LocalBoundary
    {
        /// <summary>
        /// Maximum local polygons
        /// </summary>
        const int MAX_LOCAL_POLYS = 16;
        /// <summary>
        /// Maximum segments per polygon
        /// </summary>
        const int MAX_SEGS_PER_POLY = IndexedPolygon.DT_VERTS_PER_POLYGON * 3;

        /// <summary>
        /// Center position
        /// </summary>
        private Vector3 m_center = new(float.MaxValue);
        /// <summary>
        /// Segment list
        /// </summary>
        private readonly List<LocalBoundarySegment> m_segs = [];
        /// <summary>
        /// Polygon references
        /// </summary>
        private PolyRefs m_polys = null;

        /// <summary>
        /// Bonudary polygon count
        /// </summary>
        public int PolyCount { get => m_polys?.Count ?? 0; }

        /// <summary>
        /// Gets the polygon reference by index
        /// </summary>
        /// <param name="index">Index</param>
        public int GetPolygonReference(int index)
        {
            return m_polys?.GetReference(index) ?? -1;
        }

        /// <summary>
        /// Reset the local boundary
        /// </summary>
        public void Reset()
        {
            m_center = new(float.MaxValue);
            m_segs.Clear();
        }
        /// <summary>
        /// Updates the boundary
        /// </summary>
        /// <param name="r">Reference</param>
        /// <param name="pos">Position</param>
        /// <param name="collisionQueryRange">Collision range</param>
        /// <param name="navQuery">Query</param>
        /// <param name="filter">Query filter</param>
        public void Update(int r, Vector3 pos, float collisionQueryRange, NavMeshQuery navQuery, QueryFilter filter)
        {
            if (r <= 0)
            {
                m_center = new Vector3(float.MaxValue);
                return;
            }

            m_center = pos;

            float collisionQueryRangeSq = collisionQueryRange * collisionQueryRange;

            // First query non-overlapping polygons.
            navQuery.FindLocalNeighbourhood(r, pos, collisionQueryRange, filter, MAX_LOCAL_POLYS, out m_polys);

            // Secondly, store all polygon edges.
            for (int j = 0; j < m_polys.Count; ++j)
            {
                navQuery.GetPolyWallSegments(m_polys.GetReference(j), filter, MAX_SEGS_PER_POLY, out var segs);

                foreach (var seg in segs)
                {
                    // Skip too distant segments.
                    float distSqr = Utils.DistancePtSegSqr2D(pos, seg.S1, seg.S2, out _);
                    if (distSqr > collisionQueryRangeSq)
                    {
                        continue;
                    }

                    m_segs.Add(new()
                    {
                        S1 = seg.S1,
                        S2 = seg.S2,
                        D = distSqr,
                    });
                }
            }

            if (m_segs.Count <= 1)
            {
                return;
            }

            // Sort neighbour based on the distance.
            m_segs.Sort((s1, s2) =>
            {
                return s1.D.CompareTo(s2.D);
            });
        }

        /// <summary>
        /// Gets the local boundary center
        /// </summary>
        public Vector3 GetCenter()
        {
            return m_center;
        }
        /// <summary>
        /// Gets the segment list
        /// </summary>
        public IEnumerable<LocalBoundarySegment> GetSegments()
        {
            return [.. m_segs];
        }
    }
}