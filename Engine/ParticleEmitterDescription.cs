using Newtonsoft.Json;
using SharpDX;
using System;
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
        public Position3 Position { get; set; }
        /// <summary>
        /// Position vector
        /// </summary>
        [XmlElement("position")]
        [JsonIgnore]
        public string PositionText
        {
            get
            {
                return Position;
            }
            set
            {
                Position = value;
            }
        }

        /// <summary>
        /// Velocity
        /// </summary>
        [XmlIgnore]
        public Direction3 Velocity { get; set; }
        /// <summary>
        /// Velocity vector
        /// </summary>
        [XmlElement("velocity")]
        [JsonIgnore]
        public string VelocityText
        {
            get
            {
                return Velocity;
            }
            set
            {
                Velocity = value;
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
        /// Gets or sets whether the emitter duration is infinite
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
            Position = Vector3.Zero;
            Velocity = Vector3.Up;
            Scale = 1f;
            EmissionRate = 1f;
            Duration = 0f;
            InfiniteDuration = false;
            MaximumDistance = 100f;
            Distance = 0f;
        }
    }
}
