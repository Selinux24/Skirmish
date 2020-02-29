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
#if DEBUG
                using (Game cl = new Game("10 Instancing", false, 1600, 900, true, 0, 0))
#else
                using (Game cl = new Game("10 Instancing", true, 0, 0, true, 0, 4))
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

                cl.SetScene<TestScene>();

                    cl.Run();
                }
        }
    }
}
