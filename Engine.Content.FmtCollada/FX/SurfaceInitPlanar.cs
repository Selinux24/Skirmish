using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.FX
{
    using Engine.Content.FmtCollada.Types;

    [Serializable]
    public class SurfaceInitPlanar
    {
        [XmlElement("all", typeof(BasicIdRef))]
        public BasicIdRef All { get; set; }
    }
}
