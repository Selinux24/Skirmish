using SharpDX;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;
using SharpDX.XAudio2.Fx;
using System;
using System.Collections.Generic;
using System.IO;
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
        /// Sound dictionary
        /// </summary>
        private readonly Dictionary<string, GameAudioSound> sounds = new Dictionary<string, GameAudioSound>();

        /// <summary>
        /// Device
        /// </summary>
        internal XAudio2 Device { get; private set; }
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
                return (Speakers & Speakers.LowFrequency) != 0;
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
                this.MasteringVoice.GetVolume(out float masterVolume);
                return masterVolume;
            }
            set
            {
                float masterVolume = MathUtil.Clamp(value, 0.0f, 1.0f);
                this.MasteringVoice.SetVolume(masterVolume);
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
        /// Loads a file in the audio buffer
        /// </summary>
        /// <param name="name">Effect name</param>
        /// <param name="fileName">File name</param>
        public GameAudioSound LoadFromFile(string name, string fileName)
        {
            GameAudioSound sound = new GameAudioSound(this, name);

            using (var stream = new SoundStream(File.OpenRead(fileName)))
            {
                var buffer = stream.ToDataStream();

                sound.WaveFormat = stream.Format;
                sound.DecodedPacketsInfo = stream.DecodedPacketsInfo;
                sound.AudioBuffer = new AudioBuffer
                {
                    Stream = buffer,
                    AudioBytes = (int)buffer.Length,
                    Flags = BufferFlags.EndOfStream
                };
                sound.LoopedAudioBuffer = new AudioBuffer
                {
                    Stream = buffer,
                    AudioBytes = (int)buffer.Length,
                    Flags = BufferFlags.EndOfStream,
                    LoopCount = AudioBuffer.LoopInfinite,
                };
                sound.Duration = TimeSpan.Zero;
                if (stream.Format.SampleRate > 0)
                {
                    var samplesDuration = GameAudioSound.GetSamplesDuration(
                        stream.Format,
                        buffer.Length,
                        stream.DecodedPacketsInfo);

                    var milliseconds = samplesDuration * 1000 / stream.Format.SampleRate;

                    sound.Duration = TimeSpan.FromMilliseconds(milliseconds);
                }
            }

            return sound;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        internal GameAudio(XAudio2Version version = XAudio2Version.Default, int sampleRate = 48000)
        {
            XAudio2Flags audio2Flags = XAudio2Flags.None;
#if DEBUG
            audio2Flags = XAudio2Flags.DebugEngine;
#endif

            this.Device = new XAudio2(audio2Flags, ProcessorSpecifier.DefaultProcessor, version);
            this.Device.StopEngine();
#if DEBUG
            DebugConfiguration debugConfiguration = new DebugConfiguration()
            {
                TraceMask = (int)(LogType.Errors | LogType.Warnings),
                BreakMask = (int)(LogType.Errors),
            };
            this.Device.SetDebugConfiguration(debugConfiguration, IntPtr.Zero);
#endif

            this.MasteringVoice = new MasteringVoice(this.Device, 2, sampleRate);

            if (this.Device.Version == XAudio2Version.Version27)
            {
                var details = this.MasteringVoice.VoiceDetails;
                this.InputSampleRate = details.InputSampleRate;
                this.InputChannelCount = details.InputChannelCount;
                int channelMask = this.MasteringVoice.ChannelMask;
                this.Speakers = (Speakers)channelMask;
            }
            else
            {
                this.MasteringVoice.GetVoiceDetails(out var details);
                this.InputSampleRate = details.InputSampleRate;
                this.InputChannelCount = details.InputChannelCount;
                this.MasteringVoice.GetChannelMask(out int channelMask);
                this.Speakers = (Speakers)channelMask;
            }

            if (this.Speakers == Speakers.None)
            {
                this.Speakers = Speakers.FrontLeft | Speakers.FrontRight;
            }

            this.MasteringVoice.SetVolume(1f);
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
                this.sounds.Values.ToList().ForEach(e => e.Dispose());
                this.sounds.Clear();

                this.x3DInstance = null;

                this.MasteringVoice?.DestroyVoice();
                this.MasteringVoice?.Dispose();
                this.MasteringVoice = null;

                this.masteringLimiter?.Dispose();
                this.masteringLimiter = null;

                this.Device?.StopEngine();
                this.Device?.Dispose();
            }
        }

        /// <summary>
        /// Starts the audio device
        /// </summary>
        public void Start()
        {
            Device.StartEngine();
        }
        /// <summary>
        /// Stops the audio device
        /// </summary>
        public void Stop()
        {
            Device.StopEngine();
        }

        /// <summary>
        /// Updates the internal state
        /// </summary>
        internal void Update()
        {
            sounds?
                .ToList()
                .ForEach(e => e.Value?.Update());
        }

        /// <summary>
        /// Creates a source voice
        /// </summary>
        /// <param name="waveFormat">Wave format</param>
        /// <returns>Returns the souce voice</returns>
        internal SourceVoice CreateSourceVoice(WaveFormat waveFormat)
        {
            return new SourceVoice(Device, waveFormat);
        }
        /// <summary>
        /// Creates a reverb effect
        /// </summary>
        /// <param name="isUsingDebuging">Use debug</param>
        /// <returns>Returns the reverb effect</returns>
        internal Reverb CreateReverb(bool isUsingDebuging = false)
        {
            return new Reverb(Device, isUsingDebuging);
        }
        /// <summary>
        /// Creates a submix voice
        /// </summary>
        /// <param name="inputChannelCount">Input channels</param>
        /// <param name="inputSampleRate">Input sample rate</param>
        /// <param name="sendFlags">Send flags</param>
        /// <param name="processingStage">Processing stage</param>
        /// <returns>Returns the submix voice</returns>
        internal SubmixVoice CreatesSubmixVoice(int inputChannelCount, int inputSampleRate, SubmixVoiceFlags sendFlags, int processingStage)
        {
            return new SubmixVoice(
                Device,
                inputChannelCount,
                inputSampleRate,
                sendFlags,
                processingStage);
        }
        /// <summary>
        /// Gets a sound
        /// </summary>
        /// <param name="name">Sound name</param>
        /// <param name="fileName">File name</param>
        /// <returns>Returns the new created sound</returns>
        internal GameAudioSound GetSound(string name, string fileName)
        {
            if (sounds.ContainsKey(name))
            {
                return sounds[name];
            }

            var sound = LoadFromFile(name, fileName);

            sounds.Add(name, sound);

            return sound;
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
                this.masteringLimiter = new MasteringLimiter(this.Device);
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
        /// Calculates the 3D audio effect
        /// </summary>
        /// <param name="listener">Listener</param>
        /// <param name="emitter">Emitter</param>
        /// <param name="flags">Calculate flags</param>
        /// <param name="dspSettings">DSP settings</param>
        internal void Calculate3D(Listener listener, Emitter emitter, CalculateFlags flags, DspSettings dspSettings)
        {
            if (this.x3DInstance == null)
            {
                this.x3DInstance = new X3DAudio(this.Speakers, X3DAudio.SpeedOfSound);
            }

            this.x3DInstance.Calculate(listener, emitter, flags, dspSettings);
        }
    }
}
