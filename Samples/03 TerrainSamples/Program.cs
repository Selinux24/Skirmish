using Engine;
using Engine.Content.FmtCollada;
using System;

namespace TerrainSamples
{
    using Engine.Content.FmtObj;
    using Engine.Windows;
    using TerrainSamples.SceneStart;

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
                using Game cl = new("Terrain Samples", WindowsEngineForm.ScreenSize * 0.4f);
#else
                using Game cl = new("Terrain Samples");
#endif

                GameResourceManager.RegisterLoader<LoaderCollada>();
                GameResourceManager.RegisterLoader<LoaderObj>();

                cl.SetScene<StartScene>();

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
