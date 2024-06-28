using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    [Serializable]
    public class Spline
    {
        [XmlAttribute("closed")]
        public bool Closed { get; set; }
        [XmlElement("source", typeof(Source))]
        public Source[] Sources { get; set; }
        [XmlElement("control_vertices", typeof(SplineControlVertices))]
        public SplineControlVertices ControlVertices { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }

        public Spline()
        {
            Closed = false;
        }
    }

    [Serializable]
    public class SplineControlVertices
    {
        [XmlElement("input", typeof(InputLocal))]
        public InputLocal[] Input { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
    }
}
