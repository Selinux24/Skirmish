using SharpDX;
using System;

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
        public readonly Keyframe[] Keyframes;
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
                return this.EndTime - this.StartTime;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public JointAnimation(string jointName, Keyframe[] keyframes)
        {
            this.Joint = jointName;
            this.Keyframes = keyframes;
            this.StartTime = keyframes[0].Time;
            this.EndTime = keyframes[keyframes.Length - 1].Time;

            //Pre-normalize rotations
            for (int i = 0; i < this.Keyframes.Length; i++)
            {
                this.Keyframes[i].Rotation.Normalize();
            }
        }

        /// <summary>
        /// Interpolate bone transformation
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Return interpolated transformation</returns>
        public Matrix Interpolate(float time)
        {
            this.Interpolate(time, out Vector3 translation, out Quaternion rotation, out Vector3 scale);

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
            if (this.Keyframes == null)
            {
                translation = Vector3.Zero;
                rotation = Quaternion.Identity;
                scale = Vector3.One;
            }
            else
            {
                var deltaTime = 0.0f;
                if (this.Duration > 0.0f)
                {
                    deltaTime = time % this.Duration;
                }

                var currFrame = 0;
                while (currFrame < this.Keyframes.Length - 1)
                {
                    if (deltaTime < this.Keyframes[currFrame + 1].Time)
                    {
                        break;
                    }
                    currFrame++;
                }

                if (currFrame >= this.Keyframes.Length)
                {
                    currFrame = 0;
                }

                var nextFrame = (currFrame + 1) % this.Keyframes.Length;

                var currKey = this.Keyframes[currFrame];
                var nextKey = this.Keyframes[nextFrame];

                var diffTime = nextKey.Time - currKey.Time;
                if (diffTime < 0.0)
                {
                    diffTime += this.Duration;
                }

                if (diffTime > 0.0)
                {
                    //Interpolate
                    var factor = (float)((deltaTime - currKey.Time) / diffTime);

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
            return string.Format("Start: {0:0.00000}; End: {1:0.00000}; Keyframes: {2}", this.StartTime, this.EndTime, this.Keyframes.Length);
        }
        /// <summary>
        /// Gets whether the current instance is equal to the other instance
        /// </summary>
        /// <param name="other">The other instance</param>
        /// <returns>Returns true if both instances are equal</returns>
        public bool Equals(JointAnimation other)
        {
            return
                this.Joint == other.Joint &&
                Helper.ListIsEqual(this.Keyframes, other.Keyframes) &&
                this.StartTime == other.StartTime &&
                this.EndTime == other.EndTime;
        }
    }
}
