using Engine;
using System;

namespace Heightmap
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("8 Heightmap", false, 1600, 900, true, 0, 0))
#else
            using (Game cl = new Game("8 Heightmap", true, 0, 0, true, 0, 4))
#endif
            {
#if DEBUG
                cl.VisibleMouse = false;
                cl.LockMouse = false;
#else
                cl.VisibleMouse = false;
                cl.LockMouse = true;
#endif

                cl.SetScene<TestScene3D>();

                cl.Run();
            }
        }
    }
}
