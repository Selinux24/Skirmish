using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.Types
{
    [Serializable]
    public enum EnumVersions
    {
        [XmlEnum("1.4.0")]
        v1_4_0,
        [XmlEnum("1.4.1")]
        v1_4_1,
    }
}
