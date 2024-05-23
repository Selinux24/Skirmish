using Engine;
using Engine.Audio;
using Engine.Common;
using System;
using System.Collections.Generic;

namespace TerrainSamples.SceneModularDungeon
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

        private string soundDoor = null;
        private string soundLadder = null;
        private string soundTorch = null;

        private string[] soundWinds = null;

        private string ratSoundMove = null;
        private string ratSoundTalk = null;

        private IGameAudioEffect ratSoundInstance = null;

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
            audioManager.UseMasteringLimiter = true;
            audioManager.SetMasteringLimit(15, 1500);

            //Sounds
            soundDoor = "door";
            soundLadder = "ladder";
            audioManager.LoadSound(soundDoor, resourceFolder, "door.wav");
            audioManager.LoadSound(soundLadder, resourceFolder, "ladder.wav");

            string soundWind1 = "wind1";
            string soundWind2 = "wind2";
            string soundWind3 = "wind3";
            audioManager.LoadSound(soundWind1, resourceFolder, "Wind1_S.wav");
            audioManager.LoadSound(soundWind2, resourceFolder, "Wind2_S.wav");
            audioManager.LoadSound(soundWind3, resourceFolder, "Wind3_S.wav");
            soundWinds = [soundWind1, soundWind2, soundWind3];

            ratSoundMove = "mouseMove";
            ratSoundTalk = "mouseTalk";
            audioManager.LoadSound(ratSoundMove, resourceFolder, "mouse1.wav");
            audioManager.LoadSound(ratSoundTalk, resourceFolder, "mouse2.wav");

            soundTorch = "torch";
            audioManager.LoadSound(soundTorch, resourceFolder, "loop_torch.wav");

            //Effects
            audioManager.AddEffectParams(
                soundDoor,
                new GameAudioEffectParameters
                {
                    SoundName = soundDoor,
                    DestroyWhenFinished = true,
                    Volume = 1f,
                    UseAudio3D = true,
                    ReverbPreset = GameAudioReverbPresets.StoneRoom,
                    EmitterRadius = 3,
                    ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                });

            audioManager.AddEffectParams(
                soundLadder,
                new GameAudioEffectParameters
                {
                    SoundName = soundLadder,
                    DestroyWhenFinished = true,
                    Volume = 1f,
                    UseAudio3D = true,
                    ReverbPreset = GameAudioReverbPresets.StoneRoom,
                    EmitterRadius = 3,
                    ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                });

            for (int i = 0; i < soundWinds.Length; i++)
            {
                audioManager.AddEffectParams(
                    soundWinds[i],
                    new GameAudioEffectParameters
                    {
                        DestroyWhenFinished = true,
                        IsLooped = false,
                        SoundName = soundWinds[i],
                        Volume = 1f,
                        UseAudio3D = true,
                        ReverbPreset = GameAudioReverbPresets.StoneRoom,
                        EmitterRadius = 15,
                        ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                    });
            }

            audioManager.AddEffectParams(
                ratSoundMove,
                new GameAudioEffectParameters
                {
                    SoundName = ratSoundMove,
                    DestroyWhenFinished = false,
                    Volume = 1f,
                    IsLooped = true,
                    UseAudio3D = true,
                    ReverbPreset = GameAudioReverbPresets.StoneRoom,
                    EmitterRadius = 3,
                    ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                });

            audioManager.AddEffectParams(
                ratSoundTalk,
                new GameAudioEffectParameters
                {
                    SoundName = ratSoundTalk,
                    DestroyWhenFinished = true,
                    Volume = 1f,
                    IsLooped = false,
                    UseAudio3D = true,
                    ReverbPreset = GameAudioReverbPresets.StoneRoom,
                    EmitterRadius = 3,
                    ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                });
        }

        public void Start(float masterVolume)
        {
            audioManager.MasterVolume = masterVolume;
            audioManager.Start();
        }
        public void Stop()
        {
            audioManager.Stop();
            audioManager.ClearEffects();
        }

        public void CreateRatSounds(ITransformable3D rat)
        {
            ratSoundInstance = audioManager.CreateEffectInstance(ratSoundMove, rat, Scene.Camera);
        }
        public void PlayRatMove()
        {
            ratSoundInstance?.Play();
        }
        public void StopRatMove()
        {
            ratSoundInstance?.Pause();
        }
        public void PlayRatTalk(ITransformable3D rat)
        {
            audioManager.CreateEffectInstance(ratSoundTalk, rat, Scene.Camera)?.Play();
        }

        public void CreateTorchEmitters(IEnumerable<ModelInstance> torchs)
        {
            int index = 0;
            foreach (var item in torchs)
            {
                string effectName = $"torch{index++}";

                audioManager.AddEffectParams(
                    effectName,
                    new GameAudioEffectParameters
                    {
                        SoundName = soundTorch,
                        DestroyWhenFinished = false,
                        Volume = 0.05f,
                        IsLooped = true,
                        UseAudio3D = true,
                        EmitterRadius = 2,
                        ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                    });

                audioManager.CreateEffectInstance(effectName, item, Scene.Camera)?.Play();
            }
        }
        public void CreateBigFireEmitters(IEnumerable<ModelInstance> fires)
        {
            int index = 0;
            foreach (var item in fires)
            {
                string effectName = $"bigFire{index++}";

                audioManager.AddEffectParams(
                    effectName,
                    new GameAudioEffectParameters
                    {
                        SoundName = soundTorch,
                        DestroyWhenFinished = false,
                        Volume = 1,
                        IsLooped = true,
                        UseAudio3D = true,
                        EmitterRadius = 5,
                        ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                    });

                audioManager.CreateEffectInstance(effectName, item, Scene.Camera)?.Play();
            }
        }

        public TimeSpan PlayDoor(ITransformable3D emitter, float volume = 0.5f)
        {
            var effect = audioManager.CreateEffectInstance(soundDoor, emitter, Scene.Camera);
            if (effect == null) return TimeSpan.Zero;

            effect.Volume = volume;
            effect.Play();
            return effect.Duration;
        }
        public void PlayLadder(ITransformable3D emitter, float volume = 0.5f)
        {
            var effect = audioManager.CreateEffectInstance(soundLadder, emitter, Scene.Camera);
            if (effect == null) return;

            effect.Volume = volume;
            effect.Play();
        }

        public TimeSpan PlayWind(IManipulator3D man)
        {
            int index = Helper.RandomGenerator.Next(0, soundWinds.Length + 1);
            index %= soundWinds.Length;

            var soundEffect = soundWinds[index];

            var windInstance = audioManager.CreateEffectInstance(soundEffect, man, Scene.Camera);
            if (windInstance != null)
            {
                windInstance.Play();
                return windInstance.Duration;
            }

            return TimeSpan.Zero;
        }
    }
}
