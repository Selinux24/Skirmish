using System;
using System.Collections.Generic;
using SharpDX;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;

namespace Engine.Common
{
    using Engine.Helpers;

    /// <summary>
    /// Vertex helper
    /// </summary>
    [Serializable]
    public struct VertexData
    {
        /// <summary>
        /// Face index
        /// </summary>
        public int FaceIndex;
        /// <summary>
        /// Vertex index
        /// </summary>
        public int VertexIndex;
        /// <summary>
        /// Position
        /// </summary>
        public Vector3? Position;
        /// <summary>
        /// Normal
        /// </summary>
        public Vector3? Normal;
        /// <summary>
        /// Tangent
        /// </summary>
        public Vector3? Tangent;
        /// <summary>
        /// Binormal
        /// </summary>
        public Vector3? BiNormal;
        /// <summary>
        /// Texture UV
        /// </summary>
        public Vector2? Texture;
        /// <summary>
        /// Color
        /// </summary>
        public Color4? Color;
        /// <summary>
        /// Sprite size
        /// </summary>
        public Vector2? Size;
        /// <summary>
        /// Vertex weights
        /// </summary>
        public float[] Weights;
        /// <summary>
        /// Bone weights
        /// </summary>
        public byte[] BoneIndices;

        /// <summary>
        /// Generates vertex helper from components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="size">Sprite size</param>
        /// <returns>Returns helper</returns>
        public static VertexData CreateVertexBillboard(Vector3 position, Vector2 size)
        {
            return new VertexData()
            {
                Position = position,
                Size = size,
            };
        }
        /// <summary>
        /// Generates vertex helper from components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="size">Sprite size</param>
        /// <returns>Returns helper</returns>
        public static VertexData CreateVertexParticle(Vector3 position, Vector2 size)
        {
            return new VertexData
            {
                Position = position,
                Size = size,
            };
        }
        /// <summary>
        /// Generates vertex helper from components
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns helper</returns>
        public static VertexData CreateVertexPosition(Vector3 position)
        {
            return new VertexData()
            {
                Position = position,
            };
        }
        /// <summary>
        /// Generates vertex helper from components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="color">Color</param>
        /// <returns>Returns helper</returns>
        public static VertexData CreateVertexPositionColor(Vector3 position, Color4 color)
        {
            return new VertexData()
            {
                Position = position,
                Color = color,
            };
        }
        /// <summary>
        /// Generates vertex helper from components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="texture">Texture</param>
        /// <returns>Returns helper</returns>
        public static VertexData CreateVertexPositionTexture(Vector3 position, Vector2 texture)
        {
            return new VertexData()
            {
                Position = position,
                Texture = texture,
            };
        }
        /// <summary>
        /// Generates vertex helper from components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="normal">Normal</param>
        /// <param name="color">Color</param>
        /// <returns>Returns helper</returns>
        public static VertexData CreateVertexPositionNormalColor(Vector3 position, Vector3 normal, Color4 color)
        {
            return new VertexData()
            {
                Position = position,
                Normal = normal,
                Color = color,
            };
        }
        /// <summary>
        /// Generates vertex helper from components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="normal">Normal</param>
        /// <param name="texture">Texture</param>
        /// <returns>Returns helper</returns>
        public static VertexData CreateVertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 texture)
        {
            return new VertexData()
            {
                Position = position,
                Normal = normal,
                Texture = texture,
            };
        }
        /// <summary>
        /// Generates vertex helper from components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="normal">Normal</param>
        /// <param name="tangent">Tangent</param>
        /// <param name="binormal">Binormal</param>
        /// <param name="texture">Texture</param>
        /// <returns>Returns helper</returns>
        public static VertexData CreateVertexPositionNormalTextureTangent(Vector3 position, Vector3 normal, Vector3 tangent, Vector3 binormal, Vector2 texture)
        {
            return new VertexData()
            {
                Position = position,
                Normal = normal,
                Tangent = tangent,
                BiNormal = binormal,
                Texture = texture,
            };
        }
        /// <summary>
        /// Generates vertex helper from components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="weights">Weights</param>
        /// <param name="boneIndices">Bone indices</param>
        /// <returns>Returns helper</returns>
        public static VertexData CreateVertexSkinnedPosition(Vector3 position, float[] weights, byte[] boneIndices)
        {
            return new VertexData()
            {
                Position = position,
                Weights = weights,
                BoneIndices = boneIndices,
            };
        }
        /// <summary>
        /// Generates vertex helper from components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="color">Color</param>
        /// <param name="weights">Weights</param>
        /// <param name="boneIndices">Bone indices</param>
        /// <returns>Returns helper</returns>
        public static VertexData CreateVertexSkinnedPositionColor(Vector3 position, Color4 color, float[] weights, byte[] boneIndices)
        {
            return new VertexData()
            {
                Position = position,
                Color = color,
                Weights = weights,
                BoneIndices = boneIndices,
            };
        }
        /// <summary>
        /// Generates vertex helper from components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="texture">Texture</param>
        /// <param name="weights">Weights</param>
        /// <param name="boneIndices">Bone indices</param>
        /// <returns>Returns helper</returns>
        public static VertexData CreateVertexSkinnedPositionTexture(Vector3 position, Vector2 texture, float[] weights, byte[] boneIndices)
        {
            return new VertexData()
            {
                Position = position,
                Texture = texture,
                Weights = weights,
                BoneIndices = boneIndices,
            };
        }
        /// <summary>
        /// Generates vertex helper from components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="normal">Normal</param>
        /// <param name="color">Color</param>
        /// <param name="weights">Weights</param>
        /// <param name="boneIndices">Bone indices</param>
        /// <returns>Returns helper</returns>
        public static VertexData CreateVertexSkinnedPositionNormalColor(Vector3 position, Vector3 normal, Color4 color, float[] weights, byte[] boneIndices)
        {
            return new VertexData()
            {
                Position = position,
                Normal = normal,
                Color = color,
                Weights = weights,
                BoneIndices = boneIndices,
            };
        }
        /// <summary>
        /// Generates vertex helper from components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="normal">Normal</param>
        /// <param name="texture">Texture</param>
        /// <param name="weights">Weights</param>
        /// <param name="boneIndices">Bone indices</param>
        /// <returns>Returns helper</returns>
        public static VertexData CreateVertexSkinnedPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 texture, float[] weights, byte[] boneIndices)
        {
            return new VertexData()
            {
                Position = position,
                Normal = normal,
                Texture = texture,
                Weights = weights,
                BoneIndices = boneIndices,
            };
        }
        /// <summary>
        /// Generates vertex helper from components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="normal">Normal</param>
        /// <param name="tangent">Tangent</param>
        /// <param name="binormal">Binormal</param>
        /// <param name="texture">Texture</param>
        /// <param name="weights">Weights</param>
        /// <param name="boneIndices">Bone indices</param>
        /// <returns>Returns helper</returns>
        public static VertexData CreateVertexSkinnedPositionNormalTextureTangent(Vector3 position, Vector3 normal, Vector3 tangent, Vector3 binormal, Vector2 texture, float[] weights, byte[] boneIndices)
        {
            return new VertexData()
            {
                Position = position,
                Normal = normal,
                Tangent = tangent,
                BiNormal = binormal,
                Texture = texture,
                Weights = weights,
                BoneIndices = boneIndices,
            };
        }

        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexBillboard CreateVertexBillboard(VertexData v)
        {
            return new VertexBillboard
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Size = v.Size.HasValue ? v.Size.Value : Vector2.One,
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexParticle CreateVertexParticle(VertexData v)
        {
            return new VertexParticle
            {
                Type = 0,
                Age = 0,
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Velocity = Vector3.Zero,
                Size = v.Size.HasValue ? v.Size.Value : Vector2.One,
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexPosition CreateVertexPosition(VertexData v)
        {
            return new VertexPosition
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexPositionColor CreateVertexPositionColor(VertexData v)
        {
            return new VertexPositionColor
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Color = v.Color.HasValue ? v.Color.Value : Color4.Black,
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexPositionNormalColor CreateVertexPositionNormalColor(VertexData v)
        {
            return new VertexPositionNormalColor
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Normal = v.Normal.HasValue ? v.Normal.Value : Vector3.Zero,
                Color = v.Color.HasValue ? v.Color.Value : Color4.Black,
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexPositionNormalTexture CreateVertexPositionNormalTexture(VertexData v)
        {
            return new VertexPositionNormalTexture
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Normal = v.Normal.HasValue ? v.Normal.Value : Vector3.Zero,
                Texture = v.Texture.HasValue ? v.Texture.Value : Vector2.Zero
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexPositionNormalTextureTangent CreateVertexPositionNormalTextureTangent(VertexData v)
        {
            return new VertexPositionNormalTextureTangent
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Normal = v.Normal.HasValue ? v.Normal.Value : Vector3.Zero,
                Texture = v.Texture.HasValue ? v.Texture.Value : Vector2.Zero,
                Tangent = v.Tangent.HasValue ? v.Tangent.Value : Vector3.Zero,
                BiNormal = v.BiNormal.HasValue ? v.BiNormal.Value : Vector3.Zero,
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexPositionTexture CreateVertexPositionTexture(VertexData v)
        {
            return new VertexPositionTexture
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Texture = v.Texture.HasValue ? v.Texture.Value : Vector2.Zero
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPosition CreateVertexSkinnedPosition(VertexData v, Weight[] vw)
        {
            return new VertexSkinnedPosition
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Weight1 = ((vw != null) && (vw.Length > 0)) ? vw[0].WeightValue : 0f,
                Weight2 = ((vw != null) && (vw.Length > 1)) ? vw[1].WeightValue : 0f,
                Weight3 = ((vw != null) && (vw.Length > 2)) ? vw[2].WeightValue : 0f,
                BoneIndex1 = ((vw != null) && (vw.Length > 0)) ? ((byte)vw[0].BoneIndex) : ((byte)0),
                BoneIndex2 = ((vw != null) && (vw.Length > 1)) ? ((byte)vw[1].BoneIndex) : ((byte)0),
                BoneIndex3 = ((vw != null) && (vw.Length > 2)) ? ((byte)vw[2].BoneIndex) : ((byte)0),
                BoneIndex4 = ((vw != null) && (vw.Length > 3)) ? ((byte)vw[3].BoneIndex) : ((byte)0)
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPositionColor CreateVertexSkinnedPositionColor(VertexData v, Weight[] vw)
        {
            return new VertexSkinnedPositionColor
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Color = v.Color.HasValue ? v.Color.Value : Color4.Black,
                Weight1 = ((vw != null) && (vw.Length > 0)) ? vw[0].WeightValue : 0f,
                Weight2 = ((vw != null) && (vw.Length > 1)) ? vw[1].WeightValue : 0f,
                Weight3 = ((vw != null) && (vw.Length > 2)) ? vw[2].WeightValue : 0f,
                BoneIndex1 = ((vw != null) && (vw.Length > 0)) ? ((byte)vw[0].BoneIndex) : ((byte)0),
                BoneIndex2 = ((vw != null) && (vw.Length > 1)) ? ((byte)vw[1].BoneIndex) : ((byte)0),
                BoneIndex3 = ((vw != null) && (vw.Length > 2)) ? ((byte)vw[2].BoneIndex) : ((byte)0),
                BoneIndex4 = ((vw != null) && (vw.Length > 3)) ? ((byte)vw[3].BoneIndex) : ((byte)0)
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPositionNormalColor CreateVertexSkinnedPositionNormalColor(VertexData v, Weight[] vw)
        {
            return new VertexSkinnedPositionNormalColor
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Normal = v.Normal.HasValue ? v.Normal.Value : Vector3.Zero,
                Color = v.Color.HasValue ? v.Color.Value : Color4.Black,
                Weight1 = ((vw != null) && (vw.Length > 0)) ? vw[0].WeightValue : 0f,
                Weight2 = ((vw != null) && (vw.Length > 1)) ? vw[1].WeightValue : 0f,
                Weight3 = ((vw != null) && (vw.Length > 2)) ? vw[2].WeightValue : 0f,
                BoneIndex1 = ((vw != null) && (vw.Length > 0)) ? ((byte)vw[0].BoneIndex) : ((byte)0),
                BoneIndex2 = ((vw != null) && (vw.Length > 1)) ? ((byte)vw[1].BoneIndex) : ((byte)0),
                BoneIndex3 = ((vw != null) && (vw.Length > 2)) ? ((byte)vw[2].BoneIndex) : ((byte)0),
                BoneIndex4 = ((vw != null) && (vw.Length > 3)) ? ((byte)vw[3].BoneIndex) : ((byte)0)
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPositionNormalTexture CreateVertexSkinnedPositionNormalTexture(VertexData v, Weight[] vw)
        {
            return new VertexSkinnedPositionNormalTexture
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Normal = v.Normal.HasValue ? v.Normal.Value : Vector3.Zero,
                Texture = v.Texture.HasValue ? v.Texture.Value : Vector2.Zero,
                Weight1 = ((vw != null) && (vw.Length > 0)) ? vw[0].WeightValue : 0f,
                Weight2 = ((vw != null) && (vw.Length > 1)) ? vw[1].WeightValue : 0f,
                Weight3 = ((vw != null) && (vw.Length > 2)) ? vw[2].WeightValue : 0f,
                BoneIndex1 = ((vw != null) && (vw.Length > 0)) ? ((byte)vw[0].BoneIndex) : ((byte)0),
                BoneIndex2 = ((vw != null) && (vw.Length > 1)) ? ((byte)vw[1].BoneIndex) : ((byte)0),
                BoneIndex3 = ((vw != null) && (vw.Length > 2)) ? ((byte)vw[2].BoneIndex) : ((byte)0),
                BoneIndex4 = ((vw != null) && (vw.Length > 3)) ? ((byte)vw[3].BoneIndex) : ((byte)0)
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPositionNormalTextureTangent CreateVertexSkinnedPositionNormalTextureTangent(VertexData v, Weight[] vw)
        {
            return new VertexSkinnedPositionNormalTextureTangent
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Normal = v.Normal.HasValue ? v.Normal.Value : Vector3.Zero,
                Texture = v.Texture.HasValue ? v.Texture.Value : Vector2.Zero,
                Tangent = v.Tangent.HasValue ? v.Tangent.Value : Vector3.Zero,
                BiNormal = v.BiNormal.HasValue ? v.BiNormal.Value : Vector3.Zero,
                Weight1 = ((vw != null) && (vw.Length > 0)) ? vw[0].WeightValue : 0f,
                Weight2 = ((vw != null) && (vw.Length > 1)) ? vw[1].WeightValue : 0f,
                Weight3 = ((vw != null) && (vw.Length > 2)) ? vw[2].WeightValue : 0f,
                BoneIndex1 = ((vw != null) && (vw.Length > 0)) ? ((byte)vw[0].BoneIndex) : ((byte)0),
                BoneIndex2 = ((vw != null) && (vw.Length > 1)) ? ((byte)vw[1].BoneIndex) : ((byte)0),
                BoneIndex3 = ((vw != null) && (vw.Length > 2)) ? ((byte)vw[2].BoneIndex) : ((byte)0),
                BoneIndex4 = ((vw != null) && (vw.Length > 3)) ? ((byte)vw[3].BoneIndex) : ((byte)0)
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPositionTexture CreateVertexSkinnedPositionTexture(VertexData v, Weight[] vw)
        {
            return new VertexSkinnedPositionTexture
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Texture = v.Texture.HasValue ? v.Texture.Value : Vector2.Zero,
                Weight1 = ((vw != null) && (vw.Length > 0)) ? vw[0].WeightValue : 0f,
                Weight2 = ((vw != null) && (vw.Length > 1)) ? vw[1].WeightValue : 0f,
                Weight3 = ((vw != null) && (vw.Length > 2)) ? vw[2].WeightValue : 0f,
                BoneIndex1 = ((vw != null) && (vw.Length > 0)) ? ((byte)vw[0].BoneIndex) : ((byte)0),
                BoneIndex2 = ((vw != null) && (vw.Length > 1)) ? ((byte)vw[1].BoneIndex) : ((byte)0),
                BoneIndex3 = ((vw != null) && (vw.Length > 2)) ? ((byte)vw[2].BoneIndex) : ((byte)0),
                BoneIndex4 = ((vw != null) && (vw.Length > 3)) ? ((byte)vw[3].BoneIndex) : ((byte)0)
            };
        }

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
        /// Create vertex buffer from vertices
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        /// <returns>Returns new buffer</returns>
        public static Buffer CreateVertexBuffer(Device device, IVertexData[] vertices, bool dynamic)
        {
            Buffer buffer = null;

            if (vertices != null && vertices.Length > 0)
            {
                if (vertices[0].VertexType == VertexTypes.Billboard)
                {
                    buffer = CreateVertexBuffer<VertexBillboard>(device, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.Particle)
                {
                    buffer = CreateVertexBuffer<VertexParticle>(device, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.Position)
                {
                    buffer = CreateVertexBuffer<VertexPosition>(device, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionColor)
                {
                    buffer = CreateVertexBuffer<VertexPositionColor>(device, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalColor)
                {
                    buffer = CreateVertexBuffer<VertexPositionNormalColor>(device, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionTexture)
                {
                    buffer = CreateVertexBuffer<VertexPositionTexture>(device, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalTexture)
                {
                    buffer = CreateVertexBuffer<VertexPositionNormalTexture>(device, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureTangent)
                {
                    buffer = CreateVertexBuffer<VertexPositionNormalTextureTangent>(device, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPosition>(device, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionColorSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPositionColor>(device, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalColorSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPositionNormalColor>(device, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionTextureSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPositionTexture>(device, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPositionNormalTexture>(device, vertices, dynamic);
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureTangentSkinned)
                {
                    buffer = CreateVertexBuffer<VertexSkinnedPositionNormalTextureTangent>(device, vertices, dynamic);
                }
                else
                {
                    throw new Exception(string.Format("Unknown vertex type: {0}", vertices[0].VertexType));
                }
            }

            return buffer;
        }
        /// <summary>
        /// Creates a vertex buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="device">Device</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns new buffer</returns>
        public static Buffer CreateVertexBuffer<T>(Device device, IVertexData[] vertices, bool dynamic) where T : struct, IVertexData
        {
            T[] data = Array.ConvertAll((IVertexData[])vertices, v => (T)v);

            if (dynamic)
            {
                return device.CreateVertexBufferWrite(data);
            }
            else
            {
                return device.CreateVertexBufferImmutable(data);
            }
        }
        /// <summary>
        /// Writes vertex buffer data
        /// </summary>
        /// <param name="deviceContext">Graphics context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="vertices">Vertices</param>
        public static void WriteVertexBuffer(DeviceContext deviceContext, Buffer buffer, IVertexData[] vertices)
        {
            if (vertices[0].VertexType == VertexTypes.Billboard)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexBillboard)v));
            }
            else if (vertices[0].VertexType == VertexTypes.Position)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexPosition)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionColor)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexPositionColor)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalColor)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexPositionNormalColor)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionTexture)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexPositionTexture)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalTexture)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexPositionNormalTexture)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureTangent)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexPositionNormalTextureTangent)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionSkinned)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPosition)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionColorSkinned)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPositionColor)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalColorSkinned)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPositionNormalColor)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionTextureSkinned)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPositionTexture)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureSkinned)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPositionNormalTexture)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureTangentSkinned)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPositionNormalTextureTangent)v));
            }
            else
            {
                throw new Exception(string.Format("Unknown vertex type: {0}", vertices[0].VertexType));
            }
        }
        /// <summary>
        /// Converts helpers to vertices
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="vertices">Helpers</param>
        /// <param name="weights">Weight information</param>
        /// <returns>Returns generated vertices</returns>
        public static IVertexData[] Convert(VertexTypes vertexType, VertexData[] vertices, Weight[] weights)
        {
            List<IVertexData> vertexList = new List<IVertexData>();

            if (vertexType == VertexTypes.Billboard)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexData.CreateVertexBillboard(v)); });
            }
            else if (vertexType == VertexTypes.Position)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexData.CreateVertexPosition(v)); });
            }
            else if (vertexType == VertexTypes.PositionColor)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexData.CreateVertexPositionColor(v)); });
            }
            else if (vertexType == VertexTypes.PositionNormalColor)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexData.CreateVertexPositionNormalColor(v)); });
            }
            else if (vertexType == VertexTypes.PositionTexture)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexData.CreateVertexPositionTexture(v)); });
            }
            else if (vertexType == VertexTypes.PositionNormalTexture)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexData.CreateVertexPositionNormalTexture(v)); });
            }
            else if (vertexType == VertexTypes.PositionNormalTextureTangent)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexData.CreateVertexPositionNormalTextureTangent(v)); });
            }

            else if (vertexType == VertexTypes.PositionSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexData.CreateVertexSkinnedPosition(v, vw));
                });
            }
            else if (vertexType == VertexTypes.PositionColorSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexData.CreateVertexSkinnedPositionColor(v, vw));
                });
            }
            else if (vertexType == VertexTypes.PositionNormalColorSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexData.CreateVertexSkinnedPositionNormalColor(v, vw));
                });
            }
            else if (vertexType == VertexTypes.PositionTextureSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexData.CreateVertexSkinnedPositionTexture(v, vw));
                });
            }
            else if (vertexType == VertexTypes.PositionNormalTextureSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexData.CreateVertexSkinnedPositionNormalTexture(v, vw));
                });
            }
            else if (vertexType == VertexTypes.PositionNormalTextureTangentSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexData.CreateVertexSkinnedPositionNormalTextureTangent(v, vw));
                });
            }

            else
            {
                throw new Exception(string.Format("Unknown vertex type: {0}", vertexType));
            }

            return vertexList.ToArray();
        }

        /// <summary>
        /// Calculate tangent, normal and binormals of vertices
        /// </summary>
        /// <param name="vertex1">Vertex 1</param>
        /// <param name="vertex2">Vertex 2</param>
        /// <param name="vertex3">Vertex 3</param>
        /// <param name="tangent">Tangen result</param>
        /// <param name="binormal">Binormal result</param>
        /// <param name="normal">Normal result</param>
        public static void CalculateNormals(VertexData vertex1, VertexData vertex2, VertexData vertex3, out Vector3 tangent, out Vector3 binormal, out Vector3 normal)
        {
            // Calculate the two vectors for the face.
            Vector3 vector1 = vertex2.Position.Value - vertex1.Position.Value;
            Vector3 vector2 = vertex3.Position.Value - vertex1.Position.Value;

            // Calculate the tu and tv texture space vectors.
            Vector2 tuVector = new Vector2(
                vertex2.Texture.Value.X - vertex1.Texture.Value.X,
                vertex3.Texture.Value.X - vertex1.Texture.Value.X);
            Vector2 tvVector = new Vector2(
                vertex2.Texture.Value.Y - vertex1.Texture.Value.Y,
                vertex3.Texture.Value.Y - vertex1.Texture.Value.Y);

            // Calculate the denominator of the tangent / binormal equation.
            var den = 1.0f / (tuVector[0] * tvVector[1] - tuVector[1] * tvVector[0]);

            // Calculate the cross products and multiply by the coefficient to get the tangent and binormal.
            tangent.X = (tvVector[1] * vector1.X - tvVector[0] * vector2.X) * den;
            tangent.Y = (tvVector[1] * vector1.Y - tvVector[0] * vector2.Y) * den;
            tangent.Z = (tvVector[1] * vector1.Z - tvVector[0] * vector2.Z) * den;

            binormal.X = (tuVector[0] * vector2.X - tuVector[1] * vector1.X) * den;
            binormal.Y = (tuVector[0] * vector2.Y - tuVector[1] * vector1.Y) * den;
            binormal.Z = (tuVector[0] * vector2.Z - tuVector[1] * vector1.Z) * den;

            tangent.Normalize();
            binormal.Normalize();

            // Calculate the cross product of the tangent and binormal which will give the normal vector.
            normal = Vector3.Cross(tangent, binormal);

            normal.Normalize();
        }

        /// <summary>
        /// Transforms helper by given matrix
        /// </summary>
        /// <param name="transform">Transformation matrix</param>
        public void Transform(Matrix transform)
        {
            if (!transform.IsIdentity)
            {
                if (this.Position.HasValue)
                {
                    Vector3 position = this.Position.Value;

                    Vector3.TransformCoordinate(ref position, ref transform, out position);

                    this.Position = position;
                }

                if (this.Normal.HasValue)
                {
                    Vector3 normal = this.Normal.Value;

                    Vector3.TransformNormal(ref normal, ref transform, out normal);

                    this.Normal = normal;
                }

                if (this.Tangent.HasValue)
                {
                    Vector3 tangent = this.Tangent.Value;

                    Vector3.TransformNormal(ref tangent, ref transform, out tangent);

                    this.Tangent = tangent;
                }

                if (this.BiNormal.HasValue)
                {
                    Vector3 biNormal = this.BiNormal.Value;

                    Vector3.TransformNormal(ref biNormal, ref transform, out biNormal);

                    this.BiNormal = biNormal;
                }
            }
        }

        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        public override string ToString()
        {
            string text = null;

            if (this.Weights != null && this.Weights.Length > 0) text += "Skinned; ";

            text += string.Format("FaceIndex: {0}; ", this.FaceIndex);
            text += string.Format("VertexIndex: {0}; ", this.VertexIndex);

            if (this.Position.HasValue) text += string.Format("Position: {0}; ", this.Position);
            if (this.Normal.HasValue) text += string.Format("Normal: {0}; ", this.Normal);
            if (this.Tangent.HasValue) text += string.Format("Tangent: {0}; ", this.Tangent);
            if (this.BiNormal.HasValue) text += string.Format("BiNormal: {0}; ", this.BiNormal);
            if (this.Texture.HasValue) text += string.Format("Texture: {0}; ", this.Texture);
            if (this.Color.HasValue) text += string.Format("Color: {0}; ", this.Color);
            if (this.Size.HasValue) text += string.Format("Size: {0}; ", this.Size);
            if (this.Weights != null && this.Weights.Length > 0) text += string.Format("Weights: {0}; ", this.Weights.Length);
            if (this.BoneIndices != null && this.BoneIndices.Length > 0) text += string.Format("BoneIndices: {0}; ", this.BoneIndices.Length);

            return text;
        }
    }

    /// <summary>
    /// Vertex Channels
    /// </summary>
    public enum VertexDataChannels
    {
        /// <summary>
        /// Position
        /// </summary>
        Position,
        /// <summary>
        /// Normal
        /// </summary>
        Normal,
        /// <summary>
        /// Tangent
        /// </summary>
        Tangent,
        /// <summary>
        /// Binormal
        /// </summary>
        BiNormal,
        /// <summary>
        /// Texture UV
        /// </summary>
        Texture,
        /// <summary>
        /// Color
        /// </summary>
        Color,
        /// <summary>
        /// Sprite size
        /// </summary>
        Size,
        /// <summary>
        /// Vertex weights
        /// </summary>
        Weights,
        /// <summary>
        /// Bone weights
        /// </summary>
        BoneIndices,
    }
}
