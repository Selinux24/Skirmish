using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class NamedFloatArray : NamedArray
    {
        [XmlAttribute("digits")]
        public int Digits { get; set; }
        [XmlAttribute("magnitude")]
        public int Magnitude { get; set; }
        [XmlText]
        public string Text
        {
            get
            {
                return Collada.ConvertArrayToString(Values);
            }
            set
            {
                Values = Collada.ConvertArray<float>(value);
            }
        }
        [XmlIgnore]
        public float[] Values { get; set; }
        [XmlIgnore]
        public float this[int index]
        {
            get
            {
                return Values[index];
            }
            set
            {
                Values[index] = value;
            }
        }

        public NamedFloatArray()
        {
            Digits = 6;
            Magnitude = 38;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return base.ToString() + $" Values: {Text};";
        }
    }
}
