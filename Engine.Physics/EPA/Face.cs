using SharpDX;
using System;

namespace Engine.Physics.EPA
{
    /// <summary>
    /// Face helper
    /// </summary>
    public struct Face
    {
        /// <summary>
        /// Point A
        /// </summary>
        public Vector3 A { get; set; }
        /// <summary>
        /// Point B
        /// </summary>
        public Vector3 B { get; set; }
        /// <summary>
        /// Point C
        /// </summary>
        public Vector3 C { get; set; }
        /// <summary>
        /// Normal
        /// </summary>
        public Vector3 Normal { get; private set; }

        /// <summary>
        /// Gets the face point by index
        /// </summary>
        /// <param name="index">Index</param>
        /// <exception cref="ArgumentOutOfRangeException">Expected 0 to 2 values</exception>
        public Vector3 this[int index]
        {
            get
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

        /// <summary>
        /// Constructor
        /// </summary>
        public Face(Vector3 a, Vector3 b, Vector3 c)
        {
            A = a;
            B = b;
            C = c;
            Normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
        }

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
        public Edge GetEdge(int index)
        {
            return new Edge
            {
                A = this[index],
                B = this[(index + 1) % 3]
            };
        }
    }
}
