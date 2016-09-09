using SharpDX;
using System.Text;

namespace Engine.Animation
{
    /// <summary>
    /// Bone animation
    /// </summary>
    public struct BoneAnimation
    {
        /// <summary>
        /// Joint name
        /// </summary>
        public string Joint;
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
                return
                    Matrix.Scaling(start.Scale) *
                    Matrix.RotationQuaternion(start.Rotation) *
                    Matrix.Translation(start.Translation);
            }
            else if (time >= end.Time)
            {
                return
                    Matrix.Scaling(end.Scale) *
                    Matrix.RotationQuaternion(end.Rotation) *
                    Matrix.Translation(end.Translation);
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

                        return
                            Matrix.Scaling(Vector3.Lerp(from.Scale, to.Scale, amount)) *
                            Matrix.RotationQuaternion(Quaternion.Slerp(from.Rotation, to.Rotation, amount)) *
                            Matrix.Translation(Vector3.Lerp(from.Translation, to.Translation, amount));
                    }
                }
            }

            return Matrix.Identity;
        }

        /// <summary>
        /// Fills keyframe description into the specified StringBuilder
        /// </summary>
        /// <param name="desc">Description to fill</param>
        public void GetDescription(ref StringBuilder desc)
        {
            desc.AppendLine("==> " + this.Joint);

            for (int i = 0; i < this.Keyframes.Length; i++)
            {
                this.Keyframes[i].GetDescription(ref desc);
            }
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
