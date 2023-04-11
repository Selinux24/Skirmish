using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Physics.Colliders
{
    /// <summary>
    /// Collision triangle soup
    /// </summary>
    public class MeshCollider : Collider
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
        public MeshCollider(IEnumerable<Triangle> triangles) : base()
        {
            if (triangles?.Any() != true)
            {
                throw new ArgumentOutOfRangeException(nameof(triangles), $"{nameof(MeshCollider)} must have one triangle at least.");
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
            var furthest_point = triangles[0].Point1;

            // Dumb O(n) support function, just brute force check all points
            float max_dot = Vector3.Dot(furthest_point, dir);

            for (int t = 0; t < triangles.Length; t++)
            {
                var verts = triangles[t].GetVertices();

                for (int i = 0; i < 3; i++)
                {
                    var v = verts.ElementAt(i);

                    float d = Vector3.Dot(v, dir);
                    if (d > max_dot)
                    {
                        max_dot = d;
                        furthest_point = v;
                    }
                }
            }

            //convert support to world space
            return Vector3.TransformNormal(furthest_point, RotationScale) + Position;
        }
    }
}
