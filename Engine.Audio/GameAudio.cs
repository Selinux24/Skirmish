using SharpDX;
using System;

namespace Engine.Audio
{
    using SharpDX.Multimedia;
    using SharpDX.X3DAudio;
    using SharpDX.XAudio2;
    using SharpDX.XAudio2.Fx;
    using MasteringLimiter = SharpDX.XAPO.Fx.MasteringLimiter;
    using MasteringLimiterParameters = SharpDX.XAPO.Fx.MasteringLimiterParameters;

    /// <summary>
    /// Game audio
    /// </summary>
    class GameAudio : IGameAudio
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
        /// Audio speakers
        /// </summary>
        private readonly GameAudioSpeakers speakers;

        /// <summary>
        /// Mastering voice
        /// </summary>
        public MasteringVoice MasteringVoice { get; private set; }
        /// <inheritdoc/>
        public int InputSampleRate { get; private set; }
        /// <inheritdoc/>
        public int InputChannelCount { get; private set; }
        /// <inheritdoc/>
        public bool UseRedirectToLFE
        {
            get
            {
                return speakers.HasFlag(GameAudioSpeakers.LowFrequency);
            }
        }

        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
        public GameAudio() : this(48000)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public GameAudio(int sampleRate)
        {
            XAudio2Flags audio2Flags;
#if DEBUG
            audio2Flags = XAudio2Flags.DebugEngine;
#else
            audio2Flags = XAudio2Flags.None;
#endif
            device = new XAudio2(audio2Flags, ProcessorSpecifier.DefaultProcessor, XAudio2Version.Default);
            device.StopEngine();
#if DEBUG
            DebugConfiguration debugConfiguration = new()
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
                speakers = (GameAudioSpeakers)channelMask;
            }
            else
            {
                MasteringVoice.GetVoiceDetails(out var details);
                InputSampleRate = details.InputSampleRate;
                InputChannelCount = details.InputChannelCount;
                MasteringVoice.GetChannelMask(out int channelMask);
                speakers = (GameAudioSpeakers)channelMask;
            }

            if (speakers == GameAudioSpeakers.None)
            {
                speakers = GameAudioSpeakers.Stereo;
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

        /// <inheritdoc/>
        public void Start()
        {
            device.StartEngine();
        }
        /// <inheritdoc/>
        public void Stop()
        {
            device.StopEngine();
        }

        /// <inheritdoc/>
        public IGameAudioEffect CreateEffect(string fileName, GameAudioEffectParameters effectParameters)
        {
            return new GameAudioEffect(this, fileName, effectParameters);
        }

        /// <inheritdoc/>
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
        /// Creates a source voice
        /// </summary>
        /// <param name="waveFormat">Wave format</param>
        /// <param name="useFilter">Use filters</param>
        /// <returns>Returns the souce voice</returns>
        public SourceVoice CreateSourceVoice(WaveFormat waveFormat, bool useFilter = false)
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
        private Reverb CreateReverb(bool isUsingDebuging = false)
        {
            return new Reverb(device, isUsingDebuging);
        }
        /// <summary>
        /// Creates a submix voice
        /// </summary>
        /// <param name="inputChannelCount">Input channels</param>
        /// <param name="inputSampleRate">Input sample rate</param>
        /// <returns>Returns the submix voice</returns>
        private SubmixVoice CreatesSubmixVoice(int inputChannelCount, int inputSampleRate)
        {
            return new SubmixVoice(device, inputChannelCount, inputSampleRate);
        }
        /// <summary>
        /// Creates a new reverb voice
        /// </summary>
        public SubmixVoice CreateReverbVoice()
        {
            // Create reverb effect
            using var reverbEffect = CreateReverb();

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

        /// <summary>
        /// Gets the speakers configuration
        /// </summary>
        public GameAudioSpeakers GetAudioSpeakers()
        {
            return speakers;
        }
        /// <summary>
        /// Calculates the 3D audio effect
        /// </summary>
        /// <param name="listener">Listener</param>
        /// <param name="emitter">Emitter</param>
        /// <param name="flags">Calculate flags</param>
        /// <param name="dspSettings">DSP settings</param>
        public void Calculate3D(Listener listener, Emitter emitter, CalculateFlags flags, DspSettings dspSettings)
        {
            x3DInstance ??= new X3DAudio((Speakers)speakers, X3DAudio.SpeedOfSound);

            x3DInstance.Calculate(listener, emitter, flags, dspSettings);
        }
    }
}
