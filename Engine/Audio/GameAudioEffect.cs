using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.IO;

namespace Engine.Audio
{
    /// <summary>
    /// Audio effect
    /// </summary>
    public class GameAudioEffect : IDisposable
    {
        /// <summary>
        /// To delete instance list
        /// </summary>
        private readonly List<GameAudioEffectInstance> toDelete = new List<GameAudioEffectInstance>();

        public GameAudio GameAudio { get; set; }
        public string Name { get; set; }
        public WaveFormat WaveFormat { get; set; }
        public uint[] DecodedPacketsInfo { get; set; }
        public AudioBuffer AudioBuffer { get; set; }
        public AudioBuffer LoopedAudioBuffer { get; set; }
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Loads a file in the audio buffer
        /// </summary>
        /// <param name="gameAudio">Game audio</param>
        /// <param name="name">Effect name</param>
        /// <param name="fileName">File name</param>
        internal static GameAudioEffect Load(GameAudio gameAudio, string name, string fileName)
        {
            GameAudioEffect effect = new GameAudioEffect(gameAudio, name);

            using (var stream = new SoundStream(File.OpenRead(fileName)))
            {
                var buffer = stream.ToDataStream();

                effect.WaveFormat = stream.Format;
                effect.DecodedPacketsInfo = stream.DecodedPacketsInfo;
                effect.AudioBuffer = new AudioBuffer
                {
                    Stream = buffer,
                    AudioBytes = (int)buffer.Length,
                    Flags = BufferFlags.EndOfStream
                };
                effect.LoopedAudioBuffer = new AudioBuffer
                {
                    Stream = buffer,
                    AudioBytes = (int)buffer.Length,
                    Flags = BufferFlags.EndOfStream,
                    LoopCount = AudioBuffer.LoopInfinite,
                };
                effect.Duration = TimeSpan.Zero;
                if (stream.Format.SampleRate > 0)
                {
                    var samplesDuration = GetSamplesDuration(
                        stream.Format,
                        buffer.Length,
                        stream.DecodedPacketsInfo);

                    var milliseconds = samplesDuration * 1000 / stream.Format.SampleRate;

                    effect.Duration = TimeSpan.FromMilliseconds(milliseconds);
                }
            }

            return effect;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Effect name</param>
        internal GameAudioEffect(GameAudio gameAudio, string name)
        {
            this.GameAudio = gameAudio;
            this.Name = name;
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
                toDelete.ForEach(i => i.Dispose());
                toDelete.Clear();

                AudioBuffer?.Stream.Dispose();
                AudioBuffer = null;

                AudioBuffer = null;
                LoopedAudioBuffer = null;
            }
        }

        /// <summary>
        /// Creates a new effect instance
        /// </summary>
        /// <returns>Returns the new created instance</returns>
        public GameAudioEffectInstance Create()
        {
            var sourceVoice = this.GameAudio.CreateVoice(this.WaveFormat);

            var instance = new GameAudioEffectInstance(this, sourceVoice);

            toDelete.Add(instance);

            return instance;
        }

        /// <summary>
        /// Gets the wave samples duration.
        /// </summary>
        /// <returns>Wave samples duration or 0 (zero) if the format encoding is not known.</returns>
        private static long GetSamplesDuration(WaveFormat format, long audioBytes, uint[] decodedPacketsInfo)
        {
            switch (format.Encoding)
            {
                case WaveFormatEncoding.Adpcm:
                    var adpcmFormat = format as WaveFormatAdpcm;
                    long duration = (audioBytes / adpcmFormat.BlockAlign) * adpcmFormat.SamplesPerBlock;
                    long partial = audioBytes % adpcmFormat.BlockAlign;
                    if (partial >= (7 * adpcmFormat.Channels))
                    {
                        duration += (partial * 2) / (adpcmFormat.Channels - 12);
                    }

                    return duration;

                case WaveFormatEncoding.Wmaudio2:
                case WaveFormatEncoding.Wmaudio3:
                    if (decodedPacketsInfo != null)
                    {
                        return decodedPacketsInfo[decodedPacketsInfo.Length - 1] / format.Channels;
                    }
                    break;

                case WaveFormatEncoding.Pcm:
                    if (format.BitsPerSample > 0)
                    {
                        return (audioBytes) * 8 / (format.BitsPerSample * format.Channels);
                    }
                    break;
            }

            return 0;
        }
    }
}
