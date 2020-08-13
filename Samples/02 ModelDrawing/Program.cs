using Engine;
using System;
using System.IO;

namespace ModelDrawing
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
#if DEBUG
                int sWidth = (int)(System.Windows.Forms.SystemInformation.VirtualScreen.Width * .8f);
                int sHeight = (int)(System.Windows.Forms.SystemInformation.VirtualScreen.Height * .8f);

                using (Game cl = new Game("2 ModelDrawing", false, sWidth, sHeight, true, 0, 0))
#else
                using (Game cl = new Game("2 ModelDrawing", true, 0, 0, true, 0, 4))
#endif
                {
#if DEBUG
                    cl.VisibleMouse = false;
                    cl.LockMouse = false;
#else
                    cl.VisibleMouse = false;
                    cl.LockMouse = true;
#endif

                    cl.SetScene<TestScene>();

                    cl.Run();
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText("dump.txt", ex.ToString());
            }
        }
    }
}
