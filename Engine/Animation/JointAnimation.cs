using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Animation
{
    /// <summary>
    /// Bone animation
    /// </summary>
    public struct JointAnimation : IEquatable<JointAnimation>
    {
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
        public JointAnimation(string jointName, Keyframe[] keyframes)
        {
            Joint = jointName;

            if (keyframes?.Any() != true)
            {
                Keyframes = new Keyframe[] { };
                StartTime = 0;
                EndTime = 0;

                return;
            }

            //Pre-normalize rotations
            var tmp = new List<Keyframe>(keyframes);
            for (int i = 0; i < tmp.Count; i++)
            {
                tmp[i].Rotation.Normalize();
            }

            Keyframes = tmp;
            StartTime = tmp.First().Time;
            EndTime = tmp.Last().Time;
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

            if (Keyframes != null)
            {
                var deltaTime = 0.0f;
                if (Duration > 0.0f)
                {
                    deltaTime = time % Duration;
                }

                var currFrame = 0;
                while (currFrame < Keyframes.Count - 1)
                {
                    if (deltaTime < Keyframes.ElementAt(currFrame + 1).Time)
                    {
                        break;
                    }
                    currFrame++;
                }

                if (currFrame >= Keyframes.Count)
                {
                    currFrame = 0;
                }

                var nextFrame = (currFrame + 1) % Keyframes.Count;

                var currKey = Keyframes.ElementAt(currFrame);
                var nextKey = Keyframes.ElementAt(nextFrame);

                var diffTime = nextKey.Time - currKey.Time;
                if (diffTime < 0.0)
                {
                    diffTime += Duration;
                }

                if (diffTime > 0.0)
                {
                    //Interpolate
                    var factor = (deltaTime - currKey.Time) / diffTime;

                    translation = currKey.Translation + (nextKey.Translation - currKey.Translation) * factor;
                    rotation = Quaternion.Slerp(currKey.Rotation, nextKey.Rotation, factor);
                    scale = currKey.Scale + (nextKey.Scale - currKey.Scale) * factor;
                }
                else
                {
                    //Use current frame
                    translation = currKey.Translation;
                    rotation = currKey.Rotation;
                    scale = currKey.Scale;
                }
            }
        }
        /// <summary>
        /// Gets text representation
        /// </summary>
        /// <returns>Returns text representation</returns>
        public override string ToString()
        {
            return $"Start: {StartTime:0.00000}; End: {EndTime:0.00000}; Keyframes: {Keyframes.Count}";
        }
        /// <summary>
        /// Gets whether the current instance is equal to the other instance
        /// </summary>
        /// <param name="other">The other instance</param>
        /// <returns>Returns true if both instances are equal</returns>
        public bool Equals(JointAnimation other)
        {
            return
                Joint == other.Joint &&
                Helper.ListIsEqual(Keyframes, other.Keyframes) &&
                StartTime == other.StartTime &&
                EndTime == other.EndTime;
        }

        /// <summary>
        /// Makes a copy of the instance keyframes
        /// </summary>
        /// <returns>Returns a copy of the instance keyframes</returns>
        public JointAnimation Copy()
        {
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
