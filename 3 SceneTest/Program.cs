using Engine;

namespace SceneTest
{
    static class Program
    {
        static void Main()
        {
            using (Game cl = new Game("3 SceneTest", 400, 267, false, false, true))
            {
                cl.AddScene(new TestSceneHID(cl) { Active = true, Order = 0, UseZBuffer = false, });
                cl.AddScene(new TestScene3D(cl) { Active = true, Order = 1, UseZBuffer = true, });
                cl.AddScene(new TestSceneBackground(cl) { Active = true, Order = 2, UseZBuffer = false, });

                cl.Run();
            }
        }
    }
}
