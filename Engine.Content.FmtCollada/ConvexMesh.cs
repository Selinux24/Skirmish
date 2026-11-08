using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    [Serializable]
    public class ConvexMesh
    {
        [XmlAttribute("convex_hull_of")]
        public string ConvexHullOf { get; set; }
        [XmlElement("source", typeof(Source))]
        public Source[] Sources { get; set; }
        [XmlElement("vertices", typeof(Vertices))]
        public Vertices Vertices { get; set; }
        [XmlElement("lines", typeof(Lines))]
        public Lines[] Lines { get; set; }
        [XmlElement("linestrips", typeof(LineStrips))]
        public LineStrips[] LineStrips { get; set; }
        [XmlElement("polygons", typeof(Polygons))]
        public Polygons[] Polygons { get; set; }
        [XmlElement("polylist", typeof(PolyList))]
        public PolyList[] PolyList { get; set; }
        [XmlElement("triangles", typeof(Triangles))]
        public Triangles[] Triangles { get; set; }
        [XmlElement("trifans", typeof(TriFans))]
        public TriFans[] TriFans { get; set; }
        [XmlElement("tristrips", typeof(TriStrips))]
        public TriStrips[] TriStrips { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
    }
}
