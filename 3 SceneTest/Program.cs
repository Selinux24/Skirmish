using Engine;

namespace SceneTest
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("3 SceneTest", false, 800, 450))
#else
            using (Game cl = new Game("3 SceneTest"))
#endif
            {
#if DEBUG
                cl.VisibleMouse = false;
                cl.LockMouse = false;
#else
                cl.VisibleMouse = false;
                cl.LockMouse = true;
#endif

                cl.AddScene(new TestScene(cl) { Active = true });

                cl.Run();
            }
        }
    }
}
