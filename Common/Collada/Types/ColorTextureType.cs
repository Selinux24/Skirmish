using System;
using System.Xml.Serialization;
using SharpDX;

namespace Common.Collada.Types
{
    [Serializable]
    public class ColorTextureType
    {
        [XmlElement("color")]
        public string ColorText
        {
            get { return Dae.Convert(this.Color); }
            set { this.Color = Dae.ConvertColor4(value); }
        }
        [XmlIgnore]
        public Color4 Color { get; set; }
        [XmlElement("texture")]
        public TextureType Texture { get; set; }
    }
}
