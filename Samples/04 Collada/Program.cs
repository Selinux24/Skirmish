using Engine;
using Engine.Content.FmtCollada;
using Engine.Content.FmtObj;
using System;
using System.IO;

namespace Collada
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

                using (Game cl = new Game("4 Collada", false, sWidth, sHeight, true, 0, 0))
#else
                using (Game cl = new Game("4 Collada", true, 0, 0, true, 0, 0))
#endif
                {
                    GameResourceManager.RegisterLoader<LoaderObj>();
                    GameResourceManager.RegisterLoader<LoaderCollada>();

                    cl.SetScene<SceneStart>();

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
