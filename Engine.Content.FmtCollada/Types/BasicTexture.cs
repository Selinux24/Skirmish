using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.Types
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Texture: {Texture}; TextureCoordinate: {TextureCoordinate};";
        }
    }
}
