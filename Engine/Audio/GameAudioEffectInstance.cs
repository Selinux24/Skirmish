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
        private readonly bool destroyWhenFinished;

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
                value = MathUtil.Clamp(value, -1.0f, 1.0f);

                if (MathUtil.NearEqual(pan, value))
                {
                    return;
                }

                pan = value;

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
                value = MathUtil.Clamp(value, -1.0f, 1.0f);

                if (MathUtil.NearEqual(pitch, value))
                {
                    return;
                }

                pitch = value;

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
                value = MathUtil.Clamp(value, 0.0f, 1.0f);

                if (MathUtil.NearEqual(volume, value))
                {
                    return;
                }

                volume = value;

                voice.SetVolume(volume);
            }
        }
        /// <summary>
        /// Emitter
        /// </summary>
        public GameAudioEmitter Emitter { get; private set; }
        /// <summary>
        /// Listener
        /// </summary>
        public GameAudioListener Listener { get; private set; }
        /// <summary>
        /// The instance is due to dispose
        /// </summary>
        public bool DueToDispose { get; private set; } = false;

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
        /// <param name="emitterDescription">Emitter description</param>
        /// <param name="listenerDescription">Listener description</param>
        /// <param name="destroyWhenFinished">Sets whether the instance must be disposed after it's finished</param>
        internal GameAudioEffectInstance(
            GameAudioEffect soundEffect,
            SourceVoice sourceVoice,
            GameAudioSourceDescription emitterDescription,
            GameAudioSourceDescription listenerDescription,
            bool destroyWhenFinished)
        {
            Effect = soundEffect;
            voice = sourceVoice;
            paused = false;
            IsLooped = false;
            volume = 1.0f;
            pan = 0.0f;
            pitch = 0.0f;
            outputMatrix = null;

            Emitter = new GameAudioEmitter(emitterDescription);
            Listener = new GameAudioListener(listenerDescription);

            voice.BufferStart += SourceVoice_BufferStart;
            voice.BufferEnd += SourceVoice_BufferEnd;
            voice.LoopEnd += SourceVoice_LoopEnd;

            this.destroyWhenFinished = destroyWhenFinished;
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
            UpdateListener(Listener);
            UpdateEmitter(Emitter);

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
        /// <param name="audioListener">Listener state</param>
        private void UpdateListener(GameAudioListener audioListener)
        {
            if (listener == null)
            {
                listener = new Listener();
            }

            listener.OrientFront = audioListener.Forward;
            listener.OrientTop = audioListener.Up;
            listener.Position = audioListener.Position;
            listener.Velocity = audioListener.Velocity;

            if (audioListener.Cone.HasValue)
            {
                if (listener.Cone == null)
                {
                    listener.Cone = new Cone();
                }

                listener.Cone.InnerAngle = audioListener.Cone.Value.InnerAngle;
                listener.Cone.InnerVolume = audioListener.Cone.Value.InnerVolume;
                listener.Cone.InnerLpf = audioListener.Cone.Value.InnerLpf;
                listener.Cone.InnerReverb = audioListener.Cone.Value.InnerReverb;

                listener.Cone.OuterAngle = audioListener.Cone.Value.OuterAngle;
                listener.Cone.OuterVolume = audioListener.Cone.Value.OuterVolume;
                listener.Cone.OuterLpf = audioListener.Cone.Value.OuterLpf;
                listener.Cone.OuterReverb = audioListener.Cone.Value.OuterReverb;
            }
        }
        /// <summary>
        /// Updates emitter state
        /// </summary>
        /// <param name="audioEmitter">Emitter state</param>
        private void UpdateEmitter(GameAudioEmitter audioEmitter)
        {
            if (emitter == null)
            {
                emitter = new Emitter();
            }

            emitter.Position = audioEmitter.Position;
            emitter.OrientFront = audioEmitter.Forward;
            emitter.OrientTop = audioEmitter.Up;
            emitter.Velocity = audioEmitter.Velocity;

            emitter.ChannelCount = this.Effect.WaveFormat.Channels;
            emitter.ChannelRadius = 1;
            if (emitter.ChannelCount > 1)
            {
                emitter.ChannelAzimuths = new float[emitter.ChannelCount];
            }

            emitter.InnerRadius = 2;
            emitter.InnerRadiusAngle = MathUtil.PiOverFour;

            emitter.VolumeCurve = GameAudioPresets.DefaultLinearCurve;
            emitter.LfeCurve = GameAudioPresets.DefaultEmitterLfeCurve;
            emitter.ReverbCurve = GameAudioPresets.DefaultEmitterReverbCurve;

            emitter.CurveDistanceScaler = GameAudio.DistanceScale * audioEmitter.Radius;
            emitter.DopplerScaler = GameAudio.DopplerScale;

            if (audioEmitter.Cone.HasValue)
            {
                if (emitter.Cone == null)
                {
                    emitter.Cone = new Cone();
                }

                emitter.Cone.InnerAngle = audioEmitter.Cone.Value.InnerAngle;
                emitter.Cone.InnerVolume = audioEmitter.Cone.Value.InnerVolume;
                emitter.Cone.InnerLpf = audioEmitter.Cone.Value.InnerLpf;
                emitter.Cone.InnerReverb = audioEmitter.Cone.Value.InnerReverb;

                emitter.Cone.OuterAngle = audioEmitter.Cone.Value.OuterAngle;
                emitter.Cone.OuterVolume = audioEmitter.Cone.Value.OuterVolume;
                emitter.Cone.OuterLpf = audioEmitter.Cone.Value.OuterLpf;
                emitter.Cone.OuterReverb = audioEmitter.Cone.Value.OuterReverb;
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
        public void Pause()
        {
            voice.Stop();
            paused = true;
        }
        /// <summary>
        /// Plays the current instance. If it is already playing - the call is ignored.
        /// </summary>
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

            this.Update();

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
        public void Stop()
        {
            voice.Stop(0);
            voice.FlushSourceBuffers();

            paused = false;

            FireAudioEnd();
        }
        /// <summary>
        /// Stops the playback of the current instance indicating whether the stop should occur immediately of at the end of the sound.
        /// </summary>
        /// <param name="immediate">A value indicating whether the playback should be stopped immediately or at the end of the sound.</param>
        public void Stop(bool immediate)
        {
            if (immediate && IsLooped)
            {
                voice.ExitLoop();
            }

            Stop();
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

            if (destroyWhenFinished)
            {
                this.DueToDispose = true;
            }
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
