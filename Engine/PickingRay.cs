using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// Picking ray
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public struct PickingRay(Vector3 start, Vector3 direction, PickingHullTypes rayPickingParams, float length) : IEquatable<PickingRay>
    {
        /// <summary>
        /// Start position
        /// </summary>
        public Vector3 Start { get; set; } = start;
        /// <summary>
        /// End position
        /// </summary>
        public readonly Vector3 End { get { return Start + Direction * RayLength; } }
        /// <summary>
        /// Direction
        /// </summary>
        public Vector3 Direction { get; set; } = direction;
        /// <summary>
        /// Ray length
        /// </summary>
        public float RayLength { get; set; } = length;
        /// <summary>
        /// Picking parameters
        /// </summary>
        public PickingHullTypes RayPickingParams { get; set; } = rayPickingParams;
        /// <summary>
        /// Detect facing only primitive normals
        /// </summary>
        public readonly bool FacingOnly
        {
            get
            {
                return RayPickingParams.HasFlag(PickingHullTypes.FacingOnly);
            }
        }
        /// <summary>
        /// Maximum ray distance
        /// </summary>
        public readonly float MaxDistance
        {
            get
            {
                return RayLength <= 0 ? float.MaxValue : RayLength;
            }
        }
        /// <summary>
        /// Gets the ray segment
        /// </summary>
        public readonly Segment Segment
        {
            get
            {
                return new Segment(Start, End);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PickingRay(Ray ray) : this(ray.Position, ray.Direction, PickingHullTypes.Default, float.MaxValue)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public PickingRay(Ray ray, PickingHullTypes rayPickingParams) : this(ray.Position, ray.Direction, rayPickingParams, float.MaxValue)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public PickingRay(Ray ray, PickingHullTypes rayPickingParams, float length) : this(ray.Position, ray.Direction, rayPickingParams, length)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public PickingRay(Vector3 start, Vector3 direction) : this(start, direction, PickingHullTypes.Default, float.MaxValue)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public PickingRay(Vector3 start, Vector3 direction, PickingHullTypes rayPickingParams) : this(start, direction, rayPickingParams, float.MaxValue)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public PickingRay(Segment segment) : this(segment.Point1, segment.Direction, PickingHullTypes.Default, segment.Length)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public PickingRay(Segment segment, PickingHullTypes rayPickingParams) : this(segment.Point1, segment.Direction, rayPickingParams, segment.Length)
        {

        }

        /// <summary>
        /// Compares two instances of <see cref="PickingRay"/> for equality.
        /// </summary>
        /// <param name="left">An instance of <see cref="PickingRay"/>.</param>
        /// <param name="right">Another instance of <see cref="PickingRay"/>.</param>
        /// <returns>A value indicating whether the two instances are equal.</returns>
        public static bool operator ==(PickingRay left, PickingRay right)
        {
            return left.Equals(right);
        }
        /// <summary>
        /// Compares two instances of <see cref="PickingRay"/> for inequality.
        /// </summary>
        /// <param name="left">An instance of <see cref="PickingRay"/>.</param>
        /// <param name="right">Another instance of <see cref="PickingRay"/>.</param>
        /// <returns>A value indicating whether the two instances are unequal.</returns>
        public static bool operator !=(PickingRay left, PickingRay right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Implicit conversion between Ray and PickingRay
        /// </summary>
        public static implicit operator Ray(PickingRay value)
        {
            return new Ray(value.Start, value.Direction);
        }
        /// <summary>
        /// Implicit conversion between PickingRay and Ray
        /// </summary>
        public static implicit operator PickingRay(Ray value)
        {
            return new PickingRay(value);
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            PickingRay? objV = obj as PickingRay?;
            if (objV != null)
            {
                return Equals(objV);
            }

            return false;
        }
        /// <inheritdoc/>
        public readonly bool Equals(PickingRay other)
        {
            return
                Start == other.Start &&
                Direction == other.Direction &&
                MathUtil.NearEqual(RayLength, other.RayLength) &&
                RayPickingParams == other.RayPickingParams;
        }
        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Start, Direction, RayLength, RayPickingParams);
        }
    }
}
