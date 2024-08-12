using Engine.BuiltIn.Format;
using Engine.Common;
using SharpDX;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Content
{
    /// <summary>
    /// Mesh information
    /// </summary>
    public struct MeshInfo
    {
        /// <summary>
        /// Created mesh
        /// </summary>
        public IMesh Mesh { get; set; }
        /// <summary>
        /// Material name
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// Creates a mesh information structure from a submesh content
        /// </summary>
        /// <param name="meshName">Mesh name</param>
        /// <param name="geometry">Submesh content</param>
        /// <param name="skinningInfo">Skinning information</param>
        /// <param name="constraint">Geometry constraint</param>
        public static async Task<MeshInfo?> CreateMesh(string meshName, SubMeshContent geometry, bool loadNormalMaps, IMaterialContent meshMaterial, SkinningInfo? skinningInfo, BoundingBox? constraint)
        {
            if (geometry.Vertices?.Length == 0)
            {
                return null;
            }

            //Process the vertex data
            bool preferTextured = geometry.Textured;
            bool useNormals = loadNormalMaps && meshMaterial?.NormalMapTexture != null;
            var vertexData = await geometry.ProcessVertexData(useNormals, constraint);
            var vertices = vertexData.vertices;
            var indices = vertexData.indices;

            IEnumerable<Weight> weights = [];
            IEnumerable<string> bones = [];
            bool useSkinning = skinningInfo.HasValue;
            if (useSkinning)
            {
                var bindShapeMatrix = skinningInfo.Value.BindShapeMatrix;
                if (!bindShapeMatrix.IsIdentity)
                {
                    vertices = VertexData.Transform(vertices, bindShapeMatrix);
                }

                weights = skinningInfo.Value.Weights ?? [];
                bones = skinningInfo.Value.BoneNames ?? [];
            }

            var cMesh = new MeshCreateParams(
                meshName,
                geometry.Topology,
                geometry.Transform,
                vertices,
                weights,
                bones,
                indices);

            var nMesh = await TryCreateMesh(cMesh, preferTextured, useNormals, useSkinning);

            //Material name
            string materialName = string.IsNullOrEmpty(geometry.Material) ? ContentData.NoMaterial : geometry.Material;

            return new MeshInfo()
            {
                Mesh = nMesh,
                MaterialName = materialName,
            };
        }

        /// <summary>
        /// Mesh creation parameters helper class
        /// </summary>
        /// <param name="Name">Mesh name</param>
        /// <param name="Geometry">Geometry data</param>
        /// <param name="Vertices">Vertex data</param>
        /// <param name="Weights">Weight data</param>
        /// <param name="BoneNames">Bone names</param>
        /// <param name="Indices">Indices</param>
        record MeshCreateParams(string Name, Topology Topology, Matrix Transform, IEnumerable<VertexData> Vertices, IEnumerable<Weight> Weights, IEnumerable<string> BoneNames, IEnumerable<uint> Indices);

        /// <summary>
        /// Try to create a new mesh
        /// </summary>
        /// <param name="createParams">Creation parameters</param>
        /// <param name="preferTextured">Sets whether textured formats were prefered over vertex colored formats</param>
        /// <param name="useNormalMapping">Use normal mapping</param>
        /// <param name="useSkinning">Use skinning info</param>
        private static async Task<IMesh> TryCreateMesh(MeshCreateParams createParams, bool preferTextured, bool useNormalMapping, bool useSkinning)
        {
            var v = createParams.Vertices.First();
            if (!v.Position.HasValue)
            {
                return null;
            }

            if (useNormalMapping || v.Normal.HasValue)
            {
                return await GetPositionNormalVariant(createParams, preferTextured, useSkinning);
            }

            return await GetPositionOnlyVariant(createParams, preferTextured, useSkinning);
        }

        /// <summary>
        /// Try to create a new mesh (Position without Normal)
        /// </summary>
        /// <param name="createParams">Creation parameters</param>
        /// <param name="preferTextured">Sets whether textured formats were prefered over vertex colored formats</param>
        /// <param name="useSkinning">Use skinning info</param>
        private static async Task<IMesh> GetPositionOnlyVariant(MeshCreateParams createParams, bool preferTextured, bool useSkinning)
        {
            var v = createParams.Vertices.First();
            if (preferTextured && v.Texture.HasValue)
            {
                return await GetPositionTextureVariant(createParams, useSkinning);
            }

            if (v.Color.HasValue)
            {
                return await GetPositionColorVariant(createParams, useSkinning);
            }

            if (useSkinning)
            {
                var vertices = await VertexSkinnedPosition.Convert(createParams.Vertices, createParams.Weights, createParams.BoneNames);
                return CreateMesh(createParams, vertices);
            }
            else
            {
                var vertices = await VertexPosition.Convert(createParams.Vertices);
                return CreateMesh(createParams, vertices);
            }
        }
        private static async Task<IMesh> GetPositionColorVariant(MeshCreateParams createParams, bool useSkinning)
        {
            if (useSkinning)
            {
                var vertices = await VertexSkinnedPositionColor.Convert(createParams.Vertices, createParams.Weights, createParams.BoneNames);
                return CreateMesh(createParams, vertices);
            }
            else
            {
                var vertices = await VertexPositionColor.Convert(createParams.Vertices);
                return CreateMesh(createParams, vertices);
            }
        }
        private static async Task<IMesh> GetPositionTextureVariant(MeshCreateParams createParams, bool useSkinning)
        {
            if (useSkinning)
            {
                var vertices = await VertexSkinnedPositionTexture.Convert(createParams.Vertices, createParams.Weights, createParams.BoneNames);
                return CreateMesh(createParams, vertices);
            }
            else
            {
                var vertices = await VertexPositionTexture.Convert(createParams.Vertices);
                return CreateMesh(createParams, vertices);
            }
        }

        /// <summary>
        /// Try to create a new mesh (Position with Normal)
        /// </summary>
        /// <param name="createParams">Creation parameters</param>
        /// <param name="preferTextured">Sets whether textured formats were prefered over vertex colored formats</param>
        /// <param name="useSkinning">Use skinning info</param>
        private static async Task<IMesh> GetPositionNormalVariant(MeshCreateParams createParams, bool preferTextured, bool useSkinning)
        {
            var v = createParams.Vertices.First();

            if (preferTextured && v.Texture.HasValue)
            {
                return await GetPositionNormalTangentVariant(createParams, useSkinning);
            }

            return await GetPositionNormalColorVariant(createParams, useSkinning);
        }
        private static async Task<IMesh> GetPositionNormalColorVariant(MeshCreateParams createParams, bool useSkinning)
        {
            if (useSkinning)
            {
                var vertices = await VertexSkinnedPositionNormalColor.Convert(createParams.Vertices, createParams.Weights, createParams.BoneNames);
                return CreateMesh(createParams, vertices);
            }
            else
            {
                var vertices = await VertexPositionNormalColor.Convert(createParams.Vertices);
                return CreateMesh(createParams, vertices);
            }
        }
        private static async Task<IMesh> GetPositionNormalTangentVariant(MeshCreateParams createParams, bool useSkinning)
        {
            var v = createParams.Vertices.First();

            if (useSkinning)
            {
                if (v.Tangent.HasValue)
                {
                    var vertices = await VertexSkinnedPositionNormalTextureTangent.Convert(createParams.Vertices, createParams.Weights, createParams.BoneNames);
                    return CreateMesh(createParams, vertices);
                }
                else
                {
                    var vertices = await VertexSkinnedPositionNormalTexture.Convert(createParams.Vertices, createParams.Weights, createParams.BoneNames);
                    return CreateMesh(createParams, vertices);
                }
            }

            if (v.Tangent.HasValue)
            {
                var vertices = await VertexPositionNormalTextureTangent.Convert(createParams.Vertices);
                return CreateMesh(createParams, vertices);
            }
            else
            {
                var vertices = await VertexPositionNormalTexture.Convert(createParams.Vertices);
                return CreateMesh(createParams, vertices);
            }
        }

        /// <summary>
        /// Creates a typed mesh
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="createParams">Creation parameters</param>
        /// <param name="vertices">Formatted vertices</param>
        private static Mesh<T> CreateMesh<T>(MeshCreateParams createParams, IEnumerable<T> vertices)
            where T : struct, IVertexData
        {
            return new Mesh<T>(
                createParams.Name,
                createParams.Topology,
                createParams.Transform,
                vertices,
                createParams.Indices);
        }
    }
}
