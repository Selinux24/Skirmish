using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Physics.Colliders
{
    /// <summary>
    /// Triangle collider
    /// </summary>
    public class TriangleCollider : Collider
    {
        /// <summary>
        /// Triangle
        /// </summary>
        private readonly Triangle triangle;
        /// <summary>
        /// Vertex list
        /// </summary>
        private readonly Vector3[] vertices;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="triangle">Triangle</param>
        public TriangleCollider(Triangle triangle) : base()
        {
            this.triangle = triangle;

            vertices = triangle.GetVertices().ToArray();

            boundingBox = BoundingBox.FromPoints(vertices);
            boundingSphere = BoundingSphere.FromPoints(vertices);
            orientedBoundingBox = new OrientedBoundingBox(boundingBox);
        }

        /// <summary>
        /// Gets the triangle
        /// </summary>
        /// <param name="transform">Use rigid body transform matrix</param>
        public Triangle GetTriangle(bool transform = false)
        {
            if (!transform || !HasTransform)
            {
                return triangle;
            }

            return Triangle.Transform(triangle, RigidBody.Transform);
        }
        /// <summary>
        /// Gets the vertex list
        /// </summary>
        /// <param name="transform">Use rigid body transform matrix</param>
        public IEnumerable<Vector3> GetVertices(bool transform = false)
        {
            if (!transform || !HasTransform)
            {
                return vertices;
            }

            var trn = RigidBody.Transform;
            var verts = vertices.ToArray();
            Vector3.TransformCoordinate(verts, ref trn, verts);
            return verts;
        }

        /// <inheritdoc/>
        public override Vector3 Support(Vector3 dir)
        {
            //find support in model space
            dir = Vector3.TransformNormal(dir, RotationScaleInverse);

            Vector3 furthest_point = vertices[0];
            float max_dot = Vector3.Dot(furthest_point, dir);

            for (int i = 1; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
                float d = Vector3.Dot(v, dir);
                if (d > max_dot)
                {
                    max_dot = d;
                    furthest_point = v;
                }
            }

            //Fake some depth behind triangle so we have volume
            if (Vector3.Dot(dir, triangle.Normal) < 0)
            {
                furthest_point -= triangle.Normal;
            }

            //convert support to world space
            return Vector3.TransformNormal(furthest_point, RotationScale) + Position;
        }
    }
}
