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
        private readonly Dictionary<string, GameAudio> gameAudios = new Dictionary<string, GameAudio>();

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
                gameAudios.Values.ToList().ForEach(a => a.Dispose());
                gameAudios.Clear();
            }
        }

        /// <summary>
        /// Creates a list of game audios
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="fileName">Resource file name</param>
        /// <returns>Returns a list of game audios</returns>
        public GameAudio CreateAudio(string name)
        {
            if (gameAudios.ContainsKey(name))
            {
                return gameAudios[name];
            }

            var audio = new GameAudio();

            gameAudios.Add(name, audio);

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
            var audio = gameAudios[audioName];

            var paths = ContentManager.FindPaths(contentFolder, fileName);

            return audio.GetEffect(effectName, paths.FirstOrDefault());
        }

        /// <summary>
        /// Removes a single audio
        /// </summary>
        /// <param name="name">Audio to remove</param>
        public void RemoveAudio(string name)
        {
            if (gameAudios.ContainsKey(name))
            {
                gameAudios[name].Dispose();
                gameAudios.Remove(name);
            }
        }
    }
}
