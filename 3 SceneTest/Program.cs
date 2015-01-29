using Engine;

namespace SceneTest
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("3 SceneTest", false, 800, 600))
#else
            using (Game cl = new Game("3 SceneTest"))
#endif
            {
                //TODO: Order isnt work
                cl.AddScene(new TestSceneHID(cl) { Active = true, Order = 3, });
                cl.AddScene(new TestScene3D(cl) { Active = true, Order = 2, });
                cl.AddScene(new TestSceneBackground(cl) { Active = true, Order = 1, });

                cl.Run();
            }
        }
    }
}
