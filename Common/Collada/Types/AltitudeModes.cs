using System;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public enum AltitudeModes
    {
        [XmlEnum("absolute")]
        Absolute,
        [XmlEnum("relativeToGround")]
        RelativeToGround,
    }
}
