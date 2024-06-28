using Engine;
using Engine.Audio;
using Engine.Common;
using System;

namespace TerrainSamples.SceneSkybox
{
    /// <summary>
    /// Sound effects manager
    /// </summary>
    /// <param name="scene">Scene</param>
    /// <param name="id">Component id</param>
    /// <param name="name">Component name</param>
    class SoundEffectsManager(Scene scene, string id, string name) : Updatable<SceneObjectDescription>(scene, id, name), IDisposable
    {
        private readonly GameAudioManager audioManager = new();

        private const string sphereEffect = "Sphere";
        private IGameAudioEffect fireAudioEffect;

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
        /// <param name="resourceFolder">Base resource folder</param>
        public void InitializeAudio(string resourceFolder)
        {
            const string sphereSound = "target_balls_single_loop";

            audioManager.LoadSound(sphereSound, resourceFolder, "target_balls_single_loop.wav");

            audioManager.AddEffectParams(
                sphereEffect,
                new GameAudioEffectParameters
                {
                    SoundName = sphereSound,
                    IsLooped = true,
                    UseAudio3D = true,
                    ReverbPreset = GameAudioReverbPresets.StoneRoom,
                    Volume = 0.25f,
                    EmitterRadius = 6,
                    ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                });
        }

        public void Start(float masterVolume)
        {
            audioManager.MasterVolume = masterVolume;
            audioManager.Start();
        }

        public void PlaySphereEffect(ITransformable3D emitter)
        {
            if (fireAudioEffect == null)
            {
                fireAudioEffect = audioManager.CreateEffectInstance(sphereEffect, emitter, Scene.Camera);
                fireAudioEffect.Play();
            }
        }

        public float[] GetOutputMatrix()
        {
            return fireAudioEffect.GetOutputMatrix();
        }
    }
}
