using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Physics.Colliders
{
    /// <summary>
    /// Collision triangle
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
            var furthest_point = triangle.Point1;

            //Find which triangle vertex is furthest along dir
            float dot0 = Vector3.Dot(triangle.Point1, dir);
            float dot1 = Vector3.Dot(triangle.Point2, dir);
            float dot2 = Vector3.Dot(triangle.Point3, dir);

            if (dot1 > dot0)
            {
                furthest_point = triangle.Point2;
                if (dot2 > dot1)
                {
                    furthest_point = triangle.Point3;
                }
            }
            else if (dot2 > dot0)
            {
                furthest_point = triangle.Point3;
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
