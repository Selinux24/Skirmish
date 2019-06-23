
namespace Engine.Audio
{
    /// <summary>
    /// Audio source description
    /// </summary>
    public struct GameAudioSourceDescription
    {
        /// <summary>
        /// Sound radius
        /// </summary>
        public float Radius { get; set; }
        /// <summary>
        /// Specifies directionality for a single-channel non-LFE emitter by scaling DSP behavior with respect to the emitter's orientation.
        /// </summary>
        public GameAudioConeDescription? Cone { get; set; }
    }
}
