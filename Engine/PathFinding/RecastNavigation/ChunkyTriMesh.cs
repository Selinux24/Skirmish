using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Chunky triangle mesh
    /// </summary>
    public class ChunkyTriMesh
    {
        private static bool CheckOverlapRect(Vector2 amin, Vector2 amax, Vector2 bmin, Vector2 bmax)
        {
            bool overlap = true;
            overlap = (amin.X > bmax.X || amax.X < bmin.X) ? false : overlap;
            overlap = (amin.Y > bmax.Y || amax.Y < bmin.Y) ? false : overlap;
            return overlap;
        }

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

        /// <summary>
        /// Gets the chunks overlapping the specified rectangle
        /// </summary>
        /// <param name="bmin">Bounding box minimum</param>
        /// <param name="bmax">Bounding box maximum</param>
        /// <returns>Returns a list of indices</returns>
        public IEnumerable<int> GetChunksOverlappingRect(Vector2 bmin, Vector2 bmax)
        {
            List<int> ids = new List<int>();

            // Traverse tree
            int i = 0;
            while (i < nnodes)
            {
                var node = nodes[i];
                bool overlap = CheckOverlapRect(bmin, bmax, node.bmin, node.bmax);
                bool isLeafNode = node.i >= 0;

                if (isLeafNode && overlap)
                {
                    ids.Add(i);
                }

                if (overlap || isLeafNode)
                {
                    i++;
                }
                else
                {
                    int escapeIndex = -node.i;
                    i += escapeIndex;
                }
            }

            return ids;
        }
    }
}
