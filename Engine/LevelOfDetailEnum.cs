
namespace Engine
{
    /// <summary>
    /// Levels of detail
    /// </summary>
    public enum LevelOfDetailEnum
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// High (scale 1)
        /// </summary>
        High = 1,
        /// <summary>
        /// Medium (scale 1:2)
        /// </summary>
        Medium = 2,
        /// <summary>
        /// Low (scale 1:4)
        /// </summary>
        Low = 4,
        /// <summary>
        /// Minimum level (scale 1:8)
        /// </summary>
        Minimum = 8,
    }
}
