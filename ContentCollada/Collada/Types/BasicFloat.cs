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
                return COLLADA.ConvertToString(this.Value);
            }
            set
            {
                this.Value = COLLADA.Convert<float>(value);
            }
        }
        [XmlIgnore]
        public float Value { get; private set; }

        public BasicFloat()
        {

        }

        public BasicFloat(float value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return string.Format("{0}", this.Value);
        }
    }
}
