using System;
using System.Xml.Serialization;

namespace Engine.Collada.Types
{
    [Serializable]
    public enum EnumMorphMethods
    {
        [XmlEnum("NORMALIZED")]
        Normalized,
        [XmlEnum("RELATIVE")]
        Relative,
    }
}
