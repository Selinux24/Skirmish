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
    using Engine.Physics.GJK;

    /// <summary>
    /// EPA solver class
    /// </summary>
    public static class Solver
    {
        public const float EPA_TOLERANCE = 0.0001f;
        private const float EPA_BIAS = 0.000001f;
        private const int EPA_MAX_NUM_FACES = 64;
        private const int EPA_MAX_NUM_LOOSE_EDGES = 32;
        private const int EPA_MAX_NUM_ITERATIONS = 64;

        /// <summary>
        /// Expanding Polytope Algorithm. Used to find the minimum translation vector of two intersecting colliders using the final simplex obtained with the GJK algorithm
        /// </summary>
        public static (Face face, float dist) EPA(Simplex simplex, ICollider coll1, ICollider coll2)
        {
            // Array of faces, each with 3 vertices and a normal
            // Initialize with final simplex from GJK
            var faces = Initialize(simplex, out int num_faces);
            int closest_face = 0;

            var loose_edges = new Edge[EPA_MAX_NUM_LOOSE_EDGES];

            for (int iterations = 0; iterations < EPA_MAX_NUM_ITERATIONS; iterations++)
            {
                // Find face that's closest to origin
                GetClosestFaceToOrigin(faces, num_faces, out float min_dist, out closest_face);

                // Search normal to face that's closest to origin
                var search_dir = faces[closest_face].Normal;
                var p = coll2.Support(search_dir) - coll1.Support(-search_dir);

                float sdist = Vector3.Dot(p, search_dir);
                if (sdist - min_dist < EPA_TOLERANCE)
                {
                    // Convergence (new point is not significantly further from origin)
                    return (faces[closest_face], sdist);
                }

                // Keep track of edges we need to fix after removing faces
                int num_loose_edges = 0;

                // Find all triangles that are facing p
                for (int i = 0; i < num_faces; i++)
                {
                    if (Vector3.Dot(faces[i].Normal, p - faces[i].A) <= 0)
                    {
                        continue;
                    }

                    // Triangle i faces p, remove it
                    // Add removed triangle's edges to loose edge list.
                    // If it's already there, remove it (both triangles it belonged to are gone)

                    // Three edges per face
                    for (int j = 0; j < 3; j++)
                    {
                        var current_edge = faces[i].GetEdge(j);
                        bool found_edge = false;

                        //Check if current edge is already in list
                        for (int k = 0; k < num_loose_edges; k++)
                        {
                            if (loose_edges[k].B == current_edge.A && loose_edges[k].A == current_edge.B)
                            {
                                // Edge is already in the list, remove it
                                // THIS ASSUMES EDGE CAN ONLY BE SHARED BY 2 TRIANGLES (which should be true)
                                // THIS ALSO ASSUMES SHARED EDGE WILL BE REVERSED IN THE TRIANGLES (which 
                                // Should be true provided every triangle is wound CCW)
                                loose_edges[k] = loose_edges[num_loose_edges - 1]; //Overwrite current edge with last edge in list
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
                            loose_edges[num_loose_edges] = current_edge;
                            num_loose_edges++;
                        }
                    }

                    // Remove triangle i from list
                    faces[i] = faces[num_faces - 1];
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

                    faces[num_faces] = new Face(loose_edges[i].A, loose_edges[i].B, p);

                    // Check for wrong normal to maintain CCW winding in case dot result is only slightly < 0 (because origin is on face)
                    if (Vector3.Dot(faces[num_faces].A, faces[num_faces].Normal) + EPA_BIAS < 0)
                    {
                        faces[num_faces].Reverse();
                    }

                    num_faces++;
                }
            }

#if DEBUG
            Console.WriteLine("EPA did not converge");
#endif

            // Return most recent closest point
            return (faces[closest_face], Vector3.Dot(faces[closest_face].A, faces[closest_face].Normal));
        }
        /// <summary>
        /// Initialize the face list
        /// </summary>
        /// <param name="simplex">GJK simplex</param>
        /// <param name="num_faces">Number of initial faces</param>
        private static Face[] Initialize(Simplex simplex, out int num_faces)
        {
            // Array of faces, each with 3 vertices and a normal
            Face[] faces = new Face[EPA_MAX_NUM_FACES];

            // Initialize with final simplex from GJK
            faces[0] = new Face(simplex.A, simplex.B, simplex.C); //ABC
            faces[1] = new Face(simplex.A, simplex.C, simplex.D); //ACD
            faces[2] = new Face(simplex.A, simplex.D, simplex.B); //ADB
            faces[3] = new Face(simplex.B, simplex.D, simplex.C); //BDC

            num_faces = 4;

            return faces;
        }
        /// <summary>
        /// Find face that's closest to origin
        /// </summary>
        /// <param name="faces">Face list</param>
        /// <param name="num_faces">Number of faces</param>
        /// <param name="min_dist">Minimal distance to origin</param>
        /// <param name="closest_face">Closest face index</param>
        private static void GetClosestFaceToOrigin(Face[] faces, int num_faces, out float min_dist, out int closest_face)
        {
            min_dist = Vector3.Dot(faces[0].A, faces[0].Normal);
            closest_face = 0;

            for (int i = 1; i < num_faces; i++)
            {
                float dist = Vector3.Dot(faces[i].A, faces[i].Normal);
                if (dist < min_dist)
                {
                    min_dist = dist;
                    closest_face = i;
                }
            }
        }
    }
}
