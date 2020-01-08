using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.IO;

namespace Engine.Audio
{
    /// <summary>
    /// Audio effect
    /// </summary>
    public class GameAudioSound : IDisposable
    {
        /// <summary>
        /// File name
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// Wave format
        /// </summary>
        public WaveFormat WaveFormat { get; set; }
        /// <summary>
        /// Decoded packets info
        /// </summary>
        public uint[] DecodedPacketsInfo { get; set; }
        /// <summary>
        /// Default audio buffer
        /// </summary>
        public AudioBuffer AudioBuffer { get; set; }
        /// <summary>
        /// Looped audio buffer
        /// </summary>
        public AudioBuffer LoopedAudioBuffer { get; set; }
        /// <summary>
        /// Effect duration
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Loads a file in the audio buffer
        /// </summary>
        /// <param name="fileName">File name</param>
        public static GameAudioSound LoadFromFile(string fileName)
        {
            GameAudioSound sound = new GameAudioSound
            {
                FileName = fileName,
            };

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
        internal GameAudioSound()
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~GameAudioSound()
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
                AudioBuffer?.Stream.Dispose();
                AudioBuffer = null;

                AudioBuffer = null;
                LoopedAudioBuffer = null;
            }
        }

        /// <summary>
        /// Gets the wave samples duration.
        /// </summary>
        /// <returns>Wave samples duration or 0 (zero) if the format encoding is not known.</returns>
        public static long GetSamplesDuration(WaveFormat format, long audioBytes, uint[] decodedPacketsInfo)
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
