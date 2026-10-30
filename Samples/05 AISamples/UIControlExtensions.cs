using System;

namespace AISamples
{
    using Engine;
    using Engine.BuiltIn.UI;
    using Engine.Mathematics;
    using Engine.Tween;
    using Engine.UI.Tween;

    static class UIControlExtensions
    {
        public static void ScaleInScaleOut(this UIControlTweener tweener, IUIControl ctrl, float from, float to, long milliseconds)
        {
            tweener.TweenScaleBounce(ctrl, from, to, milliseconds, ScaleFuncs.Linear);
        }

        public static void ScaleColor(this UIControlTweener tweener, IUIControl ctrl, Vector4 from, Vector4 to, long milliseconds)
        {
            tweener.TweenBaseColorBounce(ctrl, from, to, milliseconds, ScaleFuncs.CubicEaseInOut);
        }

        public static void LocateButtons(IEngineForm form, UIButton[] sceneButtons, float butWidth, float butHeight, int cols)
        {
            int numButtons = sceneButtons.Length;
            int rowCount = (int)MathF.Ceiling(numButtons / (float)cols);
            int div = cols + 1;

            int h = 8;
            int hv = h - 1;

            int formWidth = (int)form.RenderWidth;
            int formHeight = (int)form.RenderHeight;

            int i = 0;
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (i >= sceneButtons.Length)
                    {
                        break;
                    }

                    var button = sceneButtons[i++];
                    if (button == null)
                    {
                        continue;
                    }

                    button.Left = (formWidth / div * (col + 1)) - (butWidth / 2);
                    button.Top = formHeight / h * hv - (butHeight / 2) + (row * (butHeight + 10));
                }
            }
        }
    }
}
