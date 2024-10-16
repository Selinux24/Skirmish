using System;
using System.Xml.Serialization;

namespace Engine.Content.FmtCollada
{
    using Engine.Content.FmtCollada.Types;

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

        /// <inheritdoc/>
        public override string ToString()
        {
            string typeName = "None";
            if (ConvexMesh != null) typeName = nameof(ConvexMesh);
            if (Mesh != null) typeName = nameof(Mesh);
            if (Spline != null) typeName = nameof(Spline);

            return base.ToString() + $" Type: {typeName};";
        }
    }
}
