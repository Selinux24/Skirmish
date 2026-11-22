using Engine.Mathematics;
using System;

namespace Engine.Animation
{
    /// <summary>
    /// Bone animation
    /// </summary>
    public readonly struct JointAnimation : IEquatable<JointAnimation>
    {
        /// <inheritdoc/>
        public static bool operator ==(JointAnimation left, JointAnimation right)
        {
            return left.Equals(right);
        }
        /// <inheritdoc/>
        public static bool operator !=(JointAnimation left, JointAnimation right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Keyframe list
        /// </summary>
        private readonly Keyframe[] keyframes;
        /// <summary>
        /// Start time
        /// </summary>
        private readonly float startTime;
        /// <summary>
        /// End time
        /// </summary>
        private readonly float endTime;
        /// <summary>
        /// Joint name
        /// </summary>
        private readonly string jointName;

        /// <summary>
        /// Joint name
        /// </summary>
        public readonly string JointName => jointName;
        /// <summary>
        /// Animation duration
        /// </summary>
        public readonly float Duration => endTime - startTime;

        /// <summary>
        /// Constructor
        /// </summary>
        public JointAnimation(string jointName, Keyframe[] keyframes)
        {
            ArgumentNullException.ThrowIfNull(keyframes);

            this.jointName = jointName;

            if (keyframes.Length == 0)
            {
                this.keyframes = [];
                startTime = 0;
                endTime = 0;

                return;
            }

            //Pre-normalize rotations
            Keyframe[] tmp = [.. keyframes];
            for (int i = 0; i < tmp.Length; i++)
            {
                tmp[i].Rotation.Normalize();
            }

            this.keyframes = tmp;
            startTime = tmp[0].Time;
            endTime = tmp[^1].Time;
        }

        /// <summary>
        /// Interpolate bone transformation
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Return interpolated transformation</returns>
        public Matrix4x4 Interpolate(float time)
        {
            Interpolate(time, out Vector3 translation, out Quaternion rotation, out Vector3 scale);

            //Create the combined transformation matrix
            return
                Matrix4x4.Scaling(scale) *
                Matrix4x4.RotationQuaternion(rotation) *
                Matrix4x4.Translation(translation);
        }
        /// <summary>
        /// Interpolate bone transformation
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="translation">Gets the interpolated translation</param>
        /// <param name="rotation">Gets the interpolated rotation</param>
        /// <param name="scale">Gets the interpolated scale</param>
        public void Interpolate(float time, out Vector3 translation, out Quaternion rotation, out Vector3 scale)
        {
            translation = Vector3.Zero;
            rotation = Quaternion.Identity;
            scale = Vector3.One;

            if (keyframes.Length == 0)
            {
                return;
            }

            var deltaTime = 0f;
            if (Duration > 0f)
            {
                deltaTime = time % Duration;
            }

            int currFrame = FindFrame(deltaTime);
            int nextFrame = (currFrame + 1) % keyframes.Length;

            var currKeyframe = keyframes[currFrame];
            var nextKeyframe = keyframes[nextFrame];

            var diffTime = nextKeyframe.Time - currKeyframe.Time;
            if (diffTime < 0f)
            {
                diffTime += Duration;
            }

            if (diffTime > 0f)
            {
                //Interpolate between frames
                var amount = (deltaTime - currKeyframe.Time) / diffTime;

                translation = Vector3.Lerp(currKeyframe.Translation, nextKeyframe.Translation, amount);
                rotation = Quaternion.Slerp(currKeyframe.Rotation, nextKeyframe.Rotation, amount);
                scale = Vector3.Lerp(currKeyframe.Scale, nextKeyframe.Scale, amount);
            }
            else
            {
                //Use current frame
                translation = currKeyframe.Translation;
                rotation = currKeyframe.Rotation;
                scale = currKeyframe.Scale;
            }
        }
        /// <summary>
        /// Finds the frame index of the specified key time
        /// </summary>
        /// <param name="keyTime">Key time</param>
        private int FindFrame(float keyTime)
        {
            if (keyframes.Length == 0)
            {
                return 0;
            }

            int frameIndex = 0;

            while (frameIndex < keyframes.Length - 1)
            {
                if (keyTime < keyframes[frameIndex + 1].Time)
                {
                    break;
                }
                frameIndex++;
            }

            if (frameIndex >= keyframes.Length)
            {
                frameIndex = 0;
            }

            return frameIndex;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Start: {startTime:0.00000}; End: {endTime:0.00000}; Keyframes: {keyframes?.Length ?? 0}";
        }
        /// <inheritdoc/>
        public bool Equals(JointAnimation other)
        {
            return
                jointName == other.jointName &&
                Helper.CompareArrays(keyframes, other.keyframes) &&
                MathUtils.NearEqual(startTime, other.startTime) &&
                MathUtils.NearEqual(endTime, other.endTime);
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is JointAnimation animation && Equals(animation);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(jointName, keyframes, startTime, endTime);
        }

        /// <summary>
        /// Makes a copy of the instance keyframes
        /// </summary>
        /// <returns>Returns a copy of the instance keyframes</returns>
        public JointAnimation Copy()
        {
            if (keyframes.Length == 0)
            {
                return new JointAnimation(jointName, []);
            }

            Keyframe[] kfs = new Keyframe[keyframes.Length];
            Array.Copy(keyframes, kfs, kfs.Length);

            return new JointAnimation(jointName, kfs);
        }
        /// <summary>
        /// Makes a copy of a range of the instance keyframes
        /// </summary>
        /// <param name="indexFrom">Keyframe from</param>
        /// <param name="indexTo">Keyframe to</param>
        /// <returns>Returns a copy of the instance keyframes</returns>
        public JointAnimation Copy(int indexFrom, int indexTo)
        {
            if (keyframes.Length == 0)
            {
                return new JointAnimation(jointName, []);
            }

            Keyframe[] kfs = new Keyframe[indexTo - indexFrom + 1];
            Array.Copy(keyframes, indexFrom, kfs, 0, kfs.Length);

            //Adjust copy time
            float dTime = kfs[0].Time;
            for (int k = 0; k < kfs.Length; k++)
            {
                kfs[k].Time -= dTime;
            }

            return new JointAnimation(jointName, kfs);
        }
    }
}
