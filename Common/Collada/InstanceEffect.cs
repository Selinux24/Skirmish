using System;
using System.Xml.Serialization;

namespace Common.Collada
{
    [Serializable]
    public class InstanceEffect
    {
        [XmlAttribute("url")]
        public string Url { get; set; }
    }
}
