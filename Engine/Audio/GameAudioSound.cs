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
        /// Loads a file in the audio buffer
        /// </summary>
        /// <param name="fileName">File name</param>
        public static GameAudioSound LoadFromFile(string fileName)
        {
            return new GameAudioSound(fileName);
        }
        /// <summary>
        /// Calcs the sound duration
        /// </summary>
        /// <param name="format">Wave format</param>
        /// <param name="audioBytes">Number of bytes</param>
        /// <param name="decodedPacketsInfo">Decoded packets info</param>
        /// <returns>Returns the sound duration</returns>
        private static TimeSpan CalcSoundDuration(WaveFormat format, long audioBytes, uint[] decodedPacketsInfo)
        {
            TimeSpan duration = TimeSpan.Zero;
            if (format.SampleRate > 0)
            {
                var samplesDuration = GameAudioSound.GetSamplesDuration(
                    format,
                    audioBytes,
                    decodedPacketsInfo);

                var milliseconds = samplesDuration * 1000 / format.SampleRate;

                duration = TimeSpan.FromMilliseconds(milliseconds);
            }

            return duration;
        }
        /// <summary>
        /// Gets the wave samples duration.
        /// </summary>
        /// <param name="format">Wave format</param>
        /// <param name="audioBytes">Number of bytes</param>
        /// <param name="decodedPacketsInfo">Decoded packets info</param>
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

        /// <summary>
        /// Default audio buffer
        /// </summary>
        private readonly AudioBuffer audioBuffer;
        /// <summary>
        /// Looped audio buffer
        /// </summary>
        private readonly AudioBuffer loopedAudioBuffer;

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
        /// Effect duration
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">File name</param>
        internal GameAudioSound(string fileName)
        {
            using (var stream = new SoundStream(File.OpenRead(fileName)))
            {
                var buffer = stream.ToDataStream();

                WaveFormat = stream.Format;
                DecodedPacketsInfo = stream.DecodedPacketsInfo;
                Duration = CalcSoundDuration(stream.Format, buffer.Length, stream.DecodedPacketsInfo);

                audioBuffer = new AudioBuffer
                {
                    Stream = buffer,
                    AudioBytes = (int)buffer.Length,
                    Flags = BufferFlags.EndOfStream
                };
                loopedAudioBuffer = new AudioBuffer
                {
                    Stream = buffer,
                    AudioBytes = (int)buffer.Length,
                    Flags = BufferFlags.EndOfStream,
                    LoopCount = AudioBuffer.LoopInfinite,
                };
            }
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
                audioBuffer?.Stream.Dispose();

                loopedAudioBuffer?.Stream.Dispose();
            }
        }

        /// <summary>
        /// Gets the audio buffer
        /// </summary>
        /// <param name="looped">Looped flag</param>
        /// <returns>Returns the buffer to play</returns>
        public AudioBuffer GetAudioBuffer(bool looped)
        {
            return looped ? loopedAudioBuffer : audioBuffer;
        }
    }
}
