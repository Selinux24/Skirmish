using Common;

namespace SceneTest
{
    static class Program
    {
        static void Main()
        {
            using (Game cl = new Game("3 SceneTest", 400, 267, true, false))
            {
                cl.AddScene(new TestSceneHID(cl) { Active = true, Order = 99, UseZBuffer = false, });
                cl.AddScene(new TestScene3D(cl) { Active = true, Order = 00, });

                cl.Run();
            }
        }
    }
}
