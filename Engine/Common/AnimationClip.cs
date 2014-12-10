using System;
using SharpDX;

namespace Engine.Common
{
    public struct AnimationClip
    {
        public BoneAnimation[] BoneAnimations;

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
        public void Interpolate(float time, ref Matrix[] boneTransforms)
        {
            for (int i = 0; i < this.BoneAnimations.Length; i++)
            {
                boneTransforms[i] = this.BoneAnimations[i].Interpolate(time);
            }
        }
    }
}
