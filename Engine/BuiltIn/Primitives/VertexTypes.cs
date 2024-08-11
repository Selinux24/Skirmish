using Engine.Common;
using Engine.Content;
using SharpDX;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Primitives
{
    /// <summary>
    /// Vertext types enumeration
    /// </summary>
    public enum VertexTypes
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Billboard
        /// </summary>
        Billboard,
        /// <summary>
        /// Font
        /// </summary>
        Font,
        /// <summary>
        /// CPU Particle
        /// </summary>
        CPUParticle,
        /// <summary>
        /// GPU Particles
        /// </summary>
        GPUParticle,
        /// <summary>
        /// Terrain
        /// </summary>
        Terrain,
        /// <summary>
        /// Decal
        /// </summary>
        Decal,

        /// <summary>
        /// Position
        /// </summary>
        Position,
        /// <summary>
        /// Position and color
        /// </summary>
        PositionColor,
        /// <summary>
        /// Position and texture
        /// </summary>
        PositionTexture,
        /// <summary>
        /// Position, normal and color
        /// </summary>
        PositionNormalColor,
        /// <summary>
        /// Position, normal and texture
        /// </summary>
        PositionNormalTexture,
        /// <summary>
        /// Position, normal, texture and tangents
        /// </summary>
        PositionNormalTextureTangent,

        /// <summary>
        /// Position for skinning animation
        /// </summary>
        PositionSkinned,
        /// <summary>
        /// Position and color for skinning animation
        /// </summary>
        PositionColorSkinned,
        /// <summary>
        /// Position and texture for skinning animation
        /// </summary>
        PositionTextureSkinned,
        /// <summary>
        /// Position, normal and color for skinning animation
        /// </summary>
        PositionNormalColorSkinned,
        /// <summary>
        /// Position, normal and texture for skinning animation
        /// </summary>
        PositionNormalTextureSkinned,
        /// <summary>
        /// Position, normal, texture and tangents for skinning animation
        /// </summary>
        PositionNormalTextureTangentSkinned,
    }

    /// <summary>
    /// Vertex types helper
    /// </summary>
    public static class VertexTypesHelper
    {
        /// <summary>
        /// Gets whether specified vertex type is skinned or not
        /// </summary>
        /// <param name="vertexTypes">Vertex type</param>
        /// <returns>Returns true if the vertex type contains skinning info</returns>
        public static bool IsSkinned(VertexTypes vertexTypes)
        {
            return
                vertexTypes == VertexTypes.PositionSkinned ||
                vertexTypes == VertexTypes.PositionColorSkinned ||
                vertexTypes == VertexTypes.PositionNormalColorSkinned ||
                vertexTypes == VertexTypes.PositionTextureSkinned ||
                vertexTypes == VertexTypes.PositionNormalTextureSkinned ||
                vertexTypes == VertexTypes.PositionNormalTextureTangentSkinned;
        }
        /// <summary>
        /// Gets whether specified vertex type has tangent channel or not
        /// </summary>
        /// <param name="vertexTypes">Vertex type</param>
        /// <returns>Returns true if the vertex type has tangent channel info</returns>
        public static bool IsTangent(VertexTypes vertexTypes)
        {
            return
                vertexTypes == VertexTypes.PositionNormalTextureTangent ||
                vertexTypes == VertexTypes.PositionNormalTextureTangentSkinned;
        }
        /// <summary>
        /// Gets whether specified vertex type is textured or not
        /// </summary>
        /// <param name="vertexTypes">Vertex type</param>
        /// <returns>Returns true if the vertex type contains texture map info</returns>
        public static bool IsTextured(VertexTypes vertexTypes)
        {
            return
                vertexTypes == VertexTypes.Billboard ||

                vertexTypes == VertexTypes.PositionTexture ||
                vertexTypes == VertexTypes.PositionNormalTexture ||
                vertexTypes == VertexTypes.PositionNormalTextureTangent ||

                vertexTypes == VertexTypes.PositionTextureSkinned ||
                vertexTypes == VertexTypes.PositionNormalTextureSkinned ||
                vertexTypes == VertexTypes.PositionNormalTextureTangentSkinned;
        }
        /// <summary>
        /// Gets the vertex type based on vertex data
        /// </summary>
        /// <param name="v">Vertex</param>
        /// <param name="preferTextured">Sets whether textured formats were prefered over vertex colored formats</param>
        /// <returns>Returns the vertex type</returns>
        public static VertexTypes GetVertexType(VertexData v, bool preferTextured = true)
        {
            if (v.Position.HasValue)
            {
                if (v.Normal.HasValue)
                {
                    return GetPositionNormalVariant(v, preferTextured);
                }
                else
                {
                    return GetPositionOnlyVariant(v, preferTextured);
                }
            }
            else
            {
                return VertexTypes.Unknown;
            }
        }
        /// <summary>
        /// Gets the vertex type based on vertex data (Position with Normal)
        /// </summary>
        /// <param name="v">Vertex</param>
        /// <param name="preferTextured">Sets whether textured formats were prefered over vertex colored formats</param>
        /// <returns>Returns the vertex type</returns>
        public static VertexTypes GetPositionOnlyVariant(VertexData v, bool preferTextured)
        {
            if (!preferTextured && v.Color.HasValue)
            {
                return VertexTypes.PositionColor;
            }
            else if (preferTextured && v.Texture.HasValue)
            {
                return VertexTypes.PositionTexture;
            }
            else
            {
                return VertexTypes.PositionColor;
            }
        }
        /// <summary>
        /// Gets the vertex type based on vertex data (Position without Normal)
        /// </summary>
        /// <param name="v">Vertex</param>
        /// <param name="preferTextured">Sets whether textured formats were prefered over vertex colored formats</param>
        /// <returns>Returns the vertex type</returns>
        public static VertexTypes GetPositionNormalVariant(VertexData v, bool preferTextured)
        {
            if (!preferTextured && v.Color.HasValue)
            {
                return VertexTypes.PositionNormalColor;
            }
            else if (preferTextured && v.Texture.HasValue)
            {
                if (v.Tangent.HasValue)
                {
                    return VertexTypes.PositionNormalTextureTangent;
                }
                else
                {
                    return VertexTypes.PositionNormalTexture;
                }
            }
            else
            {
                return VertexTypes.PositionNormalColor;
            }
        }
        /// <summary>
        /// Gets skinned equivalent for specified non skinning type
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <returns>Returns skinned equivalent for specified non skinning type</returns>
        public static VertexTypes GetSkinnedEquivalent(VertexTypes vertexType)
        {
            if (vertexType == VertexTypes.Position) return VertexTypes.PositionSkinned;
            if (vertexType == VertexTypes.PositionColor) return VertexTypes.PositionColorSkinned;
            if (vertexType == VertexTypes.PositionNormalColor) return VertexTypes.PositionNormalColorSkinned;
            if (vertexType == VertexTypes.PositionTexture) return VertexTypes.PositionTextureSkinned;
            if (vertexType == VertexTypes.PositionNormalTexture) return VertexTypes.PositionNormalTextureSkinned;
            if (vertexType == VertexTypes.PositionNormalTextureTangent) return VertexTypes.PositionNormalTextureTangentSkinned;

            return VertexTypes.Unknown;
        }
        /// <summary>
        /// Gets tangent equivalent for specified non tangent type
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <returns>Returns tangent equivalent for specified non tangent type</returns>
        public static VertexTypes GetTangentEquivalent(VertexTypes vertexType)
        {
            if (vertexType == VertexTypes.PositionNormalTexture) return VertexTypes.PositionNormalTextureTangent;
            if (vertexType == VertexTypes.PositionNormalTextureSkinned) return VertexTypes.PositionNormalTextureTangentSkinned;

            return VertexTypes.Unknown;
        }

        /// <summary>
        /// Creates a mesh information structure from a submesh content
        /// </summary>
        /// <param name="meshName">Mesh name</param>
        /// <param name="geometry">Submesh content</param>
        /// <param name="loadNormalMaps">Conaints normal map information</param>
        /// <param name="material">Material</param>
        /// <param name="skinningInfo">Skinning information</param>
        /// <param name="constraint">Geometry constraint</param>
        public static async Task<MeshInfo?> CreateMesh(string meshName, SubMeshContent geometry, bool loadNormalMaps, IMaterialContent material, SkinningInfo? skinningInfo, BoundingBox? constraint)
        {
            var vertexType = GetVertexType(geometry, skinningInfo.HasValue, loadNormalMaps, material);

            //Process the vertex data
            bool computeTangents = IsTangent(vertexType);
            var vertexData = await geometry.ProcessVertexData(computeTangents, constraint);
            var vertices = vertexData.vertices;
            var indices = vertexData.indices;
            IEnumerable<Weight> weights = [];
            IEnumerable<string> bones = [];

            if (skinningInfo.HasValue)
            {
                var bindShapeMatrix = skinningInfo.Value.BindShapeMatrix;
                if (!bindShapeMatrix.IsIdentity)
                {
                    vertices = VertexData.Transform(vertices, bindShapeMatrix);
                }

                weights = skinningInfo.Value.Weights;
                bones = skinningInfo.Value.BoneNames;
            }

            var nMesh = await CreateMesh(
                vertexType,
                meshName,
                geometry,
                vertices,
                weights,
                bones,
                indices);

            //Material name
            string materialName = string.IsNullOrEmpty(geometry.Material) ? ContentData.NoMaterial : geometry.Material;

            return new MeshInfo()
            {
                Mesh = nMesh,
                MaterialName = materialName,
            };
        }
        /// <summary>
        /// Get vertex type from geometry
        /// </summary>
        /// <param name="geometry">Geometry</param>
        /// <param name="isSkinned">Load skining data</param>
        /// <param name="loadNormalMaps">Load normal maps flag</param>
        /// <param name="material">Material content</param>
        /// <returns>Returns the vertex type</returns>
        private static VertexTypes GetVertexType(SubMeshContent geometry, bool isSkinned, bool loadNormalMaps, IMaterialContent material)
        {
            var res = geometry.VertexType;
            if (isSkinned)
            {
                //Get skinned equivalent
                res = GetSkinnedEquivalent(res);
            }

            if (!loadNormalMaps)
            {
                return res;
            }

            if (IsTextured(res) && !IsTangent(res) && material?.NormalMapTexture != null)
            {
                //Get tangent equivalent
                res = GetTangentEquivalent(res);
            }

            return res;
        }
        /// <summary>
        /// Creates a mesh from the specified vertex type
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="meshName">Mesh name</param>
        /// <param name="geometry">Submesh conente</param>
        /// <param name="vertices">Vertex data</param>
        /// <param name="weights">Weights</param>
        /// <param name="skinBoneNames">Skin bone names</param>
        /// <param name="indices">Indices</param>
        /// <returns>Returns the generated mesh</returns>
        private static async Task<IMesh> CreateMesh(VertexTypes vertexType, string meshName, SubMeshContent geometry, IEnumerable<VertexData> vertices, IEnumerable<Weight> weights, IEnumerable<string> skinBoneNames, IEnumerable<uint> indices)
        {
            return vertexType switch
            {
                VertexTypes.Position => CreateMesh(meshName, geometry.Topology, geometry.Transform, await VertexPosition.Convert(vertices), indices),
                VertexTypes.PositionColor => CreateMesh(meshName, geometry.Topology, geometry.Transform, await VertexPositionColor.Convert(vertices), indices),
                VertexTypes.PositionNormalColor => CreateMesh(meshName, geometry.Topology, geometry.Transform, await VertexPositionNormalColor.Convert(vertices), indices),
                VertexTypes.PositionTexture => CreateMesh(meshName, geometry.Topology, geometry.Transform, await VertexPositionTexture.Convert(vertices), indices),
                VertexTypes.PositionNormalTexture => CreateMesh(meshName, geometry.Topology, geometry.Transform, await VertexPositionNormalTexture.Convert(vertices), indices),
                VertexTypes.PositionNormalTextureTangent => CreateMesh(meshName, geometry.Topology, geometry.Transform, await VertexPositionNormalTextureTangent.Convert(vertices), indices),
                VertexTypes.PositionSkinned => CreateMesh(meshName, geometry.Topology, geometry.Transform, await VertexSkinnedPosition.Convert(vertices, weights, skinBoneNames), indices),
                VertexTypes.PositionColorSkinned => CreateMesh(meshName, geometry.Topology, geometry.Transform, await VertexSkinnedPositionColor.Convert(vertices, weights, skinBoneNames), indices),
                VertexTypes.PositionNormalColorSkinned => CreateMesh(meshName, geometry.Topology, geometry.Transform, await VertexSkinnedPositionNormalColor.Convert(vertices, weights, skinBoneNames), indices),
                VertexTypes.PositionTextureSkinned => CreateMesh(meshName, geometry.Topology, geometry.Transform, await VertexSkinnedPositionTexture.Convert(vertices, weights, skinBoneNames), indices),
                VertexTypes.PositionNormalTextureSkinned => CreateMesh(meshName, geometry.Topology, geometry.Transform, await VertexSkinnedPositionNormalTexture.Convert(vertices, weights, skinBoneNames), indices),
                VertexTypes.PositionNormalTextureTangentSkinned => CreateMesh(meshName, geometry.Topology, geometry.Transform, await VertexSkinnedPositionNormalTextureTangent.Convert(vertices, weights, skinBoneNames), indices),
                VertexTypes.Terrain => CreateMesh(meshName, geometry.Topology, geometry.Transform, await VertexTerrain.Convert(vertices), indices),
                _ => throw new EngineException($"Unknown vertex type: {vertexType}")
            };
        }
        /// <summary>
        /// Creates a typed mesh
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="meshName">Mesh name</param>
        /// <param name="topology">Topology</param>
        /// <param name="transform">Transform</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        private static Mesh<T> CreateMesh<T>(string meshName, Topology topology, Matrix transform, IEnumerable<T> vertices, IEnumerable<uint> indices)
            where T : struct, IVertexData
        {
            return new Mesh<T>(
                meshName,
                topology,
                transform,
                vertices,
                indices);
        }
    }
}
