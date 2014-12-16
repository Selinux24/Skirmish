using Engine;

namespace SpriteDrawing
{
    static class Program
    {
        static void Main()
        {
            using (Game cl = new Game("1 SpriteDrawing", 800, 600, false))
            {
                cl.AddScene(new TestScene(cl) { Active = true, });

                cl.Run();
            }
        }
    }
}
