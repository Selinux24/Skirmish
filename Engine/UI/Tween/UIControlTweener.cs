using Engine.Tween;
using SharpDX;

namespace Engine.UI.Tween
{
    /// <summary>
    /// UI control tween extensions
    /// </summary>
    public class UIControlTweener
    {
        /// <summary>
        /// Tweener
        /// </summary>
        private readonly Tweener tweener;
        /// <summary>
        /// Tween collection
        /// </summary>
        private readonly UIControlTweenCollection collection = new();

        /// <summary>
        /// constructor
        /// </summary>
        public UIControlTweener(Tweener tweener)
        {
            this.tweener = tweener;

            // Register the collection into the tween manager
            this.tweener.AddTweenCollection(collection);
        }

        /// <summary>
        /// Clears all tweens
        /// </summary>
        /// <param name="control">Control</param>
        public void ClearTween(IUIControl control)
        {
            collection.ClearTween(control);
        }

        /// <summary>
        /// Scale up a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public void TweenScaleUp(IUIControl control, long duration, ScaleFunc fnc)
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
        public void TweenScaleDown(IUIControl control, long duration, ScaleFunc fnc)
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
        public void TweenScale(IUIControl control, float from, float to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftScale = new();

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
        public void TweenScaleBounce(IUIControl control, float from, float to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftScale = new();

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
        public void TweenRotate(IUIControl control, float targetAngle, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftRotate = new();

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
        public void TweenRotateRepeat(IUIControl control, float from, float to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftRotate = new();

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
        public void TweenRotateBounce(IUIControl control, float from, float to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftRotate = new();

            ftRotate.Start(from, to, duration, fnc);

            AddRotateBounce(control, ftRotate);
        }

        /// <summary>
        /// Shows a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public void TweenShow(IUIControl control, long duration, ScaleFunc fnc)
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
        public void TweenHide(IUIControl control, long duration, ScaleFunc fnc)
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
        public void TweenAlpha(IUIControl control, float from, float to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftAlpha = new();

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
        public void TweenAlphaBounce(IUIControl control, float from, float to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftAlpha = new();

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
        public void TweenBaseColor(IUIControl control, Color4 from, Color4 to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftColorR = new();
            FloatTween ftColorG = new();
            FloatTween ftColorB = new();

            ftColorR.Start(from.Red, to.Red, duration, fnc);
            ftColorG.Start(from.Green, to.Green, duration, fnc);
            ftColorB.Start(from.Blue, to.Blue, duration, fnc);

            AddBaseColorTween(control, ftColorR, ftColorG, ftColorB);
        }
        /// <summary>
        /// Bouncing the color of a control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="fnc">Scale function</param>
        public void TweenBaseColorBounce(IUIControl control, Color4 from, Color4 to, long duration, ScaleFunc fnc)
        {
            if (control == null)
            {
                return;
            }

            FloatTween ftColorR = new();
            FloatTween ftColorG = new();
            FloatTween ftColorB = new();

            ftColorR.Start(from.Red, to.Red, duration, fnc);
            ftColorG.Start(from.Green, to.Green, duration, fnc);
            ftColorB.Start(from.Blue, to.Blue, duration, fnc);

            AddBaseColorBounce(control, ftColorR, ftColorG, ftColorB);
        }

        /// <summary>
        /// Adds a scale task to the internal task list
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="ftScale">Scale tween</param>
        public void AddScaleTween(IUIControl control, FloatTween ftScale)
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
        public void AddScaleBounce(IUIControl control, FloatTween ftScale)
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
        public void AddRotateTween(IUIControl control, FloatTween ftRotate)
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
        public void AddRotateRepeat(IUIControl control, FloatTween ftRotate)
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
        public void AddRotateBounce(IUIControl control, FloatTween ftRotate)
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
        public void AddBaseColorTween(IUIControl control, FloatTween ftColorR, FloatTween ftColorG, FloatTween ftColorB)
        {
            control.BaseColor = new Color(ftColorR.StartValue, ftColorG.StartValue, ftColorB.StartValue);

            collection.AddTween(control, (d) =>
            {
                ftColorR.Update(d);
                ftColorG.Update(d);
                ftColorB.Update(d);

                control.BaseColor = new Color(ftColorR.CurrentValue, ftColorG.CurrentValue, ftColorB.CurrentValue);

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
        public void AddBaseColorBounce(IUIControl control, FloatTween ftColorR, FloatTween ftColorG, FloatTween ftColorB)
        {
            control.BaseColor = new Color(ftColorR.StartValue, ftColorG.StartValue, ftColorB.StartValue);

            collection.AddTween(control, (d) =>
            {
                ftColorR.Update(d);
                ftColorG.Update(d);
                ftColorB.Update(d);

                control.BaseColor = new Color(ftColorR.CurrentValue, ftColorG.CurrentValue, ftColorB.CurrentValue);

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
        public void AddAlphaTween(IUIControl control, FloatTween ftAlpha)
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
        public void AddAlphaBounce(IUIControl control, FloatTween ftAlpha)
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

    /// <summary>
    /// Tweener extensions
    /// </summary>
    public static class UIControlTweenerExtensions
    {
        /// <summary>
        /// Creates a new tweener component
        /// </summary>
        /// <param name="scene">Scene</param>
        public static UIControlTweener AddUIControlTweener(this Scene scene)
        {
            var tweener = scene.Components.First<Tweener>() ?? throw new EngineException($"{nameof(Tweener)} scene component not present.");

            return new UIControlTweener(tweener);
        }
    }
}
