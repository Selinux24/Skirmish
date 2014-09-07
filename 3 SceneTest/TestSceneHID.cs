using Common;

namespace SceneTest
{
    public class TestSceneHID : Scene3D
    {
        private BasicSprite background = null;
        private BasicSprite sprite = null;

        public TestSceneHID(Game game)
            : base(game)
        {
            this.background = this.AddSprite(
                "background.jpg",
                game.Graphics.Width,
                game.Graphics.Height,
                0);

            this.sprite = this.AddSprite(
                "smiley.jpg",
                128,
                128,
                99);
        }
    }
}
