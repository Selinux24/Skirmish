using SharpDX;
using System;

namespace Engine.Animation
{
    /// <summary>
    /// Skeleton's Joint
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="name">Joint name</param>
    /// <param name="bone">Bone name</param>
    /// <param name="parent">Parent joint</param>
    /// <param name="local">Local transform</param>
    /// <param name="global">Global transform</param>
    public sealed class Joint(string name, string bone, Joint parent, Matrix local, Matrix global) : IEquatable<Joint>
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; } = name;
        /// <summary>
        /// Bone name
        /// </summary>
        public string Bone { get; private set; } = bone;
        /// <summary>
        /// Parent joint
        /// </summary>
        public Joint Parent { get; set; } = parent;
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
        public Matrix LocalTransform { get; set; } = local;
        /// <summary>
        /// World transform matrix
        /// </summary>
        public Matrix GlobalTransform { get; set; } = global;

        /// <summary>
        /// Finds a joint by name recursively
        /// </summary>
        /// <param name="boneName">Bone name</param>
        /// <returns>Returns the joint with the specified name</returns>
        public Joint FindJoint(string boneName)
        {
            if (string.Equals(Bone, boneName, StringComparison.Ordinal))
            {
                return this;
            }

            if ((Childs?.Length ?? 0) == 0)
            {
                return null;
            }

            foreach (var child in Childs)
            {
                var j = child.FindJoint(boneName);
                if (j != null)
                {
                    return j;
                }
            }

            return null;
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
