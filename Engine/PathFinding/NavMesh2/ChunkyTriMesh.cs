using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.NavMesh2
{
    public class ChunkyTriMesh
    {
        public int maxTrisPerChunk;
        public ChunkyTriMeshNode[] nodes;
        public int nnodes;
        public int[] tris;
        public int ntris;

        internal Triangle[] GetTriangles(IEnumerable<Triangle> tris, int index)
        {
            ChunkyTriMeshNode node = nodes[index];
            Triangle[] res = new Triangle[node.n];
            Array.Copy(tris.ToArray(), node.i, res, 0, node.n);

            return res;
        }
    }
}
