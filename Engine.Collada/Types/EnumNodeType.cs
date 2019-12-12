using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
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
