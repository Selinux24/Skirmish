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
        /// Gets the triangle list
        /// </summary>
        public IEnumerable<Triangle> Triangles
        {
            get
            {
                if ((RigidBody?.Transform ?? Matrix.Identity) == Matrix.Identity)
                {
                    return triangles;
                }

                return Triangle.Transform(triangles, RigidBody.Transform);
            }
        }
        /// <summary>
        /// Gets the vertex list
        /// </summary>
        public IEnumerable<Vector3> Vertices
        {
            get
            {
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

            this.triangles = triangles.ToArray();

            vertices = triangles.SelectMany(t => t.GetVertices()).Distinct().ToArray();

            boundingBox = BoundingBox.FromPoints(vertices);
            boundingSphere = BoundingSphere.FromPoints(vertices);
            orientedBoundingBox = new OrientedBoundingBox(boundingBox);
        }
    }
}
