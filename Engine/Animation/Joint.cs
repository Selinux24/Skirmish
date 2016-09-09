using SharpDX;
using System.Collections.Generic;
using System.Text;

namespace Engine.Animation
{
    /// <summary>
    /// Skeleton's Joint
    /// </summary>
    public class Joint
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Parent joint
        /// </summary>
        public Joint Parent { get; set; }
        /// <summary>
        /// Child joints
        /// </summary>
        public Joint[] Childs { get; set; }
        /// <summary>
        /// Local transform matrix
        /// </summary>
        public Matrix LocalTransform { get; set; }
        /// <summary>
        /// World transform matrix
        /// </summary>
        public Matrix WorldTransform { get; set; }
        /// <summary>
        /// Inverse bind matrix
        /// </summary>
        public Matrix InverseBindMatrix { get; set; }
        /// <summary>
        /// Joint animation dictionary
        /// </summary>
        public Dictionary<string, BoneAnimation> Animations { get; set; }
        /// <summary>
        /// Skinning transform
        /// </summary>
        public Matrix SkinningTransform { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Joint name</param>
        /// <param name="parent">Parent joint</param>
        /// <param name="local">Local transform</param>
        /// <param name="world">World transform</param>
        public Joint(string name, Joint parent, Matrix local, Matrix world)
        {
            this.Animations = new Dictionary<string, BoneAnimation>();

            this.Name = name;
            this.Parent = parent;
            this.WorldTransform = world;
            this.LocalTransform = local;
        }

        /// <summary>
        /// Fills joint description into the specified StringBuilder
        /// </summary>
        /// <param name="desc">Description to fill</param>
        public void GetDescription(ref StringBuilder desc)
        {
            desc.AppendFormat("Name: {0}; Parent: {1}; Childs: {2};",
                this.Name,
                this.Parent != null ? this.Parent.Name : "-",
                this.Childs != null ? this.Childs.Length : 0);

            desc.AppendLine();
            desc.AppendLine("BIND MATRIX");
            desc.AppendLine(this.InverseBindMatrix.GetDescription());
            desc.AppendLine("LOCAL");
            desc.AppendLine(this.LocalTransform.GetDescription());
            desc.AppendLine("GLOBAL");
            desc.AppendLine(this.WorldTransform.GetDescription());
            desc.AppendLine("SKINNING");
            desc.AppendLine(this.SkinningTransform.GetDescription());
            desc.AppendLine();
        }
        /// <summary>
        /// Gets text representation
        /// </summary>
        /// <returns>Return text representation</returns>
        public override string ToString()
        {
            return string.Format("Name: {0};", string.IsNullOrEmpty(this.Name) ? "root" : this.Name);
        }
    }
}
