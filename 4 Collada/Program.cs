using Engine;

namespace Collada
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("4 Collada", false, 800, 600, 60))
#else
            using (Game cl = new Game("4 Collada"))
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
