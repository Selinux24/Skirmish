using SharpDX.Direct3D;

namespace Engine
{
    /// <summary>
    /// Primitive topology
    /// </summary>
    public enum Topology
    {
        /// <summary>
        /// Point list
        /// </summary>
        PointList = PrimitiveTopology.PointList,
        /// <summary>
        /// Line list
        /// </summary>
        LineList = PrimitiveTopology.LineList,
        /// <summary>
        /// Triangle list
        /// </summary>
        TriangleList = PrimitiveTopology.TriangleList,
    }
}
