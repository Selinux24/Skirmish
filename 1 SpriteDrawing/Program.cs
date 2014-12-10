using Engine;

namespace SpriteDrawing
{
    static class Program
    {
        static void Main()
        {
            using (Game cl = new Game("1 SpriteDrawing", 400, 267, true, false))
            {
                cl.AddScene(new TestScene(cl) { Active = true, });

                cl.Run();
            }
        }
    }
}
