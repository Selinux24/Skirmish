using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicInt
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
                this.Value = Collada.Convert<int>(value);
            }
        }
        [XmlIgnore]
        public int Value { get; private set; }

        public BasicInt()
        {

        }

        public override string ToString()
        {
            return string.Format("{0}", this.Value);
        }
    }
}
