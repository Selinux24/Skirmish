using System;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public class FloatType
    {
        [XmlElement("float")]
        public float Value { get; set; }
    }
}
