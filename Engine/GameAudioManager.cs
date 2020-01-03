using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// Audio
        /// </summary>
        private readonly GameAudio audio;
        /// <summary>
        /// Sound dictionary
        /// </summary>
        private readonly Dictionary<string, GameAudioSound> soundList = new Dictionary<string, GameAudioSound>();
        /// <summary>
        /// Effect parameters library
        /// </summary>
        private readonly Dictionary<string, GameAudioEffectParameters> effectParamsLib = new Dictionary<string, GameAudioEffectParameters>();
        /// <summary>
        /// Effect duration library
        /// </summary>
        private readonly Dictionary<string, float> effectDurationsLib = new Dictionary<string, float>();
        /// <summary>
        /// Effect instances list
        /// </summary>
        private readonly List<GameAudioEffect> effectInstances = new List<GameAudioEffect>();
        /// <summary>
        /// Frame to update audio
        /// </summary>
        private int frameToUpdateAudio = 0;

        /// <summary>
        /// Gets or sets the master volume
        /// </summary>
        /// <remarks>From 0 to 1</remarks>
        public float MasterVolume
        {
            get
            {
                return audio.MasterVolume;
            }
            set
            {
                audio.MasterVolume = value;
            }
        }
        /// <summary>
        /// Gets or sets whether the master voice uses a limiter or not
        /// </summary>
        public bool UseMasteringLimiter
        {
            get
            {
                return audio.UseMasteringLimiter;
            }
            set
            {
                audio.UseMasteringLimiter = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="audio">Game audio instance</param>
        public GameAudioManager()
        {
            this.audio = new GameAudio();
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
                effectInstances.ForEach(i => i.Dispose());
                effectInstances.Clear();

                soundList.Values.ToList().ForEach(a => a.Dispose());
                soundList.Clear();

                audio.Dispose();
            }
        }

        /// <summary>
        /// Updates the internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            //Update effects duration
            var keys = effectDurationsLib.Keys.ToArray();
            foreach (var effectName in keys)
            {
                effectDurationsLib[effectName] -= gameTime.ElapsedSeconds;
            }

            //Extract a remove the effects due to dispose
            var toDispose = effectInstances.FindAll(i => i.DueToDispose);
            if (toDispose.Any())
            {
                effectInstances.RemoveAll(i => i.DueToDispose);
                Task.Run(() =>
                {
                    toDispose.ForEach(i => i.Dispose());
                }).ConfigureAwait(false);
            }

            var toUpdate = effectInstances.FindAll(i => !i.DueToDispose && i.State == AudioState.Playing && i.UseAudio3D);
            int effectCount = toUpdate.Count;
            if (effectCount == 0)
            {
                return;
            }

            for (int i = frameToUpdateAudio; i < effectCount; i += 2)
            {
                var effect = toUpdate[i];
                effect.Apply3D();
            }

            frameToUpdateAudio++;
            frameToUpdateAudio %= 2;
        }

        /// <summary>
        /// Loads a sound file into the library
        /// </summary>
        /// <param name="soundName">Sound name</param>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="fileName">File name</param>
        public void LoadSound(string soundName, string contentFolder, string fileName)
        {
            if (soundList.ContainsKey(soundName))
            {
                return;
            }

            var paths = ContentManager.FindPaths(contentFolder, fileName);

            var sound = audio.GetSound(soundName, paths.FirstOrDefault());

            soundList.Add(soundName, sound);
        }
        /// <summary>
        /// Adds an effect parametrization to the library
        /// </summary>
        /// <param name="effectName">Effect name</param>
        /// <param name="effectParams">Effect parameters</param>
        public void AddEffectParams(string effectName, GameAudioEffectParameters effectParams)
        {
            if (!effectParamsLib.ContainsKey(effectName))
            {
                effectParamsLib.Add(effectName, effectParams);
            }
        }
        /// <summary>
        /// Creates an effect instance
        /// </summary>
        /// <param name="effectName">Effect name</param>
        /// <returns>Returns the new created instance. Returns null if the effect name not exists, o if the effect instance is currently playing</returns>
        public GameAudioEffect CreateEffectInstance(string effectName)
        {
            if (!effectParamsLib.ContainsKey(effectName))
            {
                //Not exists
                return null;
            }

            if (effectDurationsLib.ContainsKey(effectName) && effectDurationsLib[effectName] > 0)
            {
                //Effect currently playing
                return null;
            }

            var effectParams = effectParamsLib[effectName];

            if (!soundList.ContainsKey(effectParams.SoundName))
            {
                //Sound not exists
                return null;
            }

            //Gets the sound
            var sound = soundList[effectParams.SoundName];

            //Creates the effect
            var instance = new GameAudioEffect(sound, effectParams);

            if (!instance.IsLooped)
            {
                //Adds effect to playing list
                if (!effectDurationsLib.ContainsKey(effectName))
                {
                    effectDurationsLib.Add(effectName, 0);
                }

                effectDurationsLib[effectName] = (float)instance.Duration.TotalSeconds;
            }

            //Adds effect to "to delete" effect list
            effectInstances.Add(instance);

            return instance;
        }

        /// <summary>
        /// Starts the audio device
        /// </summary>
        public void Start()
        {
            audio.Start();
        }
        /// <summary>
        /// Stops the audio device
        /// </summary>
        public void Stop()
        {
            audio.Stop();
        }

        /// <summary>
        /// Sets the mastering limiter parameters
        /// </summary>
        /// <param name="release">Speed at which the limiter stops affecting audio once it drops below the limiter's threshold</param>
        /// <param name="loudness">Threshold of the limiter</param>
        public void SetMasteringLimit(int release, int loudness)
        {
            audio.SetMasteringLimit(release, loudness);
        }
    }
}
