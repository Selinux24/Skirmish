using Engine;
using Engine.Audio;
using Engine.Content.FmtCollada.Fmt;
using Engine.Content.FmtObj.Fmt;
using Engine.Windows;
using Logger;
using System;

namespace TerrainSamples
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
#if DEBUG
                Game.Logger.LogLevel = LogLevel.Debug;
                Game.Logger.EnableConsole = true;
#else
                Game.Logger.LogLevel = LogLevel.Error;
#endif

                WindowsExtensions.Startup();

#if DEBUG
                using Game cl = new("Terrain Samples", WindowsEngineForm.ScreenSize * 0.8f);
#else
                using Game cl = new("Terrain Samples");
#endif

                GameResourceManager.RegisterLoader<LoaderCollada>();
                GameResourceManager.RegisterLoader<LoaderObj>();
                GameAudioManager.RegisterAudio<GameAudioLoader>(48000);

                cl.SetScene<SceneStart.StartScene>();

                cl.Run();
            }
            catch (Exception ex)
            {
                Game.Logger.WriteError(nameof(Program), ex);
            }
            finally
            {
#if DEBUG
                Game.Logger.Dump("dumpDEBUG.txt");
#else
                if (Game.Logger.HasErrors())
                {
                    Game.Logger.Dump($"dump{DateTime.Now:yyyyMMddHHmmss.fff}.txt");
                }
#endif
            }
        }
    }
}
