﻿using Engine.Tween;
using Engine;
using Engine.UI.Tween;
using SharpDX;

namespace SpriteDrawing
{
    static class UIControlExtensions
    {
        public static void Show(this IUIControl ctrl, long milliseconds)
        {
            ctrl.TweenShow(milliseconds, ScaleFuncs.Linear);
        }

        public static void Hide(this IUIControl ctrl, long milliseconds)
        {
            ctrl.TweenHide(milliseconds, ScaleFuncs.Linear);
        }

        public static void Roll(this IUIControl ctrl, long milliseconds)
        {
            ctrl.TweenRotate(MathUtil.TwoPi, milliseconds, ScaleFuncs.Linear);
            ctrl.TweenScale(1, 0.5f, milliseconds, ScaleFuncs.QuinticEaseOut);
        }

        public static void ShowRoll(this IUIControl ctrl, long milliseconds)
        {
            ctrl.TweenScaleUp(milliseconds, ScaleFuncs.QuinticEaseOut);
            ctrl.TweenShow(milliseconds / 4, ScaleFuncs.Linear);
            ctrl.TweenRotate(MathUtil.TwoPi, milliseconds / 4, ScaleFuncs.Linear);
        }

        public static void HideRoll(this IUIControl ctrl, long milliseconds)
        {
            ctrl.TweenScaleDown(milliseconds, ScaleFuncs.QuinticEaseOut);
            ctrl.TweenHide(milliseconds / 4, ScaleFuncs.Linear);
            ctrl.TweenRotate(-MathUtil.TwoPi, milliseconds / 4, ScaleFuncs.Linear);
        }

        public static void ScaleInScaleOut(this IUIControl ctrl, float from, float to, long milliseconds)
        {
            ctrl.TweenScaleBounce(from, to, milliseconds, ScaleFuncs.Linear);
        }
    }
}
