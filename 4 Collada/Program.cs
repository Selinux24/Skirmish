using Engine;

namespace Collada
{
    static class Program
    {
        static void Main()
        {
            using (Game cl = new Game("4 Collada", 1366, 768, false, true))
            {
                cl.AddScene(new TestScene3D(cl) { Active = true, });

                cl.Run();
            }
        }
    }
}
