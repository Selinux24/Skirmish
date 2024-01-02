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

    /// <summary>
    /// Sample oly Flag Types extents
    /// </summary>
    public static class SamplePolyFlagTypesExtents
    {
        /// <summary>
        /// Evaluates area type and returns spected flag type
        /// </summary>
        /// <param name="area">Area type</param>
        /// <returns>Returns the flag type</returns>
        public static SamplePolyFlagTypes EvaluateArea(SamplePolyAreas area)
        {
            if (area == SamplePolyAreas.Ground ||
                area == SamplePolyAreas.Grass ||
                area == SamplePolyAreas.Road)
            {
                return SamplePolyFlagTypes.Walk;
            }
            else if (area == SamplePolyAreas.Water)
            {
                return SamplePolyFlagTypes.Swim;
            }
            else if (area == SamplePolyAreas.Door)
            {
                return SamplePolyFlagTypes.Walk | SamplePolyFlagTypes.Door;
            }

            return SamplePolyFlagTypes.None;
        }
    }
}
