using Engine;
using Engine.Content.FmtCollada;
using Engine.Content.FmtObj;
using Engine.Windows;
using System;

namespace Skybox
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
                using Game cl = new("5 Skybox", WindowsEngineForm.ScreenSize * 0.8f);
#else
                using Game cl = new("5 Skybox");
#endif
#if DEBUG
                cl.VisibleMouse = false;
                cl.LockMouse = false;
#else
                    cl.VisibleMouse = false;
                    cl.LockMouse = true;
#endif

                GameResourceManager.RegisterLoader<LoaderCollada>();
                GameResourceManager.RegisterLoader<LoaderObj>();

                cl.SetScene<TestScene3D>();

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
