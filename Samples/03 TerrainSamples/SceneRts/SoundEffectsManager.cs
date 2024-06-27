using Engine;
using Engine.Audio;
using Engine.Common;
using System;

namespace TerrainSamples.SceneRts
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

        private string heliEffect;
        private string tank1Effect;
        private string tank2Effect;

        private const string forestEffect = "Forest";
        private const string heliDestroyedEffect = "HelicopterDestroyed";
        private const string tank1DestroyedEffect = "Tank1Destroyed";
        private const string tank2DestroyedEffect = "Tank2Destroyed";
        private const string tank1ShootingEffect = "Tank1Shooting";
        private const string tank2ShootingEffect = "Tank2Shooting";
        private readonly string[] impactEffects = ["Impact1", "Impact2", "Impact3", "Impact4"];
        private readonly string[] damageEffects = ["Damage1", "Damage2", "Damage3", "Damage4"];

        private IGameAudioEffect forestEffectInstance;
        private IGameAudioEffect heliEffectInstance;
        private IGameAudioEffect tank1EffectInstance;
        private IGameAudioEffect tank2EffectInstance;

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
        public void InitializeAudio(string resourceFolder, string helicopterString, string tank1String, string tank2String)
        {
            heliEffect = helicopterString;
            tank1Effect = tank1String;
            tank2Effect = tank2String;

            audioManager.LoadSound(forestEffect, resourceFolder, "wind_birds_forest_01.wav");
            audioManager.LoadSound(heliEffect, resourceFolder, "heli.wav");
            audioManager.LoadSound(heliDestroyedEffect, resourceFolder, "explosion_helicopter_close_01.wav");
            audioManager.LoadSound("Tank", resourceFolder, "tank_engine.wav");
            audioManager.LoadSound("TankDestroyed", resourceFolder, "explosion_vehicle_small_close_01.wav");
            audioManager.LoadSound("TankShooting", resourceFolder, "machinegun-shooting.wav");
            audioManager.LoadSound(impactEffects[0], resourceFolder, "metal_grate_large_01.wav");
            audioManager.LoadSound(impactEffects[1], resourceFolder, "metal_grate_large_02.wav");
            audioManager.LoadSound(impactEffects[2], resourceFolder, "metal_grate_large_03.wav");
            audioManager.LoadSound(impactEffects[3], resourceFolder, "metal_grate_large_04.wav");
            audioManager.LoadSound(damageEffects[0], resourceFolder, "metal_pipe_large_01.wav");
            audioManager.LoadSound(damageEffects[1], resourceFolder, "metal_pipe_large_02.wav");
            audioManager.LoadSound(damageEffects[2], resourceFolder, "metal_pipe_large_03.wav");
            audioManager.LoadSound(damageEffects[3], resourceFolder, "metal_pipe_large_04.wav");

            audioManager.AddEffectParams(
                forestEffect,
                new GameAudioEffectParameters
                {
                    SoundName = forestEffect,
                    DestroyWhenFinished = false,
                    IsLooped = true,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });

            audioManager.AddEffectParams(
                heliEffect,
                new GameAudioEffectParameters
                {
                    SoundName = heliEffect,
                    DestroyWhenFinished = false,
                    IsLooped = true,
                    UseAudio3D = true,
                    EmitterRadius = 200,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });

            audioManager.AddEffectParams(
                heliDestroyedEffect,
                new GameAudioEffectParameters
                {
                    SoundName = heliDestroyedEffect,
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });

            audioManager.AddEffectParams(
                tank1Effect,
                new GameAudioEffectParameters
                {
                    SoundName = "Tank",
                    DestroyWhenFinished = false,
                    IsLooped = true,
                    UseAudio3D = true,
                    EmitterRadius = 150,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });

            audioManager.AddEffectParams(
                tank2Effect,
                new GameAudioEffectParameters
                {
                    SoundName = "Tank",
                    DestroyWhenFinished = false,
                    IsLooped = true,
                    UseAudio3D = true,
                    EmitterRadius = 150,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });

            audioManager.AddEffectParams(
                tank1DestroyedEffect,
                new GameAudioEffectParameters
                {
                    SoundName = "TankDestroyed",
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });

            audioManager.AddEffectParams(
                tank2DestroyedEffect,
                new GameAudioEffectParameters
                {
                    SoundName = "TankDestroyed",
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });

            audioManager.AddEffectParams(
                tank1ShootingEffect,
                new GameAudioEffectParameters
                {
                    SoundName = "TankShooting",
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });

            audioManager.AddEffectParams(
                tank2ShootingEffect,
                new GameAudioEffectParameters
                {
                    SoundName = "TankShooting",
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });

            audioManager.AddEffectParams(
                impactEffects[0],
                new GameAudioEffectParameters
                {
                    SoundName = impactEffects[0],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });
            audioManager.AddEffectParams(
                impactEffects[1],
                new GameAudioEffectParameters
                {
                    SoundName = impactEffects[1],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });
            audioManager.AddEffectParams(
                impactEffects[2],
                new GameAudioEffectParameters
                {
                    SoundName = impactEffects[2],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });
            audioManager.AddEffectParams(
                impactEffects[3],
                new GameAudioEffectParameters
                {
                    SoundName = impactEffects[3],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });

            audioManager.AddEffectParams(
                damageEffects[0],
                new GameAudioEffectParameters
                {
                    SoundName = damageEffects[0],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });
            audioManager.AddEffectParams(
                damageEffects[1],
                new GameAudioEffectParameters
                {
                    SoundName = damageEffects[1],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });
            audioManager.AddEffectParams(
                damageEffects[2],
                new GameAudioEffectParameters
                {
                    SoundName = damageEffects[2],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });
            audioManager.AddEffectParams(
                damageEffects[3],
                new GameAudioEffectParameters
                {
                    SoundName = damageEffects[3],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    ReverbPreset = GameAudioReverbPresets.Forest,
                    Volume = 1f,
                });

        }

        public void Start(float masterVolume)
        {
            audioManager.MasterVolume = masterVolume;
            audioManager.Start();
        }

        public void PlayForest()
        {
            if (forestEffectInstance == null)
            {
                forestEffectInstance = audioManager.CreateEffectInstance(forestEffect);
                forestEffectInstance.Play();
            }
        }

        public void PlayHelicopterMoving(ITransformable3D helicopter)
        {
            if (heliEffectInstance == null)
            {
                heliEffectInstance = audioManager.CreateEffectInstance(heliEffect, helicopter, Scene.Camera);
                heliEffectInstance.Play();
            }
        }
        public void StopHelicopterMoving()
        {
            heliEffectInstance?.Stop();
        }
        public void PlayHelicopterDestroyed(ITransform emitter)
        {
            audioManager.CreateEffectInstance(heliDestroyedEffect, emitter, Scene.Camera)?.Play();
        }

        public void PlayTank1Moving(ITransformable3D tankP1)
        {
            if (tank1EffectInstance == null)
            {
                tank1EffectInstance = audioManager.CreateEffectInstance(tank1Effect, tankP1, Scene.Camera);
                tank1EffectInstance?.Play();
            }
        }
        public void StopTank1Moving()
        {
            tank1EffectInstance?.Stop();
        }
        public void PlayTank1Shooting(ITransform emitter)
        {
            audioManager.CreateEffectInstance(tank1ShootingEffect, emitter, Scene.Camera)?.Play();
        }
        public void PlayTank1Destroyed(ITransform emitter)
        {
            audioManager.CreateEffectInstance(tank1DestroyedEffect, emitter, Scene.Camera)?.Play();
        }

        public void PlayTank2Moving(ITransformable3D tankP2)
        {
            if (tank2EffectInstance == null)
            {
                tank2EffectInstance = audioManager.CreateEffectInstance(tank2Effect, tankP2, Scene.Camera);
                tank2EffectInstance?.Play();
            }
        }
        public void StopTank2Moving()
        {
            tank2EffectInstance?.Stop();
        }
        public void PlayTank2Shooting(ITransform emitter)
        {
            audioManager.CreateEffectInstance(tank2ShootingEffect, emitter, Scene.Camera)?.Play();
        }
        public void PlayTank2Destroyed(ITransform emitter)
        {
            audioManager.CreateEffectInstance(tank2DestroyedEffect, emitter, Scene.Camera)?.Play();
        }

        public void PlayImpact(ITransform emitter)
        {
            int index = Helper.RandomGenerator.Next(0, 4);
            index %= 3;
            audioManager.CreateEffectInstance(impactEffects[index], emitter, Scene.Camera)?.Play();
        }
        public void PlayDamage(ITransform emitter)
        {
            int index = Helper.RandomGenerator.Next(0, 4);
            index %= 3;
            audioManager.CreateEffectInstance(damageEffects[index], emitter, Scene.Camera)?.Play();
        }
    }
}
