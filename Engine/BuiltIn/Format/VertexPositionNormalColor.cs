using Engine.Common;
using Engine.Mathematics;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Format
{
    using Format = Vortice.DXGI.Format;

    /// <summary>
    /// Position normal color vertex format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalColor : IVertexData
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
                new ("COLOR", 0, Format.R32G32B32A32_Float, 24, slot, EngineInputClassification.PerVertexData, 0),
            ];
        }
        /// <summary>
        /// Generates a vertex array from specified components
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="normals">Normals</param>
        /// <param name="color">Color for all vertices</param>
        /// <returns>Returns the new generated vertex array</returns>
        public static VertexPositionNormalColor[] Generate(Vector3[] vertices, Vector3[] normals, Color4 color)
        {
            ArgumentNullException.ThrowIfNull(vertices);
            ArgumentNullException.ThrowIfNull(normals);
            if (vertices.Length != normals.Length) throw new ArgumentException("Vertices and normals must have the same length");

            VertexPositionNormalColor[] res = new VertexPositionNormalColor[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                res[i] = new VertexPositionNormalColor() { Position = vertices[i], Normal = normals[i], Color = color };
            }

            return res;
        }
        /// <summary>
        /// Generates a vertex array from specified components
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="normals">Normals</param>
        /// <param name="colors">Colors</param>
        /// <returns>Returns the new generated vertex array</returns>
        public static VertexPositionNormalColor[] Generate(Vector3[] vertices, Vector3[] normals, Color4[] colors)
        {
            ArgumentNullException.ThrowIfNull(vertices);
            ArgumentNullException.ThrowIfNull(normals);
            ArgumentNullException.ThrowIfNull(colors);
            if (vertices.Length != colors.Length) throw new ArgumentException("Vertices and colors must have the same length");
            if (vertices.Length != normals.Length) throw new ArgumentException("Vertices and normals must have the same length");

            VertexPositionNormalColor[] res = new VertexPositionNormalColor[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                res[i] = new VertexPositionNormalColor() { Position = vertices[i], Normal = normals[i], Color = colors[i] };
            }

            return res;
        }
        /// <summary>
        /// Converts a vertex data list to a vertex array
        /// </summary>
        /// <param name="vertices">Vertices list</param>
        public static async Task<VertexPositionNormalColor[]> Convert(VertexData[] vertices)
        {
            ArgumentNullException.ThrowIfNull(vertices);

            var res = new VertexPositionNormalColor[vertices.Length];

            Parallel.For(0, vertices.Length, (index) =>
            {
                var v = vertices[index];

                res[index] = new VertexPositionNormalColor
                {
                    Position = v.Position ?? Vector3.Zero,
                    Normal = v.Normal ?? Vector3.Zero,
                    Color = v.Color ?? Color4.White,
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
        /// Color
        /// </summary>
        public Color4 Color;

        /// <inheritdoc/>
        public readonly bool HasChannel(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return true;
            else if (channel == VertexDataChannels.Normal) return true;
            else if (channel == VertexDataChannels.Color) return true;
            else return false;
        }
        /// <inheritdoc/>
        public readonly T GetChannelValue<T>(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return (T)(object)Position;
            else if (channel == VertexDataChannels.Normal) return (T)(object)Normal;
            else if (channel == VertexDataChannels.Color) return (T)(object)Color;
            else throw new EngineException($"Channel data not found: {channel}");
        }
        /// <inheritdoc/>
        public void SetChannelValue<T>(VertexDataChannels channel, T value)
        {
            if (channel == VertexDataChannels.Position) Position = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Normal) Normal = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Color) Color = (Color4)(object)value;
            else throw new EngineException($"Channel data not found: {channel}");
        }

        /// <inheritdoc/>
        public readonly uint GetStride()
        {
            return (uint)Marshal.SizeOf<VertexPositionNormalColor>();
        }
        /// <inheritdoc/>
        public readonly EngineInputElement[] GetInput(uint slot)
        {
            return Input(slot);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Position: {Position} Normal: {Normal}; Color: {Color};";
        }
    }
}
