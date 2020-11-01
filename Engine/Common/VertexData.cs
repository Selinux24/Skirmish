using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexPosition CreateVertexPosition(VertexData v)
        {
            return new VertexPosition
            {
                Position = v.Position ?? Vector3.Zero,
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
                Position = v.Position ?? Vector3.Zero,
                Color = v.Color ?? Color4.White,
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
                Position = v.Position ?? Vector3.Zero,
                Normal = v.Normal ?? Vector3.Zero,
                Color = v.Color ?? Color4.White,
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
                Position = v.Position ?? Vector3.Zero,
                Texture = v.Texture ?? Vector2.Zero
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
                Position = v.Position ?? Vector3.Zero,
                Normal = v.Normal ?? Vector3.Zero,
                Texture = v.Texture ?? Vector2.Zero
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
                Position = v.Position ?? Vector3.Zero,
                Normal = v.Normal ?? Vector3.Zero,
                Texture = v.Texture ?? Vector2.Zero,
                Tangent = v.Tangent ?? Vector3.UnitX,
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
                Position = v.Position ?? Vector3.Zero,
                Weight1 = vw?.Length > 0 ? vw[0].WeightValue : 0f,
                Weight2 = vw?.Length > 1 ? vw[1].WeightValue : 0f,
                Weight3 = vw?.Length > 2 ? vw[2].WeightValue : 0f,
                BoneIndex1 = vw?.Length > 0 ? (FindBoneIndex(skinBoneNames, vw[0].Joint)) : ((byte)0),
                BoneIndex2 = vw?.Length > 1 ? (FindBoneIndex(skinBoneNames, vw[1].Joint)) : ((byte)0),
                BoneIndex3 = vw?.Length > 2 ? (FindBoneIndex(skinBoneNames, vw[2].Joint)) : ((byte)0),
                BoneIndex4 = vw?.Length > 3 ? (FindBoneIndex(skinBoneNames, vw[3].Joint)) : ((byte)0)
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
                Position = v.Position ?? Vector3.Zero,
                Color = v.Color ?? Color4.White,
                Weight1 = vw?.Length > 0 ? vw[0].WeightValue : 0f,
                Weight2 = vw?.Length > 1 ? vw[1].WeightValue : 0f,
                Weight3 = vw?.Length > 2 ? vw[2].WeightValue : 0f,
                BoneIndex1 = vw?.Length > 0 ? (FindBoneIndex(skinBoneNames, vw[0].Joint)) : ((byte)0),
                BoneIndex2 = vw?.Length > 1 ? (FindBoneIndex(skinBoneNames, vw[1].Joint)) : ((byte)0),
                BoneIndex3 = vw?.Length > 2 ? (FindBoneIndex(skinBoneNames, vw[2].Joint)) : ((byte)0),
                BoneIndex4 = vw?.Length > 3 ? (FindBoneIndex(skinBoneNames, vw[3].Joint)) : ((byte)0)
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
                Position = v.Position ?? Vector3.Zero,
                Normal = v.Normal ?? Vector3.Zero,
                Color = v.Color ?? Color4.White,
                Weight1 = vw?.Length > 0 ? vw[0].WeightValue : 0f,
                Weight2 = vw?.Length > 1 ? vw[1].WeightValue : 0f,
                Weight3 = vw?.Length > 2 ? vw[2].WeightValue : 0f,
                BoneIndex1 = vw?.Length > 0 ? (FindBoneIndex(skinBoneNames, vw[0].Joint)) : ((byte)0),
                BoneIndex2 = vw?.Length > 1 ? (FindBoneIndex(skinBoneNames, vw[1].Joint)) : ((byte)0),
                BoneIndex3 = vw?.Length > 2 ? (FindBoneIndex(skinBoneNames, vw[2].Joint)) : ((byte)0),
                BoneIndex4 = vw?.Length > 3 ? (FindBoneIndex(skinBoneNames, vw[3].Joint)) : ((byte)0)
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
                Position = v.Position ?? Vector3.Zero,
                Texture = v.Texture ?? Vector2.Zero,
                Weight1 = vw?.Length > 0 ? vw[0].WeightValue : 0f,
                Weight2 = vw?.Length > 1 ? vw[1].WeightValue : 0f,
                Weight3 = vw?.Length > 2 ? vw[2].WeightValue : 0f,
                BoneIndex1 = vw?.Length > 0 ? (FindBoneIndex(skinBoneNames, vw[0].Joint)) : ((byte)0),
                BoneIndex2 = vw?.Length > 1 ? (FindBoneIndex(skinBoneNames, vw[1].Joint)) : ((byte)0),
                BoneIndex3 = vw?.Length > 2 ? (FindBoneIndex(skinBoneNames, vw[2].Joint)) : ((byte)0),
                BoneIndex4 = vw?.Length > 3 ? (FindBoneIndex(skinBoneNames, vw[3].Joint)) : ((byte)0)
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
                Position = v.Position ?? Vector3.Zero,
                Normal = v.Normal ?? Vector3.Zero,
                Texture = v.Texture ?? Vector2.Zero,
                Weight1 = vw?.Length > 0 ? vw[0].WeightValue : 0f,
                Weight2 = vw?.Length > 1 ? vw[1].WeightValue : 0f,
                Weight3 = vw?.Length > 2 ? vw[2].WeightValue : 0f,
                BoneIndex1 = vw?.Length > 0 ? (FindBoneIndex(skinBoneNames, vw[0].Joint)) : ((byte)0),
                BoneIndex2 = vw?.Length > 1 ? (FindBoneIndex(skinBoneNames, vw[1].Joint)) : ((byte)0),
                BoneIndex3 = vw?.Length > 2 ? (FindBoneIndex(skinBoneNames, vw[2].Joint)) : ((byte)0),
                BoneIndex4 = vw?.Length > 3 ? (FindBoneIndex(skinBoneNames, vw[3].Joint)) : ((byte)0)
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
                Position = v.Position ?? Vector3.Zero,
                Normal = v.Normal ?? Vector3.Zero,
                Texture = v.Texture ?? Vector2.Zero,
                Tangent = v.Tangent ?? Vector3.UnitX,
                Weight1 = vw?.Length > 0 ? vw[0].WeightValue : 0f,
                Weight2 = vw?.Length > 1 ? vw[1].WeightValue : 0f,
                Weight3 = vw?.Length > 2 ? vw[2].WeightValue : 0f,
                BoneIndex1 = vw?.Length > 0 ? (FindBoneIndex(skinBoneNames, vw[0].Joint)) : ((byte)0),
                BoneIndex2 = vw?.Length > 1 ? (FindBoneIndex(skinBoneNames, vw[1].Joint)) : ((byte)0),
                BoneIndex3 = vw?.Length > 2 ? (FindBoneIndex(skinBoneNames, vw[2].Joint)) : ((byte)0),
                BoneIndex4 = vw?.Length > 3 ? (FindBoneIndex(skinBoneNames, vw[3].Joint)) : ((byte)0)
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
                Position = v.Position ?? Vector3.Zero,
                Normal = v.Normal ?? Vector3.Zero,
                Texture = v.Texture ?? Vector2.Zero,
                Tangent = v.Tangent ?? Vector3.UnitX,
                Color = v.Color ?? Color4.White,
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

            return 0;
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
        /// Gets the vertex type based on vertex data
        /// </summary>
        /// <param name="v">Vertex</param>
        /// <param name="preferTextured">Sets wether textured formats were prefered over vertex colored formats</param>
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
        /// <param name="preferTextured">Sets wether textured formats were prefered over vertex colored formats</param>
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
        /// <param name="preferTextured">Sets wether textured formats were prefered over vertex colored formats</param>
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
        public static IEnumerable<IVertexData> Convert(VertexTypes vertexType, IEnumerable<VertexData> vertices, IEnumerable<Weight> weights, IEnumerable<string> skinBoneNames)
        {
            List<IVertexData> vertexList = new List<IVertexData>();

            if (vertexType == VertexTypes.Position)
            {
                vertices.ToList().ForEach((v) => { vertexList.Add(CreateVertexPosition(v)); });
            }
            else if (vertexType == VertexTypes.PositionColor)
            {
                vertices.ToList().ForEach((v) => { vertexList.Add(CreateVertexPositionColor(v)); });
            }
            else if (vertexType == VertexTypes.PositionNormalColor)
            {
                vertices.ToList().ForEach((v) => { vertexList.Add(CreateVertexPositionNormalColor(v)); });
            }
            else if (vertexType == VertexTypes.PositionTexture)
            {
                vertices.ToList().ForEach((v) => { vertexList.Add(CreateVertexPositionTexture(v)); });
            }
            else if (vertexType == VertexTypes.PositionNormalTexture)
            {
                vertices.ToList().ForEach((v) => { vertexList.Add(CreateVertexPositionNormalTexture(v)); });
            }
            else if (vertexType == VertexTypes.PositionNormalTextureTangent)
            {
                vertices.ToList().ForEach((v) => { vertexList.Add(CreateVertexPositionNormalTextureTangent(v)); });
            }
            else if (vertexType == VertexTypes.PositionSkinned)
            {
                vertices.ToList().ForEach((v) =>
                {
                    var vw = weights.Where(w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(CreateVertexSkinnedPosition(v, vw.ToArray(), skinBoneNames.ToArray()));
                });
            }
            else if (vertexType == VertexTypes.PositionColorSkinned)
            {
                vertices.ToList().ForEach((v) =>
                {
                    var vw = weights.Where(w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(CreateVertexSkinnedPositionColor(v, vw.ToArray(), skinBoneNames.ToArray()));
                });
            }
            else if (vertexType == VertexTypes.PositionNormalColorSkinned)
            {
                vertices.ToList().ForEach((v) =>
                {
                    var vw = weights.Where(w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(CreateVertexSkinnedPositionNormalColor(v, vw.ToArray(), skinBoneNames.ToArray()));
                });
            }
            else if (vertexType == VertexTypes.PositionTextureSkinned)
            {
                vertices.ToList().ForEach((v) =>
                {
                    var vw = weights.Where(w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(CreateVertexSkinnedPositionTexture(v, vw.ToArray(), skinBoneNames.ToArray()));
                });
            }
            else if (vertexType == VertexTypes.PositionNormalTextureSkinned)
            {
                vertices.ToList().ForEach((v) =>
                {
                    var vw = weights.Where(w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(CreateVertexSkinnedPositionNormalTexture(v, vw.ToArray(), skinBoneNames.ToArray()));
                });
            }
            else if (vertexType == VertexTypes.PositionNormalTextureTangentSkinned)
            {
                vertices.ToList().ForEach((v) =>
                {
                    var vw = weights.Where(w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(CreateVertexSkinnedPositionNormalTextureTangent(v, vw.ToArray(), skinBoneNames.ToArray()));
                });
            }
            else if (vertexType == VertexTypes.Terrain)
            {
                vertices.ToList().ForEach((v) => { vertexList.Add(CreateVertexTerrain(v)); });
            }
            else
            {
                throw new EngineException(string.Format("Unknown vertex type: {0}", vertexType));
            }

            return vertexList.ToArray();
        }
        /// <summary>
        /// Apply weighted transforms to the vertext data
        /// </summary>
        /// <param name="vertex">Vertex data</param>
        /// <param name="boneTransforms">Bone transforms list</param>
        /// <returns>Returns the weighted position</returns>
        public static Vector3 ApplyWeight(IVertexData vertex, IEnumerable<Matrix> boneTransforms)
        {
            Vector3 position = vertex.HasChannel(VertexDataChannels.Position) ? vertex.GetChannelValue<Vector3>(VertexDataChannels.Position) : Vector3.Zero;

            if (!IsSkinned(vertex.VertexType))
            {
                return position;
            }

            byte[] boneIndices = vertex.HasChannel(VertexDataChannels.BoneIndices) ? vertex.GetChannelValue<byte[]>(VertexDataChannels.BoneIndices) : new byte[] { };
            float[] boneWeights = vertex.HasChannel(VertexDataChannels.Weights) ? vertex.GetChannelValue<float[]>(VertexDataChannels.Weights) : new float[] { };
            Matrix[] transforms = boneTransforms.ToArray();

            Vector3 t = Vector3.Zero;

            for (int w = 0; w < boneIndices.Length; w++)
            {
                float weight = boneWeights[w];
                if (weight > 0)
                {
                    byte index = boneIndices[w];
                    var boneTransform = transforms != null ? transforms[index] : Matrix.Identity;

                    Vector3.TransformCoordinate(ref position, ref boneTransform, out Vector3 p);

                    t += (p * weight);
                }
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
                return new VertexData[] { };
            }

            if (transform.IsIdentity)
            {
                return new List<VertexData>(vertices);
            }

            List<VertexData> result = new List<VertexData>();

            foreach (var v in vertices)
            {
                result.Add(Transform(v, transform));
            }

            return result;
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
        /// Generates a vertex data array from a geometry descriptor
        /// </summary>
        /// <param name="descriptor">Geometry descriptor</param>
        /// <returns>Returns a vertex array</returns>
        public static IEnumerable<VertexData> FromDescriptor(GeometryDescriptor descriptor)
        {
            List<VertexData> res = new List<VertexData>();

            var vertices = descriptor.Vertices?.ToArray() ?? new Vector3[] { };
            var normals = descriptor.Normals?.ToArray() ?? new Vector3[] { };
            var uvs = descriptor.Uvs?.ToArray() ?? new Vector2[] { };
            var tangents = descriptor.Tangents?.ToArray() ?? new Vector3[] { };
            var binormals = descriptor.Binormals?.ToArray() ?? new Vector3[] { };

            for (int i = 0; i < vertices.Length; i++)
            {
                res.Add(new VertexData()
                {
                    Position = vertices[i],
                    Normal = normals.Any() ? normals[i] : (Vector3?)null,
                    Texture = uvs.Any() ? uvs[i] : (Vector2?)null,
                    Tangent = tangents.Any() ? tangents[i] : (Vector3?)null,
                    BiNormal = binormals.Any() ? binormals[i] : (Vector3?)null,
                });
            }

            return res.ToArray();
        }

        /// <summary>
        /// Transforms this vertex by the given matrix
        /// </summary>
        /// <param name="transform">Transformation matrix</param>
        /// <returns>Returns the transformed vertex</returns>
        public VertexData Transform(Matrix transform)
        {
            return Transform(this, transform);
        }
        /// <summary>
        /// Gets the vertex list stride
        /// </summary>
        /// <returns>Returns the list stride</returns>
        public int GetStride()
        {
            return 1;
        }
        /// <summary>
        /// Gets the vertex list
        /// </summary>
        /// <returns>Returns a vertex list</returns>
        public IEnumerable<Vector3> GetVertices()
        {
            return new Vector3[] { Position.Value };
        }
        /// <summary>
        /// Gets the vertex list topology
        /// </summary>
        /// <returns>Returns the list topology</returns>
        public Topology GetTopology()
        {
            return Topology.PointList;
        }

        /// <inheritdoc/>
        public override string ToString()
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
