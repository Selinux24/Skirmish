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
        /// Gets whether the descriptor has any data or not
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                bool anyData =
                    ContainsVertexData ||
                    ContainsNormalData ||
                    ContainsUvData ||
                    ContainsTangentData ||
                    ContainsBinormalData ||
                    ContainsIndexData;

                return !anyData;
            }
        }
        /// <summary>
        /// Gets whether the descriptor has vertex data or not
        /// </summary>
        public bool ContainsVertexData { get { return Vertices?.Any() ?? false; } }
        /// <summary>
        /// Gets whether the descriptor has normal data or not
        /// </summary>
        public bool ContainsNormalData { get { return Normals?.Any() ?? false; } }
        /// <summary>
        /// Gets whether the descriptor has UV data or not
        /// </summary>
        public bool ContainsUvData { get { return Uvs?.Any() ?? false; } }
        /// <summary>
        /// Gets whether the descriptor has tangent data or not
        /// </summary>
        public bool ContainsTangentData { get { return Tangents?.Any() ?? false; } }
        /// <summary>
        /// Gets whether the descriptor has bi-normal data or not
        /// </summary>
        public bool ContainsBinormalData { get { return Binormals?.Any() ?? false; } }
        /// <summary>
        /// Gets whether the descriptor has index data or not
        /// </summary>
        public bool ContainsIndexData { get { return Indices?.Any() ?? false; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public GeometryDescriptor()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public GeometryDescriptor(IEnumerable<GeometryDescriptor> descriptors)
        {
            foreach (var desc in descriptors)
            {
                Merge(desc);
            }
        }

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

        /// <summary>
        /// Validates the merge with other geometry descriptor
        /// </summary>
        /// <param name="geom">Geometry descriptor to merge with</param>
        public bool ValidateMerge(GeometryDescriptor geom)
        {
            if (IsEmpty)
            {
                return true;
            }

            if (geom == null)
            {
                return false;
            }

            if (geom.IsEmpty)
            {
                return true;
            }

            if (ContainsVertexData != geom.ContainsVertexData ||
                ContainsNormalData != geom.ContainsNormalData ||
                ContainsUvData != geom.ContainsUvData ||
                ContainsTangentData != geom.ContainsTangentData ||
                ContainsBinormalData != geom.ContainsBinormalData ||
                ContainsIndexData != geom.ContainsIndexData)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Merges the specified geometry descriptor
        /// </summary>
        /// <param name="geom">Geometry descriptor</param>
        /// <returns>Returns true if the merge works.</returns>
        public bool Merge(GeometryDescriptor geom)
        {
            if (!ValidateMerge(geom))
            {
                return false;
            }

            uint indexOffset = 0;
            if (geom.ContainsVertexData)
            {
                indexOffset = (uint)(Vertices?.Count() ?? 0);

                Vertices = Vertices?.Concat(geom.Vertices) ?? geom.Vertices;
            }

            if (geom.ContainsNormalData)
            {
                Normals = Normals?.Concat(geom.Normals) ?? geom.Normals;
            }

            if (geom.ContainsUvData)
            {
                Uvs = Uvs?.Concat(geom.Uvs) ?? geom.Uvs;
            }

            if (geom.ContainsTangentData)
            {
                Tangents = Tangents?.Concat(geom.Tangents) ?? geom.Tangents;
            }

            if (geom.ContainsBinormalData)
            {
                Binormals = Binormals?.Concat(geom.Binormals) ?? geom.Binormals;
            }

            if (geom.ContainsIndexData)
            {
                Indices = Indices?.Concat(geom.Indices.Select(i => i + indexOffset)) ?? geom.Indices;
            }

            return true;
        }
    }
}
