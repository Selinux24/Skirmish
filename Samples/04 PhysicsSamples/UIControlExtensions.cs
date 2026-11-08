using Engine;
using Engine.Tween;
using Engine.UI.Tween;

namespace PhysicsSamples
{
    static class UIControlExtensions
    {
        public static void ScaleInScaleOut(this UIControlTweener tweener, IUIControl ctrl, float from, float to, long milliseconds)
        {
            tweener.TweenScaleBounce(ctrl, from, to, milliseconds, ScaleFuncs.Linear);
        }
    }
}
