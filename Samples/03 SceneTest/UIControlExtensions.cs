using Engine;
using Engine.Tween;
using Engine.UI.Tween;

namespace SceneTest
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

        public static void ScaleInScaleOut(this UIControlTweener tweener, IUIControl ctrl, float from, float to, long milliseconds)
        {
            tweener.TweenScaleBounce(ctrl, from, to, milliseconds, ScaleFuncs.Linear);
        }
    }
}
