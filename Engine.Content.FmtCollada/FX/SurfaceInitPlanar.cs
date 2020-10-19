using System;
using System.Xml.Serialization;

namespace Engine.Collada.FX
{
    using Engine.Collada.Types;

    [Serializable]
    public class SurfaceInitPlanar
    {
        [XmlElement("all", typeof(BasicIdRef))]
        public BasicIdRef All { get; set; }
    }
}
