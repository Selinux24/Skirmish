using Engine.Mathematics;
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
        public Vector3[] Vertices { get; set; }
        /// <summary>
        /// Normals
        /// </summary>
        public Vector3[] Normals { get; set; }
        /// <summary>
        /// UV texture coordinates
        /// </summary>
        public Vector2[] Uvs { get; set; }
        /// <summary>
        /// Tangents
        /// </summary>
        public Vector3[] Tangents { get; set; }
        /// <summary>
        /// Binormals
        /// </summary>
        public Vector3[] Binormals { get; set; }
        /// <summary>
        /// Index data
        /// </summary>
        public uint[] Indices { get; set; }
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
        public bool ContainsVertexData { get { return (Vertices?.Length ?? 0) > 0; } }
        /// <summary>
        /// Gets whether the descriptor has normal data or not
        /// </summary>
        public bool ContainsNormalData { get { return (Normals?.Length ?? 0) > 0; } }
        /// <summary>
        /// Gets whether the descriptor has UV data or not
        /// </summary>
        public bool ContainsUvData { get { return (Uvs?.Length ?? 0) > 0; } }
        /// <summary>
        /// Gets whether the descriptor has tangent data or not
        /// </summary>
        public bool ContainsTangentData { get { return (Tangents?.Length ?? 0) > 0; } }
        /// <summary>
        /// Gets whether the descriptor has bi-normal data or not
        /// </summary>
        public bool ContainsBinormalData { get { return (Binormals?.Length ?? 0) > 0; } }
        /// <summary>
        /// Gets whether the descriptor has index data or not
        /// </summary>
        public bool ContainsIndexData { get { return (Indices?.Length ?? 0) > 0; } }

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
        public void Transform(Matrix4x4 transform)
        {
            if (ContainsVertexData) Vector3.TransformCoordinate(Vertices, ref transform, Vertices);
            if (ContainsNormalData) Vector3.TransformNormal(Normals, ref transform, Normals);
            if (ContainsTangentData) Vector3.TransformNormal(Tangents, ref transform, Tangents);
            if (ContainsBinormalData) Vector3.TransformNormal(Binormals, ref transform, Binormals);
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
                indexOffset = (uint)(Vertices?.Length ?? 0);

                Vertices = [.. Vertices, .. geom.Vertices ?? []];
            }

            if (geom.ContainsNormalData)
            {
                Normals = [.. Normals, .. geom.Normals ?? []];
            }

            if (geom.ContainsUvData)
            {
                Uvs = [.. Uvs, .. geom.Uvs ?? []];
            }

            if (geom.ContainsTangentData)
            {
                Tangents = [.. Tangents, .. geom.Tangents ?? []];
            }

            if (geom.ContainsBinormalData)
            {
                Binormals = [.. Binormals, .. geom.Binormals ?? []];
            }

            if (geom.ContainsIndexData)
            {
                uint[] idx = geom.Indices?.Select(i => i + indexOffset).ToArray() ?? [];

                Indices = [.. Indices, .. idx];
            }

            return true;
        }
    }
}
