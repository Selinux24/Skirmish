using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.FX
{
    [Serializable]
    public enum EnumSamplerFilter
    {
        [XmlEnum("NONE")]
        None,
        [XmlEnum("NEAREST")]
        Nearest,
        [XmlEnum("LINEAR")]
        Linear,
        [XmlEnum("NEAREST_MIPMAP_NEAREST")]
        NearestMipmapNearest,
        [XmlEnum("LINEAR_MIPMAP_NEAREST")]
        LinearMipmapNearest,
        [XmlEnum("NEAREST_MIPMAP_LINEAR")]
        NearestMipmapLinear,
        [XmlEnum("LINEAR_MIPMAP_LINEAR")]
        LinearMipmapLinear,
    }
}
