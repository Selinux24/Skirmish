using SharpDX;

namespace Engine
{
    /// <summary>
    /// Controller path
    /// </summary>
    public interface IControllerPath
    {
        /// <summary>
        /// Total length of path
        /// </summary>
        float Length { get; }
        /// <summary>
        /// Gets path position in specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Returns path position</returns>
        Vector3 GetPosition(float time);
        /// <summary>
        /// Gets the next control point in specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Returns next path control point</returns>
        Vector3 GetNextControlPoint(float time);
    }
}
