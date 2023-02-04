using System;

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
        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Point1;
                    case 1:
                        return Point2;
                    case 2:
                        return Point3;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index), "Bad triangle index");
                }
            }
        }

        /// <summary>
        /// Get flags for edge in detail triangle.
        /// </summary>
        /// <param name="edgeIndex">The index of the first vertex of the edge. For instance, if 0, returns flags for edge AB.</param>
        /// <returns></returns>
        public DetailTriEdgeFlagTypes GetDetailTriEdgeFlags(int edgeIndex)
        {
            return (DetailTriEdgeFlagTypes)((Flags >> (edgeIndex * 2)) & 0x3);
        }
    }
}
