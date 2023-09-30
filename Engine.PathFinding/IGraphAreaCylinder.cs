using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph cylinder area interface
    /// </summary>
    public interface IGraphAreaCylinder : IGraphArea
    {
        /// <summary>
        /// Position
        /// </summary>
        Vector3 Position { get; set; }
        /// <summary>
        /// Radius
        /// </summary>
        float Radius { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        float Height { get; set; }
    }
}
