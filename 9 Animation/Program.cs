using Engine;

namespace AnimationTest
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("9 Animation", false, 1024, 576, true, 0, 4))
#else
            using (Game cl = new Game("9 Animation", true, 0, 0, true, 0, 4))
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
