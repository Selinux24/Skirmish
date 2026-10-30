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
    /// Define a Rectangle. This structure is slightly different from System.Drawing.Rectangle as it is
    /// internally storing Left,Top,Right,Bottom instead of Left,Top,Width,Height.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="RectI"/> struct.
    /// </remarks>
    /// <param name="x">The left.</param>
    /// <param name="y">The top.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    [StructLayout(LayoutKind.Sequential)]
    public struct RectI(int x, int y, int width, int height) : IEquatable<RectI>
    {
        const string RectIStr = "X:{0} Y:{1} Width:{2} Height:{3}";

        /// <summary>
        /// An empty rectangle.
        /// </summary>
        public static readonly RectI Empty;

        static RectI()
        {
            Empty = new RectI();
        }

        /// <summary>
        /// The left.
        /// </summary>
        public int Left = x;
        /// <summary>
        /// The top.
        /// </summary>
        public int Top = y;
        /// <summary>
        /// The right.
        /// </summary>
        public int Right = x + width;
        /// <summary>
        /// The bottom.
        /// </summary>
        public int Bottom = y + height;

        /// <summary>
        /// Gets or sets the X position.
        /// </summary>
        /// <value>The X position.</value>
        public int X
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
        public int Y
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
        public int Width
        {
            readonly get => Right - Left;
            set { Right = Left + value; }
        }
        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        public int Height
        {
            readonly get => Bottom - Top;
            set { Bottom = Top + value; }
        }
        /// <summary>
        /// Gets a value that indicates whether the rectangle is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is empty]; otherwise, <c>false</c>.
        /// </value>
        public readonly bool IsEmpty => (Width == 0) && (Height == 0) && (X == 0) && (Y == 0);

        /// <summary>Changes the position of the rectangle.</summary>
        /// <param name="offsetX">Change in the x-position.</param>
        /// <param name="offsetY">Change in the y-position.</param>
        public void Offset(int offsetX, int offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        /// <summary>Pushes the edges of the rectangle out by the horizontal and vertical values specified.</summary>
        /// <param name="horizontalAmount">Value to push the sides out by.</param>
        /// <param name="verticalAmount">Value to push the top and bottom out by.</param>
        public void Inflate(int horizontalAmount, int verticalAmount)
        {
            X -= horizontalAmount;
            Y -= verticalAmount;
            Width += horizontalAmount * 2;
            Height += verticalAmount * 2;
        }

        /// <summary>Determines whether this rectangle contains a specified point represented by its x- and y-coordinates.</summary>
        /// <param name="x">The x-coordinate of the specified point.</param>
        /// <param name="y">The y-coordinate of the specified point.</param>
        public readonly bool Contains(int x, int y)
        {
            return (X <= x) && (x < Right) && (Y <= y) && (y < Bottom);
        }
        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public readonly bool Contains(RectI value)
        {
            Contains(ref value, out bool result);
            return result;
        }
        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        /// <param name="result">[OutAttribute] On exit, is true if this rectangle entirely contains the specified rectangle, or false if not.</param>
        public readonly void Contains(ref RectI value, out bool result)
        {
            result = (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);
        }
        /// <summary>
        /// Checks, if specified point is inside <see cref="RectI"/>.
        /// </summary>
        /// <param name="x">X point coordinate.</param>
        /// <param name="y">Y point coordinate.</param>
        /// <returns><c>true</c> if point is inside <see cref="RectI"/>, otherwise <c>false</c>.</returns>
        public readonly bool Contains(float x, float y)
        {
            return (x >= Left && x <= Right && y >= Top && y <= Bottom);
        }

        /// <summary>Determines whether a specified rectangle intersects with this rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public readonly bool Intersects(RectI value)
        {
            Intersects(ref value, out bool result);
            return result;
        }
        /// <summary>
        /// Determines whether a specified rectangle intersects with this rectangle.
        /// </summary>
        /// <param name="value">The rectangle to evaluate</param>
        /// <param name="result">[OutAttribute] true if the specified rectangle intersects with this one; false otherwise.</param>
        public readonly void Intersects(ref RectI value, out bool result)
        {
            result = (value.X < Right) && (X < value.Right) && (value.Y < Bottom) && (Y < value.Bottom);
        }
        /// <summary>
        /// Creates a rectangle defining the area where one rectangle overlaps with another rectangle.
        /// </summary>
        /// <param name="value1">The first rectangle to compare.</param>
        /// <param name="value2">The second rectangle to compare.</param>
        /// <returns>The intersection rectangle.</returns>
        public static RectI Intersect(RectI value1, RectI value2)
        {
            Intersect(ref value1, ref value2, out var result);
            return result;
        }
        /// <summary>Creates a rectangle defining the area where one rectangle overlaps with another rectangle.</summary>
        /// <param name="value1">The first rectangle to compare.</param>
        /// <param name="value2">The second rectangle to compare.</param>
        /// <param name="result">[OutAttribute] The area where the two first parameters overlap.</param>
        public static void Intersect(ref RectI value1, ref RectI value2, out RectI result)
        {
            int newLeft = (value1.X > value2.X) ? value1.X : value2.X;
            int newTop = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            int newRight = (value1.Right < value2.Right) ? value1.Right : value2.Right;
            int newBottom = (value1.Bottom < value2.Bottom) ? value1.Bottom : value2.Bottom;

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
        public static RectI Union(RectI value1, RectI value2)
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
        public static void Union(ref RectI value1, ref RectI value2, out RectI result)
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
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override readonly bool Equals(object obj)
        {
            if (obj is not RectI) return false;

            var strongValue = (RectI)obj;
            return Equals(ref strongValue);
        }
        /// <summary>
        /// Determines whether the specified <see cref="RectI"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="RectI"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="RectI"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ref RectI other)
        {
            return other.Left == Left && other.Top == Top && other.Right == Right && other.Bottom == Bottom;
        }
        /// <summary>
        /// Determines whether the specified <see cref="RectI"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="RectI"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="RectI"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(RectI other)
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
        public static bool operator ==(RectI left, RectI right)
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
        public static bool operator !=(RectI left, RectI right)
        {
            return !left.Equals(ref right);
        }
        /// <summary>
        /// Performs an explicit conversion to <see cref="System.Drawing.Rectangle"/> structure.
        /// </summary>
        /// <remarks>Performs direct float to int conversion, any fractional data is truncated.</remarks>
        /// <param name="value">The source <see cref="Rect"/> value.</param>
        /// <returns>A converted <see cref="System.Drawing.Rectangle"/> structure.</returns>
        public static explicit operator System.Drawing.Rectangle(RectI value)
        {
            return new(value.X, value.Y, value.Width, value.Height);
        }
        /// <summary>
        /// Performs an explicit conversion to <see cref="Vortice.Mathematics.RectI"/> structure.
        /// </summary>
        /// <remarks>Performs direct float to int conversion, any fractional data is truncated.</remarks>
        /// <param name="value">The source <see cref="Rect"/> value.</param>
        /// <returns>A converted <see cref="Vortice.Mathematics.RectI"/> structure.</returns>
        public static explicit operator Vortice.Mathematics.RectI(RectI value)
        {
            return new(value.X, value.Y, value.Width, value.Height);
        }
        /// <summary>
        /// Performs an explicit conversion to <see cref="System.Drawing.RectangleF"/> structure.
        /// </summary>
        /// <remarks>Performs direct float to int conversion, any fractional data is truncated.</remarks>
        /// <param name="value">The source <see cref="Rect"/> value.</param>
        /// <returns>A converted <see cref="System.Drawing.RectangleF"/> structure.</returns>
        public static explicit operator System.Drawing.RectangleF(RectI value)
        {
            return new(value.X, value.Y, value.Width, value.Height);
        }
        /// <summary>
        /// Performs an explicit conversion to <see cref="Vortice.Mathematics.Rect"/> structure.
        /// </summary>
        /// <remarks>Performs direct float to int conversion, any fractional data is truncated.</remarks>
        /// <param name="value">The source <see cref="Rect"/> value.</param>
        /// <returns>A converted <see cref="Vortice.Mathematics.Rect"/> structure.</returns>
        public static explicit operator Vortice.Mathematics.Rect(RectI value)
        {
            return new(value.X, value.Y, value.Width, value.Height);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Left, Top, Right, Bottom);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                RectIStr,
                X, Y, Width, Height);
        }
    }
}
