using SharpDX;

namespace Engine
{
    /// <summary>
    /// Particle system description
    /// </summary>
    public class ParticleSystemDescription
    {
        /// <summary>
        /// Emitter types
        /// </summary>
        public enum EmitterTypes
        {
            /// <summary>
            /// Emit from position
            /// </summary>
            FixedPosition,
            /// <summary>
            /// Emit from camera
            /// </summary>
            FromCamera,
        }

        /// <summary>
        /// Particle class
        /// </summary>
        public ParticleClasses ParticleClass = ParticleClasses.Unknown;
        /// <summary>
        /// Maximum particles
        /// </summary>
        public int MaximumParticles = 1000;
        /// <summary>
        /// Maximum age of particle
        /// </summary>
        public float MaximumAge = 5f;
        /// <summary>
        /// Particle age to emit new flares
        /// </summary>
        public float EmitterAge = 0.001f;
        /// <summary>
        /// Acceleration vector
        /// </summary>
        public Vector3 Acceleration = GameEnvironment.Gravity;
        /// <summary>
        /// Emitter type
        /// </summary>
        public EmitterTypes EmitterType = EmitterTypes.FromCamera;
        /// <summary>
        /// Texture list
        /// </summary>
        public string[] Textures = null;
        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath = "Resources";
        /// <summary>
        /// Gets or sets whether the object cast shadow
        /// </summary>
        public bool CastShadow { get; set; }
        /// <summary>
        /// Emitters
        /// </summary>
        public ParticleEmitter[] Emitters = null;

        /// <summary>
        /// Creates a fire particle system
        /// </summary>
        /// <param name="emitter">Emitter</param>
        /// <param name="textures">Texture list</param>
        /// <returns>Returns particle system description</returns>
        public static ParticleSystemDescription Fire(ParticleEmitter emitter, params string[] textures)
        {
            return Fire(new[] { emitter }, textures);
        }
        /// <summary>
        /// Creates a fire particle system
        /// </summary>
        /// <param name="emitters">Emitter list</param>
        /// <param name="textures">Texture list</param>
        /// <returns>Returns particle system description</returns>
        public static ParticleSystemDescription Fire(ParticleEmitter[] emitters, params string[] textures)
        {
            return new ParticleSystemDescription()
            {
                ParticleClass = ParticleClasses.Fire,
                MaximumParticles = 500,
                MaximumAge = 0.5f,
                EmitterAge = 0.005f,
                Acceleration = new Vector3(0.0f, 5.8f, 0.0f),
                EmitterType = EmitterTypes.FixedPosition,
                Textures = textures,
                Emitters = emitters
            };
        }
        /// <summary>
        /// Creates a smoke particle system
        /// </summary>
        /// <param name="emitter">Emitter</param>
        /// <param name="textures">Texture list</param>
        /// <returns>Returns particle system description</returns>
        public static ParticleSystemDescription Smoke(ParticleEmitter emitter, params string[] textures)
        {
            return Smoke(new[] { emitter }, textures);
        }
        /// <summary>
        /// Creates a smoke particle system
        /// </summary>
        /// <param name="emitters">Emitter list</param>
        /// <param name="textures">Texture list</param>
        /// <returns>Returns particle system description</returns>
        public static ParticleSystemDescription Smoke(ParticleEmitter[] emitters, params string[] textures)
        {
            return new ParticleSystemDescription()
            {
                ParticleClass = ParticleClasses.Smoke,
                MaximumParticles = 500,
                MaximumAge = 1.0f,
                EmitterAge = 0.33f,
                Acceleration = new Vector3(0.0f, 2f, 0.0f),
                EmitterType = EmitterTypes.FixedPosition,
                Textures = textures,
                Emitters = emitters
            };
        }
        /// <summary>
        /// Creates a rain particle system
        /// </summary>
        /// <param name="emitter">Emitter</param>
        /// <param name="textures">Texture list</param>
        /// <returns>Returns particle system description</returns>
        public static ParticleSystemDescription Rain(ParticleEmitter emitter, params string[] textures)
        {
            return Rain(new[] { emitter }, textures);
        }
        /// <summary>
        /// Creates a rain particle system
        /// </summary>
        /// <param name="emitters">Emitter list</param>
        /// <param name="textures">Texture list</param>
        /// <returns>Returns particle system description</returns>
        public static ParticleSystemDescription Rain(ParticleEmitter[] emitters, params string[] textures)
        {
            return new ParticleSystemDescription()
            {
                ParticleClass = ParticleClasses.Rain,
                MaximumParticles = 10000,
                MaximumAge = 3.0f,
                EmitterAge = 0.002f,
                Acceleration = (GameEnvironment.Gravity + Vector3.UnitX),
                EmitterType = EmitterTypes.FromCamera,
                Textures = textures,
                Emitters = emitters
            };
        }
    }
}
