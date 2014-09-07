using System;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public enum Versions
    {
        [XmlEnum("1.4.1")]
        v1_4_1,
        [XmlEnum("1.5.0")]
        v1_5_0,
    }
}
