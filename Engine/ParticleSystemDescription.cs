using SharpDX;
using System;
using System.Globalization;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Particle system description
    /// </summary>
    [Serializable]
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
        /// <param name="scale">Scale</param>
        /// <returns>Returns the new generated particle system description</returns>
        internal static ParticleSystemDescription Initialize(ParticleSystemDescription particleDesc, float scale = 1f)
        {
            return Initialize(particleDesc.ParticleType, particleDesc.ContentPath, particleDesc.TextureName, scale);
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
            ParticleSystemDescription settings = new ParticleSystemDescription
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

                Transparent = true,
                Additive = false
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
            ParticleSystemDescription settings = new ParticleSystemDescription
            {
                Name = "Fire",

                ContentPath = contentPath,
                TextureName = texture,

                MaxDuration = 2,
                MaxDurationRandomness = 1,

                MinHorizontalVelocity = 0,
                MaxHorizontalVelocity = 1.5f,

                MinVerticalVelocity = -1,
                MaxVerticalVelocity = 1,

                Gravity = new Vector3(0, 1, 0),

                MinColor = new Color(1f, 1f, 1f, 0.55f),
                MaxColor = new Color(1f, 1f, 1f, 1f),

                MinStartSize = 0.5f,
                MaxStartSize = 1,

                MinEndSize = 2.5f,
                MaxEndSize = 4f,

                Transparent = true,
                Additive = true
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
            ParticleSystemDescription settings = new ParticleSystemDescription
            {
                Name = "SmokePlume",

                ContentPath = contentPath,
                TextureName = texture,

                MaxDuration = 10,

                MinHorizontalVelocity = 0,
                MaxHorizontalVelocity = 0.5f,

                MinVerticalVelocity = 1.0f,
                MaxVerticalVelocity = 2.0f,

                Gravity = new Vector3(0.0f, 0.5f, 0.0f),

                EndVelocity = 0.75f,

                MinRotateSpeed = -1f,
                MaxRotateSpeed = 1f,

                MinStartSize = 1,
                MaxStartSize = 2,

                MinEndSize = 5,
                MaxEndSize = 20,

                Transparent = true,
                Additive = false
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
            ParticleSystemDescription settings = new ParticleSystemDescription
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

                Transparent = true,
                Additive = false
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
            ParticleSystemDescription settings = new ParticleSystemDescription
            {
                Name = "Explosion",

                ContentPath = contentPath,
                TextureName = texture,

                MaxDuration = 1.5f,
                MaxDurationRandomness = 1,

                MinHorizontalVelocity = 1.0f,
                MaxHorizontalVelocity = 1.5f,

                MinVerticalVelocity = -1f,
                MaxVerticalVelocity = 1f,

                EndVelocity = 0,

                MinColor = Color.DarkGray,
                MaxColor = Color.Gray,

                MinRotateSpeed = -1,
                MaxRotateSpeed = 1,

                MinStartSize = 0.25f,
                MaxStartSize = 0.25f,

                MinEndSize = 5,
                MaxEndSize = 10,

                Transparent = true,
                Additive = true
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
            ParticleSystemDescription settings = new ParticleSystemDescription
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

                Transparent = true,
                Additive = false
            };

            settings.Scale(scale);

            return settings;
        }

        /// <summary>
        /// Particle type
        /// </summary>
        [XmlAttribute("type")]
        public ParticleTypes ParticleType { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
        /// <summary>
        /// Content path
        /// </summary>
        [XmlAttribute("contentPath")]
        public string ContentPath { get; set; }
        /// <summary>
        /// Texture name
        /// </summary>
        [XmlAttribute("textureName")]
        public string TextureName { get; set; }

        /// <summary>
        /// Maximum particle duration
        /// </summary>
        [XmlElement("maxDuration")]
        public float MaxDuration { get; set; }
        /// <summary>
        /// Duration randomness
        /// </summary>
        [XmlElement("maxDurationRandomness")]
        public float MaxDurationRandomness { get; set; }

        /// <summary>
        /// Maximum horizontal velocity
        /// </summary>
        [XmlElement("maxHorizontalVelocity")]
        public float MaxHorizontalVelocity { get; set; }
        /// <summary>
        /// Minimum horizontal velocity
        /// </summary>
        [XmlElement("minHorizontalVelocity")]
        public float MinHorizontalVelocity { get; set; }

        /// <summary>
        /// Maximum vertical velocity
        /// </summary>
        [XmlElement("maxVerticalVelocity")]
        public float MaxVerticalVelocity { get; set; }
        /// <summary>
        /// Minimum vertical velocity
        /// </summary>
        [XmlElement("minVerticalVelocity")]
        public float MinVerticalVelocity { get; set; }

        /// <summary>
        /// Gravity
        /// </summary>
        [XmlIgnore]
        public Vector3 Gravity { get; set; }
        /// <summary>
        /// Gravity vector
        /// </summary>
        [XmlElement("gravity")]
        public string GravityText
        {
            get
            {
                return string.Format("{0} {1} {2}", Gravity.X, Gravity.Y, Gravity.Z);
            }
            set
            {
                var floats = this.Split(value);
                if (floats.Length == 3)
                {
                    Gravity = new Vector3(floats);
                }
                else if (floats.Length == 1)
                {
                    Gravity = new Vector3(floats[0]);
                }
                else
                {
                    Gravity = DefaultGravity;
                }
            }
        }

        /// <summary>
        /// Velocity at end
        /// </summary>
        [XmlElement("endVelocity")]
        public float EndVelocity { get; set; }

        /// <summary>
        /// Minimum color variation
        /// </summary>
        [XmlIgnore]
        public Color MinColor { get; set; }
        /// <summary>
        /// Minimum color variation
        /// </summary>
        [XmlElement("minColor")]
        public string MinColorText
        {
            get
            {
                return string.Format("{0} {1} {2} {3}", MinColor.R, MinColor.G, MinColor.B, MinColor.A);
            }
            set
            {
                var floats = this.Split(value);
                if (floats.Length == 4)
                {
                    MinColor = new Color(floats);
                }
                else if (floats.Length == 1)
                {
                    MinColor = new Color(floats[0]);
                }
                else
                {
                    MinColor = DefaultMinColor;
                }
            }
        }
        /// <summary>
        /// Maximum color variation
        /// </summary>
        [XmlIgnore]
        public Color MaxColor { get; set; }
        /// <summary>
        /// Maximum color variation
        /// </summary>
        [XmlElement("maxColor")]
        public string MaxColorText
        {
            get
            {
                return string.Format("{0} {1} {2} {3}", MaxColor.R, MaxColor.G, MaxColor.B, MaxColor.A);
            }
            set
            {
                var floats = this.Split(value);
                if (floats.Length == 4)
                {
                    MaxColor = new Color(floats);
                }
                else if (floats.Length == 1)
                {
                    MaxColor = new Color(floats[0]);
                }
                else
                {
                    MaxColor = DefaultMaxColor;
                }
            }
        }

        /// <summary>
        /// Minimum rotation speed
        /// </summary>
        [XmlElement("minRotateSpeed")]
        public float MinRotateSpeed { get; set; }
        /// <summary>
        /// Maximum rotation speed
        /// </summary>
        [XmlElement("maxRotateSpeed")]
        public float MaxRotateSpeed { get; set; }

        /// <summary>
        /// Minimum starting size
        /// </summary>
        [XmlElement("minStartSize")]
        public float MinStartSize { get; set; }
        /// <summary>
        /// Maximum starting size
        /// </summary>
        [XmlElement("maxStartSize")]
        public float MaxStartSize { get; set; }

        /// <summary>
        /// Minimum ending size
        /// </summary>
        [XmlElement("minEndSize")]
        public float MinEndSize { get; set; }
        /// <summary>
        /// Maximum ending size
        /// </summary>
        [XmlElement("maxEndSize")]
        public float MaxEndSize { get; set; }

        /// <summary>
        /// Gets or sets wheter the particles were transparent
        /// </summary>
        [XmlElement("transparent")]
        public bool Transparent { get; set; }
        /// <summary>
        /// Gets or sets wheter the particles were additive
        /// </summary>
        [XmlElement("additive")]
        public bool Additive { get; set; }

        /// <summary>
        /// Emitter velocity sensitivity
        /// </summary>
        [XmlElement("emitterVelocitySensitivity")]
        public float EmitterVelocitySensitivity { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParticleSystemDescription()
        {
            this.ParticleType = ParticleTypes.None;
            this.Name = null;
            this.ContentPath = null;
            this.TextureName = null;

            this.MaxDuration = 0;
            this.MaxDurationRandomness = 1;
            this.MaxHorizontalVelocity = 0;
            this.MinHorizontalVelocity = 0;
            this.MaxVerticalVelocity = 0;
            this.MinVerticalVelocity = 0;
            this.Gravity = DefaultGravity;
            this.EndVelocity = 1;
            this.MinColor = DefaultMinColor;
            this.MaxColor = DefaultMaxColor;
            this.MinRotateSpeed = 0;
            this.MaxRotateSpeed = 0;
            this.MinStartSize = 1;
            this.MaxStartSize = 1;
            this.MinEndSize = 1;
            this.MaxEndSize = 1;
            this.Transparent = false;
            this.EmitterVelocitySensitivity = 0;
        }

        /// <summary>
        /// Scales the particle system
        /// </summary>
        /// <param name="scale">Scale to apply</param>
        public void Scale(float scale)
        {
            if (scale != 1f)
            {
                this.MaxHorizontalVelocity *= scale;
                this.MinHorizontalVelocity *= scale;
                this.MaxVerticalVelocity *= scale;
                this.MinVerticalVelocity *= scale;

                this.Gravity *= scale;
                this.EndVelocity *= scale;

                this.MinStartSize *= scale;
                this.MaxStartSize *= scale;
                this.MinEndSize *= scale;
                this.MaxEndSize *= scale;
            }
        }
        /// <summary>
        /// Updates the current particle parameters with the specified particle description
        /// </summary>
        /// <param name="other">The other particle description</param>
        public void Update(ParticleSystemDescription other)
        {
            this.MaxDuration = other.MaxDuration;
            this.MaxDurationRandomness = other.MaxDurationRandomness;
            this.MaxHorizontalVelocity = other.MaxHorizontalVelocity;
            this.MinHorizontalVelocity = other.MinHorizontalVelocity;
            this.MaxVerticalVelocity = other.MaxVerticalVelocity;
            this.MinVerticalVelocity = other.MinVerticalVelocity;
            this.Gravity = other.Gravity;
            this.EndVelocity = other.EndVelocity;
            this.MinColor = other.MinColor;
            this.MaxColor = other.MaxColor;
            this.MinRotateSpeed = other.MinRotateSpeed;
            this.MaxRotateSpeed = other.MaxRotateSpeed;
            this.MinStartSize = other.MinStartSize;
            this.MaxStartSize = other.MaxStartSize;
            this.MinEndSize = other.MinEndSize;
            this.MaxEndSize = other.MaxEndSize;
            this.Transparent = other.Transparent;
            this.EmitterVelocitySensitivity = other.EmitterVelocitySensitivity;
        }

        /// <summary>
        /// Splits the text into a float array
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Returns a float array</returns>
        private float[] Split(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var bits = text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                bool allOk = true;
                float[] res = new float[bits.Length];

                for (int i = 0; i < res.Length; i++)
                {
                    if (float.TryParse(bits[i], NumberStyles.Float, CultureInfo.InvariantCulture, out float n))
                    {
                        res[i] = n;
                    }
                    else
                    {
                        allOk = false;
                        break;
                    }
                }

                if (allOk)
                {
                    return res;
                }
            }

            return new float[] { };
        }
    }
}
