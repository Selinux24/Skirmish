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
        /// Game audios dictionary
        /// </summary>
        private readonly Dictionary<string, GameAudio> gameAudioList = new Dictionary<string, GameAudio>();

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
                gameAudioList.Values.ToList().ForEach(a => a.Dispose());
                gameAudioList.Clear();
            }
        }

        /// <summary>
        /// Updates the internal state
        /// </summary>
        public void Update()
        {
            gameAudioList?
                .ToList()
                .ForEach(a => a.Value?.Update());
        }

        /// <summary>
        /// Creates a list of game audios
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="fileName">Resource file name</param>
        /// <returns>Returns a list of game audios</returns>
        public GameAudio CreateAudio(string name)
        {
            if (gameAudioList.ContainsKey(name))
            {
                return gameAudioList[name];
            }

            var audio = new GameAudio();

            gameAudioList.Add(name, audio);

            return audio;
        }
        /// <summary>
        /// Creates a new effect
        /// </summary>
        /// <param name="audioName">Audio name</param>
        /// <param name="effectName">Effect name</param>
        /// <param name="contentFolder">Conten folder</param>
        /// <param name="fileName">File name</param>
        /// <returns></returns>
        public GameAudioEffect CreateEffect(string audioName, string effectName, string contentFolder, string fileName)
        {
            var audio = gameAudioList[audioName];

            var paths = ContentManager.FindPaths(contentFolder, fileName);

            return audio.GetEffect(effectName, paths.FirstOrDefault());
        }

        /// <summary>
        /// Removes a single audio
        /// </summary>
        /// <param name="name">Audio to remove</param>
        public void RemoveAudio(string name)
        {
            if (gameAudioList.ContainsKey(name))
            {
                gameAudioList[name].Dispose();
                gameAudioList.Remove(name);
            }
        }
    }
}
