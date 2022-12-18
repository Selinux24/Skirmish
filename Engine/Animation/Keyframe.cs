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
        public float Time { get; set; }
        /// <summary>
        /// Curve position
        /// </summary>
        /// <remarks>Only for Bezier interpolations</remarks>
        public float Position { get; set; }
        /// <summary>
        /// Frame transformation
        /// </summary>
        public Matrix Transform
        {
            get
            {
                return transform;
            }
            set
            {
                Matrix trn = value;

                if (trn.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 location))
                {
                    transform = trn;
                    Translation = location;
                    Rotation = rotation;
                    Scale = scale;
                }
                else
                {
                    throw new EngineException("Bad transform");
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
        public KeyframeInterpolations Interpolation { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Interpolation == KeyframeInterpolations.Linear)
            {
                return $"Time: {Time:0.00000}: {Interpolation} {Transform.GetDescription()}";
            }
            else
            {
                return $"Time: {Time:0.00000}: {Interpolation} {Position:0.00000}";
            }
        }
        /// <summary>
        /// Gets whether the current instance is equal to the other instance
        /// </summary>
        /// <param name="other">The other instance</param>
        /// <returns>Returns true if both instances are equal</returns>
        public bool Equals(Keyframe other)
        {
            return
                Time == other.Time &&
                Position == other.Position &&
                Translation == other.Translation &&
                Rotation == other.Rotation &&
                Scale == other.Scale &&
                Interpolation == other.Interpolation;
        }
    }
}
