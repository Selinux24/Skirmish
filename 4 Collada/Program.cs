using Engine;

namespace Collada
{
    static class Program
    {
        static void Main()
        {
            using (Game cl = new Game("4 Collada", 680, 480, false, false))
            {
                cl.AddScene(new TestScene3D(cl) { Active = true, });

                cl.Run();
            }
        }
    }
}
