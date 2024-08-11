using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Common
{
    /// <summary>
    /// Vertex helper
    /// </summary>
    [Serializable]
    public struct VertexData : IVertexList
    {
        /// <summary>
        /// Face index
        /// </summary>
        public int FaceIndex { get; set; }
        /// <summary>
        /// Vertex index
        /// </summary>
        public int VertexIndex { get; set; }
        /// <summary>
        /// Position
        /// </summary>
        public Vector3? Position { get; set; }
        /// <summary>
        /// Normal
        /// </summary>
        public Vector3? Normal { get; set; }
        /// <summary>
        /// Tangent
        /// </summary>
        public Vector3? Tangent { get; set; }
        /// <summary>
        /// Binormal
        /// </summary>
        public Vector3? BiNormal { get; set; }
        /// <summary>
        /// Texture UV
        /// </summary>
        public Vector2? Texture { get; set; }
        /// <summary>
        /// Color
        /// </summary>
        public Color4? Color { get; set; }
        /// <summary>
        /// Sprite size
        /// </summary>
        public Vector2? Size { get; set; }
        /// <summary>
        /// Vertex weights
        /// </summary>
        public float[] Weights { get; set; }
        /// <summary>
        /// Bone weights
        /// </summary>
        public byte[] BoneIndices { get; set; }

        /// <summary>
        /// Apply weighted transforms to the vertext data
        /// </summary>
        /// <param name="vertex">Vertex data</param>
        /// <param name="boneTransforms">Bone transforms list</param>
        /// <returns>Returns the weighted position</returns>
        public static Vector3 ApplyWeight(IVertexData vertex, IEnumerable<Matrix> boneTransforms)
        {
            if (!vertex.HasChannel(VertexDataChannels.Position))
            {
                return Vector3.Zero;
            }

            var position = vertex.GetChannelValue<Vector3>(VertexDataChannels.Position);

            if (!vertex.HasChannel(VertexDataChannels.BoneIndices) || !vertex.HasChannel(VertexDataChannels.Weights))
            {
                return position;
            }

            var boneIndices = vertex.GetChannelValue<byte[]>(VertexDataChannels.BoneIndices);
            var boneWeights = vertex.GetChannelValue<float[]>(VertexDataChannels.Weights);
            var transforms = boneTransforms.ToArray();

            var t = Vector3.Zero;

            for (int w = 0; w < boneIndices.Length; w++)
            {
                var p = position;
                var weight = boneWeights[w];
                if (weight <= 0)
                {
                    continue;
                }

                var index = boneIndices[w];
                var boneTransform = transforms[index];
                if (!boneTransform.IsIdentity)
                {
                    Vector3.TransformCoordinate(ref position, ref boneTransform, out p);
                }

                t += p * weight;
            }

            return t;
        }

        /// <summary>
        /// Transforms the specified vertex list by the given transform matrix
        /// </summary>
        /// <param name="vertices">Vertex list</param>
        /// <param name="transform">Transform matrix</param>
        /// <returns>Returns the transformed vertex list</returns>
        public static IEnumerable<VertexData> Transform(IEnumerable<VertexData> vertices, Matrix transform)
        {
            if (vertices?.Any() != true)
            {
                return [];
            }

            if (transform.IsIdentity)
            {
                return vertices.ToArray();
            }

            return vertices
                .AsParallel()
                .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                .Select(r => Transform(r, transform))
                .ToArray();
        }
        /// <summary>
        /// Transforms the specified vertex by the given transform matrix
        /// </summary>
        /// <param name="vertex">Vertex</param>
        /// <param name="transform">Transform matrix</param>
        /// <returns>Returns the transformed vertex</returns>
        public static VertexData Transform(VertexData vertex, Matrix transform)
        {
            if (transform.IsIdentity)
            {
                return vertex;
            }

            VertexData result = vertex;

            if (result.Position.HasValue)
            {
                result.Position = Vector3.TransformCoordinate(result.Position.Value, transform);
            }

            if (result.Normal.HasValue)
            {
                result.Normal = Vector3.TransformNormal(result.Normal.Value, transform);
            }

            if (result.Tangent.HasValue)
            {
                result.Tangent = Vector3.TransformNormal(result.Tangent.Value, transform);
            }

            if (result.BiNormal.HasValue)
            {
                result.BiNormal = Vector3.TransformNormal(result.BiNormal.Value, transform);
            }

            return result;
        }
        /// <summary>
        /// Transforms the specified vertex list by the given transform matrix
        /// </summary>
        /// <param name="vertices">Vertex list</param>
        /// <param name="transform">Transform matrix</param>
        /// <returns>Returns the transformed vertex list</returns>
        public static IEnumerable<T> Transform<T>(IEnumerable<T> vertices, Matrix transform)
            where T : struct, IVertexData
        {
            if (vertices?.Any() != true)
            {
                return [];
            }

            if (transform.IsIdentity)
            {
                return vertices.ToArray();
            }

            return vertices
                .AsParallel()
                .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                .Select(r => Transform(r, transform))
                .ToArray();
        }
        /// <summary>
        /// Transforms the specified vertex by the given transform matrix
        /// </summary>
        /// <param name="vertex">Vertex</param>
        /// <param name="transform">Transform matrix</param>
        /// <returns>Returns the transformed vertex</returns>
        public static T Transform<T>(T vertex, Matrix transform)
            where T : struct, IVertexData
        {
            if (transform.IsIdentity)
            {
                return vertex;
            }

            T result = vertex;

            if (result.HasChannel(VertexDataChannels.Position))
            {
                var position = result.GetChannelValue<Vector3>(VertexDataChannels.Position);
                result.SetChannelValue(VertexDataChannels.Position, Vector3.TransformCoordinate(position, transform));
            }

            if (result.HasChannel(VertexDataChannels.Normal))
            {
                var normal = result.GetChannelValue<Vector3>(VertexDataChannels.Normal);
                result.SetChannelValue(VertexDataChannels.Normal, Vector3.TransformNormal(normal, transform));
            }

            if (result.HasChannel(VertexDataChannels.Tangent))
            {
                var tangent = result.GetChannelValue<Vector3>(VertexDataChannels.Tangent);
                result.SetChannelValue(VertexDataChannels.Tangent, Vector3.TransformNormal(tangent, transform));
            }

            if (result.HasChannel(VertexDataChannels.BiNormal))
            {
                var binormal = result.GetChannelValue<Vector3>(VertexDataChannels.BiNormal);
                result.SetChannelValue(VertexDataChannels.BiNormal, Vector3.TransformNormal(binormal, transform));
            }

            return result;
        }

        /// <summary>
        /// Generates a vertex data array from a geometry descriptor
        /// </summary>
        /// <param name="descriptor">Geometry descriptor</param>
        /// <returns>Returns a vertex array</returns>
        public static IEnumerable<VertexData> FromDescriptor(GeometryDescriptor descriptor)
        {
            var vertices = descriptor.Vertices?.ToArray() ?? [];
            var normals = descriptor.Normals?.ToArray() ?? [];
            var uvs = descriptor.Uvs?.ToArray() ?? [];
            var tangents = descriptor.Tangents?.ToArray() ?? [];
            var binormals = descriptor.Binormals?.ToArray() ?? [];

            VertexData[] res = new VertexData[vertices.Length];

            Parallel.For(0, vertices.Length, (i) =>
            {
                res[i] = new VertexData()
                {
                    Position = vertices[i],
                    Normal = normals.Length != 0 ? normals[i] : null,
                    Texture = uvs.Length != 0 ? uvs[i] : null,
                    Tangent = tangents.Length != 0 ? tangents[i] : null,
                    BiNormal = binormals.Length != 0 ? binormals[i] : null,
                };
            });

            return res;
        }

        /// <summary>
        /// Transforms this vertex by the given matrix
        /// </summary>
        /// <param name="transform">Transformation matrix</param>
        /// <returns>Returns the transformed vertex</returns>
        public readonly VertexData Transform(Matrix transform)
        {
            return Transform(this, transform);
        }

        /// <inheritdoc/>
        public readonly IEnumerable<Vector3> GetVertices()
        {
            return [Position.Value];
        }
        /// <inheritdoc/>
        public readonly int GetStride()
        {
            return 1;
        }
        /// <inheritdoc/>
        public readonly Topology GetTopology()
        {
            return Topology.PointList;
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            string text = null;

            if (Weights != null && Weights.Length > 0) text += "Skinned; ";

            text += string.Format("FaceIndex: {0}; ", FaceIndex);
            text += string.Format("VertexIndex: {0}; ", VertexIndex);

            if (Position.HasValue) text += string.Format("Position: {0}; ", Position);
            if (Normal.HasValue) text += string.Format("Normal: {0}; ", Normal);
            if (Tangent.HasValue) text += string.Format("Tangent: {0}; ", Tangent);
            if (BiNormal.HasValue) text += string.Format("BiNormal: {0}; ", BiNormal);
            if (Texture.HasValue) text += string.Format("Texture: {0}; ", Texture);
            if (Color.HasValue) text += string.Format("Color: {0}; ", Color);
            if (Size.HasValue) text += string.Format("Size: {0}; ", Size);
            if (Weights != null && Weights.Length > 0) text += string.Format("Weights: {0}; ", Weights.Length);
            if (BoneIndices != null && BoneIndices.Length > 0) text += string.Format("BoneIndices: {0}; ", BoneIndices.Length);

            return text;
        }
    }
}
