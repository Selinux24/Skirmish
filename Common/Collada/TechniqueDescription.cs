using System;
using System.Xml.Serialization;

namespace Common.Collada
{
    using Common.Collada.Types;

    [Serializable]
    public class TechniqueDescription
    {
        [XmlElement("emission")]
        public ColorTextureType Emission { get; set; }
        [XmlElement("ambient")]
        public ColorTextureType Ambient { get; set; }
        [XmlElement("diffuse")]
        public ColorTextureType Diffuse { get; set; }
        [XmlElement("specular")]
        public ColorTextureType Specular { get; set; }
        [XmlElement("shininess")]
        public FloatParamType Shininess { get; set; }
        [XmlElement("reflective")]
        public ColorTextureType Reflective { get; set; }
        [XmlElement("reflectivity")]
        public FloatParamType Reflectivity { get; set; }
        [XmlElement("transparent")]
        public ColorTextureType Transparent { get; set; }
        [XmlElement("transparency")]
        public FloatParamType Transparency { get; set; }
        [XmlElement("index_of_refraction")]
        public FloatParamType IndexOfRefraction { get; set; }
    }
}
