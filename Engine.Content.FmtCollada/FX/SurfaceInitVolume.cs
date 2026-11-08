using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.FX
{
    using Engine.Content.FmtCollada.Types;

    [Serializable]
    public class SurfaceInitVolume
    {
        [XmlElement("all", typeof(BasicIdRef))]
        public BasicIdRef All { get; set; }
        [XmlElement("primary", typeof(BasicIdRef))]
        public BasicIdRef Primary { get; set; }
    }
}
