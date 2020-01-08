using Engine;
using System;
using System.IO;

namespace Collada
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
#if DEBUG
                using (Game cl = new Game("4 Collada", false, 1600, 900, true, 0, 0))
#else
                using (Game cl = new Game("4 Collada", true, 0, 0, true, 0, 0))
#endif
                {
                    cl.AddScene<SceneStart>();

                    cl.Run();
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText("dump.txt", ex.ToString());
#if DEBUG
                throw;
#endif
            }
        }
    }
}
