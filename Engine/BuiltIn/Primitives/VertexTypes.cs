using Engine.Common;
using Engine.Content;
using SharpDX;
using System.Collections.Generic;
using System.Linq;
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
        /// Converts helpers to vertices
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="vertices">Helpers</param>
        /// <param name="weights">Weight information</param>
        /// <returns>Returns generated vertices</returns>
        public static async Task<IEnumerable<IVertexData>> Convert(VertexTypes vertexType, IEnumerable<VertexData> vertices, IEnumerable<Weight> weights, IEnumerable<string> skinBoneNames)
        {
            return vertexType switch
            {
                VertexTypes.Position => await VertexPosition.Convert(vertices),
                VertexTypes.PositionColor => await VertexPositionColor.Convert(vertices),
                VertexTypes.PositionNormalColor => await VertexPositionNormalColor.Convert(vertices),
                VertexTypes.PositionTexture => await VertexPositionTexture.Convert(vertices),
                VertexTypes.PositionNormalTexture => await VertexPositionNormalTexture.Convert(vertices),
                VertexTypes.PositionNormalTextureTangent => await VertexPositionNormalTextureTangent.Convert(vertices),
                VertexTypes.PositionSkinned => await VertexSkinnedPosition.Convert(vertices, weights, skinBoneNames),
                VertexTypes.PositionColorSkinned => await VertexSkinnedPositionColor.Convert(vertices, weights, skinBoneNames),
                VertexTypes.PositionNormalColorSkinned => await VertexSkinnedPositionNormalColor.Convert(vertices, weights, skinBoneNames),
                VertexTypes.PositionTextureSkinned => await VertexSkinnedPositionTexture.Convert(vertices, weights, skinBoneNames),
                VertexTypes.PositionNormalTextureSkinned => await VertexSkinnedPositionNormalTexture.Convert(vertices, weights, skinBoneNames),
                VertexTypes.PositionNormalTextureTangentSkinned => await VertexSkinnedPositionNormalTextureTangent.Convert(vertices, weights, skinBoneNames),
                VertexTypes.Terrain => await VertexTerrain.Convert(vertices),
                _ => throw new EngineException($"Unknown vertex type: {vertexType}")
            };
        }

        /// <summary>
        /// Creates a mesh
        /// </summary>
        /// <param name="meshName">Mesh name</param>
        /// <param name="geometry">Submesh content</param>
        /// <param name="vertexType">Vertext type</param>
        /// <param name="constraint">Geometry constraint</param>
        /// <param name="skinningInfo">Skinning information</param>
        public static async Task<MeshInfo?> CreateMesh(string meshName, SubMeshContent geometry, bool isSkinned, bool loadNormalMaps, IMaterialContent material, BoundingBox? constraint, SkinningInfo? skinningInfo)
        {
            var vertexType = GetVertexType(geometry, isSkinned, loadNormalMaps, material);

            //Process the vertex data
            var vertexData = await geometry.ProcessVertexData(vertexType, constraint);
            var vertices = vertexData.vertices;
            var indices = vertexData.indices;

            IEnumerable<IVertexData> vertexList;
            if (skinningInfo.HasValue)
            {
                if (!skinningInfo.Value.BindShapeMatrix.IsIdentity)
                {
                    vertices = VertexData.Transform(vertices, skinningInfo.Value.BindShapeMatrix);
                }

                //Convert the vertex data to final mesh data
                vertexList = await Convert(
                    vertexType,
                    vertices,
                    skinningInfo.Value.Weights,
                    skinningInfo.Value.BoneNames);
            }
            else
            {
                vertexList = await Convert(
                    vertexType,
                    vertices,
                    [],
                    []);
            }

            if (!vertexList.Any())
            {
                return null;
            }

            //Create the mesh
            var nMesh = new Mesh(
                meshName,
                geometry.Topology,
                geometry.Transform,
                vertexList,
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
    }
}
