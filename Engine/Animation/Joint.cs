using SharpDX;

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
        /// Inverse bind matrix
        /// </summary>
        public Matrix Offset { get; set; }
        /// <summary>
        /// Local transform matrix
        /// </summary>
        public Matrix LocalTransform { get; set; }
        /// <summary>
        /// World transform matrix
        /// </summary>
        public Matrix GlobalTransform { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Joint name</param>
        /// <param name="parent">Parent joint</param>
        /// <param name="local">Local transform</param>
        /// <param name="global">Global transform</param>
        public Joint(string name, Joint parent, Matrix local, Matrix global)
        {
            this.Name = name;
            this.Parent = parent;
            this.LocalTransform = local;
            this.GlobalTransform = global;
        }

        /// <summary>
        /// Gets text representation
        /// </summary>
        /// <returns>Return text representation</returns>
        public override string ToString()
        {
            return string.Format("Name: {0}; Root: {1}", this.Name, this.Parent == null);
        }
    }
}
