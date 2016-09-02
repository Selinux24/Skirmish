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
        /// Creates a line list of VertexPositionColor VertexData
        /// </summary>
        /// <param name="lines">Line list</param>
        /// <param name="color">Color</param>
        /// <param name="v">Result vertices</param>
        public static void CreateLineList(Line3[] lines, Color4 color, out VertexData[] v)
        {
            //TODO: Vertex generation specifying data channels

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
        /// Creates a screen of VertexPositionTexture VertexData
        /// </summary>
        /// <param name="position">Screen position</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="v">Result vertices</param>
        /// <param name="i">Result indices</param>
        public static void CreateScreen(EngineForm form, out VertexData[] v, out uint[] i)
        {
            v = new VertexData[4];
            i = new uint[6];

            float width = form.RenderWidth;
            float height = form.RenderHeight;

            float left = (float)((width / 2) * -1);
            float right = left + (float)width;
            float top = (float)(height / 2);
            float bottom = top - (float)height;

            v[0].Position = new Vector3(left, top, 0.0f);
            v[0].Texture0 = new Vector2(0.0f, 0.0f);

            v[1].Position = new Vector3(right, bottom, 0.0f);
            v[1].Texture0 = new Vector2(1.0f, 1.0f);

            v[2].Position = new Vector3(left, bottom, 0.0f);
            v[2].Texture0 = new Vector2(0.0f, 1.0f);

            v[3].Position = new Vector3(right, top, 0.0f);
            v[3].Texture0 = new Vector2(1.0f, 0.0f);

            i[0] = 0;
            i[1] = 1;
            i[2] = 2;

            i[3] = 0;
            i[4] = 3;
            i[5] = 1;
        }
        /// <summary>
        /// Creates a sprite of VertexPositionTexture VertexData
        /// </summary>
        /// <param name="position">Sprite position</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="formWidth">Render form width</param>
        /// <param name="formHeight">Render form height</param>
        /// <param name="v">Result vertices</param>
        /// <param name="i">Result indices</param>
        public static void CreateSprite(Vector2 position, float width, float height, float formWidth, float formHeight, out VertexData[] v, out uint[] i)
        {
            v = new VertexData[4];
            i = new uint[6];

            float left = (formWidth * 0.5f * -1f) + position.X;
            float right = left + width;
            float top = (formHeight * 0.5f) - position.Y;
            float bottom = top - height;

            v[0].Position = new Vector3(left, top, 0.0f);
            v[0].Normal = new Vector3(0, 0, -1);
            v[0].Texture0 = Vector2.Zero;

            v[1].Position = new Vector3(right, bottom, 0.0f);
            v[1].Normal = new Vector3(0, 0, -1);
            v[1].Texture0 = Vector2.One;

            v[2].Position = new Vector3(left, bottom, 0.0f);
            v[2].Normal = new Vector3(0, 0, -1);
            v[2].Texture0 = Vector2.UnitY;

            v[3].Position = new Vector3(right, top, 0.0f);
            v[3].Normal = new Vector3(0, 0, -1);
            v[3].Texture0 = Vector2.UnitX;

            i[0] = 0;
            i[1] = 1;
            i[2] = 2;

            i[3] = 0;
            i[4] = 3;
            i[5] = 1;
        }
        /// <summary>
        /// Creates a box of VertexPosition VertexData
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="depth">Depth</param>
        /// <param name="v">Result vertices</param>
        /// <param name="i">Result indices</param>
        public static void CreateBox(float width, float height, float depth, out VertexData[] v, out uint[] i)
        {
            v = new VertexData[24];
            i = new uint[36];

            float w2 = 0.5f * width;
            float h2 = 0.5f * height;
            float d2 = 0.5f * depth;

            // Fill in the front face vertex data.
            v[0] = VertexData.CreateVertexPosition(new Vector3(-w2, -h2, -d2));
            v[1] = VertexData.CreateVertexPosition(new Vector3(-w2, +h2, -d2));
            v[2] = VertexData.CreateVertexPosition(new Vector3(+w2, +h2, -d2));
            v[3] = VertexData.CreateVertexPosition(new Vector3(+w2, -h2, -d2));

            // Fill in the back face vertex data.
            v[4] = VertexData.CreateVertexPosition(new Vector3(-w2, -h2, +d2));
            v[5] = VertexData.CreateVertexPosition(new Vector3(+w2, -h2, +d2));
            v[6] = VertexData.CreateVertexPosition(new Vector3(+w2, +h2, +d2));
            v[7] = VertexData.CreateVertexPosition(new Vector3(-w2, +h2, +d2));

            // Fill in the top face vertex data.
            v[8] = VertexData.CreateVertexPosition(new Vector3(-w2, +h2, -d2));
            v[9] = VertexData.CreateVertexPosition(new Vector3(-w2, +h2, +d2));
            v[10] = VertexData.CreateVertexPosition(new Vector3(+w2, +h2, +d2));
            v[11] = VertexData.CreateVertexPosition(new Vector3(+w2, +h2, -d2));

            // Fill in the bottom face vertex data.
            v[12] = VertexData.CreateVertexPosition(new Vector3(-w2, -h2, -d2));
            v[13] = VertexData.CreateVertexPosition(new Vector3(+w2, -h2, -d2));
            v[14] = VertexData.CreateVertexPosition(new Vector3(+w2, -h2, +d2));
            v[15] = VertexData.CreateVertexPosition(new Vector3(-w2, -h2, +d2));

            // Fill in the left face vertex data.
            v[16] = VertexData.CreateVertexPosition(new Vector3(-w2, -h2, +d2));
            v[17] = VertexData.CreateVertexPosition(new Vector3(-w2, +h2, +d2));
            v[18] = VertexData.CreateVertexPosition(new Vector3(-w2, +h2, -d2));
            v[19] = VertexData.CreateVertexPosition(new Vector3(-w2, -h2, -d2));

            // Fill in the right face vertex data.
            v[20] = VertexData.CreateVertexPosition(new Vector3(+w2, -h2, -d2));
            v[21] = VertexData.CreateVertexPosition(new Vector3(+w2, +h2, -d2));
            v[22] = VertexData.CreateVertexPosition(new Vector3(+w2, +h2, +d2));
            v[23] = VertexData.CreateVertexPosition(new Vector3(+w2, -h2, +d2));

            // Fill in the front face index data
            i[0] = 0; i[1] = 1; i[2] = 2;
            i[3] = 0; i[4] = 2; i[5] = 3;

            // Fill in the back face index data
            i[6] = 4; i[7] = 5; i[8] = 6;
            i[9] = 4; i[10] = 6; i[11] = 7;

            // Fill in the top face index data
            i[12] = 8; i[13] = 9; i[14] = 10;
            i[15] = 8; i[16] = 10; i[17] = 11;

            // Fill in the bottom face index data
            i[18] = 12; i[19] = 13; i[20] = 14;
            i[21] = 12; i[22] = 14; i[23] = 15;

            // Fill in the left face index data
            i[24] = 16; i[25] = 17; i[26] = 18;
            i[27] = 16; i[28] = 18; i[29] = 19;

            // Fill in the right face index data
            i[30] = 20; i[31] = 21; i[32] = 22;
            i[33] = 20; i[34] = 22; i[35] = 23;
        }
        /// <summary>
        /// Creates a sphere of VertexPositionNormalTextureTangent VertexData
        /// </summary>
        /// <param name="radius">Radius</param>
        /// <param name="sliceCount">Slice count</param>
        /// <param name="stackCount">Stack count</param>
        /// <param name="v">Result vertices</param>
        /// <param name="i">Result indices</param>
        public static void CreateSphere(float radius, uint sliceCount, uint stackCount, out VertexData[] v, out uint[] i)
        {
            List<VertexData> vertList = new List<VertexData>();

            //
            // Compute the vertices stating at the top pole and moving down the stacks.
            //

            // Poles: note that there will be texture coordinate distortion as there is
            // not a unique point on the texture map to assign to the pole when mapping
            // a rectangular texture onto a sphere.

            vertList.Add(VertexData.CreateVertexPositionNormalTextureTangent(
                new Vector3(0.0f, +radius, 0.0f),
                new Vector3(0.0f, +1.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector2(0.0f, 0.0f)));

            float phiStep = MathUtil.Pi / stackCount;
            float thetaStep = 2.0f * MathUtil.Pi / sliceCount;

            // Compute vertices for each stack ring (do not count the poles as rings).
            for (int st = 1; st <= stackCount - 1; ++st)
            {
                float phi = st * phiStep;

                // Vertices of ring.
                for (int sl = 0; sl <= sliceCount; ++sl)
                {
                    float theta = sl * thetaStep;

                    Vector3 position;
                    Vector3 normal;
                    Vector3 tangent;
                    Vector3 binormal;
                    Vector2 texture;

                    // spherical to cartesian
                    position.X = radius * (float)Math.Sin(phi) * (float)Math.Cos(theta);
                    position.Y = radius * (float)Math.Cos(phi);
                    position.Z = radius * (float)Math.Sin(phi) * (float)Math.Sin(theta);

                    normal = position;
                    normal.Normalize();

                    // Partial derivative of P with respect to theta
                    tangent.X = -radius * (float)Math.Sin(phi) * (float)Math.Sin(theta);
                    tangent.Y = 0.0f;
                    tangent.Z = +radius * (float)Math.Sin(phi) * (float)Math.Cos(theta);
                    //tangent.W = 0.0f;
                    tangent.Normalize();

                    binormal = tangent;

                    texture.X = theta / MathUtil.Pi * 2f;
                    texture.Y = phi / MathUtil.Pi;

                    vertList.Add(VertexData.CreateVertexPositionNormalTextureTangent(
                        position,
                        normal,
                        tangent,
                        binormal,
                        texture));
                }
            }

            vertList.Add(VertexData.CreateVertexPositionNormalTextureTangent(
                new Vector3(0.0f, -radius, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector2(0.0f, 1.0f)));

            List<uint> indexList = new List<uint>();

            for (uint index = 1; index <= sliceCount; ++index)
            {
                indexList.Add(0);
                indexList.Add(index + 1);
                indexList.Add(index);
            }

            //
            // Compute indices for inner stacks (not connected to poles).
            //

            // Offset the indices to the index of the first vertex in the first ring.
            // This is just skipping the top pole vertex.
            uint baseIndex = 1;
            uint ringVertexCount = sliceCount + 1;
            for (uint st = 0; st < stackCount - 2; ++st)
            {
                for (uint sl = 0; sl < sliceCount; ++sl)
                {
                    indexList.Add(baseIndex + st * ringVertexCount + sl);
                    indexList.Add(baseIndex + st * ringVertexCount + sl + 1);
                    indexList.Add(baseIndex + (st + 1) * ringVertexCount + sl);

                    indexList.Add(baseIndex + (st + 1) * ringVertexCount + sl);
                    indexList.Add(baseIndex + st * ringVertexCount + sl + 1);
                    indexList.Add(baseIndex + (st + 1) * ringVertexCount + sl + 1);
                }
            }

            //
            // Compute indices for bottom stack.  The bottom stack was written last to the vertex buffer
            // and connects the bottom pole to the bottom ring.
            //

            // South pole vertex was added last.
            uint southPoleIndex = (uint)vertList.Count - 1;

            // Offset the indices to the index of the first vertex in the last ring.
            baseIndex = southPoleIndex - ringVertexCount;

            for (uint index = 0; index < sliceCount; ++index)
            {
                indexList.Add(southPoleIndex);
                indexList.Add(baseIndex + index);
                indexList.Add(baseIndex + index + 1);
            }

            v = vertList.ToArray();
            i = indexList.ToArray();
        }
        /// <summary>
        /// Creates a cone of VertexPositionNormalTextureTangent VertexData
        /// </summary>
        /// <param name="radius">The base radius</param>
        /// <param name="sliceCount">The base slice count</param>
        /// <param name="height">Cone height</param>
        /// <param name="v">Result vertices</param>
        /// <param name="i">Result indices</param>
        public static void CreateCone(float radius, uint sliceCount, float height, out VertexData[] v, out uint[] i)
        {
            List<VertexData> vertList = new List<VertexData>();
            List<uint> indexList = new List<uint>();

            vertList.Add(VertexData.CreateVertexPositionNormalTextureTangent(
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(0.0f, +1.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector2(0.0f, 0.0f)));

            vertList.Add(VertexData.CreateVertexPositionNormalTextureTangent(
                new Vector3(0.0f, -height, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector2(0.0f, 0.0f)));

            float thetaStep = MathUtil.TwoPi / (float)sliceCount;

            for (int sl = 0; sl < sliceCount; sl++)
            {
                float theta = sl * thetaStep;

                Vector3 position;
                Vector3 normal;
                Vector3 tangent;
                Vector3 binormal;
                Vector2 texture;

                // spherical to cartesian
                position.X = radius * (float)Math.Sin(MathUtil.PiOverTwo) * (float)Math.Cos(theta);
                position.Y = -height;
                position.Z = radius * (float)Math.Sin(MathUtil.PiOverTwo) * (float)Math.Sin(theta);

                normal = position;
                normal.Normalize();

                // Partial derivative of P with respect to theta
                tangent.X = -radius * (float)Math.Sin(MathUtil.PiOverTwo) * (float)Math.Sin(theta);
                tangent.Y = 0.0f;
                tangent.Z = +radius * (float)Math.Sin(MathUtil.PiOverTwo) * (float)Math.Cos(theta);
                tangent.Normalize();

                binormal = tangent;

                texture.X = theta / MathUtil.TwoPi;
                texture.Y = 1f;

                vertList.Add(VertexData.CreateVertexPositionNormalTextureTangent(
                    position,
                    normal,
                    tangent,
                    binormal,
                    texture));
            }

            for (uint index = 0; index < sliceCount; index++)
            {
                indexList.Add(0);
                indexList.Add(index + 2);
                indexList.Add(index == sliceCount - 1 ? 2 : index + 3);

                indexList.Add(1);
                indexList.Add(index == sliceCount - 1 ? 2 : index + 3);
                indexList.Add(index + 2);
            }

            v = vertList.ToArray();
            i = indexList.ToArray();
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
                else if (vertices[0].VertexType == VertexTypes.Terrain)
                {
                    buffer = CreateVertexBuffer<VertexTerrain>(device, vertices, dynamic);
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
            else if (vertices[0].VertexType == VertexTypes.Terrain)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexTerrain)v));
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
        /// Calculate tangent, normal and binormals of triangle vertices
        /// </summary>
        /// <param name="vertex1">Vertex 1</param>
        /// <param name="vertex2">Vertex 2</param>
        /// <param name="vertex3">Vertex 3</param>
        /// <param name="tangent">Tangen result</param>
        /// <param name="binormal">Binormal result</param>
        /// <param name="normal">Normal result</param>
        public static void ComputeNormals(VertexData vertex1, VertexData vertex2, VertexData vertex3, out Vector3 tangent, out Vector3 binormal, out Vector3 normal)
        {
            // Calculate the two vectors for the face.
            Vector3 vector1 = vertex2.Position.Value - vertex1.Position.Value;
            Vector3 vector2 = vertex3.Position.Value - vertex1.Position.Value;

            // Calculate the tu and tv texture space vectors.
            Vector2 tuVector = new Vector2(
                vertex2.Texture0.Value.X - vertex1.Texture0.Value.X,
                vertex3.Texture0.Value.X - vertex1.Texture0.Value.X);
            Vector2 tvVector = new Vector2(
                vertex2.Texture0.Value.Y - vertex1.Texture0.Value.Y,
                vertex3.Texture0.Value.Y - vertex1.Texture0.Value.Y);

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
            if (this.Texture0.HasValue) text += string.Format("Texture: {0}; ", this.Texture0);
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
        /// Texture UV
        /// </summary>
        Texture1,
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
