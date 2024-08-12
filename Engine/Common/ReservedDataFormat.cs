using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.Common
{
    /// <summary>
    /// Reserved format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ReservedDataFormat : IVertexData
    {
        /// <summary>
        /// Data
        /// </summary>
        public Vector4 Data;

        /// <inheritdoc/>
        public readonly bool HasChannel(VertexDataChannels channel)
        {
            return false;
        }
        /// <inheritdoc/>
        public readonly T GetChannelValue<T>(VertexDataChannels channel)
        {
            throw new EngineException($"Channel data not found: {channel}");
        }
        /// <inheritdoc/>
        public void SetChannelValue<T>(VertexDataChannels channel, T value)
        {
            throw new EngineException($"Channel data not found: {channel}");
        }

        /// <inheritdoc/>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(ReservedDataFormat));
        }
        /// <inheritdoc/>
        public readonly EngineInputElement[] GetInput(int slot)
        {
            return
            [
                new ("POSITION", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0, slot, EngineInputClassification.PerVertexData, 0),
            ];
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Data: {Data};";
        }
    }
}
