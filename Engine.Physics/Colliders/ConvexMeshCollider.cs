using Engine.Mathematics;
using System;
using System.Collections.Generic;

namespace Engine.Physics.Colliders
{
    /// <summary>
    /// Convex mesh collider
    /// </summary>
    public class ConvexMeshCollider : Collider
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
        public ConvexMeshCollider(Triangle[] triangles) : base()
        {
            ArgumentNullException.ThrowIfNull(triangles);
            if (triangles.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(triangles), $"{nameof(ConvexMeshCollider)} must have one triangle at least.");
            }

            this.triangles = Helper.Distinct(triangles);

            List<Vector3> vertList = [];
            Array.ForEach(this.triangles, t => vertList.AddRange(t.GetVertices()));

            vertices = new Vector3[vertList.Count];
            vertList.CopyTo(vertices);

            vertices = Helper.Distinct(vertices);

            boundingBox = BoundingBox.FromPoints(vertices);
            boundingSphere = BoundingSphere.FromPoints(vertices);
            orientedBoundingBox = new OrientedBoundingBox(boundingBox);
        }

        /// <summary>
        /// Gets the triangle list
        /// </summary>
        /// <param name="transform">Use rigid body transform matrix</param>
        public Triangle[] GetTriangles(bool transform = false)
        {
            if (!transform || !HasTransform)
            {
                return triangles;
            }

            return Triangle.Transform(triangles, RigidBody.Transform);
        }
        /// <summary>
        /// Gets the vertex list
        /// </summary>
        /// <param name="transform">Use rigid body transform matrix</param>
        public Vector3[] GetVertices(bool transform = false)
        {
            if (!transform || !HasTransform)
            {
                return vertices;
            }

            var trn = RigidBody.Transform;
            var verts = new Vector3[vertices.Length];
            Vector3.TransformCoordinate(vertices, ref trn, verts);
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

            //convert support to world space
            return Vector3.TransformNormal(furthest_point, RotationScale) + Position;
        }
    }
}
