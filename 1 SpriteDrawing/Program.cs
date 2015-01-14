using Engine;

namespace SpriteDrawing
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("1 SpriteDrawing", false, 800, 600))
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
