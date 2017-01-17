using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Runtime.InteropServices;

namespace Engine.Common
{
    /// <summary>
    /// Position vertex format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPosition : IVertexData
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
            };
        }
        /// <summary>
        /// Generates a vertex array from specified components
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <returns>Returns the new generated vertex array</returns>
        public static VertexPosition[] Generate(Vector3[] vertices)
        {
            VertexPosition[] res = new VertexPosition[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                res[i] = new VertexPosition() { Position = vertices[i] };
            }

            return res;
        }

        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Vertex type
        /// </summary>
        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.Position;
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
            if (channel == VertexDataChannels.Position) return this.Position.Cast<T>();
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
            if (channel == VertexDataChannels.Position) this.Position = value.Cast<Vector3>();
            else throw new Exception(string.Format("Channel data not found: {0}", channel));
        }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(VertexPosition));
        }

        /// <summary>
        /// Text representation of vertex
        /// </summary>
        /// <returns>Returns the text representation of vertex</returns>
        public override string ToString()
        {
            return string.Format("Position: {0}", this.Position);
        }
    };
}
