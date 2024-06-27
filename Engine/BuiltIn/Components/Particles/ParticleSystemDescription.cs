using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SharpDX;

namespace Engine.BuiltIn.Components.Particles
{
    using Engine.Content;
    using Engine.Content.Persistence;

    /// <summary>
    /// Particle system description
    /// </summary>
    public class ParticleSystemDescription
    {
        /// <summary>
        /// Default gravity value
        /// </summary>
        public static readonly Vector3 DefaultGravity = Vector3.Down;
        /// <summary>
        /// Default minimum color variation
        /// </summary>
        public static readonly Color DefaultMinColor = Color.Black;
        /// <summary>
        /// Default maximum color variation
        /// </summary>
        public static readonly Color DefaultMaxColor = Color.White;

        /// <summary>
        /// Creates a new particle description from another one
        /// </summary>
        /// <param name="particleDesc">The other particle description</param>
        /// <param name="contentPath">Content path</param>
        /// <param name="scale">Scale</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static ParticleSystemDescription Initialize(ParticleSystemFile particleDesc, string contentPath, float scale = 1f)
        {
            return Initialize((ParticleTypes)particleDesc.ParticleType, contentPath, particleDesc.TextureName, scale);
        }
        /// <summary>
        /// Initializes particle system by type
        /// </summary>
        /// <param name="type">Particle type enum</param>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Texture</param>
        /// <param name="scale">Scale</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static ParticleSystemDescription Initialize(ParticleTypes type, string contentPath, string texture, float scale = 1f)
        {
            if (type == ParticleTypes.Dust) return InitializeDust(contentPath, texture, scale);
            if (type == ParticleTypes.Fire) return InitializeFire(contentPath, texture, scale);
            if (type == ParticleTypes.SmokePlume) return InitializeSmokePlume(contentPath, texture, scale);
            if (type == ParticleTypes.ProjectileTrail) return InitializeProjectileTrail(contentPath, texture, scale);
            if (type == ParticleTypes.Explosion) return InitializeExplosion(contentPath, texture, scale);
            if (type == ParticleTypes.ExplosionSmoke) return InitializeSmokeExplosion(contentPath, texture, scale);

            return null;
        }
        /// <summary>
        /// Initializes dust particle systems
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Texture</param>
        /// <param name="scale">System scale</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static ParticleSystemDescription InitializeDust(string contentPath, string texture, float scale = 1f)
        {
            ParticleSystemDescription settings = new()
            {
                Name = "Dust",

                ContentPath = contentPath,
                TextureName = texture,

                MaxDuration = 1,

                MinHorizontalVelocity = 0,
                MaxHorizontalVelocity = 0.5f,

                MinVerticalVelocity = -1,
                MaxVerticalVelocity = 1,

                Gravity = new Vector3(0.0f, -0.35f, 0.0f),

                EndVelocity = 0.1f,

                MinColor = Color.SandyBrown * 0.5f,
                MaxColor = Color.SandyBrown,

                MinRotateSpeed = -1,
                MaxRotateSpeed = 1,

                MinStartSize = 0.25f,
                MaxStartSize = 0.5f,

                MinEndSize = 0.5f,
                MaxEndSize = 1f,

                BlendMode = BlendModes.Alpha,
            };

            settings.Scale(scale);

            return settings;
        }
        /// <summary>
        /// Initializes fire particle systems
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Texture</param>
        /// <param name="scale">System scale</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static ParticleSystemDescription InitializeFire(string contentPath, string texture, float scale = 1f)
        {
            ParticleSystemDescription settings = new()
            {
                Name = "Fire",

                ContentPath = contentPath,
                TextureName = texture,

                MaxDuration = 2,
                MaxDurationRandomness = 1,

                MinHorizontalVelocity = -1f,
                MaxHorizontalVelocity = +1f,

                MinVerticalVelocity = -1f,
                MaxVerticalVelocity = 1f,

                Gravity = new Vector3(0, 0, 0),

                MinColor = new Color(1f, 1f, 1f, 0.85f),
                MaxColor = new Color(1f, 1f, 1f, 1f),

                MinStartSize = 0.1f,
                MaxStartSize = 0.5f,

                MinEndSize = 2.5f,
                MaxEndSize = 5f,

                EmitterVelocitySensitivity = 1f,

                BlendMode = BlendModes.Alpha | BlendModes.Additive,
            };

            settings.Scale(scale);

            return settings;
        }
        /// <summary>
        /// Initializes smoke plume particle systems
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Texture</param>
        /// <param name="scale">System scale</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static ParticleSystemDescription InitializeSmokePlume(string contentPath, string texture, float scale = 1f)
        {
            ParticleSystemDescription settings = new()
            {
                Name = "SmokePlume",

                ContentPath = contentPath,
                TextureName = texture,

                MaxDuration = 10,

                MinHorizontalVelocity = -1.0f,
                MaxHorizontalVelocity = +1.0f,

                MinVerticalVelocity = -1.0f,
                MaxVerticalVelocity = +1.0f,

                Gravity = Vector3.Zero,

                EndVelocity = 0.75f,

                MinRotateSpeed = 0.5f,
                MaxRotateSpeed = 1.0f,

                MinStartSize = 1f,
                MaxStartSize = 2f,

                MinEndSize = 5f,
                MaxEndSize = 20f,

                MaxDurationRandomness = 1f,

                EmitterVelocitySensitivity = 1f,

                BlendMode = BlendModes.Alpha,
            };

            settings.Scale(scale);

            return settings;
        }
        /// <summary>
        /// Initializes porjectile trail particle systems
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Texture</param>
        /// <param name="scale">System scale</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static ParticleSystemDescription InitializeProjectileTrail(string contentPath, string texture, float scale = 1f)
        {
            ParticleSystemDescription settings = new()
            {
                Name = "ProjectileTrail",

                ContentPath = contentPath,
                TextureName = texture,

                MaxDuration = 0.5f,
                MaxDurationRandomness = 1.5f,

                EmitterVelocitySensitivity = 0.1f,

                MinHorizontalVelocity = -0.1f,
                MaxHorizontalVelocity = 0.1f,

                MinVerticalVelocity = -0.1f,
                MaxVerticalVelocity = 0.1f,

                MinColor = Color.Gray,
                MaxColor = Color.White,

                MinRotateSpeed = 1,
                MaxRotateSpeed = 1,

                MinStartSize = 0.25f,
                MaxStartSize = 0.5f,

                MinEndSize = 0.5f,
                MaxEndSize = 1.0f,

                BlendMode = BlendModes.Alpha,
            };

            settings.Scale(scale);

            return settings;
        }
        /// <summary>
        /// Initializes explosion particle systems
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Texture</param>
        /// <param name="scale">System scale</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static ParticleSystemDescription InitializeExplosion(string contentPath, string texture, float scale = 1f)
        {
            ParticleSystemDescription settings = new()
            {
                Name = "Explosion",

                ContentPath = contentPath,
                TextureName = texture,

                MaxDuration = 1f,
                MaxDurationRandomness = 1f,

                MinHorizontalVelocity = 0.5f,
                MaxHorizontalVelocity = 1.0f,

                MinVerticalVelocity = -1f,
                MaxVerticalVelocity = 1f,

                EndVelocity = 1,

                MinColor = Color.White,
                MaxColor = Color.White,

                MinRotateSpeed = -1,
                MaxRotateSpeed = 1,

                MinStartSize = 1.0f,
                MaxStartSize = 2.0f,

                MinEndSize = 3f,
                MaxEndSize = 5f,

                BlendMode = BlendModes.Alpha | BlendModes.Additive,
            };

            settings.Scale(scale);

            return settings;
        }
        /// <summary>
        /// Initializes explosion with smoke particle systems
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Texture</param>
        /// <param name="scale">System scale</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static ParticleSystemDescription InitializeSmokeExplosion(string contentPath, string texture, float scale = 1f)
        {
            ParticleSystemDescription settings = new()
            {
                Name = "SmokeExplosion",

                ContentPath = contentPath,
                TextureName = texture,

                MaxDuration = 4,

                MinHorizontalVelocity = 0,
                MaxHorizontalVelocity = 5,

                MinVerticalVelocity = -1,
                MaxVerticalVelocity = 5,

                Gravity = new Vector3(0, -20, 0),

                EndVelocity = 0,

                MinColor = Color.LightGray,
                MaxColor = Color.White,

                MinRotateSpeed = -2,
                MaxRotateSpeed = 2,

                MinStartSize = 1,
                MaxStartSize = 1,

                MinEndSize = 10,
                MaxEndSize = 20,

                BlendMode = BlendModes.Alpha,
            };

            settings.Scale(scale);

            return settings;
        }

