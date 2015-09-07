using Engine;

namespace DeferredTest
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("7 Deferred", false, 800, 450, true))
#else
            using (Game cl = new Game("7 Deferred"))
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
