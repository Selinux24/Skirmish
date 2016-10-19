using System;
using System.Xml.Serialization;

namespace Engine.Collada.FX
{
    using Collada.Types;

    [Serializable]
    public class SurfaceInitPlanar
    {
        [XmlElement("all", typeof(BasicIDREF))]
        public BasicIDREF All { get; set; }
    }
}
