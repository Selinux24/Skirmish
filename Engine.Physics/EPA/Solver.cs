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
using System;

namespace Engine.Physics.EPA
{
    /// <summary>
    /// EPA solver class
    /// </summary>
    public static class Solver
    {
        public const float EPA_TOLERANCE = 0.0001f;
        public const float EPA_BIAS = 0.000001f;
        private const int EPA_MAX_NUM_FACES = 64;
        private const int EPA_MAX_NUM_LOOSE_EDGES = 32;
        private const int EPA_MAX_NUM_ITERATIONS = 64;

        /// <summary>
        /// Expanding Polytope Algorithm. Used to find the minimum translation vector of two intersecting colliders using the final simplex obtained with the GJK algorithm
        /// </summary>
        public static (Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal, float dist) EPA(Vector3 a, Vector3 b, Vector3 c, Vector3 d, ICollider coll1, ICollider coll2)
        {
            // Array of faces, each with 3 vertices and a normal
            Vector3[,] faces = new Vector3[EPA_MAX_NUM_FACES, 4];

            // Initialize with final simplex from GJK
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
                // Find face that's closest to origin
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

                // Search normal to face that's closest to origin
                Vector3 search_dir = faces[closest_face, 3];
                Vector3 p = coll2.Support(search_dir) - coll1.Support(-search_dir);

                float sdist = Vector3.Dot(p, search_dir);
                if (sdist - min_dist < EPA_TOLERANCE)
                {
                    // Convergence (new point is not significantly further from origin)
                    return (faces[closest_face, 0], faces[closest_face, 1], faces[closest_face, 2], faces[closest_face, 3], sdist);
                }

                Vector3[,] loose_edges = new Vector3[EPA_MAX_NUM_LOOSE_EDGES, 2];
                // Keep track of edges we need to fix after removing faces
                int num_loose_edges = 0;

                // Find all triangles that are facing p
                for (int i = 0; i < num_faces; i++)
                {
                    if (Vector3.Dot(faces[i, 3], p - faces[i, 0]) <= 0)
                    {
                        continue;
                    }

                    // Triangle i faces p, remove it
                    // Add removed triangle's edges to loose edge list.
                    // If it's already there, remove it (both triangles it belonged to are gone)

                    // Three edges per face
                    for (int j = 0; j < 3; j++)
                    {
                        Vector3[] current_edge = new Vector3[] { faces[i, j], faces[i, (j + 1) % 3] };
                        bool found_edge = false;
                        for (int k = 0; k < num_loose_edges; k++) //Check if current edge is already in list
                        {
                            if (loose_edges[k, 1] == current_edge[0] && loose_edges[k, 0] == current_edge[1])
                            {
                                // Edge is already in the list, remove it
                                // THIS ASSUMES EDGE CAN ONLY BE SHARED BY 2 TRIANGLES (which should be true)
                                // THIS ALSO ASSUMES SHARED EDGE WILL BE REVERSED IN THE TRIANGLES (which 
                                // Should be true provided every triangle is wound CCW)
                                loose_edges[k, 0] = loose_edges[num_loose_edges - 1, 0]; //Overwrite current edge
                                loose_edges[k, 1] = loose_edges[num_loose_edges - 1, 1]; //with last edge in list
                                num_loose_edges--;
                                found_edge = true;

                                //Exit loop because edge can only be shared once
                                break;
                            }
                        }

                        if (!found_edge)
                        {
                            // Add current edge to list
                            if (num_loose_edges >= EPA_MAX_NUM_LOOSE_EDGES) break;
                            loose_edges[num_loose_edges, 0] = current_edge[0];
                            loose_edges[num_loose_edges, 1] = current_edge[1];
                            num_loose_edges++;
                        }
                    }

                    // Remove triangle i from list
                    faces[i, 0] = faces[num_faces - 1, 0];
                    faces[i, 1] = faces[num_faces - 1, 1];
                    faces[i, 2] = faces[num_faces - 1, 2];
                    faces[i, 3] = faces[num_faces - 1, 3];
                    num_faces--;
                    i--;
                }

                // Reconstruct polytope with p added
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

                    // Check for wrong normal to maintain CCW winding in case dot result is only slightly < 0 (because origin is on face)
                    float dd = Vector3.Dot(faces[num_faces, 0], faces[num_faces, 3]) + EPA_BIAS;
                    if (dd < 0)
                    {
                        (faces[num_faces, 1], faces[num_faces, 0]) = (faces[num_faces, 0], faces[num_faces, 1]);
                        faces[num_faces, 3] = -faces[num_faces, 3];
                    }

                    num_faces++;
                }
            }

            Console.WriteLine("EPA did not converge");

            // Return most recent closest point
            return (faces[closest_face, 0], faces[closest_face, 1], faces[closest_face, 2], faces[closest_face, 3], Vector3.Dot(faces[closest_face, 0], faces[closest_face, 3]));
        }
    }
}
