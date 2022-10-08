using Engine;
using Engine.Content.FmtCollada;
using Engine.Content.FmtObj;
using System;

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
                Logger.LogLevel = LogLevel.Debug;
                Logger.LogStackSize = 0;
                Logger.EnableConsole = true;
#else
                Logger.LogLevel = LogLevel.Error;
#endif

#if DEBUG
                using (Game cl = new Game("4 Collada", EngineForm.ScreenSize * 0.8f, true, 0, 0))
#else
                using (Game cl = new Game("4 Collada"))
#endif
                {
                    GameResourceManager.RegisterLoader<LoaderObj>();
                    GameResourceManager.RegisterLoader<LoaderCollada>();

                    cl.SetScene<Start.SceneStart>();

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
