using Engine;
using Engine.Tween;
using Engine.UI.Tween;
using SharpDX;

namespace AISamples
{
    static class UIControlExtensions
    {
        public static void ScaleInScaleOut(this UIControlTweener tweener, IUIControl ctrl, float from, float to, long milliseconds)
        {
            tweener.TweenScaleBounce(ctrl, from, to, milliseconds, ScaleFuncs.Linear);
        }

        public static void ScaleColor(this UIControlTweener tweener, IUIControl ctrl, Color4 from, Color4 to, long milliseconds)
        {
            tweener.TweenBaseColorBounce(ctrl, from, to, milliseconds, ScaleFuncs.CubicEaseInOut);
        }
    }
}
