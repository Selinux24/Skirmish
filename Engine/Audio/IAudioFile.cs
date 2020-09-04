using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;

namespace Engine.Audio
{
    /// <summary>
    /// Audio file interface
    /// </summary>
    public interface IAudioFile : IDisposable
    {
        /// <summary>
        /// File name
        /// </summary>
        string FileName { get; }
        /// <summary>
        /// Wave format
        /// </summary>
        WaveFormat WaveFormat { get; }
        /// <summary>
        /// Sound duration
        /// </summary>
        TimeSpan Duration { get; }
        /// <summary>
        /// Buffer count
        /// </summary>
        int BufferCount { get; }

        /// <summary>
        /// Set sound position
        /// </summary>
        /// <param name="start">Start time</param>
        void SetPosition(TimeSpan start);
        /// <summary>
        /// Reads the buffer data from the decoder sample pointer, and writes into the next audio buffer to submit to the Source Voice
        /// </summary>
        /// <param name="buffer">Returns the audio buffer prepared to submit</param>
        /// <returns>Returns true if there are more buffers to play</returns>
        bool GetNextAudioBuffer(out AudioBuffer buffer);
        /// <summary>
        /// Gets the complete audio buffer
        /// </summary>
        /// <param name="isLooped">Looped buffer</param>
        /// <returns>Returns the audio buffer prepared to submit</returns>
        AudioBuffer GetCompleteAudioBuffer(bool isLooped);
    }
}
