using System;

namespace Engine.PathFinding.RecastNavigation
{
    [Flags]
    public enum SamplePolyFlagTypes
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0x00,
        /// <summary>
        /// SAMPLE_POLYFLAGS_WALK. Ability to walk (ground, grass, road)
        /// </summary>
        Walk = 0x01,
        /// <summary>
        /// SAMPLE_POLYFLAGS_SWIM. Ability to swim (water).
        /// </summary>
        Swim = 0x02,
        /// <summary>
        /// SAMPLE_POLYFLAGS_DOOR. Ability to move through doors.
        /// </summary>
        Door = 0x04,
        /// <summary>
        /// SAMPLE_POLYFLAGS_JUMP. Ability to jump.
        /// </summary>
        Jump = 0x08,
        /// <summary>
        /// SAMPLE_POLYFLAGS_DISABLED. Disabled polygon
        /// </summary>
        Disabled = 0x10,
    }
}
