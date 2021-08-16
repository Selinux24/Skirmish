using Engine;
using System;

namespace ModelDrawing
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

                var screen = EngineForm.ScreenSize * 0.8f;

                using (Game cl = new Game("2 ModelDrawing", false, screen.X, screen.Y, true, 0, 0))
#else
                Logger.LogLevel = LogLevel.Error;

                using (Game cl = new Game("2 ModelDrawing", true, 0, 0, true, 0, 4))
#endif
                {
#if DEBUG
                    cl.VisibleMouse = false;
                    cl.LockMouse = false;
#else
                    cl.VisibleMouse = false;
                    cl.LockMouse = true;
#endif

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
