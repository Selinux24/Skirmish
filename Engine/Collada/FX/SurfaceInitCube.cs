using System;
using System.Xml.Serialization;

namespace Engine.Collada.FX
{
    using Collada.Types;

    [Serializable]
    public class SurfaceInitCube
    {
        [XmlElement("all", typeof(BasicIDREF))]
        public BasicIDREF All { get; set; }
        [XmlElement("face", typeof(BasicIDREF))]
        public BasicIDREF Face { get; set; }
        [XmlElement("primary", typeof(SurfaceInitCubePrimary))]
        public SurfaceInitCubePrimary Primary { get; set; }
    }

    [Serializable]
    public class SurfaceInitCubePrimary : BasicIDREF
    {
        [XmlElement("order", typeof(EnumSurfaceFaces))]
        public EnumSurfaceFaces[] order { get; set; }
    }
}
