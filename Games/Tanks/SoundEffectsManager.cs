using Engine;
using Engine.Audio;
using Engine.Common;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace Tanks
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

        private string music;
        private string tankMoveEffect;
        private IGameAudioEffect musicEffectInstance;
        private IGameAudioEffect tankMoveEffectInstance;
        private string tankDestroyedEffect;
        private string tankShootingEffect;
        private string[] impactEffects;
        private string[] damageEffects;

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
        public void InitializeAudio(string resourceFolder, float nearRadius)
        {
            GameAudioReverbPresets preset = GameAudioReverbPresets.Default;

            music = "Music";
            tankMoveEffect = "TankMove";
            tankDestroyedEffect = "TankDestroyed";
            tankShootingEffect = "TankShooting";
            impactEffects = ["Impact1", "Impact2", "Impact3", "Impact4"];
            damageEffects = ["Damage1", "Damage2", "Damage3", "Damage4"];

            audioManager.LoadSound(music, resourceFolder, "elsasong.wav");
            audioManager.LoadSound("Tank", resourceFolder, "tank_engine.wav");
            audioManager.LoadSound("TankDestroyed", resourceFolder, "explosion_vehicle_small_close_01.wav");
            audioManager.LoadSound("TankShooting", resourceFolder, "cannon-shooting.wav");
            audioManager.LoadSound(impactEffects[0], resourceFolder, "metal_grate_large_01.wav");
            audioManager.LoadSound(impactEffects[1], resourceFolder, "metal_grate_large_02.wav");
            audioManager.LoadSound(impactEffects[2], resourceFolder, "metal_grate_large_03.wav");
            audioManager.LoadSound(impactEffects[3], resourceFolder, "metal_grate_large_04.wav");
            audioManager.LoadSound(damageEffects[0], resourceFolder, "metal_pipe_large_01.wav");
            audioManager.LoadSound(damageEffects[1], resourceFolder, "metal_pipe_large_02.wav");
            audioManager.LoadSound(damageEffects[2], resourceFolder, "metal_pipe_large_03.wav");
            audioManager.LoadSound(damageEffects[3], resourceFolder, "metal_pipe_large_04.wav");

            audioManager.AddEffectParams(
                music,
                new GameAudioEffectParameters
                {
                    IsLooped = true,
                    SoundName = music,
                    UseAudio3D = false,
                });

            audioManager.AddEffectParams(
                tankMoveEffect,
                new GameAudioEffectParameters
                {
                    SoundName = "Tank",
                    DestroyWhenFinished = false,
                    IsLooped = true,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 0.5f,
                });

            audioManager.AddEffectParams(
                tankDestroyedEffect,
                new GameAudioEffectParameters
                {
                    SoundName = "TankDestroyed",
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });

            audioManager.AddEffectParams(
                tankShootingEffect,
                new GameAudioEffectParameters
                {
                    SoundName = "TankShooting",
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });

            audioManager.AddEffectParams(
                impactEffects[0],
                new GameAudioEffectParameters
                {
                    SoundName = impactEffects[0],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });
            audioManager.AddEffectParams(
                impactEffects[1],
                new GameAudioEffectParameters
                {
                    SoundName = impactEffects[1],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });
            audioManager.AddEffectParams(
                impactEffects[2],
                new GameAudioEffectParameters
                {
                    SoundName = impactEffects[2],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });
            audioManager.AddEffectParams(
                impactEffects[3],
                new GameAudioEffectParameters
                {
                    SoundName = impactEffects[3],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });

            audioManager.AddEffectParams(
                damageEffects[0],
                new GameAudioEffectParameters
                {
                    SoundName = damageEffects[0],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });
            audioManager.AddEffectParams(
                damageEffects[1],
                new GameAudioEffectParameters
                {
                    SoundName = damageEffects[1],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });
            audioManager.AddEffectParams(
                damageEffects[2],
                new GameAudioEffectParameters
                {
                    SoundName = damageEffects[2],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });
            audioManager.AddEffectParams(
                damageEffects[3],
                new GameAudioEffectParameters
                {
                    SoundName = damageEffects[3],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });
        }

        public void Start(float masterVolume)
        {
            audioManager.MasterVolume = masterVolume;
            audioManager.Start();
        }

        public void PlayMusic()
        {
            if (musicEffectInstance == null)
            {
                musicEffectInstance = audioManager.CreateEffectInstance(music);
                musicEffectInstance.Volume = 0.5f;
                musicEffectInstance.Play();
            }
        }
        public void PlayEffectMove(ITransformable3D emitter)
        {
            if (tankMoveEffectInstance == null)
            {
                tankMoveEffectInstance = audioManager.CreateEffectInstance(tankMoveEffect, emitter, Scene.Camera);
                tankMoveEffectInstance.Volume = 0.5f;
                tankMoveEffectInstance.Play();

                Task.Run(async () =>
                {
                    await Task.Delay(10000);
                    tankMoveEffectInstance.Stop();
                    tankMoveEffectInstance.Dispose();
                    tankMoveEffectInstance = null;
                });
            }
        }
        public void PlayEffectShooting(ITransformable3D emitter)
        {
            audioManager.CreateEffectInstance(tankShootingEffect, emitter, Scene.Camera)?.Play();
        }
        public void PlayEffectImpact(ITransformable3D emitter)
        {
            int index = Helper.RandomGenerator.Next(0, impactEffects.Length);
            index %= impactEffects.Length - 1;
            audioManager.CreateEffectInstance(impactEffects[index], emitter, Scene.Camera)?.Play();
        }
        public void PlayEffectDamage(ITransformable3D emitter)
        {
            int index = Helper.RandomGenerator.Next(0, damageEffects.Length);
            index %= damageEffects.Length - 1;
            audioManager.CreateEffectInstance(damageEffects[index], emitter, Scene.Camera)?.Play();
        }
        public void PlayEffectDestroyed(ITransformable3D emitter)
        {
            audioManager.CreateEffectInstance(tankDestroyedEffect, emitter, Scene.Camera)?.Play();
        }
        public void PlayEffectDestroyed(Vector3 emitter)
        {
            audioManager.CreateEffectInstance(tankDestroyedEffect, emitter, Scene.Camera)?.Play();
        }
    }
}
