//Kevin's implementation of the Gilbert-Johnson-Keerthi intersection algorithm
//and the Expanding Polytope Algorithm
//Most useful references (Huge thanks to all the authors):

// "Implementing GJK" by Casey Muratori:
// The best description of the algorithm from the ground up
// https://www.youtube.com/watch?v=Qupqu1xe7Io

// "Implementing a GJK Intersection Query" by Phill Djonov
// Interesting tips for implementing the algorithm
// http://vec3.ca/gjk/implementation/

// "GJK Algorithm 3D" by Sergiu Craitoiu
// Has nice diagrams to visualise the tetrahedral case
// http://in2gpu.com/2014/05/18/gjk-algorithm-3d/

// "GJK + Expanding Polytope Algorithm - Implementation and Visualization"
// Good breakdown of EPA with demo for visualisation
// https://www.youtube.com/watch?v=6rgiPrzqt9w

// Ported to c# from https://github.com/kevinmoran/GJK
//-----------------------------------------------------------------------------

using SharpDX;
using System;

namespace Engine.Physics.GJK
{
    /// <summary>
    /// GJK-EPA solver class
    /// </summary>
    public static class Solver
    {
        private const int GJK_MAX_NUM_ITERATIONS = 64;

        /// <summary>
        /// Returns true if two colliders are intersecting. Has optional Minimum Translation Vector output param;
        /// If supplied the EPA will be used to find the vector to separate coll1 from coll2
        /// </summary>
        /// <param name="coll1"></param>
        /// <param name="coll2"></param>
        /// <param name="calcmtv"></param>
        /// <param name="mtv"></param>
        /// <returns></returns>
        public static bool GJK(ICollider coll1, ICollider coll2, bool calcmtv, out Vector3 mtv)
        {
            mtv = Vector3.Zero;

            //Simplex: just a set of points (a is always most recently added)
            Vector3 a, b, c, d = Vector3.Zero;

            //initial search direction between colliders
            Vector3 search_dir = coll1.Position - coll2.Position;

            //Get initial point for simplex
            c = coll2.Support(search_dir) - coll1.Support(-search_dir);
            //search in direction of origin
            search_dir = -c;

            //Get second point for a line segment simplex
            b = coll2.Support(search_dir) - coll1.Support(-search_dir);

            if (Vector3.Dot(b, search_dir) < 0)
            {
                //we didn't reach the origin, won't enclose it
                return false;
            }

            //search perpendicular to line segment towards origin
            search_dir = Vector3.Cross(Vector3.Cross(c - b, -b), c - b);
            if (search_dir == Vector3.Zero)
            {
                //origin is on this line segment
                //Apparently any normal search vector will do?

                //normal with x-axis
                search_dir = Vector3.Cross(c - b, Vector3.Right);
                if (search_dir == Vector3.Zero)
                {
                    //normal with z-axis
                    search_dir = Vector3.Cross(c - b, Vector3.BackwardLH);
                }
            }

            //simplex dimension
            int simp_dim = 2;
            for (int iterations = 0; iterations < GJK_MAX_NUM_ITERATIONS; iterations++)
            {
                a = coll2.Support(search_dir) - coll1.Support(-search_dir);
                float dd = Vector3.Dot(a, search_dir);
                if (!MathUtil.IsZero(dd) && dd < 0)
                {
                    //we didn't reach the origin, won't enclose it
                    return false;
                }

                simp_dim++;
                if (simp_dim == 3)
                {
                    UpdateSimplex3(ref a, ref b, ref c, ref d, ref simp_dim, ref search_dir);
                }
                else if (UpdateSimplex4(ref a, ref b, ref c, ref d, ref simp_dim, ref search_dir))
                {
                    if (calcmtv)
                    {
                        mtv = EPA(a, b, c, d, coll1, coll2);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Triangle case
        /// </summary>
        private static void UpdateSimplex3(ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector3 d, ref int simp_dim, ref Vector3 search_dir)
        {
            /* Required winding order:
            //  b
            //  | \
            //  |   \
            //  |    a
            //  |   /
            //  | /
            //  c
            */

            //triangle's normal
            Vector3 n = Vector3.Cross(b - a, c - a);
            //direction to origin
            Vector3 AO = -a;

            //Determine which feature is closest to origin, make that the new simplex
            simp_dim = 2;
            if (Vector3.Dot(Vector3.Cross(b - a, n), AO) > 0)
            {
                //Closest to edge AB
                c = a;
                search_dir = Vector3.Cross(Vector3.Cross(b - a, AO), b - a);
                return;
            }

            if (Vector3.Dot(Vector3.Cross(n, c - a), AO) > 0)
            {
                //Closest to edge AC
                b = a;
                search_dir = Vector3.Cross(Vector3.Cross(c - a, AO), c - a);
                return;
            }

            simp_dim = 3;
            if (Vector3.Dot(n, AO) > 0)
            {
                //Above triangle
                d = c;
                c = b;
                b = a;
                search_dir = n;
                return;
            }

            //else //Below triangle
            d = b;
            b = a;
            search_dir = -n;
            return;
        }

        /// <summary>
        /// Tetrahedral case
        /// </summary>
        private static bool UpdateSimplex4(ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector3 d, ref int simp_dim, ref Vector3 search_dir)
        {
            // a is peak/tip of pyramid, BCD is the base (counterclockwise winding order)
            //We know a priori that origin is above BCD and below a

            //Get normals of three new faces
            Vector3 ABC = Vector3.Cross(b - a, c - a);
            Vector3 ACD = Vector3.Cross(c - a, d - a);
            Vector3 ADB = Vector3.Cross(d - a, b - a);

            Vector3 AO = -a; //dir to origin
            simp_dim = 3; //hoisting this just cause

            //Plane-test origin with 3 faces
            /*
            // Note: Kind of primitive approach used here; If origin is in front of a face, just use it as the new simplex.
            // We just go through the faces sequentially and exit at the first one which satisfies dot product. Not sure this 
            // is optimal or if edges should be considered as possible simplices? Thinking this through in my head I feel like 
            // this method is good enough. Makes no difference for AABBS, should test with more complex colliders.
            */
            if (Vector3.Dot(ABC, AO) > 0)
            {
                //In front of ABC
                d = c;
                c = b;
                b = a;
                search_dir = ABC;
                return false;
            }

            if (Vector3.Dot(ACD, AO) > 0)
            {
                //In front of ACD
                b = a;
                search_dir = ACD;
                return false;
            }

            if (Vector3.Dot(ADB, AO) > 0)
            {
                //In front of ADB
                c = d;
                d = b;
                b = a;
                search_dir = ADB;
                return false;
            }

            //else inside tetrahedron; enclosed!
            return true;

            //Note: in the case where two of the faces have similar normals,
            //The origin could conceivably be closest to an edge on the tetrahedron
            //Right now I don't think it'll make a difference to limit our new simplices
            //to just one of the faces, maybe test it later.
        }

        public const float EPA_TOLERANCE = 0.0001f;
        private const int EPA_MAX_NUM_FACES = 64;
        private const int EPA_MAX_NUM_LOOSE_EDGES = 32;
        private const int EPA_MAX_NUM_ITERATIONS = 64;

        /// <summary>
        /// Expanding Polytope Algorithm. Used to find the mtv of two intersecting colliders using the final simplex obtained with the GJK algorithm
        /// </summary>
        private static Vector3 EPA(Vector3 a, Vector3 b, Vector3 c, Vector3 d, ICollider coll1, ICollider coll2)
        {
            //Array of faces, each with 3 verts and a normal
            Vector3[,] faces = new Vector3[EPA_MAX_NUM_FACES, 4];

            //Init with final simplex from GJK
            faces[0, 0] = a;
            faces[0, 1] = b;
            faces[0, 2] = c;
            faces[0, 3] = Vector3.Normalize(Vector3.Cross(b - a, c - a)); //ABC
            faces[1, 0] = a;
            faces[1, 1] = c;
            faces[1, 2] = d;
            faces[1, 3] = Vector3.Normalize(Vector3.Cross(c - a, d - a)); //ACD
            faces[2, 0] = a;
            faces[2, 1] = d;
            faces[2, 2] = b;
            faces[2, 3] = Vector3.Normalize(Vector3.Cross(d - a, b - a)); //ADB
            faces[3, 0] = b;
            faces[3, 1] = d;
            faces[3, 2] = c;
            faces[3, 3] = Vector3.Normalize(Vector3.Cross(d - b, c - b)); //BDC

            int num_faces = 4;
            int closest_face = 0;

            for (int iterations = 0; iterations < EPA_MAX_NUM_ITERATIONS; iterations++)
            {
                //Find face that's closest to origin
                float min_dist = Vector3.Dot(faces[0, 0], faces[0, 3]);
                closest_face = 0;
                for (int i = 1; i < num_faces; i++)
                {
                    float dist = Vector3.Dot(faces[i, 0], faces[i, 3]);
                    if (dist < min_dist)
                    {
                        min_dist = dist;
                        closest_face = i;
                    }
                }

                //search normal to face that's closest to origin
                Vector3 search_dir = faces[closest_face, 3];
                Vector3 p = coll2.Support(search_dir) - coll1.Support(-search_dir);

                float sdist = Vector3.Dot(p, search_dir);
                if (sdist - min_dist < EPA_TOLERANCE)
                {
                    //Convergence (new point is not significantly further from origin)
                    return faces[closest_face, 3] * sdist; //dot vertex with normal to resolve collision along normal!
                }

                Vector3[,] loose_edges = new Vector3[EPA_MAX_NUM_LOOSE_EDGES, 2]; //keep track of edges we need to fix after removing faces
                int num_loose_edges = 0;

                //Find all triangles that are facing p
                for (int i = 0; i < num_faces; i++)
                {
                    if (Vector3.Dot(faces[i, 3], p - faces[i, 0]) <= 0) //triangle i faces p, remove it
                    {
                        continue;
                    }

                    //Add removed triangle's edges to loose edge list.
                    //If it's already there, remove it (both triangles it belonged to are gone)
                    for (int j = 0; j < 3; j++) //Three edges per face
                    {
                        Vector3[] current_edge = new Vector3[] { faces[i, j], faces[i, (j + 1) % 3] };
                        bool found_edge = false;
                        for (int k = 0; k < num_loose_edges; k++) //Check if current edge is already in list
                        {
                            if (loose_edges[k, 1] == current_edge[0] && loose_edges[k, 0] == current_edge[1])
                            {
                                //Edge is already in the list, remove it
                                //THIS ASSUMES EDGE CAN ONLY BE SHARED BY 2 TRIANGLES (which should be true)
                                //THIS ALSO ASSUMES SHARED EDGE WILL BE REVERSED IN THE TRIANGLES (which 
                                //should be true provided every triangle is wound CCW)
                                loose_edges[k, 0] = loose_edges[num_loose_edges - 1, 0]; //Overwrite current edge
                                loose_edges[k, 1] = loose_edges[num_loose_edges - 1, 1]; //with last edge in list
                                num_loose_edges--;
                                found_edge = true;
                                k = num_loose_edges; //exit loop because edge can only be shared once
                            }
                        }

                        if (!found_edge)
                        {
                            //add current edge to list
                            if (num_loose_edges >= EPA_MAX_NUM_LOOSE_EDGES) break;
                            loose_edges[num_loose_edges, 0] = current_edge[0];
                            loose_edges[num_loose_edges, 1] = current_edge[1];
                            num_loose_edges++;
                        }
                    }

                    //Remove triangle i from list
                    faces[i, 0] = faces[num_faces - 1, 0];
                    faces[i, 1] = faces[num_faces - 1, 1];
                    faces[i, 2] = faces[num_faces - 1, 2];
                    faces[i, 3] = faces[num_faces - 1, 3];
                    num_faces--;
                    i--;
                }

                //Reconstruct polytope with p added
                for (int i = 0; i < num_loose_edges; i++)
                {
                    if (num_faces >= EPA_MAX_NUM_FACES)
                    {
                        break;
                    }

                    faces[num_faces, 0] = loose_edges[i, 0];
                    faces[num_faces, 1] = loose_edges[i, 1];
                    faces[num_faces, 2] = p;
                    faces[num_faces, 3] = Vector3.Normalize(Vector3.Cross(loose_edges[i, 0] - loose_edges[i, 1], loose_edges[i, 0] - p));

                    //Check for wrong normal to maintain CCW winding in case dot result is only slightly < 0 (because origin is on face)
                    float bias = 0.000001f;
                    float dd = Vector3.Dot(faces[num_faces, 0], faces[num_faces, 3]) + bias;
                    if (dd < 0)
                    {
                        (faces[num_faces, 1], faces[num_faces, 0]) = (faces[num_faces, 0], faces[num_faces, 1]);
                        faces[num_faces, 3] = -faces[num_faces, 3];
                    }
                    num_faces++;
                }
            }

            Console.WriteLine("EPA did not converge");

            //Return most recent closest point
            return faces[closest_face, 3] * Vector3.Dot(faces[closest_face, 0], faces[closest_face, 3]);
        }
    }
}
