using SharpDX;

namespace Engine.BuiltIn.Drawers.Particles
{
    /// <summary>
    /// Particle state for particle effects
    /// </summary>
    public struct BuiltInParticlesState
    {
        /// <summary>
        /// Total time
        /// </summary>
        public float TotalTime { get; set; }
        /// <summary>
        /// Elapsed time
        /// </summary>
        public float ElapsedTime { get; set; }
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
        /// <summary>
        /// Maximum particle duration
        /// </summary>
        public float MaxDuration { get; set; }
        /// <summary>
        /// Maximum duration randomness
        /// </summary>
        public float MaxDurationRandomness { get; set; }
        /// <summary>
        /// End velocity
        /// </summary>
        public float EndVelocity { get; set; }
        /// <summary>
        /// Gravity
        /// </summary>
        public Vector3 Gravity { get; set; }
        /// <summary>
        /// Start size
        /// </summary>
        public Vector2 StartSize { get; set; }
        /// <summary>
        /// End size
        /// </summary>
        public Vector2 EndSize { get; set; }
        /// <summary>
        /// Minimum color
        /// </summary>
        public Color4 MinColor { get; set; }
        /// <summary>
        /// Maximum color
        /// </summary>
        public Color4 MaxColor { get; set; }
        /// <summary>
        /// Use texture rotation
        /// </summary>
        public bool UseRotation { get; set; }
        /// <summary>
        /// Rotation speed
        /// </summary>
        public Vector2 RotateSpeed { get; set; }
    }
}
