using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SharpDX;
using System;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Object reference
    /// </summary>
    [Serializable]
    public class ModularSceneryObjectReference
    {
        /// <summary>
        /// Asset map id
        /// </summary>
        [XmlAttribute("asset_map_id")]
        public string AssetMapId { get; set; }
        /// <summary>
        /// Asset id
        /// </summary>
        [XmlAttribute("asset_id")]
        public string AssetId { get; set; }
        /// <summary>
        /// Item id
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }
        /// <summary>
        /// Item name
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
        /// <summary>
        /// Asset name
        /// </summary>
        [XmlAttribute("asset_name")]
        public string AssetName { get; set; }
        /// <summary>
        /// Item type
        /// </summary>
        [XmlAttribute("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ModularSceneryObjectTypes Type { get; set; } = ModularSceneryObjectTypes.Default;
        /// <summary>
        /// Include object in path finding
        /// </summary>
        [XmlAttribute("path_finding")]
        public bool PathFinding { get; set; } = false;
        /// <summary>
        /// Animation plan list
        /// </summary>
        [XmlArray("animation_plans")]
        [XmlArrayItem("plan", typeof(ModularSceneryObjectAnimationPlan))]
        public ModularSceneryObjectAnimationPlan[] AnimationPlans { get; set; } = null;
        /// <summary>
        /// Action list
        /// </summary>
        [XmlArray("actions")]
        [XmlArrayItem("action", typeof(ModularSceneryObjectAction))]
        public ModularSceneryObjectAction[] Actions { get; set; } = null;
        /// <summary>
        /// States list
        /// </summary>
        [XmlArray("states")]
        [XmlArrayItem("state", typeof(ModularSceneryObjectState))]
        public ModularSceneryObjectState[] States { get; set; } = null;
        /// <summary>
        /// Next level
        /// </summary>
        /// <remarks>Only for exit doors</remarks>
        [XmlAttribute("next_level")]
        public string NextLevel { get; set; }

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
        /// Load model lights into scene
        /// </summary>
        [XmlAttribute("load_lights")]
        public bool LoadLights { get; set; } = true;
        /// <summary>
        /// Lights cast shadows
        /// </summary>
        [XmlAttribute("cast_shadows")]
        public bool CastShadows { get; set; } = true;
        /// <summary>
        /// Particle
        /// </summary>
        [XmlElement("particleLight", Type = typeof(ParticleEmitterDescription))]
        public ParticleEmitterDescription ParticleLight { get; set; }

        /// <summary>
        /// Gets the asset transform
        /// </summary>
        /// <returns>Returns a matrix with the reference transform</returns>
        public Matrix GetTransform()
        {
            return ModularSceneryExtents.Transformation(this.Position, this.Rotation, this.Scale);
        }
        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns a string</returns>
        public override string ToString()
        {
            return string.Format("Type: {0}; Id: {1}; AssetName: {2}; AssetMapId: {3}; AssetId: {4};", Type, Id, AssetName, AssetMapId, AssetId);
        }
    }
}
