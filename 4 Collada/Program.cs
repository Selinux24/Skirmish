using Common;

namespace Collada
{
    static class Program
    {
        static void Main()
        {
            using (Game cl = new Game("4 Collada", 1280, 600, false, false))
            {
                cl.AddScene(new TestScene3D(cl) { Active = true, });

                cl.Run();
            }
        }
    }
}
