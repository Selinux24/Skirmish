using Engine;

namespace Skybox
{
    static class Program
    {
        static void Main()
        {
            using (Game cl = new Game("5 Skybox", 800, 600, false, false))
            {
                cl.AddScene(new TestScene3D(cl) { Active = true, });

                cl.Run();
            }
        }
    }
}
