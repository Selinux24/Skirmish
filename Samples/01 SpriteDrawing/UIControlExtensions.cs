using Engine.Tween;
using Engine.UI;
using SharpDX;

namespace SpriteDrawing
{
    static class UIControlExtensions
    {
        public static void Show(this UIControl ctrl, float time)
        {
            ctrl.TweenShow(time, ScaleFuncs.Linear);
        }

        public static void Hide(this UIControl ctrl, float time)
        {
            ctrl.TweenHide(time, ScaleFuncs.Linear);
        }

        public static void Roll(this UIControl ctrl, float time)
        {
            ctrl.TweenRotate(MathUtil.TwoPi, time, ScaleFuncs.Linear);
            ctrl.TweenScale(1, 0.5f, time, ScaleFuncs.QuinticEaseOut);
        }

        public static void ShowRoll(this UIControl ctrl, float time)
        {
            ctrl.TweenScaleUp(time, ScaleFuncs.QuinticEaseOut);
            ctrl.TweenShow(time * 0.25f, ScaleFuncs.Linear);
            ctrl.TweenRotate(MathUtil.TwoPi, time * 0.25f, ScaleFuncs.Linear);
        }

        public static void HideRoll(this UIControl ctrl, float time)
        {
            ctrl.TweenScaleDown(time, ScaleFuncs.QuinticEaseOut);
            ctrl.TweenHide(time * 0.25f, ScaleFuncs.Linear);
            ctrl.TweenRotate(-MathUtil.TwoPi, time * 0.25f, ScaleFuncs.Linear);
        }


        public static void ScaleInScaleOut(this UIControl ctrl, float from, float to, float time)
        {
            ctrl.TweenScaleBounce(from, to, time, ScaleFuncs.Linear);
        }
    }
}
