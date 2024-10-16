using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada.Types
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
