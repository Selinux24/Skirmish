using Engine;
using System;

namespace SpriteDrawing
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
                var screen = EngineForm.ScreenSize * 0.8f;
                using (Game cl = new Game("1 SpriteDrawing", false, screen.X, screen.Y, true, 0, 0))
#else
                using (Game cl = new Game("1 SpriteDrawing", true, 0, 0, true, 0, 4))
#endif
                {
                    cl.SetScene<TestScene>();

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
