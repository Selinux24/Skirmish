using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    using Engine.Content.FmtCollada.Types;

    [Serializable]
    public class Perspective
    {
        [XmlElement("xfov")]
        public BasicFloat XFov { get; set; }
        [XmlElement("yfov")]
        public BasicFloat YFov { get; set; }
        [XmlElement("aspect_ratio")]
        public BasicFloat AspectRatio { get; set; }
        [XmlElement("znear")]
        public BasicFloat ZNear { get; set; }
        [XmlElement("zfar")]
        public BasicFloat ZFar { get; set; }
    }
}
