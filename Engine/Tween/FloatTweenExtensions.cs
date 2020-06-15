using Engine.UI;

namespace Engine.Tween
{
    /// <summary>
    /// Float tween extensions
    /// </summary>
    public static class FloatTweenExtensions
    {
        /// <summary>
        /// Clears all tweens
        /// </summary>
        /// <param name="control">Control</param>
        public static void ClearTween(this UIControl control)
        {
            FloatTweenManager.ClearTween(control);
        }

        /// <summary>
        /// Scale up a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="duration">Duration</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenScaleUp(this UIControl control, float duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            TweenScale(control, 0, 1, duration, fnc);
        }
        /// <summary>
        /// Scales down a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="duration">Duration</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenScaleDown(this UIControl control, float duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            TweenScale(control, 1, 0, duration, fnc);
        }
        /// <summary>
        /// Scales a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenScale(this UIControl control, float from, float to, float duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftScale = new FloatTween();

            ftScale.Start(from, to, duration, fnc);

            FloatTweenManager.AddScaleTween(control, ftScale);
        }

        /// <summary>
        /// Bouncing scale a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenScaleBounce(this UIControl control, float from, float to, float duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftScale = new FloatTween();

            ftScale.Start(from, to, duration, fnc);

            FloatTweenManager.AddScaleBounce(control, ftScale);
        }

        /// <summary>
        /// Rotate a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="targetAngle">Target angle</param>
        /// <param name="duration">Duration</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenRotate(this UIControl control, float targetAngle, float duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftRotate = new FloatTween();

            ftRotate.Start(control.Rotation, targetAngle, duration, fnc);

            FloatTweenManager.AddRotateTween(control, ftRotate);
        }

        /// <summary>
        /// Shows a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="duration">Duration</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenShow(this UIControl control, float duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            TweenAlpha(control, 0, 1, duration, fnc);
        }
        /// <summary>
        /// Hides a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="duration">Duration</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenHide(this UIControl control, float duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            TweenAlpha(control, 1, 0, duration, fnc);
        }
        /// <summary>
        /// Changes the alpha component of a control color
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenAlpha(this UIControl control, float from, float to, float duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftAlpha = new FloatTween();

            ftAlpha.Start(from, to, duration, fnc);

            FloatTweenManager.AddAlphaTween(control, ftAlpha);
        }
    }
}
