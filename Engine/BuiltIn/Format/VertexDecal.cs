using Engine.Common;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Format
{
    using Engine.Mathematics;

    /// <summary>
    /// Decal data buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexDecal : IVertexData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        /// <param name="slot">Slot</param>
        public static EngineInputElement[] Input(uint slot)
        {
            return
            [
                new ("POSITION", 0, Vortice.DXGI.Format.R32G32B32_Float, 0, slot, EngineInputClassification.PerVertexData, 0),
                new ("NORMAL", 0, Vortice.DXGI.Format.R32G32B32_Float, 12, slot, EngineInputClassification.PerVertexData, 0),
                new ("SIZE", 0, Vortice.DXGI.Format.R32G32_Float, 24, slot, EngineInputClassification.PerVertexData, 0),
                new ("START_TIME", 0, Vortice.DXGI.Format.R32_Float, 32, slot, EngineInputClassification.PerVertexData, 0),
                new ("MAX_AGE", 0, Vortice.DXGI.Format.R32_Float, 36, slot, EngineInputClassification.PerVertexData, 0),
            ];
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
        /// Size
        /// </summary>
        public Vector2 Size;
        /// <summary>
        /// Start time
        /// </summary>
        public float StartTime;
        /// <summary>
        /// Maximum age
        /// </summary>
        public float MaxAge;

        /// <inheritdoc/>
        public readonly bool HasChannel(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return true;
            else return false;
        }
        /// <inheritdoc/>
        public readonly T GetChannelValue<T>(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return (T)(object)Position;
            else throw new EngineException($"Channel data not found: {channel}");
        }
        /// <inheritdoc/>
        public void SetChannelValue<T>(VertexDataChannels channel, T value)
        {
            if (channel == VertexDataChannels.Position) Position = (Vector3)(object)value;
            else throw new EngineException($"Channel data not found: {channel}");
        }

        /// <inheritdoc/>
        public readonly uint GetStride()
        {
            return (uint)Marshal.SizeOf<VertexDecal>();
        }
        /// <inheritdoc/>
        public readonly EngineInputElement[] GetInput(uint slot)
        {
            return Input(slot);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Position: {Position};";
        }
    }
}
