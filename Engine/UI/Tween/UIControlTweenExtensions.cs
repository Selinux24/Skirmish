using Engine.Tween;
using SharpDX;

namespace Engine.UI.Tween
{
    /// <summary>
    /// UI control tween extensions
    /// </summary>
    public static class UIControlTweenExtensions
    {
        /// <summary>
        /// Tween collection
        /// </summary>
        private static readonly UIControlTweenCollection collection = new UIControlTweenCollection();

        /// <summary>
        /// Static constructor
        /// </summary>
        static UIControlTweenExtensions()
        {
            // Register the collection into the tween manager
            FloatTweenManager.AddTweenCollection(collection);
        }

        /// <summary>
        /// Clears all tweens
        /// </summary>
        /// <param name="control">Control</param>
        public static void ClearTween(this UIControl control)
        {
            collection.ClearTween(control);
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

            AddScaleTween(control, ftScale);
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

            AddScaleBounce(control, ftScale);
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

            AddRotateTween(control, ftRotate);
        }
        /// <summary>
        /// Rotate a control and repeat
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public static void TweenRotateRepeat(this UIControl control, float from, float to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftRotate = new FloatTween();

            ftRotate.Start(from, to, duration, fnc);

            AddRotateRepeat(control, ftRotate);
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

            AddRotateBounce(control, ftRotate);
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

            AddAlphaTween(control, ftAlpha);
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

            AddAlphaBounce(control, ftAlpha);
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

            AddColorTween(control, ftColorR, ftColorG, ftColorB);
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

            AddColorBounce(control, ftColorR, ftColorG, ftColorB);
        }

