using SharpDX;

namespace Engine.Animation
{
    /// <summary>
    /// Transition manager between two animation clips
    /// </summary>
    public class Transition
    {
        /// <summary>
        /// From clip
        /// </summary>
        private AnimationClip from;
        /// <summary>
        /// To clip
        /// </summary>
        private AnimationClip to;
        /// <summary>
        /// Starting time of from clip
        /// </summary>
        private float fromStartTime;
        /// <summary>
        /// Starting time of to clip
        /// </summary>
        private float toStartTime;

        /// <summary>
        /// Animation duration
        /// </summary>
        public float Duration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="from">From clip</param>
        /// <param name="to">To clip</param>
        /// <param name="duration">Transition duration</param>
        /// <param name="fromStart">Starting time of from clip</param>
        /// <param name="toStart">Starting time of to clip</param>
        public Transition(AnimationClip from, AnimationClip to, float duration, float fromStart, float toStart)
        {
            this.from = from;
            this.to = to;
            this.Duration = duration;
            this.fromStartTime = fromStart;
            this.toStartTime = toStart;
        }

        /// <summary>
        /// Gets the interpolated transformation matrix list at specified time
        /// </summary>
        /// <param name="time">Time to interpolate relative to transition duration</param>
        /// <returns>Returns the interpolated transformation matrix list at specified time</returns>
        public Matrix[] Interpolate(float time)
        {
            //The time variable is relative to totalTime so
            float factor = 1.0f - (time / this.Duration);

            int count = from.Animations.Length;

            Matrix[] res = new Matrix[count];

            for (int i = 0; i < count; i++)
            {
                Vector3 fPos;
                Quaternion fRot;
                Vector3 fSca;
                from.Animations[i].Interpolate(time + fromStartTime, out fPos, out fRot, out fSca);

                Vector3 tPos;
                Quaternion tRot;
                Vector3 tSca;
                to.Animations[i].Interpolate(time + toStartTime, out tPos, out tRot, out tSca);

                var translation = fPos + (tPos - fPos) * factor;
                var rotation = Quaternion.Slerp(fRot, tRot, factor);
                var scale = fSca + (tSca - fSca) * factor;

                var mat = Matrix.RotationQuaternion(rotation);

                mat.M11 *= scale.X; mat.M21 *= scale.X; mat.M31 *= scale.X;
                mat.M12 *= scale.Y; mat.M22 *= scale.Y; mat.M32 *= scale.Y;
                mat.M13 *= scale.Z; mat.M23 *= scale.Z; mat.M33 *= scale.Z;

                mat.M41 = translation.X;
                mat.M42 = translation.Y;
                mat.M43 = translation.Z;

                res[i] = mat;
            }

            return res;
        }
    }
}
