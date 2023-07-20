using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.IO;

namespace Engine.Audio
{
    class GameAudioFile : IAudioFile
    {
        private const int DefaultBufferCount = 3;
        private const int DefaulBuffertSize = 32 * 1024; // default size 32Kb

        /// <summary>
        /// Loads a file
        /// </summary>
        /// <param name="fileName">File name</param>
        public static GameAudioFile LoadFromFile(string fileName)
        {
            return new GameAudioFile(fileName);
        }

        private IEnumerator<DataPointer> sampleIterator = null;
        private int currentSample = 0;

        private readonly AudioDecoder audioDecoder;
        private readonly AudioBuffer[] audioBuffers;
        private readonly DataBuffer[] memBuffers;
        private int nextBuffer = 0;

        /// <summary>
        /// File name
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// Wave format
        /// </summary>
        public WaveFormat WaveFormat
        {
            get
            {
                return audioDecoder.WaveFormat;
            }
        }
        /// <summary>
        /// Sound duration
        /// </summary>
        public TimeSpan Duration
        {
            get { return audioDecoder.Duration; }
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

            // Initialize the audio decoder
            audioDecoder = new AudioDecoder(File.OpenRead(fileName));

            // Pre-allocate buffers
            audioBuffers = new AudioBuffer[DefaultBufferCount];
            memBuffers = new DataBuffer[DefaultBufferCount];

            for (int i = 0; i < DefaultBufferCount; i++)
            {
                audioBuffers[i] = new AudioBuffer();
                memBuffers[i] = new DataBuffer(DefaulBuffertSize);
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~GameAudioFile()
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
                audioDecoder?.Dispose();

                for (int i = 0; i < BufferCount; i++)
                {
                    memBuffers[i]?.Dispose();
                }
            }
        }

        /// <summary>
        /// Set sound position
        /// </summary>
        /// <param name="start">Start time</param>
        public void SetPosition(TimeSpan start)
        {
            sampleIterator = audioDecoder.GetSamples(start).GetEnumerator();
            currentSample = 0;
        }

        /// <summary>
        /// Reads the buffer data from the decoder sample pointer, and writes into the next audio buffer to submit to the Source Voice
        /// </summary>
        /// <param name="buffer">Returns the audio buffer prepared to submit</param>
        /// <returns>Returns true if there are more buffers to play</returns>
        public bool GetNextAudioBuffer(out AudioBuffer buffer)
        {
            buffer = null;

            if (!sampleIterator.MoveNext())
            {
                //End of audio
                return false;
            }

            Logger.WriteTrace(this, $"Sample: {currentSample++}");

            var bufferPointer = sampleIterator.Current;

            // Go to next entry in the ringg audio buffer
            nextBuffer = ++nextBuffer % BufferCount;

            // Check that our ring buffer has enough space to store the audio buffer.
            if (bufferPointer.Size > memBuffers[nextBuffer].Size)
            {
                memBuffers[nextBuffer].Dispose();
                memBuffers[nextBuffer] = new DataBuffer(bufferPointer.Size);
            }

            // Copy to data fuffer
            memBuffers[nextBuffer].Set(0, bufferPointer.ToArray());

            // Set the pointer to the data.
            audioBuffers[nextBuffer].AudioDataPointer = memBuffers[nextBuffer].DataPointer;
            audioBuffers[nextBuffer].AudioBytes = bufferPointer.Size;

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
            var bufferBytes = new List<byte>();

            var iterator = audioDecoder.GetSamples().GetEnumerator();
            while (iterator.MoveNext())
            {
                var pointer = iterator.Current;

                bufferBytes.AddRange(pointer.ToArray());
            }

            var dataBuffer = new DataBuffer(bufferBytes.Count);
            dataBuffer.Set(0, bufferBytes.ToArray());

            var result = new AudioBuffer
            {
                AudioDataPointer = dataBuffer.DataPointer,
                AudioBytes = bufferBytes.Count,
                Flags = BufferFlags.EndOfStream,
                LoopCount = isLooped ? AudioBuffer.LoopInfinite : 0,
            };

            return result;
        }
    }
}
