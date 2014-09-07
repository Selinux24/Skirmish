using System;
using System.Xml.Serialization;
using SharpDX;

namespace Common.Collada.Types
{
    [Serializable]
    public class ColorType
    {
        [XmlElement("color")]
        public string ValueText
        {
            get { return Dae.Convert(this.Value); }
            set { this.Value = Dae.ConvertColor4(value); }
        }
        [XmlIgnore]
        public Color4 Value { get; set; }
    }
}
