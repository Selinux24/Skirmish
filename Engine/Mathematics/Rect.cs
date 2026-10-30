// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Mathematics
{
    /// <summary>
    /// Define a RectangleF. This structure is slightly different from System.Drawing.RectangleF as it is
    /// internally storing Left,Top,Right,Bottom instead of Left,Top,Width,Height.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Rect"/> struct.
    /// </remarks>
    /// <param name="x">The left.</param>
    /// <param name="y">The top.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect(float x, float y, float width, float height) : IEquatable<Rect>
    {
        const string RectStr = "X:{0} Y:{1} Width:{2} Height:{3}";

        static Rect()
        {
            Empty = new Rect();
            Infinite = new Rect
            {
                Left = float.NegativeInfinity,
                Top = float.NegativeInfinity,
                Right = float.PositiveInfinity,
                Bottom = float.PositiveInfinity
            };
        }

        /// <summary>
        /// An empty rectangle.
        /// </summary>
        public static readonly Rect Empty;
        /// <summary>
        /// An infinite rectangle. See remarks.
        /// </summary>
        /// <remarks>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/dd372261%28v=vs.85%29.aspx
        /// Any properties that involve computations, like <see cref="Center"/>, <see cref="Width"/> or <see cref="Height"/>
        /// may return incorrect results - <see cref="float.NaN"/>.
        /// </remarks>
        public static readonly Rect Infinite;

        /// <summary>
        /// The left.
        /// </summary>
        public float Left = x;
        /// <summary>
        /// The top.
        /// </summary>
        public float Top = y;
        /// <summary>
        /// The right.
        /// </summary>
        public float Right = x + width;
        /// <summary>
        /// The bottom.
        /// </summary>
        public float Bottom = y + height;

        /// <summary>
        /// Gets or sets the X position.
        /// </summary>
        /// <value>The X position.</value>
        public float X
        {
            readonly get => Left;
            set
            {
                Right = value + Width;
                Left = value;
            }
        }
        /// <summary>
        /// Gets or sets the Y position.
        /// </summary>
        /// <value>The Y position.</value>
        public float Y
        {
            readonly get => Top;
            set
            {
                Bottom = value + Height;
                Top = value;
            }
        }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>The width.</value>
        public float Width
        {
            readonly get => Right - Left;
            set { Right = Left + value; }
        }
        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        public float Height
        {
            readonly get => Bottom - Top;
            set { Bottom = Top + value; }
        }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public Vector2 Location
        {
            readonly get => new(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
        /// <summary>
        /// Gets the Point that specifies the center of the rectangle.
        /// </summary>
        /// <value>
        /// The center.
        /// </value>
        public readonly Vector2 Center => new(X + (Width / 2), Y + (Height / 2));

        /// <summary>
        /// Gets a value that indicates whether the rectangle is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is empty]; otherwise, <c>false</c>.
        /// </value>
        public readonly bool IsEmpty => (Width == 0.0f) && (Height == 0.0f) && (X == 0.0f) && (Y == 0.0f);
        /// <summary>
        /// Gets or sets the size of the rectangle.
        /// </summary>
        /// <value>The size of the rectangle.</value>
        public Vector2 Size
        {
            readonly get => new(Width, Height);
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        /// <summary>
        /// Gets the position of the top-left corner of the rectangle.
        /// </summary>
        /// <value>The top-left corner of the rectangle.</value>
        public readonly Vector2 TopLeft { get { return new Vector2(Left, Top); } }
        /// <summary>
        /// Gets the position of the top-right corner of the rectangle.
        /// </summary>
        /// <value>The top-right corner of the rectangle.</value>
        public readonly Vector2 TopRight { get { return new Vector2(Right, Top); } }
        /// <summary>
        /// Gets the position of the bottom-left corner of the rectangle.
        /// </summary>
        /// <value>The bottom-left corner of the rectangle.</value>
        public readonly Vector2 BottomLeft { get { return new Vector2(Left, Bottom); } }
        /// <summary>
        /// Gets the position of the bottom-right corner of the rectangle.
        /// </summary>
        /// <value>The bottom-right corner of the rectangle.</value>
        public readonly Vector2 BottomRight { get { return new Vector2(Right, Bottom); } }

        /// <summary>Changes the position of the rectangle.</summary>
        /// <param name="amount">The values to adjust the position of the rectangle by.</param>
        public void Offset(System.Drawing.Point amount)
        {
            Offset(amount.X, amount.Y);
        }
        /// <summary>Changes the position of the rectangle.</summary>
        /// <param name="amount">The values to adjust the position of the rectangle by.</param>
        public void Offset(Vector2 amount)
        {
            Offset(amount.X, amount.Y);
        }
        /// <summary>Changes the position of the rectangle.</summary>
        /// <param name="offsetX">Change in the x-position.</param>
        /// <param name="offsetY">Change in the y-position.</param>
        public void Offset(float offsetX, float offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        /// <summary>Pushes the edges of the rectangle out by the horizontal and vertical values specified.</summary>
        /// <param name="horizontalAmount">Value to push the sides out by.</param>
        /// <param name="verticalAmount">Value to push the top and bottom out by.</param>
        public void Inflate(float horizontalAmount, float verticalAmount)
        {
            X -= horizontalAmount;
            Y -= verticalAmount;
            Width += horizontalAmount * 2;
            Height += verticalAmount * 2;
        }

        /// <summary>Determines whether this rectangle contains a specified Point.</summary>
        /// <param name="value">The Point to evaluate.</param>
        /// <param name="result">[OutAttribute] true if the specified Point is contained within this rectangle; false otherwise.</param>
        public readonly void Contains(ref Vector2 value, out bool result)
        {
            result = (value.X >= Left && value.X <= Right && value.Y >= Top && value.Y <= Bottom);
        }
        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public readonly bool Contains(System.Drawing.Rectangle value)
        {
            return (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);
        }
        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        /// <param name="result">[OutAttribute] On exit, is true if this rectangle entirely contains the specified rectangle, or false if not.</param>
        public readonly void Contains(ref Rect value, out bool result)
        {
            result = (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);
        }
        /// <summary>
        /// Checks, if specified point is inside <see cref="Rect"/>.
        /// </summary>
        /// <param name="x">X point coordinate.</param>
        /// <param name="y">Y point coordinate.</param>
        /// <returns><c>true</c> if point is inside <see cref="Rect"/>, otherwise <c>false</c>.</returns>
        public readonly bool Contains(float x, float y)
        {
            return (x >= Left && x <= Right && y >= Top && y <= Bottom);
        }
        /// <summary>
        /// Checks, if specified <see cref="Vector2"/> is inside <see cref="Rect"/>.
        /// </summary>
        /// <param name="vector2D">Coordinate <see cref="Vector2"/>.</param>
        /// <returns><c>true</c> if <see cref="Vector2"/> is inside <see cref="Rect"/>, otherwise <c>false</c>.</returns>
        public readonly bool Contains(Vector2 vector2D)
        {
            return Contains(vector2D.X, vector2D.Y);
        }
        /// <summary>
        /// Checks, if specified <see cref="Point"/> is inside <see cref="Rect"/>.
        /// </summary>
        /// <param name="point">Coordinate <see cref="Point"/>.</param>
        /// <returns><c>true</c> if <see cref="Point"/> is inside <see cref="Rect"/>, otherwise <c>false</c>.</returns>
        public readonly bool Contains(System.Drawing.Point point)
        {
            return Contains(point.X, point.Y);
        }

        /// <summary>Determines whether a specified rectangle intersects with this rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public readonly bool Intersects(Rect value)
        {
            Intersects(ref value, out bool result);
            return result;
        }
        /// <summary>
        /// Determines whether a specified rectangle intersects with this rectangle.
        /// </summary>
        /// <param name="value">The rectangle to evaluate</param>
        /// <param name="result">[OutAttribute] true if the specified rectangle intersects with this one; false otherwise.</param>
        public readonly void Intersects(ref Rect value, out bool result)
        {
            result = (value.X < Right) && (X < value.Right) && (value.Y < Bottom) && (Y < value.Bottom);
        }
        /// <summary>
        /// Creates a rectangle defining the area where one rectangle overlaps with another rectangle.
        /// </summary>
        /// <param name="value1">The first Rectangle to compare.</param>
        /// <param name="value2">The second Rectangle to compare.</param>
        /// <returns>The intersection rectangle.</returns>
        public static Rect Intersect(Rect value1, Rect value2)
        {
            Intersect(ref value1, ref value2, out var result);
            return result;
        }
        /// <summary>Creates a rectangle defining the area where one rectangle overlaps with another rectangle.</summary>
        /// <param name="value1">The first rectangle to compare.</param>
        /// <param name="value2">The second rectangle to compare.</param>
        /// <param name="result">[OutAttribute] The area where the two first parameters overlap.</param>
        public static void Intersect(ref Rect value1, ref Rect value2, out Rect result)
        {
            float newLeft = (value1.X > value2.X) ? value1.X : value2.X;
            float newTop = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            float newRight = (value1.Right < value2.Right) ? value1.Right : value2.Right;
            float newBottom = (value1.Bottom < value2.Bottom) ? value1.Bottom : value2.Bottom;

            if ((newRight > newLeft) && (newBottom > newTop))
            {
                result = new(newLeft, newTop, newRight - newLeft, newBottom - newTop);
            }
            else
            {
                result = Empty;
            }
        }

        /// <summary>
        /// Creates a new rectangle that exactly contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first rectangle to contain.</param>
        /// <param name="value2">The second rectangle to contain.</param>
        /// <returns>The union rectangle.</returns>
        public static Rect Union(Rect value1, Rect value2)
        {
            Union(ref value1, ref value2, out var result);
            return result;
        }
        /// <summary>
        /// Creates a new rectangle that exactly contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first rectangle to contain.</param>
        /// <param name="value2">The second rectangle to contain.</param>
        /// <param name="result">[OutAttribute] The rectangle that must be the union of the first two rectangles.</param>
        public static void Union(ref Rect value1, ref Rect value2, out Rect result)
        {
            var left = Math.Min(value1.Left, value2.Left);
            var right = Math.Max(value1.Right, value2.Right);
            var top = Math.Min(value1.Top, value2.Top);
            var bottom = Math.Max(value1.Bottom, value2.Bottom);

            result = new(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override readonly bool Equals(object obj)
        {
            if (obj is not Rect) return false;

            var strongValue = (Rect)obj;
            return Equals(ref strongValue);
        }
        /// <summary>
        /// Determines whether the specified <see cref="Rect"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Rect"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Rect"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public readonly bool Equals(ref Rect other)
        {
            return
                Helper.NearEqual(other.Left, Left) &&
                Helper.NearEqual(other.Right, Right) &&
                Helper.NearEqual(other.Top, Top) &&
                Helper.NearEqual(other.Bottom, Bottom);
        }
        /// <summary>
        /// Determines whether the specified <see cref="Rect"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Rect"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Rect"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Rect other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rect left, Rect right)
        {
            return left.Equals(ref right);
        }
        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Rect left, Rect right)
        {
            return !left.Equals(ref right);
        }
        /// <summary>
        /// Performs an explicit conversion to <see cref="System.Drawing.Rectangle"/> structure.
        /// </summary>
        /// <remarks>Performs direct float to int conversion, any fractional data is truncated.</remarks>
        /// <param name="value">The source <see cref="Rect"/> value.</param>
        /// <returns>A converted <see cref="System.Drawing.Rectangle"/> structure.</returns>
        public static explicit operator System.Drawing.Rectangle(Rect value)
        {
            return new((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);
        }
        /// <summary>
        /// Performs an explicit conversion to <see cref="Vortice.Mathematics.RectI"/> structure.
        /// </summary>
        /// <remarks>Performs direct float to int conversion, any fractional data is truncated.</remarks>
        /// <param name="value">The source <see cref="Rect"/> value.</param>
        /// <returns>A converted <see cref="Vortice.Mathematics.RectI"/> structure.</returns>
        public static explicit operator Vortice.Mathematics.RectI(Rect value)
        {
            return new((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);
        }
        /// <summary>
        /// Performs an explicit conversion to <see cref="System.Drawing.RectangleF"/> structure.
        /// </summary>
        /// <remarks>Performs direct float to int conversion, any fractional data is truncated.</remarks>
        /// <param name="value">The source <see cref="Rect"/> value.</param>
        /// <returns>A converted <see cref="System.Drawing.RectangleF"/> structure.</returns>
        public static explicit operator System.Drawing.RectangleF(Rect value)
        {
            return new(value.X, value.Y, value.Width, value.Height);
        }
        /// <summary>
        /// Performs an explicit conversion to <see cref="Vortice.Mathematics.Rect"/> structure.
        /// </summary>
        /// <remarks>Performs direct float to int conversion, any fractional data is truncated.</remarks>
        /// <param name="value">The source <see cref="Rect"/> value.</param>
        /// <returns>A converted <see cref="Vortice.Mathematics.Rect"/> structure.</returns>
        public static explicit operator Vortice.Mathematics.Rect(Rect value)
        {
            return new(value.X, value.Y, value.Width, value.Height);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Left, Top, Right, Bottom);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                RectStr,
                X, Y, Width, Height);
        }
    }
}
