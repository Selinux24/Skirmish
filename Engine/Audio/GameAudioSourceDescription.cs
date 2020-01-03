using SharpDX;

namespace Engine.Audio
{
    /// <summary>
    /// Audio source description
    /// </summary>
    public struct GameAudioSourceDescription
    {
        /// <summary>
        /// Default human listener cone
        /// </summary>
        public static GameAudioConeDescription DefaultListenerCone
        {
            get
            {
                return new GameAudioConeDescription()
                {
                    InnerAngle = MathUtil.Pi * 5.0f / 6.0f,
                    OuterAngle = MathUtil.Pi * 11.0f / 6.0f,
                    InnerVolume = 1.0f,
                    OuterVolume = 0.75f,
                    InnerLpf = 0.0f,
                    OuterLpf = 0.25f,
                    InnerReverb = 0.708f,
                    OuterReverb = 1.0f
                };
            }
        }

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
