
namespace Engine.Audio
{
    /// <summary>
    /// Audio cone description
    /// </summary>
    public struct GameAudioConeDescription
    {
        /// <summary>
        /// Inner cone angle in radians.
        /// </summary>
        /// <remarks>This value must be within 0.0f to 2PI.</remarks>
        public float InnerAngle { get; set; }
        /// <summary>
        /// Outer cone angle in radians.
        /// </summary>
        /// <remarks>This value must be within InnerAngle to 2PI.</remarks>
        public float OuterAngle { get; set; }
        /// <summary>
        /// Volume scaler on/within inner cone.
        /// </summary>
        /// <remarks>This value must be within 0.0f to 2.0f</remarks>
        public float InnerVolume { get; set; }
        /// <summary>
        /// Volume scaler on/beyond outer cone.
        /// </summary>
        /// <remarks>This value must be within 0.0f to 2.0f.</remarks>
        public float OuterVolume { get; set; }
        /// <summary>
        /// LPF direct-path or reverb-path coefficient scaler on/within inner cone.
        /// </summary>
        /// <remarks>This value is only used for LPF calculations and must be within 0.0f to 1.0f.</remarks>
        public float InnerLpf { get; set; }
        /// <summary>
        /// LPF direct-path or reverb-path coefficient scaler on or beyond outer cone.
        /// </summary>
        /// <remarks>This value is only used for LPF calculations and must be within 0.0f to 1.0f.</remarks>
        public float OuterLpf { get; set; }
        /// <summary>
        /// Reverb send level scaler on or within inner cone.
        /// </summary>
        /// <remarks>This must be within 0.0f to 2.0f.</remarks>
        public float InnerReverb { get; set; }
        /// <summary>
        /// Reverb send level scaler on/beyond outer cone.
        /// </summary>
        /// <remarks>This must be within 0.0f to 2.0f.</remarks>
        public float OuterReverb { get; set; }
    }
}
