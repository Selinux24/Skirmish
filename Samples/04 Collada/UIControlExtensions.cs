using Engine.Tween;
using Engine.UI;

namespace Collada
{
    static class UIControlExtensions
    {
        public static void Show(this UIControl ctrl, float time)
        {
            ctrl.TweenShow(time, ScaleFuncs.Linear);
        }
        public static void Show(this UIControl ctrl, float time, float delay)
        {
            ctrl.TweenAlpha(-delay * time, 1, time, ScaleFuncs.Linear);
        }

        public static void Hide(this UIControl ctrl, float time)
        {
            ctrl.TweenHide(time, ScaleFuncs.Linear);
        }
        public static void Hide(this UIControl ctrl, float time, float delay)
        {
            ctrl.TweenAlpha(delay * time, 0, time, ScaleFuncs.Linear);
        }

        public static void ScaleInScaleOut(this UIControl ctrl, float from, float to, float time)
        {
            ctrl.TweenScaleBounce(from, to, time, ScaleFuncs.Linear);
        }
    }
}
