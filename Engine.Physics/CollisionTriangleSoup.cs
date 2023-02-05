using SharpDX;
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
        private List<Triangle> triangleList = new List<Triangle>();

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
        /// <param name="rigidBody">Rigid body</param>
        /// <param name="triangles">Triangle list</param>
        public CollisionTriangleSoup(IRigidBody rigidBody, IEnumerable<Triangle> triangles) : base(rigidBody)
        {
            AddTriangles(triangles);
        }

        /// <summary>
        /// Adds the specified list of triangles to the triangle soup
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        public void AddTriangles(IEnumerable<Triangle> triangles)
        {
            if (triangles?.Any() != true)
            {
                return;
            }

            triangleList.AddRange(triangles);

            Update();
        }
        /// <summary>
        /// Updates the container bodies of the primitives
        /// </summary>
        private void Update()
        {
            List<Vector3> vertexList = new List<Vector3>();

            foreach (var tri in triangleList)
            {
                vertexList.AddRange(tri.GetVertices());
            }

            AABB = BoundingBox.FromPoints(vertexList.ToArray());
            SPH = BoundingSphere.FromPoints(vertexList.ToArray());
        }
    }
}
