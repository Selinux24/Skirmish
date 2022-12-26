using Engine;
using Engine.Content.FmtCollada;
using Engine.Windows;
using System;

namespace GameLogic
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

                EngineServiceFactory.Register<IEngineForm, WindowsEngineFormFactory>();

#if DEBUG
                using (Game game = new Game("Game Logic", WindowsEngineForm.ScreenSize * 0.8f))
#else
                using (Game game = new Game("Game Logic"))
#endif
                {
                    game.VisibleMouse = true;
                    game.LockMouse = false;

                    GameResourceManager.RegisterLoader<LoaderCollada>();

                    game.SetScene<SceneObjects>();

                    game.Run();
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
