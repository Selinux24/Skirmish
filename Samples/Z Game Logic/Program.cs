using Engine;
using Engine.Content.FmtCollada;
using System;
using System.IO;

namespace GameLogic
{
    static class Program
    {
        private static Game game = null;

        [STAThread]
        static void Main()
        {
            try
            {
#if DEBUG
                int sWidth = (int)(System.Windows.Forms.SystemInformation.VirtualScreen.Width * .8f);
                int sHeight = (int)(System.Windows.Forms.SystemInformation.VirtualScreen.Height * .8f);

                using (game = new Game("Game Logic", false, sWidth, sHeight, true, 0, 0))
#else
                using (game = new Game("Game Logic", true, 0, 0, true, 0, 4))
#endif
                {
                    game.VisibleMouse = true;
                    game.LockMouse = false;

                    GameResourceManager.RegisterLoader<LoaderCollada>();

                    game.SetScene<SceneObjects>();

                    game.Run();
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText("dump.txt", ex.ToString());
            }
        }
    }
}
