using Engine.Audio;

namespace Engine
{
    /// <summary>
    /// Game audio listener interface
    /// </summary>
    public interface IGameAudioListener : IGameAudioAgent
    {
        /// <summary>
        /// Cone
        /// </summary>
        GameAudioConeDescription? Cone { get; set; }
    }
}
