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
    using GJKSimplex = GJK.Simplex;
    using GJKSupportPoint = GJK.SupportPoint;

    /// <summary>
    /// EPA solver class
    /// </summary>
    public static class Solver
    {
        public const float EPA_TOLERANCE = 0.0005f;
        private const float EPA_BIAS = 0.000001f;
        private const int EPA_MAX_NUM_FACES = 64;
        private const int EPA_MAX_NUM_LOOSE_EDGES = 32;
        private const int EPA_MAX_NUM_ITERATIONS = 64;

        /// <summary>
        /// Expanding Polytope Algorithm. Used to find the minimum translation vector of two intersecting colliders using the final simplex obtained with the GJK algorithm
        /// </summary>
        public static (Face face, float dist) EPA(GJKSimplex simplex, ICollider coll1, ICollider coll2)
        {
            // Array of faces, each with 3 vertices and a normal
            // Initialize with final simplex from GJK
            var faces = Initialize(simplex, out int num_faces);
            int closest_face = 0;

            for (int iterations = 0; iterations < EPA_MAX_NUM_ITERATIONS; iterations++)
            {
                // Find face that's closest to origin
                GetClosestFaceToOrigin(faces, num_faces, out float min_dist, out closest_face);

                // Search normal to face that's closest to origin
                var search_dir = faces[closest_face].Normal;
                GJKSupportPoint p = new(coll1, coll2, search_dir);

                float sdist = Vector3.Dot(p.Point, search_dir);
                if (sdist - min_dist < EPA_TOLERANCE)
                {
                    // Convergence (new point is not significantly further from origin)
                    return (faces[closest_face], sdist);
                }

                // Find all triangles that are facing p
                var (loose_edges, num_loose_edges) = FindTriangles(faces, ref num_faces, p);

                // Reconstruct polytope with p added
                ReconstructPolytope(faces, ref num_faces, loose_edges, num_loose_edges, p);
            }

#if DEBUG
            Console.WriteLine("EPA did not converge");
#endif

            // Return most recent closest point
            return (faces[closest_face], Vector3.Dot(faces[closest_face].A.Point, faces[closest_face].Normal));
        }
        /// <summary>
        /// Initialize the face list
        /// </summary>
        /// <param name="simplex">GJK simplex</param>
        /// <param name="num_faces">Number of initial faces</param>
        private static Face[] Initialize(GJKSimplex simplex, out int num_faces)
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
            min_dist = Vector3.Dot(faces[0].A.Point, faces[0].Normal);
            closest_face = 0;

            for (int i = 1; i < num_faces; i++)
            {
                float dist = Vector3.Dot(faces[i].A.Point, faces[i].Normal);
                if (dist < min_dist)
                {
                    min_dist = dist;
                    closest_face = i;
                }
            }
        }
        /// <summary>
        /// Finds all triangles that are facing p
        /// </summary>
        /// <param name="faces">Face list</param>
        /// <param name="num_faces">Number of faces in the list</param>
        /// <param name="p">Support point</param>
        private static (Edge[] edges, int num_edges) FindTriangles(Face[] faces, ref int num_faces, GJKSupportPoint p)
        {
            Edge[] loose_edges = new Edge[EPA_MAX_NUM_LOOSE_EDGES];

            // Keep track of edges we need to fix after removing faces
            int num_loose_edges = 0;

            int i = 0;
            while (i < num_faces)
            {
                if (Vector3.Dot(faces[i].Normal, p.Point - faces[i].A.Point) <= 0)
                {
                    i++;
                    continue;
                }

                // Triangle i faces p, remove it
                // Add removed triangle's edges to loose edge list.
                // If it's already there, remove it (both triangles it belonged to are gone)

                // Three edges per face
                for (int j = 0; j < 3; j++)
                {
                    var current_edge = faces[i].GetEdge(j);

                    //Check if current edge is already in list
                    bool found_edge = FindEdge(current_edge, loose_edges, ref num_loose_edges);
                    if (found_edge)
                    {
                        continue;
                    }

                    if (num_loose_edges >= EPA_MAX_NUM_LOOSE_EDGES)
                    {
                        break;
                    }

                    // Add current edge to list
                    loose_edges[num_loose_edges++] = current_edge;
                }

                // Remove triangle i from list
                faces[i] = faces[num_faces - 1];
                num_faces--;
            }

            return (loose_edges, num_loose_edges);
        }
        /// <summary>
        /// Checks if the specified edge is already in list
        /// </summary>
        /// <param name="current_edge">Edge to test</param>
        /// <param name="loose_edges">Loose edges</param>
        /// <param name="num_loose_edges">Number of loose edges in the list</param>
        private static bool FindEdge(Edge current_edge, Edge[] loose_edges, ref int num_loose_edges)
        {
            bool found_edge = false;

            for (int k = 0; k < num_loose_edges; k++)
            {
                if (loose_edges[k].B.Point == current_edge.A.Point && loose_edges[k].A.Point == current_edge.B.Point)
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

            return found_edge;
        }
        /// <summary>
        /// Reconstructs polytope with p added
        /// </summary>
        /// <param name="faces">Face list</param>
        /// <param name="num_faces">Number of faces in the list</param>
        /// <param name="loose_edges">Loose edges</param>
        /// <param name="num_loose_edges">Number of loose edges in the list</param>
        /// <param name="p">Support point</param>
        private static void ReconstructPolytope(Face[] faces, ref int num_faces, Edge[] loose_edges, int num_loose_edges, GJKSupportPoint p)
        {
            for (int i = 0; i < num_loose_edges; i++)
            {
                if (num_faces >= EPA_MAX_NUM_FACES)
                {
                    break;
                }

                faces[num_faces] = new(loose_edges[i].A, loose_edges[i].B, p);

                // Check for wrong normal to maintain CCW winding in case dot result is only slightly < 0 (because origin is on face)
                if (Vector3.Dot(faces[num_faces].A.Point, faces[num_faces].Normal) + EPA_BIAS < 0)
                {
                    faces[num_faces].Reverse();
                }

                num_faces++;
            }
        }
    }
}
