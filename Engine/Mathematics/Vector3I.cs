using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Mathematics
{
    /// <summary>
    /// A 3d vector represented by integers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vector3I : IEquatable<Vector3I>, IFormattable
    {
        const string Vector3IStr = "X:{0} Y:{1} Z:{2}";
        const string Vector3IDescStr = "X:{0:0}; Y:{1:0}; Z:{2:0}";

        /// <summary>
        /// The size of the <see cref="Vector3I"/> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf<Vector3I>();

        /// <summary>
        /// A <see cref="Vector3I"/> with all of its components set to zero.
        /// </summary>
        public static readonly Vector3I Zero = new();
        /// <summary>
        /// A <see cref="Vector3I"/> with all of its components set to one.
        /// </summary>
        public static readonly Vector3I One = new(1, 1, 1);
        /// <summary>
        /// A unit <see cref="Vector3I"/> designating up (0, 1, 0).
        /// </summary>
        public static readonly Vector3I Up = new(0, 1, 0);
        /// <summary>
        /// A unit <see cref="Vector3I"/> designating down (0, -1, 0).
        /// </summary>
        public static readonly Vector3I Down = new(0, -1, 0);
        /// <summary>
        /// A unit <see cref="Vector3I"/> designating left (-1, 0, 0).
        /// </summary>
        public static readonly Vector3I Left = new(-1, 0, 0);
        /// <summary>
        /// A unit <see cref="Vector3I"/> designating right (1, 0, 0).
        /// </summary>
        public static readonly Vector3I Right = new(1, 0, 0);
        /// <summary>
        /// A unit <see cref="Vector3I"/> designating forward in a right-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3I ForwardRH = new(0, 0, -1);
        /// <summary>
        /// A unit <see cref="Vector3I"/> designating forward in a left-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3I ForwardLH = new(0, 0, 1);
        /// <summary>
        /// A unit <see cref="Vector3I"/> designating backward in a right-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3I BackwardRH = new(0, 0, 1);
        /// <summary>
        /// A unit <see cref="Vector3I"/> designating backward in a left-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3I BackwardLH = new(0, 0, -1);

        /// <summary>
        /// The X unit <see cref="Vector3I"/> (1, 0, 0).
        /// </summary>
        public static readonly Vector3I UnitX = new(1, 0, 0);
        /// <summary>
        /// The Y unit <see cref="Vector3I"/> (0, 1, 0).
        /// </summary>
        public static readonly Vector3I UnitY = new(0, 1, 0);
        /// <summary>
        /// The Z unit <see cref="Vector3I"/> (0, 0, 1).
        /// </summary>
        public static readonly Vector3I UnitZ = new(0, 0, 1);

        /// <summary>
        /// The X coordinate.
        /// </summary>
        public int X;
        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public int Y;
        /// <summary>
        /// The Z coordinate.
        /// </summary>
        public int Z;

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public readonly bool IsZero => X == 0 && Y == 0 && Z == 0;

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y, or Z component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component, and 2 for the Z component.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 2].</exception>
        public int this[int index]
        {
            readonly get => index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                _ => throw new ArgumentOutOfRangeException(nameof(index), "Indices for Vector3I run from 0 to 2, inclusive."),
            };
            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    default: throw new ArgumentOutOfRangeException(nameof(index), "Indices for Vector3I run from 0 to 2, inclusive.");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3I"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Vector3I(int value)
        {
            X = value;
            Y = value;
            Z = value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3I"/> struct.
        /// </summary>
        /// <param name="x">Initial value for the X component of the vector.</param>
        /// <param name="y">Initial value for the Y component of the vector.</param>
        /// <param name="z">Initial value for the Z component of the vector.</param>
        public Vector3I(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3I"/> struct.
        /// </summary>
        /// <param name="value">A vector containing the values with which to initialize the X and Y components.</param>
        /// <param name="z">Initial value for the Z component of the vector.</param>
        public Vector3I(Vector2I value, int z)
        {
            X = value.X;
            Y = value.Y;
            Z = z;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3I"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, and Z components of the vector. This must be an array with three elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than three elements.</exception>
        public Vector3I(int[] values)
        {
            ArgumentNullException.ThrowIfNull(values);
            if (values.Length != 3) throw new ArgumentOutOfRangeException(nameof(values), "There must be three and only three input values for Vector3I.");

            X = values[0];
            Y = values[1];
            Z = values[2];
        }

        /// <summary>
        /// Creates an array containing the elements of the vector.
        /// </summary>
        /// <returns>A three-element array containing the components of the vector.</returns>
        public readonly int[] ToArray()
        {
            return [X, Y, Z];
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="left">The first vector to add.</param>
        /// <param name="right">The second vector to add.</param>
        /// <param name="result">When the method completes, contains the sum of the two vectors.</param>
        public static void Add(ref Vector3I left, ref Vector3I right, out Vector3I result)
        {
            result = new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }
        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="left">The first vector to add.</param>
        /// <param name="right">The second vector to add.</param>
        /// <returns>The sum of the two vectors.</returns>
        public static Vector3I Add(Vector3I left, Vector3I right)
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
        public static void Add(ref Vector3I left, int right, out Vector3I result)
        {
            result = new(left.X + right, left.Y + right, left.Z + right);
        }
        /// <summary>
        /// Perform a component-wise addition
        /// </summary>
        /// <param name="left">The input vector</param>
        /// <param name="right">The scalar value to be added to elements</param>
        /// <returns>The vector with added scalar for each element.</returns>
        public static Vector3I Add(Vector3I left, int right)
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
        public static void Subtract(ref Vector3I left, ref Vector3I right, out Vector3I result)
        {
            result = new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }
        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="left">The first vector to subtract.</param>
        /// <param name="right">The second vector to subtract.</param>
        /// <returns>The difference of the two vectors.</returns>
        public static Vector3I Subtract(Vector3I left, Vector3I right)
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
        public static void Subtract(ref Vector3I left, int right, out Vector3I result)
        {
            result = new(left.X - right, left.Y - right, left.Z - right);
        }
        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="left">The input vector</param>
        /// <param name="right">The scalar value to be subtraced from elements</param>
        /// <returns>The vector with subtracted scalar for each element.</returns>
        public static Vector3I Subtract(Vector3I left, int right)
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
        public static void Subtract(int left, ref Vector3I right, out Vector3I result)
        {
            result = new(left - right.X, left - right.Y, left - right.Z);
        }
        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="left">The scalar value to be subtraced from elements</param>
        /// <param name="right">The input vector.</param>
        /// <returns>The vector with subtracted scalar for each element.</returns>
        public static Vector3I Subtract(int left, Vector3I right)
        {
            Subtract(left, ref right, out var result);
            return result;
        }

        /// <summary>
        /// Reverses the direction of a given vector.
        /// </summary>
        /// <param name="value">The vector to negate.</param>
        /// <param name="result">When the method completes, contains a vector facing in the opposite direction.</param>
        public static void Negate(ref Vector3I value, out Vector3I result)
        {
            result = new(-value.X, -value.Y, -value.Z);
        }
        /// <summary>
        /// Reverses the direction of a given vector.
        /// </summary>
        /// <param name="value">The vector to negate.</param>
        /// <returns>A vector facing in the opposite direction.</returns>
        public static Vector3I Negate(Vector3I value)
        {
            Negate(ref value, out var result);
            return result;
        }

        /// <summary>
        /// Returns per component absolute value of a vector
        /// </summary>
        /// <param name="value">Input vector</param>
        /// <param name="result">When the method completes, contains a vector with each component being the absolute value of the input component</param>
        public static void Abs(ref Vector3I value, out Vector3I result)
        {
            result = new(
                Math.Abs(value.X),
                Math.Abs(value.Y),
                Math.Abs(value.Z));
        }
        /// <summary>
        /// Returns per component absolute value of a vector
        /// </summary>
        /// <param name="value">Input vector</param>
        /// <returns>A vector with each component being the absolute value of the input component</returns>
        public static Vector3I Abs(Vector3I value)
        {
            Abs(ref value, out var result);
            return result;
        }

        /// <summary>
        /// Calculates the component-wise minimum of two vertices.
        /// </summary>
        /// <param name="a">A vertex.</param>
        /// <param name="b">Another vertex.</param>
        /// <returns>The component-wise minimum of the two vertices.</returns>
        public static Vector3I ComponentMin(Vector3I a, Vector3I b)
        {
            var x = a.X < b.X ? a.X : b.X;
            var y = a.Y < b.Y ? a.Y : b.Y;
            var z = a.Z < b.Z ? a.Z : b.Z;

            return new Vector3I(x, y, z);
        }
        /// <summary>
        /// Calculates the component-wise maximum of two vertices.
        /// </summary>
        /// <param name="a">A vertex.</param>
        /// <param name="b">Another vertex.</param>
        /// <returns>The component-wise maximum of the two vertices.</returns>
        public static Vector3I ComponentMax(Vector3I a, Vector3I b)
        {
            var x = a.X > b.X ? a.X : b.X;
            var y = a.Y > b.Y ? a.Y : b.Y;
            var z = a.Z > b.Z ? a.Z : b.Z;

            return new Vector3I(x, y, z);
        }

        /// <summary>
        /// Returns a vector containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <param name="result">When the method completes, contains an new vector composed of the largest components of the source vectors.</param>
        public static void Max(ref Vector3I left, ref Vector3I right, out Vector3I result)
        {
            result.X = (left.X > right.X) ? left.X : right.X;
            result.Y = (left.Y > right.Y) ? left.Y : right.Y;
            result.Z = (left.Z > right.Z) ? left.Z : right.Z;
        }
        /// <summary>
        /// Returns a vector containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>A vector containing the largest components of the source vectors.</returns>
        public static Vector3I Max(Vector3I left, Vector3I right)
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
        public static void Min(ref Vector3I left, ref Vector3I right, out Vector3I result)
        {
            result.X = (left.X < right.X) ? left.X : right.X;
            result.Y = (left.Y < right.Y) ? left.Y : right.Y;
            result.Z = (left.Z < right.Z) ? left.Z : right.Z;
        }
        /// <summary>
        /// Returns a vector containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>A vector containing the smallest components of the source vectors.</returns>
        public static Vector3I Min(Vector3I left, Vector3I right)
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
        public static Vector3I operator +(Vector3I left, Vector3I right)
        {
            Add(ref left, ref right, out var result);
            return result;
        }
        /// <summary>
        /// Assert a vector (return it unchanged).
        /// </summary>
        /// <param name="value">The vector to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) vector.</returns>
        public static Vector3I operator +(Vector3I value)
        {
            return value;
        }
        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="left">The first vector to subtract.</param>
        /// <param name="right">The second vector to subtract.</param>
        /// <returns>The difference of the two vectors.</returns>
        public static Vector3I operator -(Vector3I left, Vector3I right)
        {
            Subtract(ref left, ref right, out var result);
            return result;
        }
        /// <summary>
        /// Reverses the direction of a given vector.
        /// </summary>
        /// <param name="value">The vector to negate.</param>
        /// <returns>A vector facing in the opposite direction.</returns>
        public static Vector3I operator -(Vector3I value)
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
        public static Vector3I operator +(Vector3I value, int scalar)
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
        public static Vector3I operator +(int scalar, Vector3I value)
        {
            Add(ref value, scalar, out var result);
            return result;
        }
        /// <summary>
        /// Perform a component-wise subtraction
        /// </summary>
        /// <param name="value">The input vector.</param>
        /// <param name="scalar">The scalar value to be subtraced from elements</param>
        /// <returns>The vector with added scalar from each element.</returns>
        public static Vector3I operator -(Vector3I value, int scalar)
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
        public static Vector3I operator -(int scalar, Vector3I value)
        {
            Subtract(scalar, ref value, out var result);
            return result;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3I left, Vector3I right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3I left, Vector3I right)
        {
            return !left.Equals(ref right);
        }

        /// <inheritdoc/>
        public static explicit operator Vector3I(Vortice.Mathematics.Size3 Value)
        {
            return new(Value.Width, Value.Height, Value.Depth);
        }
        /// <inheritdoc/>
        public static explicit operator Vortice.Mathematics.Size3(Vector3I Value)
        {
            return new(Value.X, Value.Y, Value.Z);
        }

        /// <inheritdoc/>
        public static explicit operator Vector3I(Vortice.Mathematics.Int3 Value)
        {
            return new(Value.X, Value.Y, Value.Z);
        }
        /// <inheritdoc/>
        public static explicit operator Vortice.Mathematics.Int3(Vector3I Value)
        {
            return new(Value.X, Value.Y, Value.Z);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Vector3IStr,
                X, Y, Z);
        }
        /// <inheritdoc/>
        public readonly string ToString(string format)
        {
            if (format == null) return ToString();

            return string.Format(
                CultureInfo.CurrentCulture,
                Vector3IStr,
                X.ToString(format, CultureInfo.CurrentCulture),
                Y.ToString(format, CultureInfo.CurrentCulture),
                Z.ToString(format, CultureInfo.CurrentCulture));
        }
        /// <inheritdoc/>
        public readonly string ToString(IFormatProvider formatProvider)
        {
            return string.Format(
                formatProvider,
                Vector3IStr,
                X, Y, Z);
        }
        /// <inheritdoc/>
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) return ToString(formatProvider);

            return string.Format(
                formatProvider,
                Vector3IStr,
                X.ToString(format, formatProvider),
                Y.ToString(format, formatProvider),
                Z.ToString(format, formatProvider));
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ref Vector3I other)
        {
            return
                X == other.X &&
                Y == other.Y &&
                Z == other.Z;
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Vector3I other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object value)
        {
            if (value is not Vector3I) return false;

            var strongValue = (Vector3I)value;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Gets vector description
        /// </summary>
        /// <returns>Return vector description</returns>
        public readonly string GetDescription()
        {
            return string.Format(
                Vector3IDescStr,
                X, Y, Z);
        }
    }
}
