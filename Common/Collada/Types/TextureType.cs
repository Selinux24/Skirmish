using System;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public class TextureType
    {
        [XmlAttribute("texture")]
        public string Texture { get; set; }
        [XmlAttribute("texcoord")]
        public string TextureCoordinates { get; set; }
    }
}
