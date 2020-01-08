
namespace Engine.Audio
{
    /// <summary>
    /// Game audio emitter interface
    /// </summary>
    public interface IGameAudioEmitter : IGameAudioAgent
    {
        /// <summary>
        /// Sound radius
        /// </summary>
        float Radius { get; set; }
        /// <summary>
        /// Cone
        /// </summary>
        GameAudioConeDescription? Cone { get; set; }
        /// <summary>
        /// Inner radius
        /// </summary>
        float InnerRadius { get; set; }
        /// <summary>
        /// Inner radius angle
        /// </summary>
        float InnerRadiusAngle { get; set; }
        /// <summary>
        /// Volume curve
        /// </summary>
        GameAudioCurvePoint[] VolumeCurve { get; set; }
        /// <summary>
        /// LFE curve
        /// </summary>
        GameAudioCurvePoint[] LfeCurve { get; set; }
        /// <summary>
        /// Reverb curve
        /// </summary>
        GameAudioCurvePoint[] ReverbCurve { get; set; }
    }
}
