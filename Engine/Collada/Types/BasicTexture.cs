using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class BasicTexture
    {
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlAttribute("texture")]
        public string Texture { get; set; }
        [XmlAttribute("texcoord")]
        public string TextureCoordinate { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra Extra { get; set; }

        public override string ToString()
        {
            return string.Format("Texture: {0}; TextureCoordinate: {1};", this.Texture, this.TextureCoordinate);
        }
    }
}
