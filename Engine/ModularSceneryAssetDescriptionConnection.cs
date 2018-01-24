using SharpDX;
using System;
using System.Globalization;
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
        /// Direction
        /// </summary>
        [XmlIgnore]
        public Vector3 Direction = new Vector3(0, 0, 0);
        /// <summary>
        /// Direction vector
        /// </summary>
        [XmlElement("direction")]
        public string DirectionText
        {
            get
            {
                return string.Format("{0} {1} {2}", Direction.X, Direction.Y, Direction.Z);
            }
            set
            {
                var floats = this.Split(value);
                if (floats.Length == 3)
                {
                    Direction = new Vector3(floats);
                }
                else
                {
                    Direction = ReadReservedWordsForDirection(value);
                }
            }
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
        /// Parse value for position reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Position</returns>
        private Vector3 ReadReservedWordsForDirection(string value)
        {
            if (string.Equals(value, "Up", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.Up;
            }
            else if (string.Equals(value, "Down", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.Down;
            }
            else if (string.Equals(value, "Forward", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.ForwardLH;
            }
            else if (string.Equals(value, "Backward", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.BackwardLH;
            }
            else if (string.Equals(value, "Left", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.Left;
            }
            else if (string.Equals(value, "Right", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.Right;
            }
            else
            {
                return Vector3.Zero;
            }
        }
    }
}
