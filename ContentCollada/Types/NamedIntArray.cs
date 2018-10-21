using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
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
                return Collada.ConvertArrayToString(this.Values);
            }
            set
            {
                this.Values = Collada.ConvertArray<int>(value);
            }
        }
        [XmlIgnore]
        public int[] Values { get; set; }
        [XmlIgnore]
        public int this[int index]
        {
            get
            {
                return this.Values[index];
            }
            set
            {
                this.Values[index] = value;
            }
        }

        public NamedIntArray()
        {
            this.MinInclusive = -2147483648;
            this.MaxInclusive = 2147483647;
        }

        public override string ToString()
        {
            return string.Format("Values: {0}; ", this.Text) + base.ToString();
        }
    }
}
