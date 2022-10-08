using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

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
        public static InputElement[] Input(int slot)
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, slot, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 12, slot, InputClassification.PerVertexData, 0),
                new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 20, slot, InputClassification.PerVertexData, 0),
            };
        }
        /// <summary>
        /// Generates a vertex array from specified components
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="uvs">Uv texture coordinates</param>
        /// <param name="color">Color</param>
        /// <returns>Returns the new generated vertex array</returns>
        public static IEnumerable<VertexFont> Generate(IEnumerable<Vector3> vertices, IEnumerable<Vector2> uvs, Color4 color)
        {
            if (vertices.Count() != uvs.Count()) throw new ArgumentException("Vertices and uvs must have the same length");

            var vArray = vertices.ToArray();
            var uvArray = uvs.ToArray();

            List<VertexFont> res = new List<VertexFont>();

            for (int i = 0; i < vArray.Length; i++)
            {
                res.Add(new VertexFont() { Position = vArray[i], Texture = uvArray[i], Color = color });
            }

            return res.ToArray();
        }
        /// <summary>
        /// Generates a vertex array from specified components
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="uvs">Uv texture coordinates</param>
        /// <param name="colors">Colors</param>
        /// <returns>Returns the new generated vertex array</returns>
        public static IEnumerable<VertexFont> Generate(IEnumerable<Vector3> vertices, IEnumerable<Vector2> uvs, IEnumerable<Color4> colors)
        {
            if (vertices.Count() != uvs.Count()) throw new ArgumentException("Vertices and uvs must have the same length");
            if (vertices.Count() != colors.Count()) throw new ArgumentException("Vertices and colors must have the same length");

            var vArray = vertices.ToArray();
            var uvArray = uvs.ToArray();
            var cArray = colors.ToArray();

            List<VertexFont> res = new List<VertexFont>();

            for (int i = 0; i < vArray.Length; i++)
            {
                res.Add(new VertexFont() { Position = vArray[i], Texture = uvArray[i], Color = cArray[i] });
            }

            return res.ToArray();
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
        public Color4 Color;
        /// <summary>
        /// Vertex type
        /// </summary>
        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.Font;
            }
        }

        /// <summary>
        /// Gets if structure contains data for the specified channel
        /// </summary>
        /// <param name="channel">Data channel</param>
        /// <returns>Returns true if structure contains data for the specified channel</returns>
        public bool HasChannel(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return true;
            else if (channel == VertexDataChannels.Texture) return true;
            else if (channel == VertexDataChannels.Color) return true;
            else return false;
        }
        /// <summary>
        /// Gets data channel value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Data channel</param>
        /// <returns>Returns data for the specified channel</returns>
        public T GetChannelValue<T>(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return (T)(object)Position;
            else if (channel == VertexDataChannels.Texture) return (T)(object)Texture;
            else if (channel == VertexDataChannels.Color) return (T)(object)Color;
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
            else if (channel == VertexDataChannels.Color) Color = (Color4)(object)value;
            else throw new EngineException($"Channel data not found: {channel}");
        }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(VertexFont));
        }
        /// <summary>
        /// Get input elements
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <returns>Returns input elements</returns>
        public InputElement[] GetInput(int slot)
        {
            return Input(slot);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Position: {Position}; Texture: {Texture}; Color: {Color};";
        }
    };
}
