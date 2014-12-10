using Engine;

namespace SceneTest
{
    public class TestSceneHID : Scene3D
    {
        private Sprite background = null;
        private Sprite sprite = null;

        public TestSceneHID(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.background = this.AddSprite(
                "background.jpg",
                this.Game.Form.RenderWidth,
                this.Game.Form.RenderHeight,
                0);

            this.sprite = this.AddSprite(
                "smiley.jpg",
                128,
                128,
                99);
        }
    }
}
