using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Runtime.InteropServices;

namespace Engine.Common
{
    /// <summary>
    /// Terrain vertex format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexTerrain : IVertexData
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
                new InputElement("TEXCOORD", 1, SharpDX.DXGI.Format.R32G32_Float, 32, 0, InputClassification.PerVertexData, 0),
                new InputElement("TANGENT", 0, SharpDX.DXGI.Format.R32G32B32_Float, 40, 0, InputClassification.PerVertexData, 0),
                new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 52, 0, InputClassification.PerVertexData, 0),
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
        /// Texture UV 0
        /// </summary>
        public Vector2 Texture0;
        /// <summary>
        /// Texture UV 1
        /// </summary>
        public Vector2 Texture1;
        /// <summary>
        /// Tangent
        /// </summary>
        public Vector3 Tangent;
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
                return VertexTypes.Terrain;
            }
        }
        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexTerrain));
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
            else if (channel == VertexDataChannels.Texture1) return true;
            else if (channel == VertexDataChannels.Tangent) return true;
            else if (channel == VertexDataChannels.Color) return true;
            else return false;
        }
        /// <summary>
        /// Gets data channel value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Data channel</param>
        /// <returns>Returns data for the specified channel</returns>
        public T GetChannelValue<T>(VertexDataChannels channel) where T : struct
        {
            if (channel == VertexDataChannels.Position) return this.Position.Cast<T>();
            else if (channel == VertexDataChannels.Normal) return this.Normal.Cast<T>();
            else if (channel == VertexDataChannels.Texture) return this.Texture0.Cast<T>();
            else if (channel == VertexDataChannels.Texture1) return this.Texture1.Cast<T>();
            else if (channel == VertexDataChannels.Tangent) return this.Tangent.Cast<T>();
            else if (channel == VertexDataChannels.Color) return this.Color.Cast<T>();
            else throw new Exception(string.Format("Channel data not found: {0}", channel));
        }
        /// <summary>
        /// Sets the channer value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Channel</param>
        /// <param name="value">Value</param>
        public void SetChannelValue<T>(VertexDataChannels channel, T value) where T : struct
        {
            if (channel == VertexDataChannels.Position) this.Position = value.Cast<Vector3>();
            else if (channel == VertexDataChannels.Normal) this.Normal = value.Cast<Vector3>();
            else if (channel == VertexDataChannels.Texture) this.Texture0 = value.Cast<Vector2>();
            else if (channel == VertexDataChannels.Texture1) this.Texture1 = value.Cast<Vector2>();
            else if (channel == VertexDataChannels.Tangent) this.Tangent = value.Cast<Vector3>();
            else if (channel == VertexDataChannels.Color) this.Color = value.Cast<Color4>();
            else throw new Exception(string.Format("Channel data not found: {0}", channel));
        }

        /// <summary>
        /// Text representation of vertex
        /// </summary>
        /// <returns>Returns the text representation of vertex</returns>
        public override string ToString()
        {
            return string.Format(
                "Position: {0}; Normal: {1}; Texture0: {2}; Texture1: {3}; Tangent: {4}", 
                this.Position, 
                this.Normal,
                this.Texture0, this.Texture1, 
                this.Tangent);
        }
    };
}
