using System;
using System.Xml.Serialization;

namespace Engine.Collada.FX
{
    using global::Engine.Collada.Types;

    [Serializable]
    public class BlinnPhong
    {
        [XmlElement("emission", typeof(VarColorOrTexture))]
        public VarColorOrTexture Emission { get; set; }
        [XmlElement("ambient", typeof(VarColorOrTexture))]
        public VarColorOrTexture Ambient { get; set; }
        [XmlElement("diffuse", typeof(VarColorOrTexture))]
        public VarColorOrTexture Diffuse { get; set; }
        [XmlElement("specular", typeof(VarColorOrTexture))]
        public VarColorOrTexture Specular { get; set; }
        [XmlElement("shininess", typeof(VarFloatOrParam))]
        public VarFloatOrParam Shininess { get; set; }
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

        public override string ToString()
        {
            return "Blinn / Phong Effect";
        }
    }
}
