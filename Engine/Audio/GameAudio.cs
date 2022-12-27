using SharpDX;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;
using SharpDX.XAudio2.Fx;
using System;
using MasteringLimiter = SharpDX.XAPO.Fx.MasteringLimiter;
using MasteringLimiterParameters = SharpDX.XAPO.Fx.MasteringLimiterParameters;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio
    /// </summary>
    class GameAudio : IDisposable
    {
        /// <summary>
        /// Gets or sets the distance scaling ratio. Default is 1f.
        /// </summary>
        public static float DistanceScale { get; set; } = 1f;
        /// <summary>
        /// Gets or sets the Doppler effect scale ratio. Default is 1f.
        /// </summary>
        public static float DopplerScale { get; set; } = 1f;

        /// <summary>
        /// Device
        /// </summary>
        private readonly XAudio2 device;
        /// <summary>
        /// 3D audio instance
        /// </summary>
        private X3DAudio x3DInstance = null;
        /// <summary>
        /// Mastering limiter
        /// </summary>
        private MasteringLimiter masteringLimiter = null;
        /// <summary>
        /// Mastering limiter flag
        /// </summary>
        private bool useMasteringLimiter = false;

        /// <summary>
        /// Mastering voice
        /// </summary>
        internal MasteringVoice MasteringVoice { get; private set; }
        /// <summary>
        /// Input sample rate
        /// </summary>
        public int InputSampleRate { get; private set; }
        /// <summary>
        /// Speakers configuration
        /// </summary>
        public Speakers Speakers { get; private set; }
        /// <summary>
        /// Output channels
        /// </summary>
        public int InputChannelCount { get; private set; }
        /// <summary>
        /// Use redirect to LFE
        /// </summary>
        public bool UseRedirectToLFE
        {
            get
            {
                return Speakers.HasFlag(Speakers.LowFrequency);
            }
        }

        /// <summary>
        /// Gets or sets the master volume value
        /// </summary>
        /// <remarks>From 0 to 1</remarks>
        public float MasterVolume
        {
            get
            {
                MasteringVoice.GetVolume(out float masterVolume);
                return masterVolume;
            }
            set
            {
                float masterVolume = MathUtil.Clamp(value, 0.0f, 1.0f);
                MasteringVoice.SetVolume(masterVolume);
            }
        }
        /// <summary>
        /// Gets or sets whether the master voice uses a limiter or not
        /// </summary>
        public bool UseMasteringLimiter
        {
            get
            {
                return useMasteringLimiter;
            }
            set
            {
                useMasteringLimiter = value;

                if (useMasteringLimiter)
                {
                    EnableMasteringLimiter();
                }
                else
                {
                    DisableMasteringLimiter();
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        internal GameAudio(XAudio2Version version = XAudio2Version.Default, int sampleRate = 48000)
        {
            XAudio2Flags audio2Flags;
#if DEBUG
            audio2Flags = XAudio2Flags.DebugEngine;
#else
            audio2Flags = XAudio2Flags.None;
#endif
            device = new XAudio2(audio2Flags, ProcessorSpecifier.DefaultProcessor, version);
            device.StopEngine();
#if DEBUG
            DebugConfiguration debugConfiguration = new DebugConfiguration()
            {
                TraceMask = (int)(LogType.Errors | LogType.Warnings),
                BreakMask = (int)LogType.Errors,
            };
            device.SetDebugConfiguration(debugConfiguration, IntPtr.Zero);
#endif

            MasteringVoice = new MasteringVoice(device, 2, sampleRate);

            if (device.Version == XAudio2Version.Version27)
            {
                var details = MasteringVoice.VoiceDetails;
                InputSampleRate = details.InputSampleRate;
                InputChannelCount = details.InputChannelCount;
                int channelMask = MasteringVoice.ChannelMask;
                Speakers = (Speakers)channelMask;
            }
            else
            {
                MasteringVoice.GetVoiceDetails(out var details);
                InputSampleRate = details.InputSampleRate;
                InputChannelCount = details.InputChannelCount;
                MasteringVoice.GetChannelMask(out int channelMask);
                Speakers = (Speakers)channelMask;
            }

            if (Speakers == Speakers.None)
            {
                Speakers = Speakers.FrontLeft | Speakers.FrontRight;
            }

            MasteringVoice.SetVolume(1f);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~GameAudio()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                x3DInstance = null;

                if (MasteringVoice?.IsDisposed != true)
                {
                    MasteringVoice?.DestroyVoice();
                    MasteringVoice?.Dispose();
                    MasteringVoice = null;
                }

                if (masteringLimiter?.IsDisposed != true)
                {
                    masteringLimiter?.Dispose();
                    masteringLimiter = null;
                }

                if (device?.IsDisposed != true)
                {
                    device?.StopEngine();
                    device?.Dispose();
                }
            }
        }

        /// <summary>
        /// Starts the audio device
        /// </summary>
        public void Start()
        {
            device.StartEngine();
        }
        /// <summary>
        /// Stops the audio device
        /// </summary>
        public void Stop()
        {
            device.StopEngine();
        }

        /// <summary>
        /// Creates a source voice
        /// </summary>
        /// <param name="waveFormat">Wave format</param>
        /// <param name="useFilter">Use filters</param>
        /// <returns>Returns the souce voice</returns>
        internal SourceVoice CreateSourceVoice(WaveFormat waveFormat, bool useFilter = false)
        {
            if (useFilter)
            {
                return new SourceVoice(device, waveFormat, VoiceFlags.UseFilter, XAudio2.MaximumFilterFrequency);
            }
            else
            {
                return new SourceVoice(device, waveFormat);
            }
        }
        /// <summary>
        /// Creates a reverb effect
        /// </summary>
        /// <param name="isUsingDebuging">Use debug</param>
        /// <returns>Returns the reverb effect</returns>
        internal Reverb CreateReverb(bool isUsingDebuging = false)
        {
            return new Reverb(device, isUsingDebuging);
        }
        /// <summary>
        /// Creates a submix voice
        /// </summary>
        /// <param name="inputChannelCount">Input channels</param>
        /// <param name="inputSampleRate">Input sample rate</param>
        /// <returns>Returns the submix voice</returns>
        internal SubmixVoice CreatesSubmixVoice(int inputChannelCount, int inputSampleRate)
        {
            return new SubmixVoice(
                device,
                inputChannelCount,
                inputSampleRate);
        }
        /// <summary>
        /// Creates a new reverb voice
        /// </summary>
        internal SubmixVoice CreateReverbVoice()
        {
            // Create reverb effect
            using (var reverbEffect = CreateReverb())
            {
                // Create a submix voice
                var submixVoice = CreatesSubmixVoice(InputChannelCount, InputSampleRate);

                // Performance tip: you need not run global FX with the sample number
                // of channels as the final mix.  For example, this sample runs
                // the reverb in mono mode, thus reducing CPU overhead.
                var desc = new EffectDescriptor(reverbEffect)
                {
                    InitialState = true,
                    OutputChannelCount = InputChannelCount,
                };
                submixVoice.SetEffectChain(desc);

                return submixVoice;
            }
        }

        /// <summary>
        /// Sets the mastering limiter parameters
        /// </summary>
        /// <param name="release">Speed at which the limiter stops affecting audio once it drops below the limiter's threshold</param>
        /// <param name="loudness">Threshold of the limiter</param>
        public void SetMasteringLimit(int release, int loudness)
        {
            if (release < MasteringLimiter.MinimumRelease || release > MasteringLimiter.MaximumRelease)
            {
                throw new ArgumentOutOfRangeException(nameof(release), $"Must be a value between {MasteringLimiter.MinimumRelease} and {MasteringLimiter.MaximumRelease}");
            }

            if (loudness < MasteringLimiter.MinimumLoudness || loudness > MasteringLimiter.MaximumLoudness)
            {
                throw new ArgumentOutOfRangeException(nameof(loudness), $"Must be a value between {MasteringLimiter.MinimumLoudness} and {MasteringLimiter.MaximumLoudness}");
            }

            if (useMasteringLimiter)
            {
                var parameters = new MasteringLimiterParameters
                {
                    Loudness = loudness,
                    Release = release
                };

                MasteringVoice?.SetEffectParameters(0, parameters);
            }
        }
        /// <summary>
        /// Enables the mastering limiter
        /// </summary>
        private void EnableMasteringLimiter()
        {
            if (masteringLimiter == null)
            {
                masteringLimiter = new MasteringLimiter(device);
                MasteringVoice.SetEffectChain(new EffectDescriptor(masteringLimiter));
            }

            MasteringVoice?.EnableEffect(0);
        }
        /// <summary>
        /// Disables the mastering limiter
        /// </summary>
        private void DisableMasteringLimiter()
        {
            MasteringVoice?.DisableEffect(0);
        }

        /// <summary>
        /// Calculates the 3D audio effect
        /// </summary>
        /// <param name="listener">Listener</param>
        /// <param name="emitter">Emitter</param>
        /// <param name="flags">Calculate flags</param>
        /// <param name="dspSettings">DSP settings</param>
        internal void Calculate3D(Listener listener, Emitter emitter, CalculateFlags flags, DspSettings dspSettings)
        {
            if (x3DInstance == null)
            {
                x3DInstance = new X3DAudio(this.Speakers, X3DAudio.SpeedOfSound);
            }

            x3DInstance.Calculate(listener, emitter, flags, dspSettings);
        }
    }
}
