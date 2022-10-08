using System;
using System.Xml.Serialization;

namespace Engine.Collada.FX
{
    using Engine.Collada.Types;

    [Serializable]
    public class SurfaceInitVolume
    {
        [XmlElement("all", typeof(BasicIdRef))]
        public BasicIdRef All { get; set; }
        [XmlElement("primary", typeof(BasicIdRef))]
        public BasicIdRef Primary { get; set; }
    }
}
