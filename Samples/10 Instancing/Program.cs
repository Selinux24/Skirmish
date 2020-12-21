using Engine;
using Engine.Content.FmtCollada;
using System;
using System.IO;

namespace Instancing
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

                using (Game cl = new Game("10 Instancing", false, sWidth, sHeight, true, 0, 0))
#else
                using (Game cl = new Game("10 Instancing", true, 0, 0, true, 0, 0))
#endif
                {
                    cl.VisibleMouse = false;
#if DEBUG
                    cl.LockMouse = false;
#else
                    cl.LockMouse = true;
#endif

                    GameResourceManager.RegisterLoader<LoaderCollada>();

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
