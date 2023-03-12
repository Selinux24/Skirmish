using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Physics
{
    /// <summary>
    /// Collision triangle
    /// </summary>
    public class CollisionTriangle : CollisionPrimitive
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
        public CollisionTriangle(Triangle triangle) : base()
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
            if (!transform)
            {
                return triangle;
            }

            if ((RigidBody?.Transform ?? Matrix.Identity) == Matrix.Identity)
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
            if (!transform)
            {
                return vertices;
            }

            if ((RigidBody?.Transform ?? Matrix.Identity) == Matrix.Identity)
            {
                return vertices;
            }

            var trn = RigidBody.Transform;
            var verts = vertices.ToArray();
            Vector3.TransformCoordinate(verts, ref trn, verts);
            return verts;
        }
    }
}
