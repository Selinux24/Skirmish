using Engine;

namespace Skybox
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("5 Skybox", false, 800, 600))
#else
            using (Game cl = new Game("5 Skybox"))
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
