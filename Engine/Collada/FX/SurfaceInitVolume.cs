using System;
using System.Xml.Serialization;

namespace Engine.Collada.FX
{
    using Collada.Types;

    [Serializable]
    public class SurfaceInitVolume
    {
        [XmlElement("all", typeof(BasicIDREF))]
        public BasicIDREF All { get; set; }
        [XmlElement("primary", typeof(BasicIDREF))]
        public BasicIDREF Primary { get; set; }
    }
}
