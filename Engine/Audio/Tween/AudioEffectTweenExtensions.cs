using Engine.Tween;

namespace Engine.Audio.Tween
{
    /// <summary>
    /// Audio effect tween extensions
    /// </summary>
    public static class AudioEffectTweenExtensions
    {
        /// <summary>
        /// Tween collection
        /// </summary>
        private static readonly AudioEffectTweenCollection collection = new AudioEffectTweenCollection();

        /// <summary>
        /// Static constructor
        /// </summary>
        static AudioEffectTweenExtensions()
        {
            // Register the collection into the tween manager
            FloatTweenManager.AddTweenCollection(collection);
        }

        /// <summary>
        /// Clears all tweens
        /// </summary>
        /// <param name="effect">Effect</param>
        public static void ClearTween(this IAudioEffect effect)
        {
            collection.ClearTween(effect);
        }

        /// <summary>
        /// Volume up an effect
        /// </summary>
        /// <param name="effect">Effect</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenVolumeUp(this IAudioEffect effect, long duration, ScaleFunc fnc)
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
        public static void TweenVolumeDown(this IAudioEffect effect, long duration, ScaleFunc fnc)
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
        public static void TweenVolume(this IAudioEffect effect, float from, float to, long duration, ScaleFunc fnc)
        {
            if (effect == null)
            {
                return;
            }

            FloatTween ftScale = new FloatTween();

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
        public static void TweenVolumeBounce(this IAudioEffect effect, float from, float to, long duration, ScaleFunc fnc)
        {
            if (effect == null)
            {
                return;
            }

            FloatTween ftScale = new FloatTween();

            ftScale.Start(from, to, duration, fnc);

            AddVolumeBounce(effect, ftScale);
        }

        /// <summary>
        /// Adds a scale task to the internal task list
        /// </summary>
        /// <param name="effect">Effect</param>
        /// <param name="ftVolume">Volume tween</param>
        public static void AddVolumeTween(this IAudioEffect effect, FloatTween ftVolume)
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
        public static void AddVolumeBounce(this IAudioEffect effect, FloatTween ftVolume)
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
}
