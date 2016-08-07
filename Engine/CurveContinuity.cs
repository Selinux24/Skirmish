
namespace Engine
{
    /// <summary>
    /// Defines the continuity of keys on a <see cref="Curve"/>.
    /// </summary>
    public enum CurveContinuity
    {
        /// <summary>
        /// Interpolation can be used between this key and the next.
        /// </summary>
        Smooth,
        /// <summary>
        /// Interpolation cannot be used. A position between the two points returns this point.
        /// </summary>
        Step
    }
}
