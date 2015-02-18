using Engine;

namespace Terrain
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("6 Terrain", false, 640, 480))
#else
            using (Game cl = new Game("6 Terrain"))
#endif
            {
#if DEBUG
                cl.VisibleMouse = false;
                cl.LockMouse = false;
#else
                cl.VisibleMouse = false;
                cl.LockMouse = true;
#endif

                cl.AddScene(new TestScene3D(cl) { Active = true, });

                cl.Run();
            }
        }
    }
}
