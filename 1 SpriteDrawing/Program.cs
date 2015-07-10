using Engine;

namespace SpriteDrawing
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("1 SpriteDrawing", false, 800, 450))
#else
            using (Game cl = new Game("1 SpriteDrawing"))
#endif
            {
                cl.AddScene(new TestScene(cl) { Active = true, });

                cl.Run();
            }
        }
    }
}
