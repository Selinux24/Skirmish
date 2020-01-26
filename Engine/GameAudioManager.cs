using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
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
        /// Effect instance helper class
        /// </summary>
        class EffectInstance
        {
            /// <summary>
            /// Effect name
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Effect instance
            /// </summary>
            public GameAudioEffect Effect { get; set; }
        }

        /// <summary>
        /// Game audio
        /// </summary>
        private readonly GameAudio gameAudio;
        /// <summary>
        /// Sound dictionary
        /// </summary>
        private readonly Dictionary<string, GameAudioSound> soundList = new Dictionary<string, GameAudioSound>();
        /// <summary>
        /// Effect parameters library
        /// </summary>
        private readonly Dictionary<string, GameAudioEffectParameters> effectParamsLib = new Dictionary<string, GameAudioEffectParameters>();
        /// <summary>
        /// Effect instances list
        /// </summary>
        private readonly List<EffectInstance> effectInstances = new List<EffectInstance>();
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
                return gameAudio.MasterVolume;
            }
            set
            {
                gameAudio.MasterVolume = value;
            }
        }
        /// <summary>
        /// Gets or sets whether the master voice uses a limiter or not
        /// </summary>
        public bool UseMasteringLimiter
        {
            get
            {
                return gameAudio.UseMasteringLimiter;
            }
            set
            {
                gameAudio.UseMasteringLimiter = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="audio">Game audio instance</param>
        public GameAudioManager()
        {
            this.gameAudio = new GameAudio();
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
                effectInstances.ForEach(i => i.Effect.Dispose());
                effectInstances.Clear();

                soundList.Values.ToList().ForEach(a => a.Dispose());
                soundList.Clear();

                gameAudio.Dispose();
            }
        }

        /// <summary>
        /// Updates the internal state
        /// </summary>
        public void Update()
        {
            //Extract a remove the effects due to dispose
            var toDispose = effectInstances
                .FindAll(i => i.Effect.DueToDispose)
                .ToArray();

            if (toDispose.Any())
            {
                effectInstances.RemoveAll(i => i.Effect.DueToDispose);
                Task.Run(() =>
                {
                    toDispose.ToList().ForEach(i => i.Effect.Dispose());
                }).ConfigureAwait(false);
            }

            var toUpdate = effectInstances
                .FindAll(i => !i.Effect.DueToDispose && i.Effect.State == AudioState.Playing && i.Effect.UseAudio3D)
                .ToArray();

            int effectCount = toUpdate.Length;
            if (effectCount == 0)
            {
                return;
            }

            for (int i = frameToUpdateAudio; i < effectCount; i += 2)
            {
                toUpdate[i].Effect.Apply3D();
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
        public void LoadSound(string soundName, string contentFolder, string fileName, bool replaceIfExists = true)
        {
            string path = ContentManager
                .FindPaths(contentFolder, fileName)
                .FirstOrDefault(p => File.Exists(p));

            if (string.IsNullOrEmpty(path))
            {
                throw new EngineException($"The specified file not exists: [{contentFolder}][{fileName}]");
            }

            var sound = GameAudioSound.LoadFromFile(path);

            if (soundList.ContainsKey(soundName))
            {
                if (replaceIfExists)
                {
                    //Replaces the sound
                    soundList[soundName].Dispose();
                    soundList[soundName] = sound;
                }
                else
                {
                    throw new EngineException($"{soundName} already exists in the sound dictionary.");
                }
            }
            else
            {
                //Adds the sound to the collection
                soundList.Add(soundName, sound);
            }
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

            if (effectInstances.Exists(i => i.Name == effectName && i.Effect.State != AudioState.Stopped))
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
            var instance = new GameAudioEffect(gameAudio, sound, effectParams);

            //Adds effect to "to delete" effect list
            effectInstances.Add(new EffectInstance { Name = effectName, Effect = instance });

            return instance;
        }
        /// <summary>
        /// Creates an effect instance
        /// </summary>
        /// <param name="effectName">Effect name</param>
        /// <param name="emitter">Emitter fixed position</param>
        /// <param name="listener">Listener manipulator object</param>
        /// <returns>Returns the new created instance. Returns null if the effect name not exists, o if the effect instance is currently playing</returns>
        public GameAudioEffect CreateEffectInstance(string effectName, Vector3 emitter, IManipulator listener)
        {
            Manipulator3D emitterManipulator = new Manipulator3D();
            emitterManipulator.SetPosition(emitter);

            return CreateEffectInstance(effectName, emitterManipulator, listener);
        }
        /// <summary>
        /// Creates an effect instance
        /// </summary>
        /// <param name="effectName">Effect name</param>
        /// <param name="emitter">Emitter manipulator object</param>
        /// <param name="listener">Listener manipulator object</param>
        /// <returns>Returns the new created instance. Returns null if the effect name not exists, o if the effect instance is currently playing</returns>
        public GameAudioEffect CreateEffectInstance(string effectName, IManipulator emitter, IManipulator listener)
        {
            var instance = CreateEffectInstance(effectName);

            instance?.Emitter.SetSource(emitter);
            instance?.Listener.SetSource(listener);

            return instance;
        }
        /// <summary>
        /// Creates an effect instance
        /// </summary>
        /// <param name="effectName">Effect name</param>
        /// <param name="emitter">Emitter 3D transformable object</param>
        /// <param name="listener">Listener manipulator object</param>
        /// <returns>Returns the new created instance. Returns null if the effect name not exists, o if the effect instance is currently playing</returns>
        public GameAudioEffect CreateEffectInstance(string effectName, ITransformable3D emitter, IManipulator listener)
        {
            var instance = CreateEffectInstance(effectName);

            instance?.Emitter.SetSource(emitter);
            instance?.Listener.SetSource(listener);

            return instance;
        }

        /// <summary>
        /// Starts the audio device
        /// </summary>
        public void Start()
        {
            gameAudio.Start();
        }
        /// <summary>
        /// Stops the audio device
        /// </summary>
        public void Stop()
        {
            gameAudio.Stop();
        }

        /// <summary>
        /// Clear current effects
        /// </summary>
        public void ClearEffects()
        {
            var toDispose = effectInstances.ToList();
            if (toDispose.Any())
            {
                effectInstances.Clear();
                Task.Run(() =>
                {
                    toDispose.ForEach(i => i.Effect.Dispose());
                }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sets the mastering limiter parameters
        /// </summary>
        /// <param name="release">Speed at which the limiter stops affecting audio once it drops below the limiter's threshold</param>
        /// <param name="loudness">Threshold of the limiter</param>
        public void SetMasteringLimit(int release, int loudness)
        {
            gameAudio.SetMasteringLimit(release, loudness);
        }
    }
}
