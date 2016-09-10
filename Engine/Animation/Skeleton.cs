using SharpDX;
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
        /// Gets joint by name
        /// </summary>
        /// <param name="jointName">Joint name</param>
        /// <returns>Returns the joint with the specified name</returns>
        public Joint this[string jointName]
        {
            get
            {
                return this.FindJoint(this.Root, jointName);
            }
        }
        /// <summary>
        /// Joint names
        /// </summary>
        public string[] JointNames { get; private set; }
        /// <summary>
        /// Final transforms
        /// </summary>
        public Matrix[] FinalTransforms { get; private set; }

        /// <summary>
        /// Flatten skeleton
        /// </summary>
        /// <param name="joint">Joint</param>
        /// <param name="parentIndex">Parent joint index</param>
        /// <param name="indices">Joint indices</param>
        /// <param name="names">Joint names</param>
        private static void FlattenSkeleton(Joint joint, List<string> names)
        {
            names.Add(joint.Name);

            if (joint.Childs != null && joint.Childs.Length > 0)
            {
                for (int i = 0; i < joint.Childs.Length; i++)
                {
                    FlattenSkeleton(joint.Childs[i], names);
                }
            }
        }

        private static void BuildTransforms(float time, string clipName, Joint j)
        {
            j.LocalTransform = j.Animations[clipName].Interpolate(time);

            if (j.Childs != null && j.Childs.Length > 0)
            {
                for (int i = 0; i < j.Childs.Length; i++)
                {
                    BuildTransforms(time, clipName, j.Childs[i]);
                }
            }
        }

        private static void UpdateTransforms(Joint node)
        {
            CalculateBoneToWorldTransform(node);

            if (node.Childs != null && node.Childs.Length > 0)
            {
                for (int i = 0; i < node.Childs.Length; i++)
                {
                    UpdateTransforms(node.Childs[i]);
                }
            }
        }

        public static void CalculateBoneToWorldTransform(Joint joint)
        {
            joint.GlobalTransform = joint.LocalTransform;

            var parent = joint.Parent;
            while (parent != null)
            {
                joint.GlobalTransform *= parent.LocalTransform;
                parent = parent.Parent;
            }
        }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="root">Root joint</param>
        public Skeleton(Joint root)
        {
            this.Root = root;

            List<string> names = new List<string>();
            FlattenSkeleton(root, names);

            this.JointNames = names.ToArray();

            this.FinalTransforms = new Matrix[names.Count];
        }


        public void Update(float time, string clipName)
        {
            BuildTransforms(time, clipName, this.Root);

            UpdateTransforms(this.Root);

            for (int i = 0; i < this.JointNames.Length; i++)
            {
                this.FinalTransforms[i] = this[this.JointNames[i]].Offset * this[this.JointNames[i]].GlobalTransform;
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
    }
}
