using Engine;

namespace SpriteDrawing
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("1 SpriteDrawing", false, 800, 450, true, 0, 4))
#else
            using (Game cl = new Game("1 SpriteDrawing", true, 0, 0, true, 0, 4))
#endif
            {
                cl.AddScene(new TestScene(cl) { Active = true, });

                cl.Run();
            }
        }
    }
}
