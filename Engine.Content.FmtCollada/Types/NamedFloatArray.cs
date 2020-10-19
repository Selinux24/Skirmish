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
                return Collada.ConvertArrayToString(this.Values);
            }
            set
            {
                this.Values = Collada.ConvertArray<float>(value);
            }
        }
        [XmlIgnore]
        public float[] Values { get; set; }
        [XmlIgnore]
        public float this[int index]
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

        public NamedFloatArray()
        {
            this.Digits = 6;
            this.Magnitude = 38;
        }

        public override string ToString()
        {
            return string.Format("Values: {0}; ", this.Text) + base.ToString();
        }
    }
}
