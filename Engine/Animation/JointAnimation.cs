using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Joint name
        /// </summary>
        public readonly string Joint;
        /// <summary>
        /// Keyframe list
        /// </summary>
        public readonly IReadOnlyCollection<Keyframe> Keyframes;
        /// <summary>
        /// Start time
        /// </summary>
        public readonly float StartTime;
        /// <summary>
        /// End time
        /// </summary>
        public readonly float EndTime;
        /// <summary>
        /// Animation duration
        /// </summary>
        public float Duration
        {
            get
            {
                return EndTime - StartTime;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public JointAnimation(string jointName, IEnumerable<Keyframe> keyframes)
        {
            Joint = jointName;

            if (keyframes?.Any() != true)
            {
                Keyframes = [];
                StartTime = 0;
                EndTime = 0;

                return;
            }

            //Pre-normalize rotations
            List<Keyframe> tmp = new(keyframes);
            for (int i = 0; i < tmp.Count; i++)
            {
                tmp[i].Rotation.Normalize();
            }

            Keyframes = tmp;
            StartTime = tmp[0].Time;
            EndTime = tmp[^1].Time;
        }

        /// <summary>
        /// Interpolate bone transformation
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Return interpolated transformation</returns>
        public Matrix Interpolate(float time)
        {
            Interpolate(time, out Vector3 translation, out Quaternion rotation, out Vector3 scale);

            //Create the combined transformation matrix
            return
                Matrix.Scaling(scale) *
                Matrix.RotationQuaternion(rotation) *
                Matrix.Translation(translation);
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

            if (Keyframes.Count == 0)
            {
                return;
            }

            var deltaTime = 0f;
            if (Duration > 0f)
            {
                deltaTime = time % Duration;
            }

            int currFrame = FindFrame(deltaTime);
            int nextFrame = (currFrame + 1) % Keyframes.Count;

            var currKeyframe = Keyframes.ElementAt(currFrame);
            var nextKeyframe = Keyframes.ElementAt(nextFrame);

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
            if (Keyframes.Count == 0)
            {
                return 0;
            }

            int frameIndex = 0;

            while (frameIndex < Keyframes.Count - 1)
            {
                if (keyTime < Keyframes.ElementAt(frameIndex + 1).Time)
                {
                    break;
                }
                frameIndex++;
            }

            if (frameIndex >= Keyframes.Count)
            {
                frameIndex = 0;
            }

            return frameIndex;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Start: {StartTime:0.00000}; End: {EndTime:0.00000}; Keyframes: {Keyframes?.Count ?? 0}";
        }
        /// <inheritdoc/>
        public bool Equals(JointAnimation other)
        {
            return
                Joint == other.Joint &&
                Helper.CompareEnumerables(Keyframes, other.Keyframes) &&
                StartTime == other.StartTime &&
                EndTime == other.EndTime;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is JointAnimation animation && Equals(animation);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Joint.GetHashCode() ^ Keyframes.GetHashCode() ^ StartTime.GetHashCode() ^ EndTime.GetHashCode();
        }

        /// <summary>
        /// Makes a copy of the instance keyframes
        /// </summary>
        /// <returns>Returns a copy of the instance keyframes</returns>
        public JointAnimation Copy()
        {
            if (Keyframes.Count == 0)
            {
                return new JointAnimation(Joint, []);
            }

            Keyframe[] kfs = new Keyframe[Keyframes.Count];
            Array.Copy(Keyframes.ToArray(), kfs, kfs.Length);

            return new JointAnimation(Joint, kfs);
        }
        /// <summary>
        /// Makes a copy of a range of the instance keyframes
        /// </summary>
        /// <param name="indexFrom">Keyframe from</param>
        /// <param name="indexTo">Keyframe to</param>
        /// <returns>Returns a copy of the instance keyframes</returns>
        public JointAnimation Copy(int indexFrom, int indexTo)
        {
            if (Keyframes.Count == 0)
            {
                return new JointAnimation(Joint, []);
            }

            Keyframe[] kfs = new Keyframe[indexTo - indexFrom + 1];
            Array.Copy(Keyframes.ToArray(), indexFrom, kfs, 0, kfs.Length);

            //Adjust copy time
            float dTime = kfs[0].Time;
            for (int k = 0; k < kfs.Length; k++)
            {
                kfs[k].Time -= dTime;
            }

            return new JointAnimation(Joint, kfs);
        }
    }
}
