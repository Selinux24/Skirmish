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
    /// Position texture format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionTexture : IVertexData
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
            ];
        }
        /// <summary>
        /// Generates a vertex array from specified components
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="uvs">Uv texture coordinates</param>
        /// <returns>Returns the new generated vertex array</returns>
        public static IEnumerable<VertexPositionTexture> Generate(IEnumerable<Vector3> vertices, IEnumerable<Vector2> uvs)
        {
            if (vertices.Count() != uvs.Count()) throw new ArgumentException("Vertices and uvs must have the same length");

            var vArray = vertices.ToArray();
            var uvArray = uvs.ToArray();

            var res = new List<VertexPositionTexture>();

            for (int i = 0; i < vArray.Length; i++)
            {
                res.Add(new VertexPositionTexture() { Position = vArray[i], Texture = uvArray[i] });
            }

            return res;
        }
        /// <summary>
        /// Converts a vertex data list to a vertex array
        /// </summary>
        /// <param name="vertices">Vertices list</param>
        public static async Task<IEnumerable<VertexPositionTexture>> Convert(IEnumerable<VertexData> vertices)
        {
            var vArray = vertices.ToArray();

            var res = new VertexPositionTexture[vArray.Length];

            Parallel.For(0, vArray.Length, (index) =>
            {
                var v = vArray[index];

                res[index] = new VertexPositionTexture
                {
                    Position = v.Position ?? Vector3.Zero,
                    Texture = v.Texture ?? Vector2.Zero
                };
            });

            return await Task.FromResult(res);
        }

        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Texture UV
        /// </summary>
        public Vector2 Texture;

        /// <inheritdoc/>
        public readonly bool HasChannel(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return true;
            else if (channel == VertexDataChannels.Texture) return true;
            else return false;
        }
        /// <inheritdoc/>
        public readonly T GetChannelValue<T>(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return (T)(object)Position;
            else if (channel == VertexDataChannels.Texture) return (T)(object)Texture;
            else throw new EngineException($"Channel data not found: {channel}");
        }
        /// <inheritdoc/>
        public void SetChannelValue<T>(VertexDataChannels channel, T value)
        {
            if (channel == VertexDataChannels.Position) Position = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Texture) Texture = (Vector2)(object)value;
            else throw new EngineException($"Channel data not found: {channel}");
        }

        /// <inheritdoc/>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(VertexPositionTexture));
        }
        /// <inheritdoc/>
        public readonly EngineInputElement[] GetInput(int slot)
        {
            return Input(slot);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Position: {Position}; Texture: {Texture};";
        }
    }
}
