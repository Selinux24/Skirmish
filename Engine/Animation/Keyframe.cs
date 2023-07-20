using SharpDX;
using System;

namespace Engine.Animation
{
    /// <summary>
    /// Animation keyframe
    /// </summary>
    public struct Keyframe : IEquatable<Keyframe>
    {
        /// <inheritdoc/>
        public static bool operator ==(Keyframe left, Keyframe right)
        {
            return left.Equals(right);
        }
        /// <inheritdoc/>
        public static bool operator !=(Keyframe left, Keyframe right)
        {
            return !(left == right);
        }

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
            readonly get
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
        public override readonly string ToString()
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
        /// <inheritdoc/>
        public readonly bool Equals(Keyframe other)
        {
            return
                Time == other.Time &&
                Position == other.Position &&
                Translation == other.Translation &&
                Rotation == other.Rotation &&
                Scale == other.Scale &&
                Interpolation == other.Interpolation;
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            return obj is Keyframe keyframe && Equals(keyframe);
        }
        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Time, Position, Translation, Rotation, Scale, Interpolation);
        }
    }
}
