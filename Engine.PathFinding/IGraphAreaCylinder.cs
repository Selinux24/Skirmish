
namespace Engine.PathFinding
{
    using Engine.Mathematics;

    /// <summary>
    /// Graph cylinder area interface
    /// </summary>
    public interface IGraphAreaCylinder : IGraphArea
    {
        /// <summary>
        /// Center
        /// </summary>
        Vector3 Center { get; set; }
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
