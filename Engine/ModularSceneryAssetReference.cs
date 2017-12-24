using SharpDX;
using System;
using System.Globalization;
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
        public string AssetName;
        /// <summary>
        /// Asset type
        /// </summary>
        [XmlAttribute("type")]
        public ModularSceneryAssetTypeEnum Type = ModularSceneryAssetTypeEnum.None;
        /// <summary>
        /// Position
        /// </summary>
        [XmlIgnore]
        public Vector3 Position = new Vector3(0, 0, 0);
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
                    Position = ReadReservedWordsForPosition(value);
                }
            }
        }
        /// <summary>
        /// Rotation
        /// </summary>
        [XmlIgnore]
        public Quaternion Rotation = new Quaternion(0, 0, 0, 1);
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
                var floats = this.Split(value);
                if (floats.Length == 4)
                {
                    Rotation = new Quaternion(floats);
                }
                else
                {
                    Rotation = ReadReservedWordsForRotation(value);
                }
            }
        }
        /// <summary>
        /// Scale
        /// </summary>
        [XmlIgnore]
        public Vector3 Scale = new Vector3(1, 1, 1);
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
                var floats = this.Split(value);
                if (floats.Length == 3)
                {
                    Scale = new Vector3(floats);
                }
                else if (floats.Length == 1)
                {
                    Scale = new Vector3(floats[0]);
                }
                else
                {
                    Scale = ReadReservedWordsForScale(value);
                }
            }
        }

        /// <summary>
        /// Gets the asset transform
        /// </summary>
        /// <returns>Returns a matrix with the reference transform</returns>
        public Matrix GetTransform()
        {
            return Matrix.Transformation(
                Vector3.Zero, Quaternion.Identity, this.Scale,
                Vector3.Zero, this.Rotation,
                this.Position);
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
        /// <summary>
        /// Parse value for position reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Position</returns>
        private Vector3 ReadReservedWordsForPosition(string value)
        {
            if (string.Equals(value, "Zero", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.Zero;
            }
            else
            {
                return Vector3.Zero;
            }
        }
        /// <summary>
        /// Parse value for rotation reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed rotation</returns>
        private Quaternion ReadReservedWordsForRotation(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (string.Equals(value, "Identity", StringComparison.OrdinalIgnoreCase))
                {
                    return Quaternion.Identity;
                }
                else if (value.StartsWith("Rot", StringComparison.OrdinalIgnoreCase))
                {
                    var degrees = value.Substring(3);

                    float d;
                    if (float.TryParse(degrees, NumberStyles.Float, CultureInfo.InvariantCulture, out d))
                    {
                        return Quaternion.RotationAxis(Vector3.Up, MathUtil.DegreesToRadians(d));
                    }
                }
            }

            return Quaternion.Identity;
        }
        /// <summary>
        /// Parse value for scale reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed scale</returns>
        private Vector3 ReadReservedWordsForScale(string value)
        {
            if (string.Equals(value, "One", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.One;
            }
            else if (string.Equals(value, "Two", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.One * 2f;
            }
            else if (string.Equals(value, "1/5", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.One / 5f;
            }
            else if (string.Equals(value, "1/4", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.One / 4f;
            }
            else if (string.Equals(value, "1/3", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.One / 3f;
            }
            else if (string.Equals(value, "1/2", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.One / 2f;
            }
            else
            {
                return Vector3.One;
            }
        }
    }
}
