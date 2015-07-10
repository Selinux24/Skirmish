using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicBoolArray
    {
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlText]
        public string Text
        {
            get
            {
                return COLLADA.ConvertArrayToString(this.Values);
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
            return string.Format("Count: {0};", this.Values != null ? this.Values.Length : 0);
        }
    }
}
