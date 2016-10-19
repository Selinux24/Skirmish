using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class NamedNameArray : NamedArray
    {
        [XmlText]
        public string Text
        {
            get
            {
                return COLLADA.ConvertArrayToString(this.Values);
            }
            set
            {
                this.Values = COLLADA.ConvertArray<string>(value);
            }
        }
        [XmlIgnore]
        public string[] Values { get; set; }
        [XmlIgnore]
        public string this[int index]
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
