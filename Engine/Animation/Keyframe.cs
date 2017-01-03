using SharpDX;
using System;

namespace Engine.Animation
{
    /// <summary>
    /// Animation keyframe
    /// </summary>
    public struct Keyframe : IEquatable<Keyframe>
    {
        /// <summary>
        /// Frame transformation
        /// </summary>
        private Matrix transform;

        /// <summary>
        /// Time
        /// </summary>
        public float Time;
        /// <summary>
        /// Frame transformation
        /// </summary>
        public Matrix Transform
        {
            get
            {
                return this.transform;
            }
            set
            {
                Matrix trn = value;

                Vector3 location;
                Quaternion rotation;
                Vector3 scale;
                if (trn.Decompose(out scale, out rotation, out location))
                {
                    this.transform = trn;
                    this.Translation = location;
                    this.Rotation = rotation;
                    this.Scale = scale;
                }
                else
                {
                    throw new Exception("Bad transform");
                }
            }
        }
        /// <summary>
        /// Translation
        /// </summary>
        public Vector3 Translation { get; private set; }
        /// <summary>
        /// Rotation
        /// </summary>
        public Quaternion Rotation { get; private set; }
        /// <summary>
        /// Scale
        /// </summary>
        public Vector3 Scale { get; private set; }
        /// <summary>
        /// Interpolation type
        /// </summary>
        public string Interpolation;

        /// <summary>
        /// Gets text representation
        /// </summary>
        /// <returns>Return text representation</returns>
        public override string ToString()
        {
            return string.Format("Time: {0:0.00000}: {1}", this.Time, this.Transform.GetDescription());
        }
        /// <summary>
        /// Gets whether the current instance is equal to the other instance
        /// </summary>
        /// <param name="other">The other instance</param>
        /// <returns>Returns true if both instances are equal</returns>
        public bool Equals(Keyframe other)
        {
            return
                this.Time == other.Time &&
                this.Translation == other.Translation &&
                this.Rotation == other.Rotation &&
                this.Scale == other.Scale &&
                this.Interpolation == other.Interpolation;
        }
    }
}
