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
        /// Gets the top left corner of the rectangle.
        /// </summary>
        public readonly Vector2 TopLeft => new(Top, Left);
        /// <summary>
        /// Gets the top right corner of the rectangle.
        /// </summary>
        public readonly Vector2 TopRight => new(Top, Right);
        /// <summary>
        /// Gets the bottom left corner of the rectangle.
        /// </summary>
        public readonly Vector2 BottomLeft => new(Bottom, Left);
        /// <summary>
        /// Gets the bottom right corner of the rectangle.
        /// </summary>
        public readonly Vector2 BottomRight => new(Bottom, Right);
        /// <summary>
        /// Gets the center of the rectangle.
        /// </summary>
        public readonly Vector2 Center => new(Left + (Width / 2), Top + (Height / 2));

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
        /// Gets a value that indicates whether the rectangle is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is empty]; otherwise, <c>false</c>.
        /// </value>
        public readonly bool IsEmpty => (Width == 0.0f) && (Height == 0.0f) && (X == 0.0f) && (Y == 0.0f);

        /// <summary>
        /// Get the vertices of the rectangle in clockwise order starting from top left.
        /// </summary>
        public readonly Vector2[] GetVertices()
        {
            return
            [
                new (Left, Top),
                new (Right, Top),
                new (Right, Bottom),
                new (Left, Bottom),
            ];
        }

        /// <summary>Changes the position of the rectangle.</summary>
        /// <param name="amount">The values to adjust the position of the rectangle by.</param>
        public void Offset(System.Drawing.PointF amount)
        {
            Offset(amount.X, amount.Y);
        }
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
        /// <summary>
        /// Checks, if specified <see cref="Vector2"/> is inside <see cref="Rect"/>.
        /// </summary>
        /// <param name="vector">Coordinate <see cref="Vector2"/>.</param>
        /// <returns><c>true</c> if <see cref="Vector2"/> is inside <see cref="Rect"/>, otherwise <c>false</c>.</returns>
        public readonly bool Contains(Vector2 vector)
        {
            Contains(ref vector, out var result);
            return result;
        }
        /// <summary>Determines whether this rectangle contains a specified Point.</summary>
        /// <param name="value">The Point to evaluate.</param>
        /// <param name="result">[OutAttribute] true if the specified Point is contained within this rectangle; false otherwise.</param>
        public readonly void Contains(ref Vector2I value, out bool result)
        {
            result = (value.X >= Left && value.X <= Right && value.Y >= Top && value.Y <= Bottom);
        }
        /// <summary>
        /// Checks, if specified <see cref="Vector2I"/> is inside <see cref="Rect"/>.
        /// </summary>
        /// <param name="vector">Coordinate <see cref="Vector2I"/>.</param>
        /// <returns><c>true</c> if <see cref="Vector2I"/> is inside <see cref="Rect"/>, otherwise <c>false</c>.</returns>
        public readonly bool Contains(Vector2I vector)
        {
            Contains(ref vector, out var result);
            return result;
        }
        /// <summary>
        /// Checks, if specified <see cref="System.Drawing.PointF"/> is inside <see cref="Rect"/>.
        /// </summary>
        /// <param name="point">Coordinate <see cref="System.Drawing.PointF"/>.</param>
        /// <returns><c>true</c> if <see cref="System.Drawing.PointF"/> is inside <see cref="Rect"/>, otherwise <c>false</c>.</returns>
        public readonly void Contains(ref System.Drawing.PointF point, out bool result)
        {
            result = Contains(point.X, point.Y);
        }
        /// <summary>
        /// Checks, if specified <see cref="System.Drawing.PointF"/> is inside <see cref="Rect"/>.
        /// </summary>
        /// <param name="point">Coordinate <see cref="System.Drawing.PointF"/>.</param>
        /// <returns><c>true</c> if <see cref="System.Drawing.PointF"/> is inside <see cref="Rect"/>, otherwise <c>false</c>.</returns>
        public readonly bool Contains(System.Drawing.PointF point)
        {
            Contains(ref point, out bool result);
            return result;
        }
        /// <summary>
        /// Checks, if specified <see cref="System.Drawing.Point"/> is inside <see cref="Rect"/>.
        /// </summary>
        /// <param name="point">Coordinate <see cref="System.Drawing.Point"/>.</param>
        /// <returns><c>true</c> if <see cref="System.Drawing.Point"/> is inside <see cref="Rect"/>, otherwise <c>false</c>.</returns>
        public readonly void Contains(ref System.Drawing.Point point, out bool result)
        {
            result = Contains(point.X, point.Y);
        }
        /// <summary>
        /// Checks, if specified <see cref="System.Drawing.Point"/> is inside <see cref="Rect"/>.
        /// </summary>
        /// <param name="point">Coordinate <see cref="System.Drawing.Point"/>.</param>
        /// <returns><c>true</c> if <see cref="System.Drawing.Point"/> is inside <see cref="Rect"/>, otherwise <c>false</c>.</returns>
        public readonly bool Contains(System.Drawing.Point point)
        {
            Contains(ref point, out bool result);
            return result;
        }

        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        /// <param name="result">[OutAttribute] On exit, is true if this rectangle entirely contains the specified rectangle, or false if not.</param>
        public readonly void Contains(ref Rect value, out bool result)
        {
            result = (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);
        }
        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public readonly bool Contains(Rect value)
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
        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public readonly bool Contains(RectI value)
        {
            Contains(ref value, out bool result);
            return result;
        }
        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public readonly void Contains(ref System.Drawing.RectangleF value, out bool result)
        {
            result = (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);
        }
        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public readonly bool Contains(System.Drawing.RectangleF value)
        {
            Contains(ref value, out bool result);
            return result;
        }
        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public readonly void Contains(ref System.Drawing.Rectangle value, out bool result)
        {
            result = (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);
        }
        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public readonly bool Contains(System.Drawing.Rectangle value)
        {
            Contains(ref value, out bool result);
            return result;
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
        /// Checks, if specified point is inside <see cref="Rect"/>.
        /// </summary>
        /// <param name="x">X point coordinate.</param>
        /// <param name="y">Y point coordinate.</param>
        /// <returns><c>true</c> if point is inside <see cref="Rect"/>, otherwise <c>false</c>.</returns>
        public readonly bool Contains(int x, int y)
        {
            return (x >= Left && x <= Right && y >= Top && y <= Bottom);
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
        /// <summary>Determines whether a specified rectangle intersects with this rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public readonly bool Intersects(Rect value)
        {
            Intersects(ref value, out bool result);
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

        /// <summary>
        /// Computes the intersection area of two rectangles.
        /// </summary>
        /// <param name="a">First rectangle.</param>
        /// <param name="b">Second rectangle.</param>
        /// <returns>The intersection area.</returns>
        public static float ComputeIntersectionArea(Rect a, Rect b)
        {
            return ComputeIntersectionArea(ref a, ref b);
        }
        /// <summary>
        /// Computes the intersection area of two rectangles.
        /// </summary>
        /// <param name="a">First rectangle.</param>
        /// <param name="b">Second rectangle.</param>
        /// <returns>The intersection area.</returns>
        public static float ComputeIntersectionArea(ref Rect a, ref Rect b)
        {
            float ax1 = a.X;
            float ay1 = a.Y;
            float ax2 = a.Right;
            float ay2 = a.Bottom;

            float bx1 = b.X;
            float by1 = b.Y;
            float bx2 = b.Right;
            float by2 = b.Bottom;

            return ComputeIntersectionArea(ax1, ay1, ax2, ay2, bx1, by1, bx2, by2);
        }
        /// <summary>
        /// Computes the intersection area of two rectangles.
        /// </summary>
        /// <param name="ax1">X coordinate of the first rectangle's left edge.</param>
        /// <param name="ay1">Y coordinate of the first rectangle's top edge.</param>
        /// <param name="ax2">X coordinate of the first rectangle's right edge.</param>
        /// <param name="ay2">Y coordinate of the first rectangle's bottom edge.</param>
        /// <param name="bx1">X coordinate of the second rectangle's left edge.</param>
        /// <param name="by1">Y coordinate of the second rectangle's top edge.</param>
        /// <param name="bx2">X coordinate of the second rectangle's right edge.</param>
        /// <param name="by2">Y coordinate of the second rectangle's bottom edge.</param>
        /// <returns>The intersection area.</returns>
        private static float ComputeIntersectionArea(
            float ax1, float ay1, float ax2, float ay2,
            float bx1, float by1, float bx2, float by2)
        {
            return Math.Max(0, Math.Min(ax2, bx2) - Math.Max(ax1, bx1)) * Math.Max(0, Math.Min(ay2, by2) - Math.Max(ay1, by1));
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
        /// Scales the specified rectangle by the specified amount.
        /// </summary>
        /// <param name="rectangle">Rectangle</param>
        /// <param name="scale">Scale</param>
        /// <param name="result">Return value</param>
        public static void Scale(ref Rect rectangle, float scale, out Rect result)
        {
            float width = rectangle.Width * scale;
            float height = rectangle.Height * scale;
            float dWidth = width - rectangle.Width;
            float dHeight = height - rectangle.Height;

            float left = rectangle.Left - (dWidth * 0.5f);
            float top = rectangle.Top - (dHeight * 0.5f);

            result = new(left, top, width, height);
        }
        /// <summary>
        /// Scales the specified rectangle by the specified amount.
        /// </summary>
        /// <param name="rectangle">Rectangle</param>
        /// <param name="scale">Scale</param>
        public static Rect Scale(Rect rectangle, float scale)
        {
            Scale(ref rectangle, scale, out var result);
            return result;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ref Rect other)
        {
            return
                Helper.NearEqual(other.Left, Left) &&
                Helper.NearEqual(other.Right, Right) &&
                Helper.NearEqual(other.Top, Top) &&
                Helper.NearEqual(other.Bottom, Bottom);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Rect other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            if (obj is not Rect) return false;

            var strongValue = (Rect)obj;
            return Equals(ref strongValue);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rect left, Rect right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Rect left, Rect right)
        {
            return !left.Equals(ref right);
        }

        /// <inheritdoc/>
        public static implicit operator RectI(Rect value)
        {
            return new((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);
        }

        /// <inheritdoc/>
        public static explicit operator System.Drawing.Rectangle(Rect value)
        {
            return new((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);
        }
        /// <inheritdoc/>
        public static explicit operator System.Drawing.RectangleF(Rect value)
        {
            return new(value.X, value.Y, value.Width, value.Height);
        }

        /// <inheritdoc/>
        public static explicit operator Vortice.Mathematics.RectI(Rect value)
        {
            return new((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);
        }
        /// <inheritdoc/>
        public static explicit operator Vortice.Mathematics.Rect(Rect value)
        {
            return new(value.X, value.Y, value.Width, value.Height);
        }

        /// <inheritdoc/>
        public static implicit operator Vortice.RawRect(Rect value)
        {
            return new((int)value.Left, (int)value.Top, (int)value.Right, (int)value.Bottom);
        }
        /// <inheritdoc/>
        public static implicit operator Rect(Vortice.RawRect value)
        {
            return new(value.Left, value.Top, value.Right - value.Left, value.Bottom - value.Top);
        }

        /// <inheritdoc/>
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
