using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Format
{
    using Engine.Common;
    using Engine.Mathematics;

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
                new ("POSITION", 0, Vortice.DXGI.Format.R32G32B32_Float, 0, slot, EngineInputClassification.PerVertexData, 0),
                new ("NORMAL", 0, Vortice.DXGI.Format.R32G32B32_Float, 12, slot, EngineInputClassification.PerVertexData, 0),
                new ("COLOR", 0, Vortice.DXGI.Format.R32G32B32A32_Float, 24, slot, EngineInputClassification.PerVertexData, 0),
            ];
        }
        /// <summary>
        /// Generates a vertex array from specified components
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="normals">Normals</param>
        /// <param name="color">Color for all vertices</param>
        /// <returns>Returns the new generated vertex array</returns>
        public static IEnumerable<VertexPositionNormalColor> Generate(IEnumerable<Vector3> vertices, IEnumerable<Vector3> normals, Vector4 color)
        {
            if (vertices.Count() != normals.Count()) throw new ArgumentException("Vertices and normals must have the same length");

            var vArray = vertices.ToArray();
            var nArray = normals.ToArray();

            VertexPositionNormalColor[] res = new VertexPositionNormalColor[vArray.Length];

            for (int i = 0; i < vArray.Length; i++)
            {
                res[i] = new VertexPositionNormalColor() { Position = vArray[i], Normal = nArray[i], Color = color };
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
        public static IEnumerable<VertexPositionNormalColor> Generate(IEnumerable<Vector3> vertices, IEnumerable<Vector3> normals, IEnumerable<Vector4> colors)
        {
            if (vertices.Count() != colors.Count()) throw new ArgumentException("Vertices and colors must have the same length");
            if (vertices.Count() != normals.Count()) throw new ArgumentException("Vertices and normals must have the same length");

            var vArray = vertices.ToArray();
            var nArray = normals.ToArray();
            var cArray = colors.ToArray();

            VertexPositionNormalColor[] res = new VertexPositionNormalColor[vArray.Length];

            for (int i = 0; i < vArray.Length; i++)
            {
                res[i] = new VertexPositionNormalColor() { Position = vArray[i], Normal = nArray[i], Color = cArray[i] };
            }

            return res;
        }
        /// <summary>
        /// Converts a vertex data list to a vertex array
        /// </summary>
        /// <param name="vertices">Vertices list</param>
        public static async Task<IEnumerable<VertexPositionNormalColor>> Convert(IEnumerable<VertexData> vertices)
        {
            var vArray = vertices.ToArray();

            var res = new VertexPositionNormalColor[vArray.Length];

            Parallel.For(0, vArray.Length, (index) =>
            {
                var v = vArray[index];

                res[index] = new VertexPositionNormalColor
                {
                    Position = v.Position ?? Vector3.Zero,
                    Normal = v.Normal ?? Vector3.Zero,
                    Color = v.Color ?? new(1, 1, 1, 1),
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
        public Vector4 Color;

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
            else if (channel == VertexDataChannels.Color) Color = (Vector4)(object)value;
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
