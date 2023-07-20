using Engine;
using Engine.Content.FmtCollada;
using Engine.Windows;
using System;

namespace Deferred
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

                WindowsExtensions.Startup();

#if DEBUG
                using Game cl = new("7 Deferred", WindowsEngineForm.ScreenSize * 0.8f);
#else
                using Game cl = new("7 Deferred");
#endif
#if DEBUG
                cl.VisibleMouse = false;
                cl.LockMouse = false;
#else
                    cl.VisibleMouse = false;
                    cl.LockMouse = true;
#endif

                GameResourceManager.RegisterLoader<LoaderCollada>();

                cl.SetScene<TestScene3D>(SceneModes.DeferredLightning);

                cl.Run();
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
