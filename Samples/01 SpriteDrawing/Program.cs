using Engine;
using System;

namespace SpriteDrawing
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
#if DEBUG
            int sWidth = (int)(System.Windows.Forms.SystemInformation.VirtualScreen.Width * .8f);
            int sHeight = (int)(System.Windows.Forms.SystemInformation.VirtualScreen.Height * .8f);

            using (Game cl = new Game("1 SpriteDrawing", false, sWidth, sHeight, true, 0, 0))
#else
            using (Game cl = new Game("1 SpriteDrawing", true, 0, 0, true, 0, 4))
#endif
            {
                cl.SetScene<TestScene>();

                cl.Run();
            }

        }
    }
}
