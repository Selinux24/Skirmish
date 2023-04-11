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

            // Simplex: just a set of points (a is always most recently added)
            var simplex = new Simplex();

            // Initial search direction between colliders
            simplex.SearchDir = coll1.Position - coll2.Position;

            // Get initial point for simplex
            simplex.C = coll2.Support(simplex.SearchDir) - coll1.Support(-simplex.SearchDir);
            // Search in direction of origin
            simplex.SearchDir = -simplex.C;

            // Get second point for a line segment simplex
            simplex.B = coll2.Support(simplex.SearchDir) - coll1.Support(-simplex.SearchDir);

            if (Vector3.Dot(simplex.B, simplex.SearchDir) < 0)
            {
                // We didn't reach the origin, won't enclose it
                return false;
            }

            // Search perpendicular to line segment towards origin
            simplex.SearchDir = Vector3.Cross(Vector3.Cross(simplex.C - simplex.B, -simplex.B), simplex.C - simplex.B);
            if (simplex.SearchDir == Vector3.Zero)
            {
                // Origin is on this line segment, apparently any normal search vector will do?

                // Normal with x-axis
                simplex.SearchDir = Vector3.Cross(simplex.C - simplex.B, Vector3.Right);
                if (simplex.SearchDir == Vector3.Zero)
                {
                    // Normal with z-axis
                    simplex.SearchDir = Vector3.Cross(simplex.C - simplex.B, Vector3.BackwardLH);
                }
            }

            // Simplex dimension
            simplex.Dimension = 2;

            for (int iterations = 0; iterations < GJK_MAX_NUM_ITERATIONS; iterations++)
            {
                simplex.A = coll2.Support(simplex.SearchDir) - coll1.Support(-simplex.SearchDir);

                float dd = Vector3.Dot(simplex.A, simplex.SearchDir);
                if (!MathUtil.IsZero(dd) && dd < 0)
                {
                    // We didn't reach the origin, won't enclose it
                    return false;
                }

                simplex.Dimension++;
                if (simplex.Dimension == 3)
                {
                    simplex = UpdateSimplex3(simplex);

                    continue;
                }

                if (UpdateSimplex4(simplex, out simplex))
                {
                    if (calcContact)
                    {
                        var (_, _, _, n, sdist) = EPASolver.EPA(simplex.A, simplex.B, simplex.C, simplex.D, coll1, coll2);

                        normal = Vector3.Normalize(n);
                        penetration = sdist;
                        point = coll1.Position + (normal * sdist);
                    }

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Triangle case
        /// </summary>
        /// <remarks>
        /// Required winding order:
        ///  b
        ///  | \
        ///  |   \
        ///  |    a
        ///  |   /
        ///  | /
        ///  c
        /// </remarks>
        private static Simplex UpdateSimplex3(Simplex simplex)
        {
            Simplex nSimplex = simplex;

            // Triangle's normal
            Vector3 n = nSimplex.ABC;

            // Direction to origin
            Vector3 AO = nSimplex.AO;

            // Determine which feature is closest to origin, make that the new simplex
            nSimplex.Dimension = 2;
            if (Vector3.Dot(Vector3.Cross(nSimplex.B - nSimplex.A, n), AO) > 0)
            {
                // Closest to edge AB
                nSimplex.C = nSimplex.A;
                nSimplex.SearchDir = Vector3.Cross(Vector3.Cross(nSimplex.B - nSimplex.A, AO), nSimplex.B - nSimplex.A);
                return nSimplex;
            }

            if (Vector3.Dot(Vector3.Cross(n, nSimplex.C - nSimplex.A), AO) > 0)
            {
                // Closest to edge AC
                nSimplex.B = nSimplex.A;
                nSimplex.SearchDir = Vector3.Cross(Vector3.Cross(nSimplex.C - nSimplex.A, AO), nSimplex.C - nSimplex.A);
                return nSimplex;
            }

            nSimplex.Dimension = 3;
            if (Vector3.Dot(n, AO) > 0)
            {
                // Above triangle
                nSimplex.D = nSimplex.C;
                nSimplex.C = nSimplex.B;
                nSimplex.B = nSimplex.A;
                nSimplex.SearchDir = n;
                return nSimplex;
            }

            // Below triangle
            nSimplex.D = nSimplex.B;
            nSimplex.B = nSimplex.A;
            nSimplex.SearchDir = -n;
            return nSimplex;
        }
        /// <summary>
        /// Tetrahedral case
        /// </summary>
        /// <remarks>
        /// (a) is peak/tip of pyramid, BCD is the base (counterclockwise winding order)
        /// </remarks>
        private static bool UpdateSimplex4(Simplex simplex, out Simplex nSimplex)
        {
            nSimplex = simplex;

            // We know a priori that origin is above BCD and below (a)

            //Direction to origin
            Vector3 AO = nSimplex.AO;

            // Hoisting this just cause
            nSimplex.Dimension = 3;

            // Plane-test origin with 3 faces
            // Note: Kind of primitive approach used here; If origin is in front of a face, just use it as the new simplex.
            // We just go through the faces sequentially and exit at the first one which satisfies dot product.
            // Not sure this is optimal or if edges should be considered as possible simplices?
            // Thinking this through in my head I feel like this method is good enough.
            // Makes no difference for AABBS, should test with more complex colliders.

            //Get ABC face normal
            Vector3 ABC = nSimplex.ABC;
            if (Vector3.Dot(ABC, AO) > 0)
            {
                //In front of ABC
                nSimplex.D = nSimplex.C;
                nSimplex.C = nSimplex.B;
                nSimplex.B = nSimplex.A;
                nSimplex.SearchDir = ABC;
                return false;
            }

            //Get ACD face normal
            Vector3 ACD = nSimplex.ACD;
            if (Vector3.Dot(ACD, AO) > 0)
            {
                // In front of ACD
                nSimplex.B = nSimplex.A;
                nSimplex.SearchDir = ACD;
                return false;
            }

            //Get ADB face normal
            Vector3 ADB = nSimplex.ADB;
            if (Vector3.Dot(ADB, AO) > 0)
            {
                // In front of ADB
                nSimplex.C = nSimplex.D;
                nSimplex.D = nSimplex.B;
                nSimplex.B = nSimplex.A;
                nSimplex.SearchDir = ADB;
                return false;
            }

            //Inside tetrahedron. Enclosed!
            return true;

            // Note: in the case where two of the faces have similar normals, the origin could conceivably be closest to an edge on the tetrahedron
            // Right now I don't think it'll make a difference to limit our new simplices to just one of the faces, maybe test it later.
        }
    }

    /// <summary>
    /// Simplex data
    /// </summary>
    public struct Simplex
    {
        public Vector3 A { get; set; }
        public Vector3 B { get; set; }
        public Vector3 C { get; set; }
        public Vector3 D { get; set; }
        public int Dimension { get; set; }
        public Vector3 SearchDir { get; set; }

        /// <summary>
        /// Direction to origin
        /// </summary>
        public Vector3 AO { get { return -A; } }
        /// <summary>
        /// ABC face normal
        /// </summary>
        public Vector3 ABC { get { return Vector3.Cross(B - A, C - A); } }
        /// <summary>
        /// ACB face normal
        /// </summary>
        public Vector3 ACD { get { return Vector3.Cross(C - A, D - A); } }
        /// <summary>
        /// ADB face normal
        /// </summary>
        public Vector3 ADB { get { return Vector3.Cross(D - A, B - A); } }
    }
}