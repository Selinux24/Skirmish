using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicFloat
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
                this.Value = Collada.Convert<float>(value);
            }
        }
        [XmlIgnore]
        public float Value { get; set; }

        public BasicFloat()
        {

        }

        public override string ToString()
        {
            return string.Format("{0}", this.Value);
        }
    }
}
