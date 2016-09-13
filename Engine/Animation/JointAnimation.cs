using SharpDX;

namespace Engine.Animation
{
    /// <summary>
    /// Bone animation
    /// </summary>
    public struct JointAnimation
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
        }
        
        /// <summary>
        /// Interpolate bone transformation
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Return interpolated transformation</returns>
        public Matrix Interpolate(float time)
        {
            time *= 1;
            var dTime = 0.0f;
            if (Duration > 0.0f)
            {
                dTime = time % Duration;
            }

            // interpolate position keyframes
            var pPosition = new Vector3();

            {
                var frame = 0;
                while (frame < this.Keyframes.Length - 1)
                {
                    if (dTime < this.Keyframes[frame + 1].Time)
                    {
                        break;
                    }
                    frame++;
                }
                if (frame >= this.Keyframes.Length)
                {
                    frame = 0;
                }

                var nextFrame = (frame + 1) % this.Keyframes.Length;

                var key = this.Keyframes[frame];
                var nextKey = this.Keyframes[nextFrame];
                var diffTime = nextKey.Time - key.Time;
                if (diffTime < 0.0)
                {
                    diffTime += Duration;
                }
                if (diffTime > 0.0)
                {
                    var factor = (float)((dTime - key.Time) / diffTime);
                    pPosition = key.Translation + (nextKey.Translation - key.Translation) * factor;
                }
                else
                {
                    pPosition = key.Translation;
                }
            }

            // interpolate rotation keyframes
            var pRot = new Quaternion(1, 0, 0, 0);

            {
                var frame = 0;
                while (frame < this.Keyframes.Length - 1)
                {
                    if (dTime < this.Keyframes[frame + 1].Time)
                    {
                        break;
                    }
                    frame++;
                }
                if (frame >= this.Keyframes.Length)
                {
                    frame = 0;
                }
                var nextFrame = (frame + 1) % this.Keyframes.Length;

                var key = this.Keyframes[frame];
                var nextKey = this.Keyframes[nextFrame];
                key.Rotation.Normalize();
                nextKey.Rotation.Normalize();
                var diffTime = nextKey.Time - key.Time;
                if (diffTime < 0.0)
                {
                    diffTime += Duration;
                }
                if (diffTime > 0)
                {
                    var factor = (float)((dTime - key.Time) / diffTime);
                    pRot = Quaternion.Slerp(key.Rotation, nextKey.Rotation, factor);
                }
                else
                {
                    pRot = key.Rotation;
                }
            }

            // interpolate scale keyframes
            var pscale = new Vector3(1);

            {
                var frame = 0;
                while (frame < this.Keyframes.Length - 1)
                {
                    if (dTime < this.Keyframes[frame + 1].Time)
                    {
                        break;
                    }
                    frame++;
                }
                if (frame >= this.Keyframes.Length)
                {
                    frame = 0;
                }
            }

            // create the combined transformation matrix
            var mat = Matrix.RotationQuaternion(pRot);
            mat.M11 *= pscale.X; mat.M21 *= pscale.X; mat.M31 *= pscale.X;
            mat.M12 *= pscale.Y; mat.M22 *= pscale.Y; mat.M32 *= pscale.Y;
            mat.M13 *= pscale.Z; mat.M23 *= pscale.Z; mat.M33 *= pscale.Z;

            mat.M41 = pPosition.X;
            mat.M42 = pPosition.Y; 
            mat.M43 = pPosition.Z;

            return mat;
        }
        /// <summary>
        /// Gets text representation
        /// </summary>
        /// <returns>Returns text representation</returns>
        public override string ToString()
        {
            return string.Format("Start: {0:0.00000}; End: {1:0.00000}; Keyframes: {2}", this.StartTime, this.EndTime, this.Keyframes.Length);
        }
    }
}
