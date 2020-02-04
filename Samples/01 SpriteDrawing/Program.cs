using Engine;
using System;

namespace SpriteDrawing
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            
#if DEBUG
                using (Game cl = new Game("1 SpriteDrawing", false, 1600, 900, true, 0, 0))
#else
                using (Game cl = new Game("1 SpriteDrawing", true, 0, 0, true, 0, 4))
#endif
                {
                    cl.SetScene<TestScene>();

                    cl.Run();
                }
            
        }
    }
}
