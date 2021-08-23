using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Animation
{
    /// <summary>
    /// Skeleton class
    /// </summary>
    public sealed class Skeleton : IEquatable<Skeleton>
    {
        /// <summary>
        /// Joint names list
        /// </summary>
        private readonly List<string> jointNames = new List<string>();

        /// <summary>
        /// Root joint
        /// </summary>
        public Joint Root { get; private set; }
        /// <summary>
        /// Number of joints in the skeleton
        /// </summary>
        public int JointCount
        {
            get
            {
                return jointNames.Count;
            }
        }
        /// <summary>
        /// Gets joint by name
        /// </summary>
        /// <param name="jointName">Joint name</param>
        /// <returns>Returns the joint with the specified name</returns>
        public Joint this[string jointName]
        {
            get
            {
                return FindJoint(Root, jointName);
            }
        }

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
        /// <param name="joint">Joint</param>
        /// <param name="time">Time</param>
        /// <param name="animations">Animation list</param>
        private static void BuildTransforms(Joint joint, float time, IEnumerable<JointAnimation> animations)
        {
            joint.LocalTransform = animations.First(a => a.Joint == joint.Name).Interpolate(time);

            if (joint.Childs != null && joint.Childs.Length > 0)
            {
                for (int i = 0; i < joint.Childs.Length; i++)
                {
                    BuildTransforms(joint.Childs[i], time, animations);
                }
            }
        }
        /// <summary>
        /// Built skeleton transforms interpolated between two times with specified factor
        /// </summary>
        /// <param name="joint">Joint</param>
        /// <param name="time1">Time 1</param>
        /// <param name="animations1">Animation list 1</param>
        /// <param name="time2">Time 2</param>
        /// <param name="animations2">Animation list 2</param>
        /// <param name="factor">Interpolation factor</param>
        private static void BuildTransforms(Joint joint, float time1, IEnumerable<JointAnimation> animations1, float time2, IEnumerable<JointAnimation> animations2, float factor)
        {
            animations1.First(a => a.Joint == joint.Name).Interpolate(time1, out Vector3 pos1, out Quaternion rot1, out Vector3 sca1);
            animations2.First(a => a.Joint == joint.Name).Interpolate(time2, out Vector3 pos2, out Quaternion rot2, out Vector3 sca2);

            Vector3 translation = pos1 + (pos2 - pos1) * factor;
            Quaternion rotation = Quaternion.Slerp(rot1, rot2, factor);
            Vector3 scale = sca1 + (sca2 - sca1) * factor;

            joint.LocalTransform =
                Matrix.Scaling(scale) *
                Matrix.RotationQuaternion(rotation) *
                Matrix.Translation(translation);

            if (joint.Childs != null && joint.Childs.Length > 0)
            {
                for (int i = 0; i < joint.Childs.Length; i++)
                {
                    BuildTransforms(joint.Childs[i], time1, animations1, time2, animations2, factor);
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
            Root = root;

            FlattenSkeleton(root, jointNames);
        }

        /// <summary>
        /// Gets the transforms list of the pose at specified time
        /// </summary>
        /// <param name="time">Pose time</param>
        /// <param name="animations">Joint animations</param>
        /// <param name="transforms">Returns the transforms list of the pose at specified time</param>
        public void GetPoseAtTime(float time, IEnumerable<JointAnimation> animations, ref Matrix[] transforms)
        {
            BuildTransforms(Root, time, animations);

            UpdateTransforms(Root);

            for (int i = 0; i < jointNames.Count; i++)
            {
                transforms[i] = this[jointNames[i]].Offset * this[jointNames[i]].GlobalTransform;
            }
        }
        /// <summary>
        /// Gets the transforms list of tow poses at specified time
        /// </summary>
        /// <param name="time1">First pose time</param>
        /// <param name="animations1">First joint animation set</param>
        /// <param name="time2">Second pose time</param>
        /// <param name="animations2">Second joint animation set</param>
        /// <param name="factor">Interpolation factor</param>
        /// <param name="transforms">Returns the transforms list of the pose at specified time</param>
        public void GetPoseAtTime(float time1, IEnumerable<JointAnimation> animations1, float time2, IEnumerable<JointAnimation> animations2, float factor, ref Matrix[] transforms)
        {
            BuildTransforms(Root, time1, animations1, time2, animations2, factor);

            UpdateTransforms(Root);

            for (int i = 0; i < jointNames.Count; i++)
            {
                transforms[i] = this[jointNames[i]].Offset * this[jointNames[i]].GlobalTransform;
            }
        }
        /// <summary>
        /// Gets the joint names list
        /// </summary>
        /// <returns>Returns the joint names list</returns>
        public IEnumerable<string> GetJointNames()
        {
            return jointNames.ToArray();
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
        /// Gets whether the current instance is equal to the other instance
        /// </summary>
        /// <param name="other">The other instance</param>
        /// <returns>Returns true if both instances are equal</returns>
        public bool Equals(Skeleton other)
        {
            return
                Helper.ListIsEqual(jointNames, other.jointNames) &&
                Root.Equals(other.Root);
        }
    }
}