        /// <summary>
        /// Adds a scale task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftScale">Scale tween</param>
        public static void AddScaleTween(this UIControl control, FloatTween ftScale)
        {
            control.Scale = ftScale.StartValue;

            collection.AddTween(control, (d) =>
            {
                ftScale.Update(d);

                control.Scale = ftScale.CurrentValue;
                control.Visible = control.Scale != 0;

                if (ftScale.CurrentValue == ftScale.EndValue)
                {
                    return true;
                }

                return false;
            });
        }
        /// <summary>
        /// Adds a bouncing scale task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftScale">Scale tween</param>
        public static void AddScaleBounce(this UIControl control, FloatTween ftScale)
        {
            control.Scale = ftScale.StartValue;

            collection.AddTween(control, (d) =>
            {
                ftScale.Update(d);

                control.Scale = ftScale.CurrentValue;
                control.Visible = control.Scale != 0;

                if (ftScale.CurrentValue == ftScale.EndValue)
                {
                    var newStart = ftScale.EndValue;
                    var newEnd = ftScale.StartValue;

                    ftScale.Restart(newStart, newEnd);
                }

                return false;
            });
        }
        /// <summary>
        /// Adds a rotation task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftRotate">Rotation tween</param>
        public static void AddRotateTween(this UIControl control, FloatTween ftRotate)
        {
            control.Rotation = ftRotate.StartValue;

            collection.AddTween(control, (d) =>
            {
                ftRotate.Update(d);

                control.Rotation = ftRotate.CurrentValue;

                if (ftRotate.CurrentValue == ftRotate.EndValue)
                {
                    return true;
                }

                return false;
            });
        }
        /// <summary>
        /// Adds a repeat rotation task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftRotate">Rotation tween</param>
        public static void AddRotateRepeat(this UIControl control, FloatTween ftRotate)
        {
            control.Rotation = ftRotate.StartValue;

            collection.AddTween(control, (d) =>
            {
                ftRotate.Update(d);

                control.Rotation = ftRotate.CurrentValue;

                if (ftRotate.CurrentValue == ftRotate.EndValue)
                {
                    var newStart = ftRotate.StartValue;
                    var newEnd = ftRotate.EndValue;

                    ftRotate.Restart(newStart, newEnd);
                }

                return false;
            });
        }
        /// <summary>
        /// Adds a bouncing rotation task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftRotate">Rotation tween</param>
        public static void AddRotateBounce(this UIControl control, FloatTween ftRotate)
        {
            control.Rotation = ftRotate.StartValue;

            collection.AddTween(control, (d) =>
            {
                ftRotate.Update(d);

                control.Rotation = ftRotate.CurrentValue;

                if (ftRotate.CurrentValue == ftRotate.EndValue)
                {
                    var newStart = ftRotate.EndValue;
                    var newEnd = ftRotate.StartValue;

                    ftRotate.Restart(newStart, newEnd);
                }

                return false;
            });
        }
        /// <summary>
        /// Adds a color tweening task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftColorR">Red tween</param>
        /// <param name="ftColorG">Green tween</param>
        /// <param name="ftColorB">Blue tween</param>
        public static void AddColorTween(this UIControl control, FloatTween ftColorR, FloatTween ftColorG, FloatTween ftColorB)
        {
            control.TintColor = new Color(ftColorR.StartValue, ftColorG.StartValue, ftColorB.StartValue);

            collection.AddTween(control, (d) =>
            {
                ftColorR.Update(d);
                ftColorG.Update(d);
                ftColorB.Update(d);

                control.TintColor = new Color(ftColorR.CurrentValue, ftColorG.CurrentValue, ftColorB.CurrentValue);

                if (ftColorR.CurrentValue == ftColorR.EndValue && ftColorG.CurrentValue == ftColorG.EndValue && ftColorB.CurrentValue == ftColorB.EndValue)
                {
                    return true;
                }

                return false;
            });
        }
        /// <summary>
        /// Adds a bouncing color tweening task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftColorR">Red tween</param>
        /// <param name="ftColorG">Green tween</param>
        /// <param name="ftColorB">Blue tween</param>
        public static void AddColorBounce(this UIControl control, FloatTween ftColorR, FloatTween ftColorG, FloatTween ftColorB)
        {
            control.TintColor = new Color(ftColorR.StartValue, ftColorG.StartValue, ftColorB.StartValue);

            collection.AddTween(control, (d) =>
            {
                ftColorR.Update(d);
                ftColorG.Update(d);
                ftColorB.Update(d);

                control.TintColor = new Color(ftColorR.CurrentValue, ftColorG.CurrentValue, ftColorB.CurrentValue);

                if (ftColorR.CurrentValue == ftColorR.EndValue && ftColorG.CurrentValue == ftColorG.EndValue && ftColorB.CurrentValue == ftColorB.EndValue)
                {
                    var newStartR = ftColorR.EndValue;
                    var newStartG = ftColorG.EndValue;
                    var newStartB = ftColorB.EndValue;

                    var newEndR = ftColorR.StartValue;
                    var newEndG = ftColorG.StartValue;
                    var newEndB = ftColorB.StartValue;

                    ftColorR.Restart(newStartR, newEndR);
                    ftColorG.Restart(newStartG, newEndG);
                    ftColorB.Restart(newStartB, newEndB);
                }

                return false;
            });
        }
        /// <summary>
        /// Adds an alpha task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftAlpha">Alpha tween</param>
        public static void AddAlphaTween(this UIControl control, FloatTween ftAlpha)
        {
            control.Alpha = ftAlpha.StartValue;

            collection.AddTween(control, (d) =>
            {
                ftAlpha.Update(d);

                control.Alpha = MathUtil.Clamp(ftAlpha.CurrentValue, 0f, 1f);
                control.Visible = control.Alpha != 0;

                if (ftAlpha.CurrentValue == ftAlpha.EndValue)
                {
                    return true;
                }

                return false;
            });
        }
        /// <summary>
        /// Adds a bouncing alpha task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftAlpha">Alpha tween</param>
        public static void AddAlphaBounce(this UIControl control, FloatTween ftAlpha)
        {
            control.Alpha = ftAlpha.StartValue;

            collection.AddTween(control, (d) =>
            {
                ftAlpha.Update(d);

                control.Alpha = MathUtil.Clamp(ftAlpha.CurrentValue, 0f, 1f);
                control.Visible = control.Alpha != 0;

                if (ftAlpha.CurrentValue == ftAlpha.EndValue)
                {
                    var newStart = ftAlpha.EndValue;
                    var newEnd = ftAlpha.StartValue;

                    ftAlpha.Restart(newStart, newEnd);
                }

                return false;
            });
        }
    }
}
