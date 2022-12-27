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
        public static VertexSkinnedPosition CreateVertexSkinnedPosition(VertexData v, IEnumerable<Weight> vw, IEnumerable<string> skinBoneNames)
        {
            return new VertexSkinnedPosition
            {
                Position = v.Position ?? Vector3.Zero,
                Weight1 = vw?.Count() > 0 ? vw.ElementAt(0).WeightValue : 0f,
                Weight2 = vw?.Count() > 1 ? vw.ElementAt(1).WeightValue : 0f,
                Weight3 = vw?.Count() > 2 ? vw.ElementAt(2).WeightValue : 0f,
                BoneIndex1 = vw?.Count() > 0 ? FindBoneIndex(skinBoneNames, vw.ElementAt(0).Joint) : ((byte)0),
                BoneIndex2 = vw?.Count() > 1 ? FindBoneIndex(skinBoneNames, vw.ElementAt(1).Joint) : ((byte)0),
                BoneIndex3 = vw?.Count() > 2 ? FindBoneIndex(skinBoneNames, vw.ElementAt(2).Joint) : ((byte)0),
                BoneIndex4 = vw?.Count() > 3 ? FindBoneIndex(skinBoneNames, vw.ElementAt(3).Joint) : ((byte)0)
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPositionColor CreateVertexSkinnedPositionColor(VertexData v, IEnumerable<Weight> vw, IEnumerable<string> skinBoneNames)
        {
            return new VertexSkinnedPositionColor
            {
                Position = v.Position ?? Vector3.Zero,
                Color = v.Color ?? Color4.White,
                Weight1 = vw?.Count() > 0 ? vw.ElementAt(0).WeightValue : 0f,
                Weight2 = vw?.Count() > 1 ? vw.ElementAt(1).WeightValue : 0f,
                Weight3 = vw?.Count() > 2 ? vw.ElementAt(2).WeightValue : 0f,
                BoneIndex1 = vw?.Count() > 0 ? FindBoneIndex(skinBoneNames, vw.ElementAt(0).Joint) : ((byte)0),
                BoneIndex2 = vw?.Count() > 1 ? FindBoneIndex(skinBoneNames, vw.ElementAt(1).Joint) : ((byte)0),
                BoneIndex3 = vw?.Count() > 2 ? FindBoneIndex(skinBoneNames, vw.ElementAt(2).Joint) : ((byte)0),
                BoneIndex4 = vw?.Count() > 3 ? FindBoneIndex(skinBoneNames, vw.ElementAt(3).Joint) : ((byte)0)
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPositionNormalColor CreateVertexSkinnedPositionNormalColor(VertexData v, IEnumerable<Weight> vw, IEnumerable<string> skinBoneNames)
        {
            return new VertexSkinnedPositionNormalColor
            {
                Position = v.Position ?? Vector3.Zero,
                Normal = v.Normal ?? Vector3.Zero,
                Color = v.Color ?? Color4.White,
                Weight1 = vw?.Count() > 0 ? vw.ElementAt(0).WeightValue : 0f,
                Weight2 = vw?.Count() > 1 ? vw.ElementAt(1).WeightValue : 0f,
                Weight3 = vw?.Count() > 2 ? vw.ElementAt(2).WeightValue : 0f,
                BoneIndex1 = vw?.Count() > 0 ? FindBoneIndex(skinBoneNames, vw.ElementAt(0).Joint) : ((byte)0),
                BoneIndex2 = vw?.Count() > 1 ? FindBoneIndex(skinBoneNames, vw.ElementAt(1).Joint) : ((byte)0),
                BoneIndex3 = vw?.Count() > 2 ? FindBoneIndex(skinBoneNames, vw.ElementAt(2).Joint) : ((byte)0),
                BoneIndex4 = vw?.Count() > 3 ? FindBoneIndex(skinBoneNames, vw.ElementAt(3).Joint) : ((byte)0)
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPositionTexture CreateVertexSkinnedPositionTexture(VertexData v, IEnumerable<Weight> vw, IEnumerable<string> skinBoneNames)
        {
            return new VertexSkinnedPositionTexture
            {
                Position = v.Position ?? Vector3.Zero,
                Texture = v.Texture ?? Vector2.Zero,
                Weight1 = vw?.Count() > 0 ? vw.ElementAt(0).WeightValue : 0f,
                Weight2 = vw?.Count() > 1 ? vw.ElementAt(1).WeightValue : 0f,
                Weight3 = vw?.Count() > 2 ? vw.ElementAt(2).WeightValue : 0f,
                BoneIndex1 = vw?.Count() > 0 ? FindBoneIndex(skinBoneNames, vw.ElementAt(0).Joint) : ((byte)0),
                BoneIndex2 = vw?.Count() > 1 ? FindBoneIndex(skinBoneNames, vw.ElementAt(1).Joint) : ((byte)0),
                BoneIndex3 = vw?.Count() > 2 ? FindBoneIndex(skinBoneNames, vw.ElementAt(2).Joint) : ((byte)0),
                BoneIndex4 = vw?.Count() > 3 ? FindBoneIndex(skinBoneNames, vw.ElementAt(3).Joint) : ((byte)0)
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPositionNormalTexture CreateVertexSkinnedPositionNormalTexture(VertexData v, IEnumerable<Weight> vw, IEnumerable<string> skinBoneNames)
        {
            return new VertexSkinnedPositionNormalTexture
            {
                Position = v.Position ?? Vector3.Zero,
                Normal = v.Normal ?? Vector3.Zero,
                Texture = v.Texture ?? Vector2.Zero,
                Weight1 = vw?.Count() > 0 ? vw.ElementAt(0).WeightValue : 0f,
                Weight2 = vw?.Count() > 1 ? vw.ElementAt(1).WeightValue : 0f,
                Weight3 = vw?.Count() > 2 ? vw.ElementAt(2).WeightValue : 0f,
                BoneIndex1 = vw?.Count() > 0 ? FindBoneIndex(skinBoneNames, vw.ElementAt(0).Joint) : ((byte)0),
                BoneIndex2 = vw?.Count() > 1 ? FindBoneIndex(skinBoneNames, vw.ElementAt(1).Joint) : ((byte)0),
                BoneIndex3 = vw?.Count() > 2 ? FindBoneIndex(skinBoneNames, vw.ElementAt(2).Joint) : ((byte)0),
                BoneIndex4 = vw?.Count() > 3 ? FindBoneIndex(skinBoneNames, vw.ElementAt(3).Joint) : ((byte)0)
            };
        }
        /// <summary>
        /// Generates vertex from helper
        /// </summary>
        /// <param name="v">Helper</param>
        /// <param name="vw">Weights</param>
        /// <returns>Returns the generated vertex</returns>
        public static VertexSkinnedPositionNormalTextureTangent CreateVertexSkinnedPositionNormalTextureTangent(VertexData v, IEnumerable<Weight> vw, IEnumerable<string> skinBoneNames)
        {
            return new VertexSkinnedPositionNormalTextureTangent
            {
                Position = v.Position ?? Vector3.Zero,
                Normal = v.Normal ?? Vector3.Zero,
                Texture = v.Texture ?? Vector2.Zero,
                Tangent = v.Tangent ?? Vector3.UnitX,
                Weight1 = vw?.Count() > 0 ? vw.ElementAt(0).WeightValue : 0f,
                Weight2 = vw?.Count() > 1 ? vw.ElementAt(1).WeightValue : 0f,
                Weight3 = vw?.Count() > 2 ? vw.ElementAt(2).WeightValue : 0f,
                BoneIndex1 = vw?.Count() > 0 ? FindBoneIndex(skinBoneNames, vw.ElementAt(0).Joint) : ((byte)0),
                BoneIndex2 = vw?.Count() > 1 ? FindBoneIndex(skinBoneNames, vw.ElementAt(1).Joint) : ((byte)0),
                BoneIndex3 = vw?.Count() > 2 ? FindBoneIndex(skinBoneNames, vw.ElementAt(2).Joint) : ((byte)0),
                BoneIndex4 = vw?.Count() > 3 ? FindBoneIndex(skinBoneNames, vw.ElementAt(3).Joint) : ((byte)0)
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
        private static byte FindBoneIndex(IEnumerable<string> jointNames, string joint)
        {
            byte result = 0;

            int index = Array.IndexOf(jointNames.ToArray(), joint);
            if (index >= 0)
            {
                result = (byte)index;
            }

            return result;
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
        public static async Task<IEnumerable<IVertexData>> Convert(VertexTypes vertexType, IEnumerable<VertexData> vertices, IEnumerable<Weight> weights, IEnumerable<string> skinBoneNames)
        {
            if (vertexType == VertexTypes.Position)
            {
                var res = new IVertexData[vertices.Count()];

                Parallel.For(0, vertices.Count(), (index) =>
                {
                    var v = vertices.ElementAt(index);

                    res[index] = CreateVertexPosition(v);
                });

                return await Task.FromResult(res);
            }
            else if (vertexType == VertexTypes.PositionColor)
            {
                var res = new IVertexData[vertices.Count()];

                Parallel.For(0, vertices.Count(), (index) =>
                {
                    var v = vertices.ElementAt(index);

                    res[index] = CreateVertexPositionColor(v);
                });

                return await Task.FromResult(res);
            }
            else if (vertexType == VertexTypes.PositionNormalColor)
            {
                var res = new IVertexData[vertices.Count()];

                Parallel.For(0, vertices.Count(), (index) =>
                {
                    var v = vertices.ElementAt(index);

                    res[index] = CreateVertexPositionNormalColor(v);
                });

                return await Task.FromResult(res);
            }
            else if (vertexType == VertexTypes.PositionTexture)
            {
                var res = new IVertexData[vertices.Count()];

                Parallel.For(0, vertices.Count(), (index) =>
                {
                    var v = vertices.ElementAt(index);

                    res[index] = CreateVertexPositionTexture(v);
                });

                return await Task.FromResult(res);
            }
            else if (vertexType == VertexTypes.PositionNormalTexture)
            {
                var res = new IVertexData[vertices.Count()];

                Parallel.For(0, vertices.Count(), (index) =>
                {
                    var v = vertices.ElementAt(index);

                    res[index] = CreateVertexPositionNormalTexture(v);
                });

                return await Task.FromResult(res);
            }
            else if (vertexType == VertexTypes.PositionNormalTextureTangent)
            {
                var res = new IVertexData[vertices.Count()];

                Parallel.For(0, vertices.Count(), (index) =>
                {
                    var v = vertices.ElementAt(index);

                    res[index] = CreateVertexPositionNormalTextureTangent(v);
                });

                return await Task.FromResult(res);
            }
            else if (vertexType == VertexTypes.PositionSkinned)
            {
                var res = new IVertexData[vertices.Count()];

                Parallel.For(0, vertices.Count(), (index) =>
                {
                    var v = vertices.ElementAt(index);
                    var vw = weights.Where(w => w.VertexIndex == v.VertexIndex);

                    res[index] = CreateVertexSkinnedPosition(v, vw, skinBoneNames);
                });

                return await Task.FromResult(res);
            }
            else if (vertexType == VertexTypes.PositionColorSkinned)
            {
                var res = new IVertexData[vertices.Count()];

                Parallel.For(0, vertices.Count(), (index) =>
                {
                    var v = vertices.ElementAt(index);
                    var vw = weights.Where(w => w.VertexIndex == v.VertexIndex);

                    res[index] = CreateVertexSkinnedPositionColor(v, vw, skinBoneNames);
                });

                return await Task.FromResult(res);
            }
            else if (vertexType == VertexTypes.PositionNormalColorSkinned)
            {
                var res = new IVertexData[vertices.Count()];

                Parallel.For(0, vertices.Count(), (index) =>
                {
                    var v = vertices.ElementAt(index);
                    var vw = weights.Where(w => w.VertexIndex == v.VertexIndex);

                    res[index] = CreateVertexSkinnedPositionNormalColor(v, vw, skinBoneNames);
                });

                return await Task.FromResult(res);
            }
            else if (vertexType == VertexTypes.PositionTextureSkinned)
            {
                var res = new IVertexData[vertices.Count()];

                Parallel.For(0, vertices.Count(), (index) =>
                {
                    var v = vertices.ElementAt(index);
                    var vw = weights.Where(w => w.VertexIndex == v.VertexIndex);

                    res[index] = CreateVertexSkinnedPositionTexture(v, vw, skinBoneNames);
                });

                return await Task.FromResult(res);
            }
            else if (vertexType == VertexTypes.PositionNormalTextureSkinned)
            {
                var res = new IVertexData[vertices.Count()];

                Parallel.For(0, vertices.Count(), (index) =>
                {
                    var v = vertices.ElementAt(index);
                    var vw = weights.Where(w => w.VertexIndex == v.VertexIndex);

                    res[index] = CreateVertexSkinnedPositionNormalTexture(v, vw, skinBoneNames);
                });

                return await Task.FromResult(res);
            }
            else if (vertexType == VertexTypes.PositionNormalTextureTangentSkinned)
            {
                var res = new IVertexData[vertices.Count()];

                Parallel.For(0, vertices.Count(), (index) =>
                {
                    var v = vertices.ElementAt(index);
                    var vw = weights.Where(w => w.VertexIndex == v.VertexIndex);

                    res[index] = CreateVertexSkinnedPositionNormalTextureTangent(v, vw, skinBoneNames);
                });

                return await Task.FromResult(res);
            }
            else if (vertexType == VertexTypes.Terrain)
            {
                var res = new IVertexData[vertices.Count()];

                Parallel.For(0, vertices.Count(), (index) =>
                {
                    var v = vertices.ElementAt(index);

                    res[index] = CreateVertexTerrain(v);
                });

                return await Task.FromResult(res);
            }
            else
            {
                throw new EngineException(string.Format("Unknown vertex type: {0}", vertexType));
            }
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

            byte[] boneIndices = vertex.HasChannel(VertexDataChannels.BoneIndices) ? vertex.GetChannelValue<byte[]>(VertexDataChannels.BoneIndices) : Array.Empty<byte>();
            float[] boneWeights = vertex.HasChannel(VertexDataChannels.Weights) ? vertex.GetChannelValue<float[]>(VertexDataChannels.Weights) : Array.Empty<float>();
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
                return Array.Empty<VertexData>();
            }

            if (transform.IsIdentity)
            {
                return vertices.ToArray();
            }

            VertexData[] result = new VertexData[vertices.Count()];

            Parallel.For(0, vertices.Count(), (index) =>
            {
                result[index] = Transform(vertices.ElementAt(index), transform);
            });

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
            var vertices = descriptor.Vertices?.ToArray() ?? Array.Empty<Vector3>();
            var normals = descriptor.Normals?.ToArray() ?? Array.Empty<Vector3>();
            var uvs = descriptor.Uvs?.ToArray() ?? Array.Empty<Vector2>();
            var tangents = descriptor.Tangents?.ToArray() ?? Array.Empty<Vector3>();
            var binormals = descriptor.Binormals?.ToArray() ?? Array.Empty<Vector3>();

            VertexData[] res = new VertexData[vertices.Length];

            Parallel.For(0, vertices.Length, (i) =>
            {
                res[i] = new VertexData()
                {
                    Position = vertices[i],
                    Normal = normals.Any() ? normals[i] : (Vector3?)null,
                    Texture = uvs.Any() ? uvs[i] : (Vector2?)null,
                    Tangent = tangents.Any() ? tangents[i] : (Vector3?)null,
                    BiNormal = binormals.Any() ? binormals[i] : (Vector3?)null,
                };
            });

            return res;
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
