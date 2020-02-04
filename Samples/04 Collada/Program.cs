using Engine;
using System;

namespace Collada
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
#if DEBUG
                using (Game cl = new Game("4 Collada", false, 1600, 900, true, 0, 0))
#else
                using (Game cl = new Game("4 Collada", true, 0, 0, true, 0, 0))
#endif
                {
                    cl.SetScene<SceneStart>();

                    cl.Run();
                }
        }
    }
}
