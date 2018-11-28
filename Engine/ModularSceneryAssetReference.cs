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
        public ModularSceneryAssetTypes Type { get; set; } = ModularSceneryAssetTypes.None;
        /// <summary>
        /// Position
        /// </summary>
        [XmlIgnore]
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
        /// <summary>
        /// Rotation
        /// </summary>
        [XmlIgnore]
        public Quaternion Rotation { get; set; } = new Quaternion(0, 0, 0, 1);
        /// <summary>
        /// Scale
        /// </summary>
        [XmlIgnore]
        public Vector3 Scale { get; set; } = new Vector3(1, 1, 1);

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
                var floats = value?.SplitFloats();
                if (floats?.Length == 3)
                {
                    Position = new Vector3(floats);
                }
                else
                {
                    Position = ModularSceneryExtents.ReadReservedWordsForPosition(value);
                }
            }
        }
        /// <summary>
        /// Rotation quaternion
        /// </summary>
        [XmlElement("rotation")]
        public string RotationText
        {
            get
            {
                return string.Format("{0} {1} {2} {3}", Rotation.X, Rotation.Y, Rotation.Z, Rotation.W);
            }
            set
            {
                var floats = value?.SplitFloats();
                if (floats?.Length == 4)
                {
                    Rotation = new Quaternion(floats);
                }
                else if (floats?.Length == 3)
                {
                    Rotation = Quaternion.RotationYawPitchRoll(floats[0], floats[1], floats[2]);
                }
                else
                {
                    Rotation = ModularSceneryExtents.ReadReservedWordsForRotation(value);
                }
            }
        }
        /// <summary>
        /// Scale vector
        /// </summary>
        [XmlElement("scale")]
        public string ScaleText
        {
            get
            {
                return string.Format("{0} {1} {2}", Scale.X, Scale.Y, Scale.Z);
            }
            set
            {
                var floats = value?.SplitFloats();
                if (floats?.Length == 3)
                {
                    Scale = new Vector3(floats);
                }
                else if (floats?.Length == 1)
                {
                    Scale = new Vector3(floats[0]);
                }
                else
                {
                    Scale = ModularSceneryExtents.ReadReservedWordsForScale(value);
                }
            }
        }

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
            return string.Format("Type: {0}; Id: {1}; AssetName: {2};", Type, Id, AssetName);
        }
    }
}
