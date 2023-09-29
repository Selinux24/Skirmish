using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    using Engine.PathFinding.RecastNavigation.Detour;

    /// <summary>
    /// Polygon mesh triangle indexes
    /// </summary>
    [Serializable]
    public struct PolyMeshTriangleIndices
    {
        /// <summary>
        /// Point 1 index
        /// </summary>
        public int Point1 { get; set; }
        /// <summary>
        /// Point 2 index
        /// </summary>
        public int Point2 { get; set; }
        /// <summary>
        /// Point 3 index
        /// </summary>
        public int Point3 { get; set; }
        /// <summary>
        /// By edge flags
        /// </summary>
        public int Flags { get; set; }
        /// <summary>
        /// Gets the triangle point index by index 
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the triangle point index value</returns>
        public readonly int this[int index]
        {
            get
            {
                return index switch
                {
                    0 => Point1,
                    1 => Point2,
                    2 => Point3,
                    _ => throw new ArgumentOutOfRangeException(nameof(index), "Bad triangle index"),
                };
            }
        }

        /// <summary>
        /// Builds a triangle list
        /// </summary>
        /// <param name="tris">Triangle indices</param>
        /// <param name="verts">Vertex list</param>
        /// <param name="poly">Polygon vertices</param>
        public static PolyMeshTriangleIndices[] BuildTriangleList(Int3[] tris, Vector3[] verts, Vector3[] poly)
        {
            var res = new List<PolyMeshTriangleIndices>();

            foreach (var t in tris)
            {
                res.Add(new PolyMeshTriangleIndices
                {
                    Point1 = t.X,
                    Point2 = t.Y,
                    Point3 = t.Z,
                    Flags = GetTriFlags(verts[t.X], verts[t.Y], verts[t.Z], poly),
                });
            }

            return res.ToArray();
        }
        private static int GetTriFlags(Vector3 va, Vector3 vb, Vector3 vc, Vector3[] vpoly)
        {
            int flags = 0;
            flags |= GetEdgeFlags(va, vb, vpoly) << 0;
            flags |= GetEdgeFlags(vb, vc, vpoly) << 2;
            flags |= GetEdgeFlags(vc, va, vpoly) << 4;
            return flags;
        }
        private static int GetEdgeFlags(Vector3 va, Vector3 vb, Vector3[] vpoly)
        {
            const float thrSqr = 0.001f * 0.001f;

            int npoly = vpoly.Length;

            // Return true if edge (va,vb) is part of the polygon.
            for (int i = 0, j = npoly - 1; i < npoly; j = i++)
            {
                var vi = vpoly[i];
                var vj = vpoly[j];
                if (Utils.DistancePtSegSqr2D(va, vj, vi) < thrSqr &&
                    Utils.DistancePtSegSqr2D(vb, vj, vi) < thrSqr)
                {
                    return 1;
                }
            }

            return 0;
        }

        /// <summary>
        /// Get flags for edge in detail triangle.
        /// </summary>
        /// <param name="edgeIndex">The index of the first vertex of the edge. For instance, if 0, returns flags for edge AB.</param>
        /// <returns></returns>
        public readonly DetailTriEdgeFlagTypes GetDetailTriEdgeFlags(int edgeIndex)
        {
            return (DetailTriEdgeFlagTypes)((Flags >> (edgeIndex * 2)) & 0x3);
        }
    }
}
