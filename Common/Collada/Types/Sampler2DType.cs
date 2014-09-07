using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public class Sampler2DType
    {
        [XmlElement("source", DataType = "NCName")]
        public string Source { get; set; }
        [XmlElement("wrap_s")]
        [DefaultValue(SamplerWrapEnum.WRAP)]
        public SamplerWrapEnum WrapS { get; set; }
        [XmlElement("wrap_t")]
        [DefaultValue(SamplerWrapEnum.WRAP)]
        public SamplerWrapEnum WrapT { get; set; }
        [XmlElement("minfilter")]
        [DefaultValue(SamplerFilterEnum.NONE)]
        public SamplerFilterEnum MinFilter { get; set; }
        [XmlElement("magfilter")]
        [DefaultValue(SamplerFilterEnum.NONE)]
        public SamplerFilterEnum MagFilter { get; set; }
        [XmlElement("mipfilter")]
        [DefaultValue(SamplerFilterEnum.NONE)]
        public SamplerFilterEnum MipFilter { get; set; }

        [XmlElement("border_color")]
        public string BorderColor { get; set; }
        [XmlElement("mipmap_maxlevel")]
        [DefaultValue(typeof(byte), "255")]
        public byte MipMapMaxLevel { get; set; }
        [XmlElement("mipmap_bias")]
        [DefaultValue(typeof(float), "0")]
        public float MipMapBias { get; set; }
    }
}
