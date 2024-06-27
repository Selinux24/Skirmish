
namespace Engine.Audio
{
    /// <summary>
    /// Defines a DSP setting at a given normalized distance.
    /// </summary>
    public struct GameAudioCurvePoint
    {
        /// <summary>
        /// Default linear curve
        /// </summary>
        public static readonly GameAudioCurvePoint[] DefaultLinearCurve = new GameAudioCurvePoint[]
        {
            new GameAudioCurvePoint(){ Distance = 0.0f, DspSetting = 1.0f, },
            new GameAudioCurvePoint(){ Distance = 1.0f, DspSetting = 0.0f, },
        };
        /// <summary>
        /// Default emitter lfe curve
        /// </summary>
        public static readonly GameAudioCurvePoint[] DefaultLfeCurve = new GameAudioCurvePoint[]
        {
            new GameAudioCurvePoint(){ Distance = 0.0f, DspSetting = 1.0f, },
            new GameAudioCurvePoint(){ Distance = 0.25f, DspSetting = 0.0f, },
            new GameAudioCurvePoint(){ Distance = 1.0f, DspSetting = 0.0f, },
        };
        /// <summary>
        /// Default emitter reverb curve
        /// </summary>
        public static readonly GameAudioCurvePoint[] DefaultReverbCurve = new GameAudioCurvePoint[]
        {
            new GameAudioCurvePoint(){ Distance = 0.0f, DspSetting = 0.5f, },
            new GameAudioCurvePoint(){ Distance = 0.75f, DspSetting = 1.0f, },
            new GameAudioCurvePoint(){ Distance = 1.0f, DspSetting = 0.0f, },
        };

        /// <summary>
        /// Normalized distance. This must be within 0.0f to 1.0f.
        /// </summary>
        public float Distance { get; set; }
        /// <summary>
        /// DSP control setting.
        /// </summary>
        public float DspSetting { get; set; }
    }
}
