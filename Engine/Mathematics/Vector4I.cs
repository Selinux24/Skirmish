using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Mathematics
{
    /// <summary>
    /// Represents a four dimensional mathematical vector.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [JsonObject(MemberSerialization.OptIn)]
    public struct Vector4I : IEquatable<Vector4I>, IFormattable
    {
        const string Vector4IStr = "X:{0} Y:{1} Z:{2} W:{3}";
        const string Vector4IDescStr = "X:{0:0}; Y:{1:0}; Z:{2:0}; W:{3:0}";

        /// <summary>
        /// The size of the <see cref="Vector4I"/> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf<Vector4I>();

        /// <summary>
        /// A <see cref="Vector4I"/> with all of its components set to zero.
        /// </summary>
        public static readonly Vector4I Zero = new();
        /// <summary>
        /// A <see cref="Vector4I"/> with all of its components set to one.
        /// </summary>
        public static readonly Vector4I One = new(1, 1, 1, 1);

        /// <summary>
        /// The X unit <see cref="Vector4I"/> (1, 0, 0, 0).
        /// </summary>
        public static readonly Vector4I UnitX = new(1, 0, 0, 0);
        /// <summary>
        /// The Y unit <see cref="Vector4I"/> (0, 1, 0, 0).
        /// </summary>
        public static readonly Vector4I UnitY = new(0, 1, 0, 0);
        /// <summary>
        /// The Z unit <see cref="Vector4I"/> (0, 0, 1, 0).
        /// </summary>
        public static readonly Vector4I UnitZ = new(0, 0, 1, 0);
        /// <summary>
        /// The W unit <see cref="Vector4I"/> (0, 0, 0, 1).
        /// </summary>
        public static readonly Vector4I UnitW = new(0, 0, 0, 1);

        /// <summary>
        /// The X component of the vector.
        /// </summary>
        [JsonProperty]
        public int X;
        /// <summary>
        /// The Y component of the vector.
        /// </summary>
        [JsonProperty]
        public int Y;
        /// <summary>
        /// The Z component of the vector.
        /// </summary>
        [JsonProperty]
        public int Z;
        /// <summary>
        /// The W component of the vector.
        /// </summary>
        [JsonProperty]
        public int W;

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public readonly bool IsZero => X == 0 && Y == 0 && Z == 0 && W == 0;

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y, Z, or W component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component, 2 for the Z component, and 3 for the W component.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 3].</exception>
        public int this[int index]
        {
            readonly get => index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                3 => W,
                _ => throw new ArgumentOutOfRangeException(nameof(index), "Indices for Vector4I run from 0 to 3, inclusive."),
            };
            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    case 3: W = value; break;
                    default: throw new ArgumentOutOfRangeException(nameof(index), "Indices for Vector4I run from 0 to 3, inclusive.");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4I"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Vector4I(int value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4I"/> struct.
        /// </summary>
        /// <param name="x">Initial value for the X component of the vector.</param>
        /// <param name="y">Initial value for the Y component of the vector.</param>
        /// <param name="z">Initial value for the Z component of the vector.</param>
        /// <param name="w">Initial value for the W component of the vector.</param>
        public Vector4I(int x, int y, int z, int w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4I"/> struct.
        /// </summary>
        /// <param name="value">A vector containing the values with which to initialize the X, Y, and Z components.</param>
        /// <param name="w">Initial value for the W component of the vector.</param>
        public Vector4I(Vector3I value, int w)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = w;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4I"/> struct.
        /// </summary>
        /// <param name="value">A vector containing the values with which to initialize the X and Y components.</param>
        /// <param name="z">Initial value for the Z component of the vector.</param>
        /// <param name="w">Initial value for the W component of the vector.</param>
        public Vector4I(Vector2I value, int z, int w)
        {
            X = value.X;
            Y = value.Y;
            Z = z;
            W = w;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4I"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, Z, and W components of the vector. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public Vector4I(int[] values)
        {
            ArgumentNullException.ThrowIfNull(values);
            if (values.Length != 4) throw new ArgumentOutOfRangeException(nameof(values), "There must be four and only four input values for Vector4I.");

            X = values[0];
            Y = values[1];
            Z = values[2];
            W = values[3];
        }

        /// <summary>
        /// Creates an array containing the elements of the vector.
        /// </summary>
        /// <returns>A four-element array containing the components of the vector.</returns>
        public readonly int[] ToArray()
        {
            return [X, Y, Z, W];
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="left">The first vector to add.</param>
        /// <param name="right">The second vector to add.</param>
        /// <param name="result">When the method completes, contains the sum of the two vectors.</param>
        public static void Add(ref Vector4I left, ref Vector4I right, out Vector4I result)
        {
            result = new(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        }
        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="left">The first vector to add.</param>
        /// <param name="right">The second vector to add.</param>
        /// <returns>The sum of the two vectors.</returns>
        public static Vector4I Add(Vector4I left, Vector4I right)
        {
            Add(ref left, ref right, out var result);
            return result;
        }
        /// <summary>
        /// Perform a component-wise addition
        /// </summary>
        /// <param name="left">The input vector</param>
        /// <param name="right">The scalar value to be added to elements</param>
        /// <param name="result">The vector with added scalar for each element.</param>
        public static void Add(ref Vector4I left, int right, out Vector4I result)
        {
            result = new(left.X + right, left.Y + right, left.Z + right, left.W + right);
        }
        /// <summary>
        /// Perform a component-wise addition
        /// </summary>
        /// <param name="left">The input vector</param>
        /// <param name="right">The scalar value to be added to elements</param>
        /// <returns>The vector with added scalar for each element.</returns>
        public static Vector4I Add(Vector4I left, int right)
        {
            Add(ref left, right, out var result);
            return result;
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="left">The first vector to subtract.</param>
        /// <param name="right">The second vector to subtract.</param>
        /// <param name="result">When the method completes, contains the difference of the two vectors.</param>
        public static void Subtract(ref Vector4I left, ref Vector4I right, out Vector4I result)
        {
            result = new(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        }
        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="left">The first vector to subtract.</param>
        /// <param name="right">The second vector to subtract.</param>
        /// <returns>The difference of the two vectors.</returns>
        public static Vector4I Subtract(Vector4I left, Vector4I right)
        {
            Subtract(ref left, ref right, out var result);
            return result;
        }
        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="left">The input vector</param>
        /// <param name="right">The scalar value to be subtraced from elements</param>
        /// <param name="result">The vector with subtracted scalar for each element.</param>
        public static void Subtract(ref Vector4I left, int right, out Vector4I result)
        {
            result = new(left.X - right, left.Y - right, left.Z - right, left.W - right);
        }
        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="left">The input vector</param>
        /// <param name="right">The scalar value to be subtraced from elements</param>
        /// <returns>The vector with subtracted scalar for each element.</returns>
        public static Vector4I Subtract(Vector4I left, int right)
        {
            Subtract(ref left, right, out var result);
            return result;
        }
        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="left">The scalar value to be subtraced from elements</param>
        /// <param name="right">The input vector.</param>
        /// <param name="result">The vector with subtracted scalar for each element.</param>
        public static void Subtract(int left, ref Vector4I right, out Vector4I result)
        {
            result = new(left - right.X, left - right.Y, left - right.Z, left - right.W);
        }
        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="left">The scalar value to be subtraced from elements</param>
        /// <param name="right">The input vector.</param>
        /// <returns>The vector with subtracted scalar for each element.</returns>
        public static Vector4I Subtract(int left, Vector4I right)
        {
            Subtract(left, ref right, out var result);
            return result;
        }

        /// <summary>
        /// Reverses the direction of a given vector.
        /// </summary>
        /// <param name="value">The vector to negate.</param>
        /// <param name="result">When the method completes, contains a vector facing in the opposite direction.</param>
        public static void Negate(ref Vector4I value, out Vector4I result)
        {
            result = new(-value.X, -value.Y, -value.Z, -value.W);
        }
        /// <summary>
        /// Reverses the direction of a given vector.
        /// </summary>
        /// <param name="value">The vector to negate.</param>
        /// <returns>A vector facing in the opposite direction.</returns>
        public static Vector4I Negate(Vector4I value)
        {
            Negate(ref value, out var result);
            return result;
        }

        /// <summary>
        /// Returns a vector containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <param name="result">When the method completes, contains an new vector composed of the largest components of the source vectors.</param>
        public static void Max(ref Vector4I left, ref Vector4I right, out Vector4I result)
        {
            result.X = (left.X > right.X) ? left.X : right.X;
            result.Y = (left.Y > right.Y) ? left.Y : right.Y;
            result.Z = (left.Z > right.Z) ? left.Z : right.Z;
            result.W = (left.W > right.W) ? left.W : right.W;
        }
        /// <summary>
        /// Returns a vector containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>A vector containing the largest components of the source vectors.</returns>
        public static Vector4I Max(Vector4I left, Vector4I right)
        {
            Max(ref left, ref right, out var result);
            return result;
        }

        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <param name="result">When the method completes, contains an new vector composed of the smallest components of the source vectors.</param>
        public static void Min(ref Vector4I left, ref Vector4I right, out Vector4I result)
        {
            result.X = (left.X < right.X) ? left.X : right.X;
            result.Y = (left.Y < right.Y) ? left.Y : right.Y;
            result.Z = (left.Z < right.Z) ? left.Z : right.Z;
            result.W = (left.W < right.W) ? left.W : right.W;
        }
        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>A vector containing the smallest components of the source vectors.</returns>
        public static Vector4I Min(Vector4I left, Vector4I right)
        {
            Min(ref left, ref right, out var result);
            return result;
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="left">The first vector to add.</param>
        /// <param name="right">The second vector to add.</param>
        /// <returns>The sum of the two vectors.</returns>
        public static Vector4I operator +(Vector4I left, Vector4I right)
        {
            Add(ref left, ref right, out var result);
            return result;
        }
        /// <summary>
        /// Assert a vector (return it unchanged).
        /// </summary>
        /// <param name="value">The vector to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) vector.</returns>
        public static Vector4I operator +(Vector4I value)
        {
            return value;
        }
        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="left">The first vector to subtract.</param>
        /// <param name="right">The second vector to subtract.</param>
        /// <returns>The difference of the two vectors.</returns>
        public static Vector4I operator -(Vector4I left, Vector4I right)
        {
            Subtract(ref left, ref right, out var result);
            return result;
        }
        /// <summary>
        /// Reverses the direction of a given vector.
        /// </summary>
        /// <param name="value">The vector to negate.</param>
        /// <returns>A vector facing in the opposite direction.</returns>
        public static Vector4I operator -(Vector4I value)
        {
            Negate(ref value, out var result);
            return result;
        }
        /// <summary>
        /// Perform a component-wise addition
        /// </summary>
        /// <param name="value">The input vector.</param>
        /// <param name="scalar">The scalar value to be added on elements</param>
        /// <returns>The vector with added scalar for each element.</returns>
        public static Vector4I operator +(Vector4I value, int scalar)
        {
            Add(ref value, scalar, out var result);
            return result;
        }
        /// <summary>
        /// Perform a component-wise addition
        /// </summary>
        /// <param name="value">The input vector.</param>
        /// <param name="scalar">The scalar value to be added on elements</param>
        /// <returns>The vector with added scalar for each element.</returns>
        public static Vector4I operator +(int scalar, Vector4I value)
        {
            Add(ref value, scalar, out var result);
            return result;
        }
        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="value">The input vector.</param>
        /// <param name="scalar">The scalar value to be subtraced from elements</param>
        /// <returns>The vector with subtraced scalar from each element.</returns>
        public static Vector4I operator -(Vector4I value, int scalar)
        {
            Subtract(ref value, scalar, out var result);
            return result;
        }
        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="value">The input vector.</param>
        /// <param name="scalar">The scalar value to be subtraced from elements</param>
        /// <returns>The vector with subtraced scalar from each element.</returns>
        public static Vector4I operator -(int scalar, Vector4I value)
        {
            Subtract(scalar, ref value, out var result);
            return result;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector4I left, Vector4I right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector4I left, Vector4I right)
        {
            return !left.Equals(ref right);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Vector4IStr,
                X, Y, Z, W);
        }
        /// <inheritdoc/>
        public readonly string ToString(string format)
        {
            if (format == null) return ToString();

            return string.Format(
                CultureInfo.CurrentCulture,
                Vector4IStr,
                X.ToString(format, CultureInfo.CurrentCulture),
                Y.ToString(format, CultureInfo.CurrentCulture),
                Z.ToString(format, CultureInfo.CurrentCulture),
                W.ToString(format, CultureInfo.CurrentCulture));
        }
        /// <inheritdoc/>
        public readonly string ToString(IFormatProvider formatProvider)
        {
            return string.Format(
                formatProvider,
                Vector4IStr,
                X, Y, Z, W);
        }
        /// <inheritdoc/>
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) ToString(formatProvider);

            return string.Format(
                formatProvider,
                Vector4IStr,
                X.ToString(format, formatProvider),
                Y.ToString(format, formatProvider),
                Z.ToString(format, formatProvider),
                W.ToString(format, formatProvider));
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z, W);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ref Vector4I other)
        {
            return
                X == other.X &&
                Y == other.Y &&
                Z == other.Z &&
                W == other.W;
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Vector4I other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            if (obj is not Vector4I) return false;

            var strongValue = (Vector4I)obj;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Gets vector description
        /// </summary>
        /// <returns>Return vector description</returns>
        public readonly string GetDescription()
        {
            return string.Format(
                Vector4IDescStr,
                X, Y, Z, W);
        }
    }
}
