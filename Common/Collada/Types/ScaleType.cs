using System;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public class ScaleType : Vector3Type
    {
        [XmlAttribute("sid")]
        public string Id { get; set; }
    }
}
