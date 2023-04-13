// Ported to c# from https://github.com/kevinmoran/GJK
//
//Kevin's implementation of the Gilbert-Johnson-Keerthi intersection algorithm
//and the Expanding Polytope Algorithm
//Most useful references (Huge thanks to all the authors):
//
// "Implementing GJK" by Casey Muratori:
// The best description of the algorithm from the ground up
// https://www.youtube.com/watch?v=Qupqu1xe7Io
//
// "Implementing a GJK Intersection Query" by Phill Djonov
// Interesting tips for implementing the algorithm
// http://vec3.ca/gjk/implementation/
//
// "GJK Algorithm 3D" by Sergiu Craitoiu
// Has nice diagrams to visualize the tetrahedral case
// http://in2gpu.com/2014/05/18/gjk-algorithm-3d/
//
// "GJK + Expanding Polytope Algorithm - Implementation and Visualization"
// Good breakdown of EPA with demo for visualization
// https://www.youtube.com/watch?v=6rgiPrzqt9w
//-----------------------------------------------------------------------------

using SharpDX;

namespace Engine.Physics.GJK
{
    using EPASolver = EPA.Solver;

    /// <summary>
    /// GJK solver class
    /// </summary>
    public static class Solver
    {
        private const int GJK_MAX_NUM_ITERATIONS = 64;

        /// <summary>
        /// Calculates if two colliders are intersecting.
        /// </summary>
        /// <param name="coll1">First collider</param>
        /// <param name="coll2">Second collider</param>
        /// <param name="calcContact">Calculate the minimum translation vector</param>
        /// <param name="point">Contact point</param>
        /// <param name="normal">Contact normal</param>
        /// <param name="penetration">Contact penetration</param>
        /// <remarks>
        /// If <paramref name="calcContact"/> supplied the EPA will be used to find the contact <paramref name="point"/>, <paramref name="normal"/> and <paramref name="penetration"/> to separate coll1 from coll2
        /// </remarks>
        /// <returns>Returns true if the colliders are intersecting</returns>
        public static bool GJK(ICollider coll1, ICollider coll2, bool calcContact, out Vector3 point, out Vector3 normal, out float penetration)
        {
            point = Vector3.Zero;
            normal = Vector3.Zero;
            penetration = 0;

            var simplex = new Simplex();

            if(!simplex.Initialize(coll1, coll2))
            {
                // We didn't reach the origin, won't enclose it
                return false;
            }

            for (int iterations = 0; iterations < GJK_MAX_NUM_ITERATIONS; iterations++)
            {
                if (!simplex.UpdatePoint(coll1, coll2))
                {
                    // We didn't reach the origin, won't enclose it
                    return false;
                }

                if (simplex.Dimension == 3)
                {
                    simplex.UpdateSimplex3();

                    continue;
                }

                if (simplex.UpdateSimplex4())
                {
                    if (calcContact)
                    {
                        var (face, sdist) = EPASolver.EPA(simplex, coll1, coll2);

                        normal = Vector3.Normalize(face.Normal);
                        penetration = sdist;
                        point = coll1.Position + (normal * sdist);
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
