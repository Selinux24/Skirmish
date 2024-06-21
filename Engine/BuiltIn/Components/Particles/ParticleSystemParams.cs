using SharpDX;

namespace Engine.BuiltIn.Components.Particles
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
            if (MathUtil.IsOne(scale))
            {
                return p;
            }

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
            readonly get
            {
                return maxHorizontalVelocity;
            }
            set
            {
                maxHorizontalVelocity = value;

                HorizontalVelocity = new Vector2(minHorizontalVelocity, maxHorizontalVelocity);
            }
        }
        /// <summary>
        /// Minimum horizontal velocity
        /// </summary>
        public float MinHorizontalVelocity
        {
            readonly get
            {
                return minHorizontalVelocity;
            }
            set
            {
                minHorizontalVelocity = value;

                HorizontalVelocity = new Vector2(minHorizontalVelocity, maxHorizontalVelocity);
            }
        }

        /// <summary>
        /// Maximum vertical velocity
        /// </summary>
        public float MaxVerticalVelocity
        {
            readonly get
            {
                return maxVerticalVelocity;
            }
            set
            {
                maxVerticalVelocity = value;

                VerticalVelocity = new Vector2(minVerticalVelocity, maxVerticalVelocity);
            }
        }
        /// <summary>
        /// Minimum vertical velocity
        /// </summary>
        public float MinVerticalVelocity
        {
            readonly get
            {
                return minVerticalVelocity;
            }
            set
            {
                minVerticalVelocity = value;

                VerticalVelocity = new Vector2(minVerticalVelocity, maxVerticalVelocity);
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
            readonly get
            {
                return minRotateSpeed;
            }
            set
            {
                minRotateSpeed = value;

                RotateSpeed = new Vector2(minRotateSpeed, maxRotateSpeed);
            }
        }
        /// <summary>
        /// Maximum rotation speed
        /// </summary>
        public float MaxRotateSpeed
        {
            readonly get
            {
                return maxRotateSpeed;
            }
            set
            {
                maxRotateSpeed = value;

                RotateSpeed = new Vector2(minRotateSpeed, maxRotateSpeed);
            }
        }

        /// <summary>
        /// Minimum starting size
        /// </summary>
        public float MinStartSize
        {
            readonly get
            {
                return minStartSize;
            }
            set
            {
                minStartSize = value;

                StartSize = new Vector2(minStartSize, maxStartSize);
            }
        }
        /// <summary>
        /// Maximum starting size
        /// </summary>
        public float MaxStartSize
        {
            readonly get
            {
                return maxStartSize;
            }
            set
            {
                maxStartSize = value;

                StartSize = new Vector2(minStartSize, maxStartSize);
            }
        }

        /// <summary>
        /// Minimum ending size
        /// </summary>
        public float MinEndSize
        {
            readonly get
            {
                return minEndSize;
            }
            set
            {
                minEndSize = value;

                EndSize = new Vector2(minEndSize, maxEndSize);
            }
        }
        /// <summary>
        /// Maximum ending size
        /// </summary>
        public float MaxEndSize
        {
            readonly get
            {
                return maxEndSize;
            }
            set
            {
                maxEndSize = value;

                EndSize = new Vector2(minEndSize, maxEndSize);
            }
        }

        /// <summary>
        /// Gets or sets whether the blend mode
        /// </summary>
        public BlendModes BlendMode { get; set; }

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
            minHorizontalVelocity = 0f;
            maxHorizontalVelocity = 0f;
            minVerticalVelocity = 0f;
            maxVerticalVelocity = 0f;
            minRotateSpeed = 0f;
            maxRotateSpeed = 0f;
            minStartSize = 0f;
            maxStartSize = 0f;
            minEndSize = 0f;
            maxEndSize = 0f;

            HorizontalVelocity = Vector2.Zero;
            VerticalVelocity = Vector2.Zero;
            RotateSpeed = Vector2.Zero;
            StartSize = Vector2.Zero;
            EndSize = Vector2.Zero;

            MaxDuration = description.MaxDuration;
            MaxDurationRandomness = description.MaxDurationRandomness;

            Gravity = description.Gravity;
            EndVelocity = description.EndVelocity;

            MinColor = description.MinColor;
            MaxColor = description.MaxColor;

            BlendMode = description.BlendMode;

            EmitterVelocitySensitivity = description.EmitterVelocitySensitivity;

            MinHorizontalVelocity = description.MinHorizontalVelocity;
            MaxHorizontalVelocity = description.MaxHorizontalVelocity;
            MinVerticalVelocity = description.MinVerticalVelocity;
            MaxVerticalVelocity = description.MaxVerticalVelocity;
            MinRotateSpeed = description.MinRotateSpeed;
            MaxRotateSpeed = description.MaxRotateSpeed;
            MinStartSize = description.MinStartSize;
            MaxStartSize = description.MaxStartSize;
            MinEndSize = description.MinEndSize;
            MaxEndSize = description.MaxEndSize;
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
