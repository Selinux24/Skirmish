using Engine;

namespace HeightmapTest
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("8 Heightmap", false, 800, 450))
#else
            using (Game cl = new Game("8 Heightmap"))
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
