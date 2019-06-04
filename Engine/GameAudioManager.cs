using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.Audio;
    using Engine.Content;

    /// <summary>
    /// Audio manager
    /// </summary>
    public class GameAudioManager : IDisposable
    {
        /// <summary>
        /// Audio list
        /// </summary>
        private readonly List<GameAudio> audioList = new List<GameAudio>();

        /// <summary>
        /// Constructor
        /// </summary>
        public GameAudioManager()
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~GameAudioManager()
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
                audioList.ForEach(a => a.Dispose());
                audioList.Clear();

            }
        }

        /// <summary>
        /// Creates a list of game audios
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="fileName">Resource file name</param>
        /// <returns>Returns a list of game audios</returns>
        public IEnumerable<GameAudio> CreateAudio(string contentFolder, string fileName)
        {
            List<GameAudio> res = new List<GameAudio>();

            var paths = ContentManager.FindPaths(contentFolder, fileName);
            if (paths.Any())
            {
                foreach (var filename in paths)
                {
                    res.Add(new GameAudio(filename));
                }

                //Add audios to internal "to dispose" list
                audioList.AddRange(res);
            }

            return res;
        }
        /// <summary>
        /// Removes a single audio
        /// </summary>
        /// <param name="audio">Audio to remove</param>
        public void RemoveAudio(GameAudio audio)
        {
            if (audioList.Contains(audio))
            {
                audioList.Remove(audio);
            }

            audio.Dispose();
        }
        /// <summary>
        /// Removes an audio list
        /// </summary>
        /// <param name="audios">Audio list to remove</param>
        public void RemoveAudio(IEnumerable<GameAudio> audios)
        {
            if (audios?.Any() == true)
            {
                foreach (var audio in audios)
                {
                    RemoveAudio(audio);
                }
            }
        }
    }
}
