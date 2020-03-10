using Engine;
using Engine.Content.FmtCollada;
using System;

namespace Deferred
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("7 Deferred", false, 1600, 900, true, 0, 0))
#else
            using (Game cl = new Game("7 Deferred", true, 0, 0, true, 0, 4))
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

                cl.SetScene<TestScene3D>();

                cl.Run();
            }
        }
    }
}
