using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    [Serializable]
    public class OpticsTechniqueCommon
    {
        [XmlElement("orthographic", typeof(Orthographic))]
        public Orthographic orthographic { get; set; }
        [XmlElement("perspective", typeof(Perspective))]
        public Perspective perspective { get; set; }
    }
}
