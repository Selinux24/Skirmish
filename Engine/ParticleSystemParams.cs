using SharpDX;

namespace Engine
{
    /// <summary>
    /// Particle system dynamic parameters
    /// </summary>
    public struct ParticleSystemParams
    {
        /// <summary>
        /// Scale operator
        /// </summary>
        /// <param name="p">Particle parameters</param>
        /// <param name="scale">Scale</param>
        /// <returns>Returns a new particle system parameters set scaled to the specified value</returns>
        public static ParticleSystemParams operator *(ParticleSystemParams p, float scale)
        {
            if (scale != 1f)
            {
                p.MaxHorizontalVelocity *= scale;
                p.MinHorizontalVelocity *= scale;
                p.MaxVerticalVelocity *= scale;
                p.MinVerticalVelocity *= scale;

                p.Gravity *= scale;
                p.EndVelocity *= scale;

                p.MinStartSize *= scale;
                p.MaxStartSize *= scale;
                p.MinEndSize *= scale;
                p.MaxEndSize *= scale;
            }

            return p;
        }

        /// <summary>
        /// Maximum horizontal velocity
        /// </summary>
        private float maxHorizontalVelocity;
        /// <summary>
        /// Minimum horizontal velocity
        /// </summary>
        private float minHorizontalVelocity;
        /// <summary>
        /// Maximum vertical velocity
        /// </summary>
        private float maxVerticalVelocity;
        /// <summary>
        /// Minimum vertical velocity
        /// </summary>
        private float minVerticalVelocity;
        /// <summary>
        /// Minimum rotation speed
        /// </summary>
        private float minRotateSpeed;
        /// <summary>
        /// Maximum rotation speed
        /// </summary>
        private float maxRotateSpeed;
        /// <summary>
        /// Minimum starting size
        /// </summary>
        private float minStartSize;
        /// <summary>
        /// Maximum starting size
        /// </summary>
        private float maxStartSize;
        /// <summary>
        /// Minimum ending size
        /// </summary>
        private float minEndSize;
        /// <summary>
        /// Maximum ending size
        /// </summary>
        private float maxEndSize;

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
        public float MaxHorizontalVelocity
        {
            get
            {
                return this.maxHorizontalVelocity;
            }
            set
            {
                this.maxHorizontalVelocity = value;

                this.HorizontalVelocity = new Vector2(this.minHorizontalVelocity, this.maxHorizontalVelocity);
            }
        }
        /// <summary>
        /// Minimum horizontal velocity
        /// </summary>
        public float MinHorizontalVelocity
        {
            get
            {
                return this.minHorizontalVelocity;
            }
            set
            {
                this.minHorizontalVelocity = value;

                this.HorizontalVelocity = new Vector2(this.minHorizontalVelocity, this.maxHorizontalVelocity);
            }
        }

        /// <summary>
        /// Maximum vertical velocity
        /// </summary>
        public float MaxVerticalVelocity
        {
            get
            {
                return this.maxVerticalVelocity;
            }
            set
            {
                this.maxVerticalVelocity = value;

                this.VerticalVelocity = new Vector2(this.minVerticalVelocity, this.maxVerticalVelocity);
            }
        }
        /// <summary>
        /// Minimum vertical velocity
        /// </summary>
        public float MinVerticalVelocity
        {
            get
            {
                return this.minVerticalVelocity;
            }
            set
            {
                this.minVerticalVelocity = value;

                this.VerticalVelocity = new Vector2(this.minVerticalVelocity, this.maxVerticalVelocity);
            }
        }

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
        public float MinRotateSpeed
        {
            get
            {
                return this.minRotateSpeed;
            }
            set
            {
                this.minRotateSpeed = value;

                this.RotateSpeed = new Vector2(this.minRotateSpeed, this.maxRotateSpeed);
            }
        }
        /// <summary>
        /// Maximum rotation speed
        /// </summary>
        public float MaxRotateSpeed
        {
            get
            {
                return this.maxRotateSpeed;
            }
            set
            {
                this.maxRotateSpeed = value;

                this.RotateSpeed = new Vector2(this.minRotateSpeed, this.maxRotateSpeed);
            }
        }

