using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph connection interface
    /// </summary>
    public interface IGraphConnection
    {
        /// <summary>
        /// Connection Id
        /// </summary>
        int Id { get; }
        /// <summary>
        /// Start point
        /// </summary>
        Vector3 Start { get; set; }
        /// <summary>
        /// End point
        /// </summary>
        Vector3 End { get; set; }
        /// <summary>
        /// Points radius
        /// </summary>
        float Radius { get; set; }
        /// <summary>
        /// Connection direction
        /// </summary>
        int Direction { get; set; }
        /// <summary>
        /// Area type
        /// </summary>
        GraphConnectionAreaTypes AreaType { get; set; }
        /// <summary>
        /// Area flags
        /// </summary>
        GraphConnectionFlagTypes FlagTypes { get; set; }
    }
}
