using System;
using System.Xml.Serialization;

namespace Common.Collada
{
    [Serializable]
    public class InstanceVisualScene
    {
        [XmlAttribute("url")]
        public string Url { get; set; }
    }
}
