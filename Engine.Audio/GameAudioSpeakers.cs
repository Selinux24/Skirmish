using SharpDX.Multimedia;

namespace Engine.Audio
{
    /// <summary>
    /// Speakers configuration from KSAUDIO_CHANNEL_CONFIG
    /// </summary>
    enum GameAudioSpeakers
    {
        /// <summary>
        /// None
        /// </summary>
        None = Speakers.None,
        /// <summary>
        /// Front left
        /// </summary>
        FrontLeft = Speakers.FrontLeft,
        /// <summary>
        /// Front right
        /// </summary>
        FrontRight = Speakers.FrontRight,
        /// <summary>
        /// Front center
        /// </summary>
        FrontCenter = Speakers.FrontCenter,
        /// <summary>
        /// Low frequency
        /// </summary>
        LowFrequency = Speakers.LowFrequency,
        /// <summary>
        /// Back left
        /// </summary>
        BackLeft = Speakers.BackLeft,
        /// <summary>
        /// Back right
        /// </summary>
        BackRight = Speakers.BackRight,
        /// <summary>
        /// Front left of center
        /// </summary>
        FrontLeftOfCenter = Speakers.FrontLeftOfCenter,
        /// <summary>
        /// Front right of center
        /// </summary>
        FrontRightOfCenter = Speakers.FrontRightOfCenter,
        /// <summary>
        /// Back center
        /// </summary>
        BackCenter = Speakers.BackCenter,
        /// <summary>
        /// Side left
        /// </summary>
        SideLeft = Speakers.SideLeft,
        /// <summary>
        /// Side right
        /// </summary>
        SideRight = Speakers.SideRight,
        /// <summary>
        /// Top center
        /// </summary>
        TopCenter = Speakers.TopCenter,
        /// <summary>
        /// Top front left
        /// </summary>
        TopFrontLeft = Speakers.TopFrontLeft,
        /// <summary>
        /// Top front center
        /// </summary>
        TopFrontCenter = Speakers.TopFrontCenter,
        /// <summary>
        /// Top front right
        /// </summary>
        TopFrontRight = Speakers.TopFrontRight,
        /// <summary>
        /// Top back left
        /// </summary>
        TopBackLeft = Speakers.TopBackLeft,
        /// <summary>
        /// Top back center
        /// </summary>
        TopBackCenter = Speakers.TopBackCenter,
        /// <summary>
        /// Top back right
        /// </summary>
        TopBackRight = Speakers.TopBackRight,
        /// <summary>
        /// Reserved
        /// </summary>
        Reserved = Speakers.Reserved,
        /// <summary>
        /// All
        /// </summary>
        All = Speakers.All,

#pragma warning disable CA1069
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
#pragma warning restore CA1069
    }
}
