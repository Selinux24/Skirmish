using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Format
{
    using Engine.Common;
    using Engine.Mathematics;

    /// <summary>
    /// Font format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexFont : IVertexData
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
                new("POSITION", 0, Vortice.DXGI.Format.R32G32B32_Float, 0, slot, EngineInputClassification.PerVertexData, 0),
                new("TEXCOORD", 0, Vortice.DXGI.Format.R32G32_Float, 12, slot, EngineInputClassification.PerVertexData, 0),
                new("COLOR", 0, Vortice.DXGI.Format.R32G32B32A32_Float, 20, slot, EngineInputClassification.PerVertexData, 0),
            ];
        }
        /// <summary>
        /// Generates a vertex array from specified components
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="uvs">Uv texture coordinates</param>
        /// <param name="color">Color</param>
        /// <returns>Returns the new generated vertex array</returns>
        public static IEnumerable<VertexFont> Generate(IEnumerable<Vector3> vertices, IEnumerable<Vector2> uvs, Vector4 color)
        {
            if (!vertices.Any())
            {
                return [];
            }

            if (vertices.Count() != uvs.Count()) throw new ArgumentException("Vertices and uvs must have the same length");

            return [.. vertices.Select((v, index) => new VertexFont() { Position = v, Texture = uvs.ElementAt(index), Color = color })];
        }
        /// <summary>
        /// Generates a vertex array from specified components
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="uvs">Uv texture coordinates</param>
        /// <param name="colors">Colors</param>
        /// <returns>Returns the new generated vertex array</returns>
        public static IEnumerable<VertexFont> Generate(IEnumerable<Vector3> vertices, IEnumerable<Vector2> uvs, IEnumerable<Vector4> colors)
        {
            if (!vertices.Any())
            {
                return [];
            }

            if (vertices.Count() != uvs.Count()) throw new ArgumentException("Vertices and uvs must have the same length");
            if (vertices.Count() != colors.Count()) throw new ArgumentException("Vertices and colors must have the same length");

            return [.. vertices.Select((v, index) => new VertexFont() { Position = v, Texture = uvs.ElementAt(index), Color = colors.ElementAt(index) })];
        }

        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Texture UV
        /// </summary>
        public Vector2 Texture;
        /// <summary>
        /// Color
        /// </summary>
        public Vector4 Color;

        /// <inheritdoc/>
        public readonly bool HasChannel(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return true;
            else if (channel == VertexDataChannels.Texture) return true;
            else if (channel == VertexDataChannels.Color) return true;
            else return false;
        }
        /// <inheritdoc/>
        public readonly T GetChannelValue<T>(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return (T)(object)Position;
            else if (channel == VertexDataChannels.Texture) return (T)(object)Texture;
            else if (channel == VertexDataChannels.Color) return (T)(object)Color;
            else throw new EngineException($"Channel data not found: {channel}");
        }
        /// <inheritdoc/>
        public void SetChannelValue<T>(VertexDataChannels channel, T value)
        {
            if (channel == VertexDataChannels.Position) Position = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Texture) Texture = (Vector2)(object)value;
            else if (channel == VertexDataChannels.Color) Color = (Vector4)(object)value;
            else throw new EngineException($"Channel data not found: {channel}");
        }

        /// <inheritdoc/>
        public readonly uint GetStride()
        {
            return (uint)Marshal.SizeOf<VertexFont>();
        }
        /// <inheritdoc/>
        public readonly EngineInputElement[] GetInput(uint slot)
        {
            return Input(slot);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Position: {Position}; Texture: {Texture}; Color: {Color};";
        }
    }
}
