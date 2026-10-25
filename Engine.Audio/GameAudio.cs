using System;
using Vortice.Multimedia;
using Vortice.XAPO;
using Vortice.XAudio2;

namespace Engine.Audio
{
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
        private readonly IXAudio2 device;
        /// <summary>
        /// 3D audio instance
        /// </summary>
        private X3DAudio x3DInstance = null;
        /// <summary>
        /// Mastering limiter flag
        /// </summary>
        private bool useMasteringLimiter = false;
        /// <summary>
        /// Audio speakers
        /// </summary>
        private readonly GameAudioSpeakers speakers;
        private uint operationSet = 0;
        private bool operationSetActive = false;

        /// <summary>
        /// Mastering voice
        /// </summary>
        public IXAudio2MasteringVoice MasteringVoice { get; private set; }
        /// <inheritdoc/>
        public uint InputSampleRate { get; private set; }
        /// <inheritdoc/>
        public uint InputChannelCount { get; private set; }
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
                return MasteringVoice.Volume;
            }
            set
            {
                MasteringVoice.Volume = Math.Clamp(value, 0.0f, 1.0f);
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
        /// <inheritdoc/>
        public uint OperationSet
        {
            get
            {
                return operationSetActive ? operationSet : XAudio2.CommitNow;
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
        public GameAudio(uint sampleRate)
        {
            XAudio2.XAudio2Create(ProcessorSpecifier.DefaultProcessor, out device);
            device.StopEngine();
#if DEBUG
            DebugConfiguration debugConfiguration = new()
            {
                TraceMask = LogType.Errors | LogType.Warnings,
                BreakMask = LogType.Errors,
            };
            device.SetDebugConfiguration(debugConfiguration);
#endif

            MasteringVoice = device.CreateMasteringVoice(2, sampleRate);

            var details = MasteringVoice.VoiceDetails;
            InputSampleRate = details.InputSampleRate;
            InputChannelCount = details.InputChannels;
            speakers = (GameAudioSpeakers)MasteringVoice.ChannelMask;

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

                MasteringVoice?.DestroyVoice();
                MasteringVoice?.Dispose();
                MasteringVoice = null;

                device?.StopEngine();
                device?.Dispose();
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
        public void SetMasteringLimit(uint release, uint loudness)
        {
            if (release < XAPO.MinRelease || release > XAPO.MaxRelease)
            {
                throw new ArgumentOutOfRangeException(nameof(release), $"Must be a value between {XAPO.MinRelease} and {XAPO.MaxRelease}");
            }

            if (loudness < XAPO.MinLoudness || loudness > XAPO.MaxLoudness)
            {
                throw new ArgumentOutOfRangeException(nameof(loudness), $"Must be a value between {XAPO.MinLoudness} and {XAPO.MaxLoudness}");
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
            Guid clsidLimiter = XAPO.CLSID_FXMasteringLimiter;
            var limiterEffect = XAPO.CreateFX(clsidLimiter);

            EffectDescriptor descriptor = new(limiterEffect)
            {
                InitialState = true,
                OutputChannelCount = InputChannelCount,
            };

            MasteringVoice?.SetEffectChain(descriptor);
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
        public IXAudio2SourceVoice CreateSourceVoice(WaveFormat waveFormat, bool useFilter = false)
        {
            if (useFilter)
            {
                return device.CreateSourceVoice(waveFormat, VoiceFlags.UseFilter, XAudio2.MaximumFilterFrequency);
            }
            else
            {
                return device.CreateSourceVoice(waveFormat);
            }
        }
        /// <summary>
        /// Creates a submix voice
        /// </summary>
        /// <param name="inputChannelCount">Input channels</param>
        /// <param name="inputSampleRate">Input sample rate</param>
        /// <returns>Returns the submix voice</returns>
        private IXAudio2SubmixVoice CreatesSubmixVoice(uint inputChannelCount, uint inputSampleRate)
        {
            return device.CreateSubmixVoice(inputChannelCount, inputSampleRate);
        }
        /// <summary>
        /// Creates a new reverb voice
        /// </summary>
        public IXAudio2SubmixVoice CreateReverbVoice()
        {
            // Create reverb effect
            using var reverbEffect = XAPO.CreateFX(XAPO.CLSID_FXReverb);

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

        /// <inheritdoc/>
        public uint StartOperationSet()
        {
            if (operationSetActive) return operationSet;

            operationSet++;
            operationSetActive = true;

            return operationSet;
        }
        /// <inheritdoc/>
        public void CommitOperation(uint operationSet)
        {
            if (!operationSetActive) return;

            device.CommitChanges(operationSet);

            operationSetActive = false;
        }
    }
}
