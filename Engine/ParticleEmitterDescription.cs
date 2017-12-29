using SharpDX;
using System;
using System.Globalization;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Particle emitter description
    /// </summary>
    [Serializable]
    public class ParticleEmitterDescription
    {
        /// <summary>
        /// Particle name
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Position
        /// </summary>
        [XmlIgnore]
        public Vector3 Position { get; set; }
        /// <summary>
        /// Position vector
        /// </summary>
        [XmlElement("position")]
        public string PositionText
        {
            get
            {
                return string.Format("{0} {1} {2}", Position.X, Position.Y, Position.Z);
            }
            set
            {
                var floats = this.Split(value);
                if (floats.Length == 3)
                {
                    Position = new Vector3(floats);
                }
                else
                {
                    Position = new Vector3(0, 0, 0);
                }
            }
        }
        /// <summary>
        /// Velocity
        /// </summary>
        [XmlIgnore]
        public Vector3 Velocity { get; set; }
        /// <summary>
        /// Velocity vector
        /// </summary>
        [XmlElement("velocity")]
        public string VelocityText
        {
            get
            {
                return string.Format("{0} {1} {2}", Velocity.X, Velocity.Y, Velocity.Z);
            }
            set
            {
                var floats = this.Split(value);
                if (floats.Length == 3)
                {
                    Velocity = new Vector3(floats);
                }
                else
                {
                    Velocity = new Vector3(0, 1, 0);
                }
            }
        }
        /// <summary>
        /// Particle scale
        /// </summary>
        [XmlAttribute("scale")]
        public float Scale { get; set; }
        /// <summary>
        /// Emission rate
        /// </summary>
        [XmlAttribute("emissionRate")]
        public float EmissionRate { get; set; }
        /// <summary>
        /// Emitter duration
        /// </summary>
        [XmlAttribute("duration")]
        public float Duration { get; set; }
        /// <summary>
        /// Gets or sets wheter the emitter duration is infinite
        /// </summary>
        [XmlAttribute("infiniteDuration")]
        public bool InfiniteDuration { get; set; }
        /// <summary>
        /// Gets or sets the maximum distance from camera
        /// </summary>
        [XmlAttribute("maximumDistance")]
        public float MaximumDistance { get; set; }
        /// <summary>
        /// Distance from camera
        /// </summary>
        [XmlAttribute("distance")]
        public float Distance { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParticleEmitterDescription()
        {
            this.Position = Vector3.Zero;
            this.Velocity = Vector3.Up;
            this.Scale = 1f;
            this.EmissionRate = 1f;
            this.Duration = 0f;
            this.InfiniteDuration = false;
            this.MaximumDistance = GameEnvironment.LODDistanceLow;
            this.Distance = 0f;
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
                    float n;
                    if (float.TryParse(bits[i], NumberStyles.Float, CultureInfo.InvariantCulture, out n))
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
