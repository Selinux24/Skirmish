using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class NamedBoolArray : NamedArray
    {
        [XmlText]
        public string Text
        {
            get
            {
                return COLLADA.ConvertToString(this.Values);
            }
            set
            {
                this.Values = COLLADA.ConvertArray<bool>(value);
            }
        }
        [XmlIgnore]
        public bool[] Values { get; set; }
        [XmlIgnore]
        public bool this[int index]
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

        public override string ToString()
        {
            return string.Format("Values: {0}; ", this.Text) + base.ToString();
        }
    }
}
