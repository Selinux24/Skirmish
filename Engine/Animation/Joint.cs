using SharpDX;
using System;

namespace Engine.Animation
{
    /// <summary>
    /// Skeleton's Joint
    /// </summary>
    public sealed class Joint : IEquatable<Joint>
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
        /// <summary>
        /// Gets whether the current instance is equal to the other instance
        /// </summary>
        /// <param name="other">The other instance</param>
        /// <returns>Returns true if both instances are equal</returns>
        public bool Equals(Joint other)
        {
            return
                this.Name == other.Name &&
                Helper.ListIsEqual(this.Childs, other.Childs) &&
                this.Offset == other.Offset;
        }
    }
}
