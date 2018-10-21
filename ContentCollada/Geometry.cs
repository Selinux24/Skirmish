using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Geometry : NamedNode
    {
        [XmlElement("asset", typeof(Asset))]
        public Asset Asset { get; set; }
        [XmlElement("convex_mesh", typeof(ConvexMesh))]
        public ConvexMesh ConvexMesh { get; set; }
        [XmlElement("mesh", typeof(Mesh))]
        public Mesh Mesh { get; set; }
        [XmlElement("spline", typeof(Spline))]
        public Spline Spline { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }

        public override string ToString()
        {
            return "Geometry;";
        }
    }
}
