using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Primitives
{
    /// <summary>
    /// Skinned position texture vertex format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexSkinnedPositionTexture : IVertexData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <returns>Returns input elements</returns>
        public static EngineInputElement[] Input(int slot)
        {
            return
            [
                new ("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, slot, EngineInputClassification.PerVertexData, 0),
                new ("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 12, slot, EngineInputClassification.PerVertexData, 0),
                new ("WEIGHTS", 0, SharpDX.DXGI.Format.R32G32B32_Float, 20, slot, EngineInputClassification.PerVertexData, 0),
                new ("BONEINDICES", 0, SharpDX.DXGI.Format.R8G8B8A8_UInt, 32, slot, EngineInputClassification.PerVertexData, 0 ),
            ];
        }
        /// <summary>
        /// Converts a vertex data list to a vertex array
        /// </summary>
        /// <param name="vertices">Vertices list</param>
        public static async Task<IEnumerable<IVertexData>> Convert(IEnumerable<VertexData> vertices, IEnumerable<Weight> weights, IEnumerable<string> skinBoneNames)
        {
            var vArray = vertices.ToArray();
            var vWeights = weights.ToArray();
            var vBones = skinBoneNames.ToArray();

            var res = new IVertexData[vArray.Length];

            Parallel.For(0, vArray.Length, (index) =>
            {
                var v = vArray[index];

                var p = v.Position ?? Vector3.Zero;
                var t = v.Texture ?? Vector2.Zero;

                var vw = Array.FindAll(vWeights, w => w.VertexIndex == v.VertexIndex);
                int vwCount = vw?.Length ?? 0;

                var wg0 = vwCount > 0 ? vw[0].WeightValue : 0f;
                var wg1 = vwCount > 1 ? vw[1].WeightValue : 0f;
                var wg2 = vwCount > 2 ? vw[2].WeightValue : 0f;

                var bn0 = vwCount > 0 ? Math.Max(0, Array.IndexOf(vBones, vw[0].Joint)) : 0;
                var bn1 = vwCount > 1 ? Math.Max(0, Array.IndexOf(vBones, vw[1].Joint)) : 0;
                var bn2 = vwCount > 2 ? Math.Max(0, Array.IndexOf(vBones, vw[2].Joint)) : 0;
                var bn3 = vwCount > 3 ? Math.Max(0, Array.IndexOf(vBones, vw[3].Joint)) : 0;

                res[index] = new VertexSkinnedPositionTexture
                {
                    Position = p,
                    Texture = t,
                    Weight1 = wg0,
                    Weight2 = wg1,
                    Weight3 = wg2,
                    BoneIndex1 = (byte)bn0,
                    BoneIndex2 = (byte)bn1,
                    BoneIndex3 = (byte)bn2,
                    BoneIndex4 = (byte)bn3,
                };
            });

            return await Task.FromResult(res);
        }

        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Texture
        /// </summary>
        public Vector2 Texture;
        /// <summary>
        /// Weight 1
        /// </summary>
        public float Weight1;
        /// <summary>
        /// Weight 2
        /// </summary>
        public float Weight2;
        /// <summary>
        /// Weight 3
        /// </summary>
        public float Weight3;
        /// <summary>
        /// Bone 1
        /// </summary>
        public byte BoneIndex1;
        /// <summary>
        /// Bone 2
        /// </summary>
        public byte BoneIndex2;
        /// <summary>
        /// Bone 3
        /// </summary>
        public byte BoneIndex3;
        /// <summary>
        /// Bone 4
        /// </summary>
        public byte BoneIndex4;
        /// <summary>
        /// Vertex type
        /// </summary>
        public readonly VertexTypes VertexType
        {
            get
            {
                return VertexTypes.PositionTextureSkinned;
            }
        }

        /// <summary>
        /// Gets if structure contains data for the specified channel
        /// </summary>
        /// <param name="channel">Data channel</param>
        /// <returns>Returns true if structure contains data for the specified channel</returns>
        public readonly bool HasChannel(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return true;
            else if (channel == VertexDataChannels.Texture) return true;
            else if (channel == VertexDataChannels.Weights) return true;
            else if (channel == VertexDataChannels.BoneIndices) return true;
            else return false;
        }
        /// <summary>
        /// Gets data channel value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Data channel</param>
        /// <returns>Returns data for the specified channel</returns>
        public readonly T GetChannelValue<T>(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return (T)(object)Position;
            else if (channel == VertexDataChannels.Texture) return (T)(object)Texture;
            else if (channel == VertexDataChannels.Weights) return (T)(object)(new[] { Weight1, Weight2, Weight3, 1.0f - Weight1 - Weight2 - Weight3 });
            else if (channel == VertexDataChannels.BoneIndices) return (T)(object)(new[] { BoneIndex1, BoneIndex2, BoneIndex3, BoneIndex4 });
            else throw new EngineException($"Channel data not found: {channel}");
        }
        /// <summary>
        /// Sets the channer value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Channel</param>
        /// <param name="value">Value</param>
        public void SetChannelValue<T>(VertexDataChannels channel, T value)
        {
            if (channel == VertexDataChannels.Position) Position = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Texture) Texture = (Vector2)(object)value;
            else if (channel == VertexDataChannels.Weights)
            {
                float[] weights = (float[])(object)value;

                Weight1 = weights[0];
                Weight2 = weights[1];
                Weight3 = weights[2];
            }
            else if (channel == VertexDataChannels.BoneIndices)
            {
                byte[] boneIndices = (byte[])(object)value;

                BoneIndex1 = boneIndices[0];
                BoneIndex2 = boneIndices[1];
                BoneIndex3 = boneIndices[2];
                BoneIndex4 = boneIndices[3];
            }
            else throw new EngineException($"Channel data not found: {channel}");
        }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(VertexSkinnedPositionTexture));
        }
        /// <summary>
        /// Get input elements
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <returns>Returns input elements</returns>
        public readonly EngineInputElement[] GetInput(int slot)
        {
            return Input(slot);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Skinned; Position: {Position}; Texture: {Texture};";
        }
    }
}
