using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    [Serializable]
    public class SourceTechniqueCommon
    {
        [XmlElement("accessor", typeof(Accessor))]
        public Accessor Accessor { get; set; }
    }
}