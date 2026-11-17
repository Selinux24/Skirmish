using Engine.Common;
using Engine.Mathematics;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Format
{
    using Format = Vortice.DXGI.Format;

    /// <summary>
    /// Position normal texture vertex format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalTexture : IVertexData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <returns>Returns input elements</returns>
        public static EngineInputElement[] Input(uint slot)
        {
            return
            [
                new ("POSITION", 0, Format.R32G32B32_Float, 0, slot, EngineInputClassification.PerVertexData, 0),
                new ("NORMAL", 0, Format.R32G32B32_Float, 12, slot, EngineInputClassification.PerVertexData, 0),
                new ("TEXCOORD", 0, Format.R32G32_Float, 24, slot, EngineInputClassification.PerVertexData, 0),
            ];
        }
        /// <summary>
        /// Generates a vertex array from specified components
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="normals">Normals</param>
        /// <param name="uvs">UV coordinates</param>
        /// <returns>Returns the new generated vertex array</returns>
        public static VertexPositionNormalTexture[] Generate(Vector3[] vertices, Vector3[] normals, Vector2[] uvs)
        {
            ArgumentNullException.ThrowIfNull(vertices);
            ArgumentNullException.ThrowIfNull(normals);
            ArgumentNullException.ThrowIfNull(uvs);

            if (vertices.Length != uvs.Length) throw new ArgumentException("Vertices and uvs must have the same length");
            if (vertices.Length != normals.Length) throw new ArgumentException("Vertices and normals must have the same length");

            VertexPositionNormalTexture[] res = new VertexPositionNormalTexture[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                res[i] = new VertexPositionNormalTexture() { Position = vertices[i], Normal = normals[i], Texture = uvs[i] };
            }

            return res;
        }
        /// <summary>
        /// Converts a vertex data list to a vertex array
        /// </summary>
        /// <param name="vertices">Vertices list</param>
        public static async Task<VertexPositionNormalTexture[]> Convert(VertexData[] vertices)
        {
            ArgumentNullException.ThrowIfNull(vertices);

            var res = new VertexPositionNormalTexture[vertices.Length];

            Parallel.For(0, vertices.Length, (index) =>
            {
                var v = vertices[index];

                res[index] = new VertexPositionNormalTexture
                {
                    Position = v.Position ?? Vector3.Zero,
                    Normal = v.Normal ?? Vector3.Zero,
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
        /// Normal
        /// </summary>
        public Vector3 Normal;
        /// <summary>
        /// Texture UV
        /// </summary>
        public Vector2 Texture;

        /// <inheritdoc/>
        public readonly bool HasChannel(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return true;
            else if (channel == VertexDataChannels.Normal) return true;
            else if (channel == VertexDataChannels.Texture) return true;
            else return false;
        }
        /// <inheritdoc/>
        public readonly T GetChannelValue<T>(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return (T)(object)Position;
            else if (channel == VertexDataChannels.Normal) return (T)(object)Normal;
            else if (channel == VertexDataChannels.Texture) return (T)(object)Texture;
            else throw new EngineException($"Channel data not found: {channel}");
        }
        /// <inheritdoc/>
        public void SetChannelValue<T>(VertexDataChannels channel, T value)
        {
            if (channel == VertexDataChannels.Position) Position = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Normal) Normal = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Texture) Texture = (Vector2)(object)value;
            else throw new EngineException($"Channel data not found: {channel}");
        }

        /// <inheritdoc/>
        public readonly uint GetStride()
        {
            return (uint)Marshal.SizeOf<VertexPositionNormalTexture>();
        }
        /// <inheritdoc/>
        public readonly EngineInputElement[] GetInput(uint slot)
        {
            return Input(slot);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Position: {Position}; Normal: {Normal}; Texture: {Texture};";
        }
    }
}
