using SharpDX;

namespace Engine.Common
{
    public struct BoneAnimation
    {
        public Keyframe[] Keyframes;
        public float StartTime
        {
            get
            {
                return this.Keyframes[0].Time;
            }
        }
        public float EndTime
        {
            get
            {
                return this.Keyframes[this.Keyframes.Length - 1].Time;
            }
        }
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
    }
}
