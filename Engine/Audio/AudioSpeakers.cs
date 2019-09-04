using SharpDX.Multimedia;

namespace Engine.Audio
{
    /// <summary>
    /// Speakers configuration from KSAUDIO_CHANNEL_CONFIG
    /// </summary>
    enum AudioSpeakers
    {
        /// <summary>
        /// Mono
        /// </summary>
        Mono = Speakers.FrontCenter,
        /// <summary>
        /// Stereo
        /// </summary>
        Stereo = Speakers.FrontLeft | Speakers.FrontRight,
        /// <summary>
        /// Quad
        /// </summary>
        Quad = Speakers.FrontLeft | Speakers.FrontRight | Speakers.BackLeft | Speakers.BackRight,
        /// <summary>
        /// Surround
        /// </summary>
        Surround = Speakers.FrontLeft | Speakers.FrontRight | Speakers.FrontCenter | Speakers.BackCenter,
        /// <summary>
        /// 5.1
        /// </summary>
        FivePointOne = Speakers.FrontLeft | Speakers.FrontRight | Speakers.FrontCenter | Speakers.LowFrequency | Speakers.BackLeft | Speakers.BackRight,
        /// <summary>
        /// 5.1 surround
        /// </summary>
        FivePointOneSurround = Speakers.FrontLeft | Speakers.FrontRight | Speakers.FrontCenter | Speakers.LowFrequency | Speakers.SideLeft | Speakers.SideRight,
        /// <summary>
        /// 7.1
        /// </summary>
        SevenPointOne = Speakers.FrontLeft | Speakers.FrontRight | Speakers.FrontCenter | Speakers.LowFrequency | Speakers.BackLeft | Speakers.BackRight | Speakers.FrontLeftOfCenter | Speakers.FrontRightOfCenter,
        /// <summary>
        /// 7.1 surround
        /// </summary>
        SevenPointOneSurround = Speakers.FrontLeft | Speakers.FrontRight | Speakers.FrontCenter | Speakers.LowFrequency | Speakers.BackLeft | Speakers.BackRight | Speakers.SideLeft | Speakers.SideRight,
    }
}
