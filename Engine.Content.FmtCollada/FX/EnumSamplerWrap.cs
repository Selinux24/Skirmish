﻿using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.FX
{
    [Serializable]
    public enum EnumSamplerWrap
    {
        [XmlEnum("NONE")]
        None,
        [XmlEnum("WRAP")]
        Wrap,
        [XmlEnum("MIRROR")]
        Mirror,
        [XmlEnum("CLAMP")]
        Clamp,
        [XmlEnum("BORDER")]
        Border,
    }
}
