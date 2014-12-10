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

        public override string ToString()
        {
            if (this.Color != null) return this.Color.ToString();
            else if (this.Texture != null) return this.Texture.ToString();
            else if (this.Param != null) return this.Param.ToString();
            else return "Empty";
        }
    }
}
