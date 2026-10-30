using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Engine.Audio
{
    using Engine.Mathematics;
    using Vortice.XAudio2;

    /// <summary>
    /// Game audio effect
    /// </summary>
    class GameAudioEffect : IGameAudioEffect
    {
        private const int WaitPrecision = 1;

        private readonly GameAudio gameAudio;
        private readonly GameAudioFile audioFile;

        private readonly IXAudio2SourceVoice sourceVoice;
        private readonly uint voiceInputChannels;

        private float pan;
        private float[] panOutputMatrix;
        private float pitch;

        private IXAudio2SubmixVoice submixVoice;
        private GameAudioReverbPresets? reverbPreset;
        private DspSettings dspSettings;

        private Listener listener;
        private Emitter emitter;

        private readonly Stopwatch clock = new();
        private readonly ManualResetEvent playEvent = new(false);
        private readonly ManualResetEvent waitForPlayToOutput = new(false);
        private readonly AutoResetEvent bufferEndEvent = new(false);
        private TimeSpan playPosition;
        private TimeSpan nextPlayPosition;
        private int playCounter;
        private bool disposed = false;

        /// <inheritdoc/>
        public bool IsLooped { get; set; }
        /// <inheritdoc/>
        public float Pan => pan;
        /// <inheritdoc/>
        public float Pitch => pitch;
        /// <inheritdoc/>
        public float Volume => sourceVoice.Volume;

        /// <inheritdoc/>
        public void SetPan(float value)
        {
            value = Math.Clamp(value, -1.0f, 1.0f);

            if (Helper.NearEqual(pan, value))
            {
                return;
            }

            pan = value;

            UpdateOutputMatrix();
        }
        /// <inheritdoc/>
        public void SetPitch(float value)
        {
            value = Math.Clamp(value, -1.0f, 1.0f);

            if (Helper.NearEqual(pitch, value))
            {
                return;
            }

            pitch = value;

            float targetRatio = Math.Clamp(
                MathF.Pow(2.0f, value / 12.0f),
                XAudio2.MinimumFrequencyRatio,
                XAudio2.MaximumFilterFrequency);

            sourceVoice.SetFrequencyRatio(targetRatio, gameAudio.OperationSet);
        }
        /// <inheritdoc/>
        public void SetVolume(float value)
        {
            sourceVoice.SetVolume(Math.Clamp(value, 0.0f, 1.0f), gameAudio.OperationSet);
        }

        /// <inheritdoc/>
        public bool UseAudio3D { get; set; }
        /// <inheritdoc/>
        public IGameAudioEmitter Emitter { get; set; }
        /// <inheritdoc/>
        public IGameAudioListener Listener { get; set; }

        /// <inheritdoc/>
        public TimeSpan Duration
        {
            get { return audioFile.Duration; }
        }
        /// <inheritdoc/>
        public GameAudioState State { get; private set; } = GameAudioState.Stopped;
        /// <inheritdoc/>
        public bool DueToDispose { get; private set; }
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public event GameAudioHandler AudioStart;
        /// <inheritdoc/>
        public event GameAudioHandler AudioEnd;
        /// <inheritdoc/>
        public event GameAudioHandler LoopEnd;
        /// <inheritdoc/>
        public event GameAudioProgressHandler PlayProgress;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameAudioEffect" /> class.
        /// </summary>
        /// <param name="audioState">Audio state</param>
        /// <param name="fileName">File name</param>
        /// <param name="effectParameters">Effect parameters</param>
        public GameAudioEffect(GameAudio audioState, string fileName, GameAudioEffectParameters effectParameters)
        {
            gameAudio = audioState;

            // Read in the file
            audioFile = new GameAudioFile(fileName);
            voiceInputChannels = (uint)audioFile.WaveFormat.Channels;

            // Create the source voice
            sourceVoice = audioState.CreateSourceVoice(audioFile.WaveFormat, true);
            sourceVoice.BufferEnd += SourceVoiceBufferEnd;

            // LPF direct-path
            var sendDescriptor = new VoiceSendDescriptor { Flags = VoiceSendFlags.UseFilter, OutputVoice = audioState.MasteringVoice };
            sourceVoice.SetOutputVoices(new Span<VoiceSendDescriptor>(ref sendDescriptor));

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
            SetPan(effectParameters.Pan);
            SetPitch(effectParameters.Pitch);
            SetVolume(effectParameters.Volume);

            if (effectParameters.ReverbPreset.HasValue)
            {
                SetReverb(effectParameters.ReverbPreset);
            }

            InitializeOutputMatrix();

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

                string fileName = audioFile?.FileName ?? "AlreadyDisposed.file";

                Game.Logger.WriteTrace(this, $"{fileName} Dispose Begin");

                sourceVoice?.Stop(0);
                sourceVoice?.DestroyVoice();
                sourceVoice?.Dispose();

                submixVoice?.DestroyVoice();
                submixVoice?.Dispose();

                audioFile?.Dispose();

                Game.Logger.WriteTrace(this, $"{fileName} Dispose End");
            }
        }

        /// <inheritdoc/>
        public void Play()
        {
            Play(TimeSpan.Zero);
        }
        /// <inheritdoc/>
        public void Play(TimeSpan start)
        {
            if (disposed)
            {
                return;
            }

            if (State == GameAudioState.Stopped)
            {
                nextPlayPosition = start;

                sourceVoice.Start(0);

                playCounter++;
                waitForPlayToOutput.Reset();
                State = GameAudioState.Playing;
                playEvent.Set();
                waitForPlayToOutput.WaitOne();
            }
            else if (State == GameAudioState.Paused)
            {
                Resume();
            }
        }
        /// <inheritdoc/>
        public void Stop(bool immediate = true)
        {
            if (disposed)
            {
                return;
            }

            if (State != GameAudioState.Stopped)
            {
                sourceVoice.Stop(0);

                playPosition = TimeSpan.Zero;
                nextPlayPosition = TimeSpan.Zero;
                playCounter++;

                clock.Stop();
                State = GameAudioState.Stopped;
                playEvent.Reset();
            }
        }
        /// <inheritdoc/>
        public void Pause()
        {
            if (disposed)
            {
                return;
            }

            if (State == GameAudioState.Playing)
            {
                sourceVoice.Stop();

                clock.Stop();
                State = GameAudioState.Paused;
                playEvent.Reset();
            }
        }
        /// <inheritdoc/>
        public void Resume()
        {
            if (disposed)
            {
                return;
            }

            if (State == GameAudioState.Paused)
            {
                sourceVoice.Start();

                clock.Start();
                State = GameAudioState.Playing;
                playEvent.Set();
            }
        }
        /// <inheritdoc/>
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
            AudioStart?.Invoke(this, new GameAudioEventArgs(this));

            DueToDispose = false;

            PlaySound();
        }
        /// <summary>
        /// Plays the sound
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
                        Game.Logger.WriteTrace(this, "playEvent.WaitOne - Waiting for play");
                        break;
                    }
                }

                // Playing all the samples
                PlayAllSamples(out bool endOfSong);

                // If the song is not looping (by default), then stop the audio player.
                if (State == GameAudioState.Playing && endOfSong)
                {
                    if (!IsLooped)
                    {
                        AudioEnd?.Invoke(this, new GameAudioEventArgs(this));
                        Stop();
                        DueToDispose = true;
                    }
                    else
                    {
                        LoopEnd?.Invoke(this, new GameAudioEventArgs(this));
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

                // Fires the progress event
                PlayProgress?.Invoke(this, new GameAudioProgressEventArgs(this, Duration, playPosition));
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
            if (State == GameAudioState.Stopped)
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
            if (State == GameAudioState.Stopped)
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

                Game.Logger.WriteTrace(this, "waitForPlayToOutput.Set (First time)");
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
            while (State != GameAudioState.Stopped)
            {
                // While the player is not stopped, wait for the play event
                if (playEvent.WaitOne(WaitPrecision))
                {
                    Game.Logger.WriteTrace(this, "playEvent.WaitOne - Waiting for play");
                    break;
                }
            }
        }
        /// <summary>
        /// Waits for the buffer end event
        /// </summary>
        private void WaitBufferEnd()
        {
            while (State != GameAudioState.Stopped && !disposed && sourceVoice.State.BuffersQueued == audioFile.BufferCount)
            {
                bufferEndEvent.WaitOne(WaitPrecision);
            }
            Game.Logger.WriteTrace(this, "bufferEndEvent.WaitOne - Load new buffer");
        }

        /// <inheritdoc/>
        public void Update(IGameTime gameTime)
        {
            if (!UseAudio3D)
            {
                return;
            }

            UpdateListener(gameTime.ElapsedSeconds);
            UpdateEmitter(gameTime.ElapsedSeconds);

            Calculate3D();

            UpdateVoices(gameAudio.OperationSet);
        }
        /// <summary>
        /// Updates listener state
        /// </summary>
        /// <param name="elapsedSeconds">Elapsed seconds</param>
        private void UpdateListener(float elapsedSeconds)
        {
            listener ??= new Listener()
            {
                OrientFront = (System.Numerics.Vector3)Listener.Forward,
                OrientTop = (System.Numerics.Vector3)Listener.Up,
                Position = (System.Numerics.Vector3)Listener.Position,
                Velocity = (System.Numerics.Vector3)Vector3.Zero,
            };

            if (elapsedSeconds > 0)
            {
                listener.Velocity = (System.Numerics.Vector3)(Listener.Position - (Vector3)listener.Position) / elapsedSeconds;
            }

            listener.OrientFront = (System.Numerics.Vector3)Listener.Forward;
            listener.OrientTop = (System.Numerics.Vector3)Listener.Up;
            listener.Position = (System.Numerics.Vector3)Listener.Position;

            if (Listener.Cone.HasValue)
            {
                listener.Cone = new Cone
                {
                    InnerAngle = Listener.Cone.Value.InnerAngle,
                    InnerVolume = Listener.Cone.Value.InnerVolume,
                    InnerLpf = Listener.Cone.Value.InnerLpf,
                    InnerReverb = Listener.Cone.Value.InnerReverb,

                    OuterAngle = Listener.Cone.Value.OuterAngle,
                    OuterVolume = Listener.Cone.Value.OuterVolume,
                    OuterLpf = Listener.Cone.Value.OuterLpf,
                    OuterReverb = Listener.Cone.Value.OuterReverb
                };
            }
            else
            {
                listener.Cone = null;
            }
        }
        /// <summary>
        /// Updates emitter state
        /// </summary>
        /// <param name="elapsedSeconds">Elapsed seconds</param>
        private void UpdateEmitter(float elapsedSeconds)
        {
            emitter ??= new Emitter()
            {
                OrientFront = (System.Numerics.Vector3)Emitter.Forward,
                OrientTop = (System.Numerics.Vector3)Emitter.Up,
                Position = (System.Numerics.Vector3)Emitter.Position,
                Velocity = (System.Numerics.Vector3)Vector3.Zero
            };

            if (elapsedSeconds > 0)
            {
                emitter.Velocity = (System.Numerics.Vector3)(Emitter.Position - (Vector3)emitter.Position) / elapsedSeconds;
            }

            emitter.Position = (System.Numerics.Vector3)Emitter.Position;
            emitter.OrientFront = (System.Numerics.Vector3)Emitter.Forward;
            emitter.OrientTop = (System.Numerics.Vector3)Emitter.Up;

            emitter.ChannelCount = (uint)audioFile.WaveFormat.Channels;
            emitter.ChannelRadius = 1;
            if (emitter.ChannelCount > 1)
            {
                emitter.ChannelAzimuths = new float[emitter.ChannelCount];
            }

            emitter.InnerRadius = Emitter.InnerRadius;
            emitter.InnerRadiusAngle = Emitter.InnerRadiusAngle;

            emitter.VolumeCurve = VorticeExtensions.ConvertCurve(Emitter.VolumeCurve);
            emitter.LfeCurve = VorticeExtensions.ConvertCurve(Emitter.LfeCurve);
            emitter.ReverbCurve = VorticeExtensions.ConvertCurve(Emitter.ReverbCurve);

            emitter.CurveDistanceScaler = GameAudio.DistanceScale * Emitter.Radius;
            emitter.DopplerScaler = GameAudio.DopplerScale;

            if (Emitter.Cone.HasValue)
            {
                emitter.Cone = new Cone
                {
                    InnerAngle = Emitter.Cone.Value.InnerAngle,
                    InnerVolume = Emitter.Cone.Value.InnerVolume,
                    InnerLpf = Emitter.Cone.Value.InnerLpf,
                    InnerReverb = Emitter.Cone.Value.InnerReverb,

                    OuterAngle = Emitter.Cone.Value.OuterAngle,
                    OuterVolume = Emitter.Cone.Value.OuterVolume,
                    OuterLpf = Emitter.Cone.Value.OuterLpf,
                    OuterReverb = Emitter.Cone.Value.OuterReverb
                };
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
        /// <param name="elapsedSeconds">Elapsed time</param>
        private void Calculate3D()
        {
            var flags = Calculate3DFlags();

            dspSettings ??= new DspSettings((uint)audioFile.WaveFormat.Channels, gameAudio.InputChannelCount);

            gameAudio.Calculate3D(listener, emitter, flags, dspSettings);
        }
        /// <summary>
        /// Updates the instance voices
        /// </summary>
        private void UpdateVoices(uint operationSet)
        {
            if (sourceVoice == null)
            {
                return;
            }

            UpdateOutputMatrix();

            var outputMatrix = GetOutputMatrix();

            // Apply X3DAudio generated DSP settings to XAudio2
            sourceVoice.SetFrequencyRatio(dspSettings.DopplerFactor, operationSet);

            sourceVoice.SetOutputMatrix(
                gameAudio.MasteringVoice,
                voiceInputChannels,
                gameAudio.InputChannelCount,
                outputMatrix);

            sourceVoice.SetOutputFilterParameters(
                gameAudio.MasteringVoice,
                new FilterParameters
                {
                    Type = FilterType.LowPassFilter,
                    Frequency = 2.0f * MathF.Sin(Helper.Pi / 6.0f * dspSettings.LpfDirectCoefficient),
                    OneOverQ = 1.0f
                },
                operationSet);

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
                outputMatrix);

            sourceVoice.SetOutputFilterParameters(
                submixVoice,
                new FilterParameters
                {
                    Type = FilterType.LowPassFilter,
                    Frequency = 2.0f * MathF.Sin(Helper.Pi / 6.0f * dspSettings.LpfDirectCoefficient),
                    OneOverQ = 1.0f
                },
                operationSet);
        }

        /// <inheritdoc/>
        public GameAudioReverbPresets? GetReverb()
        {
            return reverbPreset;
        }
        /// <inheritdoc/>
        public bool SetReverb(GameAudioReverbPresets? reverb)
        {
            submixVoice ??= gameAudio.CreateReverbVoice();

            if (!reverbPreset.HasValue && reverb.HasValue)
            {
                // Play the wave using a source voice that sends to both the submix and mastering voices
                VoiceSendDescriptor[] sendDescriptors =
                [
                    // LPF direct-path
                    new VoiceSendDescriptor { Flags = VoiceSendFlags.UseFilter, OutputVoice = gameAudio.MasteringVoice },
                    // LPF reverb-path -- omit for better performance at the cost of less realistic occlusion
                    new VoiceSendDescriptor { Flags = VoiceSendFlags.UseFilter, OutputVoice = submixVoice },
                ];
                sourceVoice.SetOutputVoices(sendDescriptors);
            }
            else if (!reverb.HasValue)
            {
                // Play the wave using a source voice that sends to both the submix and mastering voices
                VoiceSendDescriptor[] sendDescriptors =
                [
                    // LPF direct-path
                    new VoiceSendDescriptor { Flags = VoiceSendFlags.UseFilter, OutputVoice = gameAudio.MasteringVoice },
                ];
                sourceVoice.SetOutputVoices(sendDescriptors);

                submixVoice.DisableEffect(0, gameAudio.OperationSet);
            }

            if (reverb.HasValue)
            {
                submixVoice.SetEffectParameters(0, GameAudioPresets.Convert(reverb.Value), true, gameAudio.OperationSet);
                submixVoice.EnableEffect(0, gameAudio.OperationSet);
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
            return [.. panOutputMatrix];
        }
        /// <summary>
        /// Initializes the output matrix
        /// </summary>
        private void InitializeOutputMatrix()
        {
            if (panOutputMatrix?.Length > 0) return;

            IXAudio2Voice activeVoice = reverbPreset.HasValue ? submixVoice : gameAudio.MasteringVoice;

            uint destinationChannels = activeVoice.VoiceDetails.InputChannels;
            uint sourceChannels = sourceVoice.VoiceDetails.InputChannels;

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

            uint sourceChannels = sourceVoice.VoiceDetails.InputChannels;

            float panLeft = 0.5f - (pan * 0.5f);
            float panRight = 0.5f + (pan * 0.5f);

            //The level sent from source channel S to destination channel D is specified in the form outputMatrix[SourceChannels × D + S]
            for (uint s = 0; s < sourceChannels; s++)
            {
                switch (gameAudio.GetAudioSpeakers())
                {
                    case GameAudioSpeakers.Mono:
                        panOutputMatrix[(sourceChannels * 0) + s] = 1 * outputMatrix[s];
                        break;

                    case GameAudioSpeakers.Stereo:
                    case GameAudioSpeakers.Surround:
                        panOutputMatrix[(sourceChannels * 0) + s] = panLeft * 2f * outputMatrix[(sourceChannels * 0) + s];
                        panOutputMatrix[(sourceChannels * 1) + s] = panRight * 2f * outputMatrix[(sourceChannels * 1) + s];
                        break;

                    case GameAudioSpeakers.Quad:
                        panOutputMatrix[(sourceChannels * 0) + s] = panOutputMatrix[(sourceChannels * 2) + s] = panLeft * 2f * outputMatrix[(sourceChannels * 0) + s];
                        panOutputMatrix[(sourceChannels * 1) + s] = panOutputMatrix[(sourceChannels * 3) + s] = panRight * 2f * outputMatrix[(sourceChannels * 1) + s];
                        break;

                    case GameAudioSpeakers.FivePointOne:
                    case GameAudioSpeakers.FivePointOneSurround:
                    case GameAudioSpeakers.SevenPointOne:
                        panOutputMatrix[(sourceChannels * 0) + s] = panOutputMatrix[(sourceChannels * 4) + s] = panLeft * 2f * outputMatrix[(sourceChannels * 0) + s];
                        panOutputMatrix[(sourceChannels * 1) + s] = panOutputMatrix[(sourceChannels * 5) + s] = panRight * 2f * outputMatrix[(sourceChannels * 1) + s];
                        break;

                    case GameAudioSpeakers.SevenPointOneSurround:
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
