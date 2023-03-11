using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Physics
{
    /// <summary>
    /// Collision triangle soup
    /// </summary>
    public class CollisionTriangleSoup : CollisionPrimitive
    {
        /// <summary>
        /// Triangle list
        /// </summary>
        private readonly Triangle[] triangles;
        /// <summary>
        /// Vertex list
        /// </summary>
        private readonly Vector3[] vertices;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        public CollisionTriangleSoup(IEnumerable<Triangle> triangles) : base()
        {
            if (triangles?.Any() != true)
            {
                throw new ArgumentOutOfRangeException(nameof(triangles), $"{nameof(CollisionTriangleSoup)} must have one triangle at least.");
            }

            this.triangles = triangles.Distinct().ToArray();

            vertices = triangles.SelectMany(t => t.GetVertices()).Distinct().ToArray();

            boundingBox = BoundingBox.FromPoints(vertices);
            boundingSphere = BoundingSphere.FromPoints(vertices);
            orientedBoundingBox = new OrientedBoundingBox(boundingBox);
        }

        /// <summary>
        /// Gets the triangle list
        /// </summary>
        /// <param name="transform">Use rigid body transform matrix</param>
        public IEnumerable<Triangle> GetTriangles(bool transform = false)
        {
            if (!transform)
            {
                return triangles;
            }

            if ((RigidBody?.Transform ?? Matrix.Identity) == Matrix.Identity)
            {
                return triangles;
            }

            return Triangle.Transform(triangles, RigidBody.Transform);
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
