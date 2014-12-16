using Engine;

namespace ModelDrawing
{
    static class Program
    {
        static void Main()
        {
            using (Game cl = new Game("2 ModelDrawing", 800, 375, false))
            {
                cl.AddScene(new TestScene(cl) { Active = true, });

                cl.Run();
            }
        }
    }
}
