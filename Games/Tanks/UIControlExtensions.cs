using Engine;
using Engine.Tween;
using Engine.UI.Tween;

namespace Tanks
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
    }
}
