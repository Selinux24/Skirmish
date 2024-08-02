using Engine;
using Engine.Content.FmtCollada;
using Engine.Content.FmtObj;
using Engine.Windows;
using System;

namespace AISamples
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
                Logger.EnableConsole = true;
#else
                Logger.LogLevel = LogLevel.Error;
#endif

                WindowsExtensions.Startup();

#if DEBUG
                using Game cl = new("AI Samples", WindowsEngineForm.ScreenSize * 0.8f);
#else
                using Game cl = new("AI Samples");
#endif

                GameResourceManager.RegisterLoader<LoaderCollada>();
                GameResourceManager.RegisterLoader<LoaderObj>();

                cl.SetScene<SceneStart.StartScene>();

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
