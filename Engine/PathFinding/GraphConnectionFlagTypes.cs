
namespace Engine.PathFinding
{
    /// <summary>
    /// Graph connection flag types
    /// </summary>
    public enum GraphConnectionFlagTypes : int
    {
        /// <summary>
        /// Ability to walk (ground, grass, road)
        /// </summary>
        Walk = 0x01,
        /// <summary>
        /// Ability to swim (water).
        /// </summary>
        Swim = 0x02,
        /// <summary>
        /// Ability to move through doors.
        /// </summary>
        Door = 0x04,
        /// <summary>
        /// Ability to jump.
        /// </summary>
        Jump = 0x08,
        /// <summary>
        /// Disabled polygon
        /// </summary>
        Disabled = 0x10,
        /// <summary>
        /// All abilities.
        /// </summary>
        All = 0xffff
    }
}
