using System;
using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Animation clip
    /// </summary>
    public struct AnimationClip
    {
        /// <summary>
        /// Bone animation list
        /// </summary>
        public BoneAnimation[] BoneAnimations;

        /// <summary>
        /// Start time
        /// </summary>
        public float StartTime
        {
            get
            {
                float maxValue = float.MaxValue;

                for (int i = 0; i < this.BoneAnimations.Length; i++)
                {
                    maxValue = Math.Min(maxValue, this.BoneAnimations[i].StartTime);
                }

                return maxValue;
            }
        }
        /// <summary>
        /// End time
        /// </summary>
        public float EndTime
        {
            get
            {
                float minValue = float.MinValue;

                for (int i = 0; i < this.BoneAnimations.Length; i++)
                {
                    minValue = Math.Max(minValue, this.BoneAnimations[i].EndTime);
                }

                return minValue;
            }
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
                boneTransforms[i] = this.BoneAnimations[i].Interpolate(time);
            }
        }
    }
}
