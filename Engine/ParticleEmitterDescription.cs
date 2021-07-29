using SharpDX;

namespace Engine.Content.Persistence
{
    /// <summary>
    /// Particle emitter description
    /// </summary>
    public class ParticleEmitterDescription
    {
        /// <summary>
        /// Particle name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Position
        /// </summary>
        public Position3 Position { get; set; }
        /// <summary>
        /// Velocity
        /// </summary>
        public Direction3 Velocity { get; set; }
        /// <summary>
        /// Particle scale
        /// </summary>
        public float Scale { get; set; }
        /// <summary>
        /// Emission rate
        /// </summary>
        public float EmissionRate { get; set; }
        /// <summary>
        /// Emitter duration
        /// </summary>
        public float Duration { get; set; }
        /// <summary>
        /// Gets or sets whether the emitter duration is infinite
        /// </summary>
        public bool InfiniteDuration { get; set; }
        /// <summary>
        /// Gets or sets the maximum distance from camera
        /// </summary>
        public float MaximumDistance { get; set; }
        /// <summary>
        /// Distance from camera
        /// </summary>
        public float Distance { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParticleEmitterDescription()
        {
            Position = Vector3.Zero;
            Velocity = Vector3.Up;
            Scale = 1f;
            EmissionRate = 1f;
            Duration = 0f;
            InfiniteDuration = false;
            MaximumDistance = 100f;
            Distance = 0f;
        }
    }
}
