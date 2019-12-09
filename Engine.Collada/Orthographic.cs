using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using global::Engine.Collada.Types;

    [Serializable]
    public class Orthographic
    {
        [XmlElement("xmag")]
        public BasicFloat XMag { get; set; }
        [XmlElement("ymag")]
        public BasicFloat YMag { get; set; }
        [XmlElement("aspect_ratio")]
        public BasicFloat AspectRatio { get; set; }
        [XmlElement("znear")]
        public BasicFloat ZNear { get; set; }
        [XmlElement("zfar")]
        public BasicFloat ZFar { get; set; }
    }
}
