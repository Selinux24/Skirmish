using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Bone animation
    /// </summary>
    public struct BoneAnimation
    {
        /// <summary>
        /// Keyframe list
        /// </summary>
        public Keyframe[] Keyframes;
        /// <summary>
        /// Start time
        /// </summary>
        public float StartTime
        {
            get
            {
                return this.Keyframes[0].Time;
            }
        }
        /// <summary>
        /// End time
        /// </summary>
        public float EndTime
        {
            get
            {
                return this.Keyframes[this.Keyframes.Length - 1].Time;
            }
        }

        /// <summary>
        /// Interpolate bone transformation
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Return interpolated transformation</returns>
        public Matrix Interpolate(float time)
        {
            Keyframe start = this.Keyframes[0];
            Keyframe end = this.Keyframes[this.Keyframes.Length - 1];

            if (time <= start.Time)
            {
                return start.Transform;
            }
            else if (time >= end.Time)
            {
                return end.Transform;
            }
            else
            {
                for (int i = 0; i < this.Keyframes.Length - 1; i++)
                {
                    Keyframe from = this.Keyframes[i];
                    Keyframe to = this.Keyframes[i + 1];

                    if ((time >= from.Time) && (time <= to.Time))
                    {
                        float amount = (time - from.Time) / (to.Time - from.Time);

                        return Keyframe.Interpolate(from, to, amount);
                    }
                }
            }

            return Matrix.Identity;
        }

        /// <summary>
        /// Gets text representation
        /// </summary>
        /// <returns>Returns text representation</returns>
        public override string ToString()
        {
            return string.Format("Start: {0}; End: {1}; Keyframes: {2}", this.StartTime, this.EndTime, this.Keyframes.Length);
        }
    }
}
