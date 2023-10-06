using System;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Skin
    {
        [XmlAttribute("source")]
        public string SourceUri { get; set; }
        [XmlElement("bind_shape_matrix", typeof(BasicFloat4X4))]
        public BasicFloat4X4 BindShapeMatrix { get; set; }
        [XmlElement("source", typeof(Source))]
        public Source[] Sources { get; set; }
        [XmlElement("joints", typeof(Joints))]
        public Joints Joints { get; set; }
        [XmlElement("vertex_weights", typeof(VertexWeights))]
        public VertexWeights VertexWeights { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }
        [XmlIgnore]
        public Source this[string id]
        {
            get
            {
                return Array.Find(Sources, s => string.Equals("#" + s.Id, id));
            }
        }
    }
}
