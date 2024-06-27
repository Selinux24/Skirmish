using System;

namespace Engine
{
    /// <summary>
    /// Shadow casting algorihtms
    /// </summary>
    [Flags]
    public enum ShadowCastingAlgorihtms : uint
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Directional shadow casting
        /// </summary>
        Directional = 1,
        /// <summary>
        /// Spot shadow casting
        /// </summary>
        Spot = 2,
        /// <summary>
        /// Point shadow casting
        /// </summary>
        Point = 4,
        /// <summary>
        /// All shadow types
        /// </summary>
        All = Directional | Spot | Point,
    }
}
