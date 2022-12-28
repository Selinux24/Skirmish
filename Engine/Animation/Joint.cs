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
        /// Bone name
        /// </summary>
        public string Bone { get; private set; }
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
        /// <param name="bone">Bone name</param>
        /// <param name="parent">Parent joint</param>
        /// <param name="local">Local transform</param>
        /// <param name="global">Global transform</param>
        public Joint(string name, string bone, Joint parent, Matrix local, Matrix global)
        {
            Name = name;
            Bone = bone;
            Parent = parent;
            LocalTransform = local;
            GlobalTransform = global;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Name: {Name}; Bone: {Bone}; Root: {Parent}";
        }
        /// <inheritdoc/>
        public bool Equals(Joint other)
        {
            return
                Name == other.Name &&
                Bone == other.Bone &&
                Parent == other.Parent &&
                Helper.CompareEnumerables(Childs, other.Childs) &&
                Offset == other.Offset &&
                LocalTransform == other.LocalTransform &&
                GlobalTransform == other.GlobalTransform;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as Joint);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Bone, Parent, Childs, Offset, LocalTransform, GlobalTransform);
        }
    }
}
