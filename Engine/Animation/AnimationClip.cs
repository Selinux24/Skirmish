﻿using System;
using SharpDX;

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
            string desc = "";

            for (int i = 0; i < this.BoneAnimations.Length; i++)
            {
                var curr = this.BoneAnimations[i];

                boneTransforms[i] = Matrix.Transpose(curr.Interpolate(time));

                string d = string.Format("{0,-20} : {1}", curr.Joint, boneTransforms[i].GetDescription()) + Environment.NewLine;

                desc += d;
            }
        }

        public string GetDescription()
        {
            string desc = "";

            for (int i = 0; i < this.BoneAnimations.Length; i++)
            {
                desc += this.BoneAnimations[i].GetDescription() + Environment.NewLine;
            }

            return desc;
        }
    }
}
