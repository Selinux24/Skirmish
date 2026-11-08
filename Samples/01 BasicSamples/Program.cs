using Engine;
using Engine.Audio;
using Engine.Content.FmtCollada.Fmt;
using Engine.Windows;
using Logger;
using System;

namespace BasicSamples
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
#if DEBUG
            Game.Logger.LogLevel = LogLevel.Debug;
            Game.Logger.EnableConsole = true;
#else
            Game.Logger.LogLevel = LogLevel.Error;
#endif
            try
            {
                WindowsExtensions.Startup();

#if DEBUG
                using Game cl = new("Basic Samples", WindowsEngineForm.ScreenSize * 0.8f);
#else
                using Game cl = new("Basic Samples");
#endif

                GameResourceManager.RegisterLoader<LoaderCollada>();
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
