using SharpDX;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;
using SharpDX.XAudio2.Fx;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Engine.Audio
{
    /// <summary>
    /// Effect instance
    /// </summary>
    public class GameAudioEffect : IDisposable
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
        private SourceVoice sourceVoice;
        private SubmixVoice submixVoice;
        private bool useReverb = false;
        private ReverbPresets? reverbPreset = null;
        private readonly bool destroyWhenFinished;

        /// <summary>
        /// Gets the current audio buffer.
        /// </summary>
        protected AudioBuffer CurrentAudioBuffer
        {
            get
            {
                if (this.Sound == null || this.Sound.AudioBuffer == null)
                {
                    return null;
                }

                return this.IsLooped ? this.Sound.LoopedAudioBuffer : this.Sound.AudioBuffer;
            }
        }

        /// <summary>
        /// Gets the base sound.
        /// </summary>
        public GameAudioSound Sound { get; private set; }
        /// <summary>
        /// Attached mastering voice
        /// </summary>
        public MasteringVoice MasteringVoice
        {
            get { return this.Sound.GameAudio.MasteringVoice; }
        }
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

                sourceVoice.SetFrequencyRatio(XAudio2.SemitonesToFrequencyRatio(pitch));
            }
        }
        /// <summary>
        /// Gets the state of the current sound effect instance.
        /// </summary>
        public AudioState State
        {
            get
            {
                if (sourceVoice == null || sourceVoice.State.BuffersQueued == 0)
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
                sourceVoice.GetVolume(out float volume);
                return volume;
            }
            set
            {
                float volume = MathUtil.Clamp(value, 0.0f, 1.0f);
                sourceVoice.SetVolume(volume);
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
        /// Gets or sets whether the master voice uses 3D audio or not
        /// </summary>
        public bool UseAudio3D { get; set; } = false;
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
                if (reverbPreset == value)
                {
                    return;
                }

                reverbPreset = value;

                if (this.submixVoice == null)
                {
                    return;
                }

                var reverbParam = GameAudioPresets.Convert(reverbPreset ?? ReverbPresets.Default);

                this.submixVoice.SetEffectParameters(0, reverbParam);
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
        /// <param name="sound">Sound effect</param>
        /// <param name="emitterDescription">Emitter description</param>
        /// <param name="listenerDescription">Listener description</param>
        /// <param name="destroyWhenFinished">Sets whether the instance must be disposed after it's finished</param>
        internal GameAudioEffect(
            GameAudioSound sound,
            GameAudioSourceDescription emitterDescription,
            GameAudioSourceDescription listenerDescription,
            bool destroyWhenFinished)
        {
            Sound = sound;
            paused = false;
            IsLooped = false;
            pan = 0.0f;
            pitch = 0.0f;
            outputMatrix = null;

            Emitter = new GameAudioEmitter(emitterDescription);
            Listener = new GameAudioListener(listenerDescription);

            sourceVoice = this.Sound.GameAudio.CreateSourceVoice(this.Sound.WaveFormat);

            // Play the wave using a source voice that sends to both the submix and mastering voices
            var sendDescriptors = new[]
            {
                // LPF direct-path
                new VoiceSendDescriptor { Flags = VoiceSendFlags.UseFilter, OutputVoice = sound.GameAudio.MasteringVoice },
            };

            sourceVoice.SetOutputVoices(sendDescriptors);

            sourceVoice.BufferStart += SourceVoice_BufferStart;
            sourceVoice.BufferEnd += SourceVoice_BufferEnd;
            sourceVoice.LoopEnd += SourceVoice_LoopEnd;

            this.destroyWhenFinished = destroyWhenFinished;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~GameAudioEffect()
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
                if (sourceVoice != null)
                {
                    sourceVoice.BufferStart -= SourceVoice_BufferStart;
                    sourceVoice.BufferEnd -= SourceVoice_BufferEnd;
                    sourceVoice.LoopEnd -= SourceVoice_LoopEnd;
                }

                sourceVoice?.Stop(0);
                sourceVoice?.FlushSourceBuffers();
                if (isReverbSubmixEnabled)
                {
                    sourceVoice?.SetOutputVoices(null);
                    isReverbSubmixEnabled = false;
                }
                sourceVoice?.DestroyVoice();
                sourceVoice?.Dispose();
                sourceVoice = null;

                this.submixVoice?.DestroyVoice();
                this.submixVoice?.Dispose();
                this.submixVoice = null;
            }
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        internal void Update()
        {
            if (this.UseAudio3D)
            {
                Apply3D();
            }
            else
            {
                SetPanOutputMatrix();
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
                    this.Sound.WaveFormat.Channels,
                    this.Sound.GameAudio.InputChannelCount);
            }

            this.Sound.GameAudio.Calculate3D(listener, emitter, flags, dspSettings);

            sourceVoice.SetFrequencyRatio(dspSettings.DopplerFactor);

            sourceVoice.SetOutputMatrix(
                this.MasteringVoice,
                dspSettings.SourceChannelCount,
                dspSettings.DestinationChannelCount,
                dspSettings.MatrixCoefficients);

            sourceVoice.SetOutputFilterParameters(
                this.MasteringVoice,
                new FilterParameters
                {
                    Type = FilterType.LowPassFilter,
                    Frequency = 2.0f * (float)Math.Sin(MathUtil.Pi / 6.0f * dspSettings.LpfDirectCoefficient),
                    OneOverQ = 1.0f
                });

            if (!this.UseReverb)
            {
                return;
            }

            if (reverbLevels?.Length != this.Sound.WaveFormat.Channels)
            {
                reverbLevels = new float[this.Sound.WaveFormat.Channels];
            }

            for (int i = 0; i < reverbLevels.Length; i++)
            {
                reverbLevels[i] = dspSettings.ReverbLevel;
            }

            sourceVoice.SetOutputMatrix(this.submixVoice, this.Sound.WaveFormat.Channels, 1, reverbLevels);

            if (!this.UseReverbFilter)
            {
                return;
            }

            sourceVoice.SetOutputFilterParameters(
                this.submixVoice,
                new FilterParameters
                {
                    Type = FilterType.LowPassFilter,
                    Frequency = 2.0f * (float)Math.Sin(MathUtil.Pi / 6.0f * dspSettings.LpfReverbCoefficient),
                    OneOverQ = 1.0f
                });
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

            emitter.ChannelCount = this.Sound.WaveFormat.Channels;
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

            if ((this.Sound.GameAudio.Speakers & Speakers.LowFrequency) > 0)
            {
                // On devices with an LFE channel, allow the mono source data to be routed to the LFE destination channel.
                flags |= CalculateFlags.RedirectToLfe;
            }

            if (this.UseReverb)
            {
                flags |= CalculateFlags.Reverb | CalculateFlags.LpfReverb;
            }

            return flags;
        }
        /// <summary>
        /// Gets the destination voice
        /// </summary>
        /// <returns>Returns the destination voice</returns>
        private Voice GetDestinationVoice()
        {
            if (this.UseReverb)
            {
                return this.submixVoice;
            }
            else
            {
                return this.Sound.GameAudio.MasteringVoice;
            }
        }

        /// <summary>
        /// Pauses the playback of the current instance.
        /// </summary>
        public void Pause()
        {
            sourceVoice.Stop();
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

            if (sourceVoice.State.BuffersQueued > 0)
            {
                sourceVoice.Stop();
                sourceVoice.FlushSourceBuffers();
            }

            sourceVoice.SubmitSourceBuffer(CurrentAudioBuffer, Sound.DecodedPacketsInfo);

            sourceVoice.Start();

            this.Update();

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
            if (!IsLooped && sourceVoice.State.BuffersQueued == 0)
            {
                sourceVoice.Stop();
                sourceVoice.FlushSourceBuffers();
                sourceVoice.SubmitSourceBuffer(CurrentAudioBuffer, Sound.DecodedPacketsInfo);
            }

            sourceVoice.Start();
            paused = false;
        }
        /// <summary>
        /// Stops the playback of the current instance.
        /// </summary>
        public void Stop()
        {
            sourceVoice.Stop(0);
            sourceVoice.FlushSourceBuffers();

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
                sourceVoice.ExitLoop();
            }

            Stop();
        }

        /// <summary>
        /// Enables the reverb effect
        /// </summary>
        private void EnableReverb()
        {
            if (this.submixVoice == null)
            {
#if DEBUG
                var reverbEffect = this.Sound.GameAudio.CreateReverb(true);
#else
                var reverbEffect = this.Sound.GameAudio.CreateReverb();
#endif
                using (reverbEffect)
                {
                    //Get input data from mastering voice
                    int outputChannels = this.Sound.GameAudio.InputChannelCount;
                    int outputSampleRate = this.Sound.GameAudio.InputSampleRate;
                    var sendFlags = this.UseReverbFilter ? SubmixVoiceFlags.UseFilter : SubmixVoiceFlags.None;

                    this.submixVoice = this.Sound.GameAudio.CreatesSubmixVoice(
                        outputChannels,
                        outputSampleRate,
                        sendFlags,
                        0);

                    this.submixVoice.SetEffectChain(new EffectDescriptor(reverbEffect, outputChannels)
                    {
                        InitialState = true,
                    });
                }
            }

            var sendDescriptors = new[]
            {
                // LPF direct-path
                new VoiceSendDescriptor { Flags = VoiceSendFlags.UseFilter, OutputVoice = this.Sound.GameAudio.MasteringVoice },
                // LPF reverb-path -- omit for better performance at the cost of less realistic occlusion
                new VoiceSendDescriptor { Flags = VoiceSendFlags.UseFilter, OutputVoice = submixVoice },
            };

            sourceVoice.SetOutputVoices(sendDescriptors);

            this.submixVoice.EnableEffect(0);
        }
        /// <summary>
        /// Disables the reverb effect
        /// </summary>
        private void DisableReverb()
        {
            // Play the wave using a source voice that sends to both the submix and mastering voices
            var sendDescriptors = new[]
            {
                // LPF direct-path
                new VoiceSendDescriptor { Flags = VoiceSendFlags.UseFilter, OutputVoice = this.Sound.GameAudio.MasteringVoice },
            };

            sourceVoice.SetOutputVoices(sendDescriptors);

            this.submixVoice?.DisableEffect(0);
        }

        /// <summary>
        /// Initializes the output matrix for the source voice
        /// </summary>
        /// <param name="destinationChannels">Resulting destination channels</param>
        /// <param name="sourceChannels">Resulting source channels</param>
        private void InitializeOutputMatrix(out int destinationChannels, out int sourceChannels)
        {
            var voiceDst = this.GetDestinationVoice();

            destinationChannels = voiceDst.VoiceDetails.InputChannelCount;
            sourceChannels = this.sourceVoice.VoiceDetails.InputChannelCount;

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

            float panLeft = 0.5f - (pan * 0.5f);
            float panRight = 0.5f + (pan * 0.5f);

            //The level sent from source channel S to destination channel D is specified in the form outputMatrix[SourceChannels × D + S]
            for (int s = 0; s < sourceChannels; s++)
            {
                switch ((AudioSpeakers)this.Sound.GameAudio.Speakers)
                {
                    case AudioSpeakers.Mono:
                        outputMatrix[(sourceChannels * 0) + s] = 1;
                        break;

                    case AudioSpeakers.Stereo:
                    case AudioSpeakers.Surround:
                        outputMatrix[(sourceChannels * 0) + s] = panLeft;
                        outputMatrix[(sourceChannels * 1) + s] = panRight;
                        break;

                    case AudioSpeakers.Quad:
                        outputMatrix[(sourceChannels * 0) + s] = outputMatrix[(sourceChannels * 2) + s] = panLeft;
                        outputMatrix[(sourceChannels * 1) + s] = outputMatrix[(sourceChannels * 3) + s] = panRight;
                        break;

                    case AudioSpeakers.FivePointOne:
                    case AudioSpeakers.FivePointOneSurround:
                    case AudioSpeakers.SevenPointOne:
                        outputMatrix[(sourceChannels * 0) + s] = outputMatrix[(sourceChannels * 4) + s] = panLeft;
                        outputMatrix[(sourceChannels * 1) + s] = outputMatrix[(sourceChannels * 5) + s] = panRight;
                        break;

                    case AudioSpeakers.SevenPointOneSurround:
                        outputMatrix[(sourceChannels * 0) + s] = outputMatrix[(sourceChannels * 4) + s] = outputMatrix[(sourceChannels * 6) + s] = panLeft;
                        outputMatrix[(sourceChannels * 1) + s] = outputMatrix[(sourceChannels * 5) + s] = outputMatrix[(sourceChannels * 7) + s] = panRight;
                        break;

                    default:
                        // don't do any panning here
                        break;
                }
            }

            var voiceDst = this.GetDestinationVoice();

            sourceVoice.SetOutputMatrix(voiceDst, sourceChannels, destinationChannels, outputMatrix);
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

        /// <summary>
        /// Gets the output matrix configuration
        /// </summary>
        /// <returns>Returns an array of floats from 0 to 1.</returns>
        public float[] GetOutputMatrix()
        {
            ReadOnlyCollection<float> rc;

            if (UseAudio3D)
            {
                rc = new ReadOnlyCollection<float>(dspSettings.MatrixCoefficients);
            }
            else
            {
                rc = new ReadOnlyCollection<float>(outputMatrix);
            }

            return rc.ToArray();
        }
    }
}
