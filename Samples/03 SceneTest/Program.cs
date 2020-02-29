using Engine;
using Engine.Content.FmtCollada;
using System;
using System.IO;

namespace SceneTest
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
#if DEBUG
                using (Game cl = new Game("3 SceneTest", false, 1600, 900, true, 0, 0))
#else
                using (Game cl = new Game("3 SceneTest", true, 0, 0, true, 0, 0))
#endif
                {
                    GameResourceManager.RegisterLoader<LoaderCollada>();

                    cl.SetScene<SceneStart>();

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
