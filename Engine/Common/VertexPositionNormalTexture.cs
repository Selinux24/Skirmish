using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Runtime.InteropServices;

namespace Engine.Common
{
    /// <summary>
    /// Position normal texture vertex format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalTexture : IVertexData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        /// <returns></returns>
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 24, 0, InputClassification.PerVertexData, 0),
            };
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
        /// Vertex type
        /// </summary>
        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.PositionNormalTexture;
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
            else if (channel == VertexDataChannels.Normal) return true;
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
            else if (channel == VertexDataChannels.Normal) return (T)(object)this.Normal;
            else if (channel == VertexDataChannels.Texture) return (T)(object)this.Texture;
            else throw new Exception(string.Format("Channel data not found: {0}", channel));
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
            else if (channel == VertexDataChannels.Normal) this.Normal = (Vector3)(object)value;
            else if (channel == VertexDataChannels.Texture) this.Texture = (Vector2)(object)value;
            else throw new Exception(string.Format("Channel data not found: {0}", channel));
        }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(VertexPositionNormalTexture));
        }
        /// <summary>
        /// Get input elements
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <returns>Returns input elements</returns>
        public InputElement[] GetInput(int slot)
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, slot, InputClassification.PerVertexData, 0),
                new InputElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, slot, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 24, slot, InputClassification.PerVertexData, 0),
            };
        }

        /// <summary>
        /// Text representation of vertex
        /// </summary>
        /// <returns>Returns the text representation of vertex</returns>
        public override string ToString()
        {
            return string.Format("Position: {0}; Normal: {1}; Texture: {2}", this.Position, this.Normal, this.Texture);
        }
    };
}
