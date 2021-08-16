using Engine;
using Engine.Content.FmtCollada;
using Engine.Content.FmtObj;
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
                var screen = EngineForm.ScreenSize * 0.8f;

                using (Game cl = new Game("5 Skybox", false, screen.X, screen.Y, true, 0, 0))
#else
                using (Game cl = new Game("5 Skybox", true, 0, 0, true, 0, 4))
#endif
                {
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
