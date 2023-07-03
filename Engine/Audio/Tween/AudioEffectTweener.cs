using Engine.Tween;

namespace Engine.Audio.Tween
{
    /// <summary>
    /// Audio effect tween extensions
    /// </summary>
    public class AudioEffectTweener
    {
        /// <summary>
        /// Tweener
        /// </summary>
        private readonly Tweener tweener;
        /// <summary>
        /// Tween collection
        /// </summary>
        private readonly AudioEffectTweenCollection collection = new();

        /// <summary>
        /// constructor
        /// </summary>
        public AudioEffectTweener(Tweener tweener)
        {
            this.tweener = tweener;

            // Register the collection into the tween manager
            this.tweener.AddTweenCollection(collection);
        }

        /// <summary>
        /// Clears all tweens
        /// </summary>
        /// <param name="control">Control</param>
        public void ClearTween(IAudioEffect effect)
        {
            collection.ClearTween(effect);
        }

        /// <summary>
        /// Volume up an effect
        /// </summary>
        /// <param name="effect">Effect</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public void TweenVolumeUp(IAudioEffect effect, long duration, ScaleFunc fnc)
        {
            if (effect == null)
            {
                return;
            }

            TweenVolume(effect, 0, 1, duration, fnc);
        }
        /// <summary>
        /// Volume down an effect
        /// </summary>
        /// <param name="effect">Effect</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public void TweenVolumeDown(IAudioEffect effect, long duration, ScaleFunc fnc)
        {
            if (effect == null)
            {
                return;
            }

            TweenVolume(effect, 1, 0, duration, fnc);
        }
        /// <summary>
        /// Changes the volume of an effect
        /// </summary>
        /// <param name="effect">Effect</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public void TweenVolume(IAudioEffect effect, float from, float to, long duration, ScaleFunc fnc)
        {
            if (effect == null)
            {
                return;
            }

            FloatTween ftScale = new();

            ftScale.Start(from, to, duration, fnc);

            AddVolumeTween(effect, ftScale);
        }
        /// <summary>
        /// Bounces the volume of an effect
        /// </summary>
        /// <param name="effect">Effect</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public void TweenVolumeBounce(IAudioEffect effect, float from, float to, long duration, ScaleFunc fnc)
        {
            if (effect == null)
            {
                return;
            }

            FloatTween ftScale = new();

            ftScale.Start(from, to, duration, fnc);

            AddVolumeBounce(effect, ftScale);
        }

        /// <summary>
        /// Adds a scale task to the internal task list
        /// </summary>
        /// <param name="effect">Effect</param>
        /// <param name="ftVolume">Volume tween</param>
        public void AddVolumeTween(IAudioEffect effect, FloatTween ftVolume)
        {
            effect.Volume = ftVolume.StartValue;

            collection.AddTween(effect, (d) =>
            {
                ftVolume.Update(d);

                effect.Volume = ftVolume.CurrentValue;

                if (ftVolume.CurrentValue == ftVolume.EndValue)
                {
                    return true;
                }

                return false;
            });
        }
        /// <summary>
        /// Adds a bouncing scale task to the internal task list
        /// </summary>
        /// <param name="effect">Effect</param>
        /// <param name="ftVolume">Volume tween</param>
        public void AddVolumeBounce(IAudioEffect effect, FloatTween ftVolume)
        {
            effect.Volume = ftVolume.StartValue;

            collection.AddTween(effect, (d) =>
            {
                ftVolume.Update(d);

                effect.Volume = ftVolume.CurrentValue;

                if (ftVolume.CurrentValue == ftVolume.EndValue)
                {
                    var newStart = ftVolume.EndValue;
                    var newEnd = ftVolume.StartValue;

                    ftVolume.Restart(newStart, newEnd);
                }

                return false;
            });
        }
    }

    /// <summary>
    /// Tweener extensions
    /// </summary>
    public static class AudioEffectTweenerExtensions
    {
        /// <summary>
        /// Creates a new tweener component
        /// </summary>
        /// <param name="scene">Scene</param>
        public static AudioEffectTweener AddAudioEffectTweener(this Scene scene)
        {
            var tweener = scene.Components.First<Tweener>() ?? throw new EngineException($"{nameof(Tweener)} scene component not present.");

            return new AudioEffectTweener(tweener);
        }
    }
}
