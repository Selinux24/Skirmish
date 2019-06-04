using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.IO;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio
    /// </summary>
    public class GameAudio : IDisposable
    {
        /// <summary>
        /// Audio device
        /// </summary>
        private readonly XAudio2 device;
        /// <summary>
        /// Mastering voice
        /// </summary>
        private readonly MasteringVoice masteringVoice;
        /// <summary>
        /// Source voice
        /// </summary>
        private readonly SourceVoice sourceVoice;
        /// <summary>
        /// Audio buffer
        /// </summary>
        private readonly AudioBuffer buffer;
        /// <summary>
        /// Decoded info
        /// </summary>
        private readonly uint[] decodedPacketsInfo;
        /// <summary>
        /// Source vouice events attached flag
        /// </summary>
        private bool eventsAttached = false;

        /// <summary>
        /// Gets whether the audio is playing or not
        /// </summary>
        public bool Playing { get; private set; }

        /// <summary>
        /// Event fired when the audio starts
        /// </summary>
        public event GameAudioHandler AudioStart;
        /// <summary>
        /// Event fired when the audio ends
        /// </summary>
        public event GameAudioHandler AudioEnd;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">File name</param>
        internal GameAudio(string fileName)
        {
            this.device = new XAudio2();
            this.masteringVoice = new MasteringVoice(device);

            using (var stream = new SoundStream(File.OpenRead(fileName)))
            {
                var waveFormat = stream.Format;
                this.decodedPacketsInfo = stream.DecodedPacketsInfo;
                this.buffer = new AudioBuffer
                {
                    Stream = stream.ToDataStream(),
                    AudioBytes = (int)stream.Length,
                    Flags = BufferFlags.EndOfStream
                };

                this.sourceVoice = new SourceVoice(device, waveFormat, true);
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~GameAudio()
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
                buffer.Stream.Dispose();

                sourceVoice.DestroyVoice();
                sourceVoice.Dispose();

                masteringVoice.DestroyVoice();
                masteringVoice.Dispose();

                device.StopEngine();
                device.Dispose();
            }
        }

        /// <summary>
        /// Play audio
        /// </summary>
        public void Play()
        {
            if (!eventsAttached)
            {
                sourceVoice.BufferStart += (context) => FireAudioStart();
                sourceVoice.BufferEnd += (context) => FireAudioEnd();

                eventsAttached = true;
            }

            sourceVoice.SubmitSourceBuffer(buffer, decodedPacketsInfo);

            Playing = true;
            sourceVoice.Start();
        }
        /// <summary>
        /// Stop audio
        /// </summary>
        public void Stop()
        {
            Playing = false;
            sourceVoice.Stop();
        }
        /// <summary>
        /// Resume audio
        /// </summary>
        public void Resume()
        {
            Playing = true;
            sourceVoice.Start();
        }

        /// <summary>
        /// Fires the audio start event
        /// </summary>
        private void FireAudioStart()
        {
            Playing = true;
            AudioStart?.Invoke(this, new GameAudioEventArgs());
        }
        /// <summary>
        /// Fires the audio end event
        /// </summary>
        private void FireAudioEnd()
        {
            Playing = false;
            AudioEnd?.Invoke(this, new GameAudioEventArgs());
        }
    }
}
