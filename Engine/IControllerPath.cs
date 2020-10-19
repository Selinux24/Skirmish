using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Controller path
    /// </summary>
    public interface IControllerPath
    {
        /// <summary>
        /// First point
        /// </summary>
        Vector3 First { get; }
        /// <summary>
        /// Last point
        /// </summary>
        Vector3 Last { get; }
        /// <summary>
        /// Total length of path
        /// </summary>
        float Length { get; }
        /// <summary>
        /// Gets the position count of the path
        /// </summary>
        int PositionCount { get; }
        /// <summary>
        /// Gets the normal count of the path
        /// </summary>
        int NormalCount { get; }
        /// <summary>
        /// Gets path position in specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Returns path position</returns>
        Vector3 GetPosition(float time);
        /// <summary>
        /// Gets path normal in specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Returns path normal</returns>
        Vector3 GetNormal(float time);
        /// <summary>
        /// Gets the next control point in specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Returns next path control point</returns>
        Vector3 GetNextControlPoint(float time);
        /// <summary>
        /// Samples current path in a vector array
        /// </summary>
        /// <param name="sampleTime">Time delta</param>
        /// <returns>Returns a vector array</returns>
        IEnumerable<Vector3> SamplePath(float sampleTime);
    }
}
