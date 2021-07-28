using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SharpDX;
using System;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Connection between assets
    /// </summary>
    [Serializable]
    public class ModularSceneryAssetDescriptionConnection
    {
        /// <summary>
        /// Connection type
        /// </summary>
        [XmlAttribute("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ModularSceneryAssetDescriptionConnectionTypes Type { get; set; } = ModularSceneryAssetDescriptionConnectionTypes.None;

        /// <summary>
        /// Position
        /// </summary>
        [XmlIgnore]
        public Position3 Position { get; set; } = new Position3(0, 0, 0);
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
        /// Direction
        /// </summary>
        [XmlIgnore]
        public Direction3 Direction { get; set; } = new Direction3(0, 0, 0);
        /// <summary>
        /// Direction vector
        /// </summary>
        [XmlElement("direction")]
        [JsonIgnore]
        public string DirectionText
        {
            get
            {
                return Direction;
            }
            set
            {
                Direction = value;
            }
        }
    }
}
