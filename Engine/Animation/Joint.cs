using SharpDX;
using System;
using System.Text;

namespace Engine.Animation
{
    /// <summary>
    /// Joint
    /// </summary>
    public class Joint
    {
        /// <summary>
        /// Global transform matrix
        /// </summary>
        private Matrix globalTransform;
        /// <summary>
        /// Local transform matrix
        /// </summary>
        private Matrix localTransform;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Parent joint
        /// </summary>
        public Joint Parent { get; set; }
        /// <summary>
        /// Child joints
        /// </summary>
        public Joint[] Childs { get; set; }
        /// <summary>
        /// Global transform matrix
        /// </summary>
        public Matrix GlobalTransform
        {
            get
            {
                return this.globalTransform;
            }
            set
            {
                this.globalTransform = value;

                Vector3 scale;
                Quaternion rotation;
                Vector3 translation;
                if (this.globalTransform.Decompose(out scale, out rotation, out translation))
                {
                    this.GlobalTransformScale = scale;
                    this.GlobalTransformRotation = rotation;
                    this.GlobalTransformTranslation = translation;
                }
                else
                {
                    throw new Exception("Bad transform");
                }
            }
        }
        /// <summary>
        /// Global transform translation
        /// </summary>
        public Vector3 GlobalTransformTranslation { get; private set; }
        /// <summary>
        /// Global transform rotation
        /// </summary>
        public Quaternion GlobalTransformRotation { get; private set; }
        /// <summary>
        /// Global transform scale
        /// </summary>
        public Vector3 GlobalTransformScale { get; private set; }
        /// <summary>
        /// Local transform matrix
        /// </summary>
        public Matrix LocalTransform
        {
            get
            {
                return this.localTransform;
            }
            set
            {
                this.localTransform = value;

                Vector3 scale;
                Quaternion rotation;
                Vector3 translation;
                if (this.localTransform.Decompose(out scale, out rotation, out translation))
                {
                    this.LocalTransformScale = scale;
                    this.LocalTransformRotation = rotation;
                    this.LocalTransformTranslation = translation;
                }
                else
                {
                    throw new Exception("Bad transform");
                }
            }
        }
        /// <summary>
        /// Local transform translation
        /// </summary>
        public Vector3 LocalTransformTranslation { get; private set; }
        /// <summary>
        /// Local transform rotation
        /// </summary>
        public Quaternion LocalTransformRotation { get; private set; }
        /// <summary>
        /// Local transform scale
        /// </summary>
        public Vector3 LocalTransformScale { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Joint()
        {
            this.GlobalTransform = Matrix.Identity;
            this.LocalTransform = Matrix.Identity;
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
                this.Childs!= null ? this.Childs.Length : 0);
            desc.AppendLine();
            desc.AppendLine("LOCAL");
            desc.AppendLine(this.LocalTransform.GetDescription());
            desc.AppendLine("GLOBAL");
            desc.AppendLine(this.GlobalTransform.GetDescription());
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
