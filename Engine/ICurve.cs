using SharpDX;

namespace Engine
{
    /// <summary>
    /// Path
    /// </summary>
    public interface ICurve
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
    }
}