        /// <summary>
        /// Particle type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ParticleTypes ParticleType { get; set; }
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
        public Direction3 Gravity { get; set; }

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

        /// <summary>
        /// Constructor
        /// </summary>
        public ParticleSystemDescription()
        {
            ParticleType = ParticleTypes.None;
            Name = null;
            ContentPath = null;
            TextureName = null;

            MaxDuration = 0;
            MaxDurationRandomness = 1;
            MaxHorizontalVelocity = 0;
            MinHorizontalVelocity = 0;
            MaxVerticalVelocity = 0;
            MinVerticalVelocity = 0;
            Gravity = DefaultGravity;
            EndVelocity = 1;
            MinColor = DefaultMinColor;
            MaxColor = DefaultMaxColor;
            MinRotateSpeed = 0;
            MaxRotateSpeed = 0;
            MinStartSize = 1;
            MaxStartSize = 1;
            MinEndSize = 1;
            MaxEndSize = 1;
            BlendMode = BlendModes.Alpha;
            EmitterVelocitySensitivity = 0;
        }

        /// <summary>
        /// Scales the particle system
        /// </summary>
        /// <param name="scale">Scale to apply</param>
        public void Scale(float scale)
        {
            if (MathUtil.IsOne(scale))
            {
                return;
            }

            MaxHorizontalVelocity *= scale;
            MinHorizontalVelocity *= scale;
            MaxVerticalVelocity *= scale;
            MinVerticalVelocity *= scale;

            Gravity = (Vector3)Gravity * scale;
            EndVelocity *= scale;

            MinStartSize *= scale;
            MaxStartSize *= scale;
            MinEndSize *= scale;
            MaxEndSize *= scale;
        }
        /// <summary>
        /// Updates the current particle parameters with the specified particle description
        /// </summary>
        /// <param name="other">The other particle description</param>
        public void Update(ParticleSystemDescription other)
        {
            MaxDuration = other.MaxDuration;
            MaxDurationRandomness = other.MaxDurationRandomness;
            MaxHorizontalVelocity = other.MaxHorizontalVelocity;
            MinHorizontalVelocity = other.MinHorizontalVelocity;
            MaxVerticalVelocity = other.MaxVerticalVelocity;
            MinVerticalVelocity = other.MinVerticalVelocity;
            Gravity = other.Gravity;
            EndVelocity = other.EndVelocity;
            MinColor = other.MinColor;
            MaxColor = other.MaxColor;
            MinRotateSpeed = other.MinRotateSpeed;
            MaxRotateSpeed = other.MaxRotateSpeed;
            MinStartSize = other.MinStartSize;
            MaxStartSize = other.MaxStartSize;
            MinEndSize = other.MinEndSize;
            MaxEndSize = other.MaxEndSize;
            BlendMode = other.BlendMode;
            EmitterVelocitySensitivity = other.EmitterVelocitySensitivity;
        }
    }
}
