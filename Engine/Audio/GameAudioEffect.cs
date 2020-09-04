using SharpDX;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio effect
    /// </summary>
    class GameAudioEffect : IAudioEffect
    {
        private const int WaitPrecision = 1;

        private readonly GameAudio gameAudio;
        private readonly IAudioFile audioFile;

        private readonly SourceVoice sourceVoice;
        private readonly int voiceInputChannels;

        private float pan;
        private float[] panOutputMatrix;
        private float pitch;

        private SubmixVoice submixVoice;
        private ReverbPresets? reverbPreset;
        private DspSettings dspSettings;

        private Listener listener;
        private Emitter emitter;

        private readonly Stopwatch clock = new Stopwatch();
        private readonly ManualResetEvent playEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent waitForPlayToOutput = new ManualResetEvent(false);
        private readonly AutoResetEvent bufferEndEvent = new AutoResetEvent(false);
        private TimeSpan playPosition;
        private TimeSpan nextPlayPosition;
        private int playCounter;
        private bool disposed = false;

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

                UpdateOutputMatrix();
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
        /// Gets or sets whether the master voice uses 3D audio or not
        /// </summary>
        public bool UseAudio3D { get; set; }
        /// <summary>
        /// Emitter
        /// </summary>
        public IGameAudioEmitter Emitter { get; set; }
        /// <summary>
        /// Listener
        /// </summary>
        public IGameAudioListener Listener { get; set; }

        /// <summary>
        /// Gets the duration in seconds of the current sound.
        /// </summary>
        /// <value>The duration.</value>
        public TimeSpan Duration
        {
            get { return audioFile.Duration; }
        }
        /// <summary>
        /// Gets the state of this instance.
        /// </summary>
        /// <value>The state.</value>
        public AudioState State { get; private set; } = AudioState.Stopped;
        /// <summary>
        /// The instance is due to dispose
        /// </summary>
        public bool DueToDispose { get; private set; }
        /// <summary>
        /// Gets or sets the position in seconds.
        /// </summary>
        /// <value>The position.</value>
        public TimeSpan Position
        {
            get { return playPosition; }
            set
            {
                playPosition = value;
                nextPlayPosition = value;
                clock.Restart();
                playCounter++;
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
        /// Initializes a new instance of the <see cref="GameAudioEffect" /> class.
        /// </summary>
        /// <param name="audioState">Audio state</param>
        /// <param name="fileName">File name</param>
        /// <param name="effectParameters">Effect parameters</param>
        public GameAudioEffect(GameAudio audioState, IAudioFile audioFile, GameAudioEffectParameters effectParameters)
        {
            gameAudio = audioState;

            // Read in the file
            this.audioFile = audioFile;
            voiceInputChannels = this.audioFile.WaveFormat.Channels;

            // Create the source voice
            sourceVoice = audioState.CreateSourceVoice(this.audioFile.WaveFormat, true);
            sourceVoice.BufferEnd += SourceVoiceBufferEnd;

            // LPF direct-path
            var sendDescriptor = new VoiceSendDescriptor { Flags = VoiceSendFlags.UseFilter, OutputVoice = audioState.MasteringVoice };
            sourceVoice.SetOutputVoices(sendDescriptor);

            UseAudio3D = effectParameters.UseAudio3D;
            if (UseAudio3D)
            {
                Emitter = new GameAudioEmitter()
                {
                    Radius = effectParameters.EmitterRadius,
                    Cone = effectParameters.EmitterCone,
                    InnerRadius = effectParameters.EmitterInnerRadius,
                    InnerRadiusAngle = effectParameters.EmitterInnerRadiusAngle,
                    VolumeCurve = effectParameters.EmitterVolumeCurve,
                    LfeCurve = effectParameters.EmitterLfeCurve,
                    ReverbCurve = effectParameters.EmitterReverbCurve,
                };
                Listener = new GameAudioListener()
                {
                    Cone = effectParameters.ListenerCone,
                };
            }

            IsLooped = effectParameters.IsLooped;
            Pan = effectParameters.Pan;
            Pitch = effectParameters.Pitch;
            Volume = effectParameters.Volume;

            // Starts the playing thread
            Task.Factory.StartNew(PlayAsync, TaskCreationOptions.LongRunning);
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
                disposed = true;

                Logger.WriteDebug("DisposePlayer Begin");

                submixVoice?.DestroyVoice();
                submixVoice?.Dispose();

                sourceVoice?.Stop(0);
                sourceVoice?.DestroyVoice();
                sourceVoice?.Dispose();

                Logger.WriteDebug("DisposePlayer End");
            }
        }

        /// <summary>
        /// Plays the current instance. If it is already playing - the call is ignored.
        /// </summary>
        public void Play()
        {
            if (State == AudioState.Stopped)
            {
                sourceVoice.Start(0);

                playCounter++;
                waitForPlayToOutput.Reset();
                State = AudioState.Playing;
                playEvent.Set();
                waitForPlayToOutput.WaitOne();
            }
            else if (State == AudioState.Paused)
            {
                Resume();
            }
        }
        /// <summary>
        /// Stops the playback of the current instance indicating whether the stop should occur immediately of at the end of the sound.
        /// </summary>
        /// <param name="immediate">A value indicating whether the playback should be stopped immediately or at the end of the sound.</param>
        public void Stop(bool immediate = true)
        {
            if (State != AudioState.Stopped)
            {
                sourceVoice.Stop(0);

                playPosition = TimeSpan.Zero;
                nextPlayPosition = TimeSpan.Zero;
                playCounter++;

                clock.Stop();
                State = AudioState.Stopped;
                playEvent.Reset();
            }
        }
        /// <summary>
        /// Pauses the playback of the current instance.
        /// </summary>
        public void Pause()
        {
            if (State == AudioState.Playing)
            {
                sourceVoice.Stop();

                clock.Stop();
                State = AudioState.Paused;
                playEvent.Reset();
            }
        }
        /// <summary>
        /// Resumes playback of the current instance.
        /// </summary>
        public void Resume()
        {
            if (State == AudioState.Paused)
            {
                sourceVoice.Start();

                clock.Start();
                State = AudioState.Playing;
                playEvent.Set();
            }
        }
        /// <summary>
        /// Resets the current instance.
        /// </summary>
        public void Reset()
        {
            Stop();
            Play();
        }

        /// <summary>
        /// Internal method to play the sound.
        /// </summary>
        private void PlayAsync()
        {
            try
            {
                AudioStart?.Invoke(this, new GameAudioEventArgs());

                DueToDispose = false;

                PlaySound();
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message);

                throw;
            }
        }
        /// <summary>
        /// Plays de sound
        /// </summary>
        private void PlaySound()
        {
            while (true)
            {
                // Check that this instanced is not disposed
                if (disposed)
                {
                    break;
                }

                while (true)
                {
                    if (playEvent.WaitOne(WaitPrecision))
                    {
                        Logger.WriteDebug("playEvent.WaitOne - Waiting for play");
                        break;
                    }
                }

                // Playing all the samples
                PlayAllSamples(out bool endOfSong);

                // If the song is not looping (by default), then stop the audio player.
                if (State == AudioState.Playing && endOfSong)
                {
                    if (!IsLooped)
                    {
                        AudioEnd?.Invoke(this, new GameAudioEventArgs());
                        Stop();
                        DueToDispose = true;
                    }
                    else
                    {
                        LoopEnd?.Invoke(this, new GameAudioEventArgs());
                    }
                }
            }
        }
        /// <summary>
        /// Plays all sound samples
        /// </summary>
        /// <param name="endOfSound">End of sound flag</param>
        private void PlayAllSamples(out bool endOfSound)
        {
            clock.Restart();
            var playPositionStart = nextPlayPosition;
            playPosition = playPositionStart;
            int currentPlayCounter = playCounter;

            // Get the decoded samples from the specified starting position.
            audioFile.SetPosition(playPositionStart);

            bool isFirstTime = true;

            while (true)
            {
                if (!Sample(isFirstTime, playPositionStart, currentPlayCounter, out endOfSound))
                {
                    break;
                }

                isFirstTime = false;
            }
        }
        /// <summary>
        /// Plays a sample
        /// </summary>
        /// <param name="isFirstTime">Is first time</param>
        /// <param name="currentPlayCounter">Current play counter</param>
        /// <param name="endOfSound">End of sound flag</param>
        /// <returns>Returns true if there are more samples to play. Returns false otherwise.</returns>
        private bool Sample(bool isFirstTime, TimeSpan playPositionStart, int currentPlayCounter, out bool endOfSound)
        {
            endOfSound = false;

            if (disposed)
            {
                return false;
            }

            WaitForPlay();

            // If the player is stopped, then break of this loop
            if (State == AudioState.Stopped)
            {
                nextPlayPosition = TimeSpan.Zero;
                return false;
            }

            // If there was a change in the play position, restart the sample iterator.
            if (currentPlayCounter != playCounter)
            {
                return false;
            }

            // If the player is not stopped and the buffer queue is full, wait for the end of a buffer.
            WaitBufferEnd();

            // If the player is stopped or disposed, then break of this loop
            if (State == AudioState.Stopped)
            {
                nextPlayPosition = TimeSpan.Zero;
                return false;
            }

            // If there was a change in the play position, restart the sample iterator.
            if (currentPlayCounter != playCounter)
            {
                return false;
            }

            // Retrieve a pointer to the sample data
            if (!audioFile.GetNextAudioBuffer(out var audioBuffer))
            {
                endOfSound = true;
                return false;
            }

            // If this is a first play, restart the clock and notify play method.
            if (isFirstTime)
            {
                clock.Restart();

                Logger.WriteDebug("waitForPlayToOutput.Set (First time)");
                waitForPlayToOutput.Set();
            }

            // Update the current position used for sync
            playPosition = playPositionStart + clock.Elapsed;

            if (disposed)
            {
                return false;
            }

            // Submit the audio buffer to xaudio2
            sourceVoice.SubmitSourceBuffer(audioBuffer, null);

            return true;
        }
        /// <summary>
        /// Waits for the play event
        /// </summary>
        private void WaitForPlay()
        {
            while (State != AudioState.Stopped)
            {
                // While the player is not stopped, wait for the play event
                if (playEvent.WaitOne(WaitPrecision))
                {
                    Logger.WriteDebug("playEvent.WaitOne - Waiting for play");
                    break;
                }
            }
        }
        /// <summary>
        /// Waits for the buffer end event
        /// </summary>
        private void WaitBufferEnd()
        {
            while (State != AudioState.Stopped && !disposed && sourceVoice.State.BuffersQueued == audioFile.BufferCount)
            {
                bufferEndEvent.WaitOne(WaitPrecision);
            }
            Logger.WriteDebug("bufferEndEvent.WaitOne - Load new buffer");
        }

        /// <summary>
        /// Apply 3d configuration to voice
        /// </summary>
        public void Apply3D(float elapsedSeconds)
        {
            if (!UseAudio3D)
            {
                return;
            }

            UpdateListener();
            UpdateEmitter();

            Calculate3D(elapsedSeconds);

            UpdateVoices();
        }
        /// <summary>
        /// Updates listener state
        /// </summary>
        /// <param name="audioListener">Listener state</param>
        private void UpdateListener()
        {
            if (listener == null)
            {
                listener = new Listener();
            }

            listener.OrientFront = Listener.Forward;
            listener.OrientTop = Listener.Up;
            listener.Position = Listener.Position;
            listener.Velocity = Listener.Velocity;

            if (Listener.Cone.HasValue)
            {
                if (listener.Cone == null)
                {
                    listener.Cone = new Cone();
                }

                listener.Cone.InnerAngle = Listener.Cone.Value.InnerAngle;
                listener.Cone.InnerVolume = Listener.Cone.Value.InnerVolume;
                listener.Cone.InnerLpf = Listener.Cone.Value.InnerLpf;
                listener.Cone.InnerReverb = Listener.Cone.Value.InnerReverb;

                listener.Cone.OuterAngle = Listener.Cone.Value.OuterAngle;
                listener.Cone.OuterVolume = Listener.Cone.Value.OuterVolume;
                listener.Cone.OuterLpf = Listener.Cone.Value.OuterLpf;
                listener.Cone.OuterReverb = Listener.Cone.Value.OuterReverb;
            }
            else
            {
                listener.Cone = null;
            }
        }
        /// <summary>
        /// Updates emitter state
        /// </summary>
        /// <param name="audioEmitter">Emitter state</param>
        private void UpdateEmitter()
        {
            if (emitter == null)
            {
                emitter = new Emitter();
            }

            emitter.Position = Emitter.Position;
            emitter.OrientFront = Emitter.Forward;
            emitter.OrientTop = Emitter.Up;
            emitter.Velocity = Emitter.Velocity;

            emitter.ChannelCount = audioFile.WaveFormat.Channels;
            emitter.ChannelRadius = 1;
            if (emitter.ChannelCount > 1)
            {
                emitter.ChannelAzimuths = new float[emitter.ChannelCount];
            }

            emitter.InnerRadius = Emitter.InnerRadius;
            emitter.InnerRadiusAngle = Emitter.InnerRadiusAngle;

            emitter.VolumeCurve = GameAudioCurvePoint.ConvertCurve(Emitter.VolumeCurve);
            emitter.LfeCurve = GameAudioCurvePoint.ConvertCurve(Emitter.LfeCurve);
            emitter.ReverbCurve = GameAudioCurvePoint.ConvertCurve(Emitter.ReverbCurve);

            emitter.CurveDistanceScaler = GameAudio.DistanceScale * Emitter.Radius;
            emitter.DopplerScaler = GameAudio.DopplerScale;

            if (Emitter.Cone.HasValue)
            {
                if (emitter.Cone == null)
                {
                    emitter.Cone = new Cone();
                }

                emitter.Cone.InnerAngle = Emitter.Cone.Value.InnerAngle;
                emitter.Cone.InnerVolume = Emitter.Cone.Value.InnerVolume;
                emitter.Cone.InnerLpf = Emitter.Cone.Value.InnerLpf;
                emitter.Cone.InnerReverb = Emitter.Cone.Value.InnerReverb;

                emitter.Cone.OuterAngle = Emitter.Cone.Value.OuterAngle;
                emitter.Cone.OuterVolume = Emitter.Cone.Value.OuterVolume;
                emitter.Cone.OuterLpf = Emitter.Cone.Value.OuterLpf;
                emitter.Cone.OuterReverb = Emitter.Cone.Value.OuterReverb;
            }
            else
            {
                emitter.Cone = null;
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

            if (gameAudio.UseRedirectToLFE)
            {
                // On devices with an LFE channel, allow the mono source data to be routed to the LFE destination channel.
                flags |= CalculateFlags.RedirectToLfe;
            }

            if (reverbPreset.HasValue)
            {
                flags |= CalculateFlags.Reverb | CalculateFlags.LpfReverb;
            }

            return flags;
        }
        /// <summary>
        /// Calculates instance positions
        /// </summary>
        /// <param name="elapsedSeconds">Elpased time</param>
        private void Calculate3D(float elapsedSeconds)
        {
            if (elapsedSeconds > 0)
            {
                listener.Velocity = (Listener.Position - listener.Position) / elapsedSeconds;

                emitter.Velocity = (Emitter.Position - emitter.Position) / elapsedSeconds;
            }

            var flags = Calculate3DFlags();

            if (dspSettings == null)
            {
                dspSettings = new DspSettings(
                    audioFile.WaveFormat.Channels,
                    gameAudio.InputChannelCount);
            }

            gameAudio.Calculate3D(listener, emitter, flags, dspSettings);
        }
        /// <summary>
        /// Updates the instance voices
        /// </summary>
        private void UpdateVoices()
        {
            if (sourceVoice == null)
            {
                return;
            }

            UpdateOutputMatrix();

            // Apply X3DAudio generated DSP settings to XAudio2
            sourceVoice.SetFrequencyRatio(dspSettings.DopplerFactor);

            sourceVoice.SetOutputMatrix(
                gameAudio.MasteringVoice,
                voiceInputChannels,
                gameAudio.InputChannelCount,
                GetOutputMatrix());

            sourceVoice.SetOutputFilterParameters(
                gameAudio.MasteringVoice,
                new FilterParameters
                {
                    Type = FilterType.LowPassFilter,
                    Frequency = 2.0f * (float)Math.Sin(MathUtil.Pi / 6.0f * dspSettings.LpfDirectCoefficient),
                    OneOverQ = 1.0f
                });

            if (!reverbPreset.HasValue)
            {
                return;
            }

            if (submixVoice == null)
            {
                return;
            }

            sourceVoice.SetOutputMatrix(
                submixVoice,
                voiceInputChannels,
                gameAudio.InputChannelCount,
                GetOutputMatrix());

            sourceVoice.SetOutputFilterParameters(
                submixVoice,
                new FilterParameters
                {
                    Type = FilterType.LowPassFilter,
                    Frequency = 2.0f * (float)Math.Sin(MathUtil.Pi / 6.0f * dspSettings.LpfDirectCoefficient),
                    OneOverQ = 1.0f
                });
        }

        /// <summary>
        /// Creates a new reverb voice
        /// </summary>
        private void CreateReverbVoice()
        {
            // Create reverb effect
            using (var reverbEffect = gameAudio.CreateReverb())
            {
                // Create a submix voice
                submixVoice = gameAudio.CreatesSubmixVoice(gameAudio.InputChannelCount, gameAudio.InputSampleRate);

                // Performance tip: you need not run global FX with the sample number
                // of channels as the final mix.  For example, this sample runs
                // the reverb in mono mode, thus reducing CPU overhead.
                var desc = new EffectDescriptor(reverbEffect)
                {
                    InitialState = true,
                    OutputChannelCount = gameAudio.InputChannelCount,
                };
                submixVoice.SetEffectChain(desc);
            }
        }
        /// <summary>
        /// Gets the reverb effect
        /// </summary>
        public ReverbPresets? GetReverb()
        {
            return reverbPreset;
        }
        /// <summary>
        /// Set reverb to voice
        /// </summary>
        /// <param name="reverb">Reverb index</param>
        public bool SetReverb(ReverbPresets? reverb)
        {
            if (submixVoice == null)
            {
                CreateReverbVoice();
            }

            if (reverb.HasValue)
            {
                // Play the wave using a source voice that sends to both the submix and mastering voices
                VoiceSendDescriptor[] sendDescriptors = new[]
                {
                    // LPF direct-path
                    new VoiceSendDescriptor { Flags = VoiceSendFlags.UseFilter, OutputVoice = gameAudio.MasteringVoice },
                    // LPF reverb-path -- omit for better performance at the cost of less realistic occlusion
                    new VoiceSendDescriptor { Flags = VoiceSendFlags.UseFilter, OutputVoice = submixVoice },
                };
                sourceVoice.SetOutputVoices(sendDescriptors);

                var native = GameAudioPresets.Convert(reverb.Value, submixVoice.VoiceDetails.InputSampleRate);
                submixVoice.SetEffectParameters(0, native);
                submixVoice.EnableEffect(0);
            }
            else
            {
                // Play the wave using a source voice that sends to both the submix and mastering voices
                VoiceSendDescriptor[] sendDescriptors = new[]
                {
                    // LPF direct-path
                    new VoiceSendDescriptor { Flags = VoiceSendFlags.UseFilter, OutputVoice = gameAudio.MasteringVoice },
                };
                sourceVoice.SetOutputVoices(sendDescriptors);

                submixVoice.DisableEffect(0);
            }

            reverbPreset = reverb;

            return true;
        }

        /// <summary>
        /// Gets the output matrix configuration
        /// </summary>
        /// <returns>Returns an array of floats from 0 to 1.</returns>
        public float[] GetOutputMatrix()
        {
            return panOutputMatrix.ToArray();
        }
        /// <summary>
        /// Initializes the output matrix
        /// </summary>
        private void InitializeOutputMatrix()
        {
            if (panOutputMatrix?.Length > 0)
            {
                return;
            }

            var activeVoice = reverbPreset.HasValue ? submixVoice : (Voice)gameAudio.MasteringVoice;

            int destinationChannels = activeVoice.VoiceDetails.InputChannelCount;
            int sourceChannels = sourceVoice.VoiceDetails.InputChannelCount;

            panOutputMatrix = new float[destinationChannels * sourceChannels];

            // Default to full volume for all channels/destinations
            for (var i = 0; i < panOutputMatrix.Length; i++)
            {
                panOutputMatrix[i] = 1.0f;
            }
        }
        /// <summary>
        /// Updates the output matrix
        /// </summary>
        private void UpdateOutputMatrix()
        {
            InitializeOutputMatrix();

            var outputMatrix = dspSettings.MatrixCoefficients.ToArray();

            int sourceChannels = sourceVoice.VoiceDetails.InputChannelCount;

            float panLeft = 0.5f - (pan * 0.5f);
            float panRight = 0.5f + (pan * 0.5f);

            //The level sent from source channel S to destination channel D is specified in the form outputMatrix[SourceChannels × D + S]
            for (int s = 0; s < sourceChannels; s++)
            {
                switch ((AudioSpeakers)gameAudio.Speakers)
                {
                    case AudioSpeakers.Mono:
                        panOutputMatrix[(sourceChannels * 0) + s] = 1 * outputMatrix[s];
                        break;

                    case AudioSpeakers.Stereo:
                    case AudioSpeakers.Surround:
                        panOutputMatrix[(sourceChannels * 0) + s] = panLeft * 2f * outputMatrix[(sourceChannels * 0) + s];
                        panOutputMatrix[(sourceChannels * 1) + s] = panRight * 2f * outputMatrix[(sourceChannels * 1) + s];
                        break;

                    case AudioSpeakers.Quad:
                        panOutputMatrix[(sourceChannels * 0) + s] = panOutputMatrix[(sourceChannels * 2) + s] = panLeft * 2f * outputMatrix[(sourceChannels * 0) + s];
                        panOutputMatrix[(sourceChannels * 1) + s] = panOutputMatrix[(sourceChannels * 3) + s] = panRight * 2f * outputMatrix[(sourceChannels * 1) + s];
                        break;

                    case AudioSpeakers.FivePointOne:
                    case AudioSpeakers.FivePointOneSurround:
                    case AudioSpeakers.SevenPointOne:
                        panOutputMatrix[(sourceChannels * 0) + s] = panOutputMatrix[(sourceChannels * 4) + s] = panLeft * 2f * outputMatrix[(sourceChannels * 0) + s];
                        panOutputMatrix[(sourceChannels * 1) + s] = panOutputMatrix[(sourceChannels * 5) + s] = panRight * 2f * outputMatrix[(sourceChannels * 1) + s];
                        break;

                    case AudioSpeakers.SevenPointOneSurround:
                        panOutputMatrix[(sourceChannels * 0) + s] = panOutputMatrix[(sourceChannels * 4) + s] = panOutputMatrix[(sourceChannels * 6) + s] = panLeft * 2f * outputMatrix[(sourceChannels * 0) + s];
                        panOutputMatrix[(sourceChannels * 1) + s] = panOutputMatrix[(sourceChannels * 5) + s] = panOutputMatrix[(sourceChannels * 7) + s] = panRight * 2f * outputMatrix[(sourceChannels * 1) + s];
                        break;

                    default:
                        // don't do any panning here
                        break;
                }
            }
        }

        /// <summary>
        /// On source voice buffer ends
        /// </summary>
        /// <param name="obj">Data pointer</param>
        private void SourceVoiceBufferEnd(IntPtr obj)
        {
            bufferEndEvent.Set();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{audioFile.FileName}";
        }
    }
}
