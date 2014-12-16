using Engine;

namespace GameLogic
{
    static class Program
    {
        static void Main()
        {
            using (Game cl = new Game("Game Logic", 800, 600, false))
            {
                cl.AddScene(new TestScene3D(cl) { Active = true, });

                cl.Run();
            }
        }
    }
}
