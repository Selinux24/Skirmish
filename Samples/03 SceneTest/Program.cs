using Engine;
using Engine.Content.FmtCollada;
using System;

namespace SceneTest
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
#if DEBUG
                Logger.LogLevel = LogLevel.Debug;
                Logger.LogStackSize = 0;

                int sWidth = (int)(System.Windows.Forms.SystemInformation.VirtualScreen.Width * .8f);
                int sHeight = (int)(System.Windows.Forms.SystemInformation.VirtualScreen.Height * .8f);

                using (Game cl = new Game("3 SceneTest", false, sWidth, sHeight, true, 0, 0))
#else
                Logger.LogLevel = LogLevel.Error;

                using (Game cl = new Game("3 SceneTest", true, 0, 0, true, 0, 0))
#endif
                {
                    GameResourceManager.RegisterLoader<LoaderCollada>();

                    cl.SetScene<SceneStart.SceneStart>();

                    cl.Run();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(nameof(Program), ex);
            }
            finally
            {
#if DEBUG
                Logger.Dump("dumpDEBUG.txt");
#else
                if (Logger.HasErrors())
                {
                    Logger.Dump($"dump{DateTime.Now:yyyyMMddHHmmss.fff}.txt");
                }
#endif
            }
        }
    }
}
