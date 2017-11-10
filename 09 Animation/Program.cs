using Engine;

namespace Animation
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("9 Animation", false, 1024, 576, true, 0, 8))
#else
            using (Game cl = new Game("9 Animation", true, 0, 0, true, 0, 8))
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
