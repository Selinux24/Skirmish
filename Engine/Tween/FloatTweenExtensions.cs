using Engine.UI;
using SharpDX;

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
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenScaleUp(this UIControl control, long duration, ScaleFunc fnc)
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
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenScaleDown(this UIControl control, long duration, ScaleFunc fnc)
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
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenScale(this UIControl control, float from, float to, long duration, ScaleFunc fnc)
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
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenScaleBounce(this UIControl control, float from, float to, long duration, ScaleFunc fnc)
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
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenRotate(this UIControl control, float targetAngle, long duration, ScaleFunc fnc)
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
        /// Bouncing rotate a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenRotateBounce(this UIControl control, float from, float to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftRotate = new FloatTween();

            ftRotate.Start(from, to, duration, fnc);

            FloatTweenManager.AddRotateBounce(control, ftRotate);
        }

        /// <summary>
        /// Shows a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenShow(this UIControl control, long duration, ScaleFunc fnc)
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
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenHide(this UIControl control, long duration, ScaleFunc fnc)
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
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenAlpha(this UIControl control, float from, float to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftAlpha = new FloatTween();

            ftAlpha.Start(from, to, duration, fnc);

            FloatTweenManager.AddAlphaTween(control, ftAlpha);
        }
        /// <summary>
        /// Bouncing the alpha of a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenAlphaBounce(this UIControl control, float from, float to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftAlpha = new FloatTween();

            ftAlpha.Start(from, to, duration, fnc);

            FloatTweenManager.AddAlphaBounce(control, ftAlpha);
        }

        /// <summary>
        /// Changes the color of a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenColor(this UIControl control, Color4 from, Color4 to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftColorR = new FloatTween();
            FloatTween ftColorG = new FloatTween();
            FloatTween ftColorB = new FloatTween();

            ftColorR.Start(from.Red, to.Red, duration, fnc);
            ftColorG.Start(from.Green, to.Green, duration, fnc);
            ftColorB.Start(from.Blue, to.Blue, duration, fnc);

            FloatTweenManager.AddColorTween(control, ftColorR, ftColorG, ftColorB);
        }
        /// <summary>
        /// Bouncing the color of a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenColorBounce(this UIControl control, Color4 from, Color4 to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftColorR = new FloatTween();
            FloatTween ftColorG = new FloatTween();
            FloatTween ftColorB = new FloatTween();

            ftColorR.Start(from.Red, to.Red, duration, fnc);
            ftColorG.Start(from.Green, to.Green, duration, fnc);
            ftColorB.Start(from.Blue, to.Blue, duration, fnc);

            FloatTweenManager.AddColorBounce(control, ftColorR, ftColorG, ftColorB);
        }
    }
}
