using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Animation
{
    /// <summary>
    /// Skeleton class
    /// </summary>
    public class Skeleton
    {
        /// <summary>
        /// Root joint
        /// </summary>
        public Joint Root { get; private set; }
        /// <summary>
        /// Joint indices
        /// </summary>
        public int[] JointIndices { get; private set; }
        /// <summary>
        /// Joint names
        /// </summary>
        public string[] JointNames { get; private set; }
        /// <summary>
        /// Gets joint by name
        /// </summary>
        /// <param name="jointName">Joint name</param>
        /// <returns>Returns the joint with the specified name</returns>
        public Joint this[string jointName]
        {
            get
            {
                if (Array.Exists(this.JointNames, j => j == jointName))
                {
                    return this.FindJoint(this.Root, jointName);
                }

                return null;
            }
        }

        /// <summary>
        /// Flatten skeleton
        /// </summary>
        /// <param name="joint">Joint</param>
        /// <param name="parentIndex">Parent joint index</param>
        /// <param name="indices">Joint indices</param>
        /// <param name="names">Joint names</param>
        private static void FlattenSkeleton(Joint joint, int parentIndex, List<int> indices, List<string> names)
        {
            indices.Add(parentIndex);

            int index = names.Count;

            names.Add(joint.Name);

            if (joint.Childs != null && joint.Childs.Length > 0)
            {
                for (int i = 0; i < joint.Childs.Length; i++)
                {
                    FlattenSkeleton(
                        joint.Childs[i],
                        index,
                        indices,
                        names);
                }
            }
        }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="root">Root joint</param>
        public Skeleton(Joint root)
        {
            List<int> indices = new List<int>();
            List<string> names = new List<string>();
            FlattenSkeleton(root, -1, indices, names);

            this.Root = root;
            this.JointIndices = indices.ToArray();
            this.JointNames = names.ToArray();
        }

        /// <summary>
        /// Fills skeleton description into the specified StringBuilder
        /// </summary>
        /// <param name="desc">Description to fill</param>
        public void GetDescription(ref StringBuilder desc)
        {
            for (int i = 0; i < this.JointNames.Length; i++)
            {
                Joint j = this[this.JointNames[i]];

                j.GetDescription(ref desc);
            }
        }
        /// <summary>
        /// Finds a joint by name recursively
        /// </summary>
        /// <param name="joint">Joint</param>
        /// <param name="jointName">Joint name</param>
        /// <returns>Returns the joint with the specified name</returns>
        private Joint FindJoint(Joint joint, string jointName)
        {
            if (joint.Name == jointName) return joint;

            if (joint.Childs == null || joint.Childs.Length == 0) return null;

            for (int i = 0; i < joint.Childs.Length; i++)
            {
                var j = FindJoint(joint.Childs[i], jointName);
                if (j != null) return j;
            }

            return null;
        }
    }
}
