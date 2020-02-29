using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicBool
    {
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlText]
        public string Text
        {
            get
            {
                return Collada.ConvertToString(this.Value);
            }
            set
            {
                this.Value = Collada.Convert<bool>(value);
            }
        }
        [XmlIgnore]
        public bool Value { get; set; }

        public BasicBool()
        {

        }

        public override string ToString()
        {
            return string.Format("{0}", this.Value);
        }
    }
}
