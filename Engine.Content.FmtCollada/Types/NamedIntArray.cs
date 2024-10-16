using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.Types
{
    [Serializable]
    public class NamedIntArray : NamedArray
    {
        [XmlAttribute("minInclusive")]
        public int MinInclusive { get; set; }
        [XmlAttribute("maxInclusive")]
        public int MaxInclusive { get; set; }
        [XmlText]
        public string Text
        {
            get
            {
                return Collada.ConvertArrayToString(Values);
            }
            set
            {
                Values = Collada.ConvertArray<int>(value);
            }
        }
        [XmlIgnore]
        public int[] Values { get; set; }
        [XmlIgnore]
        public int this[int index]
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

        public NamedIntArray()
        {
            MinInclusive = -2147483648;
            MaxInclusive = 2147483647;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return base.ToString() + $"Values: {Text};";
        }
    }
}
