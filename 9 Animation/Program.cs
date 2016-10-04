using Engine;

namespace AnimationTest
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("9 Animation", false, 800, 450))
#else
            using (Game cl = new Game("9 Animation"))
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
