using System;
using System.Xml.Serialization;

namespace Engine.Collada.FX
{
    [Serializable]
    public class Sampler2D
    {
        [XmlElement("source")]
        public string Source { get; set; }
        [XmlElement("wrap_s")]
        public EnumSamplerWrap WrapS { get; set; }
        [XmlElement("wrap_t")]
        public EnumSamplerWrap WrapT { get; set; }
        [XmlElement("minfilter")]
        public EnumSamplerFilter MinFilter { get; set; }
        [XmlElement("magfilter")]
        public EnumSamplerFilter MagFilter { get; set; }
        [XmlElement("mipfilter")]
        public EnumSamplerFilter MipFilter { get; set; }
        //TODO: To complete -> Color
        [XmlElement("border_color")]
        public string BorderColor { get; set; }
        [XmlElement("mipmap_maxlevel")]
        public byte MipMapMaxLever { get; set; }
        [XmlElement("mipmap_bias")]
        public float MipMapBias { get; set; }

        public Sampler2D()
        {
            this.WrapS = EnumSamplerWrap.Wrap;
            this.WrapT = EnumSamplerWrap.Wrap;
            this.MinFilter = EnumSamplerFilter.None;
            this.MagFilter = EnumSamplerFilter.None;
            this.MipFilter = EnumSamplerFilter.None;
            this.MipMapMaxLever = 255;
            this.MipMapBias = 0;
        }
    }
}
