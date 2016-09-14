using SharpDX;
using System;
using System.Collections.Generic;

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
        /// Number of joints in the skeleton
        /// </summary>
        public int JointCount { get; private set; }
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
        /// <summary>
        /// Built skeleton transforms at time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="joint">Joint</param>
        /// <param name="animations">Animation list</param>
        private static void BuildTransforms(float time, Joint joint, JointAnimation[] animations)
        {
            joint.LocalTransform = Array.Find(animations, a => a.Joint == joint.Name).Interpolate(time);

            if (joint.Childs != null && joint.Childs.Length > 0)
            {
                for (int i = 0; i < joint.Childs.Length; i++)
                {
                    BuildTransforms(time, joint.Childs[i], animations);
                }
            }
        }
        /// <summary>
        /// Updates joint transforms
        /// </summary>
        /// <param name="joint">Joint</param>
        private static void UpdateTransforms(Joint joint)
        {
            UpdateToWorldTransform(joint);

            if (joint.Childs != null && joint.Childs.Length > 0)
            {
                for (int i = 0; i < joint.Childs.Length; i++)
                {
                    UpdateTransforms(joint.Childs[i]);
                }
            }
        }
        /// <summary>
        /// Updates joint to world transforms
        /// </summary>
        /// <param name="joint">Joint</param>
        public static void UpdateToWorldTransform(Joint joint)
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

            this.JointCount = names.Count;
            this.JointNames = names.ToArray();
            this.FinalTransforms = Helper.CreateArray<Matrix>(names.Count, Matrix.Identity);
        }

        /// <summary>
        /// Updates skeleton state at time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="animations">Joint animations list</param>
        public void Update(float time, JointAnimation[] animations)
        {
            BuildTransforms(time, this.Root, animations);

            UpdateTransforms(this.Root);

            for (int i = 0; i < this.JointNames.Length; i++)
            {
                this.FinalTransforms[i] = this[this.JointNames[i]].Offset * this[this.JointNames[i]].GlobalTransform;
            }
        }
        /// <summary>
        /// Gets the transforms list of the pose at specified time
        /// </summary>
        /// <param name="time">Pose time</param>
        /// <param name="animations">Joint animations</param>
        /// <returns>Returns the transforms list of the pose at specified time</returns>
        public Matrix[] GetPoseAtTime(float time, JointAnimation[] animations)
        {
            BuildTransforms(time, this.Root, animations);

            UpdateTransforms(this.Root);

            for (int i = 0; i < this.JointNames.Length; i++)
            {
                this.FinalTransforms[i] = this[this.JointNames[i]].Offset * this[this.JointNames[i]].GlobalTransform;
            }

            return this.FinalTransforms;
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
