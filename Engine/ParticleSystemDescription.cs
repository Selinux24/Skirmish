using SharpDX;

namespace Engine
{
    /// <summary>
    /// Particle system description
    /// </summary>
    public class ParticleSystemDescription
    {
        /// <summary>
        /// Initializes dust particle systems
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Texture</param>
        /// <param name="scale">System scale</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static ParticleSystemDescription InitializeDust(string contentPath, string texture, float scale = 1f)
        {
            ParticleSystemDescription settings = new ParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxDuration = 1;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0.5f;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 1;

            settings.Gravity = new Vector3(0.0f, -0.35f, 0.0f);

            settings.EndVelocity = 0.1f;

            settings.MinColor = Color.SandyBrown * 0.5f;
            settings.MaxColor = Color.SandyBrown;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 0.25f;
            settings.MaxStartSize = 0.5f;

            settings.MinEndSize = 0.5f;
            settings.MaxEndSize = 1f;

            settings.Transparent = true;

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
            ParticleSystemDescription settings = new ParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxDuration = 2;
            settings.MaxDurationRandomness = 1;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 1.5f;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 1;

            settings.Gravity = new Vector3(0, 1, 0);

            settings.MinColor = new Color(1f, 1f, 1f, 0.55f);
            settings.MaxColor = new Color(1f, 1f, 1f, 1f);

            settings.MinStartSize = 0.5f;
            settings.MaxStartSize = 1;

            settings.MinEndSize = 2.5f;
            settings.MaxEndSize = 4f;

            settings.Transparent = true;

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
            ParticleSystemDescription settings = new ParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxDuration = 10;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0.5f;

            settings.MinVerticalVelocity = 1.0f;
            settings.MaxVerticalVelocity = 2.0f;

            settings.Gravity = new Vector3(0.0f, 0.5f, 0.0f);

            settings.EndVelocity = 0.75f;

            settings.MinRotateSpeed = -1f;
            settings.MaxRotateSpeed = 1f;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 2;

            settings.MinEndSize = 5;
            settings.MaxEndSize = 20;

            settings.Transparent = true;

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
            ParticleSystemDescription settings = new ParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxDuration = 0.5f;
            settings.MaxDurationRandomness = 1.5f;

            settings.EmitterVelocitySensitivity = 0.1f;

            settings.MinHorizontalVelocity = -0.1f;
            settings.MaxHorizontalVelocity = 0.1f;

            settings.MinVerticalVelocity = -0.1f;
            settings.MaxVerticalVelocity = 0.1f;

            settings.MinColor = Color.Gray;
            settings.MaxColor = Color.White;

            settings.MinRotateSpeed = 1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 0.25f;
            settings.MaxStartSize = 0.5f;

            settings.MinEndSize = 0.5f;
            settings.MaxEndSize = 1.0f;

            settings.Transparent = true;

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
            ParticleSystemDescription settings = new ParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxDuration = 1.5f;
            settings.MaxDurationRandomness = 1;

            settings.MinHorizontalVelocity = 1.0f;
            settings.MaxHorizontalVelocity = 1.5f;

            settings.MinVerticalVelocity = -1f;
            settings.MaxVerticalVelocity = 1f;

            settings.EndVelocity = 0;

            settings.MinColor = Color.DarkGray;
            settings.MaxColor = Color.Gray;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 0.25f;
            settings.MaxStartSize = 0.25f;

            settings.MinEndSize = 5;
            settings.MaxEndSize = 10;

            settings.Transparent = true;

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
            ParticleSystemDescription settings = new ParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxDuration = 4;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 5;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 5;

            settings.Gravity = new Vector3(0, -20, 0);

            settings.EndVelocity = 0;

            settings.MinColor = Color.LightGray;
            settings.MaxColor = Color.White;

            settings.MinRotateSpeed = -2;
            settings.MaxRotateSpeed = 2;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 1;

            settings.MinEndSize = 10;
            settings.MaxEndSize = 20;

            settings.Transparent = true;

            settings.Scale(scale);

            return settings;
        }

        /// <summary>
        /// Particle type
        /// </summary>
        public ParticleSystemTypes ParticleType { get; set; }

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
        public Vector3 Gravity { get; set; }
        /// <summary>
        /// Velocity at end
        /// </summary>
        public float EndVelocity { get; set; }

        /// <summary>
        /// Minimum color variation
        /// </summary>
        public Color MinColor { get; set; }
        /// <summary>
        /// Maximum color variation
        /// </summary>
        public Color MaxColor { get; set; }

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
        /// Gets or sets wheter the particles were transparent
        /// </summary>
        public bool Transparent { get; set; }

        /// <summary>
        /// Emitter velocity sensitivity
        /// </summary>
        public float EmitterVelocitySensitivity { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParticleSystemDescription()
        {
            this.ParticleType = ParticleSystemTypes.None;
            this.ContentPath = "Resources";
            this.TextureName = null;
            this.MaxDuration = 0;
            this.MaxDurationRandomness = 1;
            this.MaxHorizontalVelocity = 0;
            this.MinHorizontalVelocity = 0;
            this.MaxVerticalVelocity = 0;
            this.MinVerticalVelocity = 0;
            this.Gravity = new Vector3(0, -1, 0);
            this.EndVelocity = 1;
            this.MinColor = new Color(1f, 1f, 1f, 1f);
            this.MaxColor = new Color(1f, 1f, 1f, 1f);
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
    }
}
