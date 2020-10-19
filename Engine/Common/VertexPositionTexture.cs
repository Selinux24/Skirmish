using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

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
        public static InputElement[] Input(int slot)
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, slot, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 12, slot, InputClassification.PerVertexData, 0),
            };
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

            List<VertexPositionTexture> res = new List<VertexPositionTexture>();

            for (int i = 0; i < vArray.Length; i++)
            {
                res.Add(new VertexPositionTexture() { Position = vArray[i], Texture = uvArray[i] });
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
        /// Vertex type
        /// </summary>
        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.PositionTexture;
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
            if (channel == VertexDataChannels.Position) return (T)(object)this.Position;
            else if (channel == VertexDataChannels.Texture) return (T)(object)this.Texture;
            else throw new EngineException(string.Format("Channel data not found: {0}", channel));
        }
        /// <summary>
        /// Sets the channer value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Channel</param>
        /// <param name="value">Value</param>
        public void SetChannelValue<T>(VertexDataChannels channel, T value)
        {
            if (channel == VertexDataChannels.Position) this.Position = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Texture) this.Texture = (Vector2)(object)value;
            else throw new EngineException(string.Format("Channel data not found: {0}", channel));
        }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(VertexPositionTexture));
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

        /// <summary>
        /// Text representation of vertex
        /// </summary>
        /// <returns>Returns the text representation of vertex</returns>
        public override string ToString()
        {
            return string.Format("Position: {0}; Texture: {1}", this.Position, this.Texture);
        }
    };
}
