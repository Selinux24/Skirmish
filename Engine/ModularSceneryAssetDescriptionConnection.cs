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
                var floats = ModularSceneryExtents.Split(value);
                if (floats.Length == 3)
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
                var floats = ModularSceneryExtents.Split(value);
                if (floats.Length == 3)
                {
                    Direction = new Vector3(floats);
                }
                else
                {
                    Direction = ModularSceneryExtents.ReadReservedWordsForDirection(value);
                }
            }
        }
    }
}
