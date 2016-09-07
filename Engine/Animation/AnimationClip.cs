using SharpDX;
using System;
using System.Text;

namespace Engine.Animation
{
    /// <summary>
    /// Animation clip
    /// </summary>
    public struct AnimationClip
    {
        /// <summary>
        /// Bone animation list
        /// </summary>
        public readonly BoneAnimation[] BoneAnimations;
        /// <summary>
        /// Start time
        /// </summary>
        public readonly float StartTime;
        /// <summary>
        /// End time
        /// </summary>
        public readonly float EndTime;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="boneAnimations">Bone animation list</param>
        public AnimationClip(BoneAnimation[] boneAnimations)
        {
            this.BoneAnimations = boneAnimations;

            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            for (int i = 0; i < this.BoneAnimations.Length; i++)
            {
                minValue = Math.Min(maxValue, this.BoneAnimations[i].StartTime);
                maxValue = Math.Max(minValue, this.BoneAnimations[i].EndTime);
            }

            this.StartTime = minValue;
            this.EndTime = maxValue;
        }

        /// <summary>
        /// Interpolate animation
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="boneTransforms">Current bone transformations</param>
        public void Interpolate(float time, ref Matrix[] boneTransforms)
        {
            for (int i = 0; i < this.BoneAnimations.Length; i++)
            {
                var curr = this.BoneAnimations[i];

                boneTransforms[i] = curr.Interpolate(time);
            }
        }
        /// <summary>
        /// Fills animation clip description into the specified StringBuilder
        /// </summary>
        /// <param name="desc">Description to fill</param>
        public void GetDescription(ref StringBuilder desc)
        {
            for (int i = 0; i < this.BoneAnimations.Length; i++)
            {
                this.BoneAnimations[i].GetDescription(ref desc);
            }
        }
    }
}
