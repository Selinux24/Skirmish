using Engine;
using System;
using System.IO;

namespace Instancing
{
    static class Program
    {
        static void Main()
        {
            try
            {
#if DEBUG
                using (Game cl = new Game("10 Instancing", false, 1600, 900, true, 0, 0))
#else
                using (Game cl = new Game("10 Instancing", true, 0, 0, true, 0, 4))
#endif
                {
#if DEBUG
                    cl.VisibleMouse = false;
                    cl.LockMouse = false;
#else
                    cl.VisibleMouse = false;
                    cl.LockMouse = true;
#endif

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
