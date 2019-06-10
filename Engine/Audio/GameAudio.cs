using SharpDX;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;
using SharpDX.XAudio2.Fx;
using System;
using System.IO;
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
        /// Input channels
        /// </summary>
        private const int INPUTCHANNELS = 1;

        /// <summary>
        /// Game audio state
        /// </summary>
        private GameAudioState audioState;
        /// <summary>
        /// Source vouice events attached flag
        /// </summary>
        private bool eventsAttached = false;
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
        /// Gets whether the audio is playing or not
        /// </summary>
        public bool Playing { get; private set; }
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

                audioState.MasteringVoice?.SetVolume(masterVolume);
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
                    EnableReverb();
                }
                else
                {
                    DisableReverb();
                }
            }
        }
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
                if (audioState.ReverbEffect == null || reverbPreset == value)
                {
                    return;
                }

                reverbPreset = value;

                var reverbParam = GameAudioPresets.Convert(reverbPreset ?? ReverbPresets.Default);

                audioState.ReverbVoice?.SetEffectParameters(0, reverbParam);
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
                    EnableAudio3D();
                }
            }
        }

        /// <summary>
        /// Event fired when the audio starts
        /// </summary>
        public event GameAudioHandler AudioStart;
        /// <summary>
        /// Event fired when the audio ends
        /// </summary>
        public event GameAudioHandler AudioEnd;
        /// <summary>
        /// Event fired when a loop ends
        /// </summary>
        public event GameAudioHandler LoopEnd;

        /// <summary>
        /// Constructor
        /// </summary>
        internal GameAudio()
        {
            audioState = new GameAudioState
            {
                EmitterAzimuths = new float[INPUTCHANNELS],
            };

            audioState.XAudio2 = new XAudio2();

#if DEBUG
            DebugConfiguration debugConfiguration = new DebugConfiguration()
            {
                TraceMask = (int)(LogType.Errors | LogType.Warnings),
                BreakMask = (int)(LogType.Errors),
            };
            audioState.XAudio2.SetDebugConfiguration(debugConfiguration, IntPtr.Zero);
#endif




            audioState.MasteringVoice = new MasteringVoice(audioState.XAudio2);

            var voiceDetails = audioState.MasteringVoice.VoiceDetails;

            audioState.MasteringVoice.GetChannelMask(out int channelMask);
            audioState.Speakers = (Speakers)channelMask;

            audioState.MasteringVoice.SetVolume(MasterVolume);



            audioState.ReverbVoice = new SubmixVoice(
                audioState.XAudio2,
                INPUTCHANNELS,
                voiceDetails.InputSampleRate,
                SubmixVoiceFlags.UseFilter,
                0);



            audioState.UseRedirectToLFE = audioState.Speakers.HasFlag(Speakers.LowFrequency);

            audioState.ListenerPos = Vector3.Zero;
            audioState.ListenerOrientation = Vector3.ForwardLH;
            audioState.Listener = new Listener
            {
                Position = audioState.ListenerPos,

                OrientFront = audioState.ListenerOrientation,
                OrientTop = Vector3.Up,

                Cone = GameAudioPresets.DefaultListenerDirectionalCone,
            };

            audioState.EmitterPos = Vector3.ForwardLH * 10.0f;
            audioState.EmitterOrientation = Vector3.ForwardLH;
            audioState.Emitter = new Emitter
            {
                Cone = null,

                Position = audioState.EmitterPos,

                OrientFront = audioState.EmitterOrientation,
                OrientTop = Vector3.Up,

                ChannelCount = INPUTCHANNELS,
                ChannelRadius = 1.0f,
                ChannelAzimuths = audioState.EmitterAzimuths,

                InnerRadius = 2.0f,
                InnerRadiusAngle = MathUtil.PiOverFour,

                VolumeCurve = GameAudioPresets.DefaultLinearCurve,
                LfeCurve = GameAudioPresets.DefaultEmitterLfeCurve,
                LpfDirectCurve = null, // use default curve
                LpfReverbCurve = null, // use default curve
                ReverbCurve = GameAudioPresets.DefaultEmitterReverbCurve,
                CurveDistanceScaler = 14.0f,
                DopplerScaler = 1.0f,
            };

            audioState.DspSettings = new DspSettings(INPUTCHANNELS, voiceDetails.InputChannelCount);
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
                audioState.SourceVoice.DestroyVoice();
                audioState.SourceVoice.Dispose();

                audioState.ReverbVoice.DestroyVoice();
                audioState.ReverbVoice.Dispose();

                audioState.MasteringVoice.DestroyVoice();
                audioState.MasteringVoice.Dispose();

                audioState.XAudio2.StopEngine();
                audioState.XAudio2.Dispose();

                audioState.ReverbEffect?.Dispose();
                audioState.MasteringLimiter?.Dispose();
            }
        }

        /// <summary>
        /// Loads a file in the audio buffer
        /// </summary>
        /// <param name="fileName">File name</param>
        public void Load(string fileName)
        {
            if (audioState.SourceVoice != null)
            {
                audioState.SourceVoice.Stop(0);

                if (eventsAttached)
                {
                    audioState.SourceVoice.BufferStart -= SourceVoice_BufferStart;
                    audioState.SourceVoice.BufferEnd -= SourceVoice_BufferEnd;
                    audioState.SourceVoice.LoopEnd -= SourceVoice_LoopEnd;

                    eventsAttached = false;
                }

                audioState.SourceVoice.DestroyVoice();
                audioState.SourceVoice = null;
            }

            using (var stream = new SoundStream(File.OpenRead(fileName)))
            {
                var waveFormat = stream.Format;
                var decodedPacketsInfo = stream.DecodedPacketsInfo;
                var buffer = new AudioBuffer
                {
                    Stream = stream.ToDataStream(),
                    AudioBytes = (int)stream.Length,
                    Flags = BufferFlags.EndOfStream
                };

                audioState.SourceVoice = new SourceVoice(audioState.XAudio2, waveFormat, VoiceFlags.UseFilter, true);

                VoiceSendDescriptor[] sendDescriptors = new VoiceSendDescriptor[]
                {
                    new  VoiceSendDescriptor(VoiceSendFlags.UseFilter, audioState.MasteringVoice),
                    new  VoiceSendDescriptor(VoiceSendFlags.UseFilter, audioState.ReverbVoice),
                };
                audioState.SourceVoice.SetOutputVoices(sendDescriptors);

                if (!eventsAttached)
                {
                    audioState.SourceVoice.BufferStart += SourceVoice_BufferStart;
                    audioState.SourceVoice.BufferEnd += SourceVoice_BufferEnd;
                    audioState.SourceVoice.LoopEnd += SourceVoice_LoopEnd;

                    eventsAttached = true;
                }

                audioState.SourceVoice.SubmitSourceBuffer(buffer, decodedPacketsInfo);
                audioState.FrameToApply3DAudio = 0;
            }
        }

        /// <summary>
        /// Updates the game audio
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            if (useAudio3D)
            {
                if (audioState.FrameToApply3DAudio == 0)
                {
                    this.UpdateState(gameTime);
                }

                audioState.FrameToApply3DAudio++;
                audioState.FrameToApply3DAudio &= 1;
            }
        }
        /// <summary>
        /// Updates the 3D audio state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        private void UpdateState(GameTime gameTime)
        {
            // Calculate listener orientation in x-z plane
            Vector3 pListenerPos = audioState.ListenerPos;
            Vector3 listenerPos = audioState.Listener.Position;

            if (pListenerPos.XZ() != listenerPos.XZ())
            {
                var v1 = pListenerPos;
                var v2 = listenerPos;

                var vDelta = v1 - v2;
                vDelta.Y = 0.0f;
                vDelta.Normalize();

                audioState.Listener.OrientFront = new Vector3(vDelta.X, 0.0f, vDelta.Z);
            }

            audioState.Emitter.InnerRadius = 2.0f;
            audioState.Emitter.InnerRadiusAngle = MathUtil.PiOverFour;

            if (gameTime.ElapsedSeconds > 0)
            {
                var v1 = audioState.ListenerPos;
                var v2 = audioState.Listener.Position;

                var lVelocity = (v1 - v2) / gameTime.ElapsedSeconds;
                audioState.Listener.Position = audioState.ListenerPos;

                audioState.Listener.Velocity = lVelocity;

                v1 = audioState.EmitterPos;
                v2 = audioState.Emitter.Position;

                var eVelocity = (v1 - v2) / gameTime.ElapsedSeconds;
                audioState.Emitter.Position = audioState.EmitterPos;

                audioState.Emitter.Velocity = eVelocity;
            }

            var calcFlags =
                CalculateFlags.Matrix |
                CalculateFlags.Doppler |
                CalculateFlags.LpfDirect |
                CalculateFlags.LpfReverb |
                CalculateFlags.Reverb;

            if (audioState.UseRedirectToLFE)
            {
                // On devices with an LFE channel, allow the mono source data
                // to be routed to the LFE destination channel.
                calcFlags |= CalculateFlags.RedirectToLfe;
            }

            audioState.X3DInstance.Calculate(
                audioState.Listener,
                audioState.Emitter,
                calcFlags,
                audioState.DspSettings);

            var voice = audioState.SourceVoice;
            if (voice != null)
            {
                // Apply X3DAudio generated DSP settings to XAudio2
                voice.SetFrequencyRatio(audioState.DspSettings.DopplerFactor);

                voice.SetOutputMatrix(
                    audioState.MasteringVoice,
                    audioState.DspSettings.SourceChannelCount,
                    audioState.DspSettings.DestinationChannelCount,
                    audioState.DspSettings.MatrixCoefficients);

                if (useReverb)
                {
                    voice.SetOutputMatrix(
                        audioState.ReverbVoice,
                        INPUTCHANNELS,
                        1,
                        new float[]
                        {
                            audioState.DspSettings.ReverbLevel
                        });

                    FilterParameters filterDirect = new FilterParameters()
                    {
                        Type = FilterType.LowPassFilter,
                        Frequency = 2.0f * (float)Math.Sin(MathUtil.Pi / 6.0f * audioState.DspSettings.LpfDirectCoefficient),
                        OneOverQ = 1.0f,
                    };
                    voice.SetOutputFilterParameters(audioState.MasteringVoice, filterDirect);

                    FilterParameters filterReverb = new FilterParameters()
                    {
                        Type = FilterType.LowPassFilter,
                        Frequency = 2.0f * (float)Math.Sin(MathUtil.Pi / 6.0f * audioState.DspSettings.LpfReverbCoefficient),
                        OneOverQ = 1.0f
                    };
                    voice.SetOutputFilterParameters(audioState.ReverbVoice, filterReverb);
                }
            }
        }

        /// <summary>
        /// Play audio
        /// </summary>
        public void Play()
        {
            Playing = true;
            audioState.SourceVoice.Start(0);
        }
        /// <summary>
        /// Stop audio
        /// </summary>
        public void Stop()
        {
            Playing = false;
            audioState.SourceVoice.Stop(0);
        }
        /// <summary>
        /// Pause audio
        /// </summary>
        public void Pause()
        {
            Playing = false;
            audioState.XAudio2.StopEngine();
        }
        /// <summary>
        /// Resume audio
        /// </summary>
        public void Resume()
        {
            Playing = true;
            audioState.XAudio2.StartEngine();
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

                audioState.MasteringVoice?.SetEffectParameters(0, parameters);
            }
        }

        /// <summary>
        /// Updates the emitter position and orientation
        /// </summary>
        /// <param name="position">Position coordinate</param>
        /// <param name="orientation">Orientation vector</param>
        public void UpdateEmitter(Vector3 position, Vector3 orientation)
        {
            audioState.EmitterPos = position;
            audioState.EmitterOrientation = orientation;
        }
        /// <summary>
        /// Updates the listener position and orientation
        /// </summary>
        /// <param name="position">Position coordinate</param>
        /// <param name="orientation">Orientation vector</param>
        public void UpdateListener(Vector3 position, Vector3 orientation)
        {
            audioState.ListenerPos = position;
            audioState.ListenerOrientation = orientation;
        }

        /// <summary>
        /// Enables the mastering limiter
        /// </summary>
        private void EnableMasteringLimiter()
        {
            if (audioState.MasteringLimiter == null)
            {
                audioState.MasteringLimiter = new MasteringLimiter(audioState.XAudio2);
                audioState.MasteringVoice.SetEffectChain(new EffectDescriptor(audioState.MasteringLimiter));
            }

            audioState.MasteringVoice?.EnableEffect(0);
        }
        /// <summary>
        /// Disables the mastering limiter
        /// </summary>
        private void DisableMasteringLimiter()
        {
            audioState.MasteringVoice?.DisableEffect(0);
        }

        /// <summary>
        /// Enables the reverb effect
        /// </summary>
        private void EnableReverb()
        {
            if (audioState.ReverbEffect == null)
            {
                audioState.ReverbEffect = new Reverb(audioState.XAudio2);
                audioState.ReverbVoice.SetEffectChain(new EffectDescriptor(audioState.ReverbEffect, 1));
            }

            audioState.ReverbVoice?.EnableEffect(0);
        }
        /// <summary>
        /// Disables the reverb effect
        /// </summary>
        private void DisableReverb()
        {
            audioState.ReverbVoice?.DisableEffect(0);
        }
        /// <summary>
        /// Enables the 3D audio instance
        /// </summary>
        public void EnableAudio3D(float speedOfSound = X3DAudio.SpeedOfSound)
        {
            if (audioState.X3DInstance == null)
            {
                audioState.X3DInstance = new X3DAudio(audioState.Speakers, speedOfSound);
            }
        }

        /// <summary>
        /// Internal buffer starts handler
        /// </summary>
        /// <param name="obj">Pointer</param>
        private void SourceVoice_BufferStart(IntPtr obj)
        {
            FireAudioStart();
        }
        /// <summary>
        /// Internal buffer ends handler
        /// </summary>
        /// <param name="obj">Pointer</param>
        private void SourceVoice_BufferEnd(IntPtr obj)
        {
            FireAudioEnd();
        }
        /// <summary>
        /// Internal loop ends handler
        /// </summary>
        /// <param name="obj">Pointer</param>
        private void SourceVoice_LoopEnd(IntPtr obj)
        {
            FireLoopEnd();
        }
        /// <summary>
        /// Fires the audio start event
        /// </summary>
        private void FireAudioStart()
        {
            Playing = true;
            AudioStart?.Invoke(this, new GameAudioEventArgs());
        }
        /// <summary>
        /// Fires the audio end event
        /// </summary>
        private void FireAudioEnd()
        {
            Playing = false;
            AudioEnd?.Invoke(this, new GameAudioEventArgs());
        }
        /// <summary>
        /// Fires the loop end event
        /// </summary>
        private void FireLoopEnd()
        {
            LoopEnd?.Invoke(this, new GameAudioEventArgs());
        }
    }
}
