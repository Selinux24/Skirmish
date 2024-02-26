
namespace Engine.PathFinding
{
    /// <summary>
    /// Area types enum
    /// </summary>
    public enum GraphAreaTypes
    {
        /// <summary>
        /// Unwalkable area
        /// </summary>
        Unwalkable = 0b_0000_0000,
        /// <summary>
        /// Walkable area 
        /// </summary>
        Walkable = 0b_0011_1111,
    }
}
