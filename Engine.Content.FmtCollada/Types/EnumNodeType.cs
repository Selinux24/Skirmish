using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.Types
{
    [Serializable]
    public enum EnumNodeType
    {
        [XmlEnum("JOINT")]
        Joint,
        [XmlEnum("NODE")]
        Node,
    }
}
