using SharpDX;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;
using System;

namespace Engine.Audio
{
    /// <summary>
    /// Effect instance
    /// </summary>
    public class GameAudioEffectInstance : IDisposable
    {
        private DspSettings dspSettings;
        private Emitter emitter;
        private Listener listener;
        private float[] reverbLevels;
        private bool isReverbSubmixEnabled;
        private float[] outputMatrix;

        private float pan;
        private bool paused;
        private float pitch;
        private float volume;
        private SourceVoice voice;

        /// <summary>
        /// Gets the base sound effect.
        /// </summary>
        public GameAudioEffect Effect { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance is looped.
        /// </summary>
        public bool IsLooped { get; set; }
        /// <summary>
        /// Gets or sets the pan value of the sound effect.
        /// </summary>
        /// <remarks>The value is clamped to (-1f, 1f) range.</remarks>
        /// <exception cref="ObjectDisposedException">Is thrown if the current instance was already disposed.</exception>
        public float Pan
        {
            get
            {
                return pan;
            }
            set
            {
                if (MathUtil.NearEqual(pan, value))
                {
                    return;
                }

                pan = MathUtil.Clamp(value, -1.0f, 1.0f);

                SetPanOutputMatrix();
            }
        }
        /// <summary>
        /// Gets or sets the pitch value of the sound effect.
        /// </summary>
        /// <remarks>The value is clamped to (-1f, 1f) range.</remarks>
        /// <exception cref="ObjectDisposedException">Is thrown if the current instance was already disposed.</exception>
        public float Pitch
        {
            get
            {
                return pitch;
            }
            set
            {
                if (MathUtil.NearEqual(pitch, value))
                {
                    return;
                }

                pitch = MathUtil.Clamp(value, -1.0f, 1.0f);

                voice.SetFrequencyRatio(XAudio2.SemitonesToFrequencyRatio(pitch));
            }
        }
        /// <summary>
        /// Gets the state of the current sound effect instance.
        /// </summary>
        public AudioState State
        {
            get
            {
                if (voice == null || voice.State.BuffersQueued == 0)
                {
                    return AudioState.Stopped;
                }

                if (paused)
                {
                    return AudioState.Paused;
                }

                return AudioState.Playing;
            }
        }
        /// <summary>
        /// Gets or sets the volume of the current sound effect instance.
        /// </summary>
        /// <remarks>The value is clamped to (0f, 1f) range.</remarks>
        /// <exception cref="ObjectDisposedException">Is thrown if the current instance was already disposed.</exception>
        public float Volume
        {
            get
            {
                return volume;
            }
            set
            {
                if (MathUtil.NearEqual(volume, value))
                {
                    return;
                }

                volume = MathUtil.Clamp(value, 0.0f, 1.0f);

                voice.SetVolume(volume);
            }
        }
        /// <summary>
        /// Emitter
        /// </summary>
        public GameAudioAgent EmitterAgent { get; private set; }
        /// <summary>
        /// Listener
        /// </summary>
        public GameAudioAgent ListenerAgent { get; private set; }

        /// <summary>
        /// Gets the current audio buffer.
        /// </summary>
        protected AudioBuffer CurrentAudioBuffer
        {
            get
            {
                if (this.Effect == null || this.Effect.AudioBuffer == null)
                {
                    return null;
                }

                return this.IsLooped ? this.Effect.LoopedAudioBuffer : this.Effect.AudioBuffer;
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
        /// <param name="soundEffect">Sound effect</param>
        /// <param name="sourceVoice">Source voice</param>
        internal GameAudioEffectInstance(GameAudioEffect soundEffect, SourceVoice sourceVoice)
        {
            Effect = soundEffect;
            voice = sourceVoice;
            paused = false;
            IsLooped = false;
            volume = 1.0f;
            pan = 0.0f;
            pitch = 0.0f;
            outputMatrix = null;

            EmitterAgent = new GameAudioAgent();
            ListenerAgent = new GameAudioAgent();

            voice.BufferStart += SourceVoice_BufferStart;
            voice.BufferEnd += SourceVoice_BufferEnd;
            voice.LoopEnd += SourceVoice_LoopEnd;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~GameAudioEffectInstance()
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
                if (voice != null)
                {
                    voice.BufferStart -= SourceVoice_BufferStart;
                    voice.BufferEnd -= SourceVoice_BufferEnd;
                    voice.LoopEnd -= SourceVoice_LoopEnd;
                }

                voice?.Stop(0);
                voice?.FlushSourceBuffers();
                if (isReverbSubmixEnabled)
                {
                    voice?.SetOutputVoices(null);
                    isReverbSubmixEnabled = false;
                }
                voice?.DestroyVoice();
                voice?.Dispose();
                voice = null;
            }
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        internal void Update()
        {
            if (this.Effect.GameAudio.UseReverb && !isReverbSubmixEnabled)
            {
                var sendFlags = this.Effect.GameAudio.UseReverbFilter ?
                    VoiceSendFlags.UseFilter :
                    VoiceSendFlags.None;

                var outputVoices = new VoiceSendDescriptor[]
                {
                    new VoiceSendDescriptor { OutputVoice = this.Effect.GameAudio.MasteringVoice, Flags = sendFlags },
                    new VoiceSendDescriptor { OutputVoice = this.Effect.GameAudio.ReverbVoice, Flags = sendFlags }
                };

                voice.SetOutputVoices(outputVoices);

                isReverbSubmixEnabled = true;
            }

            if (this.Effect.GameAudio.UseAudio3D)
            {
                this.Apply3D();
            }
        }

        /// <summary>
        /// Applies the 3D effect to the current sound effect instance.
        /// </summary>
        /// <param name="listenerAgent">Listener</param>
        /// <param name="emitterAgent">Emitter</param>
        private void Apply3D()
        {
            UpdateListener(ListenerAgent);
            UpdateEmitter(EmitterAgent);

            var flags = Calculate3DFlags();

            if (dspSettings == null)
            {
                dspSettings = new DspSettings(
                    this.Effect.WaveFormat.Channels,
                    this.Effect.GameAudio.MasteringVoice.VoiceDetails.InputChannelCount);
            }

            this.Effect.GameAudio.Calculate3D(listener, emitter, flags, dspSettings);

            voice.SetFrequencyRatio(dspSettings.DopplerFactor);

            voice.SetOutputMatrix(
                this.Effect.GameAudio.MasteringVoice,
                dspSettings.SourceChannelCount,
                dspSettings.DestinationChannelCount,
                dspSettings.MatrixCoefficients);

            if (!this.Effect.GameAudio.UseReverb)
            {
                return;
            }

            if (reverbLevels?.Length != this.Effect.WaveFormat.Channels)
            {
                reverbLevels = new float[this.Effect.WaveFormat.Channels];
            }

            for (int i = 0; i < reverbLevels.Length; i++)
            {
                reverbLevels[i] = dspSettings.ReverbLevel;
            }

            voice.SetOutputMatrix(this.Effect.GameAudio.ReverbVoice, this.Effect.WaveFormat.Channels, 1, reverbLevels);

            if (!this.Effect.GameAudio.UseReverbFilter)
            {
                return;
            }

            var filterDirect = new FilterParameters
            {
                Type = FilterType.LowPassFilter,
                Frequency = 2.0f * (float)Math.Sin(MathUtil.Pi / 6.0f * dspSettings.LpfDirectCoefficient),
                OneOverQ = 1.0f
            };

            voice.SetOutputFilterParameters(this.Effect.GameAudio.MasteringVoice, filterDirect);

            var filterReverb = new FilterParameters
            {
                Type = FilterType.LowPassFilter,
                Frequency = 2.0f * (float)Math.Sin(MathUtil.Pi / 6.0f * dspSettings.LpfReverbCoefficient),
                OneOverQ = 1.0f
            };

            voice.SetOutputFilterParameters(this.Effect.GameAudio.ReverbVoice, filterReverb);
        }
        /// <summary>
        /// Updates listener state
        /// </summary>
        /// <param name="listenerAgent">Listener state</param>
        private void UpdateListener(GameAudioAgent listenerAgent)
        {
            if (listener == null)
            {
                listener = new Listener();
            }

            listener.OrientFront = listenerAgent.Forward;
            listener.OrientTop = listenerAgent.Up;
            listener.Position = listenerAgent.Position;
            listener.Velocity = listenerAgent.Velocity;
        }
        /// <summary>
        /// Updates emitter state
        /// </summary>
        /// <param name="emitterAgent">Emitter state</param>
        private void UpdateEmitter(GameAudioAgent emitterAgent)
        {
            if (emitter == null)
            {
                emitter = new Emitter();
            }

            emitter.OrientFront = emitterAgent.Forward;
            emitter.OrientTop = emitterAgent.Up;
            emitter.Position = emitterAgent.Position;
            emitter.Velocity = emitterAgent.Velocity;
            emitter.DopplerScaler = GameAudio.DopplerScale;
            emitter.CurveDistanceScaler = GameAudio.DistanceScale;
            emitter.ChannelCount = this.Effect.WaveFormat.Channels;

            if (emitter.ChannelCount > 1)
            {
                emitter.ChannelAzimuths = new float[emitter.ChannelCount];
            }
        }
        /// <summary>
        /// Gets the 3D calculate flags
        /// </summary>
        /// <returns>Returns the 3D calculate flags</returns>
        private CalculateFlags Calculate3DFlags()
        {
            var flags =
                CalculateFlags.Matrix |
                CalculateFlags.Doppler |
                CalculateFlags.LpfDirect;

            if ((this.Effect.GameAudio.Speakers & Speakers.LowFrequency) > 0)
            {
                // On devices with an LFE channel, allow the mono source data to be routed to the LFE destination channel.
                flags |= CalculateFlags.RedirectToLfe;
            }

            if (this.Effect.GameAudio.UseReverb)
            {
                flags |= CalculateFlags.Reverb | CalculateFlags.LpfReverb;
            }

            return flags;
        }

        /// <summary>
        /// Pauses the playback of the current instance.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is thrown if the current instance was already disposed.</exception>
        public void Pause()
        {
            voice.Stop();
            paused = true;
        }
        /// <summary>
        /// Plays the current instance. If it is already playing - the call is ignored.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is thrown if the current instance was already disposed.</exception>
        public void Play()
        {
            if (State == AudioState.Playing)
            {
                return;
            }

            if (voice.State.BuffersQueued > 0)
            {
                voice.Stop();
                voice.FlushSourceBuffers();
            }

            voice.SubmitSourceBuffer(CurrentAudioBuffer, Effect.DecodedPacketsInfo);
            voice.Start();

            paused = false;
        }
        /// <summary>
        /// Resets the current instance.
        /// </summary>
        public void Reset()
        {
            Volume = 1.0f;
            Pitch = 0.0f;
            Pan = 0.0f;
            IsLooped = false;
        }
        /// <summary>
        /// Resumes playback of the current instance.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is thrown if the current instance was already disposed.</exception>
        public void Resume()
        {
            if (!IsLooped && voice.State.BuffersQueued == 0)
            {
                voice.Stop();
                voice.FlushSourceBuffers();
                voice.SubmitSourceBuffer(CurrentAudioBuffer, Effect.DecodedPacketsInfo);
            }

            voice.Start();
            paused = false;
        }
        /// <summary>
        /// Stops the playback of the current instance.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is thrown if the current instance was already disposed.</exception>
        public void Stop()
        {
            voice.Stop(0);
            voice.FlushSourceBuffers();

            paused = false;
        }
        /// <summary>
        /// Stops the playback of the current instance indicating whether the stop should occur immediately of at the end of the sound.
        /// </summary>
        /// <param name="immediate">A value indicating whether the playback should be stopped immediately or at the end of the sound.</param>
        /// <exception cref="ObjectDisposedException">Is thrown if the current instance was already disposed.</exception>
        public void Stop(bool immediate)
        {
            if (immediate)
            {
                voice.Stop(0);
            }
            else if (IsLooped)
            {
                voice.ExitLoop();
            }
            else
            {
                voice.Stop((int)PlayFlags.Tails);
            }

            paused = false;
        }

        /// <summary>
        /// Initializes the output matrix for the source voice
        /// </summary>
        /// <param name="destinationChannels">Resulting destination channels</param>
        /// <param name="sourceChannels">Resulting source channels</param>
        private void InitializeOutputMatrix(out int destinationChannels, out int sourceChannels)
        {
            destinationChannels = this.Effect.GameAudio.MasteringVoice.VoiceDetails.InputChannelCount;
            sourceChannels = this.Effect.WaveFormat.Channels;

            var outputMatrixSize = destinationChannels * sourceChannels;

            if (outputMatrix == null || outputMatrix.Length != outputMatrixSize)
            {
                outputMatrix = new float[outputMatrixSize];
            }

            // Default to full volume for all channels/destinations
            for (var i = 0; i < outputMatrix.Length; i++)
            {
                outputMatrix[i] = 1.0f;
            }
        }
        /// <summary>
        /// Sets the Pan output matrix in the source voice, based on the speakers configuration
        /// </summary>
        private void SetPanOutputMatrix()
        {
            InitializeOutputMatrix(out int destinationChannels, out int sourceChannels);

            if (pan != 0.0f)
            {
                var panLeft = 1.0f - pan;
                var panRight = 1.0f + pan;

                //The level sent from source channel S to destination channel D is specified in the form outputMatrix[SourceChannels × D + S]
                for (int S = 0; S < sourceChannels; S++)
                {
                    switch (this.Effect.GameAudio.Speakers)
                    {
                        case Speakers.Stereo:
                        case Speakers.TwoPointOne:
                        case Speakers.Surround:
                            outputMatrix[(sourceChannels * 0) + S] = panLeft;
                            outputMatrix[(sourceChannels * 1) + S] = panRight;
                            break;

                        case Speakers.Quad:
                            outputMatrix[(sourceChannels * 0) + S] = outputMatrix[(sourceChannels * 2) + S] = panLeft;
                            outputMatrix[(sourceChannels * 1) + S] = outputMatrix[(sourceChannels * 3) + S] = panRight;
                            break;

                        case Speakers.FourPointOne:
                            outputMatrix[(sourceChannels * 0) + S] = outputMatrix[(sourceChannels * 3) + S] = panLeft;
                            outputMatrix[(sourceChannels * 1) + S] = outputMatrix[(sourceChannels * 4) + S] = panRight;
                            break;

                        case Speakers.FivePointOne:
                        case Speakers.SevenPointOne:
                        case Speakers.FivePointOneSurround:
                            outputMatrix[(sourceChannels * 0) + S] = outputMatrix[(sourceChannels * 4) + S] = panLeft;
                            outputMatrix[(sourceChannels * 1) + S] = outputMatrix[(sourceChannels * 5) + S] = panRight;
                            break;

                        case Speakers.SevenPointOneSurround:
                            outputMatrix[(sourceChannels * 0) + S] = outputMatrix[(sourceChannels * 4) + S] = outputMatrix[(sourceChannels * 6) + S] = panLeft;
                            outputMatrix[(sourceChannels * 1) + S] = outputMatrix[(sourceChannels * 5) + S] = outputMatrix[(sourceChannels * 7) + S] = panRight;
                            break;

                        case Speakers.Mono:
                        default:
                            // don't do any panning here
                            break;
                    }
                }
            }

            voice.SetOutputMatrix(sourceChannels, destinationChannels, outputMatrix);
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
            AudioStart?.Invoke(this, new GameAudioEventArgs());
        }
        /// <summary>
        /// Fires the audio end event
        /// </summary>
        private void FireAudioEnd()
        {
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
