using SharpDX;

namespace Engine.BuiltIn.Particles
{
    /// <summary>
    /// Built-in stream-out state
    /// </summary>
    public struct BuiltInStreamOutState
    {
        /// <summary>
        /// Emission rate
        /// </summary>
        public float EmissionRate { get; set; }
        /// <summary>
        /// Velocity sensitivity
        /// </summary>
        public float VelocitySensitivity { get; set; }

        /// <summary>
        /// Horizontal velocity
        /// </summary>
        public Vector2 HorizontalVelocity { get; set; }
        /// <summary>
        /// Vertical velocity
        /// </summary>
        public Vector2 VerticalVelocity { get; set; }

        /// <summary>
        /// Random values
        /// </summary>
        public Vector4 RandomValues { get; set; }
    }
}
