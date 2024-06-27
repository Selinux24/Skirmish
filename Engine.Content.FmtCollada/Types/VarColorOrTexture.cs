using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public class VarColorOrTexture
    {
        [XmlElement("color", typeof(BasicColor))]
        public BasicColor Color { get; set; }
        [XmlElement("texture", typeof(BasicTexture))]
        public BasicTexture Texture { get; set; }
        [XmlElement("param", typeof(BasicParam))]
        public BasicParam Param { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return
                Color?.ToString() ??
                Texture?.ToString() ??
                Param?.ToString() ??
                "Empty";
        }
    }
}
