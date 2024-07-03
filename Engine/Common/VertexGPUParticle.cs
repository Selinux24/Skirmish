using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Particle data buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexGpuParticle : IVertexData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        /// <returns>Returns input elements</returns>
        public static InputElement[] Input(int slot)
        {
            return
            [
                new ("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, slot, InputClassification.PerVertexData, 0),
                new ("VELOCITY", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, slot, InputClassification.PerVertexData, 0),
                new ("RANDOM", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 24, slot, InputClassification.PerVertexData, 0),
                new ("MAX_AGE", 0, SharpDX.DXGI.Format.R32_Float, 40, slot, InputClassification.PerVertexData, 0),

                new ("TYPE", 0, SharpDX.DXGI.Format.R32_UInt, 44, slot, InputClassification.PerVertexData, 0),
                new ("EMISSION_TIME", 0, SharpDX.DXGI.Format.R32_Float, 48, slot, InputClassification.PerVertexData, 0),
            ];
        }

        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Velocity
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
        /// <summary>
        /// Particle type
        /// </summary>
        public uint Type;
        /// <summary>
        /// Total emission time
        /// </summary>
        public float EmissionTime;
        /// <summary>
        /// Vertex type
        /// </summary>
        public readonly VertexTypes VertexType
        {
            get
            {
                return VertexTypes.GPUParticle;
            }
        }

        /// <summary>
        /// Gets if structure contains data for the specified channel
        /// </summary>
        /// <param name="channel">Data channel</param>
        /// <returns>Returns true if structure contains data for the specified channel</returns>
        public readonly bool HasChannel(VertexDataChannels channel)
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
        public readonly T GetChannelValue<T>(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return (T)(object)Position;
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
            else throw new EngineException($"Channel data not found: {channel}");
        }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(VertexGpuParticle));
        }
        /// <summary>
        /// Get input elements
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <returns>Returns input elements</returns>
        public readonly InputElement[] GetInput(int slot)
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
