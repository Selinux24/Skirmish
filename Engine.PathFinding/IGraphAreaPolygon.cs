using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph polygon area interface
    /// </summary>
    public interface IGraphAreaPolygon : IGraphArea
    {
        /// <summary>
        /// Vertices (convex polygon)
        /// </summary>
        Vector3[] Vertices { get; set; }
        /// <summary>
        /// Minimum height
        /// </summary>
        float MinHeight { get; set; }
        /// <summary>
        /// Maximum height
        /// </summary>
        float MaxHeight { get; set; }
    }
}
