using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.PathFinding.RecastNavigation
{
    public enum SamplePolyFlags
    {
        /// <summary>
        /// Ability to walk (ground, grass, road)
        /// </summary>
        SAMPLE_POLYFLAGS_WALK = 0x01,
        /// <summary>
        /// Ability to swim (water).
        /// </summary>
        SAMPLE_POLYFLAGS_SWIM = 0x02,
        /// <summary>
        /// Ability to move through doors.
        /// </summary>
        SAMPLE_POLYFLAGS_DOOR = 0x04,
        /// <summary>
        /// Ability to jump.
        /// </summary>
        SAMPLE_POLYFLAGS_JUMP = 0x08,
        /// <summary>
        /// Disabled polygon
        /// </summary>
        SAMPLE_POLYFLAGS_DISABLED = 0x10,
        /// <summary>
        /// All abilities.
        /// </summary>
        SAMPLE_POLYFLAGS_ALL = 0xffff
    }
}
