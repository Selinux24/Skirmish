using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Engine
{
    /// <summary>
    /// A integer vertex
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3Int : IEquatable<Vector3Int>
    {
        /// <summary>
        /// An implementation of <see cref="IEqualityComparer{T}"/> of <see cref="Vector3Int"/> that allows for the
        /// Y coordinates of two vertices to be within a specified range and still be considered equal.
        /// </summary>
        internal class RoughYEqualityComparer : IEqualityComparer<Vector3Int>
        {
            private const int HashConstX = unchecked((int)0x8da6b343);
            private const int HashConstZ = unchecked((int)0xcb1ab31f);

            private readonly int epsilonY;

            /// <summary>
            /// Initializes a new instance of the <see cref="RoughYEqualityComparer"/> class.
            /// </summary>
            /// <param name="epsilonY">The range of Y values in which two vertices are considered equal.</param>
            public RoughYEqualityComparer(int epsilonY)
            {
                this.epsilonY = epsilonY;
            }

            /// <summary>
            /// Compares two vertices for equality.
            /// </summary>
            /// <param name="x">A vertex.</param>
            /// <param name="y">Another vertex.</param>
            /// <returns>A value indicating whether the two vertices are equal.</returns>
            public bool Equals(Vector3Int x, Vector3Int y)
            {
                return x.X == y.X && (Math.Abs(x.Y - y.Y) <= epsilonY) && x.Z == y.Z;
            }
            /// <summary>
            /// Gets a unique hash code for the contents of a <see cref="Vector3Int"/> instance.
            /// </summary>
            /// <param name="obj">A vertex.</param>
            /// <returns>A hash code.</returns>
            public int GetHashCode(Vector3Int obj)
            {
                return HashConstX * obj.X + HashConstZ * obj.Z;
            }
        }

        /// <summary>
        /// Calculates the component-wise minimum of two vertices.
        /// </summary>
        /// <param name="a">A vertex.</param>
        /// <param name="b">Another vertex.</param>
        /// <returns>The component-wise minimum of the two vertices.</returns>
        public static Vector3Int ComponentMin(Vector3Int a, Vector3Int b)
        {
            ComponentMin(ref a, ref b, out Vector3Int v);
            return v;
        }
        /// <summary>
        /// Calculates the component-wise minimum of two vertices.
        /// </summary>
        /// <param name="a">A vertex.</param>
        /// <param name="b">Another vertex.</param>
        /// <param name="result">The component-wise minimum of the two vertices.</param>
        public static void ComponentMin(ref Vector3Int a, ref Vector3Int b, out Vector3Int result)
        {
            var x = a.X < b.X ? a.X : b.X;
            var y = a.Y < b.Y ? a.Y : b.Y;
            var z = a.Z < b.Z ? a.Z : b.Z;

            result = new Vector3Int(x, y, z);
        }
        /// <summary>
        /// Calculates the component-wise maximum of two vertices.
        /// </summary>
        /// <param name="a">A vertex.</param>
        /// <param name="b">Another vertex.</param>
        /// <returns>The component-wise maximum of the two vertices.</returns>
        public static Vector3Int ComponentMax(Vector3Int a, Vector3Int b)
        {
            ComponentMax(ref a, ref b, out Vector3Int v);
            return v;
        }
        /// <summary>
        /// Calculates the component-wise maximum of two vertices.
        /// </summary>
        /// <param name="a">A vertex.</param>
        /// <param name="b">Another vertex.</param>
        /// <param name="result">The component-wise maximum of the two vertices.</param>
        public static void ComponentMax(ref Vector3Int a, ref Vector3Int b, out Vector3Int result)
        {
            var x = a.X > b.X ? a.X : b.X;
            var y = a.Y > b.Y ? a.Y : b.Y;
            var z = a.Z > b.Z ? a.Z : b.Z;

            result = new Vector3Int(x, y, z);
        }
        /// <summary>
        /// Gets the leftness of a triangle formed from 3 contour vertices.
        /// </summary>
        /// <param name="aV">The first vertex.</param>
        /// <param name="bV">The second vertex.</param>
        /// <param name="cV">The third vertex.</param>
        /// <returns>A value indicating the leftness of the triangle.</returns>
        public static bool IsLeft(ref Vector3Int aV, ref Vector3Int bV, ref Vector3Int cV)
        {
            Area2D(ref aV, ref bV, ref cV, out int area);
            return area < 0;
        }
        /// <summary>
        /// Gets the leftness (left or on) of a triangle formed from 3 contour vertices.
        /// </summary>
        /// <param name="aV">The first vertex.</param>
        /// <param name="bV">The second vertex.</param>
        /// <param name="cV">The third vertex.</param>
        /// <returns>A value indicating whether the triangle is left or on.</returns>
        public static bool IsLeftOn(ref Vector3Int aV, ref Vector3Int bV, ref Vector3Int cV)
        {
            Area2D(ref aV, ref bV, ref cV, out int area);
            return area <= 0;
        }
        /// <summary>
        /// Compares vertex equality in 2D.
        /// </summary>
        /// <param name="aV">A vertex.</param>
        /// <param name="bV">Another vertex.</param>
        /// <returns>A value indicating whether the X and Z components of both vertices are equal.</returns>
        public static bool Equal2D(ref Vector3Int aV, ref Vector3Int bV)
        {
            return aV.X == bV.X && aV.Z == bV.Z;
        }
        /// <summary>
        /// True if and only if A, B, and C are collinear and point C lies on closed segment AB
        /// </summary>
        /// <param name="aV">Point A of segment AB.</param>
        /// <param name="bV">Point B of segment AB.</param>
        /// <param name="cV">Point C.</param>
        /// <returns>A value indicating whether the three points are collinear with C in the middle.</returns>
        public static bool IsBetween(ref Vector3Int aV, ref Vector3Int bV, ref Vector3Int cV)
        {
            if (!IsCollinear(ref aV, ref bV, ref cV))
            {
                return false;
            }

            if (aV.X != bV.X)
            {
                return ((aV.X <= cV.X) && (cV.X <= bV.X)) || ((aV.X >= cV.X) && (cV.X >= bV.X));
            }
            else
            {
                return ((aV.Z <= cV.Z) && (cV.Z <= bV.Z)) || ((aV.Z >= cV.Z) && (cV.Z >= bV.Z));
            }
        }
        /// <summary>
        /// True if and only if points A, B, and C are collinear.
        /// </summary>
        /// <param name="aV">Point A.</param>
        /// <param name="bV">Point B.</param>
        /// <param name="cV">Point C.</param>
        /// <returns>A value indicating whether the points are collinear.</returns>
        public static bool IsCollinear(ref Vector3Int aV, ref Vector3Int bV, ref Vector3Int cV)
        {
            Area2D(ref aV, ref bV, ref cV, out int area);
            return area == 0;
        }
        /// <summary>
        /// Gets the 2D area of the triangle ABC.
        /// </summary>
        /// <param name="aV">Point A of triangle ABC.</param>
        /// <param name="bV">Point B of triangle ABC.</param>
        /// <param name="cV">Point C of triangle ABC.</param>
        /// <param name="area">The 2D area of the triangle.</param>
        public static void Area2D(ref Vector3Int aV, ref Vector3Int bV, ref Vector3Int cV, out int area)
        {
            area = (bV.X - aV.X) * (cV.Z - aV.Z) - (cV.X - aV.X) * (bV.Z - aV.Z);
        }
        /// <summary>
        /// True if and only if segments AB and CD intersect, properly or improperly.
        /// </summary>
        /// <param name="a">Point A of segment AB.</param>
        /// <param name="b">Point B of segment AB.</param>
        /// <param name="c">Point C of segment CD.</param>
        /// <param name="d">Point D of segment CD.</param>
        /// <returns>A value indicating whether segments AB and CD intersect.</returns>
        public static bool Intersect(ref Vector3Int a, ref Vector3Int b, ref Vector3Int c, ref Vector3Int d)
        {
            if (IntersectProp(ref a, ref b, ref c, ref d))
            {
                return true;
            }
            else if (
                IsBetween(ref a, ref b, ref c) ||
                IsBetween(ref a, ref b, ref d) ||
                IsBetween(ref c, ref d, ref a) ||
                IsBetween(ref c, ref d, ref b))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// True if and only if segments AB and CD intersect properly.
        /// </summary>
        /// <remarks>
        /// Proper intersection: A point interior to both segments is shared. Properness determined by strict leftness.
        /// </remarks>
        /// <param name="a">Point A of segment AB.</param>
        /// <param name="b">Point B of segment AB.</param>
        /// <param name="c">Point C of segment CD.</param>
        /// <param name="d">Point D of segment CD.</param>
        /// <returns>A value indicating whether segements AB and CD are intersecting properly.</returns>
        public static bool IntersectProp(ref Vector3Int a, ref Vector3Int b, ref Vector3Int c, ref Vector3Int d)
        {
            //eliminate improper cases
            if (IsCollinear(ref a, ref b, ref c) ||
                IsCollinear(ref a, ref b, ref d) ||
                IsCollinear(ref c, ref d, ref a) ||
                IsCollinear(ref c, ref d, ref b))
            {
                return false;
            }

            return
                (!IsLeft(ref a, ref b, ref c) ^ !IsLeft(ref a, ref b, ref d)) &&
                (!IsLeft(ref c, ref d, ref a) ^ !IsLeft(ref c, ref d, ref b));
        }

        /// <summary>
        /// Compares two vertices for equality.
        /// </summary>
        /// <param name="left">A vertex.</param>
        /// <param name="right">Another vertex.</param>
        /// <returns>A value indicating whether the two vertices are equal.</returns>
        public static bool operator ==(Vector3Int left, Vector3Int right)
        {
            return left.Equals(right);
        }
        /// <summary>
        /// Compares two vertices for inequality.
        /// </summary>
        /// <param name="left">A vertex.</param>
        /// <param name="right">Another vertex.</param>
        /// <returns>A value indicating whether the two vertices are not equal.</returns>
        public static bool operator !=(Vector3Int left, Vector3Int right)
        {
            return !(left == right);
        }

        /// <summary>
        /// The X coordinate.
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// The Z coordinate.
        /// </summary>
        public int Z { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3Int"/> struct.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        public Vector3Int(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Compares another <see cref="Vector3Int"/> with this instance for equality.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns>A value indicating whether the two vertices are equal.</returns>
        public bool Equals(Vector3Int other)
        {
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }
        /// <summary>
        /// Compares an object with this instance for equality.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns>A value indicating whether the object is equal to this instance.</returns>
        public override bool Equals(object obj)
        {
            Vector3Int? p = obj as Vector3Int?;
            if (p.HasValue)
            {
                return this.Equals(p.Value);
            }

            return false;
        }
        /// <summary>
        /// Gets a hash code unique to the contents of this instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode();
        }
        /// <summary>
        /// Gets a human-readable version of the vertex.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return string.Format("X: {0}; Y: {1}; Z: {2}", this.X, this.Y, this.Z);
        }
    }
}
