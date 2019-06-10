using SharpDX;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;
using SharpDX.XAudio2.Fx;
using MasteringLimiter = SharpDX.XAPO.Fx.MasteringLimiter;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio state
    /// </summary>
    public struct GameAudioState
    {
        /// <summary>
        /// Device
        /// </summary>
        public XAudio2 XAudio2 { get; set; }
        /// <summary>
        /// Mastering voice
        /// </summary>
        public MasteringVoice MasteringVoice { get; set; }
        /// <summary>
        /// Mastering limiter
        /// </summary>
        public MasteringLimiter MasteringLimiter { get; set; }
        /// <summary>
        /// Source voice
        /// </summary>
        public SourceVoice SourceVoice { get; set; }
        /// <summary>
        /// Reverb voice
        /// </summary>
        public SubmixVoice ReverbVoice { get; set; }
        /// <summary>
        /// Reverb effect
        /// </summary>
        public Reverb ReverbEffect { get; set; }
        /// <summary>
        /// 3D audio instance
        /// </summary>
        public X3DAudio X3DInstance { get; set; }
        /// <summary>
        /// Frame to apply 3d audio
        /// </summary>
        public int FrameToApply3DAudio { get; set; }
        /// <summary>
        /// Dsp settings from 3D audio calculate function
        /// </summary>
        public DspSettings DspSettings { get; set; }

        /// <summary>
        /// Speakers
        /// </summary>
        public Speakers Speakers { get; set; }
        /// <summary>
        /// Use LFE redirection
        /// </summary>
        public bool UseRedirectToLFE { get; set; }

        /// <summary>
        /// Listener
        /// </summary>
        public Listener Listener { get; set; }
        /// <summary>
        /// Listener current position
        /// </summary>
        public Vector3 ListenerPos { get; set; }
        /// <summary>
        /// Listener current orientation
        /// </summary>
        public Vector3 ListenerOrientation { get; set; }

        /// <summary>
        /// Emitter
        /// </summary>
        public Emitter Emitter { get; set; }
        /// <summary>
        /// Emitter current position
        /// </summary>
        public Vector3 EmitterPos { get; set; }
        /// <summary>
        /// Emitter current orientation
        /// </summary>
        public Vector3 EmitterOrientation { get; set; }
        /// <summary>
        /// Emitter azimuths array
        /// </summary>
        public float[] EmitterAzimuths { get; set; }
    }
}
