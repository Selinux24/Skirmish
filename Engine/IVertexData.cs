
namespace Engine
{
    using Engine.Common;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Vertex data
    /// </summary>
    public interface IVertexData : IBufferData
    {
        /// <summary>
        /// Vertex type
        /// </summary>
        VertexTypes VertexType { get; }
        /// <summary>
        /// Gets if structure contains data for the specified channel
        /// </summary>
        /// <param name="channel">Data channel</param>
        /// <returns>Returns true if structure contains data for the specified channel</returns>
        bool HasChannel(VertexDataChannels channel);
        /// <summary>
        /// Gets data channel value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Data channel</param>
        /// <returns>Returns data for the specified channel</returns>
        T GetChannelValue<T>(VertexDataChannels channel);
        /// <summary>
        /// Sets the channer value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Channel</param>
        /// <param name="value">Value</param>
        void SetChannelValue<T>(VertexDataChannels channel, T value);
        /// <summary>
        /// Get input elements
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <returns>Returns input elements</returns>
        InputElement[] GetInput(int slot);
    }
}
