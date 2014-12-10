using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    [Serializable]
    public class SourceTechniqueCommon
    {
        [XmlElement("accessor", typeof(Accessor))]
        public Accessor Accessor { get; set; }
    }
}