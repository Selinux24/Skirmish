using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Particle system description
    /// </summary>
    public class ParticleSystemDescription : DrawableDescription
    {
        /// <summary>
        /// Emitters
        /// </summary>
        private List<ParticleEmitterDescription> emitters = new List<ParticleEmitterDescription>();

        /// <summary>
        /// Gets the particle emitters collection
        /// </summary>
        public ParticleEmitterDescription[] Emitters
        {
            get
            {
                return this.emitters.ToArray();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParticleSystemDescription()
            : base()
        {

        }

        /// <summary>
        /// Creates a fire particle system
        /// </summary>
        /// <param name="emitters">Emitter list</param>
        /// <param name="textures">Texture list</param>
        /// <returns>Returns particle system description</returns>
        public void AddFire(Vector3 position, string contentPath, params string[] textures)
        {
            var emitter = ParticleEmitterDescription.GenerateFire(position, contentPath, textures);

            this.emitters.Add(emitter);
        }
    }

    /// <summary>
    /// Particle emitter description
    /// </summary>
    public class ParticleEmitterDescription
    {
        public static ParticleEmitterDescription GenerateFire(Vector3 position, string contentPath, params string[] textures)
        {
            return new ParticleEmitterDescription()
            {
                EmissionRate = 0.2f,
                EnergyMin = 2f,
                EnergyMax = 2.5f,
                Ellipsoid = false,
                OrbitPosition = true,
                OrbitVelocity = true,
                OrbitAcceleration = false,
                SizeStartMin = 1f,
                SizeStartMax = 1.25f,
                SizeEndMin = 1.25f,
                SizeEndMax = 1.5f,
                ColorStart = new Color4(1f, 0.2f, 0f, 1f),
                ColorStartVar = new Color4(0.25f, 0f, 0f, 0f),
                ColorEnd = new Color4(0, 0, 0, 0),
                ColorEndVar = new Color4(0, 0, 0, 0),
                Position = position,
                PositionVar = Vector3.Zero,
                Velocity = new Vector3(0, 0.1f, 0),
                VelocityVar = new Vector3(0.1f, 0.1f, 0.1f),
                Acceleration = new Vector3(0, 0.1f, 0),
                AccelerationVar = new Vector3(0.1f, 0.1f, 0.1f),

                ContentPath = contentPath,
                Textures = textures,
            };
        }

        public float EmissionRate;
        public float EnergyMin;
        public float EnergyMax;
        public bool Ellipsoid;
        public bool OrbitPosition;
        public bool OrbitVelocity;
        public bool OrbitAcceleration;
        public float SizeStartMin;
        public float SizeStartMax;
        public float SizeEndMin;
        public float SizeEndMax;
        public Color4 ColorStart;
        public Color4 ColorStartVar;
        public Color4 ColorEnd;
        public Color4 ColorEndVar;
        public Vector3 Position;
        public Vector3 PositionVar;
        public Vector3 Velocity;
        public Vector3 VelocityVar;
        public Vector3 Acceleration;
        public Vector3 AccelerationVar;

        public int ParticleCountMax
        {
            get
            {
                return (int)((1f / this.EmissionRate) * this.EnergyMax) * 10;
            }
        }
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
