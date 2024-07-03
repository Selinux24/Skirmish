using SharpDX;
using System;

namespace Engine.Physics.EPA
{
    using GJKSupportPoint = GJK.SupportPoint;

    /// <summary>
    /// Face helper
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public struct Face(GJKSupportPoint a, GJKSupportPoint b, GJKSupportPoint c)
    {
        /// <summary>
        /// Point A
        /// </summary>
        public GJKSupportPoint A { get; set; } = a;
        /// <summary>
        /// Point B
        /// </summary>
        public GJKSupportPoint B { get; set; } = b;
        /// <summary>
        /// Point C
        /// </summary>
        public GJKSupportPoint C { get; set; } = c;
        /// <summary>
        /// Normal
        /// </summary>
        public Vector3 Normal { get; private set; } = Vector3.Normalize(Vector3.Cross(b.Point - a.Point, c.Point - a.Point));

        /// <summary>
        /// Reverse face
        /// </summary>
        public void Reverse()
        {
            (B, A) = (A, B);
            Normal = -Normal;
        }
        /// <summary>
        /// Get face edge by index
        /// </summary>
        public readonly Edge GetEdge(int index)
        {
            return new Edge
            {
                A = GetPoint(index),
                B = GetPoint((index + 1) % 3),
            };
        }
        /// <summary>
        /// Gets the face point by index
        /// </summary>
        /// <param name="index">Index</param>
        /// <exception cref="ArgumentOutOfRangeException">Expected 0 to 2 values</exception>
        public readonly GJKSupportPoint GetPoint(int index)
        {
            return index switch
            {
                0 => A,
                1 => B,
                2 => C,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }
    }
}
