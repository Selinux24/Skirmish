using Engine.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Mathematics
{
    /// <summary>
    /// Axis aligned bounding cylinder
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="center">Center position</param>
    /// <param name="radius">Radius</param>
    /// <param name="height">Height</param>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [JsonObject(MemberSerialization.OptIn)]
    public struct BoundingCylinder(Vector3 center, float radius, float height) : IEquatable<BoundingCylinder>, IFormattable
    {
        const string BoundingCylinderStr = "Base:{0} Cap:{1} Radius:{2}";

        /// <summary>
        /// Constructs a BoundingCylinder that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the cylinder</param>
        /// <returns>When the method completes, contains the newly constructed bounding cylinder</returns>
        public static BoundingCylinder FromPoints(IEnumerable<Vector3> points)
        {
            //Find radius and height
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;

            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;

            for (int i = 0; i < points.Count(); i++)
            {
                var p = points.ElementAt(i);

                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;

                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;

                if (p.Z < minZ) minZ = p.Z;
                if (p.Z > maxZ) maxZ = p.Z;
            }

            float h = maxY - minY;
            float r = Vector2.Distance(new Vector2(minX, minZ), new Vector2(maxX, maxZ)) * 0.5f;

            //Find center
            Vector3 c = (new Vector3(minX, minY, minZ) + new Vector3(maxX, minY, maxZ)) * 0.5f;

            return new BoundingCylinder(c, r, h);
        }
        /// <summary>
        /// Constructs a BoundingCylinder that fully contains the given points
        /// </summary>
        /// <param name="points">The points that will be contained by the cylinder</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding cylinder</param>
        public static void FromPoints(IEnumerable<Vector3> points, out BoundingCylinder result)
        {
            result = FromPoints(points);
        }

        /// <summary>
        /// Radius
        /// </summary>
        [JsonProperty]
        public float Radius = radius;
        /// <summary>
        /// Height
        /// </summary>
        [JsonProperty]
        public float Height = height;
        /// <summary>
        /// Center
        /// </summary>
        [JsonProperty]
        public Vector3 Center = center;
        /// <summary>
        /// Base position
        /// </summary>
        public readonly Vector3 BasePosition => new(Center.X, Center.Y - (Height * 0.5f), Center.Z);
        /// <summary>
        /// Cap position
        /// </summary>
        public readonly Vector3 CapPosition => new(Center.X, Center.Y + (Height * 0.5f), Center.Z);

        /// <summary>
        /// Determines whether the current objects contains a point.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public readonly ContainmentType Contains(ref Vector3 point)
        {
            var c = this;
            return Collision.CylinderContainsPoint(ref c, ref point);
        }
        /// <summary>
        /// Determines whether the current objects contains a point.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public readonly ContainmentType Contains(Vector3 point)
        {
            return Contains(ref point);
        }

        /// <summary>
        /// Gets the cylinder vertices
        /// </summary>
        /// <param name="sliceCount">Slice count</param>
        /// <returns>Returns a point array of vertices</returns>
        public readonly IEnumerable<Vector3> GetVertices(int sliceCount)
        {
            var geom = GeometryUtil.CreateCylinder(Topology.TriangleList, Center, Radius, Height, sliceCount);

            return [.. geom.Vertices];
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoundingCylinder left, BoundingCylinder right)
        {
            return
                left.Center == right.Center &&
                Helper.NearEqual(left.Radius, right.Radius) &&
                Helper.NearEqual(left.Height, right.Height);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoundingCylinder left, BoundingCylinder right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                BoundingCylinderStr,
                BasePosition.ToString(),
                CapPosition.ToString(),
                Radius.ToString());
        }
        /// <inheritdoc/>
        public readonly string ToString(string format)
        {
            if (format == null) return ToString();

            return string.Format(
                CultureInfo.CurrentCulture,
                BoundingCylinderStr,
                BasePosition.ToString(format, CultureInfo.CurrentCulture),
                CapPosition.ToString(format, CultureInfo.CurrentCulture),
                Radius.ToString(format, CultureInfo.CurrentCulture));
        }
        /// <inheritdoc/>
        public readonly string ToString(IFormatProvider formatProvider)
        {
            return string.Format(
                formatProvider,
                BoundingCylinderStr,
                BasePosition.ToString(),
                CapPosition.ToString(),
                Radius.ToString());
        }
        /// <inheritdoc/>
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) return ToString(formatProvider);

            return string.Format(
                formatProvider,
                BoundingCylinderStr,
                BasePosition.ToString(format, formatProvider),
                CapPosition.ToString(format, formatProvider),
                Radius.ToString(format, formatProvider));
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Center, Radius, Height);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ref BoundingCylinder value)
        {
            return NearEqual(this, value);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(BoundingCylinder value)
        {
            return Equals(ref value);
        }
        /// <inheritdoc/>
        public readonly override bool Equals(object value)
        {
            if (value is not BoundingCylinder) return false;

            var strongValue = (BoundingCylinder)value;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Gets whether the two BoundingCylinder structures are equal.
        /// </summary>
        /// <param name="a">The first BoundingCylinder structure to compare.</param>
        /// <param name="b">The second BoundingCylinder structure to compare.</param>
        /// <param name="epsilon">The tolerance for equality.</param>
        /// <returns>true if the two structures are equal; otherwise, false.</returns>
        public static bool NearEqual(BoundingCylinder a, BoundingCylinder b, float epsilon = Helper.ZeroTolerance)
        {
            return
                Helper.WithinEpsilon(a.Radius, b.Radius, epsilon) &&
                Helper.WithinEpsilon(a.Height, b.Height, epsilon) &&
                Vector3.NearEqual(a.Center, b.Center, epsilon);
        }
    }
}
