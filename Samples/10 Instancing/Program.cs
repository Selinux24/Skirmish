using Engine;
using Engine.Content.FmtCollada;
using System;

namespace Instancing
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
#if DEBUG
                var screen = EngineForm.ScreenSize * 0.8f;

                using (Game cl = new Game("10 Instancing", false, screen.X, screen.Y, true, 0, 0))
#else
                using (Game cl = new Game("10 Instancing", true, 0, 0, true, 0, 0))
#endif
                {
                    cl.VisibleMouse = false;
#if DEBUG
                    cl.LockMouse = false;
#else
                    cl.LockMouse = true;
#endif

                    GameResourceManager.RegisterLoader<LoaderCollada>();

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
