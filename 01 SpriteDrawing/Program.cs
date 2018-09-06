using Engine;
using System;
using System.IO;

namespace SpriteDrawing
{
    static class Program
    {
        static void Main()
        {
            try
            {
#if DEBUG
                using (Game cl = new Game("1 SpriteDrawing", false, 1600, 900, true, 0, 0))
#else
                using (Game cl = new Game("1 SpriteDrawing", true, 0, 0, true, 0, 4))
#endif
                {
                    cl.AddScene<TestScene>();

                    cl.Run();
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText("dump.txt", ex.ToString());
            }
        }
    }
}
