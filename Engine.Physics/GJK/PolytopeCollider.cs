using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Physics.GJK
{
    /// <summary>
    /// Polytope: Just a set of points
    /// </summary>
    public struct PolytopeCollider : ICollider
    {
        /// <summary>
        /// (x0 y0 z0 x1 y1 z1 etc)
        /// </summary>
        public Vector3[] Points { get; set; } = Array.Empty<Vector3>();
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Matrix RotationScale { get; set; } = Matrix.Identity;
        public Matrix RotationScaleInverse => Matrix.Invert(RotationScale);

        public PolytopeCollider()
        {

        }

        public PolytopeCollider(IEnumerable<Vector3> points)
        {
            Points = points?.ToArray() ?? Array.Empty<Vector3>();
        }

        public Vector3 Support(Vector3 dir)
        {
            // Dumb O(n) support function, just brute force check all points
            dir = Vector3.TransformNormal(dir, RotationScaleInverse); //find support in model space

            Vector3 furthest_point = Points[0];
            float max_dot = Vector3.Dot(furthest_point, dir);

            for (int i = 1; i < Points.Length; i++)
            {
                Vector3 v = Points[i];
                float d = Vector3.Dot(v, dir);
                if (d > max_dot)
                {
                    max_dot = d;
                    furthest_point = v;
                }
            }

            return Vector3.TransformNormal(furthest_point, RotationScale) + Position; //convert support to world space
        }
    }
}
