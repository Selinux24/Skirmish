using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SharpDX;
using System;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Asset reference
    /// </summary>
    [Serializable]
    public class ModularSceneryAssetReference
    {
        /// <summary>
        /// Asset name
        /// </summary>
        [XmlAttribute("asset_name")]
        public string AssetName { get; set; }
        /// <summary>
        /// Id
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }
        /// <summary>
        /// Type
        /// </summary>
        [XmlAttribute("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ModularSceneryAssetTypes Type { get; set; } = ModularSceneryAssetTypes.None;

        /// <summary>
        /// Position vector
        /// </summary>
        [XmlIgnore]
        public Position3 Position { get; set; } = new Position3(0, 0, 0);
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
        /// Rotation
        /// </summary>
        [XmlIgnore]
        public RotationQ Rotation { get; set; } = new RotationQ(0, 0, 0, 1);
        /// <summary>
        /// Rotation quaternion
        /// </summary>
        [XmlElement("rotation")]
        [JsonIgnore]
        public string RotationText
        {
            get
            {
                return Rotation;
            }
            set
            {
                Rotation = value;
            }
        }

        /// <summary>
        /// Scale
        /// </summary>
        [XmlIgnore]
        public Scale3 Scale { get; set; } = new Scale3(1, 1, 1);
        /// <summary>
        /// Scale vector
        /// </summary>
        [XmlElement("scale")]
        [JsonIgnore]
        public string ScaleText
        {
            get
            {
                return Scale;
            }
            set
            {
                Scale = value;
            }
        }

        /// <summary>
        /// Gets the asset transform
        /// </summary>
        /// <returns>Returns a matrix with the reference transform</returns>
        public Matrix GetTransform()
        {
            return ModularSceneryExtents.Transformation(Position, Rotation, Scale);
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns a string</returns>
        public override string ToString()
        {
            return string.Format("Type: {0}; Id: {1}; AssetName: {2};", Type, Id, AssetName);
        }
    }
}
