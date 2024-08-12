using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Format
{
    /// <summary>
    /// Position normal texure and tangent vertex format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalTextureTangent : IVertexData
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
                new ("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, slot, EngineInputClassification.PerVertexData, 0),
                new ("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 24, slot, EngineInputClassification.PerVertexData, 0),
                new ("TANGENT", 0, SharpDX.DXGI.Format.R32G32B32_Float, 32, slot, EngineInputClassification.PerVertexData, 0),
            ];
        }
        /// <summary>
        /// Generates a vertex array from specified components
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="normals">Normals</param>
        /// <param name="tangents">Tangents</param>
        /// <param name="uvs">UV coordinates</param>
        /// <returns>Returns the new generated vertex array</returns>
        public static IEnumerable<VertexPositionNormalTextureTangent> Generate(IEnumerable<Vector3> vertices, IEnumerable<Vector3> normals, IEnumerable<Vector2> uvs, IEnumerable<Vector3> tangents)
        {
            if (vertices.Count() != uvs.Count()) throw new ArgumentException("Vertices and uvs must have the same length");
            if (vertices.Count() != normals.Count()) throw new ArgumentException("Vertices and normals must have the same length");
            if (vertices.Count() != tangents.Count()) throw new ArgumentException("Vertices and tangents must have the same length");

            var vArray = vertices.ToArray();
            var nArray = normals.ToArray();
            var tArray = tangents.ToArray();
            var uvArray = uvs.ToArray();

            VertexPositionNormalTextureTangent[] res = new VertexPositionNormalTextureTangent[vArray.Length];

            for (int i = 0; i < vArray.Length; i++)
            {
                res[i] = new VertexPositionNormalTextureTangent()
                {
                    Position = vArray[i],
                    Normal = nArray[i],
                    Tangent = tArray[i],
                    Texture = uvArray[i]
                };
            }

            return res;
        }
        /// <summary>
        /// Converts a vertex data list to a vertex array
        /// </summary>
        /// <param name="vertices">Vertices list</param>
        public static async Task<IEnumerable<VertexPositionNormalTextureTangent>> Convert(IEnumerable<VertexData> vertices)
        {
            var vArray = vertices.ToArray();

            var res = new VertexPositionNormalTextureTangent[vArray.Length];

            Parallel.For(0, vArray.Length, (index) =>
            {
                var v = vArray[index];

                res[index] = new VertexPositionNormalTextureTangent
                {
                    Position = v.Position ?? Vector3.Zero,
                    Normal = v.Normal ?? Vector3.Zero,
                    Texture = v.Texture ?? Vector2.Zero,
                    Tangent = v.Tangent ?? Vector3.UnitX,
                };
            });

            return await Task.FromResult(res);
        }

        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Normal
        /// </summary>
        public Vector3 Normal;
        /// <summary>
        /// Texture UV
        /// </summary>
        public Vector2 Texture;
        /// <summary>
        /// Tangent
        /// </summary>
        public Vector3 Tangent;

        /// <inheritdoc/>
        public readonly bool HasChannel(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return true;
            else if (channel == VertexDataChannels.Normal) return true;
            else if (channel == VertexDataChannels.Texture) return true;
            else if (channel == VertexDataChannels.Tangent) return true;
            else return false;
        }
        /// <inheritdoc/>
        public readonly T GetChannelValue<T>(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return (T)(object)Position;
            else if (channel == VertexDataChannels.Normal) return (T)(object)Normal;
            else if (channel == VertexDataChannels.Texture) return (T)(object)Texture;
            else if (channel == VertexDataChannels.Tangent) return (T)(object)Tangent;
            else throw new EngineException($"Channel data not found: {channel}");
        }
        /// <inheritdoc/>
        public void SetChannelValue<T>(VertexDataChannels channel, T value)
        {
            if (channel == VertexDataChannels.Position) Position = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Normal) Normal = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Texture) Texture = (Vector2)(object)value;
            else if (channel == VertexDataChannels.Tangent) Tangent = (Vector3)(object)value;
            else throw new EngineException($"Channel data not found: {channel}");
        }

        /// <inheritdoc/>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(VertexPositionNormalTextureTangent));
        }
        /// <inheritdoc/>
        public readonly EngineInputElement[] GetInput(int slot)
        {
            return Input(slot);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Position: {Position}; Normal: {Normal}; Texture: {Texture}; Tangent: {Tangent};";
        }
    }
}
