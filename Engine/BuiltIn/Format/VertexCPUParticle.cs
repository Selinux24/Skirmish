using Engine.Common;
using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Format
{
    /// <summary>
    /// Particle data buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexCpuParticle : IVertexData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        /// <param name="slot">Slot</param>
        public static EngineInputElement[] Input(int slot)
        {
            return
            [
                new ("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, slot, EngineInputClassification.PerVertexData, 0),
                new ("VELOCITY", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, slot, EngineInputClassification.PerVertexData, 0),
                new ("RANDOM", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 24, slot, EngineInputClassification.PerVertexData, 0),
                new ("MAX_AGE", 0, SharpDX.DXGI.Format.R32_Float, 40, slot, EngineInputClassification.PerVertexData, 0),
            ];
        }

        /// <summary>
        /// Initial position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Initial velocity
        /// </summary>
        public Vector3 Velocity;
        /// <summary>
        /// Particle random values
        /// </summary>
        public Vector4 RandomValues;
        /// <summary>
        /// Particle maximum age
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
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(VertexCpuParticle));
        }
        /// <inheritdoc/>
        public readonly EngineInputElement[] GetInput(int slot)
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
