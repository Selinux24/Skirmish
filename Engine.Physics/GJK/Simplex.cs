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
    /// <summary>
    /// Simplex. Just a set of points (a is always most recently added)
    /// </summary>
    public struct Simplex
    {
        private static readonly Vector3 ZeroTolerance = new Vector3(0.0001f);

        /// <summary>
        /// Point A. Last added point
        /// </summary>
        public SupportPoint A { get; set; }
        /// <summary>
        /// Point B
        /// </summary>
        public SupportPoint B { get; set; }
        /// <summary>
        /// Point C
        /// </summary>
        public SupportPoint C { get; set; }
        /// <summary>
        /// Point D. Ignored if triangle
        /// </summary>
        public SupportPoint D { get; set; }
        /// <summary>
        /// Simplex dimension
        /// </summary>
        public int Dimension { get; set; }
        /// <summary>
        /// Search direction
        /// </summary>
        public Vector3 SearchDir { get; set; }

        /// <summary>
        /// Direction to origin
        /// </summary>
        public Vector3 AO { get { return -A.Point; } }
        /// <summary>
        /// ABC face normal
        /// </summary>
        public Vector3 ABC { get { return Vector3.Cross(B.Point - A.Point, C.Point - A.Point); } }
        /// <summary>
        /// ACB face normal
        /// </summary>
        public Vector3 ACD { get { return Vector3.Cross(C.Point - A.Point, D.Point - A.Point); } }
        /// <summary>
        /// ADB face normal
        /// </summary>
        public Vector3 ADB { get { return Vector3.Cross(D.Point - A.Point, B.Point - A.Point); } }

        /// <summary>
        /// Initializes the simplex, and performs early exit test
        /// </summary>
        /// <param name="coll1">First collider</param>
        /// <param name="coll2">Second collider</param>
        public bool Initialize(ICollider coll1, ICollider coll2)
        {
            // Initial search direction between colliders
            SearchDir = coll1.Position - coll2.Position;

            // Get initial point for simplex
            C = new SupportPoint(coll1, coll2, SearchDir);

            // Search in direction of origin
            SearchDir = -C.Point;

            // Get second point for a line segment simplex
            B = new SupportPoint(coll1, coll2, SearchDir);

            float d = Vector3.Dot(B.Point, SearchDir);
            if (!MathUtil.IsZero(d) && d < 0)
            {
                // We didn't reach the origin, won't enclose it
                return false;
            }

            // Search perpendicular to line segment towards origin
            SearchDir = Vector3.Cross(Vector3.Cross(C.Point - B.Point, -B.Point), C.Point - B.Point);
            if (Vector3.NearEqual(SearchDir, Vector3.Zero, ZeroTolerance))
            {
                // Origin is on this line segment, apparently any normal search vector will do?

                // Normal with x-axis
                SearchDir = Vector3.Cross(C.Point - B.Point, Vector3.Right);
                if (Vector3.NearEqual(SearchDir, Vector3.Zero, ZeroTolerance))
                {
                    // Normal with z-axis
                    SearchDir = Vector3.Cross(C.Point - B.Point, Vector3.BackwardLH);
                }
            }

            // Simplex dimension
            Dimension = 2;

            return true;
        }
        /// <summary>
        /// Update simplex point
        /// </summary>
        /// <param name="coll1">First collider</param>
        /// <param name="coll2">Second collider</param>
        /// <returns>Returns whether the current simplex reachs the origin or not</returns>
        public bool UpdatePoint(ICollider coll1, ICollider coll2)
        {
            A = new SupportPoint(coll1, coll2, SearchDir);

            float d = Vector3.Dot(A.Point, SearchDir);
            if (!MathUtil.IsZero(d) && d < 0)
            {
                return false;
            }

            Dimension++;

            return true;
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
        public void UpdateSimplex3()
        {
            // Triangle's normal
            Vector3 n = ABC;

            // Determine which feature is closest to origin, make that the new simplex
            Dimension = 2;
            if (Vector3.Dot(Vector3.Cross(B.Point - A.Point, n), AO) > 0)
            {
                // Closest to edge AB
                C = A;
                SearchDir = Vector3.Cross(Vector3.Cross(B.Point - A.Point, AO), B.Point - A.Point);
                return;
            }

            if (Vector3.Dot(Vector3.Cross(n, C.Point - A.Point), AO) > 0)
            {
                // Closest to edge AC
                B = A;
                SearchDir = Vector3.Cross(Vector3.Cross(C.Point - A.Point, AO), C.Point - A.Point);
                return;
            }

            Dimension = 3;
            if (Vector3.Dot(n, AO) > 0)
            {
                // Above triangle
                D = C;
                C = B;
                B = A;
                SearchDir = n;
                return;
            }

            // Below triangle
            D = B;
            B = A;
            SearchDir = -n;
        }
        /// <summary>
        /// Tetrahedral case
        /// </summary>
        /// <remarks>
        /// (a) is peak/tip of pyramid, BCD is the base (counterclockwise winding order)
        /// </remarks>
        public bool UpdateSimplex4()
        {
            // We know a priori that origin is above BCD and below (a)

            // Hoisting this just cause
            Dimension = 3;

            // Plane-test origin with 3 faces
            // Note: Kind of primitive approach used here; If origin is in front of a face, just use it as the new simplex.
            // We just go through the faces sequentially and exit at the first one which satisfies dot product.
            // Not sure this is optimal or if edges should be considered as possible simplices?
            // Thinking this through in my head I feel like this method is good enough.
            // Makes no difference for AABBS, should test with more complex colliders.

            //Get ABC face normal
            if (Vector3.Dot(ABC, AO) > 0)
            {
                //In front of ABC
                D = C;
                C = B;
                B = A;

                SearchDir = ABC;
                return false;
            }

            //Get ACD face normal
            if (Vector3.Dot(ACD, AO) > 0)
            {
                // In front of ACD
                B = A;
                SearchDir = ACD;
                return false;
            }

            //Get ADB face normal
            if (Vector3.Dot(ADB, AO) > 0)
            {
                // In front of ADB
                C = D;
                D = B;
                B = A;
                SearchDir = ADB;
                return false;
            }

            //Inside tetrahedron. Enclosed!
            return true;

            // Note: in the case where two of the faces have similar normals, the origin could conceivably be closest to an edge on the tetrahedron
            // Right now I don't think it'll make a difference to limit our new simplices to just one of the faces, maybe test it later.
        }
    }
}
