using System;
using System.Xml.Serialization;

namespace Engine.Collada.FX
{
    using Engine.Collada.Types;

    [Serializable]
    public class Constant
    {
        [XmlElement("emission", typeof(VarColorOrTexture))]
        public VarColorOrTexture Emission { get; set; }
        [XmlElement("reflective", typeof(VarColorOrTexture))]
        public VarColorOrTexture Reflective { get; set; }
        [XmlElement("reflectivity", typeof(VarFloatOrParam))]
        public VarFloatOrParam Reflectivity { get; set; }
        [XmlElement("transparent", typeof(BasicTransparent))]
        public BasicTransparent Transparent { get; set; }
        [XmlElement("transparency", typeof(VarFloatOrParam))]
        public VarFloatOrParam Transparency { get; set; }
        [XmlElement("index_of_refraction", typeof(VarFloatOrParam))]
        public VarFloatOrParam IndexOfRefraction { get; set; }
    }
}
