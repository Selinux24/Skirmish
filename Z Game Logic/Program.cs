using Engine;
using SharpDX;

namespace GameLogic
{
    static class Program
    {
        private static Game game = null;

        static void Main()
        {
#if DEBUG
            using (game = new Game("Game Logic", false, 1280, 720, true, 0, 4))
#else
            using (game = new Game("Game Logic", true, 0, 0, true, 0, 4))
#endif
            {
                game.VisibleMouse = true;
                game.LockMouse = false;

                GameEnvironment.Background = Color.CornflowerBlue;

                game.AddScene(new SceneObjects(game) { Active = true, });

                game.Run();
            }
        }
    }
}
