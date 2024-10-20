﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Audio
{
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
            public IGameAudioEffect Effect { get; set; }
        }

        /// <summary>
        /// Game audio delegate
        /// </summary>
        private static Func<IGameAudio> audioDelegate;

        /// <summary>
        /// Game audio
        /// </summary>
        private readonly IGameAudio gameAudio;
        /// <summary>
        /// Sound dictionary
        /// </summary>
        private readonly Dictionary<string, string> soundList = [];
        /// <summary>
        /// Effect parameters library
        /// </summary>
        private readonly Dictionary<string, GameAudioEffectParameters> effectParamsLib = [];
        /// <summary>
        /// Effect instances list
        /// </summary>
        private readonly List<EffectInstance> effectInstances = [];

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
        /// Register audio
        /// </summary>
        /// <typeparam name="T">Type of audio loader</typeparam>
        /// <param name="sampleRate">Sample rate</param>
        public static void RegisterAudio<T>(int sampleRate) where T : class, IGameAudioLoader
        {
            T audio = Activator.CreateInstance<T>();

            audioDelegate = audio.GetDelegate(sampleRate);
        }
        /// <summary>
        /// Gets the audio instance
        /// </summary>
        public static IGameAudio GetAudio()
        {
            return audioDelegate?.Invoke();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public GameAudioManager()
        {
            gameAudio = GetAudio();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~GameAudioManager()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
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
                effectInstances.ForEach(i =>
                {
                    i.Effect.Stop(true);
                    i.Effect.Dispose();
                });
                effectInstances.Clear();

                soundList.Clear();

                gameAudio?.Dispose();
            }
        }

        /// <summary>
        /// Updates the internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(IGameTime gameTime)
        {
            //Extract a remove the effects due to dispose
            var toDispose = effectInstances
                .FindAll(i => i.Effect.DueToDispose)
                .ToArray();

            if (toDispose.Length != 0)
            {
                effectInstances.RemoveAll(i => i.Effect.DueToDispose);
                Task.Run(() =>
                {
                    toDispose.ToList().ForEach(i =>
                    {
                        i.Effect.Stop(true);
                        i.Effect.Dispose();
                    });
                });
            }

            var toUpdate = effectInstances
                .FindAll(i => !i.Effect.DueToDispose && i.Effect.State == GameAudioState.Playing && i.Effect.UseAudio3D)
                .ToArray();

            int effectCount = toUpdate.Length;
            if (effectCount == 0)
            {
                return;
            }

            Parallel.ForEach(toUpdate, e => e.Effect.Update(gameTime));
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

            if (!soundList.TryAdd(soundName, path))
            {
                if (replaceIfExists)
                {
                    //Replaces the sound
                    soundList[soundName] = path;
                }
                else
                {
                    throw new EngineException($"{soundName} already exists in the sound dictionary.");
                }
            }
        }
        /// <summary>
        /// Adds an effect parametrization to the library
        /// </summary>
        /// <param name="effectName">Effect name</param>
        /// <param name="effectParams">Effect parameters</param>
        public void AddEffectParams(string effectName, GameAudioEffectParameters effectParams)
        {
            effectParamsLib.TryAdd(effectName, effectParams);
        }
        /// <summary>
        /// Creates an effect instance
        /// </summary>
        /// <param name="effectName">Effect name</param>
        /// <returns>Returns the new created instance. Returns null if the effect name not exists, o if the effect instance is currently playing</returns>
        public IGameAudioEffect CreateEffectInstance(string effectName)
        {
            if (!effectParamsLib.TryGetValue(effectName, out var effectParams))
            {
                //Not exists
                return null;
            }

            if (effectInstances.Exists(i => i.Name == effectName && i.Effect.State != GameAudioState.Stopped))
            {
                //Effect currently playing
                return null;
            }

            if (!soundList.TryGetValue(effectParams.SoundName, out var soundFile))
            {
                //Sound not exists
                return null;
            }

            //Creates the effect
            var instance = gameAudio.CreateEffect(soundFile, effectParams);

            //Adds effect to "to delete" effect list
            effectInstances.Add(new() { Name = effectName, Effect = instance });

            return instance;
        }
        /// <summary>
        /// Creates an effect instance
        /// </summary>
        /// <param name="effectName">Effect name</param>
        /// <param name="emitter">Emitter fixed position</param>
        /// <param name="listener">Listener manipulator object</param>
        /// <returns>Returns the new created instance. Returns null if the effect name not exists, o if the effect instance is currently playing</returns>
        public IGameAudioEffect CreateEffectInstance(string effectName, Vector3 emitter, ITransform listener)
        {
            var emitterManipulator = new Manipulator3D();
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
        public IGameAudioEffect CreateEffectInstance(string effectName, ITransform emitter, ITransform listener)
        {
            var instance = CreateEffectInstance(effectName);

            instance?.Emitter.SetSource(emitter);
            instance?.Listener.SetSource(listener);
            instance?.Update(new GameTime());

            return instance;
        }
        /// <summary>
        /// Creates an effect instance
        /// </summary>
        /// <param name="effectName">Effect name</param>
        /// <param name="emitter">Emitter 3D transformable object</param>
        /// <param name="listener">Listener manipulator object</param>
        /// <returns>Returns the new created instance. Returns null if the effect name not exists, o if the effect instance is currently playing</returns>
        public IGameAudioEffect CreateEffectInstance(string effectName, ITransformable3D emitter, ITransform listener)
        {
            var instance = CreateEffectInstance(effectName);

            instance?.Emitter.SetSource(emitter);
            instance?.Listener.SetSource(listener);
            instance?.Update(new GameTime());

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
            if (toDispose.Count != 0)
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
