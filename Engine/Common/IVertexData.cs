using SharpDX;

namespace Engine.Common
{
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
        T GetChannelValue<T>(VertexDataChannels channel) where T : struct;
        /// <summary>
        /// Sets the channer value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Channel</param>
        /// <param name="value">Value</param>
        void SetChannelValue<T>(VertexDataChannels channel, T value) where T : struct;
    }
}
