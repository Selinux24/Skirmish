using System;
using System.Linq;
using System.Xml.Serialization;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    [Serializable]
    public class Node : NamedNode
    {
        [XmlAttribute("sid")]
        public string SId { get; set; }
        [XmlAttribute("type")]
        public EnumNodeType Type { get; set; }
        [XmlAttribute("layer")]
        public string Layer { get; set; }
        [XmlElement("asset", typeof(Asset))]
        public Asset Asset { get; set; }
        [XmlElement("lookat", typeof(BasicFloat4X4))]
        public BasicFloat4X4[] LookAt { get; set; }
        [XmlElement("matrix", typeof(BasicFloat4X4))]
        public BasicFloat4X4[] Matrix { get; set; }
        [XmlElement("rotate", typeof(BasicFloat4))]
        public BasicFloat4[] Rotate { get; set; }
        [XmlElement("scale", typeof(BasicFloat3))]
        public BasicFloat3[] Scale { get; set; }
        [XmlElement("skew", typeof(BasicFloat4))]
        public BasicFloat4[] Skew { get; set; }
        [XmlElement("translate", typeof(BasicFloat3))]
        public BasicFloat3[] Translate { get; set; }
        [XmlElement("instance_camera", typeof(InstanceWithExtra))]
        public InstanceWithExtra[] InstanceCamera { get; set; }
        [XmlElement("instance_controller", typeof(InstanceController))]
        public InstanceController[] InstanceController { get; set; }
        [XmlElement("instance_geometry", typeof(InstanceGeometry))]
        public InstanceGeometry[] InstanceGeometry { get; set; }
        [XmlElement("instance_light", typeof(InstanceWithExtra))]
        public InstanceWithExtra[] InstanceLight { get; set; }
        [XmlElement("instance_node", typeof(InstanceWithExtra))]
        public InstanceWithExtra[] InstanceNode { get; set; }
        [XmlElement("node", typeof(Node))]
        public Node[] Nodes { get; set; }
        [XmlElement("extra", typeof(Extra))]
        public Extra[] Extras { get; set; }

        [XmlIgnore]
        public bool IsLight
        {
            get
            {
                return InstanceLight != null;
            }
        }
        [XmlIgnore]
        public bool IsArmature
        {
            get
            {
                return Type == EnumNodeType.Joint;
            }
        }
        [XmlIgnore]
        public bool HasController
        {
            get
            {
                return InstanceController != null;
            }
        }
        [XmlIgnore]
        public string SkeletonId
        {
            get
            {
                return InstanceController?.FirstOrDefault()?.Skeleton?.FirstOrDefault();
            }
        }
        [XmlIgnore]
        public bool HasGeometry
        {
            get
            {
                return InstanceGeometry != null;
            }
        }

        public Node()
        {
            Type = EnumNodeType.Node;
        }
    }
}
