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

using Newtonsoft.Json;
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
    [JsonObject(MemberSerialization.OptIn)]
    public struct RectI(int x, int y, int width, int height) : IEquatable<RectI>, IFormattable
    {
        const string RectIStr = "X:{0} Y:{1} Width:{2} Height:{3}";

        /// <summary>
        /// An empty rectangle.
        /// </summary>
        public static readonly RectI Empty = new();

        /// <summary>
        /// The left.
        /// </summary>
        [JsonProperty]
        public int Left = x;
        /// <summary>
        /// The top.
        /// </summary>
        [JsonProperty]
        public int Top = y;
        /// <summary>
        /// The right.
        /// </summary>
        [JsonProperty]
        public int Right = x + width;
        /// <summary>
        /// The bottom.
        /// </summary>
        [JsonProperty]
        public int Bottom = y + height;

        /// <summary>
        /// Gets the top left corner of the rectangle.
        /// </summary>
        public readonly Vector2I TopLeft => new(Left, Top);
        /// <summary>
        /// Gets the top right corner of the rectangle.
        /// </summary>
        public readonly Vector2I TopRight => new(Right, Top);
        /// <summary>
        /// Gets the bottom left corner of the rectangle.
        /// </summary>
        public readonly Vector2I BottomLeft => new(Left, Bottom);
        /// <summary>
        /// Gets the bottom right corner of the rectangle.
        /// </summary>
        public readonly Vector2I BottomRight => new(Right, Bottom);
        /// <summary>
        /// Gets the center of the rectangle.
        /// </summary>
        public readonly Vector2I Center => new(Left + (Width / 2), Top + (Height / 2));

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
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public Vector2I Location
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
        public Vector2I Size
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
        public readonly bool IsEmpty => (Width == 0) && (Height == 0) && (X == 0) && (Y == 0);

        /// <summary>
        /// Get the vertices of the rectangle in clockwise order starting from top left.
        /// </summary>
        public readonly Vector2I[] GetVertices()
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
        public void Offset(System.Drawing.Point amount)
        {
            Offset(amount.X, amount.Y);
        }
        /// <summary>Changes the position of the rectangle.</summary>
        /// <param name="amount">The values to adjust the position of the rectangle by.</param>
        public void Offset(Vector2I amount)
        {
            Offset(amount.X, amount.Y);
        }
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
        /// Checks, if specified point is inside <see cref="RectI"/>.
        /// </summary>
        /// <param name="x">X point coordinate.</param>
        /// <param name="y">Y point coordinate.</param>
        /// <returns><c>true</c> if point is inside <see cref="RectI"/>, otherwise <c>false</c>.</returns>
        public readonly bool Contains(float x, float y)
        {
            return (x >= Left && x <= Right && y >= Top && y <= Bottom);
        }
        /// <summary>Determines whether this rectangle contains a specified point represented by its x- and y-coordinates.</summary>
        /// <param name="x">The x-coordinate of the specified point.</param>
        /// <param name="y">The y-coordinate of the specified point.</param>
        public readonly bool Contains(int x, int y)
        {
            return (X <= x) && (x < Right) && (Y <= y) && (y < Bottom);
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
        /// <summary>Determines whether a specified rectangle intersects with this rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public readonly bool Intersects(RectI value)
        {
            Intersects(ref value, out bool result);
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

        /// <summary>
        /// Computes the intersection area of two rectangles.
        /// </summary>
        /// <param name="a">First rectangle.</param>
        /// <param name="b">Second rectangle.</param>
        /// <returns>The intersection area.</returns>
        public static int ComputeIntersectionArea(RectI a, RectI b)
        {
            return ComputeIntersectionArea(ref a, ref b);
        }
        /// <summary>
        /// Computes the intersection area of two rectangles.
        /// </summary>
        /// <param name="a">First rectangle.</param>
        /// <param name="b">Second rectangle.</param>
        /// <returns>The intersection area.</returns>
        public static int ComputeIntersectionArea(ref RectI a, ref RectI b)
        {
            int ax1 = a.Left;
            int ay1 = a.Top;
            int ax2 = a.Right;
            int ay2 = a.Bottom;

            int bx1 = b.Left;
            int by1 = b.Top;
            int bx2 = b.Right;
            int by2 = b.Bottom;

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
        private static int ComputeIntersectionArea(
            int ax1, int ay1, int ax2, int ay2,
            int bx1, int by1, int bx2, int by2)
        {
            return Math.Max(0, Math.Min(ax2, bx2) - Math.Max(ax1, bx1)) * Math.Max(0, Math.Min(ay2, by2) - Math.Max(ay1, by1));
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
        /// Scales the specified rectangle by the specified amount.
        /// </summary>
        /// <param name="rectangle">Rectangle</param>
        /// <param name="scale">Scale</param>
        /// <param name="result">Return value</param>
        public static void Scale(ref RectI rectangle, int scale, out Rect result)
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
        public static RectI Scale(RectI rectangle, int scale)
        {
            Scale(ref rectangle, scale, out var result);
            return result;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ref RectI other)
        {
            return
                other.Left == Left &&
                other.Top == Top &&
                other.Right == Right &&
                other.Bottom == Bottom;
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(RectI other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            if (obj is not RectI) return false;

            var strongValue = (RectI)obj;
            return Equals(ref strongValue);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(RectI left, RectI right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(RectI left, RectI right)
        {
            return !left.Equals(ref right);
        }

        /// <inheritdoc/>
        public static implicit operator Rect(RectI value)
        {
            return new(value.X, value.Y, value.Width, value.Height);
        }

        /// <inheritdoc/>
        public static explicit operator System.Drawing.Rectangle(RectI value)
        {
            return new(value.X, value.Y, value.Width, value.Height);
        }
        /// <inheritdoc/>
        public static explicit operator System.Drawing.RectangleF(RectI value)
        {
            return new(value.X, value.Y, value.Width, value.Height);
        }

        /// <inheritdoc/>
        public static explicit operator Vortice.Mathematics.RectI(RectI value)
        {
            return new(value.X, value.Y, value.Width, value.Height);
        }
        /// <inheritdoc/>
        public static explicit operator Vortice.Mathematics.Rect(RectI value)
        {
            return new(value.X, value.Y, value.Width, value.Height);
        }

        /// <inheritdoc/>
        public static implicit operator Vortice.RawRect(RectI value)
        {
            return new(value.Left, value.Top, value.Right, value.Bottom);
        }
        /// <inheritdoc/>
        public static implicit operator RectI(Vortice.RawRect value)
        {
            return new(value.Left, value.Top, value.Right - value.Left, value.Bottom - value.Top);
        }

        /// <inheritdoc/>
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Left, Top, Right, Bottom);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                RectIStr,
                X.ToString(),
                Y.ToString(),
                Width.ToString(),
                Height.ToString());
        }
        /// <inheritdoc/>
        public readonly string ToString(string format)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                RectIStr,
                X.ToString(format, CultureInfo.CurrentCulture),
                Y.ToString(format, CultureInfo.CurrentCulture),
                Width.ToString(format, CultureInfo.CurrentCulture),
                Height.ToString(format, CultureInfo.CurrentCulture));
        }
        /// <inheritdoc/>
        public readonly string ToString(IFormatProvider formatProvider)
        {
            return string.Format(
                formatProvider,
                RectIStr,
                X.ToString(),
                Y.ToString(),
                Width.ToString(),
                Height.ToString());
        }
        /// <inheritdoc/>
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(
                formatProvider,
                RectIStr,
                X.ToString(format, formatProvider),
                Y.ToString(format, formatProvider),
                Width.ToString(format, formatProvider),
                Height.ToString(format, formatProvider));
        }
    }
}
