
namespace Engine.Audio
{
    /// <summary>
    /// Game audio emitter
    /// </summary>
    class GameAudioEmitter : GameAudioAgent, IGameAudioEmitter
    {
        /// <summary>
        /// Sound radius
        /// </summary>
        public float Radius { get; set; }
        /// <summary>
        /// Cone
        /// </summary>
        public GameAudioConeDescription? Cone { get; set; }
        /// <summary>
        /// Inner radius
        /// </summary>
        public float InnerRadius { get; set; }
        /// <summary>
        /// Inner radius angle
        /// </summary>
        public float InnerRadiusAngle { get; set; }
        /// <summary>
        /// Volume curve
        /// </summary>
        public GameAudioCurvePoint[] VolumeCurve { get; set; }
        /// <summary>
        /// LFE curve
        /// </summary>
        public GameAudioCurvePoint[] LfeCurve { get; set; }
        /// <summary>
        /// Reverb curve
        /// </summary>
        public GameAudioCurvePoint[] ReverbCurve { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GameAudioEmitter() : base()
        {

        }
    }
}
