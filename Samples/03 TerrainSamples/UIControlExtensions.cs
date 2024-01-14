using Engine;
using Engine.Tween;
using Engine.UI.Tween;
using SharpDX;

namespace TerrainSamples
{
    static class UIControlExtensions
    {
        public static void Show(this UIControlTweener tweener, IUIControl ctrl, long milliseconds)
        {
            tweener.TweenShow(ctrl, milliseconds, ScaleFuncs.Linear);
        }

        public static void Hide(this UIControlTweener tweener, IUIControl ctrl, long milliseconds)
        {
            tweener.TweenHide(ctrl, milliseconds, ScaleFuncs.Linear);
        }

        public static void FadeOff(this UIControlTweener tweener, IUIControl ctrl, long milliseconds)
        {
            tweener.TweenHide(ctrl, milliseconds, ScaleFuncs.CubicEaseIn);
        }

        public static void Roll(this UIControlTweener tweener, IUIControl ctrl, long milliseconds)
        {
            tweener.TweenRotate(ctrl, MathUtil.TwoPi, milliseconds, ScaleFuncs.Linear);
            tweener.TweenScale(ctrl, 1, 0.5f, milliseconds, ScaleFuncs.QuinticEaseOut);
        }

        public static void ShowRoll(this UIControlTweener tweener, IUIControl ctrl, long milliseconds)
        {
            tweener.TweenScaleUp(ctrl, milliseconds, ScaleFuncs.QuinticEaseOut);
            tweener.TweenShow(ctrl, milliseconds / 4, ScaleFuncs.Linear);
            tweener.TweenRotate(ctrl, MathUtil.TwoPi, milliseconds / 4, ScaleFuncs.Linear);
        }

        public static void HideRoll(this UIControlTweener tweener, IUIControl ctrl, long milliseconds)
        {
            tweener.TweenScaleDown(ctrl, milliseconds, ScaleFuncs.QuinticEaseOut);
            tweener.TweenHide(ctrl, milliseconds / 4, ScaleFuncs.Linear);
            tweener.TweenRotate(ctrl, -MathUtil.TwoPi, milliseconds / 4, ScaleFuncs.Linear);
        }

        public static void ScaleInScaleOut(this UIControlTweener tweener, IUIControl ctrl, float from, float to, long milliseconds)
        {
            tweener.TweenScaleBounce(ctrl, from, to, milliseconds, ScaleFuncs.Linear);
        }
    }
}
