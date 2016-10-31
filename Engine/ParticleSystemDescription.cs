using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Particle system description
    /// </summary>
    public class ParticleSystemDescription : DrawableDescription
    {
        private List<ParticleEmitterDescription> emitters = new List<ParticleEmitterDescription>();

        /// <summary>
        /// Emitters
        /// </summary>
        public ParticleEmitterDescription[] Emitters
        {
            get
            {
                return this.emitters.ToArray();
            }
            set
            {
                this.emitters.Clear();

                if (value != null && value.Length > 0)
                {
                    this.emitters.AddRange(value);
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParticleSystemDescription()
            : base()
        {

        }

        public void Add(ParticleEmitterDescription desc)
        {
            this.emitters.Add(desc);
        }
    }

    public class ParticleEmitterDescription
    {
        public static ParticleEmitterDescription Fire(string contentPath, params string[] textures)
        {
            return new ParticleEmitterDescription()
            {
                ParticleCountMax = 5000,
                EmissionRate = 300,

                Ellipsoid = true,
                OrbitPosition = true,
                OrbitVelocity = true,
                OrbitAcceleration = false,
                SizeStartMin = 1.5f,
                SizeStartMax = 2f,
                SizeEndMin = 0.5f,
                SizeEndMax = 1f,
                EnergyMin = 750f,
                EnergyMax = 1000f,
                ColorStart = new Color4(1, 0.2f, 0, 1),
                ColorStartVariance = new Color4(0.25f, 0, 0, 0),
                ColorEnd = new Color4(0, 0, 0, 0),
                ColorEndVariance = new Color4(0, 0, 0, 0),
                Position = Vector3.Zero,
                PositionVariance = Vector3.Zero,
                Velocity = new Vector3(0, 10, 0),
                VelocityVariance = new Vector3(0, 10, 0),
                Acceleration = new Vector3(0, 2, 0),
                AccelerationVariance = new Vector3(0, 2, 0),
                RotationParticleSpeedMin = -1.5f,
                RotationParticleSpeedMax = 1.5f,
                RotationSpeedMin = 0f,
                RotationSpeedMax = 0f,
                Angle = 0f,
                RotationAxis = Vector3.Zero,
                RotationAxisVariance = Vector3.Zero,

                Textures = textures,
                ContentPath = contentPath,
            };
        }

        public int ParticleCountMax;
        public int EmissionRate;
        public bool Ellipsoid;
        public bool OrbitPosition;
        public bool OrbitVelocity;
        public bool OrbitAcceleration;

        public Vector3 Position;
        public Vector3 PositionVariance;
        public Vector3 Velocity;
        public Vector3 VelocityVariance;
        public Vector3 Acceleration;
        public Vector3 AccelerationVariance;
        public Color4 ColorStart;
        public Color4 ColorStartVariance;
        public Color4 ColorEnd;
        public Color4 ColorEndVariance;
        public float RotationParticleSpeedMin;
        public float RotationParticleSpeedMax;
        public Vector3 RotationAxis;
        public Vector3 RotationAxisVariance;
        public float RotationSpeedMin;
        public float RotationSpeedMax;
        public float Angle;
        public float EnergyMin;
        public float EnergyMax;
        public float SizeStartMin;
        public float SizeStartMax;
        public float SizeEndMin;
        public float SizeEndMax;

        public Vector3 Translation;
        public Quaternion Rotation;

        /// <summary>
        /// Texture list
        /// </summary>
        public string[] Textures = null;
        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath = "Resources";
    }
}
