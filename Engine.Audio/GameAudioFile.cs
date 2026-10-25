using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace Engine.Audio
{
    using Vortice.Multimedia;
    using Vortice.XAudio2;

    /// <summary>
    /// Game audio file
    /// </summary>
    class GameAudioFile : IDisposable
    {
        private const int DefaultBufferCount = 3;
        private const int DefaultBufferSize = 32 * 1024; //32Kb

        /// <summary>
        /// Loads a file
        /// </summary>
        /// <param name="fileName">File name</param>
        public static GameAudioFile LoadFromFile(string fileName)
        {
            return new GameAudioFile(fileName);
        }

        private int currentSample = 0;
        private readonly AudioFileReader audioDecoder;
        private readonly AudioBuffer[] audioBuffers;
        private readonly byte[][] memBuffers;
        private int nextBuffer = 0;

        /// <summary>
        /// File name
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// Wave format
        /// </summary>
        public WaveFormat WaveFormat { get; private set; }
        /// <summary>
        /// Sound duration
        /// </summary>
        public TimeSpan Duration
        {
            get { return audioDecoder.TotalTime; }
        }
        /// <summary>
        /// Buffer count
        /// </summary>
        public int BufferCount { get; private set; } = DefaultBufferCount;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">File name</param>
        public GameAudioFile(string fileName)
        {
            FileName = fileName;

            // Initialize NAudio universal decoder
            audioDecoder = new AudioFileReader(fileName);

            // Convert and map the NAudio format to the native Vortice WaveFormat
            WaveFormat = WaveFormat.CreateCustomFormat(
                (WaveFormatEncoding)audioDecoder.WaveFormat.Encoding,
                audioDecoder.WaveFormat.SampleRate,
                audioDecoder.WaveFormat.Channels,
                audioDecoder.WaveFormat.AverageBytesPerSecond,
                audioDecoder.WaveFormat.BlockAlign,
                audioDecoder.WaveFormat.BitsPerSample
            );

            // Initialize the circle buffer struct
            audioBuffers = new AudioBuffer[DefaultBufferCount];
            memBuffers = new byte[DefaultBufferCount][];

            for (int i = 0; i < DefaultBufferCount; i++)
            {
                memBuffers[i] = new byte[DefaultBufferSize];
                audioBuffers[i] = new AudioBuffer(memBuffers[i]);
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~GameAudioFile()
        {
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
                audioDecoder?.Dispose();
            }
        }

        /// <summary>
        /// Set sound position
        /// </summary>
        /// <param name="start">Start time</param>
        public void SetPosition(TimeSpan start)
        {
            audioDecoder.CurrentTime = start;
            currentSample = 0;
        }

        /// <summary>
        /// Reads the buffer data from the decoder, and writes into the next audio buffer to submit to the Source Voice
        /// </summary>
        /// <param name="buffer">Returns the audio buffer prepared to submit</param>
        /// <returns>Returns true if there are more buffers to play</returns>
        public bool GetNextAudioBuffer(out AudioBuffer buffer)
        {
            buffer = null;

            // Move to the next index in the circular buffer
            nextBuffer = (nextBuffer + 1) % BufferCount;

            // Read the bytes directly from NAudio using the specified size
            int bytesToRead = DefaultBufferSize;
            byte[] activeStorage = memBuffers[nextBuffer];

            uint bytesRead = (uint)audioDecoder.Read(activeStorage, 0, bytesToRead);

            // If no bytes were read, it means we have reached the end of the file.
            if (bytesRead <= 0)
            {
                return false;
            }

            Game.Logger.WriteTrace(this, $"Sample: {currentSample++}");

            // Adjust the native Vortice AudioBuffer to point to the segment of read bytes
            var targetBuffer = new AudioBuffer(activeStorage, BufferFlags.None)
            {
                AudioBytes = bytesRead
            };

            audioBuffers[nextBuffer] = targetBuffer;
            buffer = audioBuffers[nextBuffer];

            return true;
        }

        /// <summary>
        /// Gets the complete audio buffer
        /// </summary>
        /// <param name="isLooped">Looped buffer</param>
        /// <returns>Returns the audio buffer prepared to submit</returns>
        public AudioBuffer GetCompleteAudioBuffer(bool isLooped)
        {
            // Reset the file position to the beginning to perform a full read
            audioDecoder.Position = 0;

            var bufferBytes = new List<byte>();
            byte[] tempBuffer = new byte[DefaultBufferSize];
            int bytesRead;

            // Read the entire multimedia file sequentially and accumulate it
            while ((bytesRead = audioDecoder.Read(tempBuffer, 0, tempBuffer.Length)) > 0)
            {
                if (bytesRead == tempBuffer.Length)
                {
                    bufferBytes.AddRange(tempBuffer);
                }
                else
                {
                    byte[] exactBuffer = new byte[bytesRead];
                    Buffer.BlockCopy(tempBuffer, 0, exactBuffer, 0, bytesRead);
                    bufferBytes.AddRange(exactBuffer);
                }
            }

            var finalArray = bufferBytes.ToArray();

            // Instantiate the Vortice buffer by passing it the total number of bytes.
            var result = new AudioBuffer(finalArray)
            {
                AudioBytes = (uint)finalArray.Length,
                Flags = BufferFlags.EndOfStream,
                LoopCount = isLooped ? (uint)XAudio2.LoopInfinite : 0,
            };

            return result;
        }
    }
}
