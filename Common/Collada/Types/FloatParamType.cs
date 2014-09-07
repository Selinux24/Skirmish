using System;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public class FloatParamType
    {
        [XmlElement("float")]
        public float Float { get; set; }
    }
}
