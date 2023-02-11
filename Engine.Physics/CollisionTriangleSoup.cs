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
        private readonly List<Triangle> triangleList = new List<Triangle>();

        /// <summary>
        /// Gets the triangle list
        /// </summary>
        public IEnumerable<Triangle> Triangles
        {
            get
            {
                return triangleList.ToArray();
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

            triangleList.AddRange(triangles);

            var vertexList = triangleList.SelectMany(t => t.GetVertices()).ToArray();

            boundingBox = BoundingBox.FromPoints(vertexList);
            boundingSphere = BoundingSphere.FromPoints(vertexList);
            orientedBoundingBox = new OrientedBoundingBox(boundingBox);
        }
    }
}
