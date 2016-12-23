
namespace Engine.Common
{
    /// <summary>
    /// Index buffer shape
    /// </summary>
    public enum IndexBufferShapeEnum : int
    {
        /// <summary>
        /// None
        /// </summary>
        None = -1,
        /// <summary>
        /// Use full vertex list
        /// </summary>
        Full = 0,
        /// <summary>
        /// Top side simplified
        /// </summary>
        SideTop = 1,
        /// <summary>
        /// Bottom side simplified
        /// </summary>
        SideBottom = 2,
        /// <summary>
        /// Left side simplified
        /// </summary>
        SideLeft = 3,
        /// <summary>
        /// Right side simplified
        /// </summary>
        SideRight = 4,
        /// <summary>
        /// Top left sides simplified
        /// </summary>
        CornerTopLeft = 5,
        /// <summary>
        /// Bottom left sides simplified
        /// </summary>
        CornerBottomLeft = 6,
        /// <summary>
        /// Top right sides simplified
        /// </summary>
        CornerTopRight = 7,
        /// <summary>
        /// Bottom right sides simplified
        /// </summary>
        CornerBottomRight = 8,
    }
}
