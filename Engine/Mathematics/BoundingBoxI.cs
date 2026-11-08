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
    /// A bounding box for vertices in a <see cref="PolyMesh"/>.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BoundingBoxI"/> struct.
    /// </remarks>
    /// <param name="minimum">The lower bound of the bounding box.</param>
    /// <param name="maximum">The upper bound of the bounding box.</param>
    [StructLayout(LayoutKind.Sequential)]
    [JsonObject(MemberSerialization.OptIn)]
    public struct BoundingBoxI(Vector3I minimum, Vector3I maximum) : IEquatable<BoundingBoxI>, IFormattable
    {
        const string BoundingBoxIStr = "Minimum:{0} Maximum:{1}";

        /// <summary>
        /// The lower bound of the bounding box.
        /// </summary>
        [JsonProperty]
        public Vector3I Minimum = minimum;
        /// <summary>
        /// The upper bound of the bounding box.
        /// </summary>
        [JsonProperty]
        public Vector3I Maximum = maximum;
        /// <summary>
        /// Returns the width of the bounding box
        /// </summary>
        public readonly int Width => Maximum.X - Minimum.X;
        /// <summary>
        /// Returns the height of the bounding box
        /// </summary>
        public readonly int Height => Maximum.Y - Minimum.Y;
        /// <summary>
        /// Returns the height of the bounding box
        /// </summary>
        public readonly int Depth => Maximum.Z - Minimum.Z;
        /// <summary>
        /// Returns the size of the bounding box
        /// </summary>
        public readonly Vector3I Size => Maximum - Minimum;

        /// <summary>
        /// Retrieves the eight corners of the bounding box.
        /// </summary>
        /// <returns>An array of points representing the eight corners of the bounding box.</returns>
        public readonly Vector3I[] GetCorners()
        {
            Vector3I[] results = new Vector3I[8];
            GetCorners(results);
            return results;
        }
        /// <summary>
        /// Retrieves the eight corners of the bounding box.
        /// </summary>
        /// <returns>An array of points representing the eight corners of the bounding box.</returns>
        public readonly void GetCorners(Vector3I[] corners)
        {
            corners[0] = new Vector3I(Minimum.X, Maximum.Y, Maximum.Z);
            corners[1] = new Vector3I(Maximum.X, Maximum.Y, Maximum.Z);
            corners[2] = new Vector3I(Maximum.X, Minimum.Y, Maximum.Z);
            corners[3] = new Vector3I(Minimum.X, Minimum.Y, Maximum.Z);
            corners[4] = new Vector3I(Minimum.X, Maximum.Y, Minimum.Z);
            corners[5] = new Vector3I(Maximum.X, Maximum.Y, Minimum.Z);
            corners[6] = new Vector3I(Maximum.X, Minimum.Y, Minimum.Z);
            corners[7] = new Vector3I(Minimum.X, Minimum.Y, Minimum.Z);
        }
        /// <summary>
        /// Gets the specified vertex
        /// </summary>
        /// <param name="vertex">Box vertex</param>
        public readonly Vector3I GetVertex(BoxVertices vertex)
        {
            return GetVertex(GetVertices(), (int)vertex);
        }
        /// <summary>
        /// Gets the specified vertex
        /// </summary>
        /// <param name="index">Vertex index</param>
        public readonly Vector3I GetVertex(int index)
        {
            return GetVertex(GetVertices(), index);
        }
        /// <summary>
        /// Gets the bounding box vertices
        /// </summary>
        public readonly IEnumerable<Vector3I> GetVertices()
        {
            yield return new Vector3I(Maximum.X, Maximum.Y, Maximum.Z);
            yield return new Vector3I(Maximum.X, Maximum.Y, Minimum.Z);
            yield return new Vector3I(Minimum.X, Maximum.Y, Minimum.Z);
            yield return new Vector3I(Minimum.X, Maximum.Y, Maximum.Z);
            yield return new Vector3I(Maximum.X, Minimum.Y, Maximum.Z);
            yield return new Vector3I(Maximum.X, Minimum.Y, Minimum.Z);
            yield return new Vector3I(Minimum.X, Minimum.Y, Minimum.Z);
            yield return new Vector3I(Minimum.X, Minimum.Y, Maximum.Z);
        }
        /// <summary>
        /// Gets the XY rectangle of the box
        /// </summary>
        /// <returns>Returns the XY rectangle of the box</returns>
        public RectI GetRectangleXY()
        {
            return new()
            {
                Left = Minimum.X,
                Top = Minimum.Y,
                Right = Maximum.X,
                Bottom = Maximum.Y,
            };
        }
        /// <summary>
        /// Gets the XZ rectangle of the box
        /// </summary>
        /// <returns>Returns the XZ rectangle of the box</returns>
        public RectI GetRectangleXZ()
        {
            return new()
            {
                Left = Minimum.X,
                Top = Minimum.Z,
                Right = Maximum.X,
                Bottom = Maximum.Z,
            };
        }

        /// <summary>
        /// Constructs a <see cref="BoundingBoxI"/> that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the box.</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is <c>null</c>.</exception>
        public static void FromPoints(IEnumerable<Vector3I> points, out BoundingBoxI result)
        {
            ArgumentNullException.ThrowIfNull(points);

            Vector3I min = new(int.MaxValue);
            Vector3I max = new(int.MinValue);

            foreach (var p in points)
            {
                min = Vector3I.Min(min, p);
                max = Vector3I.Max(max, p);
            }

            result = new(min, max);
        }

        /// <summary>
        /// Constructs a <see cref="BoundingBoxI"/> that is as large as the total combined area of the two specified boxes.
        /// </summary>
        /// <param name="value1">The first box to merge.</param>
        /// <param name="value2">The second box to merge.</param>
        /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
        public static void Merge(ref BoundingBoxI value1, ref BoundingBoxI value2, out BoundingBoxI result)
        {
            result.Minimum = Vector3I.Min(value1.Minimum, value2.Minimum);
            result.Maximum = Vector3I.Max(value1.Maximum, value2.Maximum);
        }
        /// <summary>
        /// Constructs a <see cref="BoundingBoxI"/> that is as large as the total combined area of the two specified boxes.
        /// </summary>
        /// <param name="value1">The first box to merge.</param>
        /// <param name="value2">The second box to merge.</param>
        /// <returns>The newly constructed bounding box.</returns>
        public static BoundingBoxI Merge(BoundingBoxI value1, BoundingBoxI value2)
        {
            BoundingBoxI box;
            box.Minimum = Vector3I.Min(value1.Minimum, value2.Minimum);
            box.Maximum = Vector3I.Max(value1.Maximum, value2.Maximum);
            return box;
        }
        /// <summary>
        /// Merges the array of bounding boxes into one bounding box
        /// </summary>
        /// <param name="boxes">Array of boxes</param>
        /// <returns>Returns a bounding box containing all the boxes in the array</returns>
        public static BoundingBoxI Merge(IEnumerable<BoundingBoxI> boxes)
        {
            var fbbox = new BoundingBoxI();

            if (boxes?.Any() != true)
            {
                return fbbox;
            }

            var defaultBox = new BoundingBoxI();

            foreach (var bbox in boxes.Where(b => b != defaultBox))
            {
                fbbox = Merge(fbbox, bbox);
            }

            return fbbox;
        }

        /// <summary>
        /// Gets the specified vertex
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="vertex">Box vertex</param>
        public static Vector3I GetVertex(IEnumerable<Vector3I> vertices, BoxVertices vertex)
        {
            return vertices.ElementAtOrDefault((int)vertex);
        }
        /// <summary>
        /// Gets the specified vertex
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="index">Vertex index</param>
        public static Vector3I GetVertex(IEnumerable<Vector3I> vertices, int index)
        {
            return vertices.ElementAtOrDefault(index);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoundingBoxI left, BoundingBoxI right)
        {
            return left.Equals(right);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoundingBoxI left, BoundingBoxI right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                BoundingBoxIStr,
                Minimum.ToString(),
                Maximum.ToString());
        }
        /// <inheritdoc/>
        public readonly string ToString(string format)
        {
            if (format == null) return ToString();

            return string.Format(
                CultureInfo.CurrentCulture,
                BoundingBoxIStr,
                Minimum.ToString(format, CultureInfo.CurrentCulture),
                Maximum.ToString(format, CultureInfo.CurrentCulture));
        }
        /// <inheritdoc/>
        public readonly string ToString(IFormatProvider formatProvider)
        {
            return string.Format(
                formatProvider,
                BoundingBoxIStr,
                Minimum.ToString(),
                Maximum.ToString());
        }
        /// <inheritdoc/>
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) return ToString(formatProvider);

            return string.Format(
                formatProvider,
                BoundingBoxIStr,
                Minimum.ToString(format, formatProvider),
                Maximum.ToString(format, formatProvider));
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Minimum, Maximum);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ref BoundingBoxI value)
        {
            return
                Minimum == value.Minimum &&
                Maximum == value.Maximum;
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(BoundingBoxI value)
        {
            return Equals(ref value);
        }
        /// <inheritdoc/>
        public readonly override bool Equals(object value)
        {
            if (value is not BoundingBoxI) return false;

            var strongValue = (BoundingBoxI)value;
            return Equals(ref strongValue);
        }
    }
}
