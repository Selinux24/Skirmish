using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Engine.Content.Persistence
{
    /// <summary>
    /// Particle system description
    /// </summary>
    public class ParticleSystemFile
    {
        /// <summary>
        /// Particle type
        /// </summary>
        public int ParticleType { get; set; } = 0;
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath { get; set; }
        /// <summary>
        /// Texture name
        /// </summary>
        public string TextureName { get; set; }

        /// <summary>
        /// Maximum particle duration
        /// </summary>
        public float MaxDuration { get; set; }
        /// <summary>
        /// Duration randomness
        /// </summary>
        public float MaxDurationRandomness { get; set; }

        /// <summary>
        /// Maximum horizontal velocity
        /// </summary>
        public float MaxHorizontalVelocity { get; set; }
        /// <summary>
        /// Minimum horizontal velocity
        /// </summary>
        public float MinHorizontalVelocity { get; set; }

        /// <summary>
        /// Maximum vertical velocity
        /// </summary>
        public float MaxVerticalVelocity { get; set; }
        /// <summary>
        /// Minimum vertical velocity
        /// </summary>
        public float MinVerticalVelocity { get; set; }

        /// <summary>
        /// Gravity
        /// </summary>
        public Direction3 Gravity { get; set; } = Direction3.Down;

        /// <summary>
        /// Velocity at end
        /// </summary>
        public float EndVelocity { get; set; }

        /// <summary>
        /// Minimum color variation
        /// </summary>
        public ColorRgba MinColor { get; set; }
        /// <summary>
        /// Maximum color variation
        /// </summary>
        public ColorRgba MaxColor { get; set; }

        /// <summary>
        /// Minimum rotation speed
        /// </summary>
        public float MinRotateSpeed { get; set; }
        /// <summary>
        /// Maximum rotation speed
        /// </summary>
        public float MaxRotateSpeed { get; set; }

        /// <summary>
        /// Minimum starting size
        /// </summary>
        public float MinStartSize { get; set; }
        /// <summary>
        /// Maximum starting size
        /// </summary>
        public float MaxStartSize { get; set; }

        /// <summary>
        /// Minimum ending size
        /// </summary>
        public float MinEndSize { get; set; }
        /// <summary>
        /// Maximum ending size
        /// </summary>
        public float MaxEndSize { get; set; }

        /// <summary>
        /// Gets or sets whether the blend mode
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public BlendModes BlendMode { get; set; }

        /// <summary>
        /// Emitter velocity sensitivity
        /// </summary>
        public float EmitterVelocitySensitivity { get; set; }
    }
}
