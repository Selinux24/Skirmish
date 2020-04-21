using Engine;
using Engine.Content.FmtCollada;
using Engine.Content.FmtObj;
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
                GameResourceManager.RegisterLoader<LoaderObj>();
                GameResourceManager.RegisterLoader<LoaderCollada>();

                cl.SetScene<SceneNavmeshTest>();

                cl.Run();
            }
        }
    }
}
