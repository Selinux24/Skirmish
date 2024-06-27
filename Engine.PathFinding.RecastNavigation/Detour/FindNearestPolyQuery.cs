using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Find nearest polygon query
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="navMesh">Navigation mesh</param>
    /// <param name="center">Look up center</param>
    public class FindNearestPolyQuery(NavMesh navMesh, Vector3 center) : IPolyQuery
    {
        /// <summary>
        /// Navigation mesh
        /// </summary>
        private readonly NavMesh navMesh = navMesh;
        /// <summary>
        /// Look up center
        /// </summary>
        private readonly Vector3 center = center;
        /// <summary>
        /// Nearest distance squared
        /// </summary>
        private float nearestDistanceSqr = float.MaxValue;
        /// <summary>
        /// Nearest reference
        /// </summary>
        private int nearestRef = 0;
        /// <summary>
        /// Nearest point
        /// </summary>
        private Vector3 nearestPoint = Vector3.Zero;

        /// <inheritdoc/>
        public void Process(MeshTile tile, IEnumerable<int> refs)
        {
            if (refs?.Any() != true)
            {
                return;
            }

            foreach (var r in refs)
            {
                navMesh.ClosestPointOnPoly(r, center, out var closestPtPoly, out bool posOverPoly);

                // If a point is directly over a polygon and closer than
                // climb height, favor that instead of straight line nearest point.
                float d;
                var diff = Vector3.Subtract(center, closestPtPoly);
                if (posOverPoly)
                {
                    d = MathF.Abs(diff.Y) - tile.Header.WalkableClimb;
                    d = d > 0 ? d * d : 0;
                }
                else
                {
                    d = diff.LengthSquared();
                }

                if (d < nearestDistanceSqr)
                {
                    nearestPoint = closestPtPoly;

                    nearestDistanceSqr = d;
                    nearestRef = r;
                }
            }
        }

        /// <summary>
        /// Nearest reference
        /// </summary>
        public int NearestRef()
        {
            return nearestRef;
        }
        /// <summary>
        /// Nearest point
        /// </summary>
        public Vector3 NearestPoint()
        {
            return nearestPoint;
        }
    }
}
