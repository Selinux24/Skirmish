using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.Common
{
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
        public Vector2? Texture0;
        /// <summary>
        /// Texture UV
        /// </summary>
        public Vector2? Texture1;
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
                Texture0 = texture,
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
                Texture0 = texture,
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
                Texture0 = texture,
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
                Texture0 = texture,
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
                Texture0 = texture,
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
                Texture0 = texture,
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
        /// <param name="color">Color</param>
        /// <returns>Returns helper</returns>
        public static VertexData CreateVertexTerrain(Vector3 position, Vector3 normal, Vector3 tangent, Vector3 binormal, Vector2 texture0, Vector2 texture1, Color4 color)
        {
            return new VertexData()
            {
                Position = position,
                Normal = normal,
                Tangent = tangent,
                BiNormal = binormal,
                Texture0 = texture0,
                Texture1 = texture1,
                Color = color,
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
        public static VertexCPUParticle CreateVertexCPUParticle(VertexData v)
        {
            return new VertexCPUParticle
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Velocity = Vector3.Zero,
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexGPUParticle CreateVertexGPUParticle(VertexData v)
        {
            return new VertexGPUParticle
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Velocity = Vector3.Zero,
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
                Color = v.Color.HasValue ? v.Color.Value : Color4.White,
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
                Color = v.Color.HasValue ? v.Color.Value : Color4.White,
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
                Texture = v.Texture0.HasValue ? v.Texture0.Value : Vector2.Zero
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
                Texture = v.Texture0.HasValue ? v.Texture0.Value : Vector2.Zero
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
                Texture = v.Texture0.HasValue ? v.Texture0.Value : Vector2.Zero,
                Tangent = v.Tangent.HasValue ? v.Tangent.Value : Vector3.UnitX,
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexTerrain CreateVertexTerrain(VertexData v)
        {
            return new VertexTerrain
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Normal = v.Normal.HasValue ? v.Normal.Value : Vector3.Zero,
                Texture0 = v.Texture0.HasValue ? v.Texture0.Value : Vector2.Zero,
                Texture1 = v.Texture1.HasValue ? v.Texture1.Value : Vector2.Zero,
                Tangent = v.Tangent.HasValue ? v.Tangent.Value : Vector3.UnitX,
                Color = v.Color.HasValue ? v.Color.Value : Color4.White,
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPosition CreateVertexSkinnedPosition(VertexData v, Weight[] vw, string[] skinBoneNames)
        {
            return new VertexSkinnedPosition
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Weight1 = ((vw != null) && (vw.Length > 0)) ? vw[0].WeightValue : 0f,
                Weight2 = ((vw != null) && (vw.Length > 1)) ? vw[1].WeightValue : 0f,
                Weight3 = ((vw != null) && (vw.Length > 2)) ? vw[2].WeightValue : 0f,
                BoneIndex1 = ((vw != null) && (vw.Length > 0)) ? ((byte)Array.IndexOf(skinBoneNames, vw[0].Joint)) : ((byte)0),
                BoneIndex2 = ((vw != null) && (vw.Length > 1)) ? ((byte)Array.IndexOf(skinBoneNames, vw[1].Joint)) : ((byte)0),
                BoneIndex3 = ((vw != null) && (vw.Length > 2)) ? ((byte)Array.IndexOf(skinBoneNames, vw[2].Joint)) : ((byte)0),
                BoneIndex4 = ((vw != null) && (vw.Length > 3)) ? ((byte)Array.IndexOf(skinBoneNames, vw[3].Joint)) : ((byte)0)
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPositionColor CreateVertexSkinnedPositionColor(VertexData v, Weight[] vw, string[] skinBoneNames)
        {
            return new VertexSkinnedPositionColor
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Color = v.Color.HasValue ? v.Color.Value : Color4.White,
                Weight1 = ((vw != null) && (vw.Length > 0)) ? vw[0].WeightValue : 0f,
                Weight2 = ((vw != null) && (vw.Length > 1)) ? vw[1].WeightValue : 0f,
                Weight3 = ((vw != null) && (vw.Length > 2)) ? vw[2].WeightValue : 0f,
                BoneIndex1 = ((vw != null) && (vw.Length > 0)) ? ((byte)Array.IndexOf(skinBoneNames, vw[0].Joint)) : ((byte)0),
                BoneIndex2 = ((vw != null) && (vw.Length > 1)) ? ((byte)Array.IndexOf(skinBoneNames, vw[1].Joint)) : ((byte)0),
                BoneIndex3 = ((vw != null) && (vw.Length > 2)) ? ((byte)Array.IndexOf(skinBoneNames, vw[2].Joint)) : ((byte)0),
                BoneIndex4 = ((vw != null) && (vw.Length > 3)) ? ((byte)Array.IndexOf(skinBoneNames, vw[3].Joint)) : ((byte)0)
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPositionNormalColor CreateVertexSkinnedPositionNormalColor(VertexData v, Weight[] vw, string[] skinBoneNames)
        {
            return new VertexSkinnedPositionNormalColor
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Normal = v.Normal.HasValue ? v.Normal.Value : Vector3.Zero,
                Color = v.Color.HasValue ? v.Color.Value : Color4.White,
                Weight1 = ((vw != null) && (vw.Length > 0)) ? vw[0].WeightValue : 0f,
                Weight2 = ((vw != null) && (vw.Length > 1)) ? vw[1].WeightValue : 0f,
                Weight3 = ((vw != null) && (vw.Length > 2)) ? vw[2].WeightValue : 0f,
                BoneIndex1 = ((vw != null) && (vw.Length > 0)) ? ((byte)Array.IndexOf(skinBoneNames, vw[0].Joint)) : ((byte)0),
                BoneIndex2 = ((vw != null) && (vw.Length > 1)) ? ((byte)Array.IndexOf(skinBoneNames, vw[1].Joint)) : ((byte)0),
                BoneIndex3 = ((vw != null) && (vw.Length > 2)) ? ((byte)Array.IndexOf(skinBoneNames, vw[2].Joint)) : ((byte)0),
                BoneIndex4 = ((vw != null) && (vw.Length > 3)) ? ((byte)Array.IndexOf(skinBoneNames, vw[3].Joint)) : ((byte)0)
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPositionTexture CreateVertexSkinnedPositionTexture(VertexData v, Weight[] vw, string[] skinBoneNames)
        {
            return new VertexSkinnedPositionTexture
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Texture = v.Texture0.HasValue ? v.Texture0.Value : Vector2.Zero,
                Weight1 = ((vw != null) && (vw.Length > 0)) ? vw[0].WeightValue : 0f,
                Weight2 = ((vw != null) && (vw.Length > 1)) ? vw[1].WeightValue : 0f,
                Weight3 = ((vw != null) && (vw.Length > 2)) ? vw[2].WeightValue : 0f,
                BoneIndex1 = ((vw != null) && (vw.Length > 0)) ? ((byte)Array.IndexOf(skinBoneNames, vw[0].Joint)) : ((byte)0),
                BoneIndex2 = ((vw != null) && (vw.Length > 1)) ? ((byte)Array.IndexOf(skinBoneNames, vw[1].Joint)) : ((byte)0),
                BoneIndex3 = ((vw != null) && (vw.Length > 2)) ? ((byte)Array.IndexOf(skinBoneNames, vw[2].Joint)) : ((byte)0),
                BoneIndex4 = ((vw != null) && (vw.Length > 3)) ? ((byte)Array.IndexOf(skinBoneNames, vw[3].Joint)) : ((byte)0)
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPositionNormalTexture CreateVertexSkinnedPositionNormalTexture(VertexData v, Weight[] vw, string[] skinBoneNames)
        {
            return new VertexSkinnedPositionNormalTexture
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Normal = v.Normal.HasValue ? v.Normal.Value : Vector3.Zero,
                Texture = v.Texture0.HasValue ? v.Texture0.Value : Vector2.Zero,
                Weight1 = ((vw != null) && (vw.Length > 0)) ? vw[0].WeightValue : 0f,
                Weight2 = ((vw != null) && (vw.Length > 1)) ? vw[1].WeightValue : 0f,
                Weight3 = ((vw != null) && (vw.Length > 2)) ? vw[2].WeightValue : 0f,
                BoneIndex1 = ((vw != null) && (vw.Length > 0)) ? (FindBoneIndex(skinBoneNames, vw[0].Joint)) : ((byte)0),
                BoneIndex2 = ((vw != null) && (vw.Length > 1)) ? (FindBoneIndex(skinBoneNames, vw[1].Joint)) : ((byte)0),
                BoneIndex3 = ((vw != null) && (vw.Length > 2)) ? (FindBoneIndex(skinBoneNames, vw[2].Joint)) : ((byte)0),
                BoneIndex4 = ((vw != null) && (vw.Length > 3)) ? (FindBoneIndex(skinBoneNames, vw[3].Joint)) : ((byte)0)
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPositionNormalTextureTangent CreateVertexSkinnedPositionNormalTextureTangent(VertexData v, Weight[] vw, string[] skinBoneNames)
        {
            return new VertexSkinnedPositionNormalTextureTangent
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Normal = v.Normal.HasValue ? v.Normal.Value : Vector3.Zero,
                Texture = v.Texture0.HasValue ? v.Texture0.Value : Vector2.Zero,
                Tangent = v.Tangent.HasValue ? v.Tangent.Value : Vector3.UnitX,
                Weight1 = ((vw != null) && (vw.Length > 0)) ? vw[0].WeightValue : 0f,
                Weight2 = ((vw != null) && (vw.Length > 1)) ? vw[1].WeightValue : 0f,
                Weight3 = ((vw != null) && (vw.Length > 2)) ? vw[2].WeightValue : 0f,
                BoneIndex1 = ((vw != null) && (vw.Length > 0)) ? ((byte)Array.IndexOf(skinBoneNames, vw[0].Joint)) : ((byte)0),
                BoneIndex2 = ((vw != null) && (vw.Length > 1)) ? ((byte)Array.IndexOf(skinBoneNames, vw[1].Joint)) : ((byte)0),
                BoneIndex3 = ((vw != null) && (vw.Length > 2)) ? ((byte)Array.IndexOf(skinBoneNames, vw[2].Joint)) : ((byte)0),
                BoneIndex4 = ((vw != null) && (vw.Length > 3)) ? ((byte)Array.IndexOf(skinBoneNames, vw[3].Joint)) : ((byte)0)
            };
        }
        /// <summary>
        /// Finds bone index by name
        /// </summary>
        /// <param name="jointNames">Bone names list</param>
        /// <param name="joint">Bone name</param>
        /// <returns>Returns the bone index or 0 if not found</returns>
        private static byte FindBoneIndex(string[] jointNames, string joint)
        {
            int index = Array.IndexOf(jointNames, joint);
            if (index >= 0)
            {
                return (byte)index;
            }

            return (byte)0;
        }

        /// <summary>
        /// Creates a line list of VertexPositionColor VertexData
        /// </summary>
        /// <param name="lines">Line list</param>
        /// <param name="color">Color</param>
        /// <param name="v">Result vertices</param>
        public static void CreateLineList(Line3D[] lines, Color4 color, out VertexData[] v)
        {
            List<VertexData> data = new List<VertexData>();

            for (int i = 0; i < lines.Length; i++)
            {
                data.Add(VertexData.CreateVertexPositionColor(lines[i].Point1, color));
                data.Add(VertexData.CreateVertexPositionColor(lines[i].Point2, color));
            }

            v = data.ToArray();
        }
        /// <summary>
        /// Creates a triangle list of VertexPositionColor VertexData
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        /// <param name="color">Color</param>
        /// <param name="v">Result vertices</param>
        public static void CreateTriangleList(Triangle[] triangles, Color4 color, out VertexData[] v)
        {
            List<VertexData> vList = new List<VertexData>();

            for (int i = 0; i < triangles.Length; i++)
            {
                vList.Add(VertexData.CreateVertexPositionColor(triangles[i].Point1, color));
                vList.Add(VertexData.CreateVertexPositionColor(triangles[i].Point2, color));
                vList.Add(VertexData.CreateVertexPositionColor(triangles[i].Point3, color));
            }

            v = vList.ToArray();
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
        /// <param name="transform">Transfor to apply to all vertices</param>
        /// <returns>Returns generated vertices</returns>
        public static IVertexData[] Convert(VertexTypes vertexType, VertexData[] vertices, Weight[] weights, string[] skinBoneNames, Matrix transform)
        {
            List<IVertexData> vertexList = new List<IVertexData>();

            if (!transform.IsIdentity)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i].Transform(transform);
                }
            }

            if (vertexType == VertexTypes.Billboard)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexData.CreateVertexBillboard(v)); });
            }
            else if (vertexType == VertexTypes.Particle)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexData.CreateVertexCPUParticle(v)); });
            }
            else if (vertexType == VertexTypes.GPUParticle)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexData.CreateVertexGPUParticle(v)); });
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
            else if (vertexType == VertexTypes.Terrain)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexData.CreateVertexTerrain(v)); });
            }
            else if (vertexType == VertexTypes.PositionSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexData.CreateVertexSkinnedPosition(v, vw, skinBoneNames));
                });
            }
            else if (vertexType == VertexTypes.PositionColorSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexData.CreateVertexSkinnedPositionColor(v, vw, skinBoneNames));
                });
            }
            else if (vertexType == VertexTypes.PositionNormalColorSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexData.CreateVertexSkinnedPositionNormalColor(v, vw, skinBoneNames));
                });
            }
            else if (vertexType == VertexTypes.PositionTextureSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexData.CreateVertexSkinnedPositionTexture(v, vw, skinBoneNames));
                });
            }
            else if (vertexType == VertexTypes.PositionNormalTextureSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexData.CreateVertexSkinnedPositionNormalTexture(v, vw, skinBoneNames));
                });
            }
            else if (vertexType == VertexTypes.PositionNormalTextureTangentSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexData.CreateVertexSkinnedPositionNormalTextureTangent(v, vw, skinBoneNames));
                });
            }

            else
            {
                throw new Exception(string.Format("Unknown vertex type: {0}", vertexType));
            }

            return vertexList.ToArray();
        }
        /// <summary>
        /// Apply weighted transforms to the vertext data
        /// </summary>
        /// <param name="vertex">Vertex data</param>
        /// <param name="boneTransforms">Bone transforms list</param>
        /// <returns>Returns the weighted position</returns>
        public static Vector3 ApplyWeight(IVertexData vertex, Matrix[] boneTransforms)
        {
            Vector3 position = vertex.HasChannel(VertexDataChannels.Position) ? vertex.GetChannelValue<Vector3>(VertexDataChannels.Position) : Vector3.Zero;
            if (VertexData.IsSkinned(vertex.VertexType))
            {
                byte[] boneIndices = vertex.HasChannel(VertexDataChannels.BoneIndices) ? vertex.GetChannelValue<byte[]>(VertexDataChannels.BoneIndices) : null;
                float[] boneWeights = vertex.HasChannel(VertexDataChannels.Weights) ? vertex.GetChannelValue<float[]>(VertexDataChannels.Weights) : null;

                Vector3 t = Vector3.Zero;

                for (int w = 0; w < boneIndices.Length; w++)
                {
                    float weight = boneWeights[w];
                    if (weight > 0)
                    {
                        byte index = boneIndices[w];
                        var boneTransform = boneTransforms[index];

                        Vector3 p;
                        Vector3.TransformCoordinate(ref position, ref boneTransform, out p);

                        t += (p * weight);
                    }
                }

                return t;
            }
            else
            {
                return position;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="v">Vertex data</param>
        public VertexData(VertexData v)
        {
            this.FaceIndex = v.FaceIndex;
            this.VertexIndex = v.VertexIndex;
            this.Position = v.Position;
            this.Normal = v.Normal;
            this.Tangent = v.Tangent;
            this.BiNormal = v.BiNormal;
            this.Texture0 = v.Texture0;
            this.Texture1 = v.Texture1;
            this.Color = v.Color;
            this.Size = v.Size;
            this.Weights = v.Weights;
            this.BoneIndices = v.BoneIndices;
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

                    Vector3 p;
                    Vector3.TransformCoordinate(ref position, ref transform, out p);

                    this.Position = p;
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
            if (this.Texture0.HasValue) text += string.Format("Texture: {0}; ", this.Texture0);
            if (this.Color.HasValue) text += string.Format("Color: {0}; ", this.Color);
            if (this.Size.HasValue) text += string.Format("Size: {0}; ", this.Size);
            if (this.Weights != null && this.Weights.Length > 0) text += string.Format("Weights: {0}; ", this.Weights.Length);
            if (this.BoneIndices != null && this.BoneIndices.Length > 0) text += string.Format("BoneIndices: {0}; ", this.BoneIndices.Length);

            return text;
        }
    }
}
