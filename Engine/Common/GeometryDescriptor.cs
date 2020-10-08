using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Geometry descriptor
    /// </summary>
    public class GeometryDescriptor
    {
        /// <summary>
        /// Vertex data
        /// </summary>
        public IEnumerable<Vector3> Vertices { get; set; }
        /// <summary>
        /// Normals
        /// </summary>
        public IEnumerable<Vector3> Normals { get; set; }
        /// <summary>
        /// UV texture coordinates
        /// </summary>
        public IEnumerable<Vector2> Uvs { get; set; }
        /// <summary>
        /// Tangents
        /// </summary>
        public IEnumerable<Vector3> Tangents { get; set; }
        /// <summary>
        /// Binormals
        /// </summary>
        public IEnumerable<Vector3> Binormals { get; set; }
        /// <summary>
        /// Index data
        /// </summary>
        public IEnumerable<uint> Indices { get; set; }

        /// <summary>
        /// Transforms the geometry data
        /// </summary>
        /// <param name="transform">Transform matrix</param>
        public void Transform(Matrix transform)
        {
            Vertices = Vertices?.Select(v => Vector3.TransformCoordinate(v, transform));
            Normals = Normals?.Select(v => Vector3.TransformNormal(v, transform));
            Tangents = Tangents?.Select(v => Vector3.TransformNormal(v, transform));
            Binormals = Binormals?.Select(v => Vector3.TransformNormal(v, transform));
        }
    }
}
