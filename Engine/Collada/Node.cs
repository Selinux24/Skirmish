using System;
using System.Xml.Serialization;
using SharpDX;

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
        [XmlElement("lookat", typeof(BasicFloat4x4))]
        public BasicFloat4x4[] LookAt { get; set; }
        [XmlElement("matrix", typeof(BasicFloat4x4))]
        public BasicFloat4x4[] Matrix { get; set; }
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
        public bool IsArmature
        {
            get
            {
                return (this.Nodes != null && this.Nodes[0].Type == EnumNodeType.Joint);
            }
        }
        [XmlIgnore]
        public bool HasController
        {
            get
            {
                return this.InstanceController != null;
            }
        }
        [XmlIgnore]
        public bool HasGeometry
        {
            get
            {
                return this.InstanceGeometry != null;
            }
        }

        public Node()
        {
            this.Type = EnumNodeType.Node;
        }

        public Transforms ReadTransforms()
        {
            Matrix finalTranslation = SharpDX.Matrix.Identity;
            Matrix finalRotation = SharpDX.Matrix.Identity;
            Matrix finalScale = SharpDX.Matrix.Identity;

            if (this.Translate != null)
            {
                BasicFloat3 loc = Array.Find(this.Translate, t => string.Equals(t.SId, "location"));
                if (loc != null) finalTranslation *= SharpDX.Matrix.Translation(loc.ToVector3());
            }

            if (this.Rotate != null)
            {
                BasicFloat4 rotX = Array.Find(this.Rotate, t => string.Equals(t.SId, "rotationX"));
                if (rotX != null)
                {
                    Vector4 r = rotX.ToVector4();
                    finalRotation *= SharpDX.Matrix.RotationAxis(new Vector3(r.X, r.Y, r.Z), r.W);
                }

                BasicFloat4 rotY = Array.Find(this.Rotate, t => string.Equals(t.SId, "rotationY"));
                if (rotY != null)
                {
                    Vector4 r = rotY.ToVector4();
                    finalRotation *= SharpDX.Matrix.RotationAxis(new Vector3(r.X, r.Y, r.Z), r.W);
                }

                BasicFloat4 rotZ = Array.Find(this.Rotate, t => string.Equals(t.SId, "rotationZ"));
                if (rotZ != null)
                {
                    Vector4 r = rotZ.ToVector4();
                    finalRotation *= SharpDX.Matrix.RotationAxis(new Vector3(r.X, r.Y, r.Z), r.W);
                }
            }

            if (this.Scale != null)
            {
                BasicFloat3 sca = Array.Find(this.Scale, t => string.Equals(t.SId, "scale"));
                if (sca != null) finalScale *= SharpDX.Matrix.Scaling(sca.ToVector3());
            }

            return new Transforms()
            {
                Translation = finalTranslation,
                Rotation = finalRotation,
                Scale = finalScale,
            };
        }

        public Matrix ReadMatrix()
        {
            Matrix m = SharpDX.Matrix.Identity;

            if (this.Matrix != null)
            {
                BasicFloat4x4 trn = Array.Find(this.Matrix, t => string.Equals(t.SId, "transform"));
                if (trn != null) m = trn.ToMatrix();
            }

            return m;
        }
    }
}
