using Engine;
using Engine.Audio;
using Engine.Audio.Tween;
using Engine.Common;
using Engine.Tween;
using System;

namespace IntermediateSamples.SceneStart
{
    /// <summary>
    /// Sound effects manager
    /// </summary>
    /// <param name="scene">Scene</param>
    /// <param name="id">Component id</param>
    /// <param name="name">Component name</param>
    class SoundEffectsManager(Scene scene, string id, string name) : Updatable<SceneObjectDescription>(scene, id, name), IDisposable
    {
        private const string MusicString = "Music";

        private readonly GameAudioManager audioManager = new();
        private AudioEffectTweener audioTweener;
        private IGameAudioEffect currentMusic = null;

        /// <summary>
        /// Destructor
        /// </summary>
        ~SoundEffectsManager()
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
            if (!disposing)
            {
                return;
            }

            audioManager.Dispose();
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            audioManager.Update(context.GameTime);
        }

        /// <summary>
        /// Initializes the sounf effects
        /// </summary>
        public void InitializeAudio(string resourceFolder)
        {
            audioManager.LoadSound(MusicString, resourceFolder, "anttisinstrumentals+keepshiningoninstrumental.mp3");
            audioManager.AddEffectParams(
                MusicString,
                new GameAudioEffectParameters
                {
                    DestroyWhenFinished = false,
                    SoundName = MusicString,
                    IsLooped = true,
                    UseAudio3D = true,
                });

            currentMusic = audioManager.CreateEffectInstance(MusicString);
        }

        public void Start(float masterVolume)
        {
            audioManager.MasterVolume = masterVolume;
            audioManager.Start();
        }

        public void Play()
        {
            currentMusic?.Play();

            audioTweener ??= Scene.AddAudioEffectTweener();
            audioTweener.TweenVolumeUp(currentMusic, (long)(currentMusic?.Duration.TotalMilliseconds * 0.2f), ScaleFuncs.Linear);
        }
    }
}
