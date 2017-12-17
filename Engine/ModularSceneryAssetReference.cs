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

                float[] res = new float[bits.Length];

                for (int i = 0; i < res.Length; i++)
                {
                    res[i] = float.Parse(bits[i], NumberStyles.Float, CultureInfo.InvariantCulture);
                }

                return res;
            }

            return new float[] { };
        }
    }
}
