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
        /// Game audio dictionary
        /// </summary>
        private readonly Dictionary<string, GameAudio> gameAudioList = new Dictionary<string, GameAudio>();

        /// <summary>
        /// Gets a game audio instance by name
        /// </summary>
        /// <param name="name">Game audio instance name</param>
        /// <returns>Returns the named game audio, or null if not exists</returns>
        public GameAudio this[string name]
        {
            get
            {
                if (gameAudioList.ContainsKey(name))
                {
                    return gameAudioList[name];
                }

                return null;
            }
        }

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

        /// <summary>
        /// Creates a new sound
        /// </summary>
        /// <param name="audioName">Audio name</param>
        /// <param name="soundName">Sound name</param>
        /// <param name="contentFolder">Conten folder</param>
        /// <param name="fileName">File name</param>
        /// <returns>Returns the new created game sound</returns>
        public GameAudioSound CreateSound(string audioName, string soundName, string contentFolder, string fileName)
        {
            if (!gameAudioList.ContainsKey(audioName))
            {
                CreateAudio(audioName);
            }

            var audio = gameAudioList[audioName];

            var paths = ContentManager.FindPaths(contentFolder, fileName);

            return audio.GetSound(soundName, paths.FirstOrDefault());
        }
    }
}
