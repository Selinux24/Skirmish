using SharpDX;

namespace Engine
{
    /// <summary>
    /// Particle system description
    /// </summary>
    public class CPUParticleSystemDescription
    {
        /// <summary>
        /// Initializes dust particle systems
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Texture</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static CPUParticleSystemDescription InitializeDust(string contentPath, string texture)
        {
            CPUParticleSystemDescription settings = new CPUParticleSystemDescription();

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

            return settings;
        }
        /// <summary>
        /// Initializes fire particle systems
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Texture</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static CPUParticleSystemDescription InitializeFire(string contentPath, string texture)
        {
            CPUParticleSystemDescription settings = new CPUParticleSystemDescription();

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

            return settings;
        }
        /// <summary>
        /// Initializes smoke plume particle systems
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Texture</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static CPUParticleSystemDescription InitializeSmokePlume(string contentPath, string texture)
        {
            CPUParticleSystemDescription settings = new CPUParticleSystemDescription();

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

            return settings;
        }
        /// <summary>
        /// Initializes porjectile trail particle systems
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Texture</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static CPUParticleSystemDescription InitializeProjectileTrail(string contentPath, string texture)
        {
            CPUParticleSystemDescription settings = new CPUParticleSystemDescription();

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

            return settings;
        }


        /// <summary>
        /// Initializes explosion particle systems
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Texture</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static CPUParticleSystemDescription InitializeExplosion(string contentPath, string texture)
        {
            CPUParticleSystemDescription settings = new CPUParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxDuration = 2;
            settings.MaxDurationRandomness = 1;

            settings.MinHorizontalVelocity = 20;
            settings.MaxHorizontalVelocity = 30;

            settings.MinVerticalVelocity = -20;
            settings.MaxVerticalVelocity = 20;

            settings.EndVelocity = 0;

            settings.MinColor = Color.DarkGray;
            settings.MaxColor = Color.Gray;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 100;
            settings.MaxEndSize = 200;

            settings.Transparent = true;

            return settings;
        }
        /// <summary>
        /// Initializes explosion with smoke particle systems
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Texture</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static CPUParticleSystemDescription InitializeExplosionSmoke(string contentPath, string texture)
        {
            CPUParticleSystemDescription settings = new CPUParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxDuration = 4;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 50;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 50;

            settings.Gravity = new Vector3(0, -20, 0);

            settings.EndVelocity = 0;

            settings.MinColor = Color.LightGray;
            settings.MaxColor = Color.White;

            settings.MinRotateSpeed = -2;
            settings.MaxRotateSpeed = 2;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 100;
            settings.MaxEndSize = 200;

            return settings;
        }
        /// <summary>
        /// Initializes plasma engine particle systems
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Texture</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static CPUParticleSystemDescription InitializePlasmaEngine(string contentPath, string texture)
        {
            CPUParticleSystemDescription settings = new CPUParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxDuration = 0.5f;
            settings.MaxDurationRandomness = 0f;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 0;

            settings.Gravity = new Vector3(0, 0, 0);

            settings.MinColor = Color.AliceBlue;
            settings.MaxColor = Color.LightBlue;

            settings.MinStartSize = 1f;
            settings.MaxStartSize = 1f;

            settings.MinEndSize = 0.1f;
            settings.MaxEndSize = 0.1f;

            settings.Transparent = true;

            return settings;
        }
        /// <summary>
        /// Initializes smoke engine particle systems
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Texture</param>
        /// <returns>Returns the new generated particle system description</returns>
        public static CPUParticleSystemDescription InitializeSmokeEngine(string contentPath, string texture)
        {
            CPUParticleSystemDescription settings = new CPUParticleSystemDescription();

            settings.ContentPath = contentPath;
            settings.TextureName = texture;

            settings.MaxDuration = 1;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 2;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 2;

            settings.Gravity = new Vector3(-1, -1, 0);

            settings.EndVelocity = 0.15f;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 2;

            settings.MinEndSize = 2;
            settings.MaxEndSize = 4;

            return settings;
        }

        /// <summary>
        /// Particle type
        /// </summary>
        public CPUParticleSystemTypes ParticleType { get; set; }

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
        public CPUParticleSystemDescription()
        {
            this.ParticleType = CPUParticleSystemTypes.None;
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
    }
}
