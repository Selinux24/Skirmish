using Engine;

namespace SceneTest
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("3 SceneTest", false, 1600, 900, true, 0, 0))
#else
            using (Game cl = new Game("3 SceneTest", true, 0, 0, true, 0, 8))
#endif
            {
                cl.AddScene<SceneStart>();

                cl.Run();
            }
        }
    }
}
