using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;

namespace Engine.Mathematics
{
    /// <summary>
    /// Picking ray
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [JsonObject(MemberSerialization.OptIn)]
    public struct PickingRay : IEquatable<PickingRay>
    {
        /// <summary>
        /// Position
        /// </summary>
        [JsonProperty]
        public Vector3 Position;
        /// <summary>
        /// Target
        /// </summary>
        public readonly Vector3 Target { get { return Position + (Direction * Length); } }
        /// <summary>
        /// Direction
        /// </summary>
        [JsonProperty]
        public Vector3 Direction;
        /// <summary>
        /// Length
        /// </summary>
        [JsonProperty]
        public float Length;
        /// <summary>
        /// Picking parameters
        /// </summary>
        [JsonProperty]
        public PickingHullTypes PickingParams;
        /// <summary>
        /// Detect facing only primitive normals
        /// </summary>
        public readonly bool FacingOnly => PickingParams.HasFlag(PickingHullTypes.FacingOnly);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">Start position</param>
        /// <param name="direction">Ray direction</param>
        /// <param name="pickingParams">Picking parameters</param>
        /// <param name="length">Ray length</param>
        public PickingRay(Vector3 position, Vector3 direction, PickingHullTypes pickingParams, float length)
        {
            if (length < 0) throw new ArgumentException("Ray length must be 0 or positive.", nameof(length));

            Position = position;
            Direction = Vector3.Normalize(direction);
            PickingParams = pickingParams;
            Length = length;
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
        public PickingRay(Vector3 position, Vector3 direction) : this(position, direction, PickingHullTypes.Default, float.MaxValue)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public PickingRay(Vector3 position, Vector3 direction, PickingHullTypes rayPickingParams) : this(position, direction, rayPickingParams, float.MaxValue)
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
            return new Ray(value.Position, value.Direction);
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
                Position == other.Position &&
                Direction == other.Direction &&
                MathUtils.NearEqual(Length, other.Length) &&
                PickingParams == other.PickingParams;
        }
        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Position, Direction, Length, PickingParams);
        }
    }
}
