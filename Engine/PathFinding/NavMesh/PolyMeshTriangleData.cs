using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh
{
    using Engine.Common;

    /// <summary>
    /// The triangle info contains three vertex hashes and a flag
    /// </summary>
    struct PolyMeshTriangleData
    {
        /// <summary>
        /// Determine whether an edge of the triangle is part of the polygon (1 if true, 0 if false)
        /// </summary>
        /// <param name="va">Triangle vertex A</param>
        /// <param name="vb">Triangle vertex B</param>
        /// <param name="vpoly">Polygon vertex data</param>
        /// <returns>1 if the vertices are close, 0 if otherwise</returns>
        private static int GetEdgeFlags(Vector3 va, Vector3 vb, Vector3[] vpoly, int npoly)
        {
            //true if edge is part of polygon
            float thrSqr = 0.001f * 0.001f;

            for (int i = 0, j = npoly - 1; i < npoly; j = i++)
            {
                Vector3 pt1 = va;
                Vector3 pt2 = vb;

                //the vertices pt1 (va) and pt2 (vb) are extremely close to the polygon edge
                if (Intersection.PointToSegment2DSquared(pt1, vpoly[j], vpoly[i]) < thrSqr &&
                    Intersection.PointToSegment2DSquared(pt2, vpoly[j], vpoly[i]) < thrSqr)
                {
                    return 1;
                }
            }

            return 0;
        }

        /// <summary>
        /// Vertex hash 0
        /// </summary>
        public int VertexHash0;
        /// <summary>
        /// Vertex hash 1
        /// </summary>
        public int VertexHash1;
        /// <summary>
        /// Vertex hash 2
        /// </summary>
        public int VertexHash2;
        /// <summary>
        /// Indicates which 3 vertices are part of the polygon
        /// </summary>
        public int Flags;

        /// <summary>
        /// Gets a triangle's particular vertex
        /// </summary>
        /// <param name="index">Vertex index</param>
        /// <returns>Triangle vertex hash</returns>
        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return VertexHash0;
                    case 1:
                        return VertexHash1;
                    case 2:
                    default:
                        return VertexHash2;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriangleData" /> struct.
        /// </summary>
        /// <param name="hash0">Vertex A</param>
        /// <param name="hash1">Vertex B</param>
        /// <param name="hash2">Vertex C</param>
        public PolyMeshTriangleData(int hash0, int hash1, int hash2)
        {
            VertexHash0 = hash0;
            VertexHash1 = hash1;
            VertexHash2 = hash2;
            Flags = 0;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TriangleData" /> struct.
        /// </summary>
        /// <param name="hash0">Vertex A</param>
        /// <param name="hash1">Vertex B</param>
        /// <param name="hash2">Vertex C</param>
        /// <param name="flags">The triangle flags</param>
        public PolyMeshTriangleData(int hash0, int hash1, int hash2, int flags)
        {
            VertexHash0 = hash0;
            VertexHash1 = hash1;
            VertexHash2 = hash2;
            Flags = flags;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TriangleData" /> struct.
        /// </summary>
        /// <param name="data">The triangle itself</param>
        /// <param name="verts">The list of all the vertices</param>
        /// <param name="vpoly">The list of the polygon's vertices</param>
        public PolyMeshTriangleData(PolyMeshTriangleData data, List<Vector3> verts, Vector3[] vpoly, int npoly)
        {
            VertexHash0 = data.VertexHash0;
            VertexHash1 = data.VertexHash1;
            VertexHash2 = data.VertexHash2;
            Flags = GetTriFlags(ref data, verts, vpoly, npoly);
        }

        /// <summary>
        /// Determine which edges of the triangle are part of the polygon
        /// </summary>
        /// <param name="t">A triangle.</param>
        /// <param name="verts">The vertex buffer that the triangle is referencing.</param>
        /// <param name="vpoly">Polygon vertex data.</param>
        /// <returns>The triangle's flags.</returns>
        public static int GetTriFlags(ref PolyMeshTriangleData t, List<Vector3> verts, Vector3[] vpoly, int npoly)
        {
            int flags = 0;

            //the triangle flags store five bits ?0?0? (like 10001, 10101, etc..)
            //each bit stores whether two vertices are close enough to a polygon edge 
            //since triangle has three vertices, there are three distinct pairs of vertices (va,vb), (vb,vc) and (vc,va)
            flags |= GetEdgeFlags(verts[t.VertexHash0], verts[t.VertexHash1], vpoly, npoly) << 0;
            flags |= GetEdgeFlags(verts[t.VertexHash1], verts[t.VertexHash2], vpoly, npoly) << 2;
            flags |= GetEdgeFlags(verts[t.VertexHash2], verts[t.VertexHash0], vpoly, npoly) << 4;

            return flags;
        }
    }

}
