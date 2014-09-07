using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Common.Collada
{
    using Common.Collada.Types;

    [Serializable]
    public class LibraryVisualSceneNode
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlElement("lookat")]
        public List<LookAtType> LookAt { get; set; }
        [XmlElement("matrix")]
        public List<MatrixType> Matrix { get; set; }
        [XmlElement("rotate")]
        public List<RotationType> Rotate { get; set; }
        [XmlElement("scale")]
        public List<ScaleType> Scale { get; set; }
        [XmlElement("skew")]
        public List<SkewType> Skew { get; set; }
        [XmlElement("translate")]
        public List<TranslateType> Translate { get; set; }

        [XmlElement("instance_geometry")]
        public List<InstanceGeometry> InstanceGeometry { get; set; }
    }
}
