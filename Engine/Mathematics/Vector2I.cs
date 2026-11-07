using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Mathematics
{
    /// <summary>
    /// A 2d vector represented by integers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vector2I : IEquatable<Vector2I>, IFormattable
    {
        const string Vector2IStr = "X:{0} Y:{1}";
        const string Vector2IDescStr = "X:{0:0}; Y:{1:0}";

        /// <summary>
        /// The size of the <see cref="Vector2I"/> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf<Vector2I>();

        /// <summary>
        /// A <see cref="Vector2I"/> with all of its components set to zero.
        /// </summary>
        public static readonly Vector2I Zero = new(0, 0);
        /// <summary>
        /// A <see cref="Vector2I"/> with all of its components set to one.
        /// </summary>
        public static readonly Vector2I One = new(1, 1);

        /// <summary>
        /// The X unit <see cref="Vector2I"/> (1, 0).
        /// </summary>
        public static readonly Vector2I UnitX = new(1, 0);
        /// <summary>
        /// The Y unit <see cref="Vector2I"/> (0, 1).
        /// </summary>
        public static readonly Vector2I UnitY = new(0, 1);

        /// <summary>
        /// The X coordinate.
        /// </summary>
        public int X;
        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public int Y;

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public readonly bool IsZero => X == 0 && Y == 0;

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X or Y component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component and 1 for the Y component.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 1].</exception>
        public int this[int index]
        {
            readonly get => index switch
            {
                0 => X,
                1 => Y,
                _ => throw new ArgumentOutOfRangeException(nameof(index), "Indices for Vector2I run from 0 to 1, inclusive."),
            };
            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    default: throw new ArgumentOutOfRangeException(nameof(index), "Indices for Vector2I run from 0 to 1, inclusive.");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2I"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Vector2I(int value)
        {
            X = value;
            Y = value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2I"/> struct.
        /// </summary>
        /// <param name="x">Initial value for the X component of the vector.</param>
        /// <param name="y">Initial value for the Y component of the vector.</param>
        public Vector2I(int x, int y)
        {
            X = x;
            Y = y;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2I"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X and Y components of the vector. This must be an array with two elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector2I(int[] values)
        {
            ArgumentNullException.ThrowIfNull(values);
            if (values.Length != 2) throw new ArgumentOutOfRangeException(nameof(values), "There must be two and only two input values for Vector2I.");

            X = values[0];
            Y = values[1];
        }

        /// <summary>
        /// Creates an array containing the elements of the vector.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public readonly int[] ToArray()
        {
            return [X, Y];
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="left">The first vector to add.</param>
        /// <param name="right">The second vector to add.</param>
        /// <param name="result">When the method completes, contains the sum of the two vectors.</param>
        public static void Add(ref Vector2I left, ref Vector2I right, out Vector2I result)
        {
            result = new(left.X + right.X, left.Y + right.Y);
        }
        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="left">The first vector to add.</param>
        /// <param name="right">The second vector to add.</param>
        /// <returns>The sum of the two vectors.</returns>
        public static Vector2I Add(Vector2I left, Vector2I right)
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
        public static void Add(ref Vector2I left, int right, out Vector2I result)
        {
            result = new(left.X + right, left.Y + right);
        }
        /// <summary>
        /// Perform a component-wise addition
        /// </summary>
        /// <param name="left">The input vector</param>
        /// <param name="right">The scalar value to be added to elements</param>
        /// <returns>The vector with added scalar for each element.</returns>
        public static Vector2I Add(Vector2I left, int right)
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
        public static void Subtract(ref Vector2I left, ref Vector2I right, out Vector2I result)
        {
            result = new(left.X - right.X, left.Y - right.Y);
        }
        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="left">The first vector to subtract.</param>
        /// <param name="right">The second vector to subtract.</param>
        /// <returns>The difference of the two vectors.</returns>
        public static Vector2I Subtract(Vector2I left, Vector2I right)
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
        public static void Subtract(ref Vector2I left, int right, out Vector2I result)
        {
            result = new(left.X - right, left.Y - right);
        }
        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="left">The input vector</param>
        /// <param name="right">The scalar value to be subtraced from elements</param>
        /// <returns>The vector with subtracted scalar for each element.</returns>
        public static Vector2I Subtract(Vector2I left, int right)
        {
            Subtract(ref left, right, out var result);
            return result;
        }
        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="left">The scalar value to be subtraced from elements</param>
        /// <param name="right">The input vector</param>
        /// <param name="result">The vector with subtracted scalar for each element.</param>
        public static void Subtract(int left, ref Vector2I right, out Vector2I result)
        {
            result = new(left - right.X, left - right.Y);
        }
        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="left">The scalar value to be subtraced from elements</param>
        /// <param name="right">The input vector</param>
        /// <returns>The vector with subtracted scalar for each element.</returns>
        public static Vector2I Subtract(int left, Vector2I right)
        {
            Subtract(left, ref right, out var result);
            return result;
        }

        /// <summary>
        /// Reverses the direction of a given vector.
        /// </summary>
        /// <param name="value">The vector to negate.</param>
        /// <param name="result">When the method completes, contains a vector facing in the opposite direction.</param>
        public static void Negate(ref Vector2I value, out Vector2I result)
        {
            result = new(-value.X, -value.Y);
        }
        /// <summary>
        /// Reverses the direction of a given vector.
        /// </summary>
        /// <param name="value">The vector to negate.</param>
        /// <returns>A vector facing in the opposite direction.</returns>
        public static Vector2I Negate(Vector2I value)
        {
            Negate(ref value, out var result);
            return result;
        }

        /// <summary>
        /// Returns per component absolute value of a vector
        /// </summary>
        /// <param name="value">Input vector</param>
        /// <param name="result">When the method completes, contains a vector with each component being the absolute value of the input component</param>
        public static void Abs(ref Vector2I value, out Vector2I result)
        {
            result = new(
                value.X > 0.0f ? value.X : -value.X,
                value.Y > 0.0f ? value.Y : -value.Y);
        }
        /// <summary>
        /// Returns per component absolute value of a vector
        /// </summary>
        /// <param name="value">Input vector</param>
        /// <returns>A vector with each component being the absolute value of the input component</returns>
        public static Vector2I Abs(Vector2I value)
        {
            Abs(ref value, out var result);
            return result;
        }

        /// <summary>
        /// Returns a vector containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <param name="result">When the method completes, contains an new vector composed of the largest components of the source vectors.</param>
        public static void Max(ref Vector2I left, ref Vector2I right, out Vector2I result)
        {
            result.X = (left.X > right.X) ? left.X : right.X;
            result.Y = (left.Y > right.Y) ? left.Y : right.Y;
        }
        /// <summary>
        /// Returns a vector containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>A vector containing the largest components of the source vectors.</returns>
        public static Vector2I Max(Vector2I left, Vector2I right)
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
        public static void Min(ref Vector2I left, ref Vector2I right, out Vector2I result)
        {
            result.X = (left.X < right.X) ? left.X : right.X;
            result.Y = (left.Y < right.Y) ? left.Y : right.Y;
        }
        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>A vector containing the smallest components of the source vectors.</returns>
        public static Vector2I Min(Vector2I left, Vector2I right)
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
        public static Vector2I operator +(Vector2I left, Vector2I right)
        {
            Add(ref left, ref right, out var result);
            return result;
        }
        /// <summary>
        /// Assert a vector (return it unchanged).
        /// </summary>
        /// <param name="value">The vector to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) vector.</returns>
        public static Vector2I operator +(Vector2I value)
        {
            return value;
        }
        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="left">The first vector to subtract.</param>
        /// <param name="right">The second vector to subtract.</param>
        /// <returns>The difference of the two vectors.</returns>
        public static Vector2I operator -(Vector2I left, Vector2I right)
        {
            Subtract(ref left, ref right, out var result);
            return result;
        }
        /// <summary>
        /// Reverses the direction of a given vector.
        /// </summary>
        /// <param name="value">The vector to negate.</param>
        /// <returns>A vector facing in the opposite direction.</returns>
        public static Vector2I operator -(Vector2I value)
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
        public static Vector2I operator +(Vector2I value, int scalar)
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
        public static Vector2I operator +(int scalar, Vector2I value)
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
        public static Vector2I operator -(Vector2I value, int scalar)
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
        public static Vector2I operator -(int scalar, Vector2I value)
        {
            Subtract(scalar, ref value, out var result);
            return result;
        }

        /// <inheritdoc/>
        public static bool operator ==(Vector2I left, Vector2I right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        public static bool operator !=(Vector2I left, Vector2I right)
        {
            return !left.Equals(ref right);
        }

        /// <inheritdoc/>
        public static explicit operator Vector2I(Vortice.Mathematics.SizeI Value)
        {
            return new(Value.Width, Value.Height);
        }
        /// <inheritdoc/>
        public static explicit operator Vortice.Mathematics.SizeI(Vector2I Value)
        {
            return new(Value.X, Value.Y);
        }

        /// <inheritdoc/>
        public static explicit operator Vector2I(Vortice.Mathematics.Int2 Value)
        {
            return new(Value.X, Value.Y);
        }
        /// <inheritdoc/>
        public static explicit operator Vortice.Mathematics.Int2(Vector2I Value)
        {
            return new(Value.X, Value.Y);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Vector2IStr,
                X, Y);
        }
        /// <inheritdoc/>
        public readonly string ToString(string format)
        {
            if (format == null) return ToString();

            return string.Format(
                CultureInfo.CurrentCulture,
                Vector2IStr,
                X.ToString(format, CultureInfo.CurrentCulture),
                Y.ToString(format, CultureInfo.CurrentCulture));
        }
        /// <inheritdoc/>
        public readonly string ToString(IFormatProvider formatProvider)
        {
            return string.Format(
                formatProvider,
                Vector2IStr,
                X, Y);
        }
        /// <inheritdoc/>
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) ToString(formatProvider);

            return string.Format(
                formatProvider,
                Vector2IStr,
                X.ToString(format, formatProvider),
                Y.ToString(format, formatProvider));
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ref Vector2I other)
        {
            return
                X == other.X &&
                Y == other.Y;
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Vector2I other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object value)
        {
            if (value is not Vector2I) return false;

            var strongValue = (Vector2I)value;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Gets vector description
        /// </summary>
        /// <returns>Return vector description</returns>
        public readonly string GetDescription()
        {
            return string.Format(
                Vector2IDescStr,
                X, Y);
        }
    }
}
