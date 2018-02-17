using System;
using System.Linq;

namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Chunky triangle mesh
    /// </summary>
    public class ChunkyTriMesh
    {
        /// <summary>
        /// Triangle list
        /// </summary>
        public Triangle[] triangles;
        /// <summary>
        /// Maximum number of triangles per chunk
        /// </summary>
        public int maxTrisPerChunk;
        /// <summary>
        /// Chunk nodes
        /// </summary>
        public ChunkyTriMeshNode[] nodes;
        /// <summary>
        /// Node count
        /// </summary>
        public int nnodes;
        /// <summary>
        /// Chunk triangle indices
        /// </summary>
        public int[] tris;
        /// <summary>
        /// Triangle index count
        /// </summary>
        public int ntris;

        /// <summary>
        /// Gets the triangles in the specified node
        /// </summary>
        /// <param name="index">Node index</param>
        /// <returns>Returns the triangles in the specified node</returns>
        public Triangle[] GetTriangles(int index)
        {
            return GetTriangles(this.nodes[index]);
        }
        /// <summary>
        /// Gets the triangles in the specified node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns the triangles in the specified node</returns>
        public Triangle[] GetTriangles(ChunkyTriMeshNode node)
        {
            if (node.i >= 0)
            {
                int[] indices = new int[node.n];
                Array.Copy(tris.ToArray(), node.i, indices, 0, node.n);

                Triangle[] res = new Triangle[node.n];
                for (int i = 0; i < node.n; i++)
                {
                    res[i] = triangles[indices[i]];
                }
                return res;
            }

            return null;
        }
    }
}
