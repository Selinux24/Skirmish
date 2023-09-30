using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph box area interface
    /// </summary>
    public interface IGraphAreaBox : IGraphArea
    {
        /// <summary>
        /// Box minimum point
        /// </summary>
        Vector3 BMin { get; set; }
        /// <summary>
        /// Box maximum point
        /// </summary>
        Vector3 BMax { get; set; }
    }
}
