using Engine;

namespace ModelDrawing
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("2 ModelDrawing", false, 800, 450))
#else
            using (Game cl = new Game("2 ModelDrawing"))
#endif
            {
                cl.AddScene(new TestScene(cl) { Active = true, });

                cl.Run();
            }
        }
    }
}
