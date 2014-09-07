using System;
using System.Xml.Serialization;

namespace Common.Collada.Types
{
    [Serializable]
    public enum UpAxisType
    {
        [XmlEnum("X_UP")]
        X,
        [XmlEnum("Y_UP")]
        Y,
        [XmlEnum("Z_UP")]
        Z,
    }
}