        /// <summary>
        /// Minimum starting size
        /// </summary>
        public float MinStartSize
        {
            get
            {
                return this.minStartSize;
            }
            set
            {
                this.minStartSize = value;

                this.StartSize = new Vector2(this.minStartSize, this.maxStartSize);
            }
        }
        /// <summary>
        /// Maximum starting size
        /// </summary>
        public float MaxStartSize
        {
            get
            {
                return this.maxStartSize;
            }
            set
            {
                this.maxStartSize = value;

                this.StartSize = new Vector2(this.minStartSize, this.maxStartSize);
            }
        }

        /// <summary>
        /// Minimum ending size
        /// </summary>
        public float MinEndSize
        {
            get
            {
                return this.minEndSize;
            }
            set
            {
                this.minEndSize = value;

                this.EndSize = new Vector2(this.minEndSize, this.maxEndSize);
            }
        }
        /// <summary>
        /// Maximum ending size
        /// </summary>
        public float MaxEndSize
        {
            get
            {
                return this.maxEndSize;
            }
            set
            {
                this.maxEndSize = value;

                this.EndSize = new Vector2(this.minEndSize, this.maxEndSize);
            }
        }

        /// <summary>
        /// Gets or sets wheter the particles were transparent
        /// </summary>
        public bool Transparent { get; set; }
        /// <summary>
        /// Gets or sets wheter the particles were additive
        /// </summary>
        public bool Additive { get; set; }

        /// <summary>
        /// Emitter velocity sensitivity
        /// </summary>
        public float EmitterVelocitySensitivity { get; set; }

        /// <summary>
        /// Gets the horizontal velocity vector
        /// </summary>
        public Vector2 HorizontalVelocity { get; private set; }
        /// <summary>
        /// Gets the vertical velocity vector
        /// </summary>
        public Vector2 VerticalVelocity { get; private set; }

        /// <summary>
        /// Gets the start size vector
        /// </summary>
        public Vector2 StartSize { get; private set; }
        /// <summary>
        /// Gets the end size vector
        /// </summary>
        public Vector2 EndSize { get; private set; }

        /// <summary>
        /// Gets the rotation speed vector
        /// </summary>
        public Vector2 RotateSpeed { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description">Particle system description</param>
        public ParticleSystemParams(ParticleSystemDescription description) : this()
        {
            this.minHorizontalVelocity = 0f;
            this.maxHorizontalVelocity = 0f;
            this.minVerticalVelocity = 0f;
            this.maxVerticalVelocity = 0f;
            this.minRotateSpeed = 0f;
            this.maxRotateSpeed = 0f;
            this.minStartSize = 0f;
            this.maxStartSize = 0f;
            this.minEndSize = 0f;
            this.maxEndSize = 0f;

            this.HorizontalVelocity = Vector2.Zero;
            this.VerticalVelocity = Vector2.Zero;
            this.RotateSpeed = Vector2.Zero;
            this.StartSize = Vector2.Zero;
            this.EndSize = Vector2.Zero;

            this.MaxDuration = description.MaxDuration;
            this.MaxDurationRandomness = description.MaxDurationRandomness;

            this.Gravity = description.Gravity;
            this.EndVelocity = description.EndVelocity;

            this.MinColor = description.MinColor;
            this.MaxColor = description.MaxColor;

            this.Transparent = description.Transparent;
            this.Additive = description.Additive;

            this.EmitterVelocitySensitivity = description.EmitterVelocitySensitivity;

            this.MinHorizontalVelocity = description.MinHorizontalVelocity;
            this.MaxHorizontalVelocity = description.MaxHorizontalVelocity;
            this.MinVerticalVelocity = description.MinVerticalVelocity;
            this.MaxVerticalVelocity = description.MaxVerticalVelocity;
            this.MinRotateSpeed = description.MinRotateSpeed;
            this.MaxRotateSpeed = description.MaxRotateSpeed;
            this.MinStartSize = description.MinStartSize;
            this.MaxStartSize = description.MaxStartSize;
            this.MinEndSize = description.MinEndSize;
            this.MaxEndSize = description.MaxEndSize;
        }

        /// <summary>
        /// Scales the particle system
        /// </summary>
        /// <param name="scale">Scale to apply</param>
        public void Scale(float scale)
        {
            this *= scale;
        }
    }
}
