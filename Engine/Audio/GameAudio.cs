using SharpDX;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;
using SharpDX.XAudio2.Fx;
using System;
using System.Collections.Generic;
using System.Linq;
using MasteringLimiter = SharpDX.XAPO.Fx.MasteringLimiter;
using MasteringLimiterParameters = SharpDX.XAPO.Fx.MasteringLimiterParameters;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio
    /// </summary>
    public class GameAudio : IDisposable
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
        private readonly XAudio2 device = null;
        /// <summary>
        /// 3D audio instance
        /// </summary>
        private X3DAudio x3DInstance = null;
        /// <summary>
        /// Mastering limiter
        /// </summary>
        private MasteringLimiter masteringLimiter = null;
        /// <summary>
        /// Master volume (from 0 to 1)
        /// </summary>
        private float masterVolume = 1.0f;
        /// <summary>
        /// Mastering limiter flag
        /// </summary>
        private bool useMasteringLimiter = false;
        /// <summary>
        /// Reverb flag
        /// </summary>
        private bool useReverb = false;
        /// <summary>
        /// Current reverb preset variable
        /// </summary>
        private ReverbPresets? reverbPreset = null;
        /// <summary>
        /// Audio 3D flag
        /// </summary>
        private bool useAudio3D = false;
        /// <summary>
        /// Effects dictionary
        /// </summary>
        private readonly Dictionary<string, GameAudioEffect> effects = new Dictionary<string, GameAudioEffect>();

        /// <summary>
        /// Mastering voice
        /// </summary>
        internal MasteringVoice MasteringVoice { get; set; }
        /// <summary>
        /// Reverb voice
        /// </summary>
        internal SubmixVoice ReverbVoice { get; set; }
        /// <summary>
        /// Reverb effect
        /// </summary>
        internal Reverb ReverbEffect { get; set; }
        /// <summary>
        /// Speakers
        /// </summary>
        internal Speakers Speakers { get; set; }

        /// <summary>
        /// Gets or sets the master volume value
        /// </summary>
        /// <remarks>From 0 to 1</remarks>
        public float MasterVolume
        {
            get
            {
                return masterVolume;
            }
            set
            {
                value = MathUtil.Clamp(value, 0, 1);

                if (masterVolume == value)
                {
                    return;
                }

                masterVolume = value;

                this.MasteringVoice?.SetVolume(masterVolume);
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
                    this.EnableMasteringLimiter();
                }
                else
                {
                    this.DisableMasteringLimiter();
                }
            }
        }
        /// <summary>
        /// Gets or sets whether the sub-mix voice uses a reverb effect or not
        /// </summary>
        public bool UseReverb
        {
            get
            {
                return useReverb;
            }
            set
            {
                useReverb = value;

                if (useReverb)
                {
                    this.EnableReverb();
                }
                else
                {
                    this.DisableReverb();
                }
            }
        }
        /// <summary>
        /// Gets or sets whether the reverb effect use filters or not
        /// </summary>
        public bool UseReverbFilter { get; set; } = true;
        /// <summary>
        /// Gets or sets the current reverb preset configuration
        /// </summary>
        public ReverbPresets? ReverbPreset
        {
            get
            {
                return reverbPreset;
            }
            set
            {
                if (this.ReverbEffect == null || reverbPreset == value)
                {
                    return;
                }

                reverbPreset = value;

                if (this.ReverbVoice == null)
                {
                    return;
                }

                var reverbParam = GameAudioPresets.Convert(reverbPreset ?? ReverbPresets.Default);

                this.ReverbVoice.SetEffectParameters(0, reverbParam);
            }
        }
        /// <summary>
        /// Gets or sets whether the master voice uses 3D audio or not
        /// </summary>
        public bool UseAudio3D
        {
            get
            {
                return useAudio3D;
            }
            set
            {
                useAudio3D = value;

                if (useAudio3D)
                {
                    this.EnableAudio3D();
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        internal GameAudio()
        {
            this.device = new XAudio2();

#if DEBUG
            DebugConfiguration debugConfiguration = new DebugConfiguration()
            {
                TraceMask = (int)(LogType.Errors | LogType.Warnings),
                BreakMask = (int)(LogType.Errors),
            };
            this.device.SetDebugConfiguration(debugConfiguration, IntPtr.Zero);
#endif

            this.MasteringVoice = new MasteringVoice(this.device);

            this.MasteringVoice.GetChannelMask(out int channelMask);
            this.Speakers = (Speakers)channelMask;

            this.MasteringVoice.SetVolume(this.MasterVolume);
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
                this.effects.Values.ToList().ForEach(e => e.Dispose());
                this.effects.Clear();

                this.x3DInstance = null;

                this.ReverbVoice?.DestroyVoice();
                this.ReverbVoice?.Dispose();
                this.ReverbVoice = null;
                this.ReverbEffect?.Dispose();
                this.ReverbEffect = null;

                this.MasteringVoice?.DestroyVoice();
                this.MasteringVoice?.Dispose();
                this.MasteringVoice = null;

                this.masteringLimiter?.Dispose();
                this.masteringLimiter = null;

                this.device?.StopEngine();
                this.device?.Dispose();
            }
        }

        /// <summary>
        /// Gets an effect from de audio
        /// </summary>
        /// <param name="name">Effect name</param>
        /// <param name="fileName">File name</param>
        /// <returns>Returns the new created effect</returns>
        internal GameAudioEffect GetEffect(string name, string fileName)
        {
            if (effects.ContainsKey(name))
            {
                return effects[name];
            }

            var effect = GameAudioEffect.Load(this, name, fileName);

            effects.Add(name, effect);

            return effect;
        }

        /// <summary>
        /// Creates a new source voice
        /// </summary>
        /// <param name="format">Voice format</param>
        /// <param name="voiceFlags">Voice flags</param>
        /// <returns>Returns the new voice</returns>
        internal SourceVoice CreateVoice(WaveFormat format, VoiceFlags voiceFlags)
        {
            return new SourceVoice(
                this.device,
                format,
                voiceFlags,
                XAudio2.MaximumFrequencyRatio);
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
                throw new ArgumentOutOfRangeException("release", $"Must be a value between {MasteringLimiter.MinimumRelease} and {MasteringLimiter.MaximumRelease}");
            }

            if (loudness < MasteringLimiter.MinimumLoudness || loudness > MasteringLimiter.MaximumLoudness)
            {
                throw new ArgumentOutOfRangeException("loudness", $"Must be a value between {MasteringLimiter.MinimumLoudness} and {MasteringLimiter.MaximumLoudness}");
            }

            if (useMasteringLimiter)
            {
                var parameters = new MasteringLimiterParameters
                {
                    Loudness = loudness,
                    Release = release
                };

                this.MasteringVoice?.SetEffectParameters(0, parameters);
            }
        }
        /// <summary>
        /// Enables the mastering limiter
        /// </summary>
        private void EnableMasteringLimiter()
        {
            if (this.masteringLimiter == null)
            {
                this.masteringLimiter = new MasteringLimiter(this.device);
                this.MasteringVoice.SetEffectChain(new EffectDescriptor(this.masteringLimiter));
            }

            this.MasteringVoice?.EnableEffect(0);
        }
        /// <summary>
        /// Disables the mastering limiter
        /// </summary>
        private void DisableMasteringLimiter()
        {
            this.MasteringVoice?.DisableEffect(0);
        }

        /// <summary>
        /// Enables the reverb effect
        /// </summary>
        private void EnableReverb()
        {
            if (this.ReverbVoice == null)
            {
                var masterDetails = this.MasteringVoice.VoiceDetails;
                var sendFlags = this.UseReverbFilter ? SubmixVoiceFlags.UseFilter : SubmixVoiceFlags.None;
                this.ReverbVoice = new SubmixVoice(this.device, 1, masterDetails.InputSampleRate, sendFlags, 0);
            }

            if (this.ReverbEffect == null)
            {
                this.ReverbEffect = new Reverb(this.device);
                this.ReverbVoice.SetEffectChain(new EffectDescriptor(this.ReverbEffect, 1));
            }

            this.ReverbVoice.EnableEffect(0);
        }
        /// <summary>
        /// Disables the reverb effect
        /// </summary>
        private void DisableReverb()
        {
            this.ReverbVoice?.DisableEffect(0);
        }

        /// <summary>
        /// Enables the 3D audio instance
        /// </summary>
        public void EnableAudio3D(float speedOfSound = X3DAudio.SpeedOfSound)
        {
            if (this.x3DInstance == null)
            {
                this.x3DInstance = new X3DAudio(this.Speakers, speedOfSound);
            }
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
            this.x3DInstance?.Calculate(listener, emitter, flags, dspSettings);
        }
    }
}
