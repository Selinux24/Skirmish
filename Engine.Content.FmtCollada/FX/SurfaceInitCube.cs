using System;
using System.Xml.Serialization;

namespace Engine.Collada.FX
{
    using Engine.Collada.Types;

    [Serializable]
    public class SurfaceInitCube
    {
        [XmlElement("all", typeof(BasicIdRef))]
        public BasicIdRef All { get; set; }
        [XmlElement("face", typeof(BasicIdRef))]
        public BasicIdRef Face { get; set; }
        [XmlElement("primary", typeof(SurfaceInitCubePrimary))]
        public SurfaceInitCubePrimary Primary { get; set; }
    }

    [Serializable]
    public class SurfaceInitCubePrimary : BasicIdRef
    {
        [XmlElement("order", typeof(EnumSurfaceFaces))]
        public EnumSurfaceFaces[] order { get; set; }
    }
}
